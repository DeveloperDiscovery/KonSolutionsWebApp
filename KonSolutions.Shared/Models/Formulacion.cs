namespace KONSolutions.Shared.Models.Formulacion;

// Constructor de formulas (script SQL 145; API formulacion/componentes, formulacion/elementos y formulacion/formulas). Espejo de los modelos de la API.

/// <summary>Los 6 campos de auditoria que devuelven los SP: el panel de auditoria de los dialogos los lee.</summary>
public abstract class FormulacionAuditable
{
    public string? COD_USUARIO_REGISTRO { get; set; }
    public string? COD_ESTACION_REGISTRO { get; set; }
    public DateTime? FEC_REGISTRO { get; set; }
    public string? COD_USUARIO_ACTUALIZACION { get; set; }
    public string? COD_ESTACION_ACTUALIZACION { get; set; }
    public DateTime? FEC_ACTUALIZACION { get; set; }
}

/// <summary>Categoria de la paleta (CONSM, CONST, OPEMT...): su color y sus banderas.</summary>
public class FormulacionComponente : FormulacionAuditable
{
    public string COD_COMPONENTE_FORMULACION { get; set; } = "";
    public string? DES_COMPONENTE_FORMULACION { get; set; }
    public bool? FLG_ELEMENTOS { get; set; }
    public bool? FLG_HABILITADO { get; set; }
    public bool? FLG_ESPECIFICACIONES_CONSUMO { get; set; }
    public bool? FLG_ESPECIFICACIONES { get; set; }
    public bool? FLG_CONSUMO { get; set; }
    public bool? FLG_CONSUMO_SUPERIOR { get; set; }
    public bool? FLG_CONSUMO_INFERIOR { get; set; }
    public bool? FLG_ESPECIFICACIONES_PIVOT { get; set; }
    public string? DES_NAME_TABLE { get; set; }
    public string? DES_BACKCOLOR { get; set; }
    public string? DES_FORECOLOR { get; set; }
    public byte[]? IMG_PICTURE { get; set; }
    public int CAN_ELEMENTOS { get; set; }
}

public class FormulacionComponenteGuardarDto
{
    public string COD_COMPONENTE_FORMULACION { get; set; } = "";
    public string DES_COMPONENTE_FORMULACION { get; set; } = "";
    public bool? FLG_ELEMENTOS { get; set; } = true;
    public bool? FLG_HABILITADO { get; set; } = true;
    public bool? FLG_ESPECIFICACIONES_CONSUMO { get; set; }
    public bool? FLG_ESPECIFICACIONES { get; set; }
    public bool? FLG_CONSUMO { get; set; }
    public bool? FLG_CONSUMO_SUPERIOR { get; set; }
    public bool? FLG_CONSUMO_INFERIOR { get; set; }
    public bool? FLG_ESPECIFICACIONES_PIVOT { get; set; }
    public string? DES_NAME_TABLE { get; set; }
    public string? DES_BACKCOLOR { get; set; }
    public string? DES_FORECOLOR { get; set; }
    public byte[]? IMG_PICTURE { get; set; }
}

/// <summary>Ficha de un componente. COD_SINTAXIS es su rol: VAL, OPE, ABR, CIE, CAS, FIN, THE, ELS, SEP o NOT.</summary>
public class FormulacionElemento : FormulacionAuditable
{
    public string COD_COMPONENTE_FORMULACION { get; set; } = "";
    public string? DES_COMPONENTE_FORMULACION { get; set; }
    public string COD_ELEMENTO_FORMULACION { get; set; } = "";
    public string? DES_ELEMENTO_FORMULACION { get; set; }
    public string? COD_ALIAS_FORMULACION { get; set; }
    public string? COD_TIPO_ESPECIFICACION_TECNICA { get; set; }
    public string? DES_TIPO_ESPECIFICACION_TECNICA { get; set; }
    public bool? FLG_NO_ES_RANGO { get; set; }
    public bool? FLG_HABILITADO { get; set; }
    public byte[]? IMG_PICTURE { get; set; }
    public string? DES_BACKCOLOR { get; set; }
    public string? DES_FORECOLOR { get; set; }
    public string? COD_SINTAXIS { get; set; }
    /// <summary>Cuantas fichas de formulas armadas lo usan (solo viene al consultar uno).</summary>
    public int? CAN_USOS { get; set; }
}

public class FormulacionElementoGuardarDto
{
    public string COD_COMPONENTE_FORMULACION { get; set; } = "";
    /// <summary>Al crear puede ir vacio: la base asigna el siguiente codigo del componente.</summary>
    public string? COD_ELEMENTO_FORMULACION { get; set; }
    public string DES_ELEMENTO_FORMULACION { get; set; } = "";
    public string COD_ALIAS_FORMULACION { get; set; } = "";
    public string? COD_TIPO_ESPECIFICACION_TECNICA { get; set; }
    public bool? FLG_NO_ES_RANGO { get; set; }
    public bool? FLG_HABILITADO { get; set; } = true;
    public byte[]? IMG_PICTURE { get; set; }
}

public class FormulaCabecera : FormulacionAuditable
{
    public int NUM_FORMULACION { get; set; }
    public string? DES_FORMULACION { get; set; }
    public string? COD_GRUPO_PRODUCTO_CONSUMO { get; set; }
    public string? COD_FAMILIA_PRODUCTO_CONSUMO { get; set; }
    public string? COD_SUB_FAMILIA_PRODUCTO_CONSUMO { get; set; }
    public string? DES_SUB_FAMILIA_CONSUMO { get; set; }
    public string? COD_GRUPO_PRODUCTO { get; set; }
    public string? COD_FAMILIA_PRODUCTO { get; set; }
    public string? COD_SUB_FAMILIA_PRODUCTO { get; set; }
    public string? DES_SUB_FAMILIA_PRODUCTO { get; set; }
    /// <summary>Texto tecnico, armado por la base.</summary>
    public string? DES_FORMULA { get; set; }
    /// <summary>Texto que se muestra, armado por la base.</summary>
    public string? DES_ALIAS { get; set; }
    public short? COD_TIPO_ESTADO { get; set; }
    public short? COD_ESTADO { get; set; }
    public string? DES_ESTADO { get; set; }
    public string? DES_ESTADO_BACKCOLOR { get; set; }
    public string? DES_ESTADO_FORECOLOR { get; set; }
    public int CAN_FICHAS { get; set; }
}

public class FormulaFicha
{
    public int NUM_FORMULACION { get; set; }
    public short NUM_SECUENCIA { get; set; }
    public string COD_COMPONENTE_FORMULACION { get; set; } = "";
    public string? DES_COMPONENTE_FORMULACION { get; set; }
    public string COD_ELEMENTO_FORMULACION { get; set; } = "";
    public string? DES_ELEMENTO_FORMULACION { get; set; }
    public string? COD_ALIAS_FORMULACION { get; set; }
    /// <summary>Fragmento tecnico (o el valor, si es una constante).</summary>
    public string? DES_FORMULA { get; set; }
    public string? DES_ALIAS { get; set; }
    public int? NUM_ORDEN_PRESENTACION { get; set; }
    public string? DES_BACKCOLOR { get; set; }
    public string? DES_FORECOLOR { get; set; }
    public string? COD_SINTAXIS { get; set; }
    /// <summary>true: la ficha es una especificacion tecnica (su texto va en DES_FORMULA).</summary>
    public bool? FLG_ESPECIFICACION { get; set; }
}

public class FormulaDetalle
{
    public FormulaCabecera Cabecera { get; set; } = new();
    public List<FormulaFicha> Fichas { get; set; } = new();
}

/// <summary>Una ficha al grabar o validar. DES_FORMULA solo se usa en las constantes (CONST): es su valor.</summary>
public class FormulaFichaDto
{
    public string COD_COMPONENTE_FORMULACION { get; set; } = "";
    /// <summary>Puede ir vacio en una especificacion tecnica: la base elige el elemento portador.</summary>
    public string COD_ELEMENTO_FORMULACION { get; set; } = "";
    public string? DES_FORMULA { get; set; }
    /// <summary>Solo en las fichas de especificaciones tecnicas.</summary>
    public string? COD_ESPECIFICACION_TECNICA { get; set; }
}

public class FormulaGuardarDto
{
    /// <summary>Vacio o 0 = formula nueva.</summary>
    public int? NUM_FORMULACION { get; set; }
    public string DES_FORMULACION { get; set; } = "";
    public string? COD_GRUPO_PRODUCTO_CONSUMO { get; set; }
    public string? COD_FAMILIA_PRODUCTO_CONSUMO { get; set; }
    public string? COD_SUB_FAMILIA_PRODUCTO_CONSUMO { get; set; }
    public string? COD_GRUPO_PRODUCTO { get; set; }
    public string? COD_FAMILIA_PRODUCTO { get; set; }
    public string? COD_SUB_FAMILIA_PRODUCTO { get; set; }
    /// <summary>0 BORRADOR, 1 ACTIVA, 2 INACTIVA.</summary>
    public short? COD_ESTADO { get; set; }
    public List<FormulaFichaDto> Fichas { get; set; } = new();
}

public class FormulaValidarDto
{
    public List<FormulaFichaDto> Fichas { get; set; } = new();
}

/// <summary>Un problema de la forma de la formula. NUM_POSICION es la ficha (desde 1); vacio si es de toda la formula.</summary>
public class FormulaError
{
    public int NUM { get; set; }
    public int? NUM_POSICION { get; set; }
    public string DES_ERROR { get; set; } = "";
}

public class FormulaValidacion
{
    public bool ES_VALIDA { get; set; }
    public List<FormulaError> Errores { get; set; } = new();
}

/// <summary>Fila de la paleta: un componente habilitado con una de sus fichas habilitadas (el elemento viene vacio si el componente no tiene ninguna).</summary>
public class PaletaFicha
{
    public string COD_COMPONENTE_FORMULACION { get; set; } = "";
    public string? DES_COMPONENTE_FORMULACION { get; set; }
    public bool? FLG_ESPECIFICACIONES_CONSUMO { get; set; }
    public bool? FLG_ESPECIFICACIONES { get; set; }
    public string? DES_BACKCOLOR { get; set; }
    public string? DES_FORECOLOR { get; set; }
    public byte[]? IMG_COMPONENTE { get; set; }
    public string? COD_ELEMENTO_FORMULACION { get; set; }
    public string? DES_ELEMENTO_FORMULACION { get; set; }
    public string? COD_ALIAS_FORMULACION { get; set; }
    public string? COD_TIPO_ESPECIFICACION_TECNICA { get; set; }
    public bool? FLG_NO_ES_RANGO { get; set; }
    public byte[]? IMG_ELEMENTO { get; set; }
    public string? COD_SINTAXIS { get; set; }
    /// <summary>Solo en los componentes de especificaciones: la especificacion tecnica de la sub familia que representa la ficha.</summary>
    public string? COD_ESPECIFICACION_TECNICA { get; set; }
    /// <summary>Su descripcion en singular (es lo que se muestra y lo que queda en la formula).</summary>
    public string? DES_ESPECIFICACION_TECNICA_SINGULAR { get; set; }
}

/// <summary>Resultado de una escritura: el codigo o numero que genero la base (en Codigo).</summary>
public class FormulacionEscritura
{
    public string Estado { get; set; } = "OK";
    public int AffectedRows { get; set; }
    public string Codigo { get; set; } = "";
}
