namespace KONSolutions.Shared.Models.Estandar;

// ───────────── ESTANDAR.TIPOS_PERIODO ─────────────
public class TiposPeriodo
{
    public string COD_TIPO_PERIODO { get; set; } = "";
    public string DES_TIPO_PERIODO { get; set; } = "";
    public string DES_PREFIJO { get; set; } = "";
    public int? COD_TIPO_ESTADO { get; set; }
    public bool? FLG_QUINQUENAL { get; set; }
    public bool? FLG_BIANUAL { get; set; }
    public bool? FLG_ANUAL { get; set; }
    public bool? FLG_SEMESTRAL { get; set; }
    public bool? FLG_TRIMESTRAL { get; set; }
    public bool? FLG_MENSUAL { get; set; }
    public bool? FLG_QUINCENAL { get; set; }
    public bool? FLG_SEMANAL { get; set; }
    public bool? FLG_DIARIO { get; set; }
    public bool? FLG_MODULO_CONTABLE { get; set; }
    public bool? FLG_MODULO_COSTOS { get; set; }
    public bool? FLG_MODULO_RRHH_NOMINA { get; set; }
    public bool? FLG_MODULO_COMERCIAL { get; set; }
    public bool? FLG_MODULO_NETWORKING { get; set; }
    public bool? FLG_MODULO_PLANEAMIENTO { get; set; }
    public string DES_TIPO_ESTADO { get; set; } = "";
}
