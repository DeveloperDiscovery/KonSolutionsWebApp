namespace KONSolutions.Shared.Models.Logistica;

/// <summary>
/// Una entrega programada de UNA línea del pedido para UNA fecha.
/// LOGISTICA.PEDIDOS_COMERCIALES_CRONOGRAMA_ENTREGAS.
///
/// La clave es (operación, pedido, secuencia, FEC_ENTREGA): la misma línea puede estar en
/// varias fechas —eso es una entrega parcial— pero no dos veces en la misma fecha.
///
/// Las cuatro últimas columnas las escribe el despacho, no esta pantalla: acá se muestran.
/// </summary>
public class PedidoComercialCronogramaEntrega
{
    public string?   COD_TIPO_OPERACION       { get; set; }
    public string?   COD_CLASE_OPERACION      { get; set; }
    public decimal?  NUM_PEDIDO_COMERCIAL     { get; set; }
    public int?      NUM_SECUENCIA            { get; set; }
    public DateTime? FEC_ENTREGA              { get; set; }
    public DateTime? FEC_ENTREGA_REALIZADA    { get; set; }
    public decimal?  NUM_MOVIMIENTO_ALMACEN   { get; set; }
    public int?      NUM_SECUENCIA_MOVIMIENTO { get; set; }
    public decimal?  CAN_PRODUCTO_ENTREGAR    { get; set; }
    public decimal?  CAN_PRODUCTO_ENTREGADO   { get; set; }

    // Vienen del LEFT JOIN contra el detalle del pedido. Antes volvían siempre NULL porque
    // el join tenía COD_TIPO_OPERACION y COD_CLASE_OPERACION cruzados entre las dos tablas.
    public string?   COD_PRODUCTO             { get; set; }
    public decimal?  CAN_PRODUCTO             { get; set; }
    public string?   COD_UNIDAD_MEDIDA        { get; set; }
    public string?   DES_OBSERVACION          { get; set; }
}

public class PedidoComercialCronogramaEntregaDto
{
    public string   COD_TIPO_OPERACION       { get; set; } = "";
    public string   COD_CLASE_OPERACION      { get; set; } = "";
    public decimal  NUM_PEDIDO_COMERCIAL     { get; set; }
    public int      NUM_SECUENCIA            { get; set; }
    public DateTime FEC_ENTREGA              { get; set; }
    public DateTime? FEC_ENTREGA_REALIZADA   { get; set; }
    public decimal? NUM_MOVIMIENTO_ALMACEN   { get; set; }
    public int?     NUM_SECUENCIA_MOVIMIENTO { get; set; }
    public decimal? CAN_PRODUCTO_ENTREGAR    { get; set; }
    public decimal? CAN_PRODUCTO_ENTREGADO   { get; set; }
}
