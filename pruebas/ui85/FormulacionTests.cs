using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Blazored.LocalStorage;
using Bunit;
using KONSolutions.Shared.Models.Formulacion;
using KONSolutions.Web.Pages.Ingenieria;
using KONSolutions.Web.Pages.Shared;
using KONSolutions.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using Xunit;

// Constructor de formulas (script SQL 145): el constructor, el mantenimiento de componentes y el de elementos, contra una API simulada.
public class FakeFormulacion : HttpMessageHandler
{
    public List<(string M, string U, JsonElement? B)> Calls = new();
    public string? MensajeFalla;                 // si se pone, las escrituras fallan con este mensaje
    public FormulaValidacion Validacion = new() { ES_VALIDA = true };
    int _sig = 3;

    static FormulaFicha Fi(short n, string comp, string dcomp, string elem, string alias, string? tec, string? valor, string back, string sin) => new()
    {
        NUM_FORMULACION = 1, NUM_SECUENCIA = n, COD_COMPONENTE_FORMULACION = comp, DES_COMPONENTE_FORMULACION = dcomp, COD_ELEMENTO_FORMULACION = elem, COD_ALIAS_FORMULACION = alias,
        DES_ELEMENTO_FORMULACION = tec, DES_FORMULA = valor ?? tec, DES_ALIAS = valor ?? alias, DES_BACKCOLOR = back, DES_FORECOLOR = "#ffffff", COD_SINTAXIS = sin
    };

    // ( Consumo x 100 / ( 100 - %Merma ) )
    public List<FormulaFicha> Fichas = new()
    {
        Fi(1, "OPELG", "OPERADORES LOGICOS", "00003", "(", "(", null, "#2a6ad4", "ABR"), Fi(2, "CONSM", "DATOS DE CONSUMO", "00002", "Consumo", "CAN_CONSUMO", null, "#1f9d55", "VAL"),
        Fi(3, "OPEMT", "OPERADORES MATEMATICOS", "00003", "x", "*", null, "#e07b1f", "OPE"), Fi(4, "CONST", "CONSTANTES", "00002", "ENT", "ENTERO", "100", "#d93025", "VAL"),
        Fi(5, "OPEMT", "OPERADORES MATEMATICOS", "00004", "/", "/", null, "#e07b1f", "OPE"), Fi(6, "OPELG", "OPERADORES LOGICOS", "00003", "(", "(", null, "#2a6ad4", "ABR"),
        Fi(7, "CONST", "CONSTANTES", "00002", "ENT", "ENTERO", "100", "#d93025", "VAL"), Fi(8, "OPEMT", "OPERADORES MATEMATICOS", "00002", "-", "-", null, "#e07b1f", "OPE"),
        Fi(9, "CONSM", "DATOS DE CONSUMO", "00005", "%Merma", "TASA DE MERMA", null, "#1f9d55", "VAL"), Fi(10, "OPELG", "OPERADORES LOGICOS", "00004", ")", ")", null, "#2a6ad4", "CIE"),
        Fi(11, "OPELG", "OPERADORES LOGICOS", "00004", ")", ")", null, "#2a6ad4", "CIE")
    };

    public List<FormulaCabecera> Formulas = new()
    {
        new() { NUM_FORMULACION = 1, DES_FORMULACION = "CONSUMO CON MERMA", COD_GRUPO_PRODUCTO_CONSUMO = "G1", COD_FAMILIA_PRODUCTO_CONSUMO = "F1", COD_SUB_FAMILIA_PRODUCTO_CONSUMO = "S1", DES_SUB_FAMILIA_CONSUMO = "TELA CRUDA",
                COD_GRUPO_PRODUCTO = "G2", COD_FAMILIA_PRODUCTO = "F2", COD_SUB_FAMILIA_PRODUCTO = "S2", DES_SUB_FAMILIA_PRODUCTO = "HILADO", DES_ALIAS = "(Consumo x 100 / (100 - %Merma))", DES_FORMULA = "( CAN_CONSUMO * 100 / ( 100 - TASA DE MERMA ) )",
                COD_TIPO_ESTADO = 9, COD_ESTADO = 1, DES_ESTADO = "ACTIVA", DES_ESTADO_BACKCOLOR = "#16a34a", DES_ESTADO_FORECOLOR = "#ffffff", CAN_FICHAS = 11,
                COD_USUARIO_REGISTRO = "PEDRO", COD_ESTACION_REGISTRO = "LOCALHOST", FEC_REGISTRO = new DateTime(2026, 11, 1), COD_USUARIO_ACTUALIZACION = "PEDRO", FEC_ACTUALIZACION = new DateTime(2026, 11, 2) },
        new() { NUM_FORMULACION = 2, DES_FORMULACION = "CONSUMO POR RENDIMIENTO", DES_ALIAS = "(Cantidad / Rendimiento)", COD_TIPO_ESTADO = 9, COD_ESTADO = 0, DES_ESTADO = "EN DESARROLLO", CAN_FICHAS = 5 }
    };

    static PaletaFicha P(string comp, string dcomp, string back, string? elem, string? alias, string? tec, string sin = "VAL", bool espC = false) => new()
    {
        COD_COMPONENTE_FORMULACION = comp, DES_COMPONENTE_FORMULACION = dcomp, DES_BACKCOLOR = back, DES_FORECOLOR = "#ffffff", FLG_ESPECIFICACIONES_CONSUMO = espC,
        COD_ELEMENTO_FORMULACION = elem, COD_ALIAS_FORMULACION = alias, DES_ELEMENTO_FORMULACION = tec, COD_SINTAXIS = sin
    };

    static PaletaFicha Esp(string comp, string dcomp, string back, string espCod, string singular, string? elem = "00001") => new()
    {
        COD_COMPONENTE_FORMULACION = comp, DES_COMPONENTE_FORMULACION = dcomp, DES_BACKCOLOR = back, DES_FORECOLOR = "#ffffff", FLG_ESPECIFICACIONES = true,
        COD_ELEMENTO_FORMULACION = elem, DES_ELEMENTO_FORMULACION = elem is null ? null : singular, COD_ALIAS_FORMULACION = singular, COD_SINTAXIS = "VAL",
        COD_ESPECIFICACION_TECNICA = espCod, DES_ESPECIFICACION_TECNICA_SINGULAR = singular
    };

    public List<PaletaFicha> Paleta = new()
    {
        P("CONSM", "DATOS DE CONSUMO", "#1f9d55", "00002", "Consumo", "CAN_CONSUMO"), P("CONSM", "DATOS DE CONSUMO", "#1f9d55", "00005", "%Merma", "TASA DE MERMA"),
        P("CONST", "CONSTANTES", "#d93025", "00002", "ENT", "ENTERO"), P("CONST", "CONSTANTES", "#d93025", "00009", "NUM", "NUMERICO"),
        P("OPEMT", "OPERADORES MATEMATICOS", "#e07b1f", "00001", "+", "+", "OPE"), P("OPEMT", "OPERADORES MATEMATICOS", "#e07b1f", "00002", "-", "-", "OPE"), P("OPEMT", "OPERADORES MATEMATICOS", "#e07b1f", "00003", "x", "*", "OPE"),
        P("OPELG", "OPERADORES LOGICOS", "#2a6ad4", "00003", "(", "(", "ABR"), P("OPELG", "OPERADORES LOGICOS", "#2a6ad4", "00004", ")", ")", "CIE"),
        P("ESPEC", "ESPECIFICACIONES TECNICAS", "#0b8fc7", null, null, null, "VAL", true),      // componente de especificaciones sin fichas (ninguna aplica a la sub familia)
        // especificaciones de la estructura: dos de la sub familia comparten el mismo elemento portador y se distinguen por su codigo; se muestra su descripcion en singular
        Esp("ESPST", "ESPECIFICACIONES DE LA ESTRUCTURA", "#7c3aed", "GRAMAJE", "Gramaje"), Esp("ESPST", "ESPECIFICACIONES DE LA ESTRUCTURA", "#7c3aed", "ANCHO", "Ancho util", null)      // esta no tiene elemento portador: la base lo resuelve al grabar
    };

    public List<FormulacionComponente> Comps = new()
    {
        new() { COD_COMPONENTE_FORMULACION = "CONSM", DES_COMPONENTE_FORMULACION = "DATOS DE CONSUMO", FLG_ELEMENTOS = true, FLG_HABILITADO = true, DES_BACKCOLOR = "#1f9d55", DES_FORECOLOR = "#ffffff", CAN_ELEMENTOS = 7,
                COD_USUARIO_REGISTRO = "PEDRO", FEC_REGISTRO = new DateTime(2026, 11, 1) },
        new() { COD_COMPONENTE_FORMULACION = "ESPEC", DES_COMPONENTE_FORMULACION = "ESPECIFICACIONES TECNICAS", FLG_ELEMENTOS = true, FLG_HABILITADO = true, FLG_ESPECIFICACIONES_CONSUMO = true, DES_BACKCOLOR = "#0b8fc7", DES_FORECOLOR = "#ffffff", CAN_ELEMENTOS = 0 },
        new() { COD_COMPONENTE_FORMULACION = "ESPPR", DES_COMPONENTE_FORMULACION = "ESPECIFICACIONES ORDENES PRODUCCION", FLG_ELEMENTOS = true, FLG_HABILITADO = false, DES_BACKCOLOR = "#8d8d8d", DES_FORECOLOR = "#ffffff", CAN_ELEMENTOS = 0 }
    };

    public List<FormulacionElemento> Elementos = new()
    {
        new() { COD_COMPONENTE_FORMULACION = "CONSM", DES_COMPONENTE_FORMULACION = "DATOS DE CONSUMO", COD_ELEMENTO_FORMULACION = "00002", DES_ELEMENTO_FORMULACION = "CAN_CONSUMO", COD_ALIAS_FORMULACION = "Consumo", FLG_HABILITADO = true, DES_BACKCOLOR = "#1f9d55", DES_FORECOLOR = "#ffffff",
                CAN_USOS = 3, COD_USUARIO_REGISTRO = "PEDRO", FEC_REGISTRO = new DateTime(2026, 11, 1) },
        new() { COD_COMPONENTE_FORMULACION = "CONSM", DES_COMPONENTE_FORMULACION = "DATOS DE CONSUMO", COD_ELEMENTO_FORMULACION = "00005", DES_ELEMENTO_FORMULACION = "TASA DE MERMA", COD_ALIAS_FORMULACION = "%Merma", FLG_HABILITADO = true, DES_BACKCOLOR = "#1f9d55", DES_FORECOLOR = "#ffffff", CAN_USOS = 0 },
        new() { COD_COMPONENTE_FORMULACION = "ESPEC", DES_COMPONENTE_FORMULACION = "ESPECIFICACIONES TECNICAS", COD_ELEMENTO_FORMULACION = "00001", DES_ELEMENTO_FORMULACION = "Gramaje", COD_ALIAS_FORMULACION = "Gramaje", COD_TIPO_ESPECIFICACION_TECNICA = "ENT", DES_TIPO_ESPECIFICACION_TECNICA = "ENTERO", FLG_HABILITADO = true, DES_BACKCOLOR = "#0b8fc7", DES_FORECOLOR = "#ffffff", CAN_USOS = 0 }
    };

    static JsonElement G(JsonElement e, string n) => e.EnumerateObject().First(x => string.Equals(x.Name, n, StringComparison.OrdinalIgnoreCase)).Value;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage r, CancellationToken ct)
    {
        var u = r.RequestUri!.PathAndQuery;
        JsonElement? body = r.Content is null ? null : JsonDocument.Parse(await r.Content.ReadAsStringAsync(ct)).RootElement.Clone();
        Calls.Add((r.Method.Method, u, body));
        HttpResponseMessage Ok(object? data) => new(HttpStatusCode.OK) { Content = JsonContent.Create(new { Success = true, Data = data }) };
        HttpResponseMessage Mal(string msg) => new(HttpStatusCode.BadRequest) { Content = JsonContent.Create(new { Success = false, Message = msg }) };
        var path = u.Split('?')[0];
        if (r.Method == HttpMethod.Get)
        {
            if (path.EndsWith("/formulacion/formulas")) return Ok(Formulas);
            if (path.EndsWith("/formulacion/formulas/paleta")) return Ok(Paleta);
            if (path.Contains("/formulacion/formulas/")) { var n = int.Parse(path.Split('/').Last()); return Ok(new FormulaDetalle { Cabecera = Formulas.First(f => f.NUM_FORMULACION == n), Fichas = n == 1 ? Fichas : new() }); }
            if (path.EndsWith("/formulacion/componentes")) return Ok(Comps);
            if (path.EndsWith("/formulacion/elementos"))
            {
                var q = System.Web.HttpUtility.ParseQueryString(r.RequestUri.Query)["componente"];
                return Ok(Elementos.Where(e => q is null || e.COD_COMPONENTE_FORMULACION == q).ToList());
            }
            if (path.Contains("/formulacion/elementos/")) { var p = path.Split('/'); return Ok(Elementos.First(e => e.COD_COMPONENTE_FORMULACION == p[^2] && e.COD_ELEMENTO_FORMULACION == p[^1])); }
            return Ok(new object[0]);
        }
        if (path.EndsWith("/formulas/validar")) return Ok(Validacion);
        if (MensajeFalla != null) return Mal(MensajeFalla);
        if (r.Method == HttpMethod.Post && path.EndsWith("/copiar")) { var n = _sig++; Formulas.Add(new() { NUM_FORMULACION = n, DES_FORMULACION = "CONSUMO CON MERMA (copia)", DES_ESTADO = "EN DESARROLLO", COD_ESTADO = 0, CAN_FICHAS = 11 }); return Ok(new FormulacionEscritura { Codigo = n.ToString() }); }
        if (r.Method == HttpMethod.Post && path.EndsWith("/formulacion/formulas")) { var n = _sig++; Formulas.Add(new() { NUM_FORMULACION = n, DES_FORMULACION = G(body!.Value, "DES_FORMULACION").GetString(), DES_ESTADO = "EN DESARROLLO", COD_ESTADO = 0 }); return Ok(new FormulacionEscritura { Codigo = n.ToString() }); }
        if (r.Method == HttpMethod.Put && path.EndsWith("/formulacion/formulas")) return Ok(new FormulacionEscritura { Codigo = G(body!.Value, "NUM_FORMULACION").GetInt32().ToString() });
        if (r.Method == HttpMethod.Delete && path.Contains("/formulacion/formulas/")) { var n = int.Parse(path.Split('/').Last()); Formulas.RemoveAll(f => f.NUM_FORMULACION == n); return Ok(new FormulacionEscritura { Codigo = n.ToString() }); }
        if (r.Method == HttpMethod.Post && path.EndsWith("/formulacion/componentes")) { var cod = G(body!.Value, "COD_COMPONENTE_FORMULACION").GetString()!; Comps.Add(new() { COD_COMPONENTE_FORMULACION = cod, DES_COMPONENTE_FORMULACION = G(body.Value, "DES_COMPONENTE_FORMULACION").GetString() }); return Ok(new FormulacionEscritura { Codigo = cod }); }
        if (r.Method == HttpMethod.Put && path.EndsWith("/formulacion/componentes")) return Ok(new FormulacionEscritura { Codigo = G(body!.Value, "COD_COMPONENTE_FORMULACION").GetString()! });
        if (r.Method == HttpMethod.Delete && path.Contains("/formulacion/componentes/")) { Comps.RemoveAll(c => path.EndsWith("/" + c.COD_COMPONENTE_FORMULACION)); return Ok(new FormulacionEscritura()); }
        if (r.Method == HttpMethod.Post && path.EndsWith("/formulacion/elementos")) { return Ok(new FormulacionEscritura { Codigo = "00077" }); }
        if (r.Method == HttpMethod.Put && path.EndsWith("/formulacion/elementos")) return Ok(new FormulacionEscritura { Codigo = G(body!.Value, "COD_ELEMENTO_FORMULACION").GetString()! });
        if (r.Method == HttpMethod.Delete && path.Contains("/formulacion/elementos/")) return Ok(new FormulacionEscritura());
        return Ok(true);
    }
}

public abstract class FormulacionBase : TestContext
{
    protected readonly FakeFormulacion F = new();
    protected IRenderedComponent<MudDialogProvider>? Prov;

    protected void Preparar()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton(new HttpClient(F) { BaseAddress = new Uri("http://x/") });
        Services.AddBlazoredLocalStorage();
        Services.AddScoped<ErrorLogService>(); Services.AddScoped<ConfigService>(); Services.AddScoped<EtiquetasService>();
        Services.AddSingleton<Microsoft.Extensions.Configuration.IConfiguration>(new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build());
        Services.AddScoped<AccessLogService>(); Services.AddScoped<MenuService>(); Services.AddScoped<ControlCambiosClientService>();
        Services.AddSingleton(new SeguridadContextService(new FakeAuthConAuditoria(), null!));     // el PanelAuditoria pregunta si el perfil ve la auditoria
        ComponentFactories.AddStub<AccionesBotones>();
        ComponentFactories.AddStub<ColorConSwatch>();                                              // el selector de color trae su propio banco de colores
        RenderComponent<MudPopoverProvider>(); RenderComponent<MudSnackbarProvider>();
        Prov = RenderComponent<MudDialogProvider>();
    }

    protected static JsonElement Prop(JsonElement e, string n) => e.EnumerateObject().First(x => string.Equals(x.Name, n, StringComparison.OrdinalIgnoreCase)).Value;
    protected int Cuantas(string metodo, string fin) => F.Calls.Count(c => c.M == metodo && c.U.Split('?')[0].EndsWith(fin));
    protected (string M, string U, JsonElement? B) Ultima(string metodo, string fin) => F.Calls.Last(c => c.M == metodo && c.U.Split('?')[0].EndsWith(fin));
    protected static bool Deshabilitado(AngleSharp.Dom.IElement e) => e.HasAttribute("disabled") || (e.ClassName ?? "").Contains("mud-disabled");
}

public class FormulasTests : FormulacionBase
{
    IRenderedComponent<Formulas> Pagina()
    {
        Preparar();
        var pg = RenderComponent<Formulas>();
        pg.WaitForAssertion(() => Assert.Contains("CONSUMO CON MERMA", pg.Markup));
        return pg;
    }

    async Task<IRenderedComponent<Formulas>> Elegida(int num = 1)
    {
        var pg = Pagina();
        await pg.InvokeAsync(() => pg.Find($"[data-formula='{num}']").Click());
        pg.WaitForAssertion(() => Assert.NotEmpty(pg.FindAll("[data-zona='lienzo']")));
        return pg;
    }

    string Lienzo(IRenderedComponent<Formulas> pg) => string.Join("|", pg.FindAll("[data-ficha]").Select(x => x.QuerySelector("input") is { } i ? i.GetAttribute("value") + x.QuerySelector("small")!.TextContent.Trim() : x.TextContent.Trim()));
    async Task Comp(IRenderedComponent<Formulas> pg, string cod) { await pg.InvokeAsync(() => pg.Find($"[data-comp='{cod}']").Click()); }
    async Task El(IRenderedComponent<Formulas> pg, string cod) { await pg.InvokeAsync(() => pg.Find($"[data-el='{cod}']").Click()); }
    async Task Accion(IRenderedComponent<Formulas> pg, string a) { await pg.InvokeAsync(() => pg.Find($"[data-accion='{a}']").Click()); }

    [Fact]
    public void ListaLasFormulas_ConSuSubFamilia_ElTextoDePantalla_YElEstado()
    {
        var pg = Pagina();
        var f1 = pg.Find("[data-formula='1']").TextContent;
        Assert.Contains("TELA CRUDA", f1); Assert.Contains("HILADO", f1); Assert.Contains("(Consumo x 100 / (100 - %Merma))", f1); Assert.Contains("ACTIVA", f1);
        Assert.Contains("Cualquier consumo", pg.Find("[data-formula='2']").TextContent);
        Assert.NotEmpty(pg.FindAll("[data-vacio-detalle]"));                                      // todavia no hay ninguna elegida
        Assert.Empty(pg.FindAll("[data-zona='lienzo']"));
    }

    [Fact]
    public void ElFiltroDeLaLista_BuscaPorNombreAliasYSubFamilia()
    {
        var pg = Pagina();
        pg.Find("[data-campo='buscar']").Input("rendimiento");
        Assert.Single(pg.FindAll("[data-formula]")); Assert.NotEmpty(pg.FindAll("[data-formula='2']"));
        pg.Find("[data-campo='buscar']").Input("hilado");
        Assert.Single(pg.FindAll("[data-formula]")); Assert.NotEmpty(pg.FindAll("[data-formula='1']"));
        pg.Find("[data-campo='buscar']").Input("zzz");
        Assert.Empty(pg.FindAll("[data-formula]")); Assert.NotEmpty(pg.FindAll("[data-vacio]"));
    }

    [Fact]
    public async Task ElegirUnaFormula_CargaLasFichasEnOrden_ElTextoDePantalla_YLaPaletaConSusSubFamilias()
    {
        var pg = await Elegida();
        Assert.Equal("(|Consumo|x|100ENT|/|(|100ENT|-|%Merma|)|)", Lienzo(pg));
        Assert.Equal("( (Consumo x 100 / (100 - %Merma)) )", pg.Find("[data-alias]").TextContent.Trim());
        Assert.Equal("CONSUMO CON MERMA", pg.Find("[data-campo='nombre']").GetAttribute("value"));
        Assert.Contains("background:#1f9d55", pg.Find("[data-ficha='1']").GetAttribute("style"));       // el color de su componente
        var paleta = F.Calls.Last(c => c.M == "GET" && c.U.Contains("/formulas/paleta")).U;
        Assert.Contains("COD_SUB_FAMILIA_PRODUCTO_CONSUMO=S1", paleta); Assert.Contains("COD_SUB_FAMILIA_PRODUCTO=S2", paleta);      // la paleta se pide con las sub familias de la formula
    }

    [Fact]
    public async Task LaFormulaTecnicaSoloSeVeSiSeMarca_YUsaElValorDeLasConstantes()
    {
        var pg = await Elegida();
        Assert.Empty(pg.FindAll("[data-tecnico]"));
        await pg.InvokeAsync(() => pg.Find("[data-campo='ver-tecnico']").Change(true));
        Assert.Equal("( CAN_CONSUMO * 100 / ( 100 - TASA DE MERMA ) )", pg.Find("[data-tecnico]").TextContent.Trim());
    }

    [Fact]
    public async Task ElPanelDeAuditoria_MuestraSuBotonConLosDatosDeLaFormula()
    {
        var pg = await Elegida();
        pg.WaitForAssertion(() => Assert.NotEmpty(pg.FindAll("[data-zona='cabecera'] button svg")));
        Assert.NotEmpty(pg.FindAll("[data-estado-chip]"));
        Assert.Contains("ACTIVA", pg.Find("[data-estado-chip]").TextContent);
    }

    [Fact]
    public async Task LaPaleta_MuestraLosComponentesConSuColor_YAlElegirUnoSusElementos()
    {
        var pg = await Elegida();
        foreach (var c in new[] { "CONSM", "CONST", "OPEMT", "OPELG", "ESPEC" }) Assert.NotEmpty(pg.FindAll($"[data-comp='{c}']"));
        Assert.Contains("background:#d93025", pg.Find("[data-comp='CONST']").GetAttribute("style"));
        Assert.Equal("true", pg.Find("[data-comp='CONSM']").GetAttribute("aria-pressed"));           // el primero queda elegido
        Assert.Equal(2, pg.FindAll("[data-el]").Count);
        await Comp(pg, "OPEMT");
        Assert.Equal(new[] { "+", "-", "x" }, pg.FindAll("[data-el]").Select(x => x.TextContent.Trim()).ToArray());
    }

    [Fact]
    public async Task UnComponenteDeEspecificacionesSinFichas_ExplicaQueHayQueElegirLaSubFamilia()
    {
        var pg = await Elegida();
        await Comp(pg, "ESPEC");
        Assert.Empty(pg.FindAll("[data-el]"));
        Assert.Contains("sub familia", pg.Find("[data-sin-elementos]").TextContent);
        Assert.Contains("consumo", pg.Markup);
    }

    [Fact]
    public async Task LasEspecificacionesDeLaSubFamilia_MuestranSuDescripcionEnSingular_YCadaUnaEsUnaFichaDistinta()
    {
        var pg = await Elegida();
        await Comp(pg, "ESPST");
        Assert.Equal(new[] { "Gramaje", "Ancho util" }, pg.FindAll("[data-el]").Select(x => x.TextContent.Trim()).ToArray());   // comparten el elemento 00001 pero son dos fichas
        Assert.Contains("producto", pg.Markup);
        await El(pg, "ANCHO"); await Accion(pg, "adicionar");
        Assert.Equal(12, pg.FindAll("[data-ficha]").Count);
        Assert.EndsWith("Ancho util", Lienzo(pg));                                                  // en la formula queda la descripcion en singular
        await El(pg, "GRAMAJE");
        Assert.Empty(pg.FindAll("[data-ficha='12'].fm-ficha-sel"));
        await Accion(pg, "validar");
        pg.WaitForAssertion(() => Assert.NotEmpty(pg.FindAll("[data-validacion]")));
        var fichas = Prop(Ultima("POST", "/formulas/validar").B!.Value, "Fichas").EnumerateArray().ToList();
        Assert.Equal("ESPST", Prop(fichas[11], "COD_COMPONENTE_FORMULACION").GetString());
        Assert.Equal("Ancho util", Prop(fichas[11], "DES_FORMULA").GetString());                    // la especificacion lleva su texto propio (como las constantes su valor)
        Assert.Equal("ANCHO", Prop(fichas[11], "COD_ESPECIFICACION_TECNICA").GetString());          // y su codigo: sin portador en la paleta, la base lo resuelve
        Assert.True(Prop(fichas[11], "COD_ELEMENTO_FORMULACION").ValueKind == JsonValueKind.Null || Prop(fichas[11], "COD_ELEMENTO_FORMULACION").GetString() == "");
        Assert.Equal(JsonValueKind.Null, Prop(fichas[1], "DES_FORMULA").ValueKind);
    }

    // ---------------- cabecera ocultable / sin botones de quitar sub familia ----------------
    [Fact]
    public async Task LaCabeceraSePuedeOcultar_YMostrar_SinPerderLoEscrito_YSinBotonesDeQuitarSubFamilia()
    {
        var pg = await Elegida();
        Assert.Empty(pg.FindAll("[data-accion^='quitar-']"));                                       // ya no hay botones "Quitar la sub familia"
        Assert.NotEmpty(pg.FindAll("[data-campo='nombre']")); Assert.NotEmpty(pg.FindAll("[data-sfs]"));
        pg.Find("[data-campo='nombre']").Input("NOMBRE NUEVO");
        await Accion(pg, "cabecera");
        Assert.Empty(pg.FindAll("[data-campo='nombre']")); Assert.Empty(pg.FindAll("[data-sfs]"));
        Assert.Contains("NOMBRE NUEVO", pg.Find("[data-resumen-cabecera]").TextContent);            // queda el resumen
        Assert.NotEmpty(pg.FindAll("[data-alias]")); Assert.NotEmpty(pg.FindAll("[data-lienzo]")); // el resto sigue visible
        await Accion(pg, "cabecera");
        Assert.Equal("NOMBRE NUEVO", pg.Find("[data-campo='nombre']").GetAttribute("value"));
        Assert.Empty(pg.FindAll("[data-resumen-cabecera]"));
    }

    // ---------------- varios componentes / todos los elementos ----------------
    [Fact]
    public async Task UnClicEnUnComponente_MuestraSoloSusElementos_ReemplazandoAlAnterior()
    {
        var pg = await Elegida();
        await Comp(pg, "CONSM"); await Comp(pg, "OPEMT");
        Assert.Equal(new[] { "OPEMT" }, pg.FindAll("[data-grupo]").Select(x => x.GetAttribute("data-grupo")).ToArray());
        Assert.Equal(0, pg.FindAll(".fm-grupo-titulo").Count);                                      // con un solo componente no hace falta el titulo
        Assert.Equal("false", pg.Find("[data-comp='CONSM']").GetAttribute("aria-pressed"));
    }

    [Fact]
    public async Task ConCtrlClic_ElComponenteSeSumaOSeQuita_YCadaGrupoLlevaSuTitulo()
    {
        var pg = await Elegida();                                                                   // arranca con el primero (CONSM)
        await pg.InvokeAsync(() => pg.Find("[data-comp='OPEMT']").Click(new Microsoft.AspNetCore.Components.Web.MouseEventArgs { CtrlKey = true }));
        Assert.Equal(new[] { "CONSM", "OPEMT" }, pg.FindAll("[data-grupo]").Select(x => x.GetAttribute("data-grupo")).OrderBy(x => x).ToArray());
        Assert.Equal(2, pg.FindAll(".fm-grupo-titulo").Count);
        await pg.InvokeAsync(() => pg.Find("[data-comp='CONSM']").Click(new Microsoft.AspNetCore.Components.Web.MouseEventArgs { CtrlKey = true }));
        Assert.Equal(new[] { "OPEMT" }, pg.FindAll("[data-grupo]").Select(x => x.GetAttribute("data-grupo")).ToArray());
    }

    [Fact]
    public async Task ElegirVariosComponentes_SinTeclado_ConLaCasilla()
    {
        var pg = await Elegida();
        await pg.InvokeAsync(() => pg.Find("[data-campo='multi']").Change(true));
        await Comp(pg, "OPEMT"); await Comp(pg, "CONST");
        Assert.Equal(new[] { "CONSM", "CONST", "OPEMT" }, pg.FindAll("[data-grupo]").Select(x => x.GetAttribute("data-grupo")).OrderBy(x => x).ToArray());
    }

    [Fact]
    public async Task MostrarTodos_ListaLosElementosDeTodosLosComponentes_YSePuedeAdicionarUnoDeCualquiera()
    {
        var pg = await Elegida();
        await pg.InvokeAsync(() => pg.Find("[data-campo='ver-todos']").Change(true));
        Assert.Equal(pg.FindAll("[data-comp]").Count, pg.FindAll("[data-grupo]").Count);            // un grupo por componente
        // el elemento 00001 existe en varios componentes: se agrega el del grupo en que se hizo doble clic
        await pg.InvokeAsync(() => pg.Find("[data-grupo='OPEMT'] [data-el='00001']").DoubleClick());
        Assert.Equal("+", pg.Find("[data-ficha='11']").TextContent.Trim());
        await pg.InvokeAsync(() => pg.Find("[data-grupo='CONSM'] [data-el='00005']").Click());
        await Accion(pg, "adicionar");
        Assert.Equal("%Merma", pg.Find("[data-ficha='12']").TextContent.Trim());
        await pg.InvokeAsync(() => pg.Find("[data-grupo='CONSM'] [data-el='00005']").DragStart());
        await pg.InvokeAsync(() => pg.Find("[data-lienzo]").Drop());
        Assert.Equal(14, pg.FindAll("[data-ficha]").Count);
        await pg.InvokeAsync(() => pg.Find("[data-campo='ver-todos']").Change(false));
        Assert.Equal(1, pg.FindAll("[data-grupo]").Count);                                          // vuelve a los componentes elegidos
    }

    [Fact]
    public async Task SinNingunComponenteElegido_ExplicaQueHacer()
    {
        var pg = await Elegida();
        await pg.InvokeAsync(() => pg.Find("[data-comp='CONSM']").Click(new Microsoft.AspNetCore.Components.Web.MouseEventArgs { CtrlKey = true }));   // lo quita
        Assert.Empty(pg.FindAll("[data-grupo]"));
        Assert.Contains("Elige uno o varios", pg.Find("[data-sin-seleccion]").TextContent);
    }

    // ---------------- estado EN DESARROLLO al modificar ----------------
    [Fact]
    public async Task ModificarUnaFormulaActiva_LaPasaAEnDesarrollo_YSeGrabaConEseEstado()
    {
        var pg = await Elegida();                                                                   // la 1 esta ACTIVA
        Assert.Equal("1", pg.FindComponent<MudBlazor.MudSelect<short>>().Instance.Value.ToString());
        Assert.Empty(pg.FindAll("[data-aviso-estado]"));
        await pg.InvokeAsync(() => pg.Find("[data-ficha='1']").DoubleClick());                    // elimina una ficha
        Assert.Equal(0, pg.FindComponent<MudBlazor.MudSelect<short>>().Instance.Value);
        Assert.Contains("EN DESARROLLO", pg.Find("[data-aviso-estado]").TextContent);
        pg.Find("[data-campo='nombre']").Input("CONSUMO CON MERMA 2");
        await Accion(pg, "grabar");
        pg.WaitForAssertion(() => Assert.Equal(1, Cuantas("PUT", "/formulacion/formulas")));
        Assert.Equal(0, Prop(Ultima("PUT", "/formulacion/formulas").B!.Value, "COD_ESTADO").GetInt32());
    }

    [Fact]
    public async Task ModificarSoloLaDescripcion_TambienLaPasaAEnDesarrollo_PeroPuedeVolverAActivarlaAProposito()
    {
        var pg = await Elegida();
        pg.Find("[data-campo='nombre']").Input("OTRO NOMBRE");
        Assert.Equal(0, pg.FindComponent<MudBlazor.MudSelect<short>>().Instance.Value);
        await pg.InvokeAsync(() => pg.FindComponent<MudBlazor.MudSelect<short>>().Instance.ValueChanged.InvokeAsync(1));      // el usuario la activa a proposito
        Assert.Empty(pg.FindAll("[data-aviso-estado]"));
        await Accion(pg, "grabar");
        pg.WaitForAssertion(() => Assert.Equal(1, Cuantas("PUT", "/formulacion/formulas")));
        Assert.Equal(1, Prop(Ultima("PUT", "/formulacion/formulas").B!.Value, "COD_ESTADO").GetInt32());
    }

    [Fact]
    public async Task UnaFormulaNueva_NoCambiaDeEstadoAlArmarla()
    {
        var pg = await Elegida();
        await Accion(pg, "nueva");
        await pg.InvokeAsync(() => pg.FindComponent<MudBlazor.MudSelect<short>>().Instance.ValueChanged.InvokeAsync(1));
        pg.Find("[data-campo='nombre']").Input("NUEVA");
        Assert.Equal(1, pg.FindComponent<MudBlazor.MudSelect<short>>().Instance.Value);             // solo las ya grabadas vuelven a EN DESARROLLO
    }

    // ---------------- constantes editables en la formula armada ----------------
    [Fact]
    public async Task UnaConstanteDeLaFormulaArmada_SeEditaEnLaPropiaFicha()
    {
        var pg = await Elegida();
        Assert.Equal("100", pg.Find("[data-valor-ficha='3']").GetAttribute("value"));
        pg.Find("[data-valor-ficha='3']").Change("250");
        Assert.Contains("250ENT", Lienzo(pg));
        Assert.Contains("250", pg.Find("[data-alias]").TextContent);                                // el texto de pantalla se actualiza
        Assert.Empty(pg.FindAll("[data-valor-ficha='1']"));                                         // solo las constantes tienen campo
        await Accion(pg, "validar");
        pg.WaitForAssertion(() => Assert.NotEmpty(pg.FindAll("[data-validacion]")));
        Assert.Equal("250", Prop(Prop(Ultima("POST", "/formulas/validar").B!.Value, "Fichas").EnumerateArray().ToList()[3], "DES_FORMULA").GetString());
        Assert.Equal(11, pg.FindAll("[data-ficha]").Count);                                         // editar no elimina la ficha
    }

    [Fact]
    public async Task UnaConstanteSinValor_NoSePermite_YVuelveAlValorAnterior()
    {
        var pg = await Elegida();
        pg.Find("[data-valor-ficha='3']").Change("  ");
        Assert.Contains("100ENT", Lienzo(pg));
        Assert.Equal("100", pg.Find("[data-valor-ficha='3']").GetAttribute("value"));
    }

    // ---------------- doble clic ----------------
    [Fact]
    public async Task DobleClicEnUnElementoDeLaPaleta_LoAdicionaAlFinal()
    {
        var pg = await Elegida();
        await Comp(pg, "CONSM");
        await pg.InvokeAsync(() => pg.Find("[data-el='00005']").Click());                           // los dos clics del doble clic: seleccionar y deseleccionar
        await pg.InvokeAsync(() => pg.Find("[data-el='00005']").Click());
        await pg.InvokeAsync(() => pg.Find("[data-el='00005']").DoubleClick());
        Assert.Equal(12, pg.FindAll("[data-ficha]").Count);
        Assert.Equal("%Merma", pg.Find("[data-ficha='11']").TextContent.Trim());
    }

    [Fact]
    public async Task DobleClicEnUnaConstanteSinValor_NoLaAdiciona_ConValorSi()
    {
        var pg = await Elegida();
        await Comp(pg, "CONST");
        await pg.InvokeAsync(() => pg.Find("[data-el='00009']").DoubleClick());
        Assert.Equal(11, pg.FindAll("[data-ficha]").Count);
        pg.Find("[data-campo='valor']").Input("7");
        await pg.InvokeAsync(() => pg.Find("[data-el='00009']").DoubleClick());
        Assert.Equal(12, pg.FindAll("[data-ficha]").Count); Assert.EndsWith("7NUM", Lienzo(pg));
    }

    [Fact]
    public async Task DobleClicEnUnaFichaDeLaFormula_LaElimina()
    {
        var pg = await Elegida();
        var antes = Lienzo(pg).Split('|');
        await pg.InvokeAsync(() => pg.Find("[data-ficha='1']").Click());
        await pg.InvokeAsync(() => pg.Find("[data-ficha='1']").Click());
        await pg.InvokeAsync(() => pg.Find("[data-ficha='1']").DoubleClick());                     // Consumo
        var d = Lienzo(pg).Split('|');
        Assert.Equal(10, d.Length);
        Assert.Equal(antes.Where((_, i) => i != 1).ToArray(), d);
        Assert.Equal(-1, pg.FindAll("[data-ficha]").ToList().FindIndex(x => x.ClassName!.Contains("fm-ficha-sel")));   // nada queda seleccionado
    }

    // ---------------- arrastrar y soltar ----------------
    [Fact]
    public async Task ArrastrarUnElementoDeLaPaleta_AlaMitadDerechaDeUnaFicha_LoInsertaDespues()
    {
        var pg = await Elegida();
        await Comp(pg, "CONSM");
        await pg.InvokeAsync(() => pg.Find("[data-el='00005']").DragStart());                       // %Merma
        Assert.Contains("fm-arrastrando", pg.Find("[data-lienzo]").ClassName);
        await pg.InvokeAsync(() => pg.Find("[data-slot='1'] [data-zona-ins='2']").DragOver());
        Assert.Contains("fm-ins-izq", pg.Find("[data-slot='2']").ClassName);                        // la barra marca donde caera: antes de la ficha 2
        await pg.InvokeAsync(() => pg.Find("[data-slot='1'] [data-zona-ins='2']").Drop());
        Assert.Equal(12, pg.FindAll("[data-ficha]").Count);
        Assert.Equal("%Merma", pg.Find("[data-ficha='2']").TextContent.Trim());
        Assert.Equal("Consumo", pg.Find("[data-ficha='1']").TextContent.Trim());
        Assert.DoesNotContain("fm-arrastrando", pg.Find("[data-lienzo]").ClassName);                // termina el arrastre
    }

    [Fact]
    public async Task ArrastrarAlaMitadIzquierda_InsertaAntes_YAlHuecoDelLienzo_AgregaAlFinal()
    {
        var pg = await Elegida();
        await Comp(pg, "OPEMT");
        await pg.InvokeAsync(() => pg.Find("[data-el='00001']").DragStart());                       // +
        await pg.InvokeAsync(() => pg.Find("[data-slot='0'] [data-zona-ins='0']").Drop());
        Assert.Equal("+", pg.Find("[data-ficha='0']").TextContent.Trim());
        await pg.InvokeAsync(() => pg.Find("[data-el='00001']").DragStart());
        await pg.InvokeAsync(() => pg.Find("[data-lienzo]").DragOver());
        Assert.Contains("fm-ins-der", pg.Find("[data-slot='11']").ClassName);                       // al final: barra a la derecha de la ultima
        await pg.InvokeAsync(() => pg.Find("[data-lienzo]").Drop());
        Assert.Equal(13, pg.FindAll("[data-ficha]").Count);
        Assert.Equal("+", pg.Find("[data-ficha='12']").TextContent.Trim());
    }

    [Fact]
    public async Task ArrastrarUnaFichaDelLienzo_LaMueve_ConLaPosicionCorrectaAlSacarlaDeAntes()
    {
        var pg = await Elegida();
        var antes = Lienzo(pg).Split('|');                                                          // ( Consumo x ...
        await pg.InvokeAsync(() => pg.Find("[data-ficha='0']").DragStart());
        await pg.InvokeAsync(() => pg.Find("[data-slot='2'] [data-zona-ins='3']").Drop());          // despues de "x"
        var d = Lienzo(pg).Split('|');
        Assert.Equal(11, d.Length);
        Assert.Equal(new[] { antes[1], antes[2], antes[0] }, d.Take(3).ToArray());                  // Consumo, x, (
        Assert.Contains("fm-ficha-sel", pg.Find("[data-ficha='2']").ClassName);                     // queda seleccionada donde cayo
        // soltarla donde ya esta no cambia nada
        await pg.InvokeAsync(() => pg.Find("[data-ficha='1']").DragStart());
        await pg.InvokeAsync(() => pg.Find("[data-slot='1'] [data-zona-ins='2']").Drop());
        Assert.Equal(d, Lienzo(pg).Split('|'));
    }

    [Fact]
    public async Task ArrastrarUnaConstante_SinValorNoAgrega_ConValorLaAgrega()
    {
        var pg = await Elegida();
        await Comp(pg, "CONST");
        await pg.InvokeAsync(() => pg.Find("[data-el='00009']").DragStart());
        await pg.InvokeAsync(() => pg.Find("[data-lienzo]").Drop());
        Assert.Equal(11, pg.FindAll("[data-ficha]").Count);                                         // sin valor: avisa y no agrega
        pg.Find("[data-campo='valor']").Input("1.05");
        await pg.InvokeAsync(() => pg.Find("[data-el='00009']").DragStart());
        await pg.InvokeAsync(() => pg.Find("[data-lienzo]").Drop());
        Assert.Equal(12, pg.FindAll("[data-ficha]").Count); Assert.EndsWith("1.05NUM", Lienzo(pg));
    }

    [Fact]
    public async Task UnArrastreCancelado_NoCambiaLaFormula_YLaMarcaDesaparece()
    {
        var pg = await Elegida();
        await Comp(pg, "CONSM");
        await pg.InvokeAsync(() => pg.Find("[data-el='00005']").DragStart());
        await pg.InvokeAsync(() => pg.Find("[data-el='00005']").DragEnd());                         // soltado fuera del lienzo
        Assert.Equal(11, pg.FindAll("[data-ficha]").Count);
        Assert.DoesNotContain("fm-arrastrando", pg.Find("[data-lienzo]").ClassName);
        Assert.Equal(0, pg.FindAll(".fm-ins-izq, .fm-ins-der").Count);
    }

    [Fact]
    public async Task Adicionar_AgregaAlFinal_ConElColorDelComponente_YQuedaSeleccionada()
    {
        var pg = await Elegida();
        Assert.True(Deshabilitado(pg.Find("[data-accion='adicionar']")));                          // sin elemento elegido
        await Comp(pg, "OPEMT"); await El(pg, "00001");
        Assert.False(Deshabilitado(pg.Find("[data-accion='adicionar']")));
        await Accion(pg, "adicionar");
        Assert.EndsWith(")|)|+", Lienzo(pg));
        Assert.Equal("true", pg.Find("[data-ficha='11']").GetAttribute("aria-pressed"));
        Assert.Contains("con cambios sin grabar", pg.Markup);
        Assert.Contains("+", pg.Find("[data-alias]").TextContent);
    }

    [Fact]
    public async Task Insertar_AgregaAntesDeLaFichaSeleccionada()
    {
        var pg = await Elegida();
        await Comp(pg, "OPEMT"); await El(pg, "00001");
        Assert.True(Deshabilitado(pg.Find("[data-accion='insertar']")));                           // sin ficha elegida
        await pg.InvokeAsync(() => pg.Find("[data-ficha='1']").Click());                           // Consumo (indice 1)
        await Accion(pg, "insertar");
        Assert.StartsWith("(|+|Consumo|x", Lienzo(pg));
    }

    [Fact]
    public async Task EliminarFicha_QuitaLaSeleccionada_YLasFlechasLaMueven()
    {
        var pg = await Elegida();
        await pg.InvokeAsync(() => pg.Find("[data-ficha='7']").Click());                           // el menos (indice 7)
        await Accion(pg, "quitar");
        Assert.Equal("(|Consumo|x|100ENT|/|(|100ENT|%Merma|)|)", Lienzo(pg));
        await pg.InvokeAsync(() => pg.Find("[data-ficha='1']").Click());                           // Consumo
        await Accion(pg, "bajar");
        Assert.StartsWith("(|x|Consumo|100ENT", Lienzo(pg));
        await Accion(pg, "subir");
        Assert.StartsWith("(|Consumo|x|100ENT", Lienzo(pg));
    }

    [Fact]
    public async Task UnaConstante_PideSuValor_YModificarSoloCambiaElValorDeLasConstantes()
    {
        var pg = await Elegida();
        await Comp(pg, "CONST"); await El(pg, "00009");                                            // numerica
        Assert.Contains("decimales", pg.Find("[data-ayuda-valor]").TextContent);
        await Accion(pg, "adicionar");                                                             // sin valor: no agrega
        Assert.Equal(11, pg.FindAll("[data-ficha]").Count);
        pg.Find("[data-campo='valor']").Input("1.05");
        await Accion(pg, "adicionar");
        Assert.Equal(12, pg.FindAll("[data-ficha]").Count); Assert.EndsWith("1.05NUM", Lienzo(pg));
        // modificar: elegir la constante y cambiar su valor
        await pg.InvokeAsync(() => pg.Find("[data-ficha='3']").Click());                           // 100
        Assert.False(Deshabilitado(pg.Find("[data-accion='modificar']")));
        pg.Find("[data-campo='valor']").Input("250");
        await Accion(pg, "modificar");
        Assert.Contains("250ENT", Lienzo(pg));
        await pg.InvokeAsync(() => pg.Find("[data-ficha='1']").Click());                           // Consumo: no es constante
        Assert.True(Deshabilitado(pg.Find("[data-accion='modificar']")));
    }

    [Fact]
    public async Task Validar_EnviaLasFichasEnOrdenConElValorSoloEnLasConstantes_YMuestraQueEsValida()
    {
        var pg = await Elegida();
        await Accion(pg, "validar");
        pg.WaitForAssertion(() => Assert.NotEmpty(pg.FindAll("[data-validacion='ok']")));
        var b = Ultima("POST", "/formulas/validar").B!.Value;
        var fichas = Prop(b, "Fichas").EnumerateArray().ToList();
        Assert.Equal(11, fichas.Count);
        Assert.Equal("OPELG", Prop(fichas[0], "COD_COMPONENTE_FORMULACION").GetString()); Assert.Equal("00003", Prop(fichas[0], "COD_ELEMENTO_FORMULACION").GetString());
        Assert.Equal("100", Prop(fichas[3], "DES_FORMULA").GetString());                           // la constante lleva su valor
        Assert.Equal(JsonValueKind.Null, Prop(fichas[1], "DES_FORMULA").ValueKind);               // las demas no
    }

    [Fact]
    public async Task Validar_ConProblemas_ListaLosMotivos_YMarcaLasFichasEnSuPosicion()
    {
        var pg = await Elegida();
        F.Validacion = new FormulaValidacion { ES_VALIDA = false, Errores = new() { new() { NUM = 1, NUM_POSICION = 3, DES_ERROR = "Falta un operador antes de \"x\"." }, new() { NUM = 2, DES_ERROR = "Faltan 1 cierre(s)." } } };
        await Accion(pg, "validar");
        pg.WaitForAssertion(() => Assert.NotEmpty(pg.FindAll("[data-validacion='error']")));
        var msg = pg.Find("[data-validacion='error']").TextContent;
        Assert.Contains("Falta un operador", msg); Assert.Contains("Faltan 1 cierre", msg);
        Assert.Contains("fm-ficha-err", pg.Find("[data-ficha='2']").ClassName);                    // posicion 3 = la ficha de indice 2
        Assert.DoesNotContain("fm-ficha-err", pg.Find("[data-ficha='1']").ClassName);
        await Comp(pg, "OPEMT"); await El(pg, "00001"); await Accion(pg, "adicionar");             // al cambiar la formula la validacion anterior ya no vale
        Assert.Empty(pg.FindAll("[data-validacion]"));
    }

    [Fact]
    public async Task Grabar_UnaFormulaExistente_HaceUnPutConElNumero_ElNombreYLasFichas()
    {
        var pg = await Elegida();
        pg.Find("[data-campo='nombre']").Input("CONSUMO CAMBIADO");
        await Comp(pg, "OPEMT"); await El(pg, "00001"); await Accion(pg, "adicionar");
        await Accion(pg, "grabar");
        pg.WaitForAssertion(() => Assert.Equal(1, Cuantas("PUT", "/formulacion/formulas")));
        var b = Ultima("PUT", "/formulacion/formulas").B!.Value;
        Assert.Equal(1, Prop(b, "NUM_FORMULACION").GetInt32()); Assert.Equal("CONSUMO CAMBIADO", Prop(b, "DES_FORMULACION").GetString());
        Assert.Equal(12, Prop(b, "Fichas").GetArrayLength());
        Assert.Equal("S1", Prop(b, "COD_SUB_FAMILIA_PRODUCTO_CONSUMO").GetString()); Assert.Equal("S2", Prop(b, "COD_SUB_FAMILIA_PRODUCTO").GetString());
        pg.WaitForAssertion(() => Assert.DoesNotContain("con cambios sin grabar", pg.Markup));      // tras grabar ya no hay cambios pendientes
        Assert.True(Cuantas("GET", "/formulacion/formulas") >= 2);                                 // se vuelve a leer la lista
    }

    [Fact]
    public async Task Grabar_UnaFormulaNueva_HaceUnPostSinNumero_YLaDejaElegida()
    {
        var pg = Pagina();
        await Accion(pg, "nueva");
        pg.WaitForAssertion(() => Assert.NotEmpty(pg.FindAll("[data-zona='lienzo']")));
        Assert.NotEmpty(pg.FindAll("[data-lienzo-vacio]"));
        pg.Find("[data-campo='nombre']").Input("MI FORMULA");
        await Comp(pg, "CONSM"); await El(pg, "00002"); await Accion(pg, "adicionar");
        await Accion(pg, "grabar");
        pg.WaitForAssertion(() => Assert.Equal(1, Cuantas("POST", "/formulacion/formulas")));
        var b = Ultima("POST", "/formulacion/formulas").B!.Value;
        Assert.Equal("MI FORMULA", Prop(b, "DES_FORMULACION").GetString()); Assert.Equal(1, Prop(b, "Fichas").GetArrayLength());
        Assert.True(!b.TryGetProperty("NUM_FORMULACION", out var n) || n.ValueKind == JsonValueKind.Null || n.GetInt32() == 0);
        pg.WaitForAssertion(() => Assert.Contains("MI FORMULA", pg.Find("[data-formula='3']").TextContent));
    }

    [Fact]
    public async Task AlCambiarLaSubFamiliaDelConsumo_SeVuelveAPedirLaPaleta_YLaFormulaQuedaConCambios()
    {
        var pg = await Elegida();
        var antes = F.Calls.Count(c => c.M == "GET" && c.U.Contains("/formulas/paleta"));
        var consumo = pg.FindComponents<SubFamiliaSelector>().First(x => x.Instance.Id == "consumo");
        await pg.InvokeAsync(() => consumo.Instance.OnCambio.InvokeAsync(("G1", "F1", "S9")));
        pg.WaitForAssertion(() => Assert.True(F.Calls.Count(c => c.M == "GET" && c.U.Contains("/formulas/paleta")) > antes));
        Assert.Contains("COD_SUB_FAMILIA_PRODUCTO_CONSUMO=S9", F.Calls.Last(c => c.M == "GET" && c.U.Contains("/formulas/paleta")).U);
        Assert.Contains("con cambios sin grabar", pg.Markup);
    }

    [Fact]
    public async Task Grabar_SinDescripcion_NoEnviaNada()
    {
        var pg = Pagina();
        await Accion(pg, "nueva");
        pg.WaitForAssertion(() => Assert.NotEmpty(pg.FindAll("[data-zona='lienzo']")));
        await Accion(pg, "grabar");
        Assert.Contains("descripción", pg.Find("[data-error='1']").TextContent);
        Assert.Equal(0, Cuantas("POST", "/formulacion/formulas"));
    }

    [Fact]
    public async Task Grabar_ConUnaSubFamiliaIncompleta_NoEnviaNada()
    {
        var pg = Pagina();
        await Accion(pg, "nueva");
        pg.WaitForAssertion(() => Assert.NotEmpty(pg.FindAll("[data-zona='lienzo']")));
        pg.Find("[data-campo='nombre']").Input("X");
        // solo el grupo: se simula desde el selector del consumo
        var sel = pg.FindComponent<SubFamiliaSelector>();
        await pg.InvokeAsync(() => sel.Instance.OnCambio.InvokeAsync(("G1", null, null)));
        await Accion(pg, "grabar");
        Assert.Contains("completos", pg.Find("[data-error='1']").TextContent);
        Assert.Equal(0, Cuantas("POST", "/formulacion/formulas"));
    }

    [Fact]
    public async Task SiLaBaseRechazaLaFormula_MuestraElMotivo_YVuelveAValidarParaMarcarLasFichas()
    {
        var pg = await Elegida();
        F.MensajeFalla = "La formula no puede terminar con \"x\".";
        F.Validacion = new FormulaValidacion { ES_VALIDA = false, Errores = new() { new() { NUM = 1, NUM_POSICION = 11, DES_ERROR = "La formula no puede terminar con \"x\"." } } };
        await Comp(pg, "OPEMT"); await El(pg, "00003"); await Accion(pg, "adicionar");
        await Accion(pg, "grabar");
        pg.WaitForAssertion(() => Assert.Contains("no puede terminar", pg.Find("[data-error='1']").TextContent));
        pg.WaitForAssertion(() => Assert.Contains("fm-ficha-err", pg.Find("[data-ficha='10']").ClassName));
        Assert.Contains("sin grabar", pg.Markup);                                                  // sigue con cambios pendientes
    }

    [Fact]
    public async Task Descartar_VuelveALeerLaFormulaYQuitaLosCambios()
    {
        var pg = await Elegida();
        await pg.InvokeAsync(() => pg.Find("[data-ficha='8']").Click()); await Accion(pg, "quitar");
        Assert.Equal(10, pg.FindAll("[data-ficha]").Count);
        await Accion(pg, "descartar");
        pg.WaitForAssertion(() => Assert.Equal(11, pg.FindAll("[data-ficha]").Count));
        Assert.DoesNotContain("con cambios sin grabar", pg.Markup);
    }

    [Fact]
    public async Task Copiar_HaceElPostDeCopia_YAbreLaCopia()
    {
        var pg = await Elegida();
        await Accion(pg, "copiar");
        pg.WaitForAssertion(() => Assert.Equal(1, Cuantas("POST", "/formulacion/formulas/1/copiar")));
        pg.WaitForAssertion(() => Assert.Contains("(copia)", pg.Find("[data-formula='3']").TextContent));
    }

    [Fact]
    public async Task Eliminar_PideConfirmacion_YLuegoBorraLaFormula()
    {
        var pg = await Elegida();
        await pg.InvokeAsync(() => pg.Find("[data-accion='eliminar']").Click());
        Prov!.WaitForAssertion(() => Assert.Contains("Eliminar fórmula", Prov.Markup));
        Assert.Equal(0, Cuantas("DELETE", "/formulacion/formulas/1"));                             // aun no
        await Prov.InvokeAsync(() => Prov.FindAll("button").First(b => b.TextContent.Trim() == "Eliminar").Click());
        pg.WaitForAssertion(() => Assert.Equal(1, Cuantas("DELETE", "/formulacion/formulas/1")));
        pg.WaitForAssertion(() => Assert.Empty(pg.FindAll("[data-formula='1']")));
        Assert.NotEmpty(pg.FindAll("[data-vacio-detalle]"));
    }

    [Fact]
    public async Task ConCambiosSinGrabar_ElegirOtraFormulaPideConfirmacion()
    {
        var pg = await Elegida();
        await pg.InvokeAsync(() => pg.Find("[data-ficha='8']").Click()); await Accion(pg, "quitar");
        await pg.InvokeAsync(() => pg.Find("[data-formula='2']").Click());
        Prov!.WaitForAssertion(() => Assert.Contains("Hay cambios sin grabar", Prov.Markup));
        await Prov.InvokeAsync(() => Prov.FindAll("button").First(b => b.TextContent.Trim() == "Seguir editando").Click());
        Assert.Equal(10, pg.FindAll("[data-ficha]").Count);                                        // se queda donde estaba
        Assert.Equal(0, Cuantas("GET", "/formulacion/formulas/2"));
    }

    [Fact]
    public async Task SinCambios_ElegirOtraFormulaNoPregunta()
    {
        var pg = await Elegida();
        await pg.InvokeAsync(() => pg.Find("[data-formula='2']").Click());
        pg.WaitForAssertion(() => Assert.Equal(1, Cuantas("GET", "/formulacion/formulas/2")));
        Assert.DoesNotContain("Hay cambios sin grabar", Prov!.Markup);
    }
}


// Reemplazos de los controles compartidos: dejan teclear y pulsar lo que la prueba necesita sin traer sus servicios.
public class CampoTextoFalso : Microsoft.AspNetCore.Components.ComponentBase
{
    [Microsoft.AspNetCore.Components.Parameter] public string Tabla { get; set; } = "";
    [Microsoft.AspNetCore.Components.Parameter] public string Columna { get; set; } = "";
    [Microsoft.AspNetCore.Components.Parameter] public string? Label { get; set; }
    [Microsoft.AspNetCore.Components.Parameter] public string? Value { get; set; }
    [Microsoft.AspNetCore.Components.Parameter] public Microsoft.AspNetCore.Components.EventCallback<string?> ValueChanged { get; set; }
    [Microsoft.AspNetCore.Components.Parameter] public System.Linq.Expressions.Expression<Func<string?>>? ValueExpression { get; set; }
    [Microsoft.AspNetCore.Components.Parameter] public bool Disabled { get; set; }
    [Microsoft.AspNetCore.Components.Parameter] public bool Obligatorio { get; set; }
    [Microsoft.AspNetCore.Components.Parameter] public bool NoObligatorio { get; set; }
    [Microsoft.AspNetCore.Components.Parameter] public string? HelperText { get; set; }
    protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder b)
    {
        b.OpenElement(0, "input"); b.AddAttribute(1, "data-campo", Columna); b.AddAttribute(2, "value", Value ?? "");
        if (Disabled) b.AddAttribute(3, "disabled", true);
        b.AddAttribute(4, "onchange", Microsoft.AspNetCore.Components.EventCallback.Factory.Create<Microsoft.AspNetCore.Components.ChangeEventArgs>(this, e => ValueChanged.InvokeAsync(e.Value?.ToString())));
        b.CloseElement();
    }
}

public class AccionesFalsas : Microsoft.AspNetCore.Components.ComponentBase
{
    [Microsoft.AspNetCore.Components.Parameter] public string CodAplicacion { get; set; } = "";
    [Microsoft.AspNetCore.Components.Parameter] public string CodOpcionAplicacion { get; set; } = "";
    [Microsoft.AspNetCore.Components.Parameter] public AccionesBotones.ZonaBotones Zona { get; set; }
    [Microsoft.AspNetCore.Components.Parameter] public Microsoft.AspNetCore.Components.EventCallback<KONSolutions.Shared.Models.Seguridad.BotonTreeviewItem> OnAccion { get; set; }
    protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder b)
    {
        var botones = Zona == AccionesBotones.ZonaBotones.Header
            ? new[] { ("nuevo", new KONSolutions.Shared.Models.Seguridad.BotonTreeviewItem { FLG_ACCION_POST = true }) }
            : new[] { ("editar", new KONSolutions.Shared.Models.Seguridad.BotonTreeviewItem { FLG_ACCION_PUT = true }), ("eliminar", new KONSolutions.Shared.Models.Seguridad.BotonTreeviewItem { FLG_ACCION_DELETE = true }) };
        var i = 0;
        foreach (var (n, bt) in botones)
        {
            b.OpenElement(i++, "button"); b.AddAttribute(i++, "data-accion", n);
            b.AddAttribute(i++, "onclick", Microsoft.AspNetCore.Components.EventCallback.Factory.Create(this, () => OnAccion.InvokeAsync(bt)));
            b.AddContent(i++, n); b.CloseElement();
        }
    }
}

public abstract class FormulacionMantBase : FormulacionBase
{
    protected void PrepararMant()
    {
        Preparar();
        ComponentFactories.Add<CampoTexto, CampoTextoFalso>();
        ComponentFactories.Add<AccionesBotones, AccionesFalsas>();
        ComponentFactories.AddStub<AriaCombo<KONSolutions.Shared.Common.ComboboxItem>>(p => $"<span data-combo=\"1\" data-disabled=\"{p.Get(x => x.Disabled)}\"></span>");
        ComponentFactories.AddStub<CrudDialogActions>(); ComponentFactories.AddStub<PanelAuditoria>(); ComponentFactories.AddStub<CrudDialogTitle>(); ComponentFactories.AddStub<ImagePicker>();
    }
    protected static JsonElement Cuerpo(FakeFormulacion f, string m, string fin) => f.Calls.Last(c => c.M == m && c.U.Split('?')[0].EndsWith(fin)).B!.Value;
}

// Componentes y elementos: una sola pantalla padre/hijo (solapas encadenadas) con un dialogo CRUD por nivel, como GruposProducto.
public class FormulacionComponentesTests : FormulacionMantBase
{
    IRenderedComponent<FormulacionComponentes> Pagina()
    {
        PrepararMant();
        var pg = RenderComponent<FormulacionComponentes>();
        pg.WaitForAssertion(() => Assert.Contains("DATOS DE CONSUMO", pg.Markup));
        return pg;
    }

    [Fact]
    public void ListaLosComponentes_ConLaCantidadDeElementosYSiEstanHabilitados()
    {
        var pg = Pagina();
        Assert.Equal(3, pg.FindAll("[data-accion='editar']").Count);
        Assert.Contains("background:#1f9d55", pg.Markup);
        var fila = pg.FindAll("tbody tr").First(t => t.TextContent.Contains("CONSM"));
        Assert.Contains("7", fila.TextContent); Assert.Contains("Sí", fila.TextContent);
        Assert.Contains("No", pg.FindAll("tbody tr").First(t => t.TextContent.Contains("ESPPR")).TextContent);
    }

    [Fact]
    public void LaPrimeraColumna_MuestraElIconoDelComponente_YSinIconoQuedaVacia()
    {
        F.Comps.First(c => c.COD_COMPONENTE_FORMULACION == "CONSM").IMG_PICTURE = new byte[] { 1, 2, 3 };
        var pg = Pagina();
        var filaCon = pg.FindAll("tbody tr").First(t => t.TextContent.Contains("CONSM"));
        var img = filaCon.QuerySelectorAll("td")[0].QuerySelector("img[data-icono='componente']");
        Assert.NotNull(img);
        Assert.Equal("data:image/png;base64," + Convert.ToBase64String(new byte[] { 1, 2, 3 }), img!.GetAttribute("src"));
        var filaSin = pg.FindAll("tbody tr").First(t => t.TextContent.Contains("ESPPR"));
        Assert.Empty(filaSin.QuerySelectorAll("img"));
        Assert.Equal("Ícono", pg.FindAll("thead th")[0].TextContent.Trim());
    }

    [Fact]
    public async Task ElementosLaPrimeraColumna_MuestraSuIcono_OElDeSuComponenteSiNoTiene()
    {
        F.Comps.First(c => c.COD_COMPONENTE_FORMULACION == "CONSM").IMG_PICTURE = new byte[] { 9, 9 };
        F.Elementos.First(e => e.COD_ELEMENTO_FORMULACION == "00005" && e.COD_COMPONENTE_FORMULACION == "CONSM").IMG_PICTURE = new byte[] { 7 };
        var pg = Pagina();
        var fila = pg.FindAll("tbody tr").First(t => t.TextContent.Contains("CONSM"));
        await pg.InvokeAsync(() => fila.QuerySelector("[data-accion='abrir-elementos']")!.Click());
        pg.WaitForAssertion(() => Assert.Contains("CAN_CONSUMO", pg.Markup));
        var filaMerma = pg.FindAll("tbody tr").First(t => t.TextContent.Contains("TASA DE MERMA"));
        Assert.Equal("data:image/png;base64," + Convert.ToBase64String(new byte[] { 7 }), filaMerma.QuerySelector("td img[data-icono='elemento']")!.GetAttribute("src"));
        var filaConsumo = pg.FindAll("tbody tr").First(t => t.TextContent.Contains("CAN_CONSUMO"));
        Assert.Equal("data:image/png;base64," + Convert.ToBase64String(new byte[] { 9, 9 }), filaConsumo.QuerySelector("td img[data-icono='elemento']")!.GetAttribute("src"));
    }

    [Fact]
    public void LaSolapaDeElementos_ArrancaDeshabilitada()
    {
        var pg = Pagina();
        var solapas = pg.FindAll(".mud-tab");
        Assert.Equal(2, solapas.Count);
        Assert.False(Deshabilitado(solapas[0])); Assert.True(Deshabilitado(solapas[1]));
        Assert.Empty(pg.FindAll("[data-accion='cerrar-elementos']"));
    }

    [Fact]
    public async Task LaFlechaDeLaFila_PasaALosElementosDeEseComponente_YCerrarVuelve()
    {
        var pg = Pagina();
        var fila = pg.FindAll("tbody tr").First(t => t.TextContent.Contains("CONSM"));
        await pg.InvokeAsync(() => fila.QuerySelector("[data-accion='abrir-elementos']")!.Click());
        pg.WaitForAssertion(() => Assert.NotEmpty(pg.FindAll("[data-accion='cerrar-elementos']")));
        Assert.Contains(F.Calls, c => c.M == "GET" && c.U.EndsWith("/formulacion/elementos?componente=CONSM"));
        pg.WaitForAssertion(() => Assert.Contains("CAN_CONSUMO", pg.Markup));
        Assert.DoesNotContain("Gramaje", pg.Markup);                                             // los de otro componente no aparecen
        var solapas = pg.FindAll(".mud-tab");
        Assert.True(Deshabilitado(solapas[0])); Assert.False(Deshabilitado(solapas[1]));        // nunca estan las dos habilitadas
        Assert.Contains("DATOS DE CONSUMO", solapas[1].TextContent);
        await pg.InvokeAsync(() => pg.Find("[data-accion='cerrar-elementos']").Click());
        pg.WaitForAssertion(() => Assert.Empty(pg.FindAll("[data-accion='cerrar-elementos']")));
        Assert.False(Deshabilitado(pg.FindAll(".mud-tab")[0])); Assert.True(Deshabilitado(pg.FindAll(".mud-tab")[1]));
    }

    [Fact]
    public async Task Nuevo_AbreElDialogoYHaceUnPostConElCodigoEscrito()
    {
        var pg = Pagina();
        await pg.InvokeAsync(() => pg.Find("[data-accion='nuevo']").Click());
        Prov!.WaitForAssertion(() => Assert.NotEmpty(Prov.FindAll("[data-campo='COD_COMPONENTE_FORMULACION']")));
        Assert.False(Deshabilitado(Prov.Find("[data-campo='COD_COMPONENTE_FORMULACION']")));
        Prov.Find("[data-campo='COD_COMPONENTE_FORMULACION']").Change("OPEFN"); Prov.Find("[data-campo='DES_COMPONENTE_FORMULACION']").Change("OPERADORES FINANCIEROS");
        await Prov.InvokeAsync(() => Prov.Find("form").Submit());
        pg.WaitForAssertion(() => Assert.Equal(1, Cuantas("POST", "/formulacion/componentes")));
        Assert.Equal("OPEFN", Prop(Cuerpo(F, "POST", "/formulacion/componentes"), "COD_COMPONENTE_FORMULACION").GetString());
        pg.WaitForAssertion(() => Assert.Contains("OPERADORES FINANCIEROS", pg.Markup));          // la lista se vuelve a leer
    }

    [Fact]
    public async Task Editar_HaceUnPutConLosCambios_ConElCodigoSoloDeLectura()
    {
        var pg = Pagina();
        var fila = pg.FindAll("tbody tr").First(t => t.TextContent.Contains("CONSM"));
        await pg.InvokeAsync(() => fila.QuerySelector("[data-accion='editar']")!.Click());
        Prov!.WaitForAssertion(() => Assert.NotEmpty(Prov.FindAll("[data-campo='COD_COMPONENTE_FORMULACION']")));
        Assert.True(Deshabilitado(Prov.Find("[data-campo='COD_COMPONENTE_FORMULACION']")));
        Assert.Equal("DATOS DE CONSUMO", Prov.Find("[data-campo='DES_COMPONENTE_FORMULACION']").GetAttribute("value"));
        Prov.Find("[data-campo='DES_COMPONENTE_FORMULACION']").Change("DATOS DEL CONSUMO");
        await Prov.InvokeAsync(() => Prov.Find("form").Submit());
        pg.WaitForAssertion(() => Assert.Equal(1, Cuantas("PUT", "/formulacion/componentes")));
        var b = Cuerpo(F, "PUT", "/formulacion/componentes");
        Assert.Equal("CONSM", Prop(b, "COD_COMPONENTE_FORMULACION").GetString()); Assert.Equal("DATOS DEL CONSUMO", Prop(b, "DES_COMPONENTE_FORMULACION").GetString());
        Assert.Equal("#1f9d55", Prop(b, "DES_BACKCOLOR").GetString());
    }

    [Fact]
    public async Task Guardar_SinCodigoONiDescripcion_NoEnviaNada()
    {
        var pg = Pagina();
        await pg.InvokeAsync(() => pg.Find("[data-accion='nuevo']").Click());
        Prov!.WaitForAssertion(() => Assert.NotEmpty(Prov.FindAll("form")));
        await Prov.InvokeAsync(() => Prov.Find("form").Submit());
        Prov.WaitForAssertion(() => Assert.Contains("código del componente", Prov.Markup));
        Assert.Equal(0, Cuantas("POST", "/formulacion/componentes"));
    }

    [Fact]
    public async Task LasEspecificacionesDeConsumoYDeEstructura_SonExcluyentes()
    {
        var pg = Pagina();
        var fila = pg.FindAll("tbody tr").First(t => t.TextContent.Contains("ESPEC") && t.TextContent.Contains("TECNICAS"));
        await pg.InvokeAsync(() => fila.QuerySelector("[data-accion='editar']")!.Click());              // viene con las de consumo
        Prov!.WaitForAssertion(() => Assert.NotEmpty(Prov.FindAll("form")));
        var sw = Prov.FindComponents<MudSwitch<bool?>>();                                               // orden: elementos, consumo, estructura, habilitado
        await Prov.InvokeAsync(() => sw[2].Instance.ValueChanged.InvokeAsync(true));
        await Prov.InvokeAsync(() => Prov.Find("form").Submit());
        pg.WaitForAssertion(() => Assert.Equal(1, Cuantas("PUT", "/formulacion/componentes")));
        var b = Cuerpo(F, "PUT", "/formulacion/componentes");
        Assert.True(Prop(b, "FLG_ESPECIFICACIONES").GetBoolean()); Assert.False(Prop(b, "FLG_ESPECIFICACIONES_CONSUMO").GetBoolean());   // al encender una se apaga la otra
    }

    [Fact]
    public async Task EditarComponente_ConsumoSuperiorInferiorYNombreDeTabla_ViajanAlGuardar()
    {
        var pg = Pagina();
        var fila = pg.FindAll("tbody tr").First(t => t.TextContent.Contains("ESPEC") && t.TextContent.Contains("TECNICAS"));
        await pg.InvokeAsync(() => fila.QuerySelector("[data-accion='editar']")!.Click());
        Prov!.WaitForAssertion(() => Assert.NotEmpty(Prov.FindAll("form")));
        Assert.NotEmpty(Prov.FindAll("[data-flag='consumo-superior']")); Assert.NotEmpty(Prov.FindAll("[data-flag='consumo-inferior']"));
        var cons = Prov.FindComponents<MudSwitch<bool?>>().First(s => s.Markup.Contains("data-flag=\"consumo\""));
        await Prov.InvokeAsync(() => cons.Instance.ValueChanged.InvokeAsync(true));
        var sup = Prov.FindComponents<MudSwitch<bool?>>().First(s => s.Markup.Contains("consumo-superior"));
        var inf = Prov.FindComponents<MudSwitch<bool?>>().First(s => s.Markup.Contains("consumo-inferior"));
        await Prov.InvokeAsync(() => sup.Instance.ValueChanged.InvokeAsync(true));
        await Prov.InvokeAsync(() => inf.Instance.ValueChanged.InvokeAsync(true));
        Assert.NotEmpty(Prov.FindAll("[data-flag='espec-pivot']"));
        var piv = Prov.FindComponents<MudSwitch<bool?>>().First(s => s.Markup.Contains("espec-pivot"));
        await Prov.InvokeAsync(() => piv.Instance.ValueChanged.InvokeAsync(true));
        Prov.Find("[data-campo='DES_NAME_TABLE']").Change("INGENIERIA.TABLA_X");
        await Prov.InvokeAsync(() => Prov.Find("form").Submit());
        pg.WaitForAssertion(() => Assert.Equal(1, Cuantas("PUT", "/formulacion/componentes")));
        var b = Cuerpo(F, "PUT", "/formulacion/componentes");
        Assert.True(Prop(b, "FLG_CONSUMO").GetBoolean()); Assert.True(Prop(b, "FLG_CONSUMO_SUPERIOR").GetBoolean()); Assert.True(Prop(b, "FLG_CONSUMO_INFERIOR").GetBoolean()); Assert.True(Prop(b, "FLG_ESPECIFICACIONES_PIVOT").GetBoolean());
        Assert.Equal("INGENIERIA.TABLA_X", Prop(b, "DES_NAME_TABLE").GetString());
    }

    [Fact]
    public async Task SiLaApiRechaza_MuestraElMotivo_YElDialogoSigueAbierto()
    {
        var pg = Pagina();
        var fila = pg.FindAll("tbody tr").First(t => t.TextContent.Contains("CONSM"));
        await pg.InvokeAsync(() => fila.QuerySelector("[data-accion='editar']")!.Click());
        Prov!.WaitForAssertion(() => Assert.NotEmpty(Prov.FindAll("form")));
        F.MensajeFalla = "Ya existe un componente con ese codigo.";
        await Prov.InvokeAsync(() => Prov.Find("form").Submit());
        Prov.WaitForAssertion(() => Assert.Contains("Ya existe un componente", Prov.Markup));
        Assert.NotEmpty(Prov.FindAll("[data-campo='COD_COMPONENTE_FORMULACION']"));
    }

    [Fact]
    public async Task Eliminar_AvisaSiTieneElementos_YBorraElComponente()
    {
        var pg = Pagina();
        var fila = pg.FindAll("tbody tr").First(t => t.TextContent.Contains("ESPPR"));
        await pg.InvokeAsync(() => fila.QuerySelector("[data-accion='eliminar']")!.Click());
        Prov!.WaitForAssertion(() => Assert.NotEmpty(Prov.FindAll("form")));
        Assert.Empty(Prov.FindAll("[data-aviso='elementos']"));
        Assert.True(Deshabilitado(Prov.Find("[data-campo='DES_COMPONENTE_FORMULACION']")));         // en eliminar no se edita
        await Prov.InvokeAsync(() => Prov.Find("form").Submit());
        pg.WaitForAssertion(() => Assert.Equal(1, Cuantas("DELETE", "/formulacion/componentes/ESPPR")));
        pg.WaitForAssertion(() => Assert.DoesNotContain("ESPECIFICACIONES ORDENES PRODUCCION", pg.Markup));
    }

    [Fact]
    public async Task EliminarUnComponenteConElementos_MuestraElAviso()
    {
        var pg = Pagina();
        var fila = pg.FindAll("tbody tr").First(t => t.TextContent.Contains("CONSM"));
        await pg.InvokeAsync(() => fila.QuerySelector("[data-accion='eliminar']")!.Click());
        Prov!.WaitForAssertion(() => Assert.Contains("7 elemento", Prov.Find("[data-aviso='elementos']").TextContent));
    }

    [Fact]
    public void LaBusqueda_FiltraPorCodigoODescripcion()
    {
        var pg = Pagina();
        pg.Find(".grid-header-search input").Input("espec");
        pg.WaitForAssertion(() => Assert.Equal(2, pg.FindAll("[data-accion='editar']").Count));
    }

    // ---------------- nivel de elementos ----------------
    async Task<IRenderedComponent<FormulacionComponentes>> EnElementos(string comp)
    {
        var pg = Pagina();
        var fila = pg.FindAll("tbody tr").First(t => t.TextContent.Contains(comp));
        await pg.InvokeAsync(() => fila.QuerySelector("[data-accion='abrir-elementos']")!.Click());
        pg.WaitForAssertion(() => Assert.NotEmpty(pg.FindAll("[data-accion='cerrar-elementos']")));
        return pg;
    }

    [Fact]
    public async Task LosElementos_MuestranSuAliasEnElColorDelComponente_YElTipoDeEspecificacion()
    {
        var pg = await EnElementos("ESPEC");
        pg.WaitForAssertion(() => Assert.Contains("Gramaje", pg.Markup));
        Assert.Contains("ENTERO", pg.Markup);
        Assert.Contains("background:#0b8fc7", pg.Markup);
    }

    [Fact]
    public async Task NuevoElemento_SinCodigo_LaBaseLoAsigna_YElPostNoLoLleva()
    {
        var pg = await EnElementos("CONSM");
        pg.WaitForAssertion(() => Assert.Contains("CAN_CONSUMO", pg.Markup));
        await pg.InvokeAsync(() => pg.FindAll("[data-accion='nuevo']").Last().Click());
        Prov!.WaitForAssertion(() => Assert.NotEmpty(Prov.FindAll("[data-campo='DES_ELEMENTO_FORMULACION']")));
        Assert.Equal("", Prov.Find("[data-campo='COD_ELEMENTO_FORMULACION']").GetAttribute("value"));
        Prov.Find("[data-campo='DES_ELEMENTO_FORMULACION']").Change("CANTIDAD NETA"); Prov.Find("[data-campo='COD_ALIAS_FORMULACION']").Change("Neto");
        await Prov.InvokeAsync(() => Prov.Find("form").Submit());
        pg.WaitForAssertion(() => Assert.Equal(1, Cuantas("POST", "/formulacion/elementos")));
        var b = Cuerpo(F, "POST", "/formulacion/elementos");
        Assert.Equal("CONSM", Prop(b, "COD_COMPONENTE_FORMULACION").GetString()); Assert.Equal("Neto", Prop(b, "COD_ALIAS_FORMULACION").GetString());
        var cod = b.EnumerateObject().FirstOrDefault(x => string.Equals(x.Name, "COD_ELEMENTO_FORMULACION", StringComparison.OrdinalIgnoreCase));
        Assert.True(cod.Value.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null || cod.Value.GetString() == "");
    }

    [Fact]
    public async Task EditarUnElemento_TraeElDetalle_ConLosUsos_YHaceUnPutConElAliasNuevo()
    {
        var pg = await EnElementos("CONSM");
        pg.WaitForAssertion(() => Assert.Contains("CAN_CONSUMO", pg.Markup));
        var fila = pg.FindAll("tbody tr").First(t => t.TextContent.Contains("TASA DE MERMA"));
        await pg.InvokeAsync(() => fila.QuerySelector("[data-accion='editar']")!.Click());
        Prov!.WaitForAssertion(() => Assert.NotEmpty(Prov.FindAll("[data-campo='COD_ALIAS_FORMULACION']")));
        Assert.Contains(F.Calls, c => c.M == "GET" && c.U.EndsWith("/formulacion/elementos/CONSM/00005"));
        Assert.True(Deshabilitado(Prov.Find("[data-campo='COD_ELEMENTO_FORMULACION']")));
        Prov.Find("[data-campo='COD_ALIAS_FORMULACION']").Change("% de merma");
        await Prov.InvokeAsync(() => Prov.Find("form").Submit());
        pg.WaitForAssertion(() => Assert.Equal(1, Cuantas("PUT", "/formulacion/elementos")));
        var b = Cuerpo(F, "PUT", "/formulacion/elementos");
        Assert.Equal("CONSM", Prop(b, "COD_COMPONENTE_FORMULACION").GetString()); Assert.Equal("00005", Prop(b, "COD_ELEMENTO_FORMULACION").GetString());
        Assert.Equal("% de merma", Prop(b, "COD_ALIAS_FORMULACION").GetString()); Assert.Equal("TASA DE MERMA", Prop(b, "DES_ELEMENTO_FORMULACION").GetString());
    }

    [Fact]
    public async Task UnElementoEnUso_AvisaAlEditar_YUnoLibreNo()
    {
        var pg = await EnElementos("CONSM");
        pg.WaitForAssertion(() => Assert.Contains("CAN_CONSUMO", pg.Markup));
        var usado = pg.FindAll("tbody tr").First(t => t.TextContent.Contains("CAN_CONSUMO"));
        await pg.InvokeAsync(() => usado.QuerySelector("[data-accion='editar']")!.Click());
        Prov!.WaitForAssertion(() => Assert.Contains("3 ficha", Prov.Find("[data-aviso='usos']").TextContent));
    }

    [Fact]
    public async Task ElTipoDeEspecificacion_EstaSiempreHabilitado_AunqueElComponenteNoSeaDeEspecificaciones()
    {
        var pg = await EnElementos("CONSM");                                                                   // un componente que NO es de especificaciones
        pg.WaitForAssertion(() => Assert.Contains("CAN_CONSUMO", pg.Markup));
        await pg.InvokeAsync(() => pg.FindAll("[data-accion='nuevo']").Last().Click());
        Prov!.WaitForAssertion(() => Assert.NotEmpty(Prov.FindAll("[data-combo]")));
        Assert.Equal("False", Prov.Find("[data-combo]").GetAttribute("data-disabled"));
        Assert.DoesNotContain("Solo aplica", Prov.Find("[data-ayuda-tipo]").TextContent);
    }

    [Fact]
    public async Task ModificarUnElemento_ConservaSuTipo_YPermiteCambiarlo()
    {
        F.Elementos.First(e => e.COD_COMPONENTE_FORMULACION == "CONSM" && e.COD_ELEMENTO_FORMULACION == "00005").COD_TIPO_ESPECIFICACION_TECNICA = "ENT";
        var pg = await EnElementos("CONSM");
        pg.WaitForAssertion(() => Assert.Contains("TASA DE MERMA", pg.Markup));
        var fila = pg.FindAll("tbody tr").First(t => t.TextContent.Contains("TASA DE MERMA"));
        await pg.InvokeAsync(() => fila.QuerySelector("[data-accion='editar']")!.Click());
        Prov!.WaitForAssertion(() => Assert.NotEmpty(Prov.FindAll("form")));
        await Prov.InvokeAsync(() => Prov.Find("form").Submit());
        pg.WaitForAssertion(() => Assert.Equal(1, Cuantas("PUT", "/formulacion/elementos")));
        Assert.Equal("ENT", Prop(Cuerpo(F, "PUT", "/formulacion/elementos"), "COD_TIPO_ESPECIFICACION_TECNICA").GetString());
    }

    [Fact]
    public async Task ElTipoDeEspecificacion_EnUnComponenteDeEspecificaciones_EstaHabilitado()
    {
        var pg = await EnElementos("ESPEC");
        pg.WaitForAssertion(() => Assert.Contains("Gramaje", pg.Markup));
        await pg.InvokeAsync(() => pg.FindAll("[data-accion='nuevo']").Last().Click());
        Prov!.WaitForAssertion(() => Assert.NotEmpty(Prov.FindAll("[data-combo]")));
        Assert.Equal("False", Prov.Find("[data-combo]").GetAttribute("data-disabled"));
        Assert.Contains("Enlaza", Prov.Find("[data-ayuda-tipo]").TextContent);
    }

    [Fact]
    public async Task GuardarUnElemento_SinElementoOAlias_NoEnviaNada()
    {
        var pg = await EnElementos("CONSM");
        pg.WaitForAssertion(() => Assert.Contains("CAN_CONSUMO", pg.Markup));
        await pg.InvokeAsync(() => pg.FindAll("[data-accion='nuevo']").Last().Click());
        Prov!.WaitForAssertion(() => Assert.NotEmpty(Prov.FindAll("form")));
        await Prov.InvokeAsync(() => Prov.Find("form").Submit());
        Prov.WaitForAssertion(() => Assert.Contains("fragmento técnico", Prov.Markup));
        Assert.Equal(0, Cuantas("POST", "/formulacion/elementos"));
    }

    [Fact]
    public async Task EliminarUnElemento_HaceElDeleteConLaClaveCompuesta()
    {
        var pg = await EnElementos("CONSM");
        pg.WaitForAssertion(() => Assert.Contains("CAN_CONSUMO", pg.Markup));
        var fila = pg.FindAll("tbody tr").First(t => t.TextContent.Contains("TASA DE MERMA"));
        await pg.InvokeAsync(() => fila.QuerySelector("[data-accion='eliminar']")!.Click());
        Prov!.WaitForAssertion(() => Assert.NotEmpty(Prov.FindAll("form")));
        await Prov.InvokeAsync(() => Prov.Find("form").Submit());
        pg.WaitForAssertion(() => Assert.Equal(1, Cuantas("DELETE", "/formulacion/elementos/CONSM/00005")));
    }

    [Fact]
    public async Task SiLaApiRechazaElElemento_MuestraElMotivo()
    {
        var pg = await EnElementos("CONSM");
        pg.WaitForAssertion(() => Assert.Contains("CAN_CONSUMO", pg.Markup));
        var fila = pg.FindAll("tbody tr").First(t => t.TextContent.Contains("TASA DE MERMA"));
        await pg.InvokeAsync(() => fila.QuerySelector("[data-accion='editar']")!.Click());
        Prov!.WaitForAssertion(() => Assert.NotEmpty(Prov.FindAll("form")));
        F.MensajeFalla = "Ya hay un elemento con ese alias en el componente.";
        await Prov.InvokeAsync(() => Prov.Find("form").Submit());
        Prov.WaitForAssertion(() => Assert.Contains("Ya hay un elemento", Prov.Markup));
    }
}
