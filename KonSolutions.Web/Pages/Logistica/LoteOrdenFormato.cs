using KONSolutions.Shared.Common;

namespace KONSolutions.Web.Pages.Logistica;

/// <summary>El texto de la celda «Lote / Orden». Lo usan la grilla del detalle del movimiento y la de Lecturas de una línea, y por eso
/// está acá y no repetido en cada pantalla: las dos se tienen que leer igual.</summary>
public static class LoteOrdenFormato
{
    /// <summary>El lote y, si hay proceso, el proceso con su orden: <c>lote  ·  proceso/orden/ítem/unidad</c>.
    /// El lote va con su DESCRIPCIÓN (se busca en <paramref name="lotes"/>, la lista del combo «Lote de Control» de la cabecera);
    /// el código queda de respaldo si el lote no aparece ahí. El cero SÍ se muestra: es un número de orden real en esta base.
    /// Sin lote ni proceso, un guion largo.</summary>
    public static string Resumen(string? lote, string? proceso, int? orden, short? item, byte? unidad, IEnumerable<ComboboxItem> lotes)
    {
        var partes = new List<string>();

        if (!string.IsNullOrWhiteSpace(lote))
        {
            var texto = lotes.FirstOrDefault(l => string.Equals(l.Id, lote, StringComparison.OrdinalIgnoreCase)) is { } l && !string.IsNullOrWhiteSpace(l.Texto)
                ? l.Texto
                : lote!;
            partes.Add(texto);
        }

        if (!string.IsNullOrWhiteSpace(proceso))
        {
            var texto = proceso!;
            if (orden is { } n)
            {
                texto += $"/{n}";
                if (item   is { } i) texto += $"/{i}";
                if (unidad is { } u) texto += $"/{u}";
            }
            partes.Add(texto);
        }

        return partes.Count == 0 ? "—" : string.Join("  ·  ", partes);
    }
}
