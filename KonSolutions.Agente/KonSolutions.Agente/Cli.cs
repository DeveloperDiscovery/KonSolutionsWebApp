using System.Globalization;
using KonSolutions.Agente.Api;
using KonSolutions.Agente.Configuracion;
using KonSolutions.Agente.Emision;
using KonSolutions.Agente.Impresora;
using KonSolutions.Agente.Lector;
using KonSolutions.Agente.Servicio;
using KonSolutions.Agente.Util;
using KONSolutions.Shared.Rfid.Agente;

namespace KonSolutions.Agente;

/// <summary>Las piezas externas del agente, para poder cambiarlas en las pruebas (lector, API, consola, relojes).</summary>
public sealed class Entorno
{
    public Func<string, string?> Variable { get; init; } = Environment.GetEnvironmentVariable;
    public Func<string?, IBitacora> CrearBitacora { get; init; } = carpeta => new BitacoraConsola(carpeta);
    public Func<ApiOpciones, Func<string?>, IBitacora, IApiRfid> CrearApi { get; init; } = (api, password, log) => new ApiRfidCliente(ApiRfidCliente.CrearHttp(api), api, password, log);
    public Func<IImpinjLector> CrearLector { get; init; } = () => new ImpinjOctaneLector();
    public Func<IImpresoraZpl> CrearImpresora { get; init; } = () => new ImpresoraTcp();
    /// <summary>Lee un archivo de texto (la plantilla ZPL: normalmente una ruta de red \\servidor\carpeta\archivo.zpl).</summary>
    public Func<string, string> LeerArchivo { get; init; } = File.ReadAllText;
    public Func<TimeSpan, CancellationToken, Task>? Esperar { get; init; }
    public TextWriter Salida { get; init; } = Console.Out;
    public string CarpetaBase { get; init; } = AppContext.BaseDirectory;
}

/// <summary>Los argumentos de la linea de comandos: un comando y opciones "--clave valor" o "--bandera".</summary>
public sealed class Argumentos
{
    public string? Comando { get; private set; }
    private readonly Dictionary<string, string?> _opciones = new(StringComparer.OrdinalIgnoreCase);

    public static Argumentos Leer(IReadOnlyList<string> args)
    {
        var a = new Argumentos();
        for (int i = 0; i < args.Count; i++)
        {
            var t = args[i];
            if (t.StartsWith("--", StringComparison.Ordinal) && t.Length > 2)
            {
                var clave = t[2..];
                var eq = clave.IndexOf('=');
                if (eq > 0) { a._opciones[clave[..eq]] = clave[(eq + 1)..]; continue; }
                if (i + 1 < args.Count && !args[i + 1].StartsWith("--", StringComparison.Ordinal)) a._opciones[clave] = args[++i];
                else a._opciones[clave] = null;
            }
            else if (a.Comando is null) a.Comando = t.ToLowerInvariant();
        }
        return a;
    }

    public bool Tiene(string clave) => _opciones.ContainsKey(clave);
    public string? Valor(string clave) => _opciones.TryGetValue(clave, out var v) ? v : null;
    public int? Entero(string clave) => int.TryParse(Valor(clave), NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) ? n : null;
    public decimal? Decimal(string clave) => decimal.TryParse(Valor(clave)?.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out var n) ? n : null;
    public IReadOnlyCollection<string> Claves => _opciones.Keys;
}

public static class Cli
{
    private static readonly string[] Conocidas = { "config", "equipo", "emision", "todas", "max", "direccion", "puerto", "antenas", "potencia", "fabricante", "comando", "ayuda" };

    public const int Bien = 0, Uso = 1, SinConexion = 2, ConFallos = 3;

    public static async Task<int> EjecutarAsync(string[] args, Entorno entorno, CancellationToken ct)
    {
        var o = entorno.Salida;
        var a = Argumentos.Leer(args);
        if (a.Comando is null || a.Comando == "ayuda" || a.Tiene("ayuda")) { Ayuda(o); return a.Comando is null ? Uso : Bien; }
        if (a.Comando is not ("probar" or "leer" or "emitir" or "diagnostico" or "servicio")) { o.WriteLine($"No conozco el comando '{a.Comando}'."); Ayuda(o); return Uso; }
        var desconocidas = a.Claves.Where(c => !Conocidas.Contains(c, StringComparer.OrdinalIgnoreCase)).ToList();
        if (desconocidas.Count > 0) { o.WriteLine("Opción desconocida: " + string.Join(", ", desconocidas.Select(c => "--" + c))); Ayuda(o); return Uso; }

        AgenteConfig cfg;
        try { cfg = AgenteConfig.Cargar(RutaConfig(a, entorno)); }
        catch (ConfiguracionInvalidaException ex) { o.WriteLine(ex.Message); return Uso; }

        var directo = !string.IsNullOrWhiteSpace(a.Valor("direccion"));
        var fabricanteDirecto = (a.Valor("fabricante") ?? "impinj").Trim().ToLowerInvariant();
        if (fabricanteDirecto is not ("impinj" or "zebra")) { o.WriteLine("--fabricante tiene que ser impinj o zebra."); return Uso; }
        if (directo && a.Comando is "emitir" or "servicio") { o.WriteLine($"'{a.Comando}' necesita la API (enrola, deja constancia y atiende la cola de impresión): no se puede usar con --direccion."); return Uso; }
        var problemas = directo ? cfg.ValidarComportamiento() : cfg.Validar(entorno.Variable);
        if (problemas.Count > 0) { o.WriteLine("La configuración tiene problemas:"); foreach (var p in problemas) o.WriteLine("  · " + p); return Uso; }

        var log = entorno.CrearBitacora(cfg.Agente.CarpetaLogs);
        IApiRfid? api = directo ? null : entorno.CrearApi(cfg.Api, () => cfg.ObtenerPassword(entorno.Variable), log);

        if (a.Comando == "servicio") return await ServicioAsync(a, cfg, api!, log, entorno, ct);

        // ───────── qué equipo ─────────
        RfidEquipoConfig equipo;
        if (directo)
        {
            if (!TryAntenas(a.Valor("antenas"), out var antenas)) { o.WriteLine("--antenas tiene que ser una lista de números, por ejemplo 1,2."); return Uso; }
            var zebraDirecto = fabricanteDirecto == "zebra";
            equipo = new RfidEquipoConfig
            {
                CodProducto = a.Valor("equipo") ?? "(directo)", Fabricante = zebraDirecto ? "ZEBRA" : "IMPINJ", Direccion = a.Valor("direccion")!.Trim(), Puerto = a.Entero("puerto"),
                PuertosAntena = antenas, PotenciaEscritura = a.Decimal("potencia"), Protocolo = zebraDirecto ? "ZPL" : "LLRP",
            };
        }
        else
        {
            var eq = await api!.EquiposAsync(ct);
            if (!eq.Ok || eq.Datos is null) { o.WriteLine("No pude leer los equipos de la API: " + eq.Mensaje); return SinConexion; }
            var r = ResolverEquipo.Elegir(eq.Datos, a.Valor("equipo"), cfg.Agente.CodProductoEquipo, cfg.Agente.Estacion);
            if (r.Equipo is null) { o.WriteLine(r.Error); return Uso; }
            equipo = RfidEquipoConfig.Desde(r.Equipo);
            o.WriteLine(equipo.EsDe("zebra")
                ? $"Equipo: {equipo.CodProducto} · {equipo.Descripcion} · {equipo.Fabricante} {equipo.Modelo} · {equipo.Direccion}:{equipo.Puerto?.ToString(CultureInfo.InvariantCulture) ?? cfg.Zebra.PuertoPorOmision.ToString(CultureInfo.InvariantCulture)} · plantilla {(string.IsNullOrWhiteSpace(equipo.PlantillaImpresion) ? "(la básica del agente)" : equipo.PlantillaImpresion)}"
                : $"Equipo: {equipo.CodProducto} · {equipo.Descripcion} · {equipo.Fabricante} {equipo.Modelo} · {equipo.Direccion}:{equipo.Puerto} · antenas {string.Join(",", equipo.PuertosAntena)} · potencia {equipo.PotenciaEscritura?.ToString("0.##", CultureInfo.InvariantCulture) ?? "(por omisión)"} dBm");
            if (!string.IsNullOrWhiteSpace(equipo.ProblemasFicha)) { o.WriteLine("AVISO: la ficha del equipo tiene un problema: " + equipo.ProblemasFicha); log.Aviso("Ficha del equipo: " + equipo.ProblemasFicha); }
        }

        // ───────── conectar ─────────
        var esImpresora = equipo.EsDe("zebra");
        if (a.Comando == "diagnostico" && !esImpresora) { o.WriteLine("El diagnóstico es solo para impresoras Zebra (--equipo de una impresora, o --direccion IP --fabricante zebra)."); return Uso; }
        await using IRfidAdaptador adaptador = CrearAdaptador(equipo, cfg, entorno, log);
        try { await adaptador.ConectarAsync(equipo, ct); }
        catch (OperationCanceledException) { return Bien; }
        catch (Exception ex) { o.WriteLine($"No se pudo conectar con {(esImpresora ? "la impresora" : "el lector")}: " + ex.Message); return SinConexion; }

        try
        {
            return a.Comando switch
            {
                "diagnostico" => await DiagnosticoAsync((AdaptadorZebra)adaptador, a.Valor("comando"), o, ct),
                "probar" => await ProbarAsync(adaptador, o, ct),
                "leer" => await LeerAsync(adaptador, o, ct),
                _ => await EmitirAsync(a, cfg, api!, adaptador, equipo, log, entorno, ct),
            };
        }
        finally { try { await adaptador.DesconectarAsync(CancellationToken.None); } catch { /* ya estaba caido */ } }
    }

    /// <summary>El adaptador que corresponde al equipo: la impresora Zebra o, por omisión, el lector Impinj.</summary>
    public static IRfidAdaptador CrearAdaptador(RfidEquipoConfig equipo, AgenteConfig cfg, Entorno entorno, IBitacora log)
        => equipo.EsDe("zebra")
            ? new AdaptadorZebra(entorno.CrearImpresora(), entorno.LeerArchivo, cfg.Zebra, log, entorno.Esperar)
            : new AdaptadorImpinj(entorno.CrearLector(), cfg.Impinj, log);

    // ───────── comandos ─────────

    /// <summary>El servicio: se queda atendiendo los trabajos de impresión que se piden desde la web, hasta que lo detengan.</summary>
    private static async Task<int> ServicioAsync(Argumentos a, AgenteConfig cfg, IApiRfid api, IBitacora log, Entorno entorno, CancellationToken ct)
    {
        var o = entorno.Salida;
        var equipos = (a.Valor("equipo") is { Length: > 0 } uno ? new[] { uno } : cfg.Servicio.Equipos)
            .Select(x => x.Trim()).Where(x => x.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (equipos.Count == 0 && !string.IsNullOrWhiteSpace(cfg.Agente.CodProductoEquipo)) equipos.Add(cfg.Agente.CodProductoEquipo.Trim());
        if (equipos.Count == 0)
        { o.WriteLine("Di qué impresora atiende este servicio: --equipo CODIGO, o «Servicio.Equipos» (o «Agente.CodProductoEquipo») en el appsettings.json."); return Uso; }
        var servicio = new ServicioImpresion(cfg, api, equipo => CrearAdaptador(equipo, cfg, entorno, log), log, entorno.Esperar);
        await servicio.EjecutarAsync(equipos, ct);
        return Bien;
    }

    private static async Task<int> DiagnosticoAsync(AdaptadorZebra adaptador, string? comando, TextWriter o, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(comando))
        {
            o.WriteLine($"Envío: {comando}");
            o.WriteLine("Contesta: " + ZebraComandos.Visible(await adaptador.ConsultarCrudoAsync(comando, ct), 2000));
            return Bien;
        }
        await adaptador.DiagnosticarAsync(o, ct);
        return Bien;
    }

    private static async Task<int> ProbarAsync(IRfidAdaptador adaptador, TextWriter o, CancellationToken ct)
    {
        var estado = await adaptador.EstadoAsync(ct);
        o.WriteLine($"Estado: {(estado.Conectado ? "conectado" : "SIN conexión")}, {(estado.ListoParaGrabar ? "listo para grabar" : "NO listo para grabar")}{(estado.Mensaje is null ? "" : " — " + estado.Mensaje)}");
        o.WriteLine(adaptador.Capacidades.Imprime ? "Deja una etiqueta con chip sobre la antena de la impresora (espero hasta 5 segundos)…" : "Pon un chip frente a la antena (espero hasta 5 segundos)…");
        var l = await adaptador.LeerTidAsync(new RfidLecturaOpciones { TiempoEsperaMs = 5000 }, ct);
        if (l.Leido) o.WriteLine(Linea(l));
        else if (l.CantidadChips == 0) o.WriteLine(adaptador.Capacidades.Imprime ? "No vi ninguna etiqueta con chip (la conexión funciona; revisa que la etiqueta esté sobre la antena)." : "No vi ningún chip (la conexión funciona; acerca un chip o sube la potencia).");
        else o.WriteLine(l.Error);
        return estado.Conectado && estado.ListoParaGrabar ? Bien : SinConexion;
    }

    private static string Linea(RfidLecturaTid l) => l.EpcActual is null ? $"Chip: TID {l.Tid}" : $"Chip: TID {l.Tid} · EPC {l.EpcActual}";

    private static async Task<int> LeerAsync(IRfidAdaptador adaptador, TextWriter o, CancellationToken ct)
    {
        o.WriteLine("Leyendo chips (Ctrl+C para terminar). Solo muestra: no enrola ni graba nada.");
        string? ultimo = null;
        while (!ct.IsCancellationRequested)
        {
            RfidLecturaTid l;
            try { l = await adaptador.LeerTidAsync(new RfidLecturaOpciones { TiempoEsperaMs = 2000 }, ct); }
            catch (OperationCanceledException) { break; }
            var linea = l.Leido ? (l.EpcActual is null ? $"TID {l.Tid}" : $"TID {l.Tid} · EPC {l.EpcActual}") : l.CantidadChips == 0 ? "(ningún chip)" : l.Error ?? "(no se pudo leer)";
            if (linea != ultimo) { o.WriteLine(linea); ultimo = linea; }
        }
        return Bien;
    }

    private static async Task<int> EmitirAsync(Argumentos a, AgenteConfig cfg, IApiRfid api, IRfidAdaptador adaptador, RfidEquipoConfig equipo, IBitacora log, Entorno entorno, CancellationToken ct)
    {
        var o = entorno.Salida;
        var numeros = new List<int>();
        if (a.Tiene("emision"))
        {
            if (a.Entero("emision") is not { } n || n <= 0) { o.WriteLine("--emision tiene que ser el número de la emisión."); return Uso; }
            numeros.Add(n);
        }
        else if (a.Tiene("todas"))
        {
            foreach (var estado in new[] { "E", "P" })
            {
                var l = await api.EmisionesAsync(estado, ct);
                if (!l.Ok || l.Datos is null) { o.WriteLine("No pude leer las emisiones abiertas: " + l.Mensaje); return SinConexion; }
                numeros.AddRange(l.Datos.Where(e => !e.FLG_ANULADA && e.CAN_PENDIENTES > 0
                        && (string.IsNullOrWhiteSpace(e.COD_PRODUCTO_EQUIPO) || string.Equals(e.COD_PRODUCTO_EQUIPO.Trim(), equipo.CodProducto, StringComparison.OrdinalIgnoreCase)))
                    .Select(e => e.NUM_EMISION));
            }
            numeros = numeros.Distinct().OrderBy(n => n).ToList();
            if (numeros.Count == 0) { o.WriteLine("No hay emisiones abiertas con etiquetas pendientes para este equipo."); return Bien; }
            o.WriteLine("Emisiones a procesar: " + string.Join(", ", numeros));
        }
        else { o.WriteLine("Di qué emitir: --emision <número> o --todas."); return Uso; }

        var opciones = new OpcionesEmision
        {
            Reintentos = cfg.Agente.Reintentos, AnularSiFalla = cfg.Agente.AnularSiFalla,
            EsperaChipSegundos = cfg.Agente.EsperaChipSegundos, RetiroChipSegundos = cfg.Agente.RetiroChipSegundos,
            MaxEtiquetas = Math.Max(0, a.Entero("max") ?? 0),
        };
        var emisor = new EmisorEtiquetas(api, adaptador, equipo.CodProducto == "(directo)" ? null : equipo.CodProducto, opciones, log, entorno.Esperar);
        var todasBien = true;
        foreach (var n in numeros)
        {
            var r = await emisor.EmitirAsync(n, ct);
            if (!r.Exitosa) todasBien = false;
            if (r.Cancelada || ct.IsCancellationRequested) { o.WriteLine("Cancelado."); return ConFallos; }
        }
        return todasBien ? Bien : ConFallos;
    }

    // ───────── ayudas ─────────

    private static string RutaConfig(Argumentos a, Entorno e)
    {
        if (a.Valor("config") is { Length: > 0 } dada) return dada;
        var junto = Path.Combine(e.CarpetaBase, "appsettings.json");
        return File.Exists(junto) ? junto : "appsettings.json";
    }

    private static bool TryAntenas(string? texto, out List<int> antenas)
    {
        antenas = new List<int>();
        if (string.IsNullOrWhiteSpace(texto)) return true;
        foreach (var t in texto.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (!int.TryParse(t, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) || n < 1) return false;
            antenas.Add(n);
        }
        return true;
    }

    private static void Ayuda(TextWriter o)
    {
        o.WriteLine("""
            agente-rfid · agente local de lectores Impinj e impresoras Zebra para la emisión de etiquetas RFID

            Uso:
              agente-rfid probar  [--equipo COD] [--config ruta]
              agente-rfid probar  --direccion IP [--puerto 5084] [--antenas 1,2] [--potencia 20]    (sin API: solo prueba el lector Impinj)
              agente-rfid probar  --direccion IP --fabricante zebra [--puerto 9100]                  (sin API: solo prueba la impresora Zebra)
              agente-rfid leer    [--equipo COD | --direccion IP [--fabricante zebra] ...]            (muestra el TID de lo que ve; no graba)
              agente-rfid emitir  --emision N [--equipo COD] [--max N]                                (enrola, graba/imprime y deja constancia)
              agente-rfid emitir  --todas     [--equipo COD]                                          (todas las emisiones abiertas de este equipo)
              agente-rfid diagnostico [--equipo COD | --direccion IP --fabricante zebra] [--comando "~HS"]
                                                                                                      (solo Zebra: muestra lo que la impresora contesta de verdad)
              agente-rfid servicio [--equipo COD]                                                     (se queda atendiendo el botón Imprimir de la web; como servicio de Windows: ver LEEME_SERVICIO.txt)

            La API, el usuario y la estación se configuran en appsettings.json (copia appsettings.ejemplo.json).
            La contraseña va en la variable de entorno KONSOLUTIONS_AGENTE_PASSWORD (el nombre se puede cambiar en el archivo).
            El equipo (IP, puerto, antenas, potencia, plantilla de la etiqueta) sale de la ficha técnica del producto-equipo en la ERP.
            Si en la estación hay un lector y una impresora, dile cuál con --equipo.

            Códigos de salida: 0 bien · 1 uso o configuración · 2 sin conexión con la API, el lector o la impresora · 3 con fallos, cancelado o incompleto.
            """);
    }
}
