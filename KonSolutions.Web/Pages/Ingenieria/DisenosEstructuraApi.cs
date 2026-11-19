using System.Net.Http.Json;
using KONSolutions.Shared.Common;

namespace KONSolutions.Web.Pages.Ingenieria;

/// <summary>Llamadas de escritura de la estructura de diseno: devuelven (Ok, mensaje) en vez de lanzar, para
/// mostrar el motivo que da la API (version bloqueada, ciclo, mas de 10 niveles, etc.).</summary>
public static class DisenosEstructuraApi
{
    public static async Task<(bool Ok, string Mensaje, T? Data)> PostAsync<T>(this HttpClient http, string url, object dto)
    {
        try
        {
            var resp = await http.PostAsJsonAsync(url, dto);
            return await LeerAsync<T>(resp);
        }
        catch (Exception ex) { return (false, ex.Message, default); }
    }

    public static async Task<(bool Ok, string Mensaje, T? Data)> DeleteAsync<T>(this HttpClient http, string url)
    {
        try { return await LeerAsync<T>(await http.DeleteAsync(url)); }
        catch (Exception ex) { return (false, ex.Message, default); }
    }

    private static async Task<(bool, string, T?)> LeerAsync<T>(HttpResponseMessage resp)
    {
        ApiResponse<T>? body = null;
        try { body = await resp.Content.ReadFromJsonAsync<ApiResponse<T>>(); } catch { }
        if (!resp.IsSuccessStatusCode || body is null || !body.Success)
            return (false, string.IsNullOrWhiteSpace(body?.Message) ? $"Error {(int)resp.StatusCode}" : body!.Message, default);
        return (true, body.Message ?? "", body.Data);
    }
}
