using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;

// Sirve la web (Blazor WebAssembly publicada) desde la carpeta «sitio» al lado del .exe (o Sitio:Carpeta de appsettings.json).
// Puerto, HTTP/HTTPS y certificado: seccion Kestrel de appsettings.Production.json (lo escribe el instalador).
var builder = WebApplication.CreateBuilder(new WebApplicationOptions { Args = args, ContentRootPath = AppContext.BaseDirectory });
builder.Host.UseWindowsService(o => o.ServiceName = "KonSolutionsWeb");
var app = builder.Build();

var carpeta = Path.GetFullPath(builder.Configuration["Sitio:Carpeta"] ?? "sitio", AppContext.BaseDirectory);
if (!Directory.Exists(carpeta)) { app.Logger.LogCritical("No existe la carpeta del sitio: {Carpeta}. Copia ahi lo publicado de KonSolutions.Web (wwwroot).", carpeta); return 1; }
var archivos = new PhysicalFileProvider(carpeta);
var tipos = new FileExtensionContentTypeProvider();
tipos.Mappings[".wasm"] = "application/wasm"; tipos.Mappings[".dat"] = "application/octet-stream"; tipos.Mappings[".blat"] = "application/octet-stream";
tipos.Mappings[".dll"] = "application/octet-stream"; tipos.Mappings[".pdb"] = "application/octet-stream"; tipos.Mappings[".webmanifest"] = "application/manifest+json"; tipos.Mappings[".woff2"] = "font/woff2";

app.Use(async (ctx, next) =>
{
    ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";
    ctx.Response.Headers["Referrer-Policy"] = "same-origin";
    await next();
});

app.MapGet("/health", () => Results.Ok(new { status = "Healthy", sitio = carpeta }));

// La publicacion de Blazor deja cada archivo grande tambien como .br y .gz: se entrega ya comprimido si el navegador lo acepta (el .wasm de ~10 MB baja a ~3 MB).
app.Use(async (ctx, next) =>
{
    if (!HttpMethods.IsGet(ctx.Request.Method) && !HttpMethods.IsHead(ctx.Request.Method)) { await next(); return; }
    var ruta = ctx.Request.Path.Value ?? "/";
    if (ruta.Contains("..") || ruta.EndsWith('/')) { await next(); return; }
    if (!tipos.TryGetContentType(ruta, out var tipo)) { await next(); return; }   // sin extension conocida (rutas de la aplicacion): no es un archivo
    var acepta = ctx.Request.Headers.AcceptEncoding.ToString();
    foreach (var (enc, ext) in new[] { ("br", ".br"), ("gzip", ".gz") })
    {
        if (!acepta.Contains(enc, StringComparison.OrdinalIgnoreCase)) continue;
        var comp = archivos.GetFileInfo(ruta.TrimStart('/') + ext);
        if (!comp.Exists || comp.PhysicalPath is null) continue;
        // si el archivo original se edito despues de generar el comprimido (por ejemplo appsettings.json con la direccion de la API), el comprimido esta viejo: se sirve el original
        var original = archivos.GetFileInfo(ruta.TrimStart('/'));
        if (original.Exists && original.LastModified > comp.LastModified.AddSeconds(2)) continue;
        ctx.Response.Headers.ContentEncoding = enc; ctx.Response.Headers.Vary = "Accept-Encoding"; ctx.Response.ContentType = tipo;
        ctx.Response.Headers.CacheControl = NoCache(ruta) ? "no-cache" : "public, max-age=0, must-revalidate";
        ctx.Response.ContentLength = comp.Length;
        if (HttpMethods.IsGet(ctx.Request.Method)) await ctx.Response.SendFileAsync(comp);
        return;
    }
    await next();
});

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = archivos, ContentTypeProvider = tipos, ServeUnknownFileTypes = false,
    OnPrepareResponse = c => { if (NoCache(c.Context.Request.Path.Value ?? "")) c.Context.Response.Headers.CacheControl = "no-cache"; }
});

// las rutas de la aplicacion (/comercial/pedidos...) no son archivos: devuelven index.html y Blazor decide la pantalla. Un archivo que falta (.js, .css, .dll) si es 404.
app.MapFallback(async ctx =>
{
    if (Path.HasExtension(ctx.Request.Path.Value ?? "")) { ctx.Response.StatusCode = 404; return; }
    var index = archivos.GetFileInfo("index.html");
    if (!index.Exists) { ctx.Response.StatusCode = 404; return; }
    ctx.Response.ContentType = "text/html; charset=utf-8"; ctx.Response.Headers.CacheControl = "no-cache";
    await ctx.Response.SendFileAsync(index);
});

app.Run();
return 0;

// el punto de entrada y la configuracion cambian con cada version: nunca se guardan en cache sin revalidar
static bool NoCache(string ruta) => ruta.EndsWith("index.html", StringComparison.OrdinalIgnoreCase) || ruta.EndsWith(".json", StringComparison.OrdinalIgnoreCase) || ruta.EndsWith("service-worker.js", StringComparison.OrdinalIgnoreCase);
