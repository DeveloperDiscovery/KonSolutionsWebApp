namespace KONSolutions.Shared.Tizado;

/// <summary>Desplazamiento de un vertice del molde base al pasar a otra talla, en mm.</summary>
public readonly record struct DeltaPunto(double Dx, double Dy);

/// <summary>
/// Gradacion de tallas: genera el contorno de otra talla a partir del molde base desplazando sus vertices.
/// - PorIncrementos: crecimiento proporcional a la posicion de cada vertice dentro de la caja de la pieza (origen fijo): la caja pasa de
///   (largo, ancho) a (largo + incX, ancho + incY). Es un escalado por ejes: sirve para piezas rectas y como primera aproximacion.
/// - PorReglas: un desplazamiento (dx, dy) POR VERTICE, como las reglas de gradacion de los CAD de patronaje (cada punto de gradacion crece distinto).
/// Los vertices conservan su orden: el indice de la regla es el indice del vertice del molde base.
/// </summary>
public static class Gradacion
{
    /// <summary>Desplazamientos por vertice equivalentes a un incremento uniforme de la caja (para verlos y editarlos como reglas por punto).</summary>
    public static List<DeltaPunto> DeltasDesdeIncrementos(Poligono baseP, double incX, double incY)
    {
        if (baseP.Cantidad < 3) throw new ArgumentException("El molde base no es valido.");
        var l = baseP.Limites; double w = l.MaxX - l.MinX, h = l.MaxY - l.MinY;
        return baseP.Puntos.Select(p => new DeltaPunto(w > 1e-9 ? (p.X - l.MinX) / w * incX : 0, h > 1e-9 ? (p.Y - l.MinY) / h * incY : 0)).ToList();
    }

    /// <summary>Contorno de la talla destino por incremento uniforme de la caja.</summary>
    public static Poligono PorIncrementos(Poligono baseP, double incX, double incY) => PorReglas(baseP, DeltasDesdeIncrementos(baseP, incX, incY));

    /// <summary>Contorno de la talla destino aplicando un desplazamiento por vertice. Lanza ArgumentException si la cantidad de reglas no coincide o el resultado no es una pieza valida.</summary>
    public static Poligono PorReglas(Poligono baseP, IReadOnlyList<DeltaPunto> deltas)
    {
        if (baseP.Cantidad < 3) throw new ArgumentException("El molde base no es valido.");
        if (deltas.Count != baseP.Cantidad) throw new ArgumentException($"Las reglas son {deltas.Count} y el molde tiene {baseP.Cantidad} puntos: debe haber una por punto.");
        var pts = new Punto[baseP.Cantidad];
        for (int i = 0; i < pts.Length; i++)
        {
            var d = deltas[i];
            if (double.IsNaN(d.Dx) || double.IsInfinity(d.Dx) || double.IsNaN(d.Dy) || double.IsInfinity(d.Dy)) throw new ArgumentException("Las reglas tienen valores no validos.");
            pts[i] = new Punto(baseP.Puntos[i].X + d.Dx, baseP.Puntos[i].Y + d.Dy);
        }
        var res = new Poligono(pts);
        if (!res.EsValido || res.Cantidad != baseP.Cantidad) throw new ArgumentException("Con esas reglas la pieza queda sin area o con puntos repetidos.");
        if (res.AreaConSigno * baseP.AreaConSigno <= 0) throw new ArgumentException("Con esas reglas la pieza se invierte: reduce el desplazamiento.");
        if (Cruza(res)) throw new ArgumentException("Con esas reglas el contorno se cruza consigo mismo: revisa los desplazamientos.");
        return res;
    }

    /// <summary>El contorno se corta a si mismo (dos lados no contiguos que se cruzan).</summary>
    public static bool Cruza(Poligono p)
    {
        var v = p.Puntos; int n = v.Length;
        for (int i = 0; i < n; i++)
            for (int j = i + 2; j < n; j++)
            {
                if (i == 0 && j == n - 1) continue;                   // lados contiguos (el ultimo con el primero)
                if (Geometria.SegmentosSeCruzan(v[i], v[(i + 1) % n], v[j], v[(j + 1) % n])) return true;
            }
        return false;
    }
}
