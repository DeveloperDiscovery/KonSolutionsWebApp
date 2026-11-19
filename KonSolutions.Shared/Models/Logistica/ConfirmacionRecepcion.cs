using System.ComponentModel.DataAnnotations;
namespace KONSolutions.Shared.Models.Logistica;

// Confirmación de recepción de transferencias (pantalla Logistica/ConfirmacionRecepcionTransferencias).
// Tienen que existir IGUAL en la API y en Shared: un campo que está en uno solo se pierde al
// serializar, sin error.

/// <summary>Una salida por transferencia que espera que el almacén destino la reciba
/// (PROC_LOGISTICA_MOVIMIENTOS_ALMACEN_SHOWPORRECIBIR).</summary>
public class RecepcionPendiente
{
    public decimal?  NUM_MOVIMIENTO_ALMACEN           { get; set; }
    public DateTime? FEC_MOVIMENTO_ALMACEN            { get; set; }
    public string?   COD_ALMACEN_ORIGEN               { get; set; }
    public string?   DES_ALMACEN_ORIGEN               { get; set; }
    public string?   COD_ALMACEN_DESTINO              { get; set; }
    public string?   DES_ALMACEN_DESTINO              { get; set; }
    public string?   COD_CLASE_MOVIMIENTO             { get; set; }
    public string?   COD_TIPO_MOVIMIENTO              { get; set; }
    public string?   DES_TIPO_MOVIMIENTO              { get; set; }
    public string?   COD_TIPO_MOVIMIENTO_SINCRONIZADO { get; set; }
    public string?   DES_TIPO_MOVIMIENTO_SINCRONIZADO { get; set; }
    public string?   COD_TIPO_COMPROBANTE_PAGO        { get; set; }
    public string?   DES_TIPO_COMPROBANTE_PAGO        { get; set; }
    public string?   NUM_COMPROBANTE_PAGO             { get; set; }
    public string?   DES_OBSERVACION                  { get; set; }
    public int?      COD_TIPO_ESTADO                  { get; set; }
    public int?      COD_ESTADO                       { get; set; }
    public string?   DES_ESTADO                       { get; set; }
    public string?   DES_BACKCOLOR                    { get; set; }
    public string?   DES_FORECOLOR                    { get; set; }
    public int?      CAN_LINEAS                       { get; set; }
    public string?   COD_USUARIO_ENVIO                { get; set; }
    public DateTime? FEC_ENVIO                        { get; set; }
    /// <summary>El rol tiene en el almacén destino el tipo de ingreso (el sincronizado). Sin él
    /// la salida se ve, pero no se puede confirmar.</summary>
    public bool      FLG_TIPO_INGRESO_HABILITADO      { get; set; }
}

/// <summary>Una línea de la salida (PROC_LOGISTICA_MOVIMIENTOS_ALMACEN_SHOWPORRECIBIR_DETALLE).
/// COD_UBICACION_DESTINO es la que ya trae la línea, si existe en el almacén destino.</summary>
public class RecepcionLinea
{
    public decimal? NUM_MOVIMIENTO_ALMACEN       { get; set; }
    public int?     NUM_SECUENCIA                { get; set; }
    public string?  COD_PRODUCTO                 { get; set; }
    public string?  DES_PRODUCTO                 { get; set; }
    public string?  NUM_LOTE_CONTROL             { get; set; }
    public string?  COD_PROCESO                  { get; set; }
    public int?     NUM_ORDEN_PRODUCCION         { get; set; }
    public short?   NUM_ORDEN_PRODUCCION_ITEM    { get; set; }
    public byte?    NUM_ORDEN_PRODUCCION_UNIDAD  { get; set; }
    public decimal? CAN_PRODUCTO                 { get; set; }
    public string?  COD_UNIDAD_MEDIDA            { get; set; }
    public decimal? CAN_PRODUCTO_SECUNDARIO      { get; set; }
    public string?  COD_UNIDAD_MEDIDA_SECUNDARIO { get; set; }
    public string?  COD_ALMACEN_ORIGEN           { get; set; }
    public string?  COD_UBICACION_ORIGEN         { get; set; }
    public string?  DES_UBICACION_ORIGEN         { get; set; }
    public string?  COD_ALMACEN_DESTINO          { get; set; }
    public string?  COD_UBICACION_DESTINO        { get; set; }
    public string?  DES_UBICACION_DESTINO        { get; set; }
}

/// <summary>El estado RECHAZADO del tipo de estado de la salida y su tipo de motivo
/// (primer resultset de PROC_LOGISTICA_MOVIMIENTOS_ALMACEN_RECHAZO_MOTIVOS), y el tipo de ingreso
/// por anulación de salida (positivo y de anulación) con el que el stock vuelve al origen.
/// CAN_TIPOS_ANULACION: cuántos tipos así tiene la clase (si hay más de uno, el diálogo lo dice).</summary>
public class RecepcionEstadoRechazo
{
    public int?    COD_TIPO_ESTADO               { get; set; }
    public int?    COD_ESTADO_RECHAZADO          { get; set; }
    public string? DES_ESTADO_RECHAZADO          { get; set; }
    public int?    COD_TIPO_MOTIVO               { get; set; }
    public string? DES_TIPO_MOTIVO               { get; set; }
    public string? COD_TIPO_MOVIMIENTO_ANULACION { get; set; }
    public string? DES_TIPO_MOVIMIENTO_ANULACION { get; set; }
    public int?    CAN_TIPOS_ANULACION           { get; set; }
}

/// <summary>Un motivo de rechazo. FLG_EXPLICAR: la observación es obligatoria.</summary>
public class RecepcionMotivo
{
    public int?    COD_TIPO_MOTIVO { get; set; }
    public int?    COD_MOTIVO      { get; set; }
    public string? DES_MOTIVO      { get; set; }
    public bool    FLG_EXPLICAR    { get; set; }
    public bool    FLG_DEFAULT     { get; set; }
}

/// <summary>Lo que necesita el diálogo de rechazo, en el orden de los dos resultsets.</summary>
public class RecepcionMotivosRechazo
{
    public RecepcionEstadoRechazo Estado  { get; set; } = new();
    public List<RecepcionMotivo>  Motivos { get; set; } = new();
}

/// <summary>Una línea al confirmar: la línea de la salida y la ubicación del almacén destino
/// donde entra.</summary>
public class RecepcionConfirmarLinea
{
    public int?    NUM_SECUENCIA         { get; set; }
    public string? COD_UBICACION_DESTINO { get; set; }
}

/// <summary>Lo que manda la pantalla al confirmar (PROC_LOGISTICA_MOVIMIENTOS_ALMACEN_CONFIRMAR_RECEPCION).
/// El rol es el activo de la pantalla: el token trae el del login, y el usuario puede haber
/// cambiado de rol en la barra superior.</summary>
public class RecepcionConfirmarDto
{
    public decimal? NUM_MOVIMIENTO_ALMACEN { get; set; }
    public string?  COD_ROL_USUARIO        { get; set; }
    public List<RecepcionConfirmarLinea> Lineas { get; set; } = new();
    /// <summary>Las etiquetas RFID leídas al recibir (opcional). Se asignan a las líneas del ingreso al confirmar; si el tipo
    /// de ingreso exige lectura (R), el SP no confirma sin ellas.</summary>
    public List<RecepcionConfirmarEtiqueta> Etiquetas { get; set; } = new();
}

/// <summary>Lo que manda el diálogo de rechazo (PROC_LOGISTICA_MOVIMIENTOS_ALMACEN_RECHAZAR_RECEPCION).</summary>
public class RecepcionRechazarDto
{
    public decimal? NUM_MOVIMIENTO_ALMACEN { get; set; }
    public string?  COD_ROL_USUARIO        { get; set; }
    public int?     COD_TIPO_MOTIVO        { get; set; }
    public int?     COD_MOTIVO             { get; set; }
    public string?  DES_OBSERVACION        { get; set; }
}

/// <summary>El movimiento que se generó: el ingreso en el destino (confirmar) o el ingreso por
/// anulación de salida en el origen (rechazar).</summary>
public class RecepcionResultado
{
    public decimal? NUM_MOVIMIENTO_ALMACEN { get; set; }
}

// ── Lectura de etiquetas al recibir (PROC_..._RECEPCION_LECTURA_CONTEXTO / _RECEPCION_LEER) ──────────────────────

/// <summary>Una línea de la salida por recibir, con lo que la pantalla necesita para leer: si su producto se lee por RFID,
/// lo enviado y lo que la salida tiene etiquetado. COD_REQUISITO_LECTURA (N / O / R) es el del tipo de ingreso que se
/// generará, y viene repetido en cada fila.</summary>
public class RecepcionLecturaLinea
{
    public int?     NUM_SECUENCIA                { get; set; }
    public string?  COD_PRODUCTO                 { get; set; }
    public string?  DES_PRODUCTO                 { get; set; }
    public decimal? CAN_PRODUCTO                 { get; set; }
    public string?  COD_UNIDAD_MEDIDA            { get; set; }
    public bool     FLG_RFID                     { get; set; }
    public bool     FLG_CODEBAR                  { get; set; }
    public bool     FLG_MEDICION                 { get; set; }
    public int?     CAN_ETIQUETAS_SALIDA         { get; set; }
    public decimal? CAN_ETIQUETADA_SALIDA        { get; set; }
    public string?  COD_TIPO_MOVIMIENTO_INGRESO  { get; set; }
    public string?  COD_REQUISITO_LECTURA        { get; set; }
}

/// <summary>Lo que ya se leyó en pantalla de una línea de la salida: le sirve al SP para elegir la línea a la que cabe
/// la etiqueta y para no pasarse de lo enviado.</summary>
public class RecepcionLecturaLeida
{
    public int?     NUM_SECUENCIA { get; set; }
    public decimal? CAN_PRODUCTO  { get; set; }
}

/// <summary>Una etiqueta que se acaba de leer al recibir. No graba nada: el ingreso todavía no existe, la lectura queda
/// en la pantalla y se graba al confirmar. CAN_PRODUCTO solo hace falta si el producto se mide y la etiqueta no se leyó
/// en la salida (si falta, el SP responde 50708 y la pantalla pide el peso). FLG_FORZAR reenvía una etiqueta que el control
/// entre movimientos rechazó (50706).</summary>
public class RecepcionLecturaDto
{
    [Required] public decimal NUM_MOVIMIENTO_ALMACEN { get; set; }
    [Required] public string  COD_STICKER_RFID       { get; set; } = string.Empty;
    public decimal? CAN_PRODUCTO { get; set; }
    public bool     FLG_FORZAR   { get; set; }
    public List<RecepcionLecturaLeida> Leidas { get; set; } = new();
}

/// <summary>El resultado de leer una etiqueta al recibir. Rechazada: ErrorNumber trae el número del error del SP
/// (50708 falta el peso · 50706 ya ingresó, se puede forzar · 50717 no es RFID · 50731 no es de la salida · 50732 sobra).</summary>
public class RecepcionLecturaResultado
{
    public decimal? NUM_MOVIMIENTO_ALMACEN { get; set; }
    /// <summary>La línea de la SALIDA a la que pertenece la etiqueta.</summary>
    public int?     NUM_SECUENCIA          { get; set; }
    public string?  COD_PRODUCTO           { get; set; }
    public string?  DES_PRODUCTO           { get; set; }
    public string?  COD_UNIDAD_MEDIDA      { get; set; }
    public bool?    FLG_MEDICION           { get; set; }
    public decimal? CAN_LEIDA              { get; set; }
    public decimal? CAN_ENVIADA            { get; set; }
    /// <summary>True si la etiqueta se leyó al enviar la salida (la línea y el peso son los de esa lectura).</summary>
    public bool?    FLG_EN_SALIDA          { get; set; }
    public string?  DES_ADVERTENCIA        { get; set; }
    public int?     ErrorNumber            { get; set; }
}

/// <summary>Una etiqueta leída al recibir, tal como viaja al confirmar: la línea de la salida donde se leyó.</summary>
public class RecepcionConfirmarEtiqueta
{
    public int?     NUM_SECUENCIA    { get; set; }
    public string?  COD_STICKER_RFID { get; set; }
    public decimal? CAN_PRODUCTO     { get; set; }
    public bool     FLG_FORZAR       { get; set; }
}
