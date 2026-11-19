namespace KONSolutions.Shared.Models.Logistica;

/// <summary>LOGISTICA.PRODUCTOS_ALMACENES — parámetros de reposición de un producto en un
/// almacén puntual (stock mínimo/máximo, días de cobertura, cantidad a requerir y si el
/// requerimiento se genera solo). La clave es el par COD_PRODUCTO + COD_ALMACEN.
///
/// Los SP siempre entran por producto (PROC_LOGISTICA_PRODUCTOS_ALMACENES_SHOWALL exige
/// @COD_PRODUCTO), así que la pantalla es una subgrilla de la ficha del producto y no un
/// catálogo suelto.</summary>
public class ProductoAlmacen
{
    public string COD_PRODUCTO { get; set; } = "";
    public string COD_ALMACEN { get; set; } = "";
    public decimal? CAN_STOCK_MINIMO { get; set; }
    public decimal? CAN_STOCK_MAXIMO { get; set; }
    public int? CAN_LEAD_TIME_DIAS { get; set; }
    /// <summary>Consumo diario. El nombre lleva el error de tipeo que tiene la columna en la
    /// BD (CONUSMO, no CONSUMO): el JSON se mapea por nombre exacto contra lo que devuelve
    /// el SP, así que corregirlo acá dejaría el campo siempre vacío.</summary>
    public decimal? CAN_CONUSMO_DIARIO { get; set; }
    public decimal? CAN_STOCK_REQUERIR { get; set; }
    /// <summary>Requerimiento generado para este producto/almacén. De solo lectura en
    /// pantalla —lo escribe PROC_..._REQUERIMIENTO—, pero sí viaja en el guardado: el SP lo
    /// exige como parámetro, así que se reenvía tal cual vino para no pisarlo con NULL.</summary>
    public int? NUM_REQUERIMIENTO_STOCK { get; set; }
    /// <summary>Estado del REQUERIMIENTO generado (JOIN a REQUERIMIENTOS_STOCK y a
    /// ESTANDAR.ESTADOS), no de esta fila: PRODUCTOS_ALMACENES no tiene estado propio.
    /// Los colores salen configurados de la BD, igual que en el resto de las grillas.</summary>
    public int? COD_ESTADO { get; set; }
    public string? DES_ESTADO { get; set; }
    public string? DES_BACKCOLOR { get; set; }
    public string? DES_FORECOLOR { get; set; }
    public bool? FLG_REQUERIMIENTO_AUTOMATICO { get; set; }

    // Desnormalizados del SP (JOIN a ALMACENES y a PRODUCTOS).
    public string? DES_ABREVIADA { get; set; }
    public string? DES_ALMACEN { get; set; }
    public string? DES_POLIGONO { get; set; }
    public string? DES_DETALLADA { get; set; }
    public string? DES_PRODUCTO { get; set; }

    public string? COD_USUARIO_REGISTRO { get; set; }
    public string? COD_ESTACION_REGISTRO { get; set; }
    public DateTime? FEC_REGISTRO { get; set; }
    public string? COD_USUARIO_ACTUALIZACION { get; set; }
    public string? COD_ESTACION_ACTUALIZACION { get; set; }
    public DateTime? FEC_ACTUALIZACION { get; set; }
}

/// <summary>Cuerpo de POST/PUT. Los campos de auditoría no van: el controlador los toma del
/// JWT y de la IP (GetCodUsuario/GetEstacion), para que nadie pueda grabar a nombre de otro
/// armando el request a mano.</summary>
public class ProductoAlmacenSaveDto
{
    public string COD_PRODUCTO { get; set; } = "";
    public string COD_ALMACEN { get; set; } = "";
    public decimal? CAN_STOCK_MINIMO { get; set; }
    public decimal? CAN_STOCK_MAXIMO { get; set; }
    public int? CAN_LEAD_TIME_DIAS { get; set; }
    public decimal? CAN_CONUSMO_DIARIO { get; set; }
    public decimal? CAN_STOCK_REQUERIR { get; set; }
    public int? NUM_REQUERIMIENTO_STOCK { get; set; }
    public bool? FLG_REQUERIMIENTO_AUTOMATICO { get; set; }
}
