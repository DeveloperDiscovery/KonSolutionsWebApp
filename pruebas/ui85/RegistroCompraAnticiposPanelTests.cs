using System.Net;
using System.Net.Http.Json;
using Blazored.LocalStorage;
using Bunit;
using KONSolutions.Web.Pages.Comercial;
using KONSolutions.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using Xunit;

public class FakeAnt : HttpMessageHandler
{
    public List<(string M, string U)> Calls = new();
    public bool Falla;
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage r, CancellationToken ct)
    {
        var u = r.RequestUri!.PathAndQuery; Calls.Add((r.Method.Method, u));
        if (Falla && r.Method == HttpMethod.Post)
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = JsonContent.Create(new { Success = false, Message = "Los anticipos (2000) exceden el total de la factura (1770)." }) });
        object data = u.EndsWith("/anticipos/disponibles")
            ? new[] { new { NUM_REGISTRO = 700m, COD_TIPO_COMPROBANTE_PAGO = "FCTRA", NUM_COMPROBANTE_PAGO = "F001-00000700", FEC_COMPROBANTE_PAGO = "2026-11-01T00:00:00", COD_MONEDA = "PEN", VAL_TOTAL = 500m, DES_TIPO_OPERACION = "ANTICIPOS", DES_DETALLE = (string?)null, COD_ESTADO_ALIAS = "REG", DES_ESTADO = "Registrado" } }
            : u.EndsWith("/anticipos")
            ? new[] { new { NUM_REGISTRO = 600m, COD_TIPO_COMPROBANTE_PAGO = "FCTRA", NUM_COMPROBANTE_PAGO = "F001-00000600", FEC_COMPROBANTE_PAGO = "2026-10-01T00:00:00", COD_MONEDA = "PEN", VAL_TOTAL = 1000m, DES_TIPO_OPERACION = "ANTICIPOS", DES_DETALLE = "ADELANTO 30% SERVICIO", COD_ESTADO_ALIAS = "APR", DES_ESTADO = "Aprobado" },
                      new { NUM_REGISTRO = 601m, COD_TIPO_COMPROBANTE_PAGO = "FCTRA", NUM_COMPROBANTE_PAGO = "F001-00000601", FEC_COMPROBANTE_PAGO = "2026-10-02T00:00:00", COD_MONEDA = "PEN", VAL_TOTAL = 250.5m, DES_TIPO_OPERACION = "ANTICIPOS", DES_DETALLE = (string?)null, COD_ESTADO_ALIAS = "REG", DES_ESTADO = "Registrado" } }
            : new object[0];
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(new { Success = true, Data = data }) });
    }
}

public class AnticiposPanelTests : TestContext
{
    int _cambios;
    (IRenderedComponent<RegistroCompraAnticiposPanel> c, FakeAnt f) Dibuja(decimal num = 5m, bool soloLectura = false, bool falla = false)
    {
        var fake = new FakeAnt { Falla = falla };
        Services.AddMudServices(); JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton(new HttpClient(fake) { BaseAddress = new Uri("http://x/") });
        Services.AddBlazoredLocalStorage(); Services.AddScoped<ErrorLogService>();
        RenderComponent<MudBlazor.MudPopoverProvider>(); RenderComponent<MudBlazor.MudSnackbarProvider>(); RenderComponent<MudBlazor.MudDialogProvider>();
        var c = RenderComponent<RegistroCompraAnticiposPanel>(p => p.Add(x => x.NumRegistro, num).Add(x => x.SoloLectura, soloLectura)
            .Add(x => x.OnCambio, Microsoft.AspNetCore.Components.EventCallback.Factory.Create(this, () => _cambios++)));
        if (num > 0) c.WaitForState(() => c.Markup.Contains("F001-00000600"));
        return (c, fake);
    }

    [Fact]
    public void ListaLosAnticiposAplicados_ConSuTotal()
    {
        var (c, f) = Dibuja();
        Assert.Contains("F001-00000601", c.Markup);
        Assert.Equal("1,250.50", c.Find("[data-total='anticipos']").TextContent.Trim().Replace(" ", ""));
        Assert.Contains(f.Calls, x => x.U.EndsWith("/5/anticipos"));
        Assert.Contains(f.Calls, x => x.U.EndsWith("/5/anticipos/disponibles"));
    }

    [Fact]
    public void MuestraLaDescripcionDelAnticipo_EnGrilla()
    {
        var (c, _) = Dibuja();
        Assert.Contains("ADELANTO 30% SERVICIO", c.Find("[data-celda='descripcion']").TextContent);
        Assert.Contains("Descripción", c.Markup);
        var estados = c.FindAll("[data-celda='estado']").Select(x => x.TextContent.Trim()).ToList();
        Assert.Equal(new[] { "Aprobado", "Registrado" }, estados);     // el estado de cada factura de anticipo
        Assert.Contains("data-grilla=\"anticipos-aplicados\"", c.Markup);
    }

    [Fact]
    public async Task Aplicar_HaceElPost_RecargaYAvisaAlDocumento()
    {
        var (c, f) = Dibuja();
        var sel = c.FindComponents<MudBlazor.MudSelect<decimal?>>().First();
        await c.InvokeAsync(() => sel.Instance.ValueChanged.InvokeAsync(700m));
        await c.InvokeAsync(() => c.Find("[data-accion='anticipo-aplicar']").Click());
        c.WaitForState(() => f.Calls.Any(x => x.M == "POST"));
        Assert.EndsWith("/5/anticipos/700", f.Calls.Last(x => x.M == "POST").U);
        Assert.Equal(1, _cambios);
    }

    [Fact]
    public async Task Quitar_HaceElDelete_YAvisaAlDocumento()
    {
        var (c, f) = Dibuja();
        await c.InvokeAsync(() => c.FindAll("[data-accion='anticipo-quitar']")[0].Click());
        c.WaitForState(() => f.Calls.Any(x => x.M == "DELETE"));
        Assert.EndsWith("/5/anticipos/600", f.Calls.Last(x => x.M == "DELETE").U);
        Assert.Equal(1, _cambios);
    }

    [Fact]
    public async Task SiElServidorRechaza_NoAvisaAlDocumento()
    {
        var (c, f) = Dibuja(falla: true);
        var sel = c.FindComponents<MudBlazor.MudSelect<decimal?>>().First();
        await c.InvokeAsync(() => sel.Instance.ValueChanged.InvokeAsync(700m));
        await c.InvokeAsync(() => c.Find("[data-accion='anticipo-aplicar']").Click());
        c.WaitForState(() => f.Calls.Any(x => x.M == "POST"));
        Assert.Equal(0, _cambios);
    }

    [Fact]
    public void SoloLectura_SinBotonesNiSelector()
    {
        var (c, _) = Dibuja(soloLectura: true);
        Assert.Empty(c.FindAll("[data-accion='anticipo-quitar']"));
        Assert.Empty(c.FindAll("[data-accion='anticipo-aplicar']"));
    }

    [Fact]
    public void SinCabeceraGrabada_AvisaYNoConsulta()
    {
        var (c, f) = Dibuja(num: 0);
        Assert.Contains("sin-cabecera", c.Markup);
        Assert.Empty(f.Calls);
    }
}
