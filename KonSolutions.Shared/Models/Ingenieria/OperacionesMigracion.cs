namespace KONSolutions.Shared.Models.Ingenieria;

// ===== O8: migracion asistida de las operaciones de las rutas existentes al catalogo (script SQL 128) =====

public class MigracionResumen
{
    public int CAN_PENDIENTES { get; set; }
    public int CAN_BLOQUEADAS { get; set; }
    public int CAN_RUTAS_PENDIENTES { get; set; }
    public int CAN_MIGRADAS { get; set; }
    public int CAN_OPERACIONES_CREADAS { get; set; }
}

/// <summary>Un grupo (proceso, subproceso, tipo de operacion) de filas sin operacion de catalogo, con lo que se propone.</summary>
public class MigracionGrupo
{
    public string COD_PROCESO { get; set; } = "";
    public string? DES_PROCESO { get; set; }
    public string? COD_SUB_PROCESO { get; set; }
    public string? DES_SUB_PROCESO { get; set; }
    public string COD_TIPO_OPERACION_PRODUCCION { get; set; } = "";
    public string? DES_TIPO_OPERACION_PRODUCCION { get; set; }
    public int CAN_FILAS { get; set; }
    public int CAN_FILAS_BLOQUEADAS { get; set; }
    public int CAN_RUTAS { get; set; }
    public string? COD_UNIDAD_MEDIDA_TIEMPO { get; set; }
    public decimal? VAL_LEAD_TIME_MIN { get; set; }
    public decimal? VAL_LEAD_TIME_MAX { get; set; }
    public int CAN_COINCIDENCIAS { get; set; }
    public string? COD_OPERACION_SUGERIDA { get; set; }
    public string? DES_OPERACION_SUGERIDA { get; set; }
    public string? DES_OPERACION_NUEVA { get; set; }
}

/// <summary>La decision sobre un grupo: usar una operacion que ya existe (EXISTENTE) o crear una nueva (NUEVA).</summary>
public class MigracionGrupoDecision
{
    public string COD_PROCESO { get; set; } = "";
    public string? COD_SUB_PROCESO { get; set; }
    public string COD_TIPO_OPERACION_PRODUCCION { get; set; } = "";
    public string ACCION { get; set; } = "NUEVA";
    public string? COD_OPERACION { get; set; }
    public string? DES_OPERACION { get; set; }
}

public class MigracionAplicarDto
{
    /// <summary>Limita la migracion a una ruta (vacio = todas).</summary>
    public string? COD_RUTA_PRODUCCION { get; set; }
    public List<MigracionGrupoDecision> Grupos { get; set; } = new();
}

public class MigracionResultado
{
    public int CAN_FILAS_MIGRADAS { get; set; }
    public int CAN_FILAS_BLOQUEADAS { get; set; }
    public int CAN_OPERACIONES_CREADAS { get; set; }
}

public class MigracionDeshacerResultado
{
    public int CAN_FILAS_DESHECHAS { get; set; }
    public int CAN_FILAS_OMITIDAS { get; set; }
    public int CAN_OPERACIONES_BORRADAS { get; set; }
}
