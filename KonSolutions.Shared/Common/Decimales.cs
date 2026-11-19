namespace KONSolutions.Shared.Common;

/// <summary>Cuántos decimales mostrar en cada tipo de número, según lo que REALMENTE guarda
/// la columna en SQL Server.
///
/// EL PROBLEMA QUE RESUELVE
/// ------------------------
/// Las grillas venían con "N2" escrito a mano. Pero las cantidades se guardan en
/// dbo.VAL_DECIMAL, que es decimal(10,6): seis decimales. Una línea de 1,000125 se veía
/// como 1,00, y la suma de la columna no cerraba con la de la base sin que nada avisara.
/// Redondear al mostrar es peor que un número largo, porque el número largo se entiende y
/// el redondeo hace dudar de la base.
///
/// LOS DOMINIOS, TAL COMO ESTÁN DEFINIDOS EN LA BASE
/// --------------------------------------------------
/// Salen de los CREATE TYPE del script de la base, no de memoria:
///
///     VAL_DECIMAL           decimal(10, 6)     cantidades de producto
///     VAL_IMPORTE           numeric(18, 2)     importes
///     VAL_PRECIO            numeric(12, 2)     precios de lista
///     VAL_PRECIO_UNITARIO   decimal(18, 8)     precio unitario
///     VAL_CONSUMO           numeric(20, 8)     consumos
///     VAL_NUMERICO          numeric(20, 8)     genérico
///     TAS_PORCENTUAL        numeric(10, 3)     porcentajes
///
/// Están acá y no repetidos en cada .razor para que el día que un dominio cambie de
/// precisión se toque UN lugar. Si escribes "N2" a mano en una grilla nueva, ese número
/// queda fuera de este acuerdo y nadie se va a enterar hasta que no cuadre.</summary>
public static class Decimales
{
    /// <summary>Cantidades de producto. dbo.VAL_DECIMAL = decimal(10,6).</summary>
    public const int Cantidad = 6;

    /// <summary>Importes. dbo.VAL_IMPORTE = numeric(18,2).</summary>
    public const int Importe = 2;

    /// <summary>Precios de lista. dbo.VAL_PRECIO = numeric(12,2).</summary>
    public const int Precio = 2;

    /// <summary>Precio unitario. dbo.VAL_PRECIO_UNITARIO = decimal(18,8).</summary>
    public const int PrecioUnitario = 8;

    /// <summary>Consumos y numéricos genéricos. numeric(20,8).</summary>
    public const int Consumo = 8;

    /// <summary>Porcentajes. dbo.TAS_PORCENTUAL = numeric(10,3).</summary>
    public const int Porcentual = 3;

    // Los mismos, ya armados como cadena de formato, que es lo que piden MudDataGrid y
    // MudNumericField. Constantes y no métodos: así se pueden usar en un atributo Razor
    // sin paréntesis ni llamadas.
    public const string FormatoCantidad       = "N6";
    /// <summary>Factores y tipo de cambio. FAC_TIPO_CAMBIO y FAC_CONVERSION son VAL_DECIMAL,
    /// o sea la misma precisión que una cantidad. Tiene nombre propio igual, porque leer
    /// "FormatoCantidad" sobre un tipo de cambio confunde a quien venga después.</summary>
    public const string FormatoFactor         = "N6";
    public const string FormatoImporte        = "N2";
    public const string FormatoPrecio         = "N2";
    public const string FormatoPrecioUnitario = "N8";
    public const string FormatoConsumo        = "N8";
    public const string FormatoPorcentual     = "N3";
}
