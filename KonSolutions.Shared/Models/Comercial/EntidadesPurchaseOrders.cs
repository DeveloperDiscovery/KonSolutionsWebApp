namespace KONSolutions.Shared.Models.Comercial;

/// <summary>COMERCIAL.ENTIDADES_PURCHARSE_ORDERS — la orden de compra que manda el cliente.
///
/// El typo "PURCHARSE" está en el nombre de la tabla y de los SP, no en las clases: acá va
/// escrito bien, que es lo que se lee en la pantalla.</summary>
public class EntidadPurchaseOrder
{
    public decimal? COD_ENTIDAD               { get; set; }
    public string NUM_PURCHASE_ORDER          { get; set; } = "";
    public string? COD_DISENO                 { get; set; }
    public string? DES_DISENO_ENTIDAD         { get; set; }
    public DateTime? FEC_DISENO_ENTIDAD       { get; set; }
    public bool?  FLG_ITERAR_EMBALAJE         { get; set; }
    public bool?  FLG_PURCHASE_ORDER_GENERICA { get; set; }

    // Temporada y colección del cliente — COMERCIAL.ENTIDADES_TEMPORADAS[_COLECCIONES].
    // Las dos admiten NULL: una PO genérica no tiene por qué pertenecer a ninguna.
    //
    // La colección NO existe sin su temporada: su clave es (COD_ENTIDAD, NUM_TEMPORADA,
    // NUM_COLECCION). Colección con temporada en NULL es un estado inválido, y el diálogo
    // lo impide deshabilitando el combo hasta que haya temporada.
    public int?   NUM_TEMPORADA               { get; set; }
    public int?   NUM_COLECCION               { get; set; }

    public int?   COD_TIPO_ESTADO             { get; set; }
    public int?   COD_ESTADO                  { get; set; }
    public string COD_USUARIO_REGISTRO        { get; set; } = "";
    public string COD_ESTACION_REGISTRO       { get; set; } = "";
    public DateTime? FEC_REGISTRO             { get; set; }
    public string COD_USUARIO_ACTUALIZACION   { get; set; } = "";
    public string COD_ESTACION_ACTUALIZACION  { get; set; } = "";
    public DateTime? FEC_ACTUALIZACION        { get; set; }

    // Solo lectura (JOIN de los SP).
    public string? DES_ESTADO          { get; set; }
    public string? DES_BACKCOLOR       { get; set; }
    public string? DES_FORECOLOR       { get; set; }
    public string? DES_NOMBRE_COMPLETO { get; set; }
    public string? DES_COMERCIAL       { get; set; }
    // Descripciones que trae el SHOWALL ampliado por LEFT JOIN. Mientras el SP no las
    // devuelva llegan en NULL y la grilla muestra el número solo.
    public string? DES_TEMPORADA       { get; set; }
    public string? DES_COLECCION       { get; set; }
}

/// <summary>Especificación de despacho: cómo sale embalado. PK (entidad, PO, secuencia).</summary>
public class EntidadPurchaseOrderDespacho
{
    public decimal? COD_ENTIDAD      { get; set; }
    public string NUM_PURCHASE_ORDER { get; set; } = "";
    public int?   NUM_SECUENCIA      { get; set; }
    public decimal? CAN_EMBALAJE     { get; set; }
    public string? COD_PRODUCTO_EMBALAJE_INNERPACK  { get; set; }
    public string? COD_PRODUCTO_EMBALAJE_MASTERPACK { get; set; }
    public string COD_USUARIO_REGISTRO       { get; set; } = "";
    public string COD_ESTACION_REGISTRO      { get; set; } = "";
    public DateTime? FEC_REGISTRO            { get; set; }
    public string COD_USUARIO_ACTUALIZACION  { get; set; } = "";
    public string COD_ESTACION_ACTUALIZACION { get; set; } = "";
    public DateTime? FEC_ACTUALIZACION       { get; set; }

    // Solo lectura (JOIN de los SP).
    public string? DES_DISENO_ENTIDAD { get; set; }
    public string? DES_PRODUCTO       { get; set; }
    public string? DES_DETALLADA      { get; set; }

    /// <summary>Fila agregada en la grilla y todavía no grabada. No viaja a la API.</summary>
    public bool EsNueva { get; set; }
}

/// <summary>Especificación técnica de un despacho. PK (entidad, PO, secuencia, especificación):
/// la misma especificación no se repite dentro del mismo despacho.</summary>
public class EntidadPurchaseOrderEspecificacionTecnica
{
    public decimal? COD_ENTIDAD      { get; set; }
    public string NUM_PURCHASE_ORDER { get; set; } = "";
    public int?   NUM_SECUENCIA      { get; set; }
    public string COD_ESPECIFICACION_TECNICA { get; set; } = "";
    public int?   COD_ITEM_TECNICO   { get; set; }
    public bool?  FLG_HEADER         { get; set; }
    public string COD_USUARIO_REGISTRO       { get; set; } = "";
    public string COD_ESTACION_REGISTRO      { get; set; } = "";
    public DateTime? FEC_REGISTRO            { get; set; }
    public string COD_USUARIO_ACTUALIZACION  { get; set; } = "";
    public string COD_ESTACION_ACTUALIZACION { get; set; } = "";
    public DateTime? FEC_ACTUALIZACION       { get; set; }

    /// <summary>Fila agregada y todavía no grabada. Mientras es true, la especificación se
    /// elige con combo: una vez grabada forma parte de la clave y ya no se cambia.</summary>
    public bool EsNueva { get; set; }
}
