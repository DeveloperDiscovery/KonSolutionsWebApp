namespace KONSolutions.Shared.Models.Reportes;

// Reporteador (esquema REPORTES). Tienen que existir IGUAL en la API y en Shared (aquí faltan a propósito
// ReporteDefinicion y ReporteDefinicionCabecera: llevan el nombre del SP y viven solo en la API): un campo que está en
// uno solo se pierde al serializar, sin error. Los nombres son los de las columnas de los SP.
//
// Códigos que usan varias clases:
//   COD_TIPO_DISENO   LIS listado (varias filas) / FIC ficha (un registro) / BAN libre (lienzo por bandas)
//   COD_FORMATO       PDF / DOC (Word) / XLS (Excel) / CSV
//   COD_PAPEL         (en resultados) CAB cabecera / DET detalle / ADI adicional
//   COD_PRESENTACION  CAM campos / TAB tabla / TOT totales / OCU oculto
//   COD_CONTROL       FEC fecha / FEH fecha y hora / NUM número / TXT texto / CMB combo / CHK sí-no
//   COD_ORIGEN        AUT automático / DLG diálogo / FIL campo de la fila / FIJ fijo / SIS sistema
//   COD_ALINEACION    I izquierda / C centro / D derecha

// ── Fuentes ─────────────────────────────────────────────────────────────────────

/// <summary>Un SP registrado como fuente de datos (PROC_REPORTES_FUENTES_SHOWALL / _SHOWBYID).</summary>
public class ReporteFuente
{
    public int?      NUM_FUENTE                { get; set; }
    public string?   DES_FUENTE                { get; set; }
    public string?   DES_ESQUEMA               { get; set; }
    public string?   DES_PROCEDIMIENTO         { get; set; }
    public string?   COD_APLICACION            { get; set; }
    public string?   DES_APLICACION            { get; set; }
    public int?      NUM_TIEMPO_MAXIMO         { get; set; }
    public string?   DES_OBSERVACION           { get; set; }
    public bool      FLG_HABILITADO            { get; set; } = true;
    public bool      FLG_EXISTE                { get; set; }
    public int       NUM_PARAMETROS            { get; set; }
    public int       NUM_RESULTADOS            { get; set; }
    public int       NUM_DISENOS               { get; set; }
    public string?   COD_USUARIO_REGISTRO      { get; set; }
    public DateTime? FEC_REGISTRO              { get; set; }
    public string?   COD_USUARIO_ACTUALIZACION { get; set; }
    public DateTime? FEC_ACTUALIZACION         { get; set; }
}

/// <summary>Un parámetro del SP de la fuente, con lo que el usuario configuró. En la definición de un
/// reporte trae además de dónde sale su valor (COD_ORIGEN y DES_VALOR, según el botón).</summary>
public class ReporteFuenteParametro
{
    public int?    NUM_FUENTE              { get; set; }
    public string  DES_PARAMETRO           { get; set; } = "";
    public int?    NUM_ORDEN               { get; set; }
    public string? DES_TIPO_DATO           { get; set; }
    public int?    NUM_LONGITUD            { get; set; }
    public int?    NUM_PRECISION           { get; set; }
    public int?    NUM_ESCALA              { get; set; }
    public string? DES_ETIQUETA            { get; set; }
    public string  COD_CONTROL             { get; set; } = "TXT";
    public string? DES_COMBO_PROCEDIMIENTO { get; set; }
    public string? DES_COMBO_COLUMNA_ID    { get; set; }
    public string? DES_COMBO_COLUMNA_TEXTO { get; set; }
    public string? COD_VALOR_DEFECTO       { get; set; }
    public string? DES_VALOR_DEFECTO       { get; set; }
    public bool    FLG_OBLIGATORIO         { get; set; }
    public bool    FLG_SISTEMA             { get; set; }
    public bool    FLG_VISIBLE             { get; set; } = true;
    public string? COD_ORIGEN              { get; set; }
    public string? DES_VALOR               { get; set; }
}

/// <summary>Un resultado del SP de la fuente (el primer SELECT es el 1, el segundo el 2...).</summary>
public class ReporteFuenteResultado
{
    public int?    NUM_FUENTE    { get; set; }
    public int     NUM_RESULTADO { get; set; }
    public string? DES_RESULTADO { get; set; }
    public string  COD_PAPEL     { get; set; } = "DET";
}

/// <summary>Una columna de un resultado de la fuente, con su etiqueta, formato y alineación por defecto.</summary>
public class ReporteFuenteColumna
{
    public int?    NUM_FUENTE     { get; set; }
    public int     NUM_RESULTADO  { get; set; }
    public string  DES_COLUMNA    { get; set; } = "";
    public int?    NUM_ORDEN      { get; set; }
    public string? DES_TIPO_DATO  { get; set; }
    public string? DES_ETIQUETA   { get; set; }
    public string? DES_FORMATO    { get; set; }
    public string? COD_ALINEACION { get; set; }
}

/// <summary>La fuente completa: es lo que devuelve _SHOWBYID y lo que se manda a grabar.</summary>
public class ReporteFuenteDetalle
{
    public ReporteFuente                Fuente     { get; set; } = new();
    public List<ReporteFuenteParametro> Parametros { get; set; } = new();
    public List<ReporteFuenteResultado> Resultados { get; set; } = new();
    public List<ReporteFuenteColumna>   Columnas   { get; set; } = new();
}

/// <summary>Un SP de la base, para elegir la fuente o el SP de un combo.</summary>
public class ReporteProcedimiento
{
    public string DES_ESQUEMA       { get; set; } = "";
    public string DES_PROCEDIMIENTO { get; set; } = "";
    public string DES_NOMBRE        { get; set; } = "";
    public bool   FLG_REGISTRADO    { get; set; }
}

/// <summary>Un parámetro tal como lo declara el SP (sys.parameters).</summary>
public class ReporteParametroSp
{
    public string  DES_PARAMETRO            { get; set; } = "";
    public int     NUM_ORDEN                { get; set; }
    public string? DES_TIPO_DATO            { get; set; }
    public string? DES_TIPO_USUARIO         { get; set; }
    public int?    NUM_LONGITUD             { get; set; }
    public int?    NUM_PRECISION            { get; set; }
    public int?    NUM_ESCALA               { get; set; }
    public bool    FLG_SALIDA               { get; set; }
    public string? DES_ETIQUETA_DICCIONARIO { get; set; }
}

/// <summary>Una etiqueta del diccionario (ENTORNO.DICCIONARIO_ETIQUETAS) para una columna.</summary>
public class ReporteEtiqueta
{
    public string DES_COLUMNA  { get; set; } = "";
    public string DES_ETIQUETA { get; set; } = "";
}

/// <summary>Ejecutar el SP con valores de ejemplo para ver qué devuelve (pantalla de fuentes).</summary>
public class ReportePruebaDto
{
    public string?                     DES_ESQUEMA       { get; set; }
    public string?                     DES_PROCEDIMIENTO { get; set; }
    public int?                        NUM_TIEMPO_MAXIMO { get; set; }
    public List<ReporteFuenteParametro> Parametros       { get; set; } = new();
    /// <summary>Valor de ejemplo por parámetro, sin la @. Fechas "yyyy-MM-dd", números con punto.</summary>
    public Dictionary<string, string?> Valores           { get; set; } = new();
}

/// <summary>Lo que devolvió la prueba: un bloque por resultado, con columnas y unas filas de muestra.</summary>
public class ReportePrueba
{
    public double                       NUM_SEGUNDOS { get; set; }
    public List<ReportePruebaResultado> Resultados   { get; set; } = new();
}

public class ReportePruebaResultado
{
    public int                         NUM_RESULTADO { get; set; }
    public int                         NUM_FILAS     { get; set; }
    public List<ReporteFuenteColumna>  Columnas      { get; set; } = new();
    /// <summary>Las primeras filas, ya formateadas, en el orden de Columnas.</summary>
    public List<List<string?>>         Muestra       { get; set; } = new();
}

// ── Diseños ─────────────────────────────────────────────────────────────────────

/// <summary>Cómo se ve un reporte: listado o ficha, título, papel... (PROC_REPORTES_DISENOS_*).</summary>
public class ReporteDiseno
{
    public int?      NUM_DISENO                { get; set; }
    public int?      NUM_FUENTE                { get; set; }
    public string?   DES_FUENTE                { get; set; }
    public string?   DES_DISENO                { get; set; }
    public string    COD_TIPO_DISENO           { get; set; } = "LIS";
    public string?   DES_TITULO                { get; set; }
    public string?   DES_SUBTITULO             { get; set; }
    public string    COD_ORIENTACION           { get; set; } = "V";
    public string    COD_PAPEL                 { get; set; } = "A4";
    public int       NUM_TAMANO_LETRA          { get; set; } = 8;
    public bool      FLG_LOGO                  { get; set; } = true;
    public bool      FLG_DATOS_EMPRESA         { get; set; } = true;
    public bool      FLG_FILTROS_PIE           { get; set; } = true;
    public bool      FLG_SUBTOTAL_GRUPO        { get; set; } = true;
    public bool      FLG_SALTO_GRUPO           { get; set; }
    public bool      FLG_TOTAL_GENERAL         { get; set; } = true;
    public string?   DES_FIRMAS                { get; set; }
    public string?   DES_PIE                   { get; set; }
    public string?   DES_SEPARADOR_CSV         { get; set; }
    public bool      FLG_HABILITADO            { get; set; } = true;
    public bool      FLG_PDF                   { get; set; }
    public bool      FLG_DOC                   { get; set; }
    public bool      FLG_XLS                   { get; set; }
    public bool      FLG_CSV                   { get; set; }
    public bool      FLG_PROPIA_DOC            { get; set; }
    public bool      FLG_PROPIA_XLS            { get; set; }
    public int       NUM_BOTONES               { get; set; }
    public string?   COD_USUARIO_REGISTRO      { get; set; }
    public DateTime? FEC_REGISTRO              { get; set; }
    public string?   COD_USUARIO_ACTUALIZACION { get; set; }
    public DateTime? FEC_ACTUALIZACION         { get; set; }
}

/// <summary>Cómo se presenta un resultado de la fuente dentro del diseño.</summary>
public class ReporteDisenoSeccion
{
    public int?    NUM_DISENO       { get; set; }
    public int     NUM_RESULTADO    { get; set; }
    public string  COD_PRESENTACION { get; set; } = "TAB";
    public string? DES_TITULO       { get; set; }
    public int     NUM_ORDEN        { get; set; }
    public int?    NUM_COLUMNAS     { get; set; }
}

/// <summary>Una columna de la fuente que el diseño usa: orden, etiqueta, ancho, formato, grupo, total.</summary>
public class ReporteDisenoElemento
{
    public int?    NUM_DISENO          { get; set; }
    public int     NUM_RESULTADO       { get; set; }
    public string  DES_COLUMNA         { get; set; } = "";
    public bool    FLG_VISIBLE         { get; set; } = true;
    public int     NUM_ORDEN           { get; set; }
    public string? DES_ETIQUETA        { get; set; }
    public int?    NUM_ANCHO           { get; set; }
    public string? COD_ALINEACION      { get; set; }
    public string? DES_FORMATO         { get; set; }
    public bool    FLG_TOTALIZA        { get; set; }
    public int     NUM_NIVEL_GRUPO     { get; set; }
    public string? COD_ORDEN           { get; set; }
    public int?    NUM_PRIORIDAD_ORDEN { get; set; }
}

/// <summary>Un formato del diseño: si está habilitado y si tiene plantilla propia (sin los bytes).</summary>
public class ReporteDisenoPlantilla
{
    public int?      NUM_DISENO                { get; set; }
    public string    COD_FORMATO               { get; set; } = "PDF";
    public bool      FLG_HABILITADO            { get; set; }
    public bool      FLG_PLANTILLA_PROPIA      { get; set; }
    public string?   DES_NOMBRE_ARCHIVO        { get; set; }
    public int?      NUM_BYTES                 { get; set; }
    /// <summary>Solo en XLS de un diseño libre: el Excel como el PDF (si no, solo los datos).</summary>
    public bool      FLG_CON_FORMATO           { get; set; }
    public string?   COD_USUARIO_ACTUALIZACION { get; set; }
    public DateTime? FEC_ACTUALIZACION         { get; set; }
}

/// <summary>El diseño completo: es lo que devuelve _SHOWBYID y lo que se manda a grabar.</summary>
public class ReporteDisenoDetalle
{
    public ReporteDiseno                Diseno    { get; set; } = new();
    public List<ReporteDisenoSeccion>   Secciones { get; set; } = new();
    public List<ReporteDisenoElemento>  Elementos { get; set; } = new();
    public List<ReporteDisenoPlantilla> Formatos  { get; set; } = new();
    /// <summary>Solo en los diseños libres (COD_TIPO_DISENO BAN): grupos, bandas y objetos.</summary>
    public ReporteLienzo?               Lienzo    { get; set; }
}

/// <summary>Subir una plantilla propia (.docx o .xlsx) en base64.</summary>
public class ReportePlantillaDto
{
    public string? DES_NOMBRE_ARCHIVO { get; set; }
    public byte[]? CONTENIDO          { get; set; }
}

/// <summary>Qué pasó al subir la plantilla: las marcas que no existen en la fuente.</summary>
public class ReportePlantillaResultado
{
    public int          NUM_MARCAS { get; set; }
    public List<string> Avisos     { get; set; } = new();
}

// ── Diseño libre (lienzo por bandas) ────────────────────────────────────────────
// Códigos:
//   COD_TIPO_BANDA   RH encabezado del informe / PH encabezado de página / GH encabezado de grupo / DE detalle /
//                    GF pie de grupo / RF pie del informe / PF pie de página
//   COD_TIPO_OBJETO  CAM campo / TXT texto / RES resumen / ESP especial / LIN línea / CUA cuadro / IMG imagen /
//                    TAB tabla de un resultado / FOR fórmula (DES_ORIGEN: el nombre de la fórmula)
//   COD_FUNCION      SUM suma / CNT cuenta / CND cuenta distinta / AVG promedio / MAX máximo / MIN mínimo
//   DES_FUENTE       SANS / SERIF / MONO        COD_ALINEACION  I / C / D / J (justificado)
//   DES_BORDES       letras A arriba, B abajo, I izquierda, D derecha
// Medidas en milímetros, desde la esquina superior izquierda de la banda (dentro de los márgenes de la página).

/// <summary>Un grupo del diseño libre: agrupa las filas del resultado 1 por una columna.</summary>
public class ReporteLienzoGrupo
{
    public int     NUM_NIVEL              { get; set; }
    public string  DES_COLUMNA            { get; set; } = "";
    public string  COD_ORDEN              { get; set; } = "A";
    public bool    FLG_MANTENER_JUNTO     { get; set; }
    public bool    FLG_REPETIR_ENCABEZADO { get; set; } = true;
    public bool    FLG_NUEVA_PAGINA       { get; set; }
}

/// <summary>Una sección (banda) del lienzo.</summary>
public class ReporteLienzoBanda
{
    public int     NUM_BANDA                { get; set; }
    public string  COD_TIPO_BANDA           { get; set; } = "DE";
    public int     NUM_NIVEL                { get; set; }
    public int     NUM_SUBSECCION           { get; set; } = 1;
    public decimal NUM_ALTO                 { get; set; }
    public bool    FLG_SUPRIMIR             { get; set; }
    public bool    FLG_SUPRIMIR_EN_BLANCO   { get; set; }
    public bool    FLG_NUEVA_PAGINA_ANTES   { get; set; }
    public bool    FLG_NUEVA_PAGINA_DESPUES { get; set; }
    public bool    FLG_MANTENER_JUNTO       { get; set; } = true;
    public string? DES_COLOR_FONDO          { get; set; }
    /// <summary>Solo en el detalle: el fondo de las filas pares (filas cebra). Vuelve a empezar en cada grupo.</summary>
    public string? DES_COLOR_CEBRA          { get; set; }
}

/// <summary>Un objeto dentro de una banda: campo, texto, resumen, especial, línea, cuadro, imagen, tabla, fórmula o código
/// de barras (BAR).</summary>
public class ReporteLienzoObjeto
{
    public int      NUM_OBJETO            { get; set; }
    public int      NUM_BANDA             { get; set; }
    public string   COD_TIPO_OBJETO       { get; set; } = "TXT";
    public decimal  NUM_X                 { get; set; }
    public decimal  NUM_Y                 { get; set; }
    public decimal  NUM_ANCHO             { get; set; }
    public decimal  NUM_ALTO              { get; set; }
    public int      NUM_ORDEN             { get; set; }
    public int?     NUM_RESULTADO         { get; set; }
    public string?  DES_ORIGEN            { get; set; }
    public string?  COD_FUNCION           { get; set; }
    public int?     NUM_NIVEL             { get; set; }
    public string?  DES_FORMATO           { get; set; }
    public string?  DES_FUENTE            { get; set; }
    public decimal? NUM_TAMANO_LETRA      { get; set; }
    public bool     FLG_NEGRITA           { get; set; }
    public bool     FLG_CURSIVA           { get; set; }
    public bool     FLG_SUBRAYADO         { get; set; }
    public string?  DES_COLOR             { get; set; }
    public string?  DES_COLOR_FONDO       { get; set; }
    public string?  COD_ALINEACION        { get; set; }
    public string?  DES_BORDES            { get; set; }
    public decimal? NUM_GROSOR_BORDE      { get; set; }
    public string?  DES_COLOR_BORDE       { get; set; }
    public bool     FLG_PUEDE_CRECER      { get; set; }
    public bool     FLG_SUPRIMIR_VACIO    { get; set; }
    public bool     FLG_SUPRIMIR_REPETIDO { get; set; }
    public byte[]?  IMG_IMAGEN            { get; set; }
    /// <summary>Una línea vertical o un cuadro que cruza secciones termina en esta sección, a esta altura (mm). null: no cruza.</summary>
    public int?     NUM_BANDA_FIN         { get; set; }
    public decimal? NUM_Y_FIN             { get; set; }
    /// <summary>Texto girado: 0, 90 (se lee de abajo hacia arriba), 180 o 270 (de arriba hacia abajo) grados.</summary>
    public int      NUM_GIRO              { get; set; }
    /// <summary>Una línea vertical o un cuadro que se estira con la sección cuando un texto la hace crecer.</summary>
    public bool     FLG_HASTA_ABAJO       { get; set; }
    /// <summary>En un código (BAR): C128 (Code 128), EAN (EAN-13) o QR.</summary>
    public string?  COD_CODIGO            { get; set; }
    /// <summary>En un código de barras: el texto debajo de las barras (no en QR).</summary>
    public bool     FLG_TEXTO_CODIGO      { get; set; } = true;
    /// <summary>En un subinforme (SUB): el diseño libre que se imprime adentro, con su propia fuente.</summary>
    public int?     NUM_DISENO_SUB        { get; set; }
    /// <summary>Uno por línea. En un subinforme, de dónde sale cada parámetro de su fuente (@PARAMETRO={COLUMNA}); en una
    /// tabla o una tabla cruzada de otro resultado, con qué columnas se queda con las filas de la fila que se imprime
    /// (COLUMNA={COLUMNA_DEL_RESULTADO_1}).</summary>
    public string?  DES_ENLACES           { get; set; }
    /// <summary>En una tabla cruzada (CRU): la columna de las filas y la de las columnas. Lo que resume va en DES_ORIGEN,
    /// con COD_FUNCION. En un gráfico (GRA): la de las categorías y la de las series (opcional).</summary>
    public string?  DES_CRUCE_FILAS       { get; set; }
    public string?  DES_CRUCE_COLUMNAS    { get; set; }
    /// <summary>Gráfico (GRA): COL columnas, BAR barras, LIN líneas, TOR torta, ANI anillo.</summary>
    public string?  COD_GRAFICO           { get; set; }
    /// <summary>Gráfico: las series una sobre otra (columnas y barras).</summary>
    public bool     FLG_APILADO           { get; set; }
    /// <summary>Gráfico: escribe los valores.</summary>
    public bool     FLG_VALORES           { get; set; }
    /// <summary>Gráfico: cuántas categorías se ven; las demás se juntan en «Otros». Null: todas (en una torta, 6).</summary>
    public int?     NUM_MAXIMO_CATEGORIAS { get; set; }
    /// <summary>Gráfico: las categorías de mayor a menor valor (si no, por su nombre o fecha).</summary>
    public bool     FLG_ORDEN_VALOR       { get; set; }
}

/// <summary>Una fórmula del diseño libre. Se usa por su nombre: en un objeto FOR, en un texto ({F.Nombre}), en una
/// condición o dentro de otra fórmula.</summary>
public class ReporteLienzoFormula
{
    public string  DES_NOMBRE      { get; set; } = "";
    public string  DES_FORMULA     { get; set; } = "";
    public string? DES_DESCRIPCION { get; set; }
}

/// <summary>Una propiedad de una sección (COD_ELEMENTO B, NUM_ELEMENTO = NUM_BANDA) o de un objeto (O, NUM_OBJETO) que
/// depende de una fórmula. Sección: SUPRIMIR, FONDO, NUEVA_PAGINA_ANTES, NUEVA_PAGINA_DESPUES. Objeto: SUPRIMIR, NEGRITA,
/// COLOR, FONDO.</summary>
public class ReporteLienzoCondicion
{
    public string COD_ELEMENTO  { get; set; } = "O";
    public int    NUM_ELEMENTO  { get; set; }
    public string COD_PROPIEDAD { get; set; } = "";
    public string DES_FORMULA   { get; set; } = "";
}

/// <summary>El lienzo completo de un diseño libre: grupos, bandas, objetos, fórmulas y condiciones
/// (PROC_REPORTES_DISENOS_LIENZO_*).</summary>
public class ReporteLienzo
{
    public List<ReporteLienzoGrupo>     Grupos      { get; set; } = new();
    public List<ReporteLienzoBanda>     Bandas      { get; set; } = new();
    public List<ReporteLienzoObjeto>    Objetos     { get; set; } = new();
    public List<ReporteLienzoFormula>   Formulas    { get; set; } = new();
    public List<ReporteLienzoCondicion> Condiciones { get; set; } = new();
}

/// <summary>Comprobar una fórmula en el editor (POST disenos/formulas/comprobar).</summary>
public class ReporteFormulaComprobarDto
{
    public int                        NUM_FUENTE    { get; set; }
    public string                     DES_FORMULA   { get; set; } = "";
    /// <summary>El nombre de la fórmula que se edita (para que no se use a sí misma); null en una condición.</summary>
    public string?                    DES_NOMBRE    { get; set; }
    /// <summary>En una condición: B una sección, O un objeto. null: una fórmula.</summary>
    public string?                    COD_ELEMENTO  { get; set; }
    /// <summary>En una condición: SUPRIMIR, NEGRITA, COLOR, FONDO, NUEVA_PAGINA_ANTES, NUEVA_PAGINA_DESPUES.</summary>
    public string?                    COD_PROPIEDAD { get; set; }
    /// <summary>Las fórmulas del diseño tal como están en el editor (todavía sin grabar).</summary>
    public List<ReporteLienzoFormula> Formulas      { get; set; } = new();
    /// <summary>Las columnas de los grupos del lienzo, del nivel 1 al último.</summary>
    public List<string>               Grupos        { get; set; } = new();
    /// <summary>Calcular el valor con la primera fila: ejecuta el SP de la fuente con los valores por defecto.</summary>
    public bool                       FLG_PROBAR    { get; set; }
}

/// <summary>Un error de una fórmula, con su línea y columna (desde 1).</summary>
public class ReporteFormulaError
{
    public int    NUM_LINEA   { get; set; }
    public int    NUM_COLUMNA { get; set; }
    public string DES_MENSAJE { get; set; } = "";
}

/// <summary>Cómo salió la comprobación: los errores, qué tipo de valor da y, si se pidió, el valor con la primera fila.</summary>
public class ReporteFormulaResultado
{
    public bool                      FLG_VALIDA { get; set; }
    public string?                   DES_TIPO   { get; set; }
    public List<ReporteFormulaError> Errores    { get; set; } = new();
    public string?                   DES_VALOR  { get; set; }
    /// <summary>Por qué no se pudo probar con datos (un parámetro obligatorio sin valor por defecto, el SP sin filas...).</summary>
    public string?                   DES_AVISO  { get; set; }
}

/// <summary>Una función del lenguaje de fórmulas, para la ayuda del editor.</summary>
public class ReporteFormulaFuncion
{
    public string DES_GRUPO   { get; set; } = "";
    public string DES_NOMBRE  { get; set; } = "";
    public string DES_FIRMA   { get; set; } = "";
    public string DES_AYUDA   { get; set; } = "";
    public string DES_EJEMPLO { get; set; } = "";
}

// ── Botones ─────────────────────────────────────────────────────────────────────

/// <summary>Un botón de una opción, con el diseño que dispara si lo tiene.</summary>
public class ReporteBoton
{
    public string  COD_APLICACION        { get; set; } = "";
    public string? DES_APLICACION        { get; set; }
    public string  COD_OPCION_APLICACION { get; set; } = "";
    public string? DES_OPCION_APLICACION { get; set; }
    public string? DES_FORMULARIO        { get; set; }
    public string  COD_BOTON_OPCION      { get; set; } = "";
    public string? DES_BOTON_OPCION      { get; set; }
    public string? DES_TOOLTIPTEXT       { get; set; }
    public bool    FLG_HEADER            { get; set; }
    public bool    FLG_GRILLA            { get; set; }
    public bool    FLG_DIALOG            { get; set; }
    public int?    NUM_DISENO            { get; set; }
    public string? DES_DISENO            { get; set; }
    public string? COD_TIPO_DISENO       { get; set; }
    public string? COD_FORMATO_DEFECTO   { get; set; }
    public bool?   FLG_ELEGIR_FORMATO    { get; set; }
    public bool?   FLG_VISTA_PREVIA      { get; set; }
    /// <summary>La ruta de ENTORNO.PATHFILES donde se guarda una copia de cada archivo que genera el botón (null: no se guarda).</summary>
    public string? COD_KEY_PATHFILE      { get; set; }
    /// <summary>El nombre de la copia: texto con marcas {COLUMNA} o {COLUMNA:formato}, sin extensión (null: el de siempre).</summary>
    public string? DES_NOMBRE_ARCHIVO    { get; set; }
    /// <summary>La ruta completa (\\IP del servidor + DES_KEY_PATHFILE), solo para mostrarla.</summary>
    public string? DES_PATH_FILE         { get; set; }
}

/// <summary>Un parámetro del diseño y de dónde sale su valor al usar el botón.</summary>
public class ReporteBotonParametro
{
    public string  DES_PARAMETRO     { get; set; } = "";
    public int?    NUM_ORDEN         { get; set; }
    public string? DES_TIPO_DATO     { get; set; }
    public string? DES_ETIQUETA      { get; set; }
    public string? COD_CONTROL       { get; set; }
    public bool    FLG_OBLIGATORIO   { get; set; }
    public bool    FLG_SISTEMA       { get; set; }
    public bool    FLG_VISIBLE       { get; set; }
    public string? COD_VALOR_DEFECTO { get; set; }
    public string  COD_ORIGEN        { get; set; } = "AUT";
    public string? DES_VALOR         { get; set; }
}

public class ReporteBotonDetalle
{
    public ReporteBoton                Boton      { get; set; } = new();
    public List<ReporteBotonParametro> Parametros { get; set; } = new();
}

/// <summary>Asignar (o cambiar) el diseño de un botón.</summary>
public class ReporteBotonGrabarDto
{
    public string                      COD_APLICACION        { get; set; } = "";
    public string                      COD_OPCION_APLICACION { get; set; } = "";
    public string                      COD_BOTON_OPCION      { get; set; } = "";
    public int?                        NUM_DISENO            { get; set; }
    public string                      COD_FORMATO_DEFECTO   { get; set; } = "PDF";
    public bool                        FLG_ELEGIR_FORMATO    { get; set; }
    public bool                        FLG_VISTA_PREVIA      { get; set; } = true;
    public string?                     COD_KEY_PATHFILE      { get; set; }
    public string?                     DES_NOMBRE_ARCHIVO    { get; set; }
    public List<ReporteBotonParametro> Parametros            { get; set; } = new();
}

/// <summary>Una ruta activa de ENTORNO.PATHFILES, para elegir dónde se guarda la copia.</summary>
public class ReporteRuta
{
    public string  COD_KEY_PATHFILE { get; set; } = "";
    public string? DES_KEY_PATHFILE { get; set; }
    /// <summary>\\IP del servidor + DES_KEY_PATHFILE; null si la ruta no tiene servidor.</summary>
    public string? DES_PATH_FILE    { get; set; }
    public string? DES_KEY_IP       { get; set; }
}

// ── Ejecución ───────────────────────────────────────────────────────────────────

/// <summary>Un botón de la opción que atiende el reporteador (PROC_REPORTES_OPCION_BOTONES).</summary>
public class ReporteOpcionBoton
{
    public string  COD_BOTON_OPCION    { get; set; } = "";
    public int     NUM_DISENO          { get; set; }
    public string? DES_DISENO          { get; set; }
    public string  COD_TIPO_DISENO     { get; set; } = "LIS";
    public string  COD_FORMATO_DEFECTO { get; set; } = "PDF";
    public bool    FLG_ELEGIR_FORMATO  { get; set; }
    public bool    FLG_VISTA_PREVIA    { get; set; } = true;
    public string? DES_FORMATOS        { get; set; }
}



/// <summary>Lo que el navegador necesita para pedir un reporte: el diálogo de filtros y los formatos.
/// No lleva nombres de SP.</summary>
public class ReporteDialogo
{
    public int                         NUM_DISENO          { get; set; }
    public string?                     DES_DISENO          { get; set; }
    public string                      COD_TIPO_DISENO     { get; set; } = "LIS";
    public string?                     DES_TITULO          { get; set; }
    public string                      COD_FORMATO_DEFECTO { get; set; } = "PDF";
    public bool                        FLG_ELEGIR_FORMATO  { get; set; }
    public bool                        FLG_VISTA_PREVIA    { get; set; } = true;
    public List<string>                Formatos            { get; set; } = new();
    public List<ReporteDialogoParametro> Parametros        { get; set; } = new();
}

/// <summary>Un parámetro tal como lo ve el diálogo. FLG_PEDIR: se muestra para que el usuario lo llene.</summary>
public class ReporteDialogoParametro
{
    public string  DES_PARAMETRO     { get; set; } = "";
    public string  DES_CAMPO         { get; set; } = "";
    public string? DES_ETIQUETA      { get; set; }
    public string? DES_TIPO_DATO     { get; set; }
    public string  COD_CONTROL       { get; set; } = "TXT";
    public bool    FLG_OBLIGATORIO   { get; set; }
    public string  COD_ORIGEN        { get; set; } = "AUT";
    /// <summary>El campo de la fila de donde sale (origen FIL); vacío en los demás.</summary>
    public string? DES_CAMPO_FILA    { get; set; }
    public string? COD_VALOR_DEFECTO { get; set; }
    public string? DES_VALOR_DEFECTO { get; set; }
    public bool    FLG_VISIBLE       { get; set; } = true;
}

/// <summary>Pedir un reporte. Se identifica por botón (pantallas) o por diseño (vista previa del diseñador).
/// Valores: lo que el usuario llenó en el diálogo. Contexto: los campos de la fila (o de la pantalla).
/// Textos: lo que se ve de cada valor (el texto del combo), para el pie de filtros. Las claves van sin @.</summary>
public class ReporteGenerarDto
{
    public int?                        NUM_DISENO            { get; set; }
    public string?                     COD_APLICACION        { get; set; }
    public string?                     COD_OPCION_APLICACION { get; set; }
    public string?                     COD_BOTON_OPCION      { get; set; }
    public string?                     COD_ROL_USUARIO       { get; set; }
    public string                      COD_FORMATO           { get; set; } = "PDF";
    public Dictionary<string, string?> Valores               { get; set; } = new();
    public Dictionary<string, string?> Contexto              { get; set; } = new();
    public Dictionary<string, string?> Textos                { get; set; } = new();
}

/// <summary>Pedir los ítems del combo de un parámetro.</summary>
public class ReporteComboDto
{
    public int?                        NUM_DISENO            { get; set; }
    public string?                     COD_APLICACION        { get; set; }
    public string?                     COD_OPCION_APLICACION { get; set; }
    public string?                     COD_BOTON_OPCION      { get; set; }
    public string?                     COD_ROL_USUARIO       { get; set; }
    public string                      DES_PARAMETRO         { get; set; } = "";
    public Dictionary<string, string?> Valores               { get; set; } = new();
    public Dictionary<string, string?> Contexto              { get; set; } = new();
}

/// <summary>El archivo generado. CONTENIDO viaja en base64 dentro del JSON.</summary>
public class ReporteArchivo
{
    public string  DES_NOMBRE_ARCHIVO { get; set; } = "";
    public string  DES_CONTENT_TYPE   { get; set; } = "application/octet-stream";
    public byte[]  CONTENIDO          { get; set; } = Array.Empty<byte>();
    public int     NUM_FILAS          { get; set; }
    /// <summary>Las columnas que el SP devolvió por primera vez y se agregaron a la fuente ("COLUMNA (resultado N)").</summary>
    public List<string> COLUMNAS_NUEVAS { get; set; } = new();
    /// <summary>Si el botón guarda una copia: dónde quedó (con su versión _2, _3... si ya había uno con ese nombre).</summary>
    public string? DES_RUTA_GUARDADA  { get; set; }
    /// <summary>Si el botón guarda una copia y no se pudo: por qué. El archivo sale igual.</summary>
    public string? DES_AVISO_GUARDADO { get; set; }
}

/// <summary>Datos de la empresa para el encabezado (PROC_REPORTES_EMPRESA).</summary>
public class ReporteEmpresa
{
    public string? DES_NOMBRE_EMPRESA      { get; set; }
    public string? NUM_DOCUMENTO_IDENTIDAD { get; set; }
    public string? DES_DOMICILIO_FISCAL    { get; set; }
    public byte[]? IMG_LOGO_EMPRESA        { get; set; }
    /// <summary>El nombre y el logo de la aplicación (Configuración General), para el diseño libre.</summary>
    public string? DES_NOMBRE_APLICACION   { get; set; }
    public byte[]? IMG_LOGO_APLICACION     { get; set; }
    public string? DES_USUARIO             { get; set; }
}
