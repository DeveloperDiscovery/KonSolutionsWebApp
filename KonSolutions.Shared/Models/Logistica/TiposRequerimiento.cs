using System.ComponentModel.DataAnnotations;
namespace KONSolutions.Shared.Models.Logistica;

/// <summary>LOGISTICA.TIPOS_REQUERIMIENTO</summary>
public class TiposRequerimiento
{
    public string COD_TIPO_REQUERIMIENTO { get; set; } = string.Empty;
    public string DES_TIPO_REQUERIMIENTO { get; set; } = string.Empty;
    public string? COD_TIPO_RESERVA { get; set; }
    public int? COD_TIPO_ESTADO { get; set; }
    public bool? FLG_TRANSFERENCIA { get; set; }
    public bool? FLG_PRODUCCION { get; set; }
    public bool? FLG_COMPRA { get; set; }
    public bool? FLG_CONSUMO_INTERNO { get; set; }
    public bool? FLG_REPOSICION { get; set; }
    public bool? FLG_VENTA { get; set; }

    // Independiente de las seis de arriba: aquellas son excluyentes entre sí, ésta no.
    public bool? FLG_CONSUMO_PRODUCCION { get; set; }

    // ── Solo lectura: traído por SEARCH/SHOWALL/SHOWBYID/SHOWCOMBOX vía JOIN ──
    public string DES_TIPO_RESERVA { get; set; } = string.Empty;
    public string DES_TIPO_ESTADO { get; set; } = string.Empty;
}
