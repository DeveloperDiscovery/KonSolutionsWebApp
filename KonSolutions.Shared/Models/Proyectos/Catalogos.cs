namespace KONSolutions.Shared.Models.Proyectos;

public class ProyectosRol
{
    public string COD_ROL_TAREA { get; set; } = string.Empty;
    public string DES_ROL_TAREA { get; set; } = string.Empty;
    public string? COD_CATEGORIA_ROL { get; set; }
    public string? DES_CATEGORIA_ROL { get; set; }
    public string? COD_ROL_PADRE { get; set; }
    public string? DES_ROL_PADRE { get; set; }
}

public class ProyectosRolCategoria
{
    // Auditoria (la muestra PanelAuditoria en el dialogo)
    public string? COD_USUARIO_REGISTRO { get; set; }
    public string? COD_ESTACION_REGISTRO { get; set; }
    public DateTime? FEC_REGISTRO { get; set; }
    public string? COD_USUARIO_ACTUALIZACION { get; set; }
    public string? COD_ESTACION_ACTUALIZACION { get; set; }
    public DateTime? FEC_ACTUALIZACION { get; set; }
    public string COD_CATEGORIA_ROL { get; set; } = string.Empty;
    public string DES_CATEGORIA_ROL { get; set; } = string.Empty;
    public string? DES_DESCRIPCION { get; set; }
    public int? NUM_ORDEN_PRESENTACION { get; set; }
    public int NUM_ROLES { get; set; }
}

public class TipoDocumentoCompromiso
{
    // Auditoria (la muestra PanelAuditoria en el dialogo)
    public string? COD_USUARIO_REGISTRO { get; set; }
    public string? COD_ESTACION_REGISTRO { get; set; }
    public DateTime? FEC_REGISTRO { get; set; }
    public string? COD_USUARIO_ACTUALIZACION { get; set; }
    public string? COD_ESTACION_ACTUALIZACION { get; set; }
    public DateTime? FEC_ACTUALIZACION { get; set; }
    public string COD_TIPO_DOCUMENTO_COMPROMISO { get; set; } = string.Empty;
    public string DES_TIPO_DOCUMENTO_COMPROMISO { get; set; } = string.Empty;
    /// <summary>C = compromiso, A = aceptación.</summary>
    public string COD_CLASE_COMPROMISO { get; set; } = "C";
    public string? DES_CLASE_COMPROMISO { get; set; }
    public string? DES_DESCRIPCION { get; set; }
    public int? NUM_ORDEN_PRESENTACION { get; set; }
    public int NUM_DOCUMENTOS { get; set; }
}

public class ProyectosUnidadMedidaTarea
{
    public string COD_UNIDAD_MEDIDA_TAREAS { get; set; } = string.Empty;
    public string DES_UNIDAD_MEDIDA_TAREAS { get; set; } = string.Empty;
    public byte[]? IMG_ICONO { get; set; }
}

public class ProyectosProbabilidadRiesgo
{
    public int? COD_PROBABILIDAD_RIESGO { get; set; }
    public string DES_PROBABILIDAD_RIESGO { get; set; } = string.Empty;
    public decimal? VAL_PROBABILIDAD { get; set; }
}

public class ProyectosImpactoRiesgo
{
    public int? COD_IMPACTO_RIESGO { get; set; }
    public string DES_IMPACTO_RIESGO { get; set; } = string.Empty;
    /// <summary>Valor de impacto (0 a 1). La columna se llama VAL_PROBABILIDAD en la base.</summary>
    public decimal? VAL_PROBABILIDAD { get; set; }
}

public class Feriado
{
    // Auditoria (la muestra PanelAuditoria en el dialogo)
    public string? COD_USUARIO_REGISTRO { get; set; }
    public string? COD_ESTACION_REGISTRO { get; set; }
    public DateTime? FEC_REGISTRO { get; set; }
    public string? COD_USUARIO_ACTUALIZACION { get; set; }
    public string? COD_ESTACION_ACTUALIZACION { get; set; }
    public DateTime? FEC_ACTUALIZACION { get; set; }
    public DateTime FEC_FERIADO { get; set; }
    public string DES_FERIADO { get; set; } = string.Empty;
    /// <summary>1 = se repite todos los años (mismo día y mes).</summary>
    public bool FLG_ANUAL { get; set; }
}

public class ProyectosParametrosModelo
{
    public string? COD_KEY_PATHFILE_PROYECTOS { get; set; }
    public string? DES_PATH_FILE { get; set; }
    public string? COD_TIPO_VALOR_CALCULO_COSTO { get; set; }
}
