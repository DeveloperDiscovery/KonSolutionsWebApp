using System.Collections;
using System.Globalization;
using System.Net.Http.Json;
using System.Reflection;
using KONSolutions.Shared.Common;
using KONSolutions.Shared.Models.Reportes;
using Microsoft.JSInterop;
using MudBlazor;

namespace KONSolutions.Web.Services;

/// <summary>Reporteador del lado del navegador.
///
/// AccionesBotones le pregunta, al hacer clic, si el botón tiene un reporte asignado: si lo tiene, el reporteador
/// lo atiende (diálogo de filtros, vista previa, descarga) y la pantalla no se entera. Por eso basta con que la
/// pantalla le pase a AccionesBotones la fila (o los datos de la pantalla) con el parámetro Fila.
///
/// Los botones con reporte de cada opción se piden una vez y quedan en memoria 5 minutos por opción y rol: en una
/// grilla hay un AccionesBotones por fila y no puede salir una consulta por fila.</summary>
public class ReportesWebService
{
    private static readonly TimeSpan Vigencia = TimeSpan.FromMinutes(5);
    private readonly HttpClient _http;
    private readonly MenuService _menu;
    private readonly IJSRuntime _js;
    private readonly ErrorLogService _log;
    private readonly Dictionary<string, (DateTime Hasta, Task<List<ReporteOpcionBoton>> Botones)> _cache = new();

    public ReportesWebService(HttpClient http, MenuService menu, IJSRuntime js, ErrorLogService log)
    {
        _http = http; _menu = menu; _js = js; _log = log;
    }

    /// <summary>El reporte del botón, o null si el botón no tiene reporte (o el rol no lo tiene habilitado).</summary>
    public async Task<ReporteOpcionBoton?> BotonAsync(string codAplicacion, string codOpcion, string codBoton)
    {
        if (string.IsNullOrWhiteSpace(codAplicacion) || string.IsNullOrWhiteSpace(codOpcion) || string.IsNullOrWhiteSpace(codBoton)) return null;
        var rol = _menu.RolActivo ?? "";
        var clave = $"{codAplicacion}|{codOpcion}|{rol}";
        if (!_cache.TryGetValue(clave, out var c) || c.Hasta < DateTime.Now || c.Botones.IsFaulted)
        {
            c = (DateTime.Now.Add(Vigencia), Cargar(codAplicacion, codOpcion, rol));
            _cache[clave] = c;
        }
        var lista = await c.Botones;
        return lista.FirstOrDefault(b => string.Equals(b.COD_BOTON_OPCION, codBoton, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>Olvida lo memorizado (al reasignar botones en la pantalla de asignación).</summary>
    public void Invalidar() => _cache.Clear();

    private async Task<List<ReporteOpcionBoton>> Cargar(string codAplicacion, string codOpcion, string rol)
    {
        try
        {
            var r = await _http.GetFromJsonAsync<ApiResponse<List<ReporteOpcionBoton>>>(ApiRoutes.Reportes.OpcionBotones(codAplicacion, codOpcion, rol));
            return r?.Data ?? new();
        }
        catch (Exception ex)
        {
            // Sin esta lista, los botones siguen funcionando como siempre (los atiende la pantalla).
            await _log.LogAsync(ex, nameof(ReportesWebService), nameof(Cargar));
            return new();
        }
    }

    /// <summary>Abre el diálogo del reporte del botón. "fila": la fila de la grilla, el registro del documento o un
    /// diccionario con los datos de la pantalla; sus campos llenan los parámetros que se llaman igual.</summary>
    public async Task AbrirAsync(IDialogService dialogos, string codAplicacion, string codOpcion, ReporteOpcionBoton boton, object? fila, string? titulo)
    {
        var parametros = new DialogParameters
        {
            ["CodAplicacion"] = codAplicacion,
            ["CodOpcionAplicacion"] = codOpcion,
            ["CodBotonOpcion"] = boton.COD_BOTON_OPCION,
            ["Contexto"] = Contexto(fila),
            ["Titulo"] = string.IsNullOrWhiteSpace(titulo) ? boton.DES_DISENO : titulo
        };
        await dialogos.ShowAsync<KONSolutions.Web.Pages.Reportes.ReporteEjecutarDialog>("", parametros,
            new DialogOptions { MaxWidth = MaxWidth.Medium, FullWidth = true, CloseButton = true, BackdropClick = false });
    }

    /// <summary>Los campos de la fila como texto, para mandarlos a la API: fechas "yyyy-MM-ddTHH:mm:ss", números con
    /// punto decimal, sí/no como 1/0. Acepta un diccionario o cualquier objeto (se leen sus propiedades públicas).</summary>
    public static Dictionary<string, string?> Contexto(object? fila)
    {
        var d = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        switch (fila)
        {
            case null:
                return d;
            case IDictionary<string, string?> s:
                foreach (var (k, v) in s) d[k] = v;
                return d;
            case IDictionary dic:
                foreach (DictionaryEntry e in dic)
                    if (e.Key is string k && Texto(e.Value) is var t) d[k] = t;
                return d;
        }
        foreach (var p in fila.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!p.CanRead || p.GetIndexParameters().Length > 0) continue;
            object? v;
            try { v = p.GetValue(fila); } catch { continue; }
            var t = Texto(v);
            if (t is not null) d[p.Name] = t;
        }
        return d;
    }

    private static string? Texto(object? v) => v switch
    {
        null              => null,
        string s          => s,
        DateTime f        => f.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture),
        DateTimeOffset o  => o.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture),
        DateOnly x        => x.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        bool b            => b ? "1" : "0",
        IFormattable f    => f.ToString(null, CultureInfo.InvariantCulture),
        _                 => null
    };

    /// <summary>Descarga el archivo en el navegador.</summary>
    public async Task DescargarAsync(ReporteArchivo a)
        => await _js.InvokeVoidAsync("blobViewer.descargar", a.CONTENIDO, a.DES_NOMBRE_ARCHIVO, a.DES_CONTENT_TYPE);

    /// <summary>POST que devuelve ApiResponse; el error de la API (o del SP) vuelve como mensaje para el usuario.</summary>
    public async Task<(T? Data, string? Error)> PostAsync<T>(string url, object cuerpo)
    {
        try
        {
            var r = await _http.PostAsJsonAsync(url, cuerpo);
            ApiResponse<T>? res = null;
            try { res = await r.Content.ReadFromJsonAsync<ApiResponse<T>>(); }
            catch (System.Text.Json.JsonException) { }
            if (res is { Success: true }) return (res.Data, null);
            return (default, !string.IsNullOrWhiteSpace(res?.Message) ? res!.Message
                           : $"La API respondió {(int)r.StatusCode} ({r.ReasonPhrase}).");
        }
        catch (Exception ex)
        {
            await _log.LogAsync(ex, nameof(ReportesWebService), nameof(PostAsync), url);
            return (default, "No se pudo comunicar con la API. Ver detalle en el log de errores.");
        }
    }

    /// <summary>GET que devuelve ApiResponse, con el mismo manejo de errores.</summary>
    public async Task<(T? Data, string? Error)> GetAsync<T>(string url)
    {
        try
        {
            var r = await _http.GetAsync(url);
            ApiResponse<T>? res = null;
            try { res = await r.Content.ReadFromJsonAsync<ApiResponse<T>>(); }
            catch (System.Text.Json.JsonException) { }
            if (res is { Success: true }) return (res.Data, null);
            return (default, !string.IsNullOrWhiteSpace(res?.Message) ? res!.Message
                           : $"La API respondió {(int)r.StatusCode} ({r.ReasonPhrase}).");
        }
        catch (Exception ex)
        {
            await _log.LogAsync(ex, nameof(ReportesWebService), nameof(GetAsync), url);
            return (default, "No se pudo comunicar con la API. Ver detalle en el log de errores.");
        }
    }

    /// <summary>DELETE que devuelve ApiResponse.</summary>
    public async Task<string?> DeleteAsync(string url)
    {
        try
        {
            var r = await _http.DeleteAsync(url);
            ApiResponse<object>? res = null;
            try { res = await r.Content.ReadFromJsonAsync<ApiResponse<object>>(); }
            catch (System.Text.Json.JsonException) { }
            if (res is { Success: true }) return null;
            return !string.IsNullOrWhiteSpace(res?.Message) ? res!.Message : $"La API respondió {(int)r.StatusCode} ({r.ReasonPhrase}).";
        }
        catch (Exception ex)
        {
            await _log.LogAsync(ex, nameof(ReportesWebService), nameof(DeleteAsync), url);
            return "No se pudo comunicar con la API. Ver detalle en el log de errores.";
        }
    }

    /// <summary>"Word" para DOC, etc.</summary>
    public static string NombreFormato(string? f) => (f ?? "").ToUpperInvariant() switch
    {
        "PDF" => "PDF", "DOC" => "Word", "XLS" => "Excel", "CSV" => "CSV", _ => f ?? ""
    };

    public static string IconoFormato(string? f) => (f ?? "").ToUpperInvariant() switch
    {
        "PDF" => Icons.Material.Filled.PictureAsPdf,
        "DOC" => Icons.Material.Filled.Description,
        "XLS" => Icons.Material.Filled.TableChart,
        "CSV" => Icons.Material.Filled.TextSnippet,
        _     => Icons.Material.Filled.InsertDriveFile
    };

    /// <summary>"NUM_PEDIDO_COMERCIAL" → "N° pedido comercial" (misma regla que la API).</summary>
    public static string EtiquetaDesdeNombre(string? nombre)
    {
        var n = (nombre ?? "").Trim().TrimStart('@').ToUpperInvariant();
        if (n.Length == 0) return "";
        (string pre, string texto)[] prefijos =
        {
            ("FEC_", "Fecha "), ("CAN_", "Cantidad "), ("IMP_", "Importe "), ("PRE_", "Precio "), ("TAS_", "Tasa "),
            ("NUM_", "N° "), ("COD_", "Código "), ("VAL_", "Valor "), ("POR_", "% "), ("DES_", ""), ("FLG_", "")
        };
        var inicio = "";
        foreach (var (pre, texto) in prefijos)
            if (n.StartsWith(pre, StringComparison.Ordinal) && n.Length > pre.Length) { inicio = texto; n = n[pre.Length..]; break; }
        var t = (inicio + n.Replace('_', ' ').ToLowerInvariant()).Trim();
        return t.Length == 0 ? "" : char.ToUpperInvariant(t[0]) + t[1..];
    }
}
