namespace KONSolutions.Shared.Models.Logistica;

/// <summary>LOGISTICA.INVENTARIOS_ALMACENES_PUNTO_TOMA_PERSONAL — personas asignadas a un
/// punto de conteo. Clave compuesta NUM_INVENTARIO + COD_ALMACEN + NUM_PUNTO_TOMA_INVENTARIO +
/// COD_ENTIDAD_ASIGNADA. Tabla de asignación pura: solo Asignar (Insert=upsert)/Quitar
/// (Delete), no hay edición. El SP no trae el nombre de la entidad — se resuelve del lado
/// cliente cruzando con el combobox general de Entidades (ver
/// InventariosAlmacenesPuntoTomaPersonalGrid.razor).</summary>
public class InventariosAlmacenesPuntoTomaPersonal
{
    public int? NUM_INVENTARIO { get; set; }
    public string COD_ALMACEN { get; set; } = string.Empty;
    public byte? NUM_PUNTO_TOMA_INVENTARIO { get; set; }
    public decimal? COD_ENTIDAD_ASIGNADA { get; set; }

    public string? DES_OBSERVACION { get; set; }
}
