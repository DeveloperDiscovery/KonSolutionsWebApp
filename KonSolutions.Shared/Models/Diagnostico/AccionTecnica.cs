namespace KONSolutions.Shared.Models.Diagnostico;

/// <summary>
/// Ficha técnica de un botón de acción: qué clase, qué método y qué stored procedure hay
/// detrás. La devuelve GET api/v1/diagnostico/accion y la muestra AccionTecnicaDialog al
/// hacer clic derecho sobre cualquier botón de AccionesBotones.
///
/// Espejo del modelo del mismo nombre en la API. Los tres datos se resuelven allá por
/// reflexión sobre los controllers y sobre las constantes de SP: no salen de la
/// configuración en BD, porque DES_METODO no se está registrando.
/// </summary>
public class AccionTecnica
{
    public string? Formulario      { get; set; }
    public string? Clase           { get; set; }
    public string? Metodo          { get; set; }
    public string? Ruta            { get; set; }
    public string? StoredProcedure { get; set; }
    public string? Boton           { get; set; }
    public string? Verbo           { get; set; }

    /// <summary>Qué se pudo resolver y qué no. Cuando Resuelto es false, acá está el motivo
    /// — es el dato útil para un botón de proceso todavía sin implementar.</summary>
    public string? Observacion     { get; set; }

    public bool    Resuelto        { get; set; }
}
