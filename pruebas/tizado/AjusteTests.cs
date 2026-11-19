using KONSolutions.Shared.Tizado;
using Xunit;

// Etapa G: ajuste manual (mover, girar, espejar) y su validacion.
public class AjusteTests
{
    static Poligono Rect(double w, double h, double x = 0, double y = 0) => new(new[] { new Punto(x, y), new Punto(x + w, y), new Punto(x + w, y + h), new Punto(x, y + h) });
    static Colocacion C(string id, Poligono p, Punto? pc = null, double? fx = null, double? fy = null, int copia = 1) =>
        new() { PiezaId = id, Nombre = "P" + id, Talla = "M", Copia = copia, Contorno = p, PuntoCoincidencia = pc, FaseX = fx, FaseY = fy };
    static ReglasAjuste Reg(double ancho = 500, double sep = 4, double margen = 5, double px = 0, double py = 0, params Defecto[] def) =>
        new() { AnchoTela = ancho, Separacion = sep, Margen = margen, PeriodoX = px, PeriodoY = py, Defectos = def };
    static List<ProblemaAjuste> V(Colocacion[] c, ReglasAjuste r) => Ajuste.Validar(c, r);
    static (double MinX, double MinY, double MaxX, double MaxY) L(Colocacion c) => c.Contorno.Limites;

    // ---------- operaciones ----------
    [Fact]
    public void Mover_TrasladaLaPieza_ElPunto_YDejaElAnguloComoEstaba()
    {
        var c = Ajuste.Mover(C("1", Rect(100, 50, 10, 20), new Punto(30, 40)), 7, -3);
        Assert.Equal((17, 17, 117, 67), L(c)); Assert.Equal(37, c.PuntoCoincidencia!.Value.X, 9); Assert.Equal(37, c.PuntoCoincidencia!.Value.Y, 9);
        Assert.Equal(0, c.Angulo);
    }
    [Fact]
    public void Ubicar_DejaLaEsquinaDeLaCajaEnElLugarPedido()
    {
        var c = Ajuste.Ubicar(C("1", Rect(100, 50, 300, 200), new Punto(310, 210)), 12, 34);
        Assert.Equal((12, 34, 112, 84), L(c)); Assert.Equal(22, c.PuntoCoincidencia!.Value.X, 9); Assert.Equal(44, c.PuntoCoincidencia!.Value.Y, 9);
    }
    [Fact]
    public void Girar90_IntercambiaLasMedidasAlrededorDelCentro_ConservaElAreaYGiraElPunto()
    {
        var o = C("1", Rect(100, 50, 200, 100), new Punto(220, 110));
        var c = Ajuste.Girar(o, 90);
        var l = L(c);
        Assert.Equal(50, l.MaxX - l.MinX, 6); Assert.Equal(100, l.MaxY - l.MinY, 6);
        Assert.Equal(250, (l.MinX + l.MaxX) / 2, 6); Assert.Equal(125, (l.MinY + l.MaxY) / 2, 6);      // mismo centro
        Assert.Equal(90, c.Angulo, 9); Assert.Equal(5000, c.Contorno.Area, 6);
        // (220,110) respecto del centro (250,125) = (-30,-15) -> girado 90 antihorario = (15,-30)
        Assert.Equal(265, c.PuntoCoincidencia!.Value.X, 6); Assert.Equal(95, c.PuntoCoincidencia!.Value.Y, 6);
        Assert.True(Ajuste.EsCongruente(o.Contorno, c.Contorno));
    }
    [Fact]
    public void GirarCuatroVeces90_VuelveAlPrincipio_YElAnguloSeNormaliza()
    {
        var o = C("1", Rect(100, 50, 200, 100), new Punto(220, 110)); var c = o;
        for (int i = 0; i < 4; i++) c = Ajuste.Girar(c, 90);
        Assert.Equal(0, c.Angulo, 9);
        for (int i = 0; i < 4; i++) { Assert.Equal(o.Contorno.Limites.MinX, c.Contorno.Limites.MinX, 6); }
        Assert.Equal(220, c.PuntoCoincidencia!.Value.X, 6); Assert.Equal(110, c.PuntoCoincidencia!.Value.Y, 6);
        Assert.Equal(270, Ajuste.Girar(o, -90).Angulo, 9);
    }
    [Fact]
    public void Espejar_CambiaIzquierdaPorDerecha_ConservaElSentidoYLaCaja()
    {
        var tri = new Poligono(new[] { new Punto(0, 0), new Punto(100, 0), new Punto(20, 60) });
        var o = C("1", tri, new Punto(10, 5));
        var c = Ajuste.Espejar(o);
        Assert.Equal(L(o), L(c)); Assert.True(c.Espejada);
        Assert.Equal(90, c.PuntoCoincidencia!.Value.X, 9); Assert.Equal(5, c.PuntoCoincidencia!.Value.Y, 9);       // 100 - 10
        Assert.True(Math.Sign(o.Contorno.AreaConSigno) == Math.Sign(c.Contorno.AreaConSigno));
        Assert.True(Ajuste.EsCongruente(o.Contorno, c.Contorno));
        Assert.False(Ajuste.Espejar(c).Espejada);
        Assert.Equal(80, c.Contorno.Puntos.Select(p => p.X).Single(x => Math.Abs(x - 80) < 1e-9), 9);              // el vertice (20,60) paso a x = 80
    }

    // ---------- validacion ----------
    [Fact]
    public void UnTizadoCalculadoPorElMotor_NoTieneErroresNiAvisosDeSeparacion()
    {
        var rnd = new Random(7);
        for (int k = 0; k < 15; k++)
        {
            var p = new ParametrosTizado { AnchoTela = 600, Separacion = 4, Margen = 5, Celda = 5, Intentos = 3 };
            if (k % 3 == 0) p.Defectos.Add(new Defecto { Nombre = "d", Contorno = Rect(40, 60, 100 + rnd.Next(100), 100 + rnd.Next(100)) });
            bool rayas = k % 2 == 1; if (rayas) p.PeriodoX = 40 + rnd.NextDouble() * 60;
            var piezas = Enumerable.Range(1, 6).Select(i => new Pieza { Id = i.ToString(), Nombre = "P" + i, Talla = "M", Contorno = Rect(40 + rnd.Next(120), 40 + rnd.Next(150)), Cantidad = 1 + rnd.Next(2),
                Rotaciones = new double[] { 0, 90 }, PuntoCoincidencia = rayas ? new Punto(5, 5) : null, CoincideX = rayas }).ToList();
            var r = Tizador.Calcular(piezas, p);
            var rg = Reg(600, 4, 5, p.PeriodoX, 0, p.Defectos.ToArray());
            var prob = Ajuste.Validar(r.Colocadas, rg);
            Assert.True(prob.Count == 0, $"caso {k}: {string.Join(" | ", prob.Select(x => x.Mensaje))}");
            var m = Ajuste.Medidas(r.Colocadas, rg);
            Assert.Equal(r.Largo, m.Largo, 6); Assert.Equal(r.Eficiencia, m.Eficiencia, 9); Assert.Equal(r.AreaPiezas, m.Area, 6);
        }
    }
    [Fact]
    public void PiezasQueSePisan_SonUnErrorConLasDosPosiciones()
    {
        var pr = V(new[] { C("1", Rect(100, 100, 10, 10)), C("2", Rect(100, 100, 60, 60), copia: 2) }, Reg());
        var e = Assert.Single(pr, x => x.Tipo == "solape");
        Assert.Equal(GravedadAjuste.Error, e.Gravedad); Assert.Equal((0, 1), (e.A, e.B)); Assert.Contains("P1 M #1", e.Mensaje); Assert.Contains("P2 M #2", e.Mensaje);
    }
    [Fact]
    public void PiezasContenidasUnaEnOtra_TambienSePisan()
    {
        Assert.Contains(V(new[] { C("1", Rect(200, 200, 10, 10)), C("2", Rect(20, 20, 50, 50)) }, Reg()), x => x.Tipo == "solape");
    }
    [Fact]
    public void LejosONoSePisan_SinProblemas_Y_LaSeparacionJustaEsValida()
    {
        Assert.Empty(V(new[] { C("1", Rect(100, 100, 10, 10)), C("2", Rect(100, 100, 114, 10)) }, Reg()));       // 4 mm de aire: justo la separacion
        Assert.Empty(V(new[] { C("1", Rect(100, 100, 10, 10)), C("2", Rect(100, 100, 400, 10)) }, Reg()));
    }
    [Fact]
    public void MenosDeLaSeparacion_EsUnAviso_NoUnError()
    {
        var pr = V(new[] { C("1", Rect(100, 100, 10, 10)), C("2", Rect(100, 100, 112, 10)) }, Reg());          // 2 mm de aire
        var a = Assert.Single(pr); Assert.Equal("separacion", a.Tipo); Assert.Equal(GravedadAjuste.Aviso, a.Gravedad);
    }
    [Fact]
    public void PiezasQueSoloSeTocan_NoCuentanComoPisadas_PeroSiComoSeparacionInsuficiente()
    {
        var pr = V(new[] { C("1", Rect(100, 100, 10, 10)), C("2", Rect(100, 100, 110, 10)) }, Reg());
        Assert.DoesNotContain(pr, x => x.Tipo == "solape"); Assert.Contains(pr, x => x.Tipo == "separacion");
    }
    [Fact]
    public void FueraDeLaTela_PorElInicio_PorArribaYPorAbajo_SonErrores()
    {
        var pr = V(new[] { C("1", Rect(50, 50, 2, 100)) }, Reg()); Assert.Contains(pr, x => x.Tipo == "fuera" && x.Gravedad == GravedadAjuste.Error && x.Mensaje.Contains("inicio"));
        pr = V(new[] { C("1", Rect(50, 50, 100, 2)) }, Reg()); Assert.Contains(pr, x => x.Tipo == "fuera" && x.Mensaje.Contains("ancho"));
        pr = V(new[] { C("1", Rect(50, 50, 100, 460)) }, Reg()); Assert.Contains(pr, x => x.Tipo == "fuera" && x.Mensaje.Contains("ancho"));
        Assert.Empty(V(new[] { C("1", Rect(50, 490, 5, 5)) }, Reg()));                                         // justo en el margen por todos lados
    }
    [Fact]
    public void SobreUnDefecto_EsUnError_YCerca_UnAviso()
    {
        var d = new Defecto { Nombre = "mancha", Contorno = Rect(50, 50, 200, 200) };
        var pr = V(new[] { C("1", Rect(100, 100, 150, 150)) }, Reg(def: d));
        var e = Assert.Single(pr, x => x.Tipo == "defecto"); Assert.Equal(GravedadAjuste.Error, e.Gravedad); Assert.Equal((-1, 0), (e.A, e.B)); Assert.Contains("mancha", e.Mensaje);
        pr = V(new[] { C("1", Rect(100, 100, 98, 200)) }, Reg(def: d));                                        // a 2 mm del defecto
        Assert.Contains(pr, x => x.Tipo == "defecto-cerca" && x.Gravedad == GravedadAjuste.Aviso); Assert.DoesNotContain(pr, x => x.Tipo == "defecto");
        Assert.Empty(V(new[] { C("1", Rect(100, 100, 5, 5)) }, Reg(def: d)));
    }
    [Fact]
    public void LaRaya_SoloAvisaSiElDesvioPasaDeMedioMilimetro()
    {
        var c = C("1", Rect(80, 60, 100, 100), new Punto(110, 110), fx: 10);                                    // 110 - 10 = 100 = 1 periodo: justo
        Assert.Empty(V(new[] { c }, Reg(px: 100)));
        Assert.Empty(V(new[] { Ajuste.Mover(c, 0.4, 0) }, Reg(px: 100)));
        var a = Assert.Single(V(new[] { Ajuste.Mover(c, 10, 0) }, Reg(px: 100))); Assert.Equal("raya", a.Tipo); Assert.Equal(GravedadAjuste.Aviso, a.Gravedad); Assert.Contains("10", a.Mensaje);
        Assert.Empty(V(new[] { Ajuste.Mover(c, 100, 0) }, Reg(px: 100)));                                       // otra raya: vale
        Assert.Empty(V(new[] { Ajuste.Mover(c, 10, 0) }, Reg(px: 0)));                                          // sin periodo no hay raya
        var d = Ajuste.Desvio(Ajuste.Mover(c, 97, 0), Reg(px: 100)); Assert.Equal(3, d.X!.Value, 6); Assert.Null(d.Y);
    }
    [Fact]
    public void LaRaya_EnY_TambienSeMide()
    {
        var c = C("1", Rect(80, 60, 100, 100), new Punto(110, 110), fx: 10, fy: 0);
        var pr = V(new[] { Ajuste.Mover(c, 0, 20) }, Reg(px: 100, py: 100));
        Assert.Single(pr); Assert.Contains("ancho", pr[0].Mensaje);
    }
    [Fact]
    public void Medidas_SinPiezas_SonCero()
    {
        Assert.Equal((0.0, 0.0, 0.0), Ajuste.Medidas(Array.Empty<Colocacion>(), Reg()));
        var m = Ajuste.Medidas(new[] { C("1", Rect(100, 100, 0, 0)) }, Reg(ancho: 200, margen: 10));
        Assert.Equal(110, m.Largo, 9); Assert.Equal(10000, m.Area, 9); Assert.Equal(10000 / (110.0 * 200), m.Eficiencia, 9);
    }

    // ---------- congruencia ----------
    [Fact]
    public void Congruente_ConGiroEspejoYTraslado_PeroNoConDeformaciones()
    {
        var tri = new Poligono(new[] { new Punto(0, 0), new Punto(100, 0), new Punto(30, 60), new Punto(0, 40) });
        var o = C("1", tri);
        Assert.True(Ajuste.EsCongruente(tri, Ajuste.Mover(Ajuste.Girar(Ajuste.Espejar(o), 37), 500, 20).Contorno));
        Assert.False(Ajuste.EsCongruente(tri, tri.Escalar(1.01)));
        Assert.False(Ajuste.EsCongruente(tri, new Poligono(new[] { new Punto(0, 0), new Punto(100, 0), new Punto(30, 61), new Punto(0, 40) })));      // un vertice corrido
        Assert.False(Ajuste.EsCongruente(tri, new Poligono(new[] { new Punto(0, 0), new Punto(100, 0), new Punto(30, 60) })));                          // otro numero de vertices
        // mismo perimetro y vertices pero cuadrilatero "flexionado": cambia el area
        Assert.False(Ajuste.EsCongruente(Rect(100, 50), new Poligono(new[] { new Punto(0, 0), new Punto(100, 0), new Punto(110, 48.989), new Punto(10, 48.989) })));
    }

    [Fact]
    public void Desvio_EnYUsaElPeriodoDeY_YSinPeriodoNoHayDesvio()
    {
        var c = C("1", Rect(80, 60, 100, 100), new Punto(110, 110), fx: 10, fy: 10);
        var d = Ajuste.Desvio(Ajuste.Mover(c, 0, 30), Reg(px: 100, py: 70));        // 110+30 = 140; 140 - 10 = 130; resto de 130 sobre 70 = 60 -> mas cerca por abajo: 10
        Assert.Equal(0, d.X!.Value, 6); Assert.Equal(10, d.Y!.Value, 6);
        var sinY = Ajuste.Desvio(c, Reg(px: 100, py: 0)); Assert.Equal(0, sinY.X!.Value, 6); Assert.Null(sinY.Y);          // con fase pero sin periodo en Y
        var sinX = Ajuste.Desvio(c, Reg(px: 0, py: 100)); Assert.Null(sinX.X); Assert.NotNull(sinX.Y);
        Assert.Null(Ajuste.Desvio(C("2", Rect(10, 10)), Reg(px: 100, py: 100)).X);                                         // sin punto
    }

    [Fact]
    public void PuntoCongruente_SoloSiElPuntoQuedaEnElMismoLugarDeLaPieza()
    {
        var o = C("1", Rect(100, 50), new Punto(20, 10));
        var g = Ajuste.Mover(Ajuste.Girar(Ajuste.Espejar(o), 40), 300, 200);
        Assert.True(Ajuste.PuntoCongruente(o.Contorno, o.PuntoCoincidencia!.Value, g.Contorno, g.PuntoCoincidencia!.Value));
        Assert.False(Ajuste.PuntoCongruente(o.Contorno, o.PuntoCoincidencia!.Value, g.Contorno, new Punto(g.PuntoCoincidencia!.Value.X + 1, g.PuntoCoincidencia!.Value.Y)));
        Assert.False(Ajuste.PuntoCongruente(o.Contorno, o.PuntoCoincidencia!.Value, Rect(100, 50, 5, 5), new Punto(5 + 20, 5 + 10 + 0.5)));
        Assert.False(Ajuste.PuntoCongruente(o.Contorno, new Punto(20, 10), new Poligono(new[] { new Punto(0, 0), new Punto(1, 0), new Punto(1, 1) }), new Punto(0, 0)));
    }
}
