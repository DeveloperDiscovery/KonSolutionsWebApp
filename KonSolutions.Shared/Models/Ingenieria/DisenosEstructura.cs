namespace KONSolutions.Shared.Models.Ingenieria;

/// <summary>Un nodo de la estructura de una version de diseno (API: disenos-estructura).
/// TIPO_NODO: VER (version), SEC (sector), COM (componente), INS (insumo). Un INS con FLG_ENSAMBLE es un
/// subensamble: sus sectores cuelgan de el (hasta 10 niveles de ensamble).</summary>
public class DisenosEstructuraNodo
{
    public int ID { get; set; }
    public int? PARENT_ID { get; set; }
    public string ORDEN { get; set; } = "";
    public int NIVEL_ENSAMBLE { get; set; }
    public int PROFUNDIDAD { get; set; }
    public string TIPO_NODO { get; set; } = "";
    public string COD_DISENO { get; set; } = "";
    public int NUM_VERSION { get; set; }
    public string? COD_SECTOR_PRODUCTO { get; set; }
    public string? COD_COMPONENTE_PRODUCTO { get; set; }
    public int? NUM_SECUENCIA { get; set; }
    public string? COD_PRODUCTO_COMPONENTE { get; set; }
    public string? DESCRIPCION { get; set; }
    public string? COD_UNIDAD_MEDIDA { get; set; }
    public decimal? CAN_CONSUMO { get; set; }
    public decimal? TAS_MERMA { get; set; }
    public string? COD_TIPO_MERMA { get; set; }
    public string? COD_MONEDA { get; set; }
    public decimal? PRE_UNITARIO { get; set; }
    public decimal? CANTIDAD_ACUMULADA { get; set; }
    public decimal? COSTO_TOTAL { get; set; }
    public bool FLG_ENSAMBLE { get; set; }
    public bool FLG_CICLO { get; set; }
    public bool FLG_TRUNCADO { get; set; }
    public bool? FLG_TOTAL_PRODUCTO { get; set; }
    public bool FLG_BLOQUEADO { get; set; }
    public string? COD_DISENO_ENSAMBLE { get; set; }
    public int? NUM_VERSION_ENSAMBLE { get; set; }
    // Rutas de produccion (nodos RUT / PRC / SPR / OPE): solo lectura
    public string? COD_RUTA_PRODUCCION { get; set; }
    public int? NUM_ESTRUCTURA_RUTA_PROCESO { get; set; }
    public decimal? CAN_LEAD_TIME { get; set; }
    public string? COD_UNIDAD_MEDIDA_TIEMPO { get; set; }
    public bool? FLG_MANUAL { get; set; }
    public bool? FLG_CONTROL_CALIDAD { get; set; }
    public string? COD_PROCESO { get; set; }
    public string? COD_SUB_PROCESO { get; set; }
    public string? COD_TIPO_OPERACION_PRODUCCION { get; set; }
}

public class DisenosEstructuraSectorDto
{
    public string COD_DISENO { get; set; } = "";
    public int NUM_VERSION { get; set; }
    public string COD_SECTOR_PRODUCTO { get; set; } = "";
    public string DES_SECTOR_PRODUCTO { get; set; } = "";
    public bool FLG_TOTAL_PRODUCTO { get; set; }
}

public class DisenosEstructuraComponenteDto
{
    public string COD_DISENO { get; set; } = "";
    public int NUM_VERSION { get; set; }
    public string COD_SECTOR_PRODUCTO { get; set; } = "";
    public string COD_COMPONENTE_PRODUCTO { get; set; } = "";
}

public class DisenosEstructuraInsumoDto
{
    public string COD_DISENO { get; set; } = "";
    public int NUM_VERSION { get; set; }
    public string COD_SECTOR_PRODUCTO { get; set; } = "";
    public string COD_COMPONENTE_PRODUCTO { get; set; } = "";
    public int? NUM_SECUENCIA { get; set; }
    public string COD_PRODUCTO_COMPONENTE { get; set; } = "";
    public decimal CAN_CONSUMO { get; set; }
    public string? COD_TIPO_MERMA { get; set; }
    public decimal TAS_MERMA { get; set; }
    public string? COD_MONEDA { get; set; } = "PEN";
    public decimal PRE_UNITARIO { get; set; }
    public string? COD_DISENO_ENLACE { get; set; }
    public int? NUM_VERSION_ENLACE { get; set; }
    /// <summary>bool (no nullable): sin marcar es false, nunca NULL.</summary>
    public bool FLG_PRODUCTO_COMPONENTE_PIVOT { get; set; }
    /// <summary>Clasificacion del material en esta linea (01 a 05, opcional). Sin valor, al crear la linea la base usa el del componente o el de la sub familia.</summary>
    public string? COD_TIPO_MATERIAL { get; set; }
}

public class DisenosEstructuraComponenteCambiarDto
{
    public string COD_DISENO { get; set; } = "";
    public int NUM_VERSION { get; set; }
    public string COD_SECTOR_PRODUCTO { get; set; } = "";
    public string COD_COMPONENTE_ORIGEN { get; set; } = "";
    public string COD_COMPONENTE_DESTINO { get; set; } = "";
}

public class DisenosEstructuraInsumoClaveDto
{
    public string COD_DISENO { get; set; } = "";
    public int NUM_VERSION { get; set; }
    public string COD_SECTOR_PRODUCTO { get; set; } = "";
    public string COD_COMPONENTE_PRODUCTO { get; set; } = "";
    public int NUM_SECUENCIA { get; set; }
}

public class DisenosVersionClaveDto
{
    public string COD_DISENO { get; set; } = "";
    public int NUM_VERSION { get; set; }
}

public class DisenosVersionCopiarDto
{
    public string COD_DISENO_ORIGEN { get; set; } = "";
    public int NUM_VERSION_ORIGEN { get; set; }
    public string? COD_DISENO_DESTINO { get; set; }
    public int? NUM_VERSION_DESTINO { get; set; }
    public string? DES_VERSION { get; set; }
}

public class DisenosVersionCopiarResultado
{
    public int NUM_VERSION_NUEVA { get; set; }
    public int SECTORES { get; set; }
    public int COMPONENTES { get; set; }
    public int INSUMOS { get; set; }
}

public class DisenosSegmentoCopiarDto
{
    public string COD_DISENO_ORIGEN { get; set; } = "";
    public int NUM_VERSION_ORIGEN { get; set; }
    public string COD_SECTOR_ORIGEN { get; set; } = "";
    public string? COD_COMPONENTE_ORIGEN { get; set; }
    public string COD_DISENO_DESTINO { get; set; } = "";
    public int NUM_VERSION_DESTINO { get; set; }
    public string? COD_SECTOR_DESTINO { get; set; }
    /// <summary>AGREGAR o REEMPLAZAR.</summary>
    public string MODO { get; set; } = "AGREGAR";
}

public class DisenosSegmentoCopiarResultado
{
    public int INSUMOS_COPIADOS { get; set; }
}

public class DisenosEstructuraInsumoResultado
{
    public int NUM_SECUENCIA { get; set; }
}
