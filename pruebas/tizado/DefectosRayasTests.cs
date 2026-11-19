using KONSolutions.Shared.Tizado;
using Xunit;

// Etapa E+F: defectos de tela (zonas prohibidas) y coincidencia de rayas. Se comprueba contra la geometria exacta (Geometria.Solapan / Distancia
// y el resto de la division), no contra la cuadricula del tizador.
public class DefectosRayasTests
{
    static Poligono Rect(double w, double h, double x = 0, double y = 0) => new(new[] { new Punto(x, y), new Punto(x + w, y), new Punto(x + w, y + h), new Punto(x, y + h) });
    static Pieza P(string id, double w, double h, int cant = 1, double[]? rot = null) => new() { Id = id, Nombre = "P" + id, Talla = "M", Contorno = Rect(w, h), Cantidad = cant, Rotaciones = rot ?? new double[] { 0 } };
    static ParametrosTizado Par(double ancho = 500, double sep = 4, double margen = 5, double celda = 5) => new() { AnchoTela = ancho, Separacion = sep, Margen = margen, Celda = celda, Intentos = 4 };
    static double Mod(double v, double R) { var r = ((v % R) + R) % R; return Math.Min(r, R - r); }

    static void SinSolapes(ResultadoTizado r, double sep)
    {
        for (int i = 0; i < r.Colocadas.Count; i++)
            for (int j = i + 1; j < r.Colocadas.Count; j++)
            {
                Assert.False(Geometria.Solapan(r.Colocadas[i].Contorno, r.Colocadas[j].Contorno), $"{i} y {j} se pisan");
                Assert.True(Geometria.Distancia(r.Colocadas[i].Contorno, r.Colocadas[j].Contorno) >= sep - 1e-6, $"{i} y {j} quedan a menos de la separacion");
            }
    }

    // ===================== defectos =====================
    [Fact]
    public void UnDefecto_NingunaPiezaLoPisa_NiQuedaACercaDeLaSeparacion()
    {
        var p = Par(); p.Defectos.Add(new Defecto { Nombre = "mancha", Contorno = Rect(60, 80, 150, 200) });
        var r = Tizador.Calcular(Enumerable.Range(1, 6).Select(i => P(i.ToString(), 100, 90)), p);
        Assert.Equal(6, r.Colocadas.Count); Assert.Empty(r.NoColocadas);
        foreach (var c in r.Colocadas)
        {
            Assert.False(Geometria.Solapan(c.Contorno, p.Defectos[0].Contorno));
            Assert.True(Geometria.Distancia(c.Contorno, p.Defectos[0].Contorno) >= p.Separacion - 1e-6);
        }
        SinSolapes(r, p.Separacion);
    }

    [Fact]
    public void UnDefecto_AlargaElTizado_FrenteAlMismoTizadoSinDefecto()
    {
        var piezas = Enumerable.Range(1, 5).Select(i => P(i.ToString(), 120, 480)).ToList();
        var sin = Tizador.Calcular(piezas, Par());
        var p = Par(); p.Defectos.Add(new Defecto { Nombre = "agujero", Contorno = Rect(40, 500, 130, 0) });   // cruza todo el ancho
        var con = Tizador.Calcular(piezas, p);
        Assert.True(con.Largo > sin.Largo + 30, $"{con.Largo} vs {sin.Largo}");
        foreach (var c in con.Colocadas) Assert.False(Geometria.Solapan(c.Contorno, p.Defectos[0].Contorno));
    }

    [Fact]
    public void DefectoQueCruzaTodoElAncho_DejaPiezasATodoLosLados()
    {
        var p = Par(); p.Defectos.Add(new Defecto { Nombre = "corte", Contorno = Rect(20, 500, 200, 0) });
        var r = Tizador.Calcular(Enumerable.Range(1, 5).Select(i => P(i.ToString(), 100, 400)), p);
        Assert.Equal(5, r.Colocadas.Count);
        foreach (var c in r.Colocadas) { var l = c.Contorno.Limites; Assert.True(l.MaxX <= 200 - p.Separacion + 1e-6 || l.MinX >= 220 + p.Separacion - 1e-6); }
    }

    [Fact]
    public void DefectoEnElInicio_EmpujaLasPiezasMasAdelante()
    {
        var p = Par(); p.Defectos.Add(new Defecto { Nombre = "orillo", Contorno = Rect(100, 500, 0, 0) });
        var r = Tizador.Calcular(new[] { P("1", 50, 50) }, p);
        Assert.True(r.Colocadas[0].Contorno.Limites.MinX >= 100 + p.Separacion - 1e-6);
    }

    [Fact]
    public void SinDefectos_OConLista_Vacia_ElResultadoEsElMismo()
    {
        var piezas = Enumerable.Range(1, 4).Select(i => P(i.ToString(), 110, 70, 2, new double[] { 0, 180 })).ToList();
        var a = Tizador.Calcular(piezas, Par());
        var p = Par(); p.Defectos = new();
        var b = Tizador.Calcular(piezas, p);
        Assert.Equal(a.Largo, b.Largo, 9); Assert.Equal(a.Colocadas.Count, b.Colocadas.Count);
    }

    [Fact]
    public void VariosDefectos_TodosSeRespetan()
    {
        var p = Par(600);
        p.Defectos.Add(new Defecto { Nombre = "a", Contorno = Rect(30, 30, 50, 50) });
        p.Defectos.Add(new Defecto { Nombre = "b", Contorno = new Poligono(new[] { new Punto(200, 100), new Punto(260, 120), new Punto(230, 220) }) });
        p.Defectos.Add(new Defecto { Nombre = "c", Contorno = Rect(10, 200, 400, 300) });
        var r = Tizador.Calcular(Enumerable.Range(1, 12).Select(i => P(i.ToString(), 90, 110)), p);
        Assert.Equal(12, r.Colocadas.Count);
        foreach (var c in r.Colocadas) foreach (var d in p.Defectos) Assert.False(Geometria.Solapan(c.Contorno, d.Contorno), d.Nombre);
        SinSolapes(r, p.Separacion);
    }

    [Fact]
    public void DefectoFueraDeLaTela_oInvalido_SeRechaza()
    {
        var p = Par(); p.Defectos.Add(new Defecto { Nombre = "afuera", Contorno = Rect(50, 50, 10, 480) });          // pasa de los 500 mm de ancho
        var ex = Assert.Throws<ArgumentException>(() => Tizador.Calcular(new[] { P("1", 50, 50) }, p));
        Assert.Contains("afuera", ex.Message);
        p = Par(); p.Defectos.Add(new Defecto { Nombre = "neg", Contorno = Rect(50, 50, -10, 10) });
        Assert.Throws<ArgumentException>(() => Tizador.Calcular(new[] { P("1", 50, 50) }, p));
        p = Par(); p.Defectos.Add(new Defecto { Nombre = "linea", Contorno = new Poligono(new[] { new Punto(0, 0), new Punto(10, 0), new Punto(20, 0) }) });
        Assert.Throws<ArgumentException>(() => Tizador.Calcular(new[] { P("1", 50, 50) }, p));
    }

    [Fact]
    public void LaPiezaQueSoloCabeEntreDefectos_SeColocaEnElHueco()
    {
        var p = Par(); p.Defectos.Add(new Defecto { Nombre = "d1", Contorno = Rect(30, 500, 0, 0) }); p.Defectos.Add(new Defecto { Nombre = "d2", Contorno = Rect(30, 500, 200, 0) });
        var r = Tizador.Calcular(new[] { P("1", 150, 300) }, p);             // hueco libre: x de 30 a 200 = 170 mm; 150 + 2 * 4 cabe
        Assert.Single(r.Colocadas);
        var l = r.Colocadas[0].Contorno.Limites; Assert.True(l.MinX >= 34 - 1e-6 && l.MaxX <= 196 + 1e-6);
    }

    // ===================== rayas =====================
    static Pieza R(string id, double w, double h, Punto pc, double faseX = 0, bool x = true, bool y = false, double faseY = 0, int cant = 1, int esp = 0, double[]? rot = null) =>
        new() { Id = id, Nombre = "P" + id, Talla = "M", Contorno = Rect(w, h), Cantidad = cant, CantidadEspejada = esp, Rotaciones = rot ?? new double[] { 0 }, PuntoCoincidencia = pc, CoincideX = x, CoincideY = y, FaseX = faseX, FaseY = faseY };

    [Fact]
    public void CoincidenciaEnX_ElPuntoCaeSobreLaRaya()
    {
        var p = Par(); p.PeriodoX = 100;
        var r = Tizador.Calcular(new[] { R("1", 80, 60, new Punto(10, 10)) }, p);
        var c = r.Colocadas[0];
        Assert.Equal(0, c.ErrorCoincidenciaX!.Value, 6);
        Assert.Equal(0, Mod(c.PuntoCoincidencia!.Value.X, 100), 6);
        Assert.Null(c.ErrorCoincidenciaY);
    }

    [Fact]
    public void ConFase_ElPuntoQuedaALaFaseIndicada()
    {
        var p = Par(); p.PeriodoX = 100;
        var r = Tizador.Calcular(new[] { R("1", 80, 60, new Punto(10, 10), faseX: 35) }, p);
        Assert.Equal(0, Mod(r.Colocadas[0].PuntoCoincidencia!.Value.X - 35, 100), 6);
    }

    [Fact]
    public void VariasPiezasConLaMismaFase_QuedanSobreLaMismaRaya_SinPisarse()
    {
        var p = Par(); p.PeriodoX = 90;
        var piezas = new[] { R("1", 100, 120, new Punto(0, 60), cant: 3), R("2", 60, 50, new Punto(20, 25), cant: 4), R("3", 140, 70, new Punto(70, 35), cant: 2) };
        var r = Tizador.Calcular(piezas, p);
        Assert.Equal(9, r.Colocadas.Count); Assert.Empty(r.NoColocadas);
        foreach (var c in r.Colocadas) { Assert.True(c.ErrorCoincidenciaX!.Value < 1e-6); Assert.True(Mod(c.PuntoCoincidencia!.Value.X, 90) < 1e-6); }
        SinSolapes(r, p.Separacion);
    }

    [Fact]
    public void ElPuntoSeEspejaYSeGiraConLaPieza()
    {
        var p = Par(); p.PeriodoX = 100;
        var r = Tizador.Calcular(new[] { R("1", 100, 50, new Punto(20, 10), cant: 2, esp: 1, rot: new double[] { 0, 180 }) }, p);
        foreach (var c in r.Colocadas)
        {
            var l = c.Contorno.Limites; var q = c.PuntoCoincidencia!.Value;
            double dx = q.X - l.MinX, dy = q.Y - l.MinY;
            bool directo = Math.Abs(dx - 20) < 1e-6 && Math.Abs(dy - 10) < 1e-6;
            bool espejo = Math.Abs(dx - 80) < 1e-6 && Math.Abs(dy - 10) < 1e-6;           // X -> 100 - X
            bool giro180 = Math.Abs(dx - 80) < 1e-6 && Math.Abs(dy - 40) < 1e-6;
            bool espejoGiro = Math.Abs(dx - 20) < 1e-6 && Math.Abs(dy - 40) < 1e-6;
            Assert.True(directo || espejo || giro180 || espejoGiro, $"punto en {dx},{dy}");
            Assert.True(c.ErrorCoincidenciaX!.Value < 1e-6);
        }
    }

    [Fact]
    public void CoincidenciaEnY_ParaCuadros_ElPuntoCaeSobreLaRayaDelAncho()
    {
        var p = Par(1000); p.PeriodoX = 100; p.PeriodoY = 150;
        var piezas = new[] { R("1", 90, 100, new Punto(10, 20), faseX: 0, x: true, y: true, faseY: 30, cant: 5) };
        var r = Tizador.Calcular(piezas, p);
        Assert.Equal(5, r.Colocadas.Count);
        foreach (var c in r.Colocadas)
        {
            Assert.True(Mod(c.PuntoCoincidencia!.Value.X, 100) < 1e-6);
            Assert.True(Mod(c.PuntoCoincidencia!.Value.Y - 30, 150) < 1e-6);
            Assert.True(c.Contorno.Limites.MinY >= p.Margen - 1e-6 && c.Contorno.Limites.MaxY <= 1000 - p.Margen + 1e-6);
        }
        SinSolapes(r, p.Separacion);
    }

    [Fact]
    public void SiEnElAnchoNoHayPosicionQueCoincida_SeRechazaConMotivo_YNoSeCuelga()
    {
        var p = Par(100, margen: 0); p.PeriodoX = 50; p.PeriodoY = 500;      // el ancho es 100 y las rayas estan cada 500 mm
        var r = Tizador.Calcular(new[] { R("1", 40, 90, new Punto(5, 45), faseY: 200, x: true, y: true) }, p);
        Assert.Empty(r.Colocadas);
        Assert.Contains("raya", Assert.Single(r.NoColocadas).Motivo);
    }

    [Fact]
    public void LasPiezasSinCoincidencia_ConviveConLasQueSi()
    {
        var p = Par(); p.PeriodoX = 80;
        var piezas = new[] { R("1", 100, 100, new Punto(0, 0), cant: 2), P("2", 50, 50, 6) };
        var r = Tizador.Calcular(piezas, p);
        Assert.Equal(8, r.Colocadas.Count);
        foreach (var c in r.Colocadas.Where(c => c.PiezaId == "1")) Assert.True(c.ErrorCoincidenciaX!.Value < 1e-6);
        foreach (var c in r.Colocadas.Where(c => c.PiezaId == "2")) { Assert.Null(c.ErrorCoincidenciaX); Assert.Null(c.PuntoCoincidencia); }
        SinSolapes(r, p.Separacion);
    }

    [Fact]
    public void SinBanderas_ElPeriodoNoCambiaNada()
    {
        var piezas = Enumerable.Range(1, 4).Select(i => P(i.ToString(), 110, 70, 2)).ToList();
        var a = Tizador.Calcular(piezas, Par());
        var p = Par(); p.PeriodoX = 77; p.PeriodoY = 33;
        var b = Tizador.Calcular(piezas, p);
        Assert.Equal(a.Largo, b.Largo, 9);
    }

    [Fact]
    public void ConCoincidencia_ElTizadoEsMasLargoQueSinEllaPeroNuncaMenor()
    {
        var sin = Tizador.Calcular(Enumerable.Range(1, 8).Select(i => P(i.ToString(), 90, 110)), Par());
        var p = Par(); p.PeriodoX = 130;
        var con = Tizador.Calcular(Enumerable.Range(1, 8).Select(i => R(i.ToString(), 90, 110, new Punto(10, 10))), p);
        Assert.Equal(8, con.Colocadas.Count);
        Assert.True(con.Largo >= sin.Largo - 1e-9);
    }

    [Fact]
    public void Coincidencia_ConDefectos_SeCumplenLasDosCosas()
    {
        var p = Par(600); p.PeriodoX = 100; p.Defectos.Add(new Defecto { Nombre = "mancha", Contorno = Rect(80, 120, 150, 100) });
        var r = Tizador.Calcular(Enumerable.Range(1, 8).Select(i => R(i.ToString(), 70, 90, new Punto(35, 45), cant: 1)), p);
        Assert.Equal(8, r.Colocadas.Count);
        foreach (var c in r.Colocadas) { Assert.True(c.ErrorCoincidenciaX!.Value < 1e-6); Assert.False(Geometria.Solapan(c.Contorno, p.Defectos[0].Contorno)); }
        SinSolapes(r, p.Separacion);
    }

    [Fact]
    public void ParametrosInvalidos_SeRechazan()
    {
        var p = Par();   // pide coincidencia sin periodo
        Assert.Contains("periodo", Assert.Throws<ArgumentException>(() => Tizador.Calcular(new[] { R("1", 50, 50, new Punto(1, 1)) }, p)).Message);
        p.PeriodoX = 100;
        Assert.Contains("punto de coincidencia", Assert.Throws<ArgumentException>(() => Tizador.Calcular(new[] { new Pieza { Id = "1", Nombre = "X", Talla = "M", Contorno = Rect(50, 50), CoincideX = true } }, p)).Message);
        p.PeriodoX = -1;
        Assert.Throws<ArgumentException>(() => Tizador.Calcular(new[] { P("1", 50, 50) }, p));
        p.PeriodoX = 100; p.PeriodoY = 0;
        Assert.Contains("en X", Assert.Throws<ArgumentException>(() => Tizador.Calcular(new[] { R("1", 50, 50, new Punto(1, 1), x: false, y: true) }, new ParametrosTizado { AnchoTela = 500, PeriodoX = 100 })).Message.Replace("el ancho", "en X"));
        Assert.Contains("raya en Y", Assert.Throws<ArgumentException>(() => Tizador.Calcular(new[] { R("1", 50, 50, new Punto(1, 1), x: false, y: true) }, new ParametrosTizado { AnchoTela = 500, PeriodoX = 100 })).Message);
    }

    // ===================== casos que no caen justo en la cuadricula =====================
    [Fact]
    public void DefectoFueraDeLaCuadricula_EnX_LaSeparacionSeRespetaAlMilimetro()
    {
        // franja de 20 mm en x = 152.5 (no multiplo de la celda de 5): cada pieza de 140 solo cabe a la izquierda si se pasa de la separacion
        var p = Par(500, sep: 10); p.Defectos.Add(new Defecto { Nombre = "franja", Contorno = Rect(20, 500, 152.5, 0) });
        var r = Tizador.Calcular(Enumerable.Range(1, 4).Select(i => P(i.ToString(), 140, 480)), p);
        Assert.Equal(4, r.Colocadas.Count);
        foreach (var c in r.Colocadas) Assert.True(Geometria.Distancia(c.Contorno, p.Defectos[0].Contorno) >= 10 - 1e-6 && !Geometria.Solapan(c.Contorno, p.Defectos[0].Contorno));
    }

    [Fact]
    public void DefectoFueraDeLaCuadricula_EnY_LaSeparacionSeRespetaAlMilimetro()
    {
        var p = Par(500, sep: 10); p.Defectos.Add(new Defecto { Nombre = "franja", Contorno = Rect(5000, 20, 0, 202.5) });
        var r = Tizador.Calcular(Enumerable.Range(1, 4).Select(i => P(i.ToString(), 100, 190)), p);
        Assert.Equal(4, r.Colocadas.Count);
        foreach (var c in r.Colocadas) Assert.True(Geometria.Distancia(c.Contorno, p.Defectos[0].Contorno) >= 10 - 1e-6 && !Geometria.Solapan(c.Contorno, p.Defectos[0].Contorno));
    }

    [Fact]
    public void DefectoConYNegativa_SeRechaza()
    {
        var p = Par(); p.Defectos.Add(new Defecto { Nombre = "neg", Contorno = Rect(50, 50, 10, -10) });
        Assert.Contains("neg", Assert.Throws<ArgumentException>(() => Tizador.Calcular(new[] { P("1", 50, 50) }, p)).Message);
    }

    [Fact]
    public void ConValoresQueNoSonMultiplosDeLaCelda_ElPuntoCaeExactamenteSobreLaRaya()
    {
        // punto (10.7, 3.3), fase 0.4 y periodo 97.3: el corrimiento dentro de la celda casi nunca es 0
        var p = Par(); p.PeriodoX = 97.3;
        var r = Tizador.Calcular(Enumerable.Range(1, 6).Select(i => R(i.ToString(), 80, 60, new Punto(10.7, 3.3), faseX: 0.4)), p);
        Assert.Equal(6, r.Colocadas.Count);
        foreach (var c in r.Colocadas) Assert.True(Mod(c.PuntoCoincidencia!.Value.X - 0.4, 97.3) < 1e-6, $"{c.PuntoCoincidencia}");
        // y el punto va con la pieza: la distancia del punto a la esquina de la caja no cambia
        foreach (var c in r.Colocadas) { var l = c.Contorno.Limites; Assert.Equal(10.7, c.PuntoCoincidencia!.Value.X - l.MinX, 6); Assert.Equal(3.3, c.PuntoCoincidencia!.Value.Y - l.MinY, 6); }
        SinSolapes(r, p.Separacion);
    }

    [Fact]
    public void ConCuadrosQueNoSonMultiplosDeLaCelda_ElPuntoCaeSobreLaRayaEnLasDosDirecciones()
    {
        var p = Par(1000); p.PeriodoX = 61.7; p.PeriodoY = 83.3;
        var r = Tizador.Calcular(Enumerable.Range(1, 5).Select(i => R(i.ToString(), 90, 100, new Punto(7.3, 11.9), faseX: 2.2, y: true, faseY: 9.1)), p);
        Assert.Equal(5, r.Colocadas.Count);
        foreach (var c in r.Colocadas) { Assert.True(Mod(c.PuntoCoincidencia!.Value.X - 2.2, 61.7) < 1e-6); Assert.True(Mod(c.PuntoCoincidencia!.Value.Y - 9.1, 83.3) < 1e-6); Assert.True(c.ErrorCoincidenciaX < 1e-6 && c.ErrorCoincidenciaY < 1e-6); }
        SinSolapes(r, p.Separacion);
    }

    [Fact]
    public void ElCorrimientoParaCaerSobreLaRaya_NoRobaLaSeparacionEntrePiezas_EnX()
    {
        var p = Par(500, sep: 10, margen: 0); p.PeriodoX = 47;
        var r = Tizador.Calcular(Enumerable.Range(1, 10).Select(i => R(i.ToString(), 60, 200, new Punto(13, 7), cant: 1)), p);
        Assert.Equal(10, r.Colocadas.Count);
        SinSolapes(r, 10);
    }

    [Fact]
    public void ElCorrimientoParaCaerSobreLaRaya_NoRobaLaSeparacionEntrePiezas_EnY()
    {
        var p = Par(700, sep: 10, margen: 0); p.PeriodoX = 20; p.PeriodoY = 37;
        var r = Tizador.Calcular(Enumerable.Range(1, 10).Select(i => R(i.ToString(), 60, 90, new Punto(13, 7), y: true)), p);
        Assert.Equal(10, r.Colocadas.Count);
        SinSolapes(r, 10);
    }

    [Fact]
    public void ConMuchasCombinaciones_ElCorrimientoNuncaRobaLaSeparacion_NiHacePisarse()
    {
        var rnd = new Random(2026);
        for (int k = 0; k < 40; k++)
        {
            var p = Par(600, sep: 10, margen: 0, celda: 5); p.PeriodoX = 20 + rnd.NextDouble() * 80; bool y = k % 2 == 1; if (y) p.PeriodoY = 30 + rnd.NextDouble() * 70;
            var piezas = Enumerable.Range(1, 8).Select(i => R(i.ToString(), 5 * rnd.Next(8, 20), 5 * rnd.Next(8, 24), new Punto(rnd.NextDouble() * 30, rnd.NextDouble() * 30), faseX: rnd.NextDouble() * 20, y: y, faseY: rnd.NextDouble() * 20)).ToList();
            var r = Tizador.Calcular(piezas, p);
            foreach (var c in r.Colocadas)
            {
                Assert.True(Mod(c.PuntoCoincidencia!.Value.X - piezas.First(q => q.Id == c.PiezaId).FaseX, p.PeriodoX) < 1e-6, $"caso {k}");
                if (y) Assert.True(Mod(c.PuntoCoincidencia!.Value.Y - piezas.First(q => q.Id == c.PiezaId).FaseY, p.PeriodoY) < 1e-6, $"caso {k}");
            }
            SinSolapes(r, 10);
        }
    }

    [Theory]
    [InlineData(90, 40, 20)]        // (x,y) -> (-y,x): el punto (20,10) queda a (40,20) de la caja de 50 x 100
    [InlineData(180, 80, 40)]
    [InlineData(270, 10, 80)]       // (x,y) -> (y,-x)
    public void ElPuntoSigueElGiroDeLaPieza(double giro, double esperadoX, double esperadoY)
    {
        var p = Par(); p.PeriodoX = 100;
        var r = Tizador.Calcular(new[] { R("1", 100, 50, new Punto(20, 10), rot: new[] { giro }) }, p);
        var c = Assert.Single(r.Colocadas); var l = c.Contorno.Limites;
        Assert.Equal(esperadoX, c.PuntoCoincidencia!.Value.X - l.MinX, 6); Assert.Equal(esperadoY, c.PuntoCoincidencia!.Value.Y - l.MinY, 6);
    }

    // ===================== dibujo y DXF =====================
    [Fact]
    public void ElSvg_DibujaLosDefectos_LasRayasYLosPuntosDeCoincidencia()
    {
        var p = Par(); p.PeriodoX = 100; p.PeriodoY = 250; p.Defectos.Add(new Defecto { Nombre = "man<cha>", Contorno = Rect(60, 80, 150, 200) });
        var r = Tizador.Calcular(new[] { R("1", 80, 60, new Punto(10, 10), x: true, y: false, cant: 2) }, p);
        var svg = Exportadores.ASvg(r, true, p.Defectos, p.PeriodoX, p.PeriodoY);
        Assert.Contains("data-defecto=\"man&lt;cha&gt;\"", svg);                       // el nombre se escapa
        Assert.Equal(2, System.Text.RegularExpressions.Regex.Matches(svg, "data-coincide=").Count);
        Assert.Contains("data-raya=\"x\"", svg); Assert.Contains("data-raya=\"y\"", svg);
        var sinNada = Exportadores.ASvg(r);
        Assert.DoesNotContain("data-defecto", sinNada); Assert.DoesNotContain("data-raya", sinNada);
        Assert.Contains("data-coincide", sinNada);                                     // el punto sale aunque no se pasen rayas
    }

    [Fact]
    public void ElSvg_NoDibujaMilesDeRayas()
    {
        var p = Par(); p.PeriodoX = 100;
        var r = Tizador.Calcular(new[] { P("1", 80, 60) }, p);
        r = new ResultadoTizado { Colocadas = r.Colocadas, Ancho = 500, Largo = 1_000_000 };
        Assert.DoesNotContain("data-raya", Exportadores.ASvg(r, true, null, 1, 0));    // 1 000 000 rayas: se omiten
        Assert.Contains("data-raya", Exportadores.ASvg(r, true, null, 5000, 0));      // 200 rayas: se dibujan
    }

    [Fact]
    public void ElDxf_LlevaLosDefectosEnSuCapa()
    {
        var p = Par(); p.Defectos.Add(new Defecto { Nombre = "d", Contorno = Rect(50, 50, 10, 10) });
        var r = Tizador.Calcular(new[] { P("1", 80, 60) }, p);
        Assert.Contains("DEFECTOS", Exportadores.ADxf(r, p.Defectos));
        Assert.DoesNotContain("DEFECTOS", Exportadores.ADxf(r));
    }
}
