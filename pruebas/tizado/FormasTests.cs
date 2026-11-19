using KONSolutions.Shared.Tizado;
using Xunit;

// Formas de partida para armar moldes y defectos que no son rectangulos.
public class FormasTests
{
    static Poligono P(List<Punto> l) => new(l);
    static void Caja0(List<Punto> l, double w, double h, double tol = 1e-3)
    {
        var c = P(l).Limites;
        Assert.Equal(0, c.MinX, 3); Assert.Equal(0, c.MinY, 3); Assert.Equal(w, c.MaxX, 2); Assert.Equal(h, c.MaxY, 2);
        Assert.True(P(l).AreaConSigno > 0, "sentido antihorario");
        Assert.Null(Formas.Problema(l));
    }

    [Fact] public void Rectangulo() { var l = Formas.Rectangulo(100, 50); Caja0(l, 100, 50); Assert.Equal(5000, P(l).Area, 6); Assert.Equal(4, l.Count); }
    [Fact] public void Trapecio_AreaPorPromedioDeBases_YCentrado()
    {
        var l = Formas.Trapecio(100, 60, 40); Caja0(l, 100, 40); Assert.Equal((100 + 60) / 2.0 * 40, P(l).Area, 6);
        Assert.Contains(l, p => Math.Abs(p.X - 20) < 1e-9 && Math.Abs(p.Y - 40) < 1e-9); Assert.Contains(l, p => Math.Abs(p.X - 80) < 1e-9 && Math.Abs(p.Y - 40) < 1e-9);
        var inv = Formas.Trapecio(60, 100, 40); Caja0(inv, 100, 40); Assert.Equal(3200, P(inv).Area, 6);              // base de abajo mas chica
        Assert.Contains(inv, p => Math.Abs(p.X - 20) < 1e-9 && Math.Abs(p.Y) < 1e-9); Assert.Contains(inv, p => Math.Abs(p.X - 80) < 1e-9 && Math.Abs(p.Y) < 1e-9);   // la de abajo centrada
        Assert.Equal(P(Formas.Rectangulo(80, 40)).Area, P(Formas.Trapecio(80, 80, 40)).Area, 6);
    }
    [Fact] public void Triangulo_AreaYVerticeCentradoODesplazado()
    {
        var l = Formas.Triangulo(100, 80); Caja0(l, 100, 80); Assert.Equal(4000, P(l).Area, 6); Assert.Contains(l, p => Math.Abs(p.X - 50) < 1e-9 && Math.Abs(p.Y - 80) < 1e-9);
        var r = Formas.Triangulo(100, 80, 100); Caja0(r, 100, 80); Assert.Equal(4000, P(r).Area, 6);                    // triangulo rectangulo
        var fuera = Formas.Triangulo(100, 80, -30); Caja0(fuera, 130, 80); Assert.Equal(4000, P(fuera).Area, 6);        // vertice pasado de la base: la caja crece
    }
    [Fact] public void Paralelogramo_AreaBasePorAltura_ConSesgoPositivoONegativo()
    {
        var a = Formas.Paralelogramo(100, 50, 30); Caja0(a, 130, 50); Assert.Equal(5000, P(a).Area, 6);
        var b = Formas.Paralelogramo(100, 50, -30); Caja0(b, 130, 50); Assert.Equal(5000, P(b).Area, 6);
        Assert.Equal(P(Formas.Rectangulo(100, 50)).Area, P(Formas.Paralelogramo(100, 50, 0)).Area, 6);
    }
    [Fact] public void FormaL_AreaMenosElRecorte()
    {
        var l = Formas.FormaL(200, 100, 80, 40); Caja0(l, 200, 100); Assert.Equal(200 * 100 - 80 * 40, P(l).Area, 6); Assert.Equal(6, l.Count);
    }
    [Fact] public void Elipse_ConAreaDelPoligonoInscrito()
    {
        var l = Formas.Elipse(50, 30, 96); Caja0(l, 100, 60);
        Assert.Equal(96 / 2.0 * 50 * 30 * Math.Sin(2 * Math.PI / 96), P(l).Area, 1); Assert.True(P(l).Area < Math.PI * 50 * 30 && P(l).Area > 0.99 * Math.PI * 50 * 30);
        Assert.Equal(48, Formas.Elipse(10, 10).Count);
    }
    [Fact] public void PoligonoRegular_Hexagono_YTriangulo()
    {
        var h = Formas.PoligonoRegular(50, 6); Assert.Equal(6 / 2.0 * 50 * 50 * Math.Sin(2 * Math.PI / 6), P(h).Area, 1); Assert.Null(Formas.Problema(h));
        Assert.Equal(0, P(h).Limites.MinX, 3); Assert.Equal(0, P(h).Limites.MinY, 3);
        var t = Formas.PoligonoRegular(50, 3); Assert.Equal(3, t.Count); Assert.Null(Formas.Problema(t));
        Assert.Equal(40, Formas.PoligonoRegular(10, 40).Count);
    }
    [Fact] public void Redondeado_ConEsquinasQuePierdenAreaYSinCruzarse()
    {
        var l = Formas.Redondeado(100, 60, 10, 12); Caja0(l, 100, 60);
        Assert.Equal(4 * 13, l.Count);
        double esperado = 100 * 60 - (4 - Math.PI) * 100; Assert.True(Math.Abs(P(l).Area - esperado) < 0.02 * esperado, $"{P(l).Area} vs {esperado}");
        Caja0(Formas.Redondeado(60, 60, 30, 6), 60, 60);                                                                  // circulo: radio = mitad del lado
    }
    [Fact] public void ParametrosInvalidos_SeRechazan()
    {
        Assert.Throws<ArgumentException>(() => Formas.Trapecio(0, 10, 10)); Assert.Throws<ArgumentException>(() => Formas.Trapecio(10, 10, -1));
        Assert.Throws<ArgumentException>(() => Formas.Triangulo(0, 5)); Assert.Throws<ArgumentException>(() => Formas.Paralelogramo(10, 0, 3));
        Assert.Throws<ArgumentException>(() => Formas.FormaL(100, 50, 100, 10)); Assert.Throws<ArgumentException>(() => Formas.FormaL(100, 50, 10, 50));
        Assert.Throws<ArgumentException>(() => Formas.Elipse(0, 5)); Assert.Throws<ArgumentException>(() => Formas.Elipse(5, 5, 7)); Assert.Throws<ArgumentException>(() => Formas.Elipse(5, 5, 400));
        Assert.Throws<ArgumentException>(() => Formas.PoligonoRegular(5, 2)); Assert.Throws<ArgumentException>(() => Formas.PoligonoRegular(-1, 5)); Assert.Throws<ArgumentException>(() => Formas.PoligonoRegular(5, 101));
        Assert.Throws<ArgumentException>(() => Formas.Redondeado(100, 50, 30, 8)); Assert.Throws<ArgumentException>(() => Formas.Redondeado(100, 50, 0, 8)); Assert.Throws<ArgumentException>(() => Formas.Redondeado(100, 50, 5, 1));
        Assert.Empty(Formas.Rectangulo(0, 5).Where(_ => false)); Assert.Equal(0, Formas.Rectangulo(0, 5).Count);          // rectangulo sin medidas: sin puntos
    }
    [Fact] public void AlOrigen_LlevaLaCajaAlCero()
    {
        var l = Formas.AlOrigen(new[] { new Punto(10, 20), new Punto(30, 20), new Punto(30, 50) });
        Assert.Equal(new Punto(0, 0), l[0]); Assert.Equal(new Punto(20, 0), l[1]); Assert.Equal(new Punto(20, 30), l[2]); Assert.Empty(Formas.AlOrigen(Array.Empty<Punto>()));
    }
    [Fact] public void Leer_YATexto_VanYVienen_ConPuntoDecimal()
    {
        var l = Formas.Leer("  0,0   10.5,0\t10.5,7.25 ")!; Assert.Equal(3, l.Count); Assert.Equal(7.25, l[2].Y);
        Assert.Equal("0,0 10.5,0 10.5,7.25", Formas.ATexto(l));
        Assert.Null(Formas.Leer("0,0 a,b")); Assert.Null(Formas.Leer("1,2,3")); Assert.Null(Formas.Leer("1;2")); Assert.Null(Formas.Leer("NaN,1")); Assert.Empty(Formas.Leer("   ")!); Assert.Empty(Formas.Leer(null)!);
    }
    [Fact] public void Problema_DetectaPocosPuntosLineaYCruce()
    {
        Assert.Contains("al menos 3", Formas.Problema(new[] { new Punto(0, 0), new Punto(1, 1) })!);
        Assert.Contains("al menos 3", Formas.Problema(new[] { new Punto(0, 0), new Punto(0, 0), new Punto(1, 1) })!);                     // el repetido no cuenta
        Assert.Contains("area", Formas.Problema(new[] { new Punto(0, 0), new Punto(5, 0), new Punto(10, 0) })!);
        Assert.Contains("se cruza", Formas.Problema(new[] { new Punto(0, 0), new Punto(10, 10), new Punto(10, 0), new Punto(0, 10) })!);   // corbata
        Assert.Null(Formas.Problema(new[] { new Punto(0, 0), new Punto(10, 0), new Punto(5, 8) }));
    }
}
