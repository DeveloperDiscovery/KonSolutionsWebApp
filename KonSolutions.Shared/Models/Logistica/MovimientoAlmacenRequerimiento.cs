namespace KONSolutions.Shared.Models.Logistica;

/// <summary>Cabecera de un requerimiento de stock visto desde un movimiento de almacén.
///
/// Una sola forma para los dos lados —disponibles y asignados—, porque los dos SP
/// devuelven las mismas columnas.</summary>
public class MovimientoAlmacenRequerimiento
{
    public int       NUM_REQUERIMIENTO_STOCK { get; set; }
    public string?   DES_REQUERIMIENTO_STOCK { get; set; }
    public DateTime? FEC_REQUERIMIENTO_STOCK { get; set; }
    public string?   COD_TIPO_REQUERIMIENTO  { get; set; }
    public string?   DES_TIPO_REQUERIMIENTO  { get; set; }
    public string?   COD_CENTRO_COSTO_REQUIRIENTE_ORIGINAL { get; set; }
    public string?   DES_CENTRO_COSTO_REQUIRIENTE_ORIGINAL { get; set; }
    public int?      COD_ESTADO              { get; set; }
    public string?   DES_ESTADO              { get; set; }
    public string?   DES_BACKCOLOR           { get; set; }
    public string?   DES_FORECOLOR           { get; set; }
}

/// <summary>Línea de un requerimiento vista desde un movimiento.
///
/// Los dos SP comparten las primeras columnas; el de DISPONIBLES agrega lo que hace falta
/// para decidir: lo pendiente, la ubicación que el criterio eligió, el disponible de esa
/// ubicación, el total elegible y una cobertura ya resuelta. Del lado asignados esas
/// quedan en null y la grilla no las muestra.</summary>
public class MovimientoAlmacenRequerimientoDetalle
{
    public int      NUM_REQUERIMIENTO_STOCK { get; set; }
    public int      NUM_SECUENCIA           { get; set; }
    public string?  COD_ALMACEN             { get; set; }
    public string?  DES_ALMACEN             { get; set; }
    public string?  COD_PRODUCTO            { get; set; }
    public string?  DES_PRODUCTO            { get; set; }
    public string?  COD_UNIDAD_MEDIDA       { get; set; }
    public string?  DES_UNIDAD_MEDIDA       { get; set; }
    public decimal? CAN_PRODUCTO            { get; set; }
    public string?  COD_UNIDAD_MEDIDA_SECUNDARIA { get; set; }
    public string?  DES_UNIDAD_MEDIDA_SECUNDARIA { get; set; }
    public decimal? CAN_PRODUCTO_SECUNDARIA { get; set; }

    // ── Sólo del lado DISPONIBLES ────────────────────────────────────────────
    public decimal? CAN_PENDIENTE           { get; set; }

    /// <summary>Cuánto se despacha en ESTE movimiento. NO viene del SP: la escribe el usuario
    /// en la grilla, igual que la "Cantidad recibida" del lado de Órdenes de Compra.
    ///
    /// Queda en el modelo y no en una estructura aparte para que viaje con la línea cuando se
    /// cablee el alta: una lista paralela indexada por posición se desincroniza en cuanto la
    /// grilla se reordena o se filtra.</summary>
    public decimal? CAN_PRODUCTO_DESPACHAR { get; set; }
    // El código de lote es texto (ej. "5677-324424"), no un número: declararlo int? rompía
    // la deserialización apenas el requerimiento traía un lote real. Mismo criterio que
    // LotesControl y el resto de las pantallas que ya lo tratan como string.
    public string?  NUM_LOTE_CONTROL        { get; set; }
    public string?  COD_PROCESO             { get; set; }
    public int?     NUM_ORDEN_PRODUCCION    { get; set; }
    public int?     NUM_ORDEN_PRODUCCION_ITEM { get; set; }
    public int?     NUM_ORDEN_PRODUCCION_UNIDAD { get; set; }

    public string?  COD_UBICACION_ALMACEN_ASIGNADA { get; set; }
    public string?  DES_UBICACION_ALMACEN_ASIGNADA { get; set; }
    public string?  COD_ALMACEN_ASIGNADO           { get; set; }
    public string?  NUM_LOTE_CONTROL_ASIGNADO      { get; set; }
    public string?  COD_PROCESO_ASIGNADO           { get; set; }
    public int?     NUM_ORDEN_PRODUCCION_ASIGNADA  { get; set; }
    public int?     NUM_ORDEN_PRODUCCION_ITEM_ASIGNADO   { get; set; }
    public int?     NUM_ORDEN_PRODUCCION_UNIDAD_ASIGNADA { get; set; }
    public decimal? CAN_DISPONIBLE_ASIGNADA        { get; set; }
    public decimal? CAN_DISPONIBLE_TOTAL           { get; set; }

    // ── Del SP de cobertura (detalle de DISPONIBLES) ────────────────────────
    /// <summary>Lo que el SP logró cubrir con stock para esta línea.</summary>
    public decimal? CAN_ATENDER                    { get; set; }
    /// <summary>Lo que queda sin cubrir después de CAN_ATENDER.</summary>
    public decimal? CAN_PENDIENTE_ATENDER          { get; set; }

    /// <summary>Stock que el SP tomó para esta línea, con su reserva de origen (segundo
    /// resultado del SP). Sólo existe de este lado: la API lo entrega aparte, en
    /// CoberturaRequerimientoResultado.Stock, y el panel lo cuelga de su línea.</summary>
    public List<CoberturaStockSeleccionado> Stock { get; set; } = new();

    /// <summary>COMPLETO · ASIGNADA · PARCIAL · SIN STOCK. La resuelve el SP: es una
    /// decisión de negocio y tiene que decir lo mismo en la pantalla y en un reporte.</summary>
    public string?  DES_COBERTURA           { get; set; }
}
