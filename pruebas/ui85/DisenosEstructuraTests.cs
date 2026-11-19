using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Blazored.LocalStorage;
using Bunit;
using KONSolutions.Shared.Models.Ingenieria;
using KONSolutions.Web.Pages.Ingenieria;
using KONSolutions.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using Xunit;

public class FakeEstructura : HttpMessageHandler
{
    public List<(string M, string U, JsonElement? B)> Calls = new();
    public bool Bloqueada;            // la version viene bloqueada
    public bool Falla;                // la API rechaza las escrituras
    public int Imagenes = 3;          // cuantas imagenes hay en la carpeta
    public string MensajeFalla = "La version esta bloqueada: desbloqueela para modificar su estructura.";

    static DisenosEstructuraNodo N(int id, int? padre, int prof, int nivel, string tipo, string d, int v, string desc, decimal costo, Action<DisenosEstructuraNodo>? cfg = null)
    { var n = new DisenosEstructuraNodo { ID = id, PARENT_ID = padre, ORDEN = id.ToString("00000"), PROFUNDIDAD = prof, NIVEL_ENSAMBLE = nivel, TIPO_NODO = tipo, COD_DISENO = d, NUM_VERSION = v, DESCRIPCION = desc, COSTO_TOTAL = costo }; cfg?.Invoke(n); return n; }

    public bool SinRuta;              // la version no tiene ruta de produccion
    public bool RutaBloqueada;        // la ruta la usa una version bloqueada
    public bool ConRutas;             // agrega nodos de ruta de produccion (RUT/PRC/SPR/OPE) al arbol

    public List<DisenosEstructuraNodo> Nodos()
    {
        var l = Base();
        if (ConRutas)
        {
            l.Add(N(9, 5, 6, 2, "RUT", "D2", 1, "RUTA DE PLACAS", 0, n => { n.COD_RUTA_PRODUCCION = "RT-D2"; n.COSTO_TOTAL = null; }));
            l.Add(N(10, 9, 7, 2, "PRC", "D2", 1, "ENSAMBLADO", 0, n => { n.COD_PROCESO = "ENS"; n.COSTO_TOTAL = null; }));
            l.Add(N(11, 10, 8, 2, "SPR", "D2", 1, "SOLDADURA", 0, n => { n.COD_SUB_PROCESO = "SOL"; n.COSTO_TOTAL = null; }));
            l.Add(N(12, 11, 9, 2, "OPE", "D2", 1, "SOLDAR PLACA", 0, n => { n.COD_TIPO_OPERACION_PRODUCCION = "SOLPL"; n.CAN_LEAD_TIME = 30; n.COD_UNIDAD_MEDIDA_TIEMPO = "MIN"; n.FLG_MANUAL = true; n.FLG_CONTROL_CALIDAD = true; n.COSTO_TOTAL = null; }));
            l.Add(N(13, 1, 2, 1, "RUT", "D1", 1, "RUTA DEL PRODUCTO", 0, n => { n.COD_RUTA_PRODUCCION = "RT-D1"; n.COSTO_TOTAL = null; }));
        }
        return l;
    }

    List<DisenosEstructuraNodo> Base() => new()
    {
        N(1, null, 1, 1, "VER", "D1", 1, "DISENO UNO - v1", 100, n => n.FLG_BLOQUEADO = Bloqueada),
        N(2, 1, 2, 1, "SEC", "D1", 1, "Frente", 100, n => { n.COD_SECTOR_PRODUCTO = "FRENT"; n.FLG_BLOQUEADO = Bloqueada; }),
        N(3, 2, 3, 1, "COM", "D1", 1, "Procesador", 100, n => { n.COD_SECTOR_PRODUCTO = "FRENT"; n.COD_COMPONENTE_PRODUCTO = "PROCE"; }),
        N(4, 3, 4, 1, "INS", "D1", 1, "Procesador Core i5", 55, n => { n.COD_SECTOR_PRODUCTO = "FRENT"; n.COD_COMPONENTE_PRODUCTO = "PROCE"; n.NUM_SECUENCIA = 1; n.COD_PRODUCTO_COMPONENTE = "P1"; n.COD_UNIDAD_MEDIDA = "NIU"; n.CAN_CONSUMO = 1; n.TAS_MERMA = 10; n.PRE_UNITARIO = 50; n.CANTIDAD_ACUMULADA = 1.1m; }),
        N(5, 3, 4, 1, "INS", "D1", 1, "ENSAMBLE: MODULO", 45, n => { n.COD_SECTOR_PRODUCTO = "FRENT"; n.COD_COMPONENTE_PRODUCTO = "PROCE"; n.NUM_SECUENCIA = 2; n.COD_PRODUCTO_COMPONENTE = "P2"; n.COD_UNIDAD_MEDIDA = "NIU"; n.CAN_CONSUMO = 1; n.FLG_ENSAMBLE = true; n.COD_DISENO_ENSAMBLE = "D2"; n.NUM_VERSION_ENSAMBLE = 1; }),
        N(6, 5, 5, 2, "SEC", "D2", 1, "Sector del modulo", 45, n => n.COD_SECTOR_PRODUCTO = "S"),
        N(7, 6, 6, 2, "COM", "D2", 1, "Placa madre", 45, n => { n.COD_SECTOR_PRODUCTO = "S"; n.COD_COMPONENTE_PRODUCTO = "PLACA"; }),
        N(8, 7, 7, 2, "INS", "D2", 1, "Placa ATX", 45, n => { n.COD_SECTOR_PRODUCTO = "S"; n.COD_COMPONENTE_PRODUCTO = "PLACA"; n.NUM_SECUENCIA = 1; n.COD_PRODUCTO_COMPONENTE = "P3"; n.COD_UNIDAD_MEDIDA = "NIU"; n.CAN_CONSUMO = 1; n.PRE_UNITARIO = 45; }),
    };

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage r, CancellationToken ct)
    {
        var u = r.RequestUri!.PathAndQuery;
        JsonElement? body = r.Content is null || r.Content is MultipartFormDataContent ? null : JsonDocument.Parse(await r.Content.ReadAsStringAsync(ct)).RootElement.Clone();
        Calls.Add((r.Method.Method, u, body));
        if (u.Contains("documentos/ver"))
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(new byte[] { 0x89, 0x50, 0x4E, 0x47, 1, 2, 3 }) { Headers = { ContentType = new("image/png") } } };
        object data;
        if (r.Method == HttpMethod.Get)
        {
            if (u.Contains("documentos-carpeta")) data = Enumerable.Range(1, Imagenes).Select(i => (object)new { NombreArchivo = $"img{i}.jpg", TamanoBytes = 10, FechaModificacion = DateTime.Now, Extension = "JPG", RutaCompleta = $"\\\\srv\\{i}.jpg" }).Append((object)new { NombreArchivo = "doc.pdf", TamanoBytes = 1, FechaModificacion = DateTime.Now, Extension = "PDF", RutaCompleta = "\\\\srv\\doc.pdf" }).ToArray();
            else if (u.Contains("grupos-producto/combobox")) data = new[] { new { Id = "G1", Texto = "PRENDA" }, new { Id = "G2", Texto = "TELA" } };
            else if (u.Contains("sub-familias-producto/combobox/")) data = new[] { new { Id = "S1", Texto = "MANGA CORTA" }, new { Id = "S2", Texto = "MANGA LARGA" } };
            else if (u.Contains("/familias-producto/combobox/")) data = new[] { new { Id = "F1", Texto = "POLOS" }, new { Id = "F2", Texto = "CAMISAS" } };
            else if (u.EndsWith("/ingenieria/disenos")) data = new[]
            {
                new { COD_DISENO = "D1", DES_DISENO = "DISENO UNO", COD_GRUPO_PRODUCTO = "G1", COD_FAMILIA_PRODUCTO = "F1", COD_SUB_FAMILIA_PRODUCTO = "S1" },
                new { COD_DISENO = "D2", DES_DISENO = "DISENO DOS", COD_GRUPO_PRODUCTO = "G2", COD_FAMILIA_PRODUCTO = "F1", COD_SUB_FAMILIA_PRODUCTO = "S1" },
                new { COD_DISENO = "D3", DES_DISENO = "DISENO TRES", COD_GRUPO_PRODUCTO = "G2", COD_FAMILIA_PRODUCTO = "F2", COD_SUB_FAMILIA_PRODUCTO = "S2" },
            };
            else if (u.Contains("ruta-catalogos")) data = new
            {
                Procesos = new[] { new { Id = "CORTE", Texto = "Corte" }, new { Id = "COSTU", Texto = "Costura" } },
                SubProcesos = new[] { new { COD_PROCESO = "CORTE", Id = "TNDDO", Texto = "Tendido" }, new { COD_PROCESO = "COSTU", Id = "ARMDO", Texto = "Armado" } },
                Operaciones = new[] { new { Id = "TENDI", Texto = "Tendido de tela", FLG_CONTROL_CALIDAD = false }, new { Id = "CIERC", Texto = "Cierre de costuras", FLG_CONTROL_CALIDAD = true } },
                UnidadesTiempo = new[] { new { Id = "MIN", Texto = "MINUTO" }, new { Id = "HOR", Texto = "HORA" } },
                TiposRuta = new[] { new { Id = "PRD", Texto = "PRODUCCION" } },
                Rutas = new[] { new { Id = "RT0000001", Texto = "RUTA UNO" }, new { Id = "RT0000002", Texto = "RUTA DOS" } },
            };
            else if (u.Contains("ruta-elementos")) data = new object[]
            {
                new { NUM_ESTRUCTURA_RUTA_PROCESO = 1, NUM_ESTRUCTURA_RUTA_PROCESO_PADRE = (int?)null, NIVEL = 1, ORDEN = "a", TIPO = "PRC", COD_PROCESO = "CORTE", DESCRIPCION = "Corte", ES_PRIMERO = true, ES_ULTIMO = false },
                new { NUM_ESTRUCTURA_RUTA_PROCESO = 2, NUM_ESTRUCTURA_RUTA_PROCESO_PADRE = (int?)1, NIVEL = 2, ORDEN = "b", TIPO = "SPR", COD_PROCESO = "CORTE", COD_SUB_PROCESO = "TNDDO", DESCRIPCION = "Tendido", ES_PRIMERO = true, ES_ULTIMO = true },
                new { NUM_ESTRUCTURA_RUTA_PROCESO = 3, NUM_ESTRUCTURA_RUTA_PROCESO_PADRE = (int?)2, NIVEL = 3, ORDEN = "c", TIPO = "OPE", COD_PROCESO = "CORTE", COD_SUB_PROCESO = "TNDDO", COD_TIPO_OPERACION_PRODUCCION = "TENDI", DESCRIPCION = "Tendido de tela", CAN_LEAD_TIME = (decimal?)30m, COD_UNIDAD_MEDIDA_TIEMPO = "MIN", FLG_MANUAL = (bool?)true, FLG_CONTROL_CALIDAD = (bool?)false, ES_PRIMERO = true, ES_ULTIMO = true },
                new { NUM_ESTRUCTURA_RUTA_PROCESO = 4, NUM_ESTRUCTURA_RUTA_PROCESO_PADRE = (int?)null, NIVEL = 1, ORDEN = "d", TIPO = "PRC", COD_PROCESO = "COSTU", DESCRIPCION = "Costura", ES_PRIMERO = false, ES_ULTIMO = true },
            };
            else if (u.Contains("disenos-estructura/ruta/")) data = new
            {
                COD_DISENO = "D1", NUM_VERSION = 1, FLG_BLOQUEADO = Bloqueada,
                COD_RUTA_PRODUCCION = SinRuta ? null : "RT0000001", DES_RUTA_PRODUCCION = SinRuta ? null : "RUTA UNO", COD_TIPO_RUTA_PRODUCCION = SinRuta ? null : "PRD",
                USADA_POR = SinRuta ? 0 : (RutaBloqueada ? 2 : 1), BLOQUEADAS = RutaBloqueada ? 1 : 0, ELEMENTOS = SinRuta ? 0 : 4
            };
            else if (u.Contains("disenos-estructura/")) data = Nodos();
            else if (u.Contains("componentes-producto")) data = new[] { new { Id = "PROCE", Texto = "Procesador" }, new { Id = "MARCO", Texto = "Marco" } };
            else if (u.Contains("tipos-merma")) data = new[] { new { Id = "PRO", Texto = "PRODUCCION" }, new { Id = "COM", Texto = "MERMA DE COMPRA" } };
            else if (u.Contains("monedas")) data = new[] { new { Id = "PEN", Texto = "SOLES" }, new { Id = "USD", Texto = "DOLARES" } };
            else if (u.Contains("productos/search")) data = new object[] { new { COD_PRODUCTO = "P9", DES_PRODUCTO = "PRODUCTO NUEVE", COD_UNIDAD_MEDIDA = "NIU", COD_GRUPO_PRODUCTO = "G1", COD_FAMILIA_PRODUCTO = "F1", COD_SUB_FAMILIA_PRODUCTO = "S1" }, new { COD_PRODUCTO = "P2", DES_PRODUCTO = "ENSAMBLE: MODULO", COD_UNIDAD_MEDIDA = "NIU", COD_DISENO = "D2", NUM_VERSION = 1 } };
            else if (u.Contains("disenos-version")) data = new[] { new { COD_DISENO = "D1", NUM_VERSION = 1, DES_VERSION = "v1" }, new { COD_DISENO = "D1", NUM_VERSION = 2, DES_VERSION = "v2" } };
            else if (u.Contains("disenos/combobox")) data = new[] { new { Id = "D1", Texto = "DISENO UNO" }, new { Id = "D2", Texto = "DISENO DOS" } };
            else data = new object[0];
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(new { Success = true, Data = data }) };
        }
        if (Falla) return new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = JsonContent.Create(new { Success = false, Message = MensajeFalla }) };
        data = u.Contains("ruta/asignar") ? true
             : u.Contains("/ruta/") ? new { COD_RUTA_PRODUCCION = "RT0000999", ELEMENTOS_COPIADOS = 4, NUM_ESTRUCTURA_RUTA_PROCESO = 9, ELEMENTOS_ELIMINADOS = 3, MOVIDO = true }
             : u.Contains("copiar-version") ? new { NUM_VERSION_NUEVA = 3, SECTORES = 2, COMPONENTES = 3, INSUMOS = 5 }
             : u.Contains("copiar-segmento") ? new { INSUMOS_COPIADOS = 4 }
             : u.Contains("/insumo") && !u.Contains("eliminar") ? new { NUM_SECUENCIA = 7 } : (object)true;
        return new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(new { Success = true, Data = data }) };
    }
}

public class DisenosEstructuraTests : TestContext
{
    FakeEstructura _f = new();

    IRenderedComponent<MudDialogProvider> Prov()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton(new HttpClient(_f) { BaseAddress = new Uri("http://x/") });
        Services.AddBlazoredLocalStorage();
        Services.AddScoped<ErrorLogService>(); Services.AddScoped<ConfigService>();
        RenderComponent<MudPopoverProvider>(); RenderComponent<MudSnackbarProvider>();
        return RenderComponent<MudDialogProvider>();
    }

    async Task<IDialogReference> Abre<T>(IRenderedComponent<MudDialogProvider> p, DialogParameters parametros) where T : Microsoft.AspNetCore.Components.ComponentBase
    {
        var ds = Services.GetRequiredService<IDialogService>();
        IDialogReference? d = null;
        await p.InvokeAsync(async () => d = await ds.ShowAsync<T>("t", parametros, new DialogOptions { MaxWidth = MaxWidth.False }));
        p.WaitForAssertion(() => p.Find(".mud-dialog"));
        return d!;
    }

    Task<IDialogReference> AbreEstructura(IRenderedComponent<MudDialogProvider> p) =>
        Abre<DisenosEstructuraDialog>(p, new DialogParameters { ["CodDiseno"] = "D1", ["DesDiseno"] = "DISENO UNO", ["NumVersion"] = 1 });

    static JsonElement Prop(JsonElement e, string n) => e.EnumerateObject().First(x => string.Equals(x.Name, n, StringComparison.OrdinalIgnoreCase)).Value;

    // ---------- estructura ----------
    [Fact]
    public async Task Estructura_MuestraElArbol_ConCostoYNiveles()
    {
        var p = Prov(); await AbreEstructura(p);
        p.WaitForAssertion(() => Assert.Equal(8, p.FindAll("tr[data-nodo]").Count));
        Assert.Contains("Editable", p.Find("[data-estado]").TextContent);
        Assert.Contains("100,00", p.Find("[data-costo]").TextContent.Replace('.', ','));
        Assert.Contains("2 / 10", p.Find("[data-niveles]").TextContent);
        Assert.Contains("Subensamble D2 v1", p.Markup);
    }

    [Fact]
    public async Task Estructura_NodosDelSubensamble_NoTienenAcciones_YLosPropiosSi()
    {
        var p = Prov(); await AbreEstructura(p);
        p.WaitForAssertion(() => Assert.Equal(8, p.FindAll("tr[data-nodo]").Count));
        Assert.Empty(p.Find("tr[data-nodo='8']").QuerySelectorAll("[data-accion]"));      // insumo del subensamble D2
        Assert.Empty(p.Find("tr[data-nodo='6']").QuerySelectorAll("[data-accion]"));      // sector del subensamble
        Assert.NotEmpty(p.Find("tr[data-nodo='4']").QuerySelectorAll("[data-accion='edit-insumo']"));
        Assert.NotEmpty(p.Find("tr[data-nodo='2']").QuerySelectorAll("[data-accion='add-comp']"));
        Assert.Contains("de D2 v1", p.Find("tr[data-nodo='8']").TextContent);
    }

    [Fact]
    public async Task Estructura_Contraer_OcultaLosHijos()
    {
        var p = Prov(); await AbreEstructura(p);
        p.WaitForAssertion(() => Assert.Equal(8, p.FindAll("tr[data-nodo]").Count));
        p.Find("tr[data-nodo='3'] [data-toggle]").Click();
        p.WaitForAssertion(() => Assert.Equal(3, p.FindAll("tr[data-nodo]").Count));   // quedan raiz, sector y componente
        p.FindAll("button").First(b => b.TextContent.Contains("Expandir")).Click();
        p.WaitForAssertion(() => Assert.Equal(8, p.FindAll("tr[data-nodo]").Count));
    }

    [Fact]
    public async Task Bloqueada_NoPermiteEditar_YOfreceDesbloquear()
    {
        _f.Bloqueada = true;
        var p = Prov(); await AbreEstructura(p);
        p.WaitForAssertion(() => Assert.Equal(8, p.FindAll("tr[data-nodo]").Count));
        Assert.Contains("Bloqueada", p.Find("[data-estado]").TextContent);
        Assert.Empty(p.FindAll("tr[data-nodo] [data-accion]").Where(e => e.GetAttribute("data-accion") != "abrir-ens"));   // solo se puede abrir el subensamble
        Assert.True(p.Find("[data-accion='nuevo-sector']").HasAttribute("disabled"));
        Assert.Empty(p.FindAll("[data-accion='bloquear']"));
        Assert.NotEmpty(p.FindAll("[data-accion='desbloquear']"));
    }

    [Fact]
    public async Task Bloquear_PideConfirmar_YPublicaElBloqueo()
    {
        var p = Prov(); await AbreEstructura(p);
        p.WaitForAssertion(() => Assert.Equal(8, p.FindAll("tr[data-nodo]").Count));
        p.Find("[data-accion='bloquear']").Click();
        p.WaitForAssertion(() => Assert.Contains("Bloquear versión", p.Markup));
        Assert.DoesNotContain(_f.Calls, c => c.U.Contains("/bloquear"));        // todavia no: falta confirmar
        p.FindAll("button").Last(b => b.TextContent.Trim() == "Bloquear").Click();
        p.WaitForAssertion(() => Assert.Contains(_f.Calls, c => c.M == "POST" && c.U.EndsWith("/bloquear")));
        var b = _f.Calls.Last(c => c.U.EndsWith("/bloquear")).B!.Value;
        Assert.Equal("D1", Prop(b, "COD_DISENO").GetString()); Assert.Equal(1, Prop(b, "NUM_VERSION").GetInt32());
    }

    [Fact]
    public async Task Estructura_SiLaApiRechaza_MuestraElMotivo()
    {
        var p = Prov(); await AbreEstructura(p);
        p.WaitForAssertion(() => Assert.Equal(8, p.FindAll("tr[data-nodo]").Count));
        _f.Falla = true; _f.MensajeFalla = "La version esta bloqueada: desbloqueela para modificar su estructura.";
        p.Find("[data-accion='bloquear']").Click();
        p.WaitForAssertion(() => Assert.Contains("Bloquear versión", p.Markup));
        p.FindAll("button").Last(b => b.TextContent.Trim() == "Bloquear").Click();
        p.WaitForAssertion(() => Assert.Contains("desbloqueela", p.Find("[data-error]").TextContent));
    }

    [Fact]
    public async Task Ampliar_AlternaEntreTamanoNormalYPantallaCompleta()
    {
        var p = Prov(); await AbreEstructura(p);
        p.WaitForAssertion(() => Assert.Equal(8, p.FindAll("tr[data-nodo]").Count));
        Assert.DoesNotContain("mud-dialog-fullscreen", p.Find(".mud-dialog").ClassName);
        p.Find("[data-accion='ampliar']").Click();
        p.WaitForAssertion(() => Assert.Contains("mud-dialog-fullscreen", p.Find(".mud-dialog").ClassName));
        p.Find("[data-accion='ampliar']").Click();
        p.WaitForAssertion(() => Assert.DoesNotContain("mud-dialog-fullscreen", p.Find(".mud-dialog").ClassName));
    }

    // El alto de las listas lo decide CSS (flexbox), no JavaScript ni vh: asi no depende de un archivo desplegado aparte (una copia vieja de
    // js/estructuraFit.js mandaba sobre el script embebido y dejaba la lista ~25 % mas corta con zoom 74 %) y sigue al zoom de la aplicacion.
    [Fact]
    public async Task Estructura_ElArbolLlenaElAltoPorCss_SinScriptNiAlturasEnLinea()
    {
        var p = Prov(); await AbreEstructura(p);
        p.WaitForAssertion(() => Assert.Equal(8, p.FindAll("tr[data-nodo]").Count));
        var arbol = p.Find(".est-wrap");
        Assert.Contains("kon-llena", arbol.ClassName);
        Assert.DoesNotContain("height", arbol.GetAttribute("style") ?? "");     // ninguna altura en linea: manda la hoja de estilos
        Assert.Empty(p.FindAll("[data-fit]"));                                  // el marcador del script viejo no existe: no lo puede tocar
        Assert.Contains("kon-tabs-llena", p.Find(".mud-tabs").ClassName);       // las pestanas se estiran en pantalla completa
        Assert.Contains(".mud-dialog-fullscreen .kon-llena", p.Markup);         // las reglas viajan con la ventana
        Assert.Contains("max-height:calc(var(--llena-vh, 72vh) / var(--app-scale, 1))", p.Markup);   // el alto maximo de la ventana normal sigue el zoom propio de la app
    }

    [Fact]
    public async Task Diagrama_LlenaElAltoPorCss_SinScriptNiAlturasEnLinea()
    {
        var p = Prov(); await AbreEstructura(p);
        p.WaitForAssertion(() => Assert.Equal(8, p.FindAll("tr[data-nodo]").Count));
        p.FindAll("div.mud-tab").First(x => x.TextContent.Contains("Diagrama", StringComparison.OrdinalIgnoreCase)).Click();
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-diagrama]")));
        var d = p.Find("[data-diagrama]");
        Assert.Contains("kon-llena", d.ClassName);
        Assert.DoesNotContain("height", d.GetAttribute("style") ?? "");
        Assert.Empty(p.FindAll("[data-fit]"));
    }

    [Fact]
    public async Task Arbol_DibujaConectoresPorNivel_YMarcaElNivelDeEnsamble()
    {
        var p = Prov(); await AbreEstructura(p);
        p.WaitForAssertion(() => Assert.Equal(8, p.FindAll("tr[data-nodo]").Count));
        Assert.Empty(p.Find("tr[data-nodo='1']").QuerySelectorAll(".tr-g"));                               // la raiz no lleva conector
        var g4 = p.Find("tr[data-nodo='4']").QuerySelectorAll(".tr-g");                                    // insumo: 2 niveles de sangria + su ramal
        Assert.Equal(3, g4.Length); Assert.Contains("t", g4.Last().ClassName.Split(' '));                  // tiene un hermano despues (el subensamble)
        Assert.Contains("l", p.Find("tr[data-nodo='5']").QuerySelectorAll(".tr-g").Last().ClassName.Split(' '));   // es el ultimo
        Assert.Contains("est-n2", p.Find("tr[data-nodo='6']").ClassName);                                  // sector del subensamble: nivel 2
        Assert.Contains("Nivel 2", p.Find("tr[data-nodo='6']").TextContent);
        Assert.DoesNotContain("est-n2", p.Find("tr[data-nodo='2']").ClassName);
    }

    [Fact]
    public async Task CambiarComponente_PideElNuevo_YPublicaOrigenYDestino()
    {
        var p = Prov(); await AbreEstructura(p);
        p.WaitForAssertion(() => Assert.Equal(8, p.FindAll("tr[data-nodo]").Count));
        p.Find("tr[data-nodo='3'] [data-accion='edit-comp']").Click();
        p.WaitForAssertion(() => Assert.Contains("PROCE", p.Find("[data-actual]").TextContent));
        var combo = p.FindComponents<KONSolutions.Web.Pages.Shared.AriaCombo<KONSolutions.Shared.Common.ComboboxItem>>().Last();
        await p.InvokeAsync(() => combo.Instance.ValueChanged.InvokeAsync("MARCO"));
        p.FindAll("[data-accion='guardar']").Last().Click();
        p.WaitForAssertion(() => Assert.Contains(_f.Calls, c => c.M == "POST" && c.U.EndsWith("/componente/cambiar")));
        var b = _f.Calls.Last(c => c.U.EndsWith("/componente/cambiar")).B!.Value;
        Assert.Equal("PROCE", Prop(b, "COD_COMPONENTE_ORIGEN").GetString()); Assert.Equal("MARCO", Prop(b, "COD_COMPONENTE_DESTINO").GetString());
        Assert.Equal("FRENT", Prop(b, "COD_SECTOR_PRODUCTO").GetString());
    }

    [Fact]
    public async Task AbrirSubensamble_AbreLaEstructuraDeEseDisenoYVersion()
    {
        var p = Prov(); await AbreEstructura(p);
        p.WaitForAssertion(() => Assert.Equal(8, p.FindAll("tr[data-nodo]").Count));
        p.Find("tr[data-nodo='5'] [data-accion='abrir-ens']").Click();
        p.WaitForAssertion(() => Assert.Contains(_f.Calls, c => c.M == "GET" && c.U.Contains("disenos-estructura/D2/1")));
    }

    // ---------- diagrama ----------
    [Fact]
    public void Diagrama_DibujaUnaCajaPorNodo_YPuedeOcultarInsumos()
    {
        Services.AddMudServices(); JSInterop.Mode = JSRuntimeMode.Loose;
        RenderComponent<MudPopoverProvider>();
        var nodos = new FakeEstructura().Nodos();
        var c = RenderComponent<DisenosDiagramaEstructura>(ps => ps.Add(x => x.Nodos, nodos));
        Assert.Equal(8, c.FindAll("g[data-nodo]").Count);
        Assert.Single(c.FindAll("g[data-tipo='VER']"));
        Assert.Equal(7, c.FindAll("path.dgm-enlace").Count);                    // una linea por cada nodo que no es la raiz
        c.Find("[data-opcion='insumos']").Change(false);
        Assert.Equal(6, c.FindAll("g[data-nodo]").Count);                       // se van los dos insumos comunes; el subensamble se queda
        Assert.Contains("dgm-ens", c.Markup);
    }

    // ---------- rutas de produccion (solo lectura) ----------
    [Fact]
    public async Task Rutas_AparecenEnElArbol_ConTiempo_YSinAcciones()
    {
        _f.ConRutas = true;
        var p = Prov(); await AbreEstructura(p);
        p.WaitForAssertion(() => Assert.Equal(13, p.FindAll("tr[data-nodo]").Count));
        Assert.Equal(2, p.FindAll("tr[data-tipo='RUT']").Count);
        Assert.Contains("RT-D2", p.Find("tr[data-nodo='9']").TextContent);
        Assert.Contains("SOLDAR PLACA", p.Find("tr[data-nodo='12']").TextContent);
        Assert.Contains("30", p.Find("tr[data-nodo='12'] [data-chip='tiempo']").TextContent);
        Assert.Contains("MIN", p.Find("tr[data-nodo='12'] [data-chip='tiempo']").TextContent);
        Assert.Contains("Manual", p.Find("tr[data-nodo='12']").TextContent);
        Assert.Contains("Calidad", p.Find("tr[data-nodo='12']").TextContent);
        foreach (var id in new[] { 9, 10, 11, 12, 13 })                                   // ni la ruta del subensamble ni la propia se editan aqui
            Assert.Empty(p.Find($"tr[data-nodo='{id}']").QuerySelectorAll("[data-accion]"));
        Assert.Contains("est-ruta", p.Find("tr[data-nodo='10']").ClassName);
    }

    [Fact]
    public async Task Rutas_SeContraenConSuEnsamble()
    {
        _f.ConRutas = true;
        var p = Prov(); await AbreEstructura(p);
        p.WaitForAssertion(() => Assert.Equal(13, p.FindAll("tr[data-nodo]").Count));
        p.Find("tr[data-nodo='9'] [data-toggle]").Click();                                 // cierra la ruta: se van PRC, SPR y OPE
        Assert.Equal(10, p.FindAll("tr[data-nodo]").Count);
        Assert.Empty(p.FindAll("tr[data-nodo='12']"));
    }

    [Fact]
    public void Diagrama_DibujaLasRutasEnVerde_YPuedeOcultarlas()
    {
        Services.AddMudServices(); JSInterop.Mode = JSRuntimeMode.Loose;
        RenderComponent<MudPopoverProvider>();
        var nodos = new FakeEstructura { ConRutas = true }.Nodos();
        var c = RenderComponent<DisenosDiagramaEstructura>(ps => ps.Add(x => x.Nodos, nodos));
        Assert.Equal(13, c.FindAll("g[data-nodo]").Count);
        Assert.Equal(5, c.FindAll("g.dgm-rut").Count);
        Assert.Contains("30 MIN", c.Find("g[data-nodo='12']").TextContent);
        Assert.Contains("manual", c.Find("g[data-nodo='12']").TextContent);
        c.Find("[data-opcion='rutas']").Change(false);
        Assert.Equal(8, c.FindAll("g[data-nodo]").Count);
        Assert.Empty(c.FindAll("g.dgm-rut"));
    }

    // ---------- ficha (tarjeta) del mouse sobre las cajas del diagrama ----------
    IRenderedComponent<DisenosDiagramaEstructura> Diagrama(List<DisenosEstructuraNodo> nodos)
    {
        Services.AddMudServices(); JSInterop.Mode = JSRuntimeMode.Loose;
        RenderComponent<MudPopoverProvider>();
        return RenderComponent<DisenosDiagramaEstructura>(ps => ps.Add(x => x.Nodos, nodos));
    }

    static JsonElement Ficha(IRenderedComponent<DisenosDiagramaEstructura> c, int id) =>
        JsonDocument.Parse(c.Find($"g[data-nodo='{id}']").GetAttribute("data-ficha")!).RootElement.Clone();

    static string Fila(JsonElement ficha, string etiqueta) =>
        ficha.GetProperty("f").EnumerateArray().First(r => r[0].GetString() == etiqueta)[1].GetString()!;

    [Fact]
    public void Ficha_CadaCajaLlevaSusDatos_YYaNoUsaElTooltipNativo()
    {
        var c = Diagrama(new FakeEstructura { ConRutas = true }.Nodos());
        Assert.Equal(13, c.FindAll("g[data-nodo]").Count);
        Assert.All(c.FindAll("g[data-nodo]"), g => Assert.Equal(JsonValueKind.Object, JsonDocument.Parse(g.GetAttribute("data-ficha")!).RootElement.ValueKind));
        Assert.Empty(c.FindAll("g[data-nodo] title"));                                              // el tooltip oscuro del navegador ya no sale
        Assert.All(c.FindAll("g[data-nodo]"), g => Assert.False(string.IsNullOrWhiteSpace(g.GetAttribute("aria-label"))));   // lectores de pantalla
    }

    [Fact]
    public void Ficha_DeInsumo_TraeProductoConsumoMermaPrecioYCosto()
    {
        var f = Ficha(Diagrama(new FakeEstructura().Nodos()), 4);
        Assert.Equal("Insumo", f.GetProperty("t").GetString()); Assert.Equal("ins", f.GetProperty("c").GetString());
        Assert.Equal("Procesador Core i5", f.GetProperty("n").GetString()); Assert.Equal("P1", f.GetProperty("k").GetString());
        Assert.Equal("1 NIU", Fila(f, "Consumo")); Assert.Equal("10 %", Fila(f, "Merma"));
        Assert.Equal("1.1", Fila(f, "Cantidad acumulada").Replace(',', '.')); Assert.StartsWith("50", Fila(f, "Precio unitario"));
        Assert.Equal("Costo", f.GetProperty("x")[0].GetString()); Assert.StartsWith("55", f.GetProperty("x")[1].GetString());
        Assert.False(f.TryGetProperty("w", out _));                                                  // sin avisos
    }

    [Fact]
    public void Ficha_DeSubensamble_IndicaDisenoYVersion_YQueTomaElCostoDeSuEstructura()
    {
        var f = Ficha(Diagrama(new FakeEstructura().Nodos()), 5);
        Assert.Equal("Subensamble", f.GetProperty("t").GetString()); Assert.Equal("ens", f.GetProperty("c").GetString());
        Assert.Equal("D2 · v1", Fila(f, "Subensamble")); Assert.Equal("Toma el costo de su estructura", Fila(f, "Precio"));
        Assert.DoesNotContain(f.GetProperty("f").EnumerateArray(), r => r[0].GetString() == "Merma");               // este nodo no trae merma: no se pone "— %"
        Assert.DoesNotContain(f.GetProperty("f").EnumerateArray(), r => r[0].GetString() == "Cantidad acumulada");
    }

    [Fact]
    public void Ficha_SinCosto_NoLlevaTotal()
    {
        var nodos = new FakeEstructura().Nodos();
        nodos.First(n => n.ID == 4).COSTO_TOTAL = null;
        Assert.False(Ficha(Diagrama(nodos), 4).TryGetProperty("x", out _));
    }

    [Fact]
    public void Ficha_DeLoQueVieneDeUnSubensamble_DiceSuNivelYSuDiseno()
    {
        var f = Ficha(Diagrama(new FakeEstructura().Nodos()), 8);
        Assert.Equal("2", Fila(f, "Nivel de ensamble")); Assert.Equal("D2 · v1", Fila(f, "Pertenece a"));
        var propio = Ficha(Diagrama(new FakeEstructura().Nodos()), 4);                              // un nodo del primer nivel no lo repite
        Assert.DoesNotContain(propio.GetProperty("f").EnumerateArray(), r => r[0].GetString() == "Pertenece a");
    }

    [Fact]
    public void Ficha_DeRutaYOperacion_TraeTiempoManualYCalidad_SinCosto()
    {
        var nodos = new FakeEstructura { ConRutas = true }.Nodos();
        nodos.First(n => n.ID == 12).COD_PROCESO = "ENS"; nodos.First(n => n.ID == 12).COD_SUB_PROCESO = "SOL";
        var c = Diagrama(nodos);
        var op = Ficha(c, 12);
        Assert.Equal("Operación", op.GetProperty("t").GetString()); Assert.Equal("rut", op.GetProperty("c").GetString());
        Assert.Equal("30 MIN", Fila(op, "Tiempo")); Assert.Equal("Sí", Fila(op, "Manual")); Assert.Equal("Sí", Fila(op, "Control de calidad"));
        Assert.Equal("ENS", Fila(op, "Proceso")); Assert.Equal("SOL", Fila(op, "Subproceso"));
        Assert.DoesNotContain(Ficha(c, 11).GetProperty("f").EnumerateArray(), r => r[0].GetString() == "Proceso");   // sin dato no hay renglon vacio
        Assert.False(op.TryGetProperty("x", out _));                                                  // las rutas no tienen costo
        var ruta = Ficha(c, 9);
        Assert.Equal("Ruta de producción", ruta.GetProperty("t").GetString()); Assert.Equal("RT-D2", ruta.GetProperty("k").GetString());
    }

    [Fact]
    public void Ficha_DeVersionSectorYComponente_TraeCostoYDatosPropios()
    {
        var c = Diagrama(new FakeEstructura().Nodos());
        var v = Ficha(c, 1); Assert.Equal("Versión", v.GetProperty("t").GetString()); Assert.Equal("D1 · v1", Fila(v, "Diseño")); Assert.Equal("Editable", Fila(v, "Estado"));
        var s = Ficha(c, 2); Assert.Equal("Sector", s.GetProperty("t").GetString()); Assert.Equal("FRENT", s.GetProperty("k").GetString());
        var m = Ficha(c, 3); Assert.Equal("Componente", m.GetProperty("t").GetString()); Assert.Equal("PROCE", m.GetProperty("k").GetString()); Assert.Equal("FRENT", Fila(m, "Sector"));
        Assert.All(new[] { v, s, m }, x => Assert.Equal("100", x.GetProperty("x")[1].GetString()!.Split('.', ',')[0]));   // costo 100,0000
    }

    [Fact]
    public void Ficha_AvisaCuandoUnNodoNoSeExpande()
    {
        var nodos = new FakeEstructura().Nodos();
        nodos.First(n => n.ID == 5).FLG_CICLO = true;
        nodos.First(n => n.ID == 4).FLG_TRUNCADO = true;
        var c = Diagrama(nodos);
        Assert.Contains("ciclo", Ficha(c, 5).GetProperty("w")[0].GetString());
        Assert.Contains("máximo de niveles", Ficha(c, 4).GetProperty("w")[0].GetString());
    }

    [Fact]
    public void Ficha_LosDatosNuncaSeInterpretanComoHtml()
    {
        var nodos = new FakeEstructura().Nodos();
        nodos.First(n => n.ID == 4).DESCRIPCION = "<img src=x onerror=alert(1)>";
        var c = Diagrama(nodos);
        Assert.Equal("<img src=x onerror=alert(1)>", Ficha(c, 4).GetProperty("n").GetString());         // viaja intacto como dato...
        Assert.Empty(c.FindAll("g[data-nodo] img"));                                                  // ...y no se convierte en elemento
        Assert.DoesNotContain("innerHTML", DiagramaFicha.Script);                                      // el script solo usa textContent
    }

    [Fact]
    public void Ficha_ElScriptSeInstalaSoloSiNoExiste_Y_SeOcultaAlRedibujar()
    {
        Services.AddMudServices(); JSInterop.Mode = JSRuntimeMode.Loose;
        RenderComponent<MudPopoverProvider>();
        JSInterop.SetupVoid("dgmFicha.ocultar").SetException(new Microsoft.JSInterop.JSException("dgmFicha no existe"));    // primera vez: el script aun no esta
        RenderComponent<DisenosDiagramaEstructura>(ps => ps.Add(x => x.Nodos, new FakeEstructura().Nodos()));
        var evals = JSInterop.Invocations.Where(i => i.Identifier == "eval").ToList();
        Assert.NotEmpty(evals);
        Assert.Contains("window.dgmFicha", (string)evals[0].Arguments[0]!);                           // lo que se evalua define el script de la ficha
        Assert.Contains(JSInterop.Invocations, i => i.Identifier == "dgmFicha.ocultar");              // y antes se intento ocultar la ficha abierta
    }

    // ---------- ruta de operaciones de la version (el editor nuevo se prueba en DisenosRutaOperacionesTests) ----------
    [Fact]
    public async Task Estructura_TieneBotonParaEditarLaRuta()
    {
        var p = Prov(); await AbreEstructura(p);
        p.WaitForAssertion(() => Assert.Equal(8, p.FindAll("tr[data-nodo]").Count));
        p.Find("[data-accion='editar-ruta']").Click();
        p.WaitForAssertion(() => Assert.Contains("Ruta de operaciones · D1 v1", p.Markup));                   // el editor nuevo (matriz asignada como copia)
        Assert.Contains(_f.Calls, c => c.M == "GET" && c.U.Contains("disenos-matrices/D1/1/ruta"));
    }

    // ---------- insumo ----------
    [Fact]
    public async Task Insumo_CalculaElTotal_YGuardaConLosValores()
    {
        var p = Prov();
        await Abre<DisenosEstructuraInsumoDialog>(p, new DialogParameters { ["CodDiseno"] = "D1", ["NumVersion"] = 1, ["CodSector"] = "FRENT", ["CodComponente"] = "PROCE" });
        p.WaitForAssertion(() => p.Find("[data-total]"));
        var combo = p.FindComponent<KONSolutions.Web.Pages.Shared.AriaCombo<KONSolutions.Shared.Models.Estandar.Productos>>();
        await p.InvokeAsync(() => combo.Instance.SearchFunc!("pro", CancellationToken.None));
        await p.InvokeAsync(() => combo.Instance.ValueChanged.InvokeAsync("P9"));
        p.Find("input[data-campo='consumo']").Change("2");
        p.Find("input[data-campo='merma']").Change("10");
        p.Find("input[data-campo='precio']").Change("100");
        p.WaitForAssertion(() => Assert.Contains("220", p.Find("[data-total]").TextContent));   // 2 x 1.10 x 100
        p.Find("[data-accion='guardar']").Click();
        p.WaitForAssertion(() => Assert.Contains(_f.Calls, c => c.M == "POST" && c.U.EndsWith("/insumo")));
        var b = _f.Calls.Last(c => c.U.EndsWith("/insumo")).B!.Value;
        Assert.Equal("P9", Prop(b, "COD_PRODUCTO_COMPONENTE").GetString());
        Assert.Equal(2m, Prop(b, "CAN_CONSUMO").GetDecimal()); Assert.Equal(10m, Prop(b, "TAS_MERMA").GetDecimal()); Assert.Equal(100m, Prop(b, "PRE_UNITARIO").GetDecimal());
        Assert.Equal("FRENT", Prop(b, "COD_SECTOR_PRODUCTO").GetString()); Assert.Equal("PROCE", Prop(b, "COD_COMPONENTE_PRODUCTO").GetString());
        Assert.Equal(JsonValueKind.Null, Prop(b, "NUM_SECUENCIA").ValueKind);     // insumo nuevo
    }

    // ---------- Grupo > Familia > Sub familia enlazados ----------
    static string[] Ids(Bunit.IRenderedComponent<KONSolutions.Web.Pages.Shared.AriaCombo<KONSolutions.Shared.Common.ComboboxItem>> c) => c.Instance.Items.Select(i => i.Id).ToArray();
    Bunit.IRenderedComponent<KONSolutions.Web.Pages.Shared.AriaCombo<KONSolutions.Shared.Common.ComboboxItem>> Combo(IRenderedComponent<MudDialogProvider> p, string label) =>
        p.FindComponents<KONSolutions.Web.Pages.Shared.AriaCombo<KONSolutions.Shared.Common.ComboboxItem>>().First(c => c.Instance.Label == label);

    [Fact]
    public async Task Insumo_GrupoFamiliaSubfamilia_SeEncadenan_YFiltranLaBusquedaDeProducto()
    {
        var p = Prov();
        await Abre<DisenosEstructuraInsumoDialog>(p, new DialogParameters { ["CodDiseno"] = "D1", ["NumVersion"] = 1, ["CodSector"] = "FRENT", ["CodComponente"] = "PROCE" });
        p.WaitForAssertion(() => Assert.Equal(new[] { "G1", "G2" }, Ids(Combo(p, "Grupo"))));
        Assert.True(p.FindAll("[data-panel='clasificacion'] input")[1].HasAttribute("disabled"));          // la familia espera al grupo
        await p.InvokeAsync(() => Combo(p, "Grupo").Instance.ValueChanged.InvokeAsync("G1"));
        p.WaitForAssertion(() => Assert.Equal(new[] { "F1", "F2" }, Ids(Combo(p, "Familia"))));
        Assert.Contains(_f.Calls, c => c.U.Contains("familias-producto/combobox/G1"));
        await p.InvokeAsync(() => Combo(p, "Familia").Instance.ValueChanged.InvokeAsync("F1"));
        p.WaitForAssertion(() => Assert.Equal(new[] { "S1", "S2" }, Ids(Combo(p, "Sub familia"))));
        Assert.Contains(_f.Calls, c => c.U.Contains("sub-familias-producto/combobox/G1/F1"));
        await p.InvokeAsync(() => Combo(p, "Sub familia").Instance.ValueChanged.InvokeAsync("S1"));
        var prod = p.FindComponent<KONSolutions.Web.Pages.Shared.AriaCombo<KONSolutions.Shared.Models.Estandar.Productos>>();
        await p.InvokeAsync(() => prod.Instance.SearchFunc!("pro", CancellationToken.None));
        Assert.Contains(_f.Calls, c => c.U.Contains("productos/search") && c.U.Contains("COD_GRUPO_PRODUCTO=G1") && c.U.Contains("COD_FAMILIA_PRODUCTO=F1") && c.U.Contains("COD_SUB_FAMILIA_PRODUCTO=S1"));
        await p.InvokeAsync(() => Combo(p, "Grupo").Instance.ValueChanged.InvokeAsync("G2"));             // cambiar el grupo limpia familia y sub familia
        p.WaitForAssertion(() => Assert.Empty(Ids(Combo(p, "Sub familia"))));
        await p.InvokeAsync(() => prod.Instance.SearchFunc!("pro", CancellationToken.None));
        var ultima = _f.Calls.Last(c => c.U.Contains("productos/search")).U;
        Assert.Contains("COD_GRUPO_PRODUCTO=G2", ultima); Assert.DoesNotContain("COD_FAMILIA_PRODUCTO", ultima);
    }

    [Fact]
    public async Task Insumo_AlElegirProducto_RellenaSuGrupoFamiliaYSubfamilia()
    {
        var p = Prov();
        await Abre<DisenosEstructuraInsumoDialog>(p, new DialogParameters { ["CodDiseno"] = "D1", ["NumVersion"] = 1, ["CodSector"] = "FRENT", ["CodComponente"] = "PROCE" });
        p.WaitForAssertion(() => p.Find("[data-total]"));
        var prod = p.FindComponent<KONSolutions.Web.Pages.Shared.AriaCombo<KONSolutions.Shared.Models.Estandar.Productos>>();
        await p.InvokeAsync(() => prod.Instance.SearchFunc!("pro", CancellationToken.None));
        await p.InvokeAsync(() => prod.Instance.ValueChanged.InvokeAsync("P9"));                          // P9: G1 / F1 / S1
        p.WaitForAssertion(() => Assert.Equal("G1", Combo(p, "Grupo").Instance.Value));
        Assert.Equal("F1", Combo(p, "Familia").Instance.Value); Assert.Equal("S1", Combo(p, "Sub familia").Instance.Value);
        Assert.Equal(new[] { "S1", "S2" }, Ids(Combo(p, "Sub familia")));
    }

    [Fact]
    public async Task Insumo_LosDisenosAEnlazar_SeFiltranPorLaClasificacion()
    {
        var p = Prov();
        await Abre<DisenosEstructuraInsumoDialog>(p, new DialogParameters { ["CodDiseno"] = "D1", ["NumVersion"] = 1, ["CodSector"] = "FRENT", ["CodComponente"] = "PROCE" });
        p.WaitForAssertion(() => Assert.Equal(new[] { "D1", "D2" }, Ids(Combo(p, "Diseño enlazado"))));    // sin filtro: los del combo general
        await p.InvokeAsync(() => Combo(p, "Grupo").Instance.ValueChanged.InvokeAsync("G2"));
        p.WaitForAssertion(() => Assert.Equal(new[] { "D2", "D3" }, Ids(Combo(p, "Diseño enlazado"))));    // solo los del grupo G2
        await p.InvokeAsync(() => Combo(p, "Familia").Instance.ValueChanged.InvokeAsync("F2"));
        p.WaitForAssertion(() => Assert.Equal(new[] { "D3" }, Ids(Combo(p, "Diseño enlazado"))));
        await p.InvokeAsync(() => Combo(p, "Grupo").Instance.ValueChanged.InvokeAsync(""));                // limpiar el grupo vuelve a todos
        p.WaitForAssertion(() => Assert.Equal(new[] { "D1", "D2" }, Ids(Combo(p, "Diseño enlazado"))));
    }

    [Fact]
    public async Task Insumo_SiElProductoEsSubensamble_AvisaYNoPideElPrecio()
    {
        var p = Prov();
        await Abre<DisenosEstructuraInsumoDialog>(p, new DialogParameters { ["CodDiseno"] = "D1", ["NumVersion"] = 1, ["CodSector"] = "FRENT", ["CodComponente"] = "PROCE" });
        p.WaitForAssertion(() => p.Find("[data-total]"));
        var combo = p.FindComponent<KONSolutions.Web.Pages.Shared.AriaCombo<KONSolutions.Shared.Models.Estandar.Productos>>();
        await p.InvokeAsync(() => combo.Instance.SearchFunc!("mod", CancellationToken.None));
        await p.InvokeAsync(() => combo.Instance.ValueChanged.InvokeAsync("P2"));
        p.WaitForAssertion(() => Assert.NotNull(p.Find("[data-ensamble]")));
        Assert.True(p.Find("input[data-campo='precio']").HasAttribute("disabled"));
    }

    [Fact]
    public async Task Insumo_SiLaApiRechaza_MuestraElMotivoYNoCierra()
    {
        _f.Falla = true; _f.MensajeFalla = "El insumo es un subensamble que contiene a esta misma version: formaria un ciclo.";
        var p = Prov();
        await Abre<DisenosEstructuraInsumoDialog>(p, new DialogParameters { ["CodDiseno"] = "D1", ["NumVersion"] = 1, ["CodSector"] = "FRENT", ["CodComponente"] = "PROCE" });
        p.WaitForAssertion(() => p.Find("[data-total]"));
        var combo = p.FindComponent<KONSolutions.Web.Pages.Shared.AriaCombo<KONSolutions.Shared.Models.Estandar.Productos>>();
        await p.InvokeAsync(() => combo.Instance.SearchFunc!("pro", CancellationToken.None));
        await p.InvokeAsync(() => combo.Instance.ValueChanged.InvokeAsync("P9"));
        p.Find("input[data-campo='consumo']").Change("1");
        p.Find("[data-accion='guardar']").Click();
        p.WaitForAssertion(() => Assert.Contains("ciclo", p.Find("[data-error]").TextContent));
        Assert.NotEmpty(p.FindAll(".mud-dialog"));
    }

    [Fact]
    public async Task Insumo_EnlazaConDisenoYVersion_YNoEnviaPrecio()
    {
        var p = Prov();
        await Abre<DisenosEstructuraInsumoDialog>(p, new DialogParameters { ["CodDiseno"] = "D1", ["NumVersion"] = 1, ["CodSector"] = "FRENT", ["CodComponente"] = "PROCE" });
        p.WaitForAssertion(() => p.Find("[data-total]"));
        var prod = p.FindComponent<KONSolutions.Web.Pages.Shared.AriaCombo<KONSolutions.Shared.Models.Estandar.Productos>>();
        await p.InvokeAsync(() => prod.Instance.SearchFunc!("pro", CancellationToken.None));
        await p.InvokeAsync(() => prod.Instance.ValueChanged.InvokeAsync("P9"));          // P9 no tiene diseno asociado
        Assert.Empty(p.FindAll("[data-ensamble]"));
        p.Find("input[data-campo='consumo']").Change("2");
        p.Find("input[data-campo='precio']").Change("5");
        var enl = p.FindComponents<KONSolutions.Web.Pages.Shared.AriaCombo<KONSolutions.Shared.Common.ComboboxItem>>().First(c => c.Instance.Label == "Diseño enlazado");
        await p.InvokeAsync(() => enl.Instance.ValueChanged.InvokeAsync("D2"));
        p.WaitForAssertion(() => Assert.Single(p.FindComponents<KONSolutions.Web.Pages.Shared.AriaCombo<DisenoVersion>>()));
        await p.InvokeAsync(() => p.FindComponent<KONSolutions.Web.Pages.Shared.AriaCombo<DisenoVersion>>().Instance.ValueChanged.InvokeAsync("2"));
        p.WaitForAssertion(() => Assert.NotNull(p.Find("[data-ensamble]")));
        Assert.True(p.Find("input[data-campo='precio']").HasAttribute("disabled"));
        p.Find("[data-accion='guardar']").Click();
        p.WaitForAssertion(() => Assert.Contains(_f.Calls, c => c.M == "POST" && c.U.EndsWith("/insumo")));
        var b = _f.Calls.Last(c => c.U.EndsWith("/insumo")).B!.Value;
        Assert.Equal("D2", Prop(b, "COD_DISENO_ENLACE").GetString()); Assert.Equal(2, Prop(b, "NUM_VERSION_ENLACE").GetInt32());
        Assert.Equal(0m, Prop(b, "PRE_UNITARIO").GetDecimal());
    }

    [Fact]
    public async Task Insumo_QuitarEnlace_VuelveAPedirPrecio()
    {
        var p = Prov();
        await Abre<DisenosEstructuraInsumoDialog>(p, new DialogParameters { ["CodDiseno"] = "D1", ["NumVersion"] = 1, ["CodSector"] = "FRENT", ["CodComponente"] = "PROCE" });
        p.WaitForAssertion(() => p.Find("[data-total]"));
        var prod = p.FindComponent<KONSolutions.Web.Pages.Shared.AriaCombo<KONSolutions.Shared.Models.Estandar.Productos>>();
        await p.InvokeAsync(() => prod.Instance.SearchFunc!("mod", CancellationToken.None));
        await p.InvokeAsync(() => prod.Instance.ValueChanged.InvokeAsync("P2"));          // P2 si tiene diseno D2 v1
        p.WaitForAssertion(() => Assert.NotNull(p.Find("[data-ensamble]")));
        p.Find("[data-accion='quitar-enlace']").Click();
        p.WaitForAssertion(() => Assert.Empty(p.FindAll("[data-ensamble]")));
        Assert.False(p.Find("input[data-campo='precio']").HasAttribute("disabled"));
    }

    // ---------- sector ----------
    [Fact]
    public async Task Sector_GuardaElCodigoEnMayusculas()
    {
        var p = Prov();
        await Abre<DisenosEstructuraSectorDialog>(p, new DialogParameters { ["CodDiseno"] = "D1", ["NumVersion"] = 1 });
        p.Find("input[data-campo='cod']").Change("frent");
        p.Find("input[data-campo='des']").Change("Frente");
        p.Find("[data-accion='guardar']").Click();
        p.WaitForAssertion(() => Assert.Contains(_f.Calls, c => c.M == "POST" && c.U.EndsWith("/sector")));
        var b = _f.Calls.Last(c => c.U.EndsWith("/sector")).B!.Value;
        Assert.Equal("FRENT", Prop(b, "COD_SECTOR_PRODUCTO").GetString()); Assert.Equal("Frente", Prop(b, "DES_SECTOR_PRODUCTO").GetString());
    }

    // ---------- copiar ----------
    [Fact]
    public async Task CopiarVersion_EnviaOrigenYDestino()
    {
        var p = Prov();
        await Abre<DisenosCopiarVersionDialog>(p, new DialogParameters { ["CodDiseno"] = "D1", ["DesDiseno"] = "DISENO UNO", ["NumVersion"] = 1 });
        p.Find("input[data-campo='descripcion']").Change("Mi copia");
        p.Find("[data-accion='copiar']").Click();
        p.WaitForAssertion(() => Assert.Contains(_f.Calls, c => c.U.EndsWith("/copiar-version")));
        var b = _f.Calls.Last(c => c.U.EndsWith("/copiar-version")).B!.Value;
        Assert.Equal("D1", Prop(b, "COD_DISENO_ORIGEN").GetString()); Assert.Equal(1, Prop(b, "NUM_VERSION_ORIGEN").GetInt32());
        Assert.Equal("D1", Prop(b, "COD_DISENO_DESTINO").GetString()); Assert.Equal("Mi copia", Prop(b, "DES_VERSION").GetString());
    }

    [Fact]
    public async Task CopiarSegmento_Reemplazar_AvisaYEnviaElModo()
    {
        var p = Prov();
        await Abre<DisenosCopiarSegmentoDialog>(p, new DialogParameters { ["CodDiseno"] = "D1", ["NumVersion"] = 1, ["CodSector"] = "FRENT", ["CodComponente"] = "PROCE" });
        Assert.Contains("componente PROCE", p.Find("[data-origen]").TextContent);
        var combos = p.FindComponents<KONSolutions.Web.Pages.Shared.AriaCombo<DisenoVersion>>();
        p.WaitForAssertion(() => Assert.Single(p.FindComponents<KONSolutions.Web.Pages.Shared.AriaCombo<DisenoVersion>>()));
        await p.InvokeAsync(() => p.FindComponent<KONSolutions.Web.Pages.Shared.AriaCombo<DisenoVersion>>().Instance.ValueChanged.InvokeAsync("2"));
        Assert.DoesNotContain("No se puede deshacer", p.Markup);
        p.FindAll("input[type=radio]").Last().Click();
        p.WaitForAssertion(() => Assert.Contains("No se puede deshacer", p.Markup));
        p.Find("[data-accion='copiar']").Click();
        p.WaitForAssertion(() => Assert.Contains(_f.Calls, c => c.U.EndsWith("/copiar-segmento")));
        var b = _f.Calls.Last(c => c.U.EndsWith("/copiar-segmento")).B!.Value;
        Assert.Equal("REEMPLAZAR", Prop(b, "MODO").GetString()); Assert.Equal(2, Prop(b, "NUM_VERSION_DESTINO").GetInt32());
        Assert.Equal("PROCE", Prop(b, "COD_COMPONENTE_ORIGEN").GetString()); Assert.Equal("FRENT", Prop(b, "COD_SECTOR_DESTINO").GetString());
    }

    // ---------- galeria ----------
    [Fact]
    public void Galeria_Carrusel_Y_Panal_SeAlternan_YSeRecuerda()
    {
        Services.AddMudServices(); JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton(new HttpClient(_f) { BaseAddress = new Uri("http://x/") });
        Services.AddBlazoredLocalStorage(); Services.AddScoped<ErrorLogService>();
        _f.Imagenes = 6;
        RenderComponent<MudPopoverProvider>(); RenderComponent<MudSnackbarProvider>(); RenderComponent<MudDialogProvider>();
        var c = RenderComponent<DisenosGaleriaImagenes>(ps => ps.Add(x => x.Identificador, "D1"));
        c.WaitForAssertion(() => Assert.Contains("6 imagen", c.Find("[data-cuenta]").TextContent));    // el PDF no cuenta
        Assert.NotEmpty(c.FindAll("[data-vista='carrusel']")); Assert.Empty(c.FindAll("[data-vista='panal']"));
        c.Find("[data-modo='panal']").Click();
        c.WaitForAssertion(() => Assert.NotEmpty(c.FindAll("[data-vista='panal']")));
        Assert.Empty(c.FindAll("[data-vista='carrusel']"));
        Assert.Equal(6, c.FindAll("[data-hex]").Count);
        Assert.Equal(new[] { 5, 1 }, c.FindAll(".gal-fila").Select(f => f.QuerySelectorAll("[data-hex]").Length).ToArray());  // filas de 5 y 4: seis imagenes dejan 5 + 1
        JSInterop.VerifyInvoke("localStorage.setItem");
    }

    [Fact]
    public void Galeria_PanalAlternaFilasLargasYCortas()
    {
        Services.AddMudServices(); JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton(new HttpClient(_f) { BaseAddress = new Uri("http://x/") });
        Services.AddBlazoredLocalStorage(); Services.AddScoped<ErrorLogService>();
        _f.Imagenes = 10;
        RenderComponent<MudPopoverProvider>(); RenderComponent<MudSnackbarProvider>(); RenderComponent<MudDialogProvider>();
        JSInterop.Setup<string?>("localStorage.getItem", "diseno.galeria.modo").SetResult("panal");
        var c = RenderComponent<DisenosGaleriaImagenes>(ps => ps.Add(x => x.Identificador, "D1"));
        c.WaitForAssertion(() => Assert.Equal(10, c.FindAll("[data-hex]").Count));          // el modo guardado ("panal") se aplica al abrir
        Assert.Equal(new[] { 5, 4, 1 }, c.FindAll(".gal-fila").Select(f => f.QuerySelectorAll("[data-hex]").Length).ToArray());
    }

    [Fact]
    public void Galeria_AlcanceVersion_LeeLaCarpetaDeLaVersion()
    {
        Services.AddMudServices(); JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton(new HttpClient(_f) { BaseAddress = new Uri("http://x/") });
        Services.AddBlazoredLocalStorage(); Services.AddScoped<ErrorLogService>();
        RenderComponent<MudPopoverProvider>(); RenderComponent<MudSnackbarProvider>(); RenderComponent<MudDialogProvider>();
        var c = RenderComponent<DisenosGaleriaImagenes>(ps => ps.Add(x => x.Identificador, "D1").Add(x => x.IdentificadorVersion, "D1-V01"));
        c.WaitForAssertion(() => Assert.Contains(_f.Calls, x => x.U.Contains("documentos-carpeta") && x.U.EndsWith("/D1")));
        c.Find("[data-alcance='version']").Click();
        c.WaitForAssertion(() => Assert.Contains(_f.Calls, x => x.U.Contains("documentos-carpeta") && x.U.EndsWith("/D1-V01")));
    }

    [Fact]
    public void Galeria_SinImagenes_MuestraElVacio_YSoloLecturaNoOfreceSubir()
    {
        Services.AddMudServices(); JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton(new HttpClient(_f) { BaseAddress = new Uri("http://x/") });
        Services.AddBlazoredLocalStorage(); Services.AddScoped<ErrorLogService>();
        _f.Imagenes = 0;
        RenderComponent<MudPopoverProvider>(); RenderComponent<MudSnackbarProvider>(); RenderComponent<MudDialogProvider>();
        var c = RenderComponent<DisenosGaleriaImagenes>(ps => ps.Add(x => x.Identificador, "D1").Add(x => x.ReadOnly, true));
        c.WaitForAssertion(() => Assert.NotEmpty(c.FindAll("[data-vacio]")));
        Assert.Empty(c.FindAll("[data-accion='subir']"));
    }
}
