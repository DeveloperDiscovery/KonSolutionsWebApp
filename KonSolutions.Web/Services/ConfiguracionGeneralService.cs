using System.Net.Http.Json;
using KONSolutions.Shared.Common;
using KONSolutions.Shared.Models.Estandar;

namespace KONSolutions.Web.Services;

public class ConfiguracionGeneralService
{
    private readonly HttpClient _http;
    private readonly ErrorLogService _errorLog;
    private const string Ruta = "api/v1/estandar/configuracion-general";

    /// <summary>URL del logo, servido como ARCHIVO y no dentro del JSON.
    ///
    /// Así el navegador lo cachea un día y no lo vuelve a pedir. Antes bajaba entero en
    /// cada arranque, metido en la respuesta de configuración, que al cambiar invalida
    /// todo — incluidos unos bytes que no cambian nunca.
    ///
    /// ES ABSOLUTA, Y ESO NO ES UN DETALLE. Estas URL van en un &lt;img src&gt;, y esa la
    /// resuelve el NAVEGADOR contra la página, no contra la API. Acá no son el mismo
    /// origen —la aplicación en un puerto, la API en otro—, así que con una ruta relativa
    /// el navegador pedía la imagen al sitio de la aplicación y recibía un 404: se veía el
    /// recuadro de imagen rota.
    ///
    /// Con HttpClient no pasaba, porque ese sí resuelve contra su BaseAddress. De ahí sale
    /// también esta URL, para no repetir en ningún lado la dirección de la API.
    ///
    /// Si no hay logo cargado el endpoint responde 204 y la imagen no se dibuja; por eso
    /// las pantallas preguntan primero por FLG_TIENE_LOGO_*.</summary>
    //
    // LA VERSIÓN ES PARTE DE LA DIRECCIÓN. Sin ella la dirección era siempre la misma, y el logo se guardaba un día (en el
    // navegador y en la memoria de la API): al grabar uno nuevo se seguía viendo el anterior, ni cerrando sesión y volviendo a
    // entrar. Con la versión (que el servidor calcula del contenido) un logo distinto es otra dirección, el navegador lo baja,
    // y uno que no cambió se sigue pintando desde el disco sin pedir nada. Sin versión el servidor responde igual de bien
    // (el navegador pregunta si cambió), solo que con una petición por vez.
    public string UrlLogoEmpresa(string? version = null)    => Absoluta(ConVersion($"{Ruta}/logo-empresa", version));
    public string UrlLogoAplicacion(string? version = null) => Absoluta(ConVersion($"{Ruta}/logo-aplicacion", version));

    private static string ConVersion(string ruta, string? version) =>
        string.IsNullOrWhiteSpace(version) ? ruta : $"{ruta}?v={Uri.EscapeDataString(version)}";

    /// <summary>Los bytes del logo de la aplicación, para la copia en base64 que usan el menú, la pantalla de carga y el bloqueo.
    /// Null si no hay logo o la descarga falla (no es un error: la pantalla sigue con lo que tenía).</summary>
    public async Task<byte[]?> DescargarLogoAplicacionAsync(string? version)
    {
        try
        {
            using var r = await _http.GetAsync(UrlLogoAplicacion(version));
            return r.IsSuccessStatusCode && r.StatusCode != System.Net.HttpStatusCode.NoContent ? await r.Content.ReadAsByteArrayAsync() : null;
        }
        catch (Exception ex) { await _errorLog.LogAsync(ex, "ConfiguracionGeneral", nameof(DescargarLogoAplicacionAsync)); return null; }
    }

    private string Absoluta(string relativa) =>
        _http.BaseAddress is null ? relativa : new Uri(_http.BaseAddress, relativa).AbsoluteUri;   // AbsoluteUri y no ToString(): este último des-escapa (deja un espacio literal)

    public ConfiguracionGeneralService(HttpClient http, ErrorLogService errorLog)
    { _http = http; _errorLog = errorLog; }

    public async Task<ConfiguracionGeneral?> GetCurrentAsync()
    {
        try
        {
            var r = await _http.GetFromJsonAsync<ApiResponse<List<ConfiguracionGeneral>>>(Ruta);
            return r?.Data?.FirstOrDefault();
        }
        catch (Exception ex) { await _errorLog.LogAsync(ex, "ConfiguracionGeneral", nameof(GetCurrentAsync)); return null; }
    }

    /// <summary>El mismo registro completo, pero SIN los bytes de los dos logos.
    ///
    /// Para el layout, que necesita todo lo demás —colores, tipografías, cultura,
    /// parámetros de grilla— pero ya no los logos, porque los pide por URL. Traerlos
    /// costaba 9 932 ms en cada arranque, medidos.
    ///
    /// Vienen en su lugar FLG_TIENE_LOGO_EMPRESA y FLG_TIENE_LOGO_APLICACION, para saber
    /// si hay imagen sin tener que bajarla.</summary>
    public async Task<ConfiguracionGeneral?> GetParaLayoutAsync()
    {
        try
        {
            var r = await _http.GetFromJsonAsync<ApiResponse<List<ConfiguracionGeneral>>>($"{Ruta}/layout");
            return r?.Data?.FirstOrDefault();
        }
        catch (Exception ex) { await _errorLog.LogAsync(ex, "ConfiguracionGeneral", nameof(GetParaLayoutAsync)); return null; }
    }

    /// <summary>Para la pantalla de Login (sin sesión todavía) — GetAll()/GetCurrentAsync
    /// ahora exige autenticación (traía datos fiscales/legales expuestos sin token). Este
    /// endpoint devuelve solo lo cosmético (logo/colores/tipografía) y sigue anónimo.</summary>
    public async Task<ConfiguracionGeneralPublica?> GetPublicAsync()
    {
        try
        {
            var r = await _http.GetFromJsonAsync<ApiResponse<ConfiguracionGeneralPublica>>($"{Ruta}/public");
            return r?.Data;
        }
        catch (Exception ex) { await _errorLog.LogAsync(ex, "ConfiguracionGeneral", nameof(GetPublicAsync)); return null; }
    }

    /// <summary>PROC_ESTANDAR_CONFIGURACION_GENERAL_LOADCOMBOS: Monedas + Tipos de
    /// Configuración + Grupos de Producto en un solo viaje — antes 3 llamadas independientes
    /// desde ConfiguracionGeneral.razor.</summary>
    public async Task<ConfiguracionGeneralLoadCombosResult> GetLoadcombosAsync()
    {
        try
        {
            var r = await _http.GetFromJsonAsync<ApiResponse<ConfiguracionGeneralLoadCombosResult>>($"{Ruta}/loadcombos");
            return r?.Data ?? new();
        }
        catch (Exception ex) { await _errorLog.LogAsync(ex, "ConfiguracionGeneral", nameof(GetLoadcombosAsync)); return new(); }
    }

    /// <summary>Devuelve la respuesta cruda para que el llamador pueda mostrar el error
    /// real (ErrorDialogHelper.MostrarAsync) en vez de un mensaje genérico.</summary>
    public Task<HttpResponseMessage> SaveRawAsync(ConfiguracionGeneral cfg) => _http.PostAsJsonAsync(Ruta, cfg);

    public async Task<bool> SaveAsync(ConfiguracionGeneral cfg)
    {
        try
        {
            var resp = await SaveRawAsync(cfg);
            return resp.IsSuccessStatusCode;
        }
        catch (Exception ex) { await _errorLog.LogAsync(ex, "ConfiguracionGeneral", nameof(SaveAsync)); return false; }
    }
}
