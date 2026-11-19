using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using KonSolutions.Agente.Configuracion;
using KONSolutions.Shared.Rfid.Agente;

namespace KonSolutions.Agente.Impresora;

/// <summary>Todo lo que depende del texto que entiende (o contesta) una impresora Zebra, en un solo lugar y sin conexión alguna: armar el trabajo ZPL a partir de la plantilla,
/// leer las respuestas de ~HS, ~HI y los getvar de SGD, y decidir si una etiqueta quedó bien. Los textos de los comandos viven en <see cref="ZebraOpciones"/>.</summary>
public static class ZebraComandos
{
    /// <summary>Con estos encabezados el agente reconoce en la respuesta de la impresora lo que ^HV mandó (<see cref="ZebraOpciones.LeerDeVuelta"/>).</summary>
    public const string EncabezadoEpc = "#EPC=", EncabezadoTid = "#TID=";

    /// <summary>La etiqueta que se imprime cuando la ficha del equipo no trae plantilla: sencilla, para poder probar. Las de verdad van en una plantilla (Configuración general &gt; RFID).</summary>
    public const string PlantillaBasica = """
        ^XA
        ^CI28
        ^FO30,25^A0N,32,32^FD@PRODUCTO^FS
        ^FO30,65^A0N,24,24^FB560,2,0,L^FD@DESCRIPCION^FS
        ^FO30,130^A0N,24,24^FDLote: @LOTE^FS
        ^FO30,160^A0N,24,24^FDCant: @CANTIDAD @UM^FS
        ^FO30,200^BY1^BCN,60,Y,N,N^FD@CODIGO^FS
        ^XZ
        """;

    private static readonly Regex Marcadores = new(@"@(EPC|TID|PRODUCTO|DESCRIPCION|LOTE|CANTIDAD|UM|CODIGO)(?![A-Za-z0-9])", RegexOptions.Compiled);

    /// <summary>Una plantilla sirve si es un formato ZPL completo (^XA ... ^XZ).</summary>
    public static string? ProblemaDePlantilla(string? plantilla)
    {
        if (string.IsNullOrWhiteSpace(plantilla)) return "la plantilla está vacía";
        var xa = plantilla.IndexOf("^XA", StringComparison.OrdinalIgnoreCase);
        var xz = plantilla.LastIndexOf("^XZ", StringComparison.OrdinalIgnoreCase);
        if (xa < 0 || xz < 0) return "no es un formato ZPL: le falta ^XA o ^XZ";
        if (xz < xa) return "no es un formato ZPL: el ^XZ está antes que el ^XA";
        if (Regex.Matches(plantilla, @"\^XA", RegexOptions.IgnoreCase).Count > 1) return "trae más de una etiqueta (varios ^XA): una plantilla tiene que ser una sola";
        return null;
    }

    /// <summary>El trabajo que se manda a la impresora: la plantilla con sus marcadores (@EPC, @TID, @PRODUCTO, @DESCRIPCION, @LOTE, @CANTIDAD, @UM, @CODIGO) reemplazados,
    /// la grabación del EPC (si la plantilla no trae su propio ^RFW) y la lectura de vuelta del EPC y del TID para que el agente compruebe el resultado.</summary>
    public static string ArmarTrabajo(string plantilla, RfidImpresionSolicitud s, ZebraOpciones op)
    {
        var epc = s.Epc;
        var bytes = (epc.Length / 2).ToString(CultureInfo.InvariantCulture);
        var valores = new Dictionary<string, string>
        {
            ["EPC"] = epc,
            ["TID"] = s.Tid,
            ["PRODUCTO"] = Limpiar(s.Producto, 40),
            ["DESCRIPCION"] = Limpiar(s.DescripcionProducto, 120),
            ["LOTE"] = Limpiar(s.Lote, 30),
            ["CANTIDAD"] = Cantidad(s.Cantidad),
            ["UM"] = Limpiar(s.UnidadMedida, 10),
            ["CODIGO"] = Limpiar(s.CodigoBarras, 60),
        };
        var zpl = Marcadores.Replace(plantilla, m => valores[m.Groups[1].Value]);

        if (zpl.IndexOf("^RFW", StringComparison.OrdinalIgnoreCase) < 0)
        {
            var escribir = op.EscribirEpc.Replace("{epc}", epc).Replace("{bytes}", bytes);
            var xa = zpl.IndexOf("^XA", StringComparison.OrdinalIgnoreCase);
            zpl = zpl.Insert(xa + 3, escribir);
        }
        var leer = op.LeerDeVuelta.Replace("{bytes}", bytes);
        var xz = zpl.LastIndexOf("^XZ", StringComparison.OrdinalIgnoreCase);
        zpl = zpl.Insert(xz, leer);
        return zpl.TrimEnd() + "\r\n";
    }

    /// <summary>El texto de un campo de la etiqueta: sin ^ ni ~ (en ZPL empiezan un comando), sin saltos de línea ni caracteres de control, con los espacios juntos y con un largo máximo.</summary>
    public static string Limpiar(string? texto, int max)
    {
        if (string.IsNullOrWhiteSpace(texto)) return string.Empty;
        var sb = new StringBuilder(texto.Length);
        foreach (var c in texto)
            sb.Append(c is '^' or '~' || char.IsControl(c) ? ' ' : c);
        var limpio = Regex.Replace(sb.ToString(), @"\s+", " ").Trim();
        return limpio.Length <= max ? limpio : limpio[..max].TrimEnd();
    }

    public static string Cantidad(decimal? n) => n is null ? string.Empty : n.Value.ToString("0.###", CultureInfo.InvariantCulture);

    // ───────── respuestas ─────────

    /// <summary>Una respuesta SGD (<c>"valor"</c>) sin las comillas ni los caracteres de control (STX/ETX, CR, LF).</summary>
    public static string QuitarComillas(string? respuesta)
    {
        if (respuesta is null) return string.Empty;
        var s = new string(respuesta.Where(c => !char.IsControl(c)).ToArray()).Trim();
        if (s.Length >= 2 && s[0] == '"' && s[^1] == '"') s = s[1..^1];
        return s.Trim();
    }

    /// <summary>Las líneas de una respuesta, sin caracteres de control y sin las vacías.</summary>
    public static List<string> Lineas(string? respuesta)
        => (respuesta ?? string.Empty).Split('\n').Select(l => new string(l.Where(c => !char.IsControl(c)).ToArray()).Trim()).Where(l => l.Length > 0).ToList();

    /// <summary>Verdadero cuando ya llegaron <paramref name="n"/> líneas completas (terminadas en salto de línea).</summary>
    public static bool TieneLineas(string respuesta, int n) => respuesta.Count(c => c == '\n') >= n;

    public static string? SoloHex(string? texto)
    {
        var s = QuitarComillas(texto).Replace(" ", "").ToUpperInvariant();
        return s.Length > 0 && s.All(Uri.IsHexDigit) ? s : null;
    }

    /// <summary>El estado de la impresora según ~HS (las dos primeras líneas; ver "Host Status Return" en la guía de ZPL).</summary>
    public sealed record EstadoHs(bool SinPapel, bool EnPausa, bool CabezaArriba, bool SinCinta, bool TransferenciaTermica, bool Sobretemperatura)
    {
        public List<string> Problemas()
        {
            var p = new List<string>();
            if (SinPapel) p.Add("sin papel o etiquetas");
            if (EnPausa) p.Add("en pausa");
            if (CabezaArriba) p.Add("con la cabeza de impresión abierta");
            if (SinCinta && TransferenciaTermica) p.Add("sin cinta (ribbon)");
            if (Sobretemperatura) p.Add("con la cabeza demasiado caliente");
            return p;
        }
    }

    public static EstadoHs? LeerHs(string? respuesta)
    {
        var l = Lineas(respuesta).Where(x => x.Contains(',')).ToList();
        if (l.Count < 2) return null;
        var a = l[0].Split(',').Select(x => x.Trim()).ToArray();
        var b = l[1].Split(',').Select(x => x.Trim()).ToArray();
        if (a.Length < 12 || b.Length < 4) return null;
        return new EstadoHs(SinPapel: a[1] == "1", EnPausa: a[2] == "1", CabezaArriba: b[1] == "1", SinCinta: b[2] == "1", TransferenciaTermica: b[3] == "1", Sobretemperatura: a[11] == "1");
    }

    /// <summary>Modelo y firmware según ~HI (<c>ZD621R-203dpi,V93.21.15Z,8,8176KB</c>). Nulos si no contestó con esa forma.</summary>
    public static (string Modelo, string Firmware)? LeerHi(string? respuesta)
    {
        var l = Lineas(respuesta).FirstOrDefault(x => x.Contains(','));
        if (l is null) return null;
        var p = l.Split(',');
        return p[0].Trim().Length == 0 ? null : (p[0].Trim(), p[1].Trim());
    }

    public static int? Entero(string? respuesta)
        => int.TryParse(QuitarComillas(respuesta), NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) ? n : null;

    // ───────── lo que ^HV devolvió ─────────

    public sealed record LecturaDeVuelta(string? Epc, string? Tid);

    private static readonly Regex DeVuelta = new(Regex.Escape(EncabezadoEpc) + "(?<epc>[0-9A-Fa-f]*)|" + Regex.Escape(EncabezadoTid) + "(?<tid>[0-9A-Fa-f]*)", RegexOptions.Compiled);

    /// <summary>El EPC y el TID que la impresora leyó del chip después de grabarlo (nulos si no vinieron o vinieron vacíos).</summary>
    public static LecturaDeVuelta LeerDeVuelta(string? respuesta)
    {
        string? epc = null, tid = null;
        foreach (Match m in DeVuelta.Matches(respuesta ?? string.Empty))
        {
            if (m.Groups["epc"].Success && m.Groups["epc"].Value.Length > 0) epc = m.Groups["epc"].Value.ToUpperInvariant();
            if (m.Groups["tid"].Success && m.Groups["tid"].Value.Length > 0) tid = m.Groups["tid"].Value.ToUpperInvariant();
        }
        return new LecturaDeVuelta(epc, tid);
    }

    /// <summary>Cuando ya llegaron las dos lecturas (con al menos un hexadecimal cada una): el agente deja de esperar.</summary>
    public static bool TieneLecturaCompleta(string respuesta)
    {
        var l = LeerDeVuelta(respuesta);
        return l.Epc is not null && l.Tid is not null;
    }

    // ───────── veredicto ─────────

    public sealed record Contadores(int Validas, int Void);

    public sealed record Veredicto(bool Impreso, bool Grabado, string? EpcLeido, string? Error);

    /// <summary>Si la etiqueta quedó bien. Se compara lo que la impresora leyó del chip con lo enrolado, y los contadores de etiquetas buenas y void de antes y después.
    /// Regla de seguridad: ante cualquier señal de fallo no se da por grabada. Y si no se sabe si salió impresa, se da por impresa (no se vuelve a intentar con el mismo TID: la etiqueta ya pudo salir).</summary>
    public static Veredicto Evaluar(string epc, string tid, LecturaDeVuelta vuelta, Contadores? antes, Contadores? despues)
    {
        var comparables = antes is not null && despues is not null;
        var seAnuloUna = comparables && despues!.Void > antes!.Void;
        var nadaSeMovio = comparables && despues!.Void == antes!.Void && despues.Validas == antes.Validas;

        if (vuelta.Epc is null)
        {
            if (seAnuloUna) return new(true, false, null, "La impresora marcó la etiqueta como inválida (void): no pudo grabar el chip.");
            if (nadaSeMovio) return new(false, false, null, "La impresora no devolvió nada ni cambió sus contadores: parece que no llegó a procesar la etiqueta (¿pausa, tapa abierta, sin etiquetas?).");
            return new(true, false, null, "La impresora no devolvió la lectura del EPC después de imprimir, así que no puedo comprobar que el chip quedó grabado.");
        }
        if (vuelta.Tid is not null && vuelta.Tid != tid)
            return new(true, false, vuelta.Epc, $"El chip que la impresora grabó tiene el TID {vuelta.Tid} y no el enrolado {tid}: se grabó OTRA etiqueta. Descarta esta.");
        if (vuelta.Epc != epc)
            return new(true, false, vuelta.Epc, $"El EPC leído de vuelta ({vuelta.Epc}) no es el que se grabó ({epc}).");
        if (seAnuloUna)
            return new(true, false, vuelta.Epc, "La impresora leyó el EPC pero marcó la etiqueta como inválida (void). Descártala.");
        return new(true, true, vuelta.Epc, null);
    }

    /// <summary>Texto de una respuesta con los caracteres invisibles a la vista (para el diagnóstico y los mensajes de error).</summary>
    public static string Visible(string? texto, int max = 200)
    {
        if (string.IsNullOrEmpty(texto)) return "(nada)";
        var sb = new StringBuilder();
        foreach (var c in texto)
            sb.Append(c switch { '\r' => "\\r", '\n' => "\\n", '\u0002' => "<STX>", '\u0003' => "<ETX>", _ when char.IsControl(c) => $"<{(int)c:X2}>", _ => c.ToString() });
        var s = sb.ToString();
        return s.Length <= max ? s : s[..max] + "…";
    }
}
