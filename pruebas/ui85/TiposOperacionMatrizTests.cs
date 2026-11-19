using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Blazored.LocalStorage;
using Bunit;
using KONSolutions.Shared.Models.Estandar;
using KONSolutions.Web.Pages.Estandar;
using KONSolutions.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using Xunit;

public class FakeMatriz : HttpMessageHandler
{
    public List<(string M, string U, JsonElement? B)> Calls = new();
    public bool Falla;               // la API rechaza el POST
    public bool PideConfirmar;       // quitar un valor con afectados pide confirmar
    public List<(string tipo, string valor, string af)> Formulas = new() { ("CMLBI", "IGV", "BSIMP") };
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage r, CancellationToken ct)
    {
        var u = r.RequestUri!.PathAndQuery;
        JsonElement? body = r.Content is null ? null : JsonDocument.Parse(await r.Content.ReadAsStringAsync(ct)).RootElement.Clone();
        Calls.Add((r.Method.Method, u, body));
        object data;
        if (r.Method == HttpMethod.Get)
        {
            if (u.Contains("matriz-tipos-operacion/formulas")) data = Formulas.Select(f => new { COD_TIPO_OPERACION = f.tipo, COD_TIPO_VALOR_CALCULO = f.valor, COD_TIPO_VALOR_CALCULO_AFECTADO = f.af }).ToArray();
            else if (u.Contains("tipos-valor-calculo")) data = new object[] {
                new { COD_TIPO_VALOR_CALCULO = "BSIMP", DES_TIPO_VALOR_CALCULO = "BASE IMPONIBLE", FLG_POSITIVO = true, FLG_IMPUESTO = false },
                new { COD_TIPO_VALOR_CALCULO = "DSCTS", DES_TIPO_VALOR_CALCULO = "DESCUENTOS", FLG_POSITIVO = false, FLG_IMPUESTO = false },
                new { COD_TIPO_VALOR_CALCULO = "IGV", DES_TIPO_VALOR_CALCULO = "IMPUESTO GENERAL A LAS VENTAS", FLG_POSITIVO = true, FLG_IMPUESTO = true },
                new { COD_TIPO_VALOR_CALCULO = "IMTOT", DES_TIPO_VALOR_CALCULO = "IMPORTE TOTAL", FLG_POSITIVO = true, FLG_IMPUESTO = false } };
            else if (u.Contains("tipos-comprobantes-pago/combobox")) data = new[] { new { Id = "FCTRA", Texto = "FACTURA" }, new { Id = "BLETA", Texto = "BOLETA DE VENTA" } };
            else if (u.Contains("matriz-tipos-operacion/calculo")) data = new object[] { new { COD_TIPO_OPERACION = "CMLBI", COD_TIPO_VALOR_CALCULO = "BSIMP", NUM_ORDEN_PRESENTACION = 1, N_AFECTADOS = 0, N_REFERENCIADO = 2, FLG_CALCULO_INVERSO = false, FLG_MOSTRAR_COMPROBANTE = true, FLG_VALOR_MOSTRAR = true, FLG_POSITIVO = (bool?)null }, new { COD_TIPO_OPERACION = "CMLBI", COD_TIPO_VALOR_CALCULO = "IGV", NUM_ORDEN_PRESENTACION = 6, N_AFECTADOS = 1, N_REFERENCIADO = 0, FLG_CALCULO_INVERSO = false, FLG_MOSTRAR_COMPROBANTE = true, FLG_VALOR_MOSTRAR = true, FLG_POSITIVO = (bool?)null }, new { COD_TIPO_OPERACION = "CMLBI", COD_TIPO_VALOR_CALCULO = "DSCTS", NUM_ORDEN_PRESENTACION = 8, N_AFECTADOS = 0, N_REFERENCIADO = 0, FLG_CALCULO_INVERSO = false, FLG_MOSTRAR_COMPROBANTE = true, FLG_VALOR_MOSTRAR = true, FLG_POSITIVO = (bool?)null } };
            else if (u.Contains("matriz-tipos-operacion/comprobantes")) data = new object[] { new { COD_TIPO_OPERACION = "CMLBI", COD_TIPO_COMPROBANTE_PAGO = "FCTRA" } };
            else data = new object[0];
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(new { Success = true, Data = data }) };
        }
        if (u.Contains("/formulas"))
        {
            if (Falla) return new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = JsonContent.Create(new { Success = false, Message = "Todos los valores que suma deben estar en el mismo tipo de operacion." }) };
            var tipoF = body!.Value.EnumerateObject().First(x => x.Name.Equals("COD_TIPO_OPERACION", StringComparison.OrdinalIgnoreCase)).Value.GetString()!;
            var valorF = body.Value.EnumerateObject().First(x => x.Name.Equals("COD_TIPO_VALOR_CALCULO", StringComparison.OrdinalIgnoreCase)).Value.GetString()!;
            Formulas.RemoveAll(f => f.tipo == tipoF && f.valor == valorF);
            foreach (var a in body.Value.EnumerateObject().First(x => x.Name.Equals("AFECTADOS", StringComparison.OrdinalIgnoreCase)).Value.EnumerateArray()) Formulas.Add((tipoF, valorF, a.GetString()!));
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(new { Success = true, Data = new { ACCION = "GUARDADO", N_AGREGADOS = 1, N_QUITADOS = 0, N_ORDEN_POSTERIOR = 0 } }) };
        }
        if (Falla) return new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = JsonContent.Create(new { Success = false, Message = "El tipo de operacion no existe en la clase." }) };
        bool activo = body!.Value.EnumerateObject().First(x => x.Name.Equals("ACTIVO", StringComparison.OrdinalIgnoreCase)).Value.GetBoolean();
        bool forzar = body.Value.EnumerateObject().Any(x => x.Name.Equals("FORZAR", StringComparison.OrdinalIgnoreCase) && x.Value.GetBoolean());
        string accion = activo ? "AGREGADO" : (PideConfirmar && !forzar ? "CONFIRMAR" : "ELIMINADO");
        return new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(new { Success = true, Data = new { ACCION = accion, N_AFECTADOS = 1, N_REFERENCIADO = 0, MENSAJE = "El valor suma 1 valor(es); al quitarlo se eliminan esas relaciones." } }) };
    }
}

public class TiposOperacionMatrizTests : TestContext
{
    FakeMatriz _f = new();
    IRenderedComponent<MudDialogProvider> _dlg = default!;
    IRenderedComponent<MudSnackbarProvider> _snack = default!;
    static List<TipoOperacion> Tipos() => new()
    {
        new() { COD_CLASE_OPERACION = "CMPRS", COD_TIPO_OPERACION = "CMLBI", DES_TIPO_OPERACION = "COMPRA LOCAL DE BIENES" },
        new() { COD_CLASE_OPERACION = "CMPRS", COD_TIPO_OPERACION = "CMLSR", DES_TIPO_OPERACION = "COMPRA LOCAL DE SERVICIOS" },
    };

    IRenderedComponent<TiposOperacionMatriz> Dibuja(string modo = "calculo")
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton(new HttpClient(_f) { BaseAddress = new Uri("http://x/") });
        Services.AddBlazoredLocalStorage();
        Services.AddScoped<ErrorLogService>(); Services.AddScoped<ConfigService>();
        RenderComponent<MudPopoverProvider>(); _snack = RenderComponent<MudSnackbarProvider>(); _dlg = RenderComponent<MudDialogProvider>();
        var c = RenderComponent<TiposOperacionMatriz>(p => p.Add(x => x.CodClase, "CMPRS").Add(x => x.Tipos, Tipos()).Add(x => x.Modo, modo));
        c.WaitForState(() => c.FindAll("input[type=checkbox]").Count > 0);
        return c;
    }

    static bool Marcado(IRenderedComponent<TiposOperacionMatriz> c, string celda) => c.Find($"input[data-celda='{celda}']").HasAttribute("checked");
    static JsonElement P(JsonElement e, string n) => e.EnumerateObject().First(x => x.Name.Equals(n, StringComparison.OrdinalIgnoreCase)).Value;

    [Fact]
    public void Calculo_MuestraColumnasPorTipo_FilasPorValor_YLasMarcasQueExisten()
    {
        var c = Dibuja();
        Assert.Equal(2 * 4, c.FindAll("input[type=checkbox]").Count);     // 2 tipos (columnas) x 4 valores (filas)
        Assert.Equal(new[] { "CMLBI", "CMLSR" }, c.FindAll("thead th").Skip(1).Select(x => x.TextContent.Substring(0, 5)).ToArray());
        Assert.Equal("BSIMP", c.FindAll("tbody tr")[0].GetAttribute("data-fila"));        // filas por orden de calculo
        Assert.True(Marcado(c, "CMLBI|BSIMP"));
        Assert.True(Marcado(c, "CMLBI|IGV"));
        Assert.False(Marcado(c, "CMLBI|IMTOT"));
        Assert.False(Marcado(c, "CMLSR|BSIMP"));
        Assert.Contains("COMPRA LOCAL DE BIENES", c.Markup);
        Assert.Contains("3 marcada(s)", c.Markup);
        Assert.Contains("(1)", c.Find("tr[data-fila='BSIMP']").TextContent);   // marcas de la fila: BSIMP solo en CMLBI
    }

    [Fact]
    public void Marcar_AgregaElValor_ConLaClaseElTipoYElValor()
    {
        var c = Dibuja();
        c.Find("input[data-celda='CMLSR|IMTOT']").Change(true);
        c.WaitForState(() => _f.Calls.Any(x => x.M == "POST"));
        var (_, u, b) = _f.Calls.Single(x => x.M == "POST");
        Assert.Contains("matriz-tipos-operacion/calculo", u);
        Assert.Equal("CMPRS", P(b!.Value, "COD_CLASE_OPERACION").GetString());
        Assert.Equal("CMLSR", P(b!.Value, "COD_TIPO_OPERACION").GetString());
        Assert.Equal("IMTOT", P(b!.Value, "COD_TIPO_VALOR_CALCULO").GetString());
        Assert.True(P(b!.Value, "ACTIVO").GetBoolean());
        Assert.True(Marcado(c, "CMLSR|IMTOT"));
        Assert.Contains("4 marcada(s)", c.Markup);
    }

    [Fact]
    public void Desmarcar_SinAfectados_QuitaElValorDeInmediato()
    {
        var c = Dibuja();
        c.Find("input[data-celda='CMLBI|BSIMP']").Change(false);
        c.WaitForState(() => _f.Calls.Any(x => x.M == "POST"));
        Assert.False(P(_f.Calls.Single(x => x.M == "POST").B!.Value, "ACTIVO").GetBoolean());
        Assert.False(Marcado(c, "CMLBI|BSIMP"));
    }

    [Fact]
    public void Desmarcar_ConAfectados_PideConfirmar_Cancelar_DejaLaMarca()
    {
        _f.PideConfirmar = true;
        var c = Dibuja();
        c.Find("input[data-celda='CMLBI|IGV']").Change(false);
        c.WaitForState(() => _f.Calls.Count(x => x.M == "POST") == 1);
        var prov = _dlg;
        prov.WaitForAssertion(() => Assert.Contains("al quitarlo se eliminan esas relaciones", prov.Markup));
        prov.FindAll("button").First(x => x.TextContent.Contains("Cancelar")).Click();
        c.WaitForState(() => Marcado(c, "CMLBI|IGV"));
        Assert.Equal(1, _f.Calls.Count(x => x.M == "POST"));        // no hubo segundo POST
    }

    [Fact]
    public void Desmarcar_ConAfectados_Confirmar_EnviaForzarYQuitaLaMarca()
    {
        _f.PideConfirmar = true;
        var c = Dibuja();
        c.Find("input[data-celda='CMLBI|IGV']").Change(false);
        var prov = _dlg;
        prov.WaitForAssertion(() => Assert.Contains("Quitar", prov.Markup));
        prov.FindAll("button").First(x => x.TextContent.Trim() == "Quitar").Click();
        c.WaitForState(() => _f.Calls.Count(x => x.M == "POST") == 2 && !Marcado(c, "CMLBI|IGV"));
        Assert.True(P(_f.Calls.Last(x => x.M == "POST").B!.Value, "FORZAR").GetBoolean());
    }

    [Fact]
    public void SiLaApiRechaza_LaMarcaSeRevierteYSeAvisa()
    {
        _f.Falla = true;
        var c = Dibuja();
        c.Find("input[data-celda='CMLSR|IMTOT']").Change(true);
        c.WaitForState(() => _f.Calls.Any(x => x.M == "POST"));
        c.WaitForState(() => !Marcado(c, "CMLSR|IMTOT"));
        var snack = _snack;
        snack.WaitForAssertion(() => Assert.Contains("El tipo de operacion no existe en la clase.", snack.Markup));
    }

    [Fact]
    public void Comprobantes_UsaOtrasColumnasYOtroEndpoint()
    {
        var c = Dibuja("comprobantes");
        Assert.Equal(2 * 2, c.FindAll("input[type=checkbox]").Count);     // 2 comprobantes (filas) x 2 tipos (columnas)
        Assert.Empty(c.FindAll("button.mtx-f"));                           // los comprobantes no tienen formula
        Assert.True(Marcado(c, "CMLBI|FCTRA"));
        c.Find("input[data-celda='CMLBI|BLETA']").Change(true);
        c.WaitForState(() => _f.Calls.Any(x => x.M == "POST"));
        var (_, u, b) = _f.Calls.Single(x => x.M == "POST");
        Assert.Contains("matriz-tipos-operacion/comprobantes", u);
        Assert.Equal("BLETA", P(b!.Value, "COD_TIPO_COMPROBANTE_PAGO").GetString());
    }

    [Fact]
    public void FiltrarFilas_OcultaLasQueNoCoinciden()
    {
        var c = Dibuja();
        c.Find("input[type=text]").Input("IGV");
        c.WaitForState(() => c.FindAll("input[type=checkbox]").Count == 2);       // 1 fila x 2 tipos
        Assert.Contains("data-celda=\"CMLBI|IGV\"", c.Markup);
        Assert.DoesNotContain("data-celda=\"CMLBI|BSIMP\"", c.Markup);
    }

    // ───────── formulas ─────────
    [Fact]
    public void Formula_EnLaCasillaMarcada_MuestraCuantosSumaYSuTexto()
    {
        var c = Dibuja();
        var btn = c.Find("button[data-formula='CMLBI|IGV']");
        Assert.Contains("ƒ1", btn.TextContent);
        Assert.Contains("IGV = (BSIMP) × tasa %", btn.GetAttribute("title"));          // IGV es impuesto
        Assert.Contains("con-formula", btn.ClassName);
        Assert.Contains("ƒ", c.Find("button[data-formula='CMLBI|BSIMP']").TextContent);     // BSIMP no suma nada: sin numero
        Assert.Empty(c.FindAll("button[data-formula='CMLSR|IGV']"));                    // sin marca no hay boton
    }

    [Fact]
    public void Formula_AbrirYGuardar_EnviaLosAfectadosYActualizaElBoton()
    {
        var c = Dibuja();
        c.Find("button[data-formula='CMLBI|IGV']").Click();
        _dlg.WaitForAssertion(() => Assert.Contains("Fórmula · IGV", _dlg.Markup));
        Assert.Contains("IGV = (BSIMP) × tasa del impuesto %", _dlg.Find("[data-formula-previa]").TextContent);
        Assert.True(_dlg.Find("[data-candidato='BSIMP'] input").HasAttribute("checked"));
        Assert.False(_dlg.Find("[data-candidato='DSCTS'] input").HasAttribute("checked"));
        Assert.Empty(_dlg.FindAll("[data-candidato='IGV']"));                          // no se ofrece a si mismo
        Assert.Empty(_dlg.FindAll("[data-candidato='IMTOT']"));                        // solo los valores marcados en el tipo
        _dlg.Find("[data-candidato='DSCTS'] input").Change(true);
        Assert.Contains("(BSIMP − DSCTS)", _dlg.Find("[data-formula-previa]").TextContent);      // el signo sale del flag Positivo del valor sumado
        _dlg.FindAll("button").First(x => x.TextContent.Contains("Guardar fórmula")).Click();
        c.WaitForState(() => _f.Calls.Any(x => x.M == "POST" && x.U.Contains("/formulas")));
        var b = _f.Calls.Single(x => x.M == "POST" && x.U.Contains("/formulas")).B!.Value;
        Assert.Equal("CMLBI", P(b, "COD_TIPO_OPERACION").GetString());
        Assert.Equal("IGV", P(b, "COD_TIPO_VALOR_CALCULO").GetString());
        Assert.Equal(new[] { "BSIMP", "DSCTS" }, P(b, "AFECTADOS").EnumerateArray().Select(x => x.GetString()).ToArray());
        Assert.True(P(b, "ACTUALIZAR_PROPIEDADES").GetBoolean());
        Assert.Equal(6, P(b, "NUM_ORDEN_PRESENTACION").GetInt32());
        c.WaitForAssertion(() => Assert.Contains("ƒ2", c.Find("button[data-formula='CMLBI|IGV']").TextContent));
    }

    [Fact]
    public void Formula_AvisaDelAfectadoQueSeCalculaDespues()
    {
        var c = Dibuja();
        c.Find("button[data-formula='CMLBI|IGV']").Click();
        _dlg.WaitForAssertion(() => Assert.Contains("Fórmula · IGV", _dlg.Markup));
        Assert.DoesNotContain("se calcula después", _dlg.Markup);                      // BSIMP (orden 1) va antes que IGV (6)
        _dlg.Find("[data-candidato='DSCTS'] input").Change(true);                      // DSCTS tiene orden 8: despues de IGV
        Assert.Contains("se calcula después", _dlg.Find("[data-candidato='DSCTS']").TextContent);
        Assert.DoesNotContain("se calcula después", _dlg.Find("[data-candidato='BSIMP']").TextContent);
    }

    [Fact]
    public void Formula_SiLaApiRechaza_MuestraElErrorYNoCierra()
    {
        _f.Falla = true;
        var c = Dibuja();
        c.Find("button[data-formula='CMLBI|IGV']").Click();
        _dlg.WaitForAssertion(() => Assert.Contains("Fórmula · IGV", _dlg.Markup));
        _dlg.FindAll("button").First(x => x.TextContent.Contains("Guardar fórmula")).Click();
        _dlg.WaitForAssertion(() => Assert.Contains("Todos los valores que suma deben estar en el mismo tipo de operacion.", _dlg.Markup));
        Assert.Contains("Fórmula · IGV", _dlg.Markup);
    }

    // ───────── transponer ─────────
    [Fact]
    public void Transponer_IntercambiaFilasYColumnas_YVuelveAlOriginal()
    {
        var c = Dibuja();
        Assert.Equal("no", c.Find(".mtx-wrap").GetAttribute("data-transpuesta"));
        Assert.Equal("BSIMP", c.FindAll("tbody tr")[0].GetAttribute("data-fila"));     // filas = valores
        c.Find("button[data-transponer]").Click();
        Assert.Equal("si", c.Find(".mtx-wrap").GetAttribute("data-transpuesta"));
        Assert.Equal(new[] { "CMLBI", "CMLSR" }, c.FindAll("tbody tr").Select(x => x.GetAttribute("data-fila")).ToArray());   // filas = tipos de operacion
        Assert.Equal(4, c.FindAll("thead th.mtx-col-v").Count);                          // columnas = los 4 valores
        Assert.Contains("tipos de operación en filas", c.Markup);
        Assert.Equal(2 * 4, c.FindAll("input[type=checkbox]").Count);
        Assert.True(Marcado(c, "CMLBI|IGV"));                                            // las marcas son las mismas
        Assert.False(Marcado(c, "CMLSR|BSIMP"));
        Assert.Contains("(3)", c.Find("tr[data-fila='CMLBI']").TextContent);             // CMLBI tiene 3 valores marcados
        c.Find("button[data-transponer]").Click();
        Assert.Equal("no", c.Find(".mtx-wrap").GetAttribute("data-transpuesta"));
        Assert.Equal("BSIMP", c.FindAll("tbody tr")[0].GetAttribute("data-fila"));
    }

    [Fact]
    public void Transpuesta_MarcarEnviaLoMismo_YLaFormulaSigueDisponible()
    {
        var c = Dibuja();
        c.Find("button[data-transponer]").Click();
        c.Find("input[data-celda='CMLSR|IMTOT']").Change(true);
        c.WaitForState(() => _f.Calls.Any(x => x.M == "POST"));
        var b = _f.Calls.Single(x => x.M == "POST").B!.Value;
        Assert.Equal("CMLSR", P(b, "COD_TIPO_OPERACION").GetString());
        Assert.Equal("IMTOT", P(b, "COD_TIPO_VALOR_CALCULO").GetString());
        Assert.Contains("ƒ", c.Find("button[data-formula='CMLSR|IMTOT']").TextContent);
        Assert.Contains("ƒ1", c.Find("button[data-formula='CMLBI|IGV']").TextContent);
        c.Find("button[data-formula='CMLBI|IGV']").Click();
        _dlg.WaitForAssertion(() => Assert.Contains("Fórmula · IGV", _dlg.Markup));
    }

    [Fact]
    public void Transponer_TambienEnTiposDeDocumento()
    {
        var c = Dibuja("comprobantes");
        Assert.Equal("FCTRA", c.FindAll("tbody tr")[0].GetAttribute("data-fila"));
        c.Find("button[data-transponer]").Click();
        Assert.Equal(new[] { "CMLBI", "CMLSR" }, c.FindAll("tbody tr").Select(x => x.GetAttribute("data-fila")).ToArray());
        Assert.True(Marcado(c, "CMLBI|FCTRA"));
    }

    [Fact]
    public void LaGrilla_EsAutoAlto_SeEstiraHastaLaBarraDeEstado()
    {
        var c = Dibuja();
        var g = c.Find("[data-resizable-grid='matriz-tipos-operacion']");          // lo mide y estira gridAutoAlto.js
        Assert.False(g.HasAttribute("data-default-height"));                         // con alto fijo el script la dejaria afuera
        Assert.NotNull(g.QuerySelector(".mtx-wrap"));
        Assert.NotNull(g.QuerySelector(".grid-resize-handle"));
    }
}
