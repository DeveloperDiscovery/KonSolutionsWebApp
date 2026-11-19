using KONSolutions.Shared.Tizado;
using Xunit;

public class GradacionTests
{
    static Poligono Rect(double w, double h) => new(new[] { new Punto(0, 0), new Punto(w, 0), new Punto(w, h), new Punto(0, h) });
    static Poligono Trapecio() => new(new[] { new Punto(0, 0), new Punto(300, 0), new Punto(260, 200), new Punto(40, 200) });

    [Fact] public void PorIncrementos_AgrandaLaCaja_ConElOrigenFijo()
    {
        var g = Gradacion.PorIncrementos(Rect(100, 50), 10, 4);
        Assert.Equal((0, 0, 110, 54), g.Limites);
        Assert.Equal(110 * 54, g.Area, 6);
    }
    [Fact] public void PorIncrementos_Negativos_Reducen()
    {
        var g = Gradacion.PorIncrementos(Rect(100, 50), -10, -5);
        Assert.Equal((0, 0, 90, 45), g.Limites);
    }
    [Fact] public void PorIncrementos_Cero_DejaLaPiezaIgual()
        => Assert.Equal(Rect(100, 50).ATexto(), Gradacion.PorIncrementos(Rect(100, 50), 0, 0).ATexto());
    [Fact] public void PorIncrementos_EnUnTrapecio_RepartePorPosicion()
    {
        var g = Gradacion.PorIncrementos(Trapecio(), 30, 20);
        // la base (x 0 y 300) crece en x 0 y 30; la parte alta (y = 200) crece 20 en alto; los puntos intermedios crecen en proporcion
        Assert.Equal(0, g.Puntos[0].X, 6); Assert.Equal(330, g.Puntos[1].X, 6); Assert.Equal(0, g.Puntos[1].Y, 6);
        Assert.Equal(260 + 260.0 / 300 * 30, g.Puntos[2].X, 6); Assert.Equal(220, g.Puntos[2].Y, 6);
        Assert.Equal(40 + 40.0 / 300 * 30, g.Puntos[3].X, 6); Assert.Equal(220, g.Puntos[3].Y, 6);
    }
    [Fact] public void DeltasDesdeIncrementos_UnoPorPunto_YElMismoResultado()
    {
        var b = Trapecio(); var d = Gradacion.DeltasDesdeIncrementos(b, 30, 20);
        Assert.Equal(4, d.Count);
        Assert.Equal(Gradacion.PorIncrementos(b, 30, 20).ATexto(), Gradacion.PorReglas(b, d).ATexto());
        Assert.Equal(new DeltaPunto(0, 0), d[0]);
    }
    [Fact] public void PorReglas_CadaPuntoCreceDistinto()
    {
        var g = Gradacion.PorReglas(Rect(100, 50), new[] { new DeltaPunto(0, 0), new DeltaPunto(10, 0), new DeltaPunto(12, 6), new DeltaPunto(0, 4) });
        Assert.Equal(110, g.Puntos[1].X); Assert.Equal(112, g.Puntos[2].X); Assert.Equal(56, g.Puntos[2].Y); Assert.Equal(54, g.Puntos[3].Y);
    }
    [Fact] public void PorReglas_LaCantidadDeReglasDebeCoincidirConLosPuntos()
    {
        var e = Assert.Throws<ArgumentException>(() => Gradacion.PorReglas(Rect(100, 50), new[] { new DeltaPunto(1, 1) }));
        Assert.Contains("una por punto", e.Message);
    }
    [Fact] public void PorReglas_ValoresNoNumericos_Fallan()
    {
        Assert.Throws<ArgumentException>(() => Gradacion.PorReglas(Rect(10, 10), Enumerable.Repeat(new DeltaPunto(double.NaN, 0), 4).ToList()));
        Assert.Throws<ArgumentException>(() => Gradacion.PorReglas(Rect(10, 10), Enumerable.Repeat(new DeltaPunto(0, double.PositiveInfinity), 4).ToList()));
    }
    [Fact] public void PorReglas_SiLaPiezaQuedaSinArea_Falla()
        => Assert.Throws<ArgumentException>(() => Gradacion.PorIncrementos(Rect(100, 50), -100, 0));         // ancho 0: sin area
    [Fact] public void PorReglas_SiLaPiezaSeInvierte_Falla()
    {
        var e = Assert.Throws<ArgumentException>(() => Gradacion.PorIncrementos(Rect(100, 50), -150, 0));    // el largo se vuelve negativo: la pieza queda espejada
        Assert.True(e.Message.Contains("invierte") || e.Message.Contains("area"));
    }
    [Fact] public void PorReglas_SiElContornoSeCruza_Falla()
    {
        // mover un solo vertice del rectangulo al otro lado del opuesto hace un "mono": los lados se cruzan
        var e = Assert.Throws<ArgumentException>(() => Gradacion.PorReglas(Rect(100, 100), new[] { new DeltaPunto(0, 0), new DeltaPunto(0, 0), new DeltaPunto(-150, 0), new DeltaPunto(0, 0) }));
        Assert.Contains("cruza", e.Message);
    }
    [Fact] public void PorReglas_PiezaConUnPuntoRepetido_Falla()
        => Assert.Throws<ArgumentException>(() => Gradacion.PorReglas(Rect(100, 50), new[] { new DeltaPunto(0, 0), new DeltaPunto(-100, 0), new DeltaPunto(0, 0), new DeltaPunto(0, 0) }));        // el vertice 2 cae sobre el 1
    [Fact] public void MoldeBaseInvalido_Falla()
    {
        var dos = new Poligono(new[] { new Punto(0, 0), new Punto(1, 1) });
        Assert.Throws<ArgumentException>(() => Gradacion.PorIncrementos(dos, 1, 1));
        Assert.Throws<ArgumentException>(() => Gradacion.DeltasDesdeIncrementos(dos, 1, 1));
    }
    [Fact] public void Cruza_DistingueUnContornoSimpleDeUnoQueSeCorta()
    {
        Assert.False(Gradacion.Cruza(Rect(10, 10)));
        Assert.True(Gradacion.Cruza(new Poligono(new[] { new Punto(0, 0), new Punto(10, 10), new Punto(10, 0), new Punto(0, 10) })));      // lazo
        Assert.False(Gradacion.Cruza(new Poligono(new[] { new Punto(0, 0), new Punto(10, 0), new Punto(10, 10), new Punto(5, 5), new Punto(0, 10) })));   // concavo sin cruce
    }
    [Fact] public void LaPiezaGradada_ConservaElOrdenYElSentidoDeLosVertices()
    {
        var b = Trapecio(); var g = Gradacion.PorIncrementos(b, 20, 10);
        Assert.Equal(b.Cantidad, g.Cantidad); Assert.True(g.AreaConSigno > 0);
    }
    [Fact] public void Gradar_VariasTallas_DaAreasCrecientes()
    {
        var b = Rect(100, 50);
        var areas = new[] { -20.0, -10, 0, 10, 20 }.Select(i => Gradacion.PorIncrementos(b, i, i / 2).Area).ToList();
        Assert.True(areas.SequenceEqual(areas.OrderBy(x => x)));
    }
}
