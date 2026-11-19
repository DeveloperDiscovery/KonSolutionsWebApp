namespace KONSolutions.Shared.Models.Ingenieria;

// ---- Operaciones y tiempos estandar (scripts SQL 119 a 122) ----

// ===== O1: catalogos =====

/// <summary>Una fila de cualquiera de los cuatro catalogos de operaciones. El significado de los campos depende del catalogo:
/// categorias (A-D): DES_DETALLE = criterio, NUM_ORDEN; maquinas: FLG_1 = requiere maquina; metodos de tiempo: solo codigo y descripcion;
/// distribucion de linea: COD_REGLA (MAX_PUESTO o SUMA_OPERARIOS), FLG_1 = flujo unitario, FLG_2 = requiere precedencias.</summary>
public class CatalogoOperacionItem
{
    public string COD { get; set; } = "";
    public string DES { get; set; } = "";
    public string? DES_DETALLE { get; set; }
    public int? NUM_ORDEN { get; set; }
    public bool? FLG_1 { get; set; }
    public bool? FLG_2 { get; set; }
    public string? COD_REGLA { get; set; }
    public short? COD_TIPO_ESTADO { get; set; }
    public short? COD_ESTADO { get; set; }
    public string? DES_ESTADO { get; set; }
    public string? DES_BACKCOLOR { get; set; }
    public string? DES_FORECOLOR { get; set; }
}

// ===== O2: catalogo de operaciones y composicion =====

public class Operacion
{
    // Auditoria (la muestra PanelAuditoria en el dialogo)
    public string? COD_USUARIO_REGISTRO { get; set; }
    public string? COD_ESTACION_REGISTRO { get; set; }
    public DateTime? FEC_REGISTRO { get; set; }
    public string? COD_USUARIO_ACTUALIZACION { get; set; }
    public string? COD_ESTACION_ACTUALIZACION { get; set; }
    public DateTime? FEC_ACTUALIZACION { get; set; }
    public string COD_OPERACION { get; set; } = "";
    public string DES_OPERACION { get; set; } = "";
    public string COD_TIPO_OPERACION_PRODUCCION { get; set; } = "";
    public string COD_PROCESO { get; set; } = "";
    public string? COD_SUB_PROCESO { get; set; }
    public string? COD_CATEGORIA_OCUPACIONAL { get; set; }
    public string? COD_TIPO_MAQUINA { get; set; }
    public string COD_UNIDAD_MEDIDA_TIEMPO { get; set; } = "MIN";
    public string? DES_OBSERVACION { get; set; }
    public short? COD_TIPO_ESTADO { get; set; }
    public short? COD_ESTADO { get; set; }
    public string? DES_TIPO_OPERACION_PRODUCCION { get; set; }
    public string? COD_CLASE_OPERACION_PRODUCCION { get; set; }
    public string? DES_CLASE_OPERACION_PRODUCCION { get; set; }
    public string? COD_FUNCION_OPERACION { get; set; }
    public string? DES_FUNCION_OPERACION { get; set; }
    public string? DES_PROCESO { get; set; }
    public string? DES_SUB_PROCESO { get; set; }
    public string? DES_CATEGORIA_OCUPACIONAL { get; set; }
    public string? DES_TIPO_MAQUINA { get; set; }
    public int CAN_COMPONENTES { get; set; }
    public bool FLG_COMPUESTA { get; set; }
    public int CAN_USADA_EN { get; set; }
    public short? NUM_VERSION_VIGENTE { get; set; }
    public decimal? VAL_SAM { get; set; }
    public string? DES_ESTADO { get; set; }
    public string? DES_BACKCOLOR { get; set; }
    public string? DES_FORECOLOR { get; set; }
}

/// <summary>Alta o cambio de una operacion (el codigo no cambia).</summary>
public class OperacionGuardarDto
{
    public string COD_OPERACION { get; set; } = "";
    public string DES_OPERACION { get; set; } = "";
    public string COD_TIPO_OPERACION_PRODUCCION { get; set; } = "";
    public string COD_PROCESO { get; set; } = "";
    public string? COD_SUB_PROCESO { get; set; }
    public string? COD_CATEGORIA_OCUPACIONAL { get; set; }
    public string? COD_TIPO_MAQUINA { get; set; }
    public string? COD_UNIDAD_MEDIDA_TIEMPO { get; set; }
    public string? DES_OBSERVACION { get; set; }
    public short? COD_TIPO_ESTADO { get; set; }
    public short? COD_ESTADO { get; set; }
}

public class OperacionComponente
{
    public string COD_OPERACION_PADRE { get; set; } = "";
    public string COD_OPERACION_HIJA { get; set; } = "";
    public int NUM_ORDEN { get; set; }
    public decimal VAL_FRECUENCIA { get; set; } = 1;
    public bool FLG_PARALELO { get; set; }
    public string? DES_OPERACION_HIJA { get; set; }
    public string? COD_CATEGORIA_OCUPACIONAL { get; set; }
    public string? COD_TIPO_MAQUINA { get; set; }
    public bool FLG_COMPUESTA { get; set; }
}

public class OperacionComponenteDto
{
    public string COD_OPERACION_HIJA { get; set; } = "";
    public decimal VAL_FRECUENCIA { get; set; } = 1;
    public bool FLG_PARALELO { get; set; }
}

/// <summary>Reemplaza TODOS los componentes de la operacion (el orden es el de la lista; vacia = la deja como hoja).</summary>
public class OperacionComponentesGuardarDto
{
    public string COD_OPERACION_PADRE { get; set; } = "";
    public List<OperacionComponenteDto> Componentes { get; set; } = new();
}

/// <summary>Un nodo del arbol de una operacion (con el SAM cuando se pide el arbol con tiempos).</summary>
public class OperacionArbolNodo
{
    public string COD_OPERACION { get; set; } = "";
    public string? COD_PADRE { get; set; }
    public int NIVEL { get; set; }
    public decimal FREC_ACUM { get; set; }
    public decimal VAL_FRECUENCIA { get; set; }
    public int NUM_ORDEN { get; set; }
    public bool FLG_PARALELO { get; set; }
    public string RUTA { get; set; } = "";
    public string? DES_OPERACION { get; set; }
    public string? COD_CATEGORIA_OCUPACIONAL { get; set; }
    public string? COD_TIPO_MAQUINA { get; set; }
    public bool FLG_HOJA { get; set; }
    public short? NUM_VERSION_VIGENTE { get; set; }
    public decimal? VAL_SAM_PROPIO { get; set; }
    public decimal? VAL_SAM_NODO { get; set; }
    public decimal? VAL_SAM_PRENDA { get; set; }
    public int CAN_HOJAS_SIN_TIEMPO { get; set; }
}

public class OperacionUsadaEn
{
    public string COD_OPERACION { get; set; } = "";
    public string? DES_OPERACION { get; set; }
    public int NIVEL { get; set; }
    public decimal FREC_ACUM { get; set; }
    public string CAMINO { get; set; } = "";
    public bool FLG_RAIZ { get; set; }
}

public class OperacionUsadaEnRuta
{
    public string COD_RUTA_PRODUCCION { get; set; } = "";
    public string? DES_RUTA_PRODUCCION { get; set; }
    public int CAN_FILAS { get; set; }
    public decimal? VAL_FRECUENCIA { get; set; }
    public decimal? VAL_SAM { get; set; }
    public short? NUM_VERSION_TIEMPO { get; set; }
    public bool FLG_TIEMPO_PROPIO { get; set; }
}

// ===== O3: tiempos y estudios =====

public class OperacionTiempo
{
    public string COD_OPERACION { get; set; } = "";
    public short NUM_VERSION { get; set; }
    public string COD_METODO_TIEMPO { get; set; } = "";
    public string? DES_METODO_TIEMPO { get; set; }
    public int? NUM_ESTUDIO_TIEMPO { get; set; }
    public decimal VAL_TIEMPO_BASE { get; set; }
    public decimal VAL_SUPLEMENTO_PCT { get; set; }
    public decimal VAL_SAM { get; set; }
    public DateTime? FEC_VIGENCIA_DESDE { get; set; }
    public DateTime? FEC_VIGENCIA_HASTA { get; set; }
    public string? DES_OBSERVACION { get; set; }
    public short COD_TIPO_ESTADO { get; set; }
    public short COD_ESTADO { get; set; }
    public string? DES_ESTADO { get; set; }
    public string? COD_ESTADO_ALIAS { get; set; }
    public string? DES_BACKCOLOR { get; set; }
    public string? DES_FORECOLOR { get; set; }
    public string? COD_USUARIO_REGISTRO { get; set; }
    public DateTime? FEC_REGISTRO { get; set; }
    public bool FLG_VIGENTE { get; set; }
}

/// <summary>Version nueva (NUM_VERSION nulo) o cambio de un borrador. Con estudio y sin tiempo base, el base es el del estudio.</summary>
public class OperacionTiempoGuardarDto
{
    public string COD_OPERACION { get; set; } = "";
    public short? NUM_VERSION { get; set; }
    public string COD_METODO_TIEMPO { get; set; } = "";
    public int? NUM_ESTUDIO_TIEMPO { get; set; }
    public decimal? VAL_TIEMPO_BASE { get; set; }
    public decimal VAL_SUPLEMENTO_PCT { get; set; }
    public string? DES_OBSERVACION { get; set; }
}

public class OperacionTiempoClaveDto
{
    public string COD_OPERACION { get; set; } = "";
    public short NUM_VERSION { get; set; }
    /// <summary>Solo para aprobar: desde cuando rige (hoy si no se indica).</summary>
    public DateTime? FEC_VIGENCIA_DESDE { get; set; }
}

public class OperacionTiempoResultado
{
    public string COD_OPERACION { get; set; } = "";
    public short NUM_VERSION { get; set; }
}

public class EstudioTiempo
{
    public int NUM_ESTUDIO_TIEMPO { get; set; }
    public string COD_OPERACION { get; set; } = "";
    public string COD_METODO_TIEMPO { get; set; } = "";
    public string? DES_METODO_TIEMPO { get; set; }
    public DateTime FEC_ESTUDIO { get; set; }
    public decimal? COD_ENTIDAD_ANALISTA { get; set; }
    public decimal VAL_RITMO_PCT { get; set; }
    public int? CAN_CICLOS { get; set; }
    public decimal VAL_TIEMPO_BASE { get; set; }
    public decimal VAL_TIEMPO_MAQUINA { get; set; }
    public string? DES_OBSERVACION { get; set; }
    public int CAN_ELEMENTOS { get; set; }
    public bool FLG_APROBADO { get; set; }
    public int CAN_VERSIONES { get; set; }
}

public class EstudioTiempoElemento
{
    public int NUM_ESTUDIO_TIEMPO { get; set; }
    public int NUM_ELEMENTO { get; set; }
    public string DES_ELEMENTO { get; set; } = "";
    public decimal VAL_TIEMPO_OBSERVADO { get; set; }
    public decimal VAL_FRECUENCIA { get; set; } = 1;
    public bool FLG_ELEMENTO_MAQUINA { get; set; }
}

public class EstudioTiempoElementoDto
{
    public string DES_ELEMENTO { get; set; } = "";
    public decimal VAL_TIEMPO_OBSERVADO { get; set; }
    public decimal VAL_FRECUENCIA { get; set; } = 1;
    public bool FLG_ELEMENTO_MAQUINA { get; set; }
}

public class EstudioTiempoGuardarDto
{
    public int? NUM_ESTUDIO_TIEMPO { get; set; }
    public string COD_OPERACION { get; set; } = "";
    public string COD_METODO_TIEMPO { get; set; } = "";
    public DateTime? FEC_ESTUDIO { get; set; }
    public decimal? COD_ENTIDAD_ANALISTA { get; set; }
    public decimal VAL_RITMO_PCT { get; set; } = 100;
    public int? CAN_CICLOS { get; set; }
    public string? DES_OBSERVACION { get; set; }
    public List<EstudioTiempoElementoDto> Elementos { get; set; } = new();
}

public class EstudioTiempoResultado
{
    public int NUM_ESTUDIO_TIEMPO { get; set; }
    public decimal VAL_TIEMPO_BASE { get; set; }
    public decimal VAL_TIEMPO_MAQUINA { get; set; }
}

public class SamPorCategoria
{
    public string COD_CATEGORIA_OCUPACIONAL { get; set; } = "";
    public string? DES_CATEGORIA_OCUPACIONAL { get; set; }
    public decimal VAL_SAM { get; set; }
    public int CAN_HOJAS_SIN_TIEMPO { get; set; }
    public int CAN_HOJAS { get; set; }
}

public class SamPorClase
{
    public string COD_CLASE_OPERACION_PRODUCCION { get; set; } = "";
    public string? DES_CLASE_OPERACION_PRODUCCION { get; set; }
    public decimal VAL_SAM { get; set; }
}

// ===== O4: ruta con tiempos =====

public class RutaTiempoResumen
{
    public string COD_RUTA_PRODUCCION { get; set; } = "";
    public string? DES_RUTA_PRODUCCION { get; set; }
    public string? COD_TIPO_RUTA_PRODUCCION { get; set; }
    public int CAN_FILAS_CATALOGO { get; set; }
    public int CAN_HOJAS_CATALOGO { get; set; }
    public int CAN_HOJAS_SIN_TIEMPO { get; set; }
    public decimal? VAL_SAM_TOTAL { get; set; }
}

/// <summary>Una fila de la ruta (proceso, subproceso u operacion) con sus sumas.</summary>
public class RutaNodo
{
    public int NUM_ESTRUCTURA_RUTA_PROCESO { get; set; }
    public int? NUM_ESTRUCTURA_RUTA_PROCESO_PADRE { get; set; }
    public int NIVEL { get; set; }
    public decimal FREC_ACUM { get; set; }
    public decimal VAL_FRECUENCIA { get; set; }
    public int NUM_ORDEN { get; set; }
    public bool FLG_PARALELO { get; set; }
    /// <summary>PRC, SPR u OPE.</summary>
    public string DES_TIPO_ELEMENTO { get; set; } = "";
    public string? COD_PROCESO { get; set; }
    public string? DES_PROCESO { get; set; }
    public string? COD_SUB_PROCESO { get; set; }
    public string? DES_SUB_PROCESO { get; set; }
    public string? COD_TIPO_OPERACION_PRODUCCION { get; set; }
    public string? DES_TIPO_OPERACION_PRODUCCION { get; set; }
    public string? COD_OPERACION { get; set; }
    public string? DES_OPERACION { get; set; }
    public string? COD_CATEGORIA_OCUPACIONAL { get; set; }
    public string? COD_TIPO_MAQUINA { get; set; }
    public decimal? CAN_LEAD_TIME { get; set; }
    public string? COD_UNIDAD_MEDIDA_TIEMPO { get; set; }
    public bool FLG_HOJA { get; set; }
    public short? NUM_VERSION_TIEMPO { get; set; }
    public bool FLG_TIEMPO_PROPIO { get; set; }
    public decimal? VAL_SAM_PROPIO { get; set; }
    public decimal? VAL_SAM_NODO { get; set; }
    public decimal? VAL_SAM_PRENDA { get; set; }
    public int CAN_HOJAS_SIN_TIEMPO { get; set; }
    public bool FLG_DESACTUALIZADO { get; set; }
    public string RUTA { get; set; } = "";
}

public class RutaProcesoAgregarDto
{
    public string COD_RUTA_PRODUCCION { get; set; } = "";
    public string COD_PROCESO { get; set; } = "";
}

public class RutaSubprocesoAgregarDto
{
    public string COD_RUTA_PRODUCCION { get; set; } = "";
    public int NUM_PADRE { get; set; }
    public string COD_SUB_PROCESO { get; set; } = "";
}

public class RutaOperacionAgregarDto
{
    public string COD_RUTA_PRODUCCION { get; set; } = "";
    public int NUM_PADRE { get; set; }
    public string COD_OPERACION { get; set; } = "";
    public decimal VAL_FRECUENCIA { get; set; } = 1;
    public DateTime? FECHA { get; set; }
}

public class RutaOperacionAgregarResultado
{
    public int NUM_ESTRUCTURA_RUTA_PROCESO { get; set; }
    public int CAN_FILAS { get; set; }
}

public class RutaFilaClaveDto
{
    public string COD_RUTA_PRODUCCION { get; set; } = "";
    public int NUM_ESTRUCTURA_RUTA_PROCESO { get; set; }
    /// <summary>Solo para fijar un SAM propio: nulo = vuelve al tiempo del catalogo.</summary>
    public decimal? VAL_SAM { get; set; }
}

public class RutaOperacionEditarDto
{
    public string COD_RUTA_PRODUCCION { get; set; } = "";
    public int NUM_ESTRUCTURA_RUTA_PROCESO { get; set; }
    /// <summary>Veces que ocurre la operacion por unidad de producto (mayor que cero).</summary>
    public decimal VAL_FRECUENCIA { get; set; } = 1;
    public bool FLG_PARALELO { get; set; }
    /// <summary>Categoria ocupacional (A a D); nulo = sin categoria.</summary>
    public string? COD_CATEGORIA_OCUPACIONAL { get; set; }
}

public class RutaMoverDto
{
    public string COD_RUTA_PRODUCCION { get; set; } = "";
    public int NUM_ESTRUCTURA_RUTA_PROCESO { get; set; }
    /// <summary>-1 = subir, 1 = bajar entre sus hermanos.</summary>
    public int DIRECCION { get; set; }
}

public class RutaRecalcularDto
{
    public string COD_RUTA_PRODUCCION { get; set; } = "";
    public DateTime? FECHA { get; set; }
}

public class RutaPrecedencia
{
    public string COD_RUTA_PRODUCCION { get; set; } = "";
    public int NUM_ESTRUCTURA_ANTECESORA { get; set; }
    public int NUM_ESTRUCTURA_SUCESORA { get; set; }
    public string? DES_ANTECESORA { get; set; }
    public string? DES_SUCESORA { get; set; }
}

public class RutaPrecedenciaDto
{
    public int NUM_ESTRUCTURA_ANTECESORA { get; set; }
    public int NUM_ESTRUCTURA_SUCESORA { get; set; }
}

public class RutaPrecedenciasGuardarDto
{
    public string COD_RUTA_PRODUCCION { get; set; } = "";
    public List<RutaPrecedenciaDto> Precedencias { get; set; } = new();
}

public class RutaPrecedenciasGenerarDto
{
    public string COD_RUTA_PRODUCCION { get; set; } = "";
    public bool REEMPLAZAR { get; set; } = true;
}

/// <summary>Resultado numerico de un procedimiento (cuantas filas cambiaron, cuantas precedencias quedaron...).</summary>
public class CantidadResultado
{
    public int CANTIDAD { get; set; }
}

// ===== Matrices de operaciones (script SQL 130, API ingenieria/matrices) =====
public class MatrizResumen
{
    public string COD_RUTA_PRODUCCION { get; set; } = "";
    public string? DES_RUTA_PRODUCCION { get; set; }
    public string? COD_TIPO_RUTA_PRODUCCION { get; set; }
    public string? DES_TIPO_RUTA_PRODUCCION { get; set; }
    public string? COD_GRUPO_PRODUCTO { get; set; }
    public string? COD_FAMILIA_PRODUCTO { get; set; }
    public string? COD_SUB_FAMILIA_PRODUCTO { get; set; }
    public string? DES_SUB_FAMILIA_PRODUCTO { get; set; }
    public int CAN_ELEMENTOS { get; set; }
    public int CAN_HOJAS_CATALOGO { get; set; }
    public int CAN_HOJAS_SIN_TIEMPO { get; set; }
    public decimal? VAL_SAM_TOTAL { get; set; }
    public int CAN_COPIAS { get; set; }
    public int CAN_VERSIONES { get; set; }
    public DateTime? FEC_REGISTRO { get; set; }
    public DateTime? FEC_ACTUALIZACION { get; set; }
}

public class MatrizGuardarDto
{
    public string? COD_RUTA_PRODUCCION { get; set; }
    public string DES_RUTA { get; set; } = "";
    public string COD_TIPO_RUTA { get; set; } = "PRD";
    public string? COD_GRUPO_PRODUCTO { get; set; }
    public string? COD_FAMILIA_PRODUCTO { get; set; }
    public string? COD_SUB_FAMILIA_PRODUCTO { get; set; }
}

public class MatrizDuplicarDto
{
    public string COD_RUTA_PRODUCCION { get; set; } = "";
    public string? DES_RUTA { get; set; }
}

public class MatrizAsignarResultado
{
    public string COD_RUTA_PRODUCCION { get; set; } = "";
    public int ELEMENTOS_COPIADOS { get; set; }
    public int PRECEDENCIAS_COPIADAS { get; set; }
}

// ===== Ruta de operaciones de una version de diseno (scripts SQL 130 y 132) =====
public class MatrizParaVersion
{
    public string COD_RUTA_PRODUCCION { get; set; } = "";
    public string? DES_RUTA_PRODUCCION { get; set; }
    public string? COD_GRUPO_PRODUCTO { get; set; }
    public string? COD_FAMILIA_PRODUCTO { get; set; }
    public string? COD_SUB_FAMILIA_PRODUCTO { get; set; }
    public string? DES_SUB_FAMILIA_PRODUCTO { get; set; }
    public int CAN_ELEMENTOS { get; set; }
    public int CAN_HOJAS_CATALOGO { get; set; }
    public int CAN_HOJAS_SIN_TIEMPO { get; set; }
    public decimal? VAL_SAM_TOTAL { get; set; }
    public bool FLG_SUGERIDA { get; set; }
}

public class MatrizAsignarDto
{
    public string COD_DISENO { get; set; } = "";
    public int NUM_VERSION { get; set; }
    public string COD_MATRIZ { get; set; } = "";
    public DateTime? FEC_TIEMPOS { get; set; }
    public bool REEMPLAZAR { get; set; }
}

public class GuardarComoMatrizDto
{
    public string COD_DISENO { get; set; } = "";
    public int NUM_VERSION { get; set; }
    public string DES_MATRIZ { get; set; } = "";
    public bool SIN_SUB_FAMILIA { get; set; }
}

public class RutaIndependizarResultado
{
    public string COD_RUTA_PRODUCCION { get; set; } = "";
    public int ELEMENTOS_COPIADOS { get; set; }
    public int PRECEDENCIAS_COPIADAS { get; set; }
    public bool FLG_COPIADA { get; set; }
}

public class DisenosRutaOperacionesInfo
{
    public string COD_DISENO { get; set; } = "";
    public int NUM_VERSION { get; set; }
    public bool FLG_BLOQUEADO { get; set; }
    public string? COD_RUTA_PRODUCCION { get; set; }
    public string? DES_RUTA_PRODUCCION { get; set; }
    public string? COD_TIPO_RUTA_PRODUCCION { get; set; }
    public int USADA_POR { get; set; }
    public int BLOQUEADAS { get; set; }
    public int ELEMENTOS { get; set; }
    public int CAN_HOJAS_SIN_TIEMPO { get; set; }
    public decimal? VAL_SAM_TOTAL { get; set; }
    public string? COD_RUTA_ORIGEN { get; set; }
    public string? DES_RUTA_ORIGEN { get; set; }
    public bool FLG_ORIGEN_MATRIZ { get; set; }
    public DateTime? FEC_ORIGEN { get; set; }
}
