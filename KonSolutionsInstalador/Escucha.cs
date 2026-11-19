using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json.Nodes;

namespace KonSolutionsInstalador;

public record Certificado(string Asunto, string Huella, DateTime Vence);

/// <summary>Como escucha un servicio web (Kestrel): puerto, HTTP o HTTPS y el certificado. Todo se escribe en appsettings.Production.json, que Kestrel lee solo.</summary>
public static class Escucha
{
    /// <summary>Certificados de la maquina (almacen Personal) con clave privada y vigentes, para elegir uno.</summary>
    public static List<Certificado> CertificadosDisponibles()
    {
        var lista = new List<Certificado>();
        try
        {
            using var s = new X509Store(StoreName.My, StoreLocation.LocalMachine); s.Open(OpenFlags.ReadOnly);
            foreach (var c in s.Certificates.Where(c => c.HasPrivateKey && c.NotAfter > DateTime.Now && c.NotBefore < DateTime.Now).OrderByDescending(c => c.NotAfter))
                lista.Add(new Certificado(c.GetNameInfo(X509NameType.SimpleName, false), c.Thumbprint, c.NotAfter));
        }
        catch { }
        return lista;
    }

    /// <summary>Escribe la seccion Kestrel. HTTP: una URL. HTTPS: la URL y el certificado del almacen (por su nombre) o de un archivo .pfx (ruta y clave).</summary>
    public static void EscribirKestrel(JsonObject raiz, bool https, int puerto, string? asuntoCertificado, string? pfx, string? clavePfx)
    {
        JsonConfig.Quitar(raiz, "Kestrel");
        if (!https) { JsonConfig.Poner(raiz, "Kestrel:Endpoints:Http:Url", $"http://0.0.0.0:{puerto}"); return; }
        JsonConfig.Poner(raiz, "Kestrel:Endpoints:Https:Url", $"https://0.0.0.0:{puerto}");
        if (!string.IsNullOrWhiteSpace(pfx))
        {
            JsonConfig.Poner(raiz, "Kestrel:Endpoints:Https:Certificate:Path", pfx);
            JsonConfig.Poner(raiz, "Kestrel:Endpoints:Https:Certificate:Password", clavePfx ?? "");
        }
        else
        {
            JsonConfig.Poner(raiz, "Kestrel:Endpoints:Https:Certificate:Subject", asuntoCertificado);
            JsonConfig.Poner(raiz, "Kestrel:Endpoints:Https:Certificate:Store", "My");
            JsonConfig.Poner(raiz, "Kestrel:Endpoints:Https:Certificate:Location", "LocalMachine");
            JsonConfig.Poner(raiz, "Kestrel:Endpoints:Https:Certificate:AllowInvalid", false);
        }
    }

    public static bool PuertoLibre(int puerto)
    {
        try { var l = new TcpListener(IPAddress.Any, puerto); l.Start(); l.Stop(); return true; } catch { return false; }
    }

    public static (bool Ok, string Mensaje) AbrirFirewall(string nombreRegla, int puerto)
    {
        Servicio.Ejecutar("netsh", "advfirewall", "firewall", "delete", "rule", $"name={nombreRegla}");
        var (c, s) = Servicio.Ejecutar("netsh", "advfirewall", "firewall", "add", "rule", $"name={nombreRegla}", "dir=in", "action=allow", "protocol=TCP", $"localport={puerto}");
        return (c == 0, c == 0 ? $"Regla de firewall '{nombreRegla}' (TCP {puerto})." : s);
    }

    public static void QuitarFirewall(string nombreRegla) => Servicio.Ejecutar("netsh", "advfirewall", "firewall", "delete", "rule", $"name={nombreRegla}");

    /// <summary>GET a una URL sin validar el certificado (es una prueba local del propio servicio). Devuelve el codigo HTTP y los encabezados que interesan.</summary>
    public static async Task<(int Codigo, string Cuerpo, string? AllowOrigin, string? Error)> GetAsync(string url, string? origen = null, int segundos = 10)
    {
        try
        {
            using var h = new HttpClientHandler { ServerCertificateCustomValidationCallback = (_, _, _, _) => true };
            using var c = new HttpClient(h) { Timeout = TimeSpan.FromSeconds(segundos) };
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            if (origen is not null) req.Headers.TryAddWithoutValidation("Origin", origen);
            using var r = await c.SendAsync(req);
            var cuerpo = await r.Content.ReadAsStringAsync();
            r.Headers.TryGetValues("Access-Control-Allow-Origin", out var ao);
            return ((int)r.StatusCode, cuerpo, ao?.FirstOrDefault(), null);
        }
        catch (Exception ex) { return (0, "", null, ex.Message); }
    }

    /// <summary>Espera a que la URL conteste 200 (el servicio tarda unos segundos en arrancar).</summary>
    public static async Task<(bool Ok, string Mensaje)> EsperarAsync(string url, int segundos)
    {
        var fin = DateTime.UtcNow.AddSeconds(segundos); string ultimo = "";
        while (DateTime.UtcNow < fin)
        {
            var r = await GetAsync(url, null, 5);
            if (r.Codigo == 200) return (true, $"Responde {url}");
            ultimo = r.Error ?? $"HTTP {r.Codigo}";
            await Task.Delay(1500);
        }
        return (false, $"No respondio {url} en {segundos} segundos ({ultimo}).");
    }
}
