using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Blazored.LocalStorage;
using Bunit;
using KONSolutions.Shared.Models.Comercial;
using KONSolutions.Shared.Models.Ingenieria;
using KONSolutions.Web.Pages.Ingenieria;
using KONSolutions.Web.Pages.Shared;
using KONSolutions.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using Xunit;

// Pantalla de especificaciones tecnicas de una version de diseno (script SQL 113 + API disenos-especificaciones).
// La API se simula con FakeEspecificaciones: devuelve el panel (cabecera + filas) segun el escenario y guarda lo que le mandan.
public class FakeEspecificaciones : HttpMessageHandler
{
    public List<(string M, string U, JsonElement? B)> Calls = new();
    public bool Bloqueada;            // la version esta bloqueada (o cerrada)
    public bool Anulada;              // la version esta anulada
    public int Nivel = 1;             // plantilla que aplica: 1 sub familia, 2 familia, 3 grupo, 0 ninguna
    public bool SinFilas;             // la plantilla no marca ninguna especificacion para la ficha
    public bool PadreSinValor;        // el procesador no tiene valor ni item por defecto: el modelo depende de el
    public bool ItemsFallan;          // las listas de opciones no se pueden leer (403: el perfil no consulta Especificaciones Tecnicas)
    public bool ItemsSinRed;          // ... o la llamada ni siquiera llega (excepcion de red)
    public bool SinSistema;           // la configuracion cambio: ya no se pide el sistema operativo
    public bool Nieto;                // agrega una tercera lista que depende del modelo (procesador > modelo > version de modelo)
    public decimal? Entidad;          // entidad del diseno (marca, cliente...): filtra las listas que dependen de la entidad
    public bool Falla;                // la API rechaza las escrituras (guardar / desbloquear)
    public string? FallaLectura;      // la API rechaza la lectura (HTTP 400 con este mensaje)
    public string MensajeFalla = "La version esta bloqueada: desbloqueela para modificar sus especificaciones.";

    public int Lecturas => Calls.Count(c => c.M == "GET" && c.U.Contains("/disenos-especificaciones/"));
    public (string M, string U, JsonElement? B) Guardado => Calls.Last(c => c.M == "POST" && c.U.EndsWith("/disenos-especificaciones/guardar"));

    static DisenosEspecificacionesFila F(string cod, string plural, string singular, int orden, int? grupo, string? desGrupo, Action<DisenosEspecificacionesFila>? cfg = null)
    {
        var f = new DisenosEspecificacionesFila
        {
            COD_ESPECIFICACION_TECNICA = cod, DES_ESPECIFICACION_TECNICA = plural, DES_ESPECIFICACION_TECNICA_SINGULAR = singular,
            NUM_ORDEN_PRESENTACION = orden, COD_GRUPO_ESPECIFICACION_TECNICA = grupo, DES_GRUPO_ESPECIFICACION_TECNICA = desGrupo, FLG_FICHA_TECNICA = true,
        };
        cfg?.Invoke(f);
        return f;
    }

    static void Guardada(DisenosEspecificacionesFila f)
    { f.FLG_GUARDADO = true; f.COD_USUARIO_REGISTRO = "ADMIN"; f.COD_ESTACION_REGISTRO = "PC01"; f.FEC_REGISTRO = new DateTime(2026, 11, 10, 9, 30, 0); }

    // Cubre los nueve tipos de dato, una lista dependiente (modelo depende del procesador), una lista con item por defecto sin valor guardado,
    // dos grupos de presentacion y una especificacion sin grupo.
    public List<DisenosEspecificacionesFila> Filas()
    {
        if (SinFilas) return new();
        var l = FilasBase();
        if (SinSistema) l.RemoveAll(x => x.COD_ESPECIFICACION_TECNICA == "SISTE");
        if (Nieto) l.Insert(2, F("VERSI", "VERSIONES DE MODELO", "VERSION DE MODELO", 30, 2, "Datos de Ingeniería", f => { f.FLG_LISTA = true; f.FLG_CADENA = true; f.COD_ESPECIFICACION_TECNICA_PADRE = "MODEL"; f.COD_ITEM_TECNICO = 100; Guardada(f); }));
        l.Reverse();                      // la API puede devolverlas en cualquier orden: la pantalla ordena por NUM_ORDEN_PRESENTACION (y cada hija tras su padre)
        return l;
    }

    List<DisenosEspecificacionesFila> FilasBase() => new()
    {
        // Los catalogos reales de lista traen FLG_LISTA y FLG_CADENA a la vez (tipo LST): debe ganar la lista.
        F("PROCE", "PROCESADORES", "PROCESADOR", 1, 2, "Datos de Ingeniería", f => { f.FLG_LISTA = true; f.FLG_CADENA = true; if (!PadreSinValor) { f.COD_ITEM_TECNICO = 2; Guardada(f); } }),
        F("MODEL", "MODELOS", "MODELO", 20, 2, "Datos de Ingeniería", f => { f.FLG_LISTA = true; f.FLG_CADENA = true; f.COD_ESPECIFICACION_TECNICA_PADRE = "PROCE"; if (!PadreSinValor) { f.COD_ITEM_TECNICO = 10; Guardada(f); } }),
        F("SISTE", "SISTEMAS OPERATIVOS", "SISTEMA OPERATIVO", 3, 2, "Datos de Ingeniería", f => { f.FLG_LISTA = true; f.FLG_CADENA = true; }),     // sin valor guardado: el item por defecto
        F("GARAN", "MESES DE GARANTIA", "MES DE GARANTIA", 4, 2, "Datos de Ingeniería", f => { f.FLG_ENTERO = true; f.DES_UNIDAD_MEDIDA = "MES"; f.COD_SIMBOLO = "mes"; f.VAL_ESPECIFICACION_TENICA = "24"; Guardada(f); }),
        F("TRANS", "TRANSMITANCIAS", "TRANSMITANCIA", 5, 2, "Datos de Ingeniería", f => { f.FLG_NUMERICO = true; f.CAN_DECIMAL = 2; f.DES_UNIDAD_MEDIDA = "W/M2K"; f.COD_SIMBOLO = "W/m2K"; f.VAL_ESPECIFICACION_TENICA = "1.8"; Guardada(f); }),
        F("TEMPE", "TEMPERATURAS", "TEMPERATURA", 6, 2, "Datos de Ingeniería", f => { f.FLG_NUMERICO = true; f.FLG_RANGE = true; f.CAN_DECIMAL = 1; f.VAL_MINIMO = 5; f.VAL_MAXIMO = 40; Guardada(f); }),
        F("OBSER", "OBSERVACIONES", "OBSERVACION", 7, 1, "Datos Comerciales", f => { f.FLG_CADENA = true; f.DES_SUFIJO = "(libre)"; f.STR_ESPECIFICACION_TENICA = "hola"; Guardada(f); }),
        F("FECHA", "FECHAS DE LANZAMIENTO", "FECHA DE LANZAMIENTO", 8, 1, "Datos Comerciales", f => { f.FLG_FECHA = true; f.FEC_ESPECIFICACION_TENICA = new DateTime(2026, 3, 15); Guardada(f); }),
        F("HORA", "HORAS DE ENTREGA", "HORA DE ENTREGA", 9, 1, "Datos Comerciales", f => { f.FLG_HORA = true; f.HOR_ESPECIFICACION_TENICA = new TimeSpan(8, 30, 0); Guardada(f); }),
        F("FHORA", "FECHAS Y HORAS DE PRUEBA", "FECHA Y HORA DE PRUEBA", 10, 1, "Datos Comerciales", f => { f.FLG_FECHA_HORA = true; f.FEC_ESPECIFICACION_TENICA = new DateTime(2026, 5, 20); f.HOR_ESPECIFICACION_TENICA = new TimeSpan(14, 0, 0); Guardada(f); }),
        F("MOSQU", "MOSQUITEROS", "MOSQUITERO", 11, 1, "Datos Comerciales", f => { f.FLG_LOGICO = true; f.LOG_ESPECIFICACION_TENICA = true; Guardada(f); }),
        F("COLOR", "COLORES DE ACABADO", "COLOR DE ACABADO", 12, 1, "Datos Comerciales", f => { f.FLG_COLOR = true; f.VAL_ESPECIFICACION_TENICA = "#ff0000"; Guardada(f); }),
        F("OTRO", "OTROS DATOS", "OTRO DATO", 13, null, null, f => { f.FLG_CADENA = true; }),                                                                     // sin grupo de presentacion
    };

    public DisenosEspecificacionesPanel Panel()
    {
        var filas = Filas();
        return new DisenosEspecificacionesPanel
        {
            Cabecera = new DisenosEspecificacionesCabecera
            {
                COD_DISENO = "D1", NUM_VERSION = 3, DES_DISENO = "VENTANA CORREDIZA", DES_VERSION = "v3",
                DES_GRUPO_PRODUCTO = "PRENDA", DES_FAMILIA_PRODUCTO = "POLOS", DES_SUB_FAMILIA_PRODUCTO = "MANGA CORTA",
                COD_ENTIDAD = Entidad, NUM_NIVEL = Nivel,
                FLG_BLOQUEADO = Bloqueada, FLG_ANULADO = Anulada, DES_ESTADO = Anulada ? "ANULADO" : "ACTIVO",
                CAN_ESPECIFICACIONES = filas.Count, CAN_CON_VALOR = filas.Count(x => x.FLG_GUARDADO),
            },
            Filas = filas,
        };
    }

    // items de cada lista; la lista de modelos depende del procesador elegido
    static EspecificacionesItemsTecnicosComboDependiente I(int cod, string des, bool def = false, decimal? num = null, string? simbolo = null, string? color = null) =>
        new() { COD_ITEM_TECNICO = cod, DES_ITEM_TECNICO = des, FLG_DEFAULT = def, NUM_ORDEN_PRESENTACION = cod, VAL_NUMERICO = num, COD_SIMBOLO = simbolo, DES_COLOR = color };

    List<EspecificacionesItemsTecnicosComboDependiente> Items(string espec, string? padreItem) => espec switch
    {
        "PROCE" => new() { I(1, "Core i5", !PadreSinValor), I(2, "Core i7", num: 3.6m, simbolo: "GHz") },
        "MODEL" => padreItem == "2" ? new() { I(10, "i7-13700"), I(11, "i7-14700") } : padreItem == "1" ? new() { I(12, "i5-13400") } : new(),
        "SISTE" => new() { I(30, "Windows 10"), I(31, "Windows 11", true, color: "#0078d4") },
        "VERSI" => padreItem == "10" ? new() { I(100, "Rev A"), I(101, "Rev B") } : padreItem == "12" ? new() { I(102, "Rev C") } : new(),
        _ => new()
    };

    static string Param(string u, string n) => System.Web.HttpUtility.ParseQueryString(new Uri("http://x" + u).Query)[n] ?? "";

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage r, CancellationToken ct)
    {
        var u = r.RequestUri!.PathAndQuery;
        JsonElement? body = r.Content is null ? null : JsonDocument.Parse(await r.Content.ReadAsStringAsync(ct)).RootElement.Clone();
        Calls.Add((r.Method.Method, u, body));
        HttpResponseMessage Ok(object? data) => new(HttpStatusCode.OK) { Content = JsonContent.Create(new { Success = true, Data = data }) };
        HttpResponseMessage Mal(string msg) => new(HttpStatusCode.BadRequest) { Content = JsonContent.Create(new { Success = false, Message = msg }) };

        if (r.Method == HttpMethod.Get)
        {
            if (u.Contains("/disenos-especificaciones/")) return FallaLectura is not null ? Mal(FallaLectura) : Ok(Panel());
            if (u.Contains("especificaciones-items-tecnicos") && ItemsSinRed) throw new HttpRequestException("sin red");
            if (u.Contains("especificaciones-items-tecnicos") && ItemsFallan) return new HttpResponseMessage(HttpStatusCode.Forbidden);
            if (u.Contains("especificaciones-items-tecnicos")) return Ok(Items(Param(u, "codEspecificacionTecnica"), Param(u, "codItemTecnicoDependiente")).AsEnumerable().Reverse().ToList());
            if (u.Contains("/ingenieria/disenos-version"))
                return Ok(new[] { new DisenoVersion { COD_DISENO = "D1", NUM_VERSION = 1, DES_VERSION = "v1", DES_ESTADO = "ACTIVO" }, new DisenoVersion { COD_DISENO = "D1", NUM_VERSION = 2, DES_VERSION = "v2", DES_ESTADO = "ACTIVO" } });
            return Ok(new object[0]);
        }
        if (Falla) return Mal(MensajeFalla);
        if (u.EndsWith("/disenos-especificaciones/guardar")) return Ok(new DisenosEspecificacionesResultado { INSERTADAS = 3, ACTUALIZADAS = 2, ELIMINADAS = 1 });
        if (u.EndsWith("/disenos-estructura/desbloquear")) { Bloqueada = false; return Ok(true); }
        return Ok(true);
    }
}

// La auditoria de cada valor (PanelAuditoria) pregunta si el perfil del usuario la puede ver: aqui, un perfil de seguridad con auditoria.
class FakeAuthConAuditoria : IAuthService
{
    public Task<(bool ok, string? error)> LoginAsync(KONSolutions.Shared.Models.Auth.LoginRequest request) => Task.FromResult((true, (string?)null));
    public Task LogoutAsync(bool conservarCacheMenu = false, bool notificar = true) => Task.CompletedTask;
    public Task<KONSolutions.Shared.Models.Auth.UsuarioSesion?> GetSesionAsync() =>
        Task.FromResult<KONSolutions.Shared.Models.Auth.UsuarioSesion?>(new() { COD_USUARIO = "ADMIN", FLG_SEGURIDAD = true, FLG_AUDITORIA = true });
    public Task<(bool ok, string? error)> ValidarPasswordAsync(string codUsuario, string password) => Task.FromResult((true, (string?)null));
    public byte[]? LogoDependenciaEnMemoria => null;
}

public class DisenosEspecificacionesTests : TestContext
{
    FakeEspecificaciones _f = new();

    IRenderedComponent<MudDialogProvider> Prov()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton(new HttpClient(_f) { BaseAddress = new Uri("http://x/") });
        Services.AddBlazoredLocalStorage();
        Services.AddScoped<ErrorLogService>(); Services.AddScoped<ConfigService>(); Services.AddScoped<EtiquetasService>();   // EtiquetasService: encabezados de la grilla de versiones
        Services.AddSingleton(new SeguridadContextService(new FakeAuthConAuditoria(), null!));
        // el control de color (ColorConSwatch) lee su banco de colores y el portapapeles de colores
        Services.AddSingleton<Microsoft.Extensions.Configuration.IConfiguration>(new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build());
        Services.AddScoped<AccessLogService>(); Services.AddScoped<ColorClipboardService>();
        Services.AddScoped(sp => new CatalogoService<KONSolutions.Shared.Models.Entorno.BancoColor, KONSolutions.Shared.Models.Entorno.BancoColorDto>(
            sp.GetRequiredService<HttpClient>(), sp.GetRequiredService<AccessLogService>(), sp.GetRequiredService<ErrorLogService>(), ApiRoutes.Entorno.BancoColores, "BANCO_COLORES"));
        RenderComponent<MudPopoverProvider>(); RenderComponent<MudSnackbarProvider>();
        return RenderComponent<MudDialogProvider>();
    }

    async Task<IDialogReference> Abre(IRenderedComponent<MudDialogProvider> p)
    {
        var ds = Services.GetRequiredService<IDialogService>();
        IDialogReference? d = null;
        await p.InvokeAsync(async () => d = await ds.ShowAsync<DisenosEspecificacionesDialog>("t",
            new DialogParameters { ["CodDiseno"] = "D1", ["DesDiseno"] = "VENTANA CORREDIZA", ["NumVersion"] = 3 }, new DialogOptions { MaxWidth = MaxWidth.Medium }));
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-espec], [data-aviso], [data-error]")));
        return d!;
    }

    // ---- ayudas para encontrar los controles de cada especificacion ----
    static JsonElement Prop(JsonElement e, string n) => e.EnumerateObject().First(x => string.Equals(x.Name, n, StringComparison.OrdinalIgnoreCase)).Value;
    static bool EsNulo(JsonElement e, string n) => Prop(e, n).ValueKind == JsonValueKind.Null;

    JsonElement Fila(string cod) => Prop(_f.Guardado.B!.Value, "Filas").EnumerateArray().First(x => Prop(x, "COD_ESPECIFICACION_TECNICA").GetString() == cod);

    IRenderedComponent<AriaCombo<EspecificacionesItemsTecnicosComboDependiente>> Lista(IRenderedComponent<MudDialogProvider> p, string label) =>
        p.FindComponents<AriaCombo<EspecificacionesItemsTecnicosComboDependiente>>().First(c => c.Instance.Label == label);

    IRenderedComponent<MudNumericField<decimal?>> Dec(IRenderedComponent<MudDialogProvider> p, string label) =>
        p.FindComponents<MudNumericField<decimal?>>().First(c => c.Instance.Label == label);

    IRenderedComponent<MudNumericField<int?>> Ent(IRenderedComponent<MudDialogProvider> p) => p.FindComponents<MudNumericField<int?>>().Single();
    IRenderedComponent<CampoTextoEstable> Texto(IRenderedComponent<MudDialogProvider> p, string label) => p.FindComponents<CampoTextoEstable>().First(c => c.Instance.Label == label);
    IRenderedComponent<MudDatePicker> Fecha(IRenderedComponent<MudDialogProvider> p, string label) => p.FindComponents<MudDatePicker>().First(c => c.Instance.Label == label);
    IRenderedComponent<MudTimePicker> Hora(IRenderedComponent<MudDialogProvider> p, string label) => p.FindComponents<MudTimePicker>().First(c => c.Instance.Label == label);
    IRenderedComponent<MudSwitch<bool?>> Logico(IRenderedComponent<MudDialogProvider> p) => p.FindComponents<MudSwitch<bool?>>().Single();
    IRenderedComponent<ColorConSwatch> Color(IRenderedComponent<MudDialogProvider> p) => p.FindComponents<ColorConSwatch>().Single();
    AngleSharp.Dom.IElement Guardar(IRenderedComponent<MudDialogProvider> p) => p.Find("[data-accion='guardar']");

    // ---------- lo que se ve ----------
    [Fact]
    public async Task Muestra_EstadoClasificacionConteoYDeQueNivelSalenLasEspecificaciones()
    {
        var p = Prov(); await Abre(p);
        Assert.Contains("Editable", p.Find("[data-estado]").TextContent);
        Assert.Equal("editable", p.Find("[data-estado]").GetAttribute("data-estado"));
        Assert.Contains("PRENDA › POLOS › MANGA CORTA", p.Find("[data-clasificacion]").TextContent);
        Assert.Contains("especificaciones de la sub familia", p.Find("[data-clasificacion]").TextContent);
        Assert.Contains("11 de 13 con valor", p.Find("[data-conteo]").TextContent);     // 13 especificaciones, 11 con valor (SISTE y OTRO sin valor)
        Assert.Contains("D1 v3", p.Markup); Assert.Contains("VENTANA CORREDIZA", p.Markup);
        p.WaitForAssertion(() => Assert.Equal("2", Lista(p, "PROCESADOR").Instance.Value));      // ya cargaron las listas
        Assert.Empty(p.FindAll("[data-aviso='listas']"));                                          // y no hay ningun aviso de listas sin cargar
    }

    [Theory]
    [InlineData(1, "especificaciones de la sub familia")]
    [InlineData(2, "especificaciones de la familia")]
    [InlineData(3, "especificaciones del grupo")]
    public async Task IndicaDeQueNivelSalenLasEspecificaciones(int nivel, string texto)
    {
        _f.Nivel = nivel;
        var p = Prov(); await Abre(p);
        Assert.Contains(texto, p.Find("[data-clasificacion]").TextContent);
    }

    [Fact]
    public async Task AgrupaPorGrupo_EnOrden_YLasSinGrupoAlFinal()
    {
        var p = Prov(); await Abre(p);
        Assert.Equal(new[] { "Datos de Ingeniería", "Datos Comerciales", "Sin grupo" }, p.FindAll("[data-grupo]").Select(g => g.GetAttribute("data-grupo")).ToArray());
        Assert.Equal(new[] { "PROCE", "MODEL", "SISTE", "GARAN", "TRANS", "TEMPE" }, p.Find("[data-grupo='Datos de Ingeniería']").QuerySelectorAll("[data-espec]").Select(e => e.GetAttribute("data-espec")).ToArray());
        Assert.Equal(new[] { "OBSER", "FECHA", "HORA", "FHORA", "MOSQU", "COLOR" }, p.Find("[data-grupo='Datos Comerciales']").QuerySelectorAll("[data-espec]").Select(e => e.GetAttribute("data-espec")).ToArray());
        Assert.Equal(new[] { "OTRO" }, p.Find("[data-grupo='Sin grupo']").QuerySelectorAll("[data-espec]").Select(e => e.GetAttribute("data-espec")).ToArray());
    }

    [Fact]
    public async Task CadaTipoDeDatoDibujaSuControl_YLaListaGanaSobreLaCadena()
    {
        var p = Prov(); await Abre(p);
        string Campo(string cod) => p.Find($"[data-espec='{cod}'] [data-campo]").GetAttribute("data-campo")!;
        Assert.Equal("lista", Campo("PROCE"));      // FLG_LISTA y FLG_CADENA a la vez: lista
        Assert.Equal("lista", Campo("MODEL"));
        Assert.Equal("lista", Campo("SISTE"));
        Assert.Equal("entero", Campo("GARAN"));
        Assert.Equal("numerico", Campo("TRANS"));
        Assert.Equal("rango", Campo("TEMPE"));
        Assert.Equal("cadena", Campo("OBSER"));
        Assert.Equal("fecha", Campo("FECHA"));
        Assert.Equal("hora", Campo("HORA"));
        Assert.Equal("fechahora", Campo("FHORA"));
        Assert.Equal("logico", Campo("MOSQU"));
        Assert.Equal("color", Campo("COLOR"));
        Assert.Equal("cadena", Campo("OTRO"));
        Assert.Equal(3, p.FindComponents<AriaCombo<EspecificacionesItemsTecnicosComboDependiente>>().Count);
    }

    [Fact]
    public async Task UsaElNombreEnSingular_YMuestraUnidadYSufijo()
    {
        var p = Prov(); await Abre(p);
        Assert.NotNull(Lista(p, "PROCESADOR")); Assert.NotNull(Lista(p, "MODELO"));            // el singular, no "PROCESADORES"
        Assert.DoesNotContain("PROCESADORES", p.Markup);
        Assert.Contains("MES (mes)", p.Find("[data-espec='GARAN']").TextContent);                // unidad al costado del entero
        Assert.Contains("W/M2K (W/m2K)", p.Find("[data-espec='TRANS']").TextContent);
        Assert.Contains("(libre)", p.Find("[data-espec='OBSER']").TextContent);                  // sufijo de la cadena
    }

    [Fact]
    public async Task MuestraLosValoresGuardados_DeCadaTipo()
    {
        var p = Prov(); await Abre(p);
        p.WaitForAssertion(() => Assert.Equal("2", Lista(p, "PROCESADOR").Instance.Value));      // los items cargan despues de abrir
        Assert.Equal("10", Lista(p, "MODELO").Instance.Value);
        Assert.Equal(24, Ent(p).Instance.Value);
        Assert.Equal(1.8m, Dec(p, "TRANSMITANCIA").Instance.Value);
        Assert.Equal(5m, Dec(p, "TEMPERATURA — De").Instance.Value); Assert.Equal(40m, Dec(p, "a").Instance.Value);
        Assert.Equal("hola", Texto(p, "OBSERVACION").Instance.Value);
        Assert.Equal(new DateTime(2026, 3, 15), Fecha(p, "FECHA DE LANZAMIENTO").Instance.Date);
        Assert.Equal(new TimeSpan(8, 30, 0), Hora(p, "HORA DE ENTREGA").Instance.Time);
        Assert.Equal(new DateTime(2026, 5, 20), Fecha(p, "FECHA Y HORA DE PRUEBA").Instance.Date);
        Assert.Equal(new TimeSpan(14, 0, 0), Hora(p, "Hora").Instance.Time);
        Assert.True(Logico(p).Instance.Value);
        Assert.Equal("#ff0000", Color(p).Instance.Text);
    }

    [Fact]
    public async Task LaAuditoria_SoloAparecePorLasEspecificacionesGuardadas()
    {
        var p = Prov(); await Abre(p);
        var auditorias = p.FindComponents<PanelAuditoria>();
        Assert.Equal(11, auditorias.Count);                       // 13 especificaciones, 2 sin valor guardado (SISTE y OTRO): esas no tienen auditoria
        Assert.All(auditorias, a => { Assert.Equal("ADMIN", a.Instance.CodUsuarioRegistro); Assert.Equal("PC01", a.Instance.CodEstacionRegistro); Assert.Equal(new DateTime(2026, 11, 10, 9, 30, 0), a.Instance.FecRegistro); });
    }

    [Fact]
    public async Task ListaConItemNumericoOConColor_MuestraSuValorConUnidad_YSuColor()
    {
        var p = Prov(); await Abre(p);
        p.WaitForAssertion(() => Assert.Contains("3.6 GHz", p.Find("[data-espec='PROCE']").TextContent));            // Core i7 (el guardado) trae su valor con unidad
        Assert.DoesNotContain("GHz", p.Find("[data-espec='SISTE']").TextContent);
        p.WaitForAssertion(() => Assert.NotEmpty(p.Find("[data-espec='SISTE']").QuerySelectorAll("div[title='#0078d4']")));   // Windows 11 (el de por defecto) trae su color
        Assert.Empty(p.Find("[data-espec='PROCE']").QuerySelectorAll("div[title^='#']"));
    }

    [Fact]
    public async Task ListaSinValorGuardado_PreseleccionaElItemPorDefecto()
    {
        var p = Prov(); await Abre(p);
        p.WaitForAssertion(() => Assert.Equal("31", Lista(p, "SISTEMA OPERATIVO").Instance.Value));      // Windows 11 es el item por defecto
        Assert.Equal(new[] { "30", "31" }, Lista(p, "SISTEMA OPERATIVO").Instance.Items.Select(i => i.COD_ITEM_TECNICO.ToString()).ToArray());
    }

    // ---------- listas que dependen de otra / de la entidad ----------
    [Fact]
    public async Task ListaDependiente_SinElPadre_QuedaDeshabilitada_YPideElegirloPrimero()
    {
        _f.PadreSinValor = true;
        var p = Prov(); await Abre(p);
        p.WaitForAssertion(() => Assert.True(Lista(p, "MODELO").Instance.Disabled));
        Assert.False(Lista(p, "PROCESADOR").Instance.Disabled);
        Assert.Contains("Elija primero \"PROCESADOR\"", p.Find("[data-espec='MODEL']").TextContent);
        Assert.Empty(Lista(p, "MODELO").Instance.Items);
        Assert.DoesNotContain(_f.Calls, c => c.U.Contains("codEspecificacionTecnica=MODEL"));      // sin padre no hay nada que pedir
    }

    [Fact]
    public async Task ListaDependiente_AlCambiarElPadre_SeLimpia_YPideSusItemsDelNuevoPadre()
    {
        var p = Prov(); await Abre(p);
        p.WaitForAssertion(() => Assert.Equal("10", Lista(p, "MODELO").Instance.Value));
        await p.InvokeAsync(() => Lista(p, "PROCESADOR").Instance.ValueChanged.InvokeAsync("1"));     // Core i5
        p.WaitForAssertion(() => Assert.Null(Lista(p, "MODELO").Instance.Value));                       // el modelo del i7 ya no vale
        p.WaitForAssertion(() => Assert.Equal(new[] { "12" }, Lista(p, "MODELO").Instance.Items.Select(i => i.COD_ITEM_TECNICO.ToString()).ToArray()));
        Assert.Contains(_f.Calls, c => c.U.Contains("codEspecificacionTecnica=MODEL") && c.U.Contains("codEspecificacionTecnicaDependiente=PROCE") && c.U.Contains("codItemTecnicoDependiente=1"));
    }

    [Fact]
    public async Task ListasQueNoSePudieronCargar_SeAvisan_SinPerderLoGuardado()
    {
        _f.ItemsFallan = true;
        var p = Prov(); await Abre(p);
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-aviso='listas']")));
        var aviso = p.Find("[data-aviso='listas']").TextContent;
        Assert.Contains("PROCESADOR", aviso); Assert.Contains("MODELO", aviso); Assert.Contains("SISTEMA OPERATIVO", aviso);
        Assert.Contains("Especificaciones Técnicas", aviso);                                  // dice donde esta el motivo probable: el permiso
        Assert.Equal(24, Ent(p).Instance.Value);                                                // el resto de la ventana funciona
        Assert.False(Guardar(p).HasAttribute("disabled"));
        Guardar(p).Click();                                                                     // y guardar no pierde los items ya guardados
        p.WaitForAssertion(() => Assert.Contains(_f.Calls, c => c.M == "POST" && c.U.EndsWith("/disenos-especificaciones/guardar")));
        Assert.Equal(2, Prop(Fila("PROCE"), "COD_ITEM_TECNICO").GetInt32());
        Assert.Equal(10, Prop(Fila("MODEL"), "COD_ITEM_TECNICO").GetInt32());
    }

    [Fact]
    public async Task ListasQueFallanDosVeces_SeNombranUnaSolaVez()
    {
        _f.ItemsFallan = true;
        var p = Prov(); await Abre(p);
        p.WaitForAssertion(() => Assert.Contains("MODELO", p.Find("[data-aviso='listas']").TextContent));
        await p.InvokeAsync(() => Lista(p, "PROCESADOR").Instance.ValueChanged.InvokeAsync("1"));    // el modelo vuelve a pedir sus items y vuelve a fallar
        p.WaitForAssertion(() => Assert.Equal(2, _f.Calls.Count(c => c.U.Contains("codEspecificacionTecnica=MODEL"))));
        var aviso = p.Find("[data-aviso='listas']").TextContent;
        Assert.Equal(1, aviso.Split("MODELO").Length - 1);
    }

    [Fact]
    public async Task ListasConErrorDeRed_TambienSeAvisan()
    {
        _f.ItemsSinRed = true;
        var p = Prov(); await Abre(p);
        p.WaitForAssertion(() => Assert.Contains("SISTEMA OPERATIVO", p.Find("[data-aviso='listas']").TextContent));
        Assert.Equal(24, Ent(p).Instance.Value);
    }

    [Fact]
    public async Task ListasSinCargar_AlRecargarConExito_YaNoAvisan()
    {
        _f.ItemsFallan = true;
        var p = Prov(); await Abre(p);
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-aviso='listas']")));
        _f.ItemsFallan = false;                                                                  // el permiso se arreglo
        Guardar(p).Click();                                                                      // guardar vuelve a leer todo
        p.WaitForAssertion(() => Assert.Equal(2, _f.Lecturas));
        p.WaitForAssertion(() => Assert.Empty(p.FindAll("[data-aviso='listas']")));
        p.WaitForAssertion(() => Assert.Equal("2", Lista(p, "PROCESADOR").Instance.Value));
    }

    [Fact]
    public async Task ListasSinCargar_UnaListaQueVuelveACargar_SaleDelAviso_LasDemasSiguen()
    {
        _f.ItemsFallan = true;
        var p = Prov(); await Abre(p);
        p.WaitForAssertion(() => Assert.Contains("MODELO", p.Find("[data-aviso='listas']").TextContent));
        _f.ItemsFallan = false;
        await p.InvokeAsync(() => Lista(p, "PROCESADOR").Instance.ValueChanged.InvokeAsync("1"));    // el modelo vuelve a pedir sus items y ahora si llegan
        p.WaitForAssertion(() => Assert.DoesNotContain("MODELO", p.Find("[data-aviso='listas']").TextContent));
        var aviso = p.Find("[data-aviso='listas']").TextContent;
        Assert.Contains("PROCESADOR", aviso); Assert.Contains("SISTEMA OPERATIVO", aviso);          // esas dos nunca se recargaron
    }

    [Fact]
    public async Task ListasSinCargar_AlRecargar_ElAvisoSoloHablaDeLoQueHayAhora()
    {
        _f.ItemsFallan = true;
        var p = Prov(); await Abre(p);
        p.WaitForAssertion(() => Assert.Contains("SISTEMA OPERATIVO", p.Find("[data-aviso='listas']").TextContent));
        _f.SinSistema = true;                                                                    // mientras tanto, esa especificacion salio de la plantilla
        Guardar(p).Click();
        p.WaitForAssertion(() => Assert.Equal(2, _f.Lecturas));
        p.WaitForAssertion(() => Assert.DoesNotContain("SISTEMA OPERATIVO", p.Find("[data-aviso='listas']").TextContent));     // ya no se nombra una lista que ni existe
        Assert.Contains("PROCESADOR", p.Find("[data-aviso='listas']").TextContent);               // las que siguen fallando, si
    }

    [Fact]
    public async Task ListaDependiente_SiSeQuitaElPadre_YaNoEsUnaListaFallida()
    {
        _f.ItemsFallan = true;
        var p = Prov(); await Abre(p);
        p.WaitForAssertion(() => Assert.Contains("MODELO", p.Find("[data-aviso='listas']").TextContent));
        await p.InvokeAsync(() => Lista(p, "PROCESADOR").Instance.ValueChanged.InvokeAsync(""));     // sin padre el modelo ni se pide: no hay nada que haya fallado
        p.WaitForAssertion(() => Assert.DoesNotContain("MODELO", p.Find("[data-aviso='listas']").TextContent));
    }

    [Fact]
    public async Task ListaDependiente_EnTresNiveles_AlCambiarElAbuelo_SeLimpiaTambienElNieto()
    {
        _f.Nieto = true;
        var p = Prov(); await Abre(p);
        p.WaitForAssertion(() => Assert.Equal("100", Lista(p, "VERSION DE MODELO").Instance.Value));
        await p.InvokeAsync(() => Lista(p, "PROCESADOR").Instance.ValueChanged.InvokeAsync("1"));        // Core i5: no tiene el modelo i7-13700 ni su version
        p.WaitForAssertion(() => Assert.Null(Lista(p, "MODELO").Instance.Value));
        p.WaitForAssertion(() => Assert.Null(Lista(p, "VERSION DE MODELO").Instance.Value));               // el nieto tampoco vale
        Assert.Empty(Lista(p, "VERSION DE MODELO").Instance.Items);                                         // su lista espera a que se elija el modelo
        await p.InvokeAsync(() => Lista(p, "MODELO").Instance.ValueChanged.InvokeAsync("12"));
        p.WaitForAssertion(() => Assert.Equal(new[] { "102" }, Lista(p, "VERSION DE MODELO").Instance.Items.Select(i => i.COD_ITEM_TECNICO.ToString()).ToArray()));
    }

    [Fact]
    public async Task ListasPorEntidad_PasanLaEntidadDelDiseno()
    {
        _f.Entidad = 1234;
        var p = Prov(); await Abre(p);
        p.WaitForAssertion(() => Assert.Contains(_f.Calls, c => c.U.Contains("codEspecificacionTecnica=PROCE")));
        Assert.All(_f.Calls.Where(c => c.U.Contains("especificaciones-items-tecnicos")), c => Assert.Contains("codEntidadDependiente=1234", c.U));
    }

    [Fact]
    public async Task ListasPorEntidad_SinEntidadEnElDiseno_NoFiltran()
    {
        var p = Prov(); await Abre(p);
        p.WaitForAssertion(() => Assert.Contains(_f.Calls, c => c.U.Contains("codEspecificacionTecnica=PROCE")));
        Assert.DoesNotContain(_f.Calls, c => c.U.Contains("codEntidadDependiente"));
    }

    // ---------- guardar ----------
    [Fact]
    public async Task Guardar_EnviaTodasLasFilas_CadaValorEnLaColumnaDeSuTipo()
    {
        var p = Prov(); await Abre(p);
        p.WaitForAssertion(() => Assert.Equal("2", Lista(p, "PROCESADOR").Instance.Value));
        await p.InvokeAsync(() => Lista(p, "PROCESADOR").Instance.ValueChanged.InvokeAsync("1"));
        await p.InvokeAsync(() => Ent(p).Instance.ValueChanged.InvokeAsync(36));
        await p.InvokeAsync(() => Dec(p, "TRANSMITANCIA").Instance.ValueChanged.InvokeAsync(2.5m));
        await p.InvokeAsync(() => Dec(p, "TEMPERATURA — De").Instance.ValueChanged.InvokeAsync(10m));
        await p.InvokeAsync(() => Dec(p, "a").Instance.ValueChanged.InvokeAsync(50m));
        await p.InvokeAsync(() => Texto(p, "OBSERVACION").Instance.ValueChanged.InvokeAsync("nuevo"));
        await p.InvokeAsync(() => Fecha(p, "FECHA DE LANZAMIENTO").Instance.DateChanged.InvokeAsync(new DateTime(2026, 4, 1)));
        await p.InvokeAsync(() => Hora(p, "HORA DE ENTREGA").Instance.TimeChanged.InvokeAsync(new TimeSpan(9, 15, 0)));
        await p.InvokeAsync(() => Fecha(p, "FECHA Y HORA DE PRUEBA").Instance.DateChanged.InvokeAsync(new DateTime(2026, 6, 2)));
        await p.InvokeAsync(() => Hora(p, "Hora").Instance.TimeChanged.InvokeAsync(new TimeSpan(16, 45, 0)));
        await p.InvokeAsync(() => Logico(p).Instance.ValueChanged.InvokeAsync(false));
        await p.InvokeAsync(() => Color(p).Instance.TextChanged.InvokeAsync("#00ff00"));
        Guardar(p).Click();
        p.WaitForAssertion(() => Assert.Contains(_f.Calls, c => c.M == "POST" && c.U.EndsWith("/disenos-especificaciones/guardar")));

        var b = _f.Guardado.B!.Value;
        Assert.Equal("D1", Prop(b, "COD_DISENO").GetString()); Assert.Equal(3, Prop(b, "NUM_VERSION").GetInt32());
        Assert.Equal(13, Prop(b, "Filas").GetArrayLength());      // todas, con y sin valor: se graban juntas

        var proce = Fila("PROCE"); Assert.Equal(1, Prop(proce, "COD_ITEM_TECNICO").GetInt32()); Assert.True(EsNulo(proce, "VAL_ESPECIFICACION_TENICA"));
        var model = Fila("MODEL"); Assert.True(EsNulo(model, "COD_ITEM_TECNICO"));                              // se limpio al cambiar el procesador
        Assert.Equal(31, Prop(Fila("SISTE"), "COD_ITEM_TECNICO").GetInt32());                                   // el valor por defecto tambien se graba
        var garan = Fila("GARAN"); Assert.Equal("36", Prop(garan, "VAL_ESPECIFICACION_TENICA").GetString()); Assert.True(EsNulo(garan, "COD_ITEM_TECNICO"));
        Assert.Equal("2.5", Prop(Fila("TRANS"), "VAL_ESPECIFICACION_TENICA").GetString());                       // punto decimal siempre, sea cual sea la cultura
        var tempe = Fila("TEMPE"); Assert.Equal(10m, Prop(tempe, "VAL_MINIMO").GetDecimal()); Assert.Equal(50m, Prop(tempe, "VAL_MAXIMO").GetDecimal()); Assert.True(EsNulo(tempe, "VAL_ESPECIFICACION_TENICA"));
        var obser = Fila("OBSER"); Assert.Equal("nuevo", Prop(obser, "STR_ESPECIFICACION_TENICA").GetString()); Assert.True(EsNulo(obser, "VAL_ESPECIFICACION_TENICA"));
        var fecha = Fila("FECHA"); Assert.StartsWith("2026-04-01", Prop(fecha, "FEC_ESPECIFICACION_TENICA").GetString()); Assert.True(EsNulo(fecha, "HOR_ESPECIFICACION_TENICA"));
        var hora = Fila("HORA"); Assert.Equal("09:15:00", Prop(hora, "HOR_ESPECIFICACION_TENICA").GetString()); Assert.True(EsNulo(hora, "FEC_ESPECIFICACION_TENICA"));
        var fhora = Fila("FHORA"); Assert.StartsWith("2026-06-02", Prop(fhora, "FEC_ESPECIFICACION_TENICA").GetString()); Assert.Equal("16:45:00", Prop(fhora, "HOR_ESPECIFICACION_TENICA").GetString());
        var mosqu = Fila("MOSQU"); Assert.False(Prop(mosqu, "LOG_ESPECIFICACION_TENICA").GetBoolean());
        var color = Fila("COLOR"); Assert.Equal("#00ff00", Prop(color, "VAL_ESPECIFICACION_TENICA").GetString()); Assert.True(EsNulo(color, "STR_ESPECIFICACION_TENICA"));
        var otro = Fila("OTRO"); Assert.True(EsNulo(otro, "STR_ESPECIFICACION_TENICA")); Assert.True(EsNulo(otro, "VAL_ESPECIFICACION_TENICA"));   // sin tocar y sin valor: viaja vacia
    }

    [Fact]
    public async Task Guardar_UnValorVaciado_ViajaVacio_ParaQueLaApiLoBorre()
    {
        var p = Prov(); await Abre(p);
        p.WaitForAssertion(() => Assert.Equal(24, Ent(p).Instance.Value));
        await p.InvokeAsync(() => Ent(p).Instance.ValueChanged.InvokeAsync(null));
        Guardar(p).Click();
        p.WaitForAssertion(() => Assert.Contains(_f.Calls, c => c.M == "POST" && c.U.EndsWith("/disenos-especificaciones/guardar")));
        var garan = Fila("GARAN");                                      // la fila sigue en el envio, pero sin ningun valor
        foreach (var n in new[] { "COD_ITEM_TECNICO", "VAL_ESPECIFICACION_TENICA", "VAL_MINIMO", "VAL_MAXIMO", "FEC_ESPECIFICACION_TENICA", "HOR_ESPECIFICACION_TENICA", "LOG_ESPECIFICACION_TENICA", "STR_ESPECIFICACION_TENICA" })
            Assert.True(EsNulo(garan, n), n);
    }

    [Fact]
    public async Task Guardar_ColorYCadenaBorrados_ViajanSinValor_NoComoTextoVacio()
    {
        var p = Prov(); await Abre(p);
        p.WaitForAssertion(() => Assert.Equal("#ff0000", Color(p).Instance.Text));
        await p.InvokeAsync(() => Color(p).Instance.TextChanged.InvokeAsync(""));                  // el selector de color emite "" al borrar
        await p.InvokeAsync(() => Texto(p, "OBSERVACION").Instance.ValueChanged.InvokeAsync("  "));
        Guardar(p).Click();
        p.WaitForAssertion(() => Assert.Contains(_f.Calls, c => c.M == "POST" && c.U.EndsWith("/disenos-especificaciones/guardar")));
        Assert.True(EsNulo(Fila("COLOR"), "VAL_ESPECIFICACION_TENICA"));
        Assert.True(EsNulo(Fila("OBSER"), "STR_ESPECIFICACION_TENICA"));
    }

    // Con una cultura de coma decimal (como es-PE) los numeros se leen y se graban igual: la tabla los guarda como texto con punto.
    [Fact]
    public async Task Numeros_SeLeenYSeGrabanConPuntoDecimal_AunqueLaCulturaUseComa()
    {
        var antes = (CultureInfo.CurrentCulture, CultureInfo.CurrentUICulture, CultureInfo.DefaultThreadCurrentCulture);
        var coma = (CultureInfo)CultureInfo.InvariantCulture.Clone();
        coma.NumberFormat.NumberDecimalSeparator = ","; coma.NumberFormat.NumberGroupSeparator = ".";
        CultureInfo.CurrentCulture = coma; CultureInfo.DefaultThreadCurrentCulture = coma;
        try
        {
            var p = Prov(); await Abre(p);
            Assert.Equal(1.8m, Dec(p, "TRANSMITANCIA").Instance.Value);          // "1.8" en la tabla: 1,8 y no 18
            await p.InvokeAsync(() => Dec(p, "TRANSMITANCIA").Instance.ValueChanged.InvokeAsync(2.5m));
            Guardar(p).Click();
            p.WaitForAssertion(() => Assert.Contains(_f.Calls, c => c.M == "POST" && c.U.EndsWith("/disenos-especificaciones/guardar")));
            Assert.Equal("2.5", Prop(Fila("TRANS"), "VAL_ESPECIFICACION_TENICA").GetString());
        }
        finally { CultureInfo.CurrentCulture = antes.Item1; CultureInfo.CurrentUICulture = antes.Item2; CultureInfo.DefaultThreadCurrentCulture = antes.Item3; }
    }

    [Fact]
    public async Task Guardar_Exito_AvisaYVuelveALeerElPanel()
    {
        var p = Prov(); await Abre(p);
        Assert.Equal(1, _f.Lecturas);
        Guardar(p).Click();
        p.WaitForAssertion(() => Assert.Equal(2, _f.Lecturas));          // vuelve a leer: trae la auditoria de lo grabado y el conteo
        Assert.Contains(Services.GetRequiredService<ISnackbar>().ShownSnackbars, s => s.Message.Contains("Especificaciones guardadas"));
        Assert.Empty(p.FindAll("[data-error]"));
    }

    [Fact]
    public async Task Guardar_SiLaApiRechaza_MuestraElMotivo_YNoVuelveALeer()
    {
        var p = Prov(); await Abre(p);
        _f.Falla = true;
        Guardar(p).Click();
        p.WaitForAssertion(() => Assert.Contains("desbloqueela", p.Find("[data-error]").TextContent));
        Assert.Equal(1, _f.Lecturas);
        Assert.DoesNotContain(Services.GetRequiredService<ISnackbar>().ShownSnackbars, s => s.Message.Contains("guardadas"));
        Assert.NotEmpty(p.FindAll("[data-espec]"));                      // lo escrito sigue en pantalla
    }

    // ---------- version bloqueada, anulada, sin configurar, lectura rechazada ----------
    [Fact]
    public async Task Bloqueada_EsSoloLectura_YOfreceDesbloquear()
    {
        _f.Bloqueada = true;
        var p = Prov(); await Abre(p);
        Assert.Equal("bloqueada", p.Find("[data-estado]").GetAttribute("data-estado"));
        Assert.Contains("Bloqueada", p.Find("[data-estado]").TextContent);
        Assert.Contains("bloqueada", p.Find("[data-aviso='bloqueada']").TextContent);
        Assert.True(Guardar(p).HasAttribute("disabled"));
        Assert.NotEmpty(p.FindAll("[data-accion='desbloquear']"));
        TodosDeshabilitados(p, true);
        Assert.DoesNotContain("Elija primero", p.Markup);          // deshabilitada por ser solo lectura, no por falta del padre: no se pide nada
    }

    // Todos los controles de cada tipo (lista, entero, numerico y rango, cadena, fecha, hora, logico, color) con el mismo estado.
    void TodosDeshabilitados(IRenderedComponent<MudDialogProvider> p, bool esperado)
    {
        Assert.Equal(3, p.FindComponents<AriaCombo<EspecificacionesItemsTecnicosComboDependiente>>().Count);
        Assert.All(p.FindComponents<AriaCombo<EspecificacionesItemsTecnicosComboDependiente>>(), c => Assert.Equal(esperado, c.Instance.Disabled));
        Assert.All(p.FindComponents<MudNumericField<int?>>(), c => Assert.Equal(esperado, c.Instance.Disabled));
        Assert.Equal(3, p.FindComponents<MudNumericField<decimal?>>().Count);                                   // transmitancia + rango de/a
        Assert.All(p.FindComponents<MudNumericField<decimal?>>(), c => Assert.Equal(esperado, c.Instance.Disabled));
        Assert.All(p.FindComponents<CampoTextoEstable>(), c => Assert.Equal(esperado, c.Instance.Disabled));
        Assert.Equal(2, p.FindComponents<MudDatePicker>().Count); Assert.Equal(2, p.FindComponents<MudTimePicker>().Count);
        Assert.All(p.FindComponents<MudDatePicker>(), c => Assert.Equal(esperado, c.Instance.Disabled));
        Assert.All(p.FindComponents<MudTimePicker>(), c => Assert.Equal(esperado, c.Instance.Disabled));
        Assert.All(p.FindComponents<MudSwitch<bool?>>(), c => Assert.Equal(esperado, c.Instance.Disabled));
        Assert.All(p.FindComponents<ColorConSwatch>(), c => Assert.Equal(esperado, c.Instance.Disabled));
    }

    [Fact]
    public async Task Desbloquear_PideConfirmar_PublicaElDesbloqueo_YLaVersionQuedaEditable()
    {
        _f.Bloqueada = true;
        var p = Prov(); await Abre(p);
        p.Find("[data-accion='desbloquear']").Click();
        p.WaitForAssertion(() => Assert.Contains("Desbloquear versión", p.Markup));
        Assert.DoesNotContain(_f.Calls, c => c.U.EndsWith("/desbloquear"));                  // todavia no: falta confirmar
        p.FindAll("button").Last(b => b.TextContent.Trim() == "Desbloquear").Click();
        p.WaitForAssertion(() => Assert.Contains(_f.Calls, c => c.M == "POST" && c.U.EndsWith("/disenos-estructura/desbloquear")));
        var b = _f.Calls.Last(c => c.U.EndsWith("/desbloquear")).B!.Value;
        Assert.Equal("D1", Prop(b, "COD_DISENO").GetString()); Assert.Equal(3, Prop(b, "NUM_VERSION").GetInt32());
        p.WaitForAssertion(() => Assert.Equal("editable", p.Find("[data-estado]").GetAttribute("data-estado")));     // vuelve a leer: ya editable
        Assert.Empty(p.FindAll("[data-aviso='bloqueada']"));
        Assert.False(Guardar(p).HasAttribute("disabled"));
        p.WaitForAssertion(() => TodosDeshabilitados(p, false));
    }

    [Fact]
    public async Task Desbloquear_SiLaApiRechaza_MuestraElMotivo_YSigueBloqueada()
    {
        _f.Bloqueada = true; _f.Falla = true; _f.MensajeFalla = "No tiene permiso para desbloquear versiones.";
        var p = Prov(); await Abre(p);
        p.Find("[data-accion='desbloquear']").Click();
        p.WaitForAssertion(() => Assert.Contains("Desbloquear versión", p.Markup));
        p.FindAll("button").Last(b => b.TextContent.Trim() == "Desbloquear").Click();
        p.WaitForAssertion(() => Assert.Contains("No tiene permiso para desbloquear versiones.", p.Find("[data-error]").TextContent));
        Assert.Equal("bloqueada", p.Find("[data-estado]").GetAttribute("data-estado"));          // nada cambio
        Assert.Equal(1, _f.Lecturas);                                                              // y no se volvio a leer
        Assert.DoesNotContain(Services.GetRequiredService<ISnackbar>().ShownSnackbars, s => s.Message.Contains("desbloqueada"));
    }

    [Fact]
    public async Task Desbloquear_SiSeCancela_NoPublicaNada()
    {
        _f.Bloqueada = true;
        var p = Prov(); await Abre(p);
        p.Find("[data-accion='desbloquear']").Click();
        p.WaitForAssertion(() => Assert.Contains("Desbloquear versión", p.Markup));
        p.FindAll("button").Last(b => b.TextContent.Trim() == "Cancelar").Click();
        p.WaitForAssertion(() => Assert.DoesNotContain("Se podrán modificar de nuevo", p.Markup));
        Assert.DoesNotContain(_f.Calls, c => c.U.EndsWith("/desbloquear"));
        Assert.Equal("bloqueada", p.Find("[data-estado]").GetAttribute("data-estado"));
    }

    [Fact]
    public async Task Anulada_SoloSeConsulta_YNoSePuedeDesbloquear()
    {
        _f.Anulada = true;
        var p = Prov(); await Abre(p);
        Assert.Equal("anulada", p.Find("[data-estado]").GetAttribute("data-estado"));
        Assert.Contains("anulada", p.Find("[data-aviso='anulada']").TextContent);
        Assert.Empty(p.FindAll("[data-accion='desbloquear']"));
        Assert.True(Guardar(p).HasAttribute("disabled"));
        TodosDeshabilitados(p, true);
    }

    [Fact]
    public async Task SinConfigurar_AvisaQueFaltaLaPlantilla_YNoOfreceGuardar()
    {
        _f.Nivel = 0; _f.SinFilas = true;
        var p = Prov(); await Abre(p);
        Assert.Contains("no tiene especificaciones técnicas configuradas", p.Find("[data-aviso='sin-configurar']").TextContent);
        Assert.Empty(p.FindAll("[data-grupo]"));
        Assert.Empty(p.FindAll("[data-conteo]"));
        Assert.True(Guardar(p).HasAttribute("disabled"));
    }

    [Fact]
    public async Task PlantillaSinEspecificacionesParaLaFicha_LoDice_YNoOfreceGuardar()
    {
        _f.SinFilas = true;                                  // hay plantilla (nivel 1) pero ninguna marcada para la ficha tecnica
        var p = Prov(); await Abre(p);
        Assert.NotEmpty(p.FindAll("[data-aviso='sin-filas']"));
        Assert.Empty(p.FindAll("[data-aviso='sin-configurar']"));
        Assert.True(Guardar(p).HasAttribute("disabled"));
    }

    [Fact]
    public async Task LecturaRechazada_MuestraElMotivoQueDaLaApi_SinControles()
    {
        _f.FallaLectura = "La version no existe.";
        var p = Prov(); await Abre(p);
        Assert.Contains("La version no existe.", p.Find("[data-error]").TextContent);       // no el generico "400 Bad Request"
        Assert.Empty(p.FindAll("[data-grupo]"));
        Assert.Empty(p.FindAll("[data-estado]"));
        Assert.True(Guardar(p).HasAttribute("disabled"));
    }

    // ---------- cerrar ----------
    [Fact]
    public async Task Cerrar_CierraElDialogo_SinPublicarNada()
    {
        var p = Prov(); var d = await Abre(p);
        var llamadas = _f.Calls.Count;
        p.Find("[data-accion='cerrar']").Click();
        var r = await d.Result;
        Assert.False(r!.Canceled);
        p.WaitForAssertion(() => Assert.Empty(p.FindAll(".mud-dialog")));
        Assert.DoesNotContain(_f.Calls.Skip(llamadas), c => c.M == "POST");
    }

    // ---------- el boton de la grilla de versiones ----------
    [Fact]
    public async Task GrillaDeVersiones_TieneElBotonEspecificaciones_QueAbreLaVersionDeSuFila()
    {
        ComponentFactories.AddStub<AccionesBotones>();            // depende de seguridad/menu: no es lo que se prueba
        var p = Prov();
        var grilla = RenderComponent<DisenosVersionPanel>(ps => ps.Add(x => x.CodDiseno, "D1").Add(x => x.DesDiseno, "VENTANA CORREDIZA"));
        grilla.WaitForAssertion(() => Assert.Equal(2, grilla.FindAll("[data-accion='especificaciones']").Count));       // una por version
        grilla.FindAll("[data-accion='especificaciones']")[1].Click();                                                     // la de la version 2
        p.WaitForAssertion(() => Assert.Contains(_f.Calls, c => c.M == "GET" && c.U.EndsWith("/disenos-especificaciones/D1/2")));
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-espec]")));
        Assert.Contains("Especificaciones técnicas · D1 v2", p.Markup);          // el titulo del propio dialogo: diseno y version de la fila
    }
}
