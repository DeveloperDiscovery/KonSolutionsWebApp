using System.Net;
using System.Net.Http.Json;
using Blazored.LocalStorage;
using Bunit;
using KONSolutions.Shared.Models;
using KONSolutions.Shared.Models.Estandar;
using KONSolutions.Web.Pages.Estandar;
using KONSolutions.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using Xunit;

/// <summary>Dibuja el dialogo de Tipos de Operacion: errores de dibujo y la exclusion Devolucion/Cargo del interruptor.</summary>
public class TipoOperacionDialogTests : TestContext
{
    class H : HttpMessageHandler
    {
        public List<string> Urls = new();
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage r, CancellationToken ct)
        {
            var u = r.RequestUri!.PathAndQuery; Urls.Add(u);
            object data = u.Contains("tipos-motivos/combobox") ? new[] { new { Id = "32001", Texto = "MOTIVOS NOTA" } }
                        : u.Contains("motivos/por-tipo/32001") ? new[] { new { COD_TIPO_MOTIVO = 32001, COD_MOTIVO = 2, DES_MOTIVO = "MOTIVO DOS" } }
                        : u.Contains("loadcombos") ? new { } : new object[0];
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(new { Success = true, Data = data }) });
        }
    }

    [Fact]
    public async Task Dibuja_ConCargoYMotivo_YDevolucionCargoSonExcluyentes()
    {
        var h = new H();
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton(new HttpClient(h) { BaseAddress = new Uri("http://x/") });
        Services.AddBlazoredLocalStorage();
        Services.AddScoped<ErrorLogService>(); Services.AddScoped<ConfigService>();
        ComponentFactories.AddStub<KONSolutions.Web.Pages.Shared.CampoTexto>();
        ComponentFactories.AddStub<KONSolutions.Web.Pages.Shared.EstadoCombo>();
        ComponentFactories.AddStub<KONSolutions.Web.Pages.Shared.CrudDialogActions>();
        ComponentFactories.AddStub<KONSolutions.Web.Pages.Shared.PanelAuditoria>();
        ComponentFactories.AddStub<KONSolutions.Web.Pages.Shared.CrudDialogTitle>();
        var prov = RenderComponent<MudDialogProvider>();
        RenderComponent<MudPopoverProvider>(); RenderComponent<MudSnackbarProvider>();
        var ds = Services.GetRequiredService<IDialogService>();
        var item = new TipoOperacion { COD_CLASE_OPERACION = "ZZ86", COD_TIPO_OPERACION = "DEV", DES_TIPO_OPERACION = "Devolucion", FLG_DEVOLUCION = true, COD_TIPO_MOTIVO = 32001, COD_MOTIVO_DEFAULT = 2 };
        await prov.InvokeAsync(() => ds.ShowAsync<TipoOperacionDialog>("t", new DialogParameters { ["Item"] = item, ["Modo"] = CrudModo.Update, ["CodClaseOperacion"] = "ZZ86", ["DesClaseOperacion"] = "X" }));
        prov.WaitForState(() => prov.Markup.Contains("Cargo"), TimeSpan.FromSeconds(5));
        Assert.Contains("Tipo de motivo", prov.Markup);
        Assert.Contains("Motivo por defecto", prov.Markup);
        Assert.Contains(h.Urls, u => u.Contains("motivos/por-tipo/32001"));       // carga los motivos del tipo de motivo guardado
        var sw = prov.FindComponents<MudSwitch<bool?>>();
        var devolucion = sw.First(x => x.Instance.Label == "Devolución"); var cargo = sw.First(x => x.Instance.Label == "Cargo");
        Assert.True(devolucion.Instance.Value); Assert.NotEqual(true, cargo.Instance.Value);
        await prov.InvokeAsync(() => cargo.Instance.ValueChanged.InvokeAsync(true));      // prender Cargo apaga Devolucion
        prov.WaitForState(() => prov.FindComponents<MudSwitch<bool?>>().First(x => x.Instance.Label == "Devolución").Instance.Value != true);
        Assert.True(prov.FindComponents<MudSwitch<bool?>>().First(x => x.Instance.Label == "Cargo").Instance.Value);
        await prov.InvokeAsync(() => prov.FindComponents<MudSwitch<bool?>>().First(x => x.Instance.Label == "Devolución").Instance.ValueChanged.InvokeAsync(true));   // y al reves
        prov.WaitForState(() => prov.FindComponents<MudSwitch<bool?>>().First(x => x.Instance.Label == "Cargo").Instance.Value != true);
    }
}
