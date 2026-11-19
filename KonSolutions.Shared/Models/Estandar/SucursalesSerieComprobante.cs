using System.ComponentModel.DataAnnotations;

namespace KONSolutions.Shared.Models.Estandar;

/// <summary>ESTANDAR.SUCURSALES_SERIE_COMPROBANTE — las series de un tipo de comprobante
/// dentro de una sucursal, cada una con su último correlativo emitido.
///
/// Es nieta de SUCURSALES: su clave es sucursal + tipo de comprobante + serie. Una sucursal
/// puede tener varias series del mismo comprobante (por caja, por punto de emisión), y cada
/// una lleva su propio correlativo.
///
/// Espejo del modelo del API: un campo que exista de un solo lado se pierde al serializar.</summary>
public class SucursalesSerieComprobante
{
    public string COD_SUCURSAL { get; set; } = string.Empty;
    public string COD_TIPO_COMPROBANTE_PAGO { get; set; } = string.Empty;
    public int? NUM_SERIE_COMPROBANTE { get; set; }
    public int? NUM_ULTIMO_CORRELATIVO { get; set; }

    // Sólo lectura, de los JOIN del SP.
    public string? DES_SUCURSAL { get; set; }
    public string? DES_DIRECCION { get; set; }
    public string? DES_TIPO_COMPROBANTE_PAGO { get; set; }
    public string? DES_LINK_PATH_FACTURACION { get; set; }
    public string? DES_TOKEN_FACTURACION { get; set; }
}
