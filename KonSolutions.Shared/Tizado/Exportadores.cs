using System.Globalization;
using System.Net;
using System.Text;

namespace KONSolutions.Shared.Tizado;

public static class Exportadores
{
    static string N(double d) => d.ToString("0.##", CultureInfo.InvariantCulture);
    static readonly string[] Paleta = { "#e8b4b8", "#b4c7e8", "#b8e8c0", "#e8dcb4", "#cdb4e8", "#b4e8e4", "#e8c8b4", "#c8e8b4" };

    /// <summary>SVG del tizado (mm). Cada talla toma un color; el rotulo lleva nombre y talla.</summary>
    public static string ASvg(ResultadoTizado r, bool rotulos = true, IReadOnlyList<Defecto>? defectos = null, double periodoX = 0, double periodoY = 0)
    {
        var largo = Math.Max(r.Largo, 1);
        var tallas = r.Colocadas.Select(c => c.Talla).Distinct().ToList();
        var sb = new StringBuilder();
        sb.Append($"<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 {N(largo)} {N(r.Ancho)}\" width=\"100%\" preserveAspectRatio=\"xMinYMin meet\">");
        sb.Append($"<rect x=\"0\" y=\"0\" width=\"{N(largo)}\" height=\"{N(r.Ancho)}\" fill=\"none\" stroke=\"#888\" stroke-width=\"2\"/>");
        // rayas de la tela (referencia visual): solo si no son demasiadas
        void Rayas(double periodo, double largoTotal, bool vertical)
        {
            if (periodo <= 0 || largoTotal / periodo > 400) return;
            for (double v = 0; v <= largoTotal + 1e-9; v += periodo)
                sb.Append(vertical ? $"<line x1=\"{N(v)}\" y1=\"0\" x2=\"{N(v)}\" y2=\"{N(r.Ancho)}\" stroke=\"#6a8fd8\" stroke-width=\"1\" stroke-dasharray=\"6 6\" opacity=\"0.55\" data-raya=\"x\"/>"
                                  : $"<line x1=\"0\" y1=\"{N(v)}\" x2=\"{N(largo)}\" y2=\"{N(v)}\" stroke=\"#6a8fd8\" stroke-width=\"1\" stroke-dasharray=\"6 6\" opacity=\"0.55\" data-raya=\"y\"/>");
        }
        Rayas(periodoX, largo, true); Rayas(periodoY, r.Ancho, false);
        foreach (var c in r.Colocadas)
        {
            var color = Paleta[Math.Max(0, tallas.IndexOf(c.Talla)) % Paleta.Length];
            sb.Append($"<polygon points=\"{c.Contorno.ATexto()}\" fill=\"{color}\" stroke=\"#333\" stroke-width=\"1\" data-pieza=\"{WebUtility.HtmlEncode(c.PiezaId)}\"><title>{WebUtility.HtmlEncode(c.Nombre)} {WebUtility.HtmlEncode(c.Talla)}{(c.Espejada ? " (espejo)" : "")}</title></polygon>");
            if (rotulos)
            {
                var l = c.Contorno.Limites; var fs = Math.Max(8, Math.Min(l.MaxX - l.MinX, l.MaxY - l.MinY) / 8);
                sb.Append($"<text x=\"{N((l.MinX + l.MaxX) / 2)}\" y=\"{N((l.MinY + l.MaxY) / 2)}\" font-size=\"{N(fs)}\" text-anchor=\"middle\" fill=\"#222\">{WebUtility.HtmlEncode(c.Nombre)}</text>");
            }
        }
        foreach (var d in defectos ?? Array.Empty<Defecto>())
            sb.Append($"<polygon points=\"{d.Contorno.ATexto()}\" fill=\"#d62728\" fill-opacity=\"0.4\" stroke=\"#d62728\" stroke-width=\"1.5\" data-defecto=\"{WebUtility.HtmlEncode(d.Nombre)}\"><title>Defecto: {WebUtility.HtmlEncode(d.Nombre)}</title></polygon>");
        foreach (var c in r.Colocadas.Where(c => c.PuntoCoincidencia is not null))
        {
            var q = c.PuntoCoincidencia!.Value; var l = c.Contorno.Limites; var rad = Math.Max(3, Math.Min(l.MaxX - l.MinX, l.MaxY - l.MinY) / 25);
            sb.Append($"<circle cx=\"{N(q.X)}\" cy=\"{N(q.Y)}\" r=\"{N(rad)}\" fill=\"#1f4fd8\" stroke=\"#fff\" stroke-width=\"1\" data-coincide=\"{WebUtility.HtmlEncode(c.PiezaId)}\"/>");
        }
        sb.Append("</svg>");
        return sb.ToString();
    }

    /// <summary>DXF de texto (R12): un POLYLINE cerrado por pieza, en la capa "PIEZAS", y el contorno de la tela en "TELA". Unidades: mm.</summary>
    public static string ADxf(ResultadoTizado r, IReadOnlyList<Defecto>? defectos = null)
    {
        var sb = new StringBuilder();
        void P(int c, object v) { sb.Append(c.ToString(CultureInfo.InvariantCulture)).Append('\n').Append(v is double d ? d.ToString("0.####", CultureInfo.InvariantCulture) : v.ToString()).Append('\n'); }
        P(0, "SECTION"); P(2, "HEADER"); P(9, "$INSUNITS"); P(70, 4); P(0, "ENDSEC");
        P(0, "SECTION"); P(2, "ENTITIES");
        void Poli(string capa, IEnumerable<Punto> pts)
        {
            P(0, "POLYLINE"); P(8, capa); P(66, 1); P(70, 1);
            foreach (var p in pts) { P(0, "VERTEX"); P(8, capa); P(10, p.X); P(20, p.Y); }
            P(0, "SEQEND"); P(8, capa);
        }
        Poli("TELA", new[] { new Punto(0, 0), new Punto(r.Largo, 0), new Punto(r.Largo, r.Ancho), new Punto(0, r.Ancho) });
        foreach (var c in r.Colocadas) Poli("PIEZAS", c.Contorno.Puntos);
        foreach (var d in defectos ?? Array.Empty<Defecto>()) Poli("DEFECTOS", d.Contorno.Puntos);
        P(0, "ENDSEC"); P(0, "EOF");
        return sb.ToString();
    }
}
