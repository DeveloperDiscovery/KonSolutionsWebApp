using KONSolutions.Web.Pages.Shared;
using Xunit;



public class OperacionesGridOFTests
{
    sealed record Fila(string Cod, string Tipo, int Cant);

    static GridOF<Fila> G(out int cod, out int tipo, out int cant)
    {
        var g = new GridOF<Fila>();
        cod = g.Add(f => f.Cod); tipo = g.Add(f => f.Tipo); cant = g.Add(f => f.Cant.ToString(), f => f.Cant);
        return g;
    }

    static readonly Fila[] Datos = { new("A", "X", 10), new("B", "Y", 9), new("C", "X", 100), new("D", "Y", 2) };

    [Fact]
    public void OrdenaNumerosPorSuValorYNoPorSuTexto_AscendenteYDescendente()
    {
        var g = G(out _, out _, out var cant);
        g.ToggleOrden(cant);
        Assert.Equal(new[] { 2, 9, 10, 100 }, g.Visibles(Datos).Select(f => f.Cant).ToArray());        // como texto sería 10, 100, 2, 9
        g.ToggleOrden(cant);
        Assert.Equal(new[] { 100, 10, 9, 2 }, g.Visibles(Datos).Select(f => f.Cant).ToArray());
        Assert.False(g.Ascendente); Assert.Equal(cant, g.OrdenColumna);
    }

    [Fact]
    public void CambiarDeColumnaOrdenaAscendenteDeNuevo()
    {
        var g = G(out var cod, out _, out var cant);
        g.ToggleOrden(cant); g.ToggleOrden(cant);       // descendente por cantidad
        g.ToggleOrden(cod);
        Assert.True(g.Ascendente);
        Assert.Equal(new[] { "A", "B", "C", "D" }, g.Visibles(Datos).Select(f => f.Cod).ToArray());
    }

    [Fact]
    public void SinOrdenNiFiltroDevuelveLaListaTalCual()
    {
        var g = G(out _, out _, out _);
        Assert.Equal(Datos, g.Visibles(Datos));
        Assert.False(g.HayFiltros);
    }

    [Fact]
    public void FiltraPorLosValoresElegidos_YLimpiarLosQuita()
    {
        var g = G(out _, out var tipo, out _);
        g.SetFiltro(tipo, new HashSet<string> { "X" });
        Assert.True(g.HayFiltros);
        Assert.Equal(new[] { "A", "C" }, g.Visibles(Datos).Select(f => f.Cod).ToArray());
        g.Limpiar();
        Assert.False(g.HayFiltros);
        Assert.Equal(4, g.Visibles(Datos).Count);
    }

    [Fact]
    public void LosFiltrosSonEnCascada_BaseNoCuentaElFiltroDeLaPropiaColumna()
    {
        var g = G(out var cod, out var tipo, out _);
        g.SetFiltro(tipo, new HashSet<string> { "X" });
        g.SetFiltro(cod, new HashSet<string> { "A", "B" });
        Assert.Equal(new[] { "A" }, g.Visibles(Datos).Select(f => f.Cod).ToArray());
        // los valores que ofrece la columna Tipo salen de lo que dejan pasar las demas (Cod A o B): X e Y, aunque Tipo ya filtra por X
        Assert.Equal(new[] { "X", "Y" }, g.Base(Datos, tipo).Select(f => f.Tipo).Distinct().OrderBy(x => x).ToArray());
        // y los de Cod, los que dejan pasar las demas (Tipo X): A y C
        Assert.Equal(new[] { "A", "C" }, g.Base(Datos, cod).Select(f => f.Cod).ToArray());
    }

    [Fact]
    public void LosVaciosNoRompenElFiltroNiElOrden()
    {
        var g = new GridOF<(string? Texto, int? Num)>();
        var t = g.Add(f => f.Texto); var n = g.Add(f => f.Num?.ToString(), f => f.Num);
        var d = new (string?, int?)[] { ("b", 3), (null, null), ("a", 1) };
        g.ToggleOrden(n);
        Assert.Equal(new int?[] { null, 1, 3 }, g.Visibles(d).Select(f => f.Num).ToArray());        // los vacios primero
        g.SetFiltro(t, new HashSet<string> { "" });
        Assert.Single(g.Visibles(d));
    }

    // La grilla de Operaciones lleva la columna Proceso (antes de Subproceso) y cada cabecera usa el indice de su columna en el GridOF, sin saltos ni repetidos.
    [Fact]
    public void Operaciones_TieneLaColumnaProceso_YLosIndicesDeCabeceraVanSeguidos()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "KonSolutions.Web", "Pages", "Ingenieria", "Operaciones.razor"))) dir = dir.Parent;
        Assert.NotNull(dir);
        var src = File.ReadAllText(Path.Combine(dir!.FullName, "KonSolutions.Web", "Pages", "Ingenieria", "Operaciones.razor"));
        var cab = System.Text.RegularExpressions.Regex.Matches(src, "Col=\"(\\d+)\" Texto=\"([^\"]+)\"").Select(m => (Col: int.Parse(m.Groups[1].Value), Texto: m.Groups[2].Value)).ToList();
        Assert.Equal(Enumerable.Range(0, cab.Count), cab.Select(c => c.Col));
        var nombres = cab.Select(c => c.Texto).ToList();
        Assert.Equal(nombres.IndexOf("Subproceso") - 1, nombres.IndexOf("Proceso"));
        Assert.Equal(cab.Count, System.Text.RegularExpressions.Regex.Matches(src, "_g\\.Add\\(").Count);   // una columna del GridOF por cada cabecera
        Assert.Contains("@context.Item.DES_PROCESO", src);
    }
}
