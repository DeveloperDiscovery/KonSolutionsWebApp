using System.Text.Json;
using System.Text.Json.Nodes;

namespace KonSolutionsInstalador;

/// <summary>Lee y edita archivos appsettings sin tocar el resto de sus secciones. Las rutas llevan dos puntos: "Kestrel:Endpoints:Https:Url".</summary>
public static class JsonConfig
{
    static readonly JsonSerializerOptions Opc = new() { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    public static JsonObject Leer(string archivo) => File.Exists(archivo)
        ? (JsonNode.Parse(File.ReadAllText(archivo), documentOptions: new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true }) as JsonObject ?? new JsonObject())
        : new JsonObject();

    public static void Guardar(string archivo, JsonObject raiz) { Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(archivo))!); File.WriteAllText(archivo, raiz.ToJsonString(Opc)); }

    public static void Poner(JsonObject raiz, string ruta, JsonNode? valor)
    {
        var partes = ruta.Split(':'); var o = raiz;
        for (int i = 0; i < partes.Length - 1; i++) { if (o[partes[i]] is not JsonObject h) { h = new JsonObject(); o[partes[i]] = h; } o = h; }
        o[partes[^1]] = valor;
    }

    public static void Quitar(JsonObject raiz, string ruta)
    {
        var partes = ruta.Split(':'); JsonNode? n = raiz;
        for (int i = 0; i < partes.Length - 1 && n is not null; i++) n = (n as JsonObject)?[partes[i]];
        (n as JsonObject)?.Remove(partes[^1]);
    }

    public static string Texto(JsonObject raiz, string ruta, string porOmision = "")
    {
        JsonNode? n = raiz; foreach (var p in ruta.Split(':')) n = (n as JsonObject)?[p];
        return n is JsonValue v && v.TryGetValue<string>(out var s) ? s : (n?.ToString() ?? porOmision);
    }

    public static string[] Lista(JsonObject raiz, string ruta)
    {
        JsonNode? n = raiz; foreach (var p in ruta.Split(':')) n = (n as JsonObject)?[p];
        return (n as JsonArray)?.Select(x => x?.GetValue<string>() ?? "").Where(x => x != "").ToArray() ?? Array.Empty<string>();
    }
}
