namespace KonSolutions.Agente.Impresora;

/// <summary>El canal con una impresora que habla ZPL: manda texto y recoge lo que la impresora contesta. Es una interfaz para poder probar el adaptador sin impresora.</summary>
public interface IImpresoraZpl : IAsyncDisposable
{
    bool Conectada { get; }
    Task ConectarAsync(string direccion, int puerto, int tiempoMs, CancellationToken ct);
    Task DesconectarAsync(CancellationToken ct);
    /// <summary>Manda el texto tal cual (ZPL, ~comandos o líneas SGD con su salto de línea).</summary>
    Task EnviarAsync(string texto, CancellationToken ct);
    /// <summary>Tira lo que la impresora haya mandado y nadie leyó: así una respuesta vieja no se confunde con la de la consulta que viene.</summary>
    void LimpiarEntrada();
    /// <summary>Espera hasta que lo recibido cumpla <paramref name="completo"/> (y pasen <paramref name="silencioMs"/> sin que llegue más), o se acabe <paramref name="tiempoMs"/>.
    /// Con <paramref name="completo"/> nulo basta con haber recibido algo. Devuelve lo recibido hasta ese momento; vencer el tiempo NO es un error (se devuelve lo que haya, quizá nada).</summary>
    Task<string> EsperarRespuestaAsync(Func<string, bool>? completo, int tiempoMs, int silencioMs, CancellationToken ct);
}
