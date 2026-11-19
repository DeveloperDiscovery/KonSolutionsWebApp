namespace KONSolutions.Shared.Models.Logistica;

/// <summary>LOGISTICA.TIPOS_INVENTARIO</summary>
public class TiposInventario
{
    public string COD_TIPO_INVENTARIO { get; set; } = string.Empty;
    public string? DES_TIPO_INVENTARIO { get; set; }
    public bool? FLG_MUESTREO { get; set; }
    public bool? FLG_ABC_ALEATORIO { get; set; }
    public byte? COD_AREA_RESPONSABLE { get; set; }
    public byte? COD_CARGO_RESPONSABLE { get; set; }
    public byte? COD_AREA_ALMACEN { get; set; }
    public byte? COD_CARGO_ALMACEN { get; set; }

    // ── Solo lectura: 4 JOIN separados a AREAS/CARGOS, uno por cada código ──
    public string? DES_AREA_RESPONSABLE { get; set; }
    public string? DES_CARGO_RESPONSABLE { get; set; }
    public string? DES_AREA_ALMACEN { get; set; }
    public string? DES_CARGO_ALMACEN { get; set; }
}
