using System.Diagnostics;

namespace KonSolutionsInstalador;

/// <summary>Maneja un servicio de Windows con sc.exe (viene con Windows: no hace falta instalar nada).</summary>
public class Servicio
{
    public string Nombre { get; }
    public string Exe { get; }
    public string Visible { get; }
    public string Descripcion { get; }
    public Servicio(string nombre, string exe, string visible, string descripcion) { Nombre = nombre; Exe = exe; Visible = visible; Descripcion = descripcion; }

    public static (int Codigo, string Salida) Ejecutar(string programa, params string[] args)
    {
        var psi = new ProcessStartInfo(programa) { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false, CreateNoWindow = true };
        foreach (var a in args) psi.ArgumentList.Add(a);
        using var p = Process.Start(psi)!;
        var s = p.StandardOutput.ReadToEnd() + p.StandardError.ReadToEnd();
        p.WaitForExit();
        return (p.ExitCode, s.Trim());
    }
    (int Codigo, string Salida) Sc(params string[] args) => Ejecutar("sc.exe", args);

    public bool Existe() => Sc("query", Nombre).Codigo == 0;

    public string Estado()
    {
        var (c, s) = Sc("query", Nombre);
        if (c != 0) return "NO INSTALADO";
        foreach (var l in s.Split('\n')) if (l.Contains("STATE")) return l.Split(':')[1].Trim().Split(' ').Last();
        return "DESCONOCIDO";
    }

    public bool Esperar(string estado, int segundos)
    {
        var fin = DateTime.UtcNow.AddSeconds(segundos);
        while (DateTime.UtcNow < fin) { if (Estado() == estado) return true; Thread.Sleep(500); }
        return Estado() == estado;
    }

    public void Detener() { if (Existe() && Estado() != "STOPPED") { Sc("stop", Nombre); Esperar("STOPPED", 30); } }

    /// <summary>Crea el servicio (o lo actualiza si ya existe): inicio automatico retrasado, reinicio ante fallas y descripcion.</summary>
    public (bool Ok, string Mensaje) Crear(string rutaExe, string cuenta, string clave)
    {
        var existe = Existe();
        var args = new List<string> { existe ? "config" : "create", Nombre, "binPath=", rutaExe, "start=", "delayed-auto", "DisplayName=", Visible };
        if (!string.IsNullOrWhiteSpace(cuenta)) { args.Add("obj="); args.Add(cuenta); args.Add("password="); args.Add(clave); }
        else if (existe) { args.Add("obj="); args.Add("LocalSystem"); }
        var (c, s) = Sc(args.ToArray());
        if (c != 0) return (false, s);
        Sc("description", Nombre, Descripcion);
        Sc("failure", Nombre, "reset=", "86400", "actions=", "restart/60000/restart/60000/restart/60000");
        return (true, existe ? "Servicio actualizado." : "Servicio creado.");
    }

    public (bool Ok, string Mensaje) Iniciar()
    {
        var (c, s) = Sc("start", Nombre);
        if (c != 0 && !s.Contains("1056")) return (false, s);   // 1056 = ya estaba corriendo
        return Esperar("RUNNING", 40) ? (true, "El servicio esta corriendo.") : (false, "El servicio no llego a RUNNING: mira el Registro de eventos de Windows (Aplicacion) y la carpeta logs.");
    }

    public (bool Ok, string Mensaje) Eliminar() { Detener(); var (c, s) = Sc("delete", Nombre); return (c == 0, c == 0 ? "Servicio eliminado." : s); }
}
