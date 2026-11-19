using KONSolutions.Shared.Models.Estandar;

namespace KONSolutions.Web.Pages.Ingenieria;

/// <summary>
/// Tipos de la Matriz de Especificaciones Técnicas.
///
/// Están acá y no dentro del .razor porque los comparten la pantalla y el componente de
/// celda. Es el mismo juego de tipos que usa EspecificacionesTecnicasPanel para representar
/// una especificación y su valor; se replica en vez de reutilizarse porque allá son privados
/// del componente, y sacarlos de ahí tocaría una pantalla que hoy funciona.
/// </summary>
public enum TipoEspecMatriz
{
    Ninguno, Lista, Entero, Numerico, Cadena, FechaHora, Fecha, Hora, Logico, Color
}

/// <summary>Una especificación técnica: es una fila de la matriz, o una columna si está
/// invertida. Lo que define no es la posición sino el dato.</summary>
public class EspecMatriz
{
    public string Codigo { get; set; } = "";
    public string Descripcion { get; set; } = "";

    /// <summary>COD_ESPECIFICACION_TECNICA_PADRE tal como viene de la base.
    ///
    /// OJO: no significa "es dependiente". Las especificaciones de primer nivel traen "00000"
    /// —el Root— que no es NULL ni vacío. Para saber si realmente depende de otra hay que
    /// usar PadreDe() en la pantalla, que solo cuenta al padre si está cargado en la matriz.
    /// Leer este campo directamente convierte a toda especificación raíz en dependiente de
    /// algo inexistente, y sus combos salen vacíos.</summary>
    public string? CodPadre { get; set; }

    public TipoEspecMatriz Tipo { get; set; }
    public int? CanDecimal { get; set; }

    /// <summary>Unidad ya formateada, con la divisora si la hay: "LIBRA (lb) / PULGADA (inh)".</summary>
    public string? Unidad { get; set; }

    /// <summary>Con rango, el valor no es uno sino dos: mínimo y máximo.</summary>
    public bool FlgRange { get; set; }

    public bool? FlgFichaTecnica { get; set; }
    public bool? FlgIncluirDescripcion { get; set; }
    public int? CodGrupo { get; set; }
    public string? DesGrupo { get; set; }
    public int? NumOrden { get; set; }
}

/// <summary>El valor de un par producto–especificación.
///
/// Que el valor viva en el PAR y no en la fila es lo que permite invertir la matriz sin
/// recargar y sin perder lo editado: al girar cambia cómo se dibuja, no dónde está el dato.
/// </summary>
public class CeldaMatriz
{
    /// <summary>No hay fila en PRODUCTOS_ESPECIFICACIONES_TECNICAS para ese producto: la
    /// especificación está asignada a la sub familia pero no a este producto en particular.
    ///
    /// Es distinto de "asignada y todavía sin valor", y por eso se dibuja distinto. Sin esa
    /// diferencia, el contador de celdas pendientes miente.</summary>
    public bool NoAplica { get; set; }

    public string? Texto { get; set; }
    public string? CodItem { get; set; }
    public DateTime? Fecha { get; set; }
    public TimeSpan? Hora { get; set; }
    public bool? Logico { get; set; }
    public decimal? Minimo { get; set; }
    public decimal? Maximo { get; set; }

    public bool EstaVacia =>
        string.IsNullOrWhiteSpace(Texto) && string.IsNullOrEmpty(CodItem)
        && Fecha is null && Hora is null && Logico is null
        && Minimo is null && Maximo is null;

    /// <summary>Traduce una fila del SHOWALL a una celda.</summary>
    public static CeldaMatriz Desde(ProductosEspecificacionesTecnicas f, TipoEspecMatriz tipo) => new()
    {
        Texto   = tipo == TipoEspecMatriz.Cadena ? f.STR_ESPECIFICACION_TENICA : f.VAL_ESPECIFICACION_TENICA,
        CodItem = f.COD_ITEM_TECNICO?.ToString(),
        Fecha   = f.FEC_ESPECIFICACION_TENICA,
        Hora    = f.HOR_ESPECIFICACION_TENICA,
        Logico  = f.LOG_ESPECIFICACION_TENICA,
        Minimo  = f.VAL_MINIMO,
        Maximo  = f.VAL_MAXIMO
    };
}
