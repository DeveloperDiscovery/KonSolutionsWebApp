using System.Text;

namespace KonSolutions.Agente.Util;

/// <summary>Donde el agente deja constancia de lo que hace. Una linea por hecho, con hora y nivel.</summary>
public interface IBitacora
{
    void Info(string mensaje);
    void Aviso(string mensaje);
    void Error(string mensaje);
}

/// <summary>Escribe en la consola y, si hay carpeta, en un archivo por dia (agente-AAAAMMDD.log).</summary>
public sealed class BitacoraConsola : IBitacora
{
    private readonly string? _carpeta;
    private readonly object _candado = new();

    public BitacoraConsola(string? carpeta)
    {
        _carpeta = string.IsNullOrWhiteSpace(carpeta) ? null : carpeta;
        if (_carpeta is not null) Directory.CreateDirectory(_carpeta);
    }

    public void Info(string mensaje) => Escribir("INFO ", mensaje);
    public void Aviso(string mensaje) => Escribir("AVISO", mensaje);
    public void Error(string mensaje) => Escribir("ERROR", mensaje);

    private void Escribir(string nivel, string mensaje)
    {
        var linea = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{nivel}] {mensaje}";
        lock (_candado)
        {
            Console.WriteLine(linea);
            if (_carpeta is null) return;
            try { File.AppendAllText(Path.Combine(_carpeta, $"agente-{DateTime.Now:yyyyMMdd}.log"), linea + Environment.NewLine, new UTF8Encoding(false)); }
            catch { /* un disco lleno no debe parar la grabacion de etiquetas */ }
        }
    }
}

/// <summary>Guarda las lineas en memoria (para las pruebas).</summary>
public sealed class BitacoraMemoria : IBitacora
{
    public List<string> Lineas { get; } = new();
    public void Info(string mensaje) => Lineas.Add("INFO " + mensaje);
    public void Aviso(string mensaje) => Lineas.Add("AVISO " + mensaje);
    public void Error(string mensaje) => Lineas.Add("ERROR " + mensaje);
}
