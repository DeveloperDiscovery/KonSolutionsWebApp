namespace KONSolutions.Shared.Models.Comercial;

/// <summary>Lo atendido de un pedido comercial (por ejemplo una orden de compra), para la barra de la grilla.
/// POR_ATENDIDO = atendida * 100 / pedida (0 si no hay cantidad pedida; puede pasar de 100 si se recibió de más).</summary>
public class PedidoComercialAtencion
{
    public string COD_CLASE_OPERACION { get; set; } = "";
    public string COD_TIPO_OPERACION { get; set; } = "";
    public decimal NUM_PEDIDO_COMERCIAL { get; set; }
    public int NUM_LINEAS { get; set; }
    public decimal CAN_PEDIDA { get; set; }
    public decimal CAN_ATENDIDA { get; set; }
    public decimal POR_ATENDIDO { get; set; }
}
