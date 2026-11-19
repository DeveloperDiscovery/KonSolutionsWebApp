using System.Text.Json.Nodes;
using KonSolutionsInstalador;

// Instalador de la web (servicio de Windows KonSolutionsWeb). Sin argumentos: menu interactivo. Con argumentos (desatendido):
//   Instalador.exe --instalar --destino C:\KonSolutions\Web --puerto 443 [--http] [--certificado "nombre" | --pfx C:\cert.pfx --clave-pfx ...] --api https://servidor:5001/ [--direccion https://sistema.empresa.com]
//                  [--cuenta DOM\usu --clave-cuenta ...] [--sin-firewall] [--sin-iniciar]
//   Instalador.exe --desinstalar [--borrar-archivos --destino ...]      Instalador.exe --estado
var servicio = new Servicio("KonSolutionsWeb", "KonSolutionsWeb.exe", "KonSolutions Web", "Servidor de la aplicacion web KonSolutions (Blazor WebAssembly).");
const string Regla = "KonSolutions Web";
Console.OutputEncoding = System.Text.Encoding.UTF8;

string? Arg(string n) { var i = Array.IndexOf(args, n); return i >= 0 && i + 1 < args.Length && !args[i + 1].StartsWith("--") ? args[i + 1] : null; }
bool Tiene(string n) => args.Contains(n);

Console.ForegroundColor = ConsoleColor.Cyan; Console.WriteLine("KonSolutions - Instalador de la web (servicio de Windows KonSolutionsWeb)"); Console.ResetColor();
if (!OperatingSystem.IsWindows()) { Consola.Error("Este instalador es para Windows."); return 1; }
if (!EsAdministrador()) { Consola.Error("Ejecutalo como Administrador (clic derecho > Ejecutar como administrador)."); return 1; }

if (Tiene("--estado")) { Console.WriteLine("Servicio: " + servicio.Estado()); return 0; }
if (Tiene("--desinstalar")) return Desinstalar(Tiene("--borrar-archivos"), Arg("--destino"), false);
if (Tiene("--instalar")) return await Instalar(LeerArgs(), false);

while (true)
{
    Console.WriteLine();
    Console.WriteLine($"  Estado del servicio: {servicio.Estado()}");
    Console.WriteLine("  1) Instalar o actualizar");
    Console.WriteLine("  2) Desinstalar");
    Console.WriteLine("  3) Iniciar / reiniciar");
    Console.WriteLine("  4) Detener");
    Console.WriteLine("  5) Salir");
    switch (Consola.Preguntar("Elige", "1"))
    {
        case "1": await Instalar(Preguntar(), true); break;
        case "2": Desinstalar(Consola.Confirmar("Borrar tambien los archivos de la carpeta de instalacion?", false), null, true); break;
        case "3": servicio.Detener(); { var r = servicio.Iniciar(); if (r.Ok) Consola.Ok(r.Mensaje); else Consola.Error(r.Mensaje); } break;
        case "4": servicio.Detener(); Consola.Ok("Detenido: " + servicio.Estado()); break;
        case "5": return 0;
    }
}

static bool EsAdministrador() { using var id = System.Security.Principal.WindowsIdentity.GetCurrent(); return new System.Security.Principal.WindowsPrincipal(id).IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator); }
static string Normalizar(string url) { url = url.Trim(); return url == "" ? "" : url.TrimEnd('/'); }

Ajustes LeerArgs() => new()
{
    Destino = Arg("--destino") ?? @"C:\KonSolutions\Web", Https = !Tiene("--http"), Puerto = int.TryParse(Arg("--puerto"), out var p) ? p : (Tiene("--http") ? 80 : 443),
    Certificado = Arg("--certificado") ?? "", Pfx = Arg("--pfx") ?? "", ClavePfx = Arg("--clave-pfx") ?? "", Api = Arg("--api") ?? "", Direccion = Arg("--direccion") ?? "",
    Cuenta = Arg("--cuenta") ?? "", ClaveCuenta = Arg("--clave-cuenta") ?? "", Firewall = !Tiene("--sin-firewall"), Iniciar = !Tiene("--sin-iniciar")
};

Ajustes Preguntar()
{
    Consola.Titulo("Datos de la instalacion (Enter acepta lo que sale entre corchetes)");
    var a = new Ajustes();
    a.Destino = Consola.Preguntar("Carpeta de instalacion", a.Destino);
    var sitioCfg = JsonConfig.Leer(Path.Combine(a.Destino, "sitio", "appsettings.json"));
    a.Https = Consola.Confirmar("Usar HTTPS (recomendado)?", true);
    a.Puerto = int.TryParse(Consola.Preguntar("Puerto", a.Https ? "443" : "80"), out var pt) ? pt : (a.Https ? 443 : 80);
    if (a.Https)
    {
        var certs = Escucha.CertificadosDisponibles();
        if (certs.Count > 0)
        {
            Console.WriteLine("  Certificados de la maquina (Personal, con clave privada, vigentes):");
            for (int i = 0; i < certs.Count; i++) Console.WriteLine($"    {i + 1}) {certs[i].Asunto}   (vence {certs[i].Vence:yyyy-MM-dd})");
            Console.WriteLine("    0) Usar un archivo .pfx");
            if (int.TryParse(Consola.Preguntar("Elige el certificado", "1"), out var n) && n >= 1 && n <= certs.Count) a.Certificado = certs[n - 1].Asunto;
        }
        if (a.Certificado == "") { a.Pfx = Consola.Preguntar("Ruta del archivo .pfx"); a.ClavePfx = Consola.PreguntarOculto("Clave del .pfx"); }
    }
    var apiPrevia = JsonConfig.Texto(sitioCfg, "ApiBaseUrl");
    Console.WriteLine("  Direccion de la API, tal como la ve el NAVEGADOR de los usuarios (no localhost si abren desde otra PC), por ejemplo https://servidor:5001/");
    a.Api = Consola.Preguntar("Direccion de la API", apiPrevia == "" ? $"https://{Environment.MachineName.ToLowerInvariant()}:5001/" : apiPrevia);
    var puertoTxt = (a.Https && a.Puerto == 443) || (!a.Https && a.Puerto == 80) ? "" : ":" + a.Puerto;
    a.Direccion = Consola.Preguntar("Direccion con la que los usuarios abriran la web (para comprobar CORS)", $"{(a.Https ? "https" : "http")}://{Environment.MachineName.ToLowerInvariant()}{puertoTxt}");
    Console.WriteLine("  Cuenta con que corre el servicio (vacio = LocalSystem; la web solo sirve archivos, normalmente no hace falta otra).");
    a.Cuenta = Consola.Preguntar("Cuenta (DOMINIO\\usuario)", "");
    if (a.Cuenta != "") a.ClaveCuenta = Consola.PreguntarOculto("Clave de esa cuenta");
    a.Firewall = Consola.Confirmar($"Abrir el puerto {a.Puerto} en el Firewall de Windows?", true);
    return a;
}

async Task<int> Instalar(Ajustes a, bool interactivo)
{
    Consola.Titulo("1. Archivos de la web");
    var hostOrigen = Path.Combine(AppContext.BaseDirectory, "Host");
    var sitioOrigen = Path.Combine(AppContext.BaseDirectory, "Sitio");
    if (!File.Exists(Path.Combine(hostOrigen, servicio.Exe)) || !File.Exists(Path.Combine(sitioOrigen, "index.html"))) { Consola.Error($"No encuentro {servicio.Exe} en {hostOrigen} o index.html en {sitioOrigen}. Usa la carpeta que genera publicar.ps1 (Instalador.exe con las subcarpetas Host y Sitio)."); return 1; }
    a.Api = a.Api.Trim(); if (a.Api != "" && !a.Api.EndsWith('/')) a.Api += "/";
    var cfgSitio = Path.Combine(a.Destino, "sitio", "appsettings.json");
    if (a.Api == "") a.Api = JsonConfig.Texto(JsonConfig.Leer(cfgSitio), "ApiBaseUrl");
    if (a.Direccion == "") { var pt = (a.Https && a.Puerto == 443) || (!a.Https && a.Puerto == 80) ? "" : ":" + a.Puerto; a.Direccion = $"{(a.Https ? "https" : "http")}://{Environment.MachineName.ToLowerInvariant()}{pt}"; }

    Consola.Titulo("2. Comprobaciones");
    var fallas = 0;
    if (a.Api == "" || !Uri.TryCreate(a.Api, UriKind.Absolute, out _)) { Consola.Error("Falta la direccion de la API (por ejemplo https://servidor:5001/)."); fallas++; }
    else
    {
        var h = await Escucha.GetAsync(a.Api + "health", a.Direccion);
        if (h.Codigo == 200) Consola.Ok($"La API responde en {a.Api}");
        else Consola.Aviso($"Esta PC no pudo llamar a {a.Api}health ({(h.Codigo == 0 ? h.Error : "HTTP " + h.Codigo)}). Si la API todavia no esta instalada, instalala primero; si ya lo esta, revisa la direccion y el firewall.");
        if (h.Codigo == 200)
        {
            if (h.AllowOrigin == Normalizar(a.Direccion) || h.AllowOrigin == "*") Consola.Ok($"CORS: la API acepta llamadas desde {a.Direccion}");
            else Consola.Aviso($"CORS: la API NO acepta llamadas desde {a.Direccion}. En el instalador de la API agrega esa direccion en \"Direcciones de la web\" (o en appsettings.Production.json > Cors > Origenes).");
        }
        if (a.Https && a.Api.StartsWith("http://", StringComparison.OrdinalIgnoreCase)) Consola.Aviso("La web va por HTTPS pero la API por HTTP: el navegador bloquea esas llamadas (contenido mixto). Usa HTTPS en la API.");
    }
    if (a.Https)
    {
        if (a.Pfx != "") { if (File.Exists(a.Pfx)) Consola.Ok("Certificado .pfx encontrado."); else { Consola.Error($"No existe el archivo {a.Pfx}."); fallas++; } }
        else if (a.Certificado != "") { if (Escucha.CertificadosDisponibles().Any(c => c.Asunto == a.Certificado)) Consola.Ok($"Certificado '{a.Certificado}' disponible."); else { Consola.Error($"No encuentro un certificado vigente con clave privada llamado '{a.Certificado}' en el almacen de la maquina."); fallas++; } }
        else { Consola.Error("HTTPS necesita un certificado (--certificado o --pfx). Si no tienes, instala con HTTP."); fallas++; }
    }
    servicio.Detener();
    if (Escucha.PuertoLibre(a.Puerto)) Consola.Ok($"El puerto {a.Puerto} esta libre."); else { Consola.Error($"El puerto {a.Puerto} esta ocupado por otro programa (en el puerto 80/443 suele ser IIS u otro servidor web)."); fallas++; }
    if (fallas > 0 && interactivo && !Consola.Confirmar($"Hay {fallas} comprobacion(es) que fallaron. Instalar de todos modos?", false)) return 1;
    if (fallas > 0 && !interactivo) { Consola.Error("Instalacion cancelada por las comprobaciones."); return 1; }

    Consola.Titulo("3. Instalando");
    try
    {
        Directory.CreateDirectory(a.Destino);
        CopiarCarpeta(hostOrigen, a.Destino, saltar: "appsettings.Production.json");
        var sitioDestino = Path.Combine(a.Destino, "sitio");
        if (Directory.Exists(sitioDestino)) Directory.Delete(sitioDestino, true);          // los archivos de versiones anteriores (con hash en el nombre) no deben quedar
        CopiarCarpeta(sitioOrigen, sitioDestino);
        Consola.Ok($"Archivos copiados a {a.Destino} (servidor) y {sitioDestino} (sitio)");
        var cfgS = JsonConfig.Leer(cfgSitio); JsonConfig.Poner(cfgS, "ApiBaseUrl", a.Api); JsonConfig.Guardar(cfgSitio, cfgS);
        foreach (var viejo in new[] { cfgSitio + ".br", cfgSitio + ".gz" }) if (File.Exists(viejo)) File.Delete(viejo);   // las copias comprimidas de la publicacion quedaron con la direccion anterior
        Consola.Ok($"sitio\\appsettings.json: ApiBaseUrl = {a.Api}");
        var cfg = Path.Combine(a.Destino, "appsettings.Production.json");
        var prod = JsonConfig.Leer(cfg);
        Escucha.EscribirKestrel(prod, a.Https, a.Puerto, a.Certificado, a.Pfx, a.ClavePfx);
        JsonConfig.Guardar(cfg, prod);
        Consola.Ok("appsettings.Production.json escrito (puerto y certificado).");
        var crear = servicio.Crear(Path.Combine(a.Destino, servicio.Exe), a.Cuenta, a.ClaveCuenta);
        if (!crear.Ok) { Consola.Error(crear.Mensaje); return 1; }
        Consola.Ok(crear.Mensaje);
        if (a.Firewall) { var f = Escucha.AbrirFirewall(Regla, a.Puerto); if (f.Ok) Consola.Ok(f.Mensaje); else Consola.Aviso("No se pudo abrir el firewall: " + f.Mensaje); }
    }
    catch (Exception ex) { Consola.Error("No se pudo instalar: " + ex.Message); return 1; }

    if (a.Iniciar)
    {
        Consola.Titulo("4. Iniciando y verificando");
        var r = servicio.Iniciar();
        if (!r.Ok) { Consola.Error(r.Mensaje); return 1; }
        Consola.Ok(r.Mensaje);
        var baseUrl = $"{(a.Https ? "https" : "http")}://localhost:{a.Puerto}";
        var h = await Escucha.EsperarAsync(baseUrl + "/health", 40);
        if (!h.Ok) { Consola.Error(h.Mensaje); Consola.Info("Mira el Registro de eventos de Windows (Aplicacion)."); return 1; }
        Consola.Ok(h.Mensaje);
        var idx = await Escucha.GetAsync(baseUrl + "/");
        if (idx.Codigo == 200 && idx.Cuerpo.Contains("<base", StringComparison.OrdinalIgnoreCase)) Consola.Ok("La pagina principal carga (index.html de Blazor).");
        else { Consola.Error($"La pagina principal no es la de la aplicacion (HTTP {idx.Codigo}). Revisa que la carpeta sitio tenga lo publicado de KonSolutions.Web."); return 1; }
        var rutaApp = await Escucha.GetAsync(baseUrl + "/comercial/pedidos");
        if (rutaApp.Codigo == 200 && rutaApp.Cuerpo.Contains("<base", StringComparison.OrdinalIgnoreCase)) Consola.Ok("Las rutas de la aplicacion devuelven la pagina principal (se puede refrescar el navegador en cualquier pantalla).");
        else Consola.Aviso("Una ruta de la aplicacion no devolvio la pagina principal.");
        var cfgWeb = await Escucha.GetAsync(baseUrl + "/appsettings.json");
        if (cfgWeb.Codigo == 200 && cfgWeb.Cuerpo.Contains(a.Api)) Consola.Ok("La web ya apunta a la API: " + a.Api); else Consola.Aviso("appsettings.json del sitio no muestra la direccion de la API esperada.");
        Consola.Info($"Abre {a.Direccion} en el navegador.");
    }
    Console.WriteLine(); Consola.Ok("Listo.");
    return 0;
}

int Desinstalar(bool borrar, string? destino, bool interactivo)
{
    Consola.Titulo("Desinstalando");
    if (!servicio.Existe()) Consola.Aviso("El servicio no estaba instalado.");
    else { var r = servicio.Eliminar(); if (r.Ok) Consola.Ok(r.Mensaje); else { Consola.Error(r.Mensaje); return 1; } }
    Escucha.QuitarFirewall(Regla);
    var dir = destino ?? @"C:\KonSolutions\Web";
    if (interactivo && borrar) dir = Consola.Preguntar("Carpeta a borrar", dir);
    if (borrar && Directory.Exists(dir)) { try { Directory.Delete(dir, true); Consola.Ok("Archivos borrados: " + dir); } catch (Exception ex) { Consola.Aviso("No se pudo borrar la carpeta: " + ex.Message); } }
    return 0;
}

static void CopiarCarpeta(string origen, string destino, string? saltar = null)
{
    foreach (var d in Directory.GetDirectories(origen, "*", SearchOption.AllDirectories)) Directory.CreateDirectory(Path.Combine(destino, Path.GetRelativePath(origen, d)));
    Directory.CreateDirectory(destino);
    foreach (var f in Directory.GetFiles(origen, "*", SearchOption.AllDirectories))
    {
        var rel = Path.GetRelativePath(origen, f);
        if (saltar is not null && rel.Equals(saltar, StringComparison.OrdinalIgnoreCase) && File.Exists(Path.Combine(destino, rel))) continue;   // la configuracion de este servidor no se pisa
        File.Copy(f, Path.Combine(destino, rel), true);
    }
}

public class Ajustes
{
    public string Destino { get; set; } = @"C:\KonSolutions\Web";
    public bool Https { get; set; } = true;
    public int Puerto { get; set; } = 443;
    public string Certificado { get; set; } = "";
    public string Pfx { get; set; } = "";
    public string ClavePfx { get; set; } = "";
    public string Api { get; set; } = "";
    public string Direccion { get; set; } = "";
    public string Cuenta { get; set; } = "";
    public string ClaveCuenta { get; set; } = "";
    public bool Firewall { get; set; } = true;
    public bool Iniciar { get; set; } = true;
}
