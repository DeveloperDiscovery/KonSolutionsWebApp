using KonSolutions.Agente.Api;
using KonSolutions.Agente.Configuracion;
using KonSolutions.Agente.Emision;
using KonSolutions.Agente.Util;
using KONSolutions.Shared.Models.Rfid;
using KONSolutions.Shared.Rfid.Agente;

namespace KonSolutions.Agente.Servicio;

/// <summary>El servicio de la planta. Se queda corriendo, pregunta a la API si hay un trabajo de impresión para su impresora (los que se piden con el botón Imprimir de la web),
/// lo toma, imprime las etiquetas de la emisión con el mismo flujo de <c>emitir</c> y va informando cuántas lleva. Reglas:
///  · un trabajo lo toma un solo servicio a la vez (la API se encarga);
///  · mientras imprime manda un LATIDO cada <see cref="ServicioOpciones.LatidoSegundos"/>: así la web sabe que sigue vivo, y así se entera de que piden CANCELAR;
///  · cancelar un trabajo (o detener el servicio) es una parada SUAVE: la etiqueta que está en la impresora se termina (si se cortara a medias podría quedar impresa sin constancia) y no se
///    empieza otra; solo si el servicio se detiene y la etiqueta no termina en unos 25 s se corta de golpe;
///  · si lo detienen (Ctrl+C, parar el servicio de Windows) en mitad de un trabajo, NO lo cierra: queda "en proceso" y, pasados 3 minutos sin latido, la API lo vuelve a entregar y se retoma
///    donde quedó (lo enrolado y sin grabar se retoma con el mismo chip);
///  · cualquier otro final cierra el trabajo con su estado: T terminado, F con fallos (con el motivo), C cancelado;
///  · si la API no contesta no se cae: avisa una vez y vuelve a intentar.</summary>
public sealed class ServicioImpresion
{
    private readonly AgenteConfig _cfg;
    private readonly IApiRfid _api;
    private readonly Func<RfidEquipoConfig, IRfidAdaptador> _crearAdaptador;
    private readonly IBitacora _log;
    private readonly Func<TimeSpan, CancellationToken, Task> _esperar;
    private readonly TimeSpan _latido, _gracia;
    private string? _ultimoErrorApi;

    public ServicioImpresion(AgenteConfig cfg, IApiRfid api, Func<RfidEquipoConfig, IRfidAdaptador> crearAdaptador, IBitacora log,
        Func<TimeSpan, CancellationToken, Task>? esperar = null, TimeSpan? latido = null, TimeSpan? gracia = null)
    {
        _cfg = cfg; _api = api; _crearAdaptador = crearAdaptador; _log = log;
        _esperar = esperar ?? Task.Delay;
        _latido = latido ?? TimeSpan.FromSeconds(cfg.Servicio.LatidoSegundos);
        _gracia = gracia ?? TimeSpan.FromSeconds(25);
    }

    /// <summary>Atiende las impresoras dadas hasta que lo cancelen.</summary>
    public async Task EjecutarAsync(IReadOnlyList<string> equipos, CancellationToken ct)
    {
        _log.Info($"Servicio de impresión en marcha para {string.Join(", ", equipos)}. Pregunta a la API cada {_cfg.Servicio.PollSegundos} s. Ctrl+C (o parar el servicio) para terminar.");
        while (!ct.IsCancellationRequested)
        {
            bool atendio;
            try { atendio = await UnaVueltaAsync(equipos, ct); }
            catch (OperationCanceledException) { break; }
            if (!atendio)
            {
                try { await _esperar(TimeSpan.FromSeconds(_cfg.Servicio.PollSegundos), ct); }
                catch (OperationCanceledException) { break; }
            }
        }
        _log.Info("Servicio de impresión detenido.");
    }

    /// <summary>Una pasada: pregunta por cada impresora y atiende el trabajo que haya. Devuelve true si atendió alguno.</summary>
    public async Task<bool> UnaVueltaAsync(IReadOnlyList<string> equipos, CancellationToken ct)
    {
        var atendio = false;
        foreach (var equipo in equipos)
        {
            ct.ThrowIfCancellationRequested();
            var r = await _api.ReclamarTrabajoAsync(equipo, ct);
            if (!r.Ok) { AvisarApi(r.Mensaje); continue; }
            if (_ultimoErrorApi is not null) { _log.Info("La API volvió a responder."); _ultimoErrorApi = null; }
            if (r.Datos is null) continue;
            atendio = true;
            await AtenderAsync(r.Datos, ct);
        }
        return atendio;
    }

    /// <summary>Solo se avisa cuando cambia el problema: una API caída no llena el log con un aviso cada pocos segundos.</summary>
    private void AvisarApi(string mensaje)
    {
        if (mensaje == _ultimoErrorApi) return;
        _ultimoErrorApi = mensaje;
        _log.Error("No pude consultar la cola de impresión: " + mensaje + " (sigo intentando).");
    }

    // ───────── un trabajo ─────────

    private sealed class Avance
    {
        public int Impresas, Fallidas;
        public bool PidieronCancelar;
    }

    private async Task AtenderAsync(RfidTrabajoImpresion trabajo, CancellationToken ct)
    {
        _log.Info($"Trabajo {trabajo.NUM_TRABAJO}: emisión {trabajo.NUM_EMISION} en {trabajo.COD_PRODUCTO_EQUIPO}"
                  + (trabajo.CAN_SOLICITADAS is { } n ? $", {n} etiquetas" : ", todas las que falten") + (trabajo.CAN_IMPRESAS > 0 ? $"; ya llevaba {trabajo.CAN_IMPRESAS} impresas" : "") + ".");
        var avance = new Avance { Impresas = trabajo.CAN_IMPRESAS, Fallidas = trabajo.CAN_FALLIDAS };

        // lo que falta para llegar a lo pedido (un trabajo que se retoma ya cuenta lo que llevaba)
        var restantes = trabajo.CAN_SOLICITADAS is { } pedidas ? pedidas - trabajo.CAN_IMPRESAS : 0;
        if (trabajo.CAN_SOLICITADAS is not null && restantes <= 0)
        { await CerrarAsync(trabajo, "T", avance, $"Ya estaban impresas las {trabajo.CAN_SOLICITADAS} etiquetas pedidas.", ct); return; }

        // la impresora: su ficha se lee de la API en cada trabajo, así un cambio en la ERP (IP, plantilla) vale desde el siguiente
        var eq = await _api.EquiposAsync(ct);
        if (!eq.Ok || eq.Datos is null) { await CerrarAsync(trabajo, "F", avance, "No pude leer los equipos de la API: " + eq.Mensaje, ct); return; }
        var elegido = ResolverEquipo.Elegir(eq.Datos, trabajo.COD_PRODUCTO_EQUIPO, null, null);
        if (elegido.Equipo is null) { await CerrarAsync(trabajo, "F", avance, elegido.Error ?? "No encontré la impresora.", ct); return; }
        var config = RfidEquipoConfig.Desde(elegido.Equipo);
        if (!string.IsNullOrWhiteSpace(config.ProblemasFicha)) _log.Aviso("Ficha del equipo: " + config.ProblemasFicha);

        await using var adaptador = _crearAdaptador(config);
        try { await adaptador.ConectarAsync(config, ct); }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { await InformarAsync(trabajo, avance, "El servicio se detuvo antes de empezar; se retoma al volver."); return; }
        catch (Exception ex) { await CerrarAsync(trabajo, "F", avance, $"No se pudo conectar con {(adaptador.Capacidades.Imprime ? "la impresora" : "el lector")}: {ex.Message}", ct); return; }

        // imprimir: el latido corre aparte para que la web vea vivo al servicio aunque la impresora tarde (esperando etiquetas, por ejemplo).
        // blando = parar sin cortar la etiqueta en curso (cancelar el trabajo, detener el servicio); duro = cortar de golpe (el servicio se detuvo y la etiqueta no termina)
        using var blando = new CancellationTokenSource();
        using var duro = new CancellationTokenSource();
        using var finLatidos = new CancellationTokenSource();
        using var alDetener = ct.Register(() => { try { blando.Cancel(); duro.CancelAfter(_gracia); } catch (ObjectDisposedException) { } });
        var cerrojo = new SemaphoreSlim(1, 1);
        var previasImpresas = trabajo.CAN_IMPRESAS; var previasFallidas = trabajo.CAN_FALLIDAS;
        async Task LatirAsync(string? mensaje)
        {
            await cerrojo.WaitAsync(CancellationToken.None);
            try
            {
                var r = await _api.ProgresoTrabajoAsync(trabajo.NUM_TRABAJO, avance.Impresas, avance.Fallidas, mensaje, CancellationToken.None);
                if (!r.Ok) { AvisarApi(r.Mensaje); return; }
                if (r.Datos is { } e && (e.FLG_CANCELAR || e.COD_ESTADO_TRABAJO != "E") && !avance.PidieronCancelar)
                {
                    avance.PidieronCancelar = true;
                    _log.Aviso($"Piden cancelar el trabajo {trabajo.NUM_TRABAJO}: termino la etiqueta que está en la impresora y me detengo.");
                    blando.Cancel();
                }
            }
            finally { cerrojo.Release(); }
        }
        var latidos = Task.Run(async () =>
        {
            try { while (!finLatidos.IsCancellationRequested) { await Task.Delay(_latido, finLatidos.Token); await LatirAsync(null); } }
            catch (OperationCanceledException) { }
        });

        var opciones = new OpcionesEmision
        {
            Reintentos = _cfg.Agente.Reintentos, AnularSiFalla = _cfg.Agente.AnularSiFalla,
            EsperaChipSegundos = _cfg.Servicio.EsperaEtiquetaSegundos, RetiroChipSegundos = _cfg.Agente.RetiroChipSegundos,
            MaxEtiquetas = Math.Max(0, restantes), MaxEsperasSinChip = _cfg.Servicio.EsperasSinEtiqueta,
        };
        var emisor = new EmisorEtiquetas(_api, adaptador, config.CodProducto, opciones, _log, _esperar, async (res, _) =>
        {
            avance.Impresas = previasImpresas + res.Grabadas; avance.Fallidas = previasFallidas + res.Fallidas;
            await LatirAsync($"{avance.Impresas} impresas" + (avance.Fallidas > 0 ? $", {avance.Fallidas} fallidas (repuestas)" : "") + ".");
        }, blando.Token);

        ResumenEmision? resumen = null;
        try { resumen = await emisor.EmitirAsync(trabajo.NUM_EMISION, duro.Token); }
        catch (OperationCanceledException) when (duro.IsCancellationRequested) { /* el servicio se detuvo y la etiqueta no terminó a tiempo */ }
        finally { finLatidos.Cancel(); try { await latidos; } catch { } }
        if (resumen is null)
        {
            await InformarAsync(trabajo, avance, "El servicio se detuvo en mitad de una etiqueta; el trabajo se retoma cuando vuelva.");
            return;
        }
        avance.Impresas = previasImpresas + resumen.Grabadas; avance.Fallidas = previasFallidas + resumen.Fallidas;

        // cómo terminó
        if (ct.IsCancellationRequested && !resumen.Completa)
        {   // detuvieron el servicio: el trabajo queda en proceso y se retoma
            await InformarAsync(trabajo, avance, "El servicio se detuvo; el trabajo se retoma cuando vuelva.");
            return;
        }
        var fallos = avance.Fallidas > previasFallidas ? $" {avance.Fallidas - previasFallidas} etiqueta(s) fallaron y se repusieron." : "";
        if (avance.PidieronCancelar)
            await CerrarAsync(trabajo, "C", avance, $"Cancelado: se imprimieron {avance.Impresas} etiquetas.{fallos}", ct);
        else if (trabajo.CAN_SOLICITADAS is { } meta && avance.Impresas >= meta)
            await CerrarAsync(trabajo, "T", avance, $"Se imprimieron las {avance.Impresas} etiquetas pedidas.{fallos}", ct);
        else if (resumen.Completa)
            await CerrarAsync(trabajo, "T", avance, $"Se imprimieron las etiquetas de la emisión ({avance.Impresas}).{fallos}", ct);
        else
            await CerrarAsync(trabajo, "F", avance, $"{resumen.Motivo ?? "El trabajo no se completó."} Se imprimieron {avance.Impresas}.{fallos}", ct);
    }

    /// <summary>Informa el avance sin cerrar el trabajo (aunque se esté deteniendo el servicio se alcanza a informar: la llamada no se cancela).</summary>
    private async Task InformarAsync(RfidTrabajoImpresion trabajo, Avance avance, string mensaje)
    {
        var r = await _api.ProgresoTrabajoAsync(trabajo.NUM_TRABAJO, avance.Impresas, avance.Fallidas, Corto(mensaje), CancellationToken.None);
        if (!r.Ok) _log.Aviso($"No pude informar el avance del trabajo {trabajo.NUM_TRABAJO}: {r.Mensaje}");
        _log.Aviso($"Trabajo {trabajo.NUM_TRABAJO} queda en proceso: {mensaje}");
    }

    /// <summary>Cierra el trabajo. Si la API no contesta, lo reintenta unas veces; si aun así no puede, el trabajo queda en proceso y la API lo vuelve a entregar (al retomarlo ya no hay nada por imprimir y lo cierra).</summary>
    private async Task CerrarAsync(RfidTrabajoImpresion trabajo, string estado, Avance avance, string mensaje, CancellationToken ct)
    {
        _log.Info($"Trabajo {trabajo.NUM_TRABAJO} {Palabra(estado)}: {mensaje}");
        const int intentos = 4;
        for (int i = 1; i <= intentos; i++)
        {
            var r = await _api.TerminarTrabajoAsync(trabajo.NUM_TRABAJO, estado, avance.Impresas, avance.Fallidas, Corto(mensaje), CancellationToken.None);
            if (r.Ok) return;
            _log.Error($"No pude cerrar el trabajo {trabajo.NUM_TRABAJO} ({i}/{intentos}): {r.Mensaje}");
            if (r.Estado is >= 400 and < 500 and not 401 and not 408 and not 429) return;     // la API lo rechazó (ya estaba cerrado, no existe...): repetir no cambia nada
            if (i < intentos) { try { await _esperar(TimeSpan.FromSeconds(2 * i), ct); } catch (OperationCanceledException) { return; } }
        }
    }

    private static string Palabra(string estado) => estado switch { "T" => "terminado", "C" => "cancelado", _ => "con fallos" };
    private static string Corto(string mensaje) => mensaje.Length <= 500 ? mensaje : mensaje[..499] + "…";
}
