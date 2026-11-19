using System.Linq;

namespace KONSolutions.Shared.Common;

/// <summary>Arma el número de un comprobante a partir de lo que el usuario digita.
///
/// De "T1-453", con prefijo "T", longitud de serie 3 y longitud de número 7, sale
/// "T001-0000453".
///
/// VIVE EN SHARED, no en la pantalla, porque el número de comprobante se pide en Movimientos
/// de Almacén, en Comprobantes de Pago y en todo lo que registre un documento de tercero. La
/// regla escrita una vez es la única forma de que las tres normalicen igual.
///
/// No toca la base ni la red: recibe lo digitado y las tres configuraciones del tipo, y
/// devuelve el resultado o el motivo por el que no se pudo.</summary>
public static class NumeroComprobante
{
    /// <summary>Qué pasó al normalizar. Se devuelve todo junto para que la pantalla decida
    /// qué mostrar: el valor nuevo, el aviso y, si falló, el motivo.</summary>
    public readonly record struct Resultado(bool Ok, string Valor, string? Aviso, string? Error);

    /// <param name="digitado">Lo que escribió el usuario: "T1-453", "1-453", "453".</param>
    /// <param name="prefijo">El prefijo del tipo de comprobante. MANDA sobre las letras que se
    /// hayan digitado: si no coinciden, se corrige y se avisa.
    ///
    /// Quien llama decide cuál pasa. Para el número impreso es COD_PREFIJO_SUNAT —es el que
    /// exige SUNAT— con COD_PREFIJO de respaldo cuando el tipo todavía no lo tiene cargado;
    /// esa elección está en PrefijoDe().</param>
    /// <param name="longitudSerie">VAL_LONGITUD_SERIE — dígitos de la serie, sin el prefijo.</param>
    /// <param name="longitudNumero">VAL_LONGITUD_NUMERO — dígitos del correlativo.</param>
    /// <summary>Cuál de los dos prefijos usar para el número impreso.
    ///
    /// El de SUNAT primero, porque es el que tiene que salir en el documento. COD_PREFIJO —el
    /// interno— queda de respaldo: los tipos cargados antes de que existiera la columna no lo
    /// tienen, y sin respaldo esos comprobantes saldrían de golpe sin prefijo.</summary>
    public static string PrefijoDe(string? prefijoSunat, string? prefijoInterno)
        => !string.IsNullOrWhiteSpace(prefijoSunat) ? prefijoSunat.Trim()
         : (prefijoInterno ?? "").Trim();

    public static Resultado Normalizar(string? digitado, string? prefijo, int? longitudSerie, int? longitudNumero)
    {
        var texto = (digitado ?? "").Trim();
        if (texto.Length == 0) return new(true, "", null, null);

        // Sin longitudes configuradas no hay nada que normalizar: el tipo no declaró formato y
        // se respeta lo que el usuario escribió. Es lo que hace que esto se pueda soltar sin
        // tocar los tipos que ya existen.
        if (longitudSerie is not > 0 || longitudNumero is not > 0)
            return new(true, texto, null, null);

        // Parte por el guion. Sin guion, todo lo escrito es el correlativo y la serie queda en
        // blanco: es lo más probable cuando alguien tipea "453" de apuro.
        var guion = texto.IndexOf('-');
        var izquierda = guion >= 0 ? texto[..guion].Trim() : "";
        var derecha   = guion >= 0 ? texto[(guion + 1)..].Trim() : texto;

        var letras  = new string(izquierda.TakeWhile(char.IsLetter).ToArray());
        var serie   = new string(izquierda.SkipWhile(char.IsLetter).ToArray());
        var numero  = derecha;

        if (!serie.All(char.IsDigit) || !numero.All(char.IsDigit))
            return new(false, texto, null, "La serie y el número sólo admiten dígitos.");

        // Lo que no entra NO se recorta: recortar cambia el documento en silencio, y un número
        // de comprobante equivocado es un problema tributario, no un detalle de formato. Se
        // blanquea y se dice qué pasó.
        if (serie.Length > longitudSerie)
            return new(false, "", null,
                $"La serie admite hasta {longitudSerie} dígitos y escribiste {serie.Length}.");

        if (numero.Length > longitudNumero)
            return new(false, "", null,
                $"El número admite hasta {longitudNumero} dígitos y escribiste {numero.Length}.");

        var pre = (prefijo ?? "").Trim();

        // El prefijo lo define el TIPO de comprobante, no quien digita. Si escribió otro, se
        // corrige — pero se avisa: puede haberse equivocado de tipo, y eso conviene que lo vea
        // ahora y no cuando el documento ya está grabado.
        string? aviso = null;
        if (letras.Length > 0 && !string.Equals(letras, pre, StringComparison.OrdinalIgnoreCase))
            aviso = $"El prefijo de este tipo es «{pre}»: se reemplazó «{letras}».";

        var valor = pre
                  + serie.PadLeft(longitudSerie.Value, '0')
                  + "-"
                  + numero.PadLeft(longitudNumero.Value, '0');

        return new(true, valor, aviso, null);
    }
}
