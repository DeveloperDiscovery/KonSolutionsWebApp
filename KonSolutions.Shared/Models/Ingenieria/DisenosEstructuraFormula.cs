using KONSolutions.Shared.Models.Formulacion;

namespace KONSolutions.Shared.Models.Ingenieria;

// Formulas de consumo asignadas a las lineas de la estructura (script SQL 147).

/// <summary>Clave de una linea (insumo) de la estructura.</summary>
public class DisenosEstructuraLineaClaveDto
{
    public string COD_DISENO { get; set; } = string.Empty;
    public int NUM_VERSION { get; set; }
    public string COD_SECTOR_PRODUCTO { get; set; } = string.Empty;
    public string COD_COMPONENTE_PRODUCTO { get; set; } = string.Empty;
    public int NUM_SECUENCIA { get; set; }
    public string COD_PRODUCTO_COMPONENTE { get; set; } = string.Empty;
}

public class DisenosEstructuraFormulaAsignarDto : DisenosEstructuraLineaClaveDto
{
    public int NUM_FORMULACION { get; set; }
}

public class DisenosEstructuraFormulaCalcularDto : DisenosEstructuraLineaClaveDto
{
    /// <summary>false = solo calcula y devuelve el resultado, sin guardar CAN_TOTAL.</summary>
    public bool GUARDAR { get; set; } = true;
}

/// <summary>Formula ACTIVA que se puede asignar a la linea. FLG_COINCIDE = su sub familia (COD_GRUPO / FAMILIA / SUB_FAMILIA_PRODUCTO) es la del insumo (si no, es generica).</summary>
public class DisenosEstructuraFormulaDisponible
{
    public int NUM_FORMULACION { get; set; }
    public string DES_FORMULACION { get; set; } = string.Empty;
    public string? DES_ALIAS { get; set; }
    public string? DES_FORMULA { get; set; }
    public string? COD_GRUPO_PRODUCTO { get; set; }
    public string? COD_FAMILIA_PRODUCTO { get; set; }
    public string? COD_SUB_FAMILIA_PRODUCTO { get; set; }
    public string? DES_SUB_FAMILIA_PRODUCTO { get; set; }
    public int CAN_FICHAS { get; set; }
    public bool FLG_COINCIDE { get; set; }
}

/// <summary>Resultado de asignar o calcular: ES_VALIDA false trae el motivo en DES_ERROR (la asignacion, si hubo, queda hecha).</summary>
public class DisenosEstructuraFormulaResultado
{
    public bool ES_VALIDA { get; set; }
    public decimal? CAN_TOTAL { get; set; }
    public string? DES_ERROR { get; set; }
}

public class DisenosEstructuraFormulaFicha
{
    public int NUM_SECUENCIA_FORMULA { get; set; }
    public string? COD_COMPONENTE_FORMULACION { get; set; }
    public string? DES_COMPONENTE_FORMULACION { get; set; }
    public string? DES_BACKCOLOR { get; set; }
    public string? DES_FORECOLOR { get; set; }
    public string? COD_ELEMENTO_FORMULACION { get; set; }
    public string? COD_ALIAS_FORMULACION { get; set; }
    public string? DES_ELEMENTO_FORMULA { get; set; }
    public string? DES_ELEMENTO_MASCARA_FORMULA { get; set; }
    public int? NUM_ORDEN_PRESENTACION { get; set; }
}

/// <summary>La formula asignada a una linea: textos, ultimo resultado, los datos que usa el calculo y sus fichas en orden.</summary>
public class DisenosEstructuraFormulaLinea
{
    public string? DES_FORMULA { get; set; }
    public string? DES_MASCARA_FORMULA { get; set; }
    public decimal? CAN_TOTAL { get; set; }
    public decimal? CAN_CONSUMO { get; set; }
    public decimal? CAN_CONSUMO_SECUNDARIO { get; set; }
    public decimal? TAS_MERMA { get; set; }
    public decimal? CAN_MERMA { get; set; }
    public int CAN_FICHAS { get; set; }
    public bool? FLG_PRODUCTO_COMPONENTE_PIVOT { get; set; }
    public string? COD_TIPO_MATERIAL { get; set; }
    public int? NUM_FORMULACION_ORIGEN { get; set; }
    public string? DES_FORMULACION_ORIGEN { get; set; }
    public List<DisenosEstructuraFormulaFicha> Fichas { get; set; } = new();
}

/// <summary>Una fila por linea con formula al calcular toda la version.</summary>
public class DisenosEstructuraFormulaVersionFila
{
    public string COD_SECTOR_PRODUCTO { get; set; } = string.Empty;
    public string COD_COMPONENTE_PRODUCTO { get; set; } = string.Empty;
    public int NUM_SECUENCIA { get; set; }
    public string COD_PRODUCTO_COMPONENTE { get; set; } = string.Empty;
    public bool ES_VALIDA { get; set; }
    public decimal? CAN_TOTAL { get; set; }
    public string? DES_ERROR { get; set; }
}

/// <summary>Una ficha de la formula de una linea (para dibujarla en el diagrama); trae tambien la clave de la linea y el total.</summary>
public class DisenosEstructuraFormulaDiagrama
{
    public string COD_DISENO { get; set; } = string.Empty;
    public int NUM_VERSION { get; set; }
    public string COD_SECTOR_PRODUCTO { get; set; } = string.Empty;
    public string COD_COMPONENTE_PRODUCTO { get; set; } = string.Empty;
    public int NUM_SECUENCIA { get; set; }
    public string COD_PRODUCTO_COMPONENTE { get; set; } = string.Empty;
    public string? DES_MASCARA_FORMULA { get; set; }
    public decimal? CAN_TOTAL { get; set; }
    public int? NUM_SECUENCIA_FORMULA { get; set; }
    public string? DES_COMPONENTE_FORMULACION { get; set; }
    public string? DES_ELEMENTO_MASCARA_FORMULA { get; set; }
    public string? DES_BACKCOLOR { get; set; }
    public string? DES_FORECOLOR { get; set; }
}

/// <summary>Editar la copia de la formula de la linea: las fichas completas, en orden (las constantes y las especificaciones llevan su valor en DES_FORMULA).</summary>
public class DisenosEstructuraFormulaGuardarDto : DisenosEstructuraLineaClaveDto
{
    public List<FormulaFichaDto> Fichas { get; set; } = new();
}

/// <summary>Indicadores de un arbol de estructura. FILA = L: una linea (insumo) con su pivote y formula; FILA = V: una version con su tizado (el ultimo guardado).</summary>
public class DisenosEstructuraIndicador
{
    public string FILA { get; set; } = string.Empty;
    public string COD_DISENO { get; set; } = string.Empty;
    public int NUM_VERSION { get; set; }
    public string? COD_SECTOR_PRODUCTO { get; set; }
    public string? COD_COMPONENTE_PRODUCTO { get; set; }
    public int? NUM_SECUENCIA { get; set; }
    public string? COD_PRODUCTO_COMPONENTE { get; set; }
    public bool FLG_PIVOT { get; set; }
    public bool FLG_FORMULA { get; set; }
    public string? COD_TIPO_MATERIAL { get; set; }
    public string? DES_TIPO_MATERIAL { get; set; }
    public string? DES_ABREVIATURA_MATERIAL { get; set; }
    public string? DES_MASCARA_FORMULA { get; set; }
    public decimal? CAN_TOTAL { get; set; }
    public int? NUM_TIZADOS { get; set; }
    public string? DES_TIZADO { get; set; }
    public decimal? VAL_ANCHO_TELA { get; set; }
    public decimal? VAL_EFICIENCIA { get; set; }
    public decimal? VAL_CONSUMO_PRENDA { get; set; }
    public int? NUM_PRENDAS { get; set; }
    public string? DES_TALLAS { get; set; }
    public DateTime? FEC_TIZADO { get; set; }
}

/// <summary>Un tipo de material de la estructura (catalogo 01 a 05): para el combo del insumo y las chapitas del arbol.</summary>
public class DisenosEstructuraTipoMaterial
{
    public string Id { get; set; } = string.Empty;
    public string Texto { get; set; } = string.Empty;
    public string? DES_ABREVIATURA { get; set; }
    public string? DES_EJEMPLOS { get; set; }
}
