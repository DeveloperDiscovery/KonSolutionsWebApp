namespace KONSolutions.Shared.Models.Entorno;

/// <summary>ENTORNO.ENTIDADES_CONFIGURAR. Clave compuesta DES_TABLE + DES_COLUMN. Se llena
/// automáticamente por el SHOWALL detectando columnas COD_ENTIDAD% de toda la base — solo
/// DES_COMMANDSQL se edita a mano desde la pantalla.</summary>
public class EntidadesConfigurar
{
    public string DES_TABLE { get; set; } = string.Empty;
    public string DES_COLUMN { get; set; } = string.Empty;
    public string? DES_COMMANDSQL { get; set; }
}
