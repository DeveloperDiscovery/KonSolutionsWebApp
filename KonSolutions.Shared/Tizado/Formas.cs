using System.Globalization;

namespace KONSolutions.Shared.Tizado;

/// <summary>
/// Formas de partida para armar un molde (o un defecto) sin importar un DXF: todas devuelven sus puntos en mm con la esquina minima de la caja en (0,0)
/// y en sentido antihorario; despues se pueden mover y ajustar punto por punto.
/// </summary>
public static class Formas
{
    public static List<Punto> Rectangulo(double ancho, double alto) =>
        Validar(ancho, alto) ?? new() { new(0, 0), new(ancho, 0), new(ancho, alto), new(0, alto) };

    /// <summary>Trapecio con la base abajo (baseInferior) y la de arriba (baseSuperior) centrada. Si ambas son iguales sale un rectangulo.</summary>
    public static List<Punto> Trapecio(double baseInferior, double baseSuperior, double alto)
    {
        if (!(baseInferior > 0 && baseSuperior > 0 && alto > 0)) throw new ArgumentException("Las bases y el alto deben ser mayores que cero.");
        double m = Math.Max(baseInferior, baseSuperior), di = (m - baseInferior) / 2, ds = (m - baseSuperior) / 2;
        return new() { new(di, 0), new(di + baseInferior, 0), new(ds + baseSuperior, alto), new(ds, alto) };
    }

    /// <summary>Triangulo con la base abajo y el vertice a "posicionVertice" mm del borde izquierdo (nulo = centrado).</summary>
    public static List<Punto> Triangulo(double baseTriangulo, double alto, double? posicionVertice = null)
    {
        if (!(baseTriangulo > 0 && alto > 0)) throw new ArgumentException("La base y el alto deben ser mayores que cero.");
        var v = posicionVertice ?? baseTriangulo / 2;
        double minX = Math.Min(0, v), maxX = Math.Max(baseTriangulo, v);
        return new() { new(0 - minX, 0), new(baseTriangulo - minX, 0), new(v - minX, alto) };
    }

    /// <summary>Paralelogramo: la base de abajo y la de arriba (mismo ancho) corrida "sesgo" mm hacia la derecha (negativo = izquierda).</summary>
    public static List<Punto> Paralelogramo(double ancho, double alto, double sesgo)
    {
        var e = Validar(ancho, alto); if (e is not null) throw new ArgumentException("El ancho y el alto deben ser mayores que cero.");
        double m = Math.Min(0, sesgo);
        return new() { new(-m, 0), new(ancho - m, 0), new(ancho + sesgo - m, alto), new(sesgo - m, alto) };
    }

    /// <summary>Forma en L: caja ancho x alto a la que se le quita la esquina de arriba a la derecha (un rectangulo de recorteAncho x recorteAlto).</summary>
    public static List<Punto> FormaL(double ancho, double alto, double recorteAncho, double recorteAlto)
    {
        if (Validar(ancho, alto) is not null || !(recorteAncho > 0 && recorteAlto > 0 && recorteAncho < ancho && recorteAlto < alto)) throw new ArgumentException("El recorte debe ser mas chico que la forma.");
        return new() { new(0, 0), new(ancho, 0), new(ancho, alto - recorteAlto), new(ancho - recorteAncho, alto - recorteAlto), new(ancho - recorteAncho, alto), new(0, alto) };
    }

    /// <summary>Elipse (o circulo si rx = ry) aproximada con "segmentos" lados (8 a 360).</summary>
    public static List<Punto> Elipse(double rx, double ry, int segmentos = 48)
    {
        if (!(rx > 0 && ry > 0)) throw new ArgumentException("Los radios deben ser mayores que cero.");
        if (segmentos < 8 || segmentos > 360) throw new ArgumentException("Los segmentos deben estar entre 8 y 360.");
        var l = new List<Punto>();
        for (int i = 0; i < segmentos; i++) { double a = 2 * Math.PI * i / segmentos; l.Add(new(rx + rx * Math.Cos(a), ry + ry * Math.Sin(a))); }
        return AlOrigen(l);
    }

    /// <summary>Poligono regular de "lados" lados y radio (del centro a un vertice), con un vertice abajo.</summary>
    public static List<Punto> PoligonoRegular(double radio, int lados)
    {
        if (!(radio > 0)) throw new ArgumentException("El radio debe ser mayor que cero.");
        if (lados < 3 || lados > 100) throw new ArgumentException("Los lados deben estar entre 3 y 100.");
        var l = new List<Punto>();
        for (int i = 0; i < lados; i++) { double a = -Math.PI / 2 + 2 * Math.PI * i / lados; l.Add(new(radio * Math.Cos(a), radio * Math.Sin(a))); }
        return AlOrigen(l);
    }

    /// <summary>Rectangulo con las esquinas redondeadas (radio) y "segmentos" lados por esquina: bolsillos, cuellos, bordes.</summary>
    public static List<Punto> Redondeado(double ancho, double alto, double radio, int segmentos = 8)
    {
        if (Validar(ancho, alto) is not null) throw new ArgumentException("El ancho y el alto deben ser mayores que cero.");
        if (!(radio > 0) || radio > Math.Min(ancho, alto) / 2 + 1e-9) throw new ArgumentException("El radio debe ser mayor que cero y no pasar de la mitad del lado mas corto.");
        if (segmentos < 2 || segmentos > 90) throw new ArgumentException("Los segmentos por esquina deben estar entre 2 y 90.");
        var l = new List<Punto>();
        void Esquina(double cx, double cy, double desde)
        {
            for (int i = 0; i <= segmentos; i++) { double a = (desde + 90.0 * i / segmentos) * Math.PI / 180; l.Add(new(cx + radio * Math.Cos(a), cy + radio * Math.Sin(a))); }
        }
        Esquina(ancho - radio, radio, -90); Esquina(ancho - radio, alto - radio, 0); Esquina(radio, alto - radio, 90); Esquina(radio, radio, 180);
        return AlOrigen(l);
    }

    /// <summary>Lleva la caja de los puntos a la esquina (0,0).</summary>
    public static List<Punto> AlOrigen(IReadOnlyList<Punto> pts)
    {
        if (pts.Count == 0) return new();
        double mx = pts.Min(p => p.X), my = pts.Min(p => p.Y);
        return pts.Select(p => new Punto(Math.Round(p.X - mx, 4), Math.Round(p.Y - my, 4))).ToList();
    }

    static List<Punto>? Validar(double ancho, double alto) => ancho > 0 && alto > 0 && !double.IsNaN(ancho) && !double.IsNaN(alto) ? null : new List<Punto>();

    /// <summary>"x,y x,y ..." (punto decimal) -> puntos; nulo si el texto no se puede leer.</summary>
    public static List<Punto>? Leer(string? texto)
    {
        var res = new List<Punto>();
        foreach (var tok in (texto ?? "").Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
        {
            var xy = tok.Split(',');
            if (xy.Length != 2 || !double.TryParse(xy[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var x) || !double.TryParse(xy[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var y) || double.IsNaN(x) || double.IsNaN(y) || double.IsInfinity(x) || double.IsInfinity(y)) return null;
            res.Add(new Punto(x, y));
        }
        return res;
    }

    public static string ATexto(IEnumerable<Punto> pts) => string.Join(" ", pts.Select(p => p.X.ToString("0.###", CultureInfo.InvariantCulture) + "," + p.Y.ToString("0.###", CultureInfo.InvariantCulture)));

    /// <summary>Motivo por el que los puntos no forman un contorno usable, o nulo si estan bien: al menos 3 puntos distintos, con area y sin cruzarse.</summary>
    public static string? Problema(IReadOnlyList<Punto> pts)
    {
        var p = new Poligono(pts);
        if (p.Cantidad < 3) return "Se necesitan al menos 3 puntos distintos.";
        if (p.Cantidad >= 4 && Gradacion.Cruza(p)) return "El contorno se cruza a si mismo: reordena o corrige los puntos.";      // antes del area: una corbata simetrica suma area cero
        if (!p.EsValido) return "Los puntos no encierran un area (estan en linea).";
        return null;
    }
}
