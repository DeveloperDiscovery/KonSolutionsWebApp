namespace KONSolutions.Shared.Models.Entorno;

/// <summary>Fila de PROC_ENTORNO_ENTIDADES_PROCEDURES_PARAMETERS_SHOWALL — un parámetro del
/// SP elegido en el generador de sentencias SQL de "Entidades a Configurar".</summary>
public class EntidadProcedureParametro
{
    public string ParameterName { get; set; } = string.Empty;
    public int SystemTypeId { get; set; }
    public string TypeName { get; set; } = string.Empty;
}
