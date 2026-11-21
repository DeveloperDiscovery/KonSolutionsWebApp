using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using KONSolutions.Shared.Models.Ingenieria;
using Microsoft.JSInterop;

namespace KONSolutions.Web.Pages.Ingenieria;

/// <summary>Ficha (tarjeta) que aparece al pasar el mouse por una caja del diagrama de la estructura, en lugar del tooltip
/// nativo del navegador. Cada caja lleva sus datos en el atributo data-ficha (JSON de <see cref="Json"/>) y un solo script
/// (embebido aqui, igual que EstructuraFit: no depende de wwwroot ni de index.html) dibuja la tarjeta junto al puntero.
/// La tarjeta vive en body (fuera de #app) y se escala con el zoom propio de la app (--app-scale), como los desplegables.</summary>
public static class DiagramaFicha
{
    private sealed record FichaDto(
        [property: JsonPropertyName("t")] string Tipo,          // etiqueta del tipo: Insumo, Subensamble, Operacion...
        [property: JsonPropertyName("c")] string Clase,         // ver | sec | com | ins | ens | rut (color de la franja)
        [property: JsonPropertyName("n")] string Titulo,
        [property: JsonPropertyName("k")] string? Codigo,
        [property: JsonPropertyName("f")] List<string[]> Filas,
        [property: JsonPropertyName("x")] string[]? Total,
        [property: JsonPropertyName("q")] List<string[]>? Formula,      // fichas de la formula: [texto, fondo, letra]
        [property: JsonPropertyName("o")] List<string[]>? Operaciones,  // operaciones de la ruta: [nombre, detalle]
        [property: JsonPropertyName("w")] List<string>? Avisos,
        [property: JsonPropertyName("p")] bool? Pivote = null,                 // insumo pivote
        [property: JsonPropertyName("zt")] string? TizadoTitulo = null,        // tizado de la version: titulo y filas
        [property: JsonPropertyName("z")] List<string[]>? Tizado = null);

    private static readonly JsonSerializerOptions Opciones = new() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

    private static string N(decimal? v, string formato) => v is null ? "" : v.Value.ToString(formato, CultureInfo.CurrentCulture);
    private static string[]? Costo(string etiqueta, decimal? v) => v is null ? null : [etiqueta, N(v, "N4")];
    private static string Si(bool? v) => v == true ? "Sí" : "No";
    private static string Dv(string? d, int? v) => $"{d} · v{v}";

    /// <summary>Clave de una linea de insumo (la misma que usan las filas de formula).</summary>
    public static string ClaveLinea(string d, int v, string? s, string? c, int? n, string? p) => $"{d}|{v}|{s}|{c}|{n}|{p}";

    private const int MaxOperaciones = 14;

    private static string DetalleOperacion(DisenosEstructuraNodo o)
    {
        var p = new List<string>();
        if (o.CAN_LEAD_TIME is not null) p.Add($"{o.CAN_LEAD_TIME.Value.ToString("0.##", CultureInfo.CurrentCulture)} {o.COD_UNIDAD_MEDIDA_TIEMPO}".Trim());
        if (o.FLG_MANUAL == true) p.Add("Manual");
        if (o.FLG_CONTROL_CALIDAD == true) p.Add("Calidad");
        return string.Join(" · ", p);
    }

    /// <summary>Datos de la ficha de un nodo, como JSON para el atributo data-ficha. <paramref name="formula"/>: las filas de formula de la linea
    /// (nodos INS); <paramref name="operaciones"/>: las operaciones que cuelgan del nodo (ruta, proceso o subproceso), en orden.</summary>
    public static string Json(DisenosEstructuraNodo n, IReadOnlyList<DisenosEstructuraFormulaDiagrama>? formula = null, IReadOnlyList<DisenosEstructuraNodo>? operaciones = null,
        DisenosEstructuraIndicador? linea = null, DisenosEstructuraIndicador? tizado = null)
    {
        var f = new List<string[]>();
        var avisos = new List<string>();
        void Fila(string etiqueta, string? valor) { if (!string.IsNullOrWhiteSpace(valor)) f.Add([etiqueta, valor]); }   // lo que no tiene dato no ocupa renglon
        string tipo, clase;
        string? codigo = null;
        string[]? total = null;
        List<string[]>? chips = null, ops = null;
        switch (n.TIPO_NODO)
        {
            case "VER":
                tipo = "Versión"; clase = "ver";
                Fila("Diseño", Dv(n.COD_DISENO, n.NUM_VERSION));
                Fila("Estado", n.FLG_BLOQUEADO ? "Bloqueada" : "Editable");
                total = Costo("Costo total", n.COSTO_TOTAL);
                break;
            case "SEC":
                tipo = "Sector"; clase = "sec"; codigo = n.COD_SECTOR_PRODUCTO;
                if (n.FLG_TOTAL_PRODUCTO is bool tp) Fila("Aplica a", tp ? "Producto completo" : "Una parte del producto");
                total = Costo("Costo", n.COSTO_TOTAL);
                break;
            case "COM":
                tipo = "Componente"; clase = "com"; codigo = n.COD_COMPONENTE_PRODUCTO;
                Fila("Sector", n.COD_SECTOR_PRODUCTO ?? "");
                total = Costo("Costo", n.COSTO_TOTAL);
                break;
            case "RUT":
                tipo = "Ruta de producción"; clase = "rut"; codigo = n.COD_RUTA_PRODUCCION;
                Fila("Diseño", Dv(n.COD_DISENO, n.NUM_VERSION));
                break;
            case "PRC":
                tipo = "Proceso"; clase = "rut"; codigo = n.COD_PROCESO;
                break;
            case "SPR":
                tipo = "Subproceso"; clase = "rut"; codigo = n.COD_SUB_PROCESO;
                Fila("Proceso", n.COD_PROCESO ?? "");
                break;
            case "OPE":
                tipo = "Operación"; clase = "rut"; codigo = n.COD_TIPO_OPERACION_PRODUCCION;
                Fila("Proceso", n.COD_PROCESO ?? "");
                Fila("Subproceso", n.COD_SUB_PROCESO ?? "");
                if (n.CAN_LEAD_TIME is not null) Fila("Tiempo", $"{n.CAN_LEAD_TIME.Value.ToString("0.##", CultureInfo.CurrentCulture)} {n.COD_UNIDAD_MEDIDA_TIEMPO}".Trim());
                Fila("Manual", Si(n.FLG_MANUAL));
                Fila("Control de calidad", Si(n.FLG_CONTROL_CALIDAD));
                break;
            default: // INS: insumo o subensamble
                var ens = n.FLG_ENSAMBLE;
                tipo = ens ? "Subensamble" : "Insumo"; clase = ens ? "ens" : "ins"; codigo = n.COD_PRODUCTO_COMPONENTE;
                if (n.CAN_CONSUMO is not null) Fila("Consumo", $"{N(n.CAN_CONSUMO, "0.######")} {n.COD_UNIDAD_MEDIDA}".Trim());
                if (n.TAS_MERMA is not null) Fila("Merma", $"{N(n.TAS_MERMA, "0.###")} %");
                if (n.CANTIDAD_ACUMULADA is not null) Fila("Cantidad acumulada", N(n.CANTIDAD_ACUMULADA, "0.######"));
                if (ens)
                {
                    Fila("Subensamble", Dv(n.COD_DISENO_ENSAMBLE, n.NUM_VERSION_ENSAMBLE));
                    Fila("Precio", "Toma el costo de su estructura");
                }
                else if (n.PRE_UNITARIO is not null) Fila("Precio unitario", $"{N(n.PRE_UNITARIO, "N4")} {n.COD_MONEDA}".Trim());
                if (!string.IsNullOrWhiteSpace(linea?.DES_TIPO_MATERIAL)) Fila("Tipo de material", linea!.DES_TIPO_MATERIAL);
                if (formula is { Count: > 0 })
                {
                    if (formula[0].CAN_TOTAL is not null) Fila("Cantidad total", N(formula[0].CAN_TOTAL, "0.######"));
                    chips = formula.Where(x => !string.IsNullOrWhiteSpace(x.DES_ELEMENTO_MASCARA_FORMULA))
                                   .Select(x => new[] { x.DES_ELEMENTO_MASCARA_FORMULA!, x.DES_BACKCOLOR ?? "", x.DES_FORECOLOR ?? "" }).ToList();
                    if (chips.Count == 0 && !string.IsNullOrWhiteSpace(formula[0].DES_MASCARA_FORMULA)) chips = [[formula[0].DES_MASCARA_FORMULA!, "", ""]];   // linea sin fichas: el texto completo
                }
                total = Costo("Costo", n.COSTO_TOTAL);
                break;
        }
        if (n.TIPO_NODO is "RUT" or "PRC" or "SPR" && operaciones is { Count: > 0 })
        {
            ops = operaciones.Take(MaxOperaciones).Select(o => new[] { o.DESCRIPCION ?? o.COD_TIPO_OPERACION_PRODUCCION ?? "", DetalleOperacion(o) }).ToList();
            if (operaciones.Count > MaxOperaciones) ops.Add([$"y {operaciones.Count - MaxOperaciones} más", ""]);
        }
        // lo que viene de un subensamble dice de que diseno y a que nivel pertenece
        if (n.TIPO_NODO is "SEC" or "COM" or "INS" && n.NIVEL_ENSAMBLE > 1)
        {
            Fila("Nivel de ensamble", n.NIVEL_ENSAMBLE.ToString(CultureInfo.InvariantCulture));
            Fila("Pertenece a", Dv(n.COD_DISENO, n.NUM_VERSION));
        }
        if (n.FLG_CICLO) avisos.Add("No se expande: formaría un ciclo.");
        if (n.FLG_TRUNCADO) avisos.Add("No se expande: supera el máximo de niveles.");
        // tizado (de la version) e insumo pivote
        string? tzTitulo = null; List<string[]>? tz = null;
        if (tizado is { NUM_TIZADOS: > 0 })
        {
            tzTitulo = $"Tizado ({tizado.NUM_TIZADOS} guardado{(tizado.NUM_TIZADOS == 1 ? "" : "s")})";
            tz = new();
            void Z(string e, string? v) { if (!string.IsNullOrWhiteSpace(v)) tz.Add([e, v]); }
            Z("Último", tizado.DES_TIZADO);
            Z("Ancho de tela", N(tizado.VAL_ANCHO_TELA, "0.##"));
            Z("Eficiencia", tizado.VAL_EFICIENCIA is null ? "" : N(tizado.VAL_EFICIENCIA, "0.##") + " %");
            Z("Consumo por prenda", N(tizado.VAL_CONSUMO_PRENDA, "0.####"));
            Z("Prendas", tizado.NUM_PRENDAS?.ToString(CultureInfo.InvariantCulture));
            Z("Tallas", tizado.DES_TALLAS);
            Z("Guardado", tizado.FEC_TIZADO?.ToString("dd/MM/yyyy HH:mm", CultureInfo.CurrentCulture));
        }
        var pivote = linea?.FLG_PIVOT == true ? true : (bool?)null;
        return JsonSerializer.Serialize(new FichaDto(tipo, clase, n.DESCRIPCION ?? "", string.IsNullOrWhiteSpace(codigo) ? null : codigo, f, total, chips is { Count: > 0 } ? chips : null, ops, avisos.Count == 0 ? null : avisos, pivote, tzTitulo, tz), Opciones);
    }

    /// <summary>Texto plano equivalente (para lectores de pantalla: aria-label de la caja).</summary>
    public static string Texto(string fichaJson)
    {
        var d = JsonSerializer.Deserialize<JsonElement>(fichaJson);
        var partes = new List<string> { $"{d.GetProperty("t").GetString()}: {d.GetProperty("n").GetString()}" };
        foreach (var fila in d.GetProperty("f").EnumerateArray()) partes.Add($"{fila[0].GetString()} {fila[1].GetString()}");
        if (d.TryGetProperty("q", out var q)) partes.Add("Fórmula " + string.Join(" ", q.EnumerateArray().Select(c => c[0].GetString())));
        if (d.TryGetProperty("p", out _)) partes.Add("Insumo pivote");
        if (d.TryGetProperty("zt", out var zt)) partes.Add(zt.GetString() ?? "Tizado");
        if (d.TryGetProperty("o", out var o)) partes.Add("Operaciones " + string.Join(", ", o.EnumerateArray().Select(c => c[0].GetString())));
        if (d.TryGetProperty("x", out var x)) partes.Add($"{x[0].GetString()} {x[1].GetString()}");
        return string.Join(". ", partes);
    }

    /// <summary>Instala (una sola vez) el script de la ficha y oculta la que hubiera abierta (el diagrama se redibujo).</summary>
    public static async Task IniciarAsync(IJSRuntime js)
    {
        try { await js.InvokeVoidAsync("dgmFicha.ocultar"); }
        catch (JSException) { await js.InvokeVoidAsync("eval", Script); }
    }

    public const string Script = """
(function () {
    if (window.dgmFicha) return;
    var F = window.dgmFicha = { host: null, actual: null, timer: 0, x: 0, y: 0, pend: false, visible: false };

    function el(tag, cls, txt) {
        var e = document.createElement(tag);
        if (cls) e.className = cls;
        if (txt !== undefined && txt !== null) e.textContent = txt;      // siempre textContent: los datos nunca se interpretan como HTML
        return e;
    }

    function fila(cls, etiqueta, valor) {
        var r = el('div', cls);
        r.appendChild(el('span', 'dgm-ficha-et', etiqueta));
        r.appendChild(el('span', 'dgm-ficha-val', valor));
        return r;
    }

    function construir(d) {
        var card = el('div', 'dgm-ficha dgm-ficha-' + (d.c || 'ins'));
        card.appendChild(el('div', 'dgm-ficha-cab')).appendChild(el('span', 'dgm-ficha-tipo', d.t || ''));
        card.appendChild(el('div', 'dgm-ficha-titulo', d.n || ''));
        if (d.k) card.appendChild(el('div', 'dgm-ficha-cod', d.k));
        if (d.f && d.f.length) {
            var filas = el('div', 'dgm-ficha-filas');
            d.f.forEach(function (r) { filas.appendChild(fila('dgm-ficha-fila', r[0], r[1])); });
            card.appendChild(filas);
        }
        if (d.q && d.q.length) {
            var fo = el('div', 'dgm-ficha-form');
            fo.appendChild(el('div', 'dgm-ficha-sub', 'Fórmula'));
            var cj = el('div', 'dgm-ficha-chips');
            d.q.forEach(function (c) {
                var ch = el('span', 'dgm-ficha-chip', c[0]);
                if (/^#?[0-9a-fA-F]{3,8}$/.test(c[1] || '')) ch.style.background = c[1].charAt(0) === '#' ? c[1] : '#' + c[1];      // solo colores hex: nunca texto libre como estilo
                if (/^#?[0-9a-fA-F]{3,8}$/.test(c[2] || '')) ch.style.color = c[2].charAt(0) === '#' ? c[2] : '#' + c[2];
                cj.appendChild(ch);
            });
            fo.appendChild(cj);
            card.appendChild(fo);
        }
        if (d.p) {
            var pv = el('div', 'dgm-ficha-form');
            pv.appendChild(el('div', 'dgm-ficha-sub', 'Insumo pivote'));
            pv.appendChild(el('div', 'dgm-ficha-nota', 'Sus datos se registran; no se calcula. El componente superior se calcula en base a este.'));
            card.appendChild(pv);
        }
        if (d.z && d.z.length) {
            var tz = el('div', 'dgm-ficha-form');
            tz.appendChild(el('div', 'dgm-ficha-sub', d.zt || 'Tizado'));
            d.z.forEach(function (r) { tz.appendChild(fila('dgm-ficha-fila', r[0], r[1])); });
            card.appendChild(tz);
        }
        if (d.o && d.o.length) {
            var op = el('div', 'dgm-ficha-form');
            op.appendChild(el('div', 'dgm-ficha-sub', 'Operaciones'));
            d.o.forEach(function (r) { op.appendChild(fila('dgm-ficha-fila', r[0], r[1])); });
            card.appendChild(op);
        }
        if (d.x) card.appendChild(fila('dgm-ficha-total', d.x[0], d.x[1]));
        (d.w || []).forEach(function (a) { card.appendChild(el('div', 'dgm-ficha-aviso', a)); });
        return card;
    }

    function host() {
        if (!F.host || !F.host.isConnected) {
            var h = document.createElement('div');
            h.className = 'dgm-ficha-host';
            h.style.cssText = 'position:fixed;left:0;top:0;width:max-content;max-width:380px;z-index:100000;pointer-events:none;visibility:hidden;';
            document.body.appendChild(h);
            F.host = h;
        }
        return F.host;
    }

    // junto al puntero, sin salirse de la pantalla (coordenadas reales: la tarjeta esta fuera de #app, no la afecta el zoom de la app)
    function ubicar() {
        F.pend = false;
        if (!F.visible || !F.host) return;
        var r = F.host.getBoundingClientRect(), vw = window.innerWidth, vh = window.innerHeight;
        var x = F.x + 16, y = F.y + 18;
        if (x + r.width > vw - 6) x = F.x - r.width - 14;
        if (y + r.height > vh - 6) y = F.y - r.height - 14;
        F.host.style.left = Math.max(6, x) + 'px';
        F.host.style.top = Math.max(6, y) + 'px';
    }

    function mostrar(g) {
        var d;
        try { d = JSON.parse(g.getAttribute('data-ficha')); } catch (e) { return; }
        var h = host();
        h.textContent = '';
        h.appendChild(construir(d));
        F.visible = true;
        ubicar();
        h.style.visibility = 'visible';
    }

    F.ocultar = function () {
        clearTimeout(F.timer);
        F.actual = null; F.visible = false;
        if (F.host) { F.host.style.visibility = 'hidden'; F.host.textContent = ''; }
    };

    document.addEventListener('mouseover', function (e) {
        var t = e.target, g = t && t.closest ? t.closest('[data-ficha]') : null;
        if (!g) { if (F.actual) F.ocultar(); return; }
        F.x = e.clientX; F.y = e.clientY;
        if (g === F.actual) return;                                    // paso de un hijo de la misma caja a otro
        F.ocultar();
        F.actual = g;
        F.timer = setTimeout(function () { if (F.actual === g && g.isConnected) mostrar(g); }, 120);   // evita el parpadeo al cruzar cajas rapido
    }, true);

    document.addEventListener('mousemove', function (e) {
        if (!F.actual) return;
        F.x = e.clientX; F.y = e.clientY;
        if (!F.actual.isConnected) { F.ocultar(); return; }
        if (F.visible && !F.pend) { F.pend = true; requestAnimationFrame(ubicar); }
    }, true);

    document.addEventListener('mouseout', function (e) { if (!e.relatedTarget) F.ocultar(); }, true);      // el puntero salio de la ventana
    document.addEventListener('keydown', function (e) { if (e.key === 'Escape') F.ocultar(); }, true);
    window.addEventListener('scroll', function () { F.ocultar(); }, true);
    window.addEventListener('blur', function () { F.ocultar(); });
    window.addEventListener('kon:zoom', function () { F.ocultar(); });
})();
""";
}
