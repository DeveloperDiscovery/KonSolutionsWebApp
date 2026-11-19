using System.ComponentModel.DataAnnotations;
namespace KONSolutions.Shared.Models.Contabilidad;

/// <summary>CONTABILIDAD.CUENTAS_CONTABLES — plan de cuentas.
/// COD_CUENTA_CONTABLE_PADRE es NULL en las cuentas de nivel 1 (raíz).</summary>
public class CuentaContable
{
    // Auditoria (la muestra PanelAuditoria en el dialogo)
    public string? COD_USUARIO_REGISTRO { get; set; }
    public string? COD_ESTACION_REGISTRO { get; set; }
    public DateTime? FEC_REGISTRO { get; set; }
    public string? COD_USUARIO_ACTUALIZACION { get; set; }
    public string? COD_ESTACION_ACTUALIZACION { get; set; }
    public DateTime? FEC_ACTUALIZACION { get; set; }
    public string COD_CUENTA_CONTABLE { get; set; } = "";
    [Required] public string DES_CUENTA_CONTABLE { get; set; } = "";
    public int? NUM_NIVEL { get; set; }
    public string? COD_CUENTA_CONTABLE_PADRE { get; set; }
    public string? COD_CUENTA_NATURALEZA { get; set; }
    public bool? FLG_GENERA_ASIENTO { get; set; }
    public bool? FLG_CENTRO_COSTO { get; set; }
    public string? COD_TIPO_CUENTA_CONTABLE { get; set; }
    public int? COD_TIPO_ESTADO { get; set; }
    public int? COD_ESTADO { get; set; }

    // Solo lectura (JOIN de los SP).
    public string? DES_CUENTA_NATURALEZA { get; set; }
    public string? DES_CUENTA_NATURALEZA_ALTERNATIVA { get; set; }
    public string? DES_ESTADO { get; set; }
    public string? DES_BACKCOLOR { get; set; }
    public string? DES_FORECOLOR { get; set; }
    public string? DES_TIPO_CUENTA_CONTABLE { get; set; }
}
