using Blazored.LocalStorage;

namespace KONSolutions.Web.Services;

/// <summary>
/// Persiste en localStorage qué carpetas (CRP) del menú lateral están
/// expandidas, para que el estado se recuerde entre sesiones.
/// </summary>
public class MenuEstadoService
{
    private const string KEY = "menuCarpetasExpandidas";
    private readonly ILocalStorageService _localStorage;
    private HashSet<string> _expandidas = new();
    private bool _cargado;

    public MenuEstadoService(ILocalStorageService localStorage) => _localStorage = localStorage;

    private async Task AsegurarCargadoAsync()
    {
        if (_cargado) return;
        try
        {
            var lista = await _localStorage.GetItemAsync<List<string>>(KEY);
            _expandidas = lista is null ? new() : new HashSet<string>(lista);
        }
        catch { _expandidas = new(); }
        _cargado = true;
    }

    /// <summary>El juego completo de carpetas expandidas, de una sola vez.
    ///
    /// POR QUE HACE FALTA: recorrer el árbol del menú preguntando carpeta por carpeta con
    /// EstaExpandidaAsync son cientos de "await" seguidos. Aunque el dato ya esté en
    /// memoria, cada await cede el control al planificador de WebAssembly y vuelve por la
    /// cola de eventos del navegador. Cientos de idas y vueltas para consultar un HashSet
    /// que ya está cargado.
    ///
    /// Con esto se lee una vez y el recorrido se hace sin un solo await.</summary>
    public async Task<HashSet<string>> ObtenerExpandidasAsync()
    {
        await AsegurarCargadoAsync();
        return _expandidas;
    }

    public async Task<bool> EstaExpandidaAsync(string codOpcionAplicacion)
    {
        await AsegurarCargadoAsync();
        return _expandidas.Contains(codOpcionAplicacion);
    }

    public async Task ToggleAsync(string codOpcionAplicacion)
    {
        await AsegurarCargadoAsync();
        if (!_expandidas.Remove(codOpcionAplicacion))
            _expandidas.Add(codOpcionAplicacion);
        await _localStorage.SetItemAsync(KEY, _expandidas.ToList());
    }
}
