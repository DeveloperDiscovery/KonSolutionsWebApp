using System.Net.Http.Json;
using KONSolutions.Shared.Common;

namespace KONSolutions.Web.Services;

/// <summary>Lecturas de las pantallas de operaciones, tiempos y rutas: devuelven los datos o el mensaje que dio la API.
/// Las escrituras usan <see cref="ProyectoApi"/> (mismo contrato: ok y mensaje).</summary>
public static class OperacionesApi
{
    public static async Task<(bool Ok, T? Data, string? Mensaje)> LeerAsync<T>(HttpClient http, string url)
    {
        try
        {
            using var resp = await http.GetAsync(url);
            ApiResponse<T>? r = null;
            try { r = await resp.Content.ReadFromJsonAsync<ApiResponse<T>>(); } catch { /* el cuerpo no era una respuesta de la API */ }
            if (resp.IsSuccessStatusCode && r is { Success: true }) return (true, r.Data, r.Message);
            var msg = string.IsNullOrWhiteSpace(r?.Message) ? $"No se pudo leer (HTTP {(int)resp.StatusCode})." : r!.Message;
            return (false, default, msg);
        }
        catch (Exception ex) { return (false, default, ex.Message); }
    }

    /// <summary>Escritura que devuelve un dato (por ejemplo la fila creada o cuantas se actualizaron).</summary>
    public static async Task<(bool Ok, T? Data, string? Mensaje)> EnviarAsync<T>(HttpClient http, HttpMethod metodo, string url, object? cuerpo = null)
    {
        try
        {
            using var req = new HttpRequestMessage(metodo, url);
            if (cuerpo is not null) req.Content = JsonContent.Create(cuerpo);
            using var resp = await http.SendAsync(req);
            ApiResponse<T>? r = null;
            try { r = await resp.Content.ReadFromJsonAsync<ApiResponse<T>>(); } catch { }
            if (resp.IsSuccessStatusCode && r is { Success: true }) return (true, r.Data, r.Message);
            var msg = string.IsNullOrWhiteSpace(r?.Message) ? $"La operación no se pudo completar (HTTP {(int)resp.StatusCode})." : r!.Message;
            return (false, default, msg);
        }
        catch (Exception ex) { return (false, default, ex.Message); }
    }

    /// <summary>"0.4500" -> "0.450": los minutos estándar con tres decimales y punto, sin depender de la cultura.</summary>
    public static string Min(decimal? v) => v is null ? "—" : v.Value.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture);
}
