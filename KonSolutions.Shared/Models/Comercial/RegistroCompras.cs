using System.ComponentModel.DataAnnotations;

namespace KONSolutions.Shared.Models.Comercial;

// Registro de Compras (Comercial/RegistroCompras): FINANZAS.COMPROBANTES_PAGO y FINANZAS.COMPROBANTES_PAGO_DETALLE.
// La clase de operación (COD_CLASE_OPERACION) la fija la opción de menú con su parámetro («COD_CLASE_OPERACION = 'CMPRS'»); nunca va escrita en el código.

/// <summary>Un comprobante del proveedor. La lista (SEARCH) trae las primeras columnas; la ficha (SHOWBYID) trae todas.</summary>
public class RegistroCompra
{
    public decimal NUM_REGISTRO { get; set; }
    public string COD_CLASE_OPERACION { get; set; } = "";
    public string COD_TIPO_OPERACION { get; set; } = "";
    public string? DES_TIPO_OPERACION { get; set; }
    public decimal? COD_ENTIDAD { get; set; }
    public string? DES_ENTIDAD { get; set; }
    public string? NUM_DOCUMENTO_IDENTIDAD { get; set; }
    public int? NUM_SECUENCIA_DIRECCION { get; set; }
    public string? COD_TIPO_COMPROBANTE_PAGO { get; set; }
    public string? DES_TIPO_COMPROBANTE_PAGO { get; set; }
    public string? NUM_COMPROBANTE_PAGO { get; set; }
    public DateTime? FEC_COMPROBANTE_PAGO { get; set; }
    public DateTime? FEC_VENCIMIENTO { get; set; }
    public DateTime? FEC_RECEPCION { get; set; }
    public string? COD_MONEDA { get; set; }
    public decimal? FAC_TIPO_CAMBIO { get; set; }
    public int? COD_FORMA_PAGO { get; set; }
    public int? COD_CONDICION_PAGO { get; set; }
    public string? COD_CENTRO_COSTO_SOLICITANTE { get; set; }
    public string? COD_CENTRO_COSTO_DESTINO { get; set; }
    public int? NUM_PROYECTO { get; set; }
    public string? NUM_LOTE_CONTROL { get; set; }
    public string? COD_SUCURSAL { get; set; }
    public string? COD_INCOTERM { get; set; }
    /// <summary>Motivo de la devolucion o del cargo (solo con un tipo de operacion de devolucion o cargo; script 86).</summary>
    public int? COD_TIPO_MOTIVO { get; set; }
    public int? COD_MOTIVO { get; set; }
    /// <summary>El descuento de la cabecera: el tipo (COMERCIAL.TIPOS_DESCUENTO) y su tasa. Salen de la orden de compra al asociarla (si el comprobante no los tiene) o se eligen a mano.</summary>
    public int? COD_TIPO_DESCUENTO { get; set; }
    public string? DES_TIPO_DESCUENTO { get; set; }
    public decimal? TAS_DESCUENTO { get; set; }
    /// <summary>El importe total del documento: lo deja calculado el estándar al registrar las líneas (no se escribe). El detalle (base, impuestos…) está en RegistroCompraCalculo.</summary>
    public decimal? VAL_TOTAL { get; set; }
    public string? DES_OBSERVACION { get; set; }
    public string? DES_NOMBRE_ARCHIVO_PDF { get; set; }
    public string? DES_JUSTIFICACION { get; set; }
    /// <summary>REG registrada · OBS observada · RET retenida (falta recepción) · EXC excepción (sin orden) · BLQ bloqueada · VAL validada · APR aprobada · CON contabilizada · ANU anulada.</summary>
    /// <summary>El par estándar del estado (ESTANDAR.ESTADOS): el tipo de estado de la conciliación y el estado.</summary>
    public short? COD_TIPO_ESTADO_CONCILIACION { get; set; }
    public short? COD_ESTADO_CONCILIACION { get; set; }
    public string COD_ESTADO_ALIAS_CONCILIACION { get; set; } = "REG";
    public string? DES_ESTADO_CONCILIACION { get; set; }
    public bool? FLG_EDITABLE { get; set; }
    public int? NUM_PEDIDOS { get; set; }
    public bool? FLG_TIENE_XML { get; set; }
    public DateTime? FEC_VALIDACION { get; set; }
    public string? COD_USUARIO_APROBACION { get; set; }
    public DateTime? FEC_APROBACION { get; set; }
    public string? COD_USUARIO_REGISTRO { get; set; }
    public DateTime? FEC_REGISTRO { get; set; }
    public string? COD_USUARIO_ACTUALIZACION { get; set; }
    public DateTime? FEC_ACTUALIZACION { get; set; }

    // Lo que habilita el tipo de operación (ESTANDAR.TIPOS_OPERACION): decide qué datos se piden en la pantalla.
    public bool? FLG_BIEN { get; set; }
    public bool? FLG_POSITIVO { get; set; }
    public bool? FLG_CENTRO_COSTO { get; set; }
    public bool? FLG_PROYECTO { get; set; }
    public bool? FLG_LOTE_CONTROL { get; set; }
    public bool? FLG_COMERCIO_EXTERIOR { get; set; }
    public bool? FLG_ANTICIPO { get; set; }
    public bool? FLG_PROVISION { get; set; }
    public bool? FLG_CAJA { get; set; }
    public bool? FLG_BANCO { get; set; }
}

public class RegistroCompraSearchDto
{
    /// <summary>Obligatoria: la que trae el parámetro de la opción de menú.</summary>
    public string? COD_CLASE_OPERACION { get; set; }
    public string? SEARCH { get; set; }
    public string? COD_TIPO_OPERACION { get; set; }
    public decimal? COD_ENTIDAD { get; set; }
    public string? COD_ESTADO_ALIAS_CONCILIACION { get; set; }
    public DateTime? FEC_DESDE { get; set; }
    public DateTime? FEC_HASTA { get; set; }
    public int? NUM_FILAS { get; set; }
}

/// <summary>Los datos de la cabecera. El tipo de operación decide cuáles se guardan: el centro de costo, el proyecto, el lote y el incoterm solo si el tipo los habilita.</summary>
public class RegistroCompraSaveDto
{
    [Required] public string COD_TIPO_OPERACION { get; set; } = "";
    [Required] public decimal? COD_ENTIDAD { get; set; }
    [Required] public string COD_TIPO_COMPROBANTE_PAGO { get; set; } = "";
    /// <summary>Serie y correlativo: «F001-00000123».</summary>
    [Required] public string NUM_COMPROBANTE_PAGO { get; set; } = "";
    [Required] public DateTime? FEC_COMPROBANTE_PAGO { get; set; }
    public int? NUM_SECUENCIA_DIRECCION { get; set; }
    public DateTime? FEC_VENCIMIENTO { get; set; }
    public DateTime? FEC_RECEPCION { get; set; }
    public string? COD_MONEDA { get; set; }
    public decimal? FAC_TIPO_CAMBIO { get; set; }
    public int? COD_FORMA_PAGO { get; set; }
    public int? COD_CONDICION_PAGO { get; set; }
    public string? COD_CENTRO_COSTO_SOLICITANTE { get; set; }
    public string? COD_CENTRO_COSTO_DESTINO { get; set; }
    public int? NUM_PROYECTO { get; set; }
    public string? NUM_LOTE_CONTROL { get; set; }
    public string? COD_SUCURSAL { get; set; }
    public string? COD_INCOTERM { get; set; }
    /// <summary>Motivo de la devolucion o del cargo; sin motivo toma el por defecto del tipo de operacion y, si el tipo no es de devolucion ni cargo, queda vacio.</summary>
    public int? COD_MOTIVO { get; set; }
    /// <summary>El descuento de la cabecera (tipo y tasa). Si no se manda, queda sin descuento; al asociar una orden el comprobante lo toma de ella.</summary>
    public int? COD_TIPO_DESCUENTO { get; set; }
    public decimal? TAS_DESCUENTO { get; set; }
    // Los importes del documento no se escriben: salen del cálculo de las líneas (FINANZAS.COMPROBANTES_PAGO_CALCULO).
    public string? DES_OBSERVACION { get; set; }
    public string? DES_NOMBRE_ARCHIVO_PDF { get; set; }
}

public class RegistroCompraInsertDto : RegistroCompraSaveDto
{
    [Required] public string COD_CLASE_OPERACION { get; set; } = "";
}

public class RegistroCompraUpdateDto : RegistroCompraSaveDto
{
    [Required] public decimal NUM_REGISTRO { get; set; }
}

/// <summary>Una línea del comprobante. Si lleva orden, los cuatro datos de la orden (clase, tipo, número y línea) van juntos.</summary>
/// <summary>Un concepto del detalle de los valores totales del documento (Concepto / Tasa / Valor): base imponible, IGV, importe total… los que tiene configurados el tipo de operación.</summary>
public class RegistroCompraCalculo
{
    public decimal NUM_REGISTRO { get; set; }
    public string COD_TIPO_VALOR_CALCULO { get; set; } = "";
    public string? DES_TIPO_VALOR_CALCULO { get; set; }
    public string? COD_TIPO_IMPUESTO { get; set; }
    /// <summary>La tasa vigente a la fecha del comprobante; vacía si el concepto no es un impuesto.</summary>
    public decimal? TAS_IMPUESTO { get; set; }
    public decimal? VAL_CALCULO { get; set; }
}

public class RegistroCompraLinea
{
    public decimal NUM_REGISTRO { get; set; }
    public int NUM_SECUENCIA { get; set; }
    public string? COD_PRODUCTO { get; set; }
    public string? DES_PRODUCTO { get; set; }
    public string? DES_PRODUCTO_COMPROBANTE { get; set; }
    public string? COD_PRODUCTO_ENTIDAD { get; set; }
    public decimal? CAN_PRODUCTO { get; set; }
    public string? COD_UNIDAD_MEDIDA { get; set; }
    public string? COD_UNIDAD_MEDIDA_COMPROBANTE { get; set; }
    public int? COD_TIPO_DESCUENTO { get; set; }
    public decimal? TAS_DESCUENTO { get; set; }
    public decimal? PRE_UNITARIO { get; set; }
    public decimal? VAL_DESCUENTO { get; set; }
    public decimal? VAL_TOTAL { get; set; }
    public string? COD_CLASE_OPERACION_PEDIDO { get; set; }
    public string? COD_TIPO_OPERACION_PEDIDO { get; set; }
    public decimal? NUM_PEDIDO_COMERCIAL { get; set; }
    public int? NUM_SECUENCIA_PEDIDO_COMERCIAL { get; set; }
    public string? DES_PEDIDO { get; set; }
    /// <summary>Lo que dice la línea de la orden: cantidad (en la unidad del proveedor) y precio.</summary>
    public decimal? CAN_ORDEN { get; set; }
    public decimal? PRE_ORDEN { get; set; }
}

public class RegistroCompraLineaDto
{
    /// <summary>Vacío en el alta; con valor modifica esa línea.</summary>
    public int? NUM_SECUENCIA { get; set; }
    public string? COD_PRODUCTO { get; set; }
    public string? DES_PRODUCTO_COMPROBANTE { get; set; }
    public string? COD_PRODUCTO_ENTIDAD { get; set; }
    [Required] public decimal? CAN_PRODUCTO { get; set; }
    public string? COD_UNIDAD_MEDIDA { get; set; }
    public string? COD_UNIDAD_MEDIDA_COMPROBANTE { get; set; }
    public int? COD_TIPO_DESCUENTO { get; set; }
    public decimal? TAS_DESCUENTO { get; set; }
    public decimal? PRE_UNITARIO { get; set; }
    public decimal? VAL_DESCUENTO { get; set; }
    public decimal? VAL_TOTAL { get; set; }
    public string? COD_CLASE_OPERACION_PEDIDO { get; set; }
    public string? COD_TIPO_OPERACION_PEDIDO { get; set; }
    public decimal? NUM_PEDIDO_COMERCIAL { get; set; }
    public int? NUM_SECUENCIA_PEDIDO_COMERCIAL { get; set; }
}

/// <summary>Una orden de compra o de servicio, asociada a un comprobante o disponible para asociarla.</summary>
public class RegistroCompraPedido
{
    public decimal? NUM_REGISTRO { get; set; }
    public string COD_CLASE_OPERACION { get; set; } = "";
    public string COD_TIPO_OPERACION { get; set; } = "";
    public string? DES_TIPO_OPERACION { get; set; }
    public decimal NUM_PEDIDO_COMERCIAL { get; set; }
    public string? DES_PEDIDO_COMERCIAL { get; set; }
    public DateTime? FEC_PEDIDO { get; set; }
    public string? COD_MONEDA { get; set; }
    public decimal? VAL_TOTAL { get; set; }
    public string? DES_ESTADO { get; set; }
    public string? COD_ESTADO_ALIAS { get; set; }
    /// <summary>Verdadero si la orden es de bienes; falso si es de servicios.</summary>
    public bool? FLG_BIEN { get; set; }
    public int? NUM_LINEAS_VINCULADAS { get; set; }
}

public class RegistroCompraPedidoAsociarDto
{
    [Required] public string COD_CLASE_OPERACION { get; set; } = "";
    [Required] public string COD_TIPO_OPERACION { get; set; } = "";
    [Required] public decimal? NUM_PEDIDO_COMERCIAL { get; set; }
}

/// <summary>Una línea de las órdenes asociadas, con lo que falta facturar (en la unidad del proveedor).</summary>
public class RegistroCompraPedidoLinea
{
    public string COD_CLASE_OPERACION { get; set; } = "";
    public string COD_TIPO_OPERACION { get; set; } = "";
    public decimal NUM_PEDIDO_COMERCIAL { get; set; }
    public int NUM_SECUENCIA { get; set; }
    public string? COD_PRODUCTO { get; set; }
    public string? DES_PRODUCTO { get; set; }
    public decimal? CAN_ORDEN { get; set; }
    public decimal? PRE_UNITARIO { get; set; }
    /// <summary>El descuento de la línea de la orden (tipo y tasa), para traerlo con la línea.</summary>
    public int? COD_TIPO_DESCUENTO { get; set; }
    public decimal? TAS_DESCUENTO { get; set; }
    public string? COD_UNIDAD_MEDIDA { get; set; }
    public decimal? CAN_FACTURADA { get; set; }
    public decimal? CAN_PENDIENTE { get; set; }
    /// <summary>true en un comprobante de bienes: la cantidad a facturar es la de la guía (lo recibido en almacén) y no se edita.</summary>
    public bool FLG_POR_GUIA { get; set; }
    /// <summary>Lo recibido en almacén de esa línea de la orden (ingresos menos devoluciones), en la unidad del proveedor.</summary>
    public decimal? CAN_GUIA { get; set; }
    /// <summary>Lo que se puede facturar: en bienes, lo recibido menos lo ya facturado (nunca negativo); en servicios, lo que falta de la orden.</summary>
    public decimal? CAN_POR_FACTURAR { get; set; }
}

/// <summary>Un hallazgo de la validación. NUM_PASO: 2 SUNAT/OSE · 3 proveedor y órdenes · 4 ingresos · 5 cantidades · 6 precios · 7 importes · 8 duplicidad y condiciones.
/// COD_RESULTADO: OK · OBS observación · RET retenida · EXC excepción · BLQ bloqueo.</summary>
public class RegistroCompraHallazgo
{
    public decimal NUM_REGISTRO { get; set; }
    public int NUM_ORDEN { get; set; }
    public int NUM_PASO { get; set; }
    public string COD_RESULTADO { get; set; } = "";
    public int? NUM_SECUENCIA { get; set; }
    public string DES_MENSAJE { get; set; } = "";
    public decimal? CAN_ORDEN { get; set; }
    public decimal? CAN_INGRESADA { get; set; }
    public decimal? CAN_FACTURADA { get; set; }
    public decimal? PRE_ORDEN { get; set; }
    public decimal? PRE_COMPROBANTE { get; set; }
    public decimal? VAL_ESPERADO { get; set; }
    public decimal? VAL_COMPROBANTE { get; set; }
    public string? COD_USUARIO_VALIDACION { get; set; }
    public DateTime? FEC_VALIDACION { get; set; }
}

/// <summary>Resultado de validar: el estado en que quedó el comprobante y los hallazgos de cada paso.</summary>
public class RegistroCompraValidacion
{
    public decimal NUM_REGISTRO { get; set; }
    public string COD_ESTADO_ALIAS_CONCILIACION { get; set; } = "";
    public List<RegistroCompraHallazgo> HALLAZGOS { get; set; } = new();
}

/// <summary>Para enviar a aprobación, aprobar y anular. Enviar a aprobación y anular exigen la justificación; aprobar la exige si el comprobante está bloqueado o es una excepción.</summary>
public class RegistroCompraJustificacionDto
{
    public string? DES_JUSTIFICACION { get; set; }
}

public class RegistroCompraXml
{
    public decimal NUM_REGISTRO { get; set; }
    public string? DES_NOMBRE_ARCHIVO { get; set; }
    public string? COD_HASH { get; set; }
    public string? DES_XML { get; set; }
    public string? COD_USUARIO_REGISTRO { get; set; }
    public DateTime? FEC_REGISTRO { get; set; }
}

public class RegistroCompraImportarXmlDto
{
    [Required] public string COD_CLASE_OPERACION { get; set; } = "";
    /// <summary>El texto del XML (factura o boleta electrónica UBL 2.1).</summary>
    [Required] public string DES_XML { get; set; } = "";
    public string? DES_NOMBRE_ARCHIVO { get; set; }
    /// <summary>Si no se manda, el de la configuración general (pestaña Compras).</summary>
    public string? COD_TIPO_OPERACION { get; set; }
    /// <summary>Validar el comprobante apenas se importa.</summary>
    public bool VALIDAR { get; set; }
}

/// <summary>Lo que dejó importar un XML.</summary>
public class RegistroCompraImportado
{
    public decimal NUM_REGISTRO { get; set; }
    public string COD_ESTADO_ALIAS_CONCILIACION { get; set; } = "";
    public int NUM_LINEAS { get; set; }
    public int NUM_PEDIDOS_ASOCIADOS { get; set; }
    public int NUM_LINEAS_VINCULADAS { get; set; }
}

public class RegistroCompraImportarCarpetaDto
{
    [Required] public string COD_CLASE_OPERACION { get; set; } = "";
    public string? COD_TIPO_OPERACION { get; set; }
    public bool VALIDAR { get; set; }
}

/// <summary>Un archivo de la carpeta de XML: si se importó (y como qué comprobante) o por qué no.</summary>
public class RegistroCompraArchivoImportado
{
    public string ARCHIVO { get; set; } = "";
    public bool IMPORTADO { get; set; }
    public decimal? NUM_REGISTRO { get; set; }
    public string? COD_ESTADO_ALIAS_CONCILIACION { get; set; }
    public string? MENSAJE { get; set; }
}

public class RegistroCompraImportarCarpetaResult
{
    public string CARPETA { get; set; } = "";
    public int NUM_ARCHIVOS { get; set; }
    public int NUM_IMPORTADOS { get; set; }
    public int NUM_ERRORES { get; set; }
    public List<RegistroCompraArchivoImportado> ARCHIVOS { get; set; } = new();
}

/// <summary>Los catálogos de la pantalla, en un solo viaje.</summary>
public class RegistroCompraTipoOperacion
{
    public string COD_TIPO_OPERACION { get; set; } = "";
    public string? DES_TIPO_OPERACION { get; set; }
    public bool FLG_BIEN { get; set; }
    public bool FLG_POSITIVO { get; set; }
    public bool FLG_CENTRO_COSTO { get; set; }
    public bool FLG_PROYECTO { get; set; }
    public bool FLG_LOTE_CONTROL { get; set; }
    public bool FLG_COMERCIO_EXTERIOR { get; set; }
    public bool FLG_ANTICIPO { get; set; }
    public bool FLG_INCIDENCIA_PRODUCTO { get; set; }
    public bool FLG_DEVOLUCION { get; set; }
    public bool FLG_ANULACION { get; set; }
    public bool FLG_CARGO { get; set; }
    public int? COD_TIPO_MOTIVO { get; set; }
    public int? COD_MOTIVO_DEFAULT { get; set; }
    public string? COD_TIPO_ENTIDAD { get; set; }
}

public class RegistroCompraTipoComprobante
{
    public string? COD_TIPO_OPERACION { get; set; }
    public string COD_TIPO_COMPROBANTE_PAGO { get; set; } = "";
    public string? DES_TIPO_COMPROBANTE_PAGO { get; set; }
    public int? COD_COMPROBANTE_PAGO_SUNAT { get; set; }
    /// <summary>El formato del número (serie - correlativo): el prefijo (el de SUNAT primero; el interno de respaldo) y los dígitos de la serie y del correlativo. Sin longitudes no hay formato que aplicar.</summary>
    public string? COD_PREFIJO_SUNAT { get; set; }
    public string? COD_PREFIJO { get; set; }
    public int? VAL_LONGITUD_SERIE { get; set; }
    public int? VAL_LONGITUD_NUMERO { get; set; }
}

/// <summary>Un elemento de un combo (código y texto) tal como lo devuelven los SHOWCOMBOX existentes.</summary>
public class RegistroCompraCombo
{
    public string? Id { get; set; }
    public string? Texto { get; set; }
}

/// <summary>Un tipo de descuento (COMERCIAL.TIPOS_DESCUENTO) con su tasa.</summary>
public class RegistroCompraTipoDescuento
{
    public int COD_TIPO_DESCUENTO { get; set; }
    public string? DES_TIPO_DESCUENTO { get; set; }
    public decimal? TAS_DESCUENTO { get; set; }
}

public class RegistroCompraCombos
{
    public List<RegistroCompraTipoOperacion> TIPOS_OPERACION { get; set; } = new();
    /// <summary>Los tipos de comprobante que admite cada tipo de operación. Si un tipo de operación no figura aquí, admite cualquiera de TIPOS_COMPROBANTE.</summary>
    public List<RegistroCompraTipoComprobante> TIPOS_COMPROBANTE_POR_OPERACION { get; set; } = new();
    public List<RegistroCompraTipoComprobante> TIPOS_COMPROBANTE { get; set; } = new();
    public List<RegistroCompraCombo> MONEDAS { get; set; } = new();
    public List<RegistroCompraCombo> FORMAS_PAGO { get; set; } = new();
    public List<RegistroCompraCombo> CONDICIONES_PAGO { get; set; } = new();
    public List<RegistroCompraCombo> INCOTERMS { get; set; } = new();
    public List<RegistroCompraCombo> PROYECTOS { get; set; } = new();
    /// <summary>Los tipos de descuento con su tasa (la cabecera del comprobante elige uno).</summary>
    public List<RegistroCompraTipoDescuento> TIPOS_DESCUENTO { get; set; } = new();
}

/// <summary>Los estados de conciliación del comprobante: su texto y qué se puede hacer en cada uno. Son los mismos que calcula la base (FNCN_REGISTRO_COMPRAS_DES_ESTADO / _EDITABLE); aquí solo se usan para mostrar y para ocultar botones: la base vuelve a exigir cada regla.</summary>
public static class RegistroCompraEstados
{
    public const string Registrada = "REG", Observada = "OBS", Retenida = "RET", Excepcion = "EXC", Bloqueada = "BLQ", Validada = "VAL", Aprobada = "APR", Contabilizada = "CON", Anulada = "ANU";

    public static string Texto(string? cod) => (cod ?? "REG").Trim().ToUpperInvariant() switch
    {
        "REG" => "Registrada",
        "OBS" => "Observada",
        "RET" => "Retenida (falta recepción)",
        "EXC" => "Excepción (sin orden)",
        "BLQ" => "Bloqueada",
        "VAL" => "Validada",
        "APR" => "Aprobada",
        "CON" => "Contabilizada",
        "ANU" => "Anulada",
        var otro => otro
    };

    /// <summary>Se puede modificar mientras no esté aprobada, contabilizada o anulada.</summary>
    public static bool Editable(string? cod) => (cod ?? "REG").Trim().ToUpperInvariant() is not ("APR" or "CON" or "ANU");

    public static bool PuedeValidar(string? cod) => Editable(cod);
    public static bool PuedeEnviarAprobacion(string? cod) => (cod ?? "").Trim().ToUpperInvariant() == "OBS";
    public static bool PuedeAprobar(string? cod) => (cod ?? "").Trim().ToUpperInvariant() is "VAL" or "BLQ" or "EXC";
    /// <summary>Aprobar con justificación obligatoria: el bloqueado y la excepción.</summary>
    public static bool AprobarExigeJustificacion(string? cod) => (cod ?? "").Trim().ToUpperInvariant() is "BLQ" or "EXC";
    public static bool PuedeContabilizar(string? cod) => (cod ?? "").Trim().ToUpperInvariant() == "APR";
    public static bool PuedeAnular(string? cod) => (cod ?? "REG").Trim().ToUpperInvariant() != "ANU";
}

// ───────── Notas de crédito y débito por ítem (script 85) ─────────

/// <summary>Una factura que la nota puede afectar.</summary>
public class RegistroCompraIncidenciaOrigen
{
    public decimal NUM_REGISTRO { get; set; }
    public string? COD_TIPO_COMPROBANTE_PAGO { get; set; }
    public string? DES_TIPO_COMPROBANTE_PAGO { get; set; }
    public string? NUM_COMPROBANTE_PAGO { get; set; }
    public DateTime? FEC_COMPROBANTE_PAGO { get; set; }
    public string? COD_MONEDA { get; set; }
    public decimal? VAL_TOTAL { get; set; }
    public string? COD_ESTADO_ALIAS { get; set; }
}

/// <summary>Una línea de la factura afectada con su saldo y lo que la nota ya aplica.</summary>
public class RegistroCompraIncidenciaLinea
{
    public int NUM_SECUENCIA_ASOCIADO { get; set; }
    public string? COD_PRODUCTO { get; set; }
    /// <summary>La descripción de la línea o, si no tiene, la del producto.</summary>
    public string? DES_PRODUCTO { get; set; }
    public string? DES_PRODUCTO_COMPROBANTE { get; set; }
    public string? COD_PRODUCTO_ENTIDAD { get; set; }
    public string? COD_UNIDAD_MEDIDA { get; set; }
    public decimal? PRE_UNITARIO { get; set; }
    public decimal? TAS_DESCUENTO { get; set; }
    public decimal CAN_FACTURADA { get; set; }
    public decimal VAL_FACTURADO { get; set; }
    public decimal CAN_APLICADA_OTRAS { get; set; }
    public decimal VAL_APLICADO_OTRAS { get; set; }
    public decimal CAN_SALDO { get; set; }
    public decimal VAL_SALDO { get; set; }
    public int? NUM_SECUENCIA_NOTA { get; set; }
    public decimal? CAN_APLICADA { get; set; }
    public decimal? VAL_APLICADO { get; set; }
    /// <summary>Cantidad que dicen las guías asociadas a la nota para este producto (null si no hay guías).</summary>
    public decimal? CAN_GUIA { get; set; }
    /// <summary>True si la lista está filtrada porque la nota ya tiene guías: solo salen los items que coinciden con ellas.</summary>
    public bool FLG_FILTRADO_POR_GUIA { get; set; }
}

/// <summary>Lo que la nota ya tiene asociado.</summary>
public class RegistroCompraIncidencia
{
    public int NUM_SECUENCIA_NOTA { get; set; }
    public decimal NUM_REGISTRO_ASOCIADO { get; set; }
    public string? COD_TIPO_COMPROBANTE_PAGO { get; set; }
    public string? NUM_COMPROBANTE_PAGO { get; set; }
    public int NUM_SECUENCIA_ASOCIADO { get; set; }
    public string? COD_PRODUCTO { get; set; }
    public string? DES_PRODUCTO_COMPROBANTE { get; set; }
    public decimal? CAN_FACTURADA { get; set; }
    public decimal? VAL_FACTURADO { get; set; }
    public decimal? CAN_APLICADA { get; set; }
    public decimal? VAL_APLICADO { get; set; }
}

public class RegistroCompraIncidenciaItemDto
{
    public int NUM_SECUENCIA { get; set; }
    public decimal NUM_REGISTRO_ASOCIADO { get; set; }
    public int NUM_SECUENCIA_ASOCIADO { get; set; }
    public decimal? CAN_APLICADA { get; set; }
    public decimal? VAL_APLICADO { get; set; }
}

public class RegistroCompraIncidenciasGuardarDto
{
    public List<RegistroCompraIncidenciaItemDto> Items { get; set; } = new();
}

/// <summary>Lo elegido en la grilla de la factura: cantidad devuelta y/o importe de la diferencia por línea.</summary>
public class RegistroCompraIncidenciaAdicionarItemDto
{
    public int NUM_SECUENCIA_ASOCIADO { get; set; }
    public decimal? CAN_APLICADA { get; set; }
    public decimal? VAL_APLICADO { get; set; }
}

public class RegistroCompraIncidenciaAdicionarDto
{
    public decimal? NUM_REGISTRO_ORIGEN { get; set; }
    public List<RegistroCompraIncidenciaAdicionarItemDto> Items { get; set; } = new();
}

/// <summary>Guía de devolución (movimiento de almacén de devolución al proveedor) que se puede adjuntar.</summary>
public class RegistroCompraIncidenciaGuia
{
    public decimal NUM_MOVIMIENTO_ALMACEN { get; set; }
    public DateTime? FEC_MOVIMENTO_ALMACEN { get; set; }
    public string? DES_TIPO_MOVIMIENTO { get; set; }
    public string? COD_TIPO_GUIA { get; set; }
    public string? NUM_GUIA { get; set; }
    public int NUM_LINEAS { get; set; }
    public bool FLG_ASOCIADA { get; set; }
}

public class RegistroCompraIncidenciaGuiaAdjunta
{
    public decimal NUM_MOVIMIENTO_ALMACEN { get; set; }
    public DateTime? FEC_MOVIMENTO_ALMACEN { get; set; }
    public string? DES_TIPO_MOVIMIENTO { get; set; }
    public string? COD_TIPO_GUIA { get; set; }
    public string? NUM_GUIA { get; set; }
    public int NUM_LINEAS_VINCULADAS { get; set; }
}

/// <summary>Datos para editar una línea de la nota: la línea de la factura afectada y la cantidad del proveedor según la guía.</summary>
public class RegistroCompraIncidenciaLineaGuia
{
    public bool FLG_CON_GUIA { get; set; }
    public decimal? NUM_REGISTRO_ASOCIADO { get; set; }
    public string? COD_TIPO_COMPROBANTE_PAGO { get; set; }
    public string? NUM_COMPROBANTE_PAGO { get; set; }
    public int? NUM_SECUENCIA_ASOCIADO { get; set; }
    public string? COD_PRODUCTO { get; set; }
    public string? DES_PRODUCTO { get; set; }
    public string? COD_PRODUCTO_ENTIDAD { get; set; }
    public string? COD_UNIDAD_MEDIDA { get; set; }
    public decimal? CAN_FACTURADA { get; set; }
    public decimal? PRE_UNITARIO { get; set; }
    public decimal? TAS_DESCUENTO { get; set; }
    public decimal? VAL_FACTURADO { get; set; }
    public decimal? CAN_GUIA { get; set; }
    public string? NUM_GUIAS { get; set; }
}

public class RegistroCompraIncidenciaAnulacionDto
{
    public decimal? NUM_REGISTRO_ORIGEN { get; set; }
}


/// <summary>Factura de anticipo (tipo de operación con FLG_ANTICIPO): disponible para aplicar a la factura, o ya aplicada.</summary>
public class RegistroCompraAnticipo
{
    public decimal NUM_REGISTRO { get; set; }
    public string? COD_TIPO_COMPROBANTE_PAGO { get; set; }
    public string? NUM_COMPROBANTE_PAGO { get; set; }
    public DateTime? FEC_COMPROBANTE_PAGO { get; set; }
    public string? COD_MONEDA { get; set; }
    public decimal VAL_TOTAL { get; set; }
    public string? DES_TIPO_OPERACION { get; set; }
    /// <summary>Lo escrito en las líneas del anticipo (descripción de la línea o del producto), unidas con ' | '.</summary>
    public string? DES_DETALLE { get; set; }
    /// <summary>Estado del anticipo: alias (REG, VAL, APR, CON, ANU...) y su descripción.</summary>
    public string? COD_ESTADO_ALIAS { get; set; }
    public string? DES_ESTADO { get; set; }
}
