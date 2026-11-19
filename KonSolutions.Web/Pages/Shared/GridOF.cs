namespace KONSolutions.Web.Pages.Shared;

/// <summary>
/// Estado de una grilla con orden y filtro por columna (el mismo comportamiento que las demas grillas del proyecto:
/// cabecera con EncabezadoOrdenable + FiltroExcelColumna). Cada columna se registra una vez con la funcion que da el valor
/// que se muestra/filtra y, opcionalmente, la clave por la que se ordena (numeros y fechas se ordenan por su valor, no por su texto).
/// Los filtros son en cascada: los valores que ofrece una columna salen de lo que dejan pasar las demas.
/// </summary>
public sealed class GridOF<T>
{
    private readonly List<(Func<T, string?> Valor, Func<T, IComparable?> Orden)> _cols = new();
    private readonly Dictionary<int, HashSet<string>?> _filtros = new();

    public int? OrdenColumna { get; private set; }
    public bool Ascendente { get; private set; } = true;
    public bool HayFiltros => _filtros.Values.Any(v => v is not null);

    /// <summary>Registra una columna y devuelve su indice.</summary>
    public int Add(Func<T, string?> valor, Func<T, IComparable?>? orden = null)
    {
        _cols.Add((x => valor(x) ?? "", orden ?? (x => valor(x) ?? "")));
        return _cols.Count - 1;
    }

    public Func<T, string?> Valor(int col) => _cols[col].Valor;
    public HashSet<string>? Filtro(int col) => _filtros.GetValueOrDefault(col);
    public void SetFiltro(int col, HashSet<string>? valores) => _filtros[col] = valores;
    public void Limpiar() => _filtros.Clear();

    public void ToggleOrden(int col)
    {
        if (OrdenColumna == col) Ascendente = !Ascendente;
        else { OrdenColumna = col; Ascendente = true; }
    }

    /// <summary>Los items que dejan pasar los filtros de TODAS las columnas menos <paramref name="excepto"/> (-1 = ninguna excluida).</summary>
    public List<T> Base(IReadOnlyList<T> items, int excepto)
    {
        IEnumerable<T> q = items;
        var hay = false;
        foreach (var (c, v) in _filtros)
        {
            if (c == excepto || v is null) continue;
            hay = true;
            var valor = _cols[c].Valor;
            q = q.Where(x => v.Contains(valor(x) ?? ""));
        }
        return hay ? q.ToList() : items.ToList();
    }

    /// <summary>Filtrados por todas las columnas y ordenados.</summary>
    public List<T> Visibles(IReadOnlyList<T> items)
    {
        var l = Base(items, -1);
        if (OrdenColumna is not { } c || c >= _cols.Count) return l;
        var k = _cols[c].Orden;
        return (Ascendente ? l.OrderBy(k, Comparador) : l.OrderByDescending(k, Comparador)).ToList();
    }

    private static readonly Comparer<IComparable?> Comparador = Comparer<IComparable?>.Create((a, b) =>
        a is null ? (b is null ? 0 : -1) : b is null ? 1 : a.CompareTo(b));
}
