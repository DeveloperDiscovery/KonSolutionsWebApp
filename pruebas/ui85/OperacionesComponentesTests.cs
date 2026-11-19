using System.Net;
using System.Net.Http.Json;
using Blazored.LocalStorage;
using Bunit;
using KONSolutions.Shared.Models.Ingenieria;
using KONSolutions.Web.Pages.Ingenieria;
using KONSolutions.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using Xunit;

// Al armar el arbol de una operacion compuesta (Componentes...), el buscador ofrece TODO el catalogo (simples y compuestas),
// no solo las primeras 30: una compuesta puede ser hija de otra.
public class OperacionesComponentesTests : TestContext
{
    sealed class H : HttpMessageHandler
    {
        public List<Operacion> Catalogo = new();
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage r, CancellationToken ct)
        {
            var u = r.RequestUri!.PathAndQuery;
            object data = u.Contains("/componentes") ? new List<OperacionComponente>() : Catalogo;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(new { Success = true, Data = data }) });
        }
    }

    [Fact]
    public async Task ElBuscadorOfreceLasCompuestasAunConMasDe30OperacionesSimples()
    {
        var h = new H();
        for (var i = 1; i <= 45; i++) h.Catalogo.Add(new Operacion { COD_OPERACION = $"OP{i:0000}", DES_OPERACION = $"SIMPLE {i}" });
        h.Catalogo.Add(new Operacion { COD_OPERACION = "OP0050", DES_OPERACION = "ARMADO COMPUESTO", FLG_COMPUESTA = true, CAN_COMPONENTES = 2 });
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton(new HttpClient(h) { BaseAddress = new Uri("http://x/") });
        Services.AddBlazoredLocalStorage();
        Services.AddScoped<ErrorLogService>(); Services.AddScoped<ConfigService>(); Services.AddScoped<EtiquetasService>();
        Services.AddSingleton<Microsoft.Extensions.Configuration.IConfiguration>(new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build());
        RenderComponent<MudPopoverProvider>(); RenderComponent<MudSnackbarProvider>();
        var p = RenderComponent<MudDialogProvider>();
        var ds = Services.GetRequiredService<IDialogService>();
        await p.InvokeAsync(async () => await ds.ShowAsync<OperacionComponentesDialog>("t", new DialogParameters { ["Operacion"] = new Operacion { COD_OPERACION = "OP0099", DES_OPERACION = "RAIZ" } }));
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindComponents<MudAutocomplete<Operacion>>()));
        var ac = p.FindComponent<MudAutocomplete<Operacion>>().Instance;
        p.WaitForAssertion(async () => Assert.True((await ac.SearchFunc("", CancellationToken.None)).Any()));
        var todos = (await ac.SearchFunc("", CancellationToken.None)).ToList();
        Assert.Equal(46, todos.Count);                                            // 45 simples + 1 compuesta: nada se corta en 30
        Assert.True(ac.MaxItems is null || ac.MaxItems >= todos.Count);              // el propio desplegable tampoco recorta la lista (MaxItems)
        Assert.Contains(todos, o => o.COD_OPERACION == "OP0050" && o.FLG_COMPUESTA);
        Assert.Contains("(compuesta)", ac.ToStringFunc!(todos.Single(o => o.FLG_COMPUESTA)));   // y se distingue de las simples
        Assert.DoesNotContain("(compuesta)", ac.ToStringFunc!(todos.First()));
    }
}
