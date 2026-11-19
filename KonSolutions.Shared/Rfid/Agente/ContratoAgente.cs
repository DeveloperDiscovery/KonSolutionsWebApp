namespace KONSolutions.Shared.Rfid.Agente;

// ═══════════════════════════════════════════════════════════════════════════════════════════════════════════
//  CONTRATO DEL AGENTE LOCAL DE EQUIPOS RFID ("RFID Bridge")
//
//  El navegador (Blazor WASM) no puede hablar con una impresora RFID ni con una antena de escritura de la red del almacén. Para
//  eso hay un programa pequeño, el AGENTE, que corre en la estación de la planta (la que dice la ficha técnica del equipo, RfidEquipo.COD_ESTACION_AGENTE)
//  y hace de puente entre la API y el equipo. Este archivo es SOLO EL CONTRATO: las clases y la interfaz que tendrán que
//  respetar el agente y los adaptadores por fabricante (Impinj, Invengo, Chainway, Zebra...). El agente y los adaptadores todavía
//  NO existen; ver CONTRATO_AGENTE.md en la entrega para el flujo completo y los endpoints.
//
//  FLUJO DE UNA ETIQUETA (lo que hará el agente, con la API que ya existe):
//    1. GET  api/v1/rfid/emisiones/{id}/etiquetas?COD_ESTADO_ETIQUETA=P&TOP=n   las siguientes pendientes
//    2. adaptador.LeerTidAsync()                                                  el chip entrega su TID
//    3. POST api/v1/rfid/emisiones/{id}/enrolar  { COD_TID }                      la API devuelve el código y el EPC a grabar
//    4. adaptador.GrabarEpcAsync(TID, EPC)  (o ImprimirAsync en una impresora)    se graba y se lee de vuelta
//    5. ok     → PUT api/v1/rfid/etiquetas/grabada { COD_TID, COD_EPC_LEIDO }     constancia (la API compara el EPC)
//       falla  → PUT api/v1/rfid/emisiones/{id}/etiquetas/{item}/{etiqueta}/anular { motivo, FLG_REPONER = true }
//  Qué equipo habla con el agente: los datos salen de GET api/v1/rfid/equipos (productos que Configuración general > RFID dice que son
//  equipos, con su ficha técnica). El agente se queda con los de su estación (COD_ESTACION_AGENTE).
//  Un reintento del paso 3 con el mismo TID es seguro: la API devuelve lo ya enrolado (FLG_YA_ENROLADA).
// ═══════════════════════════════════════════════════════════════════════════════════════════════════════════

/// <summary>Cómo habla el agente con un equipo: lo que necesita un adaptador. El equipo es un PRODUCTO (ESTANDAR.PRODUCTOS) y estos datos salen de su
/// ficha técnica (ESTANDAR.PRODUCTOS_ESPECIFICACIONES_TECNICAS), según las especificaciones que dice Configuración general &gt; RFID. Se arma con
/// <see cref="Desde"/> a partir de lo que devuelve GET api/v1/rfid/equipos. Un dato que la ficha no trae llega nulo: el adaptador decide qué hace.</summary>
public class RfidEquipoConfig
{
    /// <summary>El código del producto-equipo; es el COD_PRODUCTO_EQUIPO que se manda al enrolar.</summary>
    public string CodProducto { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    /// <summary>La entidad fabricante (Impinj, Invengo, Chainway, Zebra...): la de la marca de la ficha.</summary>
    public string? Fabricante { get; set; }
    public string? Marca { get; set; }
    public string? Modelo { get; set; }
    /// <summary>LLRP · REST · SDK · ZPL · SERIAL · MANUAL... lo que diga la lista de la especificación del protocolo.</summary>
    public string? Protocolo { get; set; }
    /// <summary>IP, nombre del equipo o puerto serie (COM3).</summary>
    public string? Direccion { get; set; }
    /// <summary>Puerto de comunicación (el TCP al que se conecta el agente).</summary>
    public int? Puerto { get; set; }
    /// <summary>Puertos de antena con que se escribe (en la ficha, "1,2"). Vacío si la ficha no los dice.</summary>
    public IReadOnlyList<int> PuertosAntena { get; set; } = Array.Empty<int>();
    /// <summary>Fijo · Portátil · Impresora... Sale de la sub familia del producto (no de la ficha técnica).</summary>
    public string? TipoEquipo { get; set; }
    /// <summary>Potencia de transmisión con que se escribe (TX POWER), tal como está en la ficha (dBm en los lectores Impinj).</summary>
    public decimal? PotenciaEscritura { get; set; }
    /// <summary>La estación donde corre el agente: el COD_PRODUCTO del producto-estación (el PC) que se eligió en Configuración general &gt; RFID (es una sola para todos los equipos). Es lo que el agente pone en su <c>Estacion</c> de appsettings.json.</summary>
    public string? EstacionAgente { get; set; }
    /// <summary>Descripción del producto-estación. Nula si la estación de la ficha no es un producto del grupo (y familia) de las estaciones.</summary>
    public string? DescripcionEstacion { get; set; }
    /// <summary>Ruta completa del archivo ZPL de las impresoras (carpeta de Rutas de archivos + archivo, elegidos en Configuración general &gt; RFID); @EPC y los demás marcadores los reemplaza el adaptador. Nula si falta la carpeta o el archivo.</summary>
    public string? PlantillaImpresion { get; set; }
    /// <summary>Lo que está mal en la estación o la plantilla configuradas (nulo si todo está bien): el agente lo avisa al conectarse.</summary>
    public string? ProblemasFicha { get; set; }

    /// <summary>Verdadero si el equipo es de ese fabricante: compara sin distinguir mayúsculas contra el fabricante y la marca (así "Impinj" y "IMPINJ Inc." coinciden).</summary>
    public bool EsDe(string fabricante)
        => !string.IsNullOrWhiteSpace(fabricante)
           && ((Fabricante?.Contains(fabricante, StringComparison.OrdinalIgnoreCase) ?? false) || (Marca?.Contains(fabricante, StringComparison.OrdinalIgnoreCase) ?? false));

    /// <summary>Arma la configuración desde lo que devuelve la API. Los textos numéricos de la ficha (puerto, potencia, puertos de antena) se leen con punto decimal
    /// y se ignora lo que no sea número: un dato mal escrito en la ficha queda nulo, no rompe al agente.</summary>
    public static RfidEquipoConfig Desde(KONSolutions.Shared.Models.Rfid.RfidEquipo e) => new()
    {
        CodProducto = e.COD_PRODUCTO,
        Descripcion = e.DES_PRODUCTO,
        Fabricante = e.DES_ENTIDAD_MARCA,
        Marca = e.DES_MARCA,
        Modelo = e.DES_MODELO,
        Protocolo = e.DES_PROTOCOLO,
        Direccion = e.VAL_DIRECCION,
        Puerto = int.TryParse(e.VAL_PUERTO?.Trim(), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var p) ? p : null,
        PuertosAntena = (e.VAL_PUERTOS_ANTENA ?? string.Empty)
            .Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(x => int.TryParse(x, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var n) ? (int?)n : null)
            .Where(n => n is not null).Select(n => n!.Value).ToList(),
        TipoEquipo = e.DES_TIPO_EQUIPO,
        PotenciaEscritura = decimal.TryParse(e.VAL_POTENCIA?.Trim(), System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var w) ? w : null,
        EstacionAgente = e.COD_ESTACION_AGENTE,
        DescripcionEstacion = e.DES_ESTACION_AGENTE,
        PlantillaImpresion = e.RUTA_PLANTILLA_IMPRESION,
        ProblemasFicha = e.DES_PROBLEMAS_FICHA,
    };
}

/// <summary>Lo que sabe hacer un equipo. El agente pregunta antes de usar una operación.</summary>
public class RfidCapacidades
{
    public bool LeeTid { get; set; }
    public bool GrabaEpc { get; set; }
    /// <summary>Imprime la etiqueta (texto y código de barras) y graba el chip en el mismo paso: las impresoras RFID.</summary>
    public bool Imprime { get; set; }
    /// <summary>Largo máximo de la memoria EPC en bits (96 es lo habitual).</summary>
    public int EpcBitsMaximo { get; set; } = 96;
}

public class RfidEstadoEquipo
{
    public bool Conectado { get; set; }
    public bool ListoParaGrabar { get; set; }
    /// <summary>Lo que dice el equipo cuando no está listo (sin etiquetas, tapa abierta, antena desconectada...).</summary>
    public string? Mensaje { get; set; }
}

public class RfidLecturaOpciones
{
    /// <summary>Cuánto espera el equipo a que aparezca un chip antes de rendirse.</summary>
    public int TiempoEsperaMs { get; set; } = 5000;
    /// <summary>Antena a usar, si el equipo tiene varias (la API de cada fabricante las numera distinto: lo interpreta el adaptador).</summary>
    public int? Antena { get; set; }
}

/// <summary>El TID de un chip. Siempre en hexadecimal, en mayúsculas y sin espacios.</summary>
public class RfidLecturaTid
{
    public bool Leido { get; set; }
    public string? Tid { get; set; }
    /// <summary>El EPC que tenía el chip cuando se lo leyó (un chip virgen trae uno de fábrica).</summary>
    public string? EpcActual { get; set; }
    /// <summary>Si apareció más de un chip a la vez no se debe grabar ninguno: hay que dejar uno solo frente al equipo.</summary>
    public int CantidadChips { get; set; }
    public string? Error { get; set; }
}

public class RfidGrabacionSolicitud
{
    /// <summary>El chip al que se graba (así se evita grabar el de al lado).</summary>
    public string Tid { get; set; } = string.Empty;
    /// <summary>24 hexadecimales: 9 ceros + el correlativo de 15 dígitos (lo devuelve la API en COD_EPC).</summary>
    public string Epc { get; set; } = string.Empty;
}

public class RfidGrabacionResultado
{
    public bool Grabado { get; set; }
    /// <summary>El EPC que el equipo leyó de vuelta después de grabar. La API lo compara con el que mandó.</summary>
    public string? EpcLeido { get; set; }
    public string? Error { get; set; }
}

/// <summary>Para impresoras RFID: lo que se imprime en la etiqueta además de grabar el chip.</summary>
public class RfidImpresionSolicitud : RfidGrabacionSolicitud
{
    public string? Producto { get; set; }
    public string? DescripcionProducto { get; set; }
    public string? Lote { get; set; }
    public decimal? Cantidad { get; set; }
    public string? UnidadMedida { get; set; }
    /// <summary>Texto para el código de barras (por lo general, el mismo código completo de la etiqueta).</summary>
    public string? CodigoBarras { get; set; }
}

public class RfidImpresionResultado : RfidGrabacionResultado
{
    /// <summary>Verdadero si se imprimió. Puede imprimirse bien y fallar la grabación del chip (etiqueta "void").</summary>
    public bool Impreso { get; set; }
}

/// <summary>Lo que tiene que implementar el adaptador de cada fabricante. Una instancia atiende UN equipo.</summary>
public interface IRfidAdaptador : IAsyncDisposable
{
    /// <summary>Fabricante que atiende (IMPINJ, INVENGO, CHAINWAY, ZEBRA...). El agente elige el adaptador con <see cref="RfidEquipoConfig.EsDe"/> contra este valor.</summary>
    string Fabricante { get; }
    RfidCapacidades Capacidades { get; }

    Task ConectarAsync(RfidEquipoConfig config, CancellationToken ct = default);
    Task DesconectarAsync(CancellationToken ct = default);
    Task<RfidEstadoEquipo> EstadoAsync(CancellationToken ct = default);

    /// <summary>Lee el TID del chip que está frente al equipo.</summary>
    Task<RfidLecturaTid> LeerTidAsync(RfidLecturaOpciones? opciones = null, CancellationToken ct = default);
    /// <summary>Graba el EPC en el chip de ese TID y lo lee de vuelta.</summary>
    Task<RfidGrabacionResultado> GrabarEpcAsync(RfidGrabacionSolicitud solicitud, CancellationToken ct = default);
    /// <summary>Solo impresoras: imprime y graba. Un equipo que no imprime lanza <see cref="NotSupportedException"/>.</summary>
    Task<RfidImpresionResultado> ImprimirAsync(RfidImpresionSolicitud solicitud, CancellationToken ct = default);
}
