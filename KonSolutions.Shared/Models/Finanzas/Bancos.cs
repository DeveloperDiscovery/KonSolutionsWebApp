using System.ComponentModel.DataAnnotations;
namespace KONSolutions.Shared.Models.Finanzas;

/// <summary>FINANZAS.BANCOS — sin columnas de auditoría: la tabla no las tiene.</summary>
public class Banco
{
    public string COD_BANCO { get; set; } = "";
    [Required] public string DES_BANCO { get; set; } = "";
    public string? COD_BANCO_SUNAT { get; set; }
    public string? COD_PAIS { get; set; }
    public int? COD_TIPO_ESTADO { get; set; }
    public int? COD_ESTADO { get; set; }

    // Solo lectura (JOIN a ESTANDAR.ESTADOS / ESTANDAR.PAISES).
    public string? DES_ESTADO { get; set; }
    public string? DES_BACKCOLOR { get; set; }
    public string? DES_FORECOLOR { get; set; }
    public string? DES_PAIS { get; set; }
    public string? DES_BANDERA_EMOJI { get; set; }
}
