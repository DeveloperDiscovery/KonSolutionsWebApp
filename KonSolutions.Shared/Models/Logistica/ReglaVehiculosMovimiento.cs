namespace KONSolutions.Shared.Models.Logistica;

/// <summary>La regla de los vehículos de un movimiento, para la guía de remisión: UN vehículo
/// principal y, como opción, UN vehículo de acarreo. El acarreo no va sin principal.
///
/// Los estados válidos son tres: (0 principales, 0 acarreos), (1, 0) y (1, 1). Es la misma regla
/// que aplica la base (dbo.FNC_LOGISTICA_MOVIMIENTO_VEHICULOS_REGLA, que rechaza el alta o el
/// cambio que la rompe). Acá sirve para no ofrecerle al usuario lo que la base va a rechazar:
/// qué rol tendrá el vehículo que se agrega, y si el interruptor de acarreo puede cambiar.
///
/// Un movimiento puede venir mal de antes (dos principales, un acarreo suelto). Esos estados no
/// son válidos, pero tampoco se esconden: se avisan, y se permite el cambio que los arregla.</summary>
public static class ReglaVehiculosMovimiento
{
    /// <summary>Qué rol tendría el próximo vehículo que se agregue.</summary>
    public enum Rol { Principal, Acarreo, Ninguno }

    public static bool EsValido(int principales, int acarreos) =>
        principales <= 1 && acarreos <= 1 && acarreos <= principales;

    /// <summary>El primero que deje el movimiento en un estado válido: el principal si falta, y si
    /// no, el acarreo. Ninguno si el movimiento ya está completo, o si viene mal de antes y
    /// agregar otro lo dejaría igual de mal.</summary>
    public static Rol RolDelNuevo(int principales, int acarreos)
    {
        if (EsValido(principales + 1, acarreos)) return Rol.Principal;
        if (EsValido(principales, acarreos + 1)) return Rol.Acarreo;
        return Rol.Ninguno;
    }

    /// <summary>Si cambiar el rol de UNA fila deja el movimiento en un estado válido. No cambiar
    /// nada siempre se permite.</summary>
    public static bool PuedeCambiar(int principales, int acarreos, bool filaEsAcarreo, bool nuevoEsAcarreo)
    {
        if (filaEsAcarreo == nuevoEsAcarreo) return true;
        var p = principales - (filaEsAcarreo ? 0 : 1) + (nuevoEsAcarreo ? 0 : 1);
        var a = acarreos - (filaEsAcarreo ? 1 : 0) + (nuevoEsAcarreo ? 1 : 0);
        return EsValido(p, a);
    }

    /// <summary>Qué está mal en el movimiento tal como está, o null si está bien.</summary>
    public static string? Problema(int principales, int acarreos)
    {
        if (principales > 1)
            return $"Este movimiento tiene {principales} vehículos principales. Un movimiento lleva un solo vehículo principal: " +
                   "quita los que sobran (si la carga va en otro vehículo, es otro movimiento).";
        if (acarreos > 1)
            return $"Este movimiento tiene {acarreos} vehículos de acarreo. Se permite como máximo uno.";
        if (acarreos > principales)
            return "El vehículo de acarreo no tiene vehículo principal. Agrega el principal.";
        return null;
    }

    /// <summary>Lo que se le dice al usuario sobre el próximo vehículo que agregue.</summary>
    public static string TextoRolDelNuevo(int principales, int acarreos) => RolDelNuevo(principales, acarreos) switch
    {
        Rol.Principal => "El próximo vehículo se agrega como vehículo principal.",
        Rol.Acarreo   => "El próximo vehículo se agrega como vehículo de acarreo (opcional, uno solo).",
        _             => EsValido(principales, acarreos)
                            ? "El movimiento ya tiene su vehículo principal y su vehículo de acarreo."
                            : "Corrige los vehículos del movimiento para poder agregar otro."
    };
}
