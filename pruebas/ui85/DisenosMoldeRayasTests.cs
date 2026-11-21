using Bunit;
using KONSolutions.Web.Pages.Ingenieria;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using Xunit;

// Dialogo de coincidencia de rayas de un molde (script SQL 118).
public class DisenosMoldeRayasTests : TestContext
{
    IDialogReference? _ref;
    const string Cont = "0,0 100,0 100,50 0,50";

    async Task<IRenderedComponent<MudDialogProvider>> Abre(string contorno = Cont, bool x = false, bool y = false, double? px = null, double? py = null, double fx = 0, double fy = 0)
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
        RenderComponent<MudPopoverProvider>(); RenderComponent<MudSnackbarProvider>();
        var p = RenderComponent<MudDialogProvider>();
        var ds = Services.GetRequiredService<IDialogService>();
        await p.InvokeAsync(async () => _ref = await ds.ShowAsync<DisenosMoldeRayasDialog>("t",
            new DialogParameters { ["NombreMolde"] = "Frente", ["Contorno"] = contorno, ["CoincideX"] = x, ["CoincideY"] = y, ["PuntoX"] = px, ["PuntoY"] = py, ["FaseX"] = fx, ["FaseY"] = fy }));
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-chip='medidas']")));
        return p;
    }

    static string Texto(IRenderedComponent<MudDialogProvider> p, string sel) => p.Find(sel).TextContent;
    static async Task<DisenosMoldeRayasDialog.Resultado?> Res(IDialogReference r) => (await r.Result)?.Data as DisenosMoldeRayasDialog.Resultado;

    [Fact]
    public async Task AlAbrir_MuestraLasMedidasYCarga_LosValoresRecibidos()
    {
        var p = await Abre(x: true, px: 20, py: 10, fx: 5);
        Assert.Contains("100", p.Find("[data-chip='medidas']").TextContent);
        Assert.Equal("20", p.Find("[data-campo='punto-x']").GetAttribute("value"));
        Assert.Equal("10", p.Find("[data-campo='punto-y']").GetAttribute("value"));
        Assert.Equal("5", p.Find("[data-campo='fase-x']").GetAttribute("value"));
        Assert.Single(p.FindAll("[data-zona='vista'] svg circle[data-punto]"));
        Assert.Single(p.FindAll("line[data-eje='x']")); Assert.Empty(p.FindAll("line[data-eje='y']"));
    }

    [Fact]
    public async Task ActivarCoincidenciaSinPunto_ParteDelCentroDeLaCaja()
    {
        var p = await Abre();
        Assert.Empty(p.FindAll("circle[data-punto]"));
        p.Find("[data-campo='coincide-y']").Change(true);
        p.WaitForAssertion(() => Assert.Equal("25", p.Find("[data-campo='punto-y']").GetAttribute("value")));
        Assert.Equal("50", p.Find("[data-campo='punto-x']").GetAttribute("value"));
        Assert.Single(p.FindAll("line[data-eje='y']"));
    }

    [Fact]
    public async Task Aceptar_DevuelveTodoLoEditado()
    {
        var p = await Abre(x: true, y: true, px: 10, py: 20, fx: 1, fy: 2);
        p.Find("[data-campo='fase-y']").Change("-7.5");
        p.Find("[data-accion='aceptar']").Click();
        var r = await Res(_ref!);
        Assert.Equal(new DisenosMoldeRayasDialog.Resultado(true, true, 10, 20, 1, -7.5), r);
    }

    [Fact]
    public async Task Aceptar_ConPuntoFueraDeLaCaja_AvisaYNoCierra()
    {
        var p = await Abre(x: true, px: 150, py: 10);
        p.Find("[data-accion='aceptar']").Click();
        Assert.Contains("dentro de la caja", p.Find("[data-error]").TextContent);
        Assert.False(_ref!.Result.IsCompleted);
    }

    [Fact]
    public async Task Aceptar_PuntoEnElBorde_EsValido()
    {
        var p = await Abre(x: true, px: 100, py: 0);
        p.Find("[data-accion='aceptar']").Click();
        Assert.NotNull(await Res(_ref!));
    }

    [Fact]
    public async Task Aceptar_PuntoNegativo_Rechaza()
    {
        var p = await Abre(x: true, px: 10, py: -1);
        p.Find("[data-accion='aceptar']").Click();
        Assert.Contains("dentro de la caja", p.Find("[data-error]").TextContent);
    }

    [Fact]
    public async Task Aceptar_FaseFueraDeRango_Rechaza()
    {
        var p = await Abre(x: true, px: 10, py: 10);
        p.Find("[data-campo='fase-x']").Change("100001");
        p.Find("[data-accion='aceptar']").Click();
        Assert.Contains("fase", p.Find("[data-error]").TextContent);
    }

    [Fact]
    public async Task SinCoincidencia_SePuedeAceptarSinPunto()
    {
        var p = await Abre();
        p.Find("[data-accion='aceptar']").Click();
        var r = await Res(_ref!);
        Assert.Equal(new DisenosMoldeRayasDialog.Resultado(false, false, null, null, 0, 0), r);
    }

    [Fact]
    public async Task Cancelar_NoDevuelveNada()
    {
        var p = await Abre(x: true, px: 10, py: 10);
        p.Find("[data-accion='cancelar']").Click();
        Assert.True((await _ref!.Result)!.Canceled);
    }

    [Fact]
    public async Task ContornoInvalido_Avisa_YNoPermiteAceptar()
    {
        var p = await Abre("0,0 5,5");
        Assert.Contains("contorno", p.Find("[data-error]").TextContent);
        Assert.Empty(p.FindAll("[data-zona='vista']"));
        p.Find("[data-accion='aceptar']").Click();
        Assert.False(_ref!.Result.IsCompleted);
    }

    [Fact]
    public async Task ElBotonCentro_PoneElPuntoEnElCentro()
    {
        var p = await Abre(x: true, px: 1, py: 1);
        p.Find("[data-accion='centro']").Click();
        Assert.Equal("50", p.Find("[data-campo='punto-x']").GetAttribute("value")); Assert.Equal("25", p.Find("[data-campo='punto-y']").GetAttribute("value"));
    }

    [Fact]
    public async Task ActivarYBorrarElPunto_AlAceptar_PideElPunto()
    {
        var p = await Abre();
        p.Find("[data-campo='coincide-x']").Change(true);
        p.WaitForAssertion(() => Assert.Equal("50", p.Find("[data-campo='punto-x']").GetAttribute("value")));
        p.Find("[data-campo='punto-x']").Change("");
        p.Find("[data-accion='aceptar']").Click();
        Assert.Contains("indica el punto", Texto(p, "[data-error]")); Assert.False(_ref!.Result.IsCompleted);
    }

    [Theory]
    [InlineData(10, 60, "dentro de la caja")]          // Y pasa del ancho de la caja (50)
    [InlineData(-1, 10, "dentro de la caja")]          // X negativa
    [InlineData(101, 10, "dentro de la caja")]         // X pasa del largo (100)
    public async Task PuntoFueraDeLaCaja_Rechaza(double px, double py, string mensaje)
    {
        var p = await Abre(x: true, px: px, py: py);
        p.Find("[data-accion='aceptar']").Click();
        Assert.Contains(mensaje, Texto(p, "[data-error]")); Assert.False(_ref!.Result.IsCompleted);
    }

    [Fact]
    public async Task FaseEnYFueraDeRango_Rechaza()
    {
        var p = await Abre(y: true, px: 10, py: 10);
        p.Find("[data-campo='fase-y']").Change("100001");
        p.Find("[data-accion='aceptar']").Click();
        Assert.Contains("fase", Texto(p, "[data-error]"));
    }

    [Fact]
    public async Task LasLineasGuiaSoloSeDibujanEnLosSentidosActivados()
    {
        var p = await Abre(y: true, px: 10, py: 10);
        Assert.Empty(p.FindAll("line[data-eje='x']")); Assert.Single(p.FindAll("line[data-eje='y']"));
    }

    [Fact]
    public async Task ElPuntoSeDibujaRelativoALaCajaDelMolde_NoALaOrigenDelPlano()
    {
        var p = await Abre(contorno: "100,200 150,200 150,230 100,230", x: true, px: 10, py: 10);
        var c = p.Find("circle[data-punto]");
        Assert.Equal("110", c.GetAttribute("cx")); Assert.Equal("210", c.GetAttribute("cy"));
    }
}
