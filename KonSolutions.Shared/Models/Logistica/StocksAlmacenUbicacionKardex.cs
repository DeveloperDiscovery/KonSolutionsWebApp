namespace KONSolutions.Shared.Models.Logistica;

/// <summary>Una fila del kardex de una línea de stock: una línea de movimiento de almacén con
/// su ingreso o salida y el saldo acumulado hasta ella.
///
/// Es el resultado de PROC_LOGISTICA_STOCKS_ALMACEN_UBICACION_KARDEX. Si se pide desde una
/// fecha, la primera fila es el saldo inicial (FLG_SALDO_INICIAL = true): sólo trae la fecha y
/// el saldo, el resto va en nulo.</summary>
public class StocksAlmacenUbicacionKardex
{
    public bool?     FLG_SALDO_INICIAL                   { get; set; }
    public DateTime? FEC_MOVIMIENTO                      { get; set; }
    public decimal?  NUM_MOVIMIENTO_ALMACEN              { get; set; }
    public short?    NUM_SECUENCIA                       { get; set; }
    public string?   COD_CLASE_MOVIMIENTO                { get; set; }
    public string?   COD_TIPO_MOVIMIENTO                 { get; set; }
    public string?   DES_TIPO_MOVIMIENTO                 { get; set; }
    public bool?     FLG_POSITIVO                        { get; set; }

    // ── Con quién y con qué documento ──────────────────────────────────────
    public decimal?  COD_ENTIDAD                         { get; set; }
    public string?   DES_ENTIDAD                         { get; set; }
    public string?   COD_TIPO_COMPROBANTE_PAGO           { get; set; }
    public string?   NUM_COMPROBANTE_PAGO                { get; set; }
    public int?      NUM_REQUERIMIENTO_STOCK_ATENDIDO    { get; set; }
    public int?      NUM_RESERVA_STOCK_ATENDIDO          { get; set; }
    public int?      MUM_PEDIDO_COMERCIAL                { get; set; }
    public decimal?  NUM_MOVIMIENTO_ALMACEN_SINCRONIZADO { get; set; }
    public string?   DES_OBSERVACION                     { get; set; }

    // ── Cuánto ──────────────────────────────────────────────────────────────
    public decimal?  CAN_INGRESO                         { get; set; }
    public decimal?  CAN_SALIDA                          { get; set; }
    public decimal?  CAN_SALDO                           { get; set; }
    public string?   COD_UNIDAD_MEDIDA                   { get; set; }

    public string?   DES_ESTADO                          { get; set; }
    public string?   DES_BACKCOLOR                       { get; set; }
    public string?   DES_FORECOLOR                       { get; set; }
    public string?   COD_USUARIO_REGISTRO                { get; set; }
    /// <summary>El nombre del usuario que registró el movimiento (SEGURIDAD.USUARIOS).</summary>
    public string?   DES_USUARIO                         { get; set; }
    public DateTime? FEC_REGISTRO                        { get; set; }
}

/// <summary>La clave completa de la línea de stock y el rango de fechas, ambas opcionales.
/// Los nombres son los de los parámetros del procedimiento: Dapper los pasa tal cual.</summary>
public class StocksAlmacenUbicacionKardexDto
{
    public string?   COD_ALMACEN                  { get; set; }
    public string?   COD_UBICACION_ALMACEN        { get; set; }
    public string?   COD_PRODUCTO                 { get; set; }
    public string?   NUM_LOTE_CONTROL             { get; set; }
    public string?   COD_PROCESO                  { get; set; }
    public int?      NUM_ORDEN_PRODUCCION         { get; set; }
    public short?    NUM_ORDEN_PRODUCCION_ITEM    { get; set; }
    public byte?     NUM_ORDEN_PRODUCCION_UNIDAD  { get; set; }
    public DateTime? FEC_INICIAL                  { get; set; }
    public DateTime? FEC_FINAL                    { get; set; }
}
