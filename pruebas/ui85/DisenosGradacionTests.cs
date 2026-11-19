using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Bunit;
using KONSolutions.Shared.Models.Ingenieria;
using KONSolutions.Web.Pages.Ingenieria;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using Xunit;

// Gradacion de tallas (script SQL 117). La API se simula con FakeGradacion: devuelve las reglas guardadas y una vista previa,
// y deja el registro de lo que se le manda.
public class FakeGradacion : HttpMessageHandler
{
    public List<(string M, string U, JsonElement? B)> Calls = new();
    public List<DisenosGradacionRegla> Reglas = new();
    public string? Falla;
    public DisenosGradacionPrevia Previa = new()
    {
        NUM_SECUENCIA = 1, DES_MOLDE = "Frente", DES_TALLA_BASE = "M",
        Tallas = { new DisenosGradacionVista { DES_TALLA = "L", DES_CONTORNO = "0,0 130,0 130,70 0,70", VAL_AREA = 9100, VAL_LARGO = 130, VAL_ALTO = 70 } }
    };
    public DisenosGradacionResultado Resultado = new() { Secuencias = { 2 }, CREADOS = 1, ACTUALIZADOS = 0 };

    public static JsonElement P(JsonElement e, string n) => e.EnumerateObject().First(x => string.Equals(x.Name, n, StringComparison.OrdinalIgnoreCase)).Value;
    public JsonElement Cuerpo(string fin) => Calls.Last(c => c.M == "POST" && c.U.EndsWith(fin)).B!.Value;
    public int Veces(string fin) => Calls.Count(c => c.M == "POST" && c.U.EndsWith(fin));

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage r, CancellationToken ct)
    {
        var u = r.RequestUri!.PathAndQuery;
        JsonElement? body = r.Content is null ? null : JsonDocument.Parse(await r.Content.ReadAsStringAsync(ct)).RootElement.Clone();
        Calls.Add((r.Method.Method, u, body));
        HttpResponseMessage Ok(object? d) => new(HttpStatusCode.OK) { Content = JsonContent.Create(new { Success = true, Data = d }) };
        HttpResponseMessage Mal(string m) => new(HttpStatusCode.BadRequest) { Content = JsonContent.Create(new { Success = false, Message = m }) };
        if (r.Method == HttpMethod.Get) return Falla is not null && u.Contains("/gradacion/") ? Mal(Falla) : Ok(Reglas);
        if (Falla is not null) return Mal(Falla);
        if (u.EndsWith("/gradacion/previsualizar")) return Ok(Previa);
        if (u.EndsWith("/gradacion/generar")) return Ok(Resultado);
        return Ok(true);
    }
}

public class DisenosGradacionTests : TestContext
{
    readonly FakeGradacion _f = new();
    const string Cont = "0,0 100,0 100,50 0,50";

    IDialogReference? _ref;

    async Task<IRenderedComponent<MudDialogProvider>> Abre(string contorno = Cont)
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton(new HttpClient(_f) { BaseAddress = new Uri("http://x/") });
        RenderComponent<MudPopoverProvider>(); RenderComponent<MudSnackbarProvider>();
        var p = RenderComponent<MudDialogProvider>();
        var ds = Services.GetRequiredService<IDialogService>();
        await p.InvokeAsync(async () => _ref = await ds.ShowAsync<DisenosGradacionDialog>("t",
            new DialogParameters { ["CodDiseno"] = "D1", ["NumVersion"] = 3, ["NumSecuencia"] = 1, ["NombreMolde"] = "Frente", ["TallaBase"] = "M", ["Contorno"] = contorno },
            new DialogOptions { MaxWidth = MaxWidth.Large }));
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-chip='base']")));
        return p;
    }

    static void Escribe(IRenderedComponent<MudDialogProvider> p, string css, string valor) => p.Find(css).Change(valor);

    [Fact]
    public async Task AlAbrir_MuestraLaTallaBaseYSusMedidas()
    {
        var p = await Abre();
        Assert.Contains("M", p.Find("[data-chip='base']").TextContent);
        Assert.Contains("100", p.Find("[data-chip='medidas']").TextContent);
        Assert.Contains("50", p.Find("[data-chip='medidas']").TextContent);
        Assert.Contains(_f.Calls, c => c.M == "GET" && c.U.Contains("/gradacion/D1/3"));
    }

    [Fact]
    public async Task ReglasGuardadas_SeCarganComoFilas_SoloLasDelMoldeBase()
    {
        _f.Reglas.Add(new() { NUM_SECUENCIA = 1, DES_TALLA = "L", VAL_INC_X = 30, VAL_INC_Y = 20 });
        _f.Reglas.Add(new() { NUM_SECUENCIA = 9, DES_TALLA = "XL", VAL_INC_X = 1, VAL_INC_Y = 1 });
        var p = await Abre();
        p.WaitForAssertion(() => Assert.Single(p.FindAll("tr[data-talla]")));
        Assert.Equal("130 × 70", p.Find("tr[data-talla='L'] td.mono").TextContent.Trim());
    }

    [Fact]
    public async Task ReglaPorPunto_SeCargaConSusDeltas()
    {
        _f.Reglas.Add(new() { NUM_SECUENCIA = 1, DES_TALLA = "S", DES_DELTAS = "[[0,0],[-10,0],[-10,-5],[0,-5]]" });
        var p = await Abre();
        p.WaitForAssertion(() => Assert.Equal(4, p.FindAll("tr[data-punto]").Count));
        Assert.Equal("por punto", p.Find("tr[data-talla='S'] td.mono").TextContent.Trim());
    }

    [Fact]
    public async Task SerieRapida_ArmaLasFilasConIncrementosPorTalla_ConLaBaseEnCero()
    {
        var p = await Abre();
        Escribe(p, "[data-campo='serie-tallas']", "S, M, L, XL");
        Escribe(p, "[data-campo='paso-x']", "30"); Escribe(p, "[data-campo='paso-y']", "20");
        p.Find("[data-accion='serie']").Click();
        p.WaitForAssertion(() => Assert.Equal(3, p.FindAll("tr[data-talla]").Count));
        Assert.Empty(p.FindAll("tr[data-talla='M']"));
        Assert.Equal("70 × 30", p.Find("tr[data-talla='S'] td.mono").TextContent.Trim());
        Assert.Equal("130 × 70", p.Find("tr[data-talla='L'] td.mono").TextContent.Trim());
        Assert.Equal("160 × 90", p.Find("tr[data-talla='XL'] td.mono").TextContent.Trim());
    }

    [Fact]
    public async Task SerieRapida_SinLaTallaBase_Avisa_YNoAgregaNada()
    {
        var p = await Abre();
        Escribe(p, "[data-campo='serie-tallas']", "S, L, XL");
        p.Find("[data-accion='serie']").Click();
        Assert.Empty(p.FindAll("tr[data-talla]"));
    }

    [Fact]
    public async Task SerieRapida_ConUnaTallaYaEscrita_LaActualizaSinDuplicarla()
    {
        _f.Reglas.Add(new() { NUM_SECUENCIA = 1, DES_TALLA = "L", VAL_INC_X = 5, VAL_INC_Y = 5 });
        var p = await Abre();
        p.WaitForAssertion(() => Assert.Single(p.FindAll("tr[data-talla]")));
        Escribe(p, "[data-campo='serie-tallas']", "M, L");
        p.Find("[data-accion='serie']").Click();
        Assert.Single(p.FindAll("tr[data-talla]"));
        Assert.Equal("130 × 70", p.Find("tr[data-talla='L'] td.mono").TextContent.Trim());
    }

    [Fact]
    public async Task AgregarTalla_YQuitarla()
    {
        var p = await Abre();
        p.Find("[data-accion='agregar-talla']").Click();
        Assert.Single(p.FindAll("tr[data-talla]"));
        p.Find("[data-accion='quitar-talla']").Click();
        Assert.Empty(p.FindAll("tr[data-talla]"));
        Assert.True(p.Find("[data-accion='previa']").HasAttribute("disabled"));
        Assert.True(p.Find("[data-accion='generar']").HasAttribute("disabled"));
    }

    [Fact]
    public async Task EditarIncrementos_ActualizaElResultado()
    {
        var p = await Abre();
        p.Find("[data-accion='agregar-talla']").Click();
        Escribe(p, "[data-campo='talla']", "L");
        Escribe(p, "[data-campo='inc-x']", "12"); Escribe(p, "[data-campo='inc-y']", "-5");
        p.WaitForAssertion(() => Assert.Equal("112 × 45", p.Find("tr[data-talla='L'] td.mono").TextContent.Trim()));
    }

    [Fact]
    public async Task PorPunto_ArmaUnaFilaPorVertice_ConLosDeltasDeLosIncrementos()
    {
        var p = await Abre();
        p.Find("[data-accion='agregar-talla']").Click();
        Escribe(p, "[data-campo='talla']", "L");
        Escribe(p, "[data-campo='inc-x']", "30"); Escribe(p, "[data-campo='inc-y']", "20");
        p.Find("[data-campo='por-punto']").Change(true);
        p.WaitForAssertion(() => Assert.Equal(4, p.FindAll("tr[data-punto]").Count));
        var dx = p.FindAll("tr[data-punto] [data-campo='dx']").Select(e => e.GetAttribute("value")).ToList();
        Assert.Equal(new[] { "0", "30", "30", "0" }, dx);
        Assert.True(p.Find("tr[data-talla='L'] [data-campo='inc-x']").HasAttribute("disabled"));
    }

    [Fact]
    public async Task Previsualizar_MandaLosIncrementos_YMuestraLaTablaYElDibujo()
    {
        var p = await Abre();
        p.Find("[data-accion='agregar-talla']").Click();
        Escribe(p, "[data-campo='talla']", "L");
        Escribe(p, "[data-campo='inc-x']", "30"); Escribe(p, "[data-campo='inc-y']", "20");
        p.Find("[data-accion='previa']").Click();
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-zona='previa']")));
        var b = _f.Cuerpo("/gradacion/previsualizar");
        Assert.Equal("D1", FakeGradacion.P(b, "COD_DISENO").GetString());
        Assert.Equal(1, FakeGradacion.P(b, "NUM_SECUENCIA").GetInt32());
        var t = FakeGradacion.P(b, "Tallas")[0];
        Assert.Equal("L", FakeGradacion.P(t, "DES_TALLA").GetString());
        Assert.Equal(30, FakeGradacion.P(t, "VAL_INC_X").GetDouble());
        Assert.Equal(JsonValueKind.Null, FakeGradacion.P(t, "DELTAS").ValueKind);
        Assert.Equal(2, p.FindAll("svg polygon").Count);                      // base + talla
        Assert.Contains("+82.0%", p.Find("tr[data-previa='L']").TextContent);  // 9100 / 5000 - 1
    }

    [Fact]
    public async Task Previsualizar_PorPunto_MandaLosDeltas_SinIncrementos()
    {
        var p = await Abre();
        p.Find("[data-accion='agregar-talla']").Click();
        Escribe(p, "[data-campo='talla']", "L");
        p.Find("[data-campo='por-punto']").Change(true);
        p.WaitForAssertion(() => Assert.Equal(4, p.FindAll("tr[data-punto]").Count));
        p.FindAll("tr[data-punto] [data-campo='dx']")[1].Change("7");
        p.Find("[data-accion='previa']").Click();
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-zona='previa']")));
        var t = FakeGradacion.P(_f.Cuerpo("/gradacion/previsualizar"), "Tallas")[0];
        Assert.Equal(JsonValueKind.Null, FakeGradacion.P(t, "VAL_INC_X").ValueKind);
        Assert.Equal(4, FakeGradacion.P(t, "DELTAS").GetArrayLength());
        Assert.Equal(7, FakeGradacion.P(t, "DELTAS")[1][0].GetDouble());
    }

    [Fact]
    public async Task CambiarAlgo_DespuesDeLaVistaPrevia_LaInvalida()
    {
        var p = await Abre();
        p.Find("[data-accion='agregar-talla']").Click();
        Escribe(p, "[data-campo='talla']", "L");
        p.Find("[data-accion='previa']").Click();
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-zona='previa']")));
        Escribe(p, "[data-campo='inc-x']", "5");
        p.WaitForAssertion(() => Assert.Empty(p.FindAll("[data-zona='previa']")));
    }

    [Fact]
    public async Task Previsualizar_SiLaApiRechaza_MuestraElMotivo()
    {
        var p = await Abre();
        p.Find("[data-accion='agregar-talla']").Click();
        _f.Falla = "La talla L deja el molde invertido.";
        p.Find("[data-accion='previa']").Click();
        p.WaitForAssertion(() => Assert.Contains("invertido", p.Find("[data-error]").TextContent));
        Assert.Empty(p.FindAll("[data-zona='previa']"));
    }

    [Fact]
    public async Task Generar_ManaLasTallas_Y_CierraElDialogoConExito()
    {
        var p = await Abre();
        p.Find("[data-accion='agregar-talla']").Click();
        Escribe(p, "[data-campo='talla']", " L ");
        Escribe(p, "[data-campo='inc-x']", "30");
        p.Find("[data-accion='generar']").Click();
        p.WaitForAssertion(() => Assert.Equal(1, _f.Veces("/gradacion/generar")));
        Assert.Equal("L", FakeGradacion.P(FakeGradacion.P(_f.Cuerpo("/gradacion/generar"), "Tallas")[0], "DES_TALLA").GetString());   // sin espacios
        p.WaitForAssertion(() => Assert.Empty(p.FindAll("[data-accion='generar']")));
    }

    [Fact]
    public async Task Generar_DevuelveResultadoOk_ParaQueLaPantallaRecargue()
    {
        var p = await Abre();
        p.Find("[data-accion='agregar-talla']").Click();
        Escribe(p, "[data-campo='talla']", "L");
        p.Find("[data-accion='generar']").Click();
        var r = await _ref!.Result;
        Assert.False(r!.Canceled);
        Assert.Equal(true, r.Data);
    }

    [Fact]
    public async Task Cerrar_SinGenerar_Cancela()
    {
        var p = await Abre();
        p.Find("[data-accion='cerrar']").Click();
        Assert.True((await _ref!.Result)!.Canceled);
    }

    [Fact]
    public async Task Generar_SiLaApiRechaza_MuestraElMotivo_YNoCierra()
    {
        var p = await Abre();
        p.Find("[data-accion='agregar-talla']").Click();
        _f.Falla = "La version esta bloqueada.";
        p.Find("[data-accion='generar']").Click();
        p.WaitForAssertion(() => Assert.Contains("bloqueada", p.Find("[data-error]").TextContent));
        Assert.NotEmpty(p.FindAll("[data-accion='generar']"));
    }

    [Fact]
    public async Task SiLaLecturaDeReglasFalla_MuestraElMotivo()
    {
        _f.Falla = "Sin permiso.";
        var p = await Abre();
        p.WaitForAssertion(() => Assert.Contains("Sin permiso", p.Find("[data-error]").TextContent));
    }

    [Fact]
    public async Task ContornoInvalido_AvisaYNoDejaHacerNada()
    {
        var p = await Abre("0,0 5,5");
        Assert.Contains("contorno", p.Find("[data-error]").TextContent);
        Assert.Empty(_f.Calls);
    }
}
