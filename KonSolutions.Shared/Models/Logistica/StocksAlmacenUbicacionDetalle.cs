using KONSolutions.Shared.Common;
namespace KONSolutions.Shared.Models.Logistica;

/// <summary>Una fila de existencias: un producto, en un lote, en una ubicación de un almacén.
///
/// Es el resultado de PROC_LOGISTICA_STOCKS_ALMACEN_UBICACION_SEARCHDETAIL, que ya resuelve por
/// JOIN todas las descripciones. Se separa de la entidad StocksAlmacenUbicacion porque aquélla
/// representa la tabla y ésta el resultado de una consulta: mezclarlas obligaría a cargar de
/// nulos la entidad cada vez que se usa para escribir.</summary>
public class StocksAlmacenUbicacionDetalle
{
    // ── Dónde está ──────────────────────────────────────────────────────────
    public string?  COD_ALMACEN                  { get; set; }
    public string?  DES_ALMACEN                  { get; set; }
    public string?  COD_UBICACION_ALMACEN        { get; set; }
    public string?  DES_UBICACION_ALMACEN        { get; set; }
    /// <summary>El polígono del plano. Es lo que permite marcar la ubicación en el mapa.</summary>
    public string?  DES_POLIGONO                 { get; set; }

    // ── Qué es ──────────────────────────────────────────────────────────────
    public string?  COD_GRUPO_PRODUCTO           { get; set; }
    public string?  DES_GRUPO_PRODUCTO           { get; set; }
    public string?  COD_FAMILIA_PRODUCTO         { get; set; }
    public string?  DES_FAMILIA_PRODUCTO         { get; set; }
    public string?  COD_SUB_FAMILIA_PRODUCTO     { get; set; }
    public string?  DES_SUB_FAMILIA_PRODUCTO     { get; set; }
    public string?  COD_TIPO_PRODUCTO            { get; set; }
    public string?  DES_TIPO_PRODUCTO            { get; set; }
    public string?  COD_PRODUCTO                 { get; set; }
    public string?  DES_PRODUCTO                 { get; set; }

    // ── De qué lote viene ───────────────────────────────────────────────────
    public string?  NUM_LOTE_CONTROL             { get; set; }
    public string?  DES_LOTE_CONTROL             { get; set; }
    public string?  NUM_LOTE_CONTROL_ENTIDAD     { get; set; }
    public decimal? COD_ENTIDAD                  { get; set; }
    public string?  DES_NOMBRE_COMPLETO          { get; set; }

    // ── De qué orden de producción viene ───────────────────────────────────
    public string?  COD_PROCESO                  { get; set; }
    public int?     NUM_ORDEN_PRODUCCION          { get; set; }
    public short?   NUM_ORDEN_PRODUCCION_ITEM     { get; set; }
    public byte?    NUM_ORDEN_PRODUCCION_UNIDAD   { get; set; }
    public string?  DES_ORDEN_PRODUCCION          { get; set; }

    // ── Cuánto hay ──────────────────────────────────────────────────────────
    // Cada cantidad viene con su par en la unidad secundaria, que puede no existir.
    public decimal? CAN_PRODUCTO                 { get; set; }
    public string?  COD_UNIDAD_MEDIDA            { get; set; }
    public string?  DES_UNIDAD_MEDIDA            { get; set; }
    public decimal? CAN_PRODUCTO_SECUNDARIO      { get; set; }
    public string?  COD_UNIDAD_MEDIDA_SECUNDARIO { get; set; }
    public string?  DES_UNIDAD_MEDIDA_SECUNDARIO { get; set; }

    public decimal? CAN_RESERVADA                { get; set; }
    public decimal? CAN_RESERVADA_SECUNDARIO     { get; set; }
    public decimal? CAN_TRANSITO                 { get; set; }
    public decimal? CAN_TRANSITO_SECUNDARIO      { get; set; }
    public decimal? CAN_INMOVILIZADO             { get; set; }
    public decimal? CAN_INMOVILIZADO_SECUNDARIO  { get; set; }
    public decimal? CAN_DISPONIBLE               { get; set; }
    public decimal? CAN_DISPONIBLE_SECUNDARIO    { get; set; }
}

/// <summary>Los filtros de la consulta. Todos opcionales: el procedimiento ignora los que
/// llegan vacíos o en cero, así que la pantalla puede mandarlos todos siempre.</summary>
public class StocksAlmacenUbicacionSearchDetailDto
{
    public string?  COD_ALMACEN                 { get; set; }
    public string?  COD_UBICACION_ALMACEN       { get; set; }
    public string?  COD_GRUPO_PRODUCTO          { get; set; }
    public string?  COD_FAMILIA_PRODUCTO        { get; set; }
    public string?  COD_SUB_FAMILIA_PRODUCTO    { get; set; }
    public string?  COD_PRODUCTO                { get; set; }
    public string?  COD_TIPO_PRODUCTO           { get; set; }
    public string?  DES_PRODUCTO                { get; set; }
    public decimal? COD_ENTIDAD_PROVEEDOR       { get; set; }
    public string?  NUM_LOTE_CONTROL            { get; set; }
    public string?  NUM_LOTE_CONTROL_ENTIDAD    { get; set; }

    // La orden de producción, en cascada: item cuelga de orden, unidad cuelga de item.
    public string?  COD_PROCESO                 { get; set; }
    public int?     NUM_ORDEN_PRODUCCION         { get; set; }
    public short?   NUM_ORDEN_PRODUCCION_ITEM    { get; set; }
    public byte?    NUM_ORDEN_PRODUCCION_UNIDAD  { get; set; }

    // Rangos: cero significa "sin límite por ese lado", no "igual a cero".
    public decimal? CAN_PRODUCTO_INICIAL        { get; set; }
    public decimal? CAN_PRODUCTO_FINAL          { get; set; }
    public decimal? CAN_RESERVADA_INICIAL       { get; set; }
    public decimal? CAN_RESERVADA_FINAL         { get; set; }
    public decimal? CAN_TRANSITO_INICIAL        { get; set; }
    public decimal? CAN_TRANSITO_FINAL          { get; set; }
    public decimal? CAN_IMOVILIZADO_INICIAL     { get; set; }
    public decimal? CAN_IMOVILIZADO_FINAL       { get; set; }
    public decimal? CAN_DISPONIBLE_INICIAL      { get; set; }
    public decimal? CAN_DISPONIBLE_FINAL        { get; set; }
    public string?  DES_ESPECIFICACIONES_TECNICAS { get; set; }
}

/// <summary>Lo que se manda para leer una etiqueta en la consulta de existencias.</summary>
public class StocksAlmacenUbicacionEtiquetaDto
{
    /// <summary>La etiqueta tal como la entrega el lector: RFID (más de 100 caracteres) o código de barras.</summary>
    [System.ComponentModel.DataAnnotations.Required] public string COD_STICKER_RFID { get; set; } = string.Empty;
}

/// <summary>Lo que dice una etiqueta y cómo está su stock. Es el resultado de
/// PROC_LOGISTICA_STOCKS_ALMACEN_UBICACION_ETIQUETA, que no graba nada.
///
/// Hay dos juegos de columnas a propósito. Los de «lo que la etiqueta dice» salen vacíos si la etiqueta no trae ese dato. Los
/// _CLAVE son los mismos completados con los valores por omisión de la configuración general: es EXACTAMENTE la clave con
/// que la lectura de un movimiento busca el stock. Si el stock está cargado con otro lote (u otro proceso, u otra orden),
/// la lectura dice «disponible 0» aunque el producto sí tenga stock: acá se ve la diferencia.</summary>
public class StocksAlmacenUbicacionEtiqueta
{
    /// <summary>RFID (más de 100 caracteres) o CODEBAR.</summary>
    public string?  DES_TIPO_ETIQUETA            { get; set; }
    public string?  COD_PRODUCTO                 { get; set; }
    public string?  DES_PRODUCTO                 { get; set; }
    public string?  COD_UNIDAD_MEDIDA            { get; set; }
    public bool?    FLG_RFID                     { get; set; }
    public bool?    FLG_CODEBAR                  { get; set; }
    public string?  DES_TIPO_LECTURA             { get; set; }

    // ── Lo que la etiqueta dice (vacío = no lo trae) ────────────────────────
    public string?  NUM_LOTE_CONTROL             { get; set; }
    public string?  DES_LOTE_CONTROL             { get; set; }
    public string?  COD_PROCESO                  { get; set; }
    public string?  DES_PROCESO                  { get; set; }
    public int?     NUM_ORDEN_PRODUCCION         { get; set; }
    public short?   NUM_ORDEN_PRODUCCION_ITEM    { get; set; }
    public byte?    NUM_ORDEN_PRODUCCION_UNIDAD  { get; set; }
    public string?  NUM_SERIE                    { get; set; }

    // ── La clave con que se busca el stock (lo que dice + valores por omisión) ──
    public string?  NUM_LOTE_CONTROL_CLAVE             { get; set; }
    public string?  COD_PROCESO_CLAVE                  { get; set; }
    public int?     NUM_ORDEN_PRODUCCION_CLAVE         { get; set; }
    public short?   NUM_ORDEN_PRODUCCION_ITEM_CLAVE    { get; set; }
    public byte?    NUM_ORDEN_PRODUCCION_UNIDAD_CLAVE  { get; set; }

    // ── El stock: con esa clave y del producto sin mirar la clave (en todos los almacenes) ──
    public int      NUM_FILAS_STOCK_CLAVE        { get; set; }
    public decimal  CAN_STOCK_CLAVE              { get; set; }
    public decimal  CAN_DISPONIBLE_CLAVE         { get; set; }
    public int      NUM_FILAS_STOCK_PRODUCTO     { get; set; }
    public decimal  CAN_STOCK_PRODUCTO           { get; set; }
    public decimal  CAN_DISPONIBLE_PRODUCTO      { get; set; }

    // ── Dónde se leyó por última vez (solo RFID: el código de barras no deja rastro) ──
    public int      NUM_LECTURAS                 { get; set; }
    public decimal? NUM_MOVIMIENTO_ALMACEN       { get; set; }
    public int?     NUM_SECUENCIA                { get; set; }
    public string?  COD_TIPO_MOVIMIENTO          { get; set; }
    public string?  DES_TIPO_MOVIMIENTO          { get; set; }
    /// <summary>true = la última lectura fue un ingreso; false = una salida.</summary>
    public bool?    FLG_POSITIVO                 { get; set; }
    public string?  COD_ALMACEN                  { get; set; }
    public string?  DES_ALMACEN                  { get; set; }
    public string?  COD_UBICACION_ALMACEN        { get; set; }
    public string?  DES_UBICACION_ALMACEN        { get; set; }
    public decimal? CAN_LEIDA                    { get; set; }
    public DateTime? FEC_LECTURA                 { get; set; }
    public string?  COD_USUARIO_LECTURA          { get; set; }
}

/// <summary>Los combos que no dependen de ningún otro filtro, resueltos en una sola llamada.</summary>
public class StocksAlmacenUbicacionCombos
{
    public List<ComboboxItem> Almacenes     { get; set; } = new();
    public List<ComboboxItem> Grupos        { get; set; } = new();
    public List<ComboboxItem> TiposProducto { get; set; } = new();
    public List<ComboboxItem> Proveedores   { get; set; } = new();
    public List<ComboboxItem> Productos     { get; set; } = new();
}
