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
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using Xunit;

// Catalogo "Tipos de material de la estructura" (scripts 154 a 156): quinta pestana de la pantalla de catalogos de operaciones, contra una API simulada.
public class FakeTiposMaterial : HttpMessageHandler
{
    public List<(string M, string U, JsonElement? B)> Calls = new();
    public string? MensajeFalla;
    public List<CatalogoOperacionItem> Items = new()
    {
        new() { COD = "01", DES = "MATERIA PRIMA", DES_DETALLE = "Tela, cuero, acero, madera", NUM_ORDEN = 1, FLG_1 = true, COD_REGLA = "MP" },
        new() { COD = "02", DES = "MATERIAL COMPLEMENTARIO", DES_DETALLE = "Hilos, botones, cierres", NUM_ORDEN = 2, FLG_1 = true, COD_REGLA = "COMP" },
        new() { COD = "05", DES = "HERRAMIENTAS Y UTILLAJE", DES_DETALLE = "Moldes, matrices", NUM_ORDEN = 5, FLG_1 = false, COD_REGLA = "HER" },
    };

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage r, CancellationToken ct)
    {
        var u = r.RequestUri!.PathAndQuery;
        JsonElement? body = r.Content is null ? null : JsonDocument.Parse(await r.Content.ReadAsStringAsync(ct)).RootElement.Clone();
        Calls.Add((r.Method.Method, u, body));
        if (r.Method != HttpMethod.Get && MensajeFalla is not null)
            return new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = JsonContent.Create(new { Success = false, Message = MensajeFalla }) };
        object data = r.Method == HttpMethod.Get && u.Contains("tipos-material-estructura") ? Items : (object)true;
        return new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(new { Success = true, Data = data }) };
    }
}

public class TiposMaterialCatalogoTests : TestContext
{
    readonly FakeTiposMaterial F = new();
    IRenderedComponent<MudDialogProvider>? _dlg;

    IRenderedComponent<OperacionesCatalogos> Pagina(string ruta = "/ingenieria/tipos-material-estructura")
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton(new HttpClient(F) { BaseAddress = new Uri("http://x/") });
        Services.AddBlazoredLocalStorage();
        Services.AddScoped<ErrorLogService>(); Services.AddScoped<ConfigService>(); Services.AddScoped<EtiquetasService>();
        Services.AddSingleton<Microsoft.Extensions.Configuration.IConfiguration>(new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build());
        Services.AddScoped<AccessLogService>(); Services.AddScoped<MenuService>(); Services.AddScoped<ControlCambiosClientService>();
        ComponentFactories.AddStub<AccionesBotones>();
        Services.GetRequiredService<NavigationManager>().NavigateTo(ruta);
        RenderComponent<MudPopoverProvider>(); RenderComponent<MudSnackbarProvider>(); _dlg = RenderComponent<MudDialogProvider>();
        return RenderComponent<OperacionesCatalogos>();
    }

    static JsonElement Prop(JsonElement e, string n) => e.EnumerateObject().First(x => string.Equals(x.Name, n, StringComparison.OrdinalIgnoreCase)).Value;
    IRenderedComponent<MudTextField<string>> Txt(IRenderedComponent<OperacionesCatalogos> p, string label) => p.FindComponents<MudTextField<string>>().First(c => c.Instance.Label == label || (c.Instance.Label ?? "").StartsWith(label));

    [Fact]
    public void LaOpcionDelMenuAbreLaPestanaDeTiposDeMaterial_ConSusColumnas()
    {
        var p = Pagina();
        p.WaitForAssertion(() => Assert.Equal(3, p.FindAll("tr").Count(t => t.TextContent.Contains("Tela, cuero") || t.TextContent.Contains("Hilos, botones") || t.TextContent.Contains("Moldes, matrices"))));
        Assert.Contains(F.Calls, c => c.M == "GET" && c.U.EndsWith("/tipos-material-estructura"));
        var cab = string.Join("|", p.FindAll("th").Select(h => h.TextContent.Trim()));
        Assert.Contains("Abreviatura", cab); Assert.Contains("Ejemplos", cab); Assert.Contains("Habilitado", cab);
        Assert.DoesNotContain("Estado", cab);                                                            // este catalogo no usa estados
        Assert.Equal(new[] { "MP", "COMP", "HER" }, p.FindAll("[data-celda='abreviatura']").Select(c => c.TextContent.Trim()).ToArray());
        Assert.Equal(3, p.FindAll("[data-celda='habilitado']").Count);
        var pestanas = p.FindAll(".mud-tab").Select(t => t.TextContent.Trim()).ToArray();
        Assert.Equal(5, pestanas.Length); Assert.Equal("Tipos de material", pestanas[4]);                  // la quinta pestana
        Assert.Contains("mud-tab-active", p.FindAll(".mud-tab")[4].ClassName ?? "");                       // y es la activa
    }

    [Fact]
    public void OtrasOpcionesDelMenuSiguenAbriendoSusPestanas()
    {
        var p = Pagina("/ingenieria/tipos-maquina");
        p.WaitForAssertion(() => Assert.Contains(F.Calls, c => c.M == "GET" && c.U.EndsWith("/tipos-maquina")));
        Assert.DoesNotContain(F.Calls, c => c.U.EndsWith("/tipos-material-estructura"));
    }

    [Fact]
    public async Task Nuevo_PideAbreviaturaEjemplosOrdenYHabilitado_YGrabaConPost()
    {
        var p = Pagina();
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-accion='nuevo']")));
        await p.InvokeAsync(() => p.Find("[data-accion='nuevo']").Click());
        var raiz = _dlg!;                                                                                 // el dialogo se dibuja en el proveedor de dialogos
        raiz.WaitForAssertion(() => Assert.NotEmpty(raiz.FindAll("[data-campo='abreviatura']")));
        var cod = raiz.FindComponents<MudTextField<string>>().First(c => c.Instance.Label == "Código");
        await raiz.InvokeAsync(() => cod.Instance.ValueChanged.InvokeAsync("06"));
        await raiz.InvokeAsync(() => raiz.FindComponents<MudTextField<string>>().First(c => c.Instance.Label == "Descripción").Instance.ValueChanged.InvokeAsync("material de prueba"));
        await raiz.InvokeAsync(() => raiz.FindComponents<MudTextField<string>>().First(c => (c.Instance.Label ?? "").StartsWith("Abreviatura")).Instance.ValueChanged.InvokeAsync("PRB"));
        await raiz.InvokeAsync(() => raiz.FindComponents<MudTextField<string>>().First(c => c.Instance.Label == "Ejemplos").Instance.ValueChanged.InvokeAsync("Ejemplo uno"));
        Assert.Contains("Habilitado", raiz.Markup);
        await raiz.InvokeAsync(() => raiz.Find("[data-accion='guardar']").Click());
        raiz.WaitForAssertion(() => Assert.Contains(F.Calls, c => c.M == "POST" && c.U.EndsWith("/tipos-material-estructura")));
        var b = F.Calls.Last(c => c.M == "POST").B!.Value;
        Assert.Equal("06", Prop(b, "COD").GetString()); Assert.Equal("PRB", Prop(b, "COD_REGLA").GetString());
        Assert.Equal("Ejemplo uno", Prop(b, "DES_DETALLE").GetString());
        Assert.Equal(JsonValueKind.True, Prop(b, "FLG_1").ValueKind);                                   // habilitado por defecto: true (nunca null)
    }

    [Fact]
    public async Task Nuevo_SinAbreviatura_LaApiRechaza_YElDialogoMuestraElMotivo()
    {
        F.MensajeFalla = "La abreviatura es obligatoria.";
        var p = Pagina();
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-accion='nuevo']")));
        await p.InvokeAsync(() => p.Find("[data-accion='nuevo']").Click());
        var raiz = _dlg!;
        raiz.WaitForAssertion(() => Assert.NotEmpty(raiz.FindAll("[data-campo='abreviatura']")));
        await raiz.InvokeAsync(() => raiz.FindComponents<MudTextField<string>>().First(c => c.Instance.Label == "Código").Instance.ValueChanged.InvokeAsync("07"));
        await raiz.InvokeAsync(() => raiz.FindComponents<MudTextField<string>>().First(c => c.Instance.Label == "Descripción").Instance.ValueChanged.InvokeAsync("otro"));
        await raiz.InvokeAsync(() => raiz.Find("[data-accion='guardar']").Click());
        raiz.WaitForAssertion(() => Assert.Contains("abreviatura es obligatoria", raiz.Find("[data-error]").TextContent));
    }
}
