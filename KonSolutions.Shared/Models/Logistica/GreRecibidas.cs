namespace KONSolutions.Shared.Models.Logistica;

/// <summary>Una guía de remisión recibida (LOGISTICA.GRE_RECIBIDAS), como sale en la lista: de remitente (09) o de transportista (31).</summary>
public class GreRecibida
{
    public decimal NUM_GRE { get; set; }
    public string COD_TIPO_GUIA { get; set; } = "";
    public string? DES_TIPO_GUIA { get; set; }
    public string NUM_SERIE { get; set; } = "";
    public string NUM_CORRELATIVO { get; set; } = "";
    public string? NUM_GUIA { get; set; }
    public DateTime FEC_EMISION { get; set; }
    public DateTime? FEC_INICIO_TRASLADO { get; set; }
    public string? NUM_RUC_EMISOR { get; set; }
    public string? NUM_RUC_REMITENTE { get; set; }
    public string? DES_REMITENTE { get; set; }
    /// <summary>El remitente (proveedor) en el maestro de entidades, buscado por el RUC; vacío si no está registrado.</summary>
    public decimal? COD_ENTIDAD { get; set; }
    public string? DES_ENTIDAD { get; set; }
    public string? NUM_RUC_TRANSPORTISTA { get; set; }
    public string? DES_TRANSPORTISTA { get; set; }
    public string? COD_MOTIVO_TRASLADO { get; set; }
    public string? DES_MOTIVO_TRASLADO { get; set; }
    public decimal? CAN_PESO_BRUTO_TOTAL { get; set; }
    public string? COD_UNIDAD_PESO { get; set; }
    public string COD_ESTADO_USO { get; set; } = "PEN";
    public string? DES_ESTADO_USO { get; set; }
    /// <summary>El par del estandar de estados (ESTANDAR.ESTADOS): el tipo de estado de la tabla y el estado. COD_ESTADO_USO es el alias de ese estado (PEN, USA, DES).</summary>
    public short COD_TIPO_ESTADO { get; set; }
    public short COD_ESTADO { get; set; }
    public string? DES_BACKCOLOR_ESTADO { get; set; }
    public string? DES_FORECOLOR_ESTADO { get; set; }
    public decimal? NUM_MOVIMIENTO_ALMACEN { get; set; }
    public int NUM_LINEAS { get; set; }
    public int NUM_LINEAS_SIN_PRODUCTO { get; set; }
    public string? DES_NOMBRE_ARCHIVO { get; set; }
    public string? DES_NOMBRE_ARCHIVO_PDF { get; set; }
    public DateTime? FEC_REGISTRO { get; set; }
    /// <summary>Resultado de la validación: VAL en regla, OBS con observaciones (validación propia; SUNAT no consulta guías de terceros).</summary>
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
}

public class GreRecibidaFicha : GreRecibida
{
    public TimeSpan? HOR_EMISION { get; set; }
    public string? COD_TIPO_DOC_DESTINATARIO { get; set; }
    public string? NUM_DOC_DESTINATARIO { get; set; }
    public string? DES_DESTINATARIO { get; set; }
    public string? NUM_REGISTRO_MTC { get; set; }
    public string? COD_MODALIDAD_TRASLADO { get; set; }
    public decimal? NUM_BULTOS { get; set; }
    public string? COD_UBIGEO_PARTIDA { get; set; }
    public string? DES_DIRECCION_PARTIDA { get; set; }
    public string? COD_UBIGEO_LLEGADA { get; set; }
    public string? DES_DIRECCION_LLEGADA { get; set; }
    /// <summary>JSON con las placas.</summary>
    public string? DES_VEHICULOS { get; set; }
    public string? DES_CONDUCTORES { get; set; }
    /// <summary>JSON con los documentos que cita la guía (factura, orden de compra...).</summary>
    public string? DES_DOCUMENTOS_RELACIONADOS { get; set; }
    public string? DES_LEYENDAS { get; set; }
}

public class GreRecibidaLinea
{
    public decimal NUM_GRE { get; set; }
    public int NUM_ITEM { get; set; }
    public string? COD_PRODUCTO_VENDEDOR { get; set; }
    public string? COD_PRODUCTO_SUNAT { get; set; }
    public string? COD_GTIN { get; set; }
    public string? DES_PRODUCTO { get; set; }
    public string? COD_UNIDAD_SUNAT { get; set; }
    public decimal? CAN_PRODUCTO { get; set; }
    public string? NUM_LOTE { get; set; }
    public DateTime? FEC_VENCIMIENTO_LOTE { get; set; }
    public string? DES_SERIES { get; set; }
    public string? NUM_ORDEN_COMPRA_ITEM { get; set; }
    public string? COD_PRODUCTO { get; set; }
    public string? DES_PRODUCTO_EMPRESA { get; set; }
    public string? COD_ORIGEN_ASIGNACION { get; set; }
    /// <summary>El lote interno (LOGISTICA.LOTES_CONTROL) que se registró para el lote del proveedor de la línea.</summary>
    public string? NUM_LOTE_CONTROL { get; set; }
}

public class GreRecibidaSearchDto
{
    public string? SEARCH { get; set; }
    public string? NUM_RUC_REMITENTE { get; set; }
    /// <summary>09 remitente, 31 transportista; vacío trae las dos.</summary>
    public string? COD_TIPO_GUIA { get; set; }
    public string? COD_ESTADO_USO { get; set; }
    public DateTime? FEC_DESDE { get; set; }
    public DateTime? FEC_HASTA { get; set; }
    public int? NUM_FILAS { get; set; }
}

public class GreRecibidaXml
{
    public decimal NUM_GRE { get; set; }
    public string? DES_NOMBRE_ARCHIVO { get; set; }
    public string DES_XML { get; set; } = "";
}

public class GreRecibidaGenerarDto
{
    /// <summary>Lo eligen quien selecciona la guía: no hay valores por omisión.</summary>
    public string? COD_ALMACEN { get; set; }
    /// <summary>Vacío usa la clase de movimiento de almacén de Configuración general.</summary>
    public string? COD_CLASE_MOVIMIENTO { get; set; }
    public string? COD_TIPO_MOVIMIENTO { get; set; }
    /// <summary>Solo hace falta si el almacén tiene más de una ubicación.</summary>
    public string? COD_UBICACION_ALMACEN { get; set; }
    /// <summary>Vacío es hoy.</summary>
    public DateTime? FEC_MOVIMIENTO { get; set; }
    public bool FLG_RECORDAR_EQUIVALENCIAS { get; set; }
}

public class GreRecibidaGenerarResult
{
    public decimal NUM_MOVIMIENTO_ALMACEN { get; set; }
    public int NUM_LINEAS { get; set; }
    public int NUM_LINEAS_UNIDAD_SIN_EQUIVALENCIA { get; set; }
    public int NUM_EQUIVALENCIAS_RECORDADAS { get; set; }
    public int NUM_EQUIVALENCIAS_NO_RECORDADAS { get; set; }
}
