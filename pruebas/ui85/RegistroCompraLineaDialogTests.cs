using System.Net;
using System.Net.Http.Json;
using Blazored.LocalStorage;
using Bunit;
using KONSolutions.Shared.Models.Comercial;
using KONSolutions.Web.Pages.Comercial;
using KONSolutions.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using Xunit;

/// <summary>Dialogo de linea del Registro de Compras: con guia (devolucion/cargo con incidencia) el precio es el de la factura afectada y el total sale de cantidad x precio;
/// sin guia precio y total se calculan entre si (con el descuento general de la cabecera).</summary>
public class RegistroCompraLineaDialogTests : TestContext
{
    class H : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage r, CancellationToken ct)
        {
            var u = r.RequestUri!.PathAndQuery;
            object data = u.Contains("/incidencias/lineas/") ? new { FLG_CON_GUIA = true, NUM_COMPROBANTE_PAGO = "F001-00000001", NUM_SECUENCIA_ASOCIADO = 1, COD_PRODUCTO = "P-1", DES_PRODUCTO = "BOLSA", CAN_FACTURADA = 10m, PRE_UNITARIO = 100m, CAN_GUIA = 4m, NUM_GUIAS = "T001-00000001" }
                        : new object[0];
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(new { Success = true, Data = data }) });
        }
    }

    async Task<IRenderedFragment> Abre(bool conGuia, decimal? tasaGeneral = null)
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton(new HttpClient(new H()) { BaseAddress = new Uri("http://x/") });
        Services.AddBlazoredLocalStorage();
        Services.AddScoped<ErrorLogService>(); Services.AddScoped<ConfigService>();
        var prov = RenderComponent<MudDialogProvider>();
        RenderComponent<MudPopoverProvider>(); RenderComponent<MudSnackbarProvider>();
        var ds = Services.GetRequiredService<IDialogService>();
        var linea = new RegistroCompraLinea { NUM_SECUENCIA = 2, CAN_PRODUCTO = 1, PRE_UNITARIO = 7, VAL_TOTAL = 7 };
        await prov.InvokeAsync(() => ds.ShowAsync<RegistroCompraLineaDialog>("t", new DialogParameters { ["NumRegistro"] = 5m, ["Linea"] = linea, ["ConGuia"] = conGuia, ["TasaDescuentoGeneral"] = tasaGeneral }));
        prov.WaitForState(() => prov.Markup.Contains("Total de la l"), TimeSpan.FromSeconds(5));
        return prov;
    }

    static IRenderedComponent<MudNumericField<decimal?>> Campo(IRenderedFragment f, string campo) => f.FindComponents<MudNumericField<decimal?>>().First(x => x.Markup.Contains($"data-campo=\"{campo}\""));

    [Fact]
    public async Task ConGuia_MuestraLaFacturaAfectadaYLaCantidadDelProveedor_PrecioBloqueado_TotalCantidadPorPrecio()
    {
        var d = await Abre(conGuia: true);
        d.WaitForState(() => d.Markup.Contains("cantidad-guia"), TimeSpan.FromSeconds(5));
        Assert.Contains("F001-00000001", d.Markup);
        Assert.Contains("T001-00000001", d.Markup);
        Assert.True(Campo(d, "precio").Instance.Disabled);
        Assert.True(Campo(d, "total").Instance.Disabled);
        Assert.Equal(100m, Campo(d, "precio").Instance.Value);          // el de la factura (la linea tenia 7)
        Assert.Equal(4m, Campo(d, "cantidad").Instance.Value);          // la del proveedor segun la guia
        Assert.Equal(400m, Campo(d, "total").Instance.Value);           // 4 x 100
        await d.InvokeAsync(() => Campo(d, "cantidad").Instance.ValueChanged.InvokeAsync(3m));
        Assert.Equal(300m, Campo(d, "total").Instance.Value);          // sigue siendo cantidad x precio
    }

    [Fact]
    public async Task SinGuia_PrecioYTotalEditables_SeCalculanEntreSi()
    {
        var d = await Abre(conGuia: false);
        Assert.False(Campo(d, "precio").Instance.Disabled);
        Assert.False(Campo(d, "total").Instance.Disabled);
        Assert.DoesNotContain("factura-afectada", d.Markup);
        await d.InvokeAsync(() => Campo(d, "cantidad").Instance.ValueChanged.InvokeAsync(5m));
        await d.InvokeAsync(() => Campo(d, "precio").Instance.ValueChanged.InvokeAsync(20m));
        Assert.Equal(100m, Campo(d, "total").Instance.Value);          // precio -> total
        await d.InvokeAsync(() => Campo(d, "total").Instance.ValueChanged.InvokeAsync(250m));
        Assert.Equal(50m, Campo(d, "precio").Instance.Value);          // total -> precio
    }

    [Fact]
    public async Task ConDescuentoGeneral_ElPrecioSeCalculaConElDescuento()
    {
        var d = await Abre(conGuia: false, tasaGeneral: 10m);
        await d.InvokeAsync(() => Campo(d, "cantidad").Instance.ValueChanged.InvokeAsync(5m));
        await d.InvokeAsync(() => Campo(d, "total").Instance.ValueChanged.InvokeAsync(450m));
        Assert.Equal(100m, Campo(d, "precio").Instance.Value);         // 450 / (5 x 0.90)
        await d.InvokeAsync(() => Campo(d, "precio").Instance.ValueChanged.InvokeAsync(200m));
        Assert.Equal(900m, Campo(d, "total").Instance.Value);          // 5 x 200 x 0.90
    }
}
