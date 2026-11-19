namespace KONSolutions.Shared.Models.Comercial;

// Requerimientos de stock que originan un pedido comercial. Dos niveles de vínculo:
// el pedido con el requerimiento, y cada línea del pedido con las líneas de
// requerimiento que la alimentaron. El segundo es el que permite acumular varios
// requerimientos en una sola línea sin perder de dónde salió cada cantidad.

/// <summary>Requerimiento de compra registrado, elegible para el pedido.
/// FLG_SECCION = ya está vinculado a este pedido.</summary>
public class RequerimientoStockRegistrado
{
    public bool FLG_SECCION { get; set; }
    public int NUM_REQUERIMIENTO_STOCK { get; set; }
    public string? DES_REQUERIMIENTO_STOCK { get; set; }
    public DateTime? FEC_REQUERIMIENTO_STOCK { get; set; }
    public string? COD_TIPO_REQUERIMIENTO { get; set; }
    public string? DES_TIPO_REQUERIMIENTO { get; set; }
}

/// <summary>Línea de un requerimiento registrado.
/// FLG_SECCION_DETALLE = esta línea ya fue incorporada al pedido.</summary>
public class RequerimientoStockDetalleRegistrado
{
    public bool FLG_SECCION { get; set; }
    public bool FLG_SECCION_DETALLE { get; set; }
    /// <summary>Línea del PEDIDO en la que entró esta línea de requerimiento. Null si
    /// todavía no se incorporó. Hace falta para poder deshacer el vínculo.</summary>
    public int? NUM_SECUENCIA_PEDIDO { get; set; }
    public int NUM_REQUERIMIENTO_STOCK { get; set; }
    public int NUM_SECUENCIA { get; set; }
    public string COD_PRODUCTO { get; set; } = "";
    public string? DES_PRODUCTO { get; set; }
    /// <summary>Lote de control de la línea del requerimiento. Viaja a la línea del pedido y de ahí a la Orden de Compra.</summary>
    public string? NUM_LOTE_CONTROL { get; set; }

    // ── Proceso y orden de producción ────────────────────────────────────────────────
    // El SP los devuelve desde que el detalle del requerimiento los tiene. Sin estas
    // propiedades, Dapper los descartaba en silencio: no fallaba nada, pero en el panel dos
    // líneas del MISMO producto para órdenes distintas se veían idénticas, y el operador no
    // tenía con qué distinguirlas al marcar.
    public string? COD_PROCESO { get; set; }
    public string? DES_PROCESO { get; set; }
    public int?    NUM_ORDEN_PRODUCCION { get; set; }
    public short?  NUM_ORDEN_PRODUCCION_ITEM { get; set; }
    public byte?   NUM_ORDEN_PRODUCCION_UNIDAD { get; set; }
    public string? COD_UNIDAD_MEDIDA { get; set; }
    // ── Cantidades — el SP le sacó el typo a las dos, mapeo 1 a 1 con el detalle: ──
    //   CAN_REQUERIDA        "Cantidad Requerida"
    //   CAN_PRODUCTO         "Cantidad Propia"      -> DETALLE.CAN_PRODUCTO
    //   CAN_PRODUCTO_ENTIDAD "Cantidad Proveedor"   -> DETALLE.CAN_PRODUCTO_ENTIDAD

    /// <summary>Cantidad tal como está en la línea del requerimiento, sin convertir
    /// ("Cantidad Requerida").</summary>
    public decimal? CAN_REQUERIDA { get; set; }
    /// <summary>Factor de COMERCIAL.ENTIDADES_PRODUCTOS para pasar de la unidad del
    /// requerimiento a la propia. 1 si el producto no tiene conversión configurada.</summary>
    public decimal? FAC_CONVERSION { get; set; }
    /// <summary>CEILING(CAN_REQUERIDA / FAC_CONVERSION) * FAC_CONVERSION ("Cantidad Propia")
    /// — va directo a PEDIDOS_COMERCIALES_DETALLE.CAN_PRODUCTO al incorporar.</summary>
    public decimal? CAN_PRODUCTO { get; set; }
    /// <summary>CEILING(CAN_REQUERIDA / FAC_CONVERSION) ("Cantidad Proveedor") — va directo a
    /// PEDIDOS_COMERCIALES_DETALLE.CAN_PRODUCTO_ENTIDAD al incorporar.</summary>
    public decimal? CAN_PRODUCTO_ENTIDAD { get; set; }
    /// <summary>Unidad de CAN_PRODUCTO_ENTIDAD. Con una sola D — distinto del
    /// COD_UNIDAD_MEDIDAD_ENTIDAD (dos D) que usa el SHOWALL del detalle del pedido; son dos
    /// SP distintos, cada uno con su propio alias.</summary>
    public string? COD_UNIDAD_MEDIDA_ENTIDAD { get; set; }
    public string? COD_UNIDAD_MEDIDA_SECUNDARIA { get; set; }
    public decimal? CAN_PRODUCTO_SECUNDARIA { get; set; }

    // Precio del proveedor para ese producto, resuelto por el SP en
    // ENTIDADES_PRODUCTOS_TIPOS_PRECIOS: toma el tipo de precio que tiene configurado el
    // TIPO DE OPERACIÓN del pedido, y de ahí sale VAL_PRECIO_ENTIDAD. DES_TIPO_PRECIO dice
    // cuál se aplicó, que es lo que hace auditable el número.
    //
    // COD_MONEDA es la del PRECIO y DES_MONEDA la del PEDIDO. El SP ahora sí convierte
    // cuando difieren (FNCN_MONEDAS_TIPO_CAMBIO): VAL_PRECIO_ENTIDAD ya viene en la moneda
    // del pedido en los dos casos.
    public string? DES_TIPO_PRECIO { get; set; }
    public string? COD_MONEDA { get; set; }
    public string? DES_MONEDA { get; set; }
    public decimal? VAL_PRECIO_ENTIDAD { get; set; }
}

public class PedidoComercialRequerimientoStock
{
    public string COD_CLASE_OPERACION { get; set; } = "";
    public string COD_TIPO_OPERACION { get; set; } = "";
    public decimal NUM_PEDIDO_COMERCIAL { get; set; }
    public int NUM_REQUERIMIENTO_STOCK { get; set; }
    public string? DES_REQUERIMIENTO_STOCK { get; set; }
}

public class PedidoComercialRequerimientoStockDto
{
    public string COD_CLASE_OPERACION { get; set; } = "";
    public string COD_TIPO_OPERACION { get; set; } = "";
    public decimal NUM_PEDIDO_COMERCIAL { get; set; }
    public int NUM_REQUERIMIENTO_STOCK { get; set; }
}

public class PedidoComercialDetalleRequerimientoStockDetalleDto
{
    public string COD_CLASE_OPERACION { get; set; } = "";
    public string COD_TIPO_OPERACION { get; set; } = "";
    public decimal NUM_PEDIDO_COMERCIAL { get; set; }
    public int NUM_SECUENCIA { get; set; }
    public int NUM_REQUERIMIENTO_STOCK { get; set; }
    public int NUM_SECUENCIA_REQUERIMIENTO_STOCK { get; set; }
    public decimal? CAN_PRODUCTO { get; set; }
    public string? COD_UNIDAD_MEDIDA { get; set; }
    // El nombre va sin la N, igual que la columna de la tabla y el parámetro del SP.
    public decimal? CAN_PRODUCTO_SECUDARIO { get; set; }
    public string? COD_UNIDAD_MEDIDA_SECUNDARIO { get; set; }
}

/// <summary>Lo que el panel le pasa al documento del pedido: un producto ya acumulado y
/// las líneas de requerimiento de donde salió, para dejar la trazabilidad después.</summary>
public class GrupoRequerimientoIncorporar
{
    public string COD_PRODUCTO { get; set; } = "";
    public string? DES_PRODUCTO { get; set; }
    public string? COD_UNIDAD_MEDIDA { get; set; }
    /// <summary>Va a COMERCIAL.PEDIDOS_COMERCIALES_DETALLE.NUM_LOTE_CONTROL. Parte de la identidad del grupo.</summary>
    public string? NUM_LOTE_CONTROL { get; set; }
    /// <summary>Va a COMERCIAL.PEDIDOS_COMERCIALES_DETALLE.CAN_PRODUCTO ("Cantidad Propia").</summary>
    public decimal CAN_PRODUCTO { get; set; }
    /// <summary>Va a COMERCIAL.PEDIDOS_COMERCIALES_DETALLE.CAN_PRODUCTO_ENTIDAD
    /// ("Cantidad Proveedor").</summary>
    public decimal CAN_PRODUCTO_ENTIDAD { get; set; }
    /// <summary>Va a COMERCIAL.PEDIDOS_COMERCIALES_DETALLE.COD_UNIDAD_MEDIDAD_ENTIDAD (con dos
    /// D, el nombre de ESE campo) — la unidad de CAN_PRODUCTO_ENTIDAD.</summary>
    public string? COD_UNIDAD_MEDIDA_ENTIDAD { get; set; }
    public decimal? CAN_PRODUCTO_SECUNDARIA { get; set; }
    public string? COD_UNIDAD_MEDIDA_SECUNDARIA { get; set; }
    /// <summary>Precio unitario que viene del SP. Si las líneas agrupadas trajeran precios
    /// distintos se queda con el primero — son el mismo producto y el mismo proveedor, así
    /// que en la práctica es el mismo número.</summary>
    public decimal? PRE_UNITARIO { get; set; }
    public string? COD_MONEDA { get; set; }
    /// <summary>Qué tipo de precio se aplicó, para que el número sea auditable.</summary>
    public string? DES_TIPO_PRECIO { get; set; }
    // ── Proceso y orden de producción del grupo ──────────────────────────────────────
    // Forman parte de la IDENTIDAD del grupo, no son informativos: dos líneas del mismo
    // producto para órdenes distintas son dos líneas distintas del pedido.
    public string? COD_PROCESO { get; set; }
    public string? DES_PROCESO { get; set; }
    public int?    NUM_ORDEN_PRODUCCION { get; set; }
    public short?  NUM_ORDEN_PRODUCCION_ITEM { get; set; }
    public byte?   NUM_ORDEN_PRODUCCION_UNIDAD { get; set; }

    /// <summary>Identifica al grupo, y es la clave del diccionario con el que el panel y el
    /// documento se ponen de acuerdo sobre en qué línea del pedido quedó cada grupo.
    ///
    /// Vive ACÁ y no en cada pantalla a propósito: antes la armaban el panel y el documento
    /// por separado, con la misma cadena escrita dos veces. Cambiarla en un solo lado no da
    /// error de compilación — da un diccionario que no encuentra nada y vínculos que se
    /// pierden en silencio.</summary>
    public string Clave =>
        $"{COD_PRODUCTO}|{COD_UNIDAD_MEDIDA}|{NUM_LOTE_CONTROL}|{COD_PROCESO}|{NUM_ORDEN_PRODUCCION}|{NUM_ORDEN_PRODUCCION_ITEM}|{NUM_ORDEN_PRODUCCION_UNIDAD}";

    /// <summary>Una línea que no viene de producción: puede sumarse a una línea del pedido que
    /// ya exista para ese producto, que es como se comportó siempre. Las que sí traen orden no
    /// se suman a nada — cada orden es su propia línea.</summary>
    public bool SinOrdenProduccion => NUM_ORDEN_PRODUCCION is null or 0;

    public List<OrigenRequerimiento> Origenes { get; set; } = new();
}

public class OrigenRequerimiento
{
    public int NUM_REQUERIMIENTO_STOCK { get; set; }
    public int NUM_SECUENCIA { get; set; }
    public decimal? CAN_PRODUCTO { get; set; }
    /// <summary>Cantidad Requerida de la línea de origen (RequerimientoStockDetalleRegistrado.
    /// CAN_REQUERIDA), sin convertir. Es lo que se manda como @CAN_PRODUCTO al insertar en
    /// PEDIDOS_COMERCIALES_DETALLE_REQUERIMIENTOS_STOCK_DETALLE — a pedido explícito, ese
    /// insert de trazabilidad usa la cantidad requerida y no la "Cantidad Propia" ya
    /// convertida que sí usa la línea del pedido.</summary>
    public decimal? CAN_REQUERIDA { get; set; }
    public string? COD_UNIDAD_MEDIDA { get; set; }
    public decimal? CAN_PRODUCTO_SECUNDARIA { get; set; }
    public string? COD_UNIDAD_MEDIDA_SECUNDARIA { get; set; }
}
