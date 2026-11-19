using System.ComponentModel.DataAnnotations;

namespace KONSolutions.Shared.Models.Comercial;

/// <summary>COMERCIAL.PEDIDOS_COMERCIALES_DETALLE — líneas del pedido (Fase 2).</summary>
public class PedidoComercialDetalle
{
    // Parte de la clave: el SP de alta y el de baja las exigen.
    public string? COD_CLASE_OPERACION { get; set; }
    public string? COD_TIPO_OPERACION { get; set; }
    public decimal? NUM_PEDIDO_COMERCIAL { get; set; }
    public int? NUM_SECUENCIA { get; set; }
    public string? COD_DISENO { get; set; }
    public int? NUM_VERSION { get; set; }
    public string COD_PRODUCTO { get; set; } = "";
    public string? NUM_LOTE_CONTROL { get; set; }
    // ── Proceso y orden de producción ────────────────────────────────────────────
    // Vienen del requerimiento que originó la línea: el panel agrupa por producto + unidad
    // + proceso + orden, así que cada orden es su propia línea del pedido.
    //
    // Anulables, y con el tipo exacto de la columna (int / smallint / tinyint). No anulables
    // romperían la lectura de todo el detalle que hoy los tiene en NULL.
    public string? COD_PROCESO { get; set; }
    public int?    NUM_ORDEN_PRODUCCION { get; set; }
    public short?  NUM_ORDEN_PRODUCCION_ITEM { get; set; }
    public byte?   NUM_ORDEN_PRODUCCION_UNIDAD { get; set; }
    /// <summary>Descripciones desnormalizadas por el SHOWALL. Sólo para mostrar.</summary>
    public string? DES_PROCESO { get; set; }
    public string? DES_ORDEN_PRODUCCION { get; set; }
    public decimal? CAN_PRODUCTO { get; set; }
    public string? COD_UNIDAD_MEDIDA { get; set; }
    public decimal? CAN_PRODUCTO_SECUDARIO { get; set; }
    public string? COD_UNIDAD_MEDIDA_SECUNDARIO { get; set; }
    public string? COD_TIPO_DESCUENTO { get; set; }
    public decimal? TAS_DESCUENTO { get; set; }
    public decimal? PRE_UNITARIO { get; set; }
    public decimal? VAL_DESCUENTO { get; set; }
    public decimal? VAL_TOTAL { get; set; }
    /// <summary>"Cantidad Proveedor" — caption pedida por el usuario. CAN_PRODUCTO es
    /// "Cantidad Propia". El SP calcula VAL_TOTAL como PRE_UNITARIO * CAN_PRODUCTO_ENTIDAD,
    /// no con CAN_PRODUCTO.</summary>
    public decimal? CAN_PRODUCTO_ENTIDAD { get; set; }
    public decimal? CAN_PRODUCTO_ATENDIDO { get; set; }
    public decimal? CAN_PRODUCTO_SECUNDARIO_ATENDIDO { get; set; }
    public string? DES_OBSERVACION { get; set; }
    public int? COD_TIPO_ESTADO { get; set; }
    public int? COD_ESTADO { get; set; }
    public string? COD_USUARIO_REGISTRO { get; set; }
    public string? COD_ESTACION_REGISTRO { get; set; }
    public DateTime? FEC_REGISTRO { get; set; }

    // Desnormalizados del SP
    public string DES_BACKCOLOR { get; set; } = "";
    public string DES_ESTADO { get; set; } = "";
    public string DES_FORECOLOR { get; set; } = "";
    public string DES_LOTE_CONTROL { get; set; } = "";
    public string DES_DETALLADA { get; set; } = "";
    public string DES_PRODUCTO { get; set; } = "";
    public string DES_TIPO_DESCUENTO { get; set; } = "";
    public string DES_UNIDAD_MEDIDA { get; set; } = "";
    /// <summary>Unidad de CAN_PRODUCTO_ENTIDAD, desde ENTIDADES_PRODUCTOS.COD_UNIDAD_MEDIDA_ATENCION.
    /// El nombre repite el typo tal cual lo devuelve el SP (MEDIDAD, con dos D).</summary>
    public string? COD_UNIDAD_MEDIDAD_ENTIDAD { get; set; }
}

public class PedidoComercialDetalleInsertDto
{
    [Required] public decimal NUM_PEDIDO_COMERCIAL { get; set; }
    [Required] public int NUM_SECUENCIA { get; set; }
    public string? COD_DISENO { get; set; }
    public int? NUM_VERSION { get; set; }
    [Required] public string COD_PRODUCTO { get; set; } = "";
    public string? NUM_LOTE_CONTROL { get; set; }
    // ── Proceso y orden de producción ────────────────────────────────────────────
    // Vienen del requerimiento que originó la línea: el panel agrupa por producto + unidad
    // + proceso + orden, así que cada orden es su propia línea del pedido.
    //
    // Anulables, y con el tipo exacto de la columna (int / smallint / tinyint). No anulables
    // romperían la lectura de todo el detalle que hoy los tiene en NULL.
    public string? COD_PROCESO { get; set; }
    public int?    NUM_ORDEN_PRODUCCION { get; set; }
    public short?  NUM_ORDEN_PRODUCCION_ITEM { get; set; }
    public byte?   NUM_ORDEN_PRODUCCION_UNIDAD { get; set; }
    public decimal? CAN_PRODUCTO { get; set; }
    public string? COD_UNIDAD_MEDIDA { get; set; }
    public decimal? CAN_PRODUCTO_SECUDARIO { get; set; }
    public string? COD_UNIDAD_MEDIDA_SECUNDARIO { get; set; }
    public string? COD_TIPO_DESCUENTO { get; set; }
    public decimal? TAS_DESCUENTO { get; set; }
    public decimal? PRE_UNITARIO { get; set; }
    public decimal? VAL_DESCUENTO { get; set; }
    public decimal? VAL_TOTAL { get; set; }
    /// <summary>"Cantidad Proveedor" — caption pedida por el usuario. CAN_PRODUCTO es
    /// "Cantidad Propia". El SP calcula VAL_TOTAL como PRE_UNITARIO * CAN_PRODUCTO_ENTIDAD,
    /// no con CAN_PRODUCTO.</summary>
    public decimal? CAN_PRODUCTO_ENTIDAD { get; set; }
    public decimal? CAN_PRODUCTO_ATENDIDO { get; set; }
    public decimal? CAN_PRODUCTO_SECUNDARIO_ATENDIDO { get; set; }
    public string? DES_OBSERVACION { get; set; }
    public int? COD_TIPO_ESTADO { get; set; }
    public int? COD_ESTADO { get; set; }
}

public class PedidoComercialDetalleUpdateDto : PedidoComercialDetalleInsertDto { }
