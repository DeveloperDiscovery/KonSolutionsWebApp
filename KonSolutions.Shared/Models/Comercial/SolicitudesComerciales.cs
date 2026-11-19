using System.ComponentModel.DataAnnotations;

namespace KONSolutions.Shared.Models.Comercial;

/// <summary>Catálogo COMERCIAL.SOLICITUDES_COMERCIALES.</summary>
public class SolicitudesComerciales
{
    public int NUM_SOLICITUD_COMERCIAL { get; set; }
    public string DES_SOLICITUD_COMERCIAL { get; set; } = "";
    public DateTime? FEC_SOLICITUD_COMERCIAL { get; set; }
    public DateTime? FEC_REQUERIDA_ATENCION { get; set; }
    public DateTime? FEC_ASIGNACION_EFECTUADA { get; set; }
    public DateTime? FEC_ATENCION_EFECTUADA { get; set; }
    public string? COD_TIPO_SOLICITUD_COMERCIAL { get; set; }
    public decimal COD_ENTIDAD_CLIENTE { get; set; }
    public int? NUM_TEMPORADA { get; set; }
    public int? NUM_COLECCION { get; set; }
    public decimal? COD_ENTIDAD_RESPONSABLE { get; set; }
    /// <summary>Asesor comercial reasignable vía PROC_COMERCIAL_SOLICITUDES_COMERCIALES_REASIGNAASESOR.</summary>
    public decimal? COD_ENTIDAD_ASESOR_COMERCIAL { get; set; }
    public string? DES_OBSERVACIONES { get; set; }
    public int? COD_TIPO_ESTADO { get; set; }
    public int? COD_ESTADO { get; set; }
    public string COD_USUARIO_REGISTRO { get; set; } = "";
    public string COD_ESTACION_REGISTRO { get; set; } = "";
    public DateTime? FEC_REGISTRO { get; set; }
    public string COD_USUARIO_ACTUALIZACION { get; set; } = "";
    public string COD_ESTACION_ACTUALIZACION { get; set; } = "";
    public DateTime? FEC_ACTUALIZACION { get; set; }

    // Solo lectura (JOINs).
    public string? DES_TIPO_SOLICITUD_COMERICAL { get; set; }
    public string? DES_BACKCOLOR { get; set; }
    public string? DES_ESTADO { get; set; }
    // Alias del estado (ESTANDAR.ESTADOS): 'ANU' deja la solicitud en solo lectura.
    // Ver KONSolutions.Shared.Common.EstadoAlias.
    public string? COD_ESTADO_ALIAS { get; set; }
    public string? DES_FORECOLOR { get; set; }
    public string? DES_NOMBRE_COMPLETO { get; set; }
    public string? DES_NOMBRE_COMPLETO_RESPONSABLE { get; set; }
    public string? DES_NOMBRE_COMPLETO_ASESOR_COMERCIAL { get; set; }
    public string? DES_TEMPORADA { get; set; }
    public string? DES_COLECCION { get; set; }

    // ── Notificación en curso (ENTORNO.NOTIFICACIONES) ───────────────────────
    // Sólo lectura: la escribe el flujo de aprobación, no la pantalla. El documento
    // guarda el número y el resto viene por JOIN, incluidos los colores del estado,
    // que salen de ESTANDAR.ESTADOS como los de cualquier otro estado del sistema.
    public long?    NUM_NOTIFICACION            { get; set; }
    public string?  DES_NOTIFICACION            { get; set; }
    public DateTime? FEC_NOTIFICACION           { get; set; }
    public string?  DES_ESTADO_NOTIFICACION     { get; set; }
    public string?  DES_BACKCOLOR_NOTIFICACION  { get; set; }
    public string?  DES_FORECOLOR_NOTIFICACION  { get; set; }

}

/// <summary>DTO de guardado (PROC_COMERCIAL_SOLICITUDES_COMERCIALES_INSERT es upsert — siempre POST).</summary>
public class SolicitudesComercialesDto
{
    // Sin [Required]: al crear se genera vía IDENTITY (SCOPE_IDENTITY()) dentro del propio SP INSERT.
    public int NUM_SOLICITUD_COMERCIAL { get; set; }
    [Required] public string DES_SOLICITUD_COMERCIAL { get; set; } = "";
    public DateTime? FEC_SOLICITUD_COMERCIAL { get; set; }
    public DateTime? FEC_REQUERIDA_ATENCION { get; set; }
    public DateTime? FEC_ASIGNACION_EFECTUADA { get; set; }
    public DateTime? FEC_ATENCION_EFECTUADA { get; set; }
    public string? COD_TIPO_SOLICITUD_COMERCIAL { get; set; }
    [Required] public decimal COD_ENTIDAD_CLIENTE { get; set; }
    public int? NUM_TEMPORADA { get; set; }
    public int? NUM_COLECCION { get; set; }
    public decimal? COD_ENTIDAD_RESPONSABLE { get; set; }
    public string? DES_OBSERVACIONES { get; set; }
    public int? COD_TIPO_ESTADO { get; set; }
    public int? COD_ESTADO { get; set; }
}

public class SolicitudesComercialesSearchDto
{
    public string? Search { get; set; }
}

/// <summary>DTO para el diálogo "Asignar Responsable" (PROC_COMERCIAL_SOLICITUDES_COMERCIALES_ASIGNARRESPONSABLE).</summary>
public class SolicitudesComercialesAsignarResponsableDto
{
    public int NUM_SOLICITUD_COMERCIAL { get; set; }
    [Required] public string COD_TIPO_SOLICITUD_COMERCIAL { get; set; } = "";
    [Required] public decimal COD_ENTIDAD_RESPONSABLE { get; set; }
    public string? DES_OBSERVACIONES { get; set; }
    [Required] public int COD_TIPO_ESTADO { get; set; }

    // Aplicación/opción desde donde se hace la asignación. El SP las reenvía a
    // PROC_COMERCIAL_SOLICITUDES_COMERCIALES_NOTIFICACION para registrar la notificación
    // contra la pantalla de origen. Las provee la página vía AbrirTabOpcion.
    public string? COD_APLICACION_ORIGEN { get; set; }
    public string? COD_OPCION_APLICACION_ORIGEN { get; set; }
}

/// <summary>DTO para el diálogo "Reasignar Asesor Comercial"
/// (PROC_COMERCIAL_SOLICITUDES_COMERCIALES_REASIGNAASESOR).</summary>
public class SolicitudesComercialesReasignaAsesorDto
{
    public int NUM_SOLICITUD_COMERCIAL { get; set; }
    [Required] public decimal COD_ENTIDAD_ASESOR_COMERCIAL { get; set; }
}

/// <summary>Respuesta de /comercial/solicitudes-comerciales/loadcombos — un solo roundtrip
/// para los combos del diálogo en vez de llamadas separadas.</summary>
public class SolicitudesComercialesLoadCombosResult
{
    public int CodTipoEstado { get; set; }
    public List<KONSolutions.Shared.Common.ComboboxItem> TiposSolicitud { get; set; } = new();
    public List<KONSolutions.Shared.Common.ComboboxItem> Clientes { get; set; } = new();
    public List<KONSolutions.Shared.Common.EstadoItem> Estados { get; set; } = new();
    /// <summary>Entidades (todas) — antes se traía con una llamada HTTP aparte a
    /// EstandarRoute.EntidadesCombobox; ahora viene en el mismo LOADCOMBOS.</summary>
    public List<KONSolutions.Shared.Common.ComboboxItem> Entidades { get; set; } = new();
}
