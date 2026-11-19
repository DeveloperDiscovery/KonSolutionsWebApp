using System.ComponentModel.DataAnnotations;
namespace KONSolutions.Shared.Models.Contabilidad;

/// <summary>CONTABILIDAD.ASIENTOS_CONTABLES — catálogo de asientos/libros contables.</summary>
public class AsientoContable
{
    // Auditoria (la muestra PanelAuditoria en el dialogo)
    public string? COD_USUARIO_REGISTRO { get; set; }
    public string? COD_ESTACION_REGISTRO { get; set; }
    public DateTime? FEC_REGISTRO { get; set; }
    public string? COD_USUARIO_ACTUALIZACION { get; set; }
    public string? COD_ESTACION_ACTUALIZACION { get; set; }
    public DateTime? FEC_ACTUALIZACION { get; set; }
    public string COD_ASIENTO_CONTABLE { get; set; } = "";
    [Required] public string DES_ASIENTO_CONTABLE { get; set; } = "";
    public bool? FLG_ASIENTO_APERTURA { get; set; }
    public bool? FLG_ASIENTO_CIERRE { get; set; }
    [Required] public string COD_LIBRO_CONTABLE_SUNAT { get; set; } = "";
    public bool? FLG_APERTURA { get; set; }
    public bool? FLG_MENSUAL { get; set; }
    public bool? FLG_CIERRE { get; set; }
    public int? COD_TIPO_ESTADO { get; set; }
    public int? COD_ESTADO { get; set; }

    // Solo lectura (JOIN a ESTANDAR.ESTADOS).
    public string? DES_ESTADO { get; set; }
    public string? DES_BACKCOLOR { get; set; }
    public string? DES_FORECOLOR { get; set; }
}
