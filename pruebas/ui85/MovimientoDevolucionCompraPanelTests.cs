using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Blazored.LocalStorage;
using Bunit;
using KONSolutions.Web.Pages.Logistica;
using KONSolutions.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using Xunit;

public class FakeDev : HttpMessageHandler
{
    public List<(string M, string U, string? B)> Calls = new();
    public bool Asociada;          // la guia 11 ya esta asociada con el item 1 en 400
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage r, CancellationToken ct)
    {
        var body = r.Content is null ? null : await r.Content.ReadAsStringAsync(ct);
        var u = r.RequestUri!.PathAndQuery;
        Calls.Add((r.Method.Method, u, body));
        object data;
        if (u.Contains("/devolucion-compra/guias-disponibles"))
            data = new[] {
                new { NUM_MOVIMIENTO_ALMACEN_ORIGEN = 11, DES_GUIA = "GRERM T001-0001", FEC_MOVIMENTO_ALMACEN = "2026-10-05T00:00:00", CAN_LINEAS = 2, CAN_RECIBIDA = 130m, CAN_SALDO = 130m, FLG_ASOCIADA = Asociada },
                new { NUM_MOVIMIENTO_ALMACEN_ORIGEN = 12, DES_GUIA = "GRERM T001-0002", FEC_MOVIMENTO_ALMACEN = "2026-10-06T00:00:00", CAN_LINEAS = 1, CAN_RECIBIDA = 50m, CAN_SALDO = 50m, FLG_ASOCIADA = false } };
        else if (u.Contains("/devolucion-compra/lineas"))
        {
            var l = new List<object> {
                new { NUM_MOVIMIENTO_ALMACEN_ORIGEN = 11, NUM_SECUENCIA_ORIGEN = 1, DES_GUIA = "GRERM T001-0001", COD_PRODUCTO = "P-0001", DES_PRODUCTO = "TELA JERSEY", COD_UNIDAD_MEDIDA = "KGM", NUM_LOTE_CONTROL = "L1", CAN_RECIBIDA = 100m, CAN_YA_DEVUELTA = 0m, CAN_SALDO = 100m, NUM_SECUENCIA_DEVOLUCION = Asociada ? 1 : (int?)null, CAN_DEVOLVER = Asociada ? 400m : 0m, COD_UBICACION_DISTRIBUCION = "01076", CAN_STOCK_DISTRIBUCION = 60m, CAN_MAXIMA = Asociada ? 100m : 60m },
                new { NUM_MOVIMIENTO_ALMACEN_ORIGEN = 11, NUM_SECUENCIA_ORIGEN = 2, DES_GUIA = "GRERM T001-0001", COD_PRODUCTO = "P-0002", DES_PRODUCTO = "HILO", COD_UNIDAD_MEDIDA = "NIU", NUM_LOTE_CONTROL = (string?)null, CAN_RECIBIDA = 30m, CAN_YA_DEVUELTA = 10m, CAN_SALDO = 20m, NUM_SECUENCIA_DEVOLUCION = (int?)null, CAN_DEVOLVER = 0m, COD_UBICACION_DISTRIBUCION = "01076", CAN_STOCK_DISTRIBUCION = 500m, CAN_MAXIMA = 20m } };
            if (u.Contains("origenes=11%2C12") || u.Contains("origenes=12"))
                l.Add(new { NUM_MOVIMIENTO_ALMACEN_ORIGEN = 12, NUM_SECUENCIA_ORIGEN = 1, DES_GUIA = "GRERM T001-0002", COD_PRODUCTO = "P-0003", DES_PRODUCTO = "BOTON", COD_UNIDAD_MEDIDA = "NIU", NUM_LOTE_CONTROL = (string?)null, CAN_RECIBIDA = 50m, CAN_YA_DEVUELTA = 0m, CAN_SALDO = 50m, NUM_SECUENCIA_DEVOLUCION = (int?)null, CAN_DEVOLVER = 0m, COD_UBICACION_DISTRIBUCION = "01076", CAN_STOCK_DISTRIBUCION = 0m, CAN_MAXIMA = 0m });
            data = l;
        }
        else if (u.EndsWith("/adicionar")) data = new { CAN_LINEAS_AGREGADAS = 2, DES_MENSAJE = "Se actualizaron 2 linea(s) de devolucion." };
        else data = 1;
        return new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(new { Success = true, Data = data }) };
    }
}

public class DevolucionPanelTests : TestContext
{
    int _cambios;
    (IRenderedComponent<MovimientoDevolucionCompraPanel> c, FakeDev f) Dibuja(bool asociada = false, string? entidad = "5", decimal? num = 7m, string? soloLectura = null)
    {
        var fake = new FakeDev { Asociada = asociada };
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton(new HttpClient(fake) { BaseAddress = new Uri("http://x/") });
        Services.AddBlazoredLocalStorage();
        Services.AddScoped<ErrorLogService>();
        RenderComponent<MudBlazor.MudPopoverProvider>(); RenderComponent<MudBlazor.MudSnackbarProvider>(); RenderComponent<MudBlazor.MudDialogProvider>();
        var c = RenderComponent<MovimientoDevolucionCompraPanel>(p => p
            .Add(x => x.NumMovimiento, num).Add(x => x.CodEntidad, entidad).Add(x => x.DesEntidad, "PROVEEDOR SAC")
            .Add(x => x.MotivoSoloLectura, soloLectura)
            .Add(x => x.OnDetalleCambiado, Microsoft.AspNetCore.Components.EventCallback.Factory.Create(this, () => _cambios++)));
        if (entidad is not null && num is not null) c.WaitForState(() => c.Markup.Contains("GRERM T001-0001"));
        return (c, fake);
    }

    static JsonElement Get(JsonElement e, string n) => e.EnumerateObject().First(p => string.Equals(p.Name, n, StringComparison.OrdinalIgnoreCase)).Value;

    async Task MarcarGuia(IRenderedComponent<MovimientoDevolucionCompraPanel> c, int fila)
    {
        var cb = c.FindComponents<MudBlazor.MudCheckBox<bool>>().Where(x => x.Markup.Contains("guia-marcada")).ElementAt(fila);
        await c.InvokeAsync(() => cb.Instance.ValueChanged.InvokeAsync(true));
        c.WaitForState(() => c.Markup.Contains("TELA JERSEY"));
    }

    [Fact]
    public void SinCabecera_AvisaYNoConsulta()
    {
        var (c, f) = Dibuja(num: null);
        Assert.Contains("sin-cabecera", c.Markup);
        Assert.Empty(f.Calls);
    }

    [Fact]
    public void SinProveedor_AvisaYNoConsulta()
    {
        var (c, f) = Dibuja(entidad: null);
        Assert.Contains("sin-proveedor", c.Markup);
        Assert.Empty(f.Calls);
    }

    [Fact]
    public void ListaLasGuiasDelProveedor_ConSuSaldo()
    {
        var (c, f) = Dibuja();
        Assert.Contains("GRERM T001-0002", c.Markup);
        Assert.Contains("PROVEEDOR SAC", c.Markup);
        Assert.Contains("/7/devolucion-compra/guias-disponibles", f.Calls[0].U);
        Assert.Contains("vacio=\"items\"", c.Markup);   // aun no hay guia marcada
    }

    [Fact]
    public async Task MarcarUnaGuia_TraeSusItems_ConCodigoDescripcionYSaldo()
    {
        var (c, f) = Dibuja();
        await MarcarGuia(c, 0);
        Assert.Contains("P-0001 · TELA JERSEY", c.Markup);
        Assert.Contains("P-0002 · HILO", c.Markup);
        Assert.Contains("origenes=11", f.Calls.Last().U);
        Assert.Contains("0 items", c.Find("[data-resumen='marcados']").TextContent);
    }

    [Fact]
    public async Task SeleccionarTodo_PoneElSaldoEnCadaItem_YAdicionarEnviaTodos()
    {
        var (c, f) = Dibuja();
        await MarcarGuia(c, 0);
        await c.InvokeAsync(() => c.Find("[data-accion='seleccionar-todo']").Click());
        var cants = c.FindComponents<MudBlazor.MudNumericField<decimal?>>().Where(x => x.Markup.Contains("cantidad-devolver")).ToList();
        Assert.Equal(60m, cants[0].Instance.Value);   // saldo 100, pero solo hay 60 en la ubicacion de distribucion
        Assert.Equal(20m, cants[1].Instance.Value);   // el saldo ya descuenta lo devuelto en otras devoluciones
        await c.InvokeAsync(() => c.Find("[data-accion='adicionar']").Click());
        c.WaitForState(() => f.Calls.Any(x => x.U.EndsWith("/adicionar")));
        var post = f.Calls.Last(x => x.U.EndsWith("/adicionar"));
        Assert.Equal("POST", post.M);
        var items = Get(JsonDocument.Parse(post.B!).RootElement, "Items").EnumerateArray().ToList();
        Assert.Equal(2, items.Count);
        Assert.Equal(60m, Get(items[0], "CAN_DEVOLVER").GetDecimal());
        Assert.Equal(20m, Get(items[1], "CAN_DEVOLVER").GetDecimal());
        Assert.Equal(1, _cambios);   // avisa al documento para que recargue el detalle
    }

    [Fact]
    public async Task EditarCantidad_MarcaElItem_NuncaPasaDelSaldo()
    {
        var (c, f) = Dibuja();
        await MarcarGuia(c, 0);
        var cants = c.FindComponents<MudBlazor.MudNumericField<decimal?>>().Where(x => x.Markup.Contains("cantidad-devolver")).ToList();
        await c.InvokeAsync(() => cants[1].Instance.ValueChanged.InvokeAsync(999m));   // saldo 20
        cants = c.FindComponents<MudBlazor.MudNumericField<decimal?>>().Where(x => x.Markup.Contains("cantidad-devolver")).ToList();
        Assert.Equal(20m, cants[1].Instance.Value);
        await c.InvokeAsync(() => cants[0].Instance.ValueChanged.InvokeAsync(30m));
        await c.InvokeAsync(() => c.Find("[data-accion='adicionar']").Click());
        c.WaitForState(() => f.Calls.Any(x => x.U.EndsWith("/adicionar")));
        var items = Get(JsonDocument.Parse(f.Calls.Last(x => x.U.EndsWith("/adicionar")).B!).RootElement, "Items").EnumerateArray().ToList();
        Assert.Equal(30m, Get(items.First(i => Get(i, "NUM_SECUENCIA_ORIGEN").GetInt32() == 1), "CAN_DEVOLVER").GetDecimal());
        Assert.Equal(20m, Get(items.First(i => Get(i, "NUM_SECUENCIA_ORIGEN").GetInt32() == 2), "CAN_DEVOLVER").GetDecimal());
    }

    [Fact]
    public async Task ItemSinStockEnDistribucion_NoSePuedeDevolver_YNoPasaDelStockDeLaZona()
    {
        var (c, f) = Dibuja();
        await MarcarGuia(c, 0);
        var cb = c.FindComponents<MudBlazor.MudCheckBox<bool>>().Where(x => x.Markup.Contains("guia-marcada")).ElementAt(1);
        await c.InvokeAsync(() => cb.Instance.ValueChanged.InvokeAsync(true));
        c.WaitForState(() => c.Markup.Contains("BOTON"));
        // el boton tiene saldo 50 pero 0 en la zona de distribucion: marca y cantidad bloqueadas
        var fila = c.Find("[data-item='12-1']");
        Assert.Contains("mud-disabled", fila.QuerySelector("[data-campo='cantidad-devolver']")!.OuterHtml + fila.InnerHtml);
        Assert.Equal("0", fila.QuerySelector("[data-celda='stock-distribucion']")!.TextContent.Trim());
        // la tela tiene saldo 100 y solo 60 en la zona: no deja pasar de 60
        var cants = c.FindComponents<MudBlazor.MudNumericField<decimal?>>().Where(x => x.Markup.Contains("cantidad-devolver")).ToList();
        await c.InvokeAsync(() => cants[0].Instance.ValueChanged.InvokeAsync(100m));
        cants = c.FindComponents<MudBlazor.MudNumericField<decimal?>>().Where(x => x.Markup.Contains("cantidad-devolver")).ToList();
        Assert.Equal(60m, cants[0].Instance.Value);
    }

    [Fact]
    public async Task SinNadaMarcado_NoLlamaALaApi()
    {
        var (c, f) = Dibuja();
        await MarcarGuia(c, 0);
        await c.InvokeAsync(() => c.Find("[data-accion='adicionar']").Click());
        Assert.DoesNotContain(f.Calls, x => x.U.EndsWith("/adicionar"));
    }

    [Fact]
    public async Task DosGuias_SeVenLosItemsDeAmbas()
    {
        var (c, f) = Dibuja();
        await MarcarGuia(c, 0);
        var cb = c.FindComponents<MudBlazor.MudCheckBox<bool>>().Where(x => x.Markup.Contains("guia-marcada")).ElementAt(1);
        await c.InvokeAsync(() => cb.Instance.ValueChanged.InvokeAsync(true));
        c.WaitForState(() => c.Markup.Contains("BOTON"));
        Assert.Contains("P-0001 · TELA JERSEY", c.Markup);
        Assert.Contains("P-0003 · BOTON", c.Markup);
        Assert.Contains("origenes=11%2C12", f.Calls.Last().U);
    }

    [Fact]
    public async Task GuiaYaAsociada_ArrancaMarcada_ConSuCantidad_YDesmarcarLaQuitaConCero()
    {
        var (c, f) = Dibuja(asociada: true);
        c.WaitForState(() => c.Markup.Contains("TELA JERSEY"));   // se cargaron solos los items de la guia asociada
        Assert.Contains("Asociada", c.Markup);
        var cants = c.FindComponents<MudBlazor.MudNumericField<decimal?>>().Where(x => x.Markup.Contains("cantidad-devolver")).ToList();
        Assert.Equal(400m, cants[0].Instance.Value);
        var chk = c.FindComponents<MudBlazor.MudCheckBox<bool>>().Where(x => x.Markup.Contains("item-marcado")).First();
        await c.InvokeAsync(() => chk.Instance.ValueChanged.InvokeAsync(false));
        await c.InvokeAsync(() => c.Find("[data-accion='adicionar']").Click());
        c.WaitForState(() => f.Calls.Any(x => x.U.EndsWith("/adicionar")));
        var items = Get(JsonDocument.Parse(f.Calls.Last(x => x.U.EndsWith("/adicionar")).B!).RootElement, "Items").EnumerateArray().ToList();
        Assert.Single(items);
        Assert.Equal(0m, Get(items[0], "CAN_DEVOLVER").GetDecimal());     // 0 = quitar la linea
        Assert.Equal(1, Get(items[0], "NUM_SECUENCIA_ORIGEN").GetInt32());
    }

    [Fact]
    public async Task QuitarGuia_LlamaDelete_YAvisaAlDocumento()
    {
        var (c, f) = Dibuja(asociada: true);
        await c.InvokeAsync(() => c.Find("[data-accion='quitar-guia']").Click());
        c.WaitForState(() => f.Calls.Any(x => x.M == "DELETE"));
        Assert.EndsWith("/7/devolucion-compra/guias/11", f.Calls.Last(x => x.M == "DELETE").U);
        Assert.Equal(1, _cambios);
    }

    [Fact]
    public async Task SoloLectura_BloqueaLosBotonesYMuestraElMotivo()
    {
        var (c, _) = Dibuja(soloLectura: "El movimiento ya esta cerrado.");
        Assert.Contains("El movimiento ya esta cerrado.", c.Markup);
        await MarcarGuiaSinEscribir(c);
        Assert.True(c.Find("[data-accion='adicionar']").HasAttribute("disabled"));
        Assert.True(c.Find("[data-accion='seleccionar-todo']").HasAttribute("disabled"));
    }

    async Task MarcarGuiaSinEscribir(IRenderedComponent<MovimientoDevolucionCompraPanel> c)
    {
        // con solo lectura el check esta deshabilitado: se fuerza el cambio para ver los botones igualmente bloqueados
        var cb = c.FindComponents<MudBlazor.MudCheckBox<bool>>().Where(x => x.Markup.Contains("guia-marcada")).First();
        await c.InvokeAsync(() => cb.Instance.ValueChanged.InvokeAsync(true));
        c.WaitForState(() => c.Markup.Contains("TELA JERSEY"));
    }
}
