namespace KONSolutions.Shared.Models.Comercial;

// Cálculos de un pedido comercial: los importes que el pedido acumula por tipo de valor
// (subtotal, descuento, IGV, total…). Los produce la base, no la pantalla — acá solo se
// muestran, por eso el modelo no tiene DTO de escritura.
//
// Vienen de PROC_COMERCIAL_PEDIDOS_COMERCIALES_CALCULO_SHOWALL, que se lee SIEMPRE por
// cabecera: clase de operación + tipo de operación + número de pedido.

public class PedidoComercialCalculo
{
    public decimal? NUM_PEDIDO_COMERCIAL { get; set; }

    public string? COD_TIPO_VALOR_CALCULO { get; set; }
    /// <summary>Qué representa el importe: "Subtotal", "IGV", "Total"… Es lo que se rotula.</summary>
    public string? DES_TIPO_VALOR_CALCULO { get; set; }

    public string? COD_TIPO_IMPUESTO { get; set; }
    public string? DES_TIPO_IMPUESTO { get; set; }

    /// <summary>Tasa aplicada, tal cual la devuelve el SP. La pantalla la muestra sin
    /// convertir ni agregarle "%": no tengo confirmado si viene como 18 o como 0.18, y
    /// pintar un símbolo sobre el valor equivocado sería peor que no pintarlo.</summary>
    public decimal? TAS_IMPUESTO { get; set; }

    public decimal? VAL_CALCULO { get; set; }
}
