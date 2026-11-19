using System.Text;
using Microsoft.Extensions.Hosting;

namespace KonSolutions.Agente.Servicio;

/// <summary>Lo que corre cuando el programa se arranca como <c>agente-rfid servicio</c>: el anfitrion de .NET (servicio de Windows, o consola si se lanza a mano) que ejecuta el comando
/// «servicio» hasta que lo detengan (parar el servicio, Ctrl+C). Lo que el comando escribe en pantalla se guarda tambien en logs\\servicio-salida.log, porque un servicio no tiene consola
/// y, por ejemplo, un appsettings.json que falta de otro modo no dejaria rastro.</summary>
public sealed class TrabajadorServicio : BackgroundService
{
    private readonly string[] _args;
    private readonly IHostApplicationLifetime _vida;
    public TrabajadorServicio(string[] args, IHostApplicationLifetime vida) { _args = args; _vida = vida; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "logs"));
            using var salida = new SalidaDoble(Console.Out, Path.Combine(AppContext.BaseDirectory, "logs", "servicio-salida.log"));
            Environment.ExitCode = await Cli.EjecutarAsync(_args, new Entorno { Salida = salida }, stoppingToken);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            Environment.ExitCode = Cli.ConFallos;
            try { File.AppendAllText(Path.Combine(AppContext.BaseDirectory, "logs", "servicio-salida.log"), $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} El servicio se detuvo por un error: {ex}{Environment.NewLine}"); } catch { }
            Console.Error.WriteLine("El servicio se detuvo por un error: " + ex.Message);
        }
        finally { _vida.StopApplication(); }   // si el comando termina solo (configuracion mala), el servicio no se queda "iniciado" sin hacer nada
    }
}

/// <summary>Escribe en la consola y, con fecha y hora, en un archivo.</summary>
public sealed class SalidaDoble : TextWriter
{
    private readonly TextWriter _consola;
    private readonly string _archivo;
    private readonly StringBuilder _linea = new();
    public SalidaDoble(TextWriter consola, string archivo) { _consola = consola; _archivo = archivo; }
    public override Encoding Encoding => Encoding.UTF8;
    public override void Write(char value)
    {
        _consola.Write(value);
        if (value == '\n') { Volcar(); return; }
        if (value != '\r') _linea.Append(value);
    }
    public override void Flush() { _consola.Flush(); }
    private void Volcar()
    {
        if (_linea.Length == 0) return;
        try { File.AppendAllText(_archivo, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {_linea}{Environment.NewLine}"); } catch { }
        _linea.Clear();
    }
    protected override void Dispose(bool disposing) { if (disposing) Volcar(); base.Dispose(disposing); }
}
