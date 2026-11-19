using System.Text.Json;
using KonSolutions.Agente.Impresora;

namespace KonSolutions.Agente.Configuracion;

/// <summary>Lo que dice appsettings.json. El equipo en sí (IP, puerto, antenas, potencia...) NO está aquí: sale de la ficha técnica del producto-equipo
/// (Configuracion general > RFID). Aquí solo va cómo llegar a la API, qué estación es esta y cómo se comporta el agente.</summary>
public sealed class AgenteConfig
{
    public ApiOpciones Api { get; set; } = new();
    public AgenteOpciones Agente { get; set; } = new();
    public ImpinjOpciones Impinj { get; set; } = new();
    public ZebraOpciones Zebra { get; set; } = new();
    public ServicioOpciones Servicio { get; set; } = new();

    private static readonly JsonSerializerOptions Opciones = new(JsonSerializerDefaults.Web)
    {
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    public static AgenteConfig Cargar(string ruta)
    {
        if (!File.Exists(ruta))
            throw new ConfiguracionInvalidaException($"No encuentro el archivo de configuración '{ruta}'. Copia appsettings.ejemplo.json como appsettings.json y complétalo.");
        try { return JsonSerializer.Deserialize<AgenteConfig>(File.ReadAllText(ruta), Opciones) ?? new AgenteConfig(); }
        catch (JsonException ex) { throw new ConfiguracionInvalidaException($"'{ruta}' no es un JSON válido: {ex.Message}"); }
    }

    /// <summary>La contraseña del usuario de servicio: de la variable de entorno si existe (lo recomendado) y si no, la del archivo.</summary>
    public string? ObtenerPassword(Func<string, string?> variable)
    {
        var deEntorno = string.IsNullOrWhiteSpace(Api.VariablePassword) ? null : variable(Api.VariablePassword);
        if (!string.IsNullOrEmpty(deEntorno)) return deEntorno;
        return string.IsNullOrEmpty(Api.Password) ? null : Api.Password;
    }

    /// <summary>Los problemas de la configuración, en castellano y todos juntos (vacía si está bien).</summary>
    public List<string> Validar(Func<string, string?> variable)
    {
        var p = ValidarApi(variable);
        p.AddRange(ValidarComportamiento());
        return p;
    }

    /// <summary>Solo lo que hace falta para hablar con la API (dirección, usuario, contraseña).</summary>
    public List<string> ValidarApi(Func<string, string?> variable)
    {
        var p = new List<string>();
        if (!Uri.TryCreate(Api.BaseUrl, UriKind.Absolute, out var uri) || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            p.Add("Api.BaseUrl tiene que ser una dirección http o https completa, por ejemplo https://servidor:5001/");
        if (string.IsNullOrWhiteSpace(Api.Usuario)) p.Add("Api.Usuario está vacío: pon el usuario de servicio del agente.");
        if (ObtenerPassword(variable) is null) p.Add($"No hay contraseña: define la variable de entorno {Api.VariablePassword} (o, peor, Api.Password en el archivo).");
        if (Api.TimeoutSegundos is < 5 or > 300) p.Add("Api.TimeoutSegundos tiene que estar entre 5 y 300.");
        return p;
    }

    /// <summary>Los números de Agente.*, Impinj.* y Zebra.* (rangos razonables).</summary>
    public List<string> ValidarComportamiento()
    {
        var p = new List<string>();
        if (Agente.EsperaChipSegundos is < 5 or > 3600) p.Add("Agente.EsperaChipSegundos tiene que estar entre 5 y 3600.");
        if (Agente.RetiroChipSegundos is < 1 or > 600) p.Add("Agente.RetiroChipSegundos tiene que estar entre 1 y 600.");
        if (Agente.Reintentos is < 0 or > 10) p.Add("Agente.Reintentos tiene que estar entre 0 y 10.");
        if (Impinj.PuertoPorOmision is < 1 or > 65535) p.Add("Impinj.PuertoPorOmision no es un puerto válido.");
        if (Impinj.PotenciaPorOmisionDbm is < 10 or > 33) p.Add("Impinj.PotenciaPorOmisionDbm tiene que estar entre 10 y 33 dBm.");
        if (Impinj.VentanaLecturaMs is < 100 or > 5000) p.Add("Impinj.VentanaLecturaMs tiene que estar entre 100 y 5000.");
        if (Impinj.TiempoOperacionMs is < 500 or > 30000) p.Add("Impinj.TiempoOperacionMs tiene que estar entre 500 y 30000.");
        ValidarZebra(p);
        ValidarServicio(p);
        return p;
    }

    private void ValidarServicio(List<string> p)
    {
        var s = Servicio;
        if (s.PollSegundos is < 1 or > 300) p.Add("Servicio.PollSegundos tiene que estar entre 1 y 300.");
        if (s.LatidoSegundos is < 5 or > 60) p.Add("Servicio.LatidoSegundos tiene que estar entre 5 y 60 (la API da por caído a un servicio que pasa 3 minutos sin latido).");
        if (s.EsperaEtiquetaSegundos is < 5 or > 600) p.Add("Servicio.EsperaEtiquetaSegundos tiene que estar entre 5 y 600.");
        if (s.EsperasSinEtiqueta is < 1 or > 20) p.Add("Servicio.EsperasSinEtiqueta tiene que estar entre 1 y 20.");
        if (s.Equipos.Any(string.IsNullOrWhiteSpace)) p.Add("Servicio.Equipos no puede traer códigos vacíos.");
    }

    private void ValidarZebra(List<string> p)
    {
        var z = Zebra;
        if (z.PuertoPorOmision is < 1 or > 65535) p.Add("Zebra.PuertoPorOmision no es un puerto válido.");
        if (z.TiempoConexionMs is < 500 or > 60000) p.Add("Zebra.TiempoConexionMs tiene que estar entre 500 y 60000.");
        if (z.TiempoRespuestaMs is < 200 or > 30000) p.Add("Zebra.TiempoRespuestaMs tiene que estar entre 200 y 30000.");
        if (z.TiempoImpresionMs is < 1000 or > 300000) p.Add("Zebra.TiempoImpresionMs tiene que estar entre 1000 y 300000.");
        if (z.PausaLecturaMs is < 50 or > 10000) p.Add("Zebra.PausaLecturaMs tiene que estar entre 50 y 10000.");
        if (z.TidBitsMinimos is < 32 or > 192 || z.TidBitsMinimos % 16 != 0) p.Add("Zebra.TidBitsMinimos tiene que ser múltiplo de 16 entre 32 y 192 (el TID completo de un Monza son 96).");
        if (z.ComandosLeerTid.Length == 0 || z.ComandosLeerTid.Any(string.IsNullOrWhiteSpace)) p.Add("Zebra.ComandosLeerTid tiene que traer al menos un comando y ninguno vacío.");
        if (z.ConsultasResultadoTid.Length == 0 || z.ConsultasResultadoTid.Any(string.IsNullOrWhiteSpace)) p.Add("Zebra.ConsultasResultadoTid tiene que traer al menos una consulta y ninguna vacía.");
        if (z.ComandosPosicionar.Any(string.IsNullOrWhiteSpace)) p.Add("Zebra.ComandosPosicionar no puede traer comandos vacíos (déjalo sin elementos si no hace falta).");
        if (!z.EscribirEpc.Contains("{epc}", StringComparison.Ordinal)) p.Add("Zebra.EscribirEpc tiene que llevar {epc} donde va el EPC a grabar.");
        if (!z.LeerDeVuelta.Contains(ZebraComandos.EncabezadoEpc, StringComparison.Ordinal) || !z.LeerDeVuelta.Contains(ZebraComandos.EncabezadoTid, StringComparison.Ordinal))
            p.Add($"Zebra.LeerDeVuelta tiene que devolver el EPC con el encabezado {ZebraComandos.EncabezadoEpc} y el TID con {ZebraComandos.EncabezadoTid} (^HV): sin eso el agente no puede comprobar lo grabado.");
    }
}

public sealed class ApiOpciones
{
    /// <summary>Dirección de la API, p. ej. https://servidor:5001/ (con la barra al final o sin ella).</summary>
    public string BaseUrl { get; set; } = string.Empty;
    /// <summary>El usuario de servicio del agente: un rol que solo tenga la aplicación RFID (Emision de TAG con "Grabar etiquetas" y "Anular").</summary>
    public string Usuario { get; set; } = string.Empty;
    /// <summary>No la dejes en el archivo: usa la variable de entorno de VariablePassword.</summary>
    public string? Password { get; set; }
    public string VariablePassword { get; set; } = "KONSOLUTIONS_AGENTE_PASSWORD";
    /// <summary>Solo para pruebas con un certificado propio. En producción, false.</summary>
    public bool AceptarCertificadoInvalido { get; set; }
    public int TimeoutSegundos { get; set; } = 30;
}

public sealed class AgenteOpciones
{
    /// <summary>Esta estación (RfidEquipo.COD_ESTACION_AGENTE de la ficha del equipo). Con ella el agente encuentra sus equipos si no le dices cuál.</summary>
    public string? Estacion { get; set; }
    /// <summary>El producto-equipo fijo de esta estación (opcional; también se puede dar con --equipo).</summary>
    public string? CodProductoEquipo { get; set; }
    /// <summary>Cuánto espera un chip frente al lector antes de avisar y seguir esperando.</summary>
    public int EsperaChipSegundos { get; set; } = 120;
    /// <summary>Cuánto espera a que retires el chip ya grabado antes de seguir con el siguiente.</summary>
    public int RetiroChipSegundos { get; set; } = 30;
    /// <summary>Intentos extra de grabar el EPC (y leerlo de vuelta) antes de darlo por fallido.</summary>
    public int Reintentos { get; set; } = 3;
    /// <summary>Si la grabación falla tras los reintentos: anular la etiqueta (libera el chip) y reponerla. Si es false, solo avisa y la deja enrolada.</summary>
    public bool AnularSiFalla { get; set; } = true;
    public string CarpetaLogs { get; set; } = "logs";
}

public sealed class ImpinjOpciones
{
    /// <summary>LLRP: 5084 en los Speedway y en el R700.</summary>
    public int PuertoPorOmision { get; set; } = 5084;
    /// <summary>Potencia de escritura cuando la ficha del equipo no trae TX POWER. Baja a propósito: se graba un chip a la vez, a pocos centímetros.</summary>
    public double PotenciaPorOmisionDbm { get; set; } = 20;
    /// <summary>Cuánto dura cada lectura del campo para ver qué chips hay.</summary>
    public int VentanaLecturaMs { get; set; } = 600;
    /// <summary>Cuánto se espera la respuesta del lector a una lectura o escritura de memoria.</summary>
    public int TiempoOperacionMs { get; set; } = 4000;
    /// <summary>Pedir el TID junto con el EPC (FastID, chips Monza 4 o más nuevos). Si el lector o el chip no lo dan, se lee el TID aparte.</summary>
    public bool PedirFastId { get; set; } = true;
}

/// <summary>La impresora Zebra con chip RFID (ZD621R, ZT411R, ZT610R...). Se habla por TCP (el puerto 9100 de la impresora) con ZPL y los comandos SGD de Link-OS.
/// ESTOS TEXTOS DE COMANDOS NO SE PROBARON CONTRA UNA IMPRESORA REAL: salen del manual de ZPL/Link-OS y pueden cambiar entre modelos y firmware.
/// Si tu impresora responde distinto, se corrigen AQUI (sin recompilar). `agente-rfid diagnostico` muestra lo que contesta de verdad.</summary>
public sealed class ZebraOpciones
{
    /// <summary>9100: el puerto de impresión TCP de las impresoras Zebra.</summary>
    public int PuertoPorOmision { get; set; } = 9100;
    public int TiempoConexionMs { get; set; } = 4000;
    /// <summary>Cuánto se espera la respuesta de la impresora a una consulta (~HS, ~HI, getvar...).</summary>
    public int TiempoRespuestaMs { get; set; } = 3000;
    /// <summary>Cuánto se espera a que la impresora termine una etiqueta y devuelva lo que leyó del chip.</summary>
    public int TiempoImpresionMs { get; set; } = 20000;
    /// <summary>Pausa entre un intento de leer el TID y el siguiente mientras se espera una etiqueta.</summary>
    public int PausaLecturaMs { get; set; } = 500;
    /// <summary>El TID tiene que traer al menos estos bits. Un Monza 4/5 tiene 96 (los primeros 32 son iguales en todos los chips: no identifican a ninguno).</summary>
    public int TidBitsMinimos { get; set; } = 96;
    /// <summary>Comandos (uno por línea) que preceden a la lectura del TID para poner la etiqueta sobre la antena. Vacío = no hace falta.</summary>
    public string[] ComandosPosicionar { get; set; } = Array.Empty<string>();
    /// <summary>Comandos SGD que mandan leer el TID de la etiqueta que está sobre la antena (no sacan etiqueta).</summary>
    public string[] ComandosLeerTid { get; set; } =
    {
        "! U1 setvar \"rfid.tag.read.content\" \"tid\"",
        "! U1 setvar \"rfid.tag.read.execute\" \"\"",
    };
    /// <summary>Consultas SGD que traen el resultado de esa lectura; se queda con la primera respuesta que sea un TID.</summary>
    public string[] ConsultasResultadoTid { get; set; } =
    {
        "! U1 getvar \"rfid.tag.read.result_line1\"",
        "! U1 getvar \"rfid.tag.read.result_line2\"",
    };
    /// <summary>ZPL que graba el EPC. {epc} = los hexadecimales a grabar; {bytes} = su largo en bytes. Si la plantilla ya trae su propio ^RFW, no se agrega.</summary>
    public string EscribirEpc { get; set; } = "^RFW,H,2,{bytes},1^FD{epc}^FS";
    /// <summary>ZPL que lee de vuelta el EPC y el TID y los manda al agente (^HV). Los encabezados #EPC= y #TID= no se cambian: con ellos el agente reconoce la respuesta.</summary>
    public string LeerDeVuelta { get; set; } = "^RFR,H,2,{bytes},1^FN1^FS^HV1,,#EPC=^FS^RFR,H,0,12,2^FN2^FS^HV2,,#TID=^FS";
    /// <summary>Contadores de etiquetas buenas y malas (void) de la impresora; el agente compara antes y después de cada etiqueta.</summary>
    public string ConsultaContadorValidas { get; set; } = "! U1 getvar \"odometer.rfid.valid_resettable\"";
    public string ConsultaContadorVoid { get; set; } = "! U1 getvar \"odometer.rfid.void_resettable\"";
}

/// <summary>El servicio (agente-rfid servicio): se queda corriendo en la PC de la planta y atiende los trabajos de impresión que se piden desde la web (botón Imprimir de Emisión de TAG).</summary>
public sealed class ServicioOpciones
{
    /// <summary>Cada cuántos segundos pregunta a la API si hay un trabajo nuevo cuando no tiene ninguno.</summary>
    public int PollSegundos { get; set; } = 5;
    /// <summary>Cada cuántos segundos avisa a la API que sigue vivo mientras imprime (también así se entera de que piden cancelar). La API da por caído un servicio que pasa 3 minutos sin latido.</summary>
    public int LatidoSegundos { get; set; } = 20;
    /// <summary>Cuánto espera una etiqueta con chip sobre la antena de la impresora antes de volver a preguntar.</summary>
    public int EsperaEtiquetaSegundos { get; set; } = 30;
    /// <summary>Cuántas esperas seguidas sin etiqueta dan el trabajo por fallido (se acabó el rollo o las etiquetas no traen chip).</summary>
    public int EsperasSinEtiqueta { get; set; } = 2;
    /// <summary>Los productos-equipo (impresoras) que atiende este servicio. Vacío = el de <c>Agente.CodProductoEquipo</c>.</summary>
    public string[] Equipos { get; set; } = Array.Empty<string>();
}

public sealed class ConfiguracionInvalidaException : Exception
{
    public ConfiguracionInvalidaException(string mensaje) : base(mensaje) { }
}
