using KonSolutions.Agente.Configuracion;
using KonSolutions.Agente.Util;
using KONSolutions.Shared.Rfid.Agente;

namespace KonSolutions.Agente.Lector;

/// <summary>El adaptador de lectores Impinj (Speedway R220/R420, R700, XPortal...) del contrato del agente (<see cref="IRfidAdaptador"/>).
/// Habla con el lector por la interfaz <see cref="IImpinjLector"/>; las reglas del contrato las cumple AQUI, no el lector:
///  · si aparece mas de un chip frente al lector NO se graba ninguno;
///  · se graba al chip del TID enrolado (el TID es el filtro de la escritura), no al que este mas cerca;
///  · siempre se lee de vuelta el EPC antes de dar el resultado;
///  · el TID viaja en hexadecimal, mayusculas y sin espacios.</summary>
public sealed class AdaptadorImpinj : IRfidAdaptador
{
    private readonly IImpinjLector _lector;
    private readonly ImpinjOpciones _op;
    private readonly IBitacora _log;
    private ImpinjInfo? _info;

    public AdaptadorImpinj(IImpinjLector lector, ImpinjOpciones opciones, IBitacora log)
    { _lector = lector; _op = opciones; _log = log; }

    public string Fabricante => "IMPINJ";

    /// <summary>Los Monza 4 y mas nuevos admiten EPC de hasta 128 bits (los Monza 4E/4i, mas); el EPC de este modulo son 96 (24 hexadecimales).</summary>
    public RfidCapacidades Capacidades { get; } = new() { LeeTid = true, GrabaEpc = true, Imprime = false, EpcBitsMaximo = 128 };

    public ImpinjInfo? Info => _info;

    public async Task ConectarAsync(RfidEquipoConfig config, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(config.Direccion))
            throw new InvalidOperationException($"El equipo {config.CodProducto} no tiene dirección en su ficha técnica: revisa la especificación de la dirección en Configuración general > RFID y el dato en la ficha del producto.");
        if (!config.EsDe("impinj"))
            _log.Aviso($"El equipo {config.CodProducto} no figura como Impinj en su ficha (fabricante '{config.Fabricante}', marca '{config.Marca}'); se intenta igual con el adaptador de Impinj.");
        if (!string.IsNullOrWhiteSpace(config.Protocolo) && !config.Protocolo.Contains("LLRP", StringComparison.OrdinalIgnoreCase))
            _log.Aviso($"La ficha del equipo dice protocolo '{config.Protocolo}'; este adaptador habla LLRP (el puerto estándar es 5084).");

        var puerto = config.Puerto ?? _op.PuertoPorOmision;
        if (puerto is < 1 or > 65535) throw new InvalidOperationException($"El puerto {puerto} de la ficha del equipo no es válido.");
        var antenas = (config.PuertosAntena.Count > 0 ? config.PuertosAntena : new[] { 1 })
            .Select(a => a is >= 1 and <= 65535 ? (ushort)a : throw new InvalidOperationException($"El puerto de antena {a} de la ficha del equipo no es válido."))
            .Distinct().ToList();
        var potencia = config.PotenciaEscritura is { } w ? (double)w : _op.PotenciaPorOmisionDbm;
        if (potencia is < 10 or > 33) throw new InvalidOperationException($"La potencia {potencia} dBm de la ficha del equipo no es válida (entre 10 y 33).");

        _info = await _lector.ConectarAsync(new ImpinjConexion(config.Direccion.Trim(), puerto, antenas, potencia, _op.PedirFastId, _op.TiempoOperacionMs), ct);
        _log.Info($"Conectado al lector Impinj {_info.Modelo} (serie {_info.Serie}, firmware {_info.Firmware}) en {config.Direccion.Trim()}:{puerto}; antenas {string.Join(",", antenas)}; potencia {potencia:0.##} dBm; FastID {(_info.FastIdDisponible && _op.PedirFastId ? "sí" : "no")}.");
    }

    public async Task DesconectarAsync(CancellationToken ct = default)
    {
        await _lector.DesconectarAsync(ct);
        _log.Info("Lector desconectado.");
    }

    public Task<RfidEstadoEquipo> EstadoAsync(CancellationToken ct = default)
    {
        if (!_lector.Conectado) return Task.FromResult(new RfidEstadoEquipo { Conectado = false, ListoParaGrabar = false, Mensaje = "Sin conexión con el lector." });
        if (_info is { AccesoAEtiquetas: false })
            return Task.FromResult(new RfidEstadoEquipo { Conectado = true, ListoParaGrabar = false, Mensaje = "Este lector no permite leer ni escribir la memoria de las etiquetas." });
        return Task.FromResult(new RfidEstadoEquipo { Conectado = true, ListoParaGrabar = true });
    }

    public async Task<RfidLecturaTid> LeerTidAsync(RfidLecturaOpciones? opciones = null, CancellationToken ct = default)
    {
        opciones ??= new RfidLecturaOpciones();
        var limite = DateTime.UtcNow.AddMilliseconds(Math.Max(0, opciones.TiempoEsperaMs));
        do
        {
            ct.ThrowIfCancellationRequested();
            var chips = Distintas(await _lector.InventariarAsync(_op.VentanaLecturaMs, ct));
            if (chips.Count > 1)
                return new RfidLecturaTid { Leido = false, CantidadChips = chips.Count, Error = $"Hay {chips.Count} chips en el campo del lector: deja uno solo frente a la antena." };
            if (chips.Count == 1)
            {
                var chip = chips[0];
                var tid = Normalizar(chip.Tid) ?? Normalizar(await _lector.LeerTidAsync(chip.Epc, ct));
                if (tid is null)
                    return new RfidLecturaTid { Leido = false, CantidadChips = 1, EpcActual = Normalizar(chip.Epc), Error = $"Veo un chip (EPC {chip.Epc}) pero no respondió a la lectura de su TID: acércalo o súbele la potencia en la ficha del equipo." };
                return new RfidLecturaTid { Leido = true, Tid = tid, EpcActual = Normalizar(chip.Epc), CantidadChips = 1 };
            }
        } while (DateTime.UtcNow < limite);
        return new RfidLecturaTid { Leido = false, CantidadChips = 0 };
    }

    public async Task<RfidGrabacionResultado> GrabarEpcAsync(RfidGrabacionSolicitud solicitud, CancellationToken ct = default)
    {
        var tid = Normalizar(solicitud.Tid);
        var epc = Normalizar(solicitud.Epc);
        if (tid is null || !EsHex(tid)) return Fallo("El TID a grabar no es un hexadecimal válido.");
        if (epc is null || !EsHex(epc) || epc.Length % 4 != 0) return Fallo("El EPC a grabar tiene que ser hexadecimal y de palabras completas (múltiplo de 4 caracteres).");
        if (epc.Length * 4 > Capacidades.EpcBitsMaximo) return Fallo($"El EPC {epc} mide {epc.Length * 4} bits y el máximo que se graba aquí es {Capacidades.EpcBitsMaximo}.");

        // 1. que haya UN solo chip y que sea el enrolado
        var antes = Distintas(await _lector.InventariarAsync(_op.VentanaLecturaMs, ct));
        if (antes.Count == 0) return Fallo("No veo ningún chip frente al lector.");
        if (antes.Count > 1) return Fallo($"Hay {antes.Count} chips en el campo: no se graba ninguno. Deja uno solo frente a la antena.");
        var chip = antes[0];
        var tidVisto = Normalizar(chip.Tid) ?? Normalizar(await _lector.LeerTidAsync(chip.Epc, ct));
        if (tidVisto is null) return Fallo("El chip no respondió a la lectura de su TID: no puedo comprobar que sea el enrolado.");
        if (tidVisto != tid) return Fallo($"El chip frente al lector tiene el TID {tidVisto} y no el enrolado {tid}. No se grabó.");

        // 2. grabar, con el TID como filtro
        var esc = await _lector.EscribirEpcAsync(tid, epc, chip.PcBits, ct);
        if (!esc.Exito) return Fallo(esc.Error ?? "El lector no pudo grabar el EPC.");

        // 3. leer de vuelta
        var despues = Distintas(await _lector.InventariarAsync(_op.VentanaLecturaMs, ct));
        if (despues.Count == 0) return Fallo("Grabé, pero no pude leer el chip de vuelta (¿se movió?). No dejo constancia.");
        if (despues.Count > 1) return Fallo($"Grabé, pero ahora hay {despues.Count} chips en el campo y no puedo comprobar cuál tiene el EPC.");
        var vuelta = despues[0];
        var epcLeido = Normalizar(vuelta.Epc);
        if (Normalizar(vuelta.Tid) is { } tidVuelta && tidVuelta != tid)
            return new RfidGrabacionResultado { Grabado = false, EpcLeido = epcLeido, Error = $"El chip leído de vuelta tiene otro TID ({tidVuelta}); no es el enrolado." };
        return epcLeido == epc
            ? new RfidGrabacionResultado { Grabado = true, EpcLeido = epcLeido }
            : new RfidGrabacionResultado { Grabado = false, EpcLeido = epcLeido, Error = $"El EPC leído de vuelta ({epcLeido}) no es el que se grabó ({epc})." };
    }

    public Task<RfidImpresionResultado> ImprimirAsync(RfidImpresionSolicitud solicitud, CancellationToken ct = default)
        => throw new NotSupportedException("Los lectores Impinj no imprimen: para imprimir y grabar a la vez hace falta una impresora RFID (por ejemplo Zebra).");

    public ValueTask DisposeAsync() => _lector.DisposeAsync();

    // ───────── ayudas ─────────

    /// <summary>Los chips distintos del campo. Dos lecturas del mismo chip (por varias antenas o varias pasadas) cuentan como uno: la clave es el TID si el lector lo informo, y si no, el EPC.</summary>
    private static List<EtiquetaVista> Distintas(IEnumerable<EtiquetaVista> vistas)
        => vistas.GroupBy(v => Normalizar(v.Tid) ?? Normalizar(v.Epc) ?? string.Empty).Select(g => g.First()).Where(v => !string.IsNullOrEmpty(v.Epc)).ToList();

    /// <summary>Mayusculas y sin espacios; null si queda vacio.</summary>
    public static string? Normalizar(string? hex)
    {
        if (hex is null) return null;
        var s = new string(hex.Where(c => !char.IsWhiteSpace(c)).ToArray()).ToUpperInvariant();
        return s.Length == 0 ? null : s;
    }

    private static bool EsHex(string s) => s.All(Uri.IsHexDigit);

    private static RfidGrabacionResultado Fallo(string error) => new() { Grabado = false, Error = error };
}
