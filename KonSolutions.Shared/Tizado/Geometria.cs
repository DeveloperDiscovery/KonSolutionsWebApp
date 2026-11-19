using System.Globalization;

namespace KONSolutions.Shared.Tizado;

/// <summary>Punto 2D en milimetros. X = largo de la tela (sentido del hilo), Y = ancho de la tela.</summary>
public readonly record struct Punto(double X, double Y);

/// <summary>Contorno cerrado de una pieza (sin repetir el primer punto al final).</summary>
public sealed class Poligono
{
    public Punto[] Puntos { get; }
    public Poligono(IEnumerable<Punto> puntos) { Puntos = Limpiar(puntos); }

    public int Cantidad => Puntos.Length;
    public bool EsValido => Puntos.Length >= 3 && Math.Abs(AreaConSigno) > 1e-9;

    /// <summary>Quita puntos repetidos consecutivos y el cierre duplicado.</summary>
    static Punto[] Limpiar(IEnumerable<Punto> src)
    {
        var l = new List<Punto>();
        foreach (var p in src)
            if (l.Count == 0 || !Igual(l[^1], p)) l.Add(p);
        while (l.Count > 1 && Igual(l[0], l[^1])) l.RemoveAt(l.Count - 1);
        return l.ToArray();
    }
    static bool Igual(Punto a, Punto b) => Math.Abs(a.X - b.X) < 1e-9 && Math.Abs(a.Y - b.Y) < 1e-9;

    /// <summary>Area con signo (positiva si el recorrido es antihorario).</summary>
    public double AreaConSigno
    {
        get
        {
            double s = 0;
            for (int i = 0, n = Puntos.Length; i < n; i++)
            {
                var a = Puntos[i]; var b = Puntos[(i + 1) % n];
                s += a.X * b.Y - b.X * a.Y;
            }
            return s / 2;
        }
    }
    public double Area => Math.Abs(AreaConSigno);

    public double Perimetro
    {
        get
        {
            double s = 0;
            for (int i = 0, n = Puntos.Length; i < n; i++)
            {
                var a = Puntos[i]; var b = Puntos[(i + 1) % n];
                s += Math.Sqrt((b.X - a.X) * (b.X - a.X) + (b.Y - a.Y) * (b.Y - a.Y));
            }
            return s;
        }
    }

    public (double MinX, double MinY, double MaxX, double MaxY) Limites
    {
        get
        {
            if (Puntos.Length == 0) return (0, 0, 0, 0);
            double x0 = double.MaxValue, y0 = double.MaxValue, x1 = double.MinValue, y1 = double.MinValue;
            foreach (var p in Puntos) { x0 = Math.Min(x0, p.X); y0 = Math.Min(y0, p.Y); x1 = Math.Max(x1, p.X); y1 = Math.Max(y1, p.Y); }
            return (x0, y0, x1, y1);
        }
    }
    public double Ancho => Limites.MaxX - Limites.MinX;   // en X (largo de tela)
    public double Alto => Limites.MaxY - Limites.MinY;    // en Y (ancho de tela)

    public Poligono Trasladar(double dx, double dy) => new(Puntos.Select(p => new Punto(p.X + dx, p.Y + dy)));
    public Poligono Escalar(double k) => new(Puntos.Select(p => new Punto(p.X * k, p.Y * k)));

    /// <summary>Gira en grados (antihorario) alrededor del origen.</summary>
    public Poligono Rotar(double grados)
    {
        var r = grados * Math.PI / 180; var c = Math.Cos(r); var s = Math.Sin(r);
        return new(Puntos.Select(p => new Punto(p.X * c - p.Y * s, p.X * s + p.Y * c)));
    }

    /// <summary>Espejo respecto del eje Y (X -> -X). Invierte el recorrido para conservar el sentido.</summary>
    public Poligono Espejar() => new(Puntos.Select(p => new Punto(-p.X, p.Y)).Reverse());

    /// <summary>Lo mueve para que su caja quede con la esquina minima en (0,0).</summary>
    public Poligono AlOrigen() { var l = Limites; return Trasladar(-l.MinX, -l.MinY); }

    /// <summary>Gira la pieza para que la linea del hilo (dada por su angulo actual) quede en el sentido del largo de la tela.</summary>
    public Poligono AlinearHilo(double anguloHiloGrados) => Rotar(-anguloHiloGrados).AlOrigen();

    public string ATexto() => string.Join(" ", Puntos.Select(p => p.X.ToString("0.###", CultureInfo.InvariantCulture) + "," + p.Y.ToString("0.###", CultureInfo.InvariantCulture)));
}

public static class Geometria
{
    const double Eps = 1e-7;

    static double Orient(Punto a, Punto b, Punto c) => (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);

    /// <summary>Cruce propio de dos segmentos (el simple contacto o tocarse en un extremo no cuenta).</summary>
    public static bool SegmentosSeCruzan(Punto a, Punto b, Punto c, Punto d)
    {
        var o1 = Orient(a, b, c); var o2 = Orient(a, b, d); var o3 = Orient(c, d, a); var o4 = Orient(c, d, b);
        return ((o1 > Eps && o2 < -Eps) || (o1 < -Eps && o2 > Eps)) && ((o3 > Eps && o4 < -Eps) || (o3 < -Eps && o4 > Eps));
    }

    static double DistPuntoSegmento(Punto p, Punto a, Punto b)
    {
        double dx = b.X - a.X, dy = b.Y - a.Y, l2 = dx * dx + dy * dy;
        double t = l2 < 1e-18 ? 0 : Math.Clamp(((p.X - a.X) * dx + (p.Y - a.Y) * dy) / l2, 0, 1);
        double qx = a.X + t * dx, qy = a.Y + t * dy;
        return Math.Sqrt((p.X - qx) * (p.X - qx) + (p.Y - qy) * (p.Y - qy));
    }

    /// <summary>1 = dentro, 0 = en el borde (a tolerancia), -1 = fuera.</summary>
    public static int Ubicacion(Poligono poly, Punto p, double tol = 1e-6)
    {
        var v = poly.Puntos; int n = v.Length;
        for (int i = 0; i < n; i++) if (DistPuntoSegmento(p, v[i], v[(i + 1) % n]) <= tol) return 0;
        bool dentro = false;
        for (int i = 0, j = n - 1; i < n; j = i++)
            if ((v[i].Y > p.Y) != (v[j].Y > p.Y) && p.X < (v[j].X - v[i].X) * (p.Y - v[i].Y) / (v[j].Y - v[i].Y) + v[i].X) dentro = !dentro;
        return dentro ? 1 : -1;
    }

    /// <summary>Verdadero si las dos piezas se pisan (area en comun). Tocarse por el borde no cuenta.</summary>
    public static bool Solapan(Poligono a, Poligono b)
    {
        var la = a.Limites; var lb = b.Limites;
        if (la.MaxX <= lb.MinX + Eps || lb.MaxX <= la.MinX + Eps || la.MaxY <= lb.MinY + Eps || lb.MaxY <= la.MinY + Eps) return false;
        var pa = a.Puntos; var pb = b.Puntos;
        for (int i = 0; i < pa.Length; i++)
            for (int j = 0; j < pb.Length; j++)
                if (SegmentosSeCruzan(pa[i], pa[(i + 1) % pa.Length], pb[j], pb[(j + 1) % pb.Length])) return true;
        return AlgunPuntoDentro(a, b) || AlgunPuntoDentro(b, a);
    }

    static bool AlgunPuntoDentro(Poligono quien, Poligono en)
    {
        var v = quien.Puntos;
        for (int i = 0; i < v.Length; i++)
        {
            if (Ubicacion(en, v[i]) > 0) return true;
            var m = new Punto((v[i].X + v[(i + 1) % v.Length].X) / 2, (v[i].Y + v[(i + 1) % v.Length].Y) / 2);
            if (Ubicacion(en, m) > 0) return true;
        }
        foreach (var q in PuntosInteriores(quien)) if (Ubicacion(en, q) > 0) return true;   // piezas iguales o con el mismo contorno
        return false;
    }

    /// <summary>Puntos que seguro estan dentro de la pieza: el centro de cada tramo interior de varias lineas horizontales.</summary>
    static IEnumerable<Punto> PuntosInteriores(Poligono p)
    {
        var l = p.Limites; var v = p.Puntos; int n = v.Length; var xs = new List<double>();
        foreach (var f in new[] { 0.5, 0.27, 0.73, 0.1, 0.9 })
        {
            double y = l.MinY + (l.MaxY - l.MinY) * f; xs.Clear();
            for (int i = 0, j = n - 1; i < n; j = i++)
                if ((v[i].Y > y) != (v[j].Y > y)) xs.Add((v[j].X - v[i].X) * (y - v[i].Y) / (v[j].Y - v[i].Y) + v[i].X);
            xs.Sort();
            for (int q = 0; q + 1 < xs.Count; q += 2) if (xs[q + 1] - xs[q] > 1e-6) yield return new Punto((xs[q] + xs[q + 1]) / 2, y);
        }
    }

    /// <summary>Distancia minima entre los contornos de dos piezas que NO se pisan (0 si se pisan o se tocan).</summary>
    public static double Distancia(Poligono a, Poligono b)
    {
        if (Solapan(a, b)) return 0;
        double d = double.MaxValue;
        var pa = a.Puntos; var pb = b.Puntos;
        for (int i = 0; i < pa.Length; i++)
        {
            var a0 = pa[i]; var a1 = pa[(i + 1) % pa.Length];
            for (int j = 0; j < pb.Length; j++)
            {
                var b0 = pb[j]; var b1 = pb[(j + 1) % pb.Length];
                d = Math.Min(d, Math.Min(Math.Min(DistPuntoSegmento(a0, b0, b1), DistPuntoSegmento(a1, b0, b1)), Math.Min(DistPuntoSegmento(b0, a0, a1), DistPuntoSegmento(b1, a0, a1))));
            }
        }
        return d;
    }
}
