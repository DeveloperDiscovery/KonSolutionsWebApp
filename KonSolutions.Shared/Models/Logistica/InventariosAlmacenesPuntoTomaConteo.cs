namespace KONSolutions.Shared.Models.Logistica;

/// <summary>LOGISTICA.INVENTARIOS_ALMACENES_PUNTO_TOMA_CONTEO — líneas de conteo tomadas
/// dentro de un punto de conteo. Clave compuesta NUM_INVENTARIO + COD_ALMACEN +
/// NUM_PUNTO_TOMA_INVENTARIO + NUM_SECUENCIA (automático, ver comentario en el modelo del
/// backend). Sin colisión de columnas en estos SPs, a diferencia del padre.</summary>
public class InventariosAlmacenesPuntoTomaConteo
{
    // Auditoria (la muestra PanelAuditoria en el dialogo)
    public string? COD_USUARIO_ACTUALIZACION { get; set; }
    public string? COD_ESTACION_ACTUALIZACION { get; set; }
    public DateTime? FEC_ACTUALIZACION { get; set; }
    public int? NUM_INVENTARIO { get; set; }
    public string COD_ALMACEN { get; set; } = string.Empty;
    public byte? NUM_PUNTO_TOMA_INVENTARIO { get; set; }
    public int? NUM_SECUENCIA { get; set; }
    public string? COD_UBICACION_ALMACEN { get; set; }
    public string? COD_PRODUCTO { get; set; }
    public string? NUM_LOTE_CONTROL { get; set; }
    public string? NUM_LOTE_CONTROL_ENTIDAD { get; set; }
    public string? COD_PROCESO { get; set; }
    public string? DES_PROCESO { get; set; }
    public int? NUM_ORDEN_PRODUCCION { get; set; }
    public int? NUM_ORDEN_PRODUCCION_ITEM { get; set; }
    public byte? NUM_ORDEN_PRODUCCION_UNIDAD { get; set; }
    public decimal? CAN_PRODUCTO { get; set; }
    public string? COD_UNIDAD_MEDIDA { get; set; }
    public string? COD_USUARIO_REGISTRO { get; set; }
    public string? COD_ESTACION_REGISTRO { get; set; }
    public DateTime? FEC_REGISTRO { get; set; }

    public string? DES_OBSERVACION { get; set; }
    public string? DES_DETALLADA { get; set; }
    public string? DES_PRODUCTO { get; set; }
    public string? DES_POLIGONO { get; set; }
    public string? DES_UBICACION_ALMACEN { get; set; }
    public string? DES_ALMACEN { get; set; }
    /// <summary>"{DES_ALMACEN}/{DES_UBICACION_ALMACEN}" ya armado por el SP (_SEARCH/_SHOWALL)
    /// — se muestra en la grilla justo después de Secuencia.</summary>
    public string? DES_ALMACEN_UBIACION { get; set; }
}

/// <summary>Resultado de PROC_LOGISTICA_INVENTARIOS_ALMACENES_PUNTO_TOMA_READRFIDCODEBAR — ver
/// comentario del modelo homónimo en el backend (Domain/Models/Logistica). FLG_MEDICION=0
/// dispara el guardado automático con Cantidad=1 (ver InventariosAlmacenesPuntoTomaConteoGrid).</summary>
public class InventariosAlmacenesPuntoTomaReadRfidCodebarResult
{
    public string? COD_ALMACEN { get; set; }
    public string? DES_ALMACEN { get; set; }
    public string? COD_UBICACION_ALMACEN { get; set; }
    public string? DES_UBICACION_ALMACEN { get; set; }
    public string? COD_PRODUCTO { get; set; }
    public string? DES_PRODUCTO { get; set; }
    public string? NUM_LOTE_CONTROL { get; set; }
    public string? NUM_LOTE_CONTROL_ENTIDAD { get; set; }
    public string? COD_PROCESO { get; set; }
    public string? DES_PROCESO { get; set; }
    public int? NUM_ORDEN_PRODUCCION { get; set; }
    public int? NUM_ORDEN_PRODUCCION_ITEM { get; set; }
    public byte? NUM_ORDEN_PRODUCCION_UNIDAD { get; set; }
    public string? COD_UNIDAD_MEDIDA { get; set; }
    public string? COD_TIPO_UNIDAD_MEDIDA { get; set; }
    public bool? FLG_MEDICION { get; set; }
}
