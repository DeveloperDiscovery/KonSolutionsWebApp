using System.ComponentModel.DataAnnotations;
namespace KONSolutions.Shared.Models.Logistica;

/// <summary>LOGISTICA.ALMACENES_CIERRE — un cierre de UN almacén para un periodo (FEC_DESDE a FEC_HASTA, ambos inclusive).
/// Mientras FLG_CERRADO sea verdadero, el almacén no admite movimientos, ajustes ni transferencias con fecha dentro del
/// periodo (ni operaciones hechas HOY si hoy cae dentro). Reabrir no borra: deja FLG_CERRADO en falso con quién, cuándo y
/// por qué. PK: NUM_CIERRE_ALMACEN.</summary>
public class AlmacenesCierre
{
    public int NUM_CIERRE_ALMACEN { get; set; }
    public string COD_ALMACEN { get; set; } = "";
    public string? DES_ALMACEN { get; set; }
    public DateTime? FEC_DESDE { get; set; }
    public DateTime? FEC_HASTA { get; set; }
    /// <summary>Días que abarca el cierre (lo calcula el SP).</summary>
    public int NUM_DIAS { get; set; }
    public bool FLG_CERRADO { get; set; }
    /// <summary>CERRADO o REABIERTO.</summary>
    public string? DES_ESTADO { get; set; }
    /// <summary>Cierre vigente cuyo periodo incluye el día de hoy: el almacén no opera ahora mismo.</summary>
    public bool FLG_EN_CURSO { get; set; }
    public string? DES_OBSERVACION { get; set; }
    public string? COD_USUARIO_REAPERTURA { get; set; }
    public string? COD_ESTACION_REAPERTURA { get; set; }
    public DateTime? FEC_REAPERTURA { get; set; }
    public string? DES_MOTIVO_REAPERTURA { get; set; }
    public string? COD_USUARIO_REGISTRO { get; set; }
    public string? COD_ESTACION_REGISTRO { get; set; }
    public DateTime? FEC_REGISTRO { get; set; }
    public string? COD_USUARIO_ACTUALIZACION { get; set; }
    public string? COD_ESTACION_ACTUALIZACION { get; set; }
    public DateTime? FEC_ACTUALIZACION { get; set; }
}

/// <summary>Cierra uno o varios almacenes con el mismo periodo (todo o nada).</summary>
public class AlmacenesCierreInsertDto
{
    public List<string> COD_ALMACENES { get; set; } = new();
    [Required(ErrorMessage = "Indica desde qué fecha se cierra.")] public DateTime? FEC_DESDE { get; set; }
    [Required(ErrorMessage = "Indica hasta qué fecha se cierra.")] public DateTime? FEC_HASTA { get; set; }
    [StringLength(1000)] public string? DES_OBSERVACION { get; set; }
}

/// <summary>Cambia el periodo o la observación de un cierre vigente.</summary>
public class AlmacenesCierreUpdateDto
{
    public int NUM_CIERRE_ALMACEN { get; set; }
    [Required(ErrorMessage = "Indica desde qué fecha se cierra.")] public DateTime? FEC_DESDE { get; set; }
    [Required(ErrorMessage = "Indica hasta qué fecha se cierra.")] public DateTime? FEC_HASTA { get; set; }
    [StringLength(1000)] public string? DES_OBSERVACION { get; set; }
}

/// <summary>El motivo es obligatorio.</summary>
public class AlmacenesCierreReabrirDto
{
    [Required(ErrorMessage = "Indica el motivo de la reapertura."), StringLength(1000)] public string DES_MOTIVO_REAPERTURA { get; set; } = "";
}
