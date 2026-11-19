using System.Text;

namespace KonSolutionsInstalador;

/// <summary>Entrada y salida de la consola: preguntas con valor por omision, entrada oculta para claves y colores para que se vea que paso bien y que no.</summary>
public static class Consola
{
    public static void Titulo(string t) { Console.WriteLine(); Console.ForegroundColor = ConsoleColor.Cyan; Console.WriteLine("== " + t); Console.ResetColor(); }
    public static void Ok(string t) { Console.ForegroundColor = ConsoleColor.Green; Console.WriteLine("  [OK]    " + t); Console.ResetColor(); }
    public static void Aviso(string t) { Console.ForegroundColor = ConsoleColor.Yellow; Console.WriteLine("  [AVISO] " + t); Console.ResetColor(); }
    public static void Error(string t) { Console.ForegroundColor = ConsoleColor.Red; Console.WriteLine("  [ERROR] " + t); Console.ResetColor(); }
    public static void Info(string t) => Console.WriteLine("          " + t);

    public static string Preguntar(string pregunta, string? porOmision = null)
    {
        Console.Write(porOmision is null ? $"  {pregunta}: " : $"  {pregunta} [{porOmision}]: ");
        var r = Console.ReadLine()?.Trim();
        return string.IsNullOrEmpty(r) ? (porOmision ?? "") : r;
    }

    /// <summary>Lee un secreto sin mostrarlo. Enter vacio = "dejar el que ya esta".</summary>
    public static string PreguntarOculto(string pregunta)
    {
        Console.Write($"  {pregunta} (no se muestra; Enter = dejar el actual): ");
        var sb = new StringBuilder();
        if (Console.IsInputRedirected) { var l = Console.ReadLine(); return l?.Trim() ?? ""; }
        while (true)
        {
            var k = Console.ReadKey(true);
            if (k.Key == ConsoleKey.Enter) { Console.WriteLine(); break; }
            if (k.Key == ConsoleKey.Backspace) { if (sb.Length > 0) sb.Length--; continue; }
            if (!char.IsControl(k.KeyChar)) sb.Append(k.KeyChar);
        }
        return sb.ToString().Trim();
    }

    public static bool Confirmar(string pregunta, bool porOmision = true)
    {
        Console.Write($"  {pregunta} [{(porOmision ? "S/n" : "s/N")}]: ");
        var r = Console.ReadLine()?.Trim().ToLowerInvariant();
        return string.IsNullOrEmpty(r) ? porOmision : r is "s" or "si" or "y" or "yes";
    }
}
