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

// Personal del armado, comparacion de disposiciones (script SQL 127) y migracion de operaciones de rutas existentes (script SQL 128), contra una API simulada.
public class FakeO7O8 : HttpMessageHandler
{
    public List<(string M, string U, JsonElement? B)> Calls = new();
    public bool FallaEscritura; public string MensajeFalla = "Una persona no puede estar en dos puestos del mismo armado.";

    // armado AR1: puesto 1 (A, 2 operarios) con Ana; puesto 2 (B, 1 operario) vacio
    public List<(int Pst, int Per)> Asignado = new() { (1, 1) };
    static readonly (int Id, string Cod, string Des, string Cat, int Ord, string? Op, string? DesOp)[] Gente =
    {
        (1, "EJ011", "ANA A", "A", 1, null, null), (2, "EJ012", "BEATRIZ A", "A", 1, null, null), (3, "EJ021", "CARLA B", "B", 2, "OP5", "UNIR HOMBROS"),
        (4, "EJ022", "DORA B", "B", 2, null, null), (5, "EJ031", "ELSA C", "C", 3, null, null),
    };
    public List<ArmadoPuesto> Puestos() => new()
    {
        new() { NUM_PUESTO = 1, DES_PUESTO = "PUESTO 01", NUM_OPERARIOS = 2, COD_CATEGORIA_OCUPACIONAL = "A" },
        new() { NUM_PUESTO = 2, DES_PUESTO = "PUESTO 02", NUM_OPERARIOS = 1, COD_CATEGORIA_OCUPACIONAL = "B" },
    };
    public List<ArmadoCobertura> Cobertura() => new()
    {
        new() { COD_CATEGORIA_OCUPACIONAL = "A", DES_CATEGORIA_OCUPACIONAL = "ESPECIALIZADO", NUM_ORDEN = 1, CAN_REQUERIDOS = 2, CAN_DISPONIBLES = 2, CAN_ASIGNADOS = Asignado.Count },
        new() { COD_CATEGORIA_OCUPACIONAL = "B", DES_CATEGORIA_OCUPACIONAL = "INTERMEDIO", NUM_ORDEN = 2, CAN_REQUERIDOS = 1, CAN_DISPONIBLES = SinB ? 0 : 2, CAN_FALTANTES = SinB ? 1 : 0 },
    };
    public bool SinB;

    // migracion
    public List<MigracionGrupo> Grupos = new()
    {
        new() { COD_PROCESO = "CORTE", DES_PROCESO = "CORTE", COD_SUB_PROCESO = "CORTE", DES_SUB_PROCESO = "CORTE", COD_TIPO_OPERACION_PRODUCCION = "CORTT", DES_TIPO_OPERACION_PRODUCCION = "CORTE DE PIEZAS", CAN_FILAS = 26, CAN_FILAS_BLOQUEADAS = 6, CAN_RUTAS = 32, COD_UNIDAD_MEDIDA_TIEMPO = "MIN", VAL_LEAD_TIME_MIN = 48, VAL_LEAD_TIME_MAX = 48, CAN_COINCIDENCIAS = 1, COD_OPERACION_SUGERIDA = "OP0002", DES_OPERACION_SUGERIDA = "CORTAR PIEZAS", DES_OPERACION_NUEVA = "CORTE DE PIEZAS - CORTE" },
        new() { COD_PROCESO = "TJDRA", DES_PROCESO = "TEJEDURIA", COD_SUB_PROCESO = "MNTJE", DES_SUB_PROCESO = "MONTAJE", COD_TIPO_OPERACION_PRODUCCION = "MNTAG", DES_TIPO_OPERACION_PRODUCCION = "MONTAJE DE AGUJAS", CAN_FILAS = 30, CAN_FILAS_BLOQUEADAS = 6, CAN_RUTAS = 36, COD_UNIDAD_MEDIDA_TIEMPO = "MIN", VAL_LEAD_TIME_MIN = 108, VAL_LEAD_TIME_MAX = 144, CAN_COINCIDENCIAS = 0, DES_OPERACION_NUEVA = "MONTAJE DE AGUJAS - MONTAJE" },
        new() { COD_PROCESO = "TJDRA", DES_PROCESO = "TEJEDURIA", COD_SUB_PROCESO = "REVIS", DES_SUB_PROCESO = "REVISADO", COD_TIPO_OPERACION_PRODUCCION = "REVIC", DES_TIPO_OPERACION_PRODUCCION = "REVISADO", CAN_FILAS = 0, CAN_FILAS_BLOQUEADAS = 4, CAN_RUTAS = 4, COD_UNIDAD_MEDIDA_TIEMPO = "MIN", CAN_COINCIDENCIAS = 0, DES_OPERACION_NUEVA = "REVISADO" },
    };
    public MigracionResumen Resumen = new() { CAN_PENDIENTES = 56, CAN_BLOQUEADAS = 16, CAN_RUTAS_PENDIENTES = 40, CAN_MIGRADAS = 0, CAN_OPERACIONES_CREADAS = 0 };

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage r, CancellationToken ct)
    {
        var u = r.RequestUri!.PathAndQuery;
        JsonElement? body = r.Content is null ? null : JsonDocument.Parse(await r.Content.ReadAsStringAsync(ct)).RootElement.Clone();
        Calls.Add((r.Method.Method, u, body));
        HttpResponseMessage Ok(object? data) => new(HttpStatusCode.OK) { Content = JsonContent.Create(new { Success = true, Data = data }) };
        HttpResponseMessage Mal(string msg) => new(HttpStatusCode.BadRequest) { Content = JsonContent.Create(new { Success = false, Message = msg }) };
        JsonElement P(JsonElement e, string n) => e.EnumerateObject().First(x => string.Equals(x.Name, n, StringComparison.OrdinalIgnoreCase)).Value;

        if (r.Method == HttpMethod.Get)
        {
            if (u.EndsWith("/AR1/puestos")) return Ok(Puestos());
            if (u.EndsWith("/AR1/personal"))
                return Ok(Asignado.Select(a => { var g = Gente.First(x => x.Id == a.Per); var pu = Puestos().First(p => p.NUM_PUESTO == a.Pst);
                    return new ArmadoPersonaAsignada { NUM_PUESTO = a.Pst, COD_ENTIDAD_PERSONAL = g.Id, COD_PERSONAL = g.Cod, DES_PERSONAL = g.Des, COD_CATEGORIA_OCUPACIONAL = g.Cat, COD_CATEGORIA_PUESTO = pu.COD_CATEGORIA_OCUPACIONAL,
                        FLG_CATEGORIA_SUPERIOR = g.Ord < (pu.COD_CATEGORIA_OCUPACIONAL == "A" ? 1 : 2) }; }).ToList());
            if (u.EndsWith("/AR1/personal-disponible"))
                return Ok(Gente.Select(g => new ArmadoPersonaDisponible { COD_ENTIDAD_PERSONAL = g.Id, COD_PERSONAL = g.Cod, DES_PERSONAL = g.Des, COD_CATEGORIA_OCUPACIONAL = g.Cat, NUM_ORDEN_CATEGORIA = g.Ord, COD_OPERACION = g.Op, DES_OPERACION = g.DesOp,
                    NUM_PUESTO = Asignado.Any(a => a.Per == g.Id) ? Asignado.First(a => a.Per == g.Id).Pst : null }).ToList());
            if (u.EndsWith("/AR1/cobertura")) return Ok(Cobertura());
            if (u.EndsWith("/AR1/comparar"))
                return Ok(new ArmadoComparacionResultado
                {
                    Disposiciones = new()
                    {
                        new() { ORIGEN = "ACTUAL", COD_TIPO_DISTRIBUCION = "LIN", DES_TIPO_DISTRIBUCION = "LINEAL", COD_REGLA_CICLO = "MAX_PUESTO", CAN_PUESTOS = 7, CAN_OPERARIOS = 7, VAL_CICLO = 1.21m, VAL_CAPACIDAD_TURNO = 337.19m, TAS_EFICIENCIA_BALANCEO = 73.7m, FLG_CUMPLE_META = true },
                        new() { ORIGEN = "SUGERIDO", COD_TIPO_DISTRIBUCION = "EFI", DES_TIPO_DISTRIBUCION = "ESTACION FIJA", COD_REGLA_CICLO = "SUMA_OPERARIOS", CAN_PUESTOS = 1, CAN_OPERARIOS = 5, VAL_CICLO = 1.2484m, VAL_CAPACIDAD_TURNO = 326.82m, TAS_EFICIENCIA_BALANCEO = 100m, FLG_CUMPLE_META = false, CAN_FALTANTES = 3 },
                        new() { ORIGEN = "ACTUAL", COD_TIPO_DISTRIBUCION = "MOD", DES_TIPO_DISTRIBUCION = "MODULAR", COD_REGLA_CICLO = "SUMA_OPERARIOS", CAN_PUESTOS = 2, CAN_OPERARIOS = 4, VAL_CICLO = 1.5m, VAL_CAPACIDAD_TURNO = 270m, TAS_EFICIENCIA_BALANCEO = 90m, FLG_CUMPLE_META = false },
                    },
                    Categorias = new()
                    {
                        new() { ORIGEN = "ACTUAL", COD_TIPO_DISTRIBUCION = "LIN", COD_CATEGORIA_OCUPACIONAL = "A", NUM_ORDEN = 1, CAN_REQUERIDOS = 2, CAN_DISPONIBLES = 5 },
                        new() { ORIGEN = "ACTUAL", COD_TIPO_DISTRIBUCION = "LIN", COD_CATEGORIA_OCUPACIONAL = "B", NUM_ORDEN = 2, CAN_REQUERIDOS = 3, CAN_DISPONIBLES = 8 },
                        new() { ORIGEN = "ACTUAL", COD_TIPO_DISTRIBUCION = "MOD", COD_CATEGORIA_OCUPACIONAL = "C", NUM_ORDEN = 3, CAN_REQUERIDOS = 4, CAN_DISPONIBLES = 1 },
                        new() { ORIGEN = "SUGERIDO", COD_TIPO_DISTRIBUCION = "EFI", COD_CATEGORIA_OCUPACIONAL = "A", NUM_ORDEN = 1, CAN_REQUERIDOS = 5, CAN_DISPONIBLES = 2, CAN_FALTANTES = 3 },
                        new() { ORIGEN = "SUGERIDO", COD_TIPO_DISTRIBUCION = "EFI", COD_CATEGORIA_OCUPACIONAL = "B", NUM_ORDEN = 2, CAN_REQUERIDOS = 0, CAN_DISPONIBLES = 8 },
                    },
                });
            if (u.EndsWith("/lineas-armados/AR1"))
                return Ok(new Armado { COD_ARMADO = "AR1", DES_ARMADO = "POLO", COD_RUTA_PRODUCCION = "RT1", COD_PROCESO = "COSTU", COD_TIPO_DISTRIBUCION = "LIN", DES_TIPO_DISTRIBUCION = "LINEAL", COD_REGLA_CICLO = "MAX_PUESTO", DES_PROCESO = "COSTURA", VAL_TAKT = 1.36m, CAN_PUESTOS = 2, CAN_OPERARIOS = 3 });
            if (u.EndsWith("/AR1/hojas")) return Ok(new List<ArmadoHoja>());
            if (u.EndsWith("/AR1/avisos")) return Ok(new List<ArmadoAviso>());
            if (u.EndsWith("/AR1/operarios-por-categoria")) return Ok(new List<ArmadoOperariosCategoria>());
            if (u.Contains("/operaciones-migracion/resumen")) return Ok(Resumen);
            if (u.Contains("/operaciones-migracion/propuesta")) return Ok(Grupos);
            return Ok(new object[0]);
        }
        if (FallaEscritura) return Mal(MensajeFalla);
        if (r.Method == HttpMethod.Put && u.EndsWith("/AR1/personal"))
        {
            Asignado = P(body!.Value, "Puestos").EnumerateArray().SelectMany(p => P(p, "PERSONAS").EnumerateArray().Select(x => (P(p, "NUM_PUESTO").GetInt32(), x.GetInt32()))).ToList();
            return Ok(true);
        }
        if (r.Method == HttpMethod.Post && u.EndsWith("/AR1/personal/sugerir")) { Asignado = new() { (1, 1), (1, 2), (2, 3) }; return Ok(new ArmadoPersonalSugerido { CAN_ASIGNADAS = 3, CAN_VACANTES = 0 }); }
        if (r.Method == HttpMethod.Post && u.EndsWith("/operaciones-migracion"))
        {
            Resumen.CAN_MIGRADAS = 56; Resumen.CAN_PENDIENTES = 0; Resumen.CAN_OPERACIONES_CREADAS = 1;
            return Ok(new MigracionResultado { CAN_FILAS_MIGRADAS = 56, CAN_FILAS_BLOQUEADAS = 12, CAN_OPERACIONES_CREADAS = 1 });
        }
        if (r.Method == HttpMethod.Delete && u.Contains("/operaciones-migracion")) { Resumen.CAN_MIGRADAS = 0; Resumen.CAN_PENDIENTES = 56; return Ok(new MigracionDeshacerResultado { CAN_FILAS_DESHECHAS = 56, CAN_FILAS_OMITIDAS = 1, CAN_OPERACIONES_BORRADAS = 1 }); }
        return Ok(true);
    }
}

public class OperacionesO7O8Tests : TestContext
{
    readonly FakeO7O8 _f = new();
    IRenderedComponent<MudDialogProvider>? _prov;

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

    async Task Abre<T>(IRenderedComponent<MudDialogProvider> p, DialogParameters pr, string esperar) where T : Microsoft.AspNetCore.Components.IComponent
    {
        var ds = Services.GetRequiredService<IDialogService>();
        await p.InvokeAsync(async () => await ds.ShowAsync<T>("t", pr, new DialogOptions { MaxWidth = MaxWidth.ExtraLarge }));
        p.WaitForAssertion(() => Assert.Contains(esperar, p.Markup));
    }

    static JsonElement Prop(JsonElement e, string n) => e.EnumerateObject().First(x => string.Equals(x.Name, n, StringComparison.OrdinalIgnoreCase)).Value;
    (string M, string U, JsonElement? B) Ultima(string metodo, string fin) => _f.Calls.Last(c => c.M == metodo && c.U.Contains(fin));
    int Cuantas(string metodo, string fin) => _f.Calls.Count(c => c.M == metodo && c.U.Contains(fin));
    IRenderedComponent<MudSelect<T>> Sel<T>(IRenderedComponent<MudDialogProvider> p, string label) => p.FindComponents<MudSelect<T>>().First(c => c.Instance.Label == label);
    Task<IRenderedComponent<MudDialogProvider>> Personal() => Task.Run(async () => { var p = Prov(); await Abre<ArmadoPersonalDialog>(p, new DialogParameters { ["CodArmado"] = "AR1" }, "PUESTO 02"); return p; });

    // ======================= personal del armado (O7) =======================
    [Fact]
    public async Task Personal_MuestraLosPuestosConSuCupoYLaCobertura()
    {
        var p = await Personal();
        Assert.Equal("1 de 2", p.Find("[data-puesto='1'] [data-dato='cupo']").TextContent.Trim());
        Assert.Equal("0 de 1", p.Find("[data-puesto='2'] [data-dato='cupo']").TextContent.Trim());
        Assert.Contains("ANA A", p.Find("[data-puesto='1']").TextContent);
        Assert.Contains("pide A", p.Find("[data-puesto='1']").TextContent); Assert.Contains("pide B", p.Find("[data-puesto='2']").TextContent);
        Assert.Contains("Sin personal asignado", p.Find("[data-puesto='2']").TextContent);
        var a = p.Find("[data-cat='A']").QuerySelectorAll("td"); Assert.Equal("2", a[1].TextContent.Trim()); Assert.Equal("2", a[2].TextContent.Trim()); Assert.Equal("1", a[3].TextContent.Trim());   // pide 2, hay 2, asignado 1
        Assert.NotEmpty(p.FindAll("[data-aviso='cubierto']"));
        Assert.Contains("1 vacantes de 3 lugares".Replace("1 vacantes", "2 vacantes"), p.Markup);
    }

    [Fact]
    public async Task Personal_SinPersonalSuficiente_AvisaLosFaltantes()
    {
        _f.SinB = true;
        var p = await Personal();
        Assert.Contains("Faltan 1 operarios", p.Find("[data-aviso='faltan']").TextContent);
        Assert.Contains("ap-falta", p.Find("[data-cat='B']").ClassName ?? "");
    }

    [Fact]
    public async Task Personal_LasOpcionesSonLasPersonasLibresDeLaCategoriaQuePideOMasAlta_LaMasCercanaPrimero()
    {
        var p = await Personal();
        string[] Ops(int puesto) => p.Find($"[data-puesto='{puesto}']") is var _ ? p.FindComponents<MudSelect<int>>().ElementAt(puesto == 1 ? 0 : 1).FindComponents<MudSelectItem<int>>().Select(i => i.Markup).ToArray() : new string[0];
        // puesto 1 (pide A): solo A libres; Ana ya esta puesta -> Beatriz
        var ids1 = p.FindComponents<MudSelect<int>>().First().FindComponents<MudSelectItem<int>>().Select(i => i.Instance.Value).ToArray();
        Assert.Equal(new[] { 2 }, ids1);
        // puesto 2 (pide B): B primero (la mas cercana), luego A; nunca C
        var ids2 = p.FindComponents<MudSelect<int>>().Last().FindComponents<MudSelectItem<int>>().Select(i => i.Instance.Value).ToArray();
        Assert.Equal(new[] { 3, 4, 2 }, ids2);
    }

    [Fact]
    public async Task Personal_AgregarYQuitar_RecalculaElCupo_YGuarda()
    {
        var p = await Personal();
        await p.InvokeAsync(() => p.FindComponents<MudSelect<int>>().Last().Instance.ValueChanged.InvokeAsync(2));      // Beatriz (A) al puesto 2 (pide B): categoria superior
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-chip='sin-guardar']")));
        Assert.Equal("1 de 1", p.Find("[data-puesto='2'] [data-dato='cupo']").TextContent.Trim());
        Assert.Contains("categoría superior", p.Find("[data-puesto='2']").TextContent);
        Assert.Empty(p.FindAll("[data-puesto='2'] [data-campo='agregar']"));                                              // puesto lleno: ya no se agrega
        Assert.Equal(0, Cuantas("PUT", "/AR1/personal"));
        await p.InvokeAsync(() => p.Find("[data-accion='guardar']").Click());
        p.WaitForAssertion(() => Assert.Empty(p.FindAll("[data-chip='sin-guardar']")));
        var put = Ultima("PUT", "/AR1/personal").B!.Value;
        var ps = Prop(put, "Puestos").EnumerateArray().ToList();
        Assert.Equal(new[] { 1 }, Prop(ps[0], "PERSONAS").EnumerateArray().Select(x => x.GetInt32()).ToArray());
        Assert.Equal(new[] { 2 }, Prop(ps[1], "PERSONAS").EnumerateArray().Select(x => x.GetInt32()).ToArray());
        Assert.Equal("AR1", Prop(put, "COD_ARMADO").GetString());
        // quitar a Ana del puesto 1
        await p.InvokeAsync(() => p.Find("[data-puesto='1'] [data-persona='1'] [data-accion='quitar-persona']").Click());
        p.WaitForAssertion(() => Assert.Equal("0 de 2", p.Find("[data-puesto='1'] [data-dato='cupo']").TextContent.Trim()));
    }

    [Fact]
    public async Task Personal_DescartarCambios_YErrorDeLaApi()
    {
        var p = await Personal();
        await p.InvokeAsync(() => p.Find("[data-puesto='1'] [data-persona='1'] [data-accion='quitar-persona']").Click());
        _f.FallaEscritura = true;
        await p.InvokeAsync(() => p.Find("[data-accion='guardar']").Click());
        p.WaitForAssertion(() => Assert.Contains("dos puestos", p.Find("[data-error]").TextContent));
        Assert.NotEmpty(p.FindAll("[data-chip='sin-guardar']"));                  // no se pierde lo editado
        _f.FallaEscritura = false;
        await p.InvokeAsync(() => p.Find("[data-accion='descartar']").Click());
        p.WaitForAssertion(() => Assert.Empty(p.FindAll("[data-chip='sin-guardar']")));
        Assert.Equal("1 de 2", p.Find("[data-puesto='1'] [data-dato='cupo']").TextContent.Trim());
    }

    [Fact]
    public async Task Personal_Sugerir_PideConfirmacion()
    {
        var p = await Personal();
        await p.InvokeAsync(() => p.Find("[data-accion='sugerir-personal']").Click());
        p.WaitForAssertion(() => Assert.Contains("REEMPLAZA", p.Markup));
        await p.InvokeAsync(() => p.FindAll("button").First(b => b.TextContent.Trim() == "Cancelar").Click());
        Assert.Equal(0, Cuantas("POST", "/personal/sugerir"));
        await p.InvokeAsync(() => p.Find("[data-accion='sugerir-personal']").Click());
        p.WaitForAssertion(() => Assert.Contains("REEMPLAZA", p.Markup));
        await p.InvokeAsync(() => p.FindAll("button").First(b => b.TextContent.Trim() == "Sugerir" && b.GetAttribute("data-accion") is null).Click());
        p.WaitForAssertion(() => Assert.Equal(1, Cuantas("POST", "/personal/sugerir")));
        p.WaitForAssertion(() => Assert.Equal("2 de 2", p.Find("[data-puesto='1'] [data-dato='cupo']").TextContent.Trim()));
    }

    // ======================= comparar disposiciones (O7) =======================
    [Fact]
    public async Task Comparar_MuestraCadaDisposicionConSuBalanceYSusFaltantes()
    {
        var p = Prov(); await Abre<ArmadoComparaDialog>(p, new DialogParameters { ["CodArmado"] = "AR1" }, "ESTACION FIJA");
        var act = p.Find("[data-disposicion='LIN'][data-origen='ACTUAL']");
        Assert.Contains("Guardado", act.TextContent); Assert.Contains("1.210", act.TextContent); Assert.Contains("337", act.TextContent); Assert.Contains("73.7 %", act.TextContent);
        Assert.Contains("cp-actual", act.ClassName ?? "");
        Assert.Equal("si", act.QuerySelector("[data-meta]")!.GetAttribute("data-meta"));
        Assert.Contains("A 5/2 · B 8/3", act.TextContent);                                  // hay / pide
        Assert.DoesNotContain("C 1/4", act.TextContent);                                    // la categoria de otra disposicion no se mezcla
        Assert.Contains("C 1/4", p.Find("[data-disposicion='MOD'][data-origen='ACTUAL']").TextContent);
        var efi = p.Find("[data-disposicion='EFI'][data-origen='SUGERIDO']");
        Assert.Contains("Sugerido", efi.TextContent); Assert.Equal("no", efi.QuerySelector("[data-meta]")!.GetAttribute("data-meta"));
        Assert.Equal("3", efi.QuerySelector("[data-faltan]")!.GetAttribute("data-faltan"));
        Assert.Contains("A 2/5", efi.TextContent); Assert.DoesNotContain("B 8/0", efi.TextContent);   // no lista las categorias que no pide
        Assert.Contains("se reparten el trabajo", efi.TextContent);
    }

    [Fact]
    public async Task Comparar_ErrorDeLaApi_SeMuestra()
    {
        var p = Prov(); await Abre<ArmadoComparaDialog>(p, new DialogParameters { ["CodArmado"] = "NOEXISTE" }, "t");
        p.WaitForAssertion(() => Assert.DoesNotContain("progressbar", p.Markup));
    }

    [Fact]
    public async Task Ficha_TieneLosBotonesDePersonalYComparar()
    {
        var p = Prov(); await Abre<ArmadoFichaDialog>(p, new DialogParameters { ["Armado"] = new Armado { COD_ARMADO = "AR1", DES_ARMADO = "POLO" } }, "Personal...");
        Assert.Contains("Comparar disposiciones", p.Find("[data-accion='comparar']").TextContent);
        Assert.False(p.Find("[data-accion='personal']").HasAttribute("disabled"));
        await p.InvokeAsync(() => p.Find("[data-accion='agregar-puesto']").Click());                       // reparto sin guardar: el personal espera
        p.WaitForAssertion(() => Assert.True(p.Find("[data-accion='personal']").HasAttribute("disabled")));
    }

    // ======================= migracion (O8) =======================
    IRenderedComponent<MigracionOperaciones> Pagina()
    {
        _prov = Prov();
        var pg = RenderComponent<MigracionOperaciones>();
        pg.WaitForAssertion(() => Assert.Contains("CORTE DE PIEZAS", pg.Markup));
        return pg;
    }

    [Fact]
    public void Migracion_MuestraElResumenYLosGruposConLoQueSePropone()
    {
        var pg = Pagina();
        Assert.Equal("56", pg.Find("[data-kpi='pendientes'] b").TextContent.Trim()); Assert.Equal("16", pg.Find("[data-kpi='bloqueadas'] b").TextContent.Trim());
        Assert.Equal("0", pg.Find("[data-kpi='migradas'] b").TextContent.Trim());
        Assert.Equal(3, pg.FindAll("tr[data-grupo]").Count);
        Assert.Contains("108.000 a 144.000 MIN", pg.Find("tr[data-grupo='TJDRA|MNTJE|MNTAG']").TextContent);      // lead time en rango
        var cortt = pg.Find("tr[data-grupo='CORTE|CORTE|CORTT']");
        Assert.Contains("26", cortt.TextContent); Assert.Contains("48.000 MIN", cortt.TextContent);
        var acciones = pg.FindComponents<MudSelect<string>>().Where(c => c.Instance.Label is null).ToList();
        Assert.Equal("EXISTENTE", pg.FindComponents<MudSelect<string>>().First(c => c.Markup.Contains("data-campo=\"accion\"")).Instance.Value);          // con coincidencia: usar la sugerida
        Assert.Contains("MONTAJE DE AGUJAS - MONTAJE", pg.Markup);                                       // nombre sugerido de la nueva
        Assert.True(pg.Find("tr[data-grupo='TJDRA|REVIS|REVIC']").ClassName!.Contains("mg-bloq"));        // solo filas bloqueadas: no se puede migrar
    }

    [Fact]
    public void Migracion_MarcarSeleccionaSoloLoMigrable_YElBotonCuenta()
    {
        var pg = Pagina();
        Assert.True(pg.Find("[data-accion='migrar']").HasAttribute("disabled"));
        pg.Find("[data-accion='marcar-todos']").Click();
        pg.WaitForAssertion(() => Assert.Contains("Migrar 2 marcados", pg.Find("[data-accion='migrar']").TextContent));      // el grupo bloqueado no se marca
        Assert.Equal(new[] { true, true, false }, pg.FindComponents<MudCheckBox<bool>>().Select(c => c.Instance.Value).Take(3).ToArray());
        pg.Find("[data-accion='quitar-marcas']").Click();
        pg.WaitForAssertion(() => Assert.Contains("Migrar 0 marcados", pg.Find("[data-accion='migrar']").TextContent));
        pg.Find("[data-accion='marcar-coincidencia']").Click();
        pg.WaitForAssertion(() => Assert.Contains("Migrar 1 marcados", pg.Find("[data-accion='migrar']").TextContent));
    }

    [Fact]
    public async Task Migracion_Migrar_PideConfirmacion_EnviaLasDecisiones_YMuestraElResultado()
    {
        var pg = Pagina();
        pg.Find("[data-accion='marcar-todos']").Click();
        pg.Find("input[data-campo='ruta']").Change("RT0000026");
        await pg.InvokeAsync(() => pg.Find("[data-accion='migrar']").Click());
        _prov!.WaitForAssertion(() => Assert.Contains("56 filas", _prov.Find(".mud-dialog").TextContent));
        Assert.Equal(0, Cuantas("POST", "/operaciones-migracion"));
        await pg.InvokeAsync(() => _prov!.FindAll("button").First(b => b.TextContent.Trim() == "Cancelar").Click());
        Assert.Equal(0, Cuantas("POST", "/operaciones-migracion"));
        await pg.InvokeAsync(() => pg.Find("[data-accion='migrar']").Click());
        _prov!.WaitForAssertion(() => Assert.Contains("56 filas", _prov.Find(".mud-dialog").TextContent));
        await pg.InvokeAsync(() => _prov!.FindAll("button").First(b => b.TextContent.Trim() == "Migrar" && b.GetAttribute("data-accion") is null).Click());
        pg.WaitForAssertion(() => Assert.Equal(1, Cuantas("POST", "/operaciones-migracion")));
        var b = Ultima("POST", "/operaciones-migracion").B!.Value;
        Assert.Equal("RT0000026", Prop(b, "COD_RUTA_PRODUCCION").GetString());
        var gs = Prop(b, "Grupos").EnumerateArray().ToList();
        Assert.Equal(2, gs.Count);
        var c = gs.First(x => Prop(x, "COD_TIPO_OPERACION_PRODUCCION").GetString() == "CORTT");
        Assert.Equal("EXISTENTE", Prop(c, "ACCION").GetString()); Assert.Equal("OP0002", Prop(c, "COD_OPERACION").GetString());
        var m = gs.First(x => Prop(x, "COD_TIPO_OPERACION_PRODUCCION").GetString() == "MNTAG");
        Assert.Equal("NUEVA", Prop(m, "ACCION").GetString()); Assert.Equal("MONTAJE DE AGUJAS - MONTAJE", Prop(m, "DES_OPERACION").GetString()); Assert.Equal("MNTJE", Prop(m, "COD_SUB_PROCESO").GetString());
        pg.WaitForAssertion(() => Assert.Contains("Migradas 56 filas; 1 operaciones creadas; 12 filas saltadas", pg.Find("[data-aviso='resultado']").TextContent));
        Assert.Equal("56", pg.Find("[data-kpi='migradas'] b").TextContent.Trim());
        Assert.Contains("Migrar 2 marcados", pg.Find("[data-accion='migrar']").TextContent);          // al recargar siguen las marcas
    }

    [Fact]
    public async Task Migracion_ElErrorDeLaApiSeMuestra()
    {
        var pg = Pagina();
        pg.Find("[data-accion='marcar-todos']").Click();
        _f.FallaEscritura = true; _f.MensajeFalla = "Una operacion del catalogo indicada es compuesta.";
        await pg.InvokeAsync(() => pg.Find("[data-accion='migrar']").Click());
        _prov!.WaitForAssertion(() => Assert.Contains("filas de rutas", _prov.Find(".mud-dialog").TextContent));
        await pg.InvokeAsync(() => _prov!.FindAll("button").First(b => b.TextContent.Trim() == "Migrar" && b.GetAttribute("data-accion") is null).Click());
        pg.WaitForAssertion(() => Assert.Contains("compuesta", pg.Find("[data-error]").TextContent));
    }

    [Fact]
    public async Task Migracion_Deshacer_PideConfirmacionYLoHace()
    {
        _f.Resumen.CAN_MIGRADAS = 56;
        var pg = Pagina();
        Assert.False(pg.Find("[data-accion='deshacer']").HasAttribute("disabled"));
        await pg.InvokeAsync(() => pg.Find("[data-accion='deshacer']").Click());
        _prov!.WaitForAssertion(() => Assert.Contains("se borran las operaciones creadas", _prov.Find(".mud-dialog").TextContent));
        Assert.Equal(0, Cuantas("DELETE", "/operaciones-migracion"));
        await pg.InvokeAsync(() => _prov!.FindAll("button").First(b => b.TextContent.Trim() == "Cancelar").Click());
        Assert.Equal(0, Cuantas("DELETE", "/operaciones-migracion"));
        await pg.InvokeAsync(() => pg.Find("[data-accion='deshacer']").Click());
        _prov!.WaitForAssertion(() => Assert.Contains("se borran las operaciones creadas", _prov.Find(".mud-dialog").TextContent));
        await pg.InvokeAsync(() => _prov!.FindAll("button").First(b => b.TextContent.Trim() == "Deshacer" && b.GetAttribute("data-accion") is null).Click());
        pg.WaitForAssertion(() => Assert.Equal(1, Cuantas("DELETE", "/operaciones-migracion")));
        pg.WaitForAssertion(() => Assert.Contains("Deshechas 56 filas; 1 omitidas", pg.Find("[data-aviso='resultado']").TextContent));
    }

    [Fact]
    public void Migracion_SinNadaMigrado_NoSePuedeDeshacer_YElFiltroDeProcesoFunciona()
    {
        var pg = Pagina();
        Assert.True(pg.Find("[data-accion='deshacer']").HasAttribute("disabled"));
        var sel = pg.FindComponents<MudSelect<string>>().First(c => c.Instance.Label == "Proceso");
        pg.InvokeAsync(() => sel.Instance.ValueChanged.InvokeAsync("TJDRA"));
        pg.WaitForAssertion(() => Assert.Equal(2, pg.FindAll("tr[data-grupo]").Count));
        Assert.DoesNotContain("CORTE DE PIEZAS", pg.Find("[data-tabla='grupos']").TextContent);
    }

    // El menu abre cada opcion con CodAplicacion y CodOpcionAplicacion (DynamicComponent): sin esos parametros la pantalla falla al dibujarse.
    [Theory]
    [InlineData(typeof(KONSolutions.Web.Pages.Ingenieria.OperacionesCatalogos))]
    [InlineData(typeof(KONSolutions.Web.Pages.Ingenieria.Operaciones))]
    [InlineData(typeof(KONSolutions.Web.Pages.Ingenieria.RutasTiempos))]
    [InlineData(typeof(KONSolutions.Web.Pages.Ingenieria.PersonalOperacion))]
    [InlineData(typeof(KONSolutions.Web.Pages.Ingenieria.ArmadoLineas))]
    [InlineData(typeof(KONSolutions.Web.Pages.Ingenieria.MigracionOperaciones))]
    public void LasPantallasDeOperaciones_AceptanLosParametrosDelMenu(Type pantalla)
    {
        foreach (var nombre in new[] { "CodAplicacion", "CodOpcionAplicacion" })
        {
            var prop = pantalla.GetProperty(nombre);
            Assert.NotNull(prop);
            Assert.NotNull(prop!.GetCustomAttributes(typeof(Microsoft.AspNetCore.Components.ParameterAttribute), false).FirstOrDefault());
        }
    }

    [Fact]
    public void Migracion_LaGrillaTieneCabecerasConOrdenYFiltro_AltoAutomaticoYTirador()
    {
        var pg = Pagina();
        Assert.NotNull(pg.Find("[data-resizable-grid='migracion-operaciones']").QuerySelector(":scope > .grid-resize-handle"));
        Assert.Equal(8, pg.FindAll("th .eo-titulo").Count);
        Assert.Equal(8, pg.FindAll("th .eo-flecha").Count);                         // y su flecha de orden
        // ordenar por "Filas": de menor a mayor y luego al reves (por el numero, no por el texto: 26 < 30)
        string[] Grupos() => pg.FindAll("tr[data-grupo]").Select(tr => tr.GetAttribute("data-grupo")!).ToArray();
        pg.FindAll("th .eo-titulo").First(x => x.TextContent.Trim() == "Filas").Click();
        pg.WaitForAssertion(() => Assert.Equal(new[] { "TJDRA|REVIS|REVIC", "CORTE|CORTE|CORTT", "TJDRA|MNTJE|MNTAG" }, Grupos()));
        pg.FindAll("th .eo-titulo").First(x => x.TextContent.Trim() == "Filas").Click();
        pg.WaitForAssertion(() => Assert.Equal(new[] { "TJDRA|MNTJE|MNTAG", "CORTE|CORTE|CORTT", "TJDRA|REVIS|REVIC" }, Grupos()));
    }
}
