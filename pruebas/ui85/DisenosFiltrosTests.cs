using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AngleSharp.Dom;
using Blazored.LocalStorage;
using Bunit;
using KONSolutions.Web.Pages.Shared;
using KONSolutions.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using Xunit;
using DisenoRow = KONSolutions.Shared.Models.Ingenieria.Disenos;
using KONSolutions.Shared.Common;

// Pantalla de Disenos: filtros del panel (grupo > familia > sub familia, tipo, entidad, personal responsable, temporada / coleccion, estado),
// su boton de ocultar / mostrar y la columna "Grupo / Familia / Sub-familia de producto". La API se simula con FakeDisenos.
public class FakeDisenos : HttpMessageHandler
{
    public List<(string M, string U)> Calls = new();
    public List<DisenoRow> Todos = Datos();
    public bool SinDescripcionesGrupoFamilia;        // el SP todavia no es el del script 114: DES_GRUPO_PRODUCTO / DES_FAMILIA_PRODUCTO vienen vacios

    static DisenoRow D(string cod, string des, string g, string dg, string f, string df, string s, string ds, string tipo, string dtipo,
                       decimal? ent, string? nent, string? resp, string? temp, string? col, string estado) => new()
    {
        COD_DISENO = cod, DES_DISENO = des,
        COD_GRUPO_PRODUCTO = g, DES_GRUPO_PRODUCTO = dg, COD_FAMILIA_PRODUCTO = f, DES_FAMILIA_PRODUCTO = df, COD_SUB_FAMILIA_PRODUCTO = s, DES_SUB_FAMILIA_PRODUCTO = ds,
        COD_TIPO_DISENO = tipo, DES_TIPO_DISENO = dtipo, COD_ENTIDAD = ent, DES_NOMBRE_COMPLETO = nent, DES_ENTIDAD_ASIGNADA = resp,
        DES_TEMPORADA = temp, DES_COLECCION = col, DES_ESTADO = estado, COD_TIPO_ESTADO = 162, COD_ESTADO = 0,
    };

    // 10 disenos: tres grupos con varias familias; la familia 00001 "OTROS" existe en dos grupos distintos; dos entidades con el mismo nombre y distinto codigo.
    public static List<DisenoRow> Datos() => new()
    {
        D("D1", "POLO BASICO MC",  "00001", "PRENDA DE VESTIR", "00001", "POLOS",           "00001", "MANGA CORTA",   "PRD", "PRODUCTO", 100, "CLIENTE ALFA SAC", "ANA PEREZ",  "VERANO 2026",   "BASICOS", "ACTIVO"),
        D("D2", "POLO BASICO ML",  "00001", "PRENDA DE VESTIR", "00001", "POLOS",           "00002", "MANGA LARGA",   "PRD", "PRODUCTO", 100, "CLIENTE ALFA SAC", "ANA PEREZ",  "VERANO 2026",   "BASICOS", "ACTIVO"),
        D("D3", "CAMISA FORMAL",   "00001", "PRENDA DE VESTIR", "00002", "CAMISAS",         "00001", "FORMAL",        "MUE", "MUESTRA",  200, "CLIENTE BETA SAC", "LUIS GOMEZ", "INVIERNO 2026", "PREMIUM", "ACTIVO"),
        D("D4", "JERSEY SIMPLE",   "00003", "TELA CRUDA",       "00001", "TEJIDO DE PUNTO", "00001", "JERSEY SIMPLE", "PRD", "PRODUCTO", null, null,               null,         null,            null,      "ACTIVO"),
        D("D5", "PIQUE",           "00003", "TELA CRUDA",       "00001", "TEJIDO DE PUNTO", "00002", "PIQUE",         "MUE", "MUESTRA",  200, "CLIENTE BETA SAC", "LUIS GOMEZ", null,            null,      "BORRADOR"),
        D("D6", "TAFETAN",         "00003", "TELA CRUDA",       "00002", "TEJIDO PLANO",    "00001", "TAFETAN",       "PRD", "PRODUCTO", 100, "CLIENTE ALFA SAC", null,         null,            null,      "ANULADO"),
        D("D7", "MOCASIN",         "00004", "CALZADO",          "00001", "OTROS",           "00001", "VARIOS",        "PRD", "PRODUCTO", 300, "CLIENTE GAMMA",    null,         null,            null,      "ACTIVO"),
        D("D8", "TELA VARIOS",     "00011", "TELA ACABADA",     "00001", "OTROS",           "00001", "VARIOS",        "PRD", "PRODUCTO", 301, "CLIENTE GAMMA",    null,         null,            null,      "ACTIVO"),
        D("D9", "TELA DIVERSOS",   "00011", "TELA ACABADA",     "00001", "OTROS",           "00002", "DIVERSOS",      "PRD", "PRODUCTO", null, null,              null,         null,            null,      "ACTIVO"),
        D("D10", "SANDALIA",       "00004", "CALZADO",          "00002", "SANDALIAS",       "00005", "PLANA",         "PRD", "PRODUCTO", null, null,              null,         null,            null,      "ACTIVO"),
    };

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage r, CancellationToken ct)
    {
        var u = r.RequestUri!.PathAndQuery;
        Calls.Add((r.Method.Method, u));
        HttpResponseMessage Ok(object? data) => new(HttpStatusCode.OK) { Content = JsonContent.Create(new { Success = true, Data = data }) };
        if (r.Method == HttpMethod.Get && u.StartsWith("/api/v1/ingenieria/disenos/search"))
        {
            var s = System.Web.HttpUtility.ParseQueryString(r.RequestUri.Query)["search"] ?? "";
            return Task.FromResult(Ok(Lista().Where(d => d.COD_DISENO.Contains(s, StringComparison.OrdinalIgnoreCase) || d.DES_DISENO.Contains(s, StringComparison.OrdinalIgnoreCase)).ToList()));
        }
        if (r.Method == HttpMethod.Get && u == "/api/v1/ingenieria/disenos") return Task.FromResult(Ok(Lista()));
        return Task.FromResult(r.Method == HttpMethod.Get ? Ok(new object[0]) : Ok(true));
    }

    List<DisenoRow> Lista() => !SinDescripcionesGrupoFamilia ? Todos
        : Todos.Select(d => { var c = JsonSerializer.Deserialize<DisenoRow>(JsonSerializer.Serialize(d))!; c.DES_GRUPO_PRODUCTO = ""; c.DES_FAMILIA_PRODUCTO = ""; return c; }).ToList();
}

// Almacenamiento del navegador en memoria (donde la pantalla recuerda si los filtros estan ocultos).
public class FakeLocalStorage : ILocalStorageService
{
    public Dictionary<string, string> Datos = new();
    public bool Falla;                                   // sin almacenamiento (ventana privada, bloqueado...)
    void Chequea() { if (Falla) throw new InvalidOperationException("sin almacenamiento"); }
    public event EventHandler<ChangingEventArgs>? Changing { add { } remove { } }
    public event EventHandler<ChangedEventArgs>? Changed { add { } remove { } }
    public ValueTask ClearAsync(CancellationToken cancellationToken = default) { Chequea(); Datos.Clear(); return ValueTask.CompletedTask; }
    public ValueTask<T> GetItemAsync<T>(string key, CancellationToken cancellationToken = default) { Chequea(); return ValueTask.FromResult(JsonSerializer.Deserialize<T>(Datos[key])!); }
    public ValueTask<string> GetItemAsStringAsync(string key, CancellationToken cancellationToken = default) { Chequea(); return ValueTask.FromResult(Datos[key]); }
    public ValueTask<string> KeyAsync(int index, CancellationToken cancellationToken = default) { Chequea(); return ValueTask.FromResult(Datos.Keys.ElementAt(index)); }
    public ValueTask<IEnumerable<string>> KeysAsync(CancellationToken cancellationToken = default) { Chequea(); return ValueTask.FromResult<IEnumerable<string>>(Datos.Keys.ToList()); }
    public ValueTask<bool> ContainKeyAsync(string key, CancellationToken cancellationToken = default) { Chequea(); return ValueTask.FromResult(Datos.ContainsKey(key)); }
    public ValueTask<int> LengthAsync(CancellationToken cancellationToken = default) { Chequea(); return ValueTask.FromResult(Datos.Count); }
    public ValueTask RemoveItemAsync(string key, CancellationToken cancellationToken = default) { Chequea(); Datos.Remove(key); return ValueTask.CompletedTask; }
    public ValueTask RemoveItemsAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default) { Chequea(); foreach (var k in keys) Datos.Remove(k); return ValueTask.CompletedTask; }
    public ValueTask SetItemAsync<T>(string key, T data, CancellationToken cancellationToken = default) { Chequea(); Datos[key] = JsonSerializer.Serialize(data); return ValueTask.CompletedTask; }
    public ValueTask SetItemAsStringAsync(string key, string data, CancellationToken cancellationToken = default) { Chequea(); Datos[key] = data; return ValueTask.CompletedTask; }
}

public class DisenosFiltrosTests : TestContext
{
    readonly FakeDisenos _f = new();
    readonly FakeLocalStorage _ls = new();

    IRenderedComponent<KONSolutions.Web.Pages.Ingenieria.Disenos> Pantalla()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton(new HttpClient(_f) { BaseAddress = new Uri("http://x/") });
        Services.AddSingleton<ILocalStorageService>(_ls);
        Services.AddScoped<ErrorLogService>(); Services.AddScoped<ConfigService>(); Services.AddScoped<EtiquetasService>();
        Services.AddSingleton<Microsoft.Extensions.Configuration.IConfiguration>(new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build());
        Services.AddScoped<AccessLogService>(); Services.AddScoped<MenuService>(); Services.AddScoped<ControlCambiosClientService>();
        ComponentFactories.AddStub<AccionesBotones>();        // depende de seguridad/menu: no es lo que se prueba
        ComponentFactories.AddStub<TituloPagina>();
        RenderComponent<MudPopoverProvider>(); RenderComponent<MudSnackbarProvider>(); RenderComponent<MudDialogProvider>();
        var p = RenderComponent<KONSolutions.Web.Pages.Ingenieria.Disenos>();
        p.WaitForAssertion(() => Assert.Contains("registros", p.Markup));
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("tbody tr td")));
        return p;
    }

    // ---- ayudas ----
    static string[] Etiquetas = { "Grupo de producto", "Familia de producto", "Sub-familia de producto", "Tipo de diseño", "Entidad", "Personal responsable", "Temporada / Colección", "Estado" };
    IRenderedComponent<AriaCombo<ComboboxItem>> Combo(IRenderedComponent<KONSolutions.Web.Pages.Ingenieria.Disenos> p, string label) =>
        p.FindComponents<AriaCombo<ComboboxItem>>().First(c => c.Instance.Label == label);
    string[] Textos(IRenderedComponent<KONSolutions.Web.Pages.Ingenieria.Disenos> p, string label) => Combo(p, label).Instance.Items.Select(i => i.Texto).ToArray();
    string[] Ids(IRenderedComponent<KONSolutions.Web.Pages.Ingenieria.Disenos> p, string label) => Combo(p, label).Instance.Items.Select(i => i.Id).ToArray();
    async Task Elige(IRenderedComponent<KONSolutions.Web.Pages.Ingenieria.Disenos> p, string label, string? id) =>
        await p.InvokeAsync(() => Combo(p, label).Instance.ValueChanged.InvokeAsync(id));
    // codigos de los disenos que muestra la grilla (primera celda de cada fila), en el orden en que salen
    string[] Codigos(IRenderedComponent<KONSolutions.Web.Pages.Ingenieria.Disenos> p) =>
        p.FindAll("tbody tr").Select(tr => tr.QuerySelector("td")?.TextContent.Trim() ?? "").Where(t => t.Length > 0 && t != "Sin registros.").ToArray();
    static string[] Cods(params string[] c) => c;
    string Contador(IRenderedComponent<KONSolutions.Web.Pages.Ingenieria.Disenos> p) =>
        System.Text.RegularExpressions.Regex.Match(p.Find(".mono").TextContent, @"\d+ de \d+ registros").Value;
    IRenderedComponent<MudIconButton> BotonFiltros(IRenderedComponent<KONSolutions.Web.Pages.Ingenieria.Disenos> p) =>
        p.FindComponents<MudIconButton>().First(b => b.Markup.Contains("data-accion=\"filtros\""));
    string TooltipFiltros(IRenderedComponent<KONSolutions.Web.Pages.Ingenieria.Disenos> p) =>
        p.FindComponents<MudTooltip>().First(t => t.Markup.Contains("data-accion=\"filtros\"")).Instance.Text;

    // ---------- el panel ----------
    [Fact]
    public void ElPanelEstaVisible_ConLosOchoFiltrosEnOrden()
    {
        var p = Pantalla();
        Assert.NotEmpty(p.FindAll("[data-panel='filtros']"));
        Assert.Equal(Etiquetas, p.FindComponents<AriaCombo<ComboboxItem>>().Select(c => c.Instance.Label).ToArray());
        Assert.Equal("10 de 10 registros", Contador(p));
        Assert.Equal(Cods("D1", "D2", "D3", "D4", "D5", "D6", "D7", "D8", "D9", "D10"), Codigos(p));
    }

    [Fact]
    public void LasOpciones_SalenDeLosDisenosQueHay_Ordenadas_SinRepetir()
    {
        var p = Pantalla();
        Assert.Equal(new[] { "CALZADO", "PRENDA DE VESTIR", "TELA ACABADA", "TELA CRUDA" }, Textos(p, "Grupo de producto"));
        Assert.Equal(new[] { "MUESTRA", "PRODUCTO" }, Textos(p, "Tipo de diseño"));
        Assert.Equal(new[] { "CLIENTE ALFA SAC", "CLIENTE BETA SAC", "CLIENTE GAMMA", "CLIENTE GAMMA" }, Textos(p, "Entidad"));          // dos entidades distintas con el mismo nombre
        Assert.Equal(new[] { "100", "200", "300", "301" }, Ids(p, "Entidad").OrderBy(x => x).ToArray());                                    // se distinguen por codigo
        Assert.Equal(new[] { "ANA PEREZ", "LUIS GOMEZ" }, Textos(p, "Personal responsable"));
        Assert.Equal(new[] { "INVIERNO 2026 / PREMIUM", "VERANO 2026 / BASICOS" }, Textos(p, "Temporada / Colección"));
        Assert.Equal(new[] { "ACTIVO", "ANULADO", "BORRADOR" }, Textos(p, "Estado"));
    }

    [Fact]
    public async Task FamiliaYSubFamilia_EstanDeshabilitadasHastaElegirSuPadre_YOfrecenSoloLoDeEse()
    {
        var p = Pantalla();
        Assert.False(Combo(p, "Grupo de producto").Instance.Disabled);
        Assert.True(Combo(p, "Familia de producto").Instance.Disabled); Assert.Empty(Combo(p, "Familia de producto").Instance.Items);
        Assert.True(Combo(p, "Sub-familia de producto").Instance.Disabled); Assert.Empty(Combo(p, "Sub-familia de producto").Instance.Items);
        await Elige(p, "Grupo de producto", "00001");
        Assert.False(Combo(p, "Familia de producto").Instance.Disabled);
        Assert.Equal(new[] { "CAMISAS", "POLOS" }, Textos(p, "Familia de producto"));
        Assert.True(Combo(p, "Sub-familia de producto").Instance.Disabled);
        await Elige(p, "Familia de producto", "00001");
        Assert.False(Combo(p, "Sub-familia de producto").Instance.Disabled);
        Assert.Equal(new[] { "MANGA CORTA", "MANGA LARGA" }, Textos(p, "Sub-familia de producto"));
    }

    // ---------- cada filtro ----------
    [Fact]
    public async Task Grupo_DejaSoloLosDeEseGrupo_YElContadorLoDice()
    {
        var p = Pantalla();
        await Elige(p, "Grupo de producto", "00003");
        p.WaitForAssertion(() => Assert.Equal(Cods("D4", "D5", "D6"), Codigos(p)));
        Assert.Equal("3 de 10 registros", Contador(p));
    }

    [Fact]
    public async Task Familia_SeFiltraPorGrupoYFamilia_NoSoloPorElCodigoDeLaFamilia()
    {
        var p = Pantalla();                                      // la familia 00001 "OTROS" existe en CALZADO y en TELA ACABADA
        await Elige(p, "Grupo de producto", "00004");
        await Elige(p, "Familia de producto", "00001");
        p.WaitForAssertion(() => Assert.Equal(Cods("D7"), Codigos(p)));
        await Elige(p, "Grupo de producto", "00011");            // otro grupo: la familia se vacia
        await Elige(p, "Familia de producto", "00001");
        p.WaitForAssertion(() => Assert.Equal(Cods("D8", "D9"), Codigos(p)));
    }

    [Fact]
    public async Task SubFamilia_FiltraPorLaTerna()
    {
        var p = Pantalla();
        await Elige(p, "Grupo de producto", "00011");
        await Elige(p, "Familia de producto", "00001");
        await Elige(p, "Sub-familia de producto", "00002");
        p.WaitForAssertion(() => Assert.Equal(Cods("D9"), Codigos(p)));
    }

    [Fact]
    public async Task CambiarElGrupo_VaciaFamiliaYSubFamilia_YCambiarLaFamilia_VaciaLaSubFamilia()
    {
        var p = Pantalla();
        await Elige(p, "Grupo de producto", "00001");
        await Elige(p, "Familia de producto", "00001");
        await Elige(p, "Sub-familia de producto", "00002");
        p.WaitForAssertion(() => Assert.Equal(Cods("D2"), Codigos(p)));
        await Elige(p, "Familia de producto", "00002");          // otra familia del mismo grupo: la sub familia ya no vale
        p.WaitForAssertion(() => Assert.Equal(Cods("D3"), Codigos(p)));
        Assert.Null(Combo(p, "Sub-familia de producto").Instance.Value);
        Assert.Equal("00002", Combo(p, "Familia de producto").Instance.Value);
        await Elige(p, "Grupo de producto", "00003");            // otro grupo: familia y sub familia se vacian
        p.WaitForAssertion(() => Assert.Equal(Cods("D4", "D5", "D6"), Codigos(p)));
        Assert.Null(Combo(p, "Familia de producto").Instance.Value);
        Assert.True(Combo(p, "Sub-familia de producto").Instance.Disabled);
    }

    [Fact]
    public async Task BorrarElGrupo_QuitaElFiltroYDeshabilitaLosDeAbajo()
    {
        var p = Pantalla();
        await Elige(p, "Grupo de producto", "00001");
        await Elige(p, "Familia de producto", "00001");
        await Elige(p, "Grupo de producto", "");                 // el boton de borrar del combo emite vacio
        p.WaitForAssertion(() => Assert.Equal(10, Codigos(p).Length));
        Assert.True(Combo(p, "Familia de producto").Instance.Disabled);
        Assert.Null(Combo(p, "Familia de producto").Instance.Value);
    }

    [Fact]
    public async Task LasOpcionesDeFamiliaYSubFamilia_SonSoloLasDelPadreElegido_AunqueLosCodigosSeRepitanEnOtrosGrupos()
    {
        var p = Pantalla();
        await Elige(p, "Grupo de producto", "00001");
        await Elige(p, "Familia de producto", "00002");
        Assert.Equal(new[] { "FORMAL" }, Textos(p, "Sub-familia de producto"));            // CAMISAS tiene una sola; no las de POLOS
        await Elige(p, "Grupo de producto", "00004");
        Assert.Equal(new[] { "OTROS", "SANDALIAS" }, Textos(p, "Familia de producto"));    // no las de otros grupos
        await Elige(p, "Familia de producto", "00001");
        Assert.Equal(new[] { "VARIOS" }, Textos(p, "Sub-familia de producto"));            // la 00001 OTROS de CALZADO, no la de TELA ACABADA ni POLOS
        await Elige(p, "Grupo de producto", "00011");
        await Elige(p, "Familia de producto", "00001");
        Assert.Equal(new[] { "DIVERSOS", "VARIOS" }, Textos(p, "Sub-familia de producto"));
    }

    [Fact]
    public async Task CambiarElGrupo_NoDejaUnaSubFamiliaViejaEscondida()
    {
        var p = Pantalla();
        await Elige(p, "Grupo de producto", "00001");
        await Elige(p, "Familia de producto", "00001");
        await Elige(p, "Sub-familia de producto", "00002");
        await Elige(p, "Grupo de producto", "00003");
        await Elige(p, "Familia de producto", "00001");                                    // TEJIDO DE PUNTO: D4 (sub 00001) y D5 (sub 00002)
        p.WaitForAssertion(() => Assert.Equal(Cods("D4", "D5"), Codigos(p)));
        Assert.Null(Combo(p, "Sub-familia de producto").Instance.Value);
    }

    [Fact]
    public async Task Tipo_Filtra()
    {
        var p = Pantalla();
        await Elige(p, "Tipo de diseño", "MUE");
        p.WaitForAssertion(() => Assert.Equal(Cods("D3", "D5"), Codigos(p)));
    }

    [Fact]
    public async Task Entidad_FiltraPorCodigo_AunqueDosEntidadesSeLlamenIgual()
    {
        var p = Pantalla();
        await Elige(p, "Entidad", "300");
        p.WaitForAssertion(() => Assert.Equal(Cods("D7"), Codigos(p)));                 // "CLIENTE GAMMA" 300, no la 301
        await Elige(p, "Entidad", "100");
        p.WaitForAssertion(() => Assert.Equal(Cods("D1", "D2", "D6"), Codigos(p)));
    }

    [Fact]
    public async Task PersonalResponsable_FiltraPorLaEntidadAsignada()
    {
        var p = Pantalla();
        await Elige(p, "Personal responsable", "LUIS GOMEZ");
        p.WaitForAssertion(() => Assert.Equal(Cods("D3", "D5"), Codigos(p)));
    }

    [Fact]
    public async Task TemporadaColeccion_Filtra()
    {
        var p = Pantalla();
        await Elige(p, "Temporada / Colección", "VERANO 2026 / BASICOS");
        p.WaitForAssertion(() => Assert.Equal(Cods("D1", "D2"), Codigos(p)));
    }

    [Fact]
    public async Task Estado_Filtra()
    {
        var p = Pantalla();
        await Elige(p, "Estado", "ANULADO");
        p.WaitForAssertion(() => Assert.Equal(Cods("D6"), Codigos(p)));
        await Elige(p, "Estado", "BORRADOR");
        p.WaitForAssertion(() => Assert.Equal(Cods("D5"), Codigos(p)));
    }

    [Fact]
    public async Task LosFiltrosSeCombinan_ConY()
    {
        var p = Pantalla();
        await Elige(p, "Entidad", "100");                         // D1 D2 D6
        await Elige(p, "Tipo de diseño", "PRD");                  // los tres son PRD
        await Elige(p, "Estado", "ACTIVO");                       // sin D6 (anulado)
        p.WaitForAssertion(() => Assert.Equal(Cods("D1", "D2"), Codigos(p)));
        await Elige(p, "Grupo de producto", "00003");             // ninguno de los activos de la entidad 100 es de TELA CRUDA
        p.WaitForAssertion(() => Assert.Empty(Codigos(p)));
        Assert.Equal("0 de 10 registros", Contador(p));
    }

    [Fact]
    public async Task LosFiltrosDelPanel_SeCombinanConLosDeLasCabeceras_YLasCabecerasOfrecenSoloLoQueQueda()
    {
        var p = Pantalla();
        var cab = p.FindComponents<FiltroExcelColumna<DisenoRow>>().ToList();                 // 0 codigo, 1 descripcion, 2 clasificacion, 3 tipo...
        await Elige(p, "Grupo de producto", "00001");                                          // D1 D2 D3
        var tipos = p.FindComponents<FiltroExcelColumna<DisenoRow>>().ElementAt(3).Instance.ItemsBase;
        Assert.Equal(new[] { "D1", "D2", "D3" }, tipos.Select(x => x.COD_DISENO).ToArray());   // la cabecera del tipo parte de lo que dejo el panel
        await p.InvokeAsync(() => p.FindComponents<FiltroExcelColumna<DisenoRow>>().ElementAt(3).Instance.ValoresActivosChanged.InvokeAsync(new HashSet<string> { "MUESTRA" }));
        p.WaitForAssertion(() => Assert.Equal(Cods("D3"), Codigos(p)));
        Assert.Equal(3, cab.Count > 3 ? 3 : cab.Count);                                        // (hay una cabecera por columna con filtro)
    }

    // ---------- limpiar ----------
    [Fact]
    public async Task Limpiar_QuitaLosFiltrosDelPanel_YLosDeLasCabeceras()
    {
        var p = Pantalla();
        Assert.False(p.FindComponent<GridClearFiltersButton>().Instance.HasFilters);
        await Elige(p, "Grupo de producto", "00001");
        await Elige(p, "Estado", "ACTIVO");
        await Elige(p, "Personal responsable", "ANA PEREZ");
        await p.InvokeAsync(() => p.FindComponents<FiltroExcelColumna<DisenoRow>>().ElementAt(3).Instance.ValoresActivosChanged.InvokeAsync(new HashSet<string> { "PRODUCTO" }));
        p.WaitForAssertion(() => Assert.Equal(Cods("D1", "D2"), Codigos(p)));
        Assert.True(p.FindComponent<GridClearFiltersButton>().Instance.HasFilters);
        await p.InvokeAsync(() => p.FindComponent<GridClearFiltersButton>().Instance.OnClear.InvokeAsync());
        p.WaitForAssertion(() => Assert.Equal(10, Codigos(p).Length));
        Assert.All(Etiquetas, l => Assert.Null(Combo(p, l).Instance.Value));
        Assert.False(p.FindComponent<GridClearFiltersButton>().Instance.HasFilters);
    }

    [Fact]
    public async Task ConSoloUnFiltroDelPanel_ElBotonDeLimpiarSeActiva()
    {
        var p = Pantalla();
        await Elige(p, "Tipo de diseño", "MUE");
        Assert.True(p.FindComponent<GridClearFiltersButton>().Instance.HasFilters);
    }

    // ---------- ocultar / mostrar ----------
    [Fact]
    public async Task ElBotonOcultaYMuestraElPanel_YRecuerdaLaEleccion()
    {
        var p = Pantalla();
        Assert.Equal("Ocultar los filtros", TooltipFiltros(p));
        Assert.Equal(Color.Primary, BotonFiltros(p).Instance.Color);
        p.Find("[data-accion='filtros']").Click();
        p.WaitForAssertion(() => Assert.Empty(p.FindAll("[data-panel='filtros']")));
        Assert.Equal("Mostrar los filtros", TooltipFiltros(p));
        Assert.Equal(Color.Default, BotonFiltros(p).Instance.Color);
        Assert.Equal("false", _ls.Datos["disenos.verFiltros"]);
        p.Find("[data-accion='filtros']").Click();
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-panel='filtros']")));
        Assert.Equal("true", _ls.Datos["disenos.verFiltros"]);
        await Task.CompletedTask;
    }

    [Fact]
    public void SiLaUltimaVezLosOculto_ArrancaOculto()
    {
        _ls.Datos["disenos.verFiltros"] = "false";
        var p = Pantalla();
        Assert.Empty(p.FindAll("[data-panel='filtros']"));
        Assert.Equal("Mostrar los filtros", TooltipFiltros(p));
        Assert.Equal(10, Codigos(p).Length);
    }

    [Fact]
    public void SiLaUltimaVezLosDejoVisibles_ArrancaVisible()
    {
        _ls.Datos["disenos.verFiltros"] = "true";
        var p = Pantalla();
        Assert.NotEmpty(p.FindAll("[data-panel='filtros']"));
    }

    [Fact]
    public void SinAlmacenamientoDelNavegador_QuedanVisibles_YNoSeRompeNada()
    {
        _ls.Falla = true;
        var p = Pantalla();
        Assert.NotEmpty(p.FindAll("[data-panel='filtros']"));
        p.Find("[data-accion='filtros']").Click();                                    // ocultar tampoco falla aunque no se pueda recordar
        p.WaitForAssertion(() => Assert.Empty(p.FindAll("[data-panel='filtros']")));
    }

    [Fact]
    public async Task ConElPanelOculto_LosFiltrosSiguenAplicandose_YElBotonAvisaCuantosHay()
    {
        var p = Pantalla();
        await Elige(p, "Grupo de producto", "00003");
        await Elige(p, "Estado", "ACTIVO");
        p.WaitForAssertion(() => Assert.Equal(Cods("D4"), Codigos(p)));
        p.Find("[data-accion='filtros']").Click();
        p.WaitForAssertion(() => Assert.Empty(p.FindAll("[data-panel='filtros']")));
        Assert.Equal(Cods("D4"), Codigos(p));                                           // oculto no es "sin filtro"
        Assert.Equal(Color.Warning, BotonFiltros(p).Instance.Color);
        Assert.Equal("Mostrar los filtros (2 activos)", TooltipFiltros(p));
        await Task.CompletedTask;
    }

    [Fact]
    public async Task ElBotonDiceActivoEnSingular()
    {
        var p = Pantalla();
        await Elige(p, "Tipo de diseño", "MUE");
        p.Find("[data-accion='filtros']").Click();
        p.WaitForAssertion(() => Assert.Equal("Mostrar los filtros (1 activo)", TooltipFiltros(p)));
    }

    [Fact]
    public async Task AlVolverAMostrarElPanel_LosFiltrosSiguenPuestos()
    {
        var p = Pantalla();
        await Elige(p, "Estado", "BORRADOR");
        p.Find("[data-accion='filtros']").Click();
        p.Find("[data-accion='filtros']").Click();
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-panel='filtros']")));
        Assert.Equal("BORRADOR", Combo(p, "Estado").Instance.Value);
        Assert.Equal(Cods("D5"), Codigos(p));
    }

    // ---------- la columna Grupo / Familia / Sub-familia ----------
    [Fact]
    public void LaColumna_SeLlamaGrupoFamiliaSubFamilia_YYaNoHayUnaColumnaSoloDeSubFamilia()
    {
        var p = Pantalla();
        var titulos = p.FindAll(".eo-titulo").Select(t => t.TextContent.Trim()).ToList();
        Assert.Contains("Grupo / Familia / Sub-familia de producto", titulos);
        Assert.DoesNotContain("Sub Familia", titulos);
        Assert.Equal(1, titulos.Count(t => t == "Grupo / Familia / Sub-familia de producto"));
    }

    [Fact]
    public void CadaFila_MuestraGrupoFamiliaYSubFamilia_SeparadosPorBarra()
    {
        var p = Pantalla();
        var filas = p.FindAll("tbody tr").ToDictionary(tr => tr.QuerySelector("td")!.TextContent.Trim(), tr => tr.TextContent);
        Assert.Contains("PRENDA DE VESTIR / POLOS / MANGA CORTA", filas["D1"]);
        Assert.Contains("TELA CRUDA / TEJIDO PLANO / TAFETAN", filas["D6"]);
        Assert.Contains("TELA ACABADA / OTROS / DIVERSOS", filas["D9"]);
    }

    [Fact]
    public void SiElSPTodaviaNoTraeGrupoYFamilia_SeMuestranSusCodigos_YLosFiltrosFuncionan()
    {
        _f.SinDescripcionesGrupoFamilia = true;                       // falta aplicar el script 114
        var p = Pantalla();
        Assert.Contains("00001 / 00001 / MANGA CORTA", p.FindAll("tbody tr").First(tr => tr.QuerySelector("td")!.TextContent.Trim() == "D1").TextContent);
        Assert.Equal(new[] { "00001", "00003", "00004", "00011" }, Textos(p, "Grupo de producto"));
    }

    [Fact]
    public async Task OrdenarPorLaColumna_VaPorGrupo_LuegoFamilia_LuegoSubFamilia_Y_AlRevesAlSegundoClic()
    {
        var p = Pantalla();
        var cabecera = p.FindAll(".eo-titulo").First(t => t.TextContent.Trim() == "Grupo / Familia / Sub-familia de producto");
        cabecera.Click();
        // CALZADO / OTROS / VARIOS; PRENDA DE VESTIR / CAMISAS...; / POLOS / MANGA CORTA, MANGA LARGA; TELA ACABADA / OTROS / DIVERSOS, VARIOS; TELA CRUDA / TEJIDO DE PUNTO / JERSEY SIMPLE, PIQUE; TELA CRUDA / TEJIDO PLANO
        p.WaitForAssertion(() => Assert.Equal(Cods("D7", "D10", "D3", "D1", "D2", "D9", "D8", "D4", "D5", "D6"), Codigos(p)));
        p.FindAll(".eo-titulo").First(t => t.TextContent.Trim() == "Grupo / Familia / Sub-familia de producto").Click();
        p.WaitForAssertion(() => Assert.Equal(Cods("D6", "D5", "D4", "D8", "D9", "D2", "D1", "D3", "D10", "D7"), Codigos(p)));
        await Task.CompletedTask;
    }

    [Fact]
    public void LaCabeceraDeLaColumna_OfreceElTextoCombinado_ParaFiltrar()
    {
        var p = Pantalla();
        var f = p.FindComponents<FiltroExcelColumna<DisenoRow>>().ElementAt(2).Instance;       // la tercera: Grupo / Familia / Sub-familia
        Assert.Equal("PRENDA DE VESTIR / POLOS / MANGA CORTA", f.ValorDe(_f.Todos[0]));
        Assert.Equal(10, f.ItemsBase.Count);
    }

    [Fact]
    public async Task FiltrarPorLaCabeceraDeLaColumna_UsaElTextoCombinado()
    {
        var p = Pantalla();
        await p.InvokeAsync(() => p.FindComponents<FiltroExcelColumna<DisenoRow>>().ElementAt(2).Instance.ValoresActivosChanged.InvokeAsync(new HashSet<string> { "TELA ACABADA / OTROS / VARIOS", "CALZADO / OTROS / VARIOS" }));
        p.WaitForAssertion(() => Assert.Equal(Cods("D7", "D8"), Codigos(p)));
    }

    // ---------- buscar y recargar ----------
    [Fact]
    public async Task AlBuscar_LosFiltrosQueYaNoTienenOpcion_SeQuitan_YLaGrillaNoQuedaVaciaSinMotivo()
    {
        var p = Pantalla();
        await Elige(p, "Grupo de producto", "00004");                   // CALZADO
        p.WaitForAssertion(() => Assert.Equal(Cods("D7", "D10"), Codigos(p)));
        await p.InvokeAsync(async () => { await p.FindComponent<BarraBusqueda>().Instance.ValueChanged.InvokeAsync("POLO"); await p.FindComponent<BarraBusqueda>().Instance.OnSearch.InvokeAsync("POLO"); });
        p.WaitForAssertion(() => Assert.Equal(Cods("D1", "D2"), Codigos(p)));                   // la busqueda trae polos; CALZADO ya no existe en la lista
        Assert.Null(Combo(p, "Grupo de producto").Instance.Value);
        Assert.Equal(new[] { "PRENDA DE VESTIR" }, Textos(p, "Grupo de producto"));
    }

    [Fact]
    public async Task AlBuscar_LosFiltrosQueSiguenValiendo_SeConservan_YAcotanElResultado()
    {
        var p = Pantalla();
        await Elige(p, "Grupo de producto", "00001");
        await Elige(p, "Familia de producto", "00001");
        await Elige(p, "Sub-familia de producto", "00002");
        p.WaitForAssertion(() => Assert.Equal(Cods("D2"), Codigos(p)));
        await p.InvokeAsync(async () => { await p.FindComponent<BarraBusqueda>().Instance.ValueChanged.InvokeAsync("POLO"); await p.FindComponent<BarraBusqueda>().Instance.OnSearch.InvokeAsync("POLO"); });
        p.WaitForAssertion(() => Assert.Contains(_f.Calls, c => c.U.Contains("search=POLO")));
        Assert.Equal(Cods("D2"), Codigos(p));
        Assert.Equal("00002", Combo(p, "Sub-familia de producto").Instance.Value);
        Assert.Equal("00001", Combo(p, "Grupo de producto").Instance.Value);
    }

    [Fact]
    public async Task AlBuscar_SiSeQuitaElGrupo_LaFamiliaYLaSubFamiliaSeQuitanConEl()
    {
        var p = Pantalla();
        await Elige(p, "Grupo de producto", "00004");
        await Elige(p, "Familia de producto", "00001");
        await p.InvokeAsync(async () => { await p.FindComponent<BarraBusqueda>().Instance.ValueChanged.InvokeAsync("POLO"); await p.FindComponent<BarraBusqueda>().Instance.OnSearch.InvokeAsync("POLO"); });
        p.WaitForAssertion(() => Assert.Equal(Cods("D1", "D2"), Codigos(p)));
        Assert.Null(Combo(p, "Familia de producto").Instance.Value);
        Assert.True(Combo(p, "Familia de producto").Instance.Disabled);
    }

    [Fact]
    public async Task AlRecargar_UnFiltroCuyoValorDesaparecio_SeQuita()
    {
        var p = Pantalla();
        await Elige(p, "Estado", "BORRADOR");
        p.WaitForAssertion(() => Assert.Equal(Cods("D5"), Codigos(p)));
        _f.Todos = _f.Todos.Where(d => d.COD_DISENO != "D5").ToList();                           // alguien lo anulo o lo borro
        await p.InvokeAsync(() => p.FindComponent<GridRefreshButton>().Instance.OnRefresh.InvokeAsync());
        p.WaitForAssertion(() => Assert.Equal(9, Codigos(p).Length));
        Assert.Null(Combo(p, "Estado").Instance.Value);
    }

    [Theory]
    [InlineData("Familia de producto", "00002", "D3")]                      // grupo 00001 + familia CAMISAS
    [InlineData("Sub-familia de producto", "00002", "D2")]                  // grupo 00001 + familia POLOS + MANGA LARGA
    [InlineData("Tipo de diseño", "MUE", "D3,D5")]
    [InlineData("Entidad", "300", "D7")]
    [InlineData("Personal responsable", "ANA PEREZ", "D1,D2")]
    [InlineData("Temporada / Colección", "INVIERNO 2026 / PREMIUM", "D3")]
    public async Task AlRecargar_CadaFiltroCuyoValorDesaparecio_SeQuita_SinDejarLaGrillaVacia(string combo, string valor, string quitar)
    {
        var p = Pantalla();
        if (combo is "Familia de producto" or "Sub-familia de producto") await Elige(p, "Grupo de producto", "00001");
        if (combo == "Sub-familia de producto") await Elige(p, "Familia de producto", "00001");
        await Elige(p, combo, valor);
        p.WaitForAssertion(() => Assert.NotEmpty(Codigos(p)));
        var quitados = quitar.Split(',');
        _f.Todos = _f.Todos.Where(d => !quitados.Contains(d.COD_DISENO)).ToList();
        await p.InvokeAsync(() => p.FindComponent<GridRefreshButton>().Instance.OnRefresh.InvokeAsync());
        p.WaitForAssertion(() => Assert.Null(Combo(p, combo).Instance.Value));
        var esperados = _f.Todos.Count(d => combo is not ("Familia de producto" or "Sub-familia de producto") || d.COD_GRUPO_PRODUCTO == "00001" && (combo == "Familia de producto" || d.COD_FAMILIA_PRODUCTO == "00001"));
        Assert.Equal(esperados, Codigos(p).Length);
    }

    [Fact]
    public async Task SinDatos_LosFiltrosNoOfrecenNada_YNoSeRompe()
    {
        _f.Todos = new();
        var p = Pantalla0();
        Assert.All(Etiquetas, l => Assert.Empty(Combo(p, l).Instance.Items));
        await Task.CompletedTask;
    }

    IRenderedComponent<KONSolutions.Web.Pages.Ingenieria.Disenos> Pantalla0()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton(new HttpClient(_f) { BaseAddress = new Uri("http://x/") });
        Services.AddSingleton<ILocalStorageService>(_ls);
        Services.AddScoped<ErrorLogService>(); Services.AddScoped<ConfigService>(); Services.AddScoped<EtiquetasService>();
        Services.AddSingleton<Microsoft.Extensions.Configuration.IConfiguration>(new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build());
        Services.AddScoped<AccessLogService>(); Services.AddScoped<MenuService>(); Services.AddScoped<ControlCambiosClientService>();
        ComponentFactories.AddStub<AccionesBotones>(); ComponentFactories.AddStub<TituloPagina>();
        RenderComponent<MudPopoverProvider>(); RenderComponent<MudSnackbarProvider>(); RenderComponent<MudDialogProvider>();
        var p = RenderComponent<KONSolutions.Web.Pages.Ingenieria.Disenos>();
        p.WaitForAssertion(() => Assert.Contains("0 de 0 registros", p.Markup));
        return p;
    }
}
