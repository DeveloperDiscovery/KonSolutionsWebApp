namespace KONSolutions.Shared.Models.Estandar;

/// <summary>ESTANDAR.ESTRUCTURA_CODEBAR.</summary>
public class EstructuraCodebar
{
    public string COD_ESTRUCTURA_CODEBAR { get; set; } = string.Empty;
    public string? DES_ESTRUCTURA_CODEBAR { get; set; }
    public int? VAL_LONGITUD { get; set; }
    public int? NUM_ORDEN { get; set; }
}

public class EstructuraCodebarInsertDto
{
    public string COD_ESTRUCTURA_CODEBAR { get; set; } = string.Empty;
    public string? DES_ESTRUCTURA_CODEBAR { get; set; }
    public int? VAL_LONGITUD { get; set; }
    public int? NUM_ORDEN { get; set; }
}

public class EstructuraCodebarUpdateDto
{
    public string COD_ESTRUCTURA_CODEBAR { get; set; } = string.Empty;
    public string? DES_ESTRUCTURA_CODEBAR { get; set; }
    public int? VAL_LONGITUD { get; set; }
    public int? NUM_ORDEN { get; set; }
}
