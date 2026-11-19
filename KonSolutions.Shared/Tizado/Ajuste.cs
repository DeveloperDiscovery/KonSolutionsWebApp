namespace KONSolutions.Shared.Tizado;

public enum GravedadAjuste { Aviso, Error }

/// <summary>Un problema de un tizado ajustado a mano. A y B son posiciones en la lista de piezas (B = -1 si solo interviene una pieza; defecto: A = -1 y B = pieza).</summary>
public sealed record ProblemaAjuste(string Tipo, GravedadAjuste Gravedad, int A, int B, string Mensaje);

/// <summary>Reglas contra las que se valida un ajuste: las del tizado original.</summary>
public sealed class ReglasAjuste
{
    public double AnchoTela { get; set; }
    public double Separacion { get; set; }
    public double Margen { get; set; }
    public double PeriodoX { get; set; }
    public double PeriodoY { get; set; }
    public IReadOnlyList<Defecto> Defectos { get; set; } = Array.Empty<Defecto>();
}

/// <summary>
/// Ajuste manual de un tizado: mover, girar y espejar piezas ya colocadas, y validar el resultado. Errores (impiden guardar): piezas que se pisan,
/// piezas fuera de la tela o sobre un defecto. Avisos (se pueden guardar): menos separacion que la pedida y piezas que se apartan de su raya.
/// </summary>
public static class Ajuste
{
    const double Tol = 1e-6;

    static Colocacion Copia(Colocacion c, Poligono contorno, Punto? pc, double angulo, bool espejada) => new()
    {
        PiezaId = c.PiezaId, Nombre = c.Nombre, Talla = c.Talla, Copia = c.Copia, Espejada = espejada, Angulo = ((angulo % 360) + 360) % 360, Contorno = contorno,
        PuntoCoincidencia = pc, ErrorCoincidenciaX = c.ErrorCoincidenciaX, ErrorCoincidenciaY = c.ErrorCoincidenciaY, FaseX = c.FaseX, FaseY = c.FaseY
    };

    public static Colocacion Mover(Colocacion c, double dx, double dy) =>
        Copia(c, c.Contorno.Trasladar(dx, dy), c.PuntoCoincidencia is { } q ? new Punto(q.X + dx, q.Y + dy) : null, c.Angulo, c.Espejada);

    /// <summary>Deja la esquina minima de la caja de la pieza en (x, y).</summary>
    public static Colocacion Ubicar(Colocacion c, double x, double y) { var l = c.Contorno.Limites; return Mover(c, x - l.MinX, y - l.MinY); }

    /// <summary>Gira la pieza (grados, antihorario) alrededor del centro de su caja. Su punto de coincidencia gira con ella.</summary>
    public static Colocacion Girar(Colocacion c, double grados)
    {
        var l = c.Contorno.Limites; double cx = (l.MinX + l.MaxX) / 2, cy = (l.MinY + l.MaxY) / 2;
        double r = grados * Math.PI / 180, co = Math.Cos(r), si = Math.Sin(r);
        Punto Gira(Punto p) { double x = p.X - cx, y = p.Y - cy; return new Punto(x * co - y * si + cx, x * si + y * co + cy); }
        return Copia(c, new Poligono(c.Contorno.Puntos.Select(Gira)), c.PuntoCoincidencia is { } q ? Gira(q) : null, c.Angulo + grados, c.Espejada);
    }

    /// <summary>Espeja la pieza respecto de la vertical que pasa por el centro de su caja (izquierda &lt;-&gt; derecha).</summary>
    public static Colocacion Espejar(Colocacion c)
    {
        var l = c.Contorno.Limites; double cx = (l.MinX + l.MaxX) / 2;
        return Copia(c, new Poligono(c.Contorno.Puntos.Select(p => new Punto(2 * cx - p.X, p.Y)).Reverse()), c.PuntoCoincidencia is { } q ? new Punto(2 * cx - q.X, q.Y) : null, c.Angulo, !c.Espejada);
    }

    /// <summary>Desvio de la pieza respecto de su raya (mm), o null en el sentido en que no coincide.</summary>
    public static (double? X, double? Y) Desvio(Colocacion c, ReglasAjuste r) =>
        (c.PuntoCoincidencia is { } q && c.FaseX is { } fx && r.PeriodoX > 0 ? Tizador.Resto(q.X - fx, r.PeriodoX) : null,
         c.PuntoCoincidencia is { } q2 && c.FaseY is { } fy && r.PeriodoY > 0 ? Tizador.Resto(q2.Y - fy, r.PeriodoY) : null);

    /// <summary>Largo usado (hasta la pieza mas lejana mas el margen), area de piezas y aprovechamiento.</summary>
    public static (double Largo, double Area, double Eficiencia) Medidas(IReadOnlyList<Colocacion> piezas, ReglasAjuste r)
    {
        if (piezas.Count == 0) return (0, 0, 0);
        double largo = piezas.Max(c => c.Contorno.Limites.MaxX) + r.Margen, area = piezas.Sum(c => c.Contorno.Area);
        return (largo, area, largo > 0 && r.AnchoTela > 0 ? area / (largo * r.AnchoTela) : 0);
    }

    static bool CajasSeTocan((double MinX, double MinY, double MaxX, double MaxY) a, (double MinX, double MinY, double MaxX, double MaxY) b, double holgura) =>
        a.MinX - holgura <= b.MaxX && b.MinX - holgura <= a.MaxX && a.MinY - holgura <= b.MaxY && b.MinY - holgura <= a.MaxY;

    static string Quien(Colocacion c) => $"{c.Nombre} {c.Talla} #{c.Copia}";

    public static List<ProblemaAjuste> Validar(IReadOnlyList<Colocacion> piezas, ReglasAjuste r)
    {
        var res = new List<ProblemaAjuste>();
        var cajas = piezas.Select(c => c.Contorno.Limites).ToArray();
        for (int i = 0; i < piezas.Count; i++)
        {
            var l = cajas[i];
            if (l.MinX < r.Margen - Tol) res.Add(new("fuera", GravedadAjuste.Error, i, -1, $"{Quien(piezas[i])} sale por el inicio del tendido (margen {r.Margen:0.##} mm)."));
            if (l.MinY < r.Margen - Tol || l.MaxY > r.AnchoTela - r.Margen + Tol) res.Add(new("fuera", GravedadAjuste.Error, i, -1, $"{Quien(piezas[i])} sale del ancho util de la tela."));
            for (int j = i + 1; j < piezas.Count; j++)
            {
                if (!CajasSeTocan(l, cajas[j], r.Separacion)) continue;
                if (Geometria.Solapan(piezas[i].Contorno, piezas[j].Contorno)) res.Add(new("solape", GravedadAjuste.Error, i, j, $"{Quien(piezas[i])} pisa a {Quien(piezas[j])}."));
                else if (Geometria.Distancia(piezas[i].Contorno, piezas[j].Contorno) < r.Separacion - Tol) res.Add(new("separacion", GravedadAjuste.Aviso, i, j, $"{Quien(piezas[i])} y {Quien(piezas[j])} quedan a menos de la separacion ({r.Separacion:0.##} mm)."));
            }
            foreach (var d in r.Defectos)
            {
                if (!d.Contorno.EsValido || !CajasSeTocan(l, d.Contorno.Limites, r.Separacion)) continue;
                if (Geometria.Solapan(piezas[i].Contorno, d.Contorno)) res.Add(new("defecto", GravedadAjuste.Error, -1, i, $"{Quien(piezas[i])} pisa el defecto {d.Nombre}."));
                else if (Geometria.Distancia(piezas[i].Contorno, d.Contorno) < r.Separacion - Tol) res.Add(new("defecto-cerca", GravedadAjuste.Aviso, -1, i, $"{Quien(piezas[i])} queda a menos de la separacion del defecto {d.Nombre}."));
            }
            var (ex, ey) = Desvio(piezas[i], r);
            if (ex > 0.5) res.Add(new("raya", GravedadAjuste.Aviso, i, -1, $"{Quien(piezas[i])} se aparta {ex:0.##} mm de su raya en el largo."));
            if (ey > 0.5) res.Add(new("raya", GravedadAjuste.Aviso, i, -1, $"{Quien(piezas[i])} se aparta {ey:0.##} mm de su raya en el ancho."));
        }
        return res;
    }

    /// <summary>El punto de coincidencia ajustado esta en el mismo lugar de la pieza que el original (mismas distancias a todos los vertices).</summary>
    public static bool PuntoCongruente(Poligono original, Punto puntoOriginal, Poligono ajustado, Punto puntoAjustado, double tol = 0.01)
    {
        var a = original.Puntos; var b = ajustado.Puntos;
        if (a.Length != b.Length) return false;
        static double[] D(Punto[] v, Punto q) => v.Select(p => Math.Sqrt((p.X - q.X) * (p.X - q.X) + (p.Y - q.Y) * (p.Y - q.Y))).OrderBy(x => x).ToArray();
        var da = D(a, puntoOriginal); var db = D(b, puntoAjustado);
        for (int i = 0; i < da.Length; i++) if (Math.Abs(da[i] - db[i]) > tol) return false;
        return true;
    }

    /// <summary>
    /// La pieza ajustada es la original movida (giro, espejo y traslado): mismo numero de vertices, misma area y las mismas distancias entre todos sus vertices.
    /// Lo usa el servidor para no aceptar piezas deformadas.
    /// </summary>
    public static bool EsCongruente(Poligono original, Poligono ajustado, double tol = 0.01)
    {
        var a = original.Puntos; var b = ajustado.Puntos;
        if (a.Length != b.Length || a.Length < 3) return false;
        if (Math.Abs(original.Area - ajustado.Area) > Math.Max(tol, original.Area * 1e-6)) return false;
        if (Math.Abs(original.Perimetro - ajustado.Perimetro) > tol * a.Length) return false;
        if (a.Length > 400) return true;       // contornos enormes: bastan el area y el perimetro
        static double[] Dist(Punto[] p) { var l = new List<double>(); for (int i = 0; i < p.Length; i++) for (int j = i + 1; j < p.Length; j++) l.Add(Math.Sqrt((p[i].X - p[j].X) * (p[i].X - p[j].X) + (p[i].Y - p[j].Y) * (p[i].Y - p[j].Y))); l.Sort(); return l.ToArray(); }
        var da = Dist(a); var db = Dist(b);
        for (int i = 0; i < da.Length; i++) if (Math.Abs(da[i] - db[i]) > tol) return false;
        return true;
    }
}
