namespace KONSolutions.Web.Services;

/// <summary>Las rutas de la API de las bandejas de XML recibidos (Comercial) y de guías recibidas (Logística). Son de consulta; tienen su propio permiso, aparte del Registro de Compras y de Movimientos de Almacén.</summary>
public static class BandejaRutas
{
    public const string XmlRecibidos = "api/v1/comercial/xml-recibidos";
    public const string GuiasRecibidas = "api/v1/logistica/guias-recibidas";

    public static string XmlRecibido(decimal numCpe, string? parte = null) => $"{XmlRecibidos}/{numCpe:0}" + (parte is null ? "" : "/" + parte);
    public static string GuiaRecibida(decimal numGre, string? parte = null) => $"{GuiasRecibidas}/{numGre:0}" + (parte is null ? "" : "/" + parte);

    /// <summary>«Completar datos»: de todos los pendientes de la bandeja (sin número) o solo del documento indicado.</summary>
    public static string CompletarDatos(string ruta, decimal? num = null) => num is { } n ? $"{ruta}/{n:0}/completar-datos" : $"{ruta}/completar-datos";

    /// <summary>«Validar con SUNAT»: los comprobantes sin resultado (o cuya consulta falló), o solo el indicado.</summary>
    public static string ValidarSunat(decimal? num = null, string ruta = XmlRecibidos) => num is { } n ? $"{ruta}/{n:0}/validar-sunat" : $"{ruta}/validar-sunat";

    /// <summary>La consulta con sus filtros (los que van vacíos no se mandan). Las fechas son de emisión.</summary>
    public static string Consulta(string ruta, string? texto, string? estado, DateTime? desde, DateTime? hasta, string? tipo = null, string? nombreTipo = null, int filas = 500)
    {
        var q = new List<string>();
        void Pon(string k, string? v) { if (!string.IsNullOrWhiteSpace(v)) q.Add($"{k}={Uri.EscapeDataString(v.Trim())}"); }
        Pon("SEARCH", texto); Pon("COD_ESTADO_USO", estado);
        if (desde is { } d) q.Add($"FEC_DESDE={d:yyyy-MM-dd}");
        if (hasta is { } h) q.Add($"FEC_HASTA={h:yyyy-MM-dd}");
        if (!string.IsNullOrWhiteSpace(tipo) && !string.IsNullOrWhiteSpace(nombreTipo)) Pon(nombreTipo, tipo);
        q.Add($"NUM_FILAS={filas}");
        return ruta + "?" + string.Join("&", q);
    }
}
