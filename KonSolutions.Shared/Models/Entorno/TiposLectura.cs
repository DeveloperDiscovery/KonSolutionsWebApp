using System.ComponentModel.DataAnnotations;

namespace KONSolutions.Shared.Models.Entorno;

/// <summary>ENTORNO.TIPOS_LECTURA: cómo se lee un producto. Las tres banderas son independientes (un tipo puede permitir
/// más de una forma de lectura). El código es tinyint y lo asigna la base.</summary>
public class TiposLectura
{
    public byte?   COD_TIPO_LECTURA { get; set; }
    [Required] public string DES_TIPO_LECTURA { get; set; } = string.Empty;
    public bool?   FLG_MANUAL  { get; set; }
    public bool?   FLG_CODEBAR { get; set; }
    public bool?   FLG_RFID    { get; set; }
}
