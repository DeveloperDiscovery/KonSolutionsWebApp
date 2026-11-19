using System.ComponentModel.DataAnnotations;

namespace KONSolutions.Shared.Models.Logistica;

// ── Lecturas de etiquetas (RFID y código de barras) en un movimiento de almacén ──────────────────
// Espejo de KonSolutionsApi.Domain.Models.Logistica.MovimientosAlmacenLectura: un campo que está en
// uno solo de los dos modelos se pierde al serializar, sin error.

/// <summary>Una lectura del lector. COD_STICKER_RFID es lo que leyó el lector, sea una etiqueta RFID
/// (más de 100 caracteres) o un código de barras: el SP las distingue.</summary>
public class MovimientosAlmacenLecturaDto
{
    [Required] public decimal NUM_MOVIMIENTO_ALMACEN { get; set; }
    [Required] public string COD_STICKER_RFID { get; set; } = string.Empty;
    public string?  COD_STICKER_UBICACION { get; set; }
    public string?  COD_UBICACION_ALMACEN { get; set; }
    /// <summary>Solo hace falta si el producto se mide (un rollo en KGM): si falta, el SP responde 50708.</summary>
    public decimal? CAN_PRODUCTO { get; set; }
    /// <summary>Solo en la salida por transferencia: la ubicación del almacén destino.</summary>
    public string?  COD_UBICACION_ALMACEN_SINCRONIZADO { get; set; }
    /// <summary>Reenvía una lectura que el control entre movimientos rechazó (50706 / 50707).</summary>
    public bool     FLG_FORZAR { get; set; }
    /// <summary>Línea donde asignar la etiqueta. Sin ella, la lectura ubica o crea la línea sola.
    /// Hace falta para leer en una línea con origen (orden de compra, requerimiento, reserva o ingreso
    /// de una transferencia): ahí la lectura solo asigna etiquetas, no cambia la cantidad.</summary>
    public int?     NUM_SECUENCIA_LINEA { get; set; }
}

/// <summary>Lo que devuelve una lectura. Si se rechazó, solo viene ErrorNumber (el número del error del SP).</summary>
public class MovimientosAlmacenLecturaResultado
{
    public decimal? NUM_MOVIMIENTO_ALMACEN { get; set; }
    public int?     NUM_SECUENCIA { get; set; }
    /// <summary>"RFID" o "CODEBAR": cómo llegó la etiqueta.</summary>
    public string?  COD_TIPO_ETIQUETA { get; set; }
    public string?  COD_PRODUCTO { get; set; }
    public string?  DES_PRODUCTO { get; set; }
    public string?  COD_UNIDAD_MEDIDA { get; set; }
    /// <summary>True si el producto se mide (se pide la cantidad); false si se cuenta por unidad.</summary>
    public bool?    FLG_MEDICION { get; set; }
    public string?  COD_ALMACEN { get; set; }
    public string?  COD_UBICACION_ALMACEN { get; set; }
    public string?  NUM_LOTE_CONTROL { get; set; }
    /// <summary>Lo que sumó esta lectura.</summary>
    public decimal? CAN_LEIDA { get; set; }
    /// <summary>Lo que lleva la línea después de esta lectura (en una línea con origen, lo que pide el documento).</summary>
    public decimal? CAN_PRODUCTO_LINEA { get; set; }
    /// <summary>Lo que lleva etiquetado la línea (suma de sus etiquetas).</summary>
    public decimal? CAN_ETIQUETADA { get; set; }
    /// <summary>True si la línea viene de otro documento y la lectura solo le asignó la etiqueta.</summary>
    public bool?    FLG_LINEA_CON_ORIGEN { get; set; }
    public bool?    FLG_LINEA_NUEVA { get; set; }
    /// <summary>Aviso que no impide la lectura (por ejemplo, salida de una etiqueta que nunca ingresó).</summary>
    public string?  DES_ADVERTENCIA { get; set; }
    /// <summary>Solo cuando la lectura se rechazó.</summary>
    public int?     ErrorNumber { get; set; }
}

/// <summary>Una etiqueta leída en una línea del movimiento (LOGISTICA.MOVIMIENTOS_ALMACEN_DETALLE_CONTEO_RFID).</summary>
public class MovimientosAlmacenEtiqueta
{
    public decimal? NUM_MOVIMIENTO_ALMACEN { get; set; }
    public int?     NUM_SECUENCIA { get; set; }
    public string   COD_STICKER_RFID { get; set; } = string.Empty;
    public string?  COD_PRODUCTO { get; set; }
    public decimal? CAN_PRODUCTO { get; set; }
    public string?  COD_USUARIO_REGISTRO { get; set; }
    public string?  COD_ESTACION_REGISTRO { get; set; }
    public DateTime? FEC_REGISTRO { get; set; }
    public string?  DES_PRODUCTO { get; set; }
    public string?  COD_UNIDAD_MEDIDA { get; set; }
    public string?  COD_ALMACEN { get; set; }
    public string?  COD_UBICACION_ALMACEN { get; set; }
    /// <summary>True si el producto se mide (un rollo en KGM: la etiqueta lleva su peso); false si cada etiqueta cuenta 1.</summary>
    public bool?    FLG_MEDICION { get; set; }
    public string?  DES_ALMACEN { get; set; }
    public string?  DES_UBICACION_ALMACEN { get; set; }
    public string?  DES_UNIDAD_MEDIDA { get; set; }
    // Lo que dice la PROPIA etiqueta según ESTANDAR.ESTRUCTURA_RFID. Aparte de los de la línea (NUM_LOTE_CONTROL…): en una línea
    public string?  NUM_SERIE { get; set; }
    /// <summary>El lote de la LECTURA: el que dice la etiqueta (se guarda en la fila). En una línea que viene de una orden de compra puede
    /// ser distinto al de la línea (NUM_LOTE_CONTROL_LINEA).</summary>
    public string?  NUM_LOTE_CONTROL { get; set; }
    public string?  NUM_LOTE_CONTROL_ENTIDAD { get; set; }
    /// <summary>Proceso y orden de producción que dice la etiqueta.</summary>
    public string?  COD_PROCESO { get; set; }
    public string?  DES_PROCESO { get; set; }
    public int?     NUM_ORDEN_PRODUCCION { get; set; }
    public short?   NUM_ORDEN_PRODUCCION_ITEM { get; set; }
    public byte?    NUM_ORDEN_PRODUCCION_UNIDAD { get; set; }
    /// <summary>Descripción de la orden de producción (la misma que muestra el detalle del movimiento).</summary>
    public string?  DES_ORDEN_PRODUCCION { get; set; }
    /// <summary>El lote de la LÍNEA del movimiento (no el de la lectura).</summary>
    public string?  NUM_LOTE_CONTROL_LINEA { get; set; }

    /// <summary>RFID o CODEBAR. La regla es la misma de los procedimientos de lectura (PROC_LOGISTICA_MOVIMIENTOS_ALMACEN_LEER): más de 100 caracteres es RFID; hasta 100, código de barras.
    /// Se calcula acá (no viaja en la respuesta): la etiqueta guardada ya está sin espacios sobrantes.</summary>
    public string   TIPO_ETIQUETA => (COD_STICKER_RFID ?? string.Empty).Trim().Length > 100 ? "RFID" : "CODEBAR";
}

/// <summary>Cambiar el peso de una etiqueta ya leída.</summary>
public class MovimientosAlmacenEtiquetaUpdateDto
{
    [Required] public decimal NUM_MOVIMIENTO_ALMACEN { get; set; }
    [Required] public int     NUM_SECUENCIA { get; set; }
    [Required] public string  COD_STICKER_RFID { get; set; } = string.Empty;
    [Required] public decimal CAN_PRODUCTO { get; set; }
}

/// <summary>Una línea a la que le falta o le sobra lectura. La lista vacía significa que está todo en
/// orden, o que el tipo de movimiento no exige lectura.</summary>
public class MovimientosAlmacenLecturaPendiente
{
    public int?     NUM_SECUENCIA { get; set; }
    public string?  COD_PRODUCTO { get; set; }
    public string?  DES_PRODUCTO { get; set; }
    public decimal? CAN_PRODUCTO { get; set; }
    public int?     NUM_LECTURAS { get; set; }
    public decimal? CAN_LEIDA { get; set; }
    public string?  DES_PROBLEMA { get; set; }
}
