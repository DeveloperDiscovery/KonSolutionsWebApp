using Impinj.OctaneSdk;

namespace KonSolutions.Agente.Lector;

/// <summary>El lector Impinj de verdad, con el SDK oficial Octane (paquete OctaneSDK). Habla LLRP por el puerto 5084 con los Speedway (R220/R420), el R700 y los demas.
/// OJO: compila contra el SDK pero NO se probo con un lector real (no hay uno en el entorno de desarrollo): lo que se probo es todo lo que esta por encima
/// (<see cref="AdaptadorImpinj"/>, el flujo de emision y la API) con un lector simulado.
/// Un solo comando a la vez (un candado): el lector no admite una lectura y una escritura al mismo tiempo en este uso.</summary>
public sealed class ImpinjOctaneLector : IImpinjLector
{
    private ImpinjReader? _lector;
    private ImpinjConexion? _conexion;
    private readonly SemaphoreSlim _uso = new(1, 1);

    public bool Conectado => _lector?.IsConnected == true;

    public Task<ImpinjInfo> ConectarAsync(ImpinjConexion c, CancellationToken ct) => Task.Run(() =>
    {
        ct.ThrowIfCancellationRequested();
        var lector = new ImpinjReader { ConnectTimeout = Math.Max(5000, c.TiempoOperacionMs * 2) };
        try { lector.Connect(c.Direccion, c.Puerto); }
        catch (Exception ex) { throw new InvalidOperationException($"No pude conectar con el lector Impinj en {c.Direccion}:{c.Puerto}: {ex.Message}", ex); }

        try
        {
            var capacidades = lector.QueryFeatureSet();
            foreach (var puerto in c.Antenas)
                if (puerto > capacidades.AntennaCount)
                    throw new InvalidOperationException($"La ficha del equipo pide el puerto de antena {puerto} y el lector {capacidades.ModelName} solo tiene {capacidades.AntennaCount}.");
            if (!capacidades.IsTagAccessAvailable)
                throw new InvalidOperationException($"El lector {capacidades.ModelName} no permite leer ni escribir la memoria de las etiquetas.");

            var ajustes = lector.QueryDefaultSettings();
            ajustes.Report.IncludeFastId = c.PedirFastId && capacidades.IsFastIdAvailable;
            ajustes.Report.IncludePcBits = true;
            ajustes.Report.IncludeAntennaPortNumber = true;
            ajustes.Report.Mode = ReportMode.Individual;
            ajustes.ReaderMode = ReaderMode.AutoSetDenseReader;
            ajustes.SearchMode = SearchMode.DualTarget;      // el mismo chip se vuelve a leer en cada pasada (con SingleTarget se callaria tras la primera)
            ajustes.Session = 0;
            ajustes.AutoStart.Mode = AutoStartMode.None;      // el programa lo arranca y lo detiene
            ajustes.AutoStop.Mode = AutoStopMode.None;
            ajustes.Antennas.DisableAll();
            var potencia = PotenciaDeLaTabla(capacidades, c.PotenciaDbm);
            foreach (var puerto in c.Antenas)
            {
                var antena = ajustes.Antennas.GetAntenna(puerto);
                antena.IsEnabled = true;
                antena.TxPowerInDbm = potencia;
                antena.MaxRxSensitivity = true;
            }
            lector.ApplySettings(ajustes);
            lector.DeleteAllOpSequences();

            _lector = lector;
            _conexion = c;
            return new ImpinjInfo(capacidades.ModelName ?? capacidades.ReaderModel.ToString(), capacidades.SerialNumber ?? "?", capacidades.FirmwareVersion ?? "?",
                (int)capacidades.AntennaCount, capacidades.IsFastIdAvailable, capacidades.IsTagAccessAvailable);
        }
        catch
        {
            try { lector.Disconnect(); } catch { /* ya estaba caido */ }
            throw;
        }
    }, ct);

    public Task DesconectarAsync(CancellationToken ct) => Task.Run(() =>
    {
        var lector = _lector;
        _lector = null;
        if (lector is null || !lector.IsConnected) return;
        try { lector.Stop(); } catch { /* no estaba leyendo */ }
        try { lector.DeleteAllOpSequences(); } catch { }
        lector.Disconnect();
    }, CancellationToken.None);

    public async Task<IReadOnlyList<EtiquetaVista>> InventariarAsync(int milisegundos, CancellationToken ct)
    {
        var lector = ObtenerLector();
        await _uso.WaitAsync(ct);
        try
        {
            var vistas = new Dictionary<string, EtiquetaVista>();
            void AlReportar(ImpinjReader _, TagReport informe)
            {
                lock (vistas)
                    foreach (var t in informe.Tags)
                    {
                        var epc = Hex(t.Epc);
                        if (epc is null) continue;
                        var tid = t.IsFastIdPresent ? Hex(t.Tid) : null;
                        var pc = t.IsPcBitsPresent ? t.PcBits : (ushort)((epc.Length / 4) << 11);   // sin el PC del reporte, solo el largo (bits 15 a 11)
                        vistas[tid ?? epc] = new EtiquetaVista(epc, tid, pc);
                    }
            }
            lector.TagsReported += AlReportar;
            try
            {
                lector.Start();
                await Task.Delay(milisegundos, ct);
            }
            finally
            {
                try { lector.Stop(); } catch { /* ya estaba detenido */ }
                await Task.Delay(150, CancellationToken.None);   // los ultimos reportes llegan despues del Stop
                lector.TagsReported -= AlReportar;
            }
            lock (vistas) return vistas.Values.ToList();
        }
        finally { _uso.Release(); }
    }

    public async Task<string?> LeerTidAsync(string epc, CancellationToken ct)
    {
        // el TID de un Monza son 96 bits (6 palabras); algunos chips tienen 64 (4): si la primera lectura se pasa de la memoria, se prueba con menos
        foreach (ushort palabras in new ushort[] { 6, 4 })
        {
            var resultados = await EjecutarAsync(1, secuencia =>
            {
                secuencia.TargetTag.MemoryBank = MemoryBank.Epc;
                secuencia.TargetTag.BitPointer = BitPointers.Epc;
                secuencia.TargetTag.Data = epc;
                secuencia.Ops.Add(new TagReadOp { MemoryBank = MemoryBank.Tid, WordPointer = 0, WordCount = palabras });
            }, ct);
            var lecturas = resultados.OfType<TagReadOpResult>().ToList();
            var buena = lecturas.FirstOrDefault(r => r.Result == ReadResultStatus.Success);
            if (buena is not null) return Hex(buena.Data);
            if (!lecturas.Any(r => r.Result == ReadResultStatus.MemoryOverrunError)) break;
        }
        return null;
    }

    public async Task<ResultadoEscritura> EscribirEpcAsync(string tid, string epcNuevo, ushort pcBitsActual, CancellationToken ct)
    {
        // si el EPC nuevo tiene otro largo que el actual, tambien hay que corregir los bits PC (su largo en palabras)
        var pcNuevo = PcBits.AdjustPcBits(pcBitsActual, (ushort)(epcNuevo.Length / 4));
        var esperadas = pcNuevo != pcBitsActual ? 2 : 1;
        var resultados = await EjecutarAsync(esperadas, secuencia =>
        {
            secuencia.TargetTag.MemoryBank = MemoryBank.Tid;     // el filtro es el TID: solo ese chip responde a la escritura
            secuencia.TargetTag.BitPointer = 0;
            secuencia.TargetTag.Data = tid;
            secuencia.Ops.Add(new TagWriteOp { MemoryBank = MemoryBank.Epc, WordPointer = WordPointers.Epc, Data = TagData.FromHexString(epcNuevo) });
            if (esperadas == 2)
                secuencia.Ops.Add(new TagWriteOp { MemoryBank = MemoryBank.Epc, WordPointer = WordPointers.PcBits, Data = TagData.FromWord(pcNuevo) });
        }, ct);

        var escrituras = resultados.OfType<TagWriteOpResult>().ToList();
        var mala = escrituras.FirstOrDefault(e => e.Result != WriteResultStatus.Success);
        if (mala is not null) return new ResultadoEscritura(false, Describir(mala.Result));
        if (escrituras.Count < esperadas)
            return new ResultadoEscritura(false, "El lector no confirmó la grabación a tiempo: ¿el chip está frente a la antena y la potencia alcanza?");
        return new ResultadoEscritura(true, null);
    }

    // ───────── ayudas ─────────

    /// <summary>Corre UNA secuencia de operaciones sobre el chip elegido y junta sus resultados (hasta <paramref name="esperados"/> o hasta que se acabe el tiempo).</summary>
    private async Task<List<TagOpResult>> EjecutarAsync(int esperados, Action<TagOpSequence> configurar, CancellationToken ct)
    {
        var lector = ObtenerLector();
        var espera = _conexion?.TiempoOperacionMs ?? 4000;
        await _uso.WaitAsync(ct);
        try
        {
            var acumulado = new List<TagOpResult>();
            var completo = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            void AlTerminar(ImpinjReader _, TagOpReport informe)
            {
                lock (acumulado)
                {
                    acumulado.AddRange(informe.Results);
                    if (acumulado.Count >= esperados) completo.TrySetResult();
                }
            }
            var secuencia = new TagOpSequence { SequenceStopTrigger = SequenceTriggerType.ExecutionCount, ExecutionCount = 1 };
            configurar(secuencia);
            lector.TagOpComplete += AlTerminar;
            try
            {
                lector.DeleteAllOpSequences();
                lector.AddOpSequence(secuencia);
                lector.Start();
                await Task.WhenAny(completo.Task, Task.Delay(espera, ct));
                ct.ThrowIfCancellationRequested();
            }
            finally
            {
                try { lector.Stop(); } catch { }
                try { lector.DeleteAllOpSequences(); } catch { }
                lector.TagOpComplete -= AlTerminar;
            }
            lock (acumulado) return acumulado.ToList();
        }
        finally { _uso.Release(); }
    }

    private ImpinjReader ObtenerLector()
        => _lector is { IsConnected: true } l ? l : throw new InvalidOperationException("No hay conexión con el lector Impinj.");

    /// <summary>La potencia pedida, o la mas cercana por debajo que tiene la tabla del lector (cada modelo trae la suya).</summary>
    private static double PotenciaDeLaTabla(FeatureSet capacidades, double pedida)
    {
        var tabla = capacidades.TxPowers.Select(t => t.Dbm).OrderBy(d => d).ToList();
        if (tabla.Count == 0) return pedida;
        var posibles = tabla.Where(d => d <= pedida + 0.001).ToList();
        return posibles.Count > 0 ? posibles.Max() : tabla[0];
    }

    private static string? Hex(TagData? datos)
    {
        var s = datos?.ToHexString();
        if (string.IsNullOrWhiteSpace(s)) return null;
        return new string(s.Where(c => !char.IsWhiteSpace(c)).ToArray()).ToUpperInvariant();
    }

    private static string Describir(WriteResultStatus estado) => estado switch
    {
        WriteResultStatus.TagMemoryOverrunError => "El chip no tiene memoria para ese EPC.",
        WriteResultStatus.TagMemoryLockedError => "La memoria EPC del chip está bloqueada (lock): no se puede grabar.",
        WriteResultStatus.InsufficientPower => "Potencia insuficiente para grabar: sube la potencia de escritura (TX POWER) en la ficha del equipo o acerca el chip.",
        WriteResultStatus.NoResponseFromTag => "El chip no respondió a la grabación: ¿se movió o está lejos de la antena?",
        WriteResultStatus.NonspecificTagError => "El chip rechazó la grabación.",
        WriteResultStatus.NonspecificReaderError => "El lector informó un error al grabar.",
        WriteResultStatus.IncorrectPasswordError => "El chip tiene contraseña de acceso: no se puede grabar.",
        _ => $"La grabación falló ({estado}).",
    };

    public async ValueTask DisposeAsync() => await DesconectarAsync(CancellationToken.None);
}
