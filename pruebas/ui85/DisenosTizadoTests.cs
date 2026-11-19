using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Bunit;
using KONSolutions.Shared.Models.Ingenieria;
using KONSolutions.Web.Pages.Ingenieria;
using KONSolutions.Web.Services;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using Xunit;

// Pantalla de tizado 2D de una version de diseno (scripts SQL 115 + API disenos-tizado). La API se simula con FakeTizado:
// guarda los moldes y tizados que le mandan, devuelve la importacion y el calculo configurados y deja el registro de las llamadas.
public class FakeTizado : HttpMessageHandler
{
    public List<(string M, string U, JsonElement? B)> Calls = new();
    public List<DisenosMolde> Moldes = new();
    public List<DisenosTizadoResumen> Tizados = new();
    public bool Bloqueada;
    public string? Falla;                       // las escrituras y calculos se rechazan con este mensaje (HTTP 400)
    public string? FallaLectura;                // la lectura de moldes se rechaza
    public DisenosMoldeImportarResultado Importacion = new() { MilimetrosPorUnidad = 1 };
    public DisenosTizadoCalculo Calculo = Calc();
    public string DxfTexto = "0\nEOF\n";

    public static DisenosTizadoCalculo Calc(int piezas = 6, int noColocadas = 0) => new()
    {
        VAL_ANCHO_TELA = 1500, VAL_LARGO = 2345.6, VAL_EFICIENCIA = 0.8123, VAL_CONSUMO_PRENDA = 0.4691, NUM_PRENDAS = 5, NUM_PIEZAS = piezas, NUM_NO_COLOCADAS = noColocadas,
        Svg = "<svg viewBox=\"0 0 2345.6 1500\"><polygon points=\"0,0 10,0 10,10\" data-pieza=\"1\"/></svg>",
        Geometria = new DisenosTizadoGeometria
        {
            Ancho = 1500, Largo = 2345.6,
            NoColocadas = Enumerable.Range(0, noColocadas).Select(_ => new DisenosTizadoNoColocada { PiezaId = "9", Nombre = "Larga", Talla = "S", Motivo = "No cabe en el ancho de tela (1500 mm)." }).ToList()
        }
    };

    public static DisenosMolde M(int sec, string nombre, string talla = "S", int cant = 2, int esp = 0, string giros = "0,180", string contorno = "0,0 100,0 100,50 0,50") =>
        new() { COD_DISENO = "D1", NUM_VERSION = 3, NUM_SECUENCIA = sec, DES_MOLDE = nombre, DES_TALLA = talla, NUM_CANTIDAD = cant, NUM_CANTIDAD_ESPEJADA = esp, DES_ROTACIONES = giros, DES_CONTORNO = contorno, VAL_AREA = 5000, VAL_LARGO = 100, VAL_ALTO = 50 };

    public static DisenosTizadoResumen T(int n, string nombre = "Pedido 1") =>
        new() { COD_DISENO = "D1", NUM_VERSION = 3, NUM_SECUENCIA = n, DES_TIZADO = nombre, VAL_ANCHO_TELA = 1500, VAL_SEPARACION = 3, VAL_MARGEN = 10, VAL_CELDA = 5, NUM_INTENTOS = 6, DES_TALLAS = "S=3;M=2",
                NUM_PRENDAS = 5, NUM_PIEZAS = 21, VAL_LARGO = 2345.6m, VAL_EFICIENCIA = 0.8123m, VAL_CONSUMO_PRENDA = 0.469m, FEC_REGISTRO = new DateTime(2026, 11, 16, 10, 30, 0) };

    public JsonElement? Cuerpo(string fin) => Calls.Last(c => c.M == "POST" && c.U.EndsWith(fin)).B;
    public int Veces(string m, string fin) => Calls.Count(c => c.M == m && c.U.EndsWith(fin));
    public int Lecturas => Calls.Count(c => c.M == "GET" && c.U.Contains("/disenos-tizado/moldes/"));

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage r, CancellationToken ct)
    {
        var u = r.RequestUri!.PathAndQuery;
        JsonElement? body = r.Content is null ? null : JsonDocument.Parse(await r.Content.ReadAsStringAsync(ct)).RootElement.Clone();
        Calls.Add((r.Method.Method, u, body));
        HttpResponseMessage Ok(object? data) => new(HttpStatusCode.OK) { Content = JsonContent.Create(new { Success = true, Data = data }) };
        HttpResponseMessage Mal(string msg) => new(HttpStatusCode.BadRequest) { Content = JsonContent.Create(new { Success = false, Message = msg }) };
        static JsonElement P(JsonElement e, string n) => e.EnumerateObject().First(x => string.Equals(x.Name, n, StringComparison.OrdinalIgnoreCase)).Value;

        if (r.Method == HttpMethod.Get)
        {
            if (u.Contains("/disenos-tizado/moldes/")) return FallaLectura is not null ? Mal(FallaLectura) : Ok(Moldes.Select(m => { m.FLG_BLOQUEADA(Bloqueada); return m; }).ToList());
            if (u.EndsWith("/dxf")) return Falla is not null ? Mal(Falla) : new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(DxfTexto) };
            if (u.Contains("/disenos-tizado/tizados/") && u.Split('/').Length > 8) return Ok(Calculo);
            if (u.Contains("/disenos-tizado/tizados/")) return Ok(Tizados.OrderByDescending(t => t.NUM_SECUENCIA).ToList());
            return Ok(new object[0]);
        }
        if (u.EndsWith("/disenos-estructura/desbloquear")) { if (Falla is not null) return Mal(Falla); Bloqueada = false; return Ok(true); }
        if (Falla is not null) return Mal(Falla);
        if (u.EndsWith("/moldes/importar-dxf")) return Ok(Importacion);
        if (u.EndsWith("/moldes/guardar"))
        {
            var sec = new List<int>();
            foreach (var m in P(body!.Value, "Moldes").EnumerateArray())
            {
                var n = P(m, "NUM_SECUENCIA");
                var datos = M(n.ValueKind == JsonValueKind.Null ? Moldes.Select(x => x.NUM_SECUENCIA).DefaultIfEmpty(0).Max() + 1 : n.GetInt32(), P(m, "DES_MOLDE").GetString()!, P(m, "DES_TALLA").GetString() ?? "U", P(m, "NUM_CANTIDAD").GetInt32(), P(m, "NUM_CANTIDAD_ESPEJADA").GetInt32(), P(m, "DES_ROTACIONES").GetString() ?? "0,180", P(m, "DES_CONTORNO").GetString()!);
                static JsonElement? Q(JsonElement e, string nom) => e.EnumerateObject().Where(x => string.Equals(x.Name, nom, StringComparison.OrdinalIgnoreCase)).Select(x => (JsonElement?)x.Value).FirstOrDefault();
                datos.FLG_COINCIDE_X = Q(m, "FLG_COINCIDE_X") is { ValueKind: JsonValueKind.True }; datos.FLG_COINCIDE_Y = Q(m, "FLG_COINCIDE_Y") is { ValueKind: JsonValueKind.True };
                datos.VAL_PUNTO_X = Q(m, "VAL_PUNTO_X") is { ValueKind: JsonValueKind.Number } px ? px.GetDecimal() : null; datos.VAL_PUNTO_Y = Q(m, "VAL_PUNTO_Y") is { ValueKind: JsonValueKind.Number } py ? py.GetDecimal() : null;
                datos.VAL_FASE_X = Q(m, "VAL_FASE_X") is { ValueKind: JsonValueKind.Number } fx ? fx.GetDecimal() : 0; datos.VAL_FASE_Y = Q(m, "VAL_FASE_Y") is { ValueKind: JsonValueKind.Number } fy ? fy.GetDecimal() : 0;
                Moldes.RemoveAll(x => x.NUM_SECUENCIA == datos.NUM_SECUENCIA); Moldes.Add(datos); sec.Add(datos.NUM_SECUENCIA);
            }
            Moldes = Moldes.OrderBy(x => x.NUM_SECUENCIA).ToList();
            return Ok(new DisenosMoldesGuardarResultado { Secuencias = sec });
        }
        if (u.EndsWith("/moldes/eliminar")) { Moldes.RemoveAll(x => x.NUM_SECUENCIA == P(body!.Value, "NUM_SECUENCIA").GetInt32()); return Ok(new DisenosMoldeEliminarResultado { ELIMINADOS = 1 }); }
        if (u.EndsWith("/tizados/ajustar")) return Ok(Calculo);
        if (u.EndsWith("/calcular")) return Ok(Calculo);
        if (u.EndsWith("/disenos-tizado/guardar"))
        {
            var n = Tizados.Select(t => t.NUM_SECUENCIA).DefaultIfEmpty(0).Max() + 1;
            Tizados.Add(T(n, P(body!.Value, "DES_TIZADO").GetString()!));
            Calculo.NUM_SECUENCIA = n; return Ok(Calculo);
        }
        if (u.EndsWith("/tizados/eliminar")) { Tizados.RemoveAll(x => x.NUM_SECUENCIA == P(body!.Value, "NUM_SECUENCIA").GetInt32()); return Ok(new DisenosTizadoEliminarResultado { ELIMINADOS = 1 }); }
        return Ok(true);
    }
}

static class FakeTizadoExt { public static void FLG_BLOQUEADA(this DisenosMolde m, bool b) => m.FLG_BLOQUEADO = b; }

class ArchivoFalso : IBrowserFile
{
    readonly byte[] _b;
    public ArchivoFalso(string nombre, string texto) { Name = nombre; _b = System.Text.Encoding.UTF8.GetBytes(texto); }
    public string Name { get; }
    public DateTimeOffset LastModified => DateTimeOffset.Now;
    public long Size => _b.Length;
    public string ContentType => "application/dxf";
    public Stream OpenReadStream(long maxAllowedSize = 512000, CancellationToken cancellationToken = default) =>
        _b.Length > maxAllowedSize ? throw new IOException("demasiado grande") : new MemoryStream(_b);
}

public class DisenosTizadoTests : TestContext
{
    protected readonly FakeTizado _f = new();

    IRenderedComponent<MudDialogProvider> Prov()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton(new HttpClient(_f) { BaseAddress = new Uri("http://x/") });
        RenderComponent<MudPopoverProvider>(); RenderComponent<MudSnackbarProvider>();
        return RenderComponent<MudDialogProvider>();
    }

    protected async Task<IRenderedComponent<MudDialogProvider>> Abre()
    {
        var p = Prov();
        var ds = Services.GetRequiredService<IDialogService>();
        await p.InvokeAsync(async () => await ds.ShowAsync<DisenosTizadoDialog>("t",
            new DialogParameters { ["CodDiseno"] = "D1", ["DesDiseno"] = "CAMISA", ["NumVersion"] = 3 }, new DialogOptions { MaxWidth = MaxWidth.ExtraLarge }));
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-conteo='moldes']")));
        p.WaitForAssertion(() => Assert.Equal(1, _f.Lecturas > 0 ? 1 : 0));
        return p;
    }

    async Task<IRenderedComponent<MudDialogProvider>> AbreTizado()
    {
        var p = await Abre();
        p.FindAll(".mud-tab")[1].Click();
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-zona='parametros']")));
        return p;
    }

    static string Prop(System.Text.Json.JsonElement e, string n) => e.EnumerateObject().First(x => string.Equals(x.Name, n, StringComparison.OrdinalIgnoreCase)).Value.ToString();
    static System.Text.Json.JsonElement PropE(System.Text.Json.JsonElement e, string n) => e.EnumerateObject().First(x => string.Equals(x.Name, n, StringComparison.OrdinalIgnoreCase)).Value;
    static AngleSharp.Dom.IElement Btn(IRenderedComponent<MudDialogProvider> p, string accion) => p.Find($"[data-accion='{accion}']");
    static string Texto(IRenderedComponent<MudDialogProvider> p, string sel) => p.Find(sel).TextContent;

    IRenderedComponent<MudNumericField<double>> Num(IRenderedComponent<MudDialogProvider> p, string campo) =>
        p.FindComponents<MudNumericField<double>>().First(c => c.Markup.Contains($"data-campo=\"{campo}\""));
    IRenderedComponent<MudNumericField<int>> NumI(IRenderedComponent<MudDialogProvider> p, string campo) =>
        p.FindComponents<MudNumericField<int>>().First(c => c.Markup.Contains($"data-campo=\"{campo}\""));

    async Task Importa(IRenderedComponent<MudDialogProvider> p, string texto = "0\nEOF\n")
    {
        var up = p.FindComponent<MudFileUpload<IBrowserFile>>();
        await p.InvokeAsync(() => up.Instance.FilesChanged.InvokeAsync(new ArchivoFalso("camisa.dxf", texto)));
    }

    static DisenosMoldeImportarResultado Imp(params (string nombre, string contorno, decimal largo, decimal alto)[] c) => new()
    {
        MilimetrosPorUnidad = 10,
        Contornos = c.Select(x => new DisenosMoldeImportado { Capa = "1", DES_CONTORNO = x.contorno, VAL_LARGO = x.largo, VAL_ALTO = x.alto, VAL_AREA = x.largo * x.alto }).ToList(),
        Advertencias = new() { "2 entidad(es) sueltas (lineas, textos, bloques...) ignoradas." }
    };

    // =================== carga y moldes ===================
    [Fact]
    public async Task Gradar_AbreElDialogoConElMoldeBase_YLoPideAlServidor()
    {
        _f.Moldes = new() { FakeTizado.M(1, "Cuerpo", "M"), FakeTizado.M(2, "Manga", "M") };
        var p = await Abre();
        p.FindAll("[data-tabla='moldes'] tr[data-molde]")[1].QuerySelector("[data-accion='gradar']")!.Click();
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-chip='base']")));
        Assert.Contains("M", p.Find("[data-chip='base']").TextContent);
        Assert.Contains(_f.Calls, c => c.M == "GET" && c.U.Contains("/gradacion/D1/3"));
    }

    [Fact]
    public async Task Gradar_ConCambiosSinGuardar_AvisaYNoAbre()
    {
        _f.Moldes = new() { FakeTizado.M(1, "Cuerpo") };
        var p = await Abre();
        p.Find("input[data-campo='nombre']").Change("Otro");
        p.Find("[data-accion='gradar']").Click();
        Assert.Empty(p.FindAll("[data-chip='base']"));
    }

    [Fact]
    public async Task Gradar_EnUnaVersionBloqueada_NoSeOfrece()
    {
        _f.Moldes = new() { FakeTizado.M(1, "Cuerpo") };
        _f.Bloqueada = true;
        var p = await Abre();
        Assert.Empty(p.FindAll("[data-accion='gradar']"));
    }

    // =================== carga y moldes ===================
    [Fact]
    public async Task AlAbrir_LeeLosMoldesYLosTizados_YMuestraSusFilas()
    {
        _f.Moldes = new() { FakeTizado.M(1, "Cuerpo"), FakeTizado.M(2, "Manga", "M", 4, 2, "0") };
        _f.Tizados = new() { FakeTizado.T(1) };
        var p = await Abre();
        Assert.Equal(1, _f.Calls.Count(c => c.M == "GET" && c.U == "/api/v1/ingenieria/disenos-tizado/moldes/D1/3"));
        Assert.Equal(1, _f.Calls.Count(c => c.M == "GET" && c.U == "/api/v1/ingenieria/disenos-tizado/tizados/D1/3"));
        Assert.Contains("2 molde(s)", Texto(p, "[data-conteo='moldes']"));
        var filas = p.FindAll("[data-tabla='moldes'] tr[data-molde]");
        Assert.Equal(2, filas.Count);
        Assert.Equal("Cuerpo", filas[0].QuerySelector("input[data-campo='nombre']")!.GetAttribute("value"));
        Assert.Equal("M", filas[1].QuerySelector("input[data-campo='talla']")!.GetAttribute("value"));
        Assert.Equal("0", filas[1].QuerySelector("input[data-campo='giros']")!.GetAttribute("value"));
        Assert.Contains("100 × 50", filas[0].TextContent);
        Assert.Contains("50", filas[0].TextContent);            // area en cm2 = 5000 mm2 / 100
        Assert.NotEmpty(filas[0].QuerySelectorAll("svg polygon"));
        Assert.Empty(p.FindAll("[data-aviso='sin-moldes']"));
    }

    [Fact]
    public async Task SinMoldes_AvisaQueHayQueCargarlos()
    {
        var p = await Abre();
        Assert.NotEmpty(p.FindAll("[data-aviso='sin-moldes']"));
        Assert.Contains("0 molde(s)", Texto(p, "[data-conteo='moldes']"));
        Assert.Empty(p.FindAll("[data-tabla='moldes']"));
    }

    [Fact]
    public async Task SiLaApiRechazaLaLectura_MuestraElMotivo()
    {
        _f.FallaLectura = "La version no existe.";
        var p = Prov();
        await p.InvokeAsync(async () => await Services.GetRequiredService<IDialogService>().ShowAsync<DisenosTizadoDialog>("t", new DialogParameters { ["CodDiseno"] = "D1", ["NumVersion"] = 3 }));
        p.WaitForAssertion(() => Assert.Contains("La version no existe.", Texto(p, "[data-error]")));
    }

    // =================== importar DXF ===================
    [Fact]
    public async Task ImportarDxf_MandaElTextoDelArchivo_YMuestraLosContornosPorGuardar()
    {
        _f.Importacion = Imp(("a", "0,0 1000,0 1000,500 0,500", 1000, 500), ("b", "0,0 300,0 300,200", 300, 200));
        var p = await Abre();
        await Importa(p, "TEXTO-DEL-DXF");
        p.WaitForAssertion(() => Assert.Equal(2, p.FindAll("[data-tabla='nuevos'] tr[data-nuevo]").Count));
        var b = _f.Cuerpo("/moldes/importar-dxf")!.Value;
        Assert.Equal("TEXTO-DEL-DXF", Prop(b, "DXF")); Assert.Equal(System.Text.Json.JsonValueKind.Null, PropE(b, "MM_POR_UNIDAD").ValueKind);
        var filas = p.FindAll("[data-tabla='nuevos'] tr[data-nuevo]");
        Assert.Equal("Pieza 1", filas[0].GetAttribute("data-nuevo")); Assert.Equal("Pieza 2", filas[1].GetAttribute("data-nuevo"));
        Assert.Contains("1000 × 500", filas[0].TextContent);
        Assert.Contains("Por guardar (2 de 2)", p.Markup);
        Assert.Contains("entidad(es) sueltas", Texto(p, "[data-aviso='dxf']"));
    }

    [Fact]
    public async Task ImportarDxf_ConMilimetrosPorUnidad_LosEnvia()
    {
        _f.Importacion = Imp(("a", "0,0 10,0 10,10", 10, 10));
        var p = await Abre();
        await p.InvokeAsync(() => Num2(p).Instance.ValueChanged.InvokeAsync(25.4));
        await Importa(p);
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-nuevo]")));
        Assert.Equal("25.4", Prop(_f.Cuerpo("/moldes/importar-dxf")!.Value, "MM_POR_UNIDAD"));
    }
    IRenderedComponent<MudNumericField<double?>> Num2(IRenderedComponent<MudDialogProvider> p) => p.FindComponents<MudNumericField<double?>>().First(c => c.Markup.Contains("data-campo=\"mm\""));

    [Fact]
    public async Task ImportarDxf_SiLaApiRechaza_MuestraElMotivo_YNoAgregaNada()
    {
        _f.Falla = "El archivo DXF es demasiado grande (maximo 5 MB).";
        var p = await Abre();
        await Importa(p);
        p.WaitForAssertion(() => Assert.Contains("demasiado grande", Texto(p, "[data-error]")));
        Assert.Empty(p.FindAll("[data-nuevo]"));
    }

    [Fact]
    public async Task ImportarDxf_VariasVeces_SigueLaNumeracionDeLosNombres()
    {
        _f.Importacion = Imp(("a", "0,0 10,0 10,10", 10, 10));
        var p = await Abre();
        await Importa(p); p.WaitForAssertion(() => Assert.Single(p.FindAll("[data-nuevo]")));
        await Importa(p); p.WaitForAssertion(() => Assert.Equal(2, p.FindAll("[data-nuevo]").Count));
        Assert.Equal(new[] { "Pieza 1", "Pieza 2" }, p.FindAll("[data-nuevo]").Select(f => f.GetAttribute("data-nuevo")).ToArray());
    }

    [Fact]
    public async Task ImportarDxf_SinContornos_AvisaConElMensajeDelServidor()
    {
        _f.Importacion = new() { MilimetrosPorUnidad = 1, Advertencias = new() { "No se encontraron contornos cerrados." } };
        var p = await Abre();
        await Importa(p);
        p.WaitForAssertion(() => Assert.Contains("No se encontraron", Texto(p, "[data-aviso='dxf']")));
        Assert.Empty(p.FindAll("[data-nuevo]"));
    }

    [Fact]
    public async Task ImportarDxf_OtraVez_ReemplazaLosAvisosAnteriores()
    {
        _f.Importacion = Imp(("a", "0,0 10,0 10,10", 10, 10)); _f.Importacion.Advertencias = new() { "AVISO-UNO" };
        var p = await Abre();
        await Importa(p); p.WaitForAssertion(() => Assert.Contains("AVISO-UNO", p.Markup));
        _f.Importacion = Imp(("b", "0,0 20,0 20,20", 20, 20)); _f.Importacion.Advertencias = new() { "AVISO-DOS" };
        await Importa(p); p.WaitForAssertion(() => Assert.Contains("AVISO-DOS", p.Markup));
        Assert.DoesNotContain("AVISO-UNO", p.Markup);
    }

    [Fact]
    public async Task AgregarRectangulo_ConSoloUnaMedidaEnCero_NoAgrega()
    {
        var p = await Abre();
        await p.InvokeAsync(() => Num(p, "rect-alto").Instance.ValueChanged.InvokeAsync(0));
        Btn(p, "rectangulo").Click();
        Assert.Empty(p.FindAll("[data-nuevo]"));
        await p.InvokeAsync(() => Num(p, "rect-alto").Instance.ValueChanged.InvokeAsync(40));
        await p.InvokeAsync(() => Num(p, "rect-ancho").Instance.ValueChanged.InvokeAsync(-5));
        Btn(p, "rectangulo").Click();
        Assert.Empty(p.FindAll("[data-nuevo]"));
    }

    // =================== rectangulos ===================
    [Fact]
    public async Task AgregarRectangulo_ArmaElContorno_ConPuntoDecimalSinImportarLaCultura()
    {
        var antes = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("es-PE");
            var p = await Abre();
            await p.InvokeAsync(() => Num(p, "rect-ancho").Instance.ValueChanged.InvokeAsync(150.5));
            await p.InvokeAsync(() => Num(p, "rect-alto").Instance.ValueChanged.InvokeAsync(70));
            Btn(p, "rectangulo").Click();
            p.WaitForAssertion(() => Assert.Single(p.FindAll("[data-nuevo]")));
            Assert.Equal("Rectángulo 1", p.Find("[data-nuevo]").GetAttribute("data-nuevo"));
            Btn(p, "guardar-seleccion").Click();
            p.WaitForAssertion(() => Assert.Equal(1, _f.Veces("POST", "/moldes/guardar")));
            var m = PropE(_f.Cuerpo("/moldes/guardar")!.Value, "Moldes")[0];
            Assert.Equal("0,0 150.5,0 150.5,70 0,70", Prop(m, "DES_CONTORNO"));
        }
        finally { CultureInfo.CurrentCulture = antes; }
    }

    [Fact]
    public async Task AgregarRectangulo_ConNombre_LoUsa_YLimpiaElCampo()
    {
        var p = await Abre();
        await p.InvokeAsync(() => p.FindComponents<MudTextField<string>>().First(c => c.Markup.Contains("data-campo=\"rect-nombre\"")).Instance.ValueChanged.InvokeAsync("Bolsillo"));
        Btn(p, "rectangulo").Click();
        p.WaitForAssertion(() => Assert.Equal("Bolsillo", p.Find("[data-nuevo]").GetAttribute("data-nuevo")));
        Assert.Equal("", p.FindComponents<MudTextField<string>>().First(c => c.Markup.Contains("data-campo=\"rect-nombre\"")).Instance.Value ?? "");
    }

    [Fact]
    public async Task AgregarRectangulo_ElNombreSeGuardaSinEspaciosSobrantes()
    {
        var p = await Abre();
        await p.InvokeAsync(() => p.FindComponents<MudTextField<string>>().First(c => c.Markup.Contains("data-campo=\"rect-nombre\"")).Instance.ValueChanged.InvokeAsync("  Cuello  "));
        Btn(p, "rectangulo").Click();
        p.WaitForAssertion(() => Assert.Equal("Cuello", p.Find("[data-nuevo]").GetAttribute("data-nuevo")));
    }

    [Fact]
    public async Task AgregarRectangulo_ConMedidaCero_NoAgrega()
    {
        var p = await Abre();
        await p.InvokeAsync(() => Num(p, "rect-ancho").Instance.ValueChanged.InvokeAsync(0));
        Btn(p, "rectangulo").Click();
        Assert.Empty(p.FindAll("[data-nuevo]"));
    }

    // =================== guardar nuevos ===================
    [Fact]
    public async Task GuardarSeleccionados_EnviaSoloLosMarcados_ConTodosSusCampos_YRecarga()
    {
        _f.Importacion = Imp(("a", "0,0 1000,0 1000,500 0,500", 1000, 500), ("b", "0,0 300,0 300,200", 300, 200));
        var p = await Abre();
        await Importa(p);
        p.WaitForAssertion(() => Assert.Equal(2, p.FindAll("[data-nuevo]").Count));
        var filas = p.FindAll("[data-tabla='nuevos'] tr[data-nuevo]");
        // la segunda se desmarca; la primera se renombra, se le pone talla, cantidad, espejadas y giros
        await p.InvokeAsync(() => p.FindComponents<MudCheckBox<bool>>()[1].Instance.ValueChanged.InvokeAsync(false));
        var tf = p.FindComponents<MudTextField<string>>().Where(c => c.Markup.Contains("data-campo=")).ToList();
        string Campo(string c, int i) => c;
        await p.InvokeAsync(async () =>
        {
            await tf.First(c => c.Markup.Contains("data-campo=\"nombre\"")).Instance.ValueChanged.InvokeAsync("Cuerpo");
            await tf.First(c => c.Markup.Contains("data-campo=\"talla\"")).Instance.ValueChanged.InvokeAsync("M");
            await tf.First(c => c.Markup.Contains("data-campo=\"giros\"")).Instance.ValueChanged.InvokeAsync("0");
            await NumI(p, "cant").Instance.ValueChanged.InvokeAsync(3);
            await NumI(p, "esp").Instance.ValueChanged.InvokeAsync(1);
        });
        Assert.Contains("Por guardar (1 de 2)", p.Markup);
        var lecturasAntes = _f.Lecturas;
        Btn(p, "guardar-seleccion").Click();
        p.WaitForAssertion(() => Assert.Equal(1, _f.Veces("POST", "/moldes/guardar")));
        var b = _f.Cuerpo("/moldes/guardar")!.Value;
        Assert.Equal("D1", Prop(b, "COD_DISENO")); Assert.Equal("3", Prop(b, "NUM_VERSION"));
        var ms = PropE(b, "Moldes"); Assert.Equal(1, ms.GetArrayLength());
        Assert.Equal("Cuerpo", Prop(ms[0], "DES_MOLDE")); Assert.Equal("M", Prop(ms[0], "DES_TALLA")); Assert.Equal("3", Prop(ms[0], "NUM_CANTIDAD")); Assert.Equal("1", Prop(ms[0], "NUM_CANTIDAD_ESPEJADA"));
        Assert.Equal("0", Prop(ms[0], "DES_ROTACIONES")); Assert.Equal("0,0 1000,0 1000,500 0,500", Prop(ms[0], "DES_CONTORNO")); Assert.Equal(System.Text.Json.JsonValueKind.Null, PropE(ms[0], "NUM_SECUENCIA").ValueKind);
        p.WaitForAssertion(() => Assert.Single(p.FindAll("[data-tabla='moldes'] tr[data-molde]")));      // recargo: ahora aparece como molde guardado
        Assert.True(_f.Lecturas > lecturasAntes);
        Assert.Contains("Por guardar (0 de 1)", p.Markup);                                              // lo no marcado sigue pendiente (y sin marcar)
    }

    [Fact]
    public async Task GuardarSeleccionados_SiLaApiRechaza_MuestraElMotivo_YConservaLosPendientes()
    {
        _f.Importacion = Imp(("a", "0,0 10,0 10,10", 10, 10));
        var p = await Abre();
        await Importa(p); p.WaitForAssertion(() => Assert.Single(p.FindAll("[data-nuevo]")));
        _f.Falla = "La version esta bloqueada: desbloqueela para modificar sus moldes.";
        Btn(p, "guardar-seleccion").Click();
        p.WaitForAssertion(() => Assert.Contains("bloqueada", Texto(p, "[data-error]")));
        Assert.Single(p.FindAll("[data-nuevo]"));
    }

    [Fact]
    public async Task GuardarSeleccionados_SinNingunoMarcado_EstaDeshabilitado()
    {
        _f.Importacion = Imp(("a", "0,0 10,0 10,10", 10, 10));
        var p = await Abre();
        await Importa(p); p.WaitForAssertion(() => Assert.Single(p.FindAll("[data-nuevo]")));
        await p.InvokeAsync(() => p.FindComponents<MudCheckBox<bool>>()[0].Instance.ValueChanged.InvokeAsync(false));
        p.WaitForAssertion(() => Assert.True(Btn(p, "guardar-seleccion").HasAttribute("disabled")));
    }

    [Fact]
    public async Task QuitarYDescartar_LimpianLosPendientes()
    {
        _f.Importacion = Imp(("a", "0,0 10,0 10,10", 10, 10), ("b", "0,0 20,0 20,20", 20, 20));
        var p = await Abre();
        await Importa(p); p.WaitForAssertion(() => Assert.Equal(2, p.FindAll("[data-nuevo]").Count));
        p.FindAll("[data-accion='quitar-nuevo']")[0].Click();
        p.WaitForAssertion(() => Assert.Single(p.FindAll("[data-nuevo]")));
        Btn(p, "descartar-nuevos").Click();
        p.WaitForAssertion(() => Assert.Empty(p.FindAll("[data-tabla='nuevos']")));
        Assert.Empty(p.FindAll("[data-aviso='dxf']"));
        Assert.Equal(0, _f.Veces("POST", "/moldes/guardar"));
    }

    // =================== editar y eliminar moldes guardados ===================
    [Fact]
    public async Task EditarUnMolde_HabilitaGuardarCambios_YEnviaSoloLosModificados()
    {
        _f.Moldes = new() { FakeTizado.M(1, "Cuerpo"), FakeTizado.M(2, "Manga") };
        var p = await Abre();
        Assert.True(Btn(p, "guardar-moldes").HasAttribute("disabled"));
        var fila2 = p.FindAll("tr[data-molde='2']")[0];
        await p.InvokeAsync(() => p.FindComponents<MudTextField<string>>().Where(c => c.Markup.Contains("data-campo=\"nombre\"")).ElementAt(1).Instance.ValueChanged.InvokeAsync("Manga larga"));
        p.WaitForAssertion(() => Assert.False(Btn(p, "guardar-moldes").HasAttribute("disabled")));
        Btn(p, "guardar-moldes").Click();
        p.WaitForAssertion(() => Assert.Equal(1, _f.Veces("POST", "/moldes/guardar")));
        var ms = PropE(_f.Cuerpo("/moldes/guardar")!.Value, "Moldes");
        Assert.Equal(1, ms.GetArrayLength());
        Assert.Equal("2", Prop(ms[0], "NUM_SECUENCIA")); Assert.Equal("Manga larga", Prop(ms[0], "DES_MOLDE")); Assert.Equal("S", Prop(ms[0], "DES_TALLA")); Assert.Equal("2", Prop(ms[0], "NUM_CANTIDAD"));
        Assert.Equal("0,0 100,0 100,50 0,50", Prop(ms[0], "DES_CONTORNO"));                      // el contorno viaja tal cual
        p.WaitForAssertion(() => Assert.True(Btn(p, "guardar-moldes").HasAttribute("disabled")));  // al recargar ya no hay cambios pendientes
        Assert.Equal("Manga larga", p.FindAll("tr[data-molde='2'] input[data-campo='nombre']")[0].GetAttribute("value"));
    }

    [Fact]
    public async Task EditarCantidadEspejadasYGiros_TambienMarcanElCambio()
    {
        _f.Moldes = new() { FakeTizado.M(1, "Cuerpo") };
        var p = await Abre();
        await p.InvokeAsync(() => NumI(p, "cant").Instance.ValueChanged.InvokeAsync(4));
        p.WaitForAssertion(() => Assert.False(Btn(p, "guardar-moldes").HasAttribute("disabled")));
        Btn(p, "guardar-moldes").Click();
        p.WaitForAssertion(() => Assert.Equal(1, _f.Veces("POST", "/moldes/guardar")));
        Assert.Equal("4", Prop(PropE(_f.Cuerpo("/moldes/guardar")!.Value, "Moldes")[0], "NUM_CANTIDAD"));
        await p.InvokeAsync(() => NumI(p, "esp").Instance.ValueChanged.InvokeAsync(2));
        p.WaitForAssertion(() => Assert.False(Btn(p, "guardar-moldes").HasAttribute("disabled")));
        Btn(p, "guardar-moldes").Click();
        p.WaitForAssertion(() => Assert.Equal(2, _f.Veces("POST", "/moldes/guardar")));
        Assert.Equal("2", Prop(PropE(_f.Cuerpo("/moldes/guardar")!.Value, "Moldes")[0], "NUM_CANTIDAD_ESPEJADA"));
        await p.InvokeAsync(() => p.FindComponents<MudTextField<string>>().First(c => c.Markup.Contains("data-campo=\"giros\"")).Instance.ValueChanged.InvokeAsync("0,90,180"));
        p.WaitForAssertion(() => Assert.False(Btn(p, "guardar-moldes").HasAttribute("disabled")));
        Btn(p, "guardar-moldes").Click();
        p.WaitForAssertion(() => Assert.Equal(3, _f.Veces("POST", "/moldes/guardar")));
        Assert.Equal("0,90,180", Prop(PropE(_f.Cuerpo("/moldes/guardar")!.Value, "Moldes")[0], "DES_ROTACIONES"));
    }

    [Fact]
    public async Task GuardarCambios_SiLaApiRechaza_MuestraElMotivo_YNoRecarga()
    {
        _f.Moldes = new() { FakeTizado.M(1, "Cuerpo") };
        var p = await Abre();
        await p.InvokeAsync(() => p.FindComponents<MudTextField<string>>().First(c => c.Markup.Contains("data-campo=\"nombre\"")).Instance.ValueChanged.InvokeAsync("X"));
        _f.Falla = "Las cantidades del molde X no son validas.";
        var antes = _f.Lecturas;
        Btn(p, "guardar-moldes").Click();
        p.WaitForAssertion(() => Assert.Contains("no son validas", Texto(p, "[data-error]")));
        Assert.Equal(antes, _f.Lecturas);
    }

    [Fact]
    public async Task EliminarMolde_PideConfirmar_YPublicaLaClave()
    {
        _f.Moldes = new() { FakeTizado.M(1, "Cuerpo"), FakeTizado.M(2, "Manga") };
        var p = await Abre();
        p.FindAll("tr[data-molde='2'] [data-accion='eliminar-molde']")[0].Click();
        p.WaitForAssertion(() => Assert.Contains("Eliminar molde", p.Markup));
        Assert.Equal(0, _f.Veces("POST", "/moldes/eliminar"));
        p.FindAll("button").Last(b => b.TextContent.Trim() == "Eliminar").Click();
        p.WaitForAssertion(() => Assert.Equal(1, _f.Veces("POST", "/moldes/eliminar")));
        var b = _f.Cuerpo("/moldes/eliminar")!.Value;
        Assert.Equal("D1", Prop(b, "COD_DISENO")); Assert.Equal("3", Prop(b, "NUM_VERSION")); Assert.Equal("2", Prop(b, "NUM_SECUENCIA"));
        p.WaitForAssertion(() => Assert.Single(p.FindAll("[data-tabla='moldes'] tr[data-molde]")));
    }

    [Fact]
    public async Task EliminarMolde_SiSeCancela_NoPublicaNada()
    {
        _f.Moldes = new() { FakeTizado.M(1, "Cuerpo") };
        var p = await Abre();
        p.FindAll("[data-accion='eliminar-molde']")[0].Click();
        p.WaitForAssertion(() => Assert.Contains("Eliminar molde", p.Markup));
        p.FindAll("button").Last(b => b.TextContent.Trim() == "Cancelar").Click();
        p.WaitForAssertion(() => Assert.DoesNotContain("Eliminar molde", p.Markup));
        Assert.Equal(0, _f.Veces("POST", "/moldes/eliminar"));
    }

    // =================== version bloqueada ===================
    [Fact]
    public async Task Bloqueada_LosMoldesSonSoloLectura_SinImportarNiEliminar_YSePuedeDesbloquear()
    {
        _f.Moldes = new() { FakeTizado.M(1, "Cuerpo") }; _f.Bloqueada = true;
        var p = await Abre();
        Assert.NotEmpty(p.FindAll("[data-estado='bloqueada']")); Assert.NotEmpty(p.FindAll("[data-aviso='bloqueada']"));
        Assert.Empty(p.FindAll("[data-zona='importar']")); Assert.Empty(p.FindAll("[data-zona='rectangulo']")); Assert.Empty(p.FindAll("[data-accion='eliminar-molde']")); Assert.Empty(p.FindAll("[data-accion='guardar-moldes']"));
        Assert.True(p.FindComponents<MudTextField<string>>().First(c => c.Markup.Contains("data-campo=\"nombre\"")).Instance.ReadOnly);
        Btn(p, "desbloquear").Click();
        p.WaitForAssertion(() => Assert.Contains("Desbloquear versión", p.Markup));
        p.FindAll("button").Last(b => b.TextContent.Trim() == "Desbloquear").Click();
        p.WaitForAssertion(() => Assert.Equal(1, _f.Veces("POST", "/disenos-estructura/desbloquear")));
        p.WaitForAssertion(() => Assert.Empty(p.FindAll("[data-estado='bloqueada']")));
        Assert.NotEmpty(p.FindAll("[data-zona='importar']"));
        Assert.Equal("3", Prop(_f.Cuerpo("/disenos-estructura/desbloquear")!.Value, "NUM_VERSION"));
    }

    [Fact]
    public async Task Desbloquear_SiLaApiRechaza_MuestraElMotivo_YSigueBloqueada()
    {
        _f.Moldes = new() { FakeTizado.M(1, "Cuerpo") }; _f.Bloqueada = true; _f.Falla = "No tiene permiso para desbloquear.";
        var p = await Abre();
        Btn(p, "desbloquear").Click();
        p.WaitForAssertion(() => Assert.Contains("Desbloquear versión", p.Markup));
        p.FindAll("button").Last(b => b.TextContent.Trim() == "Desbloquear").Click();
        p.WaitForAssertion(() => Assert.Contains("No tiene permiso", Texto(p, "[data-error]")));
        Assert.NotEmpty(p.FindAll("[data-estado='bloqueada']"));
    }

    [Fact]
    public async Task ConLaVersionBloqueada_ElTizadoSiSePuedeCalcularYGuardar()
    {
        _f.Moldes = new() { FakeTizado.M(1, "Cuerpo") }; _f.Bloqueada = true;
        var p = await Abre();
        p.FindAll(".mud-tab")[1].Click();
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-zona='parametros']")));
        Assert.False(Btn(p, "calcular").HasAttribute("disabled"));
        Btn(p, "calcular").Click();
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-resultado]")));
    }

    // =================== tizado ===================
    [Fact]
    public async Task Tallas_SalenDeLosMoldes_SinLaU_Ordenadas_YConUnaPrendaPorOmision()
    {
        _f.Moldes = new() { FakeTizado.M(1, "Cuerpo", "M"), FakeTizado.M(2, "Cuerpo", "S"), FakeTizado.M(3, "Bolsillo", "U"), FakeTizado.M(4, "Manga", "m"), FakeTizado.M(5, "Cuello", "L") };
        var p = await AbreTizado();
        var tallas = p.FindAll("[data-zona='tallas'] [data-talla]").Select(e => e.GetAttribute("data-talla")).ToList();
        Assert.Equal(new[] { "L", "M", "S" }, tallas);                                      // la m minuscula y la M son la misma talla
        Assert.All(p.FindComponents<MudNumericField<int>>().Where(c => c.Markup.Contains("data-talla=")), c => Assert.Equal(1, c.Instance.Value));
        Assert.Contains("Talla S", p.Markup); Assert.DoesNotContain("Talla U", p.Markup);
        Assert.Contains("talla U (única)", p.Markup);
    }

    [Fact]
    public async Task AlEliminarLosMoldesDeUnaTalla_SuFilaDePrendasDesaparece_YLasOtrasConservanSuCantidad()
    {
        _f.Moldes = new() { FakeTizado.M(1, "Cuerpo", "S"), FakeTizado.M(2, "Cuerpo", "M") };
        var p = await Abre();
        p.FindAll(".mud-tab")[1].Click();
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-zona='tallas']")));
        await p.InvokeAsync(() => p.FindComponents<MudNumericField<int>>().First(c => c.Markup.Contains("data-talla=\"M\"")).Instance.ValueChanged.InvokeAsync(7));
        p.FindAll(".mud-tab")[0].Click();
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("tr[data-molde='1']")));
        p.FindAll("tr[data-molde='1'] [data-accion='eliminar-molde']")[0].Click();
        p.WaitForAssertion(() => Assert.Contains("Eliminar molde", p.Markup));
        p.FindAll("button").Last(b => b.TextContent.Trim() == "Eliminar").Click();
        p.WaitForAssertion(() => Assert.Single(p.FindAll("[data-tabla='moldes'] tr[data-molde]")));
        p.FindAll(".mud-tab")[1].Click();
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-zona='tallas']")));
        Assert.Equal(new[] { "M" }, p.FindAll("[data-zona='tallas'] [data-talla]").Select(e => e.GetAttribute("data-talla")).ToArray());
        Assert.Equal(7, p.FindComponents<MudNumericField<int>>().First(c => c.Markup.Contains("data-talla=\"M\"")).Instance.Value);
    }

    [Fact]
    public async Task SoloMoldesDeTallaU_PideLasPrendasEnUnaSolaFila()
    {
        _f.Moldes = new() { FakeTizado.M(1, "Cuello", "U") };
        var p = await AbreTizado();
        Assert.Equal(new[] { "U" }, p.FindAll("[data-zona='tallas'] [data-talla]").Select(e => e.GetAttribute("data-talla")).ToArray());
        Assert.Contains("Prendas (talla única)", p.Markup);
    }

    [Fact]
    public async Task SinMoldes_NoSePuedeCalcular()
    {
        var p = await AbreTizado();
        Assert.NotEmpty(p.FindAll("[data-aviso='sin-tallas']"));
        Assert.True(Btn(p, "calcular").HasAttribute("disabled")); Assert.True(Btn(p, "guardar-tizado").HasAttribute("disabled"));
    }

    [Fact]
    public async Task Calcular_EnviaLosParametrosYLasPrendasPorTalla()
    {
        _f.Moldes = new() { FakeTizado.M(1, "Cuerpo", "S"), FakeTizado.M(2, "Cuerpo", "M") };
        var p = await AbreTizado();
        await p.InvokeAsync(async () =>
        {
            await Num(p, "ancho").Instance.ValueChanged.InvokeAsync(1600);
            await Num(p, "separacion").Instance.ValueChanged.InvokeAsync(4);
            await Num(p, "margen").Instance.ValueChanged.InvokeAsync(12);
            await Num(p, "celda").Instance.ValueChanged.InvokeAsync(3);
            await NumI(p, "intentos").Instance.ValueChanged.InvokeAsync(9);
            await p.FindComponents<MudNumericField<int>>().First(c => c.Markup.Contains("data-talla=\"S\"")).Instance.ValueChanged.InvokeAsync(3);
            await p.FindComponents<MudNumericField<int>>().First(c => c.Markup.Contains("data-talla=\"M\"")).Instance.ValueChanged.InvokeAsync(2);
        });
        Btn(p, "calcular").Click();
        p.WaitForAssertion(() => Assert.Equal(1, _f.Veces("POST", "/disenos-tizado/calcular")));
        var b = _f.Cuerpo("/disenos-tizado/calcular")!.Value;
        Assert.Equal("D1", Prop(b, "COD_DISENO")); Assert.Equal("3", Prop(b, "NUM_VERSION"));
        Assert.Equal("1600", Prop(b, "VAL_ANCHO_TELA")); Assert.Equal("4", Prop(b, "VAL_SEPARACION")); Assert.Equal("12", Prop(b, "VAL_MARGEN")); Assert.Equal("3", Prop(b, "VAL_CELDA")); Assert.Equal("9", Prop(b, "NUM_INTENTOS"));
        var tallas = PropE(b, "Tallas").EnumerateArray().Select(t => (Prop(t, "DES_TALLA"), Prop(t, "NUM_PRENDAS"))).ToList();
        Assert.Equal(new[] { ("M", "2"), ("S", "3") }, tallas);
        Assert.Equal(0, _f.Veces("POST", "/disenos-tizado/guardar"));          // calcular no guarda
    }

    [Fact]
    public async Task Calcular_MuestraLargoAprovechamientoConsumoYElDibujo()
    {
        _f.Moldes = new() { FakeTizado.M(1, "Cuerpo") };
        var p = await AbreTizado();
        Btn(p, "calcular").Click();
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-resultado]")));
        Assert.Contains("2.35 m", Texto(p, "[data-chip='largo']").Replace(',', '.'));
        Assert.Contains("81.2", Texto(p, "[data-chip='eficiencia']").Replace(',', '.').Replace(" ", ""));
        Assert.Contains("0.469", Texto(p, "[data-chip='consumo']").Replace(',', '.'));
        Assert.Contains("6 pieza(s) · 5 prenda(s)", Texto(p, "[data-chip='piezas']"));
        Assert.NotEmpty(p.FindAll("[data-visor] svg polygon[data-pieza]"));
        Assert.Empty(p.FindAll("[data-chip='guardado']")); Assert.Empty(p.FindAll("[data-aviso='no-colocadas']"));
    }

    [Fact]
    public async Task Calcular_SiHayPiezasQueNoCaben_LasAvisaAgrupadas()
    {
        _f.Moldes = new() { FakeTizado.M(1, "Cuerpo") }; _f.Calculo = FakeTizado.Calc(4, 3);
        var p = await AbreTizado();
        Btn(p, "calcular").Click();
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-aviso='no-colocadas']")));
        var t = Texto(p, "[data-aviso='no-colocadas']");
        Assert.Contains("3 pieza(s)", t); Assert.Contains("Larga S ×3", t); Assert.Contains("No cabe en el ancho de tela", t);
    }

    [Fact]
    public async Task Calcular_SinConsumo_NoMuestraElChip()
    {
        _f.Moldes = new() { FakeTizado.M(1, "Cuerpo") }; _f.Calculo = FakeTizado.Calc(); _f.Calculo.VAL_CONSUMO_PRENDA = null;
        var p = await AbreTizado();
        Btn(p, "calcular").Click();
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-resultado]")));
        Assert.Empty(p.FindAll("[data-chip='consumo']"));
    }

    [Fact]
    public async Task Calcular_SiLaApiRechaza_MuestraElMotivo_YNoHayResultado()
    {
        _f.Moldes = new() { FakeTizado.M(1, "Cuerpo") }; _f.Falla = "El ancho de tela debe estar entre 100 y 5000 mm.";
        var p = await AbreTizado();
        Btn(p, "calcular").Click();
        p.WaitForAssertion(() => Assert.Contains("ancho de tela", Texto(p, "[data-error]")));
        Assert.Empty(p.FindAll("[data-resultado]"));
    }

    [Fact]
    public async Task Calcular_MuestraElProgreso_MientrasEspera()
    {
        _f.Moldes = new() { FakeTizado.M(1, "Cuerpo") };
        var p = await AbreTizado();
        Btn(p, "calcular").Click();
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-resultado]")));
        Assert.Empty(p.FindAll("[data-calculando]"));                      // al terminar el progreso desaparece
    }

    [Fact]
    public async Task ElZoom_CambiaElAnchoDelDibujo()
    {
        _f.Moldes = new() { FakeTizado.M(1, "Cuerpo") };
        var p = await AbreTizado();
        Btn(p, "calcular").Click();
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-resultado]")));
        Assert.Contains("width:100%", p.Find("[data-visor] > div").GetAttribute("style")!.Replace(" ", ""));
        await p.InvokeAsync(() => p.FindComponent<MudSlider<int>>().Instance.ValueChanged.InvokeAsync(300));
        p.WaitForAssertion(() => Assert.Contains("width:300%", p.Find("[data-visor] > div").GetAttribute("style")!.Replace(" ", "")));
        Assert.Contains("300 %", p.Markup);
    }

    // =================== guardar, ver, descargar y eliminar tizados ===================
    [Fact]
    public async Task GuardarTizado_ExigeNombre_EnviaLosParametros_YActualizaLaLista()
    {
        _f.Moldes = new() { FakeTizado.M(1, "Cuerpo") };
        var p = await AbreTizado();
        Assert.True(Btn(p, "guardar-tizado").HasAttribute("disabled"));
        await p.InvokeAsync(() => p.FindComponents<MudTextField<string>>().First(c => c.Markup.Contains("data-campo=\"nombre-tizado\"")).Instance.ValueChanged.InvokeAsync("Pedido 7"));
        p.WaitForAssertion(() => Assert.False(Btn(p, "guardar-tizado").HasAttribute("disabled")));
        Btn(p, "guardar-tizado").Click();
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-chip='guardado']")));
        var b = _f.Cuerpo("/disenos-tizado/guardar")!.Value;
        Assert.Equal("Pedido 7", Prop(b, "DES_TIZADO")); Assert.Equal("1500", Prop(b, "VAL_ANCHO_TELA"));
        Assert.Contains("N° 1", Texto(p, "[data-chip='guardado']"));
        p.WaitForAssertion(() => Assert.Single(p.FindAll("[data-tabla='tizados'] tr[data-tizado]")));
        Assert.Contains("Pedido 7", p.Find("tr[data-tizado='1']").TextContent);
        Assert.Equal(0, _f.Veces("POST", "/disenos-tizado/calcular"));
    }

    [Fact]
    public async Task GuardarTizado_SiLaApiRechaza_MuestraElMotivo_YNoAgregaALaLista()
    {
        _f.Moldes = new() { FakeTizado.M(1, "Cuerpo") }; _f.Falla = "El tizado necesita un nombre.";
        var p = await AbreTizado();
        await p.InvokeAsync(() => p.FindComponents<MudTextField<string>>().First(c => c.Markup.Contains("data-campo=\"nombre-tizado\"")).Instance.ValueChanged.InvokeAsync("X"));
        Btn(p, "guardar-tizado").Click();
        p.WaitForAssertion(() => Assert.Contains("necesita un nombre", Texto(p, "[data-error]")));
        Assert.Empty(p.FindAll("[data-tabla='tizados']"));
    }

    [Fact]
    public async Task SinTizadosGuardados_Avisa()
    {
        var p = await AbreTizado();
        Assert.NotEmpty(p.FindAll("[data-aviso='sin-tizados']"));
        Assert.Empty(p.FindAll("[data-tabla='tizados']"));
    }

    [Fact]
    public async Task TizadosGuardados_LaListaMuestraElMasRecientePrimero_ConLargoAprovechamientoYConsumo()
    {
        _f.Moldes = new() { FakeTizado.M(1, "Cuerpo") }; _f.Tizados = new() { FakeTizado.T(1, "Pedido 1"), FakeTizado.T(2, "Pedido 2") };
        var p = await AbreTizado();
        var filas = p.FindAll("[data-tabla='tizados'] tr[data-tizado]");
        Assert.Equal(new[] { "2", "1" }, filas.Select(f => f.GetAttribute("data-tizado")).ToArray());
        var t = filas[0].TextContent.Replace(',', '.').Replace(" ", "");
        Assert.Contains("Pedido2", t); Assert.Contains("S=3;M=2", t); Assert.Contains("1500", t); Assert.Contains("2.35", t); Assert.Contains("81.2", t); Assert.Contains("0.469", t);
    }

    [Fact]
    public async Task VerUnTizadoGuardado_MuestraSuResultado()
    {
        _f.Moldes = new() { FakeTizado.M(1, "Cuerpo") }; _f.Tizados = new() { FakeTizado.T(1) }; _f.Calculo = FakeTizado.Calc(21); _f.Calculo.NUM_SECUENCIA = 1;
        var p = await AbreTizado();
        p.Find("tr[data-tizado='1'] [data-accion='ver']").Click();
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-resultado]")));
        Assert.Contains("21 pieza(s)", Texto(p, "[data-chip='piezas']")); Assert.Contains("N° 1", Texto(p, "[data-chip='guardado']"));
        Assert.Contains(_f.Calls, c => c.M == "GET" && c.U == "/api/v1/ingenieria/disenos-tizado/tizados/D1/3/1");
    }

    [Fact]
    public async Task VerUnTizado_PideElDetalleDeESETizado()
    {
        _f.Moldes = new() { FakeTizado.M(1, "Cuerpo") }; _f.Tizados = new() { FakeTizado.T(1), FakeTizado.T(2), FakeTizado.T(3) }; _f.Calculo = FakeTizado.Calc(); _f.Calculo.NUM_SECUENCIA = 2;
        var p = await AbreTizado();
        p.Find("tr[data-tizado='2'] [data-accion='ver']").Click();
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-resultado]")));
        Assert.Contains(_f.Calls, c => c.M == "GET" && c.U == "/api/v1/ingenieria/disenos-tizado/tizados/D1/3/2");
        Assert.DoesNotContain(_f.Calls, c => c.M == "GET" && c.U == "/api/v1/ingenieria/disenos-tizado/tizados/D1/3/1");
    }

    [Fact]
    public async Task DescargarDxf_BajaElArchivoDeLaApi_YLoEntregaAlNavegador()
    {
        _f.Moldes = new() { FakeTizado.M(1, "Cuerpo") }; _f.Tizados = new() { FakeTizado.T(4) }; _f.DxfTexto = "CONTENIDO-DXF";
        var p = await AbreTizado();
        p.Find("tr[data-tizado='4'] [data-accion='dxf']").Click();
        p.WaitForAssertion(() => Assert.Contains(JSInterop.Invocations, i => i.Identifier == "blobViewer.descargar"));
        var inv = JSInterop.Invocations.Last(i => i.Identifier == "blobViewer.descargar");
        Assert.Equal("CONTENIDO-DXF", System.Text.Encoding.UTF8.GetString((byte[])inv.Arguments[0]!));
        Assert.Equal("tizado_D1_v3_4.dxf", inv.Arguments[1]); Assert.Equal("application/dxf", inv.Arguments[2]);
        Assert.Contains(_f.Calls, c => c.M == "GET" && c.U == "/api/v1/ingenieria/disenos-tizado/tizados/D1/3/4/dxf");
    }

    [Fact]
    public async Task DescargarDxf_SiLaApiRechaza_MuestraElError_YNoDescarga()
    {
        _f.Moldes = new() { FakeTizado.M(1, "Cuerpo") }; _f.Tizados = new() { FakeTizado.T(4) }; _f.Falla = "El tizado no existe.";
        var p = await AbreTizado();
        p.Find("[data-accion='dxf']").Click();
        p.WaitForAssertion(() => Assert.Contains("No se pudo descargar", Texto(p, "[data-error]")));
        Assert.DoesNotContain(JSInterop.Invocations, i => i.Identifier == "blobViewer.descargar");
    }

    [Fact]
    public async Task EliminarTizado_PideConfirmar_Publica_YLoQuitaDeLaLista_Y_DelVisorSiEraElMostrado()
    {
        _f.Moldes = new() { FakeTizado.M(1, "Cuerpo") }; _f.Tizados = new() { FakeTizado.T(1), FakeTizado.T(2) }; _f.Calculo = FakeTizado.Calc(); _f.Calculo.NUM_SECUENCIA = 2;
        var p = await AbreTizado();
        p.Find("tr[data-tizado='2'] [data-accion='ver']").Click();
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-resultado]")));
        p.Find("tr[data-tizado='2'] [data-accion='eliminar-tizado']").Click();
        p.WaitForAssertion(() => Assert.Contains("Eliminar tizado", p.Markup));
        Assert.Equal(0, _f.Veces("POST", "/tizados/eliminar"));
        p.FindAll("button").Last(b => b.TextContent.Trim() == "Eliminar").Click();
        p.WaitForAssertion(() => Assert.Equal(1, _f.Veces("POST", "/tizados/eliminar")));
        Assert.Equal("2", Prop(_f.Cuerpo("/tizados/eliminar")!.Value, "NUM_SECUENCIA"));
        p.WaitForAssertion(() => Assert.Single(p.FindAll("[data-tabla='tizados'] tr[data-tizado]")));
        Assert.Empty(p.FindAll("[data-resultado]"));                          // el que se veia ya no existe
    }

    [Fact]
    public async Task EliminarTizado_QueNoEraElMostrado_NoCierraElVisor()
    {
        _f.Moldes = new() { FakeTizado.M(1, "Cuerpo") }; _f.Tizados = new() { FakeTizado.T(1), FakeTizado.T(2) };
        var p = await AbreTizado();
        Btn(p, "calcular").Click();
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-resultado]")));
        p.Find("tr[data-tizado='1'] [data-accion='eliminar-tizado']").Click();
        p.WaitForAssertion(() => Assert.Contains("Eliminar tizado", p.Markup));
        p.FindAll("button").Last(b => b.TextContent.Trim() == "Eliminar").Click();
        p.WaitForAssertion(() => Assert.Single(p.FindAll("[data-tabla='tizados'] tr[data-tizado]")));
        Assert.NotEmpty(p.FindAll("[data-resultado]"));
    }

    [Fact]
    public async Task EliminarTizado_SiSeCancela_NoPublicaNada()
    {
        _f.Moldes = new() { FakeTizado.M(1, "Cuerpo") }; _f.Tizados = new() { FakeTizado.T(1) };
        var p = await AbreTizado();
        p.Find("[data-accion='eliminar-tizado']").Click();
        p.WaitForAssertion(() => Assert.Contains("Eliminar tizado", p.Markup));
        p.FindAll("button").Last(b => b.TextContent.Trim() == "Cancelar").Click();
        p.WaitForAssertion(() => Assert.DoesNotContain("Eliminar tizado", p.Markup));
        Assert.Equal(0, _f.Veces("POST", "/tizados/eliminar"));
    }

    // =================== vista previa y boton de la grilla ===================
    [Fact]
    public void Mini_DibujaElContorno_EnUnaCajaPropia_YDevuelveVacioSiNoSePuedeLeer()
    {
        var s = DisenosTizadoDialog.Mini("0,0 100,0 100,50 0,50");
        Assert.StartsWith("<svg", s); Assert.Contains("points=\"0,0 100,0 100,50 0,50\"", s);
        Assert.Contains("viewBox=\"-4 -4 108 58\"", s);                       // el margen es el 4 % del lado mayor
        Assert.Equal("", DisenosTizadoDialog.Mini("")); Assert.Equal("", DisenosTizadoDialog.Mini("0,0 10,0")); Assert.Equal("", DisenosTizadoDialog.Mini("0,0 10,0 10,x")); Assert.Equal("", DisenosTizadoDialog.Mini("0,0 10,0,3 10,10")); Assert.Equal("", DisenosTizadoDialog.Mini(null!));
        Assert.Equal(DisenosTizadoDialog.Mini("0,0 100,0 100,50"), DisenosTizadoDialog.Mini("  0,0   100,0\t100,50 "));
        var off = DisenosTizadoDialog.Mini("1000,2000 1100,2000 1100,2050");
        Assert.Contains("points=\"0,0 100,0 100,50\"", off);                   // se lleva al origen para que la caja siempre encuadre
    }

    // =================== etapa E+F: rayas y defectos ===================
    [Fact]
    public async Task Rayas_ElBotonAbreElDialogo_YAlAceptarMarcaLaFilaYGuardaTodosLosCampos()
    {
        _f.Moldes = new() { FakeTizado.M(1, "Cuerpo", "M", contorno: "0,0 100,0 100,50 0,50") };
        var p = await Abre();
        p.Find("[data-accion='rayas']").Click();
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-campo='coincide-x']")));
        p.Find("[data-campo='coincide-x']").Change(true);
        p.WaitForAssertion(() => Assert.Equal("50", p.Find("[data-campo='punto-x']").GetAttribute("value")));      // parte del centro de la caja
        p.Find("[data-campo='fase-x']").Change("12.5");
        p.Find("[data-accion='aceptar']").Click();
        p.WaitForAssertion(() => Assert.Equal("X", p.Find("[data-rayas='1']").TextContent.Trim()));
        Btn(p, "guardar-moldes").Click();
        p.WaitForAssertion(() => Assert.True(_f.Veces("POST", "/moldes/guardar") == 1));
        var m = PropE(_f.Cuerpo("/moldes/guardar")!.Value, "Moldes")[0];
        Assert.Equal("True", Prop(m, "FLG_COINCIDE_X")); Assert.Equal("False", Prop(m, "FLG_COINCIDE_Y"));
        Assert.Equal("50", Prop(m, "VAL_PUNTO_X")); Assert.Equal("25", Prop(m, "VAL_PUNTO_Y")); Assert.Equal("12.5", Prop(m, "VAL_FASE_X"));
        p.WaitForAssertion(() => Assert.True(_f.Moldes[0].FLG_COINCIDE_X));
    }

    [Fact]
    public async Task Rayas_UnMoldeConCoincidencia_LaMuestraYAlCambiarOtroCampoNoLaPierdeAlGuardar()
    {
        var m = FakeTizado.M(1, "Cuerpo", "M"); m.FLG_COINCIDE_Y = true; m.VAL_PUNTO_X = 10; m.VAL_PUNTO_Y = 20; m.VAL_FASE_Y = 3;
        _f.Moldes = new() { m };
        var p = await Abre();
        Assert.Equal("Y", p.Find("[data-rayas='1']").TextContent.Trim());
        p.Find("input[data-campo='nombre']").Change("Otro");
        Btn(p, "guardar-moldes").Click();
        p.WaitForAssertion(() => Assert.True(_f.Veces("POST", "/moldes/guardar") == 1));
        var e = PropE(_f.Cuerpo("/moldes/guardar")!.Value, "Moldes")[0];
        Assert.Equal("True", Prop(e, "FLG_COINCIDE_Y")); Assert.Equal("10", Prop(e, "VAL_PUNTO_X")); Assert.Equal("20", Prop(e, "VAL_PUNTO_Y")); Assert.Equal("3", Prop(e, "VAL_FASE_Y"));
    }

    [Fact]
    public async Task Rayas_EnUnaVersionBloqueada_NoSeOfreceElBoton()
    {
        _f.Moldes = new() { FakeTizado.M(1, "Cuerpo") }; _f.Bloqueada = true;
        var p = await Abre();
        Assert.Empty(p.FindAll("[data-accion='rayas']"));
    }

    [Fact]
    public async Task Calcular_ConRayasYDefectos_EnviaPeriodosYContornos()
    {
        _f.Moldes = new() { FakeTizado.M(1, "Cuerpo", "S") };
        var p = await AbreTizado();
        await p.InvokeAsync(async () =>
        {
            await Num(p, "periodo-x").Instance.ValueChanged.InvokeAsync(150);
            await Num(p, "periodo-y").Instance.ValueChanged.InvokeAsync(90);
            await Num(p, "nd-x").Instance.ValueChanged.InvokeAsync(300);
            await Num(p, "nd-y").Instance.ValueChanged.InvokeAsync(100);
            await Num(p, "nd-largo").Instance.ValueChanged.InvokeAsync(40);
            await Num(p, "nd-ancho").Instance.ValueChanged.InvokeAsync(60.5);
        });
        p.Find("[data-campo='nd-nombre']").Change("Mancha");
        Btn(p, "agregar-defecto").Click();
        Assert.Single(p.FindAll("[data-defecto='Mancha']"));
        Btn(p, "calcular").Click();
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-resultado]")));
        var b = _f.Cuerpo("/calcular")!.Value;
        Assert.Equal("150", Prop(b, "VAL_PERIODO_X")); Assert.Equal("90", Prop(b, "VAL_PERIODO_Y"));
        var d = PropE(b, "Defectos")[0];
        Assert.Equal("Mancha", Prop(d, "DES_DEFECTO")); Assert.Equal("300,100 340,100 340,160.5 300,160.5", Prop(d, "DES_CONTORNO"));
    }

    [Fact]
    public async Task Defectos_SinMedidas_NoAgrega_YSePuedeQuitar()
    {
        _f.Moldes = new() { FakeTizado.M(1, "Cuerpo") };
        var p = await AbreTizado();
        await p.InvokeAsync(async () => await Num(p, "nd-largo").Instance.ValueChanged.InvokeAsync(0));
        Btn(p, "agregar-defecto").Click();
        Assert.Empty(p.FindAll("[data-accion='quitar-defecto']"));
        await p.InvokeAsync(async () => await Num(p, "nd-largo").Instance.ValueChanged.InvokeAsync(10));
        Btn(p, "agregar-defecto").Click(); Btn(p, "agregar-defecto").Click();
        Assert.Equal(2, p.FindAll("[data-accion='quitar-defecto']").Count);
        Assert.NotEmpty(p.FindAll("[data-defecto='Defecto 1']")); Assert.NotEmpty(p.FindAll("[data-defecto='Defecto 2']"));
        p.FindAll("[data-accion='quitar-defecto']")[0].Click();
        Assert.Single(p.FindAll("[data-accion='quitar-defecto']"));
    }

    [Fact]
    public async Task Defectos_ConCoordenadasNegativas_NoAgrega()
    {
        _f.Moldes = new() { FakeTizado.M(1, "Cuerpo") };
        var p = await AbreTizado();
        await p.InvokeAsync(async () => await Num(p, "nd-x").Instance.ValueChanged.InvokeAsync(-5));
        Btn(p, "agregar-defecto").Click();
        Assert.Empty(p.FindAll("[data-accion='quitar-defecto']"));
    }

    [Fact]
    public async Task Resultado_ConCoincidenciaYDefectos_MuestraElDesvioMaximoYLosDefectos()
    {
        _f.Moldes = new() { FakeTizado.M(1, "Cuerpo") };
        _f.Calculo.Geometria.Colocadas = new() { new DisenosTizadoPieza { PiezaId = "1", Puntos = "0,0 10,0 10,10", Coincidencia = "5,5", ErrorX = 0.0004 }, new DisenosTizadoPieza { PiezaId = "1", Puntos = "20,0 30,0 30,10", Coincidencia = "25,5", ErrorX = 0.2, ErrorY = 0.1 } };
        _f.Calculo.Geometria.Defectos = new() { new DisenosTizadoDefecto { Nombre = "Mancha", Puntos = "0,0 1,0 1,1" }, new DisenosTizadoDefecto { Nombre = "Agujero", Puntos = "0,0 1,0 1,1" } };
        var p = await AbreTizado();
        Btn(p, "calcular").Click();
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-aviso='coincidencia']")));
        Assert.Contains("2 pieza(s)", Texto(p, "[data-aviso='coincidencia']")); Assert.Contains("0.2", Texto(p, "[data-aviso='coincidencia']").Replace(',', '.'));
        Assert.Contains("Mancha, Agujero", Texto(p, "[data-aviso='defectos']"));
    }

    [Fact]
    public async Task Resultado_SinRayasNiDefectos_NoMuestraLosAvisos()
    {
        _f.Moldes = new() { FakeTizado.M(1, "Cuerpo") };
        var p = await AbreTizado();
        Btn(p, "calcular").Click();
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-resultado]")));
        Assert.Empty(p.FindAll("[data-aviso='coincidencia']")); Assert.Empty(p.FindAll("[data-aviso='defectos']"));
    }

    // =================== etapa G: ajuste manual desde la lista de tizados ===================
    [Fact]
    public async Task Ajustar_AbreElEditorDelTizado_ConSuNumero()
    {
        _f.Moldes = new() { FakeTizado.M(1, "Cuerpo") };
        _f.Tizados = new() { FakeTizado.T(4, "Pedido 4") };
        var p = await AbreTizado();
        p.Find("[data-tizado='4'] [data-accion='ajustar']").Click();
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-visor] svg")));
        Assert.Contains(_f.Calls, c => c.M == "GET" && c.U.EndsWith("/tizados/D1/3/4"));
        Assert.Equal("Pedido 4 (ajustado)", p.Find("[data-campo='nombre-ajuste']").GetAttribute("value"));
    }


    // =================== formas que no son rectangulos ===================
    static async Task PlantillaForma(IRenderedComponent<MudDialogProvider> p, string clave) => await p.InvokeAsync(async () => await p.FindComponent<MudSelect<string>>().Instance.ValueChanged.InvokeAsync(clave));

    [Fact]
    public async Task DibujarForma_AgregaUnMoldeNuevoConSuContorno_YSeGuarda()
    {
        var p = await Abre();
        p.Find("[data-accion='forma']").Click();
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-zona='plantilla']")));
        await PlantillaForma(p, "trap");
        p.Find("[data-accion='aceptar']").Click();
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-nuevo='Forma 1']")));
        Assert.Contains("120", Texto(p, "[data-nuevo='Forma 1'] td.mono"));
        Btn(p, "guardar-seleccion").Click();
        p.WaitForAssertion(() => Assert.Equal(1, _f.Veces("POST", "/moldes/guardar")));
        var m = PropE(_f.Cuerpo("/moldes/guardar")!.Value, "Moldes")[0];
        Assert.Equal("Forma 1", Prop(m, "DES_MOLDE")); Assert.Equal("0,0 120,0 95,60 25,60", Prop(m, "DES_CONTORNO"));
    }

    [Fact]
    public async Task DibujarForma_ConElNombreEscrito_LoUsa_YCancelarNoAgrega()
    {
        var p = await Abre();
        p.Find("[data-campo='rect-nombre']").Change("Cuello");
        p.Find("[data-accion='forma']").Click();
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-zona='plantilla']")));
        await PlantillaForma(p, "elipse");
        p.Find("[data-accion='aceptar']").Click();
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-nuevo='Cuello']")));
        p.Find("[data-accion='forma']").Click();
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-zona='plantilla']")));
        p.Find("[data-accion='cancelar']").Click();
        p.WaitForAssertion(() => Assert.Empty(p.FindAll("[data-zona='plantilla']")));
        Assert.Single(p.FindAll("[data-tabla='nuevos'] tr[data-nuevo]"));
    }

    [Fact]
    public async Task EditarLaFormaDeUnMolde_ConPuntoNuevo_LoMarcaParaGuardar_YEnviaElContornoNuevo()
    {
        _f.Moldes = new() { FakeTizado.M(1, "Cuerpo", "M", contorno: "0,0 100,0 100,50 0,50") };
        var p = await Abre();
        p.Find("[data-accion='forma-molde']").Click();
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-zona='plantilla']")));
        Assert.Equal(4, p.FindAll("[data-tabla='puntos'] tr[data-fila]").Count);
        p.FindAll("[data-accion='insertar']")[1].Click();                                // entre el 2 y el 3 (el lado derecho): (100,25)
        p.Find("[data-accion='aceptar']").Click();
        p.WaitForAssertion(() => Assert.False(Btn(p, "guardar-moldes").HasAttribute("disabled")));
        Btn(p, "guardar-moldes").Click();
        p.WaitForAssertion(() => Assert.Equal(1, _f.Veces("POST", "/moldes/guardar")));
        Assert.Equal("0,0 100,0 100,25 100,50 0,50", Prop(PropE(_f.Cuerpo("/moldes/guardar")!.Value, "Moldes")[0], "DES_CONTORNO"));
    }

    [Fact]
    public async Task EditarLaFormaDeUnMolde_Cancelar_NoLoMarca()
    {
        _f.Moldes = new() { FakeTizado.M(1, "Cuerpo") };
        var p = await Abre();
        p.Find("[data-accion='forma-molde']").Click();
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-zona='plantilla']")));
        p.Find("[data-accion='cancelar']").Click();
        p.WaitForAssertion(() => Assert.Empty(p.FindAll("[data-zona='plantilla']")));
        Assert.True(Btn(p, "guardar-moldes").HasAttribute("disabled"));
    }

    [Fact]
    public async Task EnUnaVersionBloqueada_NoSeOfreceEditarLaForma()
    {
        _f.Moldes = new() { FakeTizado.M(1, "Cuerpo") }; _f.Bloqueada = true;
        var p = await Abre();
        Assert.Empty(p.FindAll("[data-accion='forma-molde']"));
    }

    [Fact]
    public async Task DibujarUnDefecto_ConForma_SeAgregaYSeEnviaAlCalcular()
    {
        _f.Moldes = new() { FakeTizado.M(1, "Cuerpo", "S") };
        var p = await AbreTizado();
        p.Find("[data-accion='forma-defecto']").Click();
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-campo='desde-x']")));
        await p.InvokeAsync(async () => { await p.FindComponents<MudNumericField<double>>().First(c => c.Markup.Contains("data-campo=\"desde-x\"")).Instance.ValueChanged.InvokeAsync(300); });
        await PlantillaForma(p, "tri");
        p.Find("[data-accion='aceptar']").Click();
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-defecto='Defecto 1']")));
        Btn(p, "calcular").Click();
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-resultado]")));
        var d = PropE(_f.Cuerpo("/calcular")!.Value, "Defectos")[0];
        Assert.Equal("300,0 400,0 350,80", Prop(d, "DES_CONTORNO"));
    }

    [Fact]
    public async Task EditarUnDefecto_CambiaSuContorno()
    {
        _f.Moldes = new() { FakeTizado.M(1, "Cuerpo", "S") };
        var p = await AbreTizado();
        await p.InvokeAsync(async () => { await Num(p, "nd-x").Instance.ValueChanged.InvokeAsync(300); await Num(p, "nd-y").Instance.ValueChanged.InvokeAsync(100); });
        Btn(p, "agregar-defecto").Click();
        p.Find("[data-accion='editar-defecto']").Click();
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-zona='plantilla']")));
        Assert.Equal(4, p.FindAll("[data-tabla='puntos'] tr[data-fila]").Count);
        p.FindAll("[data-accion='quitar-punto']")[3].Click();                            // queda un triangulo
        p.Find("[data-accion='aceptar']").Click();
        p.WaitForAssertion(() => Assert.Empty(p.FindAll("[data-zona='plantilla']")));
        Assert.Equal("300,100 400,100 400,200", p.Find("[data-campo='def-contorno']").GetAttribute("value"));
    }

    [Fact]
    public async Task EditarLaForma_ConUnContornoFueraDelOrigen_SeLlevaAlOrigen()
    {
        _f.Moldes = new() { FakeTizado.M(1, "Cuerpo", "M", contorno: "0,0 100,0 100,50 0,50") };
        var p = await Abre();
        p.Find("[data-accion='forma-molde']").Click();
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-zona='plantilla']")));
        p.Find("[data-campo='texto']").Change("200,300 260,300 230,340");
        p.Find("[data-accion='usar-texto']").Click();
        p.Find("[data-accion='aceptar']").Click();
        p.WaitForAssertion(() => Assert.False(Btn(p, "guardar-moldes").HasAttribute("disabled")));
        Btn(p, "guardar-moldes").Click();
        p.WaitForAssertion(() => Assert.Equal(1, _f.Veces("POST", "/moldes/guardar")));
        Assert.Equal("0,0 60,0 30,40", Prop(PropE(_f.Cuerpo("/moldes/guardar")!.Value, "Moldes")[0], "DES_CONTORNO"));
    }

    [Fact]
    public async Task AjustarYGuardar_RecargaLaListaDeTizados()
    {
        _f.Moldes = new() { FakeTizado.M(1, "Cuerpo") };
        _f.Tizados = new() { FakeTizado.T(4, "Pedido 4") };
        _f.Calculo.Geometria.Colocadas = new() { new DisenosTizadoPieza { PiezaId = "1", Nombre = "Cuerpo", Talla = "M", Copia = 1, Puntos = "10,10 110,10 110,60 10,60" } };
        _f.Calculo.Geometria.Separacion = 3; _f.Calculo.Geometria.Margen = 10;
        var p = await AbreTizado();
        int lecturas = _f.Calls.Count(c => c.M == "GET" && c.U.EndsWith("/tizados/D1/3"));
        p.Find("[data-tizado='4'] [data-accion='ajustar']").Click();
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-visor] svg")));
        await p.Find("[data-pieza='0']").PointerDownAsync(new Microsoft.AspNetCore.Components.Web.PointerEventArgs { ClientX = 1, ClientY = 1, PointerId = 1 });
        Btn(p, "der").Click();
        Btn(p, "guardar").Click();
        p.WaitForAssertion(() => Assert.True(_f.Calls.Count(c => c.M == "GET" && c.U.EndsWith("/tizados/D1/3")) > lecturas));
    }
}
