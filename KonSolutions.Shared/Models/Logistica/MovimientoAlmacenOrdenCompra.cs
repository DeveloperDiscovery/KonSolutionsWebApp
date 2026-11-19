namespace KONSolutions.Shared.Models.Logistica;

/// <summary>Cabecera de una orden de compra vista desde un movimiento de almacén.
/// Misma forma para los dos lados de la pantalla: los dos SP devuelven lo mismo.</summary>
public class MovimientoAlmacenOrdenCompra
{
    // Clase y tipo de operación de la orden. Vienen en la fila desde que el SP de
    // DISPONIBLES los devuelve, y son lo que el SP de detalle exige: así el detalle se
    // pide con los valores de lo que el usuario marcó, sin configuración de por medio.
    // Del lado ASIGNADOS llegan en null y no hacen falta: ese detalle no los recibe.
    public string?   COD_CLASE_OPERACION  { get; set; }
    public string?   COD_TIPO_OPERACION   { get; set; }

    /// <summary>Descripción del tipo de operación. Llega sólo del lado DISPONIBLES: es el
    /// único SP que hace el JOIN con ESTANDAR.TIPOS_OPERACION. Del lado asignados queda en
    /// null y la grilla de ese lado no muestra la columna.</summary>
    public string?   DES_TIPO_OPERACION   { get; set; }
    public int       NUM_PEDIDO_COMERCIAL { get; set; }
    public string?   DES_PEDIDO_COMERCIAL { get; set; }
    public DateTime? FEC_PEDIDO           { get; set; }
    public DateTime? FEC_ENTREGA          { get; set; }
    public string?   DES_ESTADO           { get; set; }
    public string?   DES_BACKCOLOR        { get; set; }
    public string?   DES_FORECOLOR        { get; set; }
}

/// <summary>Línea de una orden de compra vista desde un movimiento.
/// Atendido y pendiente llegan sólo del lado DISPONIBLES; del lado ASIGNADOS quedan en
/// null y la grilla no muestra esas columnas.</summary>
public class MovimientoAlmacenOrdenCompraDetalle
{
    public int      NUM_PEDIDO_COMERCIAL { get; set; }
    public int      NUM_SECUENCIA        { get; set; }

    /// <summary>Clase y tipo de operación de la orden a la que pertenece la línea.
    ///
    /// Las devuelve el SP de ASIGNADOS, que las tiene en su propia tabla. Del lado
    /// DISPONIBLES vienen en null, y ahí se toman de la cabecera.
    ///
    /// Hacen falta para QUITAR: el SP necesita saber a qué documento pertenece la línea, y
    /// del lado asignados la CABECERA las trae en null —su SP no hace el JOIN con
    /// TIPOS_OPERACION—, así que la única fuente confiable es la propia línea.</summary>
    public string?  COD_CLASE_OPERACION { get; set; }
    public string?  COD_TIPO_OPERACION { get; set; }
    public string?  COD_PRODUCTO         { get; set; }
    public string?  DES_PRODUCTO         { get; set; }
    public string?  DES_UNIDAD_MEDIDA    { get; set; }
    public decimal? CAN_PRODUCTO         { get; set; }
    public decimal? CAN_PRODUCTO_ATENDIDO           { get; set; }
    public decimal? CAN_PRODUCTO_ATENDIDO_PENDIENTE { get; set; }

    /// <summary>Lo que se va a recibir en ESTE movimiento. El SP la devuelve en cero
    /// (CAN_PRODUCTO * 0): es un campo para llenar, no un dato guardado. La escribe el
    /// usuario en la grilla y viaja hacia el alta cuando se cablee.</summary>
    public decimal? CAN_PRODUCTO_RECIBIDA           { get; set; }

    // ── Lote, proceso y orden de producción ──────────────────────────────────
    // Los devuelve el SP de DISPONIBLES desde la línea del pedido. Anulables: una compra
    // que no es de consumo de producción no los lleva, y eso es lo normal, no un error.
    public string?  NUM_LOTE_CONTROL            { get; set; }
    public string?  COD_PROCESO                 { get; set; }
    public string?  DES_PROCESO                 { get; set; }
    public int?     NUM_ORDEN_PRODUCCION        { get; set; }
    public string?  DES_ORDEN_PRODUCCION        { get; set; }
    public short?   NUM_ORDEN_PRODUCCION_ITEM   { get; set; }
    public byte?    NUM_ORDEN_PRODUCCION_UNIDAD { get; set; }

    // ── Dónde entra la mercadería ────────────────────────────────────────────
    // El almacén sale de la CABECERA del movimiento —el SP lo resuelve y lo repite en cada
    // fila—, así que no se elige: es el mismo para todas. La ubicación sí se elige, y viene
    // PROPUESTA: el SP intenta primero el contenedor marcado como nodo de distribución y
    // después la ubicación donde ese producto ya tiene stock.
    public string?  COD_ALMACEN             { get; set; }
    public string?  COD_UBICACION_ALMACEN   { get; set; }
    public string?  DES_UBICACION_ALMACEN   { get; set; }
}
