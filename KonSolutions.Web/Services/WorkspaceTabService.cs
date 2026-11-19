using Microsoft.AspNetCore.Components;

namespace KONSolutions.Web.Services;

/// <summary>Una pestaña abierta en el workspace.</summary>
public class WorkspaceTab
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Titulo { get; set; } = "";
    public string Icono { get; set; } = "";
    /// <summary>Ícono propio de la opción en la BD (base64), si existe. Tiene prioridad
    /// sobre <see cref="Icono"/> (el ícono fijo del menú tile/circular/lateral), que
    /// sirve de reserva cuando la opción no tiene un ícono propio configurado.</summary>
    public string? IconoImagenBase64 { get; set; }
    public Type ComponentType { get; set; } = default!;
    public Dictionary<string, object> Parametros { get; set; } = new();
    public string? Ruta { get; set; }
    /// <summary>Identidad de la pestaña para decidir si un clic abre una nueva o vuelve a
    /// una ya abierta. Antes ese papel lo cumplía <see cref="Ruta"/>, pero varias opciones
    /// de menú comparten la misma pantalla a propósito (Proformas, Órdenes de Compra y
    /// Pedidos de Venta son todas PedidosComerciales.razor filtrada por clase de operación,
    /// igual que Clientes y Proveedores son Entidades.razor): con la ruta como identidad,
    /// la segunda opción caía sobre la pestaña de la primera en vez de abrir la suya.
    /// Ahora la identidad es la OPCIÓN de menú, así que cada una tiene su pestaña y volver
    /// a hacer clic en la misma opción sigue reutilizándola.
    /// Cuando está vacía se usa la Ruta, que es el comportamiento de siempre para todo lo
    /// que se abre sin pasar por el menú (Post-its, documentos abiertos desde una grilla…).</summary>
    public string? Clave { get; set; }
    /// <summary>Identidad efectiva: la clave si la hay, si no la ruta.</summary>
    public string? ClaveEfectiva => string.IsNullOrEmpty(Clave) ? Ruta : Clave;
    public bool EsCerrable { get; set; } = true;
    public int Panel { get; set; } = 0;   // 0 = panel principal  1 = panel secundario
    /// <summary>True mientras la pestaña se abrió con activar:false (precarga en segundo
    /// plano, ej. durante el giro del menú circular): su componente ya está montado y
    /// cargando, pero todavía no debe aparecer en la barra de pestañas ni como activa.</summary>
    public bool Precargando { get; set; }

    /// <summary>Identidad SIN el sufijo de multi-load: la opción de menú de la que salió esta
    /// pestaña. Sirve para contar cuántas copias de la misma opción hay abiertas y numerar el
    /// título. Vacía en las pestañas que no son multi-load.</summary>
    public string? ClaveBase { get; set; }
}

/// <summary>
/// Gestiona las pestañas internas del workspace con soporte de panel dividido.
/// Panel 0 = siempre visible. Panel 1 = aparece al mover la primera pestaña ahí.
/// </summary>
public class WorkspaceTabService
{
    public List<WorkspaceTab> Tabs { get; } = new();

    /// <summary>Contador para dar identidad única a cada apertura de una opción con
    /// FLG_MULTI_LOAD. No se reinicia al cerrar pestañas: reciclando los números, una
    /// pestaña nueva podría caer sobre la identidad de otra que sigue abierta.</summary>
    private int _secuenciaMultiLoad;

    // Tab activa por panel
    public string? ActiveTabId  { get; private set; }   // panel 0
    public string? ActiveTabIdB { get; private set; }   // panel 1

    // Dirección del split: "h" = horizontal (arriba/abajo), "v" = vertical (izq/der)
    public string SplitDireccion { get; private set; } = "h";

    public bool TieneSplit => Tabs.Any(t => t.Panel == 1);

    public event Action? OnChange;

    // ── helpers ──────────────────────────────────────────────────────────────

    public IEnumerable<WorkspaceTab> TabsDePanel(int panel) => Tabs.Where(t => t.Panel == panel);

    public string? GetActiveTabId(int panel) => panel == 0 ? ActiveTabId : ActiveTabIdB;

    private void SetActiveTabId(int panel, string? id)
    {
        if (panel == 0) ActiveTabId  = id;
        else            ActiveTabIdB = id;
    }

    // ── abrir / activar ──────────────────────────────────────────────────────

    /// <summary>
    /// Abre (o activa, si ya existe por Ruta) una pestaña.
    /// <paramref name="activar"/>=false permite crear la pestaña y dejar que su componente
    /// empiece a cargar en segundo plano (WorkspaceTabHost monta todas las pestañas abiertas,
    /// ocultas con display:none) SIN cambiar la pestaña visible todavía — útil para
    /// precargar el destino mientras se reproduce una animación (ej. giro del menú circular)
    /// y activarlo recién al terminar, vía <see cref="ActivarPorRuta"/>.
    /// </summary>
    /// <returns>La identidad efectiva de la pestaña abierta (o de la que se reutilizó).
    /// Con FLG_MULTI_LOAD la identidad NO es la que se pasó —lleva un sufijo por apertura—,
    /// así que quien precargue con activar:false debe guardar ESTE valor para pasárselo
    /// después a <see cref="ActivarPorRuta"/>, en vez de volver a armar la clave.</returns>
    public string? Abrir(string titulo, string icono, Type componentType, string? ruta = null,
                      Dictionary<string, object>? parametros = null, bool esCerrable = true,
                      bool alInicio = false, string? iconoImagenBase64 = null, bool activar = true,
                      string? clave = null, bool multiLoad = false)
    {
        var identidad = string.IsNullOrEmpty(clave) ? ruta : clave;

        // FLG_MULTI_LOAD (SEGURIDAD.APLICACIONES_OPCIONES): la opción puede estar abierta en
        // varias pestañas a la vez, así que NO se busca una existente para reutilizar.
        // La identidad igual tiene que ser ÚNICA por pestaña —no vacía—, porque de ella
        // dependen activar una precarga y cerrar la correcta; se le agrega un sufijo propio
        // de esta apertura. Con la misma identidad repetida, la segunda apertura activaría
        // la primera pestaña, que es exactamente lo que se quiere evitar.
        string? claveBase = null;
        if (multiLoad && !string.IsNullOrEmpty(identidad))
        {
            claveBase = identidad;

            // Numeración visible: la PRIMERA copia se llama como siempre y la segunda en
            // adelante llevan "(2)", "(3)". Sin esto quedan dos pestañas con el mismo
            // rótulo y no hay forma de saber cuál es cuál.
            //
            // El número sale de cuántas copias hay ABIERTAS en este momento, no del
            // contador de identidades: si se cierra la (2) y se abre otra, vuelve a ser
            // la (2), que es lo que uno espera al mirar la barra. El contador, en cambio,
            // nunca se recicla, porque ahí lo que importa es no repetir una identidad viva.
            var copias = Tabs.Count(t => t.ClaveBase == claveBase);
            if (copias > 0) titulo = $"{titulo} ({copias + 1})";

            identidad = $"{identidad}~{++_secuenciaMultiLoad}";
            clave = identidad;
        }
        else if (!string.IsNullOrEmpty(identidad))
        {
            var existente = Tabs.FirstOrDefault(t => t.ClaveEfectiva == identidad);
            if (existente is not null)
            {
                if (activar) SetActiveTabId(existente.Panel, existente.Id);
                NotifyChanged();
                return existente.ClaveEfectiva;
            }
        }

        var tab = new WorkspaceTab
        {
            Titulo = titulo, Icono = icono, IconoImagenBase64 = iconoImagenBase64, ComponentType = componentType,
            Ruta = ruta, Clave = clave, ClaveBase = claveBase, Parametros = parametros ?? new(), EsCerrable = esCerrable,
            Panel = 0, Precargando = !activar
        };
        if (alInicio) Tabs.Insert(0, tab);
        else          Tabs.Add(tab);
        if (activar) ActiveTabId = tab.Id;
        NotifyChanged();
        return tab.ClaveEfectiva;
    }

    /// <summary>Activa (hace visible) una pestaña ya abierta, identificada por su Ruta.
    /// Usado tras precargar una pestaña con Abrir(..., activar:false).</summary>
    /// <remarks>Busca por identidad (ver <see cref="WorkspaceTab.ClaveEfectiva"/>), no por
    /// ruta: quien precarga una opción y después la activa debe pasar la misma identidad con
    /// la que la abrió, o activaría la pestaña de otra opción que comparte la pantalla.</remarks>
    public void ActivarPorRuta(string? ruta)
    {
        if (string.IsNullOrEmpty(ruta)) return;
        var tab = Tabs.FirstOrDefault(t => t.ClaveEfectiva == ruta);
        if (tab is null) return;
        tab.Precargando = false;
        SetActiveTabId(tab.Panel, tab.Id);
        NotifyChanged();
    }

    public void AbrirWorkspace(string titulo, string icono, Type componentType, string ruta)
    {
        if (Tabs.Any(t => t.Ruta == ruta))
        {
            var ws = Tabs.First(t => t.Ruta == ruta);
            SetActiveTabId(ws.Panel, ws.Id);
            NotifyChanged();
            return;
        }
        var tab = new WorkspaceTab
        {
            Titulo = titulo, Icono = icono, ComponentType = componentType,
            Ruta = ruta, EsCerrable = false, Panel = 0
        };
        Tabs.Insert(0, tab);
        ActiveTabId = tab.Id;
        NotifyChanged();
    }

    public void Activar(string tabId)
    {
        var tab = Tabs.FirstOrDefault(t => t.Id == tabId);
        if (tab is null) return;
        SetActiveTabId(tab.Panel, tabId);
        NotifyChanged();
    }

    // ── cerrar ───────────────────────────────────────────────────────────────

    public void Cerrar(string tabId)
    {
        var idx = Tabs.FindIndex(t => t.Id == tabId);
        if (idx < 0) return;
        var tab = Tabs[idx];
        if (!tab.EsCerrable) return;

        var panel = tab.Panel;
        Tabs.RemoveAt(idx);

        var activo = GetActiveTabId(panel);
        if (activo == tabId)
        {
            var vecinos = Tabs.Where(t => t.Panel == panel).ToList();
            SetActiveTabId(panel, vecinos.Count == 0 ? null : vecinos[Math.Min(idx, vecinos.Count) - 1 < 0 ? 0 : Math.Min(idx - 1, vecinos.Count - 1)].Id);
        }
        NotifyChanged();
    }

    public void CerrarTodas()
    {
        Tabs.Clear();
        ActiveTabId = ActiveTabIdB = null;
        NotifyChanged();
    }

    public void CerrarTodasMenosFijas()
    {
        Tabs.RemoveAll(t => t.EsCerrable);
        ActiveTabId  = Tabs.FirstOrDefault(t => t.Panel == 0)?.Id;
        ActiveTabIdB = Tabs.FirstOrDefault(t => t.Panel == 1)?.Id;
        NotifyChanged();
    }

    // ── split ─────────────────────────────────────────────────────────────────

    /// <summary>Mueve un tab al panel indicado (0 o 1).</summary>
    public void MoverAPanel(string tabId, int panelDestino)
    {
        var tab = Tabs.FirstOrDefault(t => t.Id == tabId);
        if (tab is null || tab.Panel == panelDestino) return;

        var panelOrigen = tab.Panel;
        tab.Panel = panelDestino;

        // Activar en el destino
        SetActiveTabId(panelDestino, tabId);

        // En el origen, activar otro tab si este era el activo
        if (GetActiveTabId(panelOrigen) == tabId || GetActiveTabId(panelOrigen) == null)
        {
            var vecino = Tabs.FirstOrDefault(t => t.Panel == panelOrigen);
            SetActiveTabId(panelOrigen, vecino?.Id);
        }

        NotifyChanged();
    }

    public void CambiarDireccionSplit()
    {
        SplitDireccion = SplitDireccion == "h" ? "v" : "h";
        NotifyChanged();
    }

    private void NotifyChanged() => OnChange?.Invoke();
}
