using System.ComponentModel.DataAnnotations;
namespace KONSolutions.Shared.Models.Proyectos;

// Plantillas de documento y documentos emitidos por proyecto (script 81). Tienen que existir IGUAL en la API y en Shared.

/// <summary>PROYECTOS.PLANTILLAS_DOCUMENTO: el formato de un documento (texto base con campos, diseño del reporteador y tipo de compromiso).</summary>
public class PlantillaDocumento
{
    // Auditoria (la muestra PanelAuditoria en el dialogo)
    public string? COD_USUARIO_REGISTRO { get; set; }
    public string? COD_ESTACION_REGISTRO { get; set; }
    public DateTime? FEC_REGISTRO { get; set; }
    public string? COD_USUARIO_ACTUALIZACION { get; set; }
    public string? COD_ESTACION_ACTUALIZACION { get; set; }
    public DateTime? FEC_ACTUALIZACION { get; set; }
    public string COD_PLANTILLA_DOCUMENTO { get; set; } = string.Empty;
    public string DES_PLANTILLA_DOCUMENTO { get; set; } = string.Empty;
    public string? COD_TIPO_DOCUMENTO_COMPROMISO { get; set; }
    public string? DES_TIPO_DOCUMENTO_COMPROMISO { get; set; }
    public int? NUM_DISENO { get; set; }
    public string? DES_DISENO { get; set; }
    public string DES_CONTENIDO { get; set; } = string.Empty;
    public short NUM_VERSION { get; set; }
    public bool FLG_REQUIERE_APROBACION { get; set; } = true;
    public bool FLG_HABILITADO { get; set; } = true;
    public byte? NUM_ORDEN_PRESENTACION { get; set; }
    public int NUM_DOCUMENTOS { get; set; }
}

public class PlantillaDocumentoDto
{
    [Required] public string COD_PLANTILLA_DOCUMENTO { get; set; } = string.Empty;
    [Required] public string DES_PLANTILLA_DOCUMENTO { get; set; } = string.Empty;
    public string? COD_TIPO_DOCUMENTO_COMPROMISO { get; set; }
    public int? NUM_DISENO { get; set; }
    [Required] public string DES_CONTENIDO { get; set; } = string.Empty;
    public bool FLG_REQUIERE_APROBACION { get; set; } = true;
    public bool FLG_HABILITADO { get; set; } = true;
    public byte? NUM_ORDEN_PRESENTACION { get; set; }
}

public class PlantillaDocumentoSearchDto
{
    public string? Search { get; set; }
}

/// <summary>Un documento de un proyecto (estado BOR, ENV, APR, OBS, EMI o ANU). DES_CONTENIDO solo viene al pedir uno por su secuencia.</summary>
public class DocumentoEmitido
{
    public int NUM_PROYECTO { get; set; }
    public short NUM_SECUENCIA { get; set; }
    public string COD_PLANTILLA_DOCUMENTO { get; set; } = string.Empty;
    public string? DES_PLANTILLA_DOCUMENTO { get; set; }
    public int? NUM_DISENO { get; set; }
    public string DES_TITULO { get; set; } = string.Empty;
    public byte? COD_FASE { get; set; }
    public string? DES_FASE { get; set; }
    public short NUM_VERSION { get; set; }
    public short NUM_VERSION_PLANTILLA { get; set; }
    public short? NUM_SECUENCIA_ANTERIOR { get; set; }
    public bool FLG_VIGENTE { get; set; }
    public string COD_ESTADO_DOCUMENTO { get; set; } = "BOR";
    public string? DES_ESTADO_DOCUMENTO { get; set; }
    public short? NUM_SECUENCIA_COMPROMISO { get; set; }
    public string? COD_ESTADO_COMPROMISO { get; set; }
    public short? NUM_SECUENCIA_ARCHIVO { get; set; }
    public DateTime? FEC_EMISION { get; set; }
    public decimal? COD_ENTIDAD_EMISOR { get; set; }
    public string? DES_EMISOR { get; set; }
    public string? DES_MOTIVO_ANULACION { get; set; }
    public bool FLG_REQUIERE_APROBACION { get; set; }
    public string? COD_TIPO_DOCUMENTO_COMPROMISO { get; set; }
    public string? COD_USUARIO_REGISTRO { get; set; }
    public DateTime? FEC_REGISTRO { get; set; }
    public string? COD_USUARIO_ACTUALIZACION { get; set; }
    public DateTime? FEC_ACTUALIZACION { get; set; }
    public string? DES_CONTENIDO { get; set; }
}

public class DocumentoEmitidoNuevoDto
{
    [Required] public string CodPlantilla { get; set; } = string.Empty;
    public string? DesTitulo { get; set; }
    public byte? CodFase { get; set; }
}

public class DocumentoEmitidoGuardarDto
{
    [Required] public string DesTitulo { get; set; } = string.Empty;
    public byte? CodFase { get; set; }
    [Required] public string DesContenido { get; set; } = string.Empty;
}

public class DocumentoEmitidoEnviarDto
{
    /// <summary>Solo si la plantilla no tiene tipo de documento de compromiso.</summary>
    public string? CodTipoDocumento { get; set; }
    public List<decimal> Responsables { get; set; } = new();
    public List<decimal> Aprobadores { get; set; } = new();
}

public class DocumentoEmitidoEmitirDto
{
    public decimal? CodEntidadEmisor { get; set; }
}

public class DocumentoEmitidoAnularDto
{
    [Required] public string Motivo { get; set; } = string.Empty;
}

public class DocumentoEmitidoTextoDto
{
    public string Contenido { get; set; } = string.Empty;
}
