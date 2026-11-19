namespace KONSolutions.Shared.Models.Finanzas;

/// <summary>FINANZAS.ENTIDADES_CUENTAS_BANCARIAS — cuentas bancarias de una entidad.
/// PK compuesta (COD_ENTIDAD_BANCARIA, COD_BANCO, NUM_CUENTA_BANCARIA). Sin columnas de
/// auditoría: la tabla no las tiene.</summary>
public class EntidadCuentaBancaria
{
    public decimal? COD_ENTIDAD_BANCARIA { get; set; }
    public string COD_BANCO { get; set; } = "";
    public string NUM_CUENTA_BANCARIA { get; set; } = "";
    public string? NUM_CUENTA_BANCARIA_CCI { get; set; }
    public string? DES_CUENTA_BANCARIA { get; set; }
    public bool? FLG_PRINCIPAL { get; set; }
    public bool? FLG_HABILITADA { get; set; }
    public bool? FLG_CUENTA_EMPRESA { get; set; }
    public bool? FLG_COBROS { get; set; }
    public bool? FLG_PAGOS { get; set; }
    public bool? FLG_RESTRINGIDA { get; set; }
    public string? COD_TIPO_CUENTA_BANCARIA { get; set; }
    public string? COD_MONEDA { get; set; }

    // Solo lectura (JOIN de los SP).
    public string? DES_BANCO { get; set; }
    public string? DES_TIPO_CUENTA_BANCARIA { get; set; }
}
