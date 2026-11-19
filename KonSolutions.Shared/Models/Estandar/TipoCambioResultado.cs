namespace KONSolutions.Shared.Models.Estandar;

/// <summary>Resultado de PROC_ESTANDAR_MONEDAS_TIPO_CAMBIO_OBTENER: el tipo de cambio a
/// usar para una moneda en una fecha, ya resuelto contra la moneda pivot configurada en
/// ESTANDAR.CONFIGURACION_GENERAL.</summary>
public class TipoCambioResultado
{
    public string COD_MONEDA { get; set; } = "";
    public string? COD_MONEDA_PIVOT_TIPO_CAMBIO { get; set; }
    /// <summary>True cuando COD_MONEDA ES la pivot — ahí FAC_TIPO_CAMBIO es 1 por
    /// definición, no porque se haya encontrado una cotización así.</summary>
    public bool FLG_ES_PIVOT { get; set; }
    /// <summary>Null si la moneda no es la pivot y no hay ninguna cotización cargada en o
    /// antes de la fecha pedida.</summary>
    public decimal? FAC_TIPO_CAMBIO { get; set; }
    public DateTime FEC_TIPO_CAMBIO_SOLICITADA { get; set; }
    /// <summary>La fecha de la que realmente sale FAC_TIPO_CAMBIO. Puede ser anterior a la
    /// solicitada (el SP toma la última cotización publicada) o null si no hay ninguna.</summary>
    public DateTime? FEC_TIPO_CAMBIO_VIGENTE { get; set; }
}
