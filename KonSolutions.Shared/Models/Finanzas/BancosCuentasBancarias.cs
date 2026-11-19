namespace KONSolutions.Shared.Models.Finanzas;

/// <summary>FINANZAS.BANCOS_CUENTAS_BANCARIAS — cuentas propias de un banco (distinto de
/// FINANZAS.ENTIDADES_CUENTAS_BANCARIAS, que son las cuentas de terceros en ese banco).
/// PK compuesta (COD_BANCO, NUM_CUENTA_BANCARIA).</summary>
public class BancoCuentaBancaria
{
    public string COD_BANCO { get; set; } = "";
    public string NUM_CUENTA_BANCARIA { get; set; } = "";
    public string? NUM_CUENTA_BANCARIA_CCI { get; set; }
    public string? COD_TIPO_CUENTA_BANCARIA { get; set; }
    public string? COD_MONEDA { get; set; }
    public string? COD_CUENTA_CONTABLE { get; set; }
    public decimal? VAL_SALDO_INICIAL { get; set; }
    public decimal? VAL_MOVIMIENTOS_INGRESO { get; set; }
    public decimal? VAL_MOVIMIENTOS_SALIDA { get; set; }
    public decimal? VAL_SALDO_FINAL { get; set; }
    public bool? FLG_PRINCIPAL { get; set; }
    public bool? FLG_HABILITADA { get; set; }
    public bool? FLG_CUENTA_EMPRESA { get; set; }
    public bool? FLG_COBROS { get; set; }
    public bool? FLG_PAGOS { get; set; }
    public bool? FLG_RESTRINGIDA { get; set; }
    public int? COD_TIPO_ESTADO { get; set; }
    public int? COD_ESTADO { get; set; }
    public string? COD_USUARIO_REGISTRO { get; set; }
    public string? COD_ESTACION_REGISTRO { get; set; }
    public DateTime? FEC_REGISTRO { get; set; }
    public string? COD_USUARIO_ACTUALIZACION { get; set; }
    public string? COD_ESTACION_ACTUALIZACION { get; set; }
    public DateTime? FEC_ACTUALIZACION { get; set; }

    // Solo lectura (JOIN de los SP).
    public string? DES_BANCO { get; set; }
    public string? DES_ESTADO { get; set; }
    public string? DES_BACKCOLOR { get; set; }
    public string? DES_FORECOLOR { get; set; }
    public string? DES_MONEDA { get; set; }
    public string? DES_TIPO_CUENTA_BANCARIA { get; set; }
}
