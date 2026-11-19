using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Blazored.LocalStorage;
using Bunit;
using KONSolutions.Shared.Common;
using KONSolutions.Shared.Models;
using KONSolutions.Shared.Models.Ingenieria;
using KONSolutions.Web.Pages.Ingenieria;
using KONSolutions.Web.Pages.Shared;
using KONSolutions.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using Xunit;

// Ruta de operaciones de una version de diseno (scripts SQL 130 y 132): asignar una matriz como copia, editar la copia, guardar como matriz, separar rutas compartidas. API simulada.
public class FakeRutaOperaciones : HttpMessageHandler
{
    public List<(string M, string U, JsonElement? B)> Calls = new();
    public string? MensajeFalla;
    public DisenosRutaOperacionesInfo Info = new() { COD_DISENO = "D1", NUM_VERSION = 1 };
    public List<MatrizParaVersion> Matrices = new()
    {
        new() { COD_RUTA_PRODUCCION = "RT7", DES_RUTA_PRODUCCION = "MATRIZ DRON", CAN_ELEMENTOS = 10, CAN_HOJAS_CATALOGO = 6, VAL_SAM_TOTAL = 120.5m },
        new() { COD_RUTA_PRODUCCION = "RT1", DES_RUTA_PRODUCCION = "MATRIZ POLO", DES_SUB_FAMILIA_PRODUCTO = "MANGA CORTA", CAN_ELEMENTOS = 34, CAN_HOJAS_CATALOGO = 20, VAL_SAM_TOTAL = 9.157m, FLG_SUGERIDA = true },
    };
    public List<RutaNodo> Nodos = new()
    {
        new() { NUM_ESTRUCTURA_RUTA_PROCESO = 1, DES_TIPO_ELEMENTO = "PRC", COD_PROCESO = "COSTU", DES_PROCESO = "COSTURA", NIVEL = 0, VAL_SAM_PRENDA = 9.157m, VAL_SAM_NODO = 9.157m },
        new() { NUM_ESTRUCTURA_RUTA_PROCESO = 2, NUM_ESTRUCTURA_RUTA_PROCESO_PADRE = 1, DES_TIPO_ELEMENTO = "OPE", COD_OPERACION = "OP1", DES_OPERACION = "UNIR HOMBROS", NIVEL = 1, FLG_HOJA = true, VAL_FRECUENCIA = 1, VAL_SAM_PROPIO = 1.2m },
    };

    public void ConRuta(string cod = "RT50", string? origen = "RT1", string des = "MATRIZ POLO", bool matriz = true, int usadaPor = 1, int bloqueadas = 0, bool bloqueada = false)
    {
        Info = new() { COD_DISENO = "D1", NUM_VERSION = 1, COD_RUTA_PRODUCCION = cod, DES_RUTA_PRODUCCION = des, COD_TIPO_RUTA_PRODUCCION = "PRD", USADA_POR = usadaPor, BLOQUEADAS = bloqueadas, FLG_BLOQUEADO = bloqueada,
                 ELEMENTOS = 34, VAL_SAM_TOTAL = 9.157m, COD_RUTA_ORIGEN = origen, DES_RUTA_ORIGEN = origen is null ? null : des, FLG_ORIGEN_MATRIZ = matriz, FEC_ORIGEN = origen is null ? null : new DateTime(2026, 11, 18) };
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage r, CancellationToken ct)
    {
        var u = r.RequestUri!.PathAndQuery;
        JsonElement? body = r.Content is null ? null : JsonDocument.Parse(await r.Content.ReadAsStringAsync(ct)).RootElement.Clone();
        Calls.Add((r.Method.Method, u, body));
        HttpResponseMessage Ok(object? data) => new(HttpStatusCode.OK) { Content = JsonContent.Create(new { Success = true, Data = data }) };
        HttpResponseMessage Mal(string msg) => new(HttpStatusCode.BadRequest) { Content = JsonContent.Create(new { Success = false, Message = msg }) };
        if (r.Method == HttpMethod.Get)
        {
            if (u.EndsWith("/disenos-matrices/D1/1/ruta")) return Ok(Info);
            if (u.EndsWith("/disenos-matrices/D1/1/matrices")) return Ok(Matrices);
            if (u.Contains("/ruta-catalogos")) return Ok(new { Rutas = new[] { new { Id = "RT9", Texto = "RUTA ANTIGUA" } }, TiposRuta = new[] { new { Id = "PRD", Texto = "PRODUCCION" } } });
            if (u.Contains("/arbol")) return Ok(Nodos);
            return Ok(new object[0]);
        }
        if (MensajeFalla != null) return Mal(MensajeFalla);
        if (r.Method == HttpMethod.Post && u.EndsWith("/disenos-matrices/asignar")) { ConRuta("RT50", body!.Value.EnumerateObject().First(x => x.Name.Equals("COD_MATRIZ", StringComparison.OrdinalIgnoreCase)).Value.GetString(), "MATRIZ POLO"); return Ok(new { COD_RUTA_PRODUCCION = "RT50", ELEMENTOS_COPIADOS = 34, PRECEDENCIAS_COPIADAS = 80 }); }
        if (r.Method == HttpMethod.Post && u.EndsWith("/independizar")) { Info.USADA_POR = 1; Info.BLOQUEADAS = 0; Info.COD_RUTA_PRODUCCION = "RT51"; return Ok(new { COD_RUTA_PRODUCCION = "RT51", ELEMENTOS_COPIADOS = 34, PRECEDENCIAS_COPIADAS = 80, FLG_COPIADA = true }); }
        if (r.Method == HttpMethod.Post && u.EndsWith("/guardar-como-matriz")) return Ok(new { COD_RUTA_PRODUCCION = "RT60", ELEMENTOS_COPIADOS = 34, PRECEDENCIAS_COPIADAS = 80 });
        if (r.Method == HttpMethod.Post && u.EndsWith("/ruta/copiar")) { ConRuta("RT52", "RT9", "COPIA RUTA ANTIGUA", false); return Ok(new { COD_RUTA_PRODUCCION = "RT52", ELEMENTOS_COPIADOS = 5 }); }
        if (r.Method == HttpMethod.Post && u.EndsWith("/ruta/crear")) { ConRuta("RT53", null, "RUTA NUEVA", false); return Ok(new { COD_RUTA_PRODUCCION = "RT53" }); }
        if (r.Method == HttpMethod.Post && u.EndsWith("/ruta/asignar")) { Info = new() { COD_DISENO = "D1", NUM_VERSION = 1 }; return Ok(true); }
        if (r.Method == HttpMethod.Post && u.EndsWith("/desbloquear")) { Info.FLG_BLOQUEADO = false; Info.BLOQUEADAS = 0; return Ok(true); }
        return Ok(true);
    }
}

public class DisenosRutaOperacionesTests : TestContext
{
    readonly FakeRutaOperaciones _f = new();

    IRenderedComponent<MudDialogProvider> Prov()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton(new HttpClient(_f) { BaseAddress = new Uri("http://x/") });
        Services.AddBlazoredLocalStorage();
        Services.AddScoped<ErrorLogService>(); Services.AddScoped<ConfigService>(); Services.AddScoped<EtiquetasService>();
        Services.AddSingleton<Microsoft.Extensions.Configuration.IConfiguration>(new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build());
        Services.AddScoped<AccessLogService>(); Services.AddScoped<MenuService>(); Services.AddScoped<ControlCambiosClientService>();
        ComponentFactories.AddStub<AccionesBotones>();
        RenderComponent<MudPopoverProvider>(); RenderComponent<MudSnackbarProvider>();
        return RenderComponent<MudDialogProvider>();
    }

    async Task<IRenderedComponent<MudDialogProvider>> Abre(string esperar)
    {
        var p = Prov();
        var ds = Services.GetRequiredService<IDialogService>();
        await p.InvokeAsync(async () => await ds.ShowAsync<DisenosRutaOperacionesDialog>("t", new DialogParameters { ["CodDiseno"] = "D1", ["NumVersion"] = 1 }, new DialogOptions { MaxWidth = MaxWidth.ExtraLarge }));
        p.WaitForAssertion(() => Assert.Contains(esperar, p.Markup));
        return p;
    }

    static bool Deshabilitado(AngleSharp.Dom.IElement e) => e.HasAttribute("disabled") || (e.ClassName ?? "").Contains("mud-disabled");
    static JsonElement Prop(JsonElement e, string n) => e.EnumerateObject().First(x => string.Equals(x.Name, n, StringComparison.OrdinalIgnoreCase)).Value;
    (string M, string U, JsonElement? B) Ultima(string metodo, string fin) => _f.Calls.Last(c => c.M == metodo && c.U.EndsWith(fin));
    int Cuantas(string metodo, string fin) => _f.Calls.Count(c => c.M == metodo && c.U.EndsWith(fin));
    async Task Elige(IRenderedComponent<MudDialogProvider> p, int combo, string valor) => await p.InvokeAsync(() => p.FindComponents<AriaCombo<ComboboxItem>>().ElementAt(combo).Instance.ValueChanged.InvokeAsync(valor));

    // ======================= sin ruta =======================
    [Fact]
    public async Task SinRuta_OfreceMatrizCopiarYEmpezarEnBlanco_ConLasSugeridasPrimero()
    {
        var p = await Abre("Esta versión no tiene ruta de operaciones");
        foreach (var t in new[] { "matriz", "copiar", "vacia" }) Assert.NotEmpty(p.FindAll($"[data-tarjeta='{t}']"));
        Assert.True(Deshabilitado(p.Find("[data-accion='asignar-matriz']")));
        var textos = p.FindComponents<AriaCombo<ComboboxItem>>().First().Instance.Items.Select(i => i.Texto).ToList();
        Assert.Equal(2, textos.Count);
        Assert.StartsWith("★ MATRIZ POLO · MANGA CORTA · 34 elementos", textos.First(t => t.Contains("POLO")));   // la de la sub familia del diseno lleva la estrella
        Assert.Contains("cualquier producto", textos.First(t => t.Contains("DRON")));
        Assert.Empty(p.FindAll("[data-zona='barra']"));                                                          // sin ruta no hay editor
    }

    [Fact]
    public async Task SinMatrices_LoDiceYSugiereDondeCrearlas()
    {
        _f.Matrices.Clear();
        var p = await Abre("Esta versión no tiene ruta");
        Assert.Contains("Todavía no hay matrices", p.Find("[data-vacio='matrices']").TextContent);
    }

    [Fact]
    public async Task AsignarMatriz_EnviaLaMatrizYLaFecha_YMuestraLaCopiaYSuOrigen()
    {
        var p = await Abre("Esta versión no tiene ruta");
        await Elige(p, 0, "RT1");
        Assert.False(Deshabilitado(p.Find("[data-accion='asignar-matriz']")));
        await p.InvokeAsync(() => p.Find("[data-accion='asignar-matriz']").Click());
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-origen='1']")));
        var b = Ultima("POST", "/disenos-matrices/asignar").B!.Value;
        Assert.Equal("D1", Prop(b, "COD_DISENO").GetString()); Assert.Equal(1, Prop(b, "NUM_VERSION").GetInt32()); Assert.Equal("RT1", Prop(b, "COD_MATRIZ").GetString());
        Assert.False(Prop(b, "REEMPLAZAR").GetBoolean());
        Assert.StartsWith(DateTime.Today.ToString("yyyy-MM-dd"), Prop(b, "FEC_TIEMPOS").GetString());
        Assert.Contains("Copia de la matriz MATRIZ POLO", p.Find("[data-origen='1']").TextContent);
        Assert.Contains("RT50", p.Find("[data-ruta='1']").TextContent);
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-zona='barra']")));                              // y se ve el editor de la copia
        p.WaitForAssertion(() => Assert.Contains("COSTURA", p.Find("[data-tabla='arbol']").TextContent));
        Assert.Contains(_f.Calls, c => c.M == "GET" && c.U.Contains("/rutas-tiempos/RT50/arbol"));
    }

    [Fact]
    public async Task AsignarMatriz_SiLaApiRechaza_MuestraElMensaje_YSigueSinRuta()
    {
        _f.MensajeFalla = "La version esta bloqueada: desbloqueela para modificar su estructura.";
        var p = await Abre("Esta versión no tiene ruta");
        await Elige(p, 0, "RT1");
        await p.InvokeAsync(() => p.Find("[data-accion='asignar-matriz']").Click());
        p.WaitForAssertion(() => Assert.Contains("esta bloqueada", p.Find("[data-error='1']").TextContent));
        Assert.Empty(p.FindAll("[data-origen='1']")); Assert.NotEmpty(p.FindAll("[data-tarjeta='matriz']"));
    }

    [Fact]
    public async Task CopiarOtraRuta_YEmpezarEnBlanco_UsanLosEndpointsDeSiempre()
    {
        var p = await Abre("Esta versión no tiene ruta");
        await Elige(p, 1, "RT9");
        await p.InvokeAsync(() => p.Find("[data-accion='copiar']").Click());
        p.WaitForAssertion(() => Assert.Contains("RT52", p.Find("[data-ruta='1']").TextContent));
        Assert.Equal("RT9", Prop(Ultima("POST", "/ruta/copiar").B!.Value, "COD_RUTA_ORIGEN").GetString());
        Assert.Contains("Copia de la ruta", p.Find("[data-origen='1']").TextContent);                            // origen: una ruta, no una matriz

    }

    [Fact]
    public async Task EmpezarEnBlanco_EnviaElNombre_YLaVersionQuedaConRutaPropia()
    {
        var p = await Abre("Esta versión no tiene ruta");
        Assert.True(Deshabilitado(p.Find("[data-accion='crear']")));
        await p.InvokeAsync(() => p.Find("[data-tarjeta='vacia'] input").Change("RUTA NUEVA"));
        await p.InvokeAsync(() => p.Find("[data-accion='crear']").Click());
        p.WaitForAssertion(() => Assert.Contains("RT53", p.Find("[data-ruta='1']").TextContent));
        Assert.Equal("RUTA NUEVA", Prop(Ultima("POST", "/ruta/crear").B!.Value, "DES_RUTA").GetString());
        Assert.Contains("Ruta propia", p.Find("[data-origen='propia']").TextContent);
    }

    // ======================= con ruta =======================
    [Fact]
    public async Task ConRuta_MuestraSuOrigenYElEditor_ConLasAcciones()
    {
        _f.ConRuta();
        var p = await Abre("RT50");
        Assert.Contains("Copia de la matriz MATRIZ POLO", p.Find("[data-origen='1']").TextContent);
        Assert.Empty(p.FindAll("[data-compartida='1']")); Assert.Empty(p.FindAll("[data-estado='bloqueada']"));
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-zona='barra']")));
        foreach (var a in new[] { "reemplazar", "guardar-como-matriz", "quitar-ruta" }) Assert.False(Deshabilitado(p.Find($"[data-accion='{a}']")), a);
        Assert.False(Deshabilitado(p.Find("[data-accion='agregar-proceso']")));
        Assert.Contains("MINUTOS ESTÁNDAR POR UNIDAD", p.Markup);
    }

    [Fact]
    public async Task UnaRutaPropia_DiceRutaPropia()
    {
        _f.ConRuta("RT70", null, "RUTA ANTIGUA", false);
        var p = await Abre("RT70");
        Assert.Contains("Ruta propia", p.Find("[data-origen='propia']").TextContent);
    }

    [Fact]
    public async Task Reemplazar_PideOtraMatriz_YEnviaREEMPLAZAR_ConAvisoDeQueLaAnteriorNoSeBorra()
    {
        _f.ConRuta();
        var p = await Abre("RT50");
        await p.InvokeAsync(() => p.Find("[data-accion='reemplazar']").Click());
        Assert.Contains("La ruta actual no se borra", p.Find("[data-aviso='reemplazo']").TextContent);
        Assert.Empty(p.FindAll("[data-tarjeta='copiar']"));                                                      // solo la tarjeta de matrices
        await Elige(p, 0, "RT7");
        await p.InvokeAsync(() => p.Find("[data-accion='asignar-matriz']").Click());
        p.WaitForAssertion(() => Assert.Empty(p.FindAll("[data-aviso='reemplazo']")));
        Assert.True(Prop(Ultima("POST", "/disenos-matrices/asignar").B!.Value, "REEMPLAZAR").GetBoolean());
    }

    [Fact]
    public async Task Reemplazar_Cancelar_VuelveAlEditorSinEnviarNada()
    {
        _f.ConRuta();
        var p = await Abre("RT50");
        await p.InvokeAsync(() => p.Find("[data-accion='reemplazar']").Click());
        await p.InvokeAsync(() => p.Find("[data-accion='cancelar-reemplazo']").Click());
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-zona='acciones-ruta']")));
        Assert.Equal(0, Cuantas("POST", "/disenos-matrices/asignar"));
    }

    [Fact]
    public async Task Ampliar_PasaAPantallaCompleta_YElArbolLlenaElAlto()
    {
        _f.ConRuta();
        var p = await Abre("RT50");
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-panel='arbol-ruta']")));
        var arbol = p.Find("[data-panel='arbol-ruta']");
        Assert.Contains("kon-llena", arbol.ClassName);                                                          // el arbol llena el alto por CSS (flexbox)
        Assert.Null(arbol.GetAttribute("data-resizable-grid"));                                                  // sin el tirador viejo: el alto lo da la ventana
        Assert.Empty(arbol.QuerySelectorAll(".grid-resize-handle"));
        p.WaitForAssertion(() => Assert.Contains("mud-dialog-fullscreen", p.Find(".mud-dialog").ClassName));      // se abre a pantalla completa
        Assert.Contains("kon-llena", p.Find("[data-panel='arbol-ruta']").ClassName);
        p.Find("[data-accion='ampliar']").Click();
        p.WaitForAssertion(() => Assert.DoesNotContain("mud-dialog-fullscreen", p.Find(".mud-dialog").ClassName));   // restaurar
        p.Find("[data-accion='ampliar']").Click();
        p.WaitForAssertion(() => Assert.Contains("mud-dialog-fullscreen", p.Find(".mud-dialog").ClassName));
    }

    [Fact]
    public async Task VersionBloqueada_NoPermiteEditarNiMoverLasOperaciones()
    {
        _f.ConRuta(bloqueada: true, bloqueadas: 1);
        var p = await Abre("RT50");
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("tr[data-fila='2']")));
        Assert.True(Deshabilitado(p.Find("tr[data-fila='2'] [data-accion='editar-operacion']")));
        Assert.True(Deshabilitado(p.Find("tr[data-fila='2'] [data-accion='subir']"))); Assert.True(Deshabilitado(p.Find("tr[data-fila='2'] [data-accion='bajar']")));
        Assert.Empty(p.FindAll("tr[data-fila='2'] [data-accion='tiempo-propio']"));
    }

    [Fact]
    public async Task ConRutaEditable_LaOperacionSeEditaDesdeElDialogoDeLaVersion()
    {
        _f.ConRuta();
        var p = await Abre("RT50");
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("tr[data-fila='2']")));
        Assert.False(Deshabilitado(p.Find("tr[data-fila='2'] [data-accion='editar-operacion']")));
        await p.InvokeAsync(() => p.Find("tr[data-fila='2'] [data-accion='editar-operacion']").Click());
        p.WaitForAssertion(() => Assert.Contains("Veces por unidad", p.Markup));
    }

    // ======================= rutas compartidas =======================
    [Fact]
    public async Task RutaCompartida_AvisaYPermiteDarleASuPropiaCopia()
    {
        _f.ConRuta("RT40", null, "RUTA ANTIGUA", false, usadaPor: 3);
        var p = await Abre("RT40");
        Assert.Contains("Compartida con 2 versión(es)", p.Find("[data-compartida='1']").TextContent);
        Assert.Contains("lo que cambies aquí las afecta", p.Find("[data-aviso='compartida']").TextContent);
        await p.InvokeAsync(() => p.Find("[data-accion='independizar']").Click());
        p.WaitForAssertion(() => Assert.Contains("RT51", p.Find("[data-ruta='1']").TextContent));
        Assert.Empty(p.FindAll("[data-compartida='1']")); Assert.Empty(p.FindAll("[data-aviso='compartida']"));
        Assert.Equal(1, Cuantas("POST", "/D1/1/independizar"));
    }

    [Fact]
    public async Task RutaQueNoSeComparte_NoOfreceIndependizar()
    {
        _f.ConRuta();
        var p = await Abre("RT50");
        Assert.Empty(p.FindAll("[data-accion='independizar']"));
    }

    // ======================= solo lectura =======================
    [Fact]
    public async Task VersionBloqueada_EsSoloLectura_ConDesbloquear_YElEditorNoDejaCambiar()
    {
        _f.ConRuta(bloqueada: true, bloqueadas: 1);
        var p = await Abre("RT50");
        Assert.Contains("La versión está bloqueada", p.Find("[data-aviso='solo-lectura']").TextContent);
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-zona='barra']")));
        foreach (var a in new[] { "reemplazar", "quitar-ruta", "agregar-proceso", "agregar-subproceso", "agregar-operacion", "quitar", "recalcular", "precedencias" }) Assert.True(Deshabilitado(p.Find($"[data-accion='{a}']")), a);
        Assert.False(Deshabilitado(p.Find("[data-accion='guardar-como-matriz']")));                                // guardar una copia como matriz no cambia la version
        await p.InvokeAsync(() => p.Find("[data-accion='desbloquear']").Click());
        p.WaitForAssertion(() => Assert.Contains("Se podrá modificar de nuevo", p.Markup));
        await p.InvokeAsync(() => p.FindAll("button").First(b => b.TextContent.Trim() == "Desbloquear").Click());
        p.WaitForAssertion(() => Assert.Empty(p.FindAll("[data-aviso='solo-lectura']")));
        Assert.Equal(1, Cuantas("POST", "/desbloquear"));
    }

    [Fact]
    public async Task RutaUsadaPorOtraVersionBloqueada_EsSoloLectura_PeroSePuedeSepararDeEllas()
    {
        _f.ConRuta("RT40", null, "RUTA ANTIGUA", false, usadaPor: 2, bloqueadas: 1);
        var p = await Abre("RT40");
        Assert.Contains("la usa otra versión bloqueada", p.Find("[data-aviso='solo-lectura']").TextContent);
        Assert.Empty(p.FindAll("[data-accion='desbloquear']"));                                                  // esta version no esta bloqueada: nada que desbloquear
        Assert.True(Deshabilitado(p.Find("[data-accion='quitar-ruta']")));
        await p.InvokeAsync(() => p.Find("[data-accion='independizar']").Click());                               // la salida: su propia copia
        p.WaitForAssertion(() => Assert.Empty(p.FindAll("[data-aviso='solo-lectura']")));
    }

    // ======================= guardar como matriz / quitar =======================
    async Task<IRenderedComponent<MudDialogProvider>> ConRutaYDialogoGuardar()
    {
        _f.ConRuta();
        var p = await Abre("RT50");
        await p.InvokeAsync(() => p.Find("[data-accion='guardar-como-matriz']").Click());
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-campo='sin-sub-familia']")));
        return p;
    }

    [Fact]
    public async Task GuardarComoMatriz_SinNombre_NoEnviaNada()
    {
        var p = await ConRutaYDialogoGuardar();
        var guardar = p.FindAll("[data-accion='guardar']").Last();
        await p.InvokeAsync(() => guardar.Click());
        p.WaitForAssertion(() => Assert.Contains("Escribe el nombre", p.Find("[data-error='1']").TextContent));
        Assert.Equal(0, Cuantas("POST", "/guardar-como-matriz"));
    }

    [Fact]
    public async Task GuardarComoMatriz_EnviaElNombre_YPorDefectoConLaSubFamiliaDelDiseno()
    {
        var p = await ConRutaYDialogoGuardar();
        Assert.Contains("ligada a la sub familia del diseño", p.Markup);
        await p.InvokeAsync(() => p.FindComponents<MudTextField<string>>().Last().Instance.ValueChanged.InvokeAsync("POLO AJUSTADO"));
        await p.InvokeAsync(() => p.FindAll("[data-accion='guardar']").Last().Click());
        p.WaitForAssertion(() => Assert.Equal(1, Cuantas("POST", "/guardar-como-matriz")));
        var b = Ultima("POST", "/guardar-como-matriz").B!.Value;
        Assert.Equal("POLO AJUSTADO", Prop(b, "DES_MATRIZ").GetString()); Assert.False(Prop(b, "SIN_SUB_FAMILIA").GetBoolean());
        Assert.Equal("D1", Prop(b, "COD_DISENO").GetString());
    }

    [Fact]
    public async Task GuardarComoMatriz_SinSubFamilia_LoEnvia()
    {
        var p = await ConRutaYDialogoGuardar();
        await p.InvokeAsync(() => p.FindComponents<MudTextField<string>>().Last().Instance.ValueChanged.InvokeAsync("POLO GENERICO"));
        await p.InvokeAsync(() => p.FindComponent<MudSwitch<bool>>().Instance.ValueChanged.InvokeAsync(true));
        await p.InvokeAsync(() => p.FindAll("[data-accion='guardar']").Last().Click());
        p.WaitForAssertion(() => Assert.Equal(1, Cuantas("POST", "/guardar-como-matriz")));
        Assert.True(Prop(Ultima("POST", "/guardar-como-matriz").B!.Value, "SIN_SUB_FAMILIA").GetBoolean());
    }

    [Fact]
    public async Task GuardarComoMatriz_SiLaApiRechaza_MuestraElMensaje()
    {
        var p = await ConRutaYDialogoGuardar();
        _f.MensajeFalla = "Ya existe una matriz con ese nombre.";
        await p.InvokeAsync(() => p.FindComponents<MudTextField<string>>().Last().Instance.ValueChanged.InvokeAsync("MATRIZ POLO"));
        await p.InvokeAsync(() => p.FindAll("[data-accion='guardar']").Last().Click());
        p.WaitForAssertion(() => Assert.Contains("Ya existe una matriz", p.FindAll("[data-error='1']").Last().TextContent));
    }

    [Fact]
    public async Task QuitarDeLaVersion_PideConfirmacion_YEnviaLaRutaNula_SinBorrarNada()
    {
        _f.ConRuta();
        var p = await Abre("RT50");
        await p.InvokeAsync(() => p.Find("[data-accion='quitar-ruta']").Click());
        p.WaitForAssertion(() => Assert.Contains("la ruta no se borra", p.Markup));
        Assert.Equal(0, Cuantas("POST", "/ruta/asignar"));
        await p.InvokeAsync(() => p.FindAll("button").Last(b => b.TextContent.Trim() == "Quitar").Click());
        p.WaitForAssertion(() => Assert.Contains("Esta versión no tiene ruta de operaciones", p.Markup));
        Assert.Equal(JsonValueKind.Null, Prop(Ultima("POST", "/ruta/asignar").B!.Value, "COD_RUTA").ValueKind);
    }
}
