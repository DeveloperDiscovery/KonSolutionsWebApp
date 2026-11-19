using KonSolutions.Agente.Api;
using KonSolutions.Agente.Util;
using KONSolutions.Shared.Models.Rfid;
using KONSolutions.Shared.Rfid.Agente;

namespace KonSolutions.Agente.Emision;

public sealed class OpcionesEmision
{
    /// <summary>Intentos EXTRA de grabar el EPC antes de darlo por fallido (con 3: hasta 4 intentos en total).</summary>
    public int Reintentos { get; init; } = 3;
    /// <summary>Si la grabacion falla del todo: anular la etiqueta (el chip queda libre) y reponerla.</summary>
    public bool AnularSiFalla { get; init; } = true;
    public int EsperaChipSegundos { get; init; } = 120;
    public int RetiroChipSegundos { get; init; } = 30;
    /// <summary>Cuantas etiquetas procesar como maximo en esta corrida (0 = todas las pendientes).</summary>
    public int MaxEtiquetas { get; init; }
    /// <summary>Cuántas esperas seguidas sin que aparezca ninguna etiqueta con chip dan la corrida por terminada (0 = esperar siempre, como un operador al lado del lector).
    /// El servicio lo usa: nadie va a poner una etiqueta, así que un rollo vacío tiene que acabar el trabajo.</summary>
    public int MaxEsperasSinChip { get; init; }
    /// <summary>Fallos seguidos que detienen la corrida: algo esta mal en el lector, la potencia o los chips y seguir solo gasta correlativos.</summary>
    public int MaxFallosSeguidos { get; init; } = 5;
}

public sealed class ResumenEmision
{
    public int NumEmision { get; set; }
    public int Grabadas { get; set; }
    /// <summary>Chips que ya estaban enrolados y grabados (se ignoran).</summary>
    public int YaGrabadas { get; set; }
    /// <summary>Chips que la API rechazo (ya enrolados en otra emision, TID invalido...).</summary>
    public int Rechazadas { get; set; }
    public int Fallidas { get; set; }
    /// <summary>De las fallidas, las que se anularon y repusieron.</summary>
    public int Anuladas { get; set; }
    /// <summary>Etiquetas que siguen sin enrolar (PENDIENTES en la API).</summary>
    public int Pendientes { get; set; }
    /// <summary>Etiquetas enroladas cuyo EPC todavia no esta grabado: solo se terminan poniendo otra vez su chip.</summary>
    public int SinGrabar { get; set; }
    /// <summary>No queda nada por enrolar ni por grabar.</summary>
    public bool Completa { get; set; }
    public bool Cancelada { get; set; }
    /// <summary>Por que termino sin completar (nulo si completo o se cancelo).</summary>
    public string? Motivo { get; set; }
    public bool Exitosa => Completa && Fallidas == 0;
}

/// <summary>El flujo de una etiqueta del contrato (CONTRATO_AGENTE.md), repetido hasta que la emision no tenga pendientes:
///   leer el TID del chip  →  enrolar en la API (devuelve el codigo y el EPC)  →  grabar el EPC y leerlo de vuelta  →  constancia a la API;
///   si no se pudo grabar: anular la etiqueta y reponerla.
/// Reintentar es seguro: el mismo TID en la misma emision devuelve lo ya enrolado (FLG_YA_ENROLADA), y si esa etiqueta ya estaba grabada se ignora.
/// Si se corta el programa entre enrolar y grabar, al volver a poner el chip se retoma donde quedo.</summary>
public sealed class EmisorEtiquetas
{
    private readonly IApiRfid _api;
    private readonly IRfidAdaptador _adaptador;
    private readonly string? _codProductoEquipo;
    private readonly OpcionesEmision _opc;
    private readonly IBitacora _log;
    private readonly Func<TimeSpan, CancellationToken, Task> _esperar;
    private readonly Func<ResumenEmision, CancellationToken, Task>? _alProgreso;
    private readonly CancellationToken _detener;
    /// <summary>Una impresora graba al imprimir: el chip va dentro de la etiqueta, no se "retira" y una etiqueta impresa no se puede repetir con el mismo TID.</summary>
    private bool Imprime => _adaptador.Capacidades.Imprime;

    /// <param name="alProgreso">Se llama despues de cada etiqueta procesada, con el resumen hasta ahora (el servicio lo usa para informar el avance del trabajo).</param>
    /// <param name="detener">Pide parar SUAVEMENTE: no se empieza otra etiqueta y se deja de esperar una nueva, pero la etiqueta que ya esta en curso (enrolada, imprimiendose) se termina.
    /// Cancelar de golpe con el <c>ct</c> de EmitirAsync a media etiqueta podria dejar una etiqueta impresa sin constancia, por eso el servicio usa esto para "cancelar el trabajo".</param>
    public EmisorEtiquetas(IApiRfid api, IRfidAdaptador adaptador, string? codProductoEquipo, OpcionesEmision opciones, IBitacora log, Func<TimeSpan, CancellationToken, Task>? esperar = null,
        Func<ResumenEmision, CancellationToken, Task>? alProgreso = null, CancellationToken detener = default)
    {
        _api = api; _adaptador = adaptador; _codProductoEquipo = codProductoEquipo; _opc = opciones; _log = log;
        _esperar = esperar ?? Task.Delay; _alProgreso = alProgreso; _detener = detener;
    }

    public async Task<ResumenEmision> EmitirAsync(int numEmision, CancellationToken ct)
    {
        var resumen = new ResumenEmision { NumEmision = numEmision };
        var em = await _api.EmisionAsync(numEmision, ct);
        if (!em.Ok || em.Datos is null) { resumen.Motivo = $"No pude leer la emisión {numEmision}: {em.Mensaje}"; _log.Error(resumen.Motivo); return resumen; }
        var emision = em.Datos;
        if (emision.FLG_ANULADA) { resumen.Motivo = $"La emisión {numEmision} está anulada."; _log.Error(resumen.Motivo); return resumen; }
        if (!string.IsNullOrWhiteSpace(emision.COD_PRODUCTO_EQUIPO) && !string.IsNullOrWhiteSpace(_codProductoEquipo)
            && !string.Equals(emision.COD_PRODUCTO_EQUIPO.Trim(), _codProductoEquipo.Trim(), StringComparison.OrdinalIgnoreCase))
            _log.Aviso($"La emisión {numEmision} está asignada al equipo {emision.COD_PRODUCTO_EQUIPO}; las etiquetas que grabe esta estación quedarán con el equipo {_codProductoEquipo}.");

        Anotar(resumen, emision);
        if (Faltan(resumen) == 0) { resumen.Completa = true; _log.Info($"La emisión {numEmision} no tiene etiquetas por enrolar ni por grabar."); return resumen; }
        _log.Info($"Emisión {numEmision}: faltan {resumen.Pendientes} por enrolar y {resumen.SinGrabar} por grabar. Ctrl+C para terminar.");

        string? ultimoTid = null;     // el chip que acabo de procesar: mientras siga en el campo no se vuelve a tocar
        int fallosSeguidos = 0, procesadas = 0, esperasSinChip = 0;
        while (Faltan(resumen) > 0 && !ct.IsCancellationRequested && !_detener.IsCancellationRequested)
        {
            if (_opc.MaxEtiquetas > 0 && procesadas >= _opc.MaxEtiquetas) { resumen.Motivo = $"Se llegó al máximo pedido ({_opc.MaxEtiquetas} etiquetas)."; break; }
            if (fallosSeguidos >= _opc.MaxFallosSeguidos)
            { resumen.Motivo = $"{fallosSeguidos} fallos seguidos: me detengo. Revisa el lector, la potencia de escritura de la ficha y los chips."; _log.Error(resumen.Motivo); break; }

            _log.Info($"Faltan {resumen.Pendientes} por enrolar y {resumen.SinGrabar} por grabar. {(Imprime ? "Deja la siguiente etiqueta con chip sobre la antena de la impresora…" : "Pon el siguiente chip frente a la antena…")}");
            RfidLecturaTid lectura;
            try
            {
                // esperar una etiqueta nueva SI se puede interrumpir con la parada suave (no hay nada a medias); lo que sigue, no
                using var espera = CancellationTokenSource.CreateLinkedTokenSource(ct, _detener);
                lectura = await _adaptador.LeerTidAsync(new RfidLecturaOpciones { TiempoEsperaMs = _opc.EsperaChipSegundos * 1000 }, espera.Token);
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex) { resumen.Motivo = "El lector falló al leer: " + ex.Message; _log.Error(resumen.Motivo); break; }

            if (!lectura.Leido)
            {
                if (lectura.CantidadChips == 0)
                {
                    esperasSinChip++;
                    if (_opc.MaxEsperasSinChip > 0 && esperasSinChip >= _opc.MaxEsperasSinChip)
                    {
                        resumen.Motivo = Imprime
                            ? $"No apareció ninguna etiqueta con chip sobre la antena de la impresora en {esperasSinChip * _opc.EsperaChipSegundos} s: ¿se acabó el rollo, o las etiquetas no traen chip?"
                            : $"No apareció ningún chip frente a la antena en {esperasSinChip * _opc.EsperaChipSegundos} s.";
                        _log.Error(resumen.Motivo);
                        break;
                    }
                    _log.Aviso($"No apareció {(Imprime ? "ninguna etiqueta con chip" : "ningún chip")} en {_opc.EsperaChipSegundos} s; sigo esperando.");
                }
                else { esperasSinChip = 0; _log.Aviso(lectura.Error ?? "No pude leer el chip."); await EsperarAsync(TimeSpan.FromSeconds(1), ct); }
                continue;
            }
            esperasSinChip = 0;
            var tid = lectura.Tid!;
            if (tid == ultimoTid)
            {
                if (await EsperarRetiroAsync(tid, ct)) ultimoTid = null;
                continue;
            }

            // 1. enrolar
            var enr = await _api.EnrolarAsync(numEmision, tid, _codProductoEquipo, ct);
            ultimoTid = tid;
            if (!enr.Ok || enr.Datos is null)
            {
                _log.Error($"La API no aceptó el chip {tid}: {enr.Mensaje}");
                resumen.Rechazadas++;
                if (enr.Estado is 401 or 403 or 0) { resumen.Motivo = $"No puedo seguir: {enr.Mensaje}"; break; }   // sin permiso o sin red: no tiene sentido probar el siguiente chip
                if (await EsperarRetiroAsync(tid, ct)) ultimoTid = null;
                continue;
            }
            var e = enr.Datos;
            if (e.FLG_YA_ENROLADA && e.FLG_GRABADA)
            {
                _log.Info($"El chip {tid} ya estaba enrolado y grabado (etiqueta {e.NUM_ITEM}-{e.NUM_ETIQUETA}, código {e.COD_STICKER_RFID}). Lo ignoro.");
                resumen.YaGrabadas++;
                if (await EsperarRetiroAsync(tid, ct)) ultimoTid = null;
                continue;
            }
            if (e.FLG_YA_ENROLADA) _log.Info($"El chip {tid} ya estaba enrolado (etiqueta {e.NUM_ITEM}-{e.NUM_ETIQUETA}) pero sin grabar: retomo la grabación.");

            // 2. grabar y 3. constancia
            procesadas++;
            var sigueEnLaImpresora = false;      // la impresora no llego a imprimir: la etiqueta sigue ahi, sin dañar
            var (grabado, error, intentos, consumida) = Imprime ? await ImprimirConReintentosAsync(tid, e, ct) : await GrabarConReintentosAsync(tid, e, ct);
            if (ct.IsCancellationRequested && !grabado) break;
            if (grabado)
            {
                resumen.Grabadas++; fallosSeguidos = 0;
                _log.Info($"Etiqueta {e.NUM_ITEM}-{e.NUM_ETIQUETA} lista: TID {tid} → EPC {e.COD_EPC} (código {e.COD_STICKER_RFID}).");
            }
            else
            {
                resumen.Fallidas++; fallosSeguidos++;
                _log.Error($"No pude grabar el chip {tid} tras {intentos} intento(s): {error}");
                if (!consumida)
                {
                    sigueEnLaImpresora = true;
                    _log.Aviso($"La etiqueta {e.NUM_ITEM}-{e.NUM_ETIQUETA} no llegó a imprimirse y el chip sigue sobre la antena de la impresora: queda enrolada sin grabar y se reintenta con la misma etiqueta.");
                }
                else if (_opc.AnularSiFalla)
                {
                    var motivo = $"No se pudo grabar el EPC en el chip {tid}: {error}";
                    var a = await _api.AnularEtiquetaAsync(numEmision, e.NUM_ITEM, e.NUM_ETIQUETA, motivo.Length > 500 ? motivo[..500] : motivo, reponer: true, ct);
                    if (a.Ok) { resumen.Anuladas++; _log.Aviso($"Etiqueta {e.NUM_ITEM}-{e.NUM_ETIQUETA} anulada y repuesta. {(Imprime ? "Descarta esa etiqueta impresa: no sirve." : "Retira ese chip: no sirve.")}"); }
                    else _log.Error($"No pude anular la etiqueta {e.NUM_ITEM}-{e.NUM_ETIQUETA}: {a.Mensaje}. Queda enrolada sin grabar.");
                }
                else _log.Aviso($"La etiqueta {e.NUM_ITEM}-{e.NUM_ETIQUETA} queda enrolada sin grabar. {(Imprime ? "Si el mismo chip vuelve a quedar sobre la antena se reintenta." : "Si pones el mismo chip otra vez se reintenta.")}");
            }

            // lo que dice la API ahora es lo que vale para saber cuanto falta
            var act = await _api.EmisionAsync(numEmision, ct);
            if (!act.Ok || act.Datos is null) { resumen.Motivo = $"No pude releer la emisión {numEmision}: {act.Mensaje}"; _log.Error(resumen.Motivo); break; }
            Anotar(resumen, act.Datos);
            if (Faltan(resumen) > 0) _log.Info($"Faltan {resumen.Pendientes} por enrolar y {resumen.SinGrabar} por grabar.");
            await InformarProgresoAsync(resumen, ct);
            if (sigueEnLaImpresora) { ultimoTid = null; await EsperarAsync(TimeSpan.FromSeconds(1), ct); }   // se vuelve a leer la misma etiqueta y se retoma
            else if (await EsperarRetiroAsync(tid, ct)) ultimoTid = null;
        }

        var fin = await _api.EmisionAsync(numEmision, CancellationToken.None);
        if (fin.Ok && fin.Datos is not null) Anotar(resumen, fin.Datos);
        resumen.Completa = Faltan(resumen) == 0;
        resumen.Cancelada = ct.IsCancellationRequested || _detener.IsCancellationRequested;
        if (resumen.Completa) resumen.Motivo = null;
        _log.Info($"Resumen emisión {numEmision}: {resumen.Grabadas} grabada(s), {resumen.YaGrabadas} ya grabada(s), {resumen.Rechazadas} rechazada(s), {resumen.Fallidas} fallida(s) ({resumen.Anuladas} anulada(s)); faltan {resumen.Pendientes} por enrolar y {resumen.SinGrabar} por grabar.");
        return resumen;
    }

    private async Task InformarProgresoAsync(ResumenEmision resumen, CancellationToken ct)
    {
        if (_alProgreso is null) return;
        try { await _alProgreso(resumen, ct); }
        catch (OperationCanceledException) { }
        catch (Exception ex) { _log.Aviso("No pude informar el avance: " + ex.Message); }
    }

    private static void Anotar(ResumenEmision r, RfidEmision e)
    {
        r.Pendientes = e.CAN_PENDIENTES;
        r.SinGrabar = Math.Max(0, e.CAN_ENROLADAS - e.CAN_GRABADAS);
    }

    private static int Faltan(ResumenEmision r) => r.Pendientes + r.SinGrabar;

    private async Task<(bool Grabado, string? Error, int Intentos, bool Consumida)> GrabarConReintentosAsync(string tid, RfidEnrolado e, CancellationToken ct)
    {
        string? error = null;
        var intentos = 1 + Math.Max(0, _opc.Reintentos);
        for (int i = 1; i <= intentos && !ct.IsCancellationRequested; i++)
        {
            RfidGrabacionResultado g;
            try { g = await _adaptador.GrabarEpcAsync(new RfidGrabacionSolicitud { Tid = tid, Epc = e.COD_EPC }, ct); }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex) { g = new RfidGrabacionResultado { Grabado = false, Error = "El lector falló al grabar: " + ex.Message }; }

            if (g.Grabado)
            {
                var c = await _api.GrabadaAsync(tid, g.EpcLeido, ct);
                if (c.Ok) return (true, null, i, true);
                error = $"el EPC quedó grabado pero la API no aceptó la constancia: {c.Mensaje}";
            }
            else error = g.Error ?? "el lector no confirmó la grabación";
            if (i < intentos) { _log.Aviso($"Intento {i}/{intentos}: {error}. Reintento…"); await EsperarAsync(TimeSpan.FromMilliseconds(500), ct); }
        }
        return (false, error, intentos, true);
    }

    /// <summary>Imprime la etiqueta (y con ella graba el chip). Solo se repite si la impresora NO llegó a imprimir (estaba en pausa, la etiqueta no estaba sobre la antena...): una etiqueta
    /// que ya salió no se repite, porque la siguiente es otro chip con otro TID. Si el EPC quedó grabado pero la API no acepta la constancia, lo que se repite es la constancia.
    /// Si no llegó a imprimir nada, la etiqueta sigue sin daño sobre la antena: no se anula, se deja enrolada y se retoma con la misma (Consumida = false).</summary>
    private async Task<(bool Grabado, string? Error, int Intentos, bool Consumida)> ImprimirConReintentosAsync(string tid, RfidEnrolado e, CancellationToken ct)
    {
        string? error = null;
        var intentos = 1 + Math.Max(0, _opc.Reintentos);
        for (int i = 1; i <= intentos && !ct.IsCancellationRequested; i++)
        {
            RfidImpresionResultado r;
            try
            {
                r = await _adaptador.ImprimirAsync(new RfidImpresionSolicitud
                {
                    Tid = tid, Epc = e.COD_EPC, Producto = e.COD_PRODUCTO, DescripcionProducto = e.DES_PRODUCTO, Lote = e.NUM_LOTE_CONTROL,
                    Cantidad = e.CAN_PRODUCTO, UnidadMedida = e.COD_UNIDAD_MEDIDA, CodigoBarras = e.COD_STICKER_RFID,
                }, ct);
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex) { r = new RfidImpresionResultado { Impreso = true, Grabado = false, Error = "La impresora falló al imprimir: " + ex.Message }; }

            if (r.Grabado)
            {
                for (int j = 1; j <= intentos; j++)
                {
                    var c = await _api.GrabadaAsync(tid, r.EpcLeido, ct);
                    if (c.Ok) return (true, null, i, true);
                    error = $"el EPC quedó grabado pero la API no aceptó la constancia: {c.Mensaje}";
                    if (j < intentos) { _log.Aviso($"Constancia {j}/{intentos}: {error}. Reintento…"); await EsperarAsync(TimeSpan.FromMilliseconds(500), ct); }
                    if (ct.IsCancellationRequested) break;
                }
                return (false, error, i, true);
            }
            error = r.Error ?? "la impresora no confirmó la grabación";
            if (r.Impreso) return (false, error, i, true);
            if (i < intentos) { _log.Aviso($"Intento {i}/{intentos}: {error}. Reintento…"); await EsperarAsync(TimeSpan.FromMilliseconds(500), ct); }
        }
        return (false, error, intentos, false);
    }

    /// <summary>Espera a que el chip ya procesado salga del campo (o aparezca otro). Devuelve true si ya no esta; false si sigue ahi tras el tiempo o se cancelo.</summary>
    private async Task<bool> EsperarRetiroAsync(string tid, CancellationToken ct)
    {
        var limite = DateTime.UtcNow.AddSeconds(_opc.RetiroChipSegundos);
        bool avisado = false;
        while (DateTime.UtcNow < limite && !ct.IsCancellationRequested && !_detener.IsCancellationRequested)
        {
            RfidLecturaTid l;
            try { l = await _adaptador.LeerTidAsync(new RfidLecturaOpciones { TiempoEsperaMs = 0 }, ct); }
            catch (OperationCanceledException) { return false; }
            catch { return false; }
            if (!l.Leido && l.CantidadChips == 0) return true;      // ya lo retiraron
            if (l.Leido && l.Tid != tid) return true;               // llego otro chip
            if (!avisado) { _log.Info(Imprime ? "Espero a que salga la etiqueta ya impresa…" : "Retira el chip ya procesado…"); avisado = true; }
            await EsperarAsync(TimeSpan.FromMilliseconds(300), ct);
        }
        if (!ct.IsCancellationRequested && !_detener.IsCancellationRequested) _log.Aviso($"El chip {tid} sigue {(Imprime ? "sobre la antena de la impresora" : "frente a la antena")} tras {_opc.RetiroChipSegundos} s.");
        return false;
    }

    private async Task EsperarAsync(TimeSpan t, CancellationToken ct)
    {
        try { await _esperar(t, ct); } catch (OperationCanceledException) { }
    }
}
