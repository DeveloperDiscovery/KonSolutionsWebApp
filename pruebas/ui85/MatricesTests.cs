using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Blazored.LocalStorage;
using Bunit;
using KONSolutions.Shared.Common;
using KONSolutions.Shared.Models;
using KONSolutions.Shared.Models.Ingenieria;
using KONSolutions.Web.Pages.Ingenieria;
using KONSolutions.Web.Pages.Shared;
using KONSolutions.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using Xunit;

// Matrices de operaciones (script SQL 130): pantalla, dialogo y editor de arbol reutilizable, contra una API simulada.
public class FakeMatrices : HttpMessageHandler
{
    public List<(string M, string U, JsonElement? B)> Calls = new();
    public string? MensajeFalla;       // si se pone, las escrituras fallan con este mensaje
    public List<MatrizResumen> Matrices = new()
    {
        new() { COD_RUTA_PRODUCCION = "RT1", DES_RUTA_PRODUCCION = "MATRIZ POLO BASICO", COD_TIPO_RUTA_PRODUCCION = "PRD", COD_GRUPO_PRODUCTO = "G1", COD_FAMILIA_PRODUCTO = "F1", COD_SUB_FAMILIA_PRODUCTO = "S1",
                DES_SUB_FAMILIA_PRODUCTO = "MANGA CORTA", CAN_ELEMENTOS = 34, CAN_HOJAS_CATALOGO = 20, VAL_SAM_TOTAL = 9.157m, CAN_COPIAS = 2 },
        new() { COD_RUTA_PRODUCCION = "RT2", DES_RUTA_PRODUCCION = "MATRIZ DRON", COD_TIPO_RUTA_PRODUCCION = "PRD", CAN_ELEMENTOS = 10, CAN_HOJAS_CATALOGO = 6, CAN_HOJAS_SIN_TIEMPO = 1, VAL_SAM_TOTAL = 120.5m, CAN_COPIAS = 0 },
    };
    public List<RutaNodo> Nodos = new()
    {
        new() { NUM_ESTRUCTURA_RUTA_PROCESO = 1, DES_TIPO_ELEMENTO = "PRC", COD_PROCESO = "COSTU", DES_PROCESO = "COSTURA", NIVEL = 0, VAL_SAM_PRENDA = 9.157m, VAL_SAM_NODO = 9.157m },
        new() { NUM_ESTRUCTURA_RUTA_PROCESO = 2, NUM_ESTRUCTURA_RUTA_PROCESO_PADRE = 1, DES_TIPO_ELEMENTO = "SPR", COD_SUB_PROCESO = "ARMDO", DES_SUB_PROCESO = "ARMADO", NIVEL = 1, VAL_SAM_NODO = 9.157m, VAL_SAM_PRENDA = 9.157m },
        new() { NUM_ESTRUCTURA_RUTA_PROCESO = 3, NUM_ESTRUCTURA_RUTA_PROCESO_PADRE = 2, DES_TIPO_ELEMENTO = "OPE", COD_OPERACION = "OP1", DES_OPERACION = "UNIR HOMBROS", NIVEL = 2, FLG_HOJA = true, VAL_FRECUENCIA = 1, VAL_SAM_PROPIO = 1.2m, COD_CATEGORIA_OCUPACIONAL = "A" },
        new() { NUM_ESTRUCTURA_RUTA_PROCESO = 4, NUM_ESTRUCTURA_RUTA_PROCESO_PADRE = 2, DES_TIPO_ELEMENTO = "OPE", COD_OPERACION = "OP2", DES_OPERACION = "PEGAR ETIQUETA", NIVEL = 2, FLG_HOJA = true, VAL_FRECUENCIA = 2, VAL_SAM_PROPIO = 0.5m },
    };
    int _sig = 3;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage r, CancellationToken ct)
    {
        var u = r.RequestUri!.PathAndQuery;
        JsonElement? body = r.Content is null ? null : JsonDocument.Parse(await r.Content.ReadAsStringAsync(ct)).RootElement.Clone();
        Calls.Add((r.Method.Method, u, body));
        HttpResponseMessage Ok(object? data) => new(HttpStatusCode.OK) { Content = JsonContent.Create(new { Success = true, Data = data }) };
        HttpResponseMessage Mal(string msg) => new(HttpStatusCode.BadRequest) { Content = JsonContent.Create(new { Success = false, Message = msg }) };
        if (r.Method == HttpMethod.Get)
        {
            if (u.EndsWith("/ingenieria/matrices")) return Ok(Matrices);
            if (u.EndsWith("/ingenieria/rutas-tiempos")) return Ok(new List<RutaTiempoResumen> { new() { COD_RUTA_PRODUCCION = "RT9", DES_RUTA_PRODUCCION = "RUTA COMUN", CAN_FILAS_CATALOGO = 5, CAN_HOJAS_CATALOGO = 3, VAL_SAM_TOTAL = 4m } });
            if (u.Contains("/arbol")) return Ok(Nodos);
            if (u.Contains("/operaciones/combos")) return Ok(new Dictionary<string, object[]> { ["categorias"] = new object[] { new { COD = "A", DES = "ESPECIALIZADO" }, new { COD = "B", DES = "INTERMEDIO" } } });
            if (u.Contains("/estandar/grupos-producto/combobox")) return Ok(new List<object> { new { Id = "G1", Texto = "PRENDAS" }, new { Id = "G2", Texto = "DRONES" } });
            if (u.Contains("/estandar/familias-producto")) return Ok(new List<object> { new { COD_FAMILIA_PRODUCTO = "F1", DES_FAMILIA_PRODUCTO = "POLOS" } });
            if (u.Contains("/estandar/sub-familias-producto")) return Ok(new List<object> { new { COD_SUB_FAMILIA_PRODUCTO = "S1", DES_SUB_FAMILIA_PRODUCTO = "MANGA CORTA" } });
            return Ok(new object[0]);
        }
        if (MensajeFalla != null) return Mal(MensajeFalla);
        if (r.Method == HttpMethod.Post && u.EndsWith("/matrices/duplicar"))
        {
            var cod = "RT" + _sig++;
            Matrices.Add(new() { COD_RUTA_PRODUCCION = cod, DES_RUTA_PRODUCCION = "COPIA", CAN_ELEMENTOS = 34 });
            return Ok(new MatrizAsignarResultado { COD_RUTA_PRODUCCION = cod, ELEMENTOS_COPIADOS = 34, PRECEDENCIAS_COPIADAS = 80 });
        }
        if (r.Method == HttpMethod.Post && u.EndsWith("/ingenieria/matrices")) { var cod = "RT" + _sig++; Matrices.Add(new() { COD_RUTA_PRODUCCION = cod, DES_RUTA_PRODUCCION = body!.Value.GetProperty("deS_RUTA").GetString() }); return Ok(cod); }
        if (r.Method == HttpMethod.Delete && u.Contains("/ingenieria/matrices/")) { Matrices.RemoveAll(m => u.EndsWith("/" + m.COD_RUTA_PRODUCCION)); return Ok(true); }
        if (r.Method == HttpMethod.Post && u.EndsWith("/rutas-tiempos/quitar")) return Ok(1);
        return Ok(true);
    }
}

public class MatricesTests : TestContext
{
    readonly FakeMatrices _f = new();
    IRenderedComponent<MudDialogProvider>? _prov;

    IRenderedComponent<MudDialogProvider> Prov()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton(new HttpClient(_f) { BaseAddress = new Uri("http://x/") });
        Services.AddBlazoredLocalStorage();
        Services.AddScoped<ErrorLogService>(); Services.AddScoped<ConfigService>(); Services.AddScoped<EtiquetasService>();
        Services.AddSingleton<Microsoft.Extensions.Configuration.IConfiguration>(new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build());
        Services.AddScoped<AccessLogService>(); Services.AddScoped<MenuService>(); Services.AddScoped<ControlCambiosClientService>();
        ComponentFactories.AddStub<AccionesBotones>();
        RenderComponent<MudPopoverProvider>(); RenderComponent<MudSnackbarProvider>();
        return RenderComponent<MudDialogProvider>();
    }

    IRenderedComponent<Matrices> Pagina()
    {
        _prov = Prov();
        var pg = RenderComponent<Matrices>();
        pg.WaitForAssertion(() => Assert.Contains("MATRIZ POLO BASICO", pg.Markup));
        return pg;
    }

    static JsonElement Prop(JsonElement e, string n) => e.EnumerateObject().First(x => string.Equals(x.Name, n, StringComparison.OrdinalIgnoreCase)).Value;
    int Cuantas(string metodo, string fin) => _f.Calls.Count(c => c.M == metodo && c.U.EndsWith(fin));
    static bool Deshabilitado(AngleSharp.Dom.IElement e) => e.HasAttribute("disabled") || (e.ClassName ?? "").Contains("mud-disabled");
    async Task Elige(IRenderedComponent<Matrices> pg, string cod) { await pg.InvokeAsync(() => pg.Find($"tr[data-matriz='{cod}']").Click()); pg.WaitForAssertion(() => Assert.NotEmpty(pg.FindAll("[data-zona='barra']"))); }

    [Fact]
    public void ListaLasMatricesConSuSubFamilia_SinSubFamiliaDiceCualquierProducto_YElSamTotal()
    {
        var pg = Pagina();
        var polo = pg.Find("tr[data-matriz='RT1']").TextContent;
        Assert.Contains("MANGA CORTA", polo); Assert.Contains("34", polo); Assert.Contains("9.157", polo);
        Assert.Contains("Cualquier producto", pg.Find("tr[data-matriz='RT2']").TextContent);
        Assert.NotEmpty(pg.Find("tr[data-matriz='RT2']").QuerySelectorAll("svg"));               // aviso: una hoja sin tiempo
        Assert.DoesNotContain("RUTA COMUN", pg.Markup);                                           // las rutas comunes no se mezclan en esta lista
        Assert.Contains("Elige una matriz", pg.Markup);
    }

    [Fact]
    public void SinMatrizElegida_EditarDuplicarYEliminarEstanDeshabilitados()
    {
        var pg = Pagina();
        foreach (var a in new[] { "editar", "duplicar", "eliminar" }) Assert.True(Deshabilitado(pg.Find($"[data-accion='{a}']")), a);
        Assert.False(Deshabilitado(pg.Find("[data-accion='nueva']")));
    }

    [Fact]
    public async Task ElegirUnaMatriz_HabilitaLosBotones_YCargaSuArbolEnElEditor()
    {
        var pg = Pagina();
        await Elige(pg, "RT1");
        foreach (var a in new[] { "editar", "duplicar", "eliminar" }) Assert.False(Deshabilitado(pg.Find($"[data-accion='{a}']")), a);
        pg.WaitForAssertion(() => Assert.Contains("COSTURA", pg.Find("[data-tabla='arbol']").TextContent));
        Assert.Contains("ARMADO", pg.Find("[data-tabla='arbol']").TextContent);
        Assert.Equal("9.157", pg.Find("[data-total='1']").TextContent.Trim());
        Assert.Contains("MINUTOS ESTÁNDAR POR UNIDAD", pg.Markup);                                // genérico: no depende del tipo de producto
        Assert.True(Cuantas("GET", "/rutas-tiempos/RT1/arbol") + _f.Calls.Count(c => c.M == "GET" && c.U.Contains("/rutas-tiempos/RT1/arbol")) > 0);
        Assert.NotEmpty(pg.FindAll("[data-accion='agregar-proceso']")); Assert.NotEmpty(pg.FindAll("[data-accion='precedencias']"));
    }

    [Fact]
    public async Task UnaOperacionDelCatalogoSePuedeAgregarAUnProcesoOAUnSubproceso_PeroNoSinElegirNada()
    {
        var pg = Pagina();
        await Elige(pg, "RT1");
        pg.WaitForAssertion(() => Assert.NotEmpty(pg.FindAll("tr[data-fila='2']")));
        Assert.True(Deshabilitado(pg.Find("[data-accion='agregar-operacion']")));                  // nada elegido
        await pg.InvokeAsync(() => pg.Find("tr[data-fila='1']").Click());                          // un proceso
        Assert.False(Deshabilitado(pg.Find("[data-accion='agregar-operacion']")));
        Assert.False(Deshabilitado(pg.Find("[data-accion='agregar-subproceso']")));
        await pg.InvokeAsync(() => pg.Find("tr[data-fila='2']").Click());                          // un subproceso
        Assert.False(Deshabilitado(pg.Find("[data-accion='agregar-operacion']")));
        Assert.True(Deshabilitado(pg.Find("[data-accion='agregar-subproceso']")));                 // un subproceso no recibe otro subproceso
    }

    [Fact]
    public async Task EditarUnaOperacion_AbreElDialogoConSusDatos_YGuardaVecesParaleloYCategoria()
    {
        var pg = Pagina();
        await Elige(pg, "RT1");
        pg.WaitForAssertion(() => Assert.NotEmpty(pg.FindAll("tr[data-fila='4']")));
        Assert.Empty(pg.Find("tr[data-fila='1']").QuerySelectorAll("[data-accion='editar-operacion']"));      // un proceso no tiene nada que editar
        Assert.Empty(pg.Find("tr[data-fila='2']").QuerySelectorAll("[data-accion='editar-operacion']"));      // ni un subproceso
        await pg.InvokeAsync(() => pg.Find("tr[data-fila='4'] [data-accion='editar-operacion']").Click());
        _prov!.WaitForAssertion(() => Assert.Contains("PEGAR ETIQUETA", _prov.Markup));
        Assert.Equal(2m, _prov.FindComponent<MudNumericField<decimal>>().Instance.Value);                      // trae las veces actuales
        await _prov.InvokeAsync(() => _prov.FindComponent<MudNumericField<decimal>>().Instance.ValueChanged.InvokeAsync(3m));
        await _prov.InvokeAsync(() => _prov.FindComponent<MudSwitch<bool>>().Instance.ValueChanged.InvokeAsync(true));
        _prov.WaitForAssertion(() => Assert.NotEmpty(_prov.FindComponent<MudSelect<string>>().FindComponents<MudSelectItem<string>>()));
        await _prov.InvokeAsync(() => _prov.FindComponent<MudSelect<string>>().Instance.ValueChanged.InvokeAsync("B"));
        var antes = _f.Calls.Count(c => c.M == "GET" && c.U.Contains("/rutas-tiempos/RT1/arbol"));
        await _prov.InvokeAsync(() => _prov.Find("[data-accion='guardar']").Click());
        pg.WaitForAssertion(() => Assert.Contains(_f.Calls, c => c.M == "POST" && c.U.EndsWith("/rutas-tiempos/editar")));
        var b = _f.Calls.Last(c => c.M == "POST" && c.U.EndsWith("/rutas-tiempos/editar")).B!.Value;
        Assert.Equal("RT1", Prop(b, "COD_RUTA_PRODUCCION").GetString()); Assert.Equal(4, Prop(b, "NUM_ESTRUCTURA_RUTA_PROCESO").GetInt32());
        Assert.Equal(3m, Prop(b, "VAL_FRECUENCIA").GetDecimal()); Assert.True(Prop(b, "FLG_PARALELO").GetBoolean()); Assert.Equal("B", Prop(b, "COD_CATEGORIA_OCUPACIONAL").GetString());
        pg.WaitForAssertion(() => Assert.True(_f.Calls.Count(c => c.M == "GET" && c.U.Contains("/rutas-tiempos/RT1/arbol")) > antes));   // el arbol se vuelve a leer
    }

    [Fact]
    public async Task ElArbol_MuestraLosMinutosPorProcesoYSubproceso()
    {
        var pg = Pagina();
        await Elige(pg, "RT1");
        pg.WaitForAssertion(() => Assert.NotEmpty(pg.FindAll("[data-zona='por-proceso'] [data-proc]")));
        var proc = pg.Find("[data-zona='por-proceso'] [data-proc='1']");
        Assert.Contains("COSTURA", proc.TextContent); Assert.Equal("9.157", proc.GetAttribute("data-min"));
        var sub = pg.Find("[data-zona='por-proceso'] [data-proc='2']");
        Assert.Contains("ARMADO", sub.TextContent);
        Assert.Equal(2, pg.FindAll("[data-zona='por-proceso'] [data-proc]").Count);                  // solo proceso y subproceso, no las operaciones
    }

    [Fact]
    public async Task EditarUnaOperacion_ConVecesEnCero_NoEnviaNada()
    {
        var pg = Pagina();
        await Elige(pg, "RT1");
        pg.WaitForAssertion(() => Assert.NotEmpty(pg.FindAll("tr[data-fila='3']")));
        await pg.InvokeAsync(() => pg.Find("tr[data-fila='3'] [data-accion='editar-operacion']").Click());
        _prov!.WaitForAssertion(() => Assert.Contains("UNIR HOMBROS", _prov.Markup));
        Assert.Equal("A", _prov.FindComponent<MudSelect<string>>().Instance.Value);                              // trae la categoria
        await _prov.InvokeAsync(() => _prov.FindComponent<MudNumericField<decimal>>().Instance.ValueChanged.InvokeAsync(0m));
        await _prov.InvokeAsync(() => _prov.Find("[data-accion='guardar']").Click());
        _prov.WaitForAssertion(() => Assert.Contains("mayores que cero", _prov.Find("[data-error='1']").TextContent));
        Assert.DoesNotContain(_f.Calls, c => c.U.EndsWith("/rutas-tiempos/editar"));
    }

    [Fact]
    public async Task SubirYBajar_SoloEntreHermanos_YEnvianLaDireccion()
    {
        var pg = Pagina();
        await Elige(pg, "RT1");
        pg.WaitForAssertion(() => Assert.NotEmpty(pg.FindAll("tr[data-fila='4']")));
        bool Off(string fila, string acc) => Deshabilitado(pg.Find($"tr[data-fila='{fila}'] [data-accion='{acc}']"));
        Assert.True(Off("1", "subir")); Assert.True(Off("1", "bajar"));                                          // el unico proceso
        Assert.True(Off("2", "subir")); Assert.True(Off("2", "bajar"));                                          // el unico subproceso
        Assert.True(Off("3", "subir")); Assert.False(Off("3", "bajar"));                                         // primera de dos operaciones
        Assert.False(Off("4", "subir")); Assert.True(Off("4", "bajar"));                                         // ultima
        await pg.InvokeAsync(() => pg.Find("tr[data-fila='4'] [data-accion='subir']").Click());
        pg.WaitForAssertion(() => Assert.Contains(_f.Calls, c => c.M == "POST" && c.U.EndsWith("/rutas-tiempos/mover")));
        var b = _f.Calls.Last(c => c.M == "POST" && c.U.EndsWith("/rutas-tiempos/mover")).B!.Value;
        Assert.Equal("RT1", Prop(b, "COD_RUTA_PRODUCCION").GetString()); Assert.Equal(4, Prop(b, "NUM_ESTRUCTURA_RUTA_PROCESO").GetInt32()); Assert.Equal(-1, Prop(b, "DIRECCION").GetInt32());
    }

    [Fact]
    public async Task CambiarDeMatriz_VuelveALeerElArbolDeLaOtra()
    {
        var pg = Pagina();
        await Elige(pg, "RT1");
        await Elige(pg, "RT2");
        pg.WaitForAssertion(() => Assert.Contains(_f.Calls, c => c.M == "GET" && c.U.Contains("/rutas-tiempos/RT2/arbol")));
    }

    [Fact]
    public async Task EditarElArbol_AvisaALaPantalla_QueVuelveALeerLaLista()
    {
        var pg = Pagina();
        await Elige(pg, "RT1");
        pg.WaitForAssertion(() => Assert.NotEmpty(pg.FindAll("tr[data-fila='2']")));
        var antes = _f.Calls.Count(c => c.M == "GET" && c.U.EndsWith("/ingenieria/matrices"));
        await pg.InvokeAsync(() => pg.Find("tr[data-fila='2']").Click());
        await pg.InvokeAsync(() => pg.Find("[data-accion='quitar']").Click());
        pg.WaitForAssertion(() => Assert.Contains(_f.Calls, c => c.M == "POST" && c.U.EndsWith("/rutas-tiempos/quitar")));
        var b = _f.Calls.Last(c => c.M == "POST" && c.U.EndsWith("/rutas-tiempos/quitar")).B!.Value;
        Assert.Equal("RT1", b.EnumerateObject().First(x => x.Name.Equals("COD_RUTA_PRODUCCION", StringComparison.OrdinalIgnoreCase)).Value.GetString());
        pg.WaitForAssertion(() => Assert.True(_f.Calls.Count(c => c.M == "GET" && c.U.EndsWith("/ingenieria/matrices")) > antes));   // los totales de la lista se actualizan
    }

    [Fact]
    public void LaGrillaTieneCabecerasConOrdenYFiltro_AltoAutomaticoYTirador()
    {
        var pg = Pagina();
        Assert.NotNull(pg.Find("[data-resizable-grid='matrices-lista']").QuerySelector(":scope > .grid-resize-handle"));
        Assert.Equal(5, pg.FindAll("th .eo-titulo").Count);
        Assert.Equal(5, pg.FindAll("th .eo-flecha").Count);
        string[] Filas() => pg.FindAll("tr[data-matriz]").Select(tr => tr.GetAttribute("data-matriz")!).ToArray();
        Assert.Equal(new[] { "RT1", "RT2" }, Filas());
        pg.FindAll("th .eo-titulo").First(x => x.TextContent.Trim() == "Elementos").Click();            // menor a mayor (10 < 34, por valor)
        pg.WaitForAssertion(() => Assert.Equal(new[] { "RT2", "RT1" }, Filas()));
    }

    [Fact]
    public async Task Duplicar_PideLaCopiaDeLaMatrizElegida_YQuedaElegida()
    {
        var pg = Pagina();
        await Elige(pg, "RT1");
        await pg.InvokeAsync(() => pg.Find("[data-accion='duplicar']").Click());
        pg.WaitForAssertion(() => Assert.NotEmpty(pg.FindAll("tr[data-matriz='RT3']")));
        var post = _f.Calls.Last(c => c.M == "POST" && c.U.EndsWith("/matrices/duplicar"));
        Assert.Equal("RT1", post.B!.Value.EnumerateObject().First(p => p.Name.Equals("COD_RUTA_PRODUCCION", StringComparison.OrdinalIgnoreCase)).Value.GetString());
        Assert.Contains("RT3", pg.Find("tr.rt-sel").GetAttribute("data-matriz"));
    }

    [Fact]
    public async Task Duplicar_SiLaApiRechaza_MuestraElMensajeYNoCambiaLaLista()
    {
        var pg = Pagina();
        await Elige(pg, "RT1");
        _f.MensajeFalla = "Ya existe una matriz con ese nombre.";
        await pg.InvokeAsync(() => pg.Find("[data-accion='duplicar']").Click());
        pg.WaitForAssertion(() => Assert.Contains("Ya existe una matriz con ese nombre.", pg.Find("[data-error='1']").TextContent));
        Assert.Equal(2, pg.FindAll("tr[data-matriz]").Count);
    }

    [Fact]
    public async Task Eliminar_PideConfirmacion_YSoloAlConfirmarBorra()
    {
        var pg = Pagina();
        await Elige(pg, "RT2");
        await pg.InvokeAsync(() => pg.Find("[data-accion='eliminar']").Click());
        _prov!.WaitForAssertion(() => Assert.Contains("Las copias que ya se asignaron", _prov.Markup));
        Assert.DoesNotContain(_f.Calls, c => c.M == "DELETE");                                    // todavia no
        await _prov.InvokeAsync(() => _prov.FindAll("button").First(b => b.TextContent.Trim() == "Cancelar").Click());
        Assert.DoesNotContain(_f.Calls, c => c.M == "DELETE");
        await pg.InvokeAsync(() => pg.Find("[data-accion='eliminar']").Click());
        _prov.WaitForAssertion(() => Assert.Contains("Las copias que ya se asignaron", _prov.Markup));
        await _prov.InvokeAsync(() => _prov.FindAll("button").First(b => b.TextContent.Trim() == "Eliminar").Click());
        pg.WaitForAssertion(() => Assert.Empty(pg.FindAll("tr[data-matriz='RT2']")));
        Assert.Contains(_f.Calls, c => c.M == "DELETE" && c.U.EndsWith("/ingenieria/matrices/RT2"));
    }

    [Fact]
    public void LaPantallaAceptaLosParametrosDelMenu()
    {
        foreach (var nombre in new[] { "CodAplicacion", "CodOpcionAplicacion" })
        {
            var prop = typeof(Matrices).GetProperty(nombre);
            Assert.NotNull(prop);
            Assert.NotNull(prop!.GetCustomAttributes(typeof(Microsoft.AspNetCore.Components.ParameterAttribute), false).FirstOrDefault());
        }
    }

    // ======================= dialogo de la matriz =======================
    async Task<IRenderedComponent<MudDialogProvider>> Dialogo(MatrizResumen? m)
    {
        var p = Prov();
        var ds = Services.GetRequiredService<IDialogService>();
        var pr = new DialogParameters(); if (m is not null) pr["Matriz"] = m;
        await p.InvokeAsync(async () => await ds.ShowAsync<MatrizDialog>("t", pr));
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-campo='nombre']")));
        return p;
    }

    [Fact]
    public async Task Dialogo_SinNombre_NoGuardaYLoDice()
    {
        var p = await Dialogo(null);
        await p.InvokeAsync(() => p.Find("[data-accion='guardar']").Click());
        p.WaitForAssertion(() => Assert.Contains("Escribe el nombre", p.Find("[data-error='1']").TextContent));
        Assert.DoesNotContain(_f.Calls, c => c.M == "POST");
    }

    [Fact]
    public async Task Dialogo_UnaSubFamiliaIncompleta_NoGuarda()
    {
        var p = await Dialogo(null);
        await p.InvokeAsync(() => p.FindComponents<MudTextField<string>>().First().Instance.ValueChanged.InvokeAsync("MATRIZ NUEVA"));
        var grupo = p.FindComponents<AriaCombo<ComboboxItem>>().First();
        await p.InvokeAsync(() => grupo.Instance.ValueChanged.InvokeAsync("G1"));                // solo el grupo
        await p.InvokeAsync(() => p.Find("[data-accion='guardar']").Click());
        p.WaitForAssertion(() => Assert.Contains("completos, o ninguno", p.Find("[data-error='1']").TextContent));
        Assert.DoesNotContain(_f.Calls, c => c.M == "POST");
    }

    [Fact]
    public async Task Dialogo_GuardaUnaMatrizNuevaSinSubFamilia()
    {
        var p = await Dialogo(null);
        await p.InvokeAsync(() => p.FindComponents<MudTextField<string>>().First().Instance.ValueChanged.InvokeAsync("MATRIZ NUEVA"));
        await p.InvokeAsync(() => p.Find("[data-accion='guardar']").Click());
        p.WaitForAssertion(() => Assert.Contains(_f.Calls, c => c.M == "POST" && c.U.EndsWith("/ingenieria/matrices")));
        var b = _f.Calls.Last(c => c.M == "POST" && c.U.EndsWith("/ingenieria/matrices")).B!.Value;
        Assert.Equal("MATRIZ NUEVA", b.EnumerateObject().First(x => x.Name.Equals("DES_RUTA", StringComparison.OrdinalIgnoreCase)).Value.GetString());
        Assert.Equal(JsonValueKind.Null, b.EnumerateObject().First(x => x.Name.Equals("COD_GRUPO_PRODUCTO", StringComparison.OrdinalIgnoreCase)).Value.ValueKind);
    }

    [Fact]
    public async Task Dialogo_AlCambiarUnaMatriz_PrecargaSusDatosYGuardaConPUT()
    {
        var p = await Dialogo(_f.Matrices[0]);
        Assert.Equal("MATRIZ POLO BASICO", p.FindComponents<MudTextField<string>>().First().Instance.Value);
        p.WaitForAssertion(() => Assert.Equal("S1", p.FindComponents<AriaCombo<ComboboxItem>>().Last().Instance.Value));
        await p.InvokeAsync(() => p.Find("[data-accion='guardar']").Click());
        p.WaitForAssertion(() => Assert.Contains(_f.Calls, c => c.M == "PUT" && c.U.EndsWith("/ingenieria/matrices")));
        var b = _f.Calls.Last(c => c.M == "PUT").B!.Value;
        Assert.Equal("RT1", b.EnumerateObject().First(x => x.Name.Equals("COD_RUTA_PRODUCCION", StringComparison.OrdinalIgnoreCase)).Value.GetString());
        Assert.Equal("S1", b.EnumerateObject().First(x => x.Name.Equals("COD_SUB_FAMILIA_PRODUCTO", StringComparison.OrdinalIgnoreCase)).Value.GetString());
    }

    [Fact]
    public async Task Dialogo_DejarlaSinSubFamilia_LimpiaLosTresCodigos()
    {
        var p = await Dialogo(_f.Matrices[0]);
        p.WaitForAssertion(() => Assert.NotEmpty(p.FindAll("[data-accion='sin-sub-familia']")));
        await p.InvokeAsync(() => p.Find("[data-accion='sin-sub-familia']").Click());
        await p.InvokeAsync(() => p.Find("[data-accion='guardar']").Click());
        p.WaitForAssertion(() => Assert.Contains(_f.Calls, c => c.M == "PUT"));
        var b = _f.Calls.Last(c => c.M == "PUT").B!.Value;
        foreach (var n in new[] { "COD_GRUPO_PRODUCTO", "COD_FAMILIA_PRODUCTO", "COD_SUB_FAMILIA_PRODUCTO" })
            Assert.Equal(JsonValueKind.Null, b.EnumerateObject().First(x => x.Name.Equals(n, StringComparison.OrdinalIgnoreCase)).Value.ValueKind);
    }

    [Fact]
    public async Task Dialogo_SiLaApiRechaza_MuestraElMensajeYNoSeCierra()
    {
        var p = await Dialogo(null);
        _f.MensajeFalla = "Ya existe una matriz con ese nombre.";
        await p.InvokeAsync(() => p.FindComponents<MudTextField<string>>().First().Instance.ValueChanged.InvokeAsync("MATRIZ POLO BASICO"));
        await p.InvokeAsync(() => p.Find("[data-accion='guardar']").Click());
        p.WaitForAssertion(() => Assert.Contains("Ya existe una matriz con ese nombre.", p.Find("[data-error='1']").TextContent));
    }

    // ======================= Rutas con tiempos sigue igual despues de sacar el editor =======================
    [Fact]
    public async Task RutasConTiempos_SigueMostrandoSuListaYElEditorAlElegirUnaRuta()
    {
        _prov = Prov();
        var pg = RenderComponent<RutasTiempos>();
        pg.WaitForAssertion(() => Assert.Contains("RUTA COMUN", pg.Markup));
        Assert.Contains("Elige una ruta", pg.Markup);
        Assert.Empty(pg.FindAll("[data-zona='barra']"));
        await pg.InvokeAsync(() => pg.Find("tr[data-ruta='RT9']").Click());
        pg.WaitForAssertion(() => Assert.NotEmpty(pg.FindAll("[data-zona='barra']")));
        pg.WaitForAssertion(() => Assert.Contains("COSTURA", pg.Find("[data-tabla='arbol']").TextContent));
        Assert.Contains("MINUTOS ESTÁNDAR POR UNIDAD", pg.Markup);                                // texto genérico también en Rutas con tiempos
    }
}
