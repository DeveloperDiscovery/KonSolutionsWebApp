namespace KONSolutions.Shared.Models.Logistica;

/// <summary>LOGISTICA.INVENTARIOS_ALMACENES_IMAGEN. Clave compuesta larga (9 columnas).
/// SIN CRUD manual — la puebla un proceso todavía no implementado (toma de imagen de
/// stock). El frontend solo la lista en modo solo lectura.</summary>
public class InventariosAlmacenesImagen
{
    public int? NUM_INVENTARIO { get; set; }
    public string COD_ALMACEN { get; set; } = string.Empty;
    public string COD_UBICACION_ALMACEN { get; set; } = string.Empty;
    public string COD_PRODUCTO { get; set; } = string.Empty;
    public string NUM_LOTE_CONTROL { get; set; } = string.Empty;
    public string COD_PROCESO { get; set; } = string.Empty;
    public int? NUM_ORDEN_PRODUCCION { get; set; }
    public int? NUM_ORDEN_PRODUCCION_ITEM { get; set; }
    public int? NUM_ORDEN_PRODUCCION_UNIDAD { get; set; }
    public decimal? CAN_PRODUCTO { get; set; }
    public decimal? CAN_PRODUCTO_CONTEO { get; set; }
    public decimal? CAN_PRODUCTO_CONTEO_DIFERENCIA { get; set; }
    public string? COD_UNIDAD_MEDIDA { get; set; }
    public decimal? CAN_PRODUCTO_INVENTARIADA { get; set; }
    public decimal? CAN_PRODUCTO_DIFERENCIA { get; set; }
}
