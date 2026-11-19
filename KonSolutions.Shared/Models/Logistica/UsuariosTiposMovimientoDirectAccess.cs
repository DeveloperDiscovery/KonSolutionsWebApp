namespace KONSolutions.Shared.Models.Logistica;

/// <summary>LOGISTICA.USUARIOS_TIPOS_MOVIMIENTO_DIRECT_ACCESS — los tipos de movimiento que
/// el usuario logueado marcó como acceso directo en el documento de Movimientos de Almacén.
/// DES_TIPO_MOVIMIENTO e IMG_ICONO vienen del JOIN contra el maestro (LOGISTICA.TIPOS_MOVIMIENTO),
/// sólo en el SHOWALL — SHOWBYID/SEARCH/SHOWCOMBOX no traen el ícono.</summary>
public class UsuariosTiposMovimientoDirectAccess
{
    public string COD_USUARIO { get; set; } = "";
    public string COD_CLASE_MOVIMIENTO { get; set; } = "";
    public string COD_TIPO_MOVIMIENTO { get; set; } = "";
    public string? DES_TIPO_MOVIMIENTO { get; set; }
    public byte[]? IMG_ICONO { get; set; }
}

public class UsuariosTiposMovimientoDirectAccessInsertDto
{
    public string COD_USUARIO { get; set; } = "";
    public string COD_CLASE_MOVIMIENTO { get; set; } = "";
    public string COD_TIPO_MOVIMIENTO { get; set; } = "";
}
