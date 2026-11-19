using System.ComponentModel.DataAnnotations;
namespace KONSolutions.Shared.Models.Contabilidad;

/// <summary>CONTABILIDAD.CUENTAS_NATURALEZA — sin columnas de auditoría.</summary>
public class CuentaNaturaleza
{
    public string COD_CUENTA_NATURALEZA { get; set; } = "";
    [Required] public string DES_CUENTA_NATURALEZA { get; set; } = "";
    public string? DES_CUENTA_NATURALEZA_ALTERNATIVA { get; set; }
}
