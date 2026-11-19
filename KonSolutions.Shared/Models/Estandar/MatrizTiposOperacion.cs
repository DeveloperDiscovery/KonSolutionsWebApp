namespace KONSolutions.Shared.Models.Estandar;

/// <summary>Una celda marcada de la matriz de valores de calculo: el tipo de operacion tiene ese valor.</summary>
public class MatrizCalculoCelda
{
    public string COD_TIPO_OPERACION { get; set; } = "";
    public string COD_TIPO_VALOR_CALCULO { get; set; } = "";
    public int? NUM_ORDEN_PRESENTACION { get; set; }
    /// <summary>Valores que este valor suma (TIPOS_OPERACION_CALCULO_AFECTADOS como dueno).</summary>
    public int N_AFECTADOS { get; set; }
    /// <summary>Valores del mismo tipo de operacion que suman a este.</summary>
    public int N_REFERENCIADO { get; set; }
    public bool? FLG_CALCULO_INVERSO { get; set; }
    public bool? FLG_MOSTRAR_COMPROBANTE { get; set; }
    public bool? FLG_VALOR_MOSTRAR { get; set; }
    public bool? FLG_POSITIVO { get; set; }
}

public class MatrizComprobanteCelda
{
    public string COD_TIPO_OPERACION { get; set; } = "";
    public string COD_TIPO_COMPROBANTE_PAGO { get; set; } = "";
}

public class MatrizCalculoSetDto
{
    public string COD_CLASE_OPERACION { get; set; } = "";
    public string COD_TIPO_OPERACION { get; set; } = "";
    public string COD_TIPO_VALOR_CALCULO { get; set; } = "";
    /// <summary>true: marca (agrega); false: desmarca (elimina).</summary>
    public bool ACTIVO { get; set; }
    /// <summary>Con false, quitar un valor con afectados devuelve ACCION = CONFIRMAR sin borrar; con true los borra.</summary>
    public bool FORZAR { get; set; }
}

public class MatrizComprobanteSetDto
{
    public string COD_CLASE_OPERACION { get; set; } = "";
    public string COD_TIPO_OPERACION { get; set; } = "";
    public string COD_TIPO_COMPROBANTE_PAGO { get; set; } = "";
    public bool ACTIVO { get; set; }
}

/// <summary>ACCION: AGREGADO, ELIMINADO, SIN_CAMBIO o CONFIRMAR (solo valores de calculo con afectados).</summary>
public class MatrizSetResultado
{
    public string ACCION { get; set; } = "";
    public int N_AFECTADOS { get; set; }
    public int N_REFERENCIADO { get; set; }
    public string? MENSAJE { get; set; }
}

/// <summary>Un valor que otro suma o resta dentro de un tipo de operacion (TIPOS_OPERACION_CALCULO_AFECTADOS).</summary>
public class FormulaCelda
{
    public string COD_TIPO_OPERACION { get; set; } = "";
    public string COD_TIPO_VALOR_CALCULO { get; set; } = "";
    public string COD_TIPO_VALOR_CALCULO_AFECTADO { get; set; } = "";
}

/// <summary>Reemplaza la formula (los valores que suma) de un valor de calculo en un tipo de operacion y, si se pide, su orden y flags.</summary>
public class FormulaSetDto
{
    public string COD_CLASE_OPERACION { get; set; } = "";
    public string COD_TIPO_OPERACION { get; set; } = "";
    public string COD_TIPO_VALOR_CALCULO { get; set; } = "";
    public List<string> AFECTADOS { get; set; } = new();
    /// <summary>true: tambien se aplican NUM_ORDEN_PRESENTACION y los cuatro flags (un flag en null es un valor valido).</summary>
    public bool ACTUALIZAR_PROPIEDADES { get; set; }
    public int? NUM_ORDEN_PRESENTACION { get; set; }
    public bool? FLG_CALCULO_INVERSO { get; set; }
    public bool? FLG_MOSTRAR_COMPROBANTE { get; set; }
    public bool? FLG_VALOR_MOSTRAR { get; set; }
    public bool? FLG_POSITIVO { get; set; }
}

public class FormulaSetResultado
{
    public string ACCION { get; set; } = "";
    public int N_AGREGADOS { get; set; }
    public int N_QUITADOS { get; set; }
    /// <summary>Cuantos afectados se calculan despues (o a la vez) que el valor: valen 0 cuando el valor se calcula. Aviso, no error.</summary>
    public int N_ORDEN_POSTERIOR { get; set; }
}
