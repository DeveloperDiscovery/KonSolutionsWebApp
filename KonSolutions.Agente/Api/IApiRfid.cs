using KONSolutions.Shared.Models.Rfid;

namespace KonSolutions.Agente.Api;

/// <summary>Lo que el agente recibe de cada llamada a la API. Ok = la API dijo success; Mensaje = su texto (el error de negocio tal cual, para mostrarlo al operador).</summary>
public sealed record RespuestaApi<T>(bool Ok, T? Datos, string Mensaje, int Estado)
{
    public static RespuestaApi<T> Fallo(string mensaje, int estado = 0) => new(false, default, mensaje, estado);
}

/// <summary>Los endpoints de api/v1/rfid que usa el agente (ver CONTRATO_AGENTE.md). Es una interfaz para poder probar el flujo sin red.</summary>
public interface IApiRfid
{
    /// <summary>GET api/v1/rfid/equipos: los productos-equipo con su ficha ya resuelta.</summary>
    Task<RespuestaApi<List<RfidEquipo>>> EquiposAsync(CancellationToken ct);
    /// <summary>GET api/v1/rfid/emisiones/search para el estado dado (E en proceso, P pendiente).</summary>
    Task<RespuestaApi<List<RfidEmision>>> EmisionesAsync(string codEstadoEmision, CancellationToken ct);
    /// <summary>GET api/v1/rfid/emisiones/{n}</summary>
    Task<RespuestaApi<RfidEmision>> EmisionAsync(int numEmision, CancellationToken ct);
    /// <summary>POST api/v1/rfid/emisiones/{n}/enrolar: el TID del chip recibe su etiqueta, su codigo y el EPC a grabar.</summary>
    Task<RespuestaApi<RfidEnrolado>> EnrolarAsync(int numEmision, string tid, string? codProductoEquipo, CancellationToken ct);
    /// <summary>PUT api/v1/rfid/etiquetas/grabada: constancia de que el EPC quedo grabado (la API compara el EPC leido).</summary>
    Task<RespuestaApi<object>> GrabadaAsync(string tid, string? epcLeido, CancellationToken ct);
    /// <summary>PUT api/v1/rfid/emisiones/{n}/etiquetas/{item}/{etiqueta}/anular</summary>
    Task<RespuestaApi<RfidEtiquetaAnulada>> AnularEtiquetaAsync(int numEmision, int numItem, int numEtiqueta, string motivo, bool reponer, CancellationToken ct);

    // ───────── la cola de impresión (la usa el servicio: agente-rfid servicio) ─────────
    /// <summary>POST api/v1/rfid/impresion/reclamar: el siguiente trabajo de impresión de esta impresora, ya "en proceso". Sin trabajos: éxito con Datos nulo.</summary>
    Task<RespuestaApi<RfidTrabajoImpresion?>> ReclamarTrabajoAsync(string codProductoEquipo, CancellationToken ct);
    /// <summary>PUT api/v1/rfid/impresion/{n}/progreso: cuántas lleva (acumulado) y latido. La respuesta dice si piden cancelar.</summary>
    Task<RespuestaApi<RfidTrabajoEstado>> ProgresoTrabajoAsync(int numTrabajo, int impresas, int fallidas, string? mensaje, CancellationToken ct);
    /// <summary>PUT api/v1/rfid/impresion/{n}/terminar: T terminado · F con fallos · C cancelado.</summary>
    Task<RespuestaApi<object>> TerminarTrabajoAsync(int numTrabajo, string estado, int impresas, int fallidas, string? mensaje, CancellationToken ct);
}
