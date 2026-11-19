namespace KONSolutions.Shared.Models.Estandar;

/// <summary>ESTANDAR.OPCIONES_PROCESOS_ESTADOS_FLUJOS_DEPENDENCIA — los estados previos
/// requeridos para que un flujo (botón) pueda ejecutarse. Tabla de asignación pura: sin
/// UPDATE (el INSERT hace upsert). Sin filas para un flujo = se ejecuta desde cualquier estado.</summary>
public class OpcionesProcesosEstadosFlujosDependencia
{
    public string COD_APLICACION { get; set; } = string.Empty;
    public string COD_OPCION_APLICACION { get; set; } = string.Empty;
    public short COD_TIPO_ESTADO { get; set; }
    public string COD_BOTON_OPCION { get; set; } = string.Empty;
    public short COD_ESTADO { get; set; }
    public string? COD_USUARIO_REGISTRO { get; set; }
    public string? COD_ESTACION_REGISTRO { get; set; }
    public DateTime? FEC_REGISTRO { get; set; }

    // ── Solo lectura ──
    public string? DES_ESTADO { get; set; }
    public string? DES_BACKCOLOR { get; set; }
    public string? DES_FORECOLOR { get; set; }
}

public class OpcionesProcesosEstadosFlujosDependenciaInsertDto
{
    public string COD_APLICACION { get; set; } = string.Empty;
    public string COD_OPCION_APLICACION { get; set; } = string.Empty;
    public short COD_TIPO_ESTADO { get; set; }
    public string COD_BOTON_OPCION { get; set; } = string.Empty;
    public short COD_ESTADO { get; set; }
}

/// <summary>Resultado de validar si un botón puede ejecutarse desde el estado actual del
/// registro (PROC_ESTANDAR_OPCIONES_PROCESOS_ESTADOS_FLUJOS_DEPENDENCIA_VALIDA). OJO: si el
/// botón no tiene NINGÚN estado previo configurado, el SP devuelve FLG_VALIDO=false igual —
/// llamalo solo para botones que sí tienen dependencias cargadas.</summary>
public class OpcionesProcesosEstadosFlujosDependenciaValidaResult
{
    public bool FLG_VALIDO { get; set; }
    public string? DES_MENSAJE { get; set; }
}
