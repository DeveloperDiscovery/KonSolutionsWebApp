namespace KONSolutions.Shared.Models.Estandar;

/// <summary>ESTANDAR.CONFIGURACIONES_PRODUCTOS</summary>
public class ConfiguracionesProductos
{
    public string COD_TIPO_CONFIGURACION { get; set; } = string.Empty;
    public short NUM_SECUENCIA { get; set; }
    public string? COD_GRUPO_PRODUCTO { get; set; }
    public string? COD_FAMILIA_PRODUCTO { get; set; }
    public string? COD_SUB_FAMILIA_PRODUCTO { get; set; }
    public string? COD_USUARIO_REGISTRO { get; set; }
    public string? COD_ESTACION_REGISTRO { get; set; }
    public DateTime? FEC_REGISTRO { get; set; }

    // ── Solo lectura: traído por SEARCH/SHOWALL/SHOWBYID/SHOWCOMBOX vía JOIN ──
    public string? DES_GRUPO_PRODUCTO { get; set; }
    public string? DES_FAMILIA_PRODUCTO { get; set; }
    public string? DES_SUB_FAMILIA_PRODUCTO { get; set; }
    public string? DES_TIPO_CONFIGURACION { get; set; }
}
