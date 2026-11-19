namespace KONSolutions.Shared.Models.Entorno;

/// <summary>ENTORNO.APLICACIONES_OPCIONES_APROBACIONES — un nivel de la cadena de aprobación de
/// una opción de menú. Espejo del modelo de la API.</summary>
public class AplicacionOpcionAprobacion
{
    public string COD_APLICACION        { get; set; } = "";
    public string COD_OPCION_APLICACION { get; set; } = "";
    public int?   NUM_SECUENCIA         { get; set; }
    public int?   COD_AREA              { get; set; }
    public int?   COD_CARGO             { get; set; }
    public string? COD_MONEDA           { get; set; }
    public decimal? VAL_MONTO_MINIMO    { get; set; }
    public decimal? VAL_MONTO_MAXIMO    { get; set; }
    public bool?  FLG_HABILITADO        { get; set; }
    public bool?  FLG_APROBADOR         { get; set; }
    public bool?  FLG_NOTIFICADO        { get; set; }
    public bool?  FLG_APROBADOR_ULTIMO  { get; set; }
    public int?   NUM_ORDEN_APROBACION  { get; set; }
    public string COD_USUARIO_REGISTRO  { get; set; } = "";
    public string COD_ESTACION_REGISTRO { get; set; } = "";
    public DateTime? FEC_REGISTRO       { get; set; }
    public string COD_USUARIO_ACTUALIZACION  { get; set; } = "";
    public string COD_ESTACION_ACTUALIZACION { get; set; } = "";
    public DateTime? FEC_ACTUALIZACION   { get; set; }

    /// <summary>Fila agregada en la grilla y todavía no grabada. Mientras es true no tiene
    /// NUM_SECUENCIA —lo asigna el SP al grabar— así que se manda por POST y no por PUT.
    /// No viaja a la API.</summary>
    public bool EsNueva { get; set; }
}

/// <summary>Cuántos niveles tiene cada opción. Un solo viaje para toda la grilla de opciones:
/// el SHOWALL exige aplicación + opción, así que sin esto habría que pedir una consulta por
/// cada fila del menú.</summary>
public class AplicacionOpcionAprobacionResumen
{
    public string COD_APLICACION        { get; set; } = "";
    public string COD_OPCION_APLICACION { get; set; } = "";
    public int    CAN_NIVELES           { get; set; }
    public int    CAN_APROBADORES       { get; set; }
    public int    CAN_NOTIFICADOS       { get; set; }
}
