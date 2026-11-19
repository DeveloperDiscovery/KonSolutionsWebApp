namespace KONSolutions.Web.Services;

/// <summary>Implementado opcionalmente por páginas de submódulo abiertas como pestaña del
/// workspace (ver WorkspaceTabHost.razor) para que este no las muestre hasta que terminen
/// de cargar sus datos iniciales — evita el "flash" de grilla vacía o campos en blanco
/// mientras el OnInitializedAsync todavía está en vuelo. Una página que NO implemente esta
/// interfaz se comporta como siempre: se muestra apenas se monta el componente.</summary>
public interface IWorkspaceCargable
{
    bool EstaCargado { get; }

    /// <summary>Se dispara cuando EstaCargado pasa a true, para que WorkspaceTabHost se
    /// vuelva a pintar y oculte el spinner de la pestaña.</summary>
    event Action? OnCargaCompleta;
}
