namespace KONSolutions.Web.Services;

/// <summary>
/// Los estilos de los botones de acción, en UN solo lugar.
///
/// POR QUE EXISTE
/// --------------
/// Esta misma cadena estaba repetida como constante privada en 17 archivos
/// (EstiloBotonConfirmar) y en 13 (EstiloBotonCancelar). Hoy las 30 copias son
/// idénticas —lo verifiqué una por una—, pero eso es suerte, no diseño: el día que
/// alguien ajuste el peso de la fuente o agregue un borde, lo va a hacer en el
/// archivo que tenga abierto y las otras 29 pantallas van a quedar distintas sin
/// que nadie lo note hasta verlas lado a lado.
///
/// LOS COLORES SALEN DE CONFIGURACION GENERAL
/// ------------------------------------------
/// --boton-backcolor y --boton-forecolor los escribe ConfigService a partir de
/// ESTANDAR.CONFIGURACION_GENERAL (DES_BUTTON_BACKCOLOR / DES_BUTTON_FORECOLOR).
///
/// El respaldo NO es "unset" sino --ink/--paper, que son colores reales: si alguno
/// de los dos campos queda sin configurar, el botón se ve igual pero legible, en vez
/// de quedar transparente o con texto del mismo color que el fondo.
///
/// EL CONTRASTE
/// ------------
/// Cancelar usa los MISMOS dos colores pero invertidos. Así los dos botones
/// contrastan entre sí sin importar qué par se configure: no hay combinación posible
/// que los deje iguales, porque uno es siempre el negativo del otro.
/// </summary>
public static class EstilosBoton
{
    /// <summary>Botón que confirma la acción (Crear, Modificar, Guardar, Aplicar).</summary>
    public const string Confirmar =
        "font-weight:700;background-color:var(--boton-backcolor,var(--ink));color:var(--boton-forecolor,var(--paper));";

    /// <summary>Botón que cancela o cierra. Los mismos colores, invertidos.</summary>
    public const string Cancelar =
        "font-weight:700;background-color:var(--boton-forecolor,var(--paper));color:var(--boton-backcolor,var(--ink));";

    /// <summary>Cancelar de baja jerarquía: el que cancela una edición EN LÍNEA dentro de
    /// un panel, no el del pie de un diálogo. Mismo color configurado, pero como texto y
    /// sin fondo, para que no compita con el botón de confirmar del formulario.</summary>
    public const string CancelarPlano =
        "font-weight:700;color:var(--boton-backcolor,var(--ink));";

    /// <summary>Eliminar NO toma el color configurado: se queda en el rojo de peligro.
    ///
    /// Es deliberado y conviene no "arreglarlo": si el color configurado fuera verde,
    /// un botón de eliminar verde es exactamente el tipo de detalle que hace que
    /// alguien borre lo que no quería.</summary>
    public const string Eliminar = "font-weight:700;";
}
