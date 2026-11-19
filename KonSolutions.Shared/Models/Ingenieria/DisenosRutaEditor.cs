namespace KONSolutions.Shared.Models.Ingenieria;

// ---- Ruta de operaciones de una version: crear, copiar, asignar (scripts SQL 112 y 130 a 134) ----

public class DisenosRutaCombo { public string Id { get; set; } = ""; public string Texto { get; set; } = ""; }

public class DisenosRutaCatalogos
{
    public List<DisenosRutaCombo> TiposRuta { get; set; } = new();
    public List<DisenosRutaCombo> Rutas { get; set; } = new();
}

public class DisenosRutaCrearDto
{
    public string COD_DISENO { get; set; } = "";
    public int NUM_VERSION { get; set; }
    public string DES_RUTA { get; set; } = "";
    public string COD_TIPO_RUTA { get; set; } = "PRD";
}

public class DisenosRutaAsignarDto
{
    public string COD_DISENO { get; set; } = "";
    public int NUM_VERSION { get; set; }
    public string? COD_RUTA { get; set; }
}

public class DisenosRutaCopiarDto
{
    public string COD_DISENO { get; set; } = "";
    public int NUM_VERSION { get; set; }
    public string COD_RUTA_ORIGEN { get; set; } = "";
    public string? DES_RUTA { get; set; }
}

public class DisenosRutaResultado
{
    public string? COD_RUTA_PRODUCCION { get; set; }
    public int? ELEMENTOS_COPIADOS { get; set; }
}
