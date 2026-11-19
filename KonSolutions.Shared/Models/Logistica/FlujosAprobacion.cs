using System.ComponentModel.DataAnnotations;
namespace KONSolutions.Shared.Models.Logistica;

/// <summary>LOGISTICA.FLUJOS_APROBACION — maestro del flujo de aprobación.
/// NUM_FLUJO_APROBACION lo genera la BD (IDENTITY): al crear viaja nulo.</summary>
public class FlujoAprobacion
{
    public short? NUM_FLUJO_APROBACION { get; set; }
    [Required] public string DES_FLUJO_APROBACION { get; set; } = "";
    public int? COD_TIPO_ESTADO { get; set; }
    public int? COD_ESTADO { get; set; }
    public string COD_USUARIO_REGISTRO { get; set; } = "";
    public string COD_ESTACION_REGISTRO { get; set; } = "";
    public DateTime? FEC_REGISTRO { get; set; }
    public string COD_USUARIO_ACTUALIZACION { get; set; } = "";
    public string COD_ESTACION_ACTUALIZACION { get; set; } = "";
    public DateTime? FEC_ACTUALIZACION { get; set; }

    // Solo lectura (JOIN a ESTANDAR.ESTADOS).
    public string? DES_ESTADO { get; set; }
    public string? DES_BACKCOLOR { get; set; }
    public string? DES_FORECOLOR { get; set; }
}

/// <summary>LOGISTICA.FLUJOS_APROBACION_AREA_CARGO — niveles de aprobación del flujo.
/// PK compuesta (NUM_FLUJO_APROBACION, COD_AREA, COD_CARGO).</summary>
public class FlujoAprobacionAreaCargo
{
    public short? NUM_FLUJO_APROBACION { get; set; }
    public int? COD_AREA { get; set; }
    public int? COD_CARGO { get; set; }
    public int? NUM_ORDEN_APROBACION { get; set; }
    public string? COD_MONEDA { get; set; }
    public decimal? VAL_MONTO_MINIMO { get; set; }
    public decimal? VAL_MONTO_MAXIMO { get; set; }
    public bool? FLG_ULTIMO { get; set; }
    public bool? FLG_HABILITADO { get; set; }
    public string COD_USUARIO_REGISTRO { get; set; } = "";
    public string COD_ESTACION_REGISTRO { get; set; } = "";
    public DateTime? FEC_REGISTRO { get; set; }
    public string COD_USUARIO_ACTUALIZACION { get; set; } = "";
    public string COD_ESTACION_ACTUALIZACION { get; set; } = "";
    public DateTime? FEC_ACTUALIZACION { get; set; }

    // Solo lectura (JOIN de los SP).
    public string? DES_AREA { get; set; }
    public string? DES_CARGO { get; set; }
    public string? DES_MONEDA { get; set; }

    /// <summary>Fila agregada en la grilla y todavía no grabada. Mientras es true, Área y
    /// Cargo se editan con combos; una vez grabada forman la clave y ya no se cambian.
    /// No viaja a la API.</summary>
    public bool EsNueva { get; set; }
}

/// <summary>LOGISTICA.FLUJOS_APROBACION_TIPOS_OPERACION — tipos de operación que disparan
/// el flujo. Tabla de asignación: se agrega y se quita, no se modifica.</summary>
public class FlujoAprobacionTipoOperacion
{
    public short? NUM_FLUJO_APROBACION { get; set; }
    public string COD_CLASE_OPERACION { get; set; } = "";
    public string COD_TIPO_OPERACION { get; set; } = "";
    public string COD_USUARIO_REGISTRO { get; set; } = "";
    public string COD_ESTACION_REGISTRO { get; set; } = "";
    public DateTime? FEC_REGISTRO { get; set; }

    // Solo lectura (JOIN a CLASES_OPERACION / TIPOS_OPERACION).
    public string? DES_CLASE_OPERACION { get; set; }
    public string? DES_TIPO_OPERACION { get; set; }

    /// <summary>Fila agregada en la grilla y todavía no grabada: los tres campos son la clave,
    /// así que solo se eligen mientras la fila es nueva. No viaja a la API.</summary>
    public bool EsNueva { get; set; }
}
