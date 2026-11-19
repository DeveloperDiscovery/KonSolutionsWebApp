using System.ComponentModel.DataAnnotations;

namespace KONSolutions.Shared.Models.Comercial;

/// <summary>COMERCIAL.ENTIDADES_PRODUCTOS — qué producto vende cada proveedor, con su
/// código y nombre propios, su unidad de atención, su precio y sus tolerancias.
/// PK (COD_ENTIDAD, COD_PRODUCTO): la misma fila se mira desde los dos lados, por eso el
/// módulo tiene dos pestañas y no dos tablas.</summary>
public class EntidadProducto
{
    public decimal? COD_ENTIDAD { get; set; }
    public string COD_PRODUCTO { get; set; } = "";
    public string? COD_PRODUCTO_ENTIDAD { get; set; }
    public string? DES_PRODUCTO_ENTIDAD { get; set; }
    public string? COD_UNIDAD_MEDIDA_ATENCION { get; set; }
    public decimal? FAC_CONVERSION { get; set; }
    public string? COD_MONEDA { get; set; }
    public decimal? VAL_PRECIO_ENTIDAD { get; set; }
    public decimal? TAS_DESCUENTO { get; set; }
    public int? CAN_DIAS_ATENCION { get; set; }
    public bool? FLG_DIAS_HABILES { get; set; }
    public decimal? CAN_TOLERANCIA_EXCESO { get; set; }
    public bool? FLG_TOLERANCIA_EMBALAJE { get; set; }
    public decimal? CAN_TOLERANCIA_DEFICIT { get; set; }
    public decimal? TAS_TOLERANCIA_EXCESO { get; set; }
    public decimal? TAS_TOLERANCIA_DEFICIT { get; set; }
    public string? COD_USUARIO_REGISTRO { get; set; }
    public string? COD_ESTACION_REGISTRO { get; set; }
    public DateTime? FEC_REGISTRO { get; set; }
    public string? COD_USUARIO_ACTUALIZACION { get; set; }
    public string? COD_ESTACION_ACTUALIZACION { get; set; }
    public DateTime? FEC_ACTUALIZACION { get; set; }

    // Descripciones que resuelven los LEFT JOIN del SP.
    public string? DES_UNIDAD_MEDIDA { get; set; }
    public string? DES_MONEDA { get; set; }
    public string? DES_PRODUCTO { get; set; }
    public string? DES_DETALLADA { get; set; }
    /// <summary>Nombre del proveedor. Lo trae el join a ESTANDAR.ENTIDADES que agregamos al
    /// SHOWALL: sin él, la pestaña de proveedores por producto mostraría el código.</summary>
    public string? DES_NOMBRE_COMPLETO { get; set; }
    public string? DES_COMERCIAL { get; set; }
}

public class EntidadProductoSaveDto
{
    [Required] public decimal? COD_ENTIDAD { get; set; }
    [Required] public string COD_PRODUCTO { get; set; } = "";
    public string? COD_PRODUCTO_ENTIDAD { get; set; }
    public string? DES_PRODUCTO_ENTIDAD { get; set; }
    public string? COD_UNIDAD_MEDIDA_ATENCION { get; set; }
    public decimal? FAC_CONVERSION { get; set; }
    public string? COD_MONEDA { get; set; }
    public decimal? VAL_PRECIO_ENTIDAD { get; set; }
    public decimal? TAS_DESCUENTO { get; set; }
    public int? CAN_DIAS_ATENCION { get; set; }
    public bool? FLG_DIAS_HABILES { get; set; }
    public decimal? CAN_TOLERANCIA_EXCESO { get; set; }
    public bool? FLG_TOLERANCIA_EMBALAJE { get; set; }
    public decimal? CAN_TOLERANCIA_DEFICIT { get; set; }
    public decimal? TAS_TOLERANCIA_EXCESO { get; set; }
    public decimal? TAS_TOLERANCIA_DEFICIT { get; set; }
}

/// <summary>COMERCIAL.ENTIDADES_PRODUCTOS_TIPOS_PRECIOS — el precio del producto de ese
/// proveedor para cada tipo de precio Y cada moneda. La PK son las CUATRO columnas: un
/// mismo tipo de precio puede tener un valor en soles y otro en dólares.</summary>
public class EntidadProductoTipoPrecio
{
    public decimal? COD_ENTIDAD { get; set; }
    public string COD_PRODUCTO { get; set; } = "";
    public int? COD_TIPO_PRECIO { get; set; }
    public string COD_MONEDA { get; set; } = "";
    public decimal? VAL_PRECIO { get; set; }
    public string? COD_USUARIO_REGISTRO { get; set; }
    public string? COD_ESTACION_REGISTRO { get; set; }
    public DateTime? FEC_REGISTRO { get; set; }
    public string? COD_USUARIO_ACTUALIZACION { get; set; }
    public string? COD_ESTACION_ACTUALIZACION { get; set; }
    public DateTime? FEC_ACTUALIZACION { get; set; }

    public string? DES_TIPO_PRECIO { get; set; }
    public string? DES_MONEDA { get; set; }
    public string? DES_PRODUCTO_ENTIDAD { get; set; }
    public string? DES_NOMBRE_COMPLETO { get; set; }
    public string? DES_COMERCIAL { get; set; }
}

public class EntidadProductoTipoPrecioSaveDto
{
    [Required] public decimal? COD_ENTIDAD { get; set; }
    [Required] public string COD_PRODUCTO { get; set; } = "";
    [Required] public int? COD_TIPO_PRECIO { get; set; }
    [Required] public string COD_MONEDA { get; set; } = "";
    public decimal? VAL_PRECIO { get; set; }
}
