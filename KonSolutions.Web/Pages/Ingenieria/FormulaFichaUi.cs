namespace KONSolutions.Web.Pages.Ingenieria;

/// <summary>Una ficha de una formula en el editor (FormulaFichasEditor).</summary>
public sealed class FormulaFichaUi
{
    public string Comp = "", Elem = "", Alias = "", Tec = "", DesComp = "", DesElem = "";
    public string? Valor, Back, Fore;
    /// <summary>Especificacion tecnica: el texto propio (Valor) es su descripcion en singular.</summary>
    public bool Espec;
    /// <summary>Codigo de la especificacion tecnica (solo al agregarla desde la paleta; la base resuelve el elemento portador).</summary>
    public string? EspCod;

    /// <summary>Texto de pantalla de la ficha: el valor en constantes y especificaciones, el alias en las demas.</summary>
    public static string Etiqueta(FormulaFichaUi f) => (f.Comp == "CONST" || f.Espec) && !string.IsNullOrEmpty(f.Valor) ? f.Valor! : f.Alias;
}
