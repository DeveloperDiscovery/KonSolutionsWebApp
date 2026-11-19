namespace KONSolutions.Shared.Models.Logistica;

/// <summary>LOGISTICA.INVENTARIOS_ALMACENES_PRODUCTOS. Clave compuesta NUM_INVENTARIO +
/// COD_ALMACEN + COD_PRODUCTO — productos a inventariar dentro de un almacén asignado a un
/// inventario. Tabla de asignación pura: sin UPDATE (el INSERT hace upsert).</summary>
public class InventariosAlmacenesProductos
{
    public int? NUM_INVENTARIO { get; set; }
    public string COD_ALMACEN { get; set; } = string.Empty;
    public string COD_PRODUCTO { get; set; } = string.Empty;
    public string COD_USUARIO_REGISTRO { get; set; } = string.Empty;
    public string COD_ESTACION_REGISTRO { get; set; } = string.Empty;
    public DateTime? FEC_REGISTRO { get; set; }

    // ── Solo lectura ──
    public string? DES_PRODUCTO { get; set; }
    public string? DES_DETALLADA { get; set; }
    public string? DES_OBSERVACION { get; set; }
}
