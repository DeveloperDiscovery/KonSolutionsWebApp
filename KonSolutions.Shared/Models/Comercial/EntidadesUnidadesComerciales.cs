namespace KONSolutions.Shared.Models.Comercial;

/// <summary>COMERCIAL.ENTIDADES_UNIDADES_COMERCIALES — pestaña "Unidades Comerciales" del
/// diálogo de Entidades, después de Medios de Contacto.</summary>
public class EntidadesUnidadesComerciales
{
    public decimal? COD_ENTIDAD { get; set; }
    public byte? COD_UNIDAD_COMERCIAL { get; set; }
    public string? DES_UNIDAD_COMERCIAL { get; set; }
    public string? COD_PAIS { get; set; }
    public string? DES_DIRECCION { get; set; }
    public int? COD_TIPO_ESTADO { get; set; }
    public int? COD_ESTADO { get; set; }
    public string? COD_USUARIO_REGISTRO { get; set; }
    public string? COD_ESTACION_REGISTRO { get; set; }
    public DateTime? FEC_REGISTRO { get; set; }
    public string? COD_USUARIO_ACTUALIZACION { get; set; }
    public string? COD_ESTACION_ACTUALIZACION { get; set; }
    public DateTime? FEC_ACTUALIZACION { get; set; }
}
