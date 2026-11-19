namespace KONSolutions.Shared.Common;

/// <summary>Una fila de existencias para mostrar en el globo que aparece al pasar el mouse
/// sobre un casillero en la vista Volumen. Ya viene formateada como texto: el globo no
/// necesita saber de decimales, unidades ni saltos de línea de la orden de producción, eso
/// se resuelve una vez al armar la lista, no cada vez que se pinta.
///
/// Las líneas secundarias (código/UM/categoría, lote interno/proveedor) reproducen la misma
/// información que ya muestra la grilla de Existencias por Ubicación — mismo dato, mismo
/// orden — para que un usuario que conoce esa pantalla no tenga que reaprender el globo.</summary>
public record CasilleroContenidoItem(
    string Producto, string CodigoProducto, string UnidadMedida, string Categoria,
    string Lote, string LoteInterno, string Proveedor,
    string OrdenProduccion, string Stock);
