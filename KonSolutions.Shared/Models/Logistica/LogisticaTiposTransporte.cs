namespace KONSolutions.Shared.Models.Logistica;

/// <summary>LOGISTICA.TIPOS_TRANSPORTE</summary>
public class LogisticaTiposTransporte
{
    public string COD_TIPO_TRANSPORTE { get; set; } = string.Empty;
    public string? DES_TIPO_TRANSPORTE { get; set; }
    public string? COD_GRUPO_PRODUCTO { get; set; }
    public string? COD_FAMILIA_PRODUCTO { get; set; }
    public string? COD_SUB_FAMILIA_PRODUCTO { get; set; }
    public bool? FLG_TERRESTRE { get; set; }
    public bool? FLG_MARITIMO { get; set; }
    public bool? FLG_FLUVIAL { get; set; }
    public bool? FLG_AEREO { get; set; }

    // ── Solo lectura: traído por SEARCH/SHOWALL/SHOWBYID/SHOWCOMBOX vía JOIN ──
    public string? DES_GRUPO_PRODUCTO { get; set; }
    public string? DES_FAMILIA_PRODUCTO { get; set; }
    public string? DES_SUB_FAMILIA_PRODUCTO { get; set; }
}
