namespace KONSolutions.Shared.Models.Proyectos;

/// <summary>PROYECTOS.PROYECTOS (cabecera). Entrega de alcance "solo cabecera": los 14
/// detalles del módulo quedan fuera y se agregan a pedido.</summary>
public class Proyectos
{
    public int? NUM_PROYECTO { get; set; }
    public string DES_PROYECTO { get; set; } = string.Empty;
    public DateTime? FEC_INICIO_PROGRAMADO { get; set; }
    public DateTime? FEC_FINAL_PROGRAMADO { get; set; }
    public DateTime? FEC_INICIO_EJECUTADO { get; set; }
    public DateTime? FEC_FINAL_EJECUTADO { get; set; }
    public decimal? COD_ENTIDAD_RESPONSABLE { get; set; }
    public decimal? COD_ENTIDAD_ESPONSOR { get; set; }
    public string COD_TIPO_PROYECTO { get; set; } = string.Empty;
    public string COD_MONEDA_VALORIZACION { get; set; } = string.Empty;
    public decimal? VAL_PRESUPUESTO_PROYECTO { get; set; }
    public decimal? VAL_COSTO_PROYECTO { get; set; }
    public string DES_OBSERVACION { get; set; } = string.Empty;
    public int? COD_TIPO_ESTADO { get; set; }
    public int? COD_ESTADO { get; set; }
    public string COD_USUARIO_REGISTRO { get; set; } = string.Empty;
    public string COD_ESTACION_REGISTRO { get; set; } = string.Empty;
    public DateTime? FEC_REGISTRO { get; set; }
    public string COD_USUARIO_ACTUALIZACION { get; set; } = string.Empty;
    public string COD_ESTACION_ACTUALIZACION { get; set; } = string.Empty;
    public DateTime? FEC_ACTUALIZACION { get; set; }

    // Vienen resueltas por los JOIN del SP: solo se muestran. El SP solo hace JOIN a
    // ENTIDADES para el Responsable, no para el Sponsor.
    public string DES_COMERCIAL { get; set; } = string.Empty;
    public string DES_MATERNO { get; set; } = string.Empty;
    public string DES_NOMBRE { get; set; } = string.Empty;
    public string DES_NOMBRE_COMPLETO { get; set; } = string.Empty;
    public string DES_NOMBRE2 { get; set; } = string.Empty;
    public string DES_PATERNO { get; set; } = string.Empty;
    public string DES_PREFIJO_USUARIO_COMERCIAL { get; set; } = string.Empty;
    public string DES_MONEDA { get; set; } = string.Empty;
    public string DES_TIPO_PROYECTO { get; set; } = string.Empty;
    /// <summary>Nombre de la entidad sponsor (la trae el procedimiento desde el script 68).</summary>
    public string DES_ESPONSOR { get; set; } = string.Empty;
    public string? DES_CARGO_RESPONSABLE { get; set; }
    public string? DES_AREA_RESPONSABLE { get; set; }
    public string? DES_CARGO_ESPONSOR { get; set; }
    public string? DES_AREA_ESPONSOR { get; set; }
    public string DES_BACKCOLOR { get; set; } = string.Empty;
    public string DES_ESTADO { get; set; } = string.Empty;
    public string DES_FORECOLOR { get; set; } = string.Empty;
}
