using System.Net;
using System.Net.Http.Json;
using Blazored.LocalStorage;
using Bunit;
using KONSolutions.Shared.Models.Comercial;
using KONSolutions.Web.Pages.Comercial;
using KONSolutions.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using Xunit;

/// <summary>Dibuja el documento del Registro de Compras (nuevo) con tipos de operacion distintos: atrapa errores de dibujo
/// (parametros que un componente no acepta) y comprueba cuando se habilita el motivo y la solapa de incidencias.</summary>
public class RegistroCompraDocumentoTests : TestContext
{
    class H : HttpMessageHandler
    {
        public List<string> Urls = new();
        public string _tipoFicha = "DEV";
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage r, CancellationToken ct)
        {
            var u = r.RequestUri!.PathAndQuery; Urls.Add(u);
            object data = u.EndsWith("/registro-compras/5") ? new { NUM_REGISTRO = 5m, COD_CLASE_OPERACION = "CMPRS", COD_TIPO_OPERACION = _tipoFicha, COD_ESTADO_ALIAS_CONCILIACION = "REG", COD_MOTIVO = 1 }
                : u.Contains("/motivos/por-tipo/") ? new[] { new { COD_TIPO_MOTIVO = 32001, COD_MOTIVO = 1, DES_MOTIVO = "MOTIVO UNO" }, new { COD_TIPO_MOTIVO = 32001, COD_MOTIVO = 2, DES_MOTIVO = "MOTIVO DOS" } } : new object[0];
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(new { Success = true, Data = data }) });
        }
    }

    RegistroCompraCombos Combos() => new()
    {
        TIPOS_OPERACION =
        {
            new RegistroCompraTipoOperacion { COD_TIPO_OPERACION = "NOR", DES_TIPO_OPERACION = "Normal", FLG_BIEN = true },
            new RegistroCompraTipoOperacion { COD_TIPO_OPERACION = "DEV", DES_TIPO_OPERACION = "Devolucion", FLG_BIEN = true, FLG_DEVOLUCION = true, FLG_INCIDENCIA_PRODUCTO = true, COD_TIPO_MOTIVO = 32001, COD_MOTIVO_DEFAULT = 2 },
            new RegistroCompraTipoOperacion { COD_TIPO_OPERACION = "CAR", DES_TIPO_OPERACION = "Cargo", FLG_BIEN = true, FLG_CARGO = true, COD_TIPO_MOTIVO = 32001, COD_MOTIVO_DEFAULT = 1 },
        }
    };

    (IRenderedComponent<RegistroCompraDocumento> c, H h) Dibuja(string tipo, bool guardado = false)
    {
        var h = new H { _tipoFicha = tipo };
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton(new HttpClient(h) { BaseAddress = new Uri("http://x/") });
        Services.AddBlazoredLocalStorage();
        Services.AddScoped<ErrorLogService>();
        Services.AddScoped<ConfigService>();
        ComponentFactories.AddStub<KONSolutions.Web.Pages.Shared.AccionesBotones>();   // depende de seguridad/menu: no es lo que se prueba
        ComponentFactories.AddStub<KONSolutions.Web.Pages.Shared.EncabezadoOrdenable>();
        ComponentFactories.AddStub<KONSolutions.Web.Pages.Shared.TituloPagina>();
        RenderComponent<MudBlazor.MudPopoverProvider>(); RenderComponent<MudBlazor.MudSnackbarProvider>(); RenderComponent<MudBlazor.MudDialogProvider>();
        var c = RenderComponent<RegistroCompraDocumento>(p => { p.Add(x => x.Combos, Combos()).Add(x => x.CodTipoOperacionSugerido, tipo).Add(x => x.CodAplicacion, "A").Add(x => x.CodOpcionAplicacion, "B"); if (guardado) p.Add(x => x.Item, new RegistroCompra { NUM_REGISTRO = 5m }).Add(x => x.Modo, KONSolutions.Shared.Models.CrudModo.Update); });
        c.WaitForState(() => c.Markup.Contains("Incoterm"), TimeSpan.FromSeconds(5));
        return (c, h);
    }

    [Fact]
    public void TipoNormal_DibujaYElMotivoQuedaDeshabilitado()
    {
        var (c, h) = Dibuja("NOR");
        Assert.Contains("Motivo de la devoluci", c.Markup);
        var motivo = c.FindComponents<KONSolutions.Web.Pages.Shared.AriaCombo<KONSolutions.Shared.Common.ComboboxItem>>().First(x => (x.Instance.Label ?? "").StartsWith("Motivo"));
        Assert.True(motivo.Instance.Disabled);
        Assert.DoesNotContain(h.Urls, u => u.Contains("motivos/por-tipo"));
    }

    [Fact]
    public void Devolucion_HabilitaElMotivoYProponeElPorDefecto()
    {
        var (c, h) = Dibuja("DEV");
        c.WaitForState(() => h.Urls.Any(u => u.Contains("motivos/por-tipo/32001")), TimeSpan.FromSeconds(5));
        var motivo = c.FindComponents<KONSolutions.Web.Pages.Shared.AriaCombo<KONSolutions.Shared.Common.ComboboxItem>>().First(x => (x.Instance.Label ?? "").StartsWith("Motivo"));
        Assert.False(motivo.Instance.Disabled);
        Assert.Equal("2", motivo.Instance.Value);                       // el por defecto del tipo
    }

    [Fact]
    public void Cargo_HabilitaElMotivoDelCargo()
    {
        var (c, _) = Dibuja("CAR");
        var motivo = c.FindComponents<KONSolutions.Web.Pages.Shared.AriaCombo<KONSolutions.Shared.Common.ComboboxItem>>().First(x => (x.Instance.Label ?? "").StartsWith("Motivo"));
        Assert.False(motivo.Instance.Disabled);
        Assert.Equal("Motivo del cargo", motivo.Instance.Label);
    }

    [Fact]
    public void Devolucion_Guardada_MuestraFacturaAfectadaYNoOrdenes()
    {
        var (c, _) = Dibuja("DEV", guardado: true);
        c.WaitForState(() => c.Markup.Contains("Factura afectada"), TimeSpan.FromSeconds(5));
        Assert.DoesNotContain("rdenes (", c.Markup);                    // no hay solapa de ordenes
        var motivo = c.FindComponents<KONSolutions.Web.Pages.Shared.AriaCombo<KONSolutions.Shared.Common.ComboboxItem>>().First(x => (x.Instance.Label ?? "").StartsWith("Motivo"));
        Assert.Equal("1", motivo.Instance.Value);                       // el motivo guardado no se pisa con el por defecto
    }

    [Fact]
    public void Normal_Guardada_MuestraOrdenesYNoFacturaAfectada()
    {
        var (c, _) = Dibuja("NOR", guardado: true);
        c.WaitForState(() => c.Markup.Contains("rdenes ("), TimeSpan.FromSeconds(5));
        Assert.DoesNotContain("Factura afectada", c.Markup);
    }

    [Theory]
    [InlineData("NOR", false)]
    [InlineData("DEV", true)]
    [InlineData("CAR", true)]
    public void FormaYCondicionDePago_SoloSeDeshabilitanEnNotas(string tipo, bool deshabilitadas)
    {
        var (c, _) = Dibuja(tipo);
        var combos = c.FindComponents<KONSolutions.Web.Pages.Shared.AriaCombo<KONSolutions.Shared.Common.ComboboxItem>>();
        Assert.Equal(deshabilitadas, combos.First(x => x.Instance.Label == "Forma de pago").Instance.Disabled);
        Assert.Equal(deshabilitadas, combos.First(x => x.Instance.Label == "Condici\u00f3n de pago").Instance.Disabled);
    }

    [Fact]
    public void AlCrear_LaGrillaDeLineasSeMuestraAunqueEsteVacia()
    {
        var (c, _) = Dibuja("NOR");
        Assert.Contains("detalle-bloqueado", c.Markup);                 // el aviso de grabar la cabecera sigue
        Assert.Contains("mud-table", c.Markup);                         // pero la grilla ya esta dibujada
        Assert.Contains("data-vacio=\"lineas\"", c.Markup);
    }

    [Fact]
    public void Motivo_EstaEnLaMismaFilaQueLosCentrosDeCosto()
    {
        var (c, _) = Dibuja("DEV");
        var items = c.FindAll(".mud-grid-item");
        int Idx(string label) => items.ToList().FindIndex(x => x.TextContent.Contains(label) && !x.TextContent.Contains("Proyecto"));
        Assert.True(Idx("Motivo de la devoluci") - Idx("Centro de costo destino") <= 2);   // seguidos: los tres en una fila (md=4 + 4 + 4)
        Assert.Equal(3, c.FindAll("[data-panel='cabecera'] .mud-grid-item.mud-grid-item-md-4").Count(x => x.TextContent.Contains("Centro de costo") || x.TextContent.Contains("Motivo")));
    }

    [Fact]
    public void OcultarCabecera_LaEscondeSinPerderlaYAmpliaElDetalle()
    {
        var (c, _) = Dibuja("NOR", guardado: true);
        c.WaitForState(() => c.Markup.Contains("rdenes ("), TimeSpan.FromSeconds(5));
        Assert.DoesNotContain("display:none", c.Find("[data-panel='cabecera']").GetAttribute("style") ?? "");
        Assert.Empty(c.FindAll("[data-panel='barra-cabecera']"));                     // abierta: no hay barra arriba, el boton va debajo de Incoterm
        Assert.True(c.Markup.IndexOf("Incoterm", StringComparison.Ordinal) < c.Markup.IndexOf("Ocultar cabecera", StringComparison.Ordinal));
        c.Find("[data-accion='ocultar-cabecera']").Click();
        Assert.Contains("display:none", c.Find("[data-panel='cabecera']").GetAttribute("style"));
        Assert.Contains("Mostrar cabecera", c.Markup);
        Assert.Contains("resumen-cabecera", c.Markup);
        Assert.Contains("100vh", c.Markup);                       // las grillas crecen
        c.Find("[data-accion='mostrar-cabecera']").Click();
        Assert.DoesNotContain("display:none", c.Find("[data-panel='cabecera']").GetAttribute("style") ?? "");
    }

    [Fact]
    public void Distribucion_MonedaPagoYDescuentoVanEnElPanelDerecho_DespuesDelPanelIzquierdo()
    {
        var (c, _) = Dibuja("NOR");
        var m = c.Markup;
        int Pos(string t) => m.IndexOf(t, StringComparison.Ordinal);
        // izquierdo: ... fechas, centros de costo, proyecto, lote, incoterm. Derecho: estado, moneda, tipo de cambio, forma y condicion de pago, descuento, PDF, observacion
        Assert.True(Pos("Incoterm") < Pos("Moneda"));
        Assert.True(Pos("Moneda") < Pos("Tipo de cambio") && Pos("Tipo de cambio") < Pos("Forma de pago") && Pos("Forma de pago") < Pos("Condici\u00f3n de pago"));
        Assert.True(Pos("Condici\u00f3n de pago") < Pos("Tipo de descuento") && Pos("Tipo de descuento") < Pos("Tasa de descuento"));
        Assert.True(Pos("Fecha de recepci\u00f3n") < Pos("Centro de costo solicitante"));       // en la izquierda, las fechas antes que los centros de costo
        Assert.True(Pos("Tasa de descuento") < Pos("Archivo PDF"));
    }
}
