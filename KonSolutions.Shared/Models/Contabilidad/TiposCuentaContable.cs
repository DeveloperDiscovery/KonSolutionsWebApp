using System.ComponentModel.DataAnnotations;
namespace KONSolutions.Shared.Models.Contabilidad;

/// <summary>CONTABILIDAD.TIPOS_CUENTA_CONTABLE — sin columnas de auditoría.</summary>
public class TipoCuentaContable
{
    public string COD_TIPO_CUENTA_CONTABLE { get; set; } = "";
    [Required] public string DES_TIPO_CUENTA_CONTABLE { get; set; } = "";
    public string? COD_CUENTA_NATURALEZA { get; set; }
    public bool? FLG_AFECTA_BALANCE { get; set; }
    public bool? FLG_AFECTA_RESULTADO { get; set; }

    // Solo lectura (JOIN a CONTABILIDAD.CUENTAS_NATURALEZA).
    public string? DES_CUENTA_NATURALEZA { get; set; }
    public string? DES_CUENTA_NATURALEZA_ALTERNATIVA { get; set; }
}
