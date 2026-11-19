namespace KONSolutions.Shared.Models.Entorno;

public class NotificacionAprobacionFicha
{
    public int      NUM_NOTIFICACION           { get; set; }
    public string?  DES_NOTIFICACION           { get; set; }
    public string?  DES_INFORMACION            { get; set; }
    public DateTime FEC_NOTIFICACION           { get; set; }
    public DateTime? FEC_VENCIMIENTO           { get; set; }
    public string?  DES_NOTIFICADOR            { get; set; }
    public string?  DES_CARGO                  { get; set; }
    public string?  DES_AREA                   { get; set; }
    public string?  DES_CORREO_INSTITUCIONAL   { get; set; }
    public string?  DES_ESTADO                 { get; set; }
    public byte[]?  IMG_FOTO_NOTIFICADOR       { get; set; }
    public byte[]?  IMG_TIPO_NOTIFICACION      { get; set; }
    public byte[]?  IMG_ESTADO                 { get; set; }
    public string?  DES_BACKCOLOR              { get; set; }
    public string?  DES_FORECOLOR              { get; set; }
}

public class NotificacionAprobacionDocumento
{
    public int      NUM_SECUENCIA               { get; set; }
    public string?  DES_TIPO_COMPROBANTE_PAGO    { get; set; }
    public string?  NUM_COMPROBANTE_PAGO         { get; set; }
    public string?  DES_PATHFILE                 { get; set; }
}

public class NotificacionAprobacionHistorial
{
    public byte[]?  IMG_FOTO                     { get; set; }
    public string?  COD_USUARIO                  { get; set; }
    public string?  DES_USUARIO                  { get; set; }
    public string?  DES_ACCION_CORRESPONDIENTE   { get; set; }
    public string?  DES_APROBADOR_FINAL          { get; set; }
    public string?  DES_CARGO                    { get; set; }
    public string?  DES_AREA                     { get; set; }
    public int?     COD_ESTADO                   { get; set; }
    public string?  DES_ESTADO                   { get; set; }
    public string?  DES_BACKCOLOR                { get; set; }
    public string?  DES_FORECOLOR                { get; set; }
    public byte[]?  IMG_ESTADO                   { get; set; }
    public string?  DES_CORREO_INSTITUCIONAL     { get; set; }
    public DateTime? FEC_ACTUALIZACION           { get; set; }
}

public class NotificacionAprobacionEjecucion
{
    public string?  DES_ACCION       { get; set; }
    public int?     COD_TIPO_ESTADO  { get; set; }
    // Ya no viaja: el servidor ejecuta el comando guardado, el cliente solo pide la acción.
    // Siempre llega vacío cuando la ficha se pide con usuario (lo hace la API).
    public string?  DES_COMMANDSQL   { get; set; }
    // APROBAR, RECHAZAR u OBSERVAR: la acción que se pide al servidor al presionar el botón.
    public string?  COD_ACCION       { get; set; }
    public int?     COD_ESTADO       { get; set; }
    public string?  DES_ESTADO       { get; set; }
    public byte[]?  IMG_ESTADO       { get; set; }
    public string?  DES_BACKCOLOR    { get; set; }
    public string?  DES_FORECOLOR    { get; set; }
}

public class NotificacionAprobacionResult
{
    public NotificacionAprobacionFicha?             Ficha       { get; set; }
    public List<NotificacionAprobacionDocumento>    Documentos  { get; set; } = new();
    public List<NotificacionAprobacionHistorial>    Historial   { get; set; } = new();
    public List<NotificacionAprobacionEjecucion>    Ejecucion   { get; set; } = new();
}

// DTO para PROC_ENTORNO_EJECUTAR_SENTENCIAS — ejecuta el DES_COMMANDSQL de una acción
// de notificación (ENTORNO.NOTIFICACIONES_EJECUCION).
public class EjecutarSentenciaDto
{
    public string DES_SENTENCIA { get; set; } = string.Empty;
}

/// <summary>Cuerpo de POST entorno/notificaciones-aprobaciones/{n}/{aprobar|rechazar|observar}. No lleva SQL: solo la justificación.</summary>
public class NotificacionAprobacionAccionDto
{
    public string? DES_OBSERVACION { get; set; }
}
