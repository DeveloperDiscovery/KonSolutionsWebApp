namespace KONSolutions.Shared.Models.Estandar;

// ───────────── ESTANDAR.PERIODOS ─────────────
// NUM_PERIODO es IDENTITY: lo genera la base al insertar, no lo escribe el usuario.
public class Periodos
{
    // Auditoria (la muestra PanelAuditoria en el dialogo)
    public string? COD_USUARIO_REGISTRO { get; set; }
    public string? COD_ESTACION_REGISTRO { get; set; }
    public DateTime? FEC_REGISTRO { get; set; }
    public string? COD_USUARIO_ACTUALIZACION { get; set; }
    public string? COD_ESTACION_ACTUALIZACION { get; set; }
    public DateTime? FEC_ACTUALIZACION { get; set; }
    public int? NUM_PERIODO { get; set; }
    public string COD_PERIODO { get; set; } = "";
    public string DES_PERIODO { get; set; } = "";
    public string COD_TIPO_PERIODO { get; set; } = "";
    public DateTime? FEC_INICIO { get; set; }
    public DateTime? FEC_FINAL { get; set; }
    public int? NUM_ANO { get; set; }
    public int? NUM_SEMESTRE { get; set; }
    public int? NUM_QUARTER { get; set; }
    public int? NUM_MES { get; set; }
    public int? NUM_SEMANA { get; set; }
    public int? NUM_DIA { get; set; }
    public int? COD_TIPO_ESTADO { get; set; }
    public int? COD_ESTADO { get; set; }
}
