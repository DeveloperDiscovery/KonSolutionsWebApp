using System.ComponentModel.DataAnnotations;

namespace KONSolutions.Shared.Models.Comercial;

/// <summary>COMERCIAL.ENTIDADES_INFORMACION_COMERCIAL — condiciones comerciales pactadas con
/// una entidad para un tipo de operación: moneda, condición de pago y forma de pago.
/// PK compuesta (COD_ENTIDAD, COD_CLASE_OPERACION, COD_TIPO_OPERACION), o sea que una misma
/// entidad puede tener condiciones distintas según el tipo de operación.</summary>
public class EntidadesInformacionComercial
{
    public decimal? COD_ENTIDAD { get; set; }
    public string COD_CLASE_OPERACION { get; set; } = "";
    public string COD_TIPO_OPERACION { get; set; } = "";
    public string? COD_MONEDA { get; set; }
    public int? COD_CONDICION_PAGO { get; set; }
    public int? COD_FORMA_PAGO { get; set; }
    public string? COD_USUARIO_REGISTRO { get; set; }
    public string? COD_ESTACION_REGISTRO { get; set; }
    public DateTime? FEC_REGISTRO { get; set; }
    public string? COD_USUARIO_ACTUALIZACION { get; set; }
    public string? COD_ESTACION_ACTUALIZACION { get; set; }
    public DateTime? FEC_ACTUALIZACION { get; set; }

    // Descripciones que ya resuelven los LEFT JOIN de los SP: la grilla muestra texto.
    public string? DES_MONEDA { get; set; }
    public string? DES_CONDICION_PAGO { get; set; }
    public string? DES_FORMA_PAGO { get; set; }
    public string? DES_CLASE_OPERACION { get; set; }
    public string? DES_TIPO_OPERACION { get; set; }
}

public class EntidadesInformacionComercialSaveDto
{
    [Required] public decimal? COD_ENTIDAD { get; set; }
    [Required] public string COD_CLASE_OPERACION { get; set; } = "";
    [Required] public string COD_TIPO_OPERACION { get; set; } = "";
    public string? COD_MONEDA { get; set; }
    public int? COD_CONDICION_PAGO { get; set; }
    public int? COD_FORMA_PAGO { get; set; }
}
