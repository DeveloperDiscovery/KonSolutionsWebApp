namespace KONSolutions.Shared.Models.Proyectos;

/// <summary>PROYECTOS.TIPOS_PROYECTO</summary>
public class TiposProyecto
{
    public string COD_TIPO_PROYECTO { get; set; } = string.Empty;
    public string DES_TIPO_PROYECTO { get; set; } = string.Empty;
    public bool? FLG_PRODUCCION { get; set; }
    public bool? FLG_PRESUPUESTO { get; set; }
    public bool? FLG_VALORADO { get; set; }
    public bool? FLG_SERVICIO { get; set; }
    public string COD_USUARIO_REGISTRO { get; set; } = string.Empty;
    public string COD_ESTACION_REGISTRO { get; set; } = string.Empty;
    public DateTime? FEC_REGISTRO { get; set; }
    public string COD_USUARIO_ACTUALIZACION { get; set; } = string.Empty;
    public string COD_ESTACION_ACTUALIZACION { get; set; } = string.Empty;
    public DateTime? FEC_ACTUALIZACION { get; set; }
}
