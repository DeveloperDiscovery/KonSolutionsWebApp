using System.Globalization;
using Bunit;
using KONSolutions.Web.Pages.Ingenieria;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using Xunit;

// Dialogo para dibujar formas que no son rectangulos (moldes y defectos de tela).
public class DisenosFormaTests : TestContext
{
    IDialogReference? _ref;
    BunitJSModuleInterop? _mod;

    async Task<IRenderedComponent<MudDialogProvider>> Abre(string modo = "molde", string contorno = "", bool conModulo = true)
    {
        Services.AddMudServices();
        if (conModulo) { JSInterop.Mode = JSRuntimeMode.Loose; _mod = JSInterop.SetupModule("./js/tizadoEditor.js"); _mod.Mode = JSRuntimeMode.Loose; }
        RenderComponent<MudPopoverProvider>(); RenderComponent<MudSnackbarProvider>();
        var p = RenderComponent<MudDialogProvider>();
        var ds = Services.GetRequiredService<IDialogService>();
        await p.InvokeAsync(async () => _ref = await ds.ShowAsync<DisenosFormaDialog>("t", new DialogParameters { ["Modo"] = modo, ["Contorno"] = contorno }, new DialogOptions { MaxWidth = MaxWidth.Large }));
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-zona='plantilla']")));
        return p;
    }

    void Pt(double x, double y) => _mod!.Setup<double[]>("puntoSvg", i => (double)i.Arguments[1] == x && (double)i.Arguments[2] == y).SetResult(new[] { x, y });
    static string Texto(IRenderedComponent<MudDialogProvider> p, string sel) => p.Find(sel).TextContent;
    static AngleSharp.Dom.IElement Btn(IRenderedComponent<MudDialogProvider> p, string a) => p.Find($"[data-accion='{a}']");
    static async Task Plantilla(IRenderedComponent<MudDialogProvider> p, string clave) => await p.InvokeAsync(async () => await p.FindComponent<MudSelect<string>>().Instance.ValueChanged.InvokeAsync(clave));
    IRenderedComponent<MudNumericField<double>> Num(IRenderedComponent<MudDialogProvider> p, string campo) => p.FindComponents<MudNumericField<double>>().First(c => c.Markup.Contains($"data-campo=\"{campo}\""));
    static int Filas(IRenderedComponent<MudDialogProvider> p) => p.FindAll("[data-tabla='puntos'] tr[data-fila]").Count;
    static string Cuenta(IRenderedComponent<MudDialogProvider> p) => Texto(p, "[data-chip='puntos']");
    async Task Click(IRenderedComponent<MudDialogProvider> p, double x, double y) { Pt(x, y); await p.Find("[data-zona='dibujo'] svg").PointerDownAsync(new PointerEventArgs { ClientX = x, ClientY = y, PointerId = 1 }); }
    static async Task<DisenosFormaDialog.Resultado?> Res(IDialogReference r) => (await r.Result)?.Data as DisenosFormaDialog.Resultado;

    [Fact]
    public async Task Vacio_PideDibujar_NoDejaAceptar_Y_ElDibujoNoTieneForma()
    {
        var p = await Abre();
        Assert.Contains("0 punto", Cuenta(p)); Assert.Contains("Dibuja el contorno", Texto(p, "[data-aviso='problema']"));
        Assert.True(Btn(p, "aceptar").HasAttribute("disabled")); Assert.Empty(p.FindAll("polygon[data-forma]"));
    }

    [Theory]
    [InlineData("rect", 4, "100 × 60")]
    [InlineData("trap", 4, "120 × 60")]
    [InlineData("tri", 3, "100 × 80")]
    [InlineData("para", 4, "125 × 60")]
    [InlineData("L", 6, "120 × 100")]
    [InlineData("elipse", 48, "100 × 70")]
    [InlineData("reg", 6, "86.6 × 100")]
    [InlineData("red", 36, "100 × 70")]
    public async Task CadaPlantilla_GeneraSusPuntos_ConLasMedidasEsperadas(string clave, int puntos, string medidas)
    {
        var p = await Abre();
        await Plantilla(p, clave);
        Assert.Contains($"{puntos} punto", Cuenta(p)); Assert.Equal(puntos, Filas(p)); Assert.Single(p.FindAll("polygon[data-forma]"));
        Assert.Contains(medidas, Texto(p, "[data-chip='medidas']").Replace(',', '.').Replace("  ", " "));
        Assert.False(Btn(p, "aceptar").HasAttribute("disabled"));
    }

    [Fact]
    public async Task CambiarUnaMedida_YGenerar_ReemplazaLosPuntos()
    {
        var p = await Abre();
        await Plantilla(p, "rect");
        await p.InvokeAsync(async () => await Num(p, "p1").Instance.ValueChanged.InvokeAsync(200));
        Assert.Contains("100 × 60", Texto(p, "[data-chip='medidas']"));         // todavia no se regenero
        Btn(p, "generar").Click();
        Assert.Contains("200 × 60", Texto(p, "[data-chip='medidas']"));
    }

    [Fact]
    public async Task MedidasInvalidas_MuestranElMotivo_YNoCambianLaForma()
    {
        var p = await Abre();
        await Plantilla(p, "L");
        await p.InvokeAsync(async () => await Num(p, "p3").Instance.ValueChanged.InvokeAsync(500));      // recorte mas ancho que la forma
        Btn(p, "generar").Click();
        Assert.Contains("recorte", Texto(p, "[data-error]")); Assert.Equal(6, Filas(p));
        await Plantilla(p, "rect");
        await p.InvokeAsync(async () => await Num(p, "p1").Instance.ValueChanged.InvokeAsync(0));
        Btn(p, "generar").Click();
        Assert.Contains("mayores que cero", Texto(p, "[data-error]"));
    }

    [Fact]
    public async Task TrianguloConVerticeIndicado_YAceptar_DevuelveLosPuntos()
    {
        var p = await Abre();
        await Plantilla(p, "tri");
        p.Find("[data-campo='vertice']").Change("100");
        Btn(p, "generar").Click();
        Btn(p, "aceptar").Click();
        Assert.Equal("0,0 100,0 100,80", (await Res(_ref!))!.Contorno);
    }

    [Fact]
    public async Task ClicEnElDibujo_AgregaPuntosAjustadosALaRejilla_YConTresYaHayForma()
    {
        var p = await Abre();
        await Click(p, 23, 31); await Click(p, 148, 33); await Click(p, 80, 97);
        Assert.Equal(3, Filas(p)); Assert.Single(p.FindAll("polygon[data-forma]"));
        var x = p.FindAll("[data-tabla='puntos'] input[data-campo='x']").Select(e => e.GetAttribute("value")).ToList();
        var y = p.FindAll("[data-tabla='puntos'] input[data-campo='y']").Select(e => e.GetAttribute("value")).ToList();
        Assert.Equal(new[] { "25", "150", "80" }, x); Assert.Equal(new[] { "30", "35", "95" }, y);
        Btn(p, "aceptar").Click();
        Assert.Equal("25,30 150,35 80,95", (await Res(_ref!))!.Contorno);
    }

    [Fact]
    public async Task SinRejilla_LosPuntosSeRedondeanADosDecimales()
    {
        var p = await Abre();
        await p.InvokeAsync(async () => await Num(p, "rejilla").Instance.ValueChanged.InvokeAsync(0));
        await Click(p, 12.3456, 7.891);
        Assert.Equal("12.35", p.Find("[data-tabla='puntos'] input[data-campo='x']").GetAttribute("value"));
        Assert.Equal("7.89", p.Find("[data-tabla='puntos'] input[data-campo='y']").GetAttribute("value"));
    }

    [Fact]
    public async Task ArrastrarUnPunto_LoMueve_AjustadoALaRejilla()
    {
        var p = await Abre();
        await Plantilla(p, "rect");                                           // (0,0) (100,0) (100,60) (0,60)
        Pt(100, 0); Pt(162, 38);
        await p.Find("circle[data-punto='2']").PointerDownAsync(new PointerEventArgs { ClientX = 100, ClientY = 0, PointerId = 1 });
        await p.Find("[data-zona='dibujo'] svg").PointerMoveAsync(new PointerEventArgs { ClientX = 162, ClientY = 38, PointerId = 1 });
        await p.Find("[data-zona='dibujo'] svg").PointerUpAsync(new PointerEventArgs { ClientX = 162, ClientY = 38, PointerId = 1 });
        Assert.Equal("160", p.FindAll("[data-tabla='puntos'] input[data-campo='x']")[1].GetAttribute("value"));
        Assert.Equal("40", p.FindAll("[data-tabla='puntos'] input[data-campo='y']")[1].GetAttribute("value"));
        Assert.Equal(4, Filas(p));                                            // arrastrar no agrega puntos
        // ya soltado: mover el puntero no lo mueve mas
        Pt(300, 300);
        await p.Find("[data-zona='dibujo'] svg").PointerMoveAsync(new PointerEventArgs { ClientX = 300, ClientY = 300, PointerId = 1 });
        Assert.Equal("160", p.FindAll("[data-tabla='puntos'] input[data-campo='x']")[1].GetAttribute("value"));
    }

    [Fact]
    public async Task LaTabla_PermiteEditarInsertarYQuitarPuntos()
    {
        var p = await Abre();
        await Plantilla(p, "rect");
        await p.InvokeAsync(async () => await p.FindComponents<MudNumericField<double>>().Where(c => c.Markup.Contains("data-campo=\"x\"")).ElementAt(2).Instance.ValueChanged.InvokeAsync(130));
        Assert.Equal("130", p.FindAll("[data-tabla='puntos'] input[data-campo='x']")[2].GetAttribute("value"));
        p.FindAll("[data-accion='insertar']")[0].Click();                      // entre el punto 1 y el 2: (50,0)
        Assert.Equal(5, Filas(p)); Assert.Equal("50", p.FindAll("[data-tabla='puntos'] input[data-campo='x']")[1].GetAttribute("value"));
        p.FindAll("[data-accion='insertar']")[4].Click();                      // el ultimo se une con el primero: (0, 30)
        Assert.Equal(6, Filas(p)); Assert.Equal("30", p.FindAll("[data-tabla='puntos'] input[data-campo='y']")[5].GetAttribute("value"));
        p.FindAll("[data-accion='quitar-punto']")[1].Click(); p.FindAll("[data-accion='quitar-punto']")[0].Click();
        Assert.Equal(4, Filas(p));
        p.FindAll("[data-accion='quitar-punto']")[0].Click(); p.FindAll("[data-accion='quitar-punto']")[0].Click();
        Assert.Equal(2, Filas(p)); Assert.Contains("al menos 3", Texto(p, "[data-aviso='problema']")); Assert.True(Btn(p, "aceptar").HasAttribute("disabled"));
    }

    [Fact]
    public async Task QuitarUnPunto_DejaSinSeleccion()
    {
        var p = await Abre();
        await Plantilla(p, "rect");
        await p.InvokeAsync(() => p.Find("[data-tabla='puntos'] tr[data-fila='2'] td").Click());           // selecciona el punto 2
        Assert.Contains("rgba(214", p.Find("[data-tabla='puntos'] tr[data-fila='2']").GetAttribute("style")!);
        p.FindAll("[data-accion='quitar-punto']")[1].Click();
        Assert.DoesNotContain(p.FindAll("[data-tabla='puntos'] tr[data-fila]"), f => (f.GetAttribute("style") ?? "").Contains("rgba(214"));
    }

    [Fact]
    public async Task UnContornoQueSeCruza_SeAvisa_YNoSePuedeAceptar()
    {
        var p = await Abre();
        p.Find("[data-campo='texto']").Change("0,0 100,100 100,0 0,100");
        Btn(p, "usar-texto").Click();
        Assert.Equal(4, Filas(p)); Assert.Contains("se cruza", Texto(p, "[data-aviso='problema']")); Assert.True(Btn(p, "aceptar").HasAttribute("disabled"));
    }

    [Fact]
    public async Task PegarTexto_ValidoReemplaza_InvalidoOVacioAvisan()
    {
        var p = await Abre();
        p.Find("[data-campo='texto']").Change("0,0 80,0 40,60");
        Btn(p, "usar-texto").Click();
        Assert.Equal(3, Filas(p)); Assert.Empty(p.FindAll("[data-error]"));
        p.Find("[data-campo='texto']").Change("0,0 abc");
        Btn(p, "usar-texto").Click();
        Assert.Contains("no es válido", Texto(p, "[data-error]")); Assert.Equal(3, Filas(p));
        p.Find("[data-campo='texto']").Change("  ");
        Btn(p, "usar-texto").Click();
        Assert.Contains("Escribe", Texto(p, "[data-error]"));
        Btn(p, "ver-texto").Click();
        Assert.Equal("0,0 80,0 40,60", p.Find("textarea[data-campo='texto']").TextContent.Trim());
    }

    [Fact]
    public async Task ContornoDePartida_SeCarga_YBorrarTodoLoLimpia()
    {
        var p = await Abre(contorno: "0,0 50,0 50,20 0,20");
        Assert.Equal(4, Filas(p)); Assert.Contains("50 × 20", Texto(p, "[data-chip='medidas']"));
        Btn(p, "limpiar").Click();
        Assert.Equal(0, Filas(p)); Assert.True(Btn(p, "aceptar").HasAttribute("disabled"));
    }

    [Fact]
    public async Task ModoDefecto_DesplazaLaFormaGeneradaALaPosicionDeLaTela()
    {
        var p = await Abre("defecto");
        await p.InvokeAsync(async () => { await Num(p, "desde-x").Instance.ValueChanged.InvokeAsync(300); await Num(p, "desde-y").Instance.ValueChanged.InvokeAsync(120); });
        await Plantilla(p, "rect");
        Btn(p, "aceptar").Click();
        Assert.Equal("300,120 400,120 400,180 300,180", (await Res(_ref!))!.Contorno);
    }

    [Fact]
    public async Task ModoMolde_NoOfreceLaPosicionEnLaTela()
    {
        var p = await Abre();
        Assert.Empty(p.FindAll("[data-campo='desde-x']"));
    }

    [Fact]
    public async Task Cancelar_NoDevuelveNada()
    {
        var p = await Abre();
        await Plantilla(p, "rect");
        Btn(p, "cancelar").Click();
        Assert.True((await _ref!.Result)!.Canceled);
    }

    [Fact]
    public async Task SiElDibujoConElPunteroFalla_AvisaYSePuedeSeguirConLaTabla()
    {
        var p = await Abre();
        _mod!.Setup<double[]>("puntoSvg", _ => true).SetException(new InvalidOperationException("sin js"));
        await p.Find("[data-zona='dibujo'] svg").PointerDownAsync(new PointerEventArgs { ClientX = 10, ClientY = 10, PointerId = 1 });
        p.WaitForAssertion(() => Assert.Contains("no está disponible", Texto(p, "[data-error]")));
        await Plantilla(p, "rect");                                           // las plantillas y la tabla siguen funcionando
        Assert.Equal(4, Filas(p));
    }

    [Fact]
    public async Task LaRejillaMuyFina_NoDibujaMilesDeLineas()
    {
        var p = await Abre();
        await p.InvokeAsync(async () => await Num(p, "rejilla").Instance.ValueChanged.InvokeAsync(1));
        Assert.InRange(p.FindAll("line[data-rejilla]").Count, 1, 160);
    }
}
