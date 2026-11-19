using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using KonSolutions.Agente.Configuracion;
using KonSolutions.Agente.Util;
using KONSolutions.Shared.Common;
using KONSolutions.Shared.Models.Rfid;

namespace KonSolutions.Agente.Api;

/// <summary>Cliente HTTP de la API de KonSolutions: inicia sesion una vez (JWT), reutiliza el token hasta que le falte poco y, si la API responde 401,
/// vuelve a iniciar sesion UNA vez y reintenta. No inicia sesion en cada llamada: el login de la API admite 5 intentos por IP por minuto.</summary>
public sealed class ApiRfidCliente : IApiRfid
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private static readonly TimeSpan Margen = TimeSpan.FromSeconds(60);

    private readonly HttpClient _http;
    private readonly ApiOpciones _api;
    private readonly Func<string?> _password;
    private readonly IBitacora _log;
    private readonly Func<DateTime> _ahora;
    private readonly SemaphoreSlim _sesion = new(1, 1);
    private string? _token;
    private DateTime _vence = DateTime.MinValue;

    public ApiRfidCliente(HttpClient http, ApiOpciones api, Func<string?> password, IBitacora log, Func<DateTime>? ahora = null)
    {
        _http = http; _api = api; _password = password; _log = log; _ahora = ahora ?? (() => DateTime.UtcNow);
        if (_http.BaseAddress is null && Uri.TryCreate(api.BaseUrl, UriKind.Absolute, out var b))
            _http.BaseAddress = new Uri(b.AbsoluteUri.TrimEnd('/') + "/");
    }

    /// <summary>Crea el HttpClient del agente (con el timeout y, solo si se pidio, aceptando certificados no confiables).</summary>
    public static HttpClient CrearHttp(ApiOpciones api)
    {
        var handler = new HttpClientHandler();
        if (api.AceptarCertificadoInvalido) handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
        return new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(api.TimeoutSegundos) };
    }

    // ───────── sesion ─────────

    /// <summary>Inicia sesion si hace falta. Devuelve null si quedo con sesion, o el motivo si no se pudo.</summary>
    public async Task<string?> IniciarSesionAsync(CancellationToken ct, bool forzar = false)
    {
        await _sesion.WaitAsync(ct);
        try
        {
            if (!forzar && _token is not null && _ahora() < _vence - Margen) return null;
            var pass = _password();
            if (string.IsNullOrEmpty(pass)) return "No hay contraseña para iniciar sesión.";
            try
            {
                using var resp = await _http.PostAsJsonAsync("api/v1/auth/login", new { COD_USUARIO = _api.Usuario, COD_PASSWORD = pass }, Json, ct);
                if (resp.StatusCode == HttpStatusCode.TooManyRequests) return "La API limitó los intentos de inicio de sesión (5 por minuto). Espera un minuto y vuelve a probar.";
                var cuerpo = await resp.Content.ReadAsStringAsync(ct);
                if (!resp.IsSuccessStatusCode) return "No se pudo iniciar sesión: " + MensajeDe(cuerpo, resp.StatusCode);
                var r = JsonSerializer.Deserialize<ApiResponse<LoginDatos>>(cuerpo, Json);
                if (r is null || !r.Success || string.IsNullOrWhiteSpace(r.Data?.Token)) return "La API no devolvió un token: " + (r?.Message ?? "respuesta vacía");
                if (r.Data.Usuario?.FLG_CHANGE_PASSSWORD == true) return $"El usuario {_api.Usuario} tiene que cambiar su contraseña antes de poder usarse en el agente.";
                _token = r.Data.Token;
                _vence = _ahora().AddSeconds(r.Data.ExpiresIn > 0 ? r.Data.ExpiresIn : 600);
                _log.Info($"Sesión iniciada como {_api.Usuario} (el token dura {(r.Data.ExpiresIn > 0 ? r.Data.ExpiresIn / 60 : 10)} min).");
                return null;
            }
            catch (Exception ex) when ((ex is HttpRequestException or TaskCanceledException or JsonException) && !ct.IsCancellationRequested)
            {
                return "No pude hablar con la API para iniciar sesión: " + ex.Message;
            }
        }
        finally { _sesion.Release(); }
    }

    private sealed class LoginDatos
    {
        public string? Token { get; set; }
        public int ExpiresIn { get; set; }
        public LoginUsuario? Usuario { get; set; }
    }
    private sealed class LoginUsuario { public bool FLG_CHANGE_PASSSWORD { get; set; } }

    // ───────── llamadas ─────────

    public Task<RespuestaApi<List<RfidEquipo>>> EquiposAsync(CancellationToken ct)
        => EnviarAsync<List<RfidEquipo>>(HttpMethod.Get, "api/v1/rfid/equipos", null, ct);

    public Task<RespuestaApi<List<RfidEmision>>> EmisionesAsync(string codEstadoEmision, CancellationToken ct)
        => EnviarAsync<List<RfidEmision>>(HttpMethod.Get, $"api/v1/rfid/emisiones/search?COD_ESTADO_EMISION={Uri.EscapeDataString(codEstadoEmision)}", null, ct);

    public Task<RespuestaApi<RfidEmision>> EmisionAsync(int numEmision, CancellationToken ct)
        => EnviarAsync<RfidEmision>(HttpMethod.Get, $"api/v1/rfid/emisiones/{numEmision}", null, ct);

    public Task<RespuestaApi<RfidEnrolado>> EnrolarAsync(int numEmision, string tid, string? codProductoEquipo, CancellationToken ct)
        => EnviarAsync<RfidEnrolado>(HttpMethod.Post, $"api/v1/rfid/emisiones/{numEmision}/enrolar", new RfidEnrolarDto { COD_TID = tid, COD_PRODUCTO_EQUIPO = codProductoEquipo }, ct);

    public Task<RespuestaApi<object>> GrabadaAsync(string tid, string? epcLeido, CancellationToken ct)
        => EnviarAsync<object>(HttpMethod.Put, "api/v1/rfid/etiquetas/grabada", new RfidGrabadaDto { COD_TID = tid, COD_EPC_LEIDO = epcLeido }, ct);

    public Task<RespuestaApi<RfidEtiquetaAnulada>> AnularEtiquetaAsync(int numEmision, int numItem, int numEtiqueta, string motivo, bool reponer, CancellationToken ct)
        => EnviarAsync<RfidEtiquetaAnulada>(HttpMethod.Put, $"api/v1/rfid/emisiones/{numEmision}/etiquetas/{numItem}/{numEtiqueta}/anular",
            new RfidEtiquetaAnularDto { DES_MOTIVO_ANULACION = motivo, FLG_REPONER = reponer }, ct);

    public Task<RespuestaApi<RfidTrabajoImpresion?>> ReclamarTrabajoAsync(string codProductoEquipo, CancellationToken ct)
        => EnviarAsync<RfidTrabajoImpresion?>(HttpMethod.Post, "api/v1/rfid/impresion/reclamar", new RfidTrabajoReclamarDto { COD_PRODUCTO_EQUIPO = codProductoEquipo }, ct);

    public Task<RespuestaApi<RfidTrabajoEstado>> ProgresoTrabajoAsync(int numTrabajo, int impresas, int fallidas, string? mensaje, CancellationToken ct)
        => EnviarAsync<RfidTrabajoEstado>(HttpMethod.Put, $"api/v1/rfid/impresion/{numTrabajo}/progreso",
            new RfidTrabajoProgresoDto { CAN_IMPRESAS = impresas, CAN_FALLIDAS = fallidas, DES_MENSAJE = mensaje }, ct);

    public Task<RespuestaApi<object>> TerminarTrabajoAsync(int numTrabajo, string estado, int impresas, int fallidas, string? mensaje, CancellationToken ct)
        => EnviarAsync<object>(HttpMethod.Put, $"api/v1/rfid/impresion/{numTrabajo}/terminar",
            new RfidTrabajoTerminarDto { COD_ESTADO_TRABAJO = estado, CAN_IMPRESAS = impresas, CAN_FALLIDAS = fallidas, DES_MENSAJE = mensaje }, ct);

    private async Task<RespuestaApi<T>> EnviarAsync<T>(HttpMethod metodo, string ruta, object? cuerpo, CancellationToken ct)
    {
        var motivo = await IniciarSesionAsync(ct);
        if (motivo is not null) return RespuestaApi<T>.Fallo(motivo, 0);
        var r = await UnaVezAsync<T>(metodo, ruta, cuerpo, ct);
        if (r.Estado != (int)HttpStatusCode.Unauthorized) return r;
        // el token ya no sirve (vencio antes de lo previsto o la API se reinicio con otra clave): una sola vez se vuelve a entrar
        motivo = await IniciarSesionAsync(ct, forzar: true);
        if (motivo is not null) return RespuestaApi<T>.Fallo(motivo, (int)HttpStatusCode.Unauthorized);
        return await UnaVezAsync<T>(metodo, ruta, cuerpo, ct);
    }

    private async Task<RespuestaApi<T>> UnaVezAsync<T>(HttpMethod metodo, string ruta, object? cuerpo, CancellationToken ct)
    {
        try
        {
            using var req = new HttpRequestMessage(metodo, ruta);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            if (cuerpo is not null) req.Content = JsonContent.Create(cuerpo, options: Json);
            using var resp = await _http.SendAsync(req, ct);
            var texto = await resp.Content.ReadAsStringAsync(ct);
            var estado = (int)resp.StatusCode;
            ApiResponse<T>? r = null;
            try { r = string.IsNullOrWhiteSpace(texto) ? null : JsonSerializer.Deserialize<ApiResponse<T>>(texto, Json); } catch (JsonException) { }
            if (resp.IsSuccessStatusCode && r is { Success: true }) return new RespuestaApi<T>(true, r.Data, r.Message ?? string.Empty, estado);
            return new RespuestaApi<T>(false, default, r is { Message.Length: > 0 } ? r.Message : MensajeDe(texto, resp.StatusCode), estado);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException && !ct.IsCancellationRequested)
        {
            return RespuestaApi<T>.Fallo("No pude hablar con la API: " + ex.Message, 0);
        }
    }

    /// <summary>El mensaje de una respuesta que no es el sobre normal de la API (un 403, un 429, un error de validacion...).</summary>
    private static string MensajeDe(string cuerpo, HttpStatusCode estado)
    {
        if (!string.IsNullOrWhiteSpace(cuerpo))
        {
            try
            {
                using var doc = JsonDocument.Parse(cuerpo);
                foreach (var nombre in new[] { "message", "Message", "title", "detail" })
                    if (doc.RootElement.ValueKind == JsonValueKind.Object && doc.RootElement.TryGetProperty(nombre, out var v) && v.ValueKind == JsonValueKind.String && v.GetString() is { Length: > 0 } s)
                        return s;
            }
            catch (JsonException) { }
            var corto = cuerpo.Trim();
            if (corto.Length > 200) corto = corto[..200] + "…";
            if (!corto.StartsWith('<')) return $"HTTP {(int)estado}: {corto}";
        }
        return estado switch
        {
            HttpStatusCode.Unauthorized => "HTTP 401: la API no aceptó la sesión.",
            HttpStatusCode.Forbidden => "HTTP 403: el usuario del agente no tiene permiso para esta operación.",
            HttpStatusCode.TooManyRequests => "HTTP 429: la API limitó las peticiones.",
            _ => $"HTTP {(int)estado} {estado}",
        };
    }
}
