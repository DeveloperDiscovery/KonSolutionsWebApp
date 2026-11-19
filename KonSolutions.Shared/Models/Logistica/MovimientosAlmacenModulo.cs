using KONSolutions.Shared.Common;
namespace KONSolutions.Shared.Models.Logistica;

// Modelos del módulo de Movimientos de Almacén, lado cliente.
//
// Todo anulable a propósito: son lo que devuelve la API, y una cabecera recién creada o una
// tabla de enlace vienen con la mitad de los campos en null. Un string no anulable acá
// obliga a inicializar y esconde justamente el caso que hay que ver.

/// <summary>Espejo de LOGISTICA.MovimientosAlmacen del lado cliente. Todo anulable: es lo que
/// devuelve la API, y un documento recién creado viene con la mitad en null.</summary>
public class MovimientosAlmacen
{
    public decimal? NUM_MOVIMIENTO_ALMACEN { get; set; }
    public string? COD_ALMACEN { get; set; }
    public DateTime? FEC_MOVIMENTO_ALMACEN { get; set; }
    public DateTime? FEC_RECEPCION { get; set; }
    public string? COD_CLASE_MOVIMIENTO { get; set; }
    public string? COD_TIPO_MOVIMIENTO { get; set; }
    public decimal? COD_ENTIDAD { get; set; }
    public string? COD_TIPO_COMPROBANTE_PAGO { get; set; }
    public string? NUM_COMPROBANTE_PAGO { get; set; }
    public string? DES_OBSERVACION { get; set; }
    public string? COD_CENTRO_COSTO { get; set; }
    public int? NUM_PROYECTO { get; set; }
    public string? NUM_LOTE_CONTROL { get; set; }
    public int? NUM_REQUERIMIENTO_STOCK_ATENDIDO { get; set; }
    public int? NUM_RESERVA_STOCK_ATENDIDO { get; set; }
    public string? COD_ALMACEN_SINCRONIZADO { get; set; }
    public decimal? NUM_MOVIMIENTO_ALMACEN_SINCRONIZADO { get; set; }
    public string? COD_CLASE_OPERACION { get; set; }
    public string? COD_TIPO_OPERACION { get; set; }
    public int? MUM_PEDIDO_COMERCIAL { get; set; }
    public decimal? COD_ENTIDAD_TRANSPORTISTA { get; set; }

    /// <summary>La dirección de la entidad que se eligió como el OTRO punto del traslado: el
    /// punto de llegada en una salida, el de partida en un ingreso. Es lo único de los dos
    /// puntos que el usuario decide y que se graba; el resto lo resuelve el SHOWBYID.</summary>
    public int? NUM_SECUENCIA_DIRECCION { get; set; }

    /// <summary>Los dos puntos del traslado los RESUELVE el SHOWBYID, no se graban: una salida
    /// parte de la sucursal del almacén y llega al otro punto (la sucursal del almacén
    /// sincronizado en una transferencia, la dirección elegida de la entidad en el resto); un
    /// ingreso es al revés. Acá se muestran, no se calculan. Ver ParaGrabar.</summary>
    public string? DES_DIRECCION_DESTINO { get; set; }
    public string? COD_UBIGEO_DESTINO { get; set; }

    /// <summary>Origen: dirección y ubigeo del punto de partida. Sólo de lectura — los arma el
    /// SHOWBYID, no son columnas de la tabla.</summary>
    public string? DES_DIRECCION_ORIGEN { get; set; }
    public string? DES_UBIGEO_ORIGEN { get; set; }

    /// <summary>Lo que se envía a la API al grabar: la cabecera SIN el texto de los puntos.
    ///
    /// La pantalla recibe la dirección y el ubigeo de destino, pero no debe devolverlos: el
    /// UPDATE los guardaría en las columnas de la tabla, y esa copia (la dirección de la
    /// sucursal propia, en los movimientos grabados antes de corregir el SHOWBYID) ya no
    /// coincidiría con la que se resuelve ahora. Lo único que se graba del punto de llegada es
    /// NUM_SECUENCIA_DIRECCION, la dirección elegida de la entidad.
    ///
    /// Es una copia: la pantalla sigue mostrando lo que tenía, y si el grabado falla no se pierde.</summary>
    public MovimientosAlmacen ParaGrabar()
    {
        var copia = (MovimientosAlmacen)MemberwiseClone();
        copia.DES_DIRECCION_DESTINO = null;
        copia.COD_UBIGEO_DESTINO = null;
        return copia;
    }

    public int? COD_TIPO_MOTIVO { get; set; }
    public int? COD_MOTIVO { get; set; }
    public int? COD_TIPO_ESTADO { get; set; }
    public int? COD_ESTADO { get; set; }
    public string? COD_USUARIO_REGISTRO { get; set; }
    public string? COD_ESTACION_REGISTRO { get; set; }
    public DateTime? FEC_REGISTRO { get; set; }
    public string? COD_USUARIO_ACTUALIZACION { get; set; }
    public string? COD_ESTACION_ACTUALIZACION { get; set; }
    public DateTime? FEC_ACTUALIZACION { get; set; }
    public string? DES_RESERVA_STOCK { get; set; }
    public string? DES_TIPO_MOVIMIENTO { get; set; }
    public string? DES_UBIGEO { get; set; }
    public string? DES_ABREVIADA { get; set; }
    public string? DES_ALMACEN { get; set; }
    public string? DES_COMERCIAL { get; set; }
    public string? DES_MATERNO { get; set; }
    public string? DES_NOMBRE { get; set; }
    public string? DES_NOMBRE_COMPLETO { get; set; }
    public string? DES_NOMBRE2 { get; set; }
    public string? DES_PATERNO { get; set; }
    public string? DES_DIRECCION { get; set; }
    public string? DES_INTERIOR { get; set; }
    public string? DES_KILOMETRO { get; set; }
    public string? DES_LOTE { get; set; }
    public string? DES_MANZANA { get; set; }
    public string? DES_NOMBRE_VIA { get; set; }
    public string? DES_NOMBRE_ZONA { get; set; }
    public string? DES_NUMERACION { get; set; }
    public string? DES_REFERENCIA { get; set; }
    public string? DES_BACKCOLOR { get; set; }
    public string? DES_ESTADO { get; set; }
    public string? DES_FORECOLOR { get; set; }
    public string? DES_LOTE_CONTROL { get; set; }
    public string? DES_MOTIVO { get; set; }

    /// <summary>Del tipo de movimiento, vía el SHOWBYID: con qué rótulo se pide la entidad en
    /// ESTE documento. Un movimiento de compra dice "Proveedor", uno de venta "Cliente".</summary>
    public string? COD_TIPO_ENTIDAD_ASOCIADA { get; set; }
    public string? DES_TIPO_ENTIDAD { get; set; }
    public string? DES_PEDIDO_COMERCIAL { get; set; }
    public string? DES_REQUERIMIENTO_STOCK { get; set; }
    public int? NUM_RESERVA_STOCK { get; set; }
    public string? COD_UBIGEO { get; set; }
    public decimal? NUM_REGISTRO { get; set; }
    public int? NUM_SECUENCIA { get; set; }
    public int? NUM_PEDIDO_COMERCIAL { get; set; }
    public int? NUM_REQUERIMIENTO_STOCK { get; set; }
}

/// <summary>Espejo de LOGISTICA.MovimientosAlmacenInsertDto del lado cliente. Todo anulable: es lo que
/// devuelve la API, y un documento recién creado viene con la mitad en null.</summary>
public class MovimientosAlmacenInsertDto
{
    public decimal? NUM_MOVIMIENTO_ALMACEN { get; set; }
    public string? COD_ALMACEN { get; set; }
    public DateTime? FEC_MOVIMENTO_ALMACEN { get; set; }
    public DateTime? FEC_RECEPCION { get; set; }
    public string? COD_CLASE_MOVIMIENTO { get; set; }
    public string? COD_TIPO_MOVIMIENTO { get; set; }
    public decimal? COD_ENTIDAD { get; set; }
    public string? COD_TIPO_COMPROBANTE_PAGO { get; set; }
    public string? NUM_COMPROBANTE_PAGO { get; set; }
    public string? DES_OBSERVACION { get; set; }
    public string? COD_CENTRO_COSTO { get; set; }
    public int? NUM_PROYECTO { get; set; }
    public string? NUM_LOTE_CONTROL { get; set; }
    public int? NUM_REQUERIMIENTO_STOCK_ATENDIDO { get; set; }
    public int? NUM_RESERVA_STOCK_ATENDIDO { get; set; }
    public string? COD_ALMACEN_SINCRONIZADO { get; set; }
    public decimal? NUM_MOVIMIENTO_ALMACEN_SINCRONIZADO { get; set; }
    public string? COD_CLASE_OPERACION { get; set; }
    public string? COD_TIPO_OPERACION { get; set; }
    public int? MUM_PEDIDO_COMERCIAL { get; set; }
    public decimal? NUM_REGISTRO { get; set; }
    public decimal? COD_ENTIDAD_TRANSPORTISTA { get; set; }
    public int? NUM_SECUENCIA_DIRECCION { get; set; }
    public string? DES_DIRECCION_DESTINO { get; set; }
    public string? COD_UBIGEO_DESTINO { get; set; }
    public int? COD_TIPO_MOTIVO { get; set; }
    public int? COD_MOTIVO { get; set; }
    public int? COD_TIPO_ESTADO { get; set; }
    public int? COD_ESTADO { get; set; }
    public string? COD_USUARIO_REGISTRO { get; set; }
    public string? COD_ESTACION_REGISTRO { get; set; }
}

/// <summary>Espejo de LOGISTICA.MovimientosAlmacenUpdateDto del lado cliente. Todo anulable: es lo que
/// devuelve la API, y un documento recién creado viene con la mitad en null.</summary>
public class MovimientosAlmacenUpdateDto
{
    public decimal? NUM_MOVIMIENTO_ALMACEN { get; set; }
    public string? COD_ALMACEN { get; set; }
    public DateTime? FEC_MOVIMENTO_ALMACEN { get; set; }
    public DateTime? FEC_RECEPCION { get; set; }
    public string? COD_CLASE_MOVIMIENTO { get; set; }
    public string? COD_TIPO_MOVIMIENTO { get; set; }
    public decimal? COD_ENTIDAD { get; set; }
    public string? COD_TIPO_COMPROBANTE_PAGO { get; set; }
    public string? NUM_COMPROBANTE_PAGO { get; set; }
    public string? DES_OBSERVACION { get; set; }
    public string? COD_CENTRO_COSTO { get; set; }
    public int? NUM_PROYECTO { get; set; }
    public string? NUM_LOTE_CONTROL { get; set; }
    public int? NUM_REQUERIMIENTO_STOCK_ATENDIDO { get; set; }
    public int? NUM_RESERVA_STOCK_ATENDIDO { get; set; }
    public string? COD_ALMACEN_SINCRONIZADO { get; set; }
    public decimal? NUM_MOVIMIENTO_ALMACEN_SINCRONIZADO { get; set; }
    public string? COD_CLASE_OPERACION { get; set; }
    public string? COD_TIPO_OPERACION { get; set; }
    public int? MUM_PEDIDO_COMERCIAL { get; set; }
    public decimal? NUM_REGISTRO { get; set; }
    public decimal? COD_ENTIDAD_TRANSPORTISTA { get; set; }
    public int? NUM_SECUENCIA_DIRECCION { get; set; }
    public string? DES_DIRECCION_DESTINO { get; set; }
    public string? COD_UBIGEO_DESTINO { get; set; }
    public int? COD_TIPO_MOTIVO { get; set; }
    public int? COD_MOTIVO { get; set; }
    public int? COD_TIPO_ESTADO { get; set; }
    public int? COD_ESTADO { get; set; }
    public string? COD_USUARIO_REGISTRO { get; set; }
    public string? COD_ESTACION_REGISTRO { get; set; }
}

/// <summary>Espejo de LOGISTICA.MovimientosAlmacenDetalle del lado cliente. Todo anulable: es lo que
/// devuelve la API, y un documento recién creado viene con la mitad en null.</summary>
public class MovimientosAlmacenDetalle
{
    public decimal? NUM_MOVIMIENTO_ALMACEN { get; set; }
    public int? NUM_SECUENCIA { get; set; }
    public string? COD_PRODUCTO { get; set; }
    public string? COD_CLASE_MOVIMIENTO { get; set; }
    public string? COD_TIPO_MOVIMIENTO { get; set; }
    public decimal? CAN_PRODUCTO { get; set; }
    public string? COD_UNIDAD_MEDIDA { get; set; }
    public decimal? CAN_PRODUCTO_SECUNDARIO { get; set; }
    public string? COD_UNIDAD_MEDIDA_SECUNDARIO { get; set; }
    public string? COD_ALMACEN { get; set; }
    public string? COD_UBICACION_ALMACEN { get; set; }
    public decimal? NUM_MOVIMIENTO_ALMACEN_SINCRONIZADO { get; set; }
    public string? COD_ALMACEN_SINCRONIZADO { get; set; }
    public string? COD_UBICACION_ALMACEN_SINCRONIZADO { get; set; }
    public int? NUM_REQUERIMIENTO_STOCK_ATENDIDO { get; set; }
    public int? NUM_RESERVA_STOCK_ATENDIDO { get; set; }
    public int? COD_TIPO_ESTADO { get; set; }
    public int? COD_ESTADO { get; set; }
    public string? COD_USUARIO_REGISTRO { get; set; }
    public string? COD_ESTACION_REGISTRO { get; set; }
    public DateTime? FEC_REGISTRO { get; set; }
    public string? COD_USUARIO_ACTUALIZACION { get; set; }
    public string? COD_ESTACION_ACTUALIZACION { get; set; }
    public DateTime? FEC_ACTUALIZACION { get; set; }
    public string? DES_UBICACION_ALMACEN { get; set; }
    public string? DES_BACKCOLOR { get; set; }
    public string? DES_ESTADO { get; set; }
    public string? DES_FORECOLOR { get; set; }
    public string? DES_DIRECCION_DESTINO { get; set; }
    public string? DES_OBSERVACION { get; set; }
    public string? DES_DETALLADA { get; set; }
    public string? DES_PRODUCTO { get; set; }
    public string? DES_TIPO_MOVIMIENTO { get; set; }
    public string? DES_UNIDAD_MEDIDA { get; set; }

    // ── Unidad secundaria, lote y orden de producción ────────────────────────
    // Las columnas EXISTEN en LOGISTICA.MOVIMIENTOS_ALMACEN_DETALLE y el SHOWALL ya
    // devolvía casi todas; lo que faltaba era espejarlas acá. Un campo que el SP manda y
    // el modelo no tiene, Dapper lo descarta en silencio: la grilla lo muestra vacío y no
    // hay ningún error que lo delate.
    //
    // Anulables: una compra que no es de consumo de producción no lleva lote ni orden, y
    // eso es lo normal, no un error.
    public string?  DES_UNIDAD_MEDIDA_SECUNDARIO { get; set; }
    public string?  NUM_LOTE_CONTROL            { get; set; }
    public string?  COD_PROCESO                 { get; set; }
    public string?  DES_PROCESO                 { get; set; }
    public int?     NUM_ORDEN_PRODUCCION        { get; set; }
    public string?  DES_ORDEN_PRODUCCION        { get; set; }
    public short?   NUM_ORDEN_PRODUCCION_ITEM   { get; set; }
    public byte?    NUM_ORDEN_PRODUCCION_UNIDAD { get; set; }

    // ── De dónde viene la línea ──────────────────────────────────────────────
    // Estas cuatro deciden si la línea se puede tocar a mano. Una línea con secuencia de
    // requerimiento o de pedido comercial NO se edita ni se borra desde acá: se quita
    // desde el panel que la asignó, que es el que además devuelve lo atendido a su
    // documento de origen. Borrarla de este lado dejaría el requerimiento o la orden
    // creyendo que sigue atendida.
    //
    // El SHOWALL ya las devolvía; faltaban en el modelo, y lo que Dapper no encuentra en
    // el modelo lo descarta sin avisar.
    public int? NUM_SECUENCIA_REQUERIMIENTO_STOCK { get; set; }
    public int? NUM_SECUENCIA_PEDIDO_COMERCIAL    { get; set; }
    public int? MUM_PEDIDO_COMERCIAL              { get; set; }
    public int? NUM_SECUENCIA_RESERVA_STOCK       { get; set; }
    public string? COD_CLASE_OPERACION            { get; set; }
    public string? COD_TIPO_OPERACION             { get; set; }
}

/// <summary>Espejo de LOGISTICA.MovimientosAlmacenDetalleInsertDto del lado cliente. Todo anulable: es lo que
/// devuelve la API, y un documento recién creado viene con la mitad en null.</summary>
public class MovimientosAlmacenDetalleInsertDto
{
    public decimal? NUM_MOVIMIENTO_ALMACEN { get; set; }
    public int? NUM_SECUENCIA { get; set; }
    public string? COD_PRODUCTO { get; set; }
    public string? COD_CLASE_MOVIMIENTO { get; set; }
    public string? COD_TIPO_MOVIMIENTO { get; set; }
    public decimal? CAN_PRODUCTO { get; set; }
    public string? COD_UNIDAD_MEDIDA { get; set; }
    public decimal? CAN_PRODUCTO_SECUNDARIO { get; set; }
    public string? COD_UNIDAD_MEDIDA_SECUNDARIO { get; set; }
    public string? COD_ALMACEN { get; set; }
    public string? COD_UBICACION_ALMACEN { get; set; }
    public decimal? NUM_MOVIMIENTO_ALMACEN_SINCRONIZADO { get; set; }
    public string? COD_ALMACEN_SINCRONIZADO { get; set; }
    public string? COD_UBICACION_ALMACEN_SINCRONIZADO { get; set; }
    public int? NUM_REQUERIMIENTO_STOCK_ATENDIDO { get; set; }
    public int? NUM_RESERVA_STOCK_ATENDIDO { get; set; }
    public int? COD_TIPO_ESTADO { get; set; }
    public int? COD_ESTADO { get; set; }
    public string? COD_USUARIO_REGISTRO { get; set; }
    public string? COD_ESTACION_REGISTRO { get; set; }

    // Lo que el SP también recibe: lote, proceso y orden (en null, los de Configuración
    // General), la línea del ingreso destino y los enlaces con el documento de origen.
    public int?     NUM_SECUENCIA_ALMACEN_SINCRONIZADO  { get; set; }
    public string?  NUM_LOTE_CONTROL                    { get; set; }
    public string?  COD_PROCESO                         { get; set; }
    public int?     NUM_ORDEN_PRODUCCION                { get; set; }
    public short?   NUM_ORDEN_PRODUCCION_ITEM           { get; set; }
    public byte?    NUM_ORDEN_PRODUCCION_UNIDAD         { get; set; }
    public int?     NUM_SECUENCIA_REQUERIMIENTO_STOCK   { get; set; }
    public int?     NUM_SECUENCIA_RESERVA_STOCK         { get; set; }
    public string?  COD_CLASE_OPERACION                 { get; set; }
    public string?  COD_TIPO_OPERACION                  { get; set; }
    public int?     MUM_PEDIDO_COMERCIAL                { get; set; }
    public int?     NUM_SECUENCIA_PEDIDO_COMERCIAL      { get; set; }
}

/// <summary>Espejo de LOGISTICA.MovimientosAlmacenDetalleUpdateDto del lado cliente. Todo anulable: es lo que
/// devuelve la API, y un documento recién creado viene con la mitad en null.</summary>
public class MovimientosAlmacenDetalleUpdateDto
{
    public decimal? NUM_MOVIMIENTO_ALMACEN { get; set; }
    public int? NUM_SECUENCIA { get; set; }
    public string? COD_PRODUCTO { get; set; }
    public string? COD_CLASE_MOVIMIENTO { get; set; }
    public string? COD_TIPO_MOVIMIENTO { get; set; }
    public decimal? CAN_PRODUCTO { get; set; }
    public string? COD_UNIDAD_MEDIDA { get; set; }
    public decimal? CAN_PRODUCTO_SECUNDARIO { get; set; }
    public string? COD_UNIDAD_MEDIDA_SECUNDARIO { get; set; }
    public string? COD_ALMACEN { get; set; }
    public string? COD_UBICACION_ALMACEN { get; set; }
    public decimal? NUM_MOVIMIENTO_ALMACEN_SINCRONIZADO { get; set; }
    public string? COD_ALMACEN_SINCRONIZADO { get; set; }
    public string? COD_UBICACION_ALMACEN_SINCRONIZADO { get; set; }
    public int? NUM_REQUERIMIENTO_STOCK_ATENDIDO { get; set; }
    public int? NUM_RESERVA_STOCK_ATENDIDO { get; set; }
    public int? COD_TIPO_ESTADO { get; set; }
    public int? COD_ESTADO { get; set; }
    public string? COD_USUARIO_REGISTRO { get; set; }
    public string? COD_ESTACION_REGISTRO { get; set; }

    // Lo que el SP también recibe: lote, proceso y orden (en null, los de Configuración
    // General), la línea del ingreso destino y los enlaces con el documento de origen.
    public int?     NUM_SECUENCIA_ALMACEN_SINCRONIZADO  { get; set; }
    public string?  NUM_LOTE_CONTROL                    { get; set; }
    public string?  COD_PROCESO                         { get; set; }
    public int?     NUM_ORDEN_PRODUCCION                { get; set; }
    public short?   NUM_ORDEN_PRODUCCION_ITEM           { get; set; }
    public byte?    NUM_ORDEN_PRODUCCION_UNIDAD         { get; set; }
    public int?     NUM_SECUENCIA_REQUERIMIENTO_STOCK   { get; set; }
    public int?     NUM_SECUENCIA_RESERVA_STOCK         { get; set; }
    public string?  COD_CLASE_OPERACION                 { get; set; }
    public string?  COD_TIPO_OPERACION                  { get; set; }
    public int?     MUM_PEDIDO_COMERCIAL                { get; set; }
    public int?     NUM_SECUENCIA_PEDIDO_COMERCIAL      { get; set; }
}

/// <summary>LOGISTICA.MOVIMIENTOS_ALMACEN_TRANSPORTE · Transportes asignados a un movimiento. Es la unica de las cinco tablas hijas que tiene datos propios —acarreo y observacion—, por eso lleva UPDATE.</summary>
public class MovimientosAlmacenTransporte
{
    public decimal? NUM_MOVIMIENTO_ALMACEN { get; set; }
    public string? COD_TRANSPORTE { get; set; }
    public bool? FLG_ACARREO { get; set; }
    public string? DES_OBSERVACION { get; set; }
    public string? COD_USUARIO_REGISTRO { get; set; }
    public string? COD_ESTACION_REGISTRO { get; set; }
    public DateTime? FEC_REGISTRO { get; set; }
    public string? DES_TRANSPORTE { get; set; }
    public string? DES_PLACA { get; set; }
    public string? DES_ENTIDAD { get; set; }
}

/// <summary>LOGISTICA.MOVIMIENTOS_ALMACEN_REQUERIMIENTOS_STOCK · Enlace entre un movimiento y los requerimientos de stock que atiende. Solo a nivel documento: este origen no tiene enlace por linea.</summary>
public class MovimientosAlmacenRequerimientosStock
{
    public decimal? NUM_MOVIMIENTO_ALMACEN { get; set; }
    public int? NUM_REQUERIMIENTO_STOCK { get; set; }
    public string? COD_USUARIO_REGISTRO { get; set; }
    public string? COD_ESTACION_REGISTRO { get; set; }
    public DateTime? FEC_REGISTRO { get; set; }
    public string? DES_REQUERIMIENTO_STOCK { get; set; }
    public DateTime? FEC_REQUERIMIENTO_STOCK { get; set; }
}

/// <summary>LOGISTICA.MOVIMIENTOS_ALMACEN_RESERVAS_STOCK · Enlace entre un movimiento y las reservas de stock que atiende, a nivel documento.</summary>
public class MovimientosAlmacenReservasStock
{
    public decimal? NUM_MOVIMIENTO_ALMACEN { get; set; }
    public int? NUM_RESERVA_STOCK { get; set; }
    public string? COD_USUARIO_REGISTRO { get; set; }
    public string? COD_ESTACION_REGISTRO { get; set; }
    public DateTime? FEC_REGISTRO { get; set; }
    public string? DES_RESERVA_STOCK { get; set; }
    public DateTime? FEC_RESERVA_STOCK { get; set; }
}

/// <summary>LOGISTICA.MOVIMIENTOS_ALMACEN_RESERVAS_STOCK_DETALLE · Enlace a nivel LINEA con una reserva. Reservas es el unico origen que enlaza en los dos niveles, y por eso se puede tomar una reserva entera o lineas sueltas.</summary>
public class MovimientosAlmacenReservasStockDetalle
{
    public decimal? NUM_MOVIMIENTO_ALMACEN { get; set; }
    public int? NUM_RESERVA_STOCK { get; set; }
    public int? NUM_SECUENCIA { get; set; }
    public string? COD_USUARIO_REGISTRO { get; set; }
    public string? COD_ESTACION_REGISTRO { get; set; }
    public DateTime? FEC_REGISTRO { get; set; }
    public string? COD_PRODUCTO { get; set; }
    public string? DES_PRODUCTO { get; set; }
    public decimal? CAN_PRODUCTO { get; set; }
}

/// <summary>LOGISTICA.MOVIMIENTOS_ALMACEN_PEDIDOS_COMERCIALES · Enlace entre un movimiento y las ordenes de compra que atiende, a nivel documento.</summary>
public class MovimientosAlmacenPedidosComerciales
{
    public decimal? NUM_MOVIMIENTO_ALMACEN { get; set; }
    public int? NUM_PEDIDO_COMERCIAL { get; set; }
    public string? COD_USUARIO_REGISTRO { get; set; }
    public string? COD_ESTACION_REGISTRO { get; set; }
    public DateTime? FEC_REGISTRO { get; set; }
    public string? DES_PEDIDO_COMERCIAL { get; set; }
    public DateTime? FEC_PEDIDO_COMERCIAL { get; set; }
}

/// <summary>LOGISTICA.MOVIMIENTOS_ALMACEN_PEDIDOS_COMERCIALES_DETALLE · Enlace a nivel LINEA con una orden de compra.</summary>
public class MovimientosAlmacenPedidosComercialesDetalle
{
    public decimal? NUM_MOVIMIENTO_ALMACEN { get; set; }
    public int? NUM_PEDIDO_COMERCIAL { get; set; }
    public int? NUM_SECUENCIA { get; set; }
    public string? COD_USUARIO_REGISTRO { get; set; }
    public string? COD_ESTACION_REGISTRO { get; set; }
    public DateTime? FEC_REGISTRO { get; set; }
    public string? COD_PRODUCTO { get; set; }
    public string? DES_PRODUCTO { get; set; }
    public decimal? CAN_PRODUCTO { get; set; }
}

/// <summary>Los ocho combos de la cabecera en un viaje.
///
/// El orden de las propiedades espeja el orden de los conjuntos que devuelve
/// PROC_LOGISTICA_MOVIMIENTOS_ALMACEN_LOAD_COMBOS. Es un contrato: si se cambia allá, acá
/// los combos salen cruzados y sin ningún error que lo delate.</summary>
public class MovimientosAlmacenLoadCombosResult
{
    public int                CodTipoEstado        { get; set; }
    public List<ComboboxItem> Almacenes            { get; set; } = new();
    public List<ComboboxItem> TiposMovimiento      { get; set; } = new();
    public List<ComboboxItem> Entidades            { get; set; } = new();
    public List<ComboboxItem> TiposComprobantePago { get; set; } = new();
    public List<ComboboxItem> CentrosCosto         { get; set; } = new();
    public List<ComboboxItem> LotesControl         { get; set; } = new();
    public List<ComboboxItem> UnidadesMedida       { get; set; } = new();
    public List<EstadoItem>   Estados              { get; set; } = new();

    /// <summary>La sucursal de cada almacén de <see cref="Almacenes"/> (sale de las mismas filas del
    /// primer conjunto; no cambia el orden de los conjuntos). Con ella la cabecera ofrece, en una
    /// transferencia, solo los almacenes destino que el alcance del tipo permite. Vacía si la API es
    /// anterior a este campo: la pantalla entonces no filtra por sucursal.</summary>
    public List<AlmacenSucursalItem> AlmacenesSucursal { get; set; } = new();
}

/// <summary>A qué sucursal pertenece un almacén. COD_SUCURSAL es opcional en LOGISTICA.ALMACENES:
/// un almacén sin sucursal llega con CodSucursal en null.</summary>
public class AlmacenSucursalItem
{
    public string  CodAlmacen  { get; set; } = string.Empty;
    public string? CodSucursal { get; set; }
    public string? DesSucursal { get; set; }
}

/// <summary>Lo que manda el botón «Agregar al detalle».
///
/// DES_SECUENCIAS acepta "1,3,7" para bajar solo esas líneas; en null baja todas las que
/// tengan cantidad pendiente. Se manda solo la clave del origen que corresponda.</summary>
/// <summary>Una clave de stock con disponible en el almacén del movimiento: de acá salen las
/// líneas de una salida cargada a mano (GET movimientos-almacen-detalle/stock-disponible).</summary>
public class MovimientosAlmacenDetalleStockDisponible
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
    public decimal? CAN_DISPONIBLE               { get; set; }
    public decimal? CAN_DISPONIBLE_SECUNDARIO    { get; set; }
}

/// <summary>Un producto con sus unidades, para cargar a mano la línea de un ingreso.</summary>
public class MovimientosAlmacenDetalleProducto
{
    public string? COD_PRODUCTO                 { get; set; }
    public string? DES_PRODUCTO                 { get; set; }
    public string? COD_UNIDAD_MEDIDA            { get; set; }
    public string? COD_UNIDAD_MEDIDA_SECUNDARIO { get; set; }
}

/// <summary>Los combos de la línea (GET movimientos-almacen-detalle/combos-linea).</summary>
public class MovimientosAlmacenDetalleCombosLinea
{
    public List<ComboboxItem> Ubicaciones        { get; set; } = new();
    public List<ComboboxItem> UbicacionesDestino { get; set; } = new();
    public List<MovimientosAlmacenDetalleProducto> Productos { get; set; } = new();
}

/// <summary>Lo que devuelve el endpoint del número al imprimir.
///
/// FLG_ASIGNADO_AHORA distingue el número recién consumido de uno que ya estaba: la pantalla
/// avisa sólo la primera vez, no en cada reimpresión.
///
/// NUM_COMPROBANTE_PAGO en null no es un error: ese movimiento no lleva guía con correlativo.</summary>
public class MovimientosAlmacenNumeroImpresionResult
{
    public string? NUM_COMPROBANTE_PAGO { get; set; }
    public bool FLG_ASIGNADO_AHORA { get; set; }
}

public class MovimientosAlmacenAgregarDto
{
    public int? NUM_RESERVA_STOCK       { get; set; }
    public int? NUM_REQUERIMIENTO_STOCK { get; set; }
    public int? NUM_PEDIDO_COMERCIAL    { get; set; }
    public string? DES_SECUENCIAS       { get; set; }
}

/// <summary>La respuesta del botón. Cero líneas NO es un error: quiere decir que ya estaba
/// todo agregado o que no queda pendiente, y el mensaje viene listo para mostrar.</summary>
public class MovimientosAlmacenAgregarResult
{
    public int    CAN_LINEAS_AGREGADAS { get; set; }
    public string DES_MENSAJE          { get; set; } = string.Empty;
}

// ── Devolución de compra: guías del mismo proveedor, items y cantidades a devolver ──────────────
public class MovimientoDevolucionGuia
{
    public int NUM_MOVIMIENTO_ALMACEN_ORIGEN { get; set; }
    public string? COD_TIPO_COMPROBANTE_PAGO { get; set; }
    public string? NUM_COMPROBANTE_PAGO { get; set; }
    public string DES_GUIA { get; set; } = string.Empty;
    public DateTime? FEC_MOVIMENTO_ALMACEN { get; set; }
    public string? COD_TIPO_MOVIMIENTO { get; set; }
    public int CAN_LINEAS { get; set; }
    public decimal CAN_RECIBIDA { get; set; }
    public decimal CAN_SALDO { get; set; }
    public decimal CAN_DEVUELTA { get; set; }
    public bool FLG_ASOCIADA { get; set; }
}

public class MovimientoDevolucionLinea
{
    public int NUM_MOVIMIENTO_ALMACEN_ORIGEN { get; set; }
    public int NUM_SECUENCIA_ORIGEN { get; set; }
    public string DES_GUIA { get; set; } = string.Empty;
    public string COD_PRODUCTO { get; set; } = string.Empty;
    public string? DES_PRODUCTO { get; set; }
    public string? COD_UNIDAD_MEDIDA { get; set; }
    public string? NUM_LOTE_CONTROL { get; set; }
    public string? COD_UBICACION_ALMACEN { get; set; }
    public decimal CAN_RECIBIDA { get; set; }
    public decimal CAN_YA_DEVUELTA { get; set; }
    public decimal CAN_SALDO { get; set; }
    public int? NUM_SECUENCIA_DEVOLUCION { get; set; }
    public decimal CAN_DEVOLVER { get; set; }
    /// <summary>Ubicación de distribución del almacén: de ahí sale la devolución.</summary>
    public string? COD_UBICACION_DISTRIBUCION { get; set; }
    /// <summary>Stock disponible del producto/lote en esa ubicación (más lo que esta devolución ya tomó).</summary>
    public decimal CAN_STOCK_DISTRIBUCION { get; set; }
    /// <summary>Lo máximo que se puede devolver: el menor entre el saldo de la guía y el stock en distribución.</summary>
    public decimal CAN_MAXIMA { get; set; }
}

public class MovimientoDevolucionItemDto
{
    public int NUM_MOVIMIENTO_ALMACEN_ORIGEN { get; set; }
    public int NUM_SECUENCIA_ORIGEN { get; set; }
    /// <summary>Cantidad a devolver; 0 quita el item de la devolución.</summary>
    public decimal CAN_DEVOLVER { get; set; }
}

public class MovimientoDevolucionAdicionarDto
{
    public List<MovimientoDevolucionItemDto> Items { get; set; } = new();
}
