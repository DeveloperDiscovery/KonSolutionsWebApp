namespace KONSolutions.Shared.Models.Estandar;

/// <summary>ESTANDAR.OPCIONES_PROCESOS_ESTADOS. Clave compuesta COD_APLICACION +
/// COD_OPCION_APLICACION + COD_TIPO_ESTADO — asigna qué Tipos de Estado puede usar cada
/// Aplicación+Opción del sistema. Tabla de asignación pura: sin UPDATE (el INSERT hace upsert).</summary>
public class OpcionesProcesosEstados
{
    public string COD_APLICACION { get; set; } = string.Empty;
    public string COD_OPCION_APLICACION { get; set; } = string.Empty;
    public short COD_TIPO_ESTADO { get; set; }
    public string? COD_USUARIO_REGISTRO { get; set; }
    public string? COD_ESTACION_REGISTRO { get; set; }
    public DateTime? FEC_REGISTRO { get; set; }

    // ── Solo lectura ──
    public string? DES_TIPO_ESTADO { get; set; }
    public string? DES_BACKCOLOR { get; set; }
    public string? DES_FORECOLOR { get; set; }
    public string? DES_FORMULARIO { get; set; }
    public string? DES_OBJETO { get; set; }
    public string? DES_OPCION_APLICACION { get; set; }
    public string? DES_PARAMETRO { get; set; }
}
