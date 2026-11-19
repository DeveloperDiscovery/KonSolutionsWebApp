namespace KONSolutions.Web.Services;

/// <summary>Lo que los paneles de Requerimientos y Órdenes de Compra del movimiento de
/// almacén tienen en pantalla: las cuatro listas y lo que el usuario marcó y escribió.
///
/// POR QUÉ EXISTE
/// --------------
/// Los paneles guardaban todo eso en campos propios. Mientras el componente viva, bien; pero
/// si Blazor lo destruye y lo vuelve a crear —y eso pasa al cambiar de pestaña, porque no
/// todas las MudTabs de la cadena conservan sus paneles— el componente nuevo arranca vacío,
/// vuelve a consultar, y se pierde TODO: las órdenes marcadas, las líneas marcadas, las
/// cantidades recibidas escritas a mano, el lote y la ubicación elegidos.
///
/// Guardándolo acá afuera, el componente puede morir y renacer las veces que haga falta: al
/// volver, se encuentra su estado y ni siquiera vuelve a consultar.
///
/// Se arregla así, y no poniéndole KeepPanelsAlive a cada MudTabs de la cadena, porque esto
/// funciona SIN DEPENDER de por qué se recreó el componente. Con el otro camino, cualquier
/// pestaña nueva que alguien agregue mañana sin esa propiedad vuelve a romperlo en silencio.
///
/// Es un objeto POR SESIÓN (scoped) y vive en memoria del navegador: no se persiste, y al
/// recargar la página se empieza de cero. Es lo correcto — son datos a medio editar, no un
/// documento guardado.
/// </summary>
public class EstadoPanelesMovimiento
{
    private readonly Dictionary<string, object> _porPanel = new();

    /// <summary>Estado de UN panel para UN movimiento. La clave junta las dos cosas: dos
    /// movimientos abiertos en pestañas distintas no pueden pisarse el estado.</summary>
    private static string Clave(string panel, decimal? numMovimiento)
        => $"{panel}|{numMovimiento?.ToString() ?? "-"}";

    /// <summary>Devuelve el estado guardado, o null si ese panel nunca cargó ese movimiento.
    /// El null es la señal de "hay que consultar"; no se devuelve un estado vacío, que sería
    /// indistinguible de un movimiento sin líneas.</summary>
    public Estado<TCab, TLin>? Leer<TCab, TLin>(string panel, decimal? numMovimiento)
        where TCab : class where TLin : class
        => _porPanel.TryGetValue(Clave(panel, numMovimiento), out var e)
               ? e as Estado<TCab, TLin>
               : null;

    public void Guardar<TCab, TLin>(string panel, decimal? numMovimiento, Estado<TCab, TLin> estado)
        where TCab : class where TLin : class
        => _porPanel[Clave(panel, numMovimiento)] = estado;

    /// <summary>Se llama al refrescar a mano y al grabar: ahí sí hay que volver a la base.</summary>
    public void Olvidar(string panel, decimal? numMovimiento)
        => _porPanel.Remove(Clave(panel, numMovimiento));

    /// <summary>Al cerrar el documento. Sin esto, abrir otro movimiento con el mismo número
    /// más adelante en la sesión mostraría lo de la vez anterior.</summary>
    public void OlvidarTodo(decimal? numMovimiento)
    {
        var sufijo = $"|{numMovimiento?.ToString() ?? "-"}";
        foreach (var k in _porPanel.Keys.Where(k => k.EndsWith(sufijo)).ToList())
            _porPanel.Remove(k);
    }

    /// <summary>Las cuatro listas más lo marcado. Las listas guardan las MISMAS instancias
    /// que la grilla está mostrando, no copias: por eso lo que el usuario escribió en una
    /// celda —cantidad recibida, lote, ubicación— vuelve tal cual, sin tener que guardarlo
    /// campo por campo.</summary>
    public class Estado<TCab, TLin> where TCab : class where TLin : class
    {
        public List<TCab> Disponibles { get; set; } = new();
        public List<TCab> Asignados { get; set; } = new();
        public List<TLin> DetalleDisponibles { get; set; } = new();
        public List<TLin> DetalleAsignados { get; set; } = new();

        public HashSet<TCab> SelDisponibles { get; set; } = new();
        public HashSet<TLin> SelDetalleDisponibles { get; set; } = new();
        public HashSet<TLin> SelDetalleAsignados { get; set; } = new();

        public bool HayCambiosSinGrabar { get; set; }
        public string MotivoDetalleVacio { get; set; } = "";
    }
}
