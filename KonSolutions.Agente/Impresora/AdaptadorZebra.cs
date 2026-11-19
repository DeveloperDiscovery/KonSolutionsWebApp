using KonSolutions.Agente.Configuracion;
using KonSolutions.Agente.Lector;
using KonSolutions.Agente.Util;
using KONSolutions.Shared.Rfid.Agente;

namespace KonSolutions.Agente.Impresora;

/// <summary>El adaptador de impresoras Zebra con RFID (ZD621R, ZT411R...) del contrato del agente (<see cref="IRfidAdaptador"/>). A diferencia de un lector, la impresora graba el chip
/// AL IMPRIMIR: el chip está dentro de la etiqueta. Por eso el flujo es el de las impresoras: leer el TID de la etiqueta que está sobre la antena (sin sacarla) → enrolar → imprimir
/// la plantilla con el EPC dentro del mismo trabajo y leer de vuelta. Las reglas del contrato las cumple AQUI:
///  · antes de imprimir se vuelve a leer el TID y tiene que ser el enrolado (así no se graba otra etiqueta);
///  · se lee de vuelta el EPC y el TID del chip y se mira el contador de etiquetas inválidas (void) de la impresora;
///  · ante la duda no se da por grabada, y si no se sabe si salió impresa se da por impresa (no se repite con el mismo TID).
/// Los textos de los comandos están en <see cref="ZebraOpciones"/> y NO se probaron contra una impresora real.</summary>
public sealed class AdaptadorZebra : IRfidAdaptador
{
    private readonly IImpresoraZpl _canal;
    private readonly ZebraOpciones _op;
    private readonly IBitacora _log;
    private readonly Func<string, string> _leerArchivo;
    private readonly Func<TimeSpan, CancellationToken, Task> _esperar;
    private string? _direccion;
    private int _puerto;
    private string _plantilla = ZebraComandos.PlantillaBasica;

    public AdaptadorZebra(IImpresoraZpl canal, Func<string, string> leerArchivo, ZebraOpciones opciones, IBitacora log, Func<TimeSpan, CancellationToken, Task>? esperar = null)
    { _canal = canal; _leerArchivo = leerArchivo; _op = opciones; _log = log; _esperar = esperar ?? Task.Delay; }

    public string Fabricante => "ZEBRA";

    /// <summary>Imprime y graba en un paso. El EPC de este módulo son 96 bits (24 hexadecimales).</summary>
    public RfidCapacidades Capacidades { get; } = new() { LeeTid = true, GrabaEpc = false, Imprime = true, EpcBitsMaximo = 96 };

    public string? Modelo { get; private set; }
    public string? Firmware { get; private set; }
    /// <summary>La plantilla con que se imprime (la de la ficha o la básica).</summary>
    public string Plantilla => _plantilla;

    public async Task ConectarAsync(RfidEquipoConfig config, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(config.Direccion))
            throw new InvalidOperationException($"El equipo {config.CodProducto} no tiene dirección en su ficha técnica: revisa la especificación de la dirección en Configuración general > RFID y el dato en la ficha del producto.");
        if (!config.EsDe("zebra"))
            _log.Aviso($"El equipo {config.CodProducto} no figura como Zebra en su ficha (fabricante '{config.Fabricante}', marca '{config.Marca}'); se intenta igual con el adaptador de Zebra.");
        if (!string.IsNullOrWhiteSpace(config.Protocolo) && !config.Protocolo.Contains("ZPL", StringComparison.OrdinalIgnoreCase))
            _log.Aviso($"La ficha del equipo dice protocolo '{config.Protocolo}'; este adaptador habla ZPL por TCP (el puerto estándar es 9100).");
        var puerto = config.Puerto ?? _op.PuertoPorOmision;
        if (puerto is < 1 or > 65535) throw new InvalidOperationException($"El puerto {puerto} de la ficha del equipo no es válido.");

        _plantilla = CargarPlantilla(config);

        _direccion = config.Direccion.Trim(); _puerto = puerto;
        await _canal.ConectarAsync(_direccion, _puerto, _op.TiempoConexionMs, ct);
        var hi = ZebraComandos.LeerHi(await PreguntarAsync("~HI", 1, ct));
        if (hi is null)
        {
            await _canal.DesconectarAsync(CancellationToken.None);
            throw new InvalidOperationException($"Me conecté a {_direccion}:{_puerto} pero no contestó a ~HI. ¿Es una impresora Zebra y ese es su puerto de impresión (9100)? La comunicación tiene que ser bidireccional.");
        }
        (Modelo, Firmware) = hi.Value;
        _log.Info($"Conectado a la impresora Zebra {Modelo} (firmware {Firmware}) en {_direccion}:{_puerto}; plantilla: {(string.IsNullOrWhiteSpace(config.PlantillaImpresion) ? "la básica del agente" : config.PlantillaImpresion)}.");
    }

    private string CargarPlantilla(RfidEquipoConfig config)
    {
        if (string.IsNullOrWhiteSpace(config.PlantillaImpresion))
        {
            _log.Aviso($"La ficha del equipo {config.CodProducto} no trae plantilla de impresión (Configuración general > RFID: ruta y archivo): se imprime con la etiqueta básica del agente.");
            return ZebraComandos.PlantillaBasica;
        }
        string texto;
        try { texto = _leerArchivo(config.PlantillaImpresion.Trim()); }
        catch (Exception ex) { throw new InvalidOperationException($"No pude leer la plantilla '{config.PlantillaImpresion}': {ex.Message}"); }
        if (ZebraComandos.ProblemaDePlantilla(texto) is { } problema)
            throw new InvalidOperationException($"La plantilla '{config.PlantillaImpresion}' no sirve: {problema}.");
        return texto;
    }

    public async Task DesconectarAsync(CancellationToken ct = default)
    {
        await _canal.DesconectarAsync(ct);
        _log.Info("Impresora desconectada.");
    }

    public async Task<RfidEstadoEquipo> EstadoAsync(CancellationToken ct = default)
    {
        string respuesta;
        try { respuesta = await PreguntarAsync("~HS", 3, ct); }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex) { return new RfidEstadoEquipo { Conectado = false, ListoParaGrabar = false, Mensaje = "Sin conexión con la impresora: " + ex.Message }; }
        var hs = ZebraComandos.LeerHs(respuesta);
        if (hs is null)
            return new RfidEstadoEquipo { Conectado = true, ListoParaGrabar = false, Mensaje = $"La impresora no contestó el estado (~HS) como se esperaba: {ZebraComandos.Visible(respuesta)}" };
        var problemas = hs.Problemas();
        return problemas.Count == 0
            ? new RfidEstadoEquipo { Conectado = true, ListoParaGrabar = true }
            : new RfidEstadoEquipo { Conectado = true, ListoParaGrabar = false, Mensaje = "La impresora está " + string.Join(", ", problemas) + "." };
    }

    public async Task<RfidLecturaTid> LeerTidAsync(RfidLecturaOpciones? opciones = null, CancellationToken ct = default)
    {
        opciones ??= new RfidLecturaOpciones();
        var limite = DateTime.UtcNow.AddMilliseconds(Math.Max(0, opciones.TiempoEsperaMs));
        while (true)
        {
            ct.ThrowIfCancellationRequested();
            var intento = await IntentarLeerTidAsync(ct);
            if (intento.Leido || intento.CantidadChips > 0 || DateTime.UtcNow >= limite) return intento;
            await _esperar(TimeSpan.FromMilliseconds(_op.PausaLecturaMs), ct);
        }
    }

    private async Task<RfidLecturaTid> IntentarLeerTidAsync(CancellationToken ct)
    {
        foreach (var c in _op.ComandosPosicionar) await EnviarSgdAsync(c, ct);
        foreach (var c in _op.ComandosLeerTid) await EnviarSgdAsync(c, ct);
        foreach (var consulta in _op.ConsultasResultadoTid)
        {
            if (ZebraComandos.SoloHex(await PreguntarAsync(consulta, 1, ct)) is { } hex)
            {
                if (hex.Length * 4 < _op.TidBitsMinimos)
                    return new RfidLecturaTid { Leido = false, CantidadChips = 1, Error = $"La impresora devolvió '{hex}' ({hex.Length * 4} bits) como TID: no alcanza para identificar el chip (hacen falta {_op.TidBitsMinimos}). Revisa Zebra.ComandosLeerTid con 'agente-rfid diagnostico'." };
                return new RfidLecturaTid { Leido = true, Tid = hex, CantidadChips = 1 };
            }
        }
        return new RfidLecturaTid { Leido = false, CantidadChips = 0 };
    }

    public Task<RfidGrabacionResultado> GrabarEpcAsync(RfidGrabacionSolicitud solicitud, CancellationToken ct = default)
        => throw new NotSupportedException("Una impresora Zebra graba el chip al imprimir la etiqueta: usa ImprimirAsync.");

    public async Task<RfidImpresionResultado> ImprimirAsync(RfidImpresionSolicitud solicitud, CancellationToken ct = default)
    {
        var tid = AdaptadorImpinj.Normalizar(solicitud.Tid);
        var epc = AdaptadorImpinj.Normalizar(solicitud.Epc);
        if (tid is null || !tid.All(Uri.IsHexDigit)) return NoImpreso("El TID a grabar no es un hexadecimal válido.");
        if (epc is null || !epc.All(Uri.IsHexDigit) || epc.Length % 4 != 0) return NoImpreso("El EPC a grabar tiene que ser hexadecimal y de palabras completas (múltiplo de 4 caracteres).");
        if (epc.Length * 4 > Capacidades.EpcBitsMaximo) return NoImpreso($"El EPC {epc} mide {epc.Length * 4} bits y el máximo que se graba aquí es {Capacidades.EpcBitsMaximo}.");

        // 1. la impresora tiene que estar lista y la etiqueta sobre la antena tiene que ser la enrolada
        var estado = await EstadoAsync(ct);
        if (!estado.ListoParaGrabar) return NoImpreso(estado.Mensaje ?? "La impresora no está lista.");
        var lectura = await LeerTidAsync(new RfidLecturaOpciones { TiempoEsperaMs = 0 }, ct);
        if (!lectura.Leido)
            return NoImpreso(lectura.CantidadChips == 0 ? "No veo ninguna etiqueta con chip sobre la antena de la impresora." : lectura.Error ?? "No pude leer el TID de la etiqueta.");
        if (lectura.Tid != tid) return NoImpreso($"La etiqueta que está sobre la antena tiene el TID {lectura.Tid} y no el enrolado {tid}. No se imprimió.");

        // 2. imprimir (el EPC viaja dentro del mismo trabajo) y esperar lo que la impresora leyó de vuelta
        var antes = await ContadoresAsync(ct);
        var trabajo = ZebraComandos.ArmarTrabajo(_plantilla, new RfidImpresionSolicitud
        {
            Tid = tid, Epc = epc, Producto = solicitud.Producto, DescripcionProducto = solicitud.DescripcionProducto, Lote = solicitud.Lote,
            Cantidad = solicitud.Cantidad, UnidadMedida = solicitud.UnidadMedida, CodigoBarras = solicitud.CodigoBarras,
        }, _op);
        await AsegurarConexionAsync(ct);
        _canal.LimpiarEntrada();
        await _canal.EnviarAsync(trabajo, ct);
        var texto = await _canal.EsperarRespuestaAsync(ZebraComandos.TieneLecturaCompleta, _op.TiempoImpresionMs, 250, ct);

        // 3. el veredicto
        var despues = await ContadoresAsync(ct);
        var v = ZebraComandos.Evaluar(epc, tid, ZebraComandos.LeerDeVuelta(texto), antes, despues);
        if (!v.Grabado) _log.Aviso($"La etiqueta del chip {tid} no quedó bien: {v.Error} (respuesta de la impresora: {ZebraComandos.Visible(texto)})");
        return new RfidImpresionResultado { Impreso = v.Impreso, Grabado = v.Grabado, EpcLeido = v.EpcLeido, Error = v.Error };
    }

    public ValueTask DisposeAsync() => _canal.DisposeAsync();

    // ───────── diagnóstico ─────────

    /// <summary>Manda un comando tal cual y devuelve lo que la impresora contesta (la espera es corta: hasta que deje de mandar).</summary>
    public async Task<string> ConsultarCrudoAsync(string comando, CancellationToken ct = default)
    {
        await AsegurarConexionAsync(ct);
        _canal.LimpiarEntrada();
        await _canal.EnviarAsync(comando.EndsWith('\n') ? comando : comando + "\r\n", ct);
        return await _canal.EsperarRespuestaAsync(null, _op.TiempoRespuestaMs, 400, ct);
    }

    /// <summary>Lo que contesta de verdad la impresora a lo que usa el adaptador, para ajustar <see cref="ZebraOpciones"/> sin adivinar.</summary>
    public async Task DiagnosticarAsync(TextWriter o, CancellationToken ct = default)
    {
        o.WriteLine($"Impresora: {Modelo} · firmware {Firmware} · {_direccion}:{_puerto}");
        async Task Mostrar(string titulo, string comando)
        {
            var r = await ConsultarCrudoAsync(comando, ct);
            o.WriteLine($"  {titulo}: {comando.Replace("\r\n", "")}");
            o.WriteLine($"      → {ZebraComandos.Visible(r)}");
        }
        o.WriteLine("Estado:");
        await Mostrar("~HS", "~HS");
        var e = await EstadoAsync(ct);
        o.WriteLine($"      interpretado: {(e.ListoParaGrabar ? "lista para grabar" : "NO lista")}{(e.Mensaje is null ? "" : " — " + e.Mensaje)}");
        o.WriteLine("Contadores y ajustes de RFID:");
        await Mostrar("etiquetas buenas", _op.ConsultaContadorValidas);
        await Mostrar("etiquetas void", _op.ConsultaContadorVoid);
        await Mostrar("rfid.error.response", "! U1 getvar \"rfid.error.response\"");
        await Mostrar("rfid.tag.read.content", "! U1 getvar \"rfid.tag.read.content\"");
        o.WriteLine("Lectura del TID (pon una etiqueta con chip sobre la antena):");
        foreach (var c in _op.ComandosPosicionar) { o.WriteLine($"  posicionar: {c}"); await EnviarSgdAsync(c, ct); }
        foreach (var c in _op.ComandosLeerTid) { o.WriteLine($"  enviado: {c}"); await EnviarSgdAsync(c, ct); }
        foreach (var c in _op.ConsultasResultadoTid) await Mostrar("resultado", c);
        var l = await IntentarLeerTidAsync(ct);
        o.WriteLine(l.Leido ? $"  El agente entendió: TID {l.Tid} ({l.Tid!.Length * 4} bits)." : $"  El agente NO obtuvo un TID: {l.Error ?? "ninguna etiqueta respondió."}");
    }

    // ───────── ayudas ─────────

    private static RfidImpresionResultado NoImpreso(string error) => new() { Impreso = false, Grabado = false, Error = error };

    /// <summary>Las impresoras cierran la conexión si pasa un rato sin tráfico: si se cayó, se vuelve a abrir una vez.</summary>
    private async Task AsegurarConexionAsync(CancellationToken ct)
    {
        if (_canal.Conectada) return;
        if (_direccion is null) throw new InvalidOperationException("La impresora no está conectada.");
        _log.Aviso("Se perdió la conexión con la impresora; la vuelvo a abrir…");
        await _canal.ConectarAsync(_direccion, _puerto, _op.TiempoConexionMs, ct);
    }

    private async Task EnviarSgdAsync(string comando, CancellationToken ct)
    {
        await AsegurarConexionAsync(ct);
        await _canal.EnviarAsync(comando + "\r\n", ct);
    }

    /// <summary>Manda un comando que se contesta con <paramref name="lineas"/> líneas y devuelve la respuesta (vacía si la impresora no contestó a tiempo).</summary>
    private async Task<string> PreguntarAsync(string comando, int lineas, CancellationToken ct)
    {
        await AsegurarConexionAsync(ct);
        _canal.LimpiarEntrada();
        await _canal.EnviarAsync(comando + "\r\n", ct);
        return await _canal.EsperarRespuestaAsync(r => ZebraComandos.TieneLineas(r, lineas), _op.TiempoRespuestaMs, 0, ct);
    }

    private async Task<ZebraComandos.Contadores?> ContadoresAsync(CancellationToken ct)
    {
        var v = ZebraComandos.Entero(await PreguntarAsync(_op.ConsultaContadorValidas, 1, ct));
        var x = ZebraComandos.Entero(await PreguntarAsync(_op.ConsultaContadorVoid, 1, ct));
        return v is { } a && x is { } b ? new ZebraComandos.Contadores(a, b) : null;
    }
}
