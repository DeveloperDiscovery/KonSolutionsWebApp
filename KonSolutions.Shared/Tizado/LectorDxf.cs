using System.Globalization;

namespace KONSolutions.Shared.Tizado;

public sealed class ContornoDxf
{
    public string Capa { get; init; } = "";
    public Poligono Contorno { get; init; } = new(Array.Empty<Punto>());
}

public sealed class ResultadoDxf
{
    public List<ContornoDxf> Contornos { get; } = new();
    public List<string> Advertencias { get; } = new();
    /// <summary>Milimetros por unidad del archivo (de $INSUNITS o la indicada).</summary>
    public double MilimetrosPorUnidad { get; set; } = 1;
}

/// <summary>
/// Lee DXF en texto (ASCII) y devuelve los contornos CERRADOS: LWPOLYLINE, POLYLINE/VERTEX y CIRCLE (los arcos de las polilineas
/// con "bulge" se aproximan con segmentos de ~4 grados). Las polilineas abiertas, lineas sueltas, textos y bloques se ignoran
/// (se informa en Advertencias). Todo se devuelve en milimetros.
/// </summary>
public static class LectorDxf
{
    public static ResultadoDxf Leer(string texto, double? milimetrosPorUnidad = null)
    {
        var res = new ResultadoDxf();
        if (string.IsNullOrWhiteSpace(texto)) { res.Advertencias.Add("El archivo esta vacio."); return res; }
        if (texto.StartsWith("AutoCAD Binary DXF", StringComparison.Ordinal)) { res.Advertencias.Add("DXF binario no soportado: guardalo como DXF de texto (ASCII)."); return res; }

        var lin = texto.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        var pares = new List<(int Codigo, string Valor)>(lin.Length / 2);
        for (int i = 0; i + 1 < lin.Length; i += 2)
        {
            if (!int.TryParse(lin[i].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var cod)) { res.Advertencias.Add($"Archivo DXF mal formado cerca de la linea {i + 1}."); break; }
            pares.Add((cod, lin[i + 1].Trim()));
        }

        // unidades
        double factor = 1;
        for (int i = 0; i + 1 < pares.Count; i++)
            if (pares[i].Codigo == 9 && pares[i].Valor == "$INSUNITS" && pares[i + 1].Codigo == 70 && int.TryParse(pares[i + 1].Valor, out var u))
                factor = u switch { 1 => 25.4, 2 => 304.8, 4 => 1, 5 => 10, 6 => 1000, 3 => 1609344, 7 => 1e6, 8 => 25.4e-6, 9 => 0.0254, 10 => 914.4, _ => 1 };
        if (milimetrosPorUnidad is double f && f > 0) factor = f;
        res.MilimetrosPorUnidad = factor;

        // solo ENTITIES
        int ini = pares.FindIndex(p => p.Codigo == 2 && p.Valor == "ENTITIES");
        if (ini < 0) { res.Advertencias.Add("El archivo no tiene seccion ENTITIES."); return res; }
        int fin = pares.FindIndex(ini, p => p.Codigo == 0 && p.Valor == "ENDSEC"); if (fin < 0) fin = pares.Count;

        int abiertas = 0, otras = 0;
        int k = ini + 1;
        while (k < fin)
        {
            if (pares[k].Codigo != 0) { k++; continue; }
            var tipo = pares[k].Valor;
            int ent = k + 1; int sig = ent; while (sig < fin && pares[sig].Codigo != 0) sig++;   // grupos de esta entidad: [ent, sig)

            switch (tipo)
            {
                case "LWPOLYLINE":
                    {
                        string capa = ""; bool cerrada = false; var v = new List<(double X, double Y, double B)>();
                        for (int i = ent; i < sig; i++)
                        {
                            var (c, val) = pares[i];
                            if (c == 8) capa = val;
                            else if (c == 70) cerrada = (Ent(val) & 1) == 1;
                            else if (c == 10) v.Add((Dbl(val), 0, 0));
                            else if (c == 20 && v.Count > 0) v[^1] = (v[^1].X, Dbl(val), v[^1].B);
                            else if (c == 42 && v.Count > 0) v[^1] = (v[^1].X, v[^1].Y, Dbl(val));
                        }
                        Agregar(res, capa, v, cerrada, factor, ref abiertas);
                        k = sig; break;
                    }
                case "POLYLINE":
                    {
                        string capa = ""; bool cerrada = false; var v = new List<(double X, double Y, double B)>();
                        for (int i = ent; i < sig; i++) { if (pares[i].Codigo == 8) capa = pares[i].Valor; else if (pares[i].Codigo == 70) cerrada = (Ent(pares[i].Valor) & 1) == 1; }
                        int j = sig;                                    // VERTEX ... SEQEND
                        while (j < fin && !(pares[j].Codigo == 0 && pares[j].Valor == "SEQEND"))
                        {
                            if (pares[j].Codigo == 0 && pares[j].Valor == "VERTEX")
                            {
                                double x = 0, y = 0, b = 0; int q = j + 1;
                                while (q < fin && pares[q].Codigo != 0) { if (pares[q].Codigo == 10) x = Dbl(pares[q].Valor); else if (pares[q].Codigo == 20) y = Dbl(pares[q].Valor); else if (pares[q].Codigo == 42) b = Dbl(pares[q].Valor); q++; }
                                v.Add((x, y, b)); j = q;
                            }
                            else j++;
                        }
                        Agregar(res, capa, v, cerrada, factor, ref abiertas);
                        k = j; break;
                    }
                case "CIRCLE":
                    {
                        string capa = ""; double cx = 0, cy = 0, r = 0;
                        for (int i = ent; i < sig; i++) { var (c, val) = pares[i]; if (c == 8) capa = val; else if (c == 10) cx = Dbl(val); else if (c == 20) cy = Dbl(val); else if (c == 40) r = Dbl(val); }
                        if (r > 0)
                        {
                            var pts = new List<Punto>(); for (int i = 0; i < 72; i++) { var a = 2 * Math.PI * i / 72; pts.Add(new Punto((cx + r * Math.Cos(a)) * factor, (cy + r * Math.Sin(a)) * factor)); }
                            res.Contornos.Add(new ContornoDxf { Capa = capa, Contorno = new Poligono(pts) });
                        }
                        k = sig; break;
                    }
                default:
                    if (tipo is "LINE" or "ARC" or "SPLINE" or "TEXT" or "MTEXT" or "POINT" or "INSERT" or "DIMENSION") otras++;
                    k = sig; break;
            }
        }
        if (abiertas > 0) res.Advertencias.Add($"{abiertas} polilinea(s) abierta(s) ignorada(s) (solo se usan contornos cerrados).");
        if (otras > 0) res.Advertencias.Add($"{otras} entidad(es) sueltas (lineas, textos, bloques...) ignoradas.");
        if (res.Contornos.Count == 0) res.Advertencias.Add("No se encontraron contornos cerrados.");
        return res;
    }

    static void Agregar(ResultadoDxf res, string capa, List<(double X, double Y, double B)> v, bool cerrada, double f, ref int abiertas)
    {
        if (v.Count < 2) return;
        // algunos DXF cierran repitiendo el primer punto
        if (!cerrada && v.Count >= 4 && Math.Abs(v[0].X - v[^1].X) < 1e-9 && Math.Abs(v[0].Y - v[^1].Y) < 1e-9) { cerrada = true; v = v.Take(v.Count - 1).ToList(); }
        if (!cerrada) { abiertas++; return; }
        var pts = new List<Punto>();
        for (int i = 0; i < v.Count; i++)
        {
            var a = v[i]; var b = v[(i + 1) % v.Count];
            pts.Add(new Punto(a.X * f, a.Y * f));
            if (Math.Abs(a.B) > 1e-9) pts.AddRange(Arco(a.X, a.Y, b.X, b.Y, a.B).Select(p => new Punto(p.X * f, p.Y * f)));
        }
        var poli = new Poligono(pts);
        if (poli.EsValido) res.Contornos.Add(new ContornoDxf { Capa = capa, Contorno = poli });
    }

    /// <summary>Puntos intermedios (sin los extremos) del arco definido por el bulge entre p0 y p1.</summary>
    static IEnumerable<Punto> Arco(double x0, double y0, double x1, double y1, double bulge)
    {
        double dx = x1 - x0, dy = y1 - y0, c = Math.Sqrt(dx * dx + dy * dy);
        if (c < 1e-12) yield break;
        double theta = 4 * Math.Atan(bulge);                          // con signo
        double d = (c / 2) * (1 - bulge * bulge) / (2 * bulge);        // distancia del centro a la cuerda (hacia la izquierda)
        double mx = (x0 + x1) / 2, my = (y0 + y1) / 2;
        double nx = -dy / c, ny = dx / c;
        double cx = mx + nx * d, cy = my + ny * d;
        double a0 = Math.Atan2(y0 - cy, x0 - cx), r = Math.Sqrt((x0 - cx) * (x0 - cx) + (y0 - cy) * (y0 - cy));
        int n = Math.Max(2, (int)Math.Ceiling(Math.Abs(theta) / (4 * Math.PI / 180)));
        for (int i = 1; i < n; i++) { var a = a0 + theta * i / n; yield return new Punto(cx + r * Math.Cos(a), cy + r * Math.Sin(a)); }
    }

    static double Dbl(string s) => double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var d) ? d : 0;
    static int Ent(string s) => int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var d) ? d : 0;
}
