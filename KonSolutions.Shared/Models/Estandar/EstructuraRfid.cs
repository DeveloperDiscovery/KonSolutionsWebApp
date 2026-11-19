namespace KONSolutions.Shared.Models.Estandar;

/// <summary>ESTANDAR.ESTRUCTURA_RFID.</summary>
public class EstructuraRfid
{
    public string COD_ESTRUCTURA_RFID { get; set; } = string.Empty;
    public string? DES_ESTRUCTURA_RFID { get; set; }
    public int? VAL_LONGITUD { get; set; }
    public int? NUM_ORDEN { get; set; }
}

public class EstructuraRfidInsertDto
{
    public string COD_ESTRUCTURA_RFID { get; set; } = string.Empty;
    public string? DES_ESTRUCTURA_RFID { get; set; }
    public int? VAL_LONGITUD { get; set; }
    public int? NUM_ORDEN { get; set; }
}

public class EstructuraRfidUpdateDto
{
    public string COD_ESTRUCTURA_RFID { get; set; } = string.Empty;
    public string? DES_ESTRUCTURA_RFID { get; set; }
    public int? VAL_LONGITUD { get; set; }
    public int? NUM_ORDEN { get; set; }
}
