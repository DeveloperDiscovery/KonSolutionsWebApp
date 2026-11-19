using KONSolutions.Shared.Models.Rfid;
using KONSolutions.Shared.Rfid.Agente;

namespace KonSolutions.Agente.Emision;

/// <summary>Elige el producto-equipo (lector Impinj o impresora Zebra) con que trabaja esta corrida: el que se pidio con --equipo, el fijo de appsettings, o el de esta estacion.
/// Si en la estacion hay mas de uno (por ejemplo un lector y una impresora) no adivina: pide elegir con --equipo.</summary>
public static class ResolverEquipo
{
    public sealed record Resultado(RfidEquipo? Equipo, string? Error);

    public static Resultado Elegir(IReadOnlyList<RfidEquipo> equipos, string? codPedido, string? codConfig, string? estacion)
    {
        if (equipos.Count == 0)
            return new(null, "La API no devolvió ningún equipo. Revisa Configuración general > RFID (grupo, familia) y que el producto del lector esté en ese grupo.");

        var cod = !string.IsNullOrWhiteSpace(codPedido) ? codPedido.Trim() : string.IsNullOrWhiteSpace(codConfig) ? null : codConfig.Trim();
        if (cod is not null)
        {
            var uno = equipos.FirstOrDefault(e => string.Equals(e.COD_PRODUCTO?.Trim(), cod, StringComparison.OrdinalIgnoreCase));
            return uno is null
                ? new(null, $"El producto '{cod}' no figura entre los equipos RFID. Equipos: {Listar(equipos)}.")
                : new(uno, null);
        }

        var soportados = equipos.Where(EsSoportado).ToList();
        if (!string.IsNullOrWhiteSpace(estacion))
        {
            var deLaEstacion = soportados.Where(e => string.Equals(e.COD_ESTACION_AGENTE?.Trim(), estacion.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();
            if (deLaEstacion.Count == 1) return new(deLaEstacion[0], null);
            if (deLaEstacion.Count > 1) return new(null, $"Hay {deLaEstacion.Count} equipos (Impinj o Zebra) en la estación {estacion} (la estación es una sola para todos los equipos): indica cuál con --equipo o con «CodProductoEquipo» del appsettings.json ({Listar(deLaEstacion)}).");
            return new(null, $"Ningún equipo Impinj ni Zebra tiene la estación {estacion}: la estación se elige en Configuración general > RFID y tiene que ser la misma que pones en «Estacion» del appsettings.json. Equipos: {Listar(soportados)}.");
        }
        if (soportados.Count == 1) return new(soportados[0], null);
        return new(null, soportados.Count == 0
            ? $"Ningún equipo de la lista es Impinj ni Zebra ({Listar(equipos)})."
            : $"Hay {soportados.Count} equipos (Impinj o Zebra): indica cuál con --equipo o con «CodProductoEquipo» del appsettings.json ({Listar(soportados)}).");
    }

    public static bool EsImpinj(RfidEquipo e)
        => RfidEquipoConfig.Desde(e).EsDe("impinj");

    public static bool EsZebra(RfidEquipo e)
        => RfidEquipoConfig.Desde(e).EsDe("zebra");

    /// <summary>Los que el agente sabe manejar: lectores Impinj e impresoras Zebra.</summary>
    public static bool EsSoportado(RfidEquipo e) => EsImpinj(e) || EsZebra(e);

    private static string Listar(IEnumerable<RfidEquipo> equipos)
        => string.Join(", ", equipos.Select(e => $"{e.COD_PRODUCTO} ({e.DES_PRODUCTO})"));
}
