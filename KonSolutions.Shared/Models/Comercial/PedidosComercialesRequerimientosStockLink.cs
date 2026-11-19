namespace KONSolutions.Shared.Models.Comercial;

// Lectura de los vínculos pedido↔requerimiento ya grabados (no confundir con
// RequerimientoStockRegistrado en PedidosComercialesRequerimientos.cs, que es la lista de
// candidatos para incorporar). Estos dos SHOWALL leen las tablas de vínculo tal cual quedaron.

/// <summary>PROC_COMERCIAL_PEDIDOS_COMERCIALES_REQUERIMIENTOS_STOCK_SHOWALL — los requerimientos
/// ya vinculados a la CABECERA del pedido (uno o varios).</summary>
public class PedidoComercialRequerimientoStockLink
{
    public string COD_CLASE_OPERACION { get; set; } = "";
    public string COD_TIPO_OPERACION { get; set; } = "";
    public decimal NUM_PEDIDO_COMERCIAL { get; set; }
    public int NUM_REQUERIMIENTO_STOCK { get; set; }
    public DateTime? FEC_REQUERIMIENTO_STOCK { get; set; }
    public string? DES_REQUERIMIENTO_STOCK { get; set; }
    public string? DES_OBSERVACION { get; set; }
    public string? DES_PEDIDO_COMERCIAL { get; set; }
}

/// <summary>PROC_COMERCIAL_PEDIDOS_COMERCIALES_DETALLE_REQUERIMIENTOS_STOCK_DETALLE_SHOWALL —
/// para una LÍNEA del pedido y un requerimiento puntual, qué línea(s) de ESE requerimiento la
/// alimentaron y con cuánta cantidad. La subgrilla del detalle del pedido muestra solo
/// NUM_REQUERIMIENTO_STOCK, NUM_SECUENCIA_REQUERIMIENTO_STOCK, CAN_PRODUCTO y
/// COD_UNIDAD_MEDIDA — pero el modelo trae todo lo que devuelve el SP por si hace falta después.</summary>
public class PedidoComercialDetalleRequerimientoStockDetalleLinea
{
    public string COD_CLASE_OPERACION { get; set; } = "";
    public string COD_TIPO_OPERACION { get; set; } = "";
    public decimal NUM_PEDIDO_COMERCIAL { get; set; }
    public int NUM_SECUENCIA { get; set; }
    public int NUM_REQUERIMIENTO_STOCK { get; set; }
    public int NUM_SECUENCIA_REQUERIMIENTO_STOCK { get; set; }
    public string? COD_PRODUCTO { get; set; }
    public string? DES_PRODUCTO { get; set; }
    public decimal? CAN_PRODUCTO { get; set; }
    public string? COD_UNIDAD_MEDIDA { get; set; }
    public decimal? CAN_PRODUCTO_SECUDARIO { get; set; }
    public string? COD_UNIDAD_MEDIDA_SECUNDARIO { get; set; }
    /// <summary>Cantidad ya atendida de la línea del requerimiento
    /// (LOGISTICA.REQUERIMIENTOS_STOCK_DETALLE.CAN_PRODUCTO_ATENDIDA) — no es del pedido,
    /// es el acumulado atendido del requerimiento de origen.</summary>
    public decimal? CAN_PRODUCTO_ATENDIDA { get; set; }
    public decimal? CAN_PRODUCTO_ATENDIDA_SECUNDARIA { get; set; }
    public string? DES_OBSERVACION { get; set; }
}
