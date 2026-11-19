namespace KONSolutions.Shared.Models.Finanzas;


/// <summary>Un comprobante recibido de un proveedor (FINANZAS.CPE_RECIBIDOS), como sale en la lista: factura, boleta, nota de crédito o de débito que bajó el servicio del correo.</summary>
public class CpeRecibido
{
    public decimal NUM_CPE { get; set; }
    public string COD_TIPO_COMPROBANTE_SUNAT { get; set; } = "";
    public string? DES_TIPO_COMPROBANTE { get; set; }
    /// <summary>El tipo de operación de SUNAT que trae el XML (InvoiceTypeCode listID: 0101 venta interna...); vacío en las notas.</summary>
    public string? COD_TIPO_OPERACION_SUNAT { get; set; }
    public string NUM_SERIE { get; set; } = "";
    public string NUM_CORRELATIVO { get; set; } = "";
    public string? NUM_COMPROBANTE { get; set; }
    public DateTime FEC_EMISION { get; set; }
    public DateTime? FEC_VENCIMIENTO { get; set; }
    public string? COD_MONEDA_SUNAT { get; set; }
    public decimal VAL_TOTAL { get; set; }
    /// <summary>El total con signo: las notas de crédito restan.</summary>
    public decimal VAL_TOTAL_CON_SIGNO { get; set; }
    public string NUM_RUC_EMISOR { get; set; } = "";
    public string? DES_RAZON_SOCIAL_EMISOR { get; set; }
    /// <summary>El proveedor en el maestro de entidades, buscado por el RUC; vacío si no está registrado.</summary>
    public decimal? COD_ENTIDAD { get; set; }
    public string? DES_ENTIDAD { get; set; }
    public string? NUM_ORDEN_COMPRA_CITADA { get; set; }
    public string? COD_TIPO_COMPROBANTE_SUNAT_REF { get; set; }
    public string? NUM_COMPROBANTE_REF { get; set; }
    /// <summary>PEN pendiente, USA usado (ya tiene su registro), DES descartado.</summary>
    public string COD_ESTADO_USO { get; set; } = "PEN";
    public string? DES_ESTADO_USO { get; set; }
    /// <summary>El par del estandar de estados (ESTANDAR.ESTADOS): el tipo de estado de la tabla y el estado. COD_ESTADO_USO es el alias de ese estado (PEN, USA, DES).</summary>
    public short COD_TIPO_ESTADO { get; set; }
    public short COD_ESTADO { get; set; }
    public string? DES_BACKCOLOR_ESTADO { get; set; }
    public string? DES_FORECOLOR_ESTADO { get; set; }
    public decimal? NUM_REGISTRO { get; set; }
    public string? COD_ESTADO_SUNAT { get; set; }
    public string? DES_ESTADO_SUNAT { get; set; }
    public DateTime? FEC_VALIDACION_SUNAT { get; set; }
    /// <summary>El estado de validación en el estándar (ESTANDAR.ESTADOS): el par tipo + estado, el alias (SIN, ACE, VAL, OBS, ERR), su descripción y colores.</summary>
    public short COD_TIPO_ESTADO_VALIDACION { get; set; }
    public short COD_ESTADO_VALIDACION { get; set; }
    public string? COD_ALIAS_VALIDACION { get; set; }
    public string? DES_ESTADO_VALIDACION { get; set; }
    public string? DES_BACKCOLOR_VALIDACION { get; set; }
    public string? DES_FORECOLOR_VALIDACION { get; set; }
    public int NUM_LINEAS { get; set; }
    public int NUM_LINEAS_SIN_PRODUCTO { get; set; }
    public string? DES_NOMBRE_ARCHIVO { get; set; }
    public string? DES_NOMBRE_ARCHIVO_PDF { get; set; }
    public DateTime? FEC_REGISTRO { get; set; }
}

/// <summary>La cabecera completa del comprobante recibido, para mostrar lo que traerá el registro.</summary>
public class CpeRecibidoFicha : CpeRecibido
{
    public TimeSpan? HOR_EMISION { get; set; }
    public string? DES_NOMBRE_COMERCIAL_EMISOR { get; set; }
    public string? DES_DIRECCION_EMISOR { get; set; }
    public string? COD_TIPO_DOC_RECEPTOR { get; set; }
    public string? NUM_DOC_RECEPTOR { get; set; }
    public string? DES_RECEPTOR { get; set; }
    public decimal? VAL_GRAVADO { get; set; }
    public decimal? VAL_INAFECTO { get; set; }
    public decimal? VAL_EXONERADO { get; set; }
    public decimal? VAL_GRATUITO { get; set; }
    public decimal? VAL_EXPORTACION { get; set; }
    public decimal? VAL_IGV { get; set; }
    public decimal? VAL_ISC { get; set; }
    public decimal? VAL_ICBPER { get; set; }
    public decimal? VAL_OTROS_TRIBUTOS { get; set; }
    public decimal? VAL_DESCUENTO_GLOBAL { get; set; }
    public decimal? VAL_CARGOS { get; set; }
    public decimal? VAL_ANTICIPOS { get; set; }
    public decimal? VAL_SUBTOTAL { get; set; }
    public decimal? VAL_TOTAL_CON_IMPUESTOS { get; set; }
    public string? COD_FORMA_PAGO { get; set; }
    public string? DES_CUOTAS { get; set; }
    public string? COD_DETRACCION { get; set; }
    public decimal? TAS_DETRACCION { get; set; }
    public decimal? VAL_DETRACCION { get; set; }
    public decimal? VAL_RETENCION { get; set; }
    public decimal? VAL_PERCEPCION { get; set; }
    public string? DES_GUIAS_RELACIONADAS { get; set; }
    public string? COD_MOTIVO_NOTA { get; set; }
    public string? DES_MOTIVO_NOTA { get; set; }
    public string? DES_LEYENDAS { get; set; }
}

public class CpeRecibidoLinea
{
    public decimal NUM_CPE { get; set; }
    public int NUM_ITEM { get; set; }
    public string? COD_PRODUCTO_VENDEDOR { get; set; }
    public string? COD_PRODUCTO_COMPRADOR { get; set; }
    public string? COD_PRODUCTO_SUNAT { get; set; }
    public string? COD_GTIN { get; set; }
    public string? DES_PRODUCTO { get; set; }
    public string? COD_UNIDAD_SUNAT { get; set; }
    public decimal? CAN_PRODUCTO { get; set; }
    public decimal? VAL_UNITARIO { get; set; }
    public decimal? PRE_UNITARIO { get; set; }
    public decimal? TAS_DESCUENTO { get; set; }
    public decimal? VAL_DESCUENTO_LINEA { get; set; }
    public string? COD_TIPO_AFECTACION_IGV { get; set; }
    public decimal? TAS_IGV { get; set; }
    public decimal? VAL_IGV_LINEA { get; set; }
    public decimal? VAL_ISC_LINEA { get; set; }
    public decimal? VAL_ICBPER_LINEA { get; set; }
    public decimal? VAL_VENTA_LINEA { get; set; }
    public string? NUM_LOTE { get; set; }
    public DateTime? FEC_VENCIMIENTO_LOTE { get; set; }
    public string? DES_SERIES { get; set; }
    /// <summary>El producto de la empresa (vacío hasta que se asigne).</summary>
    public string? COD_PRODUCTO { get; set; }
    public string? DES_PRODUCTO_EMPRESA { get; set; }
    /// <summary>De dónde salió el producto: EQU equivalencia guardada del proveedor, GTI código de barras, MAN asignado a mano...</summary>
    public string? COD_ORIGEN_ASIGNACION { get; set; }
    /// <summary>El lote interno (LOGISTICA.LOTES_CONTROL) que se registró para el lote del proveedor de la línea.</summary>
    public string? NUM_LOTE_CONTROL { get; set; }
}

/// <summary>El tipo de operación que le toca a un comprobante recibido según su XML (ver PROC_FINANZAS_CPE_RECIBIDOS_TIPO_OPERACION).</summary>
public class CpeRecibidoTipoOperacion
{
    public decimal NUM_CPE { get; set; }
    /// <summary>El código SUNAT del XML (InvoiceTypeCode listID).</summary>
    public string? COD_TIPO_OPERACION_SUNAT { get; set; }
    /// <summary>false si alguna línea viene en la unidad ZZ (servicio); true si no.</summary>
    public bool FLG_BIEN { get; set; }
    public int NUM_CANDIDATOS { get; set; }
    /// <summary>El tipo de operación, solo si hay exactamente uno.</summary>
    public string? COD_TIPO_OPERACION { get; set; }
    public string? DES_TIPO_OPERACION { get; set; }
    /// <summary>Por qué no hay uno solo (el XML no trae el código, ningún tipo lo tiene, o hay varios).</summary>
    public string? DES_MOTIVO { get; set; }
}

public class CpeRecibidoSearchDto
{
    public string? SEARCH { get; set; }
    public string? NUM_RUC_EMISOR { get; set; }
    public string? COD_TIPO_COMPROBANTE_SUNAT { get; set; }
    /// <summary>PEN, USA o DES; vacío trae todos.</summary>
    public string? COD_ESTADO_USO { get; set; }
    public DateTime? FEC_DESDE { get; set; }
    public DateTime? FEC_HASTA { get; set; }
    public int? NUM_FILAS { get; set; }
}

public class CpeRecibidoXml
{
    public decimal NUM_CPE { get; set; }
    public string? DES_NOMBRE_ARCHIVO { get; set; }
    public string DES_XML { get; set; } = "";
}

public class RecibidoAsignarProductoDto
{
    /// <summary>El producto de la empresa; vacío lo quita.</summary>
    public string? COD_PRODUCTO { get; set; }
}

/// <summary>Lo que hizo «Validar con SUNAT»: cuántos comprobantes se consultaron y cómo respondió SUNAT.</summary>
public class RecibidoSunatResult
{
    public int NUM_DOCUMENTOS { get; set; }
    /// <summary>SUNAT los tiene como aceptados (código 0001).</summary>
    public int NUM_ACEPTADOS { get; set; }
    /// <summary>SUNAT respondió con otro código: existe pero no está aceptado, o no existe.</summary>
    public int NUM_OBSERVADOS { get; set; }
    /// <summary>No se pudo consultar (sin conexión, Clave SOL rechazada...): el motivo queda en el comprobante.</summary>
    public int NUM_SIN_CONSULTAR { get; set; }
    /// <summary>El primer motivo de una consulta que falló, para mostrarlo.</summary>
    public string? DES_PRIMER_ERROR { get; set; }
}

/// <summary>Lo que hizo «Completar datos» (PROC_XML_RECIBIDOS_COMPLETAR_DATOS): proveedores registrados, productos asignados por el código del proveedor y lotes registrados.</summary>
public class RecibidoCompletarResult
{
    public int NUM_DOCUMENTOS { get; set; }
    public int NUM_PROVEEDORES_NUEVOS { get; set; }
    public int NUM_PROVEEDORES_CON_TIPO_PRV { get; set; }
    public int NUM_LINEAS_PRODUCTO_ASIGNADO { get; set; }
    public int NUM_LOTES_NUEVOS { get; set; }
    public int NUM_LINEAS_CON_LOTE { get; set; }
    public int NUM_LOTES_NO_REGISTRADOS { get; set; }
}

public class RecibidoDescartarDto
{
    /// <summary>true descarta el documento pendiente; false lo devuelve a pendiente.</summary>
    public bool FLG_DESCARTAR { get; set; } = true;
}

public class CpeRecibidoGenerarDto
{
    /// <summary>La clase de operación de la opción de menú (CMPRS).</summary>
    public string? COD_CLASE_OPERACION { get; set; }
    /// <summary>El que elige quien selecciona el XML. Vacío: sale del XML (el tipo de la clase con el código SUNAT del XML, de bienes o de servicios según la unidad ZZ de las líneas); si no hay uno solo, se pide elegir.</summary>
    public string? COD_TIPO_OPERACION { get; set; }
    /// <summary>El proveedor; vacío se busca por el RUC del emisor.</summary>
    public decimal? COD_ENTIDAD { get; set; }
    /// <summary>Anotar en la ficha del proveedor los productos que se asignaron a mano.</summary>
    public bool FLG_RECORDAR_EQUIVALENCIAS { get; set; }
    /// <summary>Correr la validación (conciliación) del registro al terminar.</summary>
    public bool VALIDAR { get; set; }
}

public class CpeRecibidoGenerarResult
{
    public decimal NUM_REGISTRO { get; set; }
    public string? COD_ESTADO_ALIAS_CONCILIACION { get; set; }
    public int NUM_LINEAS { get; set; }
    public int NUM_LINEAS_SIN_PRODUCTO { get; set; }
    public int NUM_PEDIDOS_ASOCIADOS { get; set; }
    public int NUM_LINEAS_VINCULADAS { get; set; }
    public int NUM_EQUIVALENCIAS_RECORDADAS { get; set; }
    public int NUM_EQUIVALENCIAS_NO_RECORDADAS { get; set; }
    public decimal? VAL_TOTAL { get; set; }
    /// <summary>El PDF que vino en el correo se copió al registro.</summary>
    public bool PDF_COPIADO { get; set; }
    /// <summary>Por qué el PDF no se copió (si venía uno y no se pudo); el registro se generó igual.</summary>
    public string? AVISO_PDF { get; set; }
}


/// <summary>Cómo salió una corrida del servicio que baja los XML y PDF del correo (lo que devuelve «Traer los nuevos del correo»).</summary>
public class XmlRecibidosCiclo
{
    public string? Mensaje { get; set; }
    public bool Omitida { get; set; }
    public int Correos { get; set; }
    public int CorreosProcesados { get; set; }
    public int CorreosConError { get; set; }
    public int CorreosSinMover { get; set; }
    public int XmlCargados { get; set; }
    public int XmlRepetidos { get; set; }
    public int PdfGuardados { get; set; }
}
