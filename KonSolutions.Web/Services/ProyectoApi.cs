using System.Net.Http.Json;
using System.Text.Json;
using KONSolutions.Shared.Common;

namespace KONSolutions.Web.Services;

/// <summary>Llamadas de escritura del expediente de proyectos: devuelven si salió bien y el mensaje que dio la API (los procedimientos explican en español por qué rechazan).</summary>
public static class ProyectoApi
{
    public static async Task<(bool Ok, string? Mensaje)> EnviarAsync(HttpClient http, HttpMethod metodo, string url, object? cuerpo = null)
    {
        using var req = new HttpRequestMessage(metodo, url);
        if (cuerpo is not null) req.Content = JsonContent.Create(cuerpo);
        using var resp = await http.SendAsync(req);
        string? msg = null;
        try
        {
            var r = await resp.Content.ReadFromJsonAsync<ApiResponse<JsonElement>>();
            msg = r?.Message;
        }
        catch { /* el cuerpo no era una respuesta de la API */ }
        if (!resp.IsSuccessStatusCode && string.IsNullOrWhiteSpace(msg)) msg = $"La operación no se pudo completar (HTTP {(int)resp.StatusCode}).";
        return (resp.IsSuccessStatusCode, msg);
    }

    public static Task<(bool Ok, string? Mensaje)> PostAsync(HttpClient http, string url, object? cuerpo = null) => EnviarAsync(http, HttpMethod.Post, url, cuerpo);
    public static Task<(bool Ok, string? Mensaje)> PutAsync(HttpClient http, string url, object? cuerpo = null) => EnviarAsync(http, HttpMethod.Put, url, cuerpo);
    public static Task<(bool Ok, string? Mensaje)> DeleteAsync(HttpClient http, string url) => EnviarAsync(http, HttpMethod.Delete, url);

    /// <summary>Alta (POST) o modificación (PUT) según sea nuevo.</summary>
    public static Task<(bool Ok, string? Mensaje)> GuardarAsync(HttpClient http, string url, object cuerpo, bool esNuevo) => esNuevo ? PostAsync(http, url, cuerpo) : PutAsync(http, url, cuerpo);

    /// <summary>Lleva una fase ("fases") o una tarea ("tareas") a su estado destino con un boton de dialogo; el destino y los estados de origen salen del flujo de la opcion.</summary>
    public static async Task<(bool Ok, string? Mensaje, KONSolutions.Shared.Models.Proyectos.ExpedienteEstadoResult? Estado)> CambiarEstadoAsync(HttpClient http, int num, string nivel, int cod, string app, string opcion, string boton, string? observacion = null)
    {
        using var req = new HttpRequestMessage(HttpMethod.Put, ApiRoutes.Proyectos.Exp(num, nivel == "" ? "estado" : $"{nivel}/{cod}/estado"));
        req.Content = JsonContent.Create(new { CodAplicacion = app, CodOpcionAplicacion = opcion, CodBotonOpcion = boton, Observacion = observacion });
        using var resp = await http.SendAsync(req);
        try
        {
            var r = await resp.Content.ReadFromJsonAsync<ApiResponse<KONSolutions.Shared.Models.Proyectos.ExpedienteEstadoResult>>();
            if (resp.IsSuccessStatusCode) return (true, r?.Message, r?.Data);
            return (false, string.IsNullOrWhiteSpace(r?.Message) ? $"La operación no se pudo completar (HTTP {(int)resp.StatusCode})." : r!.Message, null);
        }
        catch { return (resp.IsSuccessStatusCode, resp.IsSuccessStatusCode ? null : $"La operación no se pudo completar (HTTP {(int)resp.StatusCode}).", null); }
    }

    /// <summary>Los códigos de la opción de menú hija (Fase o Tarea) que lleva los botones y el flujo del diálogo; null si no está registrada (script 67).</summary>
    public static async Task<(string App, string Opcion)?> OpcionDialogoAsync(MenuService menu, string formulario)
    {
        try
        {
            var padre = menu.BuscarCodigosPorFormulario("Proyectos/Proyectos.razor");
            if (padre is null) return null;
            var c = await menu.BuscarCodigosPorFormularioAsync(padre.Value.CodAplicacion, formulario);
            return c is null ? null : (c.Value.CodAplicacion, c.Value.CodOpcionAplicacion);
        }
        catch { return null; }
    }

    /// <summary>Recalcula avance, fechas y costo del proyecto después de un cambio.</summary>
    public static Task<(bool Ok, string? Mensaje)> RecalcularAsync(HttpClient http, int num, DateTime? fecReal = null)
        => PutAsync(http, ApiRoutes.Proyectos.Exp(num, "recalcular") + (fecReal is { } f ? $"?fecReal={f:yyyy-MM-dd}" : ""));

    /// <summary>El texto de la situación calculada (TER, POR, ATR, CUR, PEN).</summary>
    public static string Situacion(string? cod) => cod switch
    {
        "TER" => "Terminada", "POR" => "Por confirmar", "ATR" => "Atrasada", "CUR" => "En curso", _ => "Pendiente"
    };

    public static string Nivel(string? cod) => cod switch { "ALT" => "Alto", "MED" => "Medio", _ => "Bajo" };

    /// <summary>Días corridos de un rango, contando el primero y el último (1/ene a 1/ene = 1 día). Null si falta una fecha o el rango es inválido.</summary>
    public static int? Dias(DateTime? ini, DateTime? fin) => ini is null || fin is null || fin.Value.Date < ini.Value.Date ? null : (fin.Value.Date - ini.Value.Date).Days + 1;
    /// <summary>Fecha final de un rango que empieza en <paramref name="ini"/> y dura <paramref name="dias"/> días (contando el primero).</summary>
    public static DateTime FinPor(DateTime ini, int dias) => ini.Date.AddDays(Math.Max(1, dias) - 1);

    public static string Fecha(DateTime? f) => f?.ToString("dd/MM/yyyy") ?? "—";
}
