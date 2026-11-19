namespace KONSolutions.Shared.Models.Comercial;

/// <summary>Historial de estados de un pedido comercial.</summary>
public class PedidosComercialesEstados
{
    public string COD_CLASE_OPERACION { get; set; } = "";
    public string COD_TIPO_OPERACION { get; set; } = "";
    public int NUM_PEDIDO_COMERCIAL { get; set; }
    public int NUM_SECUENCIA { get; set; }
    public int? COD_TIPO_ESTADO { get; set; }
    public int? COD_ESTADO { get; set; }
    public string? DES_ESTADO { get; set; }
    public byte[]? IMG_PICTURE { get; set; }
    public string? DES_BACKCOLOR { get; set; }
    public string? DES_FORECOLOR { get; set; }
    public string COD_USUARIO_REGISTRO { get; set; } = "";
    public string COD_ESTACION_REGISTRO { get; set; } = "";
    public DateTime? FEC_REGISTRO { get; set; }
}
