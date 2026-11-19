namespace KONSolutions.Shared.Models.Estandar;

/// <summary>ESTANDAR.ESTRUCTURA_PRODUCTO. Clave COD_GRUPO_PRODUCTO (un registro por Grupo).</summary>
public class EstructuraProducto
{
    public string COD_GRUPO_PRODUCTO { get; set; } = string.Empty;
    public bool? FLG_INCLUIR_ALIAS_GRUPO_PRODUCTO { get; set; }
    public bool? FLG_INCLUIR_FAMILIA_PRODUCTO { get; set; }
    public bool? FLG_INCLUIR_ALIAS_FAMILIA_PRODUCTO { get; set; }
    public bool? FLG_INCLUIR_SUB_FAMILIA_PRODUCTO { get; set; }
    public bool? FLG_INCLUIR_ALIAS_SUB_FAMILIA_PRODUCTO { get; set; }
    public int? LNG_CORRELATIVO { get; set; }

    public string? DES_GRUPO_PRODUCTO { get; set; }
}
