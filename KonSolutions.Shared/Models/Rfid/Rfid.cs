using System.ComponentModel.DataAnnotations;
namespace KONSolutions.Shared.Models.Rfid;

// Módulo RFID: emisión de códigos de etiquetas (TAG). El código de una etiqueta es  TID + correlativo de 15 dígitos
// (ESTANDAR.OBJETOS_RFID.NUM_CORRELATIVO) y lo que se graba en la memoria EPC del chip es el correlativo: 9 ceros + los 15 dígitos.

// ───────────────────────── Equipos ─────────────────────────

/// <summary>Un equipo RFID -impresora, antena de escritura, lector- es un PRODUCTO (ESTANDAR.PRODUCTOS) y sus datos son las especificaciones técnicas de su ficha
/// (ESTANDAR.PRODUCTOS_ESPECIFICACIONES_TECNICAS). Qué productos son equipos y qué especificación guarda cada dato se dice en Configuración general &gt; RFID.
/// Solo lectura: los equipos se registran en Productos y Ficha técnica. Un dato sin especificación configurada, o que el producto no tiene, viene nulo.</summary>
public class RfidEquipo
{
    public string COD_PRODUCTO { get; set; } = string.Empty;
    public string? DES_PRODUCTO { get; set; }
    public decimal? COD_ENTIDAD_PROPIETARIA { get; set; }
    public string? COD_GRUPO_PRODUCTO { get; set; }
    public string? COD_FAMILIA_PRODUCTO { get; set; }
    public string? COD_SUB_FAMILIA_PRODUCTO { get; set; }
    public string? DES_GRUPO_PRODUCTO { get; set; }
    public string? DES_FAMILIA_PRODUCTO { get; set; }
    public string? DES_SUB_FAMILIA_PRODUCTO { get; set; }
    /// <summary>La entidad fabricante: la del item de la marca (o la del modelo). Impinj, Invengo, Chainway, Zebra...</summary>
    public decimal? COD_ENTIDAD_MARCA { get; set; }
    public string? DES_ENTIDAD_MARCA { get; set; }
    public string? DES_MARCA { get; set; }
    public string? DES_MODELO { get; set; }
    /// <summary>LLRP, REST, SDK, ZPL... según la lista de la especificación del protocolo.</summary>
    public string? DES_PROTOCOLO { get; set; }
    /// <summary>IP, nombre del equipo o puerto serie.</summary>
    public string? VAL_DIRECCION { get; set; }
    /// <summary>Puerto de comunicación (el TCP al que se conecta el agente).</summary>
    public string? VAL_PUERTO { get; set; }
    /// <summary>Puertos de antena con que escribe (por ejemplo "1,2"). Si la especificación tiene varias filas se juntan con coma.</summary>
    public string? VAL_PUERTOS_ANTENA { get; set; }
    /// <summary>Fijo · Portátil · Impresora... No es una especificación: es la sub familia del producto (si no tiene, su familia; si no, su grupo).</summary>
    public string? DES_TIPO_EQUIPO { get; set; }
    /// <summary>Potencia de transmisión con que se escribe (TX POWER).</summary>
    public string? VAL_POTENCIA { get; set; }
    /// <summary>La estación donde corre el agente que le habla: el COD_PRODUCTO del producto-estación (el PC), tal como lo dice la ficha.</summary>
    public string? COD_ESTACION_AGENTE { get; set; }
    /// <summary>Descripción del producto-estación. Solo viene si es un producto del grupo (y familia) de las estaciones de Configuración general &gt; RFID.</summary>
    public string? DES_ESTACION_AGENTE { get; set; }
    /// <summary>Nombre del archivo de la plantilla de impresión (ZPL) de las impresoras, tal como lo dice la ficha (sin carpetas).</summary>
    public string? VAL_PLANTILLA_IMPRESION { get; set; }
    /// <summary>Ruta completa de la plantilla: la carpeta de Rutas de archivos que dice Configuración general &gt; RFID + el nombre del archivo. Nula si falta alguna de las dos.</summary>
    public string? RUTA_PLANTILLA_IMPRESION { get; set; }
    /// <summary>Lo que está mal en la ficha (estación que no es una estación, nombre de plantilla con carpetas, ruta sin configurar...). Nulo si todo está bien.</summary>
    public string? DES_PROBLEMAS_FICHA { get; set; }
}

/// <summary>Una fila del combo de equipos: lo justo para reconocerlos al emitir.</summary>
public class RfidEquipoCombo
{
    public string COD_PRODUCTO { get; set; } = string.Empty;
    public string? DES_PRODUCTO { get; set; }
    public string? DES_ENTIDAD_MARCA { get; set; }
    public string? DES_MARCA { get; set; }
    public string? DES_MODELO { get; set; }
    public string? DES_PROTOCOLO { get; set; }
    public string? DES_TIPO_EQUIPO { get; set; }
}

// ───────────────────────── Emisiones ─────────────────────────

/// <summary>Una emisión: una tanda de etiquetas pedida a partir de un ingreso por compra (C) o del stock (S).
/// Los contadores y el estado salen de las etiquetas: ANULADA, PENDIENTE (ninguna enrolada), EN PROCESO o TERMINADA.</summary>
public class RfidEmision
{
    public int NUM_EMISION { get; set; }
    public string COD_ORIGEN { get; set; } = string.Empty;
    public string? DES_ORIGEN { get; set; }
    public DateTime? FEC_EMISION { get; set; }
    public decimal? NUM_MOVIMIENTO_ALMACEN { get; set; }
    public string? COD_ALMACEN { get; set; }
    public string? DES_ALMACEN { get; set; }
    public string? COD_CLASE_MOVIMIENTO { get; set; }
    public string? COD_TIPO_MOVIMIENTO { get; set; }
    public string? DES_TIPO_MOVIMIENTO { get; set; }
    public DateTime? FEC_MOVIMENTO_ALMACEN { get; set; }
    public decimal? COD_ENTIDAD { get; set; }
    public string? DES_ENTIDAD { get; set; }
    public string? NUM_COMPROBANTE_PAGO { get; set; }
    /// <summary>El equipo (producto) con que se emite.</summary>
    public string? COD_PRODUCTO_EQUIPO { get; set; }
    public string? DES_EQUIPO { get; set; }
    public bool FLG_ANULADA { get; set; }
    public string? DES_OBSERVACION { get; set; }
    /// <summary>Etiquetas vivas (pendientes + enroladas; las anuladas no cuentan).</summary>
    public int CAN_ETIQUETAS { get; set; }
    public int CAN_PENDIENTES { get; set; }
    public int CAN_ENROLADAS { get; set; }
    public int CAN_GRABADAS { get; set; }
    public int CAN_ANULADAS { get; set; }
    public int CAN_LINEAS { get; set; }
    /// <summary>A anulada · P pendiente · E en proceso · T terminada.</summary>
    public string COD_ESTADO_EMISION { get; set; } = string.Empty;
    public string? DES_ESTADO_EMISION { get; set; }
    public string? COD_USUARIO_ANULACION { get; set; }
    public string? COD_ESTACION_ANULACION { get; set; }
    public DateTime? FEC_ANULACION { get; set; }
    public string? DES_MOTIVO_ANULACION { get; set; }
    public string? COD_USUARIO_REGISTRO { get; set; }
    public string? COD_ESTACION_REGISTRO { get; set; }
    public DateTime? FEC_REGISTRO { get; set; }
    public string? COD_USUARIO_ACTUALIZACION { get; set; }
    public string? COD_ESTACION_ACTUALIZACION { get; set; }
    public DateTime? FEC_ACTUALIZACION { get; set; }
}

public class RfidEmisionSearchDto
{
    public string? SEARCH { get; set; }
    /// <summary>C ingreso por compra · S stock.</summary>
    public string? COD_ORIGEN { get; set; }
    /// <summary>A anulada · P pendiente · E en proceso · T terminada.</summary>
    public string? COD_ESTADO_EMISION { get; set; }
    public DateTime? FEC_DESDE { get; set; }
    public DateTime? FEC_HASTA { get; set; }
}

/// <summary>Una línea del pedido de emisión.
/// Ingreso por compra: SEC (la línea del ingreso) y N etiquetas de cantidad CANT (1 por unidad), o ROLLOS: una etiqueta por rollo
/// con su peso o metraje. Stock: UBI + PROD + LOTE/PROC/OP/ITEM/UNI (la clave de la fila de stock del almacén de la emisión).</summary>
public class RfidEmisionLineaDto
{
    public int? SEC { get; set; }
    public string? UBI { get; set; }
    public string? PROD { get; set; }
    public string? LOTE { get; set; }
    public string? PROC { get; set; }
    public int? OP { get; set; }
    public int? ITEM { get; set; }
    public int? UNI { get; set; }
    public int? N { get; set; }
    public decimal? CANT { get; set; }
    public List<decimal>? ROLLOS { get; set; }
}

public class RfidEmisionInsertDto
{
    /// <summary>C ingreso por compra · S stock.</summary>
    [Required, StringLength(1, MinimumLength = 1)] public string COD_ORIGEN { get; set; } = string.Empty;
    public decimal? NUM_MOVIMIENTO_ALMACEN { get; set; }
    [StringLength(5)] public string? COD_ALMACEN { get; set; }
    /// <summary>El producto que es el equipo (debe ser un equipo RFID según Configuración general &gt; RFID).</summary>
    [StringLength(30)] public string? COD_PRODUCTO_EQUIPO { get; set; }
    [StringLength(500)] public string? DES_OBSERVACION { get; set; }
    [Required, MinLength(1)] public List<RfidEmisionLineaDto> LINEAS { get; set; } = new();
}

/// <summary>Lo que devuelve el SP _INSERT.</summary>
public class RfidEmisionCreada
{
    public int NUM_EMISION { get; set; }
    public int CAN_LINEAS { get; set; }
    public int CAN_ETIQUETAS { get; set; }
}

public class RfidEmisionUpdateDto
{
    [Required] public int NUM_EMISION { get; set; }
    /// <summary>El producto que es el equipo (debe ser un equipo RFID según Configuración general &gt; RFID).</summary>
    [StringLength(30)] public string? COD_PRODUCTO_EQUIPO { get; set; }
    [StringLength(500)] public string? DES_OBSERVACION { get; set; }
}

/// <summary>El motivo es obligatorio: queda registrado junto con quién y cuándo anuló.</summary>
public class RfidAnularDto
{
    [Required, StringLength(500, MinimumLength = 1)] public string DES_MOTIVO_ANULACION { get; set; } = string.Empty;
}

public class RfidEmisionDetalle
{
    public int NUM_EMISION { get; set; }
    public int NUM_ITEM { get; set; }
    public int? NUM_SECUENCIA_MOVIMIENTO { get; set; }
    public string COD_ALMACEN { get; set; } = string.Empty;
    public string COD_UBICACION_ALMACEN { get; set; } = string.Empty;
    public string COD_PRODUCTO { get; set; } = string.Empty;
    public string? DES_PRODUCTO { get; set; }
    public string? NUM_LOTE_CONTROL { get; set; }
    public string? COD_PROCESO { get; set; }
    public int? NUM_ORDEN_PRODUCCION { get; set; }
    public int? NUM_ORDEN_PRODUCCION_ITEM { get; set; }
    public int? NUM_ORDEN_PRODUCCION_UNIDAD { get; set; }
    public string? COD_UNIDAD_MEDIDA { get; set; }
    /// <summary>Cantidad del origen cuando se pidió (línea del ingreso o fila de stock).</summary>
    public decimal? CAN_ORIGEN { get; set; }
    public int CAN_ETIQUETAS { get; set; }
    public int CAN_PENDIENTES { get; set; }
    public int CAN_ENROLADAS { get; set; }
    public int CAN_ANULADAS { get; set; }
    /// <summary>Suma de lo que representan las etiquetas vivas (pares, kg, m...).</summary>
    public decimal CAN_TOTAL { get; set; }
}

// ───────────────────────── Etiquetas ─────────────────────────

/// <summary>Una etiqueta de una emisión. P pendiente (falta leer su TID) · E enrolada (ya tiene código y EPC) · N anulada.
/// FLG_LEIDA: la etiqueta enrolada ya tiene lecturas en un movimiento o en un inventario (por eso ya no se puede anular).</summary>
public class RfidEtiqueta
{
    public int NUM_EMISION { get; set; }
    public int NUM_ITEM { get; set; }
    public int NUM_ETIQUETA { get; set; }
    public string COD_ESTADO_ETIQUETA { get; set; } = string.Empty;
    public string? DES_ESTADO_ETIQUETA { get; set; }
    public string COD_PRODUCTO { get; set; } = string.Empty;
    public string? DES_PRODUCTO { get; set; }
    public string? NUM_LOTE_CONTROL { get; set; }
    public string? COD_PROCESO { get; set; }
    public int? NUM_ORDEN_PRODUCCION { get; set; }
    public int? NUM_ORDEN_PRODUCCION_ITEM { get; set; }
    public int? NUM_ORDEN_PRODUCCION_UNIDAD { get; set; }
    public string? COD_ALMACEN { get; set; }
    public string? COD_UBICACION_ALMACEN { get; set; }
    public decimal CAN_PRODUCTO { get; set; }
    public string? COD_UNIDAD_MEDIDA { get; set; }
    public string? COD_TID { get; set; }
    public int? NUM_CORRELATIVO { get; set; }
    /// <summary>TID + correlativo de 15 dígitos: la clave en ESTANDAR.OBJETOS_RFID.</summary>
    public string? COD_STICKER_RFID { get; set; }
    /// <summary>Lo que se graba en la memoria EPC del chip (24 hexadecimales).</summary>
    public string? COD_EPC { get; set; }
    /// <summary>El equipo (producto) con que se emite.</summary>
    public string? COD_PRODUCTO_EQUIPO { get; set; }
    public string? DES_EQUIPO { get; set; }
    public DateTime? FEC_ENROLAMIENTO { get; set; }
    public string? COD_USUARIO_ENROLAMIENTO { get; set; }
    public string? COD_ESTACION_ENROLAMIENTO { get; set; }
    public DateTime? FEC_GRABACION { get; set; }
    public string? COD_USUARIO_GRABACION { get; set; }
    public bool FLG_GRABADA { get; set; }
    public DateTime? FEC_ANULACION { get; set; }
    public string? COD_USUARIO_ANULACION { get; set; }
    public string? COD_ESTACION_ANULACION { get; set; }
    public string? DES_MOTIVO_ANULACION { get; set; }
    public string COD_ORIGEN { get; set; } = string.Empty;
    public decimal? NUM_MOVIMIENTO_ALMACEN { get; set; }
    public DateTime? FEC_EMISION { get; set; }
    public bool FLG_EMISION_ANULADA { get; set; }
    public bool FLG_LEIDA { get; set; }
}

public class RfidEtiquetaSearchDto
{
    public string? SEARCH { get; set; }
    /// <summary>P pendiente · E enrolada · N anulada.</summary>
    public string? COD_ESTADO_ETIQUETA { get; set; }
    public int? NUM_EMISION { get; set; }
    public string? COD_PRODUCTO { get; set; }
    /// <summary>Máximo de filas (las más recientes primero). Por omisión 2000.</summary>
    public int? TOP { get; set; }
}

/// <summary>Un chip entrega su TID: se le asigna la siguiente etiqueta pendiente (o la elegida con NUM_ITEM + NUM_ETIQUETA).</summary>
public class RfidEnrolarDto
{
    [Required, StringLength(200, MinimumLength = 1)] public string COD_TID { get; set; } = string.Empty;
    public int? NUM_ITEM { get; set; }
    public int? NUM_ETIQUETA { get; set; }
    /// <summary>El producto que es el equipo (debe ser un equipo RFID según Configuración general &gt; RFID).</summary>
    [StringLength(30)] public string? COD_PRODUCTO_EQUIPO { get; set; }
}

/// <summary>Lo que devuelve el enrolamiento: el código y el EPC que hay que grabar en el chip.</summary>
public class RfidEnrolado
{
    public int NUM_EMISION { get; set; }
    public int NUM_ITEM { get; set; }
    public int NUM_ETIQUETA { get; set; }
    public string COD_TID { get; set; } = string.Empty;
    public int NUM_CORRELATIVO { get; set; }
    public string COD_STICKER_RFID { get; set; } = string.Empty;
    public string COD_EPC { get; set; } = string.Empty;
    public string COD_PRODUCTO { get; set; } = string.Empty;
    public string? DES_PRODUCTO { get; set; }
    public string? NUM_LOTE_CONTROL { get; set; }
    public decimal CAN_PRODUCTO { get; set; }
    public string? COD_UNIDAD_MEDIDA { get; set; }
    public bool FLG_GRABADA { get; set; }
    /// <summary>El mismo TID llegó otra vez a la misma emisión (un reintento): se devolvió lo ya enrolado, sin crear nada.</summary>
    public bool FLG_YA_ENROLADA { get; set; }
    /// <summary>Etiquetas que siguen pendientes en la emisión.</summary>
    public int CAN_PENDIENTES { get; set; }
}

/// <summary>El equipo grabó el EPC y lo leyó de vuelta: si COD_EPC_LEIDO viene y no coincide, la grabación fallo.</summary>
public class RfidGrabadaDto
{
    [Required, StringLength(200, MinimumLength = 1)] public string COD_TID { get; set; } = string.Empty;
    [StringLength(200)] public string? COD_EPC_LEIDO { get; set; }
}

public class RfidEtiquetaAnularDto
{
    [Required, StringLength(500, MinimumLength = 1)] public string DES_MOTIVO_ANULACION { get; set; } = string.Empty;
    /// <summary>Crea una etiqueta pendiente nueva en lugar de la anulada (misma línea, misma cantidad).</summary>
    public bool FLG_REPONER { get; set; } = true;
}

/// <summary>Lo que devuelve el SP _ANULAR de una etiqueta.</summary>
public class RfidEtiquetaAnulada
{
    public string COD_ESTADO_ANTERIOR { get; set; } = string.Empty;
    public int? NUM_ETIQUETA_REPUESTA { get; set; }
}

// ───────────────────────── Orígenes ─────────────────────────

/// <summary>Un ingreso por compra con líneas de productos RFID, con lo ya emitido y lo que falta.</summary>
public class RfidOrigenCompra
{
    public decimal NUM_MOVIMIENTO_ALMACEN { get; set; }
    public DateTime? FEC_MOVIMENTO_ALMACEN { get; set; }
    public string? COD_ALMACEN { get; set; }
    public string? DES_ALMACEN { get; set; }
    public string? COD_TIPO_MOVIMIENTO { get; set; }
    public string? DES_TIPO_MOVIMIENTO { get; set; }
    public decimal? COD_ENTIDAD { get; set; }
    public string? DES_ENTIDAD { get; set; }
    public string? NUM_COMPROBANTE_PAGO { get; set; }
    public string? DES_OBSERVACION { get; set; }
    public int CAN_LINEAS_RFID { get; set; }
    public decimal CAN_TOTAL { get; set; }
    public decimal CAN_EMITIDA { get; set; }
    public decimal CAN_PENDIENTE { get; set; }
}

public class RfidOrigenCompraSearchDto
{
    public string? SEARCH { get; set; }
    public DateTime? FEC_DESDE { get; set; }
    public DateTime? FEC_HASTA { get; set; }
    public string? COD_ALMACEN { get; set; }
    /// <summary>Por omisión solo los que todavía tienen algo por emitir.</summary>
    public bool? FLG_SOLO_PENDIENTES { get; set; }
    public int? TOP { get; set; }
}

public class RfidOrigenCompraLinea
{
    public int NUM_SECUENCIA { get; set; }
    public string COD_PRODUCTO { get; set; } = string.Empty;
    public string? DES_PRODUCTO { get; set; }
    public string? COD_ALMACEN { get; set; }
    public string? COD_UBICACION_ALMACEN { get; set; }
    public string? NUM_LOTE_CONTROL { get; set; }
    public string? COD_PROCESO { get; set; }
    public int? NUM_ORDEN_PRODUCCION { get; set; }
    public int? NUM_ORDEN_PRODUCCION_ITEM { get; set; }
    public int? NUM_ORDEN_PRODUCCION_UNIDAD { get; set; }
    public string? COD_UNIDAD_MEDIDA { get; set; }
    public decimal CAN_PRODUCTO { get; set; }
    /// <summary>Producto de medida (kg, m...): las etiquetas son por rollo y llevan su peso o metraje.</summary>
    public bool FLG_MEDICION { get; set; }
    public decimal CAN_EMITIDA { get; set; }
    public decimal CAN_PENDIENTE { get; set; }
}

public class RfidOrigenStock
{
    public string COD_ALMACEN { get; set; } = string.Empty;
    public string COD_UBICACION_ALMACEN { get; set; } = string.Empty;
    public string? DES_UBICACION_ALMACEN { get; set; }
    public string COD_PRODUCTO { get; set; } = string.Empty;
    public string? DES_PRODUCTO { get; set; }
    public string? NUM_LOTE_CONTROL { get; set; }
    public string? COD_PROCESO { get; set; }
    public int? NUM_ORDEN_PRODUCCION { get; set; }
    public int? NUM_ORDEN_PRODUCCION_ITEM { get; set; }
    public int? NUM_ORDEN_PRODUCCION_UNIDAD { get; set; }
    public string? COD_UNIDAD_MEDIDA { get; set; }
    public decimal CAN_PRODUCTO { get; set; }
    public bool FLG_MEDICION { get; set; }
    public decimal CAN_EMITIDA { get; set; }
    public decimal CAN_PENDIENTE { get; set; }
}

public class RfidOrigenStockSearchDto
{
    public string COD_ALMACEN { get; set; } = string.Empty;
    public string? SEARCH { get; set; }
    public bool? FLG_SOLO_PENDIENTES { get; set; }
    public int? TOP { get; set; }
}

// ───────────────────────── Impresión desde la web (cola de trabajos) ─────────────────────────

/// <summary>Un trabajo de impresión: "imprimir las etiquetas que faltan de la emisión N en la impresora X". Lo pide la web y lo atiende el servicio del agente (agente-rfid servicio).
/// Estados: P pendiente (nadie lo ha tomado) · E en proceso (el servicio lo tiene y manda latidos) · T terminado · F con fallos · C cancelado.
/// NUM_SEGUNDOS_ESPERA: pendiente = desde que se pidió; en proceso = desde el último latido; si crece, el servicio de la planta no está respondiendo.</summary>
public class RfidTrabajoImpresion
{
    public int NUM_TRABAJO { get; set; }
    public int NUM_EMISION { get; set; }
    public string COD_PRODUCTO_EQUIPO { get; set; } = string.Empty;
    public string? DES_EQUIPO { get; set; }
    /// <summary>Cuántas etiquetas imprimir; nulo = todas las que falten.</summary>
    public int? CAN_SOLICITADAS { get; set; }
    public string COD_ESTADO_TRABAJO { get; set; } = string.Empty;
    public string? DES_ESTADO_TRABAJO { get; set; }
    /// <summary>Piden cancelarlo mientras se imprime: el servicio se detiene al terminar la etiqueta que tiene en la impresora.</summary>
    public bool FLG_CANCELAR { get; set; }
    public int CAN_IMPRESAS { get; set; }
    public int CAN_FALLIDAS { get; set; }
    public string? DES_MENSAJE { get; set; }
    public string? COD_USUARIO_SOLICITUD { get; set; }
    public string? COD_ESTACION_SOLICITUD { get; set; }
    public DateTime FEC_SOLICITUD { get; set; }
    public string? COD_USUARIO_AGENTE { get; set; }
    public DateTime? FEC_INICIO { get; set; }
    public DateTime? FEC_LATIDO { get; set; }
    public DateTime? FEC_FIN { get; set; }
    public int? NUM_SEGUNDOS_ESPERA { get; set; }
}

public class RfidTrabajoImpresionSearchDto
{
    public int? NUM_EMISION { get; set; }
    /// <summary>Solo los pendientes y en proceso.</summary>
    public bool? FLG_SOLO_ACTIVOS { get; set; }
    /// <summary>Máximo de filas (las más recientes primero). Por omisión 50.</summary>
    public int? TOP { get; set; }
}

/// <summary>Pedir imprimir las etiquetas de una emisión. Sin equipo usa el de la emisión; sin cantidad imprime todas las que falten.</summary>
public class RfidImprimirDto
{
    [Required] public int NUM_EMISION { get; set; }
    [StringLength(30)] public string? COD_PRODUCTO_EQUIPO { get; set; }
    public int? CAN_SOLICITADAS { get; set; }
}

public class RfidTrabajoCreado
{
    public int NUM_TRABAJO { get; set; }
    /// <summary>Etiquetas que faltan por imprimir en la emisión cuando se pidió.</summary>
    public int CAN_FALTAN { get; set; }
}

/// <summary>El servicio de una impresora pide su siguiente trabajo. Un trabajo en proceso que lleva MINUTOS_SIN_LATIDO (por omisión 3) sin latido se vuelve a entregar.</summary>
public class RfidTrabajoReclamarDto
{
    [Required, StringLength(30, MinimumLength = 1)] public string COD_PRODUCTO_EQUIPO { get; set; } = string.Empty;
    public int? MINUTOS_SIN_LATIDO { get; set; }
}

/// <summary>Cuántas lleva (acumulado) y latido del servicio.</summary>
public class RfidTrabajoProgresoDto
{
    public int CAN_IMPRESAS { get; set; }
    public int CAN_FALLIDAS { get; set; }
    [StringLength(500)] public string? DES_MENSAJE { get; set; }
}

/// <summary>El servicio cierra el trabajo: T terminado · F con fallos · C cancelado.</summary>
public class RfidTrabajoTerminarDto
{
    [Required, StringLength(1, MinimumLength = 1)] public string COD_ESTADO_TRABAJO { get; set; } = string.Empty;
    public int CAN_IMPRESAS { get; set; }
    public int CAN_FALLIDAS { get; set; }
    [StringLength(500)] public string? DES_MENSAJE { get; set; }
}

/// <summary>Lo que contestan cancelar y progreso: el estado del trabajo y si piden cancelarlo.</summary>
public class RfidTrabajoEstado
{
    public string COD_ESTADO_TRABAJO { get; set; } = string.Empty;
    public bool FLG_CANCELAR { get; set; }
}
