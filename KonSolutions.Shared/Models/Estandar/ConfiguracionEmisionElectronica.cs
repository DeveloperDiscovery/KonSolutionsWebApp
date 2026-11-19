namespace KONSolutions.Shared.Models.Estandar;

/// <summary>Lo que muestra la pantalla de Configuración de emisión electrónica. En vez de los
/// secretos trae QUÉ HAY CARGADO (TIENE_*): la API nunca devuelve un secreto.</summary>
public class ConfiguracionEmisionElectronica
{
    public string COD_AMBIENTE { get; set; } = "PRUEBAS";
    public bool FLG_ACTIVA { get; set; }
    public string? DES_URL_SEGURIDAD { get; set; }
    public string? DES_URL_API { get; set; }
    public string? DES_CLIENT_ID { get; set; }
    public string? DES_USUARIO_SOL { get; set; }
    public bool TIENE_CLIENT_SECRET { get; set; }
    public bool TIENE_CLAVE_SOL { get; set; }
    public bool TIENE_CERTIFICADO { get; set; }
    public bool TIENE_CLAVE_CERTIFICADO { get; set; }
    public string? NOM_ARCHIVO_CERTIFICADO { get; set; }
    public string? DES_SUJETO_CERTIFICADO { get; set; }
    public string? DES_HUELLA_CERTIFICADO { get; set; }
    public DateTime? FEC_VENCIMIENTO_CERTIFICADO { get; set; }
    /// <summary>Días que le quedan al certificado (negativo: ya venció).</summary>
    public int? NUM_DIAS_PARA_VENCER { get; set; }
    /// <summary>False si el servidor no tiene la clave de cifrado (variable de entorno KonSolutions_EMISION_CLAVE).</summary>
    public bool CIFRADO_DISPONIBLE { get; set; }
    public string? DES_CIFRADO_NO_DISPONIBLE { get; set; }
    /// <summary>Lo que falta para poder activar la emisión. Vacío: está completo.</summary>
    public string? DES_FALTA { get; set; }
    public string? COD_USUARIO_ACTUALIZACION { get; set; }
    public DateTime? FEC_ACTUALIZACION { get; set; }
}

/// <summary>Lo que se manda al guardar. Los campos *_NUEVO / *_NUEVA sólo se llenan cuando se
/// escribió algo: vacío significa "no cambiar lo que hay".</summary>
public class ConfiguracionEmisionElectronicaGuardar
{
    public string COD_AMBIENTE { get; set; } = "PRUEBAS";
    public bool FLG_ACTIVA { get; set; }
    public string? DES_URL_SEGURIDAD { get; set; }
    public string? DES_URL_API { get; set; }
    public string? DES_CLIENT_ID { get; set; }
    public string? DES_USUARIO_SOL { get; set; }
    public string? DES_CLIENT_SECRET_NUEVO { get; set; }
    public string? DES_CLAVE_SOL_NUEVA { get; set; }
    public string? DES_CLAVE_CERTIFICADO_NUEVA { get; set; }
    /// <summary>El archivo .pfx en base64.</summary>
    public string? DES_CERTIFICADO_BASE64 { get; set; }
    public string? NOM_ARCHIVO_CERTIFICADO { get; set; }
}
