namespace KONSolutions.Shared.Models.Logistica;

/// <summary>LOGISTICA.GUIAS_REMISION: la guía de remisión electrónica de un movimiento.
/// La ficha nunca trae el XML ni el CDR (son pesados): sólo dice si existen.</summary>
public class GuiaRemision
{
    public int NUM_GUIA_REMISION { get; set; }
    /// <summary>La guía tal como se escribe: T001-00000017.</summary>
    public string NUM_GUIA { get; set; } = string.Empty;
    public decimal NUM_MOVIMIENTO_ALMACEN { get; set; }
    public string COD_SERIE { get; set; } = string.Empty;
    public int NUM_CORRELATIVO { get; set; }
    public DateTime FEC_HORA_EMISION { get; set; }
    public DateTime? FEC_INICIO_TRASLADO { get; set; }
    public string? COD_MODALIDAD_TRASLADO { get; set; }
    public string? COD_MOTIVO_SUNAT { get; set; }
    public string? COD_TIPO_DOCUMENTO_DESTINATARIO_SUNAT { get; set; }
    public string? NUM_DOCUMENTO_DESTINATARIO { get; set; }
    public string? DES_NOMBRE_DESTINATARIO { get; set; }
    public string? NUM_PLACA_PRINCIPAL { get; set; }
    public decimal? CAN_PESO_BRUTO_TOTAL { get; set; }
    public string? COD_UNIDAD_PESO_SUNAT { get; set; }
    public string? DES_ESTADO { get; set; }
    /// <summary>PEN, ENV, ACE, REC o ANU.</summary>
    public string? COD_ESTADO_ALIAS { get; set; }
    public string? DES_BACKCOLOR { get; set; }
    public string? DES_FORECOLOR { get; set; }
    public bool FLG_VIGENTE { get; set; }
    public int NUM_INTENTOS { get; set; }
    public bool FLG_TIENE_XML { get; set; }
    public bool FLG_TIENE_CDR { get; set; }
    public string? NUM_TICKET { get; set; }
    public string? COD_RESPUESTA_SUNAT { get; set; }
    public string? DES_RESPUESTA_SUNAT { get; set; }
    public DateTime? FEC_ACEPTACION { get; set; }
    public string? COD_USUARIO_REGISTRO { get; set; }
    public DateTime? FEC_REGISTRO { get; set; }
}

/// <summary>Una vez que la guía se mandó a SUNAT (bitácora).</summary>
public class GuiaRemisionEnvio
{
    public int NUM_GUIA_REMISION { get; set; }
    public int NUM_INTENTO { get; set; }
    public DateTime FEC_HORA_ENVIO { get; set; }
    public string? NUM_TICKET { get; set; }
    public DateTime? FEC_HORA_RESPUESTA { get; set; }
    public string? COD_RESPUESTA_SUNAT { get; set; }
    public string? DES_RESPUESTA_SUNAT { get; set; }
}

/// <summary>LOGISTICA.GUIAS_REMISION_SERIES: de dónde sale el número (T001, T002...).</summary>
public class GuiaRemisionSerie
{
    // Auditoria (la muestra PanelAuditoria en el dialogo)
    public string? COD_USUARIO_REGISTRO { get; set; }
    public string? COD_ESTACION_REGISTRO { get; set; }
    public DateTime? FEC_REGISTRO { get; set; }
    public string? COD_USUARIO_ACTUALIZACION { get; set; }
    public string? COD_ESTACION_ACTUALIZACION { get; set; }
    public DateTime? FEC_ACTUALIZACION { get; set; }
    public string COD_SERIE { get; set; } = string.Empty;
    public string? DES_SERIE { get; set; }
    public string? COD_SUCURSAL { get; set; }
    public int NUM_ULTIMO_CORRELATIVO { get; set; }
    public bool FLG_ACTIVA { get; set; } = true;

    // ── Sólo lectura ──
    public string? DES_SUCURSAL { get; set; }
    /// <summary>El número que recibiría la próxima guía de la serie.</summary>
    public string? NUM_SIGUIENTE_GUIA { get; set; }
    public int NUM_GUIAS { get; set; }
}

/// <summary>Una observación del validador previo a la emisión.</summary>
public class GuiaValidacionItem
{
    public string Codigo { get; set; } = string.Empty;
    /// <summary>ERROR impide emitir; AVISO se puede emitir pero conviene revisarlo.</summary>
    public string Gravedad { get; set; } = "ERROR";
    public string Seccion { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public bool EsError => Gravedad == "ERROR";
}

public class GuiaValidacionResultado
{
    public decimal NUM_MOVIMIENTO_ALMACEN { get; set; }
    public bool PuedeEmitir { get; set; }
    public int NumErrores { get; set; }
    public int NumAvisos { get; set; }
    public List<GuiaValidacionItem> Items { get; set; } = new();
    public GuiaRemision? GuiaVigente { get; set; }
}

/// <summary>Lo que se elige al emitir y no sale del movimiento. Se guarda con la guía para que un reintento mande lo mismo.
/// Sólo se cambia mientras la guía está PENDIENTE.</summary>
public class GuiaRemisionOpcionesDto
{
    /// <summary>Traslado en vehículos de categoría M1 o L: SUNAT no exige los datos del vehículo ni del conductor.</summary>
    public bool FLG_TRASLADO_M1L { get; set; }
    /// <summary>Transbordo programado: el traslado cambia de vehículo en el camino.</summary>
    public bool FLG_TRANSBORDO_PROGRAMADO { get; set; }
    /// <summary>Documento relacionado (opcional): va completo (tipo, descripción del tipo y número) o no va.</summary>
    public string? COD_TIPO_DOCUMENTO_RELACIONADO { get; set; }
    public string? DES_TIPO_DOCUMENTO_RELACIONADO { get; set; }
    public string? NUM_DOCUMENTO_RELACIONADO { get; set; }
    public string? NUM_RUC_EMISOR_RELACIONADO { get; set; }

    public bool TieneDocumentoRelacionado =>
        !string.IsNullOrWhiteSpace(COD_TIPO_DOCUMENTO_RELACIONADO) || !string.IsNullOrWhiteSpace(DES_TIPO_DOCUMENTO_RELACIONADO)
        || !string.IsNullOrWhiteSpace(NUM_DOCUMENTO_RELACIONADO) || !string.IsNullOrWhiteSpace(NUM_RUC_EMISOR_RELACIONADO);
}

public class GuiaRemisionOpciones : GuiaRemisionOpcionesDto
{
    public int NUM_GUIA_REMISION { get; set; }
}

/// <summary>Emitir la guía de un movimiento. COD_SERIE es opcional (sin él se usa la serie que corresponde al movimiento).</summary>
public class GuiaRemisionEmitirDto
{
    public decimal NUM_MOVIMIENTO_ALMACEN { get; set; }
    public string? COD_SERIE { get; set; }
    public GuiaRemisionOpcionesDto? OPCIONES { get; set; }
}

/// <summary>El cuerpo (opcional) de Reintentar: las opciones nuevas, si se quieren cambiar.</summary>
public class GuiaRemisionReintentarEmisionDto
{
    public GuiaRemisionOpcionesDto? OPCIONES { get; set; }
}

/// <summary>Cómo terminó una emisión, una consulta o un reintento. La respuesta de SUNAT, aceptada o rechazada, es un resultado y no un error.</summary>
public static class GuiaEmisionResultados
{
    public const string Aceptada = "ACEPTADA";
    public const string Rechazada = "RECHAZADA";
    public const string EnProceso = "EN_PROCESO";
}

public class GuiaRemisionEmisionResultado
{
    public GuiaRemision? GUIA { get; set; }
    /// <summary>ACEPTADA, RECHAZADA o EN_PROCESO.</summary>
    public string COD_RESULTADO { get; set; } = string.Empty;
    public string DES_MENSAJE { get; set; } = string.Empty;
    public string? COD_RESPUESTA_SUNAT { get; set; }
    public string? DES_RESPUESTA_SUNAT { get; set; }
}

/// <summary>Si el movimiento ya no se puede modificar porque su guía está ENVIADA o ACEPTADA.</summary>
public class GuiaRemisionBloqueo
{
    public decimal NUM_MOVIMIENTO_ALMACEN { get; set; }
    public bool FLG_BLOQUEADO { get; set; }
    public string? DES_MENSAJE { get; set; }
}

/// <summary>Un registro de un catálogo al que le falta un dato que la guía necesita. Sólo aparecen los que usan las salidas.</summary>
public class GuiaRemisionCatalogoPendiente
{
    public int NUM_ORDEN { get; set; }
    public string DES_CATALOGO { get; set; } = string.Empty;
    public string? COD_REGISTRO { get; set; }
    public string? DES_REGISTRO { get; set; }
    public string DES_FALTA { get; set; } = string.Empty;
    public string DES_DONDE { get; set; } = string.Empty;
    /// <summary>Cuántas salidas (o líneas, en unidades y productos) lo usan.</summary>
    public int NUM_USOS { get; set; }
    /// <summary>Verdadero: una guía que lo use no se puede emitir. Falso: es un aviso.</summary>
    public bool FLG_IMPIDE_EMITIR { get; set; }
}
