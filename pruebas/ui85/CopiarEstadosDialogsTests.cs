using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Blazored.LocalStorage;
using Bunit;
using KONSolutions.Shared.Common;
using KONSolutions.Shared.Models;
using KONSolutions.Shared.Models.Estandar;
using KONSolutions.Web.Pages.Estandar;
using KONSolutions.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using Xunit;

public class FakeCopiaEstados : HttpMessageHandler
{
    public List<(string U, JsonElement B)> Posts = new();
    public bool Falla;   // el procedimiento rechaza (validacion 522xx)
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage r, CancellationToken ct)
    {
        var u = r.RequestUri!.PathAndQuery;
        if (r.Method == HttpMethod.Get)
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(new { Success = true, Data = new[] { new { Id = "10", Texto = "ORDENES DE COMPRA" }, new { Id = "41", Texto = "SOLICITUDES" } } }) };
        var body = JsonDocument.Parse(await r.Content!.ReadAsStringAsync(ct)).RootElement.Clone();
        Posts.Add((u, body));
        if (Falla) return new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = JsonContent.Create(new { Success = false, Message = "La opcion destino no existe." }) };
        bool eliminar = body.EnumerateObject().Any(x => string.Equals(x.Name, "ELIMINAR_ANTERIORES", StringComparison.OrdinalIgnoreCase) && x.Value.GetBoolean());
        object data = u.Contains("/configuracion")
            ? new object[] {
                new { SEC = 1, TIPO = "ASIGNACION", ACCION = "COPIAR", MOTIVO = "Se asigna el tipo de estado a la opcion destino." },
                new { SEC = 2, TIPO = "FLUJO", COD_BOTON_OPCION = "00", DES_BOTON = "NUEVO", COD_ESTADO = 0, DES_ESTADO = "REGISTRADA", DES_ESTADO_DESTINO = "REGISTRADA / RECIBIDA", ACCION = "COPIAR", MOTIVO = "" },
                new { SEC = 3, TIPO = "FLUJO", COD_BOTON_OPCION = "02", DES_BOTON = "ELIMINAR", COD_ESTADO = 9, DES_ESTADO = "ANULADA", ACCION = "OMITIDO", MOTIVO = "El estado 9 no existe en el tipo de estado destino." } }
            : new object[] {
                new { SEC = 1, LADO = "DESTINO", COD_ESTADO = 1, DES_ESTADO = "D-UNO", ACCION = eliminar ? "CONSERVAR" : "CONSERVAR", MOTIVO = eliminar ? "Esta en uso (lo referencia [X]); no se puede eliminar." : "No se eliminan los estados anteriores." },
                new { SEC = 2, LADO = "DESTINO", COD_ESTADO = 5, DES_ESTADO = "D-CINCO", ACCION = eliminar ? "ELIMINAR" : "CONSERVAR", MOTIVO = eliminar ? "Se elimina." : "No se eliminan los estados anteriores." },
                new { SEC = 3, LADO = "ORIGEN", COD_ESTADO = 0, DES_ESTADO = "O-CERO", ACCION = "COPIAR", MOTIVO = "" } };
        return new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(new { Success = true, Data = data }) };
    }
}

public class CopiarEstadosDialogsTests : TestContext
{
    FakeCopiaEstados _f = new();
    IRenderedComponent<MudDialogProvider> Prov()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton(new HttpClient(_f) { BaseAddress = new Uri("http://x/") });
        Services.AddBlazoredLocalStorage();
        Services.AddScoped<ErrorLogService>(); Services.AddScoped<ConfigService>();
        RenderComponent<MudPopoverProvider>(); RenderComponent<MudSnackbarProvider>();
        return RenderComponent<MudDialogProvider>();
    }

    async Task<IDialogReference> AbreTipo(IRenderedComponent<MudDialogProvider> p)
    {
        var ds = Services.GetRequiredService<IDialogService>();
        var destinos = new List<TipoEstado> { new() { COD_TIPO_ESTADO = 41, DES_TIPO_ESTADO = "SOLICITUDES" } };
        IDialogReference? d = null;
        await p.InvokeAsync(async () => d = await ds.ShowAsync<CopiarTipoEstadosDialog>("t", new DialogParameters { ["CodTipoOrigen"] = (short)10, ["DesTipoOrigen"] = "ORDENES", ["TiposDestino"] = destinos }));
        p.WaitForAssertion(() => p.Find(".mud-dialog"));
        return d!;
    }

    async Task<IDialogReference> AbreConfig(IRenderedComponent<MudDialogProvider> p)
    {
        var ds = Services.GetRequiredService<IDialogService>();
        var opciones = new List<ComboboxItem> { new() { Id = "00030|020100", Texto = "00030 · Solicitudes (020100)" } };
        IDialogReference? d = null;
        await p.InvokeAsync(async () => d = await ds.ShowAsync<CopiarConfiguracionEstadosDialog>("t", new DialogParameters
        { ["CodAplicacionOrigen"] = "00030", ["CodOpcionOrigen"] = "020101", ["DesOpcionOrigen"] = "Ordenes de Compra", ["CodTipoEstadoOrigen"] = (short)10, ["DesTipoEstadoOrigen"] = "ORDENES", ["Opciones"] = opciones }));
        p.WaitForAssertion(() => p.Find(".mud-dialog"));
        return d!;
    }

    static JsonElement Prop(JsonElement e, string n) => e.EnumerateObject().First(x => string.Equals(x.Name, n, StringComparison.OrdinalIgnoreCase)).Value;

    [Fact]
    public async Task Tipo_ElegirDestino_MuestraVistaPrevia_SinGrabar()
    {
        var p = Prov(); await AbreTipo(p);
        Assert.Empty(_f.Posts);
        var combo = p.FindComponent<KONSolutions.Web.Pages.Shared.AriaCombo<TipoEstado>>();
        await p.InvokeAsync(() => combo.Instance.ValueChanged.InvokeAsync("41"));
        p.WaitForState(() => p.Markup.Contains("O-CERO"));
        var (u, b) = _f.Posts.Single();
        Assert.Contains("/copia-estados/tipo", u);
        Assert.True(Prop(b, "SIMULAR").GetBoolean());
        Assert.False(Prop(b, "ELIMINAR_ANTERIORES").GetBoolean());
        Assert.Equal(10, Prop(b, "COD_TIPO_ESTADO_ORIGEN").GetInt32());
        Assert.Equal(41, Prop(b, "COD_TIPO_ESTADO_DESTINO").GetInt32());
        Assert.Contains("Se copian: 1", p.Markup);
        Assert.Contains("Se eliminan: 0", p.Markup);
    }

    [Fact]
    public async Task Tipo_PreguntaSiElimina_ElegirEliminarRecalculaYAdvierte()
    {
        var p = Prov(); await AbreTipo(p);
        await p.InvokeAsync(() => p.FindComponent<KONSolutions.Web.Pages.Shared.AriaCombo<TipoEstado>>().Instance.ValueChanged.InvokeAsync("41"));
        p.WaitForState(() => p.Markup.Contains("O-CERO"));
        Assert.Contains("¿Qué se hace con los estados que el destino ya tiene?", p.Markup);
        var rg = p.FindComponent<MudRadioGroup<bool>>();
        await p.InvokeAsync(() => rg.Instance.ValueChanged.InvokeAsync(true));
        p.WaitForState(() => _f.Posts.Count == 2 && p.Markup.Contains("Se eliminan: 1"));
        Assert.True(Prop(_f.Posts[1].B, "ELIMINAR_ANTERIORES").GetBoolean());
        Assert.Contains("En uso (se conservan): 1", p.Markup);
        Assert.Contains("No se puede deshacer", p.Markup);
    }

    [Fact]
    public async Task Tipo_Copiar_GrabaConLaEleccionYCierra()
    {
        var p = Prov(); var d = await AbreTipo(p);
        await p.InvokeAsync(() => p.FindComponent<KONSolutions.Web.Pages.Shared.AriaCombo<TipoEstado>>().Instance.ValueChanged.InvokeAsync("41"));
        p.WaitForState(() => p.Markup.Contains("O-CERO"));
        await p.InvokeAsync(() => p.FindComponent<MudRadioGroup<bool>>().Instance.ValueChanged.InvokeAsync(true));
        p.WaitForState(() => p.Markup.Contains("Se eliminan: 1"));
        p.FindAll("button").First(x => x.TextContent.Contains("Eliminar anteriores y copiar")).Click();
        var r = await d.Result;
        Assert.False(r!.Canceled);
        var ult = _f.Posts.Last().B;
        Assert.False(Prop(ult, "SIMULAR").GetBoolean());
        Assert.True(Prop(ult, "ELIMINAR_ANTERIORES").GetBoolean());
    }

    [Fact]
    public async Task Tipo_SinDestino_NoPermiteCopiar()
    {
        var p = Prov(); await AbreTipo(p);
        var btn = p.FindAll("button").First(x => x.TextContent.Trim() == "Copiar");
        Assert.True(btn.HasAttribute("disabled"));
    }

    [Fact]
    public async Task Config_NoCopiaSinValidar_ValidaMuestraOmitidosYLuegoCopia()
    {
        var p = Prov(); var d = await AbreConfig(p);
        var combos = p.FindComponents<KONSolutions.Web.Pages.Shared.AriaCombo<ComboboxItem>>();
        await p.InvokeAsync(() => combos[0].Instance.ValueChanged.InvokeAsync("00030|020100"));
        Assert.True(p.FindAll("button").First(x => x.TextContent.Trim() == "Copiar").HasAttribute("disabled"));   // aun no valido
        p.FindAll("button").First(x => x.TextContent.Trim() == "Validar").Click();
        p.WaitForState(() => p.Markup.Contains("El estado 9 no existe en el tipo de estado destino."));
        var (u, b) = _f.Posts.Single();
        Assert.Contains("/copia-estados/configuracion", u);
        Assert.True(Prop(b, "SIMULAR").GetBoolean());
        Assert.False(Prop(b, "REEMPLAZAR").GetBoolean());
        Assert.Equal("020100", Prop(b, "COD_OPCION_DESTINO").GetString());
        Assert.Equal("00030", Prop(b, "COD_APLICACION_DESTINO").GetString());
        Assert.Equal(10, Prop(b, "COD_TIPO_ESTADO_DESTINO").GetInt32());   // por defecto el mismo tipo de estado
        Assert.Contains("Copiar: 1", p.Markup);
        Assert.Contains("Omitidos: 1", p.Markup);
        Assert.Contains("REGISTRADA / RECIBIDA", p.Markup);                // el nombre del estado destino, para revisar el emparejamiento por codigo
        p.FindAll("button").First(x => x.TextContent.Trim() == "Copiar").Click();
        var r = await d.Result;
        Assert.False(r!.Canceled);
        Assert.False(Prop(_f.Posts.Last().B, "SIMULAR").GetBoolean());
    }

    [Fact]
    public async Task Config_CambiarElDestinoInvalidaLaValidacion()
    {
        var p = Prov(); await AbreConfig(p);
        var combos = p.FindComponents<KONSolutions.Web.Pages.Shared.AriaCombo<ComboboxItem>>();
        await p.InvokeAsync(() => combos[0].Instance.ValueChanged.InvokeAsync("00030|020100"));
        p.FindAll("button").First(x => x.TextContent.Trim() == "Validar").Click();
        p.WaitForState(() => p.Markup.Contains("Omitidos: 1"));
        await p.InvokeAsync(() => combos[1].Instance.ValueChanged.InvokeAsync("41"));   // otro tipo de estado destino
        Assert.DoesNotContain("Omitidos: 1", p.Markup);
        Assert.True(p.FindAll("button").First(x => x.TextContent.Trim() == "Copiar").HasAttribute("disabled"));
    }

    [Fact]
    public async Task Config_Reemplazar_AdvierteYCambiaElBoton()
    {
        var p = Prov(); await AbreConfig(p);
        var combos = p.FindComponents<KONSolutions.Web.Pages.Shared.AriaCombo<ComboboxItem>>();
        await p.InvokeAsync(() => combos[0].Instance.ValueChanged.InvokeAsync("00030|020100"));
        await p.InvokeAsync(() => p.FindComponent<MudSwitch<bool>>().Instance.ValueChanged.InvokeAsync(true));
        Assert.Contains("Se borran los flujos y las dependencias actuales del destino", p.Markup);
        p.FindAll("button").First(x => x.TextContent.Trim() == "Validar").Click();
        p.WaitForState(() => p.Markup.Contains("Omitidos: 1"));
        Assert.True(Prop(_f.Posts.Single().B, "REEMPLAZAR").GetBoolean());
        Assert.Contains("Reemplazar y copiar", p.Markup);
    }

    [Fact]
    public async Task Config_ErrorDeValidacion_SeMuestraYNoHayTabla()
    {
        _f.Falla = true;
        var p = Prov(); await AbreConfig(p);
        var combos = p.FindComponents<KONSolutions.Web.Pages.Shared.AriaCombo<ComboboxItem>>();
        await p.InvokeAsync(() => combos[0].Instance.ValueChanged.InvokeAsync("00030|020100"));
        p.FindAll("button").First(x => x.TextContent.Trim() == "Validar").Click();
        p.WaitForState(() => p.Markup.Contains("La opcion destino no existe."));
        Assert.DoesNotContain("Se omite", p.Markup);
        Assert.True(p.FindAll("button").First(x => x.TextContent.Trim() == "Copiar").HasAttribute("disabled"));
    }
}
