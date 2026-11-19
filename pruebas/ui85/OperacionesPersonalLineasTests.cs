using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Blazored.LocalStorage;
using Bunit;
using KONSolutions.Shared.Models;
using KONSolutions.Shared.Models.Ingenieria;
using KONSolutions.Web.Pages.Ingenieria;
using KONSolutions.Web.Pages.Shared;
using KONSolutions.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using Xunit;

// Personal por categoria (script SQL 124) y armado de lineas (script SQL 125): pantallas y dialogos contra una API simulada.
public class FakeOperacionesPersonalLineas : HttpMessageHandler
{
    public List<(string M, string U, JsonElement? B)> Calls = new();
    public bool FallaEscritura; public string MensajeFalla = "Una operacion no puede estar en dos puestos.";
    public bool SinDisponibles;

    // ---- armado AR1: lineal, takt 1.36; puestos 1 (11,12) y 2 (13); libres 14 y 15 (esta sin tiempo) ----
    public string Regla = "MAX_PUESTO";
    public List<(int Num, string Des, int Oper, int[] Hojas)> Puestos = new() { (1, "PUESTO 01", 1, new[] { 11, 12 }), (2, "PUESTO 02", 1, new[] { 13 }) };
    static readonly (int Num, int Sec, string Des, string? Cat, decimal Sam, bool SinTiempo)[] Hojas =
    {
        (11, 1, "COSER COLLARETE", "A", 0.448m, false), (12, 2, "PEGAR CUELLO", "A", 0.952m, false), (13, 3, "PEGAR MANGA", "B", 1.008m, false),
        (14, 4, "REMATE Y LIMPIEZA", "D", 0.440m, false), (15, 5, "OJAL Y BOTON", "A", 0m, true),
    };

    public Armado Cabecera()
    {
        var asignadas = Puestos.SelectMany(p => p.Hojas).ToHashSet();
        var sam = Hojas.Where(h => asignadas.Contains(h.Num)).Sum(h => h.Sam);
        return new Armado
        {
            COD_ARMADO = "AR1", DES_ARMADO = "POLO BASICO", COD_RUTA_PRODUCCION = "RT1", COD_PROCESO = "COSTU", COD_TIPO_DISTRIBUCION = Regla == "MAX_PUESTO" ? "LIN" : "MOD", COD_LINEA_PRODUCCION = "L01",
            VAL_MINUTOS_TURNO = 480, TAS_EFICIENCIA = 85, VAL_META_UNIDADES = 300, COD_TIPO_ESTADO = 54, COD_ESTADO = 0, DES_ESTADO = "ACTIVO",
            DES_RUTA_PRODUCCION = "RUTA POLO", DES_PROCESO = "COSTURA", DES_TIPO_DISTRIBUCION = Regla == "MAX_PUESTO" ? "LINEAL" : "MODULAR", COD_REGLA_CICLO = Regla, DES_LINEA_PRODUCCION = "LINEA DE COSTURA 1",
            VAL_TAKT = 1.36m, CAN_PUESTOS = Puestos.Count, CAN_OPERARIOS = Puestos.Sum(p => p.Oper), VAL_SAM_ASIGNADO = sam, VAL_SAM_RUTA = 1.848m, VAL_CICLO = 1.4m, VAL_CAPACIDAD_TURNO = 291.43m,
            TAS_EFICIENCIA_BALANCEO = 86.0m, CAN_HOJAS_RUTA = 5, CAN_HOJAS_SIN_ASIGNAR = 5 - asignadas.Count,
        };
    }

    List<ArmadoPuesto> PuestosApi() => Puestos.Select(p =>
    {
        var s = Hojas.Where(h => p.Hojas.Contains(h.Num)).Sum(h => h.Sam);
        return new ArmadoPuesto { NUM_PUESTO = p.Num, DES_PUESTO = p.Des, NUM_OPERARIOS = p.Oper, CAN_HOJAS = p.Hojas.Length, VAL_SAM_PUESTO = s, VAL_CICLO_PUESTO = s / p.Oper, PCT_CARGA = s / p.Oper / 1.36m * 100, COD_CATEGORIA_OCUPACIONAL = "A" };
    }).ToList();

    List<ArmadoHoja> HojasApi() => Hojas.Select(h => new ArmadoHoja
    {
        NUM_SECUENCIA = h.Sec, NUM_ESTRUCTURA_RUTA_PROCESO = h.Num, COD_OPERACION = "OP" + h.Num, DES_OPERACION = h.Des, COD_CATEGORIA_OCUPACIONAL = h.Cat, VAL_SAM = h.Sam == 0 ? null : h.Sam,
        FREC_ACUM = 1, VAL_SAM_PRENDA = h.Sam, FLG_SIN_TIEMPO = h.SinTiempo, NUM_PUESTO = Puestos.FirstOrDefault(p => p.Hojas.Contains(h.Num)) is { Num: > 0 } q ? q.Num : null,
    }).ToList();

    List<ArmadoAviso> Avisos() => new()
    {
        new() { COD_SEVERIDAD = "E", COD_REGLA = "SIN_PUESTO", DES_MENSAJE = "La operacion \"REMATE Y LIMPIEZA\" no esta en ningun puesto.", NUM_ESTRUCTURA_RUTA_PROCESO = 14 },
        new() { COD_SEVERIDAD = "A", COD_REGLA = "SOBRE_TAKT", DES_MENSAJE = "El puesto 1 tarda 1.400 min y el takt es 1.360.", NUM_PUESTO = 1 },
    };

    // ---- personal ----
    public List<PersonalOperacionItem> Personal = new()
    {
        new() { COD_ENTIDAD_PERSONAL = 1, COD_PERSONAL = "EJ011", DES_PERSONAL = "EJEMPLO COSTURERA A 01", COD_CATEGORIA_OCUPACIONAL = "A", DES_CATEGORIA_OCUPACIONAL = "ESPECIALIZADO", COD_PROCESO = "COSTU", DES_PROCESO = "COSTURA", DES_ESTADO = "ACTIVO" },
        new() { COD_ENTIDAD_PERSONAL = 2, COD_PERSONAL = "EJ021", DES_PERSONAL = "EJEMPLO COSTURERA B 01", COD_CATEGORIA_OCUPACIONAL = "B", DES_CATEGORIA_OCUPACIONAL = "INTERMEDIO", COD_PROCESO = "COSTU", DES_PROCESO = "COSTURA", COD_OPERACION = "OP0005", DES_OPERACION = "UNIR HOMBROS", DES_ESTADO = "ACTIVO" },
        new() { COD_ENTIDAD_PERSONAL = 3, COD_PERSONAL = "EJ053", DES_PERSONAL = "EJEMPLO EMPAQUE 01", COD_CATEGORIA_OCUPACIONAL = "D", DES_CATEGORIA_OCUPACIONAL = "AUXILIAR", COD_PROCESO = "EMPQE", DES_PROCESO = "EMPAQUE", DES_ESTADO = "ACTIVO", FLG_CESADO = true },
    };

    Dictionary<string, List<CatalogoOperacionItem>> CombosPersonal() => new()
    {
        ["categorias"] = new() { new() { COD = "A", DES = "ESPECIALIZADO", DES_DETALLE = "Operaciones criticas" }, new() { COD = "B", DES = "INTERMEDIO" }, new() { COD = "D", DES = "AUXILIAR" } },
        ["procesos"] = new() { new() { COD = "COSTU", DES = "COSTURA" }, new() { COD = "EMPQE", DES = "EMPAQUE" } },
        ["subprocesos"] = new() { new() { COD = "ARMDO", DES = "ARMADO DE PIEZAS", DES_DETALLE = "COSTU" } },
        ["operaciones"] = new()
        {
            new() { COD = "OP0005", DES = "UNIR HOMBROS", DES_DETALLE = "COSTU", COD_REGLA = "B" }, new() { COD = "OP0008", DES = "COSER COLLARETE", DES_DETALLE = "COSTU", COD_REGLA = "A" },
            new() { COD = "OP0014", DES = "REMATE Y LIMPIEZA", DES_DETALLE = "COSTU", COD_REGLA = "D" }, new() { COD = "OP0036", DES = "DOBLAR PRENDA", DES_DETALLE = "EMPQE", COD_REGLA = "D" },
            new() { COD = "OP0099", DES = "SIN CATEGORIA", DES_DETALLE = "COSTU", COD_REGLA = null },
        },
    };

    Dictionary<string, List<CatalogoOperacionItem>> CombosArmado() => new()
    {
        ["rutas"] = new() { new() { COD = "RT1", DES = "RUTA POLO" }, new() { COD = "RT2", DES = "RUTA CAMISA" } },
        ["rutaprocesos"] = new() { new() { COD = "COSTU", DES = "COSTURA", DES_DETALLE = "RT1" }, new() { COD = "COSTU", DES = "COSTURA", DES_DETALLE = "RT2" }, new() { COD = "EMPQE", DES = "EMPAQUE", DES_DETALLE = "RT2" } },
        ["lineas"] = new() { new() { COD = "L01", DES = "LINEA DE COSTURA 1", DES_DETALLE = "COSTU", COD_REGLA = "LIN" }, new() { COD = "M01", DES = "MODULO DE COSTURA 1", DES_DETALLE = "COSTU", COD_REGLA = "MOD" } },
        ["distribuciones"] = new() { new() { COD = "LIN", DES = "LINEAL", COD_REGLA = "MAX_PUESTO", FLG_1 = false, FLG_2 = true }, new() { COD = "MOD", DES = "MODULAR", COD_REGLA = "SUMA_OPERARIOS", FLG_1 = true, FLG_2 = true } },
    };

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage r, CancellationToken ct)
    {
        var u = r.RequestUri!.PathAndQuery;
        JsonElement? body = r.Content is null ? null : JsonDocument.Parse(await r.Content.ReadAsStringAsync(ct)).RootElement.Clone();
        Calls.Add((r.Method.Method, u, body));
        HttpResponseMessage Ok(object? data) => new(HttpStatusCode.OK) { Content = JsonContent.Create(new { Success = true, Data = data }) };
        HttpResponseMessage Mal(string msg) => new(HttpStatusCode.BadRequest) { Content = JsonContent.Create(new { Success = false, Message = msg }) };

        if (r.Method == HttpMethod.Get)
        {
            if (u.EndsWith("/personal-operacion/combos")) return Ok(CombosPersonal());
            if (u.EndsWith("/personal-operacion/disponibles")) return Ok(SinDisponibles ? new List<PersonaDisponible>() : new List<PersonaDisponible> { new() { COD_ENTIDAD_PERSONAL = 7, COD_PERSONAL = "P7", DES_PERSONAL = "ANA PEREZ" }, new() { COD_ENTIDAD_PERSONAL = 8, COD_PERSONAL = "P8", DES_PERSONAL = "LUIS RAMOS" } });
            if (u.EndsWith("/personal-operacion/resumen"))
                return Ok(new List<PersonalResumenItem> { new() { COD_CATEGORIA_OCUPACIONAL = "A", DES_CATEGORIA_OCUPACIONAL = "ESPECIALIZADO", COD_PROCESO = "COSTU", DES_PROCESO = "COSTURA", CAN_PERSONAS = 1 },
                                                           new() { COD_CATEGORIA_OCUPACIONAL = "B", DES_CATEGORIA_OCUPACIONAL = "INTERMEDIO", COD_PROCESO = "COSTU", DES_PROCESO = "COSTURA", CAN_PERSONAS = 1, CAN_CON_OPERACION = 1 } });
            if (u.EndsWith("/personal-operacion")) return Ok(Personal);
            if (u.EndsWith("/lineas-armados/combos")) return Ok(CombosArmado());
            if (u.EndsWith("/lineas-armados/siguiente-codigo")) return Ok("AR0004");
            if (u.EndsWith("/lineas-armados/AR1")) return Ok(Cabecera());
            if (u.EndsWith("/lineas-armados/AR1/puestos")) return Ok(PuestosApi());
            if (u.EndsWith("/lineas-armados/AR1/hojas")) return Ok(HojasApi());
            if (u.EndsWith("/lineas-armados/AR1/avisos")) return Ok(Avisos());
            if (u.EndsWith("/lineas-armados/AR1/operarios-por-categoria")) return Ok(new List<ArmadoOperariosCategoria> { new() { COD_CATEGORIA_OCUPACIONAL = "A", DES_CATEGORIA_OCUPACIONAL = "ESPECIALIZADO", CAN_PUESTOS = 1, CAN_OPERARIOS = 1 }, new() { COD_CATEGORIA_OCUPACIONAL = "B", DES_CATEGORIA_OCUPACIONAL = "INTERMEDIO", CAN_PUESTOS = 1, CAN_OPERARIOS = 1 } });
            if (u.EndsWith("/lineas-armados")) return Ok(new List<Armado> { Cabecera() });
            return Ok(new object[0]);
        }
        if (FallaEscritura) return Mal(MensajeFalla);
        if (r.Method == HttpMethod.Put && u.EndsWith("/lineas-armados/AR1/puestos"))
        {   // el reparto guardado pasa a ser el vigente
            JsonElement P(JsonElement e, string n) => e.EnumerateObject().First(x => string.Equals(x.Name, n, StringComparison.OrdinalIgnoreCase)).Value;
            Puestos = P(body!.Value, "Puestos").EnumerateArray().Select(p => (P(p, "NUM_PUESTO").GetInt32(), P(p, "DES_PUESTO").GetString()!, P(p, "NUM_OPERARIOS").GetInt32(),
                P(p, "HOJAS").EnumerateArray().Select(x => x.GetInt32()).ToArray())).ToList();
            return Ok(true);
        }
        if (r.Method == HttpMethod.Post && u.EndsWith("/lineas-armados/AR1/sugerir"))
        {
            Puestos = new() { (1, "PUESTO 01", 1, new[] { 11, 12 }), (2, "PUESTO 02", 1, new[] { 13, 14 }) };
            return Ok(new ArmadoSugerido { CAN_PUESTOS = 2, CAN_OPERARIOS = 2 });
        }
        return Ok(true);
    }
}

public class OperacionesPersonalLineasTests : TestContext
{
    readonly FakeOperacionesPersonalLineas _f = new();

    void Base()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton(new HttpClient(_f) { BaseAddress = new Uri("http://x/") });
        Services.AddBlazoredLocalStorage();
        Services.AddSingleton(new SeguridadContextService(new FakeAuthConAuditoria(), null!));   // el PanelAuditoria de los dialogos pregunta si el perfil ve la auditoria
        Services.AddScoped<ErrorLogService>(); Services.AddScoped<ConfigService>(); Services.AddScoped<EtiquetasService>();
        Services.AddSingleton<Microsoft.Extensions.Configuration.IConfiguration>(new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build());
        Services.AddScoped<AccessLogService>(); Services.AddScoped<MenuService>(); Services.AddScoped<ControlCambiosClientService>();
        ComponentFactories.AddStub<AccionesBotones>();
        RenderComponent<MudPopoverProvider>(); RenderComponent<MudSnackbarProvider>();
    }

    protected IRenderedComponent<MudDialogProvider> Prov() { Base(); return RenderComponent<MudDialogProvider>(); }

    async Task<IDialogReference> Abre<T>(IRenderedComponent<MudDialogProvider> p, DialogParameters pr, string esperar) where T : Microsoft.AspNetCore.Components.IComponent
    {
        var ds = Services.GetRequiredService<IDialogService>();
        IDialogReference? d = null;
        await p.InvokeAsync(async () => d = await ds.ShowAsync<T>("t", pr, new DialogOptions { MaxWidth = MaxWidth.ExtraLarge }));
        p.WaitForAssertion(() => Assert.Contains(esperar, p.Markup));
        return d!;
    }

    static JsonElement Prop(JsonElement e, string n) => e.EnumerateObject().First(x => string.Equals(x.Name, n, StringComparison.OrdinalIgnoreCase)).Value;
    (string M, string U, JsonElement? B) Ultima(string metodo, string fin) => _f.Calls.Last(c => c.M == metodo && c.U.EndsWith(fin));
    int Cuantas(string metodo, string fin) => _f.Calls.Count(c => c.M == metodo && c.U.EndsWith(fin));

    IRenderedComponent<MudSelect<T>> Sel<T>(IRenderedComponent<MudDialogProvider> p, string label) => p.FindComponents<MudSelect<T>>().First(c => c.Instance.Label == label);
    async Task Elige<T>(IRenderedComponent<MudDialogProvider> p, string label, T valor) => await p.InvokeAsync(() => Sel<T>(p, label).Instance.ValueChanged.InvokeAsync(valor));
    string Kpi(IRenderedComponent<MudDialogProvider> p, string k) => p.Find($"[data-kpi='{k}'] b").TextContent.Trim();

    // ======================= ficha del armado (O6) =======================
    Task<IDialogReference> Ficha(IRenderedComponent<MudDialogProvider> p) => Abre<ArmadoFichaDialog>(p, new DialogParameters { ["Armado"] = new Armado { COD_ARMADO = "AR1", DES_ARMADO = "POLO BASICO" } }, "PEGAR MANGA");

    [Fact]
    public async Task Ficha_MuestraElBalanceLosPuestosYLasOperacionesLibres()
    {
        var p = Prov(); await Ficha(p);
        Assert.Equal("1.360", Kpi(p, "takt")); Assert.Equal("1.400", Kpi(p, "ciclo")); Assert.Equal("291", Kpi(p, "capacidad")); Assert.Equal("2", Kpi(p, "operarios")); Assert.Equal("86.0 %", Kpi(p, "balanceo"));
        Assert.Contains("meta 300", p.Find("[data-kpi='capacidad']").TextContent);
        Assert.Contains("mínimo teórico 2", p.Find("[data-kpi='operarios']").TextContent);
        Assert.Contains("LINEAL", p.Find("[data-chip='disposicion']").TextContent);
        var p1 = p.Find("[data-puesto='1']");
        Assert.Contains("COSER COLLARETE", p1.TextContent); Assert.Contains("PEGAR CUELLO", p1.TextContent); Assert.DoesNotContain("PEGAR MANGA", p1.TextContent);
        Assert.Equal("1.400 min", p1.QuerySelector("[data-dato='minutos']")!.TextContent.Trim());
        Assert.Equal("102.9 % del takt", p1.QuerySelector("[data-dato='carga']")!.TextContent.Trim());
        Assert.Contains("PEGAR MANGA", p.Find("[data-puesto='2']").TextContent);
        var libres = p.Find("[data-zona='libres']");
        Assert.Contains("REMATE Y LIMPIEZA", libres.TextContent); Assert.Contains("OJAL Y BOTON", libres.TextContent); Assert.Contains("sin tiempo", libres.TextContent);
        Assert.Equal(2, libres.QuerySelectorAll("tr[data-hoja]").Length);
        Assert.Empty(p.FindAll("[data-chip='sin-guardar']"));
    }

    [Fact]
    public async Task Ficha_MuestraAvisosYOperariosPorCategoria()
    {
        var p = Prov(); await Ficha(p);
        Assert.Contains("REMATE Y LIMPIEZA", p.Find("[data-aviso='SIN_PUESTO']").TextContent);
        Assert.Contains("takt es 1.360", p.Find("[data-aviso='SOBRE_TAKT']").TextContent);
        Assert.Contains("ESPECIALIZADO", p.Find("[data-cat='A']").TextContent);
        Assert.Contains("INTERMEDIO", p.Find("[data-cat='B']").TextContent);
    }

    [Fact]
    public async Task Ficha_UnaOperacionSinTiempoNoSePuedeRepartir()
    {
        var p = Prov(); await Ficha(p);
        Assert.NotNull(p.Find("[data-zona='libres'] tr[data-hoja='15']"));
        Assert.Equal(1, p.FindComponents<MudSelect<int>>().Count(c => c.Instance.Disabled));   // solo una: la de la hoja sin tiempo
        Assert.True(p.Find("[data-zona='libres'] tr[data-hoja='15']").QuerySelector(".mud-input-control")!.ClassList.Contains("mud-disabled") || p.Find("[data-zona='libres'] tr[data-hoja='15']").InnerHtml.Contains("disabled"));
    }

    [Fact]
    public async Task Ficha_MoverUnaOperacion_RecalculaLoLocalYMarcaCambiosSinGuardar_YGuardaElReparto()
    {
        var p = Prov(); await Ficha(p);
        var sel = p.FindComponents<MudSelect<int>>().First(c => !c.Instance.Disabled && c.Instance.Value == 0);        // REMATE (11 y 12 estan en el puesto 1; 13 en el 2; 14 libre)
        await p.InvokeAsync(() => sel.Instance.ValueChanged.InvokeAsync(2));
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-chip='sin-guardar']")));
        Assert.Contains("REMATE Y LIMPIEZA", p.Find("[data-puesto='2']").TextContent);
        Assert.Equal("1.448 min", p.Find("[data-puesto='2'] [data-dato='minutos']").TextContent.Trim());     // 1.008 + 0.440
        Assert.Equal(1, p.Find("[data-zona='libres']").QuerySelectorAll("tr[data-hoja]").Length);
        Assert.NotEmpty(p.FindAll("[data-zona='nota-sucio']"));
        Assert.Equal(0, Cuantas("PUT", "/lineas-armados/AR1/puestos"));                                            // nada se guarda solo

        await p.InvokeAsync(() => p.Find("[data-accion='guardar']").Click());
        p.WaitForAssertion(() => Assert.Empty(p.FindAll("[data-chip='sin-guardar']")));
        var put = Ultima("PUT", "/lineas-armados/AR1/puestos").B!.Value;
        var ps = Prop(put, "Puestos").EnumerateArray().ToList();
        Assert.Equal(2, ps.Count);
        Assert.Equal(new[] { 11, 12 }, Prop(ps[0], "HOJAS").EnumerateArray().Select(x => x.GetInt32()).ToArray());
        Assert.Equal(new[] { 13, 14 }, Prop(ps[1], "HOJAS").EnumerateArray().Select(x => x.GetInt32()).ToArray());
        Assert.Equal("AR1", Prop(put, "COD_ARMADO").GetString());
        Assert.Contains("REMATE Y LIMPIEZA", p.Find("[data-puesto='2']").TextContent);                             // recargado: sigue ahi
    }

    [Fact]
    public async Task Ficha_CambiarOperarios_AjustaElCicloDelPuestoYSeGuarda()
    {
        var p = Prov(); await Ficha(p);
        var num = p.FindComponents<MudNumericField<int>>().First();
        await p.InvokeAsync(() => num.Instance.ValueChanged.InvokeAsync(2));
        p.WaitForAssertion(() => Assert.Equal("ciclo 0.700", p.Find("[data-puesto='1'] [data-dato='ciclo']").TextContent.Trim()));   // 1.4 / 2
        Assert.Equal("51.5 % del takt", p.Find("[data-puesto='1'] [data-dato='carga']").TextContent.Trim());
        await p.InvokeAsync(() => p.Find("[data-accion='guardar']").Click());
        p.WaitForAssertion(() => Assert.Equal(2, Prop(Prop(Ultima("PUT", "/lineas-armados/AR1/puestos").B!.Value, "Puestos").EnumerateArray().First(), "NUM_OPERARIOS").GetInt32()));
    }

    [Fact]
    public async Task Ficha_AgregarYQuitarPuestos()
    {
        var p = Prov(); await Ficha(p);
        await p.InvokeAsync(() => p.Find("[data-accion='agregar-puesto']").Click());
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-puesto='3']")));
        Assert.Contains("Sin operaciones", p.Find("[data-puesto='3']").TextContent);
        Assert.Equal("PUESTO 03", p.Find("[data-puesto='3'] [data-campo='nombre-puesto']").GetAttribute("value"));
        // quitar el puesto 1: sus dos operaciones quedan sin asignar
        await p.InvokeAsync(() => p.Find("[data-puesto='1'] [data-accion='quitar-puesto']").Click());
        p.WaitForAssertion(() => Assert.Empty(p.FindAll("[data-puesto='1']")));
        Assert.Equal(4, p.Find("[data-zona='libres']").QuerySelectorAll("tr[data-hoja]").Length);
        await p.InvokeAsync(() => p.Find("[data-accion='guardar']").Click());
        p.WaitForAssertion(() => Assert.Empty(p.FindAll("[data-chip='sin-guardar']")));
        var nums = Prop(Ultima("PUT", "/lineas-armados/AR1/puestos").B!.Value, "Puestos").EnumerateArray().Select(x => Prop(x, "NUM_PUESTO").GetInt32()).ToArray();
        Assert.Equal(new[] { 2, 3 }, nums);
    }

    [Fact]
    public async Task Ficha_ElPuestoNuevoTomaElNumeroSiguienteAlMayor_AunqueFalteUnoIntermedio()
    {
        var p = Prov(); await Ficha(p);
        await p.InvokeAsync(() => p.Find("[data-puesto='1'] [data-accion='quitar-puesto']").Click());      // quedan los puestos {2}
        await p.InvokeAsync(() => p.Find("[data-accion='agregar-puesto']").Click());
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-puesto='3']")));
        Assert.Equal(2, p.FindAll("[data-puesto]").Count);                                                  // 2 y 3: no se repite el 2
        Assert.Contains("PEGAR MANGA", p.Find("[data-puesto='2']").TextContent);
        Assert.DoesNotContain("PEGAR MANGA", p.Find("[data-puesto='3']").TextContent);
    }

    [Fact]
    public async Task Ficha_DescartarCambios_VuelveAlUltimoGuardado()
    {
        var p = Prov(); await Ficha(p);
        await p.InvokeAsync(() => p.Find("[data-puesto='1'] [data-accion='quitar-puesto']").Click());
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-chip='sin-guardar']")));
        await p.InvokeAsync(() => p.Find("[data-accion='descartar']").Click());
        p.WaitForAssertion(() => Assert.Empty(p.FindAll("[data-chip='sin-guardar']")));
        Assert.NotEmpty(p.FindAll("[data-puesto='1']"));
        Assert.Equal(0, Cuantas("PUT", "/lineas-armados/AR1/puestos"));
    }

    [Fact]
    public async Task Ficha_ElRepartoInvalidoMuestraElMensajeDeLaApi_ConservaLosCambios()
    {
        var p = Prov(); await Ficha(p);
        await p.InvokeAsync(() => p.Find("[data-puesto='1'] [data-accion='quitar-puesto']").Click());
        _f.FallaEscritura = true;
        await p.InvokeAsync(() => p.Find("[data-accion='guardar']").Click());
        p.WaitForAssertion(() => Assert.Contains("dos puestos", p.Find("[data-error]").TextContent));
        Assert.NotEmpty(p.FindAll("[data-chip='sin-guardar']"));                      // no se pierde lo editado
        Assert.Empty(p.FindAll("[data-puesto='1']"));
    }

    [Fact]
    public async Task Ficha_Sugerir_PideConfirmacion_YReemplazaElReparto()
    {
        var p = Prov(); await Ficha(p);
        await p.InvokeAsync(() => p.Find("[data-accion='sugerir']").Click());
        p.WaitForAssertion(() => Assert.Contains("REEMPLAZAN", p.Markup));
        Assert.Equal(0, Cuantas("POST", "/lineas-armados/AR1/sugerir"));              // aun no
        await p.InvokeAsync(() => p.FindAll("button").First(b => b.TextContent.Trim() == "Sugerir" && b.GetAttribute("data-accion") is null).Click());
        p.WaitForAssertion(() => Assert.Equal(1, Cuantas("POST", "/lineas-armados/AR1/sugerir")));
        p.WaitForAssertion(() => Assert.Contains("REMATE Y LIMPIEZA", p.Find("[data-puesto='2']").TextContent));   // lo que armo el servidor se ve
    }

    [Fact]
    public async Task Ficha_Sugerir_CancelarNoHaceNada()
    {
        var p = Prov(); await Ficha(p);
        await p.InvokeAsync(() => p.Find("[data-accion='sugerir']").Click());
        p.WaitForAssertion(() => Assert.Contains("REEMPLAZAN", p.Markup));
        await p.InvokeAsync(() => p.FindAll("button").First(b => b.TextContent.Trim() == "Cancelar").Click());
        Assert.Equal(0, Cuantas("POST", "/lineas-armados/AR1/sugerir"));
    }

    [Fact]
    public async Task Ficha_Modular_ExplicaQueLosOperariosSeReparteElTrabajo()
    {
        _f.Regla = "SUMA_OPERARIOS";
        var p = Prov(); await Ficha(p);
        Assert.Contains("minutos ÷ operarios", p.Find("[data-kpi='ciclo']").TextContent);
        Assert.Contains("los operarios se reparten el trabajo", p.Find("[data-kpi='balanceo']").TextContent);
        Assert.Contains("MODULAR", p.Find("[data-chip='disposicion']").TextContent);
    }

    // ======================= dialogo del armado (O6) =======================
    protected Task<IDialogReference> DlgArmado(IRenderedComponent<MudDialogProvider> p, Armado? it, CrudModo modo, string esperar) => Abre<ArmadoDialog>(p, new DialogParameters { ["Item"] = it, ["Modo"] = modo }, esperar);

    [Fact]
    public async Task ArmadoDialogo_Nuevo_ProponeElCodigo_ElProcesoDeLaRuta_YLaDisposicionDeLaLinea()
    {
        var p = Prov(); await DlgArmado(p, null, CrudModo.Create, "Ruta de producción");
        p.WaitForAssertion(() => Assert.Equal("AR0004", p.Find("[data-campo='cod']").GetAttribute("value")));
        await Elige(p, "Ruta de producción", "RT1");
        p.WaitForAssertion(() => Assert.Equal("COSTU", Sel<string>(p, "Proceso de la ruta").Instance.Value));          // RT1 tiene un solo proceso: se elige solo
        await Elige(p, "Línea de producción (opcional)", "M01");
        p.WaitForAssertion(() => Assert.Equal("MOD", Sel<string>(p, "Disposición").Instance.Value));                     // la disposicion por defecto de la linea
        Assert.Contains("se reparten el trabajo", p.Find("[data-zona='regla']").TextContent);
        await Elige(p, "Disposición", "LIN");
        p.WaitForAssertion(() => Assert.Contains("puesto más lento", p.Find("[data-zona='regla']").TextContent));
    }

    [Fact]
    public async Task ArmadoDialogo_RutaConVariosProcesos_NoElijeElProceso_YLasLineasSonDelProceso()
    {
        var p = Prov(); await DlgArmado(p, null, CrudModo.Create, "Ruta de producción");
        await Elige(p, "Ruta de producción", "RT2");
        p.WaitForAssertion(() => Assert.True(string.IsNullOrEmpty(Sel<string>(p, "Proceso de la ruta").Instance.Value)));
        await p.InvokeAsync(() => Sel<string>(p, "Proceso de la ruta").Instance.ValueChanged.InvokeAsync("EMPQE"));
        p.WaitForAssertion(() => Assert.Equal("EMPQE", Sel<string>(p, "Proceso de la ruta").Instance.Value));
    }

    [Fact]
    public async Task ArmadoDialogo_Guardar_ValidaYEnviaLosParametros()
    {
        var p = Prov(); await DlgArmado(p, null, CrudModo.Create, "Ruta de producción");
        await p.InvokeAsync(() => p.Find("[data-accion='guardar']").Click());
        p.WaitForAssertion(() => Assert.Contains("nombre son obligatorios", p.Find("[data-error]").TextContent));
        await p.InvokeAsync(() => p.FindComponents<MudTextField<string>>().First(c => c.Instance.Label == "Nombre del armado").Instance.ValueChanged.InvokeAsync("Polo prueba"));
        await p.InvokeAsync(() => p.Find("[data-accion='guardar']").Click());
        p.WaitForAssertion(() => Assert.Contains("Elige la ruta", p.Find("[data-error]").TextContent));
        await Elige(p, "Ruta de producción", "RT1");
        await Elige(p, "Disposición", "LIN");
        await p.InvokeAsync(() => p.FindComponents<MudNumericField<decimal?>>().First(c => c.Instance.Label == "Eficiencia (%)").Instance.ValueChanged.InvokeAsync(120m));
        await p.InvokeAsync(() => p.Find("[data-accion='guardar']").Click());
        p.WaitForAssertion(() => Assert.Contains("entre 1 y 100", p.Find("[data-error]").TextContent));
        Assert.Equal(0, Cuantas("POST", "/lineas-armados"));
        await p.InvokeAsync(() => p.FindComponents<MudNumericField<decimal?>>().First(c => c.Instance.Label == "Eficiencia (%)").Instance.ValueChanged.InvokeAsync(80m));
        await p.InvokeAsync(() => p.FindComponents<MudNumericField<decimal?>>().First(c => c.Instance.Label == "Meta de unidades por turno").Instance.ValueChanged.InvokeAsync(250m));
        await p.InvokeAsync(() => p.Find("[data-accion='guardar']").Click());
        p.WaitForAssertion(() => Assert.Equal(1, Cuantas("POST", "/lineas-armados")));
        var b = Ultima("POST", "/lineas-armados").B!.Value;
        Assert.Equal("AR0004", Prop(b, "COD_ARMADO").GetString()); Assert.Equal("Polo prueba", Prop(b, "DES_ARMADO").GetString()); Assert.Equal("RT1", Prop(b, "COD_RUTA_PRODUCCION").GetString());
        Assert.Equal("COSTU", Prop(b, "COD_PROCESO").GetString()); Assert.Equal("LIN", Prop(b, "COD_TIPO_DISTRIBUCION").GetString());
        Assert.Equal(480m, Prop(b, "VAL_MINUTOS_TURNO").GetDecimal()); Assert.Equal(80m, Prop(b, "TAS_EFICIENCIA").GetDecimal()); Assert.Equal(250m, Prop(b, "VAL_META_UNIDADES").GetDecimal());
    }

    [Fact]
    public async Task ArmadoDialogo_Editar_PrecargaYModifica_YMuestraElErrorDeLaApi()
    {
        var p = Prov(); await DlgArmado(p, _f.Cabecera(), CrudModo.Update, "Ruta de producción");
        Assert.Equal("AR1", p.Find("[data-campo='cod']").GetAttribute("value"));
        Assert.True(p.Find("[data-campo='cod']").HasAttribute("disabled"));          // el codigo no se cambia
        Assert.Equal("RT1", Sel<string>(p, "Ruta de producción").Instance.Value);
        Assert.Equal("L01", Sel<string>(p, "Línea de producción (opcional)").Instance.Value);
        _f.FallaEscritura = true; _f.MensajeFalla = "El armado ya tiene operaciones repartidas: no se puede cambiar su ruta ni su proceso.";
        await Elige(p, "Ruta de producción", "RT2");
        await p.InvokeAsync(() => p.Find("[data-accion='guardar']").Click());
        p.WaitForAssertion(() => Assert.Contains("ya tiene operaciones repartidas", p.Find("[data-error]").TextContent));
        _f.FallaEscritura = false;
        await Elige(p, "Ruta de producción", "RT1");
        await p.InvokeAsync(() => p.Find("[data-accion='guardar']").Click());
        p.WaitForAssertion(() => Assert.Equal(2, Cuantas("PUT", "/lineas-armados")));          // el intento fallido y el bueno
        Assert.Equal("AR1", Prop(Ultima("PUT", "/lineas-armados").B!.Value, "COD_ARMADO").GetString());
        Assert.Equal("RT1", Prop(Ultima("PUT", "/lineas-armados").B!.Value, "COD_RUTA_PRODUCCION").GetString());
    }

    [Fact]
    public async Task ArmadoDialogo_Eliminar_PideConfirmacionYBorra()
    {
        var p = Prov(); await DlgArmado(p, _f.Cabecera(), CrudModo.Delete, "puestos y el reparto");
        Assert.Equal(0, Cuantas("DELETE", "/lineas-armados/AR1"));
        await p.InvokeAsync(() => p.Find("[data-accion='confirmar-eliminar']").Click());
        p.WaitForAssertion(() => Assert.Equal(1, Cuantas("DELETE", "/lineas-armados/AR1")));
    }

    // ======================= dialogo del personal (O5) =======================
    Task<IDialogReference> DlgPersonal(IRenderedComponent<MudDialogProvider> p, PersonalOperacionItem? it, CrudModo modo, string esperar) => Abre<PersonalOperacionDialog>(p, new DialogParameters { ["Item"] = it, ["Modo"] = modo }, esperar);

    [Fact]
    public async Task PersonalDialogo_Nuevo_OfreceSoloPersonasSinCategoria_YAvisaSiNoHay()
    {
        var p = Prov(); await DlgPersonal(p, null, CrudModo.Create, "Persona (activa, sin categoría)");
        p.WaitForAssertion(() => Assert.Equal(2, Sel<int>(p, "Persona (activa, sin categoría)").FindComponents<MudSelectItem<int>>().Count));
        Assert.Empty(p.FindAll("[data-zona='sin-disponibles']"));
    }

    [Fact]
    public async Task PersonalDialogo_SinPersonasDisponibles_LoDice()
    {
        _f.SinDisponibles = true;
        var p = Prov(); await DlgPersonal(p, null, CrudModo.Create, "Persona (activa, sin categoría)");
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-zona='sin-disponibles']")));
    }

    [Fact]
    public async Task PersonalDialogo_LasOperacionesSonDelProcesoYDeLaCategoria_YSeLimpianSiYaNoCorresponden()
    {
        var p = Prov(); await DlgPersonal(p, null, CrudModo.Create, "Persona (activa, sin categoría)");
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindComponents<MudSelectItem<int>>()));
        await Elige(p, "Proceso", "COSTU");
        await Elige(p, "Categoría", "B");
        string[] Ops() => Sel<string>(p, "Operación fija (opcional)").FindComponents<MudSelectItem<string>>().Select(i => i.Instance.Value!).ToArray();
        p.WaitForAssertion(() => Assert.Equal(new[] { "OP0005", "OP0099" }, Ops()));        // de COSTU y categoria B (o sin categoria); no OP0036 (EMPQE) ni las de A o D
        await p.InvokeAsync(() => Sel<string>(p, "Operación fija (opcional)").Instance.ValueChanged.InvokeAsync("OP0005"));
        await Elige(p, "Categoría", "A");
        p.WaitForAssertion(() => Assert.True(string.IsNullOrEmpty(Sel<string>(p, "Operación fija (opcional)").Instance.Value)));   // OP0005 es de B: se limpia
        p.WaitForAssertion(() => Assert.Equal(new[] { "OP0008", "OP0099" }, Ops()));
        Assert.Contains("Operaciones criticas", p.Find("[data-zona='criterio']").TextContent);       // el criterio de la categoria A
        await Elige(p, "Proceso", "EMPQE");
        p.WaitForAssertion(() => Assert.Empty(Ops()));                                                // EMPQE no tiene operaciones de categoria A
    }

    [Fact]
    public async Task PersonalDialogo_AlCambiarElProceso_SeLimpianElSubprocesoYLaOperacion()
    {
        var p = Prov(); await DlgPersonal(p, null, CrudModo.Create, "Persona (activa, sin categoría)");
        await Elige(p, "Proceso", "COSTU"); await Elige(p, "Categoría", "D");
        await Elige(p, "Subproceso", "ARMDO");
        await p.InvokeAsync(() => Sel<string>(p, "Operación fija (opcional)").Instance.ValueChanged.InvokeAsync("OP0014"));
        p.WaitForAssertion(() => Assert.Equal("ARMDO", Sel<string>(p, "Subproceso").Instance.Value));
        await Elige(p, "Proceso", "EMPQE");
        p.WaitForAssertion(() => Assert.True(string.IsNullOrEmpty(Sel<string>(p, "Subproceso").Instance.Value)));            // ARMDO es de COSTU
        Assert.True(string.IsNullOrEmpty(Sel<string>(p, "Operación fija (opcional)").Instance.Value));                      // OP0014 tambien
    }

    [Fact]
    public async Task PersonalDialogo_Guardar_ValidaYEnvia()
    {
        var p = Prov(); await DlgPersonal(p, null, CrudModo.Create, "Persona (activa, sin categoría)");
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindComponents<MudSelectItem<int>>()));
        await p.InvokeAsync(() => p.Find("[data-accion='guardar']").Click());
        p.WaitForAssertion(() => Assert.Contains("Elige la persona", p.Find("[data-error]").TextContent));
        await Elige(p, "Persona (activa, sin categoría)", 8);
        await p.InvokeAsync(() => p.Find("[data-accion='guardar']").Click());
        p.WaitForAssertion(() => Assert.Contains("Elige la categoría", p.Find("[data-error]").TextContent));
        await Elige(p, "Categoría", "D");
        await p.InvokeAsync(() => p.Find("[data-accion='guardar']").Click());
        p.WaitForAssertion(() => Assert.Contains("Elige el proceso", p.Find("[data-error]").TextContent));
        Assert.Equal(0, Cuantas("POST", "/personal-operacion"));
        await Elige(p, "Proceso", "COSTU");
        await Elige(p, "Subproceso", "ARMDO");
        await p.InvokeAsync(() => Sel<string>(p, "Operación fija (opcional)").Instance.ValueChanged.InvokeAsync("OP0014"));
        _f.FallaEscritura = true; _f.MensajeFalla = "La operacion es de otra categoria: la categoria de la persona debe ser la de su operacion.";
        await p.InvokeAsync(() => p.Find("[data-accion='guardar']").Click());
        p.WaitForAssertion(() => Assert.Contains("otra categoria", p.Find("[data-error]").TextContent));
        _f.FallaEscritura = false;
        await p.InvokeAsync(() => p.Find("[data-accion='guardar']").Click());
        p.WaitForAssertion(() => Assert.Equal(2, Cuantas("POST", "/personal-operacion")));
        var b = Ultima("POST", "/personal-operacion").B!.Value;
        Assert.Equal(8, Prop(b, "COD_ENTIDAD_PERSONAL").GetInt32()); Assert.Equal("D", Prop(b, "COD_CATEGORIA_OCUPACIONAL").GetString()); Assert.Equal("COSTU", Prop(b, "COD_PROCESO").GetString());
        Assert.Equal("ARMDO", Prop(b, "COD_SUB_PROCESO").GetString()); Assert.Equal("OP0014", Prop(b, "COD_OPERACION").GetString());
    }

    [Fact]
    public async Task PersonalDialogo_Editar_NoCambiaLaPersona_YEnviaPut()
    {
        var p = Prov(); await DlgPersonal(p, _f.Personal[1], CrudModo.Update, "Persona");
        Assert.True(p.Find("[data-campo='persona-fija']").HasAttribute("disabled"));
        Assert.Contains("EJ021", p.Find("[data-campo='persona-fija']").GetAttribute("value"));
        Assert.Equal("B", Sel<string>(p, "Categoría").Instance.Value); Assert.Equal("OP0005", Sel<string>(p, "Operación fija (opcional)").Instance.Value);
        await p.InvokeAsync(() => p.Find("[data-accion='guardar']").Click());
        p.WaitForAssertion(() => Assert.Equal(1, Cuantas("PUT", "/personal-operacion")));
        Assert.Equal(2, Prop(Ultima("PUT", "/personal-operacion").B!.Value, "COD_ENTIDAD_PERSONAL").GetInt32());
        Assert.Equal(0, Cuantas("GET", "/personal-operacion/disponibles"));          // editar no pide la lista de disponibles
    }

    [Fact]
    public async Task PersonalDialogo_Quitar_PideConfirmacionYBorra()
    {
        var p = Prov(); await DlgPersonal(p, _f.Personal[0], CrudModo.Delete, "solo deja de tener categoría");
        Assert.Equal(0, Cuantas("DELETE", "/personal-operacion/1"));
        await p.InvokeAsync(() => p.Find("[data-accion='confirmar-eliminar']").Click());
        p.WaitForAssertion(() => Assert.Equal(1, Cuantas("DELETE", "/personal-operacion/1")));
    }

    // ======================= pantallas =======================
    [Fact]
    public void PantallaPersonal_MuestraElResumenPorCategoria_ElPersonalYFiltra()
    {
        Base(); RenderComponent<MudDialogProvider>();
        var pg = RenderComponent<PersonalOperacion>();
        pg.WaitForAssertion(() => Assert.Contains("EJEMPLO COSTURERA A 01", pg.Markup));
        Assert.Contains("A · ESPECIALIZADO", pg.Find("[data-resumen='A']").TextContent);
        Assert.Contains("personal activo con categoría: 2", pg.Find("[data-zona='resumen']").TextContent);
        Assert.Contains("cesado", pg.Markup);                                          // el cesado se marca
        Assert.Equal(3, pg.FindAll("tbody tr").Count(tr => tr.TextContent.Contains("EJEMPLO")));
        pg.Find("[data-resumen='B']").Click();                                          // el chip filtra por categoria
        pg.WaitForAssertion(() => Assert.Equal(1, pg.FindAll("tbody tr").Count(tr => tr.TextContent.Contains("EJEMPLO"))));
        Assert.Contains("EJEMPLO COSTURERA B 01", pg.Markup);
        Assert.Contains("UNIR HOMBROS", pg.Markup);
    }

    [Fact]
    public void PantallaArmados_MuestraElBalanceDeCadaArmado()
    {
        Base(); RenderComponent<MudDialogProvider>();
        var pg = RenderComponent<ArmadoLineas>();
        pg.WaitForAssertion(() => Assert.Contains("POLO BASICO", pg.Markup));
        var fila = pg.Find("tbody tr");
        Assert.Contains("AR1", fila.TextContent); Assert.Contains("RT1", fila.TextContent); Assert.Contains("LINEAL", fila.TextContent); Assert.Contains("LINEA DE COSTURA 1", fila.TextContent);
        Assert.Contains("1.360", fila.TextContent);                                     // takt
        Assert.Contains("291", fila.TextContent); Assert.Contains("/ 300", fila.TextContent);   // capacidad / meta
        Assert.Contains("86.0 %", fila.TextContent);
        Assert.Contains("2 de 5", fila.TextContent);                                    // faltan 2 operaciones por asignar (14 y 15)
    }

    // ======================= grillas: orden, filtro y alto automatico =======================
    [Fact]
    public void PantallaPersonal_LaGrillaTieneCabecerasConOrdenYFiltro_AltoAutomaticoYTirador()
    {
        Base(); RenderComponent<MudDialogProvider>();
        var pg = RenderComponent<PersonalOperacion>();
        pg.WaitForAssertion(() => Assert.Contains("EJEMPLO COSTURERA A 01", pg.Markup));
        var caja = pg.Find("[data-resizable-grid='personal-operacion']");            // gridAutoAlto la estira hasta la barra de estado
        Assert.NotNull(caja.QuerySelector(":scope > .grid-resize-handle"));          // tirador para ajustar el alto
        Assert.Equal(new[] { "Código", "Persona", "Cat.", "Proceso", "Subproceso", "Operación fija", "Estado" }, pg.FindAll("th .eo-titulo").Select(x => x.TextContent.Trim()).ToArray());
        Assert.Equal(7, pg.FindComponents<FiltroExcelColumna<KONSolutions.Shared.Models.Ingenieria.PersonalOperacionItem>>().Count);   // un embudo por columna con datos
        Assert.DoesNotContain("calc(100vh", pg.Markup);                              // ya no usa un alto fijo
    }

    [Fact]
    public void PantallaPersonal_ClicEnLaCabeceraOrdenaAscendenteYLuegoDescendente()
    {
        Base(); RenderComponent<MudDialogProvider>();
        var pg = RenderComponent<PersonalOperacion>();
        pg.WaitForAssertion(() => Assert.Contains("EJEMPLO COSTURERA A 01", pg.Markup));
        string[] Orden() => pg.FindAll("tbody tr").Where(tr => tr.TextContent.Contains("EJEMPLO")).Select(tr => tr.QuerySelector("td")!.TextContent.Trim()).ToArray();
        var original = Orden();
        Assert.True(original.Length >= 3);
        pg.FindAll("th .eo-titulo").First(x => x.TextContent.Trim() == "Código").Click();
        pg.WaitForAssertion(() => Assert.Equal(original.OrderBy(x => x, StringComparer.Ordinal).ToArray(), Orden()));
        pg.FindAll("th .eo-titulo").First(x => x.TextContent.Trim() == "Código").Click();
        pg.WaitForAssertion(() => Assert.Equal(original.OrderByDescending(x => x, StringComparer.Ordinal).ToArray(), Orden()));
    }

    [Fact]
    public void PantallaArmados_LaGrillaTieneCabecerasConOrdenYFiltro_AltoAutomaticoYTirador()
    {
        Base(); RenderComponent<MudDialogProvider>();
        var pg = RenderComponent<ArmadoLineas>();
        pg.WaitForAssertion(() => Assert.Contains("POLO BASICO", pg.Markup));
        Assert.NotNull(pg.Find("[data-resizable-grid='armado-lineas']").QuerySelector(":scope > .grid-resize-handle"));
        Assert.Equal(new[] { "Código", "Armado", "Disposición", "Línea", "Puestos", "Operarios", "Takt", "Ciclo", "Capacidad / meta", "Balanceo", "Sin asignar", "Estado" },
            pg.FindAll("th .eo-titulo").Select(x => x.TextContent.Trim()).ToArray());
        Assert.Equal(12, pg.FindComponents<FiltroExcelColumna<KONSolutions.Shared.Models.Ingenieria.Armado>>().Count);
    }
}
