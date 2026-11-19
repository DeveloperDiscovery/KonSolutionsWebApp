using System.Runtime.CompilerServices;

namespace KONSolutions.Web.Services;

/// <summary>
/// Convierte los bytes de una imagen a la cadena "data:...;base64,..." que va en el src,
/// y recuerda el resultado.
///
/// POR QUE HACE FALTA RECORDARLO
/// -----------------------------
/// Convert.ToBase64String se ejecutaba en CADA render de CADA fila. En la grilla de
/// Usuarios eso significa volver a codificar todas las fotos de la página cada vez que
/// algo la redibuja — y además construir una cadena un tercio más grande que la foto,
/// que Blazor después compara carácter por carácter contra la anterior para decidir si
/// el atributo cambió. Con fotos de unos cientos de KB y varias decenas de filas, un
/// solo render mueve megabytes y el navegador se queda sin aire.
///
/// La foto no cambia entre renders: lo que hay que evitar no es la conversión, es
/// repetirla.
///
/// POR QUE ConditionalWeakTable Y NO UN Dictionary
/// -----------------------------------------------
/// La clave es el propio arreglo de bytes, por referencia. Cuando la fila desaparece y
/// nadie más apunta a esa foto, la entrada se libera sola. Un Dictionary estático se
/// quedaría con todas las fotos de todas las pantallas visitadas hasta cerrar el
/// navegador — arreglar un problema de memoria creando otro.
/// </summary>
public static class ImagenDataUri
{
    private static readonly ConditionalWeakTable<byte[], string> _cache = new();

    /// <summary>Devuelve el data URI listo para el src, o cadena vacía si no hay imagen.</summary>
    public static string De(byte[]? bytes)
    {
        if (bytes is not { Length: > 0 }) return string.Empty;
        return _cache.GetValue(bytes, b => $"data:{TipoMime(b)};base64,{Convert.ToBase64String(b)}");
    }

    /// <summary>El formato REAL, mirando los primeros bytes.
    ///
    /// Antes se declaraba todo como PNG salvo el SVG, con el argumento de que el navegador
    /// igual interpreta bien un JPEG o un GIF. Eso vale para esos dos, pero NO para un BMP
    /// ni para un ícono ICO, que son justo los formatos que trae una base vieja: ahí el
    /// navegador dibuja el cuadrito de imagen rota y parece que el dato no llegó, cuando en
    /// realidad llegó entero y mal etiquetado.
    ///
    /// Cuesta comparar ocho bytes una sola vez por imagen (el resultado queda cacheado),
    /// así que no hay motivo para adivinar.</summary>
    private static string TipoMime(byte[] b)
    {
        // SVG: se guarda como texto y empieza con "<", a veces después de espacios o de la
        // declaración XML.
        var cabecera = System.Text.Encoding.UTF8.GetString(b, 0, Math.Min(16, b.Length));
        if (cabecera.TrimStart().StartsWith("<")) return "image/svg+xml";

        static bool Empieza(byte[] b, params byte[] firma)
        {
            if (b.Length < firma.Length) return false;
            for (var i = 0; i < firma.Length; i++) if (b[i] != firma[i]) return false;
            return true;
        }

        if (Empieza(b, 0x89, 0x50, 0x4E, 0x47)) return "image/png";
        if (Empieza(b, 0xFF, 0xD8, 0xFF))       return "image/jpeg";
        if (Empieza(b, 0x47, 0x49, 0x46, 0x38)) return "image/gif";
        if (Empieza(b, 0x42, 0x4D))             return "image/bmp";
        if (Empieza(b, 0x00, 0x00, 0x01, 0x00)) return "image/x-icon";
        // WEBP: "RIFF" .... "WEBP"
        if (Empieza(b, 0x52, 0x49, 0x46, 0x46) && b.Length > 11 &&
            b[8] == 0x57 && b[9] == 0x45 && b[10] == 0x42 && b[11] == 0x50) return "image/webp";

        // Desconocido: se deja PNG, que es lo que había, y el navegador hace lo que pueda.
        return "image/png";
    }
}
