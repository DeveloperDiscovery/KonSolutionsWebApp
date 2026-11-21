using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Blazored.LocalStorage;
using Bunit;
using KONSolutions.Shared.Models.Formulacion;
using KONSolutions.Shared.Models.Ingenieria;
using KONSolutions.Web.Pages.Ingenieria;
using KONSolutions.Web.Pages.Shared;
using KONSolutions.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using Xunit;

// Formulas de consumo en la linea de la estructura (script SQL 147): la seccion «Fórmula de consumo» del dialogo del insumo, contra una API simulada.
public class FakeFormulaEstructura : HttpMessageHandler
{
    public List<(string M, string U, JsonElement? B)> Calls = new();
    public bool TieneFormula;                          // la linea ya viene con formula asignada
    public decimal? Total = 2.2222m;
    public string? MensajeFalla;                       // las escrituras de formula fallan con este mensaje
    public bool FallaLectura;                          // la lectura de las formulas disponibles falla
    public string? ErrorCalculo;                       // el calculo da este motivo (ES_VALIDA = false)
    public bool ConEspecificacion;                    // la formula de la linea trae una ficha de especificacion tecnica
    public string? NombreOrigen = "Consumo con merma (tela)";   // DES_FORMULACION_ORIGEN de la linea
    public string? TipoMat;                            // COD_TIPO_MATERIAL de la linea
    public bool Pivot;                                 // FLG_PRODUCTO_COMPONENTE_PIVOT de la linea
    public JsonElement? UltimoGuardarFormula;          // cuerpo del ultimo POST formula/guardar
    public List<PaletaFicha> Paleta = new()
    {
        new() { COD_COMPONENTE_FORMULACION = "CONSM", DES_COMPONENTE_FORMULACION = "DATOS DE CONSUMO", COD_ELEMENTO_FORMULACION = "00005", COD_ALIAS_FORMULACION = "%Merma", DES_ELEMENTO_FORMULACION = "TAS_MERMA", DES_BACKCOLOR = "#1f9d55", DES_FORECOLOR = "#ffffff" },
        new() { COD_COMPONENTE_FORMULACION = "CONST", DES_COMPONENTE_FORMULACION = "CONSTANTES", COD_ELEMENTO_FORMULACION = "00001", COD_ALIAS_FORMULACION = "ENT", DES_ELEMENTO_FORMULACION = "ENT", DES_BACKCOLOR = "#d93025", DES_FORECOLOR = "#ffffff" },
    };
    public List<DisenosEstructuraFormulaDisponible> Disponibles = new()
    {
        new() { NUM_FORMULACION = 7, DES_FORMULACION = "CONSUMO CON MERMA", DES_ALIAS = "(Consumo x 100 / (100 - %Merma))", CAN_FICHAS = 11, FLG_COINCIDE = true },
        new() { NUM_FORMULACION = 9, DES_FORMULACION = "CONSUMO MAS UNO", DES_ALIAS = "Consumo + 1", CAN_FICHAS = 3, FLG_COINCIDE = false },
    };

    static DisenosEstructuraFormulaFicha Fi(int n, string comp, string back, string alias, string tec) => new()
    { NUM_SECUENCIA_FORMULA = n, COD_COMPONENTE_FORMULACION = comp, DES_COMPONENTE_FORMULACION = comp, DES_BACKCOLOR = back, DES_FORECOLOR = "#ffffff", COD_ALIAS_FORMULACION = alias, DES_ELEMENTO_FORMULA = tec, DES_ELEMENTO_MASCARA_FORMULA = alias };

    List<DisenosEstructuraFormulaFicha> FichasLinea()
    {
        var l = new List<DisenosEstructuraFormulaFicha> { Fi(1, "CONSM", "#1f9d55", "Consumo", "CAN_CONSUMO"), Fi(2, "OPEMT", "#e07b1f", "+", "+"), Fi(3, "CONST", "#d93025", "1", "1") };
        if (ConEspecificacion) l.Add(Fi(4, "ESPEC", "#336699", "Espec", "Ancho util"));
        return l;
    }

    DisenosEstructuraFormulaLinea Linea() => TieneFormula
        ? new() { DES_FORMULA = "CAN_CONSUMO + 1", DES_MASCARA_FORMULA = "Consumo + 1", CAN_TOTAL = Total, CAN_CONSUMO = 2, TAS_MERMA = 10, CAN_FICHAS = 3, FLG_PRODUCTO_COMPONENTE_PIVOT = Pivot, COD_TIPO_MATERIAL = TipoMat, DES_FORMULACION_ORIGEN = NombreOrigen, NUM_FORMULACION_ORIGEN = NombreOrigen is null ? null : 7,
                  Fichas = FichasLinea() }
        : new() { CAN_FICHAS = 0, FLG_PRODUCTO_COMPONENTE_PIVOT = Pivot, COD_TIPO_MATERIAL = TipoMat };

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage r, CancellationToken ct)
    {
        var u = r.RequestUri!.PathAndQuery;
        JsonElement? body = r.Content is null ? null : JsonDocument.Parse(await r.Content.ReadAsStringAsync(ct)).RootElement.Clone();
        Calls.Add((r.Method.Method, u, body));
        HttpResponseMessage Ok(object? d) => new(HttpStatusCode.OK) { Content = JsonContent.Create(new { Success = true, Data = d }) };
        HttpResponseMessage Mal(string m) => new(HttpStatusCode.BadRequest) { Content = JsonContent.Create(new { Success = false, Message = m }) };
        var path = u.Split('?')[0];
        if (r.Method == HttpMethod.Get)
        {
            if (path.EndsWith("/tipos-material")) return Ok(new[] { new { Id = "01", Texto = "01 - Materia prima" }, new { Id = "02", Texto = "02 - Material complementario" }, new { Id = "04", Texto = "04 - Material de embalaje" } });
            if (path.EndsWith("/formulas/paleta")) return Ok(Paleta);
            if (path.EndsWith("/formula/disponibles")) return FallaLectura ? Mal("No tienes permiso para ver las formulas.") : Ok(Disponibles);
            if (path.Contains("productos") && path.Contains("search")) return Ok(new[] { new { COD_PRODUCTO = "P1", DES_PRODUCTO = "Tela", COD_UNIDAD_MEDIDA = "MTR", COD_GRUPO_PRODUCTO = "G1", COD_FAMILIA_PRODUCTO = "F1", COD_SUB_FAMILIA_PRODUCTO = "S1" } });
            if (path.Contains("grupos-producto/combobox")) return Ok(new[] { new { Id = "G1", Texto = "TELAS" } });
            if (path.EndsWith("/sub-familias-producto")) return Ok(new[] { new { COD_SUB_FAMILIA_PRODUCTO = "S1", DES_SUB_FAMILIA_PRODUCTO = "JERSEY" } });
            if (path.EndsWith("/familias-producto")) return Ok(new[] { new { COD_FAMILIA_PRODUCTO = "F1", DES_FAMILIA_PRODUCTO = "TEJIDO" } });
            if (path.EndsWith("/formula")) return Ok(Linea());
            if (path.Contains("tipos-merma/combobox")) return Ok(new[] { new { Id = "PRO", Texto = "PRODUCCION" } });
            if (path.Contains("monedas/combobox")) return Ok(new[] { new { Id = "PEN", Texto = "SOLES" } });
            return Ok(new object[0]);
        }
        if (path.EndsWith("/formula/guardar"))
        {
            if (MensajeFalla != null) return Mal(MensajeFalla);
            UltimoGuardarFormula = body; TieneFormula = true;
            return Ok(new DisenosEstructuraFormulaResultado { ES_VALIDA = ErrorCalculo == null, CAN_TOTAL = ErrorCalculo == null ? Total : null, DES_ERROR = ErrorCalculo });
        }
        if (path.EndsWith("/formula/asignar") || path.EndsWith("/formula/quitar") || path.EndsWith("/formula/calcular"))
        {
            if (MensajeFalla != null) return Mal(MensajeFalla);
            if (path.EndsWith("/formula/asignar")) TieneFormula = true;
            if (path.EndsWith("/formula/quitar")) { TieneFormula = false; Total = null; return Ok(true); }
            return Ok(new DisenosEstructuraFormulaResultado { ES_VALIDA = ErrorCalculo == null, CAN_TOTAL = ErrorCalculo == null ? Total : null, DES_ERROR = ErrorCalculo });
        }
        return Ok(new DisenosEstructuraInsumoResultado { NUM_SECUENCIA = 1 });
    }
}

public class FormulaEstructuraTests : TestContext
{
    readonly FakeFormulaEstructura F = new();

    IRenderedComponent<MudDialogProvider> Prov()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton(new HttpClient(F) { BaseAddress = new Uri("http://x/") });
        Services.AddBlazoredLocalStorage();
        Services.AddScoped<ErrorLogService>(); Services.AddScoped<ConfigService>();
        RenderComponent<MudPopoverProvider>(); RenderComponent<MudSnackbarProvider>();
        return RenderComponent<MudDialogProvider>();
    }

    static DisenosEstructuraNodo Insumo() => new()
    { TIPO_NODO = "INS", COD_DISENO = "D1", NUM_VERSION = 1, COD_SECTOR_PRODUCTO = "FRENT", COD_COMPONENTE_PRODUCTO = "PROCE", NUM_SECUENCIA = 1, COD_PRODUCTO_COMPONENTE = "P1", DESCRIPCION = "Tela", CAN_CONSUMO = 2, TAS_MERMA = 10, COD_UNIDAD_MEDIDA = "MTR" };

    async Task<IRenderedComponent<MudDialogProvider>> Abre(bool nuevo = false)
    {
        var p = Prov();
        var ps = new DialogParameters { ["CodDiseno"] = "D1", ["NumVersion"] = 1, ["CodSector"] = "FRENT", ["CodComponente"] = "PROCE" };
        if (!nuevo) ps["Nodo"] = Insumo();
        await p.InvokeAsync(async () => await Services.GetRequiredService<IDialogService>().ShowAsync<DisenosEstructuraInsumoDialog>("t", ps, new DialogOptions { MaxWidth = MaxWidth.False }));
        p.WaitForAssertion(() => p.Find("[data-panel='formula']"));
        return p;
    }

    static JsonElement Prop(JsonElement e, string n) => e.EnumerateObject().First(x => string.Equals(x.Name, n, StringComparison.OrdinalIgnoreCase)).Value;
    IRenderedComponent<AriaCombo<DisenosEstructuraFormulaDisponible>> Combo(IRenderedComponent<MudDialogProvider> p) => p.FindComponent<AriaCombo<DisenosEstructuraFormulaDisponible>>();
    IRenderedComponent<AriaCombo<KONSolutions.Shared.Common.ComboboxItem>> Cmb(IRenderedComponent<MudDialogProvider> p, string label) => p.FindComponents<AriaCombo<KONSolutions.Shared.Common.ComboboxItem>>().First(c => c.Instance.Label == label);
    static bool Deshabilitado(AngleSharp.Dom.IElement e) => e.HasAttribute("disabled") || (e.ClassName ?? "").Contains("mud-disabled");

    [Fact]
    public async Task AlModificarUnInsumo_GrupoFamiliaYSubFamilia_SalenDelProductoGuardado()
    {
        var p = await Abre();
        p.WaitForAssertion(() => Assert.Equal("G1", Cmb(p, "Grupo").Instance.Value));
        Assert.Equal("F1", Cmb(p, "Familia").Instance.Value); Assert.Equal("S1", Cmb(p, "Sub familia").Instance.Value);
        Assert.Contains(Cmb(p, "Familia").Instance.Items, x => x.Id == "F1");                       // y sus listas dependientes estan cargadas
        Assert.Contains(Cmb(p, "Sub familia").Instance.Items, x => x.Id == "S1");
    }

    // ---------- editar la copia de la formula de la linea
    async Task<IRenderedComponent<MudDialogProvider>> EditandoFormula()
    {
        F.TieneFormula = true;
        var p = await Abre();
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-accion='formula-editar']")));
        await p.InvokeAsync(() => p.Find("[data-accion='formula-editar']").Click());
        p.WaitForAssertion(() => Assert.Equal(3, p.FindAll("[data-ficha]").Count));
        return p;
    }

    [Fact]
    public async Task EditarFormula_CambiarUnaConstante_SeGrabaConLasFichasEnOrden()
    {
        var p = await EditandoFormula();
        p.Find("input[data-valor-ficha='2']").Change("5");                                           // la constante, ahi mismo
        await p.InvokeAsync(() => p.Find("[data-accion='ed-guardar']").Click());
        p.WaitForAssertion(() => Assert.NotNull(F.UltimoGuardarFormula));
        var fichas = Prop(F.UltimoGuardarFormula!.Value, "Fichas").EnumerateArray().ToList();
        Assert.Equal(new[] { "CONSM", "OPEMT", "CONST" }, fichas.Select(x => Prop(x, "COD_COMPONENTE_FORMULACION").GetString()).ToArray());
        Assert.Equal("5", Prop(fichas[2], "DES_FORMULA").GetString());
        Assert.Equal("D1", Prop(F.UltimoGuardarFormula!.Value, "COD_DISENO").GetString());
        p.WaitForAssertion(() => Assert.Empty(p.FindAll("[data-formula-editor]")));              // sale de la edicion
    }

    [Fact]
    public async Task EditarFormula_DobleClicQuitaLaFicha()
    {
        var p = await EditandoFormula();
        p.Find("[data-ficha='1']").DoubleClick();
        p.WaitForAssertion(() => Assert.Equal(2, p.FindAll("[data-ficha]").Count));
        await p.InvokeAsync(() => p.Find("[data-accion='ed-guardar']").Click());
        p.WaitForAssertion(() => Assert.NotNull(F.UltimoGuardarFormula));
        Assert.Equal(2, Prop(F.UltimoGuardarFormula!.Value, "Fichas").GetArrayLength());
    }

    [Fact]
    public async Task EditarFormula_UsaElMismoEditorDeLaPantallaFormulas_ConPaleta()
    {
        var p = await EditandoFormula();
        p.WaitForAssertion(() => Assert.NotEmpty(p.Find("[data-formula-editor]").QuerySelectorAll("[data-comp]")));      // la paleta de componentes ya llego
        Assert.NotEmpty(p.Find("[data-formula-editor]").QuerySelectorAll("[data-zona='lienzo'], [data-lienzo]"));
        Assert.Contains(F.Calls, c => c.M == "GET" && c.U.Contains("formulas/paleta"));
    }

    [Fact]
    public async Task EditarFormula_AdicionaUnElementoDeLaPaleta_AlFinal_YInsertaAntesDeLaSeleccionada()
    {
        var p = await EditandoFormula();
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-el='00005']")));
        p.Find("[data-el='00005']").Click();                                                      // elige %Merma en la paleta
        await p.InvokeAsync(() => p.Find("[data-accion='adicionar']").Click());
        p.WaitForAssertion(() => Assert.Equal(4, p.FindAll("[data-ficha]").Count));
        Assert.Contains("%Merma", p.Find("[data-ficha='3']").TextContent);                        // al final
        p.Find("[data-ficha='0']").Click();                                                       // selecciona la primera
        await p.InvokeAsync(() => p.Find("[data-accion='insertar']").Click());
        p.WaitForAssertion(() => Assert.Equal(5, p.FindAll("[data-ficha]").Count));
        Assert.Contains("%Merma", p.Find("[data-ficha='0']").TextContent);                        // antes de la seleccionada
        await p.InvokeAsync(() => p.Find("[data-accion='ed-guardar']").Click());
        p.WaitForAssertion(() => Assert.NotNull(F.UltimoGuardarFormula));
        Assert.Equal(5, Prop(F.UltimoGuardarFormula!.Value, "Fichas").GetArrayLength());
    }

    [Fact]
    public async Task EditarFormula_UnaEspecificacionConservaSuNombre_YLasDemasFichasNoMandanTexto()
    {
        F.TieneFormula = true; F.ConEspecificacion = true;
        F.Paleta.Add(new() { COD_COMPONENTE_FORMULACION = "ESPEC", DES_COMPONENTE_FORMULACION = "ESPECIFICACIONES", FLG_ESPECIFICACIONES = true, COD_ELEMENTO_FORMULACION = "00001", COD_ALIAS_FORMULACION = "Espec", COD_ESPECIFICACION_TECNICA = "00011", DES_ESPECIFICACION_TECNICA_SINGULAR = "Ancho util" });
        var p = await Abre();
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-accion='formula-editar']")));
        await p.InvokeAsync(() => p.Find("[data-accion='formula-editar']").Click());
        p.WaitForAssertion(() => Assert.Equal(4, p.FindAll("[data-ficha]").Count));
        p.WaitForAssertion(() => Assert.Contains("Ancho util", p.Find("[data-ficha='3']").TextContent));     // se ve el nombre, no el alias del elemento
        await p.InvokeAsync(() => p.Find("[data-accion='ed-guardar']").Click());
        p.WaitForAssertion(() => Assert.NotNull(F.UltimoGuardarFormula));
        var fichas = Prop(F.UltimoGuardarFormula!.Value, "Fichas").EnumerateArray().ToList();
        Assert.Equal("Ancho util", Prop(fichas[3], "DES_FORMULA").GetString());
        Assert.Equal(JsonValueKind.Null, Prop(fichas[1], "DES_FORMULA").ValueKind);
    }

    [Fact]
    public async Task EditarFormula_UnaConstanteNuevaNecesitaValor()
    {
        var p = await EditandoFormula();
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-comp='CONST']")));
        p.Find("[data-comp='CONST']").Click();
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-el='00001']")));
        p.Find("[data-el='00001']").Click();
        await p.InvokeAsync(() => p.Find("[data-accion='adicionar']").Click());                   // sin valor: avisa y no agrega
        Assert.Equal(3, p.FindAll("[data-ficha]").Count);
        p.Find("input[data-campo='valor'], [data-campo='valor'] input").Input("7");
        await p.InvokeAsync(() => p.Find("[data-accion='adicionar']").Click());
        p.WaitForAssertion(() => Assert.Equal(4, p.FindAll("[data-ficha]").Count));
        Assert.Equal("7", p.Find("input[data-valor-ficha='3']").GetAttribute("value"));
    }

    [Fact]
    public async Task EditarFormula_MoverLaFichaSeleccionada_CambiaElOrden()
    {
        var p = await EditandoFormula();
        p.Find("[data-ficha='0']").Click();
        await p.InvokeAsync(() => p.Find("[data-accion='bajar']").Click());
        await p.InvokeAsync(() => p.Find("[data-accion='ed-guardar']").Click());
        p.WaitForAssertion(() => Assert.NotNull(F.UltimoGuardarFormula));
        var comps = Prop(F.UltimoGuardarFormula!.Value, "Fichas").EnumerateArray().Select(x => Prop(x, "COD_COMPONENTE_FORMULACION").GetString()).ToArray();
        Assert.Equal(new[] { "OPEMT", "CONSM", "CONST" }, comps);
    }

    [Fact]
    public async Task EditarFormula_Cancelar_NoGrabaNada()
    {
        var p = await EditandoFormula();
        p.Find("[data-ficha='1']").DoubleClick();
        await p.InvokeAsync(() => p.Find("[data-accion='ed-cancelar']").Click());
        p.WaitForAssertion(() => Assert.Empty(p.FindAll("[data-formula-editor]")));
        Assert.Null(F.UltimoGuardarFormula);
        Assert.Equal(3, p.Find("[data-formula-fichas]").QuerySelectorAll("span").Length);          // la formula sigue como estaba
    }

    [Fact]
    public async Task EditarFormula_SiLaApiRechaza_MuestraElMotivo_YLaEdicionSigueAbierta()
    {
        var p = await EditandoFormula();
        F.MensajeFalla = "La version esta bloqueada.";
        await p.InvokeAsync(() => p.Find("[data-accion='ed-guardar']").Click());
        p.WaitForAssertion(() => Assert.Contains("bloqueada", p.Find("[data-formula-error]").TextContent));
        Assert.NotEmpty(p.FindAll("[data-formula-editor]"));
    }

    [Fact]
    public async Task EditarFormula_SinFichasNoSePuedeGuardar()
    {
        var p = await EditandoFormula();
        for (var i = 0; i < 3; i++) p.Find("[data-ficha='0']").DoubleClick();
        p.WaitForAssertion(() => Assert.Empty(p.FindAll("[data-ficha]")));
        Assert.True(Deshabilitado(p.Find("[data-accion='ed-guardar']")));
    }

    // ---------- tipo de material de la linea
    [Fact]
    public async Task TipoMaterial_ElComboTraeLosTipos_YSaleDeLaLinea()
    {
        F.TipoMat = "02";
        var p = await Abre();
        p.WaitForAssertion(() => Assert.Equal(3, Cmb(p, "Tipo de material").Instance.Items.Count()));
        p.WaitForAssertion(() => Assert.Equal("02", Cmb(p, "Tipo de material").Instance.Value));          // el que la linea ya tiene
        Assert.Contains(F.Calls, c => c.M == "GET" && c.U.EndsWith("/tipos-material"));
    }

    [Fact]
    public async Task TipoMaterial_SinElegirVaNulo_YLaAyudaDiceQueLoDecideLaBase()
    {
        var p = await Abre();                                                                              // la linea no tiene tipo
        p.WaitForAssertion(() => Assert.Equal(3, Cmb(p, "Tipo de material").Instance.Items.Count()));
        Assert.Contains("el del componente o el de la sub familia", p.Find("[data-ayuda='tipo-material']").TextContent);   // sin elegir: lo decide la base
        await p.InvokeAsync(() => p.Find("[data-accion='guardar']").Click());
        p.WaitForAssertion(() => Assert.Contains(F.Calls, c => c.M == "POST" && c.U.EndsWith("/insumo")));
        Assert.Equal(JsonValueKind.Null, Prop(F.Calls.Last(c => c.M == "POST" && c.U.EndsWith("/insumo")).B!.Value, "COD_TIPO_MATERIAL").ValueKind);
    }

    [Fact]
    public async Task TipoMaterial_ElegidoSeEnviaEnElGuardado()
    {
        F.TipoMat = "01";
        var p = await Abre();
        p.WaitForAssertion(() => Assert.Equal("01", Cmb(p, "Tipo de material").Instance.Value));
        await p.InvokeAsync(() => Cmb(p, "Tipo de material").Instance.ValueChanged.InvokeAsync("04"));      // cambia a embalaje
        await p.InvokeAsync(() => p.Find("[data-accion='guardar']").Click());
        p.WaitForAssertion(() => Assert.Contains(F.Calls, c => c.M == "POST" && c.U.EndsWith("/insumo")));
        Assert.Equal("04", Prop(F.Calls.Last(c => c.M == "POST" && c.U.EndsWith("/insumo")).B!.Value, "COD_TIPO_MATERIAL").GetString());
    }

    // ---------- FLG_PRODUCTO_COMPONENTE_PIVOT
    [Fact]
    public async Task Pivot_SaleDeLaLinea_YSeGrabaSinMarcarComoFalse()
    {
        F.TieneFormula = true; F.Pivot = true;
        var p = await Abre();
        p.WaitForAssertion(() => Assert.True(p.FindComponents<MudSwitch<bool>>().First(s => s.Markup.Contains("producto-pivot")).Instance.Value));
        var sw = p.FindComponents<MudSwitch<bool>>().First(s => s.Markup.Contains("producto-pivot"));
        await p.InvokeAsync(() => sw.Instance.ValueChanged.InvokeAsync(false));                  // se desmarca
        await p.InvokeAsync(() => p.Find("[data-accion='guardar']").Click());
        p.WaitForAssertion(() => Assert.Contains(F.Calls, c => c.M == "POST" && c.U.EndsWith("/insumo")));
        var b = F.Calls.Last(c => c.M == "POST" && c.U.EndsWith("/insumo")).B!.Value;
        Assert.Equal(JsonValueKind.False, Prop(b, "FLG_PRODUCTO_COMPONENTE_PIVOT").ValueKind);   // false, no null
    }

    [Fact]
    public async Task Pivot_SinTocarElCheck_EnUnInsumoNuevo_VaFalse()
    {
        var p = await Abre(nuevo: true);
        Assert.False(p.FindComponents<MudSwitch<bool>>().First(s => s.Markup.Contains("producto-pivot")).Instance.Value);
    }

    [Fact]
    public async Task SinFormulasActivas_ExplicaPorQueNoHayNinguna()
    {
        F.Disponibles = new();
        var p = await Abre();
        Assert.Contains("No hay fórmulas ACTIVAS", p.Find("[data-formula-sin-disponibles]").TextContent);
    }

    [Fact]
    public async Task SiLaLecturaDeLasFormulasFalla_SeMuestraElMotivo_YNoElAvisoDeQueNoHay()
    {
        F.FallaLectura = true;
        var p = await Abre();
        Assert.Contains("permiso", p.Find("[data-formula-error]").TextContent);
        Assert.Empty(p.FindAll("[data-formula-sin-disponibles]"));
    }

    [Fact]
    public async Task UnInsumoNuevo_PideGuardarloAntesDeAsignarFormula()
    {
        var p = await Abre(nuevo: true);
        Assert.Contains("Guarda el insumo", p.Find("[data-formula-nueva]").TextContent);
        Assert.Empty(p.FindAll("[data-accion='formula-asignar']"));
        Assert.DoesNotContain(F.Calls, c => c.U.Contains("/formula"));                               // no se pregunta por formulas de una linea que no existe
    }

    [Fact]
    public async Task SinFormula_OfreceLasDisponibles_YLoDiceClaro()
    {
        var p = await Abre();
        Assert.Contains("no tiene fórmula", p.Find("[data-formula-vacia]").TextContent);
        Assert.Empty(p.FindAll("[data-formula-texto]"));
        Assert.Equal(2, Combo(p).Instance.Items.Count());
        Assert.Contains(F.Calls, c => c.M == "GET" && c.U.Contains("/formula/disponibles") && c.U.Contains("COD_PRODUCTO_COMPONENTE=P1") && c.U.Contains("NUM_SECUENCIA=1"));
        Assert.True(Deshabilitado(p.Find("[data-accion='formula-asignar']")));                      // sin elegir no se puede asignar
    }

    [Fact]
    public async Task LaFormulaAsignada_MuestraSuNombre()
    {
        F.TieneFormula = true;
        var p = await Abre();
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-formula-nombre]")));
        Assert.Contains("Consumo con merma (tela)", p.Find("[data-formula-nombre]").TextContent);     // nombre de la formula de la que salio
        Assert.Contains("Fórmula asignada", p.Find("[data-formula-nombre]").TextContent);
    }

    [Fact]
    public async Task LaFormulaSinOrigen_LoDice_ComoFormulaPropia()
    {
        F.TieneFormula = true; F.NombreOrigen = null;                                                  // linea anterior al 157: sin nombre de origen
        var q = await Abre();
        q.WaitForAssertion(() => Assert.NotEmpty(q.FindAll("[data-formula-nombre]")));
        Assert.Contains("propia de esta línea", q.Find("[data-formula-nombre]").TextContent);
    }

    [Fact]
    public async Task SinFormula_NoHayBloqueDeNombre()
    {
        F.TieneFormula = false;
        var r = await Abre();
        Assert.Empty(r.FindAll("[data-formula-nombre]"));
    }

    [Fact]
    public async Task Asignar_EnviaLaLineaYLaFormulaElegida_Y_MuestraLaFormulaConSuTotal()
    {
        var p = await Abre();
        await p.InvokeAsync(() => Combo(p).Instance.ValueChanged.InvokeAsync("7"));
        await p.InvokeAsync(() => p.Find("[data-accion='formula-asignar']").Click());
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-formula-texto]")));
        var b = F.Calls.Last(c => c.M == "POST" && c.U.EndsWith("/formula/asignar")).B!.Value;
        Assert.Equal(7, Prop(b, "NUM_FORMULACION").GetInt32()); Assert.Equal("P1", Prop(b, "COD_PRODUCTO_COMPONENTE").GetString());
        Assert.Equal("FRENT", Prop(b, "COD_SECTOR_PRODUCTO").GetString()); Assert.Equal(1, Prop(b, "NUM_SECUENCIA").GetInt32());
        Assert.Equal("Consumo + 1", p.Find("[data-formula-texto]").TextContent.Trim());
        Assert.Contains("2,2222", p.Find("[data-formula-total]").TextContent.Replace('.', ','));
        Assert.Equal(3, p.Find("[data-formula-fichas]").Children.Length);
        Assert.Contains("background:#1f9d55", p.Find("[data-formula-fichas]").Children[0].GetAttribute("style"));   // cada ficha con el color de su componente
        Assert.Empty(p.FindAll("[data-formula-error]"));
    }

    [Fact]
    public async Task SiLaFormulaNoSePuedeCalcular_QuedaAsignada_YSeMuestraElMotivo()
    {
        F.ErrorCalculo = "La formula usa fichas que todavia no se pueden calcular: \"Cantidad Total\".";
        var p = await Abre();
        await p.InvokeAsync(() => Combo(p).Instance.ValueChanged.InvokeAsync("9"));
        await p.InvokeAsync(() => p.Find("[data-accion='formula-asignar']").Click());
        p.WaitForAssertion(() => Assert.Contains("Cantidad Total", p.Find("[data-formula-error]").TextContent));
        Assert.NotEmpty(p.FindAll("[data-formula-texto]"));                                          // la asignacion si quedo
    }

    [Fact]
    public async Task SiLaApiRechazaLaAsignacion_MuestraElMotivo_YNoMarcaLaFormula()
    {
        F.MensajeFalla = "Solo se pueden asignar formulas ACTIVAS: activa la formula en el constructor.";
        var p = await Abre();
        await p.InvokeAsync(() => Combo(p).Instance.ValueChanged.InvokeAsync("7"));
        await p.InvokeAsync(() => p.Find("[data-accion='formula-asignar']").Click());
        p.WaitForAssertion(() => Assert.Contains("ACTIVAS", p.Find("[data-formula-error]").TextContent));
        Assert.Empty(p.FindAll("[data-formula-texto]"));
    }

    [Fact]
    public async Task ConFormula_SeVeTexto_Fichas_Total_YSePuedeCalcularYQuitar()
    {
        F.TieneFormula = true;
        var p = await Abre();
        Assert.Equal("Consumo + 1", p.Find("[data-formula-texto]").TextContent.Trim());
        Assert.Empty(p.FindAll("[data-formula-vacia]"));
        await p.InvokeAsync(() => p.Find("[data-accion='formula-calcular']").Click());
        p.WaitForAssertion(() => Assert.Contains(F.Calls, c => c.M == "POST" && c.U.EndsWith("/formula/calcular")));
        var b = F.Calls.Last(c => c.U.EndsWith("/formula/calcular")).B!.Value;
        Assert.True(Prop(b, "GUARDAR").GetBoolean());
        await p.InvokeAsync(() => p.Find("[data-accion='formula-quitar']").Click());
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-formula-vacia]")));
        Assert.Contains(F.Calls, c => c.M == "POST" && c.U.EndsWith("/formula/quitar"));
        Assert.Empty(p.FindAll("[data-formula-texto]"));
    }

    [Fact]
    public async Task ElTotalVacio_SeMuestraComoRayaYNoComoCero()
    {
        F.TieneFormula = true; F.Total = null;
        var p = await Abre();
        Assert.Contains("—", p.Find("[data-formula-total]").TextContent);
    }

    [Fact]
    public async Task UnaVersionBloqueada_MuestraElMensajeDeLaBaseAlQuitar()
    {
        F.TieneFormula = true; F.MensajeFalla = "La version esta bloqueada: desbloqueela para modificar su estructura.";
        var p = await Abre();
        await p.InvokeAsync(() => p.Find("[data-accion='formula-quitar']").Click());
        p.WaitForAssertion(() => Assert.Contains("bloqueada", p.Find("[data-formula-error]").TextContent));
        Assert.NotEmpty(p.FindAll("[data-formula-texto]"));                                          // sigue con su formula
    }
}
