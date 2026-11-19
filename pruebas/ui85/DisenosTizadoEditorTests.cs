using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Bunit;
using KONSolutions.Shared.Models.Ingenieria;
using KONSolutions.Web.Pages.Ingenieria;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using Xunit;

// Ajuste manual de un tizado guardado (etapa G). La API se simula con FakeAjuste; el arrastre se prueba con un modulo JS simulado que
// devuelve el punto que el test pone en ClientX / ClientY.
public class FakeAjuste : HttpMessageHandler
{
    public List<(string M, string U, JsonElement? B)> Calls = new();
    public DisenosTizadoCalculo Calculo = Base();
    public string? Falla;
    public string? FallaLectura;
    public int NuevoNumero = 7;

    static string R(double x, double y, double w, double h) => string.Join(" ", new[] { (x, y), (x + w, y), (x + w, y + h), (x, y + h) }.Select(p => p.Item1.ToString("0.###", CultureInfo.InvariantCulture) + "," + p.Item2.ToString("0.###", CultureInfo.InvariantCulture)));

    public static DisenosTizadoPieza Pz(string id, string nombre, double x, double y, double w, double h, string talla = "M", int copia = 1, string? punto = null, double? fx = null) =>
        new() { PiezaId = id, Nombre = nombre, Talla = talla, Copia = copia, Puntos = R(x, y, w, h), Coincidencia = punto, FaseX = fx, ErrorX = fx is null ? null : 0 };

    /// <summary>Tela de 500 de ancho, separacion 4, margen 10: A en (10,10) de 100 x 50 y B en (130,10) de 100 x 50; largo 240.</summary>
    public static DisenosTizadoCalculo Base() => new()
    {
        NUM_SECUENCIA = 3, VAL_ANCHO_TELA = 500, VAL_LARGO = 240, VAL_EFICIENCIA = 0.1, VAL_CONSUMO_PRENDA = 0.12, NUM_PRENDAS = 2, NUM_PIEZAS = 2,
        Geometria = new DisenosTizadoGeometria
        {
            Ancho = 500, Largo = 240, Separacion = 4, Margen = 10,
            Colocadas = new() { Pz("1", "Cuerpo", 10, 10, 100, 50), Pz("2", "Manga", 130, 10, 100, 50, "L") }
        }
    };

    public JsonElement Cuerpo() => Calls.Last(c => c.M == "POST").B!.Value;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage r, CancellationToken ct)
    {
        var u = r.RequestUri!.PathAndQuery;
        JsonElement? body = r.Content is null ? null : JsonDocument.Parse(await r.Content.ReadAsStringAsync(ct)).RootElement.Clone();
        Calls.Add((r.Method.Method, u, body));
        HttpResponseMessage Ok(object? d) => new(HttpStatusCode.OK) { Content = JsonContent.Create(new { Success = true, Data = d }) };
        HttpResponseMessage Mal(string m) => new(HttpStatusCode.BadRequest) { Content = JsonContent.Create(new { Success = false, Message = m }) };
        if (r.Method == HttpMethod.Get) return FallaLectura is not null ? Mal(FallaLectura) : Ok(Calculo);
        if (Falla is not null) return Mal(Falla);
        if (u.EndsWith("/tizados/ajustar")) return Ok(new DisenosTizadoCalculo { NUM_SECUENCIA = NuevoNumero, VAL_LARGO = 1, Geometria = new() });
        return Ok(true);
    }
}

public class DisenosTizadoEditorTests : TestContext
{
    readonly FakeAjuste _f = new();
    IDialogReference? _ref;
    BunitJSModuleInterop? _mod;

    async Task<IRenderedComponent<MudDialogProvider>> Abre()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
        _mod = JSInterop.SetupModule("./js/tizadoEditor.js"); _mod.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton(new HttpClient(_f) { BaseAddress = new Uri("http://x/") });
        RenderComponent<MudPopoverProvider>(); RenderComponent<MudSnackbarProvider>();
        var p = RenderComponent<MudDialogProvider>();
        var ds = Services.GetRequiredService<IDialogService>();
        await p.InvokeAsync(async () => _ref = await ds.ShowAsync<DisenosTizadoEditorDialog>("t",
            new DialogParameters { ["CodDiseno"] = "D1", ["NumVersion"] = 3, ["NumSecuencia"] = 3, ["NombreTizado"] = "Pedido 1" }, new DialogOptions { MaxWidth = MaxWidth.False }));
        p.WaitForAssertion(() => Assert.True(p.FindAll("[data-visor]").Count > 0 || p.FindAll("[data-error]").Count > 0));
        return p;
    }

    void Pt(double x, double y) => _mod!.Setup<double[]>("puntoSvg", i => (double)i.Arguments[1] == x && (double)i.Arguments[2] == y).SetResult(new[] { x, y });

    static string Texto(IRenderedComponent<MudDialogProvider> p, string sel) => p.Find(sel).TextContent;
    static AngleSharp.Dom.IElement Btn(IRenderedComponent<MudDialogProvider> p, string a) => p.Find($"[data-accion='{a}']");
    IRenderedComponent<MudNumericField<double>> Num(IRenderedComponent<MudDialogProvider> p, string campo) => p.FindComponents<MudNumericField<double>>().First(c => c.Markup.Contains($"data-campo=\"{campo}\""));
    static async Task Selecciona(IRenderedComponent<MudDialogProvider> p, int i) { await p.Find($"[data-pieza='{i}']").PointerDownAsync(new PointerEventArgs { ClientX = 1000 + i, ClientY = 0, PointerId = 1 }); }
    static string Puntos(IRenderedComponent<MudDialogProvider> p, int i) => p.Find($"[data-pieza='{i}']").GetAttribute("points")!;
    static (double MinX, double MinY, double MaxX, double MaxY) Caja(string pts) { var v = pts.Split(' ').Select(t => t.Split(',').Select(x => double.Parse(x, CultureInfo.InvariantCulture)).ToArray()).ToList(); return (v.Min(a => a[0]), v.Min(a => a[1]), v.Max(a => a[0]), v.Max(a => a[1])); }

    async Task Arrastra(IRenderedComponent<MudDialogProvider> p, int pieza, (double x, double y) desde, (double x, double y) hasta)
    {
        Pt(desde.x, desde.y); Pt(hasta.x, hasta.y);
        await p.Find($"[data-pieza='{pieza}']").PointerDownAsync(new PointerEventArgs { ClientX = desde.x, ClientY = desde.y, PointerId = 1 });
        await p.Find("[data-visor] svg").PointerMoveAsync(new PointerEventArgs { ClientX = hasta.x, ClientY = hasta.y, PointerId = 1 });
        await p.Find("[data-visor] svg").PointerUpAsync(new PointerEventArgs { ClientX = hasta.x, ClientY = hasta.y, PointerId = 1 });
    }

    // =================== carga ===================
    [Fact]
    public async Task AlAbrir_LeeElTizadoYMuestraSusPiezasMedidasYSinProblemas()
    {
        var p = await Abre();
        Assert.Contains(_f.Calls, c => c.M == "GET" && c.U.EndsWith("/tizados/D1/3/3"));
        Assert.Equal(2, p.FindAll("polygon[data-pieza]").Count);
        Assert.Contains("0.24 m", Texto(p, "[data-chip='largo']").Replace(',', '.'));
        Assert.Contains("Sin problemas", Texto(p, "[data-chip='estado']"));
        Assert.Contains("0.120", Texto(p, "[data-chip='consumo']").Replace(',', '.'));
        Assert.Equal("Pedido 1 (ajustado)", p.Find("[data-campo='nombre-ajuste']").GetAttribute("value"));
        Assert.True(Btn(p, "guardar").HasAttribute("disabled"));                       // sin cambios no se guarda
        Assert.NotEmpty(p.FindAll("[data-aviso='sin-seleccion']"));
    }

    [Fact]
    public async Task SiLaLecturaFalla_MuestraElMotivo_YNoHayVisor()
    {
        _f.FallaLectura = "El tizado no existe.";
        var p = await Abre();
        Assert.Contains("no existe", Texto(p, "[data-error]")); Assert.Empty(p.FindAll("[data-visor]"));
    }

    [Fact]
    public async Task UnaPiezaConContornoInvalido_AvisaYNoAbreElEditor()
    {
        _f.Calculo.Geometria.Colocadas[0].Puntos = "0,0 5,5";
        var p = await Abre();
        Assert.Contains("contorno", Texto(p, "[data-error]")); Assert.Empty(p.FindAll("[data-zona='panel']"));
    }

    [Fact]
    public async Task DibujaLasRayasLosDefectosYLosPuntosDeCoincidencia()
    {
        _f.Calculo.Geometria.PeriodoX = 100; _f.Calculo.Geometria.PeriodoY = 250;
        _f.Calculo.Geometria.Defectos = new() { new DisenosTizadoDefecto { Nombre = "Mancha", Puntos = "300,100 340,100 340,160 300,160" } };
        _f.Calculo.Geometria.Colocadas[0].Coincidencia = "20,20";
        var p = await Abre();
        Assert.NotEmpty(p.FindAll("line[data-raya='x']")); Assert.NotEmpty(p.FindAll("line[data-raya='y']"));
        Assert.Single(p.FindAll("polygon[data-defecto='Mancha']")); Assert.Single(p.FindAll("circle[data-coincide]"));
    }

    // =================== seleccion y movimientos ===================
    [Fact]
    public async Task TocarUnaPieza_LaSeleccionaYMuestraSuPosicion()
    {
        var p = await Abre();
        await Selecciona(p, 1);
        Assert.Contains("Manga", Texto(p, "[data-sel]")); Assert.Contains("#1", Texto(p, "[data-sel]"));
        Assert.Equal("130", p.Find("input[data-campo='x']").GetAttribute("value")); Assert.Equal("10", p.Find("input[data-campo='y']").GetAttribute("value"));
    }

    [Fact]
    public async Task LasFlechas_MuevenElPasoIndicado_YDeshacerRestablece()
    {
        var p = await Abre();
        await Selecciona(p, 0);
        await p.InvokeAsync(async () => await Num(p, "paso").Instance.ValueChanged.InvokeAsync(7));
        Btn(p, "der").Click(); Btn(p, "abajo").Click();
        Assert.Equal((17, 17, 117, 67), Caja(Puntos(p, 0)));
        Btn(p, "izq").Click(); Btn(p, "arriba").Click();
        Assert.Equal((10, 10, 110, 60), Caja(Puntos(p, 0)));
        Assert.True(Btn(p, "restablecer").HasAttribute("disabled"));
        Btn(p, "der").Click();
        Btn(p, "deshacer").Click();
        Assert.Equal((10, 10, 110, 60), Caja(Puntos(p, 0)));
        Btn(p, "der").Click(); Btn(p, "der").Click();
        Btn(p, "restablecer").Click();
        Assert.Equal((10, 10, 110, 60), Caja(Puntos(p, 0)));
    }

    [Fact]
    public async Task LasCoordenadas_UbicanLaEsquinaDeLaPieza()
    {
        var p = await Abre();
        await Selecciona(p, 0);
        await p.InvokeAsync(async () => { await Num(p, "x").Instance.ValueChanged.InvokeAsync(300); await Num(p, "y").Instance.ValueChanged.InvokeAsync(200); });
        Assert.Equal((300, 200, 400, 250), Caja(Puntos(p, 0)));
        Assert.Equal("200", p.Find("input[data-campo='y']").GetAttribute("value"));
    }

    [Fact]
    public async Task Girar90YEspejar_ActualizanLaPiezaElAnguloYLasMedidas()
    {
        var p = await Abre();
        await Selecciona(p, 0);
        Btn(p, "girar-90").Click();
        var c = Caja(Puntos(p, 0)); Assert.Equal(50, c.MaxX - c.MinX, 3); Assert.Equal(100, c.MaxY - c.MinY, 3);
        Assert.Contains("90", Texto(p, "[data-angulo]"));
        Btn(p, "girar-90m").Click(); Btn(p, "girar-180").Click();
        Assert.Contains("180", Texto(p, "[data-angulo]"));
        Btn(p, "espejar").Click();
        Assert.Contains("(espejo)", Texto(p, "[data-sel]"));
        await p.InvokeAsync(async () => await Num(p, "giro").Instance.ValueChanged.InvokeAsync(30));
        Btn(p, "girar-libre").Click();
        Assert.Contains("210", Texto(p, "[data-angulo]"));
    }

    [Fact]
    public async Task SinSeleccion_LosBotonesDeAjusteNoExisten()
    {
        var p = await Abre();
        Assert.Empty(p.FindAll("[data-accion='der']")); Assert.Empty(p.FindAll("[data-accion='espejar']"));
    }

    // =================== arrastre ===================
    [Fact]
    public async Task Arrastrar_MueveLaPiezaLoQueSeMovioElPuntero_YQuedaParaDeshacer()
    {
        var p = await Abre();
        await Arrastra(p, 0, (50, 30), (80.04, 50.06));
        var c = Caja(Puntos(p, 0));
        Assert.Equal(40.0, c.MinX, 6); Assert.Equal(30.1, c.MinY, 6);                  // (10 + 30.04, 10 + 20.06) redondeado a 0,1 mm
        Assert.Contains("Cuerpo", Texto(p, "[data-sel]"));
        Btn(p, "deshacer").Click();
        Assert.Equal((10, 10, 110, 60), Caja(Puntos(p, 0)));
    }

    [Fact]
    public async Task ArrastrarUnaPiezaEncimaDeOtra_AlSoltar_MarcaElError()
    {
        var p = await Abre();
        await Arrastra(p, 0, (50, 30), (150, 30));                    // la pieza 0 (10..110) pasa a 110..210 y pisa a la 1 (130..230)
        Assert.Contains("1 error", Texto(p, "[data-chip='estado']")); Assert.NotEmpty(p.FindAll("[data-problema='solape']"));
        Assert.True(Btn(p, "guardar").HasAttribute("disabled"));
    }

    [Fact]
    public async Task ElDibujoDejaEspacioLibreALaDerecha_ParaPonerPiezasMasAdelante()
    {
        var p = await Abre();
        var vb = p.Find("[data-visor] svg").GetAttribute("viewBox")!.Split(' ');
        Assert.Equal(540, double.Parse(vb[2], CultureInfo.InvariantCulture), 3);                // largo 240 + 300 de espacio libre
        Assert.Equal(500, double.Parse(vb[3], CultureInfo.InvariantCulture), 3);
        await Selecciona(p, 1);
        await p.InvokeAsync(async () => await Num(p, "x").Instance.ValueChanged.InvokeAsync(900));
        Assert.True(double.Parse(p.Find("[data-visor] svg").GetAttribute("viewBox")!.Split(' ')[2], CultureInfo.InvariantCulture) >= 1310);
    }

    [Fact]
    public async Task TocarSinArrastrar_SoloSeleccionaYNoAgregaPasosAlHistorial()
    {
        var p = await Abre();
        Pt(50, 30);
        await p.Find("[data-pieza='0']").PointerDownAsync(new PointerEventArgs { ClientX = 50, ClientY = 30, PointerId = 1 });
        await p.Find("[data-visor] svg").PointerUpAsync(new PointerEventArgs { ClientX = 50, ClientY = 30, PointerId = 1 });
        Assert.True(Btn(p, "deshacer").HasAttribute("disabled")); Assert.Equal((10, 10, 110, 60), Caja(Puntos(p, 0)));
    }

    [Fact]
    public async Task MoverElPunteroSinTenerUnaPiezaTomada_NoHaceNada()
    {
        var p = await Abre();
        Pt(300, 300);
        await p.Find("[data-visor] svg").PointerMoveAsync(new PointerEventArgs { ClientX = 300, ClientY = 300, PointerId = 1 });
        Assert.Equal((10, 10, 110, 60), Caja(Puntos(p, 0))); Assert.Equal((130, 10, 230, 60), Caja(Puntos(p, 1)));
    }

    // =================== validacion ===================
    [Fact]
    public async Task UnaPiezaEncimaDeOtra_EsUnErrorQueMarcaLasDosYImpideGuardar()
    {
        var p = await Abre();
        await Selecciona(p, 0);
        await p.InvokeAsync(async () => await Num(p, "x").Instance.ValueChanged.InvokeAsync(100));       // la pieza 0 ahora pisa a la 1 (130..230)
        Assert.Contains("1 error", Texto(p, "[data-chip='estado']"));
        var li = p.Find("[data-problema='solape']"); Assert.Equal("Error", li.GetAttribute("data-gravedad")); Assert.Contains("Cuerpo", li.TextContent); Assert.Contains("Manga", li.TextContent);
        Assert.Equal("#d62728", p.Find("[data-pieza='0']").GetAttribute("stroke")); Assert.Equal("#d62728", p.Find("[data-pieza='1']").GetAttribute("stroke"));
        Assert.True(Btn(p, "guardar").HasAttribute("disabled")); Assert.NotEmpty(p.FindAll("[data-aviso='no-guardar']"));
        await p.InvokeAsync(async () => await Num(p, "x").Instance.ValueChanged.InvokeAsync(10));
        Assert.Contains("Sin problemas", Texto(p, "[data-chip='estado']")); Assert.Empty(p.FindAll("[data-aviso='no-guardar']"));
    }

    [Fact]
    public async Task SalirseDeLaTela_EsUnError()
    {
        var p = await Abre();
        await Selecciona(p, 0);
        await p.InvokeAsync(async () => await Num(p, "y").Instance.ValueChanged.InvokeAsync(480));
        Assert.NotEmpty(p.FindAll("[data-problema='fuera']")); Assert.Contains("1 error", Texto(p, "[data-chip='estado']"));
    }

    [Fact]
    public async Task MenosDeLaSeparacion_EsUnAviso_ConNaranja_YSeSigueGuardando()
    {
        var p = await Abre();
        await Selecciona(p, 0);
        await p.InvokeAsync(async () => await Num(p, "x").Instance.ValueChanged.InvokeAsync(28));       // termina en 128; la otra empieza en 130: 2 mm de aire
        Assert.Contains("1 aviso", Texto(p, "[data-chip='estado']"));
        Assert.Equal("Aviso", p.Find("[data-problema='separacion']").GetAttribute("data-gravedad"));
        Assert.Equal("#ff7f0e", p.Find("[data-pieza='0']").GetAttribute("stroke"));
        Assert.False(Btn(p, "guardar").HasAttribute("disabled"));
    }

    [Fact]
    public async Task SobreUnDefecto_EsUnError()
    {
        _f.Calculo.Geometria.Defectos = new() { new DisenosTizadoDefecto { Nombre = "Mancha", Puntos = "300,100 400,100 400,200 300,200" } };
        var p = await Abre();
        await Selecciona(p, 0);
        await p.InvokeAsync(async () => { await Num(p, "x").Instance.ValueChanged.InvokeAsync(320); await Num(p, "y").Instance.ValueChanged.InvokeAsync(120); });
        Assert.Contains("Mancha", Texto(p, "[data-problema='defecto']"));
    }

    [Fact]
    public async Task SalirseDeSuRaya_EsUnAvisoConElDesvio()
    {
        _f.Calculo.Geometria.PeriodoX = 100;
        _f.Calculo.Geometria.Colocadas[0] = FakeAjuste.Pz("1", "Cuerpo", 10, 10, 100, 50, punto: "10,20", fx: 10);      // 10 - 10 = 0: justo sobre la raya
        var p = await Abre();
        Assert.Contains("Sin problemas", Texto(p, "[data-chip='estado']"));
        await Selecciona(p, 0);
        await p.InvokeAsync(async () => await Num(p, "paso").Instance.ValueChanged.InvokeAsync(6));
        Btn(p, "der").Click();
        Assert.Contains("6", Texto(p, "[data-problema='raya']"));
        Assert.Contains("1 aviso", Texto(p, "[data-chip='estado']"));
    }

    [Fact]
    public async Task TocarUnProblema_SeleccionaLaPieza()
    {
        var p = await Abre();
        await Selecciona(p, 0);
        await p.InvokeAsync(async () => await Num(p, "x").Instance.ValueChanged.InvokeAsync(100));
        await Selecciona(p, 1);
        Assert.Contains("Manga", Texto(p, "[data-sel]"));
        p.Find("[data-problema='solape']").Click();
        Assert.Contains("Cuerpo", Texto(p, "[data-sel]"));
    }

    [Fact]
    public async Task LasMedidasSeRecalculan_AlMoverUnaPiezaAlFinal()
    {
        var p = await Abre();
        await Selecciona(p, 1);
        await p.InvokeAsync(async () => await Num(p, "x").Instance.ValueChanged.InvokeAsync(900));
        Assert.Contains("1.01 m", Texto(p, "[data-chip='largo']").Replace(',', '.'));        // 1000 + margen 10
        Assert.DoesNotContain("10.0 %", Texto(p, "[data-chip='eficiencia']").Replace(',', '.'));
    }

    // =================== guardar ===================
    [Fact]
    public async Task Guardar_EnviaTodasLasPiezasConSuContornoFinal_YCierraConElNumeroNuevo()
    {
        var p = await Abre();
        await Selecciona(p, 1);
        Btn(p, "girar-180").Click(); Btn(p, "espejar").Click();
        await p.InvokeAsync(async () => await Num(p, "x").Instance.ValueChanged.InvokeAsync(300));
        p.Find("[data-campo='nombre-ajuste']").Change("  Mi ajuste ");
        Assert.False(Btn(p, "guardar").HasAttribute("disabled"));
        var pieza0 = Puntos(p, 0);
        Btn(p, "guardar").Click();
        var r = await _ref!.Result;
        Assert.False(r!.Canceled); Assert.Equal(7, r.Data);
        var b = _f.Cuerpo();
        string P(JsonElement e, string n) => e.EnumerateObject().First(x => string.Equals(x.Name, n, StringComparison.OrdinalIgnoreCase)).Value.ToString();
        Assert.Equal("D1", P(b, "COD_DISENO")); Assert.Equal("3", P(b, "NUM_VERSION")); Assert.Equal("3", P(b, "NUM_SECUENCIA")); Assert.Equal("Mi ajuste", P(b, "DES_TIZADO"));
        var piezas = b.EnumerateObject().First(x => string.Equals(x.Name, "Piezas", StringComparison.OrdinalIgnoreCase)).Value.EnumerateArray().ToList();
        Assert.Equal(2, piezas.Count);
        Assert.Equal(pieza0, P(piezas[0], "Puntos")); Assert.Equal("10,10 110,10 110,60 10,60", P(piezas[0], "Puntos"));
        Assert.Equal("2", P(piezas[1], "PiezaId")); Assert.Equal("True", P(piezas[1], "Espejada")); Assert.Equal("180", P(piezas[1], "Angulo"));
        Assert.Equal((300, 10, 400, 60), Caja(P(piezas[1], "Puntos")));
    }

    [Fact]
    public async Task Guardar_ConElPuntoDeCoincidenciaYSuPosicion()
    {
        _f.Calculo.Geometria.Colocadas[0] = FakeAjuste.Pz("1", "Cuerpo", 10, 10, 100, 50, punto: "20,30", fx: 0);
        var p = await Abre();
        await Selecciona(p, 0);
        await p.InvokeAsync(async () => await Num(p, "x").Instance.ValueChanged.InvokeAsync(300));
        Btn(p, "guardar").Click();
        await _ref!.Result;
        var piezas = _f.Cuerpo().EnumerateObject().First(x => string.Equals(x.Name, "Piezas", StringComparison.OrdinalIgnoreCase)).Value.EnumerateArray().ToList();
        Assert.Equal("310,30", piezas[0].EnumerateObject().First(x => string.Equals(x.Name, "Coincidencia", StringComparison.OrdinalIgnoreCase)).Value.ToString());
        Assert.Equal(JsonValueKind.Null, piezas[1].EnumerateObject().First(x => string.Equals(x.Name, "Coincidencia", StringComparison.OrdinalIgnoreCase)).Value.ValueKind);
    }

    [Fact]
    public async Task Guardar_SiElServidorRechaza_MuestraElMotivo_YNoCierra()
    {
        var p = await Abre();
        await Selecciona(p, 0); Btn(p, "der").Click();
        _f.Falla = "El ajuste no es valido: Cuerpo pisa a Manga.";
        Btn(p, "guardar").Click();
        p.WaitForAssertion(() => Assert.Contains("no es valido", Texto(p, "[data-error]")));
        Assert.False(_ref!.Result.IsCompleted);
    }

    [Fact]
    public async Task Guardar_SinNombre_OSinCambios_EstaDeshabilitado()
    {
        var p = await Abre();
        await Selecciona(p, 0); Btn(p, "der").Click();
        Assert.False(Btn(p, "guardar").HasAttribute("disabled"));
        p.Find("[data-campo='nombre-ajuste']").Change("  ");
        Assert.True(Btn(p, "guardar").HasAttribute("disabled"));
        p.Find("[data-campo='nombre-ajuste']").Change("X");
        Btn(p, "izq").Click();                                                      // vuelve a su sitio: ya no hay cambios
        Assert.True(Btn(p, "guardar").HasAttribute("disabled"));
    }

    [Fact]
    public async Task Cerrar_NoGuarda()
    {
        var p = await Abre();
        await Selecciona(p, 0); Btn(p, "der").Click();
        Btn(p, "cerrar").Click();
        Assert.True((await _ref!.Result)!.Canceled); Assert.DoesNotContain(_f.Calls, c => c.M == "POST");
    }
}
