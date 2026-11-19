namespace KONSolutions.Shared.Models.Logistica;

/// <summary>LOGISTICA.TRANSPORTES_ESPECIFICACIONES_TECNICAS (detalle de TRANSPORTES)</summary>
public class TransportesEspecificacionesTecnicas
{
    public string COD_TRANSPORTE { get; set; } = string.Empty;
    public string COD_ESPECIFICACION_TECNICA { get; set; } = string.Empty;
    public int? COD_ITEM_TECNICO { get; set; }
    public string? VAL_ESPECIFICACION_TENICA { get; set; }
    public decimal? VAL_MINIMO { get; set; }
    public decimal? VAL_MAXIMO { get; set; }
    public int? COD_TIPO_ESTADO { get; set; }
    public int? COD_ESTADO { get; set; }
    public string? COD_USUARIO_REGISTRO { get; set; }
    public string? COD_ESTACION_REGISTRO { get; set; }
    public DateTime? FEC_REGISTRO { get; set; }
    public string? COD_USUARIO_ACTUALIZACION { get; set; }
    public string? COD_ESTACION_ACTUALIZACION { get; set; }
    public DateTime? FEC_ACTUALIZACION { get; set; }

    // ── Solo lectura: traído por SEARCH/SHOWALL/SHOWBYID/SHOWCOMBOX vía JOIN ──
    public string? DES_ITEM_TECNICO { get; set; }
    public string? DES_ITEM_TECNICO_SINGULAR { get; set; }
    public string? DES_BACKCOLOR { get; set; }
    public string? DES_ESTADO { get; set; }
    public string? DES_FORECOLOR { get; set; }
    public string? DES_TRANSPORTE { get; set; }
}
