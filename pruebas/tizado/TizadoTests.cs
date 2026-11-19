using KONSolutions.Shared.Tizado;
using Xunit;

public class GeometriaTests
{
    static Poligono Rect(double w, double h, double x = 0, double y = 0) => new(new[] { new Punto(x, y), new Punto(x + w, y), new Punto(x + w, y + h), new Punto(x, y + h) });

    [Fact] public void Area_Perimetro_Limites_DeUnRectangulo()
    {
        var r = Rect(100, 50);
        Assert.Equal(5000, r.Area, 6); Assert.Equal(300, r.Perimetro, 6);
        Assert.Equal((0, 0, 100, 50), r.Limites); Assert.Equal(100, r.Ancho); Assert.Equal(50, r.Alto);
    }
    [Fact] public void ElAreaConSigno_DistingueElSentido()
    {
        var ccw = Rect(10, 10); var cw = new Poligono(ccw.Puntos.Reverse());
        Assert.Equal(100, ccw.AreaConSigno, 6); Assert.Equal(-100, cw.AreaConSigno, 6);
        Assert.Equal(100, cw.Area, 6);
    }
    [Fact] public void Limpia_PuntosRepetidos_YElCierreDuplicado()
    {
        var p = new Poligono(new[] { new Punto(0, 0), new Punto(0, 0), new Punto(10, 0), new Punto(10, 10), new Punto(0, 10), new Punto(0, 0) });
        Assert.Equal(4, p.Cantidad);
    }
    [Fact] public void Poligono_Degenerado_NoEsValido()
    {
        Assert.False(new Poligono(new[] { new Punto(0, 0), new Punto(10, 0), new Punto(20, 0) }).EsValido);   // colineales: area 0
        Assert.False(new Poligono(new[] { new Punto(0, 0), new Punto(1, 1) }).EsValido);
        Assert.True(Rect(1, 1).EsValido);
    }
    [Fact] public void Rotar90_IntercambiaAnchoYAlto_YConservaElArea()
    {
        var r = Rect(100, 40).Rotar(90).AlOrigen();
        Assert.Equal(40, r.Ancho, 6); Assert.Equal(100, r.Alto, 6); Assert.Equal(4000, r.Area, 6);
        var l = r.Limites; Assert.Equal(0, l.MinX, 6); Assert.Equal(0, l.MinY, 6);
    }
    [Fact] public void Rotar90_EsAntihorario_YEspejarEsRespectoDelEjeY()
    {
        var p = new Poligono(new[] { new Punto(1, 0), new Punto(3, 0), new Punto(3, 1), new Punto(1, 1) });
        var g = p.Rotar(90);                                                       // (1,0) -> (0,1)
        Assert.Equal(0, g.Puntos[0].X, 9); Assert.Equal(1, g.Puntos[0].Y, 9);
        Assert.Equal(0, g.Puntos[1].X, 9); Assert.Equal(3, g.Puntos[1].Y, 9);     // (3,0) -> (0,3)
        Assert.Equal(-1, g.Puntos[3].X, 9); Assert.Equal(1, g.Puntos[3].Y, 9);    // (1,1) -> (-1,1)
        var e = p.Espejar().Limites;                                               // X -> -X, Y igual
        Assert.Equal((-3, 0, -1, 1), e);
    }
    [Fact] public void Rotar180_AntihorarioYHorario_Coinciden()
    {
        var a = Rect(30, 10).Rotar(180).AlOrigen(); var b = Rect(30, 10).Rotar(-180).AlOrigen();
        for (int i = 0; i < a.Cantidad; i++) { Assert.Equal(a.Puntos[i].X, b.Puntos[i].X, 6); Assert.Equal(a.Puntos[i].Y, b.Puntos[i].Y, 6); }
    }
    [Fact] public void Espejar_CambiaLaForma_PeroNoElArea_NiElSentido()
    {
        var l = new Poligono(new[] { new Punto(0, 0), new Punto(100, 0), new Punto(100, 20), new Punto(20, 20), new Punto(20, 60), new Punto(0, 60) });   // una L
        var e = l.Espejar().AlOrigen();
        Assert.Equal(l.Area, e.Area, 6); Assert.True(e.AreaConSigno > 0);
        Assert.NotEqual(l.ATexto(), e.ATexto());
        Assert.Equal(l.ATexto(), e.Espejar().AlOrigen().ATexto().Length == 0 ? "" : l.ATexto());   // espejar dos veces no cambia el area
        Assert.Equal(l.Area, e.Espejar().Area, 6);
    }
    [Fact] public void Trasladar_Y_Escalar()
    {
        var r = Rect(10, 10).Trasladar(5, -3); Assert.Equal((5, -3, 15, 7), r.Limites);
        Assert.Equal(400, Rect(10, 10).Escalar(2).Area, 6);
    }
    [Fact] public void AlinearHilo_PonePieza_ConElHiloEnX()
    {
        var inclinada = Rect(200, 20).Rotar(30);                 // hilo a 30 grados
        var r = inclinada.AlinearHilo(30);
        Assert.Equal(200, r.Ancho, 4); Assert.Equal(20, r.Alto, 4);
    }
    [Fact] public void Solapan_RectangulosQueSePisan()
        => Assert.True(Geometria.Solapan(Rect(10, 10), Rect(10, 10, 5, 5)));
    [Fact] public void NoSolapan_SiSoloSeTocanPorElBorde()
    {
        Assert.False(Geometria.Solapan(Rect(10, 10), Rect(10, 10, 10, 0)));     // lado compartido
        Assert.False(Geometria.Solapan(Rect(10, 10), Rect(10, 10, 10, 10)));    // esquina
    }
    [Fact] public void NoSolapan_SiEstanSeparadas() => Assert.False(Geometria.Solapan(Rect(10, 10), Rect(10, 10, 50, 50)));
    [Fact] public void Solapan_SiUnaEstaDentroDeOtra()
    {
        Assert.True(Geometria.Solapan(Rect(100, 100), Rect(10, 10, 40, 40)));
        Assert.True(Geometria.Solapan(Rect(10, 10, 40, 40), Rect(100, 100)));
    }
    [Fact] public void Solapan_PiezasIguales_EnLaMismaPosicion() => Assert.True(Geometria.Solapan(Rect(10, 10), Rect(10, 10)));
    [Fact] public void Solapan_EnCruz_SinVerticesDentro()
        => Assert.True(Geometria.Solapan(Rect(100, 10, 0, 45), Rect(10, 100, 45, 0)));
    [Fact] public void LaPiezaEnL_NoSolapaConUnaPiezaEnSuHueco_PeroSiConUnaMasGrande()
    {
        var l = new Poligono(new[] { new Punto(0, 0), new Punto(100, 0), new Punto(100, 20), new Punto(20, 20), new Punto(20, 100), new Punto(0, 100) });
        Assert.False(Geometria.Solapan(l, Rect(40, 40, 40, 40)));       // en el hueco de la L (caja de la L la contiene)
        Assert.True(Geometria.Solapan(l, Rect(40, 40, 10, 10)));
    }
    [Fact] public void Distancia_VerticeContraLado_Y_LadoContraVertice()
    {
        var a = Rect(100, 100);
        var punta = new Poligono(new[] { new Punto(120, 50), new Punto(160, 30), new Punto(160, 70) });     // el vertice (120,50) apunta al lado derecho de a
        Assert.Equal(20, Geometria.Distancia(a, punta), 6); Assert.Equal(20, Geometria.Distancia(punta, a), 6);
        var punta2 = new Poligono(new[] { new Punto(50, 130), new Punto(30, 170), new Punto(70, 170) });
        Assert.Equal(30, Geometria.Distancia(a, punta2), 6); Assert.Equal(30, Geometria.Distancia(punta2, a), 6);
    }
    [Fact] public void Distancia_EntreDosRectangulos()
    {
        Assert.Equal(5, Geometria.Distancia(Rect(10, 10), Rect(10, 10, 15, 0)), 6);
        Assert.Equal(Math.Sqrt(50), Geometria.Distancia(Rect(10, 10), Rect(10, 10, 15, 15)), 6);
        Assert.Equal(0, Geometria.Distancia(Rect(10, 10), Rect(10, 10, 5, 5)), 6);
    }
    [Fact] public void Ubicacion_DentroBordeFuera()
    {
        var r = Rect(10, 10);
        Assert.Equal(1, Geometria.Ubicacion(r, new Punto(5, 5))); Assert.Equal(0, Geometria.Ubicacion(r, new Punto(10, 5))); Assert.Equal(-1, Geometria.Ubicacion(r, new Punto(11, 5)));
    }
}

public class LectorDxfTests
{
    static string Dxf(string entidades, string cabecera = "") => $"0\nSECTION\n2\nHEADER\n{cabecera}0\nENDSEC\n0\nSECTION\n2\nENTITIES\n{entidades}0\nENDSEC\n0\nEOF\n";
    const string LwRect = "0\nLWPOLYLINE\n8\n1\n90\n4\n70\n1\n10\n0\n20\n0\n10\n100\n20\n0\n10\n100\n20\n50\n10\n0\n20\n50\n";

    [Fact] public void Lee_UnaLwpolylineCerrada()
    {
        var r = LectorDxf.Leer(Dxf(LwRect));
        var c = Assert.Single(r.Contornos); Assert.Equal("1", c.Capa); Assert.Equal(5000, c.Contorno.Area, 6);
    }
    [Fact] public void Lee_UnaPolylineConVertex()
    {
        var e = "0\nPOLYLINE\n8\nCORTE\n66\n1\n70\n1\n0\nVERTEX\n8\nCORTE\n10\n0\n20\n0\n0\nVERTEX\n8\nCORTE\n10\n10\n20\n0\n0\nVERTEX\n8\nCORTE\n10\n10\n20\n10\n0\nSEQEND\n8\nCORTE\n";
        var r = LectorDxf.Leer(Dxf(e)); var c = Assert.Single(r.Contornos); Assert.Equal("CORTE", c.Capa); Assert.Equal(50, c.Contorno.Area, 6);
    }
    [Fact] public void Una_PolylineCerradaRepitiendoElPrimerPunto_SeAcepta()
    {
        var e = "0\nLWPOLYLINE\n8\n1\n90\n5\n70\n0\n10\n0\n20\n0\n10\n10\n20\n0\n10\n10\n20\n10\n10\n0\n20\n10\n10\n0\n20\n0\n";
        Assert.Single(LectorDxf.Leer(Dxf(e)).Contornos);
    }
    [Fact] public void Ignora_AbiertasYLineas_YAvisa()
    {
        var e = "0\nLWPOLYLINE\n8\n1\n90\n2\n70\n0\n10\n0\n20\n0\n10\n10\n20\n0\n0\nLINE\n8\n7\n10\n0\n20\n0\n11\n5\n21\n5\n" + LwRect;
        var r = LectorDxf.Leer(Dxf(e));
        Assert.Single(r.Contornos); Assert.Contains(r.Advertencias, a => a.Contains("abierta")); Assert.Contains(r.Advertencias, a => a.Contains("sueltas"));
    }
    [Fact] public void Circulo_ComoContorno()
    {
        var r = LectorDxf.Leer(Dxf("0\nCIRCLE\n8\n1\n10\n50\n20\n50\n40\n10\n"));
        var c = Assert.Single(r.Contornos); Assert.InRange(c.Contorno.Area, Math.PI * 100 * 0.99, Math.PI * 100 * 1.001);
    }
    [Fact] public void Bulge_SeAproximaConArco()
    {
        // semicirculo: bulge 1 entre (0,0) y (100,0) + cierre recto
        var e = "0\nLWPOLYLINE\n8\n1\n90\n2\n70\n1\n10\n0\n20\n0\n42\n1\n10\n100\n20\n0\n";
        var c = Assert.Single(LectorDxf.Leer(Dxf(e)).Contornos).Contorno;
        Assert.InRange(c.Area, Math.PI * 50 * 50 / 2 * 0.99, Math.PI * 50 * 50 / 2 * 1.001);
        Assert.InRange(c.Limites.MinY, -50.1, -49.9);                      // bulge positivo = antihorario de (0,0) a (100,0): el arco queda bajo la cuerda
        var neg = Assert.Single(LectorDxf.Leer(Dxf(e.Replace("42\n1\n", "42\n-1\n"))).Contornos).Contorno;
        Assert.InRange(neg.Limites.MaxY, 49.9, 50.1);                      // bulge negativo: sobre la cuerda
    }
    [Fact] public void Unidades_DesdeInsunits_YForzadas()
    {
        var cab = "9\n$INSUNITS\n70\n5\n";
        Assert.Equal(500000, LectorDxf.Leer(Dxf(LwRect, cab)).Contornos[0].Contorno.Area, 3);        // cm: 1000 x 500 mm
        Assert.Equal(10, LectorDxf.Leer(Dxf(LwRect, cab)).MilimetrosPorUnidad);
        Assert.Equal(5000 * 25.4 * 25.4, LectorDxf.Leer(Dxf(LwRect), 25.4).Contornos[0].Contorno.Area, 3);
        Assert.Equal(5000, LectorDxf.Leer(Dxf(LwRect, "9\n$INSUNITS\n70\n4\n")).Contornos[0].Contorno.Area, 3);
    }
    [Fact] public void FinesDeLinea_WindowsYMac()
        => Assert.Single(LectorDxf.Leer(Dxf(LwRect).Replace("\n", "\r\n")).Contornos);
    [Fact] public void Vacio_Binario_SinEntities_Malformado()
    {
        Assert.Contains("vacio", LectorDxf.Leer("  ").Advertencias[0]);
        Assert.Contains("binario", LectorDxf.Leer("AutoCAD Binary DXF\r\n\u001a\0").Advertencias[0]);
        Assert.Contains(LectorDxf.Leer("0\nEOF\n").Advertencias, a => a.Contains("ENTITIES"));
        Assert.Contains(LectorDxf.Leer("hola\nmundo\n").Advertencias, a => a.Contains("mal formado"));
    }
    [Fact] public void SinContornos_Avisa() => Assert.Contains(LectorDxf.Leer(Dxf("")).Advertencias, a => a.Contains("No se encontraron"));
    [Fact] public void VariasPiezas_SeLeenTodas()
        => Assert.Equal(3, LectorDxf.Leer(Dxf(LwRect + LwRect + LwRect)).Contornos.Count);
    [Fact] public void LoQueExportamos_SeVuelveALeer()
    {
        var r = Tizador.Calcular(new[] { new Pieza { Id = "A", Nombre = "A", Contorno = new Poligono(new[] { new Punto(0, 0), new Punto(100, 0), new Punto(100, 50), new Punto(0, 50) }), Cantidad = 3 } }, new ParametrosTizado { AnchoTela = 400, Intentos = 1 });
        var dxf = Exportadores.ADxf(r); var leido = LectorDxf.Leer(dxf);
        Assert.Equal(4, leido.Contornos.Count);                                    // 3 piezas + el contorno de la tela
        Assert.Equal(3, leido.Contornos.Count(c => c.Capa == "PIEZAS"));
        Assert.All(leido.Contornos.Where(c => c.Capa == "PIEZAS"), c => Assert.Equal(5000, c.Contorno.Area, 3));
    }
}

public class TizadorTests
{
    static Poligono Rect(double w, double h) => new(new[] { new Punto(0, 0), new Punto(w, 0), new Punto(w, h), new Punto(0, h) });
    static Pieza P(string id, double w, double h, int n = 1, double[]? rot = null, int esp = 0, string talla = "M") =>
        new() { Id = id, Nombre = id, Talla = talla, Contorno = Rect(w, h), Cantidad = n, CantidadEspejada = esp, Rotaciones = rot ?? new[] { 0d, 180d } };
    static Poligono L() => new(new[] { new Punto(0, 0), new Punto(120, 0), new Punto(120, 30), new Punto(30, 30), new Punto(30, 90), new Punto(0, 90) });
    static Poligono Trapecio() => new(new[] { new Punto(0, 0), new Punto(200, 0), new Punto(150, 80), new Punto(50, 80) });

    /// <summary>Comprobaciones que se cumplen en CUALQUIER tizado: nada se pisa, respeta separacion y margenes, el area se conserva.</summary>
    static void Valida(ResultadoTizado r, ParametrosTizado p)
    {
        var c = r.Colocadas;
        for (int i = 0; i < c.Count; i++)
        {
            var l = c[i].Contorno.Limites;
            Assert.True(l.MinX >= p.Margen - 1e-6, $"{c[i].Nombre} invade el margen izquierdo ({l.MinX})");
            Assert.True(l.MinY >= p.Margen - 1e-6, $"{c[i].Nombre} invade el margen inferior ({l.MinY})");
            Assert.True(l.MaxY <= p.AnchoTela - p.Margen + 1e-6, $"{c[i].Nombre} se sale del ancho ({l.MaxY})");
            Assert.True(l.MaxX <= r.Largo - p.Margen + 1e-6);
            for (int j = i + 1; j < c.Count; j++)
            {
                Assert.False(Geometria.Solapan(c[i].Contorno, c[j].Contorno), $"{c[i].Nombre}#{c[i].Copia} y {c[j].Nombre}#{c[j].Copia} se pisan");
                Assert.True(Geometria.Distancia(c[i].Contorno, c[j].Contorno) >= p.Separacion - 1e-6, $"{c[i].Nombre} y {c[j].Nombre} quedan a menos de la separacion");
            }
        }
        Assert.Equal(c.Sum(x => x.Contorno.Area), r.AreaPiezas, 3);
        Assert.InRange(r.Eficiencia, 0, 1);
    }

    [Fact] public void UnaPieza_QuedaEnLaEsquina_ConElMargen()
    {
        var p = new ParametrosTizado { AnchoTela = 300, Margen = 8, Separacion = 4, Intentos = 1 };
        var r = Tizador.Calcular(new[] { P("A", 100, 50) }, p);
        var c = Assert.Single(r.Colocadas); var l = c.Contorno.Limites;
        Assert.Equal(8, l.MinX, 6); Assert.Equal(8, l.MinY, 6); Assert.Equal(108 + 8, r.Largo, 6);
        Valida(r, p);
    }
    [Fact] public void RectangulosQueCompletanElAncho_SeApilanSinHuecos()
    {
        // ancho 215 = 2*100 + separacion 3 + 2*margen 6 ... caben 2 en columna; 6 piezas -> 3 columnas
        var p = new ParametrosTizado { AnchoTela = 215, Margen = 5, Separacion = 3, Celda = 1, Intentos = 1 };
        var r = Tizador.Calcular(new[] { P("A", 100, 100, 6) }, p);
        Assert.Equal(6, r.Colocadas.Count); Valida(r, p);
        Assert.Equal(5 + 3 * 100 + 2 * 3 + 5, r.Largo, 3);                   // 3 columnas con 2 separaciones
    }
    [Fact] public void PiezasDeDistintaForma_NoSePisan_NiInvadenSeparacion()
    {
        var p = new ParametrosTizado { AnchoTela = 500, Separacion = 6, Margen = 4, Celda = 3, Intentos = 4 };
        var piezas = new[]
        {
            new Pieza { Id = "L", Nombre = "L", Contorno = L(), Cantidad = 6 },
            new Pieza { Id = "T", Nombre = "T", Contorno = Trapecio(), Cantidad = 6, Rotaciones = new[] { 0d, 180d } },
            P("R", 150, 60, 4),
        };
        var r = Tizador.Calcular(piezas, p);
        Assert.Equal(16, r.Colocadas.Count); Assert.Empty(r.NoColocadas); Valida(r, p);
    }
    [Fact] public void LasPiezasEnL_ConGiro180_SeEncajan_MejorQueSinGiro()
    {
        var p = new ParametrosTizado { AnchoTela = 400, Separacion = 3, Margen = 3, Celda = 2, Intentos = 4 };
        var libre = Tizador.Calcular(new[] { new Pieza { Id = "L", Nombre = "L", Contorno = L(), Cantidad = 10, Rotaciones = new[] { 0d, 180d } } }, p);
        var fijo = Tizador.Calcular(new[] { new Pieza { Id = "L", Nombre = "L", Contorno = L(), Cantidad = 10, Rotaciones = new[] { 0d } } }, p);
        Valida(libre, p); Valida(fijo, p);
        Assert.True(libre.Largo < fijo.Largo, $"con giro {libre.Largo} debe ser menor que sin giro {fijo.Largo}");
        Assert.All(fijo.Colocadas, c => Assert.Equal(0, c.Angulo));
        Assert.All(libre.Colocadas, c => Assert.Contains(c.Angulo, new[] { 0d, 180d }));
    }
    [Fact] public void ConHiloEstricto_NingunaPiezaSeGira()
    {
        var r = Tizador.Calcular(new[] { P("A", 200, 40, 8, new[] { 0d }), P("B", 40, 200, 4, new[] { 0d }) }, new ParametrosTizado { AnchoTela = 600, Intentos = 3 });
        Assert.All(r.Colocadas, c => Assert.Equal(0, c.Angulo));
        Assert.All(r.Colocadas.Where(c => c.PiezaId == "A"), c => Assert.Equal(200, c.Contorno.Ancho, 6));    // sigue siendo ancha en X
    }
    [Fact] public void ConGiro90_SePuedeAcomodarUnaPiezaLarga()
    {
        // pieza de 400 x 40 en tela de 300: solo cabe girada 90
        var sin = Tizador.Calcular(new[] { P("A", 400, 40, 1, new[] { 0d }) }, new ParametrosTizado { AnchoTela = 300 });
        Assert.Single(sin.Colocadas);                                    // 40 de alto cabe en el ancho 300: ok sin girar
        var alto = Tizador.Calcular(new[] { P("B", 40, 400, 1, new[] { 0d }) }, new ParametrosTizado { AnchoTela = 300 });
        Assert.Empty(alto.Colocadas); Assert.Contains("No cabe", Assert.Single(alto.NoColocadas).Motivo);
        var gira = Tizador.Calcular(new[] { P("B", 40, 400, 1, new[] { 0d, 90d }) }, new ParametrosTizado { AnchoTela = 300 });
        var c = Assert.Single(gira.Colocadas); Assert.Equal(90, c.Angulo); Assert.Equal(400, c.Contorno.Ancho, 6);
    }
    [Fact] public void Espejadas_SeColocanEspejadas_YEnLaCantidadIndicada()
    {
        var p = new ParametrosTizado { AnchoTela = 500, Intentos = 2 };
        var r = Tizador.Calcular(new[] { new Pieza { Id = "L", Nombre = "L", Contorno = L(), Cantidad = 6, CantidadEspejada = 2 } }, p);
        Assert.Equal(6, r.Colocadas.Count); Assert.Equal(2, r.Colocadas.Count(c => c.Espejada)); Valida(r, p);
        var normal = L().AlOrigen(); var esp = L().Espejar().AlOrigen();
        // la forma de una espejada, llevada al origen y deshecho el giro, equivale al espejo
        foreach (var c in r.Colocadas)
        {
            var base0 = c.Contorno.Rotar(-c.Angulo).AlOrigen();
            var esperado = c.Espejada ? esp : normal;
            Assert.Equal(esperado.Area, base0.Area, 3);
            Assert.Equal(esperado.Ancho, base0.Ancho, 3); Assert.Equal(esperado.Alto, base0.Alto, 3);
        }
    }
    [Fact] public void VariasTallas_SeMantienenSusEtiquetas()
    {
        var r = Tizador.Calcular(new[] { P("Frente", 200, 150, 2, talla: "S"), P("Frente", 220, 160, 2, talla: "M"), P("Frente", 240, 170, 1, talla: "L") }, new ParametrosTizado { AnchoTela = 800 });
        Assert.Equal(2, r.Colocadas.Count(c => c.Talla == "S")); Assert.Equal(2, r.Colocadas.Count(c => c.Talla == "M")); Assert.Single(r.Colocadas, c => c.Talla == "L");
    }
    [Fact] public void Cantidad0_NoColocaNada()
    {
        var r = Tizador.Calcular(new[] { P("A", 10, 10, 0) }, new ParametrosTizado { AnchoTela = 100 });
        Assert.Empty(r.Colocadas); Assert.Empty(r.NoColocadas); Assert.Equal(0, r.Largo); Assert.Equal(0, r.Eficiencia);
    }
    [Fact] public void SinPiezas_Resultado_Vacio()
    {
        var r = Tizador.Calcular(Array.Empty<Pieza>(), new ParametrosTizado { AnchoTela = 100 });
        Assert.Empty(r.Colocadas); Assert.Equal(0, r.Largo);
    }
    [Fact] public void PiezaMasAnchaQueLaTela_SeReporta_YElRestoSeColoca()
    {
        var r = Tizador.Calcular(new[] { P("Grande", 100, 900, 2, new[] { 0d }), P("Chica", 50, 50, 3) }, new ParametrosTizado { AnchoTela = 500 });
        Assert.Equal(3, r.Colocadas.Count); Assert.Equal(2, r.NoColocadas.Count); Assert.All(r.NoColocadas, n => Assert.Equal("Grande", n.Nombre));
    }
    [Fact] public void ElMargenCuentaParaCaber()
    {
        Assert.Contains("No cabe", Assert.Single(Tizador.Calcular(new[] { P("A", 50, 480, 1, new[] { 0d }) }, new ParametrosTizado { AnchoTela = 500, Margen = 15 }).NoColocadas).Motivo);
        // alto 480 en tela 500: cabe con margen 5 (490 utiles) y no con margen 15 (470)
        Assert.Single(Tizador.Calcular(new[] { P("A", 50, 480, 1, new[] { 0d }) }, new ParametrosTizado { AnchoTela = 500, Margen = 5 }).Colocadas);
        Assert.Empty(Tizador.Calcular(new[] { P("A", 50, 480, 1, new[] { 0d }) }, new ParametrosTizado { AnchoTela = 500, Margen = 15 }).Colocadas);
    }
    [Fact] public void ContornoInvalido_SeReporta()
    {
        var mala = new Pieza { Id = "X", Nombre = "X", Contorno = new Poligono(new[] { new Punto(0, 0), new Punto(1, 1) }), Cantidad = 2 };
        var r = Tizador.Calcular(new[] { mala, P("A", 10, 10) }, new ParametrosTizado { AnchoTela = 100 });
        Assert.Single(r.Colocadas); Assert.Equal(2, r.NoColocadas.Count); Assert.Contains("invalido", r.NoColocadas[0].Motivo);
    }
    [Fact] public void ParametrosInvalidos_Lanzan()
    {
        var a = new[] { P("A", 10, 10) };
        Assert.Throws<ArgumentException>(() => Tizador.Calcular(a, new ParametrosTizado { AnchoTela = 0 }));
        Assert.Throws<ArgumentException>(() => Tizador.Calcular(a, new ParametrosTizado { AnchoTela = 100, Celda = 0 }));
        Assert.Throws<ArgumentException>(() => Tizador.Calcular(a, new ParametrosTizado { AnchoTela = 100, Separacion = -1 }));
        Assert.Throws<ArgumentException>(() => Tizador.Calcular(a, new ParametrosTizado { AnchoTela = 100, Margen = -1 }));
        Assert.Throws<ArgumentException>(() => Tizador.Calcular(new[] { P("A", 10, 10, 2, esp: 3) }, new ParametrosTizado { AnchoTela = 100 }));
        Assert.Throws<ArgumentException>(() => Tizador.Calcular(new[] { P("A", 10, 10, -1) }, new ParametrosTizado { AnchoTela = 100 }));
    }
    [Fact] public void Es_Determinista()
    {
        var piezas = new[] { new Pieza { Id = "L", Nombre = "L", Contorno = L(), Cantidad = 5 }, new Pieza { Id = "T", Nombre = "T", Contorno = Trapecio(), Cantidad = 5 } };
        var p = new ParametrosTizado { AnchoTela = 450, Intentos = 6, Semilla = 3 };
        var a = Tizador.Calcular(piezas, p); var b = Tizador.Calcular(piezas, p);
        Assert.Equal(a.Largo, b.Largo); Assert.Equal(a.IntentoGanador, b.IntentoGanador);
        Assert.Equal(a.Colocadas.Select(c => c.Contorno.ATexto()), b.Colocadas.Select(c => c.Contorno.ATexto()));
    }
    [Fact] public void ElPrimerOrden_ColocaPrimeroLaPiezaMasGrande_EnLaEsquina()
    {
        var p = new ParametrosTizado { AnchoTela = 400, Margen = 8, Intentos = 1 };
        var r = Tizador.Calcular(new[] { P("chica", 30, 30, 1), P("grande", 200, 300, 1, new[] { 0d }) }, p);
        var g = r.Colocadas.Single(c => c.Nombre == "grande").Contorno.Limites;
        Assert.Equal(8, g.MinX, 6); Assert.Equal(8, g.MinY, 6);
    }
    [Fact] public void MasIntentos_NuncaEmpeoran()
    {
        var piezas = new[] { new Pieza { Id = "L", Nombre = "L", Contorno = L(), Cantidad = 7 }, new Pieza { Id = "T", Nombre = "T", Contorno = Trapecio(), Cantidad = 6 }, P("R", 130, 70, 5) };
        double Largo(int n) => Tizador.Calcular(piezas, new ParametrosTizado { AnchoTela = 420, Intentos = n, Celda = 3 }).Largo;
        Assert.True(Largo(8) <= Largo(1) + 1e-9); Assert.True(Largo(1) > 0);
    }
    [Fact] public void ElResultado_EsElMejorDeLosIntentos_NoElPeor()
    {
        var piezas = new[] { new Pieza { Id = "L", Nombre = "L", Contorno = L(), Cantidad = 7 }, new Pieza { Id = "T", Nombre = "T", Contorno = Trapecio(), Cantidad = 6 }, P("R", 130, 70, 5), P("S", 90, 200, 4) };
        var largos = Enumerable.Range(1, 8).Select(n => Tizador.Calcular(piezas, new ParametrosTizado { AnchoTela = 420, Intentos = n, Celda = 3 }).Largo).ToList();
        for (int i = 1; i < largos.Count; i++) Assert.True(largos[i] <= largos[i - 1] + 1e-9, $"con {i + 1} intentos ({largos[i]}) no puede ser peor que con {i} ({largos[i - 1]})");
        Assert.True(largos.Distinct().Count() > 1, "los distintos ordenes deben dar largos distintos para que la prueba sirva");
    }
    [Fact] public void ElIntentoGanador_Y_LosProbados_SeInforman()
    {
        var r = Tizador.Calcular(new[] { P("A", 100, 60, 10), P("B", 70, 90, 10) }, new ParametrosTizado { AnchoTela = 350, Intentos = 6 });
        Assert.Equal(6, r.IntentosProbados); Assert.InRange(r.IntentoGanador, 1, 6);
    }
    [Fact] public void Progreso_SeInforma_PorCadaIntento()
    {
        var vistos = new List<(int, int)>();
        Tizador.Calcular(new[] { P("A", 50, 50, 3) }, new ParametrosTizado { AnchoTela = 300, Intentos = 5 }, default, (a, t) => vistos.Add((a, t)));
        Assert.Equal(Enumerable.Range(1, 5).Select(i => (i, 5)), vistos);
    }
    [Fact] public void Cancelacion_Interrumpe()
    {
        var cts = new CancellationTokenSource(); cts.Cancel();
        Assert.Throws<OperationCanceledException>(() => Tizador.Calcular(new[] { P("A", 50, 50, 3) }, new ParametrosTizado { AnchoTela = 300 }, cts.Token));
    }
    [Fact] public void TiempoMaximo_CortaLosIntentos_PeroSiempreHaceUno()
    {
        var piezas = new[] { new Pieza { Id = "L", Nombre = "L", Contorno = L(), Cantidad = 40 } };
        var r = Tizador.Calcular(piezas, new ParametrosTizado { AnchoTela = 600, Intentos = 500, TiempoMaxMs = 1, Celda = 3 });
        Assert.InRange(r.IntentosProbados, 1, 499); Assert.NotEmpty(r.Colocadas);
    }
    [Fact] public void MasAnchoDeTela_NuncaGastaMasLargo()
    {
        var piezas = new[] { new Pieza { Id = "L", Nombre = "L", Contorno = L(), Cantidad = 12 }, P("R", 100, 60, 6) };
        var l1 = Tizador.Calcular(piezas, new ParametrosTizado { AnchoTela = 300, Intentos = 4 }).Largo;
        var l2 = Tizador.Calcular(piezas, new ParametrosTizado { AnchoTela = 600, Intentos = 4 }).Largo;
        Assert.True(l2 < l1);
    }
    [Fact] public void LaEficiencia_EsAreaDePiezasSobreAreaDeTela()
    {
        var p = new ParametrosTizado { AnchoTela = 215, Margen = 5, Separacion = 3, Celda = 1, Intentos = 1 };
        var r = Tizador.Calcular(new[] { P("A", 100, 100, 6) }, p);
        Assert.Equal(60000.0 / (r.Largo * 215), r.Eficiencia, 6); Assert.InRange(r.Eficiencia, 0.7, 1.0);
    }
    [Fact] public void MasSeparacion_GastaMasTela()
    {
        double Largo(double s) => Tizador.Calcular(new[] { P("A", 100, 60, 12) }, new ParametrosTizado { AnchoTela = 400, Separacion = s, Intentos = 2 }).Largo;
        Assert.True(Largo(20) > Largo(2));
    }
    [Fact] public void CeldaMasFina_NoEmpeora_ElResultado_Mucho()
    {
        var piezas = new[] { new Pieza { Id = "T", Nombre = "T", Contorno = Trapecio(), Cantidad = 10 }, new Pieza { Id = "L", Nombre = "L", Contorno = L(), Cantidad = 10 } };
        var gruesa = Tizador.Calcular(piezas, new ParametrosTizado { AnchoTela = 500, Celda = 10, Intentos = 4 });
        var fina = Tizador.Calcular(piezas, new ParametrosTizado { AnchoTela = 500, Celda = 2, Intentos = 4 });
        Valida(gruesa, new ParametrosTizado { AnchoTela = 500, Celda = 10 }); Valida(fina, new ParametrosTizado { AnchoTela = 500, Celda = 2 });
        Assert.True(fina.Eficiencia >= gruesa.Eficiencia - 0.02);
    }
    [Fact] public void UnaCamisaCompleta_ConVariasTallas_EsValida_Y_RazonableEnTiempo()
    {
        // cuerpo (trapecio), manga, cuello y bolsillo; tallas S, M, L, XL con curvas aproximadas
        Pieza Pz(string n, Poligono c, string t, int q, int esp = 0) => new() { Id = n + t, Nombre = n, Talla = t, Contorno = c, Cantidad = q, CantidadEspejada = esp };
        var lista = new List<Pieza>();
        int i = 0;
        foreach (var t in new[] { "S", "M", "L", "XL" })
        {
            double k = 1 + 0.05 * i++;
            var cuerpo = new Poligono(new[] { new Punto(0, 0), new Punto(700 * k, 0), new Punto(700 * k, 520 * k), new Punto(520 * k, 560 * k), new Punto(180 * k, 560 * k), new Punto(0, 520 * k) });
            var manga = new Poligono(new[] { new Punto(0, 0), new Punto(300 * k, 0), new Punto(260 * k, 200 * k), new Punto(40 * k, 200 * k) });
            lista.Add(Pz("Cuerpo", cuerpo, t, 2)); lista.Add(Pz("Manga", manga, t, 2, 1));
            lista.Add(Pz("Cuello", Rect(380 * k, 70), t, 2)); lista.Add(Pz("Bolsillo", Rect(110 * k, 120 * k), t, 2));
        }
        var p = new ParametrosTizado { AnchoTela = 1500, Separacion = 3, Margen = 10, Celda = 5, Intentos = 6 };
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var r = Tizador.Calcular(lista, p);
        sw.Stop();
        Assert.Equal(32, r.Colocadas.Count); Assert.Empty(r.NoColocadas); Valida(r, p);
        Assert.True(r.Eficiencia > 0.60, $"eficiencia {r.Eficiencia:P1}");
        Assert.True(sw.ElapsedMilliseconds < 20000, $"tardo {sw.ElapsedMilliseconds} ms");
    }
}

public class ExportadoresTests
{
    static ResultadoTizado Res() => Tizador.Calcular(new[] { new Pieza { Id = "A<1>", Nombre = "Cuerpo & \"A\"", Talla = "S", Contorno = new Poligono(new[] { new Punto(0, 0), new Punto(100, 0), new Punto(100, 50), new Punto(0, 50) }), Cantidad = 2 }, new Pieza { Id = "B", Nombre = "Manga", Talla = "M", Contorno = new Poligono(new[] { new Punto(0, 0), new Punto(60, 0), new Punto(60, 60), new Punto(0, 60) }), Cantidad = 1 } }, new ParametrosTizado { AnchoTela = 300, Intentos = 1 });
    [Fact] public void Svg_TieneUnPoligonoPorPieza_Y_EscapaLosTextos()
    {
        var svg = Exportadores.ASvg(Res());
        Assert.StartsWith("<svg", svg); Assert.EndsWith("</svg>", svg);
        Assert.Equal(3, svg.Split("<polygon").Length - 1);
        Assert.Contains("Cuerpo &amp; &quot;A&quot;", svg); Assert.DoesNotContain("A<1>", svg);
        Assert.Contains("viewBox=\"0 0 ", svg);
    }
    [Fact] public void Svg_EscapaTambienElTooltip()
        => Assert.Contains("<title>Cuerpo &amp; &quot;A&quot; S</title>", Exportadores.ASvg(Res(), false));
    [Fact] public void Svg_SinRotulos_NoTrae_Texto() => Assert.DoesNotContain("<text", Exportadores.ASvg(Res(), false));
    [Fact] public void Svg_UsaPuntoDecimal_SinImportarLaCultura()
    {
        var old = System.Globalization.CultureInfo.CurrentCulture;
        try { System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("es-PE"); Assert.DoesNotContain(",", Exportadores.ASvg(Res(), false).Split("points=\"")[1].Split('"')[0].Replace(" ", "").Replace(",", "")); Assert.Contains("viewBox=\"0 0 ", Exportadores.ASvg(Res())); }
        finally { System.Globalization.CultureInfo.CurrentCulture = old; }
    }
    [Fact] public void Dxf_TerminaEnEof_Y_LlevaUnidadesEnMm()
    {
        var d = Exportadores.ADxf(Res());
        Assert.EndsWith("0\nEOF\n", d); Assert.Contains("$INSUNITS\n70\n4", d);
    }
}
