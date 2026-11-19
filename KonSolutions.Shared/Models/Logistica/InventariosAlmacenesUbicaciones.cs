namespace KONSolutions.Shared.Models.Logistica;

/// <summary>LOGISTICA.INVENTARIOS_ALMACENES_UBICACIONES. Clave compuesta NUM_INVENTARIO +
/// COD_ALMACEN + COD_UBICACION_ALMACEN — ubicaciones a inventariar dentro de un almacén
/// asignado a un inventario. Tabla de asignación pura: sin UPDATE (el INSERT hace upsert).</summary>
public class InventariosAlmacenesUbicaciones
{
    public int? NUM_INVENTARIO { get; set; }
    public string COD_ALMACEN { get; set; } = string.Empty;
    public string COD_UBICACION_ALMACEN { get; set; } = string.Empty;
    public string COD_USUARIO_REGISTRO { get; set; } = string.Empty;
    public string COD_ESTACION_REGISTRO { get; set; } = string.Empty;
    public DateTime? FEC_REGISTRO { get; set; }

    // ── Solo lectura ──
    public string? DES_UBICACION_ALMACEN { get; set; }
    public string? DES_POLIGONO { get; set; }
    public string? DES_OBSERVACION { get; set; }
}
