namespace KONSolutions.Shared.Models.Ingenieria;

// ---- Especificaciones tecnicas de una version de diseno (script SQL 113) ----

/// <summary>Diseno y version, su clasificacion (la que decide que especificaciones aplican) y el estado de la version.
/// NUM_NIVEL: 1 = hay plantilla de la sub familia, 2 = de la familia, 3 = del grupo, 0 = no hay ninguna (diseno sin configurar).</summary>
public class DisenosEspecificacionesCabecera
{
    public string COD_DISENO { get; set; } = "";
    public int NUM_VERSION { get; set; }
    public string? DES_DISENO { get; set; }
    public string? DES_VERSION { get; set; }
    public string? COD_GRUPO_PRODUCTO { get; set; }
    public string? DES_GRUPO_PRODUCTO { get; set; }
    public string? COD_FAMILIA_PRODUCTO { get; set; }
    public string? DES_FAMILIA_PRODUCTO { get; set; }
    public string? COD_SUB_FAMILIA_PRODUCTO { get; set; }
    public string? DES_SUB_FAMILIA_PRODUCTO { get; set; }
    public decimal? COD_ENTIDAD { get; set; }
    public int NUM_NIVEL { get; set; }
    public bool FLG_BLOQUEADO { get; set; }
    public bool FLG_ANULADO { get; set; }
    public int? COD_TIPO_ESTADO { get; set; }
    public int? COD_ESTADO { get; set; }
    public string? DES_ESTADO { get; set; }
    public int CAN_ESPECIFICACIONES { get; set; }
    public int CAN_CON_VALOR { get; set; }
}

/// <summary>Una especificacion que la configuracion vigente pide en la ficha tecnica, con su tipo, su unidad y el valor de la version
/// (o el valor inicial de la configuracion mientras la version no tenga uno propio: FLG_GUARDADO = false).</summary>
public class DisenosEspecificacionesFila
{
    public string COD_ESPECIFICACION_TECNICA { get; set; } = "";
    public string? DES_ESPECIFICACION_TECNICA { get; set; }
    public string? DES_ESPECIFICACION_TECNICA_SINGULAR { get; set; }
    public string? COD_ESPECIFICACION_TECNICA_PADRE { get; set; }
    public int? NUM_ORDEN_PRESENTACION { get; set; }
    public int? COD_GRUPO_ESPECIFICACION_TECNICA { get; set; }
    public string? DES_GRUPO_ESPECIFICACION_TECNICA { get; set; }
    // tipo de dato (el mismo criterio que la ficha del producto: lista, entero, numerico, cadena, fecha y hora, fecha, hora, logico, color)
    public bool? FLG_LISTA { get; set; }
    public bool? FLG_ENTERO { get; set; }
    public bool? FLG_NUMERICO { get; set; }
    public bool? FLG_CADENA { get; set; }
    public bool? FLG_FECHA_HORA { get; set; }
    public bool? FLG_FECHA { get; set; }
    public bool? FLG_HORA { get; set; }
    public bool? FLG_LOGICO { get; set; }
    public bool? FLG_COLOR { get; set; }
    public int? CAN_ENTERO { get; set; }
    public int? CAN_DECIMAL { get; set; }
    public string? DES_PREFIJO { get; set; }
    public string? DES_SUFIJO { get; set; }
    public bool? FLG_RANGE { get; set; }
    public bool? FLG_DEPENDS_ENTITY { get; set; }
    public string? DES_UNIDAD_MEDIDA { get; set; }
    public string? COD_SIMBOLO { get; set; }
    public string? DES_UNIDAD_MEDIDA_DIVISORA { get; set; }
    public string? COD_SIMBOLO_DIVISORA { get; set; }
    public bool? FLG_INCLUIR_DESCRIPCION { get; set; }
    public bool? FLG_REGISTRO_COMERCIAL { get; set; }
    public bool? FLG_FICHA_TECNICA { get; set; }
    public bool? FLG_CONTROL_CALIDAD { get; set; }
    public bool FLG_GUARDADO { get; set; }
    // valor
    public int? COD_ITEM_TECNICO { get; set; }
    public string? VAL_ESPECIFICACION_TENICA { get; set; }
    public decimal? VAL_MINIMO { get; set; }
    public decimal? VAL_MAXIMO { get; set; }
    public DateTime? FEC_ESPECIFICACION_TENICA { get; set; }
    public TimeSpan? HOR_ESPECIFICACION_TENICA { get; set; }
    public bool? LOG_ESPECIFICACION_TENICA { get; set; }
    public string? STR_ESPECIFICACION_TENICA { get; set; }
    // auditoria del valor guardado
    public string? COD_USUARIO_REGISTRO { get; set; }
    public string? COD_ESTACION_REGISTRO { get; set; }
    public DateTime? FEC_REGISTRO { get; set; }
    public string? COD_USUARIO_ACTUALIZACION { get; set; }
    public string? COD_ESTACION_ACTUALIZACION { get; set; }
    public DateTime? FEC_ACTUALIZACION { get; set; }
}

public class DisenosEspecificacionesPanel
{
    public DisenosEspecificacionesCabecera Cabecera { get; set; } = new();
    public List<DisenosEspecificacionesFila> Filas { get; set; } = new();
}

/// <summary>El valor de UNA especificacion. Una fila sin ningun valor borra el valor guardado de esa especificacion.</summary>
public class DisenosEspecificacionesValorDto
{
    public string COD_ESPECIFICACION_TECNICA { get; set; } = "";
    public int? COD_ITEM_TECNICO { get; set; }
    public string? VAL_ESPECIFICACION_TENICA { get; set; }
    public decimal? VAL_MINIMO { get; set; }
    public decimal? VAL_MAXIMO { get; set; }
    public DateTime? FEC_ESPECIFICACION_TENICA { get; set; }
    public TimeSpan? HOR_ESPECIFICACION_TENICA { get; set; }
    public bool? LOG_ESPECIFICACION_TENICA { get; set; }
    public string? STR_ESPECIFICACION_TENICA { get; set; }
}

/// <summary>Todas las especificaciones del panel (con y sin valor): se graban juntas, en una sola transaccion.</summary>
public class DisenosEspecificacionesGuardarDto
{
    public string COD_DISENO { get; set; } = "";
    public int NUM_VERSION { get; set; }
    public List<DisenosEspecificacionesValorDto> Filas { get; set; } = new();
}

public class DisenosEspecificacionesResultado
{
    public int INSERTADAS { get; set; }
    public int ACTUALIZADAS { get; set; }
    public int ELIMINADAS { get; set; }
}
