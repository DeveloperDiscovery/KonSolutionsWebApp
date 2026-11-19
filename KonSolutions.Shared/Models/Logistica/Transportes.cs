namespace KONSolutions.Shared.Models.Logistica;

/// <summary>LOGISTICA.TRANSPORTES (cabecera)</summary>
public class Transportes
{
    public string COD_TRANSPORTE { get; set; } = string.Empty;
    public string? DES_TRANSPORTE { get; set; }
    public bool? FLG_PROPIO { get; set; }
    public bool? FLG_ACARREO { get; set; }
    public string? COD_PRODUCTO_TRANSPORTE { get; set; }
    public decimal? COD_ENTIDAD_PROPIETARIO { get; set; }
    public decimal? COD_ENTIDAD_CONDUCTOR { get; set; }
    public string? COD_TIPO_TRANSPORTE { get; set; }
    public int? COD_TIPO_ESTADO { get; set; }
    public int? COD_ESTADO { get; set; }
    public string? COD_USUARIO_REGISTRO { get; set; }
    public string? COD_ESTACION_REGISTRO { get; set; }
    public DateTime? FEC_REGISTRO { get; set; }
    public string? COD_USUARIO_ACTUALIZACION { get; set; }
    public string? COD_ESTACION_ACTUALIZACION { get; set; }
    public DateTime? FEC_ACTUALIZACION { get; set; }

    // ── Solo lectura: traído por SEARCH/SHOWALL/SHOWBYID/SHOWCOMBOX vía JOIN ──
    public string? DES_ESTADO { get; set; }
    public string? DES_BACKCOLOR { get; set; }
    public string? DES_FORECOLOR { get; set; }
    public string? DES_NOMBRE_COMPLETO { get; set; }
    public string? DES_TIPO_TRANSPORTE { get; set; }
    public string? DES_PRODUCTO { get; set; }
}
