namespace KONSolutions.Shared.Models.Ingenieria;

// ===== O5: personal por categoria ocupacional (script SQL 124) =====

/// <summary>Una persona de produccion con su categoria (A-D), su proceso y, si la tiene, su operacion fija.</summary>
public class PersonalOperacionItem
{
    // Auditoria (la muestra PanelAuditoria en el dialogo)
    public string? COD_USUARIO_REGISTRO { get; set; }
    public string? COD_ESTACION_REGISTRO { get; set; }
    public DateTime? FEC_REGISTRO { get; set; }
    public string? COD_USUARIO_ACTUALIZACION { get; set; }
    public string? COD_ESTACION_ACTUALIZACION { get; set; }
    public DateTime? FEC_ACTUALIZACION { get; set; }
    public int COD_ENTIDAD_PERSONAL { get; set; }
    public string? COD_PERSONAL { get; set; }
    public string? DES_PERSONAL { get; set; }
    public bool FLG_CESADO { get; set; }
    public string COD_CATEGORIA_OCUPACIONAL { get; set; } = "";
    public string? DES_CATEGORIA_OCUPACIONAL { get; set; }
    public string COD_PROCESO { get; set; } = "";
    public string? DES_PROCESO { get; set; }
    public string? COD_SUB_PROCESO { get; set; }
    public string? DES_SUB_PROCESO { get; set; }
    public string? COD_OPERACION { get; set; }
    public string? DES_OPERACION { get; set; }
    public short? COD_TIPO_ESTADO { get; set; }
    public short? COD_ESTADO { get; set; }
    public string? DES_ESTADO { get; set; }
    public string? DES_BACKCOLOR { get; set; }
    public string? DES_FORECOLOR { get; set; }
}

public class PersonalOperacionGuardarDto
{
    public int COD_ENTIDAD_PERSONAL { get; set; }
    public string COD_CATEGORIA_OCUPACIONAL { get; set; } = "";
    public string COD_PROCESO { get; set; } = "";
    public string? COD_SUB_PROCESO { get; set; }
    public string? COD_OPERACION { get; set; }
    public short? COD_TIPO_ESTADO { get; set; }
    public short? COD_ESTADO { get; set; }
}

/// <summary>Persona activa que todavia no tiene categoria.</summary>
public class PersonaDisponible
{
    public int COD_ENTIDAD_PERSONAL { get; set; }
    public string? COD_PERSONAL { get; set; }
    public string? DES_PERSONAL { get; set; }
}

/// <summary>Personal activo por categoria y proceso.</summary>
public class PersonalResumenItem
{
    public string COD_CATEGORIA_OCUPACIONAL { get; set; } = "";
    public string? DES_CATEGORIA_OCUPACIONAL { get; set; }
    public string COD_PROCESO { get; set; } = "";
    public string? DES_PROCESO { get; set; }
    public int CAN_PERSONAS { get; set; }
    public int CAN_CON_OPERACION { get; set; }
}

// ===== O6: armado de la linea (script SQL 125) =====

/// <summary>Un armado con su balance (los totales vienen de FN_ARMADO_TOTALES).</summary>
public class Armado
{
    // Auditoria (la muestra PanelAuditoria en el dialogo)
    public string? COD_USUARIO_REGISTRO { get; set; }
    public string? COD_ESTACION_REGISTRO { get; set; }
    public DateTime? FEC_REGISTRO { get; set; }
    public string? COD_USUARIO_ACTUALIZACION { get; set; }
    public string? COD_ESTACION_ACTUALIZACION { get; set; }
    public DateTime? FEC_ACTUALIZACION { get; set; }
    public string COD_ARMADO { get; set; } = "";
    public string DES_ARMADO { get; set; } = "";
    public string COD_RUTA_PRODUCCION { get; set; } = "";
    public string COD_PROCESO { get; set; } = "";
    public string COD_TIPO_DISTRIBUCION { get; set; } = "";
    public string? COD_LINEA_PRODUCCION { get; set; }
    public decimal VAL_MINUTOS_TURNO { get; set; }
    public decimal TAS_EFICIENCIA { get; set; }
    public decimal? VAL_META_UNIDADES { get; set; }
    public string? DES_OBSERVACION { get; set; }
    public short? COD_TIPO_ESTADO { get; set; }
    public short? COD_ESTADO { get; set; }
    public string? DES_RUTA_PRODUCCION { get; set; }
    public string? DES_PROCESO { get; set; }
    public string? DES_TIPO_DISTRIBUCION { get; set; }
    public string? COD_REGLA_CICLO { get; set; }
    public string? DES_LINEA_PRODUCCION { get; set; }
    public decimal? VAL_TAKT { get; set; }
    public int? CAN_PUESTOS { get; set; }
    public int? CAN_OPERARIOS { get; set; }
    public decimal? VAL_SAM_ASIGNADO { get; set; }
    public decimal? VAL_SAM_RUTA { get; set; }
    public decimal? VAL_CICLO { get; set; }
    public decimal? VAL_CAPACIDAD_TURNO { get; set; }
    public decimal? TAS_EFICIENCIA_BALANCEO { get; set; }
    public int? CAN_HOJAS_RUTA { get; set; }
    public int? CAN_HOJAS_SIN_ASIGNAR { get; set; }
    public string? DES_ESTADO { get; set; }
    public string? DES_BACKCOLOR { get; set; }
    public string? DES_FORECOLOR { get; set; }
}

public class ArmadoGuardarDto
{
    public string COD_ARMADO { get; set; } = "";
    public string DES_ARMADO { get; set; } = "";
    public string COD_RUTA_PRODUCCION { get; set; } = "";
    public string COD_PROCESO { get; set; } = "";
    public string COD_TIPO_DISTRIBUCION { get; set; } = "";
    public string? COD_LINEA_PRODUCCION { get; set; }
    public decimal? VAL_MINUTOS_TURNO { get; set; }
    public decimal? TAS_EFICIENCIA { get; set; }
    public decimal? VAL_META_UNIDADES { get; set; }
    public string? DES_OBSERVACION { get; set; }
    public short? COD_TIPO_ESTADO { get; set; }
    public short? COD_ESTADO { get; set; }
}

/// <summary>Un puesto del armado con sus minutos, su ciclo y su carga contra el takt.</summary>
public class ArmadoPuesto
{
    public int NUM_PUESTO { get; set; }
    public string DES_PUESTO { get; set; } = "";
    public int NUM_OPERARIOS { get; set; }
    public int CAN_HOJAS { get; set; }
    public decimal VAL_SAM_PUESTO { get; set; }
    public decimal VAL_CICLO_PUESTO { get; set; }
    public decimal? PCT_CARGA { get; set; }
    public string? COD_CATEGORIA_OCUPACIONAL { get; set; }
    public string? DES_CATEGORIA_OCUPACIONAL { get; set; }
}

/// <summary>Una operacion de la ruta (en el proceso del armado) y el puesto en que esta (vacio si esta libre).</summary>
public class ArmadoHoja
{
    public int NUM_SECUENCIA { get; set; }
    public int NUM_ESTRUCTURA_RUTA_PROCESO { get; set; }
    public string? COD_OPERACION { get; set; }
    public string? DES_OPERACION { get; set; }
    public string? COD_SUB_PROCESO { get; set; }
    public string? DES_SUB_PROCESO { get; set; }
    public string? COD_CATEGORIA_OCUPACIONAL { get; set; }
    public string? COD_TIPO_MAQUINA { get; set; }
    public bool FLG_PARALELO { get; set; }
    public decimal? VAL_SAM { get; set; }
    public decimal FREC_ACUM { get; set; }
    public decimal VAL_SAM_PRENDA { get; set; }
    public bool FLG_SIN_TIEMPO { get; set; }
    public int? NUM_PUESTO { get; set; }
}

public class ArmadoAviso
{
    /// <summary>E = error, A = aviso.</summary>
    public string COD_SEVERIDAD { get; set; } = "";
    public string COD_REGLA { get; set; } = "";
    public string DES_MENSAJE { get; set; } = "";
    public int? NUM_PUESTO { get; set; }
    public int? NUM_ESTRUCTURA_RUTA_PROCESO { get; set; }
}

public class ArmadoOperariosCategoria
{
    public string COD_CATEGORIA_OCUPACIONAL { get; set; } = "";
    public string? DES_CATEGORIA_OCUPACIONAL { get; set; }
    public int CAN_PUESTOS { get; set; }
    public int CAN_OPERARIOS { get; set; }
}

/// <summary>Un puesto a guardar: su numero, nombre, operarios y las filas de la ruta (NUM_ESTRUCTURA_RUTA_PROCESO) que hace.</summary>
public class ArmadoPuestoGuardarDto
{
    public int NUM_PUESTO { get; set; }
    public string DES_PUESTO { get; set; } = "";
    public int NUM_OPERARIOS { get; set; } = 1;
    public List<int> HOJAS { get; set; } = new();
}

public class ArmadoPuestosGuardarDto
{
    public string COD_ARMADO { get; set; } = "";
    public List<ArmadoPuestoGuardarDto> Puestos { get; set; } = new();
}

/// <summary>Resultado de sugerir el balance.</summary>
public class ArmadoSugerido
{
    public int CAN_PUESTOS { get; set; }
    public int CAN_OPERARIOS { get; set; }
}

// ===== O7: personal del armado, cobertura y comparacion de disposiciones (script SQL 127) =====

/// <summary>Una persona puesta en un puesto del armado.</summary>
public class ArmadoPersonaAsignada
{
    public int NUM_PUESTO { get; set; }
    public int COD_ENTIDAD_PERSONAL { get; set; }
    public string? COD_PERSONAL { get; set; }
    public string? DES_PERSONAL { get; set; }
    public string COD_CATEGORIA_OCUPACIONAL { get; set; } = "";
    public string? DES_CATEGORIA_OCUPACIONAL { get; set; }
    public string? COD_OPERACION { get; set; }
    public string? COD_CATEGORIA_PUESTO { get; set; }
    /// <summary>La categoria de la persona es mas alta que la que pide el puesto.</summary>
    public bool FLG_CATEGORIA_SUPERIOR { get; set; }
    /// <summary>La persona cesó o su categoria esta inactiva desde que se la asigno.</summary>
    public bool FLG_NO_VIGENTE { get; set; }
}

/// <summary>Una persona que se puede poner en el armado (activa, con categoria, del proceso del armado).</summary>
public class ArmadoPersonaDisponible
{
    public int COD_ENTIDAD_PERSONAL { get; set; }
    public string? COD_PERSONAL { get; set; }
    public string? DES_PERSONAL { get; set; }
    public string COD_CATEGORIA_OCUPACIONAL { get; set; } = "";
    public string? DES_CATEGORIA_OCUPACIONAL { get; set; }
    public int NUM_ORDEN_CATEGORIA { get; set; }
    public string? COD_SUB_PROCESO { get; set; }
    public string? COD_OPERACION { get; set; }
    public string? DES_OPERACION { get; set; }
    /// <summary>El puesto de este armado en que ya esta (vacio si esta libre).</summary>
    public int? NUM_PUESTO { get; set; }
}

/// <summary>Requeridos, disponibles y faltantes de una categoria.</summary>
public class ArmadoCobertura
{
    public string COD_CATEGORIA_OCUPACIONAL { get; set; } = "";
    public string? DES_CATEGORIA_OCUPACIONAL { get; set; }
    public int NUM_ORDEN { get; set; }
    public int CAN_REQUERIDOS { get; set; }
    public int CAN_DISPONIBLES { get; set; }
    public int CAN_ASIGNADOS { get; set; }
    public int CAN_CUBIERTOS_SUPERIOR { get; set; }
    public int CAN_FALTANTES { get; set; }
    public int CAN_SOBRANTES { get; set; }
}

public class ArmadoPersonalPuestoDto
{
    public int NUM_PUESTO { get; set; }
    public List<int> PERSONAS { get; set; } = new();
}

public class ArmadoPersonalGuardarDto
{
    public string COD_ARMADO { get; set; } = "";
    public List<ArmadoPersonalPuestoDto> Puestos { get; set; } = new();
}

public class ArmadoPersonalSugerido
{
    public int CAN_ASIGNADAS { get; set; }
    public int CAN_VACANTES { get; set; }
}

/// <summary>Una fila de la comparacion de disposiciones: ACTUAL (el reparto guardado) o SUGERIDO (el balance que armaria "sugerir" para esa disposicion).</summary>
public class ArmadoComparacion
{
    public string ORIGEN { get; set; } = "";
    public string COD_TIPO_DISTRIBUCION { get; set; } = "";
    public string? DES_TIPO_DISTRIBUCION { get; set; }
    public string? COD_REGLA_CICLO { get; set; }
    public int? CAN_PUESTOS { get; set; }
    public int? CAN_OPERARIOS { get; set; }
    public decimal? VAL_CICLO { get; set; }
    public decimal? VAL_CAPACIDAD_TURNO { get; set; }
    public decimal? TAS_EFICIENCIA_BALANCEO { get; set; }
    public bool? FLG_CUMPLE_META { get; set; }
    public int CAN_FALTANTES { get; set; }
    public int CAN_CUBIERTOS_SUPERIOR { get; set; }
}

public class ArmadoComparacionCategoria
{
    public string ORIGEN { get; set; } = "";
    public string COD_TIPO_DISTRIBUCION { get; set; } = "";
    public string COD_CATEGORIA_OCUPACIONAL { get; set; } = "";
    public string? DES_CATEGORIA_OCUPACIONAL { get; set; }
    public int NUM_ORDEN { get; set; }
    public int CAN_REQUERIDOS { get; set; }
    public int CAN_DISPONIBLES { get; set; }
    public int CAN_CUBIERTOS_SUPERIOR { get; set; }
    public int CAN_FALTANTES { get; set; }
}

/// <summary>Las dos listas de la comparacion de disposiciones.</summary>
public class ArmadoComparacionResultado
{
    public List<ArmadoComparacion> Disposiciones { get; set; } = new();
    public List<ArmadoComparacionCategoria> Categorias { get; set; } = new();
}
