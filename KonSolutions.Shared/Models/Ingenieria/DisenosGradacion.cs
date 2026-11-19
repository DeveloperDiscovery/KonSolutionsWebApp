namespace KONSolutions.Shared.Models.Ingenieria;

// ---- Gradacion de tallas de los moldes (script SQL 117) ----

/// <summary>Una talla destino. Se indica por incrementos (VAL_INC_X = largo, VAL_INC_Y = ancho de la pieza, mm) o por reglas por punto
/// (DELTAS: un par [dx, dy] por vertice del molde base). Si vienen las reglas por punto mandan; si no, se calculan desde los incrementos.</summary>
public class DisenosGradacionTalla
{
    public string DES_TALLA { get; set; } = "";
    public double? VAL_INC_X { get; set; }
    public double? VAL_INC_Y { get; set; }
    public List<double[]>? DELTAS { get; set; }
}

public class DisenosGradacionDto
{
    public string COD_DISENO { get; set; } = "";
    public int NUM_VERSION { get; set; }
    /// <summary>Molde base (su NUM_SECUENCIA).</summary>
    public int NUM_SECUENCIA { get; set; }
    public List<DisenosGradacionTalla> Tallas { get; set; } = new();
}

/// <summary>Vista previa de una talla gradada (nada se guarda).</summary>
public class DisenosGradacionVista
{
    public string DES_TALLA { get; set; } = "";
    public string DES_CONTORNO { get; set; } = "";
    public decimal VAL_AREA { get; set; }
    public decimal VAL_LARGO { get; set; }
    public decimal VAL_ALTO { get; set; }
    /// <summary>Los desplazamientos por vertice que se aplicaron (los de las reglas, o los que salen de los incrementos).</summary>
    public List<double[]> DELTAS { get; set; } = new();
}

public class DisenosGradacionPrevia
{
    public int NUM_SECUENCIA { get; set; }
    public string DES_MOLDE { get; set; } = "";
    public string DES_TALLA_BASE { get; set; } = "";
    public string DES_CONTORNO_BASE { get; set; } = "";
    public List<DisenosGradacionVista> Tallas { get; set; } = new();
}

public class DisenosGradacionResultado
{
    public List<int> Secuencias { get; set; } = new();
    public int CREADOS { get; set; }
    public int ACTUALIZADOS { get; set; }
}

/// <summary>Regla guardada (molde base + talla destino).</summary>
public class DisenosGradacionRegla
{
    public string COD_DISENO { get; set; } = "";
    public int NUM_VERSION { get; set; }
    public int NUM_SECUENCIA { get; set; }
    public string DES_TALLA { get; set; } = "";
    public decimal? VAL_INC_X { get; set; }
    public decimal? VAL_INC_Y { get; set; }
    public string? DES_DELTAS { get; set; }
    public string? COD_USUARIO_REGISTRO { get; set; }
    public DateTime? FEC_REGISTRO { get; set; }
    public string? COD_USUARIO_ACTUALIZACION { get; set; }
    public DateTime? FEC_ACTUALIZACION { get; set; }
}

public class DisenosGradacionClaveDto
{
    public string COD_DISENO { get; set; } = "";
    public int NUM_VERSION { get; set; }
    public int NUM_SECUENCIA { get; set; }
    public string DES_TALLA { get; set; } = "";
}

public class DisenosGradacionEliminarResultado { public int ELIMINADOS { get; set; } }
