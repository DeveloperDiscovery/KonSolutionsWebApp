namespace KONSolutions.Web.Pages.Proyectos;

/// <summary>Los campos que se pueden poner en el texto de un documento de proyecto: se llenan con los datos del proyecto al imprimir (script 81).</summary>
public static class CamposDocumento
{
    public static readonly (string Campo, string Texto)[] Todos =
    {
        ("DES_PROYECTO", "Proyecto"), ("NUM_PROYECTO", "N° de proyecto"), ("DES_TIPO_PROYECTO", "Tipo de proyecto"), ("DES_EMPRESA", "Empresa"),
        ("DES_RESPONSABLE", "Responsable"), ("DES_CARGO_RESPONSABLE", "Cargo del responsable"), ("DES_ESPONSOR", "Sponsor / cliente"), ("DES_CARGO_ESPONSOR", "Cargo del sponsor"),
        ("FEC_INICIO_PROGRAMADO", "Inicio programado"), ("FEC_FINAL_PROGRAMADO", "Final programado"), ("FEC_INICIO_EJECUTADO", "Inicio ejecutado"), ("FEC_FINAL_EJECUTADO", "Final ejecutado"),
        ("NUM_DIAS_PROGRAMADOS", "Días programados"), ("NUM_DIAS_EJECUTADOS", "Días ejecutados"), ("DES_MONEDA", "Moneda"), ("VAL_PRESUPUESTO_PROYECTO", "Presupuesto"),
        ("VAL_COSTO_PROYECTO", "Costo real"), ("VAL_VARIACION", "Variación"), ("TAS_AVANCE_PROYECTO", "% de avance"), ("DES_ESTADO_PROYECTO", "Estado del proyecto"),
        ("DES_OBSERVACION", "Observación del proyecto"), ("FEC_EMISION", "Fecha de emisión")
    };
}
