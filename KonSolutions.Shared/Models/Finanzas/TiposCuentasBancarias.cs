using System.ComponentModel.DataAnnotations;
namespace KONSolutions.Shared.Models.Finanzas;

/// <summary>FINANZAS.TIPOS_CUENTAS_BANCARIAS — sin columnas de auditoría.</summary>
public class TipoCuentaBancaria
{
    public string COD_TIPO_CUENTA_BANCARIA { get; set; } = "";
    [Required] public string DES_TIPO_CUENTA_BANCARIA { get; set; } = "";
    public byte[]? IMG_ICONO { get; set; }
}
