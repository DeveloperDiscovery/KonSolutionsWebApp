using System.Net.Http.Json;
using Blazored.LocalStorage;
using KONSolutions.Shared.Common;
using KONSolutions.Shared.Models.Auth;
using KONSolutions.Shared.Models.Seguridad;

namespace KONSolutions.Web.Services;

/// <summary>
/// Carga roles, opciones de menú y botones autorizados del usuario logueado.
/// Diseño: al login (o al cambiar de rol activo) se hace UNA sola llamada al SP
/// TREEVIEW que trae todos los botones de todas las opciones del rol — el resto
/// del sistema filtra en memoria (PorOpcion) sin volver a llamar a la API.
/// </summary>
public class MenuService
{
    private readonly HttpClient _http;
    private readonly ILocalStorageService _localStorage;

    /// <summary>Botones del rol activo, cacheados en memoria tras el último Cargar/CambiarRol.</summary>
    private List<BotonTreeviewItem> _botones = new();
    private List<OpcionTreeviewItem> _opciones = new();

    /// <summary>Si los íconos ya se pidieron alguna vez en esta sesión.
    ///
    /// Hace falta porque FetchBotonesAsync BORRA los íconos a propósito (el TREEVIEW ya no
    /// los trae; vienen aparte por TREEVIEWLOGOS). En el login eso no se nota: MainLayout
    /// llama a CargarLogosEnSegundoPlanoAsync justo después. Pero CADA recarga posterior del
    /// árbol —cambio de rol, o ForzarRecargaAsync cuando alguien edita una opción o un
    /// botón— volvía a dejarlos en null y NADIE los volvía a pedir: todos los tiles caían al
    /// ícono genérico de reserva hasta el próximo login. Ese era el "se desaparecen los
    /// íconos del menú".
    ///
    /// Con la bandera, el primer pedido lo sigue haciendo MainLayout (sin llamada doble) y
    /// cualquier recarga posterior se trae los íconos sola.</summary>
    private bool _logosCargados;

    /// <summary>FLG_TECLADO_VIRTUAL de una opción (pantalla). null si no se encuentra.</summary>
    public bool? TecladoVirtualOpcion(string? codAplicacion, string? codOpcion)
    {
        if (string.IsNullOrWhiteSpace(codAplicacion) || string.IsNullOrWhiteSpace(codOpcion)) return null;
        var o = _opciones.FirstOrDefault(x =>
            x.COD_APLICACION == codAplicacion && x.COD_OPCION_APLICACION == codOpcion);
        return o?.FLG_TECLADO_VIRTUAL;
    }

    /// <summary>FLG_MULTI_LOAD de una opción: si la misma opción puede estar abierta en
    /// varias pestañas del workspace a la vez. null/false = la de siempre (volver a hacer
    /// clic salta a la pestaña ya abierta).
    ///
    /// Se resuelve acá y no viajando el campo por OpcionMenu/NodoMenu/BotonTreeviewItem
    /// porque los cuatro puntos de entrada al workspace (menú lateral, tiles, circular y
    /// favoritos del Workspace) usan modelos distintos: una consulta central es un solo
    /// lugar donde mirar, en vez de cuatro mapeos que se pueden desincronizar.</summary>
    public bool MultiLoadOpcion(string? codAplicacion, string? codOpcion)
    {
        if (string.IsNullOrWhiteSpace(codAplicacion) || string.IsNullOrWhiteSpace(codOpcion)) return false;
        return _opciones.FirstOrDefault(x =>
            x.COD_APLICACION == codAplicacion && x.COD_OPCION_APLICACION == codOpcion)?.FLG_MULTI_LOAD == true;
    }

    /// <summary>FLG_EXIGE_PASSWORD de una opción (pantalla). null si no se encuentra.
    /// Usar antes de abrir CUALQUIER pantalla desde CUALQUIER punto de entrada (menú
    /// lateral, favoritos del Workspace, tiles, búsqueda, etc.) — todos deben pasar
    /// por aquí en vez de construir su propio objeto sin este flag.</summary>
    public bool? ExigePasswordOpcion(string? codAplicacion, string? codOpcion)
    {
        if (string.IsNullOrWhiteSpace(codAplicacion) || string.IsNullOrWhiteSpace(codOpcion)) return null;
        var o = _opciones.FirstOrDefault(x =>
            x.COD_APLICACION == codAplicacion && x.COD_OPCION_APLICACION == codOpcion);
        return o?.FLG_EXIGE_PASSWORD;
    }

    /// <summary>Las sub-opciones (hijos directos) de una opción del menú, según la
    /// configuración de Seguridad › Opciones de Aplicaciones para el rol activo.
    ///
    /// PARA QUÉ: hay pantallas que sirven a varias opciones y traen pestañas que NO
    /// corresponden a todas. Caso real: Proformas de Compra tiene el sub-módulo
    /// "Comparativo de Proformas" y Órdenes de Compra no, pero las dos abren el mismo
    /// .razor — sin esto, la pestaña Comparativo salía en las dos.</summary>
    public List<OpcionTreeviewItem> SubOpciones(string? codAplicacion, string? codOpcionPadre)
    {
        if (string.IsNullOrWhiteSpace(codAplicacion) || string.IsNullOrWhiteSpace(codOpcionPadre))
            return new();
        return _opciones
            .Where(o => o.COD_APLICACION == codAplicacion
                     && o.COD_OPCION_APLICACION_PADRE == codOpcionPadre)
            .ToList();
    }

    /// <summary>Si la opción tiene un sub-módulo que coincide con el texto buscado, mirando
    /// tanto su descripción como su formulario. Se compara sin distinguir mayúsculas y por
    /// "contiene" a propósito: la descripción la escribe quien configura el menú
    /// ("Comparativo de Proformas", "COMPARATIVO DE COMPRAS"…) y exigir una coincidencia
    /// exacta obligaría a que el texto de la BD y el del código no se separen nunca.
    ///
    /// Si la pantalla se abrió SIN códigos de opción (fuera del workspace), devuelve false:
    /// sin menú no hay sub-módulos que consultar. El llamador decide qué hacer con eso.</summary>
    public bool TieneSubOpcion(string? codAplicacion, string? codOpcionPadre, string texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return false;
        return SubOpciones(codAplicacion, codOpcionPadre).Any(o =>
               (o.DES_OPCION_APLICACION ?? "").Contains(texto, StringComparison.OrdinalIgnoreCase)
            || (o.DES_FORMULARIO        ?? "").Contains(texto, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>DES_OPCION_APLICACION configurado en el menú para esta pantalla — permite que
    /// el título visible en la página salga de la configuración (Seguridad › Opciones de
    /// Aplicaciones) en vez de quedar hardcodeado en el .razor. null si no se encuentra
    /// (p.ej. la pantalla se abrió sin CodAplicacion/CodOpcionAplicacion) — el llamador debe
    /// tener un texto de respaldo para ese caso.</summary>
    public string? DescripcionOpcion(string? codAplicacion, string? codOpcion)
    {
        if (string.IsNullOrWhiteSpace(codAplicacion) || string.IsNullOrWhiteSpace(codOpcion)) return null;
        var o = _opciones.FirstOrDefault(x =>
            x.COD_APLICACION == codAplicacion && x.COD_OPCION_APLICACION == codOpcion);
        return o?.DES_OPCION_APLICACION;
    }

    /// <summary>Razor que dibuja una opción (DES_FORMULARIO), leído de _opciones.
    /// Lo usa AccionesBotones para la ficha técnica del clic derecho: el SP de botones por
    /// opción devuelve esa columna en nulo —viene del LEFT JOIN hacia la tabla de botones—,
    /// así que tomarla del BotonTreeviewItem daba siempre vacío aunque la opción la tuviera
    /// configurada. En _opciones sí está, que es de donde ya la lee el resto de la app.
    /// null si no se encuentra.</summary>
    public string? FormularioOpcion(string? codAplicacion, string? codOpcion)
    {
        if (string.IsNullOrWhiteSpace(codAplicacion) || string.IsNullOrWhiteSpace(codOpcion)) return null;
        var o = _opciones.FirstOrDefault(x =>
            x.COD_APLICACION == codAplicacion && x.COD_OPCION_APLICACION == codOpcion);
        return o?.DES_FORMULARIO;
    }

    /// <summary>Las opciones del árbol de menú tal como se cargaron para el rol activo.
    /// Sólo lectura: quien la consuma no puede modificar la lista interna.
    ///
    /// La usa la pantalla de Aprobaciones por Opción para listar las opciones sin pedir nada
    /// al servidor. Ojo con el alcance: son las opciones que ve el ROL ACTIVO, no todas las
    /// del sistema.</summary>
    public IReadOnlyList<OpcionTreeviewItem> OpcionesMenu => _opciones;

    /// <summary>DES_APLICACION del árbol de menú. El treeview la trae una sola vez por
    /// aplicación y al aplanar queda repetida en cada botón, así que se busca ahí: es la
    /// única lista donde el nombre está, porque _opciones sólo guarda el código.
    /// null si no se encuentra — el llamador debe poder mostrar el código como respaldo.</summary>
    public string? DescripcionAplicacion(string? codAplicacion)
    {
        if (string.IsNullOrWhiteSpace(codAplicacion)) return null;
        return _botones.FirstOrDefault(b => b.COD_APLICACION == codAplicacion
                                         && !string.IsNullOrWhiteSpace(b.DES_APLICACION))
                       ?.DES_APLICACION;
    }
    private List<RolDeUsuario> _roles = new();
    private string? _rolActivo;

    // ── Refresco en vivo de botones por opción (ver AsegurarBotonesFrescosAsync) ──
    // _botones se carga completo al login, pero un cambio de permiso hecho por el admin de
    // seguridad no debe esperar a un relogueo: cada vez que se abre una pantalla, se pide de
    // nuevo SOLO esa opción (endpoint por-opcion, ya trae FLG_HEADER/FLG_GRILLA/FLG_ACCION_*/
    // ícono desde que se corrigió el SP) y se reemplazan sus entradas en _botones. Deduplicado
    // por ventana de tiempo (no por sesión completa) para que Header + N filas de grilla de
    // la MISMA carga de pantalla disparen una sola llamada HTTP, pero la PRÓXIMA vez que se
    // entre a la pantalla sí vuelva a refrescar.
    private readonly Dictionary<string, Task> _refrescosBotonesEnCurso = new();
    private readonly Dictionary<string, DateTime> _ultimoRefrescoBotones = new();
    private static readonly TimeSpan VentanaDedupeRefrescoBotones = TimeSpan.FromSeconds(3);

    /// <summary>Se dispara cuando los íconos cargados en segundo plano (TreeviewLogos) llegan
    /// y ya están fusionados en _botones/_opciones — las pantallas de menú (tile, lateral,
    /// circular) deben suscribirse y reconstruir su vista (StateHasChanged) al recibirlo.</summary>
    public event Action? IconosActualizados;

    public MenuService(HttpClient http, ILocalStorageService localStorage)
    { _http = http; _localStorage = localStorage; }

    /// <summary>Roles asignados al usuario logueado. Vacío si aún no se cargó (llamar CargarAsync primero).</summary>
    public IReadOnlyList<RolDeUsuario> Roles => _roles;

    /// <summary>Rol actualmente activo (el que determina menú y permisos visibles).</summary>
    public string? RolActivo => _rolActivo;

    /// <summary>El selector de rol en la barra superior solo debe mostrarse con 2+ roles.</summary>
    public bool TieneVariosRoles => _roles.Count > 1;

    /// <summary>
    /// Carga inicial al login: roles del usuario + botones/opciones del rol activo
    /// (el primero de la lista, o el que ya estuviera guardado en sesión).
    /// </summary>
    public async Task CargarAsync()
    {
        var t0 = DateTime.Now;
        // Console.WriteLine($"[TRACE] {t0:HH:mm:ss.fff} — CARGA DE MENU - MenuService: inicio MisRoles");
        try
        {
            var rRoles = await _http.GetFromJsonAsync<ApiResponse<List<RolDeUsuario>>>(ApiRoutes.Seguridad.MisRoles);
            _roles = rRoles?.Data ?? new();
        }
        catch { _roles = new(); }
        // Console.WriteLine($"[TRACE] {DateTime.Now:HH:mm:ss.fff} — CARGA DE MENU - MenuService: fin MisRoles (+{(DateTime.Now-t0).TotalMilliseconds:0}ms)");

        // Rol activo: el que ya tenga la sesión (UsuarioSesion.COD_ROL_USUARIO),
        // o el primero de la lista de roles si no hay uno definido.
        var sesion = await _localStorage.GetItemAsync<UsuarioSesion>("usuarioSesion");
        _rolActivo = sesion?.COD_ROL_USUARIO;
        if (string.IsNullOrWhiteSpace(_rolActivo))
            _rolActivo = _roles.FirstOrDefault()?.COD_ROL_USUARIO;

        // Console.WriteLine($"[TRACE] {DateTime.Now:HH:mm:ss.fff} — CARGA DE MENU - MenuService: inicio BotonesTreeview (rol={_rolActivo})");
        await CargarBotonesAsync();
        // Console.WriteLine($"[TRACE] {DateTime.Now:HH:mm:ss.fff} — CARGA DE MENU - MenuService: fin BotonesTreeview (+{(DateTime.Now-t0).TotalMilliseconds:0}ms total)");
    }

    /// <summary>
    /// Cambia el rol activo: vuelve a pedir el treeview de botones para ese rol
    /// específico. El componente que llama esto debe, además, reconstruir el menú
    /// lateral y cerrar todas las pestañas salvo Workspace.
    /// </summary>
    public async Task CambiarRolAsync(string codRolUsuario)
    {
        // Limpiar caché del nuevo rol antes de cargarlo para evitar datos viejos
        var prevRol = _rolActivo;
        _rolActivo = codRolUsuario;
        try
        {
            await _localStorage.RemoveItemAsync($"botonesTreeview_{codRolUsuario}");
            await _localStorage.RemoveItemAsync($"botonesTreeview_{codRolUsuario}_ts");
        }
        catch { }
        await CargarBotonesAsync();
    }

    /// <summary>Fuerza una recarga del árbol de botones/opciones del rol activo, saltando la
    /// caché de localStorage (TTL 60 min) — se usa cuando SEGURIDAD.APLICACIONES_OPCIONES o
    /// SEGURIDAD.APLICACIONES_OPCIONES_BOTONES cambian (ver MainLayout.RefrescarMenuPorCambio),
    /// para no esperar hasta que venza la caché ni un nuevo login.</summary>
    public async Task ForzarRecargaAsync()
    {
        if (string.IsNullOrWhiteSpace(_rolActivo)) return;
        try
        {
            await _localStorage.RemoveItemAsync($"botonesTreeview_{_rolActivo}");
            await _localStorage.RemoveItemAsync($"botonesTreeview_{_rolActivo}_ts");
        }
        catch { }
        await CargarBotonesAsync();
    }

    private static readonly TimeSpan _cacheTtl = TimeSpan.FromMinutes(60);

    private async Task CargarBotonesAsync()
    {
        if (string.IsNullOrWhiteSpace(_rolActivo)) { _botones = new(); return; }

        var cacheKey = $"botonesTreeview_{_rolActivo}";
        var tsKey    = $"botonesTreeview_{_rolActivo}_ts";

        // ── Leer caché ──────────────────────────────────────────────────────
        // El SP ahora devuelve 3 resultados deduplicados (aplicaciones/opciones/botones,
        // cada ícono UNA sola vez) — eso es lo que se cachea, mucho más liviano que antes
        // (el ícono ya no viaja repetido por cada botón de la misma app/opción).
        TreeviewResult? cached = null;
        bool cacheVencida = true;
        try
        {
            var ts = await _localStorage.GetItemAsync<DateTime?>(tsKey);
            cached = await _localStorage.GetItemAsync<TreeviewResult>(cacheKey);
            cacheVencida = !ts.HasValue || DateTime.Now - ts.Value >= _cacheTtl;
        }
        catch { }

        // ── Stale-while-revalidate ──────────────────────────────────────────
        // Si hay datos en caché (aunque estén vencidos), úsalos YA para que el
        // menú aparezca de inmediato. Luego refresca en segundo plano.
        if (cached is { Botones.Count: > 0 })
        {
            _botones = Fusionar(cached);
            _opciones = cached.Opciones;
            // Console.WriteLine($"[TRACE] {DateTime.Now:HH:mm:ss.fff} — CARGA DE MENU - BotonesTreeview desde CACHE ({_botones.Count} items, vencida={cacheVencida})");
            if (cacheVencida)
                _ = RefrescarBotonesEnSegundoPlanoAsync(cacheKey, tsKey);
            else
                _ = MergeFlagsExigePasswordAsync(cached.Opciones);
            // Los íconos YA NO se disparan desde aquí — el llamador (MainLayout) los pide
            // explícitamente recién cuando el menú terminó de pintarse (splash cerrado),
            // así no compiten por red/CPU con el resto de la carga inicial.
            return;
        }

        // ── Sin caché: cargar bloqueante (primera vez) ──────────────────────
        await FetchBotonesAsync(cacheKey, tsKey);
    }

    private async Task RefrescarBotonesEnSegundoPlanoAsync(string cacheKey, string tsKey)
    {
        await FetchBotonesAsync(cacheKey, tsKey);
        // Console.WriteLine($"[TRACE] {DateTime.Now:HH:mm:ss.fff} — CARGA DE MENU - BotonesTreeview refresco bg completado ({_botones.Count} items)");
    }

    /// <summary>
    /// El SP TREEVIEW no siempre trae FLG_EXIGE_PASSWORD en su resultado de opciones —
    /// se fusiona aquí desde el maestro plano GET /seguridad/aplicaciones-opciones, que
    /// sí la tiene siempre (misma columna que ya usa el diálogo de edición de opciones).
    /// Se llama tanto al cargar desde caché como al refrescar desde la API, para que el
    /// gate de contraseña funcione incluso en la primera pintura desde caché.
    /// </summary>
    private async Task MergeFlagsExigePasswordAsync(List<OpcionTreeviewItem> opciones)
    {
        try
        {
            // GET /seguridad/aplicaciones-opciones (sin parámetros) devuelve 400 en este
            // ambiente — se pide por app usando /by-app, el mismo endpoint que ya usa el
            // diálogo de edición de opciones y funciona bien.
            var flagsMapa = new Dictionary<(string, string), KONSolutions.Shared.Models.Seguridad.AplicacionesOpcionesFlagsRow>();
            var apps = opciones.Select(o => o.COD_APLICACION).Where(a => !string.IsNullOrWhiteSpace(a)).Distinct().ToList();

            // Todas las apps en paralelo en lugar de secuencial (N+1 → 1 ronda) — antes cada
            // login/carga sin caché esperaba una ronda HTTP completa por CADA aplicación
            // distinta del menú, una detrás de la otra, sumando varios segundos.
            var tareas = apps.Select(app => _http.GetFromJsonAsync<ApiResponse<List<KONSolutions.Shared.Models.Seguridad.AplicacionesOpcionesFlagsRow>>>(
                $"{ApiRoutes.Seguridad.AplicacionesOpciones}/by-app?codAplicacion={Uri.EscapeDataString(app)}")).ToList();
            var resultados = await Task.WhenAll(tareas);
            foreach (var flagsResp in resultados)
                if (flagsResp?.Data is not null)
                    foreach (var f in flagsResp.Data)
                        flagsMapa[(f.COD_APLICACION, f.COD_OPCION_APLICACION)] = f;

            foreach (var o in opciones)
                if (flagsMapa.TryGetValue((o.COD_APLICACION, o.COD_OPCION_APLICACION), out var f))
                {
                    o.FLG_EXIGE_PASSWORD = f.FLG_EXIGE_PASSWORD;
                    o.FLG_MULTI_LOAD     = f.FLG_MULTI_LOAD;
                }

            // Ahora corre en segundo plano (ver FetchBotonesAsync) — avisa a las pantallas de
            // menú abiertas para que repinten el candado de "exige contraseña" si corresponde.
            IconosActualizados?.Invoke();
        }
        catch { /* si falla la fusión, el menú igual carga sin el gate de contraseña */ }
    }

    private async Task FetchBotonesAsync(string cacheKey, string tsKey)
    {
        try
        {
            var url = $"{ApiRoutes.Seguridad.BotonesTreeview}?codRolUsuario={Uri.EscapeDataString(_rolActivo!)}";
            var resp = await _http.GetFromJsonAsync<ApiResponse<TreeviewResult>>(url);
            var data = resp?.Data ?? new TreeviewResult();

            // Experimento: el primer render de tile/lateral/circular sale sin íconos (texto
            // ya alcanza para pintar el menú); los íconos llegan aparte y sin bloquear, vía
            // CargarLogosEnSegundoPlanoAsync() — que ahora dispara el llamador (MainLayout)
            // recién cuando el menú de texto terminó de pintarse, no este método. Se limpian
            // aquí por si el SP TREEVIEW todavía los trae embebidos
            // (si el SP en BD todavía hace SELECT de esas columnas, esto NO evita que viajen
            // por la red desde SQL Server hasta la API — solo evita que crucen a memoria del
            // browser/UI. Para que el primer render sea realmente liviano de punta a punta,
            // el PROC_..._TREEVIEW en la base debe dejar de traer IMG_ICONO_APLICACION/
            // IMG_ICONO_OPCION — esos íconos ahora los trae el SP TREEVIEWLOGOS aparte).
            foreach (var a in data.Aplicaciones) a.IMG_ICONO_APLICACION = null;
            foreach (var o in data.Opciones)     o.IMG_ICONO_OPCION     = null;

            _botones = Fusionar(data);
            _opciones = data.Opciones;

            // FLG_EXIGE_PASSWORD y los íconos NO bloquean el primer render — ambos se piden
            // en paralelo y en segundo plano; antes MergeFlagsExigePasswordAsync se esperaba
            // aquí (una ronda HTTP por cada app distinta del menú) y retrasaba la aparición
            // del menú de tiles/lateral/circular tanto como el ícono mismo.
            _ = MergeFlagsExigePasswordAsync(data.Opciones);
            // Íconos: idem caso de caché arriba — el llamador los pide después, no aquí.
            try
            {
                // Los íconos (base64) NO se persisten en localStorage — solo viven en memoria
                // para esta sesión. Con muchas aplicaciones/opciones con ícono propio, esta
                // caché podía pesar varios MB y hacer que CUALQUIER otro guardado en
                // localStorage (ej. Configuración General con logos) fallara con
                // QuotaExceededError al superar el límite total del navegador para el origen.
                var paraCache = new TreeviewResult
                {
                    Botones = data.Botones,
                    Aplicaciones = data.Aplicaciones
                        .Select(a => new AplicacionTreeviewItem
                        {
                            COD_APLICACION = a.COD_APLICACION,
                            DES_APLICACION = a.DES_APLICACION,
                            DES_BACKCOLOR_APLICACION = a.DES_BACKCOLOR_APLICACION,
                            DES_FORECOLOR_APLICACION = a.DES_FORECOLOR_APLICACION,
                            IMG_ICONO_APLICACION = null
                        }).ToList(),
                    Opciones = data.Opciones
                        .Select(o => new OpcionTreeviewItem
                        {
                            COD_APLICACION = o.COD_APLICACION,
                            COD_TIPO_OPCION_APLICACION = o.COD_TIPO_OPCION_APLICACION,
                            DES_TIPO_OPCION_APLICACION = o.DES_TIPO_OPCION_APLICACION,
                            COD_OPCION_APLICACION_PADRE = o.COD_OPCION_APLICACION_PADRE,
                            COD_OPCION_APLICACION = o.COD_OPCION_APLICACION,
                            DES_OPCION_APLICACION = o.DES_OPCION_APLICACION,
                            DES_FORMULARIO = o.DES_FORMULARIO,
                            DES_BACKCOLOR_OPCION = o.DES_BACKCOLOR_OPCION,
                            DES_FORECOLOR_OPCION = o.DES_FORECOLOR_OPCION,
                            IMG_ICONO_OPCION = null,
                            FLG_TECLADO_VIRTUAL = o.FLG_TECLADO_VIRTUAL,
                            FLG_EXIGE_PASSWORD = o.FLG_EXIGE_PASSWORD,
                            FLG_MULTI_LOAD = o.FLG_MULTI_LOAD
                        }).ToList()
                };
                await _localStorage.SetItemAsync(cacheKey, paraCache);
                await _localStorage.SetItemAsync(tsKey, DateTime.Now);
            }
            catch { }

            // Este método acaba de dejar TODOS los íconos en null (ver arriba: el TREEVIEW ya
            // no los trae). Si en esta sesión ya se habían pedido alguna vez, hay que volver a
            // pedirlos ACÁ MISMO: si no, el árbol queda sin íconos y nadie más los repone.
            //
            // En el login la bandera todavía está en false, así que no se pide dos veces —
            // ahí lo hace MainLayout, como siempre.
            if (_logosCargados) await CargarLogosEnSegundoPlanoAsync();
        }
        catch (Exception)
        {
            // Console.WriteLine($"[ERROR] CARGA DE MENU - FetchBotonesAsync falló: {ex}");
            _botones = new();
        }
    }

    /// <summary>
    /// Pide los íconos de TODO el árbol del rol activo (apps/opciones/botones) en un endpoint
    /// aparte del texto (TreeviewLogos). Público: lo dispara el llamador (MainLayout) recién
    /// cuando el menú de texto ya terminó de cargar y pintarse — antes se disparaba solo,
    /// en paralelo con el resto de la carga inicial (foto, config, favoritos), compitiendo
    /// por red y CPU justo en el tramo más lento del login. Al llegar, fusiona los íconos en
    /// _botones/_opciones (en memoria, sin localStorage) y avisa via IconosActualizados para
    /// que las pantallas de menú abiertas se repinten.
    /// </summary>
    public async Task CargarLogosEnSegundoPlanoAsync()
    {
        if (string.IsNullOrWhiteSpace(_rolActivo)) return;
        var t0 = DateTime.Now;
        // Console.WriteLine($"[TRACE] {t0:HH:mm:ss.fff} — CARGA DE ICONOS - MenuService: inicio TreeviewLogos (rol={_rolActivo})");
        try
        {
            var url = $"{ApiRoutes.Seguridad.BotonesTreeviewLogos}?codRolUsuario={Uri.EscapeDataString(_rolActivo)}";
            var resp = await _http.GetFromJsonAsync<ApiResponse<TreeviewLogosResult>>(url);
            // Console.WriteLine($"[TRACE] {DateTime.Now:HH:mm:ss.fff} — CARGA DE ICONOS - MenuService: fin HTTP TreeviewLogos (+{(DateTime.Now - t0).TotalMilliseconds:0}ms, apps={resp?.Data?.Aplicaciones.Count ?? 0}, opciones={resp?.Data?.Opciones.Count ?? 0}, botones={resp?.Data?.Botones.Count ?? 0})");
            var data = resp?.Data;
            if (data is null) return;

            // Los tres primeros resultados vienen SIN la imagen: solo con un ICONO_ID que
            // apunta a la lista Iconos, donde cada imagen distinta viaja una sola vez.
            // Aquí se deshace esa indirección y el resto del método sigue igual que antes.
            ResolverIconos(data);

            var iconosApp = data.Aplicaciones.ToDictionary(a => a.COD_APLICACION, a => a.IMG_ICONO_APLICACION);
            var iconosOpcion = data.Opciones.ToDictionary(o => (o.COD_APLICACION, o.COD_OPCION_APLICACION), o => o.IMG_ICONO_OPCION);
            var iconosBoton = data.Botones
                .Where(b => !string.IsNullOrWhiteSpace(b.COD_BOTON_OPCION))
                .ToDictionary(b => (b.COD_APLICACION, b.COD_OPCION_APLICACION, b.COD_BOTON_OPCION!), b => b.IMG_ICONO);

            foreach (var b in _botones)
            {
                if (b.IMG_ICONO_APLICACION is null && iconosApp.TryGetValue(b.COD_APLICACION, out var iconoApp))
                    b.IMG_ICONO_APLICACION = iconoApp;
                if (b.IMG_ICONO_OPCION is null && iconosOpcion.TryGetValue((b.COD_APLICACION, b.COD_OPCION_APLICACION), out var iconoOpcion))
                    b.IMG_ICONO_OPCION = iconoOpcion;
                if (b.IMG_ICONO_BOTON is null && !string.IsNullOrWhiteSpace(b.COD_BOTON_OPCION) &&
                    iconosBoton.TryGetValue((b.COD_APLICACION, b.COD_OPCION_APLICACION, b.COD_BOTON_OPCION), out var iconoBoton))
                    b.IMG_ICONO_BOTON = iconoBoton;
            }
            foreach (var o in _opciones)
                if (o.IMG_ICONO_OPCION is null && iconosOpcion.TryGetValue((o.COD_APLICACION, o.COD_OPCION_APLICACION), out var iconoOpcion))
                    o.IMG_ICONO_OPCION = iconoOpcion;

            _logosCargados = true;
            IconosActualizados?.Invoke();
            // Console.WriteLine($"[TRACE] {DateTime.Now:HH:mm:ss.fff} — CARGA DE ICONOS - MenuService: fusión + IconosActualizados (+{(DateTime.Now - t0).TotalMilliseconds:0}ms total)");
        }
        catch (Exception)
        {
            // Console.WriteLine($"[ERROR] CARGA DE ICONOS - MenuService.CargarLogosEnSegundoPlanoAsync falló (+{(DateTime.Now - t0).TotalMilliseconds:0}ms): {ex.Message}");
        }
    }

    /// <summary>
    /// Deshace la deduplicación de íconos: copia cada imagen de la lista Iconos al item
    /// que la referencia por ICONO_ID.
    ///
    /// La misma imagen se comparte entre muchos items (el ícono de NUEVO lo usan 122
    /// botones), y aquí se asigna la MISMA referencia de string a todos — no se duplica
    /// en memoria del navegador, solo se apunta al mismo objeto.
    ///
    /// Tolera respuestas del formato viejo: si Iconos viene vacío pero los items ya traen
    /// IMG_ICONO*, no se toca nada y todo sigue funcionando (útil si se despliega el
    /// frontend antes que el SP nuevo).
    /// </summary>
    private static void ResolverIconos(TreeviewLogosResult data)
    {
        if (data.Iconos.Count == 0) return;

        var porId = new Dictionary<int, string?>(data.Iconos.Count);
        foreach (var i in data.Iconos) porId[i.ICONO_ID] = i.IMG_ICONO;

        foreach (var a in data.Aplicaciones)
            if (a.IMG_ICONO_APLICACION is null && a.ICONO_ID is int idApp && porId.TryGetValue(idApp, out var imgApp))
                a.IMG_ICONO_APLICACION = imgApp;

        foreach (var o in data.Opciones)
            if (o.IMG_ICONO_OPCION is null && o.ICONO_ID is int idOpc && porId.TryGetValue(idOpc, out var imgOpc))
                o.IMG_ICONO_OPCION = imgOpc;

        foreach (var b in data.Botones)
            if (b.IMG_ICONO is null && b.ICONO_ID is int idBtn && porId.TryGetValue(idBtn, out var imgBtn))
                b.IMG_ICONO = imgBtn;
    }

    /// <summary>
    /// Reconstruye la lista aplanada List&lt;BotonTreeviewItem&gt; (un item por botón, con
    /// colores/íconos de su app/opción ya resueltos) a partir de los 3 resultados
    /// deduplicados — así el resto de la clase (GetOpciones, GetArbolMenu, etc.) sigue
    /// funcionando exactamente igual que antes, sin cambios.
    /// </summary>
    private static List<BotonTreeviewItem> Fusionar(TreeviewResult r)
    {
        var apps = r.Aplicaciones.ToDictionary(a => a.COD_APLICACION);
        var opciones = r.Opciones.ToDictionary(o => (o.COD_APLICACION, o.COD_OPCION_APLICACION));

        var result = new List<BotonTreeviewItem>(r.Botones.Count);
        foreach (var b in r.Botones)
        {
            apps.TryGetValue(b.COD_APLICACION, out var app);
            opciones.TryGetValue((b.COD_APLICACION, b.COD_OPCION_APLICACION), out var op);

            result.Add(new BotonTreeviewItem
            {
                COD_APLICACION = b.COD_APLICACION,
                DES_APLICACION = app?.DES_APLICACION,
                COD_TIPO_OPCION_APLICACION = op?.COD_TIPO_OPCION_APLICACION,
                DES_TIPO_OPCION_APLICACION = op?.DES_TIPO_OPCION_APLICACION,
                COD_OPCION_APLICACION_PADRE = b.COD_OPCION_APLICACION_PADRE ?? op?.COD_OPCION_APLICACION_PADRE,
                COD_OPCION_APLICACION = b.COD_OPCION_APLICACION,
                DES_OPCION_APLICACION = op?.DES_OPCION_APLICACION,
                DES_FORMULARIO = op?.DES_FORMULARIO,
                COD_BOTON_OPCION = b.COD_BOTON_OPCION,
                DES_BOTON_OPCION = b.DES_BOTON_OPCION,
                DES_TOOLTIPTEXT = b.DES_TOOLTIPTEXT,
                DES_ALIAS = b.DES_ALIAS,
                FLG_HABILITADO = b.FLG_HABILITADO,
                FLG_HEADER = b.FLG_HEADER,
                FLG_DIALOG = b.FLG_DIALOG,
                FLG_GRILLA = b.FLG_GRILLA,
                FLG_ACCION_PUT = b.FLG_ACCION_PUT,
                FLG_ACCION_POST = b.FLG_ACCION_POST,
                FLG_ACCION_DELETE = b.FLG_ACCION_DELETE,
                FLG_ACCION_GET = b.FLG_ACCION_GET,
                DES_METODO = b.DES_METODO,
                NUM_ORDEN_PRESENTACION = b.NUM_ORDEN_PRESENTACION,
                DES_BACKCOLOR_APLICACION = app?.DES_BACKCOLOR_APLICACION ?? "",
                DES_FORECOLOR_APLICACION = app?.DES_FORECOLOR_APLICACION ?? "",
                IMG_ICONO_APLICACION = app?.IMG_ICONO_APLICACION,
                DES_BACKCOLOR_OPCION = op?.DES_BACKCOLOR_OPCION ?? "",
                DES_FORECOLOR_OPCION = op?.DES_FORECOLOR_OPCION ?? "",
                IMG_ICONO_OPCION = op?.IMG_ICONO_OPCION,
                FLG_EXIGE_PASSWORD = op?.FLG_EXIGE_PASSWORD
            });
        }
        return result;
    }

    /// <summary>
    /// Limpia la caché de botones del rol activo (llamar al cerrar sesión o cambiar rol).
    /// </summary>
    public async Task LimpiarCacheAsync()
    {
        if (string.IsNullOrWhiteSpace(_rolActivo)) return;
        try
        {
            await _localStorage.RemoveItemAsync($"botonesTreeview_{_rolActivo}");
            await _localStorage.RemoveItemAsync($"botonesTreeview_{_rolActivo}_ts");
        }
        catch { }
    }

    /// <summary>
    /// Opciones de menú autorizadas para el rol activo — derivadas de los botones
    /// ya cargados en memoria (cada opción que tenga al menos un botón habilitado
    /// con FLG_HABILITADO = true aparece en el menú).
    /// </summary>
    public List<OpcionMenu> GetOpciones()
    {
        // El acceso a la OPCIÓN en sí (si aparece en el menú) viene de
        // ROLES_USUARIOS_APLICACIONES_OPCIONES — toda fila del treeview para el
        // rol activo ya pasó ese filtro en el SP (INNER JOIN con esa tabla vía a).
        // FLG_HABILITADO es del BOTÓN, no de la opción: una opción sin ningún
        // botón configurado aún debe aparecer en el menú (con LEFT JOIN, esa fila
        // llega con FLG_HABILITADO=NULL) — por eso aquí NO se filtra por ese flag.
        return _botones
            .GroupBy(b => new { b.COD_APLICACION, b.COD_OPCION_APLICACION })
            .Select(g => new OpcionMenu
            {
                COD_ROL_USUARIO = _rolActivo ?? "",
                COD_APLICACION = g.Key.COD_APLICACION,
                DES_APLICACION = g.First().DES_APLICACION,
                COD_OPCION_APLICACION = g.Key.COD_OPCION_APLICACION,
                FLG_ACCESO_PERMITIDO = true,
                DES_FORMULARIO = g.First().DES_FORMULARIO,
                DES_OPCION_APLICACION = g.First().DES_OPCION_APLICACION,
                FLG_EXIGE_PASSWORD = g.First().FLG_EXIGE_PASSWORD
            })
            .ToList();
    }

    /// <summary>
    /// Construye el árbol jerárquico completo del menú lateral, agrupando por
    /// COD_OPCION_APLICACION_PADRE. Filtra SMD (no se muestra) y deduplica por
    /// opción (cada opción puede tener varias filas, una por botón).
    /// </summary>
    public List<NodoMenu> GetArbolMenu()
    {
        // Una fila por opción (no por botón) — el resto de columnas son iguales
        // dentro del mismo grupo, así que basta con tomar la primera.
        // SMD SÍ se incluye aquí (para no romper la jerarquía de sus hijos) —
        // es NodoMenuItem quien decide no renderizarlo visualmente y "subir"
        // sus hijos un nivel.
        // Clave compuesta para evitar colisiones entre aplicaciones que reusan
        // los mismos COD_OPCION_APLICACION (ej. SEGURIDAD y ESTANDAR ambos tienen 00, 01, 0100…).
        var opciones = _botones
            .GroupBy(b => new { b.COD_APLICACION, b.COD_OPCION_APLICACION })
            .Select(g => g.First())
            .ToList();

        var porCodigo = opciones.ToDictionary(
            o => $"{o.COD_APLICACION}|{o.COD_OPCION_APLICACION}");

        NodoMenu Construir(BotonTreeviewItem o) => new()
        {
            COD_OPCION_APLICACION      = o.COD_OPCION_APLICACION,
            COD_APLICACION             = o.COD_APLICACION,
            DES_APLICACION             = o.DES_APLICACION,
            DES_OPCION_APLICACION      = o.DES_OPCION_APLICACION,
            DES_FORMULARIO             = o.DES_FORMULARIO,
            COD_TIPO_OPCION_APLICACION = o.COD_TIPO_OPCION_APLICACION,
            DES_BACKCOLOR_APLICACION   = o.DES_BACKCOLOR_APLICACION,
            DES_FORECOLOR_APLICACION   = o.DES_FORECOLOR_APLICACION,
            IMG_ICONO_APLICACION       = o.IMG_ICONO_APLICACION,
            DES_BACKCOLOR_OPCION       = o.DES_BACKCOLOR_OPCION,
            DES_FORECOLOR_OPCION       = o.DES_FORECOLOR_OPCION,
            IMG_ICONO_OPCION           = o.IMG_ICONO_OPCION,
            FLG_EXIGE_PASSWORD         = o.FLG_EXIGE_PASSWORD,
            Hijos = opciones
                .Where(h => h.COD_APLICACION == o.COD_APLICACION
                         && h.COD_OPCION_APLICACION_PADRE == o.COD_OPCION_APLICACION)
                .Select(Construir)
                .ToList()
        };

        // Raíces: sin padre, o con un padre que no existe en el conjunto visible del rol.
        return opciones
            .Where(o => string.IsNullOrWhiteSpace(o.COD_OPCION_APLICACION_PADRE)
                     || !porCodigo.ContainsKey($"{o.COD_APLICACION}|{o.COD_OPCION_APLICACION_PADRE}"))
            .Select(Construir)
            .ToList();
    }

    /// <summary>
    /// Botones autorizados de UNA opción específica, filtrados en memoria de los
    /// datos ya cargados — no hace ninguna llamada nueva a la API.
    /// </summary>
    /// <summary>Botones habilitados de una opción, UNO POR CÓDIGO.
    ///
    /// El DistinctBy es la parte importante: un mismo botón puede llegar a _botones más de
    /// una vez —el treeview del login y el refresco por opción son dos fuentes, y el usuario
    /// con la misma opción por dos roles la trae repetida— y como AccionesBotones pinta un
    /// ícono por elemento, eso se veía como la columna Acciones con los botones duplicados
    /// (editar, eliminar, consultar, editar, eliminar, consultar).
    ///
    /// Se resuelve aquí, en la lectura, y no en cada lugar que arma la lista: así vale para
    /// la grilla, el header y la resolución de permisos por igual.</summary>
    public List<BotonTreeviewItem> GetBotonesPorOpcion(string codAplicacion, string codOpcionAplicacion) =>
        _botones
            .Where(b => b.COD_APLICACION == codAplicacion
                     && b.COD_OPCION_APLICACION == codOpcionAplicacion
                     && b.FLG_HABILITADO == true
                     && !string.IsNullOrEmpty(b.COD_BOTON_OPCION))   // descarta filas sin botón real (LEFT JOIN sin match)
            .DistinctBy(b => b.COD_BOTON_OPCION)
            .ToList();

    /// <summary>
    /// Refresca EN VIVO los botones de una sola opción contra la BD (endpoint por-opcion,
    /// sin pasar por el caché del treeview de login) y reemplaza esas entradas dentro de
    /// _botones. Llamado por AccionesBotones.razor en su OnInitializedAsync — así, con solo
    /// entrar a la pantalla, un permiso que el admin de seguridad cambió recién se ve
    /// reflejado, sin esperar a un relogueo ni al vencimiento del caché completo.
    /// Deduplicado por ventana de tiempo: si varias instancias de AccionesBotones (cabecera +
    /// filas de grilla) piden la MISMA opción en la MISMA carga de pantalla, solo la primera
    /// dispara la llamada HTTP y el resto espera esa misma Task.
    /// </summary>
    public Task AsegurarBotonesFrescosAsync(string codAplicacion, string codOpcionAplicacion)
    {
        // El SP PROC_SEGURIDAD_ROLES_USUARIOS_APLICACIONES_OPCIONES_BOTONES_SHOWALL ya trae
        // FLG_HEADER/FLG_GRILLA/FLG_ACCION_*/IMG_ICONO (joineados desde el catálogo
        // APLICACIONES_OPCIONES_BOTONES) — con eso el endpoint "por-opcion" alcanza para
        // refrescar SOLO la opción activa, sin traer el árbol completo cada vez.
        if (string.IsNullOrWhiteSpace(codAplicacion) || string.IsNullOrWhiteSpace(codOpcionAplicacion) || string.IsNullOrWhiteSpace(_rolActivo))
            return Task.CompletedTask;

        var key = $"{codAplicacion}|{codOpcionAplicacion}";

        if (_refrescosBotonesEnCurso.TryGetValue(key, out var enCurso))
            return enCurso;

        if (_ultimoRefrescoBotones.TryGetValue(key, out var ultimo) && DateTime.UtcNow - ultimo < VentanaDedupeRefrescoBotones)
            return Task.CompletedTask;

        var task = RefrescarBotonesDeOpcionAsync(codAplicacion, codOpcionAplicacion, key);
        _refrescosBotonesEnCurso[key] = task;
        return task;
    }

    private async Task RefrescarBotonesDeOpcionAsync(string codAplicacion, string codOpcionAplicacion, string key)
    {
        try
        {
            var url = $"{ApiRoutes.Seguridad.BotonesPorOpcion}?codRolUsuario={Uri.EscapeDataString(_rolActivo!)}" +
                      $"&codAplicacion={Uri.EscapeDataString(codAplicacion)}&codOpcion={Uri.EscapeDataString(codOpcionAplicacion)}";
            var resp = await _http.GetFromJsonAsync<ApiResponse<List<BotonPorOpcionItem>>>(url);

            var frescos = (resp?.Data ?? new()).Select(x => new BotonTreeviewItem
            {
                COD_APLICACION        = codAplicacion,
                COD_OPCION_APLICACION = codOpcionAplicacion,
                COD_BOTON_OPCION      = x.COD_BOTON_OPCION,
                DES_BOTON_OPCION      = x.DES_BOTON_OPCION,
                DES_TOOLTIPTEXT       = x.DES_TOOLTIPTEXT,
                DES_ALIAS             = x.DES_ALIAS,
                FLG_HABILITADO        = x.FLG_HABILITADO,
                FLG_HEADER            = x.FLG_HEADER,
                FLG_DIALOG            = x.FLG_DIALOG,
                FLG_GRILLA            = x.FLG_GRILLA,
                FLG_ACCION_PUT        = x.FLG_ACCION_PUT,
                FLG_ACCION_POST       = x.FLG_ACCION_POST,
                FLG_ACCION_DELETE     = x.FLG_ACCION_DELETE,
                FLG_ACCION_GET        = x.FLG_ACCION_GET,
                NUM_ORDEN_PRESENTACION = x.NUM_ORDEN_PRESENTACION,
                IMG_ICONO_BOTON       = x.IMG_ICONO is null ? null : Convert.ToBase64String(x.IMG_ICONO),
            }).ToList();

            // Guarda de seguridad: una respuesta vacía es ambigua (¿de verdad no hay botones
            // habilitados, o falló algo aguas abajo sin lanzar excepción?). Ante la duda, NO
            // se pisan los botones ya cacheados desde el treeview de login.
            if (frescos.Count > 0)
            {
                _botones = _botones
                    .Where(b => !(b.COD_APLICACION == codAplicacion && b.COD_OPCION_APLICACION == codOpcionAplicacion))
                    .Concat(frescos)
                    .ToList();
            }
            _ultimoRefrescoBotones[key] = DateTime.UtcNow;
        }
        catch
        {
            // Falla silenciosa: se mantienen los botones cacheados del treeview de login.
        }
        finally
        {
            _refrescosBotonesEnCurso.Remove(key);
        }
    }

    /// <summary>
    /// Resuelve los códigos VIGENTES (COD_APLICACION, COD_OPCION_APLICACION) de una opción a
    /// partir de su ruta/formulario (ej. "Estandar/ClasesOperacion.razor") — clave estable que
    /// no cambia si el usuario mueve la opción a otra carpeta con el botón "Mover" (eso sí
    /// cambia COD_OPCION_APLICACION, vía renumeración jerárquica en el SP de mover).
    /// A diferencia de los parámetros CodAplicacion/CodOpcionAplicacion que quedan fijados una
    /// sola vez al abrir la pestaña (según qué tile de menú se clickeó), esto siempre resuelve
    /// contra los datos de menú ya cargados en memoria — si el usuario cierra sesión y vuelve
    /// a entrar tras mover la opción, refleja la ubicación actual sin tocar código de página.
    /// </summary>
    public (string CodAplicacion, string CodOpcionAplicacion)? BuscarCodigosPorFormulario(string desFormulario)
    {
        var buscado = WorkspaceRegistry.Normalizar(desFormulario);
        var match = _opciones.FirstOrDefault(o => WorkspaceRegistry.Normalizar(o.DES_FORMULARIO ?? "") == buscado);
        return match is null ? null : (match.COD_APLICACION, match.COD_OPCION_APLICACION);
    }

    /// <summary>
    /// Igual que <see cref="BuscarCodigosPorFormulario"/> pero contra el catálogo COMPLETO de
    /// la aplicación (GET .../aplicaciones-opciones/by-app), no contra el treeview cacheado del
    /// rol activo. _opciones (usada por la versión en memoria) sale del SP TREEVIEW, que solo
    /// trae lo que ese rol tiene autorizado a ver en el menú lateral — una opción nueva, recién
    /// creada, sin permiso todavía otorgado a ese rol (o excluida por profundidad/otro motivo
    /// del SP), no aparece ahí aunque exista en el árbol de administración. Esta versión evita
    /// ese problema: pide el catálogo plano por aplicación (siempre completo, no filtrado por
    /// rol) y busca por ruta de formulario — sigue siendo dinámico (no depende de la posición
    /// en el árbol), solo que resuelve contra la fuente de verdad en vez del caché de menú.
    /// </summary>
    public async Task<(string CodAplicacion, string CodOpcionAplicacion)?> BuscarCodigosPorFormularioAsync(string codAplicacion, string desFormulario)
    {
        if (string.IsNullOrWhiteSpace(codAplicacion)) return null;
        try
        {
            var resp = await _http.GetFromJsonAsync<ApiResponse<List<AplicacionesOpcionesFormDto>>>(
                $"{ApiRoutes.Seguridad.AplicacionesOpciones}/by-app?codAplicacion={Uri.EscapeDataString(codAplicacion)}");
            var buscado = WorkspaceRegistry.Normalizar(desFormulario);
            var match = (resp?.Data ?? new()).FirstOrDefault(o => WorkspaceRegistry.Normalizar(o.DES_FORMULARIO ?? "") == buscado);
            return match is null ? null : (match.COD_APLICACION, match.COD_OPCION_APLICACION);
        }
        catch { return null; }
    }

    /// <summary>
    /// Refresca en memoria el DES_FORMULARIO de UNA opción puntual contra la BD (mismo endpoint
    /// que ya usan Clientes/Proveedores para leer su Parámetro), sin esperar al sondeo de
    /// ControlCambiosClientService ni al vencimiento del caché de 60 min del treeview completo
    /// (_botones/_opciones vienen de localStorage y pueden traer un DES_FORMULARIO viejo si el
    /// admin lo cambió hace poco y el sondeo de cambios no llegó a dispararse).
    /// Se usa como último intento cuando WorkspaceRegistry.PorRuta(...) no resuelve la ruta
    /// cacheada, antes de mostrar "pantalla no registrada". Devuelve el DES_FORMULARIO fresco,
    /// o null si no se pudo obtener.
    /// </summary>
    public async Task<string?> RefrescarFormularioDeOpcionAsync(string codAplicacion, string codOpcionAplicacion)
    {
        if (string.IsNullOrWhiteSpace(codAplicacion) || string.IsNullOrWhiteSpace(codOpcionAplicacion)) return null;
        try
        {
            var url = $"{ApiRoutes.Seguridad.AplicacionesOpciones}/{Uri.EscapeDataString(codAplicacion)}/{Uri.EscapeDataString(codOpcionAplicacion)}";
            var r = await _http.GetFromJsonAsync<ApiResponse<AplicacionesOpcionesFormDto>>(url);
            var fresco = r?.Data?.DES_FORMULARIO;
            if (string.IsNullOrWhiteSpace(fresco)) return null;

            foreach (var b in _botones)
                if (b.COD_APLICACION == codAplicacion && b.COD_OPCION_APLICACION == codOpcionAplicacion)
                    b.DES_FORMULARIO = fresco;
            foreach (var o in _opciones)
                if (o.COD_APLICACION == codAplicacion && o.COD_OPCION_APLICACION == codOpcionAplicacion)
                    o.DES_FORMULARIO = fresco;

            return fresco;
        }
        catch { return null; }
    }

    /// <summary>
    /// Resuelve una pantalla por ruta, con reintento automático usando el DES_FORMULARIO
    /// fresco de la opción si el valor cacheado no matchea ninguna pantalla registrada (ver
    /// RefrescarFormularioDeOpcionAsync). ÚNICO punto que todo llamador (menú lateral, tiles,
    /// menú circular, favoritos de Workspace) debería usar para abrir una opción por su ruta,
    /// en vez de llamar a WorkspaceRegistry.PorRuta(...) directo — así el reintento por caché
    /// vieja funciona sin importar desde qué pantalla se abrió la opción.
    /// </summary>
    public async Task<WorkspaceRegistry.PantallaInfo?> ResolverPantallaAsync(string? ruta, string? codAplicacion, string? codOpcionAplicacion)
    {
        var info = WorkspaceRegistry.PorRuta(ruta ?? "");
        if (info is not null) return info;
        if (string.IsNullOrWhiteSpace(codAplicacion) || string.IsNullOrWhiteSpace(codOpcionAplicacion)) return null;

        var fresco = await RefrescarFormularioDeOpcionAsync(codAplicacion, codOpcionAplicacion);
        return string.IsNullOrWhiteSpace(fresco) ? null : WorkspaceRegistry.PorRuta(fresco);
    }

    /// <summary>True si el rol activo tiene el verbo HTTP indicado habilitado para algún
    /// botón de esa opción — usado por las pantallas para mostrar/ocultar botones de acción.</summary>
    public bool PuedeAsync(string codAplicacion, string codOpcionAplicacion, string verbo)
    {
        var botones = GetBotonesPorOpcion(codAplicacion, codOpcionAplicacion);
        return verbo.ToUpperInvariant() switch
        {
            "GET" => botones.Any(b => b.FLG_ACCION_GET == true),
            "POST" => botones.Any(b => b.FLG_ACCION_POST == true),
            "PUT" => botones.Any(b => b.FLG_ACCION_PUT == true),
            "DELETE" => botones.Any(b => b.FLG_ACCION_DELETE == true),
            _ => false
        };
    }

    /// <summary>Limpia todo el estado en memoria (al cerrar sesión).</summary>
    public void Reset()
    {
        _botones = new();
        _roles = new();
        _rolActivo = null;
    }
}
