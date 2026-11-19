namespace KONSolutions.Shared.Models.Comercial;

// Comparativo de los pedidos que atienden los mismos requerimientos.
// El SP devuelve dos listas: las cabeceras arman las columnas de la grilla y las
// líneas la llenan. El cruce producto × pedido lo hace la pantalla.

public class ComparativoPedidoCabecera
{
    public string COD_CLASE_OPERACION { get; set; } = "";
    public string COD_TIPO_OPERACION { get; set; } = "";
    public int NUM_PEDIDO_COMERCIAL { get; set; }
    public decimal? COD_ENTIDAD { get; set; }
    public string? DES_PROVEEDOR { get; set; }
    public string? COD_MONEDA { get; set; }
    public string? DES_MONEDA { get; set; }
    public DateTime? FEC_PEDIDO { get; set; }
    public DateTime? FEC_ENTREGA { get; set; }
    /// <summary>Días desde hoy hasta la entrega. Negativo si la fecha ya pasó.</summary>
    public int? CAN_DIAS_ENTREGA { get; set; }
    public int? COD_TIPO_ESTADO { get; set; }
    public int? COD_ESTADO { get; set; }
    public string? DES_ESTADO { get; set; }
    public string? DES_BACKCOLOR { get; set; }
    public string? DES_FORECOLOR { get; set; }
    public decimal VAL_TOTAL { get; set; }
}

public class ComparativoPedidoLinea
{
    public string COD_CLASE_OPERACION { get; set; } = "";
    public string COD_TIPO_OPERACION { get; set; } = "";
    public int NUM_PEDIDO_COMERCIAL { get; set; }
    public int NUM_SECUENCIA { get; set; }
    public string COD_PRODUCTO { get; set; } = "";
    public string? DES_PRODUCTO { get; set; }
    public string? COD_UNIDAD_MEDIDA { get; set; }
    public decimal? CAN_PRODUCTO { get; set; }
    public decimal? PRE_UNITARIO { get; set; }
    public decimal? TAS_DESCUENTO { get; set; }
    public decimal? VAL_TOTAL { get; set; }
    public string? COD_MONEDA { get; set; }
}

public class ComparativoResultado
{
    public List<ComparativoPedidoCabecera> Pedidos { get; set; } = new();
    public List<ComparativoPedidoLinea> Lineas { get; set; } = new();
}

public class RequerimientoConPedidos
{
    public int NUM_REQUERIMIENTO_STOCK { get; set; }
    public string? DES_REQUERIMIENTO_STOCK { get; set; }
    public DateTime? FEC_REQUERIMIENTO_STOCK { get; set; }
    public string? COD_TIPO_REQUERIMIENTO { get; set; }
    public string? DES_TIPO_REQUERIMIENTO { get; set; }
    /// <summary>Cuántos pedidos lo atienden. Con uno solo no hay comparación posible.</summary>
    public int CAN_PEDIDOS { get; set; }
}
