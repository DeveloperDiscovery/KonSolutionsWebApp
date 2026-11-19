namespace KONSolutions.Shared.Models.Logistica;

/// <summary>Un nodo del mapa de trazabilidad: un documento (requerimiento, reserva, pedido
/// comercial, movimiento de almacén) o un almacén. Lo devuelven PROC_LOGISTICA_TRAZABILIDAD_NODO y
/// PROC_LOGISTICA_TRAZABILIDAD_VECINOS.
///
/// TIPO_NODO: REQ, RES, PC, MOV o ALM. CLAVE: el número del documento, salvo el pedido comercial
/// ("CLASE|TIPO|NUM", su clave es compuesta) y el almacén (su código).</summary>
public class TrazabilidadNodo
{
    public string?   TIPO_NODO      { get; set; }
    public string?   CLAVE          { get; set; }
    public string?   DES_TIPO_NODO  { get; set; }
    public string?   NUM_DOCUMENTO  { get; set; }
    public DateTime? FEC_DOCUMENTO  { get; set; }
    public string?   DES_DOCUMENTO  { get; set; }
    /// <summary>Sólo en movimientos: ingreso (true) o salida (false).</summary>
    public bool?     FLG_POSITIVO   { get; set; }
    public string?   DES_ESTADO     { get; set; }
    public string?   DES_BACKCOLOR  { get; set; }
    public string?   DES_FORECOLOR  { get; set; }
    /// <summary>Franja de la pantalla: REQ, RES, MOV (movimientos y almacenes) o, en un pedido
    /// comercial, su clase de operación (PRFRM, ORDCM, CMPRS...). Es el COD_FRANJA de
    /// TrazabilidadFranja.</summary>
    public string?   CAT_NODO       { get; set; }

    /// <summary>Sólo al listar vecinos: "A" atrás (de dónde viene) o "D" adelante (qué generó), y
    /// el nombre de la relación con el nodo desde el que se expandió.</summary>
    public string?   SENTIDO        { get; set; }
    public string?   DES_RELACION   { get; set; }

    /// <summary>Cuántos vecinos tiene este nodo en cada sentido: dicen si se puede expandir.</summary>
    public int?      CAN_ATRAS      { get; set; }
    public int?      CAN_ADELANTE   { get; set; }
}

/// <summary>Un dato de la entidad, como etiqueta y valor ya formateado: cada tipo de nodo tiene
/// campos distintos y así el panel no necesita un modelo por tipo.</summary>
public class TrazabilidadCampo
{
    public int?    NUM_ORDEN    { get; set; }
    public string? DES_ETIQUETA { get; set; }
    public string? DES_VALOR    { get; set; }
}

/// <summary>Una línea de la entidad (detalle del requerimiento, la reserva, el pedido o el
/// movimiento), con las mismas columnas para todos los tipos.</summary>
public class TrazabilidadLinea
{
    public int?     NUM_SECUENCIA     { get; set; }
    public string?  COD_PRODUCTO      { get; set; }
    public string?  DES_PRODUCTO      { get; set; }
    public decimal? CAN_PRODUCTO      { get; set; }
    public string?  COD_UNIDAD_MEDIDA { get; set; }
    public decimal? CAN_ATENDIDA      { get; set; }
    public string?  DES_DETALLE       { get; set; }
}

/// <summary>Lo que muestra el panel al hacer clic en un nodo (PROC_LOGISTICA_TRAZABILIDAD_ENTIDAD).</summary>
public class TrazabilidadEntidad
{
    public List<TrazabilidadCampo> Campos { get; set; } = new();
    public List<TrazabilidadLinea> Lineas { get; set; } = new();
}

/// <summary>Una franja del mapa (PROC_LOGISTICA_TRAZABILIDAD_FRANJAS): requerimientos, reservas, una
/// por cada clase de operación y almacenes, en el orden en que se dibujan.</summary>
public class TrazabilidadFranja
{
    public string? COD_FRANJA { get; set; }
    public string? DES_FRANJA { get; set; }
    public int?    NUM_ORDEN  { get; set; }
}
