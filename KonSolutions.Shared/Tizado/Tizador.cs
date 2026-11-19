using System.Diagnostics;

namespace KONSolutions.Shared.Tizado;

/// <summary>Pieza del molde a cortar. El contorno debe venir con el hilo en el sentido X (largo de la tela); si no, usa Poligono.AlinearHilo.</summary>
public sealed class Pieza
{
    public string Id { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string Talla { get; set; } = "";
    public Poligono Contorno { get; set; } = new(Array.Empty<Punto>());
    /// <summary>Cantidad total a cortar de esta pieza (incluye las espejadas).</summary>
    public int Cantidad { get; set; } = 1;
    /// <summary>De la cantidad total, cuantas se cortan espejadas (izquierda/derecha). 0 = ninguna.</summary>
    public int CantidadEspejada { get; set; }
    /// <summary>Giros permitidos en grados. [0] = no se puede girar (hilo estricto); [0,180] = hilo permitido en ambos sentidos.</summary>
    public double[] Rotaciones { get; set; } = { 0, 180 };
    /// <summary>Punto de coincidencia de rayas, en las mismas coordenadas del contorno (por ejemplo la linea del pecho o una esquina del bolsillo). Gira y se espeja con la pieza.</summary>
    public Punto? PuntoCoincidencia { get; set; }
    /// <summary>Esa pieza debe coincidir con la raya en el sentido del largo (X): el punto cae a FaseX mm de una raya, con el periodo PeriodoX del tizado.</summary>
    public bool CoincideX { get; set; }
    /// <summary>Lo mismo en el sentido del ancho (Y), para cuadros: periodo PeriodoY.</summary>
    public bool CoincideY { get; set; }
    /// <summary>Desfase deseado del punto respecto de la raya, mm (0 = justo sobre la raya; dos piezas con la misma fase quedan sobre la misma raya).</summary>
    public double FaseX { get; set; }
    public double FaseY { get; set; }
}

/// <summary>Zona de la tela donde no se puede cortar (defecto del rollo). Coordenadas de la tela: X = largo, Y = ancho, mm.</summary>
public sealed class Defecto
{
    public string Nombre { get; set; } = "";
    public Poligono Contorno { get; set; } = new(Array.Empty<Punto>());
}

public sealed class ParametrosTizado
{
    /// <summary>Ancho util de la tela en mm.</summary>
    public double AnchoTela { get; set; } = 1500;
    /// <summary>Separacion minima entre piezas, mm.</summary>
    public double Separacion { get; set; } = 3;
    /// <summary>Margen al borde de la tela y al inicio del tendido, mm.</summary>
    public double Margen { get; set; } = 5;
    /// <summary>Lado de la celda de la cuadricula de trabajo, mm. Mas chica = mas precisa y mas lenta.</summary>
    public double Celda { get; set; } = 4;
    /// <summary>Cuantos ordenes distintos de colocacion se prueban (se queda el de menor largo).</summary>
    public int Intentos { get; set; } = 8;
    public int Semilla { get; set; } = 1;
    /// <summary>Tiempo maximo total en ms (0 = sin limite). Siempre se termina el primer intento.</summary>
    public int TiempoMaxMs { get; set; }
    /// <summary>Defectos de tela: ninguna pieza los pisa ni queda a menos de la separacion.</summary>
    public List<Defecto> Defectos { get; set; } = new();
    /// <summary>Periodo de la raya en el sentido del largo (mm entre rayas). 0 = sin coincidencia en X.</summary>
    public double PeriodoX { get; set; }
    /// <summary>Periodo de la raya en el sentido del ancho (cuadros). 0 = sin coincidencia en Y.</summary>
    public double PeriodoY { get; set; }
}

public sealed class Colocacion
{
    public string PiezaId { get; init; } = "";
    public string Nombre { get; init; } = "";
    public string Talla { get; init; } = "";
    public int Copia { get; init; }
    public bool Espejada { get; init; }
    public double Angulo { get; init; }
    /// <summary>Contorno ya en su posicion final sobre la tela (mm).</summary>
    public Poligono Contorno { get; init; } = new(Array.Empty<Punto>());
    /// <summary>Punto de coincidencia ya sobre la tela (solo si la pieza lo tiene).</summary>
    public Punto? PuntoCoincidencia { get; init; }
    /// <summary>Cuanto se aparta el punto de su raya, mm (solo si la pieza coincide en ese sentido). Debe ser casi 0.</summary>
    public double? ErrorCoincidenciaX { get; init; }
    public double? ErrorCoincidenciaY { get; init; }
    /// <summary>Fase pedida por la pieza (solo si coincide en ese sentido): sirve para volver a medir el desvio tras un ajuste manual.</summary>
    public double? FaseX { get; init; }
    public double? FaseY { get; init; }
}

public sealed class NoColocada { public string PiezaId { get; init; } = ""; public string Nombre { get; init; } = ""; public string Talla { get; init; } = ""; public string Motivo { get; init; } = ""; }

public sealed class ResultadoTizado
{
    public List<Colocacion> Colocadas { get; init; } = new();
    public List<NoColocada> NoColocadas { get; init; } = new();
    public double Ancho { get; init; }
    /// <summary>Largo de tela usado, mm (hasta el borde derecho de la ultima pieza mas el margen).</summary>
    public double Largo { get; init; }
    public double AreaPiezas { get; init; }
    public double Eficiencia => Largo > 0 && Ancho > 0 ? AreaPiezas / (Largo * Ancho) : 0;
    public int IntentoGanador { get; init; }
    public int IntentosProbados { get; init; }
}

/// <summary>
/// Tizador 2D por cuadricula: cada pieza se rasteriza (de forma conservadora, nunca queda area sin marcar) en cada giro permitido y se
/// coloca en la posicion mas a la izquierda y mas abajo donde no pisa lo ya colocado ni su separacion. Prueba varios ordenes y se queda con el
/// de menor largo. Es una heuristica: no garantiza el optimo.
/// </summary>
public static class Tizador
{
    public static ResultadoTizado Calcular(IEnumerable<Pieza> piezas, ParametrosTizado p, CancellationToken ct = default, Action<int, int>? progreso = null)
    {
        if (p.AnchoTela <= 0) throw new ArgumentException("El ancho de tela debe ser mayor que cero.");
        if (p.Celda <= 0) throw new ArgumentException("La celda debe ser mayor que cero.");
        if (p.Separacion < 0 || p.Margen < 0) throw new ArgumentException("Separacion y margen no pueden ser negativos.");
        if (p.PeriodoX < 0 || p.PeriodoY < 0) throw new ArgumentException("El periodo de la raya no puede ser negativo.");

        double cell = p.Celda;
        int S = (int)Math.Ceiling(p.Separacion / cell);
        int M = (int)Math.Ceiling(p.Margen / cell);
        int Hreal = (int)Math.Floor(p.AnchoTela / cell);

        // instancias
        var inst = new List<Instancia>();
        var cache = new Dictionary<(string, bool, double, bool, bool), Variante>();
        var defectos = PrepararDefectos(p, cell, S);
        int yMinG = S + M;
        var rechazadas = new List<NoColocada>();
        foreach (var pz in piezas)
        {
            if (pz.Cantidad < 0 || pz.CantidadEspejada < 0 || pz.CantidadEspejada > pz.Cantidad) throw new ArgumentException($"Cantidades invalidas en la pieza {pz.Nombre}.");
            if (!pz.Contorno.EsValido) { for (int i = 0; i < pz.Cantidad; i++) rechazadas.Add(Rech(pz, "Contorno invalido.")); continue; }
            bool ex = pz.CoincideX, ey = pz.CoincideY;
            if ((ex || ey) && pz.PuntoCoincidencia is null) throw new ArgumentException($"La pieza {pz.Nombre} pide coincidencia de rayas pero no tiene punto de coincidencia.");
            if (ex && p.PeriodoX <= 0) throw new ArgumentException($"La pieza {pz.Nombre} pide coincidencia en el largo y el tizado no tiene periodo de raya en X.");
            if (ey && p.PeriodoY <= 0) throw new ArgumentException($"La pieza {pz.Nombre} pide coincidencia en el ancho y el tizado no tiene periodo de raya en Y.");
            var rots = (pz.Rotaciones is { Length: > 0 } ? pz.Rotaciones : new[] { 0d }).Select(r => ((r % 360) + 360) % 360).Distinct().ToArray();
            for (int i = 0; i < pz.Cantidad; i++)
            {
                bool esp = i >= pz.Cantidad - pz.CantidadEspejada;
                var vars = rots.Select(r => Obtener(cache, pz, esp, r, cell, S, ex, ey)).Where(v => v.Rows <= Hreal - 2 * M && v.Cols > 0).ToList();
                if (vars.Count == 0) { rechazadas.Add(Rech(pz, $"No cabe en el ancho de tela ({p.AnchoTela:0} mm).")); continue; }
                if (ey)   // en el ancho solo hay un tramo de tela: debe existir al menos una posicion que coincida con la raya
                {
                    vars = vars.Where(v => { int yMax = S + Hreal - M - v.Rows; for (int y = yMinG; y <= yMax; y++) if (Posicion(y, S, cell, v.Pc!.Value.Y, pz.FaseY, p.PeriodoY, out _)) return true; return false; }).ToList();
                    if (vars.Count == 0) { rechazadas.Add(Rech(pz, "No hay una posicion que coincida con la raya dentro del ancho de tela.")); continue; }
                }
                inst.Add(new Instancia(pz, i + 1, esp, vars, pz.Contorno.Area));
            }
        }

        var ordenes = new List<Func<List<Instancia>>>
        {
            () => inst.OrderByDescending(x => x.Area).ToList(),
            () => inst.OrderByDescending(x => x.Variantes.Max(v => v.Rows)).ThenByDescending(x => x.Area).ToList(),
            () => inst.OrderByDescending(x => x.Pieza.Contorno.Perimetro).ToList(),
            () => inst.OrderByDescending(x => x.Variantes.Max(v => v.Cols)).ThenByDescending(x => x.Area).ToList(),
        };
        int total = Math.Max(1, p.Intentos);
        var reloj = Stopwatch.StartNew();
        ResultadoTizado? mejor = null; int probados = 0, ganador = 0;
        for (int k = 0; k < total; k++)
        {
            ct.ThrowIfCancellationRequested();
            if (k > 0 && p.TiempoMaxMs > 0 && reloj.ElapsedMilliseconds > p.TiempoMaxMs) break;
            List<Instancia> orden;
            if (k < ordenes.Count) orden = ordenes[k]();
            else { var rnd = new Random(p.Semilla * 7919 + k); orden = inst.Select(x => (x, clave: x.Area * (0.6 + 0.8 * rnd.NextDouble()))).OrderByDescending(t => t.clave).Select(t => t.x).ToList(); }
            var r = Colocar(orden, p, cell, S, M, Hreal, rechazadas, k + 1, defectos);
            probados++;
            if (mejor == null || r.Largo < mejor.Largo - 1e-9) { mejor = r; ganador = k + 1; }
            progreso?.Invoke(k + 1, total);
        }
        mejor ??= Colocar(new List<Instancia>(), p, cell, S, M, Hreal, rechazadas, 1, defectos);
        return new ResultadoTizado { Colocadas = mejor.Colocadas, NoColocadas = mejor.NoColocadas, Ancho = mejor.Ancho, Largo = mejor.Largo, AreaPiezas = mejor.AreaPiezas, IntentoGanador = ganador, IntentosProbados = probados };
    }

    static NoColocada Rech(Pieza pz, string m) => new() { PiezaId = pz.Id, Nombre = pz.Nombre, Talla = pz.Talla, Motivo = m };

    sealed record Instancia(Pieza Pieza, int Copia, bool Espejada, List<Variante> Variantes, double Area);

    sealed class Variante
    {
        public double Angulo; public Poligono Poli = null!;          // en el origen, ya girada / espejada
        public int Cols, Rows;
        public ulong[][] Mask = null!;                                // para probar (sin holgura)
        public ulong[][] MaskDil = null!; public int ColsDil, RowsDil; // para marcar (con separacion)
        public Punto? Pc;                                              // punto de coincidencia en el origen de la variante
        public bool EX, EY;                                            // coincide con la raya en X / Y (la mascara va ensanchada 1 celda)
    }

    static Variante Obtener(Dictionary<(string, bool, double, bool, bool), Variante> cache, Pieza pz, bool esp, double ang, double cell, int S, bool ex, bool ey)
    {
        var key = (pz.Id + "|" + pz.Nombre + "|" + pz.Talla + "|" + pz.Contorno.GetHashCode() + "|" + pz.PuntoCoincidencia, esp, ang, ex, ey);
        if (cache.TryGetValue(key, out var v)) return v;
        var t = (esp ? pz.Contorno.Espejar() : pz.Contorno).Rotar(ang);
        var lim = t.Limites; var poli = t.Trasladar(-lim.MinX, -lim.MinY);
        Punto? pc = null;
        if (pz.PuntoCoincidencia is { } q)      // el punto sufre el mismo espejo, giro y traslado que el contorno
        {
            double x = esp ? -q.X : q.X, y = q.Y, r = ang * Math.PI / 180, c = Math.Cos(r), s = Math.Sin(r);
            pc = new Punto(x * c - y * s - lim.MinX, x * s + y * c - lim.MinY);
        }
        var b = Rasterizar(poli, cell, out int cols, out int rows);
        b = Ensanchar(b, ref cols, ref rows, ex, ey);
        var d = Dilatar(b, cols, rows, S, out int cd, out int rd);
        v = new Variante { Angulo = ang, Poli = poli, Cols = cols, Rows = rows, Mask = Empaquetar(b, cols, rows), ColsDil = cd, RowsDil = rd, MaskDil = Empaquetar(d, cd, rd), Pc = pc, EX = ex, EY = ey };
        cache[key] = v; return v;
    }

    /// <summary>Agrega una celda hacia +X (y/o +Y): la pieza se corre hasta una celda para caer justo sobre la raya y la mascara debe cubrir ese corrimiento.</summary>
    static bool[,] Ensanchar(bool[,] m, ref int cols, ref int rows, bool ex, bool ey)
    {
        if (ex) { var o = new bool[cols + 1, rows]; for (int c = 0; c < cols; c++) for (int r = 0; r < rows; r++) if (m[c, r]) { o[c, r] = true; o[c + 1, r] = true; } m = o; cols++; }
        if (ey) { var o = new bool[cols, rows + 1]; for (int c = 0; c < cols; c++) for (int r = 0; r < rows; r++) if (m[c, r]) { o[c, r] = true; o[c, r + 1] = true; } m = o; rows++; }
        return m;
    }

    /// <summary>
    /// La celda idx (origen en idx - S) contiene un corrimiento d en [0, celda) que deja el punto de coincidencia (a pc mm del origen de la pieza)
    /// a "fase" mm de una raya de periodo R. Devuelve false si ninguna raya cae en esa celda.
    /// </summary>
    static bool Posicion(int idx, int S, double cell, double pc, double fase, double R, out double d)
    {
        double g = (idx - S) * cell;
        double t0 = (((fase - pc) % R) + R) % R;
        double k = Math.Ceiling((g - t0) / R - 1e-9);
        d = Math.Max(0, t0 + k * R - g);
        return d < cell - 1e-9;
    }

    internal static double Resto(double v, double R) { double r = ((v % R) + R) % R; return Math.Min(r, R - r); }

    /// <summary>Cada defecto como mascara ya dilatada por la separacion, con su celda de origen en la cuadricula de ocupacion.</summary>
    static List<(int Ox, int Oy, ulong[][] Mask)> PrepararDefectos(ParametrosTizado p, double cell, int S)
    {
        var res = new List<(int, int, ulong[][])>();
        foreach (var d in p.Defectos ?? new())
        {
            if (d.Contorno is null || !d.Contorno.EsValido) throw new ArgumentException($"El defecto '{d.Nombre}' no tiene un contorno valido.");
            var l = d.Contorno.Limites;
            if (l.MinX < -1e-9 || l.MinY < -1e-9 || l.MaxY > p.AnchoTela + 1e-9) throw new ArgumentException($"El defecto '{d.Nombre}' queda fuera de la tela (largo desde 0, ancho entre 0 y {p.AnchoTela:0.##} mm).");
            int ox = (int)Math.Floor(l.MinX / cell), oy = (int)Math.Floor(l.MinY / cell);
            var b = Rasterizar(d.Contorno.Trasladar(-ox * cell, -oy * cell), cell, out int c, out int r);
            var dl = Dilatar(b, c, r, S, out int cd, out int rd);
            res.Add((ox, oy, Empaquetar(dl, cd, rd)));
        }
        return res;
    }

    // ---------- rasterizado conservador ----------
    static bool[,] Rasterizar(Poligono poli, double cell, out int cols, out int rows)
    {
        var l = poli.Limites;
        cols = Math.Max(1, (int)Math.Ceiling((l.MaxX - 1e-9) / cell)); rows = Math.Max(1, (int)Math.Ceiling((l.MaxY - 1e-9) / cell));
        var m = new bool[cols, rows];
        var v = poli.Puntos; int n = v.Length;
        // interior por centros de celda (par-impar)
        var xs = new List<double>();
        for (int r = 0; r < rows; r++)
        {
            double yc = (r + 0.5) * cell; xs.Clear();
            for (int i = 0, j = n - 1; i < n; j = i++)
                if ((v[i].Y > yc) != (v[j].Y > yc)) xs.Add((v[j].X - v[i].X) * (yc - v[i].Y) / (v[j].Y - v[i].Y) + v[i].X);
            xs.Sort();
            for (int q = 0; q + 1 < xs.Count; q += 2)
            {
                int c0 = Math.Max(0, (int)Math.Ceiling(xs[q] / cell - 0.5)), c1 = Math.Min(cols - 1, (int)Math.Floor(xs[q + 1] / cell - 0.5));
                for (int c = c0; c <= c1; c++) m[c, r] = true;
            }
        }
        // bordes: toda celda que el contorno atraviesa (con un epsilon para que tocar la raya de la celda no cuente)
        const double e = 1e-6;
        for (int i = 0; i < n; i++)
        {
            var a = v[i]; var b = v[(i + 1) % n];
            int c0 = Math.Max(0, (int)Math.Floor(Math.Min(a.X, b.X) / cell)), c1 = Math.Min(cols - 1, (int)Math.Floor(Math.Max(a.X, b.X) / cell));
            int r0 = Math.Max(0, (int)Math.Floor(Math.Min(a.Y, b.Y) / cell)), r1 = Math.Min(rows - 1, (int)Math.Floor(Math.Max(a.Y, b.Y) / cell));
            for (int c = c0; c <= c1; c++)
                for (int r = r0; r <= r1; r++)
                    if (!m[c, r] && SegmentoToca(a, b, c * cell + e, r * cell + e, (c + 1) * cell - e, (r + 1) * cell - e)) m[c, r] = true;
        }
        return m;
    }

    /// <summary>Liang-Barsky: el segmento a-b toca el rectangulo.</summary>
    static bool SegmentoToca(Punto a, Punto b, double x0, double y0, double x1, double y1)
    {
        double t0 = 0, t1 = 1, dx = b.X - a.X, dy = b.Y - a.Y;
        double[] p = { -dx, dx, -dy, dy }, q = { a.X - x0, x1 - a.X, a.Y - y0, y1 - a.Y };
        for (int i = 0; i < 4; i++)
        {
            if (Math.Abs(p[i]) < 1e-12) { if (q[i] < 0) return false; }
            else
            {
                double t = q[i] / p[i];
                if (p[i] < 0) { if (t > t1) return false; if (t > t0) t0 = t; }
                else { if (t < t0) return false; if (t < t1) t1 = t; }
            }
        }
        return true;
    }

    /// <summary>Dilata s celdas en todas las direcciones (cuadrado). El origen del resultado queda desplazado -s.</summary>
    static bool[,] Dilatar(bool[,] m, int cols, int rows, int s, out int cd, out int rd)
    {
        cd = cols + 2 * s; rd = rows + 2 * s;
        var o = new bool[cd, rd];
        if (s == 0) { for (int c = 0; c < cols; c++) for (int r = 0; r < rows; r++) o[c, r] = m[c, r]; return o; }
        var t = new bool[cd, rows];                                 // dilatacion en X
        for (int c = 0; c < cols; c++) for (int r = 0; r < rows; r++) if (m[c, r]) for (int d = -s; d <= s; d++) t[c + s + d, r] = true;
        for (int c = 0; c < cd; c++) for (int r = 0; r < rows; r++) if (t[c, r]) for (int d = -s; d <= s; d++) o[c, r + s + d] = true;
        return o;
    }

    static ulong[][] Empaquetar(bool[,] m, int cols, int rows)
    {
        int w = (rows + 63) / 64; var res = new ulong[cols][];
        for (int c = 0; c < cols; c++)
        {
            var a = new ulong[w];
            for (int r = 0; r < rows; r++) if (m[c, r]) a[r >> 6] |= 1UL << (r & 63);
            res[c] = a;
        }
        return res;
    }

    // ---------- colocacion ----------
    static ResultadoTizado Colocar(List<Instancia> orden, ParametrosTizado p, double cell, int S, int M, int Hreal, List<NoColocada> rechazadas, int intento, List<(int Ox, int Oy, ulong[][] Mask)> defectos)
    {
        int Hocc = Hreal + 2 * S; int words = (Hocc + 63) / 64 + 1;
        var occ = new List<ulong[]?>();
        foreach (var df in defectos) Marcar(occ, df.Mask, df.Ox, df.Oy, words);
        var colocadas = new List<Colocacion>(); double area = 0, maxX = 0;
        int xMin = S + M, yMin = S + M;
        int extent = xMin;                                          // borde derecho ocupado, en celdas

        foreach (var it in orden)
        {
            Variante? best = null; int bx = 0, by = 0, bext = int.MaxValue; double bdx = 0, bdy = 0;
            foreach (var v in it.Variantes)
            {
                int yMax = S + Hreal - M - v.Rows;
                if (yMax < yMin) continue;
                for (int x = xMin; x + v.Cols < bext; x++)
                {
                    double dx = 0;
                    if (v.EX && !Posicion(x, S, cell, v.Pc!.Value.X, it.Pieza.FaseX, p.PeriodoX, out dx)) continue;
                    int yf = -1; double dyf = 0;
                    for (int y = yMin; y <= yMax; y++)
                    {
                        double dy = 0;
                        if (v.EY && !Posicion(y, S, cell, v.Pc!.Value.Y, it.Pieza.FaseY, p.PeriodoY, out dy)) continue;
                        if (!Choca(occ, v.Mask, x, y)) { yf = y; dyf = dy; break; }
                    }
                    if (yf >= 0)
                    {
                        int ext = x + v.Cols;
                        if (ext < bext || (ext == bext && yf < by)) { best = v; bx = x; by = yf; bext = ext; bdx = dx; bdy = dyf; }
                        break;
                    }
                }
            }
            if (best == null) { rechazadas = new List<NoColocada>(rechazadas) { Rech(it.Pieza, "No se pudo colocar.") }; continue; }
            Marcar(occ, best.MaskDil, bx - S, by - S, words);
            extent = Math.Max(extent, bext);
            double tx = (bx - S) * cell + bdx, ty = (by - S) * cell + bdy;
            var poli = best.Poli.Trasladar(tx, ty);
            Punto? pcf = best.Pc is { } q ? new Punto(q.X + tx, q.Y + ty) : null;
            maxX = Math.Max(maxX, poli.Limites.MaxX); area += it.Area;
            colocadas.Add(new Colocacion
            {
                PiezaId = it.Pieza.Id, Nombre = it.Pieza.Nombre, Talla = it.Pieza.Talla, Copia = it.Copia, Espejada = it.Espejada, Angulo = best.Angulo, Contorno = poli,
                PuntoCoincidencia = pcf,
                ErrorCoincidenciaX = best.EX ? Resto(pcf!.Value.X - it.Pieza.FaseX, p.PeriodoX) : null,
                ErrorCoincidenciaY = best.EY ? Resto(pcf!.Value.Y - it.Pieza.FaseY, p.PeriodoY) : null,
                FaseX = best.EX ? it.Pieza.FaseX : null, FaseY = best.EY ? it.Pieza.FaseY : null
            });
        }
        return new ResultadoTizado { Colocadas = colocadas, NoColocadas = rechazadas.ToList(), Ancho = p.AnchoTela, Largo = colocadas.Count == 0 ? 0 : maxX + p.Margen, AreaPiezas = area, IntentoGanador = intento };
    }

    static bool Choca(List<ulong[]?> occ, ulong[][] mask, int x0, int y0)
    {
        int s = y0 & 63, w0 = y0 >> 6;
        for (int c = 0; c < mask.Length; c++)
        {
            int xc = x0 + c; if (xc >= occ.Count) return false;      // mas alla de lo ocupado: libre
            var oc = occ[xc]; if (oc == null) continue;
            var pw = mask[c];
            for (int k = 0; k < pw.Length; k++)
            {
                ulong v = pw[k]; if (v == 0) continue;
                if ((oc[w0 + k] & (v << s)) != 0) return true;
                if (s != 0 && (oc[w0 + k + 1] & (v >> (64 - s))) != 0) return true;
            }
        }
        return false;
    }

    static void Marcar(List<ulong[]?> occ, ulong[][] mask, int x0, int y0, int words)
    {
        int s = y0 & 63, w0 = y0 >> 6;
        while (occ.Count < x0 + mask.Length) occ.Add(null);
        for (int c = 0; c < mask.Length; c++)
        {
            var pw = mask[c]; var oc = occ[x0 + c] ??= new ulong[words];
            for (int k = 0; k < pw.Length; k++)
            {
                ulong v = pw[k]; if (v == 0) continue;
                oc[w0 + k] |= v << s;
                if (s != 0) oc[w0 + k + 1] |= v >> (64 - s);
            }
        }
    }
}
