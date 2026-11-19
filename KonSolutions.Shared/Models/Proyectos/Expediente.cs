using System.ComponentModel.DataAnnotations;
namespace KONSolutions.Shared.Models.Proyectos;

// Modelos del expediente del proyecto. Los nombres de las propiedades son los de las columnas que devuelven los
// procedimientos PROC_PROYECTOS_EXPEDIENTE_*, PROC_PROYECTOS_PORTAFOLIO y PROC_PROYECTOS_MIS_TAREAS.

/// <summary>Los campos de estado (el par del estándar, su alias, descripción y colores) que traen casi todas las filas.</summary>
public class ExpedienteEstado
{
    public int? COD_TIPO_ESTADO { get; set; }
    public int? COD_ESTADO { get; set; }
    public string? COD_ESTADO_ALIAS { get; set; }
    public string? DES_ESTADO { get; set; }
    public string? DES_BACKCOLOR { get; set; }
    public string? DES_FORECOLOR { get; set; }
}

public class ExpedienteFase : ExpedienteEstado
{
    // Auditoria (la muestra PanelAuditoria en el dialogo)
    public string? COD_USUARIO_REGISTRO { get; set; }
    public string? COD_ESTACION_REGISTRO { get; set; }
    public DateTime? FEC_REGISTRO { get; set; }
    public string? COD_USUARIO_ACTUALIZACION { get; set; }
    public string? COD_ESTACION_ACTUALIZACION { get; set; }
    public DateTime? FEC_ACTUALIZACION { get; set; }
    public int NUM_PROYECTO { get; set; }
    public int COD_FASE { get; set; }
    public string? DES_FASE { get; set; }
    public int? COD_FASE_PADRE { get; set; }
    public int? COD_HITO_PROYECTO { get; set; }
    public DateTime? FEC_INICIO_PROGRAMADO { get; set; }
    public DateTime? FEC_FINAL_PROGRAMADO { get; set; }
    public DateTime? FEC_INICIO_EJECUTADO { get; set; }
    public DateTime? FEC_FINAL_EJECUTADO { get; set; }
    public decimal? COD_ENTIDAD_RESPONSABLE { get; set; }
    public string? DES_RESPONSABLE { get; set; }
    public string? DES_OBSERVACION { get; set; }
    public int NUM_TAREAS { get; set; }
    public int? NUM_TAREAS_TERMINADAS { get; set; }
    public decimal? TAS_AVANCE { get; set; }
    public decimal? VAL_PRESUPUESTO { get; set; }
    /// <summary>TER terminada, ATR atrasada, CUR en curso, PEN pendiente (se calcula, no se guarda).</summary>
    public string COD_SITUACION { get; set; } = "PEN";
}

public class ExpedienteTarea : ExpedienteEstado
{
    // Auditoria (la muestra PanelAuditoria en el dialogo)
    public string? COD_USUARIO_REGISTRO { get; set; }
    public string? COD_ESTACION_REGISTRO { get; set; }
    public DateTime? FEC_REGISTRO { get; set; }
    public string? COD_USUARIO_ACTUALIZACION { get; set; }
    public string? COD_ESTACION_ACTUALIZACION { get; set; }
    public DateTime? FEC_ACTUALIZACION { get; set; }
    public int NUM_PROYECTO { get; set; }
    public int COD_TAREA { get; set; }
    public string? DES_TAREA { get; set; }
    public int? COD_FASE { get; set; }
    public DateTime? FEC_INICIO_PROGRAMADO { get; set; }
    public DateTime? FEC_FINAL_PROGRAMADO { get; set; }
    public DateTime? FEC_INICIO_EJECUTADO { get; set; }
    public DateTime? FEC_FINAL_EJECUTADO { get; set; }
    public bool? FLG_CONTROL_EJECUCION_MANUAL { get; set; }
    public bool? FLG_REQUIERE_CONFIRMACION { get; set; }
    public decimal? COD_ENTIDAD_CONFIRMADORA { get; set; }
    public string? DES_CONFIRMADORA { get; set; }
    public string? COD_UNIDAD_MEDIDA_TAREAS { get; set; }
    public string? DES_UNIDAD { get; set; }
    public decimal? CAN_PROGRAMADAS { get; set; }
    public decimal? CAN_EJECUTADAS { get; set; }
    public decimal? TAS_EJECUCION { get; set; }
    public decimal? COD_ENTIDAD_RESPONSABLE { get; set; }
    public string? DES_RESPONSABLE { get; set; }
    public decimal? VAL_PRESUPUESTO { get; set; }
    public int NUM_SUBTAREAS { get; set; }
    public int? NUM_SUBTAREAS_TERMINADAS { get; set; }
    /// <summary>TER terminada, POR por confirmar, ATR atrasada, CUR en curso, PEN pendiente.</summary>
    public string COD_SITUACION { get; set; } = "PEN";
}

public class ExpedienteSubTarea
{
    public int NUM_PROYECTO { get; set; }
    public int COD_TAREA { get; set; }
    public int COD_SUB_TAREA { get; set; }
    public string? DES_SUB_TAREA { get; set; }
    public bool? FLG_TERMINADA { get; set; }
}

public class ExpedienteHito : ExpedienteEstado
{
    public int NUM_PROYECTO { get; set; }
    public int COD_HITO_PROYECTO { get; set; }
    public string? DES_HITO_PROYECTO { get; set; }
    public DateTime? FEC_FINAL_PROGRAMADO { get; set; }
    public DateTime? FEC_INICIO_EJECUTADO { get; set; }
}

public class ExpedientePlanificacion
{
    public List<ExpedienteFase> Fases { get; set; } = new();
    public List<ExpedienteTarea> Tareas { get; set; } = new();
    public List<ExpedienteSubTarea> SubTareas { get; set; } = new();
    public List<ExpedienteHito> Hitos { get; set; } = new();
}

public class ExpedienteRecurso : ExpedienteEstado
{
    public int NUM_PROYECTO { get; set; }
    public int COD_TAREA { get; set; }
    public string? DES_TAREA { get; set; }
    public decimal COD_ENTIDAD_ASIGNADA { get; set; }
    public string? DES_ENTIDAD { get; set; }
    public string COD_ROL_TAREA { get; set; } = "";
    public string? DES_ROL_TAREA { get; set; }
    public DateTime? FEC_INICIO_PROGRAMADO { get; set; }
    public DateTime? FEC_FINAL_PROGRAMADO { get; set; }
    public DateTime? FEC_INICIO_EJECUTADO { get; set; }
    public DateTime? FEC_FINAL_EJECUTADO { get; set; }
    public decimal? NUM_HORAS_PROGRAMADAS { get; set; }
    public decimal? NUM_HORAS_EFECTIVAS { get; set; }
    public decimal? VAL_COSTO_HORA { get; set; }
    /// <summary>Utilidad en % sobre el costo; solo para personal propio de la empresa.</summary>
    public decimal? TAS_UTILIDAD { get; set; }
    public bool FLG_PROPIO { get; set; }
    /// <summary>Horas efectivas x costo por hora.</summary>
    public decimal VAL_COSTO { get; set; }
    public decimal VAL_UTILIDAD { get; set; }
    /// <summary>Costo + utilidad.</summary>
    public decimal VAL_TOTAL { get; set; }
}

public class ExpedienteCostoResumen
{
    public int NUM_PROYECTO { get; set; }
    public string? COD_MONEDA_VALORIZACION { get; set; }
    public decimal? VAL_PRESUPUESTO_PROYECTO { get; set; }
    public decimal VAL_PRESUPUESTO_TAREAS { get; set; }
    public decimal VAL_PRESUPUESTO_MATERIALES { get; set; }
    public decimal VAL_PRESUPUESTO_RECURSOS { get; set; }
    /// <summary>Suma del presupuesto de las tablas hijas con costo (tareas + materiales + recursos + gastos adicionales).</summary>
    public decimal VAL_PRESUPUESTO_HIJOS { get; set; }
    public decimal VAL_GASTO_ASIGNADO { get; set; }
    public decimal VAL_MATERIAL_CONSUMIDO { get; set; }
    public decimal VAL_MANO_OBRA { get; set; }
    public decimal VAL_UTILIDAD_PROPIOS { get; set; }
    public decimal VAL_ADICIONAL_PRESUPUESTADO { get; set; }
    public decimal VAL_COSTO_REAL { get; set; }
    public decimal VAL_DISPONIBLE { get; set; }
    public decimal? VAL_COSTO_PROYECTO { get; set; }
    public bool? FLG_PRESUPUESTO { get; set; }
    public bool? FLG_VALORADO { get; set; }
    public bool? FLG_PRODUCCION { get; set; }
    public bool? FLG_SERVICIO { get; set; }
}

public class ExpedienteCostoFase
{
    public int? COD_FASE { get; set; }
    public string? DES_FASE { get; set; }
    public decimal VAL_PRESUPUESTO { get; set; }
    public decimal VAL_GASTO { get; set; }
    public decimal VAL_ADICIONAL { get; set; }
}

public class ExpedienteGasto
{
    public int NUM_PROYECTO { get; set; }
    public int NUM_SECUENCIA { get; set; }
    public int? COD_FASE { get; set; }
    public string? DES_FASE { get; set; }
    public int? COD_TAREA { get; set; }
    public string? DES_TAREA { get; set; }
    public decimal? NUM_REGISTRO_COMPROBANTE { get; set; }
    public string? COD_TIPO_COMPROBANTE_PAGO { get; set; }
    public string? NUM_COMPROBANTE_PAGO { get; set; }
    public decimal? COD_ENTIDAD { get; set; }
    public string? DES_ENTIDAD { get; set; }
    public string? COD_MONEDA { get; set; }
    public decimal? VAL_TOTAL { get; set; }
    public decimal? VAL_GASTO_ASIGNADO { get; set; }
}

public class ExpedienteComprobante
{
    public decimal NUM_REGISTRO { get; set; }
    public string? COD_TIPO_COMPROBANTE_PAGO { get; set; }
    public string? NUM_COMPROBANTE_PAGO { get; set; }
    public decimal? COD_ENTIDAD { get; set; }
    public string? DES_ENTIDAD { get; set; }
    public string? COD_MONEDA { get; set; }
    public decimal? VAL_TOTAL { get; set; }
    public decimal VAL_ASIGNADO { get; set; }
    public decimal VAL_SIN_ASIGNAR { get; set; }
    /// <summary>Valor de la factura sin impuestos (IMTOT de COMPROBANTES_PAGO_CALCULO).</summary>
    public decimal VAL_SIN_IMPUESTOS { get; set; }
    /// <summary>Lo que esta factura suma al costo del proyecto.</summary>
    public decimal VAL_COSTO_PROYECTO { get; set; }
}

public class ExpedienteCostos
{
    public ExpedienteCostoResumen? Resumen { get; set; }
    public List<ExpedienteCostoFase> PorFase { get; set; } = new();
    public List<ExpedienteGasto> Gastos { get; set; } = new();
    public List<ExpedienteComprobante> Comprobantes { get; set; } = new();
    public List<ExpedienteGastoAdicional> Adicionales { get; set; } = new();
}

public class ExpedienteGastoAdicional
{
    public int NUM_PROYECTO { get; set; }
    public int NUM_SECUENCIA { get; set; }
    /// <summary>Nulo = proyecto principal.</summary>
    public int? COD_FASE { get; set; }
    public string? DES_FASE { get; set; }
    public string DES_GASTO { get; set; } = "";
    public decimal VAL_PRESUPUESTADO { get; set; }
    public string? DES_OBSERVACION { get; set; }
}

public class ExpedienteRiesgo : ExpedienteEstado
{
    public int NUM_PROYECTO { get; set; }
    public int NUM_SECUENCIA { get; set; }
    public string? DES_RIESGO { get; set; }
    public int? COD_PROBABILIDAD_RIESGO { get; set; }
    public string? DES_PROBABILIDAD { get; set; }
    public decimal? VAL_PROBABILIDAD { get; set; }
    public int? COD_IMPACTO_RIESGO { get; set; }
    public string? DES_IMPACTO { get; set; }
    public decimal? VAL_IMPACTO { get; set; }
    public decimal VAL_NIVEL { get; set; }
    /// <summary>ALT desde 0.40, MED desde 0.15, BAJ debajo (probabilidad × impacto).</summary>
    public string COD_NIVEL { get; set; } = "BAJ";
    public string? COD_PLAN_MITIGACION { get; set; }
    public string? DES_PLAN_MITIGACION { get; set; }
    public int? COD_FASE { get; set; }
    public string? DES_FASE { get; set; }
    public int? COD_TAREA { get; set; }
    public string? DES_TAREA { get; set; }
}

public class ExpedientePlan : ExpedienteEstado
{
    public int NUM_PROYECTO { get; set; }
    public string COD_PLAN_MITIGACION { get; set; } = "";
    public string? DES_PLAN_MITIGACION { get; set; }
    public int NUM_RIESGOS { get; set; }
}

public class ExpedienteRiesgos
{
    public List<ExpedienteRiesgo> Riesgos { get; set; } = new();
    public List<ExpedientePlan> Planes { get; set; } = new();
}

public class ExpedienteArchivo
{
    public int NUM_PROYECTO { get; set; }
    public int NUM_SECUENCIA { get; set; }
    public int? COD_FASE { get; set; }
    public string? DES_FASE { get; set; }
    public int? COD_TAREA { get; set; }
    public string? DES_TAREA { get; set; }
    public string? DES_PATH_FILE { get; set; }
    public string? COD_USUARIO_REGISTRO { get; set; }
    public DateTime? FEC_REGISTRO { get; set; }
}

public class ExpedienteMemoria
{
    public int NUM_PROYECTO { get; set; }
    public int NUM_SECUENCIA { get; set; }
    public int? COD_FASE { get; set; }
    public string? DES_FASE { get; set; }
    public int? COD_TAREA { get; set; }
    public string? DES_TAREA { get; set; }
    public string? DES_MEMORIA { get; set; }
    public int? NUM_SECUENCIA_EVENTO { get; set; }
    public string? DES_EVENTO { get; set; }
    public bool? FLG_RETRASO { get; set; }
    public string? COD_USUARIO_REGISTRO { get; set; }
    public DateTime? FEC_REGISTRO { get; set; }
    public string? COD_USUARIO_ACTUALIZACION { get; set; }
    public DateTime? FEC_ACTUALIZACION { get; set; }
}

public class ExpedienteDocumentos
{
    public List<ExpedienteArchivo> Archivos { get; set; } = new();
    public List<ExpedienteMemoria> Memoria { get; set; } = new();
}

public class ExpedienteMaterial
{
    public int NUM_PROYECTO { get; set; }
    public int NUM_SECUENCIA { get; set; }
    public int? COD_FASE { get; set; }
    public string? DES_FASE { get; set; }
    public int? COD_TAREA { get; set; }
    public string? DES_TAREA { get; set; }
    public string COD_PRODUCTO { get; set; } = "";
    public string? DES_PRODUCTO { get; set; }
    public decimal? CAN_PROGRAMADA { get; set; }
    public decimal? VAL_COSTO_UNITARIO { get; set; }
    public string? DES_OBSERVACION { get; set; }
    public decimal CAN_CONSUMIDA { get; set; }
    public decimal? TAS_CONSUMO { get; set; }
    public decimal VAL_COSTO_CONSUMIDO { get; set; }
}

public class ExpedienteConsumoSinPlan
{
    public string COD_PRODUCTO { get; set; } = "";
    public string? DES_PRODUCTO { get; set; }
    public decimal CAN_CONSUMIDA { get; set; }
    public int NUM_MOVIMIENTOS { get; set; }
}

public class ExpedienteMovimiento
{
    public decimal NUM_MOVIMIENTO_ALMACEN { get; set; }
    public DateTime? FEC_MOVIMENTO_ALMACEN { get; set; }
    public string COD_PRODUCTO { get; set; } = "";
    public string? DES_PRODUCTO { get; set; }
    public string? COD_CLASE_MOVIMIENTO { get; set; }
    public string? COD_TIPO_MOVIMIENTO { get; set; }
    public decimal CAN_SIGNADA { get; set; }
}

public class ExpedienteMateriales
{
    public List<ExpedienteMaterial> Plan { get; set; } = new();
    public List<ExpedienteConsumoSinPlan> SinPlan { get; set; } = new();
    public List<ExpedienteMovimiento> Movimientos { get; set; } = new();
}

public class ExpedienteResumen : ExpedienteEstado
{
    public int NUM_PROYECTO { get; set; }
    public string? DES_PROYECTO { get; set; }
    public string? COD_TIPO_PROYECTO { get; set; }
    public string? DES_TIPO_PROYECTO { get; set; }
    public string? COD_MONEDA_VALORIZACION { get; set; }
    public bool FLG_CERRADO { get; set; }
    public bool FLG_PRODUCCION { get; set; }
    public bool FLG_PRESUPUESTO { get; set; }
    public bool FLG_VALORADO { get; set; }
    public bool FLG_SERVICIO { get; set; }
    public int NUM_TAREAS { get; set; }
    public int NUM_TAREAS_TERMINADAS { get; set; }
    public int NUM_TAREAS_ATRASADAS { get; set; }
    public decimal TAS_AVANCE { get; set; }
    public DateTime? FEC_INICIO_PROGRAMADO { get; set; }
    public DateTime? FEC_FINAL_PROGRAMADO { get; set; }
    public DateTime? FEC_INICIO_EJECUTADO { get; set; }
    public DateTime? FEC_FINAL_EJECUTADO { get; set; }
    public decimal? VAL_PRESUPUESTO_PROYECTO { get; set; }
    public decimal? VAL_COSTO_PROYECTO { get; set; }
}

public class ExpedienteTipoEntidad
{
    public string COD_TIPO_ENTIDAD { get; set; } = "";
    public string? DES_TIPO_ENTIDAD { get; set; }
    public bool FLG_ELEGIDO { get; set; }
}

public class PortafolioProyecto : ExpedienteEstado
{
    public int NUM_PROYECTO { get; set; }
    public string? DES_PROYECTO { get; set; }
    public string? COD_TIPO_PROYECTO { get; set; }
    public string? DES_TIPO_PROYECTO { get; set; }
    public string? COD_MONEDA_VALORIZACION { get; set; }
    public decimal? COD_ENTIDAD_RESPONSABLE { get; set; }
    public string? DES_RESPONSABLE { get; set; }
    public DateTime? FEC_INICIO_PROGRAMADO { get; set; }
    public DateTime? FEC_FINAL_PROGRAMADO { get; set; }
    public DateTime? FEC_INICIO_EJECUTADO { get; set; }
    public DateTime? FEC_FINAL_EJECUTADO { get; set; }
    public decimal? VAL_PRESUPUESTO_PROYECTO { get; set; }
    public decimal? VAL_COSTO_PROYECTO { get; set; }
    public int NUM_TAREAS { get; set; }
    public int NUM_TAREAS_TERMINADAS { get; set; }
    public int NUM_TAREAS_ATRASADAS { get; set; }
    public decimal TAS_AVANCE { get; set; }
    public int NUM_RIESGOS_ALTOS { get; set; }
    public string COD_SITUACION { get; set; } = "PEN";
}

public class MiTarea
{
    public int NUM_PROYECTO { get; set; }
    public string? DES_PROYECTO { get; set; }
    public int COD_TAREA { get; set; }
    public string? DES_TAREA { get; set; }
    public int? COD_FASE { get; set; }
    public string? DES_FASE { get; set; }
    public DateTime? FEC_INICIO_PROGRAMADO { get; set; }
    public DateTime? FEC_FINAL_PROGRAMADO { get; set; }
    public DateTime? FEC_INICIO_EJECUTADO { get; set; }
    public DateTime? FEC_FINAL_EJECUTADO { get; set; }
    public decimal? CAN_PROGRAMADAS { get; set; }
    public decimal? CAN_EJECUTADAS { get; set; }
    public string? DES_UNIDAD { get; set; }
    public decimal? TAS_EJECUCION { get; set; }
    public bool? FLG_CONTROL_EJECUCION_MANUAL { get; set; }
    public bool? FLG_REQUIERE_CONFIRMACION { get; set; }
    public decimal? COD_ENTIDAD_CONFIRMADORA { get; set; }
    public string? DES_ROLES { get; set; }
    public bool FLG_POR_CONFIRMAR { get; set; }
    public string COD_SITUACION { get; set; } = "PEN";
}

// ───────── pedidos de acción ─────────

public class ExpedienteMoverDto
{
    /// <summary>FAS fase, TAR tarea, HIT hito.</summary>
    [Required] public string Tipo { get; set; } = "";
    public int Cod { get; set; }
    public DateTime? FecInicio { get; set; }
    public DateTime FecFinal { get; set; }
    public bool MoverHijos { get; set; } = true;
    /// <summary>M mover (la fase lleva sus tareas) o R redimensionar (cambia inicio o fin; las tareas no se mueven).</summary>
    public string Modo { get; set; } = "M";
}

public class ExpedienteMovido
{
    public string TIPO { get; set; } = "";
    public int COD { get; set; }
    public DateTime? FEC_INICIO { get; set; }
    public DateTime? FEC_FINAL { get; set; }
}

public class ExpedienteAvanceDto
{
    public decimal? CanEjecutadas { get; set; }
    public decimal? TasEjecucion { get; set; }
    /// <summary>Fecha real del avance (inicio/fin reales que genere). Opcional: por omisión, hoy. No puede ser futura.</summary>
    public DateTime? FecReal { get; set; }
}

public class ExpedienteAvanceResult
{
    public int NUM_PROYECTO { get; set; }
    public int COD_TAREA { get; set; }
    public decimal? CAN_PROGRAMADAS { get; set; }
    public decimal? CAN_EJECUTADAS { get; set; }
    public decimal? TAS_EJECUCION { get; set; }
    public DateTime? FEC_INICIO_EJECUTADO { get; set; }
    public DateTime? FEC_FINAL_EJECUTADO { get; set; }
    public int? COD_ESTADO { get; set; }
    public string? COD_ESTADO_ALIAS { get; set; }
    public string? DES_ESTADO { get; set; }
    public bool FLG_POR_CONFIRMAR { get; set; }
}

public class ExpedienteRecursoCostoDto
{
    public int CodTarea { get; set; }
    public decimal CodEntidad { get; set; }
    public string CodRol { get; set; } = "";
    public decimal? HorasProgramadas { get; set; }
    public decimal? HorasEfectivas { get; set; }
    public decimal? CostoHora { get; set; }
    /// <summary>Utilidad en % sobre el costo. Solo se acepta para personal propio.</summary>
    public decimal? Utilidad { get; set; }
}

public class ExpedienteTipoPropio
{
    public string COD_TIPO_ENTIDAD { get; set; } = "";
    public string? DES_TIPO_ENTIDAD { get; set; }
    public bool FLG_PERSONAL_PROPIO { get; set; }
}

/// <summary>Un evento del proyecto (fila de PROYECTOS_EVENTOS): si fue un retraso o no, desde/hasta que fecha y hora, y el responsable.</summary>
public class ExpedienteEvento
{
    public int NUM_PROYECTO { get; set; }
    public int NUM_SECUENCIA { get; set; }
    public int? COD_FASE { get; set; }
    public string? DES_FASE { get; set; }
    public int? COD_TAREA { get; set; }
    public string? DES_TAREA { get; set; }
    public string DES_EVENTO { get; set; } = "";
    public bool FLG_RETRASO { get; set; }
    public DateTime FEC_HORA_INICIO { get; set; }
    public DateTime? FEC_HORA_FINAL { get; set; }
    /// <summary>Duracion en horas (null mientras no tenga fecha final).</summary>
    public decimal? HORAS { get; set; }
    public decimal? COD_ENTIDAD_RESPONSABLE { get; set; }
    public string? DES_RESPONSABLE { get; set; }
    public int NUM_MEMORIAS { get; set; }
    public string? DES_OBSERVACION { get; set; }
}

/// <summary>Resultado de cambiar el estado de una fase o tarea con un boton de dialogo.</summary>
public class ExpedienteEstadoResult : ExpedienteEstado { }

public class ExpedienteConfirmarDto
{
    public bool Aprobar { get; set; }
    /// <summary>La persona que confirma (debe ser la confirmadora de la tarea, si la tarea tiene una).</summary>
    public decimal? CodEntidad { get; set; }
    /// <summary>Fecha real de la confirmación (fin real de la tarea). Opcional: por omisión, hoy.</summary>
    public DateTime? FecReal { get; set; }
}

public class ExpedienteGastoDto
{
    [Required] public decimal NumRegistroComprobante { get; set; }
    public int? CodFase { get; set; }
    public int? CodTarea { get; set; }
    [Required] public decimal Valor { get; set; }
}

public class ExpedienteTiposEntidadDto
{
    public List<string> Tipos { get; set; } = new();
}

public class ExpedienteEstadoHistorial
{
    public int NUM_SECUENCIA { get; set; }
    public int? COD_ESTADO_ANTERIOR { get; set; }
    public string? DES_ESTADO_ANTERIOR { get; set; }
    public int COD_ESTADO_NUEVO { get; set; }
    public string? DES_ESTADO_NUEVO { get; set; }
    public string? DES_BOTON { get; set; }
    public string? DES_OBSERVACION { get; set; }
    public string? COD_USUARIO_REGISTRO { get; set; }
    public DateTime? FEC_REGISTRO { get; set; }
}

public class ExpedienteCargoArea
{
    public string? DES_CARGO { get; set; }
    public string? DES_AREA { get; set; }
}

public class ExpedienteOrganigramaNodo
{
    public string COD_ROL_TAREA { get; set; } = "";
    public string? DES_ROL_TAREA { get; set; }
    public string? COD_CATEGORIA_ROL { get; set; }
    public string? DES_CATEGORIA_ROL { get; set; }
    public string? COD_ROL_PADRE_EFECTIVO { get; set; }
    /// <summary>Nombres separados por |</summary>
    public string? PERSONAS { get; set; }
    /// <summary>"Nombre: cargo / area" separados por |</summary>
    public string? CARGOS { get; set; }
}

public class ExpedienteEstadoIcono
{
    public int COD_TIPO_ESTADO { get; set; }
    public int COD_ESTADO { get; set; }
    public byte[]? IMG_PICTURE { get; set; }
}

public class ExpedienteCompromiso
{
    public int NUM_PROYECTO { get; set; }
    public int NUM_SECUENCIA { get; set; }
    public string COD_TIPO_DOCUMENTO_COMPROMISO { get; set; } = "";
    public string? DES_TIPO_DOCUMENTO_COMPROMISO { get; set; }
    public string? COD_CLASE_COMPROMISO { get; set; }
    public int? COD_FASE { get; set; }
    public string? DES_FASE { get; set; }
    public int? COD_TAREA { get; set; }
    public string? DES_TAREA { get; set; }
    public string? DES_DOCUMENTO { get; set; }
    public int? NUM_SECUENCIA_ARCHIVO { get; set; }
    public string? DES_PATH_FILE { get; set; }
    public string COD_ESTADO_COMPROMISO { get; set; } = "PEN";
    public string? DES_ESTADO_COMPROMISO { get; set; }
    /// <summary>Nombres separados por |</summary>
    public string? RESPONSABLES { get; set; }
    public string? APROBADORES { get; set; }
    public int NUM_APROBADORES { get; set; }
    public int NUM_APROBADOS { get; set; }
    public string? COD_USUARIO_REGISTRO { get; set; }
    public DateTime? FEC_REGISTRO { get; set; }
    public string? COD_USUARIO_ACTUALIZACION { get; set; }
    public DateTime? FEC_ACTUALIZACION { get; set; }
}

public class ExpedienteCompromisoPersona
{
    public decimal COD_ENTIDAD { get; set; }
    public string? DES_NOMBRE_COMPLETO { get; set; }
    /// <summary>RES responsable, APR aprobador</summary>
    public string COD_FUNCION_COMPROMISO { get; set; } = "";
    public string? COD_DECISION_COMPROMISO { get; set; }
    public string? DES_OBSERVACION { get; set; }
    public DateTime? FEC_DECISION { get; set; }
    public string? COD_USUARIO_DECISION { get; set; }
}

public class ExpedienteCompromisoDecision
{
    public int NUM_DECISION { get; set; }
    public decimal COD_ENTIDAD { get; set; }
    public string? DES_NOMBRE_COMPLETO { get; set; }
    public string COD_DECISION_COMPROMISO { get; set; } = "";
    public string? DES_DECISION { get; set; }
    public string? DES_OBSERVACION { get; set; }
    public bool FLG_VIGENTE { get; set; }
    public string? COD_USUARIO_REGISTRO { get; set; }
    public DateTime? FEC_REGISTRO { get; set; }
}

public class ExpedienteCompromisoDto
{
    /// <summary>Vacio o 0 = documento nuevo.</summary>
    public int? NumSecuencia { get; set; }
    public string CodTipoDocumento { get; set; } = "";
    public int? CodFase { get; set; }
    public int? CodTarea { get; set; }
    public string? DesDocumento { get; set; }
    /// <summary>Secuencia del archivo ya subido en PROYECTOS_ARCHIVOS.</summary>
    public int? NumSecuenciaArchivo { get; set; }
    public List<decimal> Responsables { get; set; } = new();
    public List<decimal> Aprobadores { get; set; } = new();
}

public class ExpedienteCompromisoDecidirDto
{
    public decimal CodEntidad { get; set; }
    /// <summary>APR, OBS o REC</summary>
    public string CodDecision { get; set; } = "";
    public string? Observacion { get; set; }
}

public class ExpedienteCompromisoEstadoResult
{
    public string COD_ESTADO_COMPROMISO { get; set; } = "";
}
