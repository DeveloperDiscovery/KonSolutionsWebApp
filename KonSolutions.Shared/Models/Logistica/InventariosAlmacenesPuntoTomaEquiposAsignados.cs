namespace KONSolutions.Shared.Models.Logistica;

/// <summary>LOGISTICA.INVENTARIOS_ALMACENES_PUNTO_TOMA_EQUIPOS_ASIGNADOS — equipos (productos)
/// asignados a un punto de conteo. Clave compuesta NUM_INVENTARIO + COD_ALMACEN +
/// NUM_PUNTO_TOMA_INVENTARIO + COD_PRODUCTO_EQUIPO (código elegido por el usuario, alta pura).</summary>
public class InventariosAlmacenesPuntoTomaEquiposAsignados
{
    // Auditoria (la muestra PanelAuditoria en el dialogo)
    public string? COD_USUARIO_REGISTRO { get; set; }
    public string? COD_ESTACION_REGISTRO { get; set; }
    public DateTime? FEC_REGISTRO { get; set; }
    public string? COD_USUARIO_ACTUALIZACION { get; set; }
    public string? COD_ESTACION_ACTUALIZACION { get; set; }
    public DateTime? FEC_ACTUALIZACION { get; set; }
    public int? NUM_INVENTARIO { get; set; }
    public string COD_ALMACEN { get; set; } = string.Empty;
    public byte? NUM_PUNTO_TOMA_INVENTARIO { get; set; }
    public string COD_PRODUCTO_EQUIPO { get; set; } = string.Empty;
    public decimal? CAN_PRODUCTO { get; set; }

    public string? DES_DETALLADA { get; set; }
    public string? DES_PRODUCTO { get; set; }
    public string? DES_OBSERVACION { get; set; }
}
