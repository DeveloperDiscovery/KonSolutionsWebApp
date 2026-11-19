using System.Net.Sockets;
using System.Text;

namespace KonSolutions.Agente.Impresora;

/// <summary>Conexión TCP a la impresora (puerto 9100). Un hilo va guardando todo lo que ella manda; el adaptador lo consulta con <see cref="EsperarRespuestaAsync"/>.</summary>
public sealed class ImpresoraTcp : IImpresoraZpl
{
    private TcpClient? _tcp;
    private NetworkStream? _flujo;
    private CancellationTokenSource? _corte;
    private Task? _lector;
    private readonly StringBuilder _entrada = new();
    private readonly object _cerrojo = new();
    private long _recibidos;
    private volatile bool _caida;

    public bool Conectada => _tcp is { Connected: true } && !_caida;

    public async Task ConectarAsync(string direccion, int puerto, int tiempoMs, CancellationToken ct)
    {
        await DesconectarAsync(CancellationToken.None);
        var tcp = new TcpClient { NoDelay = true };
        try
        {
            using var limite = CancellationTokenSource.CreateLinkedTokenSource(ct);
            limite.CancelAfter(tiempoMs);
            try { await tcp.ConnectAsync(direccion, puerto, limite.Token); }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested)
            { throw new IOException($"La impresora {direccion}:{puerto} no contestó en {tiempoMs / 1000.0:0.#} s: ¿está encendida y en la misma red?"); }
            catch (SocketException ex) { throw new IOException($"No me pude conectar a la impresora {direccion}:{puerto}: {ex.Message}", ex); }
        }
        catch { tcp.Dispose(); throw; }

        lock (_cerrojo) { _entrada.Clear(); _recibidos = 0; }
        _caida = false;
        _tcp = tcp; _flujo = tcp.GetStream();
        _corte = new CancellationTokenSource();
        _lector = Task.Run(() => LeerSinFinAsync(_flujo, _corte.Token));
    }

    private async Task LeerSinFinAsync(NetworkStream flujo, CancellationToken ct)
    {
        var buffer = new byte[2048];
        try
        {
            while (!ct.IsCancellationRequested)
            {
                var n = await flujo.ReadAsync(buffer, ct);
                if (n == 0) break;                       // la impresora cerró la conexión
                var texto = Encoding.Latin1.GetString(buffer, 0, n);
                lock (_cerrojo) { _entrada.Append(texto); _recibidos += n; }
            }
        }
        catch { /* conexión caída o cerrada por nosotros */ }
        if (!ct.IsCancellationRequested) _caida = true;
    }

    public async Task EnviarAsync(string texto, CancellationToken ct)
    {
        var flujo = _flujo;
        if (flujo is null || !Conectada) throw new IOException("No hay conexión con la impresora.");
        try
        {
            await flujo.WriteAsync(Encoding.UTF8.GetBytes(texto), ct);
            await flujo.FlushAsync(ct);
        }
        catch (Exception ex) when (ex is IOException or SocketException or ObjectDisposedException)
        { _caida = true; throw new IOException("Se perdió la conexión con la impresora: " + ex.Message, ex); }
    }

    public void LimpiarEntrada() { lock (_cerrojo) _entrada.Clear(); }

    public async Task<string> EsperarRespuestaAsync(Func<string, bool>? completo, int tiempoMs, int silencioMs, CancellationToken ct)
    {
        var limite = DateTime.UtcNow.AddMilliseconds(tiempoMs);
        long ultimoTotal = -1; var ultimoCambio = DateTime.UtcNow;
        while (true)
        {
            ct.ThrowIfCancellationRequested();
            string texto; long total;
            lock (_cerrojo) { texto = _entrada.ToString(); total = _recibidos; }
            if (total != ultimoTotal) { ultimoTotal = total; ultimoCambio = DateTime.UtcNow; }
            var cumple = completo is null ? texto.Length > 0 : completo(texto);
            if (cumple && (DateTime.UtcNow - ultimoCambio).TotalMilliseconds >= silencioMs) return texto;
            if (DateTime.UtcNow >= limite || _caida && !cumple) return texto;
            await Task.Delay(15, ct);
        }
    }

    public async Task DesconectarAsync(CancellationToken ct)
    {
        _corte?.Cancel();
        try { _flujo?.Dispose(); } catch { }
        try { _tcp?.Dispose(); } catch { }
        if (_lector is not null) { try { await _lector.WaitAsync(TimeSpan.FromSeconds(2), CancellationToken.None); } catch { } }
        _corte?.Dispose();
        _tcp = null; _flujo = null; _corte = null; _lector = null;
    }

    public async ValueTask DisposeAsync() => await DesconectarAsync(CancellationToken.None);
}
