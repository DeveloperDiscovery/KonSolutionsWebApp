using System.ComponentModel.DataAnnotations;
namespace KONSolutions.Shared.Models.Logistica;

/// <summary>LOGISTICA.REQUERIMIENTOS_STOCK_DETALLE</summary>
public class RequerimientosStockDetalle
{
    public int? NUM_REQUERIMIENTO_STOCK { get; set; }
    public int? NUM_SECUENCIA { get; set; }
    public string COD_PRODUCTO { get; set; } = string.Empty;

    // ── Proceso y orden de producción ────────────────────────────────────────────────
    // Columnas nuevas del detalle. El proceso lo elige el usuario en la grilla (el LOADCOMBOS
    // ya trae la lista). Los tres de orden de producción los escribe el proceso de producción:
    // viajan en el modelo para no perderlos al grabar —mismo criterio que NUM_SECUENCIA_ORIGINAL—
    // pero la pantalla no los toca.
    public string? COD_PROCESO { get; set; }
    public int?    NUM_ORDEN_PRODUCCION { get; set; }
    public short?  NUM_ORDEN_PRODUCCION_ITEM { get; set; }
    public byte?   NUM_ORDEN_PRODUCCION_UNIDAD { get; set; }

    // La arma el LEFT JOIN contra PRODUCCION.PROCESOS. Sólo lectura.
    public string? DES_PROCESO { get; set; }
    public string? NUM_LOTE_CONTROL { get; set; }
    public string? COD_UNIDAD_MEDIDA { get; set; }
    public decimal? CAN_PRODUCTO { get; set; }
    public string? COD_UNIDAD_MEDIDA_SECUNDARIA { get; set; }
    public decimal? CAN_PRODUCTO_SECUNDARIA { get; set; }
    public decimal? CAN_PRODUCTO_ATENDIDA { get; set; }
    public decimal? CAN_PRODUCTO_ATENDIDA_SECUNDARIA { get; set; }
    public decimal? CAN_PRODUCTO_RESERVADA { get; set; }
    public decimal? CAN_PRODUCTO_RESERVADA_SECUNDARIA { get; set; }
    public decimal? CAN_PRODUCTO_REQUERIMIENTO_COMPRA { get; set; }
    public decimal? CAN_PRODUCTO_REQUERIMIENTO_COMPRA_SECUNDARIA { get; set; }

    // No se muestra en el detalle (ni columna ni campo): solo viaja por la API. En un alta
    // sale en null; en una modificación, la pantalla lo recibe del SHOWALL y lo devuelve tal
    // cual al guardar, sin tocarlo.
    public short? NUM_SECUENCIA_ORIGINAL { get; set; }

    // El SP de alta y el de modificación lo exigen, sin default.
    public string? COD_ALMACEN { get; set; }
    public string? COD_UBICACION_ALMACEN { get; set; }

    // Vienen resueltas por los JOIN del procedimiento: solo se muestran.
    public string DES_PRODUCTO { get; set; } = string.Empty;
    public string DES_DETALLADA { get; set; } = string.Empty;
    public string DES_LOTE_CONTROL { get; set; } = string.Empty;
    public string DES_UNIDAD_MEDIDA { get; set; } = string.Empty;
    public string DES_REQUERIMIENTO_STOCK { get; set; } = string.Empty;
    public string DES_OBSERVACION { get; set; } = string.Empty;
    public string DES_ALMACEN { get; set; } = string.Empty;
}
