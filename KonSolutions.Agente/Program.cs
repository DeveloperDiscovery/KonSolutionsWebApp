using System.Text;
using KonSolutions.Agente;
using KonSolutions.Agente.Servicio;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

Console.OutputEncoding = new UTF8Encoding(false);

// «agente-rfid servicio»: corre como servicio de Windows (o en consola si se lanza a mano) y atiende la cola de impresion de la web hasta que lo detengan.
if (args.Length > 0 && args[0].Equals("servicio", StringComparison.OrdinalIgnoreCase))
{
    // un servicio de Windows arranca con la carpeta System32 como actual: las rutas relativas (appsettings.json, logs) se resuelven junto al programa
    Directory.SetCurrentDirectory(AppContext.BaseDirectory);
    var builder = Host.CreateApplicationBuilder(args);
    builder.Services.AddWindowsService(o => o.ServiceName = "AgenteRFID");
    builder.Logging.ClearProviders();                                  // el agente lleva su propia bitacora (consola y logs\)
    builder.Services.AddSingleton(args);
    builder.Services.AddHostedService<TrabajadorServicio>();
    builder.Services.Configure<HostOptions>(o => o.ShutdownTimeout = TimeSpan.FromSeconds(45));   // da tiempo a terminar la etiqueta que esta en la impresora (25 s) y avisar a la API
    using var anfitrion = builder.Build();
    await anfitrion.RunAsync();
    return Environment.ExitCode;
}

using var cancelacion = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    if (cancelacion.IsCancellationRequested) return;   // el segundo Ctrl+C corta de verdad
    e.Cancel = true;
    Console.WriteLine("Terminando (Ctrl+C otra vez para cortar ya)…");
    cancelacion.Cancel();
};
return await Cli.EjecutarAsync(args, new Entorno(), cancelacion.Token);
