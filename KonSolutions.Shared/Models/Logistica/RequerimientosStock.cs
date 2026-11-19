using System.ComponentModel.DataAnnotations;
namespace KONSolutions.Shared.Models.Logistica;

/// <summary>LOGISTICA.REQUERIMIENTOS_STOCK</summary>
public class RequerimientosStock
{
    public int? NUM_REQUERIMIENTO_STOCK { get; set; }
    public string DES_REQUERIMIENTO_STOCK { get; set; } = string.Empty;
    public DateTime? FEC_REQUERIMIENTO_STOCK { get; set; }
    public string COD_TIPO_REQUERIMIENTO { get; set; } = string.Empty;
    public string COD_CENTRO_COSTO_REQUIRIENTE { get; set; } = string.Empty;
    public decimal? COD_ENTIDAD_REQUIRIENTE { get; set; }
    public string COD_CENTRO_COSTO_REQUIRIENTE_ORIGINAL { get; set; } = string.Empty;
    public decimal? COD_ENTIDAD_REQUIRIENTE_ORIGINAL { get; set; }

    // El SP de alta y el de modificación lo exigen los dos, sin default — es la clave anterior,
    // igual que los dos ORIGINAL de arriba. La pantalla no lo edita (el correlativo no cambia),
    // así que si el cliente no lo trae, el repositorio lo completa con el número actual/recién
    // asignado. Sin este campo en el modelo del cliente, el body nunca lo llevaba y el SP
    // rechazaba el alta con "was not supplied" aunque el repositorio ya lo mandara.
    public int? NUM_REQUERIMIENTO_STOCK_ORIGINAL { get; set; }

    // Descripción del requerimiento de origen, que ahora devuelve el SHOWBYID por su LEFT JOIN
    // contra la misma tabla. Sólo lectura: lo que se graba sigue siendo el número.
    public string? DES_REQUERIMIENTO_STOCK_ORIGINAL { get; set; }

    public DateTime? FEC_ATENCION { get; set; }
    public string DES_OBSERVACION { get; set; } = string.Empty;
    public int? COD_TIPO_ESTADO { get; set; }
    public int? COD_ESTADO { get; set; }
    public string COD_USUARIO_REGISTRO { get; set; } = string.Empty;
    public string COD_ESTACION_REGISTRO { get; set; } = string.Empty;
    /// <summary>Proyecto al que pertenece (opcional).</summary>
    public int? NUM_PROYECTO { get; set; }
    public string? DES_PROYECTO { get; set; }
    public DateTime? FEC_REGISTRO { get; set; }
    public string COD_USUARIO_ACTUALIZACION { get; set; } = string.Empty;
    public string COD_ESTACION_ACTUALIZACION { get; set; } = string.Empty;
    public DateTime? FEC_ACTUALIZACION { get; set; }

    // Vienen resueltas por los JOIN del procedimiento: solo se muestran.
    public string DES_TIPO_REQUERIMIENTO { get; set; } = string.Empty;
    public string DES_CENTRO_COSTO { get; set; } = string.Empty;
    public string DES_NOMBRE_COMPLETO { get; set; } = string.Empty;
    public string DES_COMERCIAL { get; set; } = string.Empty;
    public string DES_PATERNO { get; set; } = string.Empty;
    public string DES_MATERNO { get; set; } = string.Empty;
    public string DES_NOMBRE { get; set; } = string.Empty;
    public string DES_NOMBRE2 { get; set; } = string.Empty;
    public string DES_PREFIJO_USUARIO_COMERCIAL { get; set; } = string.Empty;
    public string DES_ESTADO { get; set; } = string.Empty;
    public string DES_BACKCOLOR { get; set; } = string.Empty;
    public string DES_FORECOLOR { get; set; } = string.Empty;
}

/// <summary>Resultado de PROC_LOGISTICA_REQUERIMIENTOS_STOCK_LOADCOMBOS: Tipos de
/// requerimiento + Centros de costo + Entidades + Lotes + Productos (los cinco combos
/// independientes de RequerimientoDocumento.razor) en un solo viaje. Productos viaja como
/// lista completa —no como ComboboxItem— porque el detalle necesita más datos del maestro
/// (unidad de medida, etc.) al elegir el producto.</summary>
public class RequerimientosStockLoadCombosResult
{
    // Tipos viaja COMPLETO y no como ComboboxItem, por el mismo motivo que Productos: la
    // pantalla necesita el COD_TIPO_ESTADO de cada tipo para armar el combo de Estado —el
    // tipo de estado del requerimiento sale de su Tipo de Requerimiento, no de la tabla.
    public List<TiposRequerimiento> Tipos { get; set; } = new();
    public List<KONSolutions.Shared.Common.ComboboxItem> Centros   { get; set; } = new();
    public List<KONSolutions.Shared.Common.ComboboxItem> Entidades { get; set; } = new();
    public List<KONSolutions.Shared.Common.ComboboxItem> Lotes     { get; set; } = new();
    public List<KONSolutions.Shared.Models.Estandar.Productos> Productos { get; set; } = new();
    public List<KONSolutions.Shared.Common.ComboboxItem> Almacenes { get; set; } = new();
    public List<KONSolutions.Shared.Common.ComboboxItem> Procesos  { get; set; } = new();
}
