using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Blazored.LocalStorage;
using Bunit;
using KONSolutions.Shared.Models.Comercial;
using KONSolutions.Web.Pages.Comercial;
using KONSolutions.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using Xunit;

public class Fake : HttpMessageHandler
{
    public List<(string M, string U, string? B)> Calls = new();
    public bool Filtra;
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage r, CancellationToken ct)
    {
        var body = r.Content is null ? null : await r.Content.ReadAsStringAsync(ct);
        Calls.Add((r.Method.Method, r.RequestUri!.PathAndQuery, body));
        var u = r.RequestUri!.PathAndQuery;
        object data = u.Contains("/incidencias/origenes") ? new[] { new { NUM_REGISTRO = 9000001m, NUM_COMPROBANTE_PAGO = "F001-00000001", COD_MONEDA = "PEN", VAL_TOTAL = 1500m } }
            : u.Contains("/incidencias/disponibles") ? new[] { new { NUM_SECUENCIA_ASOCIADO = 1, CAN_GUIA = Filtra ? 4m : (decimal?)null, FLG_FILTRADO_POR_GUIA = Filtra, COD_PRODUCTO = (string?)"P-0001", DES_PRODUCTO = "PRODUCTO DEL MAESTRO", DES_PRODUCTO_COMPROBANTE = (string?)null, COD_PRODUCTO_ENTIDAD = "PROV-A1", PRE_UNITARIO = 100m, TAS_DESCUENTO = 5m, COD_UNIDAD_MEDIDA = "UND", CAN_FACTURADA = 10m, VAL_FACTURADO = 1000m, CAN_APLICADA_OTRAS = 0m, VAL_APLICADO_OTRAS = 0m, CAN_SALDO = 10m, VAL_SALDO = 1000m },
                                                                              new { NUM_SECUENCIA_ASOCIADO = 2, CAN_GUIA = Filtra ? 4m : (decimal?)null, FLG_FILTRADO_POR_GUIA = Filtra, COD_PRODUCTO = (string?)"P-0002", DES_PRODUCTO = "ITEM B", DES_PRODUCTO_COMPROBANTE = "ITEM B", COD_PRODUCTO_ENTIDAD = (string?)null, PRE_UNITARIO = 100m, TAS_DESCUENTO = 0m, COD_UNIDAD_MEDIDA = "UND", CAN_FACTURADA = 5m, VAL_FACTURADO = 500m, CAN_APLICADA_OTRAS = 0m, VAL_APLICADO_OTRAS = 0m, CAN_SALDO = 5m, VAL_SALDO = 500m } }
            : u.Contains("/incidencias/guias/disponibles") ? new[] { new { NUM_MOVIMIENTO_ALMACEN = 9000001m, NUM_GUIA = "T001-00000001", NUM_LINEAS = 2, FLG_ASOCIADA = false } }
            : u.EndsWith("/incidencias/guias") && r.Method == HttpMethod.Get ? new[] { new { NUM_MOVIMIENTO_ALMACEN = 9000005m, NUM_GUIA = "T001-00000005", NUM_LINEAS_VINCULADAS = 1 } }
            : u.EndsWith("/incidencias") && r.Method == HttpMethod.Get ? new[] { new { NUM_SECUENCIA_NOTA = 1, NUM_REGISTRO_ASOCIADO = 9000099m, NUM_COMPROBANTE_PAGO = "F001-00000099", NUM_SECUENCIA_ASOCIADO = 7, DES_PRODUCTO_COMPROBANTE = "OTRA FACTURA", CAN_APLICADA = 1m, VAL_APLICADO = 10m } }
            : new object[0];
        return new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(new { Success = true, Data = data }) };
    }
}

public class Tests : TestContext
{
    (IRenderedComponent<RegistroCompraIncidenciasPanel> c, Fake f) Dibuja(bool devolucion)
    {
        var fake = new Fake();
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton(new HttpClient(fake) { BaseAddress = new Uri("http://x/") });
        Services.AddBlazoredLocalStorage();
        Services.AddScoped<ErrorLogService>();
        RenderComponent<MudBlazor.MudPopoverProvider>(); RenderComponent<MudBlazor.MudSnackbarProvider>(); RenderComponent<MudBlazor.MudDialogProvider>();
        var c = RenderComponent<RegistroCompraIncidenciasPanel>(p => p.Add(x => x.NumRegistro, 9000002m).Add(x => x.EsDevolucion, devolucion).Add(x => x.PermiteGuia, devolucion));
        c.WaitForState(() => c.Markup.Contains("OTRA FACTURA"));
        return (c, fake);
    }

    static JsonElement Get(JsonElement e, string n) => e.EnumerateObject().First(p => string.Equals(p.Name, n, StringComparison.OrdinalIgnoreCase)).Value;

    async Task ElegirFactura(IRenderedComponent<RegistroCompraIncidenciasPanel> c)
    {
        var sel = c.FindComponents<MudBlazor.MudSelect<decimal?>>().First(x => x.Instance.Label == "Factura");
        await c.InvokeAsync(() => sel.Instance.ValueChanged.InvokeAsync(9000001m));
        c.WaitForState(() => c.Markup.Contains("PRODUCTO DEL MAESTRO"));
    }

    [Fact]
    public async Task LaGrillaTraeLoMismoQueLasLineas_ProductoDelMaestroSiLaLineaNoTieneDescripcion()
    {
        var (c, _) = Dibuja(true);
        await ElegirFactura(c);
        Assert.Contains("P-0001 \u00b7 PRODUCTO DEL MAESTRO", c.Markup);   // codigo + descripcion
        Assert.Contains("PRODUCTO DEL MAESTRO", c.Markup);                // la linea no tenia descripcion: sale la del producto
        Assert.Contains("ITEM B", c.Markup);
        foreach (var col in new[] { "N\u00b0", "Producto", "C\u00f3d. proveedor", "Cantidad", "Unidad", "Precio", "Desc. %", "Total", "Importe de diferencia" })
            Assert.Contains(col, c.Markup);
        Assert.Contains("PROV-A1", c.Markup);
        Assert.Contains("100.00", c.Markup.Replace(",", "."));            // precio
    }

    [Fact]
    public async Task Devolucion_ElImporteSaleProporcionalYAdicionaAlComprobante()
    {
        var (c, fake) = Dibuja(true);
        await ElegirFactura(c);
        var cants = c.FindComponents<MudBlazor.MudNumericField<decimal?>>().Where(x => x.Markup.Contains("cantidad-devuelta")).ToList();
        await c.InvokeAsync(() => cants[0].Instance.ValueChanged.InvokeAsync(4m));   // 4 de 10, total 1000 => 400
        var imp = c.FindComponents<MudBlazor.MudNumericField<decimal?>>().Where(x => x.Markup.Contains("importe-diferencia")).First();
        Assert.Equal(400m, imp.Instance.Value);
        await c.InvokeAsync(() => c.Find("[data-accion='adicionar']").Click());
        c.WaitForState(() => fake.Calls.Any(x => x.M == "POST" && x.U.Contains("/incidencias/adicionar")));
        var post = fake.Calls.First(x => x.M == "POST" && x.U.Contains("/incidencias/adicionar"));
        using var doc = JsonDocument.Parse(post.B!);
        Assert.Equal(9000001m, Get(doc.RootElement, "NUM_REGISTRO_ORIGEN").GetDecimal());
        var items = Get(doc.RootElement, "Items").EnumerateArray().ToList();
        var it = Assert.Single(items);
        Assert.Equal(1, Get(it, "NUM_SECUENCIA_ASOCIADO").GetInt32());
        Assert.Equal(4m, Get(it, "CAN_APLICADA").GetDecimal());
        Assert.Equal(400m, Get(it, "VAL_APLICADO").GetDecimal());
    }

    [Fact]
    public async Task DiferenciaDePrecio_SoloImporte_SinColumnaDeCantidadDevuelta()
    {
        var (c, fake) = Dibuja(false);
        await ElegirFactura(c);
        Assert.DoesNotContain("Cant. devuelta", c.Markup);
        Assert.DoesNotContain("Gu\u00eda de devoluci\u00f3n", c.Markup);     // la guia es solo de devolucion
        var imp = c.FindComponents<MudBlazor.MudNumericField<decimal?>>().Where(x => x.Markup.Contains("importe-diferencia")).ToList();
        await c.InvokeAsync(() => imp[1].Instance.ValueChanged.InvokeAsync(50m));
        await c.InvokeAsync(() => c.Find("[data-accion='adicionar']").Click());
        c.WaitForState(() => fake.Calls.Any(x => x.M == "POST" && x.U.Contains("/incidencias/adicionar")));
        using var doc = JsonDocument.Parse(fake.Calls.First(x => x.U.Contains("/incidencias/adicionar")).B!);
        var it = Assert.Single(Get(doc.RootElement, "Items").EnumerateArray().ToList());
        Assert.Equal(2, Get(it, "NUM_SECUENCIA_ASOCIADO").GetInt32());
        Assert.Equal(JsonValueKind.Null, Get(it, "CAN_APLICADA").ValueKind);
        Assert.Equal(50m, Get(it, "VAL_APLICADO").GetDecimal());
    }

    [Fact]
    public async Task Devolucion_PorImporte_SinCantidad_SeAdiciona()
    {
        var (c, fake) = Dibuja(true);
        await ElegirFactura(c);
        var imp = c.FindComponents<MudBlazor.MudNumericField<decimal?>>().Where(x => x.Markup.Contains("importe-diferencia")).First();
        await c.InvokeAsync(() => imp.Instance.ValueChanged.InvokeAsync(120m));
        await c.InvokeAsync(() => c.Find("[data-accion='adicionar']").Click());
        c.WaitForState(() => fake.Calls.Any(x => x.M == "POST" && x.U.Contains("/incidencias/adicionar")));
        using var doc = JsonDocument.Parse(fake.Calls.First(x => x.U.Contains("/incidencias/adicionar")).B!);
        var it = Assert.Single(Get(doc.RootElement, "Items").EnumerateArray().ToList());
        Assert.Equal(JsonValueKind.Null, Get(it, "CAN_APLICADA").ValueKind);
        Assert.Equal(120m, Get(it, "VAL_APLICADO").GetDecimal());
    }

    [Fact]
    public void Cargo_ConGuia_PeroSinColumnaDeCantidadDevuelta()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton(new HttpClient(new Fake()) { BaseAddress = new Uri("http://x/") });
        Services.AddBlazoredLocalStorage(); Services.AddScoped<ErrorLogService>();
        RenderComponent<MudBlazor.MudPopoverProvider>(); RenderComponent<MudBlazor.MudSnackbarProvider>(); RenderComponent<MudBlazor.MudDialogProvider>();
        var c = RenderComponent<RegistroCompraIncidenciasPanel>(p => p.Add(x => x.NumRegistro, 9000002m).Add(x => x.EsDevolucion, false).Add(x => x.PermiteGuia, true));
        c.WaitForState(() => c.Markup.Contains("T001-00000005"));
        Assert.Contains("Gu\u00eda del proveedor", c.Markup);            // cargo: la guia es la del proveedor
        Assert.DoesNotContain("Cant. devuelta", c.Markup);
    }

    [Fact]
    public async Task SinNadaElegido_NoLlamaALaApi()
    {
        var (c, fake) = Dibuja(true);
        await ElegirFactura(c);
        await c.InvokeAsync(() => c.Find("[data-accion='adicionar']").Click());
        Assert.DoesNotContain(fake.Calls, x => x.U.Contains("/incidencias/adicionar"));
    }

    [Fact]
    public async Task LaGuiaVaPrimeroQueLaFactura_YSiHayGuiaLosItemsSalenFiltrados()
    {
        var (c, fake) = Dibuja(true);
        c.WaitForState(() => c.Markup.Contains("T001-00000005"));
        // orden: primero la guia, luego la factura
        Assert.True(c.Markup.IndexOf("1. Gu") >= 0 && c.Markup.IndexOf("2. Factura que afecta") > c.Markup.IndexOf("1. Gu"));
        fake.Filtra = true;
        await ElegirFactura(c);
        // la lista pide los items a la API (que filtra por guia) y avisa que esta filtrada
        Assert.Contains(fake.Calls, x => x.U.Contains("/incidencias/disponibles"));
        Assert.Contains("filtrado-por-guia", c.Markup);
        Assert.Contains("cantidad-guia", c.Markup);
    }

    [Fact]
    public async Task ConGuia_LaCantidadSeIncorporaSolaYNoSeEdita()
    {
        var (c, fake) = Dibuja(true);
        c.WaitForState(() => c.Markup.Contains("T001-00000005"));
        fake.Filtra = true;
        await ElegirFactura(c);
        var cant = c.FindComponents<MudBlazor.MudNumericField<decimal?>>().First(x => x.Markup.Contains("cantidad-devuelta"));
        Assert.Equal(4m, cant.Instance.Value);          // la de la guia, sin digitar
        Assert.True(cant.Instance.ReadOnly);
        await c.InvokeAsync(() => c.Find("[data-accion='adicionar']").Click());
        c.WaitForState(() => fake.Calls.Any(x => x.U.Contains("/incidencias/adicionar")));
        Assert.Contains("\"can_aplicada\":4", fake.Calls.Last(x => x.U.Contains("/incidencias/adicionar")).B!.ToLowerInvariant());
    }

    [Fact]
    public async Task AlQuitarLaGuia_SeVuelvenAPedirLasFacturas()
    {
        var (c, fake) = Dibuja(true);
        c.WaitForState(() => c.Markup.Contains("T001-00000005"));
        var antes = fake.Calls.Count(x => x.U.Contains("/incidencias/origenes"));
        await c.InvokeAsync(() => c.Find("[data-accion='guia-quitar']").Click());
        c.WaitForState(() => fake.Calls.Count(x => x.U.Contains("/incidencias/origenes")) > antes);
    }

    [Fact]
    public void SinGuiasPermitidas_SoloFactura_SinNumeracion()
    {
        var (c, _) = Dibuja(false);
        Assert.Contains("Factura que afecta esta nota", c.Markup);
        Assert.DoesNotContain("2. Factura", c.Markup);
    }

    [Fact]
    public async Task Devolucion_MuestraLasGuiasAdjuntasYPermiteAdjuntarYQuitar()
    {
        var (c, fake) = Dibuja(true);
        c.WaitForState(() => c.Markup.Contains("T001-00000005"));          // adjunta
        Assert.Contains("Gu\u00eda de devoluci\u00f3n", c.Markup);
        var sel = c.FindComponents<MudBlazor.MudSelect<decimal?>>().First(x => x.Markup.Contains("guia-disponible") || x.Instance.Label == "Gu\u00eda de devoluci\u00f3n");
        await c.InvokeAsync(() => sel.Instance.ValueChanged.InvokeAsync(9000001m));
        await c.InvokeAsync(() => c.Find("[data-accion='guia-adjuntar']").Click());
        c.WaitForState(() => fake.Calls.Any(x => x.M == "POST" && x.U.EndsWith("/incidencias/guias/9000001")));
        await c.InvokeAsync(() => c.Find("[data-accion='guia-quitar']").Click());
        c.WaitForState(() => fake.Calls.Any(x => x.M == "DELETE" && x.U.EndsWith("/incidencias/guias/9000005")));
    }
}
