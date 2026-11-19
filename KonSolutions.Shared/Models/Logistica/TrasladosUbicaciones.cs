namespace KONSolutions.Shared.Models.Logistica;

// Traslados entre ubicaciones de un mismo almacén (pantalla Logistica/TransferenciasUbicaciones).
// Tienen que existir IGUAL en la API y en Shared: un campo que está en uno solo se pierde al
// serializar, sin error.

/// <summary>Una salida con alcance "Entre ubicaciones del almacén"
/// (PROC_LOGISTICA_TRASLADO_UBICACIONES_TIPOS). DES_PROBLEMA dice qué le falta configurar.</summary>
public class TrasladoUbicacionesTipo
{
    public string? COD_TIPO_MOVIMIENTO              { get; set; }
    public string? DES_TIPO_MOVIMIENTO              { get; set; }
    public string? COD_TIPO_MOVIMIENTO_SINCRONIZADO { get; set; }
    public string? DES_PROBLEMA                     { get; set; }
}

/// <summary>Un rack (contenedor de almacenaje) con casilleros en el almacén.</summary>
public class TrasladoUbicacionesRack
{
    public string? COD_CONTENEDOR { get; set; }
    public string? DES_CONTENEDOR { get; set; }
    public int?    NUM_FILAS      { get; set; }
    public int?    NUM_COLUMNAS   { get; set; }
}

/// <summary>Un casillero de un rack: su fila, su columna y la ubicación que le corresponde.</summary>
public class TrasladoUbicacionesCasillero
{
    public string? COD_CONTENEDOR            { get; set; }
    public string? COD_CASILLERO             { get; set; }
    public string? DES_CASILLERO             { get; set; }
    public int?    NUM_FILA                  { get; set; }
    public int?    NUM_COLUMNA               { get; set; }
    public string? COD_UBICACION_ALMACEN     { get; set; }
    public string? DES_UBICACION_ALMACEN     { get; set; }
    public int?    CAN_PRODUCTOS             { get; set; }
    public int?    CAN_PRODUCTOS_DISPONIBLES { get; set; }
}

/// <summary>Una ubicación que no es casillero de ningún rack (zona de recepción, piso, ...).</summary>
public class TrasladoUbicacionesUbicacion
{
    public string? COD_UBICACION_ALMACEN     { get; set; }
    public string? DES_UBICACION_ALMACEN     { get; set; }
    public int?    CAN_PRODUCTOS             { get; set; }
    public int?    CAN_PRODUCTOS_DISPONIBLES { get; set; }
}

/// <summary>El mapa del almacén (PROC_LOGISTICA_TRASLADO_UBICACIONES_MAPA), en el orden de sus
/// tres resultsets.</summary>
public class TrasladoUbicacionesMapa
{
    public List<TrasladoUbicacionesRack>       Racks      { get; set; } = new();
    public List<TrasladoUbicacionesCasillero>  Casilleros { get; set; } = new();
    public List<TrasladoUbicacionesUbicacion>  Sueltas    { get; set; } = new();
}

/// <summary>Una clave de stock de una ubicación (PROC_LOGISTICA_TRASLADO_UBICACIONES_STOCK).</summary>
public class TrasladoUbicacionesStock
{
    public string?  COD_ALMACEN                  { get; set; }
    public string?  COD_UBICACION_ALMACEN        { get; set; }
    public string?  DES_UBICACION_ALMACEN        { get; set; }
    public string?  COD_PRODUCTO                 { get; set; }
    public string?  DES_PRODUCTO                 { get; set; }
    public string?  NUM_LOTE_CONTROL             { get; set; }
    public string?  COD_PROCESO                  { get; set; }
    public int?     NUM_ORDEN_PRODUCCION         { get; set; }
    public short?   NUM_ORDEN_PRODUCCION_ITEM    { get; set; }
    public byte?    NUM_ORDEN_PRODUCCION_UNIDAD  { get; set; }
    public string?  COD_UNIDAD_MEDIDA            { get; set; }
    public string?  COD_UNIDAD_MEDIDA_SECUNDARIO { get; set; }
    public decimal? CAN_PRODUCTO                 { get; set; }
    public decimal? CAN_RESERVADA                { get; set; }
    public decimal? CAN_INMOVILIZADO             { get; set; }
    public decimal? CAN_DISPONIBLE               { get; set; }
    public decimal? CAN_DISPONIBLE_SECUNDARIO    { get; set; }
}

/// <summary>Una línea del traslado: qué clave de stock sale de qué ubicación, a cuál va y cuánto.</summary>
public class TrasladoUbicacionesLinea
{
    public string?  COD_PRODUCTO                { get; set; }
    public string?  NUM_LOTE_CONTROL            { get; set; }
    public string?  COD_PROCESO                 { get; set; }
    public int?     NUM_ORDEN_PRODUCCION        { get; set; }
    public short?   NUM_ORDEN_PRODUCCION_ITEM   { get; set; }
    public byte?    NUM_ORDEN_PRODUCCION_UNIDAD { get; set; }
    public string?  COD_UBICACION_ORIGEN        { get; set; }
    public string?  COD_UBICACION_DESTINO       { get; set; }
    public decimal? CAN_PRODUCTO                { get; set; }
    /// <summary>De CAN_PRODUCTO, lo que es reservado y viaja con su reserva (script 79). 0 o nulo: solo disponible.</summary>
    public decimal? CAN_RESERVADA_MOVER         { get; set; }
}

/// <summary>Lo que manda la pantalla al confirmar (PROC_LOGISTICA_TRASLADO_UBICACIONES_REGISTRA).</summary>
public class TrasladoUbicacionesRegistraDto
{
    public string?   COD_ALMACEN           { get; set; }
    public string?   COD_CLASE_MOVIMIENTO  { get; set; }
    public string?   COD_TIPO_MOVIMIENTO   { get; set; }
    public DateTime? FEC_MOVIMENTO_ALMACEN { get; set; }
    public string?   DES_OBSERVACION       { get; set; }
    public List<TrasladoUbicacionesLinea> Lineas { get; set; } = new();
}

/// <summary>Los dos movimientos que generó el traslado.</summary>
public class TrasladoUbicacionesResultado
{
    public decimal? NUM_MOVIMIENTO_SALIDA  { get; set; }
    public decimal? NUM_MOVIMIENTO_INGRESO { get; set; }
}

/// <summary>Una línea del historial (PROC_LOGISTICA_TRASLADO_UBICACIONES_HISTORIAL).</summary>
public class TrasladoUbicacionesHistorial
{
    public DateTime? FEC_MOVIMENTO_ALMACEN       { get; set; }
    public decimal?  NUM_MOVIMIENTO_SALIDA       { get; set; }
    public decimal?  NUM_MOVIMIENTO_INGRESO      { get; set; }
    public short?    NUM_SECUENCIA               { get; set; }
    public string?   COD_TIPO_MOVIMIENTO         { get; set; }
    public string?   DES_TIPO_MOVIMIENTO         { get; set; }
    public string?   COD_PRODUCTO                { get; set; }
    public string?   DES_PRODUCTO                { get; set; }
    public string?   NUM_LOTE_CONTROL            { get; set; }
    public string?   COD_PROCESO                 { get; set; }
    public int?      NUM_ORDEN_PRODUCCION        { get; set; }
    public short?    NUM_ORDEN_PRODUCCION_ITEM   { get; set; }
    public byte?     NUM_ORDEN_PRODUCCION_UNIDAD { get; set; }
    public decimal?  CAN_PRODUCTO                { get; set; }
    public string?   COD_UNIDAD_MEDIDA           { get; set; }
    public string?   COD_UBICACION_ORIGEN        { get; set; }
    public string?   DES_UBICACION_ORIGEN        { get; set; }
    public string?   COD_UBICACION_DESTINO       { get; set; }
    public string?   DES_UBICACION_DESTINO       { get; set; }
    public string?   DES_OBSERVACION             { get; set; }
    public string?   DES_ESTADO                  { get; set; }
    public string?   DES_BACKCOLOR               { get; set; }
    public string?   DES_FORECOLOR               { get; set; }
    public string?   COD_USUARIO_REGISTRO        { get; set; }
    public DateTime? FEC_REGISTRO                { get; set; }
}
