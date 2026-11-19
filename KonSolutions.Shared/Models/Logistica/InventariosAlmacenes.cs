namespace KONSolutions.Shared.Models.Logistica;

/// <summary>LOGISTICA.INVENTARIOS_ALMACENES. Clave compuesta NUM_INVENTARIO + COD_ALMACEN,
/// sin autogenerado — COD_ALMACEN lo elige el usuario por combo.</summary>
public class InventariosAlmacenes
{
    // Auditoria (la muestra PanelAuditoria en el dialogo)
    public string? COD_USUARIO_REGISTRO { get; set; }
    public string? COD_ESTACION_REGISTRO { get; set; }
    public DateTime? FEC_REGISTRO { get; set; }
    public string? COD_USUARIO_ACTUALIZACION { get; set; }
    public string? COD_ESTACION_ACTUALIZACION { get; set; }
    public DateTime? FEC_ACTUALIZACION { get; set; }
    public int? NUM_INVENTARIO { get; set; }
    public string COD_ALMACEN { get; set; } = string.Empty;
    /// <summary>Del almacén (join e, LOGISTICA.ALMACENES) — SEARCH/SHOWALL/SHOWBYID/SHOWCOMBOX.</summary>
    public string? DES_ALMACEN { get; set; }
    public bool? FLG_TOTAL { get; set; }
    public bool? FLG_INMOVILIZACION { get; set; }
    public decimal? COD_ENTIDAD_RESPONSABLE { get; set; }
    public DateTime? FEC_INICIO_PROGRAMADA { get; set; }
    public DateTime? FEC_FINAL_PROGRAMADA { get; set; }
    public DateTime? FEC_INICIO_EFECTUADA { get; set; }
    public DateTime? FEC_FINAL_EFECTUADA { get; set; }
    public DateTime? FEC_CORTE { get; set; }
    public string? DES_OBSERVACION { get; set; }
    public int? COD_TIPO_ESTADO { get; set; }
    public int? COD_ESTADO { get; set; }

    // ── Solo lectura: traído por SEARCH/SHOWALL/SHOWBYID/SHOWCOMBOX vía JOIN ──
    public string? DES_NOMBRE_COMPLETO { get; set; }
    public string? DES_ESTADO { get; set; }
    public string? DES_BACKCOLOR { get; set; }
    public string? DES_FORECOLOR { get; set; }
    public string? DES_INVENTARIO { get; set; }
}
