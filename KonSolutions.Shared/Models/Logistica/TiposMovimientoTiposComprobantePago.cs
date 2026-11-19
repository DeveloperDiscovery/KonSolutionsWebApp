using System.ComponentModel.DataAnnotations;

namespace KONSolutions.Shared.Models.Logistica;

/// <summary>LOGISTICA.TIPOS_MOVIMIENTO_TIPOS_COMPROBATE_PAGO — los tipos de comprobante de
/// pago que acepta un tipo de movimiento.
///
/// Espejo exacto del modelo del API: un campo que exista de un solo lado se pierde en
/// silencio al serializar.</summary>
public class TiposMovimientoTiposComprobantePago
{
    public string COD_CLASE_MOVIMIENTO { get; set; } = string.Empty;
    public string COD_TIPO_MOVIMIENTO { get; set; } = string.Empty;
    [Required] public string COD_TIPO_COMPROBANTE_PAGO { get; set; } = string.Empty;
    public string? COD_USUARIO_REGISTRO { get; set; }
    public string? COD_ESTACION_REGISTRO { get; set; }
    public DateTime? FEC_REGISTRO { get; set; }
    public string? COD_USUARIO_ACTUALIZACION { get; set; }
    public string? COD_ESTACION_ACTUALIZACION { get; set; }
    public DateTime? FEC_ACTUALIZACION { get; set; }
    public int FLG_CONTROL_CORRELATIVO { get; set; }
    public string? DES_OBSERVACION { get; set; }
    public string? DES_TIPO_MOVIMIENTO { get; set; }
    public string? DES_TIPO_COMPROBANTE_PAGO { get; set; }

    /// <summary>El formato del número, traído del tipo de comprobante por el JOIN del SP. Va
    /// acá para que la pantalla que registra un documento no tenga que ir a buscarlo a
    /// ESTANDAR.TIPOS_COMPROBANTES_PAGO, que es otro endpoint con otro permiso.</summary>
    public string? COD_PREFIJO { get; set; }
    public string? COD_PREFIJO_SUNAT { get; set; }
    public int? VAL_LONGITUD_SERIE { get; set; }
    public int? VAL_LONGITUD_NUMERO { get; set; }
}
