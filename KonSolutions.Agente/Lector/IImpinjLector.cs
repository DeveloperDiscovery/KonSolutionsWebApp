namespace KonSolutions.Agente.Lector;

/// <summary>Como hay que conectarse al lector. Sale de la ficha tecnica del equipo (direccion, puerto, puertos de antena, potencia) y de appsettings.json.</summary>
public sealed record ImpinjConexion(string Direccion, int Puerto, IReadOnlyList<ushort> Antenas, double PotenciaDbm, bool PedirFastId, int TiempoOperacionMs);

/// <summary>Lo que el lector dice de si mismo al conectarse.</summary>
public sealed record ImpinjInfo(string Modelo, string Serie, string Firmware, int Antenas, bool FastIdDisponible, bool AccesoAEtiquetas);

/// <summary>Un chip visto en el campo del lector. Tid viene si el lector lo informo (FastID); si no, se lee aparte.</summary>
public sealed record EtiquetaVista(string Epc, string? Tid, ushort PcBits);

public sealed record ResultadoEscritura(bool Exito, string? Error);

/// <summary>Lo minimo que el adaptador necesita de un lector Impinj. La implementacion real usa el SDK Octane (<see cref="ImpinjOctaneLector"/>); las pruebas usan un lector falso.
/// Todo en hexadecimal, mayusculas y sin espacios.</summary>
public interface IImpinjLector : IAsyncDisposable
{
    bool Conectado { get; }
    Task<ImpinjInfo> ConectarAsync(ImpinjConexion conexion, CancellationToken ct);
    Task DesconectarAsync(CancellationToken ct);
    /// <summary>Lee el campo durante ese tiempo y devuelve los chips distintos que vio.</summary>
    Task<IReadOnlyList<EtiquetaVista>> InventariarAsync(int milisegundos, CancellationToken ct);
    /// <summary>Lee el banco TID del chip que tiene ese EPC. Null si no respondio.</summary>
    Task<string?> LeerTidAsync(string epc, CancellationToken ct);
    /// <summary>Graba el EPC en el chip con ese TID (el TID es el filtro: no se toca ningun otro chip). pcBitsActual sirve para ajustar el largo del EPC si hace falta.</summary>
    Task<ResultadoEscritura> EscribirEpcAsync(string tid, string epcNuevo, ushort pcBitsActual, CancellationToken ct);
}
