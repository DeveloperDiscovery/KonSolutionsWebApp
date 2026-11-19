namespace KONSolutions.Shared.Models.Ingenieria;

// ---- Moldes y tizados de una version de diseno (script SQL 115) ----

/// <summary>Una pieza del patron de la version: su contorno 2D en mm ("x,y x,y ..."), talla, cantidades y giros permitidos.</summary>
public class DisenosMolde
{
    public string COD_DISENO { get; set; } = "";
    public int NUM_VERSION { get; set; }
    public int NUM_SECUENCIA { get; set; }
    public string DES_MOLDE { get; set; } = "";
    public string DES_TALLA { get; set; } = "U";
    public int NUM_CANTIDAD { get; set; }
    public int NUM_CANTIDAD_ESPEJADA { get; set; }
    public string DES_ROTACIONES { get; set; } = "0,180";
    public string DES_CONTORNO { get; set; } = "";
    public decimal? VAL_AREA { get; set; }
    public decimal? VAL_LARGO { get; set; }
    public decimal? VAL_ALTO { get; set; }
    // coincidencia de rayas (script SQL 118): el punto va en las mismas coordenadas del contorno
    public bool FLG_COINCIDE_X { get; set; }
    public bool FLG_COINCIDE_Y { get; set; }
    public decimal? VAL_PUNTO_X { get; set; }
    public decimal? VAL_PUNTO_Y { get; set; }
    public decimal VAL_FASE_X { get; set; }
    public decimal VAL_FASE_Y { get; set; }
    public bool FLG_BLOQUEADO { get; set; }
    public string? COD_USUARIO_REGISTRO { get; set; }
    public DateTime? FEC_REGISTRO { get; set; }
    public string? COD_USUARIO_ACTUALIZACION { get; set; }
    public DateTime? FEC_ACTUALIZACION { get; set; }
}

/// <summary>Molde a guardar. NUM_SECUENCIA nulo = molde nuevo; con numero = cambia ese molde. El area y las medidas las calcula el servidor.</summary>
public class DisenosMoldeEntrada
{
    public int? NUM_SECUENCIA { get; set; }
    public string DES_MOLDE { get; set; } = "";
    public string? DES_TALLA { get; set; }
    public int NUM_CANTIDAD { get; set; } = 1;
    public int NUM_CANTIDAD_ESPEJADA { get; set; }
    public string? DES_ROTACIONES { get; set; }
    public string DES_CONTORNO { get; set; } = "";
    /// <summary>Coincidencia de rayas: si coincide en el largo (X) y/o el ancho (Y) de la tela, el punto de coincidencia (mm, mismas coordenadas del contorno) y la fase (mm de la raya).</summary>
    public bool FLG_COINCIDE_X { get; set; }
    public bool FLG_COINCIDE_Y { get; set; }
    public double? VAL_PUNTO_X { get; set; }
    public double? VAL_PUNTO_Y { get; set; }
    public double VAL_FASE_X { get; set; }
    public double VAL_FASE_Y { get; set; }
}

public class DisenosMoldesGuardarDto
{
    public string COD_DISENO { get; set; } = "";
    public int NUM_VERSION { get; set; }
    public List<DisenosMoldeEntrada> Moldes { get; set; } = new();
}

public class DisenosMoldeClaveDto
{
    public string COD_DISENO { get; set; } = "";
    public int NUM_VERSION { get; set; }
    public int NUM_SECUENCIA { get; set; }
}

public class DisenosMoldesGuardarResultado { public List<int> Secuencias { get; set; } = new(); }
public class DisenosMoldeEliminarResultado { public int ELIMINADOS { get; set; } }

public class DisenosMoldeImportarDto
{
    /// <summary>Contenido del archivo DXF (texto).</summary>
    public string DXF { get; set; } = "";
    /// <summary>Milimetros por unidad del archivo. Nulo = el que diga el archivo ($INSUNITS) o milimetros.</summary>
    public double? MM_POR_UNIDAD { get; set; }
}

public class DisenosMoldeImportado
{
    public string Capa { get; set; } = "";
    public string DES_CONTORNO { get; set; } = "";
    public decimal VAL_AREA { get; set; }
    public decimal VAL_LARGO { get; set; }
    public decimal VAL_ALTO { get; set; }
}

public class DisenosMoldeImportarResultado
{
    public List<DisenosMoldeImportado> Contornos { get; set; } = new();
    public List<string> Advertencias { get; set; } = new();
    public double MilimetrosPorUnidad { get; set; } = 1;
}

/// <summary>Prendas a cortar de una talla. Los moldes de talla "U" (unica) se cortan para todas las prendas.</summary>
public class DisenosTizadoTalla
{
    public string DES_TALLA { get; set; } = "";
    public int NUM_PRENDAS { get; set; }
}

/// <summary>Defecto de la tela: zona donde no se corta. Contorno "x,y x,y ..." en mm de la tela (X = largo, Y = ancho).</summary>
public class DisenosTizadoDefectoEntrada
{
    public string DES_DEFECTO { get; set; } = "";
    public string DES_CONTORNO { get; set; } = "";
}

public class DisenosTizadoCalcularDto
{
    public string COD_DISENO { get; set; } = "";
    public int NUM_VERSION { get; set; }
    public string? DES_TIZADO { get; set; }
    public double VAL_ANCHO_TELA { get; set; } = 1500;
    public double VAL_SEPARACION { get; set; } = 3;
    public double VAL_MARGEN { get; set; } = 10;
    public double VAL_CELDA { get; set; } = 5;
    public int NUM_INTENTOS { get; set; } = 6;
    public List<DisenosTizadoTalla> Tallas { get; set; } = new();
    /// <summary>Defectos de tela (zonas prohibidas). Opcional.</summary>
    public List<DisenosTizadoDefectoEntrada> Defectos { get; set; } = new();
    /// <summary>Periodo de la raya en el largo / ancho de la tela, mm. 0 = sin coincidencia en ese sentido.</summary>
    public double VAL_PERIODO_X { get; set; }
    public double VAL_PERIODO_Y { get; set; }
}

public class DisenosTizadoPieza
{
    public string PiezaId { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string Talla { get; set; } = "";
    public int Copia { get; set; }
    public bool Espejada { get; set; }
    public double Angulo { get; set; }
    /// <summary>Contorno ya en su posicion sobre la tela, mm: "x,y x,y ...".</summary>
    public string Puntos { get; set; } = "";
    /// <summary>Punto de coincidencia de rayas ya sobre la tela ("x,y"), si la pieza lo tiene.</summary>
    public string? Coincidencia { get; set; }
    /// <summary>Cuanto se aparta de su raya, mm (solo si la pieza coincide en ese sentido). Debe ser casi 0.</summary>
    public double? ErrorX { get; set; }
    public double? ErrorY { get; set; }
    /// <summary>Fase de rayas de la pieza (solo si coincide en ese sentido).</summary>
    public double? FaseX { get; set; }
    public double? FaseY { get; set; }
}

public class DisenosTizadoDefecto { public string Nombre { get; set; } = ""; public string Puntos { get; set; } = ""; }

public class DisenosTizadoNoColocada { public string PiezaId { get; set; } = ""; public string Nombre { get; set; } = ""; public string Talla { get; set; } = ""; public string Motivo { get; set; } = ""; }

/// <summary>Lo que se guarda en DES_RESULTADO y lo que devuelve el calculo.</summary>
public class DisenosTizadoGeometria
{
    public double Ancho { get; set; }
    public double Largo { get; set; }
    public double AreaPiezas { get; set; }
    public List<DisenosTizadoPieza> Colocadas { get; set; } = new();
    public List<DisenosTizadoNoColocada> NoColocadas { get; set; } = new();
    public List<DisenosTizadoDefecto> Defectos { get; set; } = new();
    public double PeriodoX { get; set; }
    public double PeriodoY { get; set; }
    /// <summary>Separacion y margen con que se calculo (los necesita el ajuste manual para validar).</summary>
    public double Separacion { get; set; }
    public double Margen { get; set; }
    /// <summary>true si el tizado salio de un ajuste manual y no del calculo del motor.</summary>
    public bool Ajustado { get; set; }
}

/// <summary>Ajuste manual de un tizado guardado: se guarda como un tizado nuevo con las mismas piezas movidas, giradas o espejadas.</summary>
public class DisenosTizadoAjusteDto
{
    public string COD_DISENO { get; set; } = "";
    public int NUM_VERSION { get; set; }
    /// <summary>Tizado guardado del que se parte.</summary>
    public int NUM_SECUENCIA { get; set; }
    /// <summary>Nombre del tizado nuevo.</summary>
    public string DES_TIZADO { get; set; } = "";
    /// <summary>Todas las piezas del tizado original (PiezaId + Copia las identifica) con su contorno final.</summary>
    public List<DisenosTizadoPieza> Piezas { get; set; } = new();
}

/// <summary>Resultado de un calculo (guardado o no): medidas, consumo y el dibujo.</summary>
public class DisenosTizadoCalculo
{
    public int? NUM_SECUENCIA { get; set; }
    public double VAL_ANCHO_TELA { get; set; }
    public double VAL_LARGO { get; set; }
    public double VAL_EFICIENCIA { get; set; }
    /// <summary>Metros de tela por prenda (largo / prendas). Nulo si no hay prendas.</summary>
    public double? VAL_CONSUMO_PRENDA { get; set; }
    public int NUM_PRENDAS { get; set; }
    public int NUM_PIEZAS { get; set; }
    public int NUM_NO_COLOCADAS { get; set; }
    public int IntentoGanador { get; set; }
    public DisenosTizadoGeometria Geometria { get; set; } = new();
    public string Svg { get; set; } = "";
}

/// <summary>Fila del listado de tizados guardados (sin el dibujo).</summary>
public class DisenosTizadoResumen
{
    public string COD_DISENO { get; set; } = "";
    public int NUM_VERSION { get; set; }
    public int NUM_SECUENCIA { get; set; }
    public string DES_TIZADO { get; set; } = "";
    public decimal VAL_ANCHO_TELA { get; set; }
    public decimal VAL_SEPARACION { get; set; }
    public decimal VAL_MARGEN { get; set; }
    public decimal VAL_CELDA { get; set; }
    public int NUM_INTENTOS { get; set; }
    public string DES_TALLAS { get; set; } = "";
    public int NUM_PRENDAS { get; set; }
    public int NUM_PIEZAS { get; set; }
    public int NUM_NO_COLOCADAS { get; set; }
    public decimal VAL_LARGO { get; set; }
    public decimal VAL_EFICIENCIA { get; set; }
    public decimal? VAL_CONSUMO_PRENDA { get; set; }
    public string? COD_USUARIO_REGISTRO { get; set; }
    public DateTime? FEC_REGISTRO { get; set; }
}

/// <summary>Fila completa de un tizado guardado (con DES_RESULTADO en JSON).</summary>
public class DisenosTizadoFila : DisenosTizadoResumen { public string DES_RESULTADO { get; set; } = "{}"; }

public class DisenosTizadoEliminarResultado { public int ELIMINADOS { get; set; } }
