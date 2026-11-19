using System.Net.Http.Json;
using KONSolutions.Shared.Common;
using KONSolutions.Shared.Models.Seguridad;

namespace KONSolutions.Web.Services;

public class FavoritoItem
{
    public string? COD_USUARIO { get; set; }
    public string? COD_APLICACION { get; set; }
    public string? COD_OPCION_APLICACION { get; set; }
}

/// <summary>
/// Favoritos del menú: SEGURIDAD.USUARIOS_APLICACIONES_OPCIONES_FAVORITOS.
/// Permite listar, agregar (clic derecho) y quitar opciones favoritas por aplicación.
/// </summary>
public class FavoritoService
{
    private readonly HttpClient _http;
    private readonly ErrorLogService _errorLog;
    private const string Ruta = ApiRoutes.Seguridad.Favoritos;

    public event Action? OnCambio;

    public FavoritoService(HttpClient http, ErrorLogService errorLog)
    { _http = http; _errorLog = errorLog; }

    /// <summary>TODOS los favoritos del usuario (todas sus aplicaciones) en una sola llamada.
    /// El SP siempre devolvió el listado completo por usuario, así que pedirlo una vez por
    /// aplicación —como hacía el menú lateral— eran N peticiones idénticas.</summary>
    public async Task<List<FavoritoItem>> GetTodosAsync(string codUsuario)
    {
        try
        {
            var r = await _http.GetFromJsonAsync<ApiResponse<List<FavoritoItem>>>(
                $"{Ruta}/{Uri.EscapeDataString(codUsuario)}");
            return r?.Data ?? new();
        }
        catch (Exception ex) { await _errorLog.LogAsync(ex, "Favoritos", nameof(GetTodosAsync)); return new(); }
    }

    /// <summary>Favoritos de un usuario en una aplicación.</summary>
    public async Task<List<FavoritoItem>> GetAsync(string codUsuario, string codAplicacion)
    {
        try
        {
            var r = await _http.GetFromJsonAsync<ApiResponse<List<FavoritoItem>>>(
                $"{Ruta}/{Uri.EscapeDataString(codUsuario)}/{Uri.EscapeDataString(codAplicacion)}");
            return r?.Data ?? new();
        }
        catch (Exception ex) { await _errorLog.LogAsync(ex, "Favoritos", nameof(GetAsync)); return new(); }
    }

    public async Task<bool> AgregarAsync(string codUsuario, string codAplicacion, string codOpcion)
    {
        try
        {
            var dto = new { COD_USUARIO = codUsuario, COD_APLICACION = codAplicacion, COD_OPCION_APLICACION = codOpcion };
            var resp = await _http.PostAsJsonAsync(Ruta, dto);
            if (resp.IsSuccessStatusCode) { InvalidarLogosCache(); OnCambio?.Invoke(); }
            return resp.IsSuccessStatusCode;
        }
        catch (Exception ex) { await _errorLog.LogAsync(ex, "Favoritos", nameof(AgregarAsync)); return false; }
    }

    public async Task<List<FavoritoWorkspace>> GetWorkspaceAsync(string codUsuario)
    {
        try
        {
            var r = await _http.GetFromJsonAsync<ApiResponse<List<FavoritoWorkspace>>>(
                $"{ApiRoutes.Seguridad.FavoritosWorkspace}/{Uri.EscapeDataString(codUsuario)}");
            return r?.Data ?? new();
        }
        catch (Exception ex) { await _errorLog.LogAsync(ex, "Favoritos", nameof(GetWorkspaceAsync)); return new(); }
    }

    // Íconos ya pedidos en esta sesión, y la petición en vuelo si todavía no terminó. Vive
    // en el servicio (compartido por toda la app) y no en cada componente: la guarda que ya
    // tenía Workspace.razor era por instancia, así que cada pantalla de menú abierta al
    // arrancar (Workspace, Menú de Tiles, ...) lanzaba su propia copia del mismo pedido —en
    // el log se veían 3 idénticos de 3 a 5 segundos peleándose entre sí.
    private readonly Dictionary<string, Task<List<FavoritoLogoItem>>> _logosEnVuelo = new();
    private readonly Dictionary<string, List<FavoritoLogoItem>> _logosCache = new();

    /// <summary>Íconos de los favoritos del Workspace, en un endpoint aparte del texto
    /// (GetWorkspaceAsync) — se pide en paralelo, sin bloquear el primer render. Se pide una
    /// sola vez por usuario: los llamadores simultáneos comparten la misma petición y los
    /// posteriores reciben el resultado ya cacheado.</summary>
    public Task<List<FavoritoLogoItem>> GetWorkspaceLogosAsync(string codUsuario)
    {
        if (_logosCache.TryGetValue(codUsuario, out var cacheado))
            return Task.FromResult(cacheado);

        if (_logosEnVuelo.TryGetValue(codUsuario, out var enVuelo))
            return enVuelo;

        var tarea = FetchWorkspaceLogosAsync(codUsuario);
        _logosEnVuelo[codUsuario] = tarea;
        return tarea;
    }

    /// <summary>Tope propio para esta llamada, más corto que el del HttpClient.
    ///
    /// POR QUE: el HttpClient global espera 100 segundos. Estos íconos NO son
    /// indispensables —sin ellos el Workspace se ve igual, con el ícono por defecto—, pero
    /// mientras la petición esté en vuelo ocupa UNA de las seis conexiones que el navegador
    /// permite por servidor. O sea que una llamada opcional y lenta le quita el lugar a las
    /// que sí hacen falta para que la aplicación arranque.
    ///
    /// Con 20 segundos: si el servidor responde, perfecto; si no, se suelta la conexión y
    /// el arranque sigue. No se pierde nada más que unos íconos.</summary>
    private static readonly TimeSpan ToleranciaLogos = TimeSpan.FromSeconds(20);

    private async Task<List<FavoritoLogoItem>> FetchWorkspaceLogosAsync(string codUsuario)
    {
        try
        {
            using var corte = new CancellationTokenSource(ToleranciaLogos);
            var r = await _http.GetFromJsonAsync<ApiResponse<List<FavoritoLogoItem>>>(
                $"{ApiRoutes.Seguridad.FavoritosWorkspaceLogos}/{Uri.EscapeDataString(codUsuario)}",
                corte.Token);
            var datos = r?.Data ?? new();
            // Solo se cachea un resultado con contenido: si vino vacío por un fallo puntual,
            // el próximo llamador vuelve a intentarlo en vez de quedarse sin íconos siempre.
            if (datos.Count > 0) _logosCache[codUsuario] = datos;
            return datos;
        }
        catch (OperationCanceledException)
        {
            // Que tarden los íconos NO es un error de la aplicación: es una decoración que
            // no llegó. Registrarlo como excepción llenaba el log de algo que no hay que
            // arreglar a las corridas, y encima asustaba en pantalla.
            Console.WriteLine($"[TRACE] Favoritos: los íconos del Workspace no llegaron en " +
                              $"{ToleranciaLogos.TotalSeconds:0} s. Se siguen sin ellos.");
            return new();
        }
        catch (Exception ex) { await _errorLog.LogAsync(ex, "Favoritos", nameof(GetWorkspaceLogosAsync)); return new(); }
        finally { _logosEnVuelo.Remove(codUsuario); }
    }

    /// <summary>Descarta los íconos cacheados — al agregar/quitar un favorito hay que volver
    /// a pedirlos.</summary>
    public void InvalidarLogosCache() { _logosCache.Clear(); _logosEnVuelo.Clear(); }

    public async Task<bool> QuitarAsync(string codUsuario, string codAplicacion, string codOpcion)
    {
        try
        {
            var resp = await _http.DeleteAsync(
                $"{Ruta}/{Uri.EscapeDataString(codUsuario)}/{Uri.EscapeDataString(codAplicacion)}/{Uri.EscapeDataString(codOpcion)}");
            if (resp.IsSuccessStatusCode) { InvalidarLogosCache(); OnCambio?.Invoke(); }
            return resp.IsSuccessStatusCode;
        }
        catch (Exception ex) { await _errorLog.LogAsync(ex, "Favoritos", nameof(QuitarAsync)); return false; }
    }
}
