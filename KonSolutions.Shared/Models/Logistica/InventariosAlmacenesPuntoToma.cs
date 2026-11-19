using System.ComponentModel.DataAnnotations;
namespace KONSolutions.Shared.Models.Logistica;

/// <summary>LOGISTICA.INVENTARIOS_ALMACENES_PUNTO_TOMA — puntos de conteo dentro de un
/// almacén de un inventario puntual. Clave compuesta NUM_INVENTARIO + COD_ALMACEN +
/// NUM_PUNTO_TOMA_INVENTARIO.
///
/// COLISIÓN DE NOMBRES: SHOWALL/SHOWBYID/SHOWCOMBOX/SEARCH devuelven DOS columnas
/// "DES_OBSERVACION" (la propia del punto de toma y la del INVENTARIOS_ALMACENES padre). La
/// API ya resuelve esa ambigüedad leyendo por ordinal (ver comentario en el Domain Model del
/// backend) y expone la del padre como DES_OBSERVACION_INVENTARIO — este modelo espejo replica
/// esos dos nombres tal cual llegan del JSON.
/// Los campos DES_* al final son de solo lectura: los resuelven los JOIN de los SP.</summary>
public class InventariosAlmacenesPuntoToma
{
    // Auditoria (la muestra PanelAuditoria en el dialogo)
    public string? COD_USUARIO_REGISTRO { get; set; }
    public string? COD_ESTACION_REGISTRO { get; set; }
    public DateTime? FEC_REGISTRO { get; set; }
    public string? COD_USUARIO_ACTUALIZACION { get; set; }
    public string? COD_ESTACION_ACTUALIZACION { get; set; }
    public DateTime? FEC_ACTUALIZACION { get; set; }
    public int? NUM_INVENTARIO { get; set; }
    public string COD_ALMACEN { get; set; } = string.Empty;
    public byte? NUM_PUNTO_TOMA_INVENTARIO { get; set; }
    public decimal? COD_ENTIDAD_RESPONSABLE { get; set; }
    public DateTime? FEC_INICIO_PROGRAMADA { get; set; }
    public DateTime? FEC_FINAL_PROGRAMADA { get; set; }
    public DateTime? FEC_INICIO_EFECTUADA { get; set; }
    public DateTime? FEC_FINAL_EFECTUADA { get; set; }
    public string? DES_OBSERVACION { get; set; }
    public bool? FLG_READER_RFID { get; set; }
    public bool? FLG_READER_CODEBAR { get; set; }
    public int? COD_TIPO_ESTADO { get; set; }
    public int? COD_ESTADO { get; set; }

    public string? DES_BACKCOLOR { get; set; }
    public string? DES_ESTADO { get; set; }
    public string? DES_FORECOLOR { get; set; }
    public string? DES_COMERCIAL { get; set; }
    public string? DES_MATERNO { get; set; }
    public string? DES_NOMBRE { get; set; }
    public string? DES_NOMBRE_COMPLETO { get; set; }
    public string? DES_NOMBRE2 { get; set; }
    public string? DES_PATERNO { get; set; }
    public string? DES_PREFIJO_USUARIO_COMERCIAL { get; set; }
    /// <summary>DES_OBSERVACION del INVENTARIOS_ALMACENES padre — ver comentario de colisión arriba.</summary>
    public string? DES_OBSERVACION_INVENTARIO { get; set; }
}
