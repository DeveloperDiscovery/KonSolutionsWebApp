using System.ComponentModel.DataAnnotations;
namespace KONSolutions.Shared.Models.Logistica;

/// <summary>LOGISTICA.RESERVAS_STOCK_DETALLE</summary>
public class ReservasStockDetalle
{
    public int? NUM_RESERVA_STOCK { get; set; }
    public int? NUM_SECUENCIA { get; set; }
    // La ubicación de dónde sale la reserva. Se completan al elegir una fila del combo de
    // existencias (PROC_LOGISTICA_STOCKS_ALMACEN_UBICACION_SEARCHDETAIL) — no son parte del
    // filtro de producto, son PARTE del ítem: la misma reserva puede tener dos líneas del
    // mismo producto en dos ubicaciones distintas.
    public string? COD_ALMACEN { get; set; }
    public string? COD_UBICACION_ALMACEN { get; set; }
    public string COD_PRODUCTO { get; set; } = string.Empty;
    // La orden de producción de la que sale esta reserva: se completa sola al elegir la
    // ubicación de stock (esa fila de existencias ya la trae), no se tipea.
    public string? COD_PROCESO { get; set; }
    public string? NUM_LOTE_CONTROL { get; set; }
    public int? NUM_ORDEN_PRODUCCION { get; set; }
    public short? NUM_ORDEN_PRODUCCION_ITEM { get; set; }
    public byte? NUM_ORDEN_PRODUCCION_UNIDAD { get; set; }
    public string? COD_UNIDAD_MEDIDA { get; set; }
    public decimal? CAN_PRODUCTO { get; set; }
    public string? COD_UNIDAD_MEDIDA_SECUNDARIA { get; set; }
    public decimal? CAN_PRODUCTO_SECUNDARIA { get; set; }
    public decimal? CAN_PRODUCTO_ATENDIDA { get; set; }
    public decimal? CAN_PRODUCTO_ATENDIDA_SECUNDARIA { get; set; }

    // Vienen resueltas por los JOIN del procedimiento: sólo se muestran.
    public string? DES_ORDEN_PRODUCCION { get; set; }
    public string? DES_ALMACEN { get; set; }
    public string? DES_UBICACION_ALMACEN { get; set; }
    public string? DES_PRODUCTO { get; set; }
    public string? DES_DETALLADA { get; set; }
    public string? DES_UNIDAD_MEDIDA { get; set; }
    public string? DES_UNIDAD_MEDIDA_SECUNDARIA { get; set; }
    public string? DES_LOTE_CONTROL { get; set; }
    public string? NUM_LOTE_CONTROL_ENTIDAD { get; set; }
    public string? DES_POLIGONO_ALMACEN { get; set; }
    public string? DES_POLIGONO_UBICACIONES_ALMACEN { get; set; }

    // Vienen resueltas por los JOIN del procedimiento: solo se muestran.
    public string DES_RESERVA_STOCK { get; set; } = string.Empty;
    public string DES_OBSERVACION { get; set; } = string.Empty;
}
