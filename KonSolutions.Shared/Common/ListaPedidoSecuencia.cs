using System.Globalization;

namespace KONSolutions.Shared.Common;

/// <summary>Arma la cadena que reciben PROC_MOVIMIENTOS_ALMACEN_PEDIDOS_COMERCIALES_ASIGNA y
/// _DESASIGNA para saber qué líneas mover y CUÁNTO de cada una.
///
/// EL FORMATO
/// ----------
///     P&lt;pedido&gt;S&lt;sec&gt;C&lt;cantidad&gt;S&lt;sec&gt;C&lt;cantidad&gt;_P&lt;pedido&gt;S&lt;sec&gt;C&lt;cantidad&gt;
///
/// Por ejemplo 'P10S1C25S2C10_P25S3C4' son tres líneas: del pedido 10 la secuencia 1 por 25
/// y la 2 por 10, y del pedido 25 la secuencia 3 por 4.
///
/// La C se agregó al formato original —que era P10S1S2_P25S3— para poder recibir PARCIALES.
/// Antes el SP tomaba la cantidad comprada de la línea del pedido, así que una orden de 100
/// de la que llegaban 60 entraba igual por 100. El formato viejo sigue leyéndose bien: una
/// secuencia sin C se interpreta como cantidad cero, y del lado de la base eso significa
/// "toda la pendiente", para no romper lo que ya existe.
///
/// LA CANTIDAD VA CON PUNTO DECIMAL Y CULTURA INVARIANTE. No es un detalle: con la
/// configuración regional peruana un decimal se escribiría con coma, y del otro lado
/// TRY_CONVERT devolvería NULL sin dar error. La línea entraría en cero.
///
/// POR QUÉ VIVE ACÁ Y NO EN LA PANTALLA
/// ------------------------------------
/// Porque un error al armarla NO SE VE. Del otro lado se parsea con TRY_CONVERT: una cadena
/// mal formada no falla, devuelve una lista vacía o con nulos, el SP recorre cero líneas y la
/// operación informa ÉXITO sin haber hecho nada.
///
/// Con el armado en un solo lugar, ese riesgo se concentra en un método corto en vez de
/// repartirse por cada pantalla que quiera llamar al SP.</summary>
public static class ListaPedidoSecuencia
{
    /// <summary>Agrupa por pedido y devuelve la cadena. Lista vacía -&gt; cadena vacía, que
    /// quien llama tiene que leer como "no hay nada que hacer" y no como llamada válida.</summary>
    public static string Armar(IEnumerable<(int Pedido, int Secuencia, decimal Cantidad)> lineas)
    {
        if (lineas is null) return "";

        var grupos = lineas
            // Una línea marcada dos veces no puede duplicar el movimiento. Se agrupa por
            // pedido+secuencia y se queda la ÚLTIMA cantidad: si el usuario corrigió el
            // número, lo que vale es lo último que escribió.
            .GroupBy(x => (x.Pedido, x.Secuencia))
            .Select(g => (g.Key.Pedido, g.Key.Secuencia, Cantidad: g.Last().Cantidad))
            .GroupBy(x => x.Pedido)
            .OrderBy(g => g.Key)
            .Select(g => "P" + g.Key.ToString(CultureInfo.InvariantCulture) +
                         string.Concat(g.OrderBy(x => x.Secuencia).Select(x =>
                             "S" + x.Secuencia.ToString(CultureInfo.InvariantCulture) +
                             "C" + x.Cantidad.ToString(CultureInfo.InvariantCulture))))
            .Where(t => t.Contains('S'))
            .ToList();

        return string.Join("_", grupos);
    }

    /// <summary>Vuelve a leer la cadena. Sirve para probar el armado y para que la API pueda
    /// rechazar una cadena rota antes de llegar a la base.
    ///
    /// Una secuencia sin C devuelve cantidad cero — es el formato viejo, y así se sigue
    /// entendiendo.</summary>
    public static List<(int Pedido, int Secuencia, decimal Cantidad)> Leer(string? cadena)
    {
        var salida = new List<(int, int, decimal)>();
        if (string.IsNullOrWhiteSpace(cadena)) return salida;

        foreach (var bloque in cadena.Split('_', StringSplitOptions.RemoveEmptyEntries))
        {
            if (bloque.Length < 4 || bloque[0] != 'P') continue;

            var corte = bloque.IndexOf('S');
            if (corte < 2) continue;
            if (!int.TryParse(bloque[1..corte], NumberStyles.Integer, CultureInfo.InvariantCulture, out var pedido)) continue;

            foreach (var parte in bloque[corte..].Split('S', StringSplitOptions.RemoveEmptyEntries))
            {
                var sec = parte;
                decimal cantidad = 0;

                var c = parte.IndexOf('C');
                if (c >= 0)
                {
                    sec = parte[..c];
                    decimal.TryParse(parte[(c + 1)..], NumberStyles.Number, CultureInfo.InvariantCulture, out cantidad);
                }

                if (int.TryParse(sec, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n))
                    salida.Add((pedido, n, cantidad));
            }
        }
        return salida;
    }

    /// <summary>True si la cadena representa exactamente esas líneas, cantidades incluidas.
    /// La API la usa antes de ejecutar: si el armado y la lectura no coinciden, algo se
    /// perdió en el camino y es mejor un error claro que un movimiento incompleto.</summary>
    public static bool Coincide(string? cadena, IEnumerable<(int Pedido, int Secuencia, decimal Cantidad)> lineas)
    {
        static IEnumerable<(int, int, decimal)> Norm(IEnumerable<(int Pedido, int Secuencia, decimal Cantidad)> x)
            => x.GroupBy(v => (v.Pedido, v.Secuencia))
                .Select(g => (g.Key.Pedido, g.Key.Secuencia, g.Last().Cantidad))
                .OrderBy(v => v.Pedido).ThenBy(v => v.Secuencia);

        var esperadas = Norm(lineas ?? Enumerable.Empty<(int, int, decimal)>()).ToList();
        var leidas = Norm(Leer(cadena)).ToList();
        return esperadas.Count == leidas.Count && esperadas.SequenceEqual(leidas);
    }
}
