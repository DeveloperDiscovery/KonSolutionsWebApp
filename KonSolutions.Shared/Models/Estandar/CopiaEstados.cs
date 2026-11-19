namespace KONSolutions.Shared.Models.Estandar;

/// <summary>Copia de la configuracion de estados (flujos por boton y dependencias) de una opcion / tipo de estado a otra.</summary>
public class CopiaConfiguracionEstadosDto
{
    public string COD_APLICACION_ORIGEN { get; set; } = "";
    public string COD_OPCION_ORIGEN { get; set; } = "";
    public short COD_TIPO_ESTADO_ORIGEN { get; set; }
    public string COD_APLICACION_DESTINO { get; set; } = "";
    public string COD_OPCION_DESTINO { get; set; } = "";
    public short COD_TIPO_ESTADO_DESTINO { get; set; }
    /// <summary>true: borra primero la configuracion del destino y deja la del origen. false: respeta lo que el destino ya tiene.</summary>
    public bool REEMPLAZAR { get; set; }
    /// <summary>true (por defecto): solo valida y devuelve el detalle, sin grabar.</summary>
    public bool SIMULAR { get; set; } = true;
}

/// <summary>Una fila del detalle: ASIGNACION, FLUJO o DEPENDENCIA, con ACCION COPIAR / EXISTE / OMITIDO y el motivo.</summary>
public class CopiaConfiguracionEstadosFila
{
    public int SEC { get; set; }
    public string TIPO { get; set; } = "";
    public string? COD_BOTON_OPCION { get; set; }
    public string? DES_BOTON { get; set; }
    public int? COD_ESTADO { get; set; }
    public string? DES_ESTADO { get; set; }
    public int? COD_ESTADO_DEP { get; set; }
    public string? DES_ESTADO_DEP { get; set; }
    public string? DES_ESTADO_DESTINO { get; set; }
    public string ACCION { get; set; } = "";
    public string? MOTIVO { get; set; }
}

/// <summary>Copia de los estados de un tipo de estado a otro.</summary>
public class CopiaEstadosTipoDto
{
    public short COD_TIPO_ESTADO_ORIGEN { get; set; }
    public short COD_TIPO_ESTADO_DESTINO { get; set; }
    /// <summary>true: elimina antes los estados del destino que no esten en uso (los que estan en uso se conservan).</summary>
    public bool ELIMINAR_ANTERIORES { get; set; }
    public bool SIMULAR { get; set; } = true;
}

/// <summary>Una fila del detalle: LADO ORIGEN / DESTINO / AVISO, con ACCION COPIAR / EXISTE / ELIMINAR / CONSERVAR.</summary>
public class CopiaEstadosTipoFila
{
    public int SEC { get; set; }
    public string LADO { get; set; } = "";
    public int? COD_ESTADO { get; set; }
    public string? DES_ESTADO { get; set; }
    public string ACCION { get; set; } = "";
    public string? MOTIVO { get; set; }
}
