using EstandarEntidades      = KONSolutions.Web.Pages.Estandar.Entidades;
using EstandarMonedas        = KONSolutions.Web.Pages.Estandar.Monedas;
using EstandarMonedasTC      = KONSolutions.Web.Pages.Estandar.MonedasTipoCambio;
using KONSolutions.Web.Pages.Externo;
using KONSolutions.Web.Pages.Seguridad;
using KONSolutions.Web.Pages.Estandar;
using EstandarSexos = KONSolutions.Web.Pages.Estandar.Sexos;
using EstandarUbigeos = KONSolutions.Web.Pages.Estandar.Ubigeos;
using EstandarPaises = KONSolutions.Web.Pages.Estandar.Paises;
using KONSolutions.Web.Pages;
using KONSolutions.Web.Pages.Developer;
using MenuCircularPage = KONSolutions.Web.Pages.MenuCircularPage;
using KONSolutions.Web.Pages.Seguridad.Treeview;
using KONSolutions.Web.Pages.Ingenieria;
using KONSolutions.Web.Pages.Comercial;
using KONSolutions.Web.Pages.Controlcalidad;
using KONSolutions.Web.Pages.Entorno;
using KONSolutions.Web.Pages.Administracion;
using KONSolutions.Web.Pages.Contabilidad;
using MudBlazor;

namespace KONSolutions.Web.Services;

/// <summary>
/// Registro central de pantallas que pueden abrirse como pestañas.
/// Mapea una ruta a su tipo de componente, título e icono.
/// </summary>
public static class WorkspaceRegistry
{
    public record PantallaInfo(string Ruta, string Titulo, string Icono, Type ComponentType);

    public static readonly List<PantallaInfo> Pantallas = new()
    {
        // Seguridad
        new("/seguridad/usuarios",            "Usuarios",              Icons.Material.Filled.People,              typeof(Usuarios)),
        new("/seguridad/perfiles",            "Perfiles",              Icons.Material.Filled.Badge,               typeof(Perfiles)),
        new("/seguridad/roles",               "Roles",                 Icons.Material.Filled.AdminPanelSettings,  typeof(Roles)),
        new("/seguridad/aplicaciones",        "Aplicaciones",          Icons.Material.Filled.Apps,                typeof(Aplicaciones)),
        new("/seguridad/opciones-aplicaciones",          "Opciones Aplicaciones", Icons.Material.Filled.AccountTree, typeof(KONSolutions.Web.Pages.Seguridad.Treeview.OpcionesAplicaciones)),
        new("/seguridad/treeview/opciones-aplicaciones", "Opciones Aplicaciones", Icons.Material.Filled.AccountTree, typeof(KONSolutions.Web.Pages.Seguridad.Treeview.OpcionesAplicaciones)),
        new("/seguridad/opciones-menu",                 "Opciones Menú",         Icons.Material.Filled.Interests,   typeof(OpcionesMenu)),
        new("/seguridad/treeview/opciones-menu",        "Opciones Menú",         Icons.Material.Filled.Interests,   typeof(OpcionesMenu)),
        new("/seguridad/complejidad-password","Complejidad contraseña",Icons.Material.Filled.Password,            typeof(ComplejidadPassword)),
        new("/seguridad/botones-estandar",    "Botones Estándar",      Icons.Material.Filled.SmartButton,         typeof(BotonesEstandar)),
        new("/seguridad/asignacion-roles",    "Asignación de Roles",   Icons.Material.Filled.GroupAdd,            typeof(AsignacionRoles)),
        new("/seguridad/asignacion-opciones-roles", "Asignación de Opciones de Menú a Roles", Icons.Material.Filled.AccountTree, typeof(AsignacionOpcionesRoles)),
        new("/seguridad/tipos-opciones",      "Tipos de Opciones",     Icons.Material.Filled.AccountTree,         typeof(KONSolutions.Web.Pages.Seguridad.TiposOpciones)),
        // Estándar
        new("/estandar/configuracion-general",    "Configuración General",  Icons.Material.Filled.Tune,                typeof(ConfiguracionGeneral)),
        new("/estandar/configuracion-emision-electronica", "Configuración de Emisión Electrónica", Icons.Material.Filled.VerifiedUser, typeof(ConfiguracionEmisionElectronica)),
        new("/configuracion-general",             "Configuración General",  Icons.Material.Filled.Tune,                typeof(ConfiguracionGeneral)),
        new("/estandar/configuracion-usuario",    "Configuración de Usuario", Icons.Material.Filled.Person,             typeof(ConfiguracionUsuario)),
        new("/configuracion-usuario",              "Configuración de Usuario", Icons.Material.Filled.Person,             typeof(ConfiguracionUsuario)),
        new("/estandar/tipos-estados",        "Tipos de Estados",      Icons.Material.Filled.Label,               typeof(TiposEstados)),
        new("/estandar/tablas-tipos-estado",   "Tablas x Tipo de Estado", Icons.Material.Filled.TableChart,        typeof(TablasTiposEstado)),
        new("/estandar/estados-estandar",     "Estados Estándar",      Icons.Material.Filled.Style,               typeof(EstadosEstandar)),
        new("/estandar/tipos-motivos",          "Tipos de Motivos",              Icons.Material.Filled.Category,         typeof(TiposMotivos)),
        new("/estandar/tipos-menu-opciones",    "Tipos de Menú de Opciones",     Icons.Material.Filled.Category,         typeof(KONSolutions.Web.Pages.Estandar.TiposMenuOpciones)),
        new("/entorno/tipos-notificaciones",    "Tipos de Notificaciones",       Icons.Material.Filled.Notifications,    typeof(TiposNotificaciones)),
        new("/entorno/banco-colores",           "Banco de Colores",              Icons.Material.Filled.Palette,          typeof(BancoColores)),
        new("/entorno/diccionario-etiquetas",   "Diccionario de Etiquetas",      Icons.Material.Filled.Translate,        typeof(DiccionarioEtiquetas)),
        new("/administracion/tipos-cargo-administrativo", "Tipos de Cargo Administrativo", Icons.Material.Filled.Badge,   typeof(TiposCargoAdministrativo)),
        new("/administracion/cargos",            "Cargos",                        Icons.Material.Filled.Badge,           typeof(Cargos)),
        new("/administracion/areas",              "Áreas",                         Icons.Material.Filled.AccountTree,     typeof(Areas)),
        new("/administracion/areas-cargos-entidades", "Áreas · Cargos · Entidades", Icons.Material.Filled.AccountTree,     typeof(AreasCargosEntidades)),
        new("/contabilidad/tipos-centro-costo",   "Tipos de Centro de Costo",      Icons.Material.Filled.Category,        typeof(TiposCentroCosto)),
        new("/contabilidad/centros-costo",        "Centros de Costo",              Icons.Material.Filled.AccountTree,     typeof(CentrosCosto)),
        new("/contabilidad/tipos-cuenta-contable", "Tipos de Cuenta Contable",     Icons.Material.Filled.Category,        typeof(TiposCuentaContable)),
        new("/contabilidad/cuentas-naturaleza",   "Naturaleza de las Cuentas",     Icons.Material.Filled.Balance,         typeof(CuentasNaturaleza)),
        new("/contabilidad/cuentas-contables",    "Plan de Cuentas",               Icons.Material.Filled.AccountTree,     typeof(CuentasContables)),
        new("/contabilidad/asientos-contables",   "Tipos de Libro Contable (Asiento)", Icons.Material.Filled.MenuBook,    typeof(AsientosContables)),
        new("/finanzas/bancos",                   "Bancos",                        Icons.Material.Filled.AccountBalance,  typeof(KONSolutions.Web.Pages.Finanzas.Bancos)),
        new("/finanzas/tipos-cuentas-bancarias",  "Tipos de Cuenta Bancaria",      Icons.Material.Filled.CreditCard,      typeof(KONSolutions.Web.Pages.Finanzas.TiposCuentasBancarias)),
        new("/logistica/flujos-aprobacion",       "Flujos de Aprobación",          Icons.Material.Filled.AccountTree,     typeof(KONSolutions.Web.Pages.Logistica.FlujosAprobacion)),
        new("/entorno/entidades-configurar",      "Entidades a Configurar",        Icons.Material.Filled.Settings,        typeof(EntidadesConfigurar)),
        new("/estandar/opciones-procesos-estados", "Configuración de Flujos de los Estados", Icons.Material.Filled.AccountTree, typeof(OpcionesProcesosEstados)),
        new("/estandar/tipos-configuracion",      "Tipos de Configuración de Objetos", Icons.Material.Filled.Tune,        typeof(TiposConfiguracion)),
        // Fase 1: solo cabecera de COMERCIAL.PEDIDOS_COMERCIALES. Reutilizable para
        // Proformas / Órdenes de Compra / Pedidos de Venta vía COD_CLASE_OPERACION en
        // DES_PARAMETRO — cada opción de menú necesita su PROPIO Formulario/ruta (no
        // "Comercial/PedidosComerciales.razor" repetido) para no chocar de pestaña, mismo
        // criterio que Entidades/Clientes/Proveedores.
        new("/comercial/pedidos-comerciales",     "Pedidos Comerciales",           Icons.Material.Filled.Assignment,      typeof(PedidosComerciales)),
        // Registro de Compras: comprobantes de proveedores asociados a sus órdenes y validados contra ellas. DES_FORMULARIO de la opción: "Comercial/RegistroCompras.razor"; la clase de operación sale de su parámetro.
        // Bandejas de consulta de lo que cargo el servicio de XML recibidos. DES_FORMULARIO de las opciones: "Comercial/CPE_Recibidos.razor" y "Logistica/GRE_Recibidas.razor".
        new("/comercial/cpe-recibidos",   "Bandeja de XML recibidos",      Icons.Material.Filled.MarkEmailRead,   typeof(KONSolutions.Web.Pages.Comercial.CPE_Recibidos)),
        // Normalizar() convierte "CPE_Recibidos" en "cpe_-recibidos" (PascalToKebab pone el guion tras el guion bajo): se registran los alias.
        new("/logistica/gre-recibidas", "Bandeja de guías recibidas",   Icons.Material.Filled.LocalShipping,   typeof(KONSolutions.Web.Pages.Logistica.GRE_Recibidas)),
        new("/comercial/cpe_-recibidos", "Bandeja de XML recibidos", Icons.Material.Filled.MarkEmailRead, typeof(KONSolutions.Web.Pages.Comercial.CPE_Recibidos)),
        new("/comercial/cpe_recibidos", "Bandeja de XML recibidos", Icons.Material.Filled.MarkEmailRead, typeof(KONSolutions.Web.Pages.Comercial.CPE_Recibidos)),
        new("/logistica/gre_-recibidas", "Bandeja de guías recibidas", Icons.Material.Filled.LocalShipping, typeof(KONSolutions.Web.Pages.Logistica.GRE_Recibidas)),
        new("/logistica/gre_recibidas", "Bandeja de guías recibidas", Icons.Material.Filled.LocalShipping, typeof(KONSolutions.Web.Pages.Logistica.GRE_Recibidas)),
        new("/comercial/registro-compras",        "Registro de Compras",           Icons.Material.Filled.ReceiptLong,     typeof(KONSolutions.Web.Pages.Comercial.RegistroCompras)),
        new("/comercial/proformas",               "Proformas",                     Icons.Material.Filled.Assignment,      typeof(PedidosComerciales)),
        // Rutas propias opcionales: con la identidad de pestaña por opción ya no hacen falta
        // para que cada opción abra su pestaña, pero quedan disponibles por si preferís que
        // cada opción tenga su DES_FORMULARIO propio en SEGURIDAD.APLICACIONES_OPCIONES.
        new("/comercial/ordenes-compra",          "Órdenes de Compra",             Icons.Material.Filled.Assignment,      typeof(PedidosComerciales)),
        new("/comercial/pedidos-venta",           "Pedidos de Venta",              Icons.Material.Filled.Assignment,      typeof(PedidosComerciales)),
        new("/comercial/incoterms-fases",         "Fases de Incoterm",             Icons.Material.Filled.Flight,          typeof(IncotermsFases)),
        new("/comercial/incoterms-tipos-transporte", "Incoterms - Tipos de Transporte", Icons.Material.Filled.LocalShipping, typeof(IncotermsTiposTransporte)),
        new("/comercial/tipos-solicitud-comercial-areas", "Áreas por Tipo de Solicitud Comercial", Icons.Material.Filled.AccountTree, typeof(TiposSolicitudComercialAreas)),
        new("/comercial/tipos-solicitud-comercial-areas-cargos", "Cargos por Área de Solicitud Comercial", Icons.Material.Filled.Badge, typeof(TiposSolicitudComercialAreasCargos)),
        new("/logistica/transportes",             "Transportes",                   Icons.Material.Filled.LocalShipping,   typeof(KONSolutions.Web.Pages.Logistica.Transportes)),
        new("/logistica/tipos-transporte",        "Tipos de Transporte",           Icons.Material.Filled.LocalShipping,   typeof(KONSolutions.Web.Pages.Logistica.TiposTransporte)),
        new("/logistica/inventarios",             "Inventarios",                   Icons.Material.Filled.Inventory,       typeof(KONSolutions.Web.Pages.Logistica.Inventarios)),
        new("/logistica/tipos-inventario",        "Tipos de Inventario",           Icons.Material.Filled.Inventory,       typeof(KONSolutions.Web.Pages.Logistica.TiposInventario)),
        new("/entorno/servidores",              "Servidores",                    Icons.Material.Filled.Dns,              typeof(Servidores)),
        new("/entorno/tipos-lectura",           "Tipos de Lectura",              Icons.Material.Filled.QrCodeScanner,    typeof(TiposLectura)),
        new("/entorno/path-files",               "Rutas de Archivos",             Icons.Material.Filled.Folder,           typeof(PathFiles)),
        new("/entorno/postits",                 "Post-its",                      Icons.Material.Filled.PushPin,          typeof(PostItsBoard)),
        new("/entorno/notificaciones-consulta", "Notificaciones",                Icons.Material.Filled.Notifications,    typeof(NotificacionesConsulta)),
        // Aprobaciones por opción de menú (ENTORNO.APLICACIONES_OPCIONES_APROBACIONES).
        // La opción del menú tiene DES_FORMULARIO = "Entorno/AplicacionesOpcionesAprobaciones.razor",
        // que Normalizar() convierte en "/entorno/aplicaciones-opciones-aprobaciones".
        // Se registra además el alias con el que figura la opción en el menú ("Configuración
        // de Aprobaciones") por si el DES_FORMULARIO se cambia por ese nombre.
        new("/entorno/aplicaciones-opciones-aprobaciones", "Aprobaciones por Opción", Icons.Material.Filled.FactCheck, typeof(AplicacionesOpcionesAprobaciones)),
        new("/entorno/configuracion-aprobaciones",         "Aprobaciones por Opción", Icons.Material.Filled.FactCheck, typeof(AplicacionesOpcionesAprobaciones)),
        new("/estandar/tipos-comprobantes-pago", "Tipos de Comprobante de Pago", Icons.Material.Filled.ReceiptLong,      typeof(KONSolutions.Web.Pages.Estandar.TiposComprobantesPago)),
        new("/estandar/tipos-entidad",          "Tipos de Entidad",              Icons.Material.Filled.Category,         typeof(TiposEntidad)),
        new("/estandar/tipos-entidades",        "Tipos de Entidad",              Icons.Material.Filled.Category,         typeof(TiposEntidad)),
        new("/estandar/tipos-vias",             "Tipos de Vías",                 Icons.Material.Filled.Signpost,         typeof(TiposVias)),
        new("/estandar/tipos-via",              "Tipos de Vías",                 Icons.Material.Filled.Signpost,         typeof(TiposVias)),
        new("/estandar/tipos-documento-identidad", "Tipos Documento Identidad",  Icons.Material.Filled.Badge,            typeof(TipoDocumentoIdentidad)),
        new("/estandar/tipo-documento-identidad",  "Tipos Documento Identidad",  Icons.Material.Filled.Badge,            typeof(TipoDocumentoIdentidad)),
        new("/estandar/sexos",                  "Sexos",                         Icons.Material.Filled.People,           typeof(EstandarSexos)),
        new("/estandar/tipos-personas",         "Tipos de Persona",              Icons.Material.Filled.PersonOutline,    typeof(KONSolutions.Web.Pages.Estandar.TiposPersonas)),
        // Alias: el árbol de menú en BD tiene DES_FORMULARIO = "Estandar/TiposPersona.razor" (singular).
        new("/estandar/tipos-persona",          "Tipos de Persona",              Icons.Material.Filled.PersonOutline,    typeof(KONSolutions.Web.Pages.Estandar.TiposPersonas)),
        new("/estandar/anos",                   "Años",                          Icons.Material.Filled.CalendarToday,    typeof(KONSolutions.Web.Pages.Estandar.Anos)),
        new("/estandar/meses",                  "Meses",                         Icons.Material.Filled.CalendarMonth,    typeof(KONSolutions.Web.Pages.Estandar.Meses)),
        new("/estandar/ubigeos",                "Ubigeos",                       Icons.Material.Filled.LocationOn,       typeof(EstandarUbigeos)),
        new("/estandar/paises",                 "Países",                        Icons.Material.Filled.Flag,             typeof(EstandarPaises)),
        new("/estandar/entidades",              "Entidades",      Icons.Material.Filled.Business,         typeof(EstandarEntidades)),
        // Mismo componente (Entidades.razor) que la ruta genérica de arriba — el ÚNICO motivo
        // de tener rutas separadas por catálogo es que las pestañas se identifican por Ruta
        // (ver WorkspaceTabService.Abrir): si Clientes y Proveedores compartieran
        // "/estandar/entidades", abrir uno reactivaría la pestaña del otro en vez de abrir la
        // suya (con SUS CodAplicacion/CodOpcionAplicacion). El Formulario configurado en
        // SEGURIDAD.APLICACIONES_OPCIONES para cada opción es solo un texto que normaliza a
        // esta ruta — NO hace falta que exista un archivo .razor con ese nombre.
        new("/estandar/entidades-clientes",     "Catálogo de Clientes",    Icons.Material.Filled.Business,        typeof(EstandarEntidades)),
        new("/estandar/entidades-proveedores",  "Catálogo de Proveedores", Icons.Material.Filled.LocalShipping,   typeof(EstandarEntidades)),
        new("/entidades",                       "Entidades",      Icons.Material.Filled.Business,         typeof(EstandarEntidades)),
        new("/estandar/tipos-producto",           "Tipos de Producto",               Icons.Material.Filled.Category,      typeof(TiposProducto)),
        new("/estandar/clases-operacion",         "Clases de Operación",             Icons.Material.Filled.Category,      typeof(KONSolutions.Web.Pages.Estandar.ClasesOperacion)),
        new("/estandar/tipos-periodo",            "Tipos de Periodo",                Icons.Material.Filled.DateRange,     typeof(KONSolutions.Web.Pages.Estandar.TiposPeriodo)),
        new("/estandar/tipos-operacion",          "Tipos de Operación",              Icons.Material.Filled.Category,      typeof(KONSolutions.Web.Pages.Estandar.TiposOperacion)),
        new("/estandar/tiposproducto",            "Tipos de Producto",               Icons.Material.Filled.Category,      typeof(TiposProducto)),
        new("/estandar/grupos-producto",          "Grupos de Producto",              Icons.Material.Filled.Folder,        typeof(GruposProducto)),
        new("/estandar/gruposproducto",           "Grupos de Producto",              Icons.Material.Filled.Folder,        typeof(GruposProducto)),
        new("/estandar/grupos-productos",         "Grupos de Producto",              Icons.Material.Filled.Folder,        typeof(GruposProducto)),
        new("/estandar/gruposproductos",          "Grupos de Producto",              Icons.Material.Filled.Folder,        typeof(GruposProducto)),
        new("/estandar/familias-producto",        "Grupos de Producto",              Icons.Material.Filled.Folder,        typeof(GruposProducto)),
        new("/estandar/familiasproducto",         "Grupos de Producto",              Icons.Material.Filled.Folder,        typeof(GruposProducto)),
        new("/estandar/familias-productos",       "Grupos de Producto",              Icons.Material.Filled.Folder,        typeof(GruposProducto)),
        new("/estandar/familiasproductos",        "Grupos de Producto",              Icons.Material.Filled.Folder,        typeof(GruposProducto)),
        new("/estandar/sub-familias-producto",    "Grupos de Producto",              Icons.Material.Filled.Folder,        typeof(GruposProducto)),
        new("/estandar/sub-familias-productos",   "Grupos de Producto",              Icons.Material.Filled.Folder,        typeof(GruposProducto)),
        new("/estandar/subfamiliasproducto",      "Grupos de Producto",              Icons.Material.Filled.Folder,        typeof(GruposProducto)),
        new("/estandar/subfamiliasproductos",     "Grupos de Producto",              Icons.Material.Filled.Folder,        typeof(GruposProducto)),
        new("/estandar/magnitudes",             "Magnitudes / Unidades de Medida", Icons.Material.Filled.Straighten,    typeof(Magnitudes)),
        new("/estandar/equivalencias",          "Equivalencias de Unidades",        Icons.Material.Filled.CompareArrows, typeof(Equivalencias)),
        new("/estandar/monedas",               "Monedas",        Icons.Material.Filled.AttachMoney,      typeof(EstandarMonedas)),
        new("/monedas",                         "Monedas",        Icons.Material.Filled.AttachMoney,      typeof(EstandarMonedas)),
        new("/estandar/monedas-tipo-cambio",   "Tipo de Cambio", Icons.Material.Filled.CurrencyExchange, typeof(EstandarMonedasTC)),
        new("/tipo-cambio",                     "Tipo de Cambio", Icons.Material.Filled.CurrencyExchange, typeof(EstandarMonedasTC)),
        new("/monedas-tipo-cambio",             "Tipo de Cambio", Icons.Material.Filled.CurrencyExchange, typeof(EstandarMonedasTC)),
        // Ingeniería
        new("/ingenieria/tipos-especificacion-tecnica",  "Tipos de Especificación Técnica", Icons.Material.Filled.Rule, typeof(TiposEspecificacionTecnica)),
        new("/ingenieria/tiposespecificaciontecnica",    "Tipos de Especificación Técnica", Icons.Material.Filled.Rule, typeof(TiposEspecificacionTecnica)),
        new("/ingenieria/especificaciones-tecnicas",     "Especificaciones Técnicas", Icons.Material.Filled.AccountTree, typeof(EspecificacionesTecnicas)),
        new("/ingenieria/especificacionestecnicas",      "Especificaciones Técnicas", Icons.Material.Filled.AccountTree, typeof(EspecificacionesTecnicas)),
        new("/ingenieria/registro-items-tecnicos",       "Registro de Ítems Técnicos", Icons.Material.Filled.AccountTree, typeof(RegistroItemsTecnicos)),
        new("/ingenieria/grupos-especificacion-tecnica", "Asignación de Especificaciones Técnicas a Grupos", Icons.Material.Filled.AccountTree, typeof(GruposEspecificacionTecnica)),
        new("/ingenieria/tipos-diseno",                  "Tipos de Diseño", Icons.Material.Filled.DesignServices, typeof(KONSolutions.Web.Pages.Ingenieria.TiposDiseno)),
        new("/ingenieria/disenos",                       "Diseños", Icons.Material.Filled.DesignServices, typeof(KONSolutions.Web.Pages.Ingenieria.Disenos)),
        // Operaciones y tiempos estandar (scripts SQL 119 a 122). DES_FORMULARIO de las opciones: "Ingenieria/CategoriasOcupacionales.razor", "Ingenieria/TiposMaquina.razor",
        // "Ingenieria/MetodosTiempo.razor" y "Produccion/TiposDistribucionLinea.razor" (las cuatro abren OperacionesCatalogos en su pestana), "Ingenieria/Operaciones.razor" y "Ingenieria/RutasTiempos.razor".
        new("/ingenieria/categorias-ocupacionales",      "Categorías ocupacionales", Icons.Material.Filled.Category, typeof(OperacionesCatalogos)),
        new("/ingenieria/tipos-maquina",                 "Tipos de máquina", Icons.Material.Filled.PrecisionManufacturing, typeof(OperacionesCatalogos)),
        new("/ingenieria/metodos-tiempo",                "Métodos de tiempo", Icons.Material.Filled.Timer, typeof(OperacionesCatalogos)),
        new("/produccion/tipos-distribucion-linea",      "Distribución de línea", Icons.Material.Filled.ViewModule, typeof(OperacionesCatalogos)),
        new("/ingenieria/operaciones",                   "Operaciones", Icons.Material.Filled.AccountTree, typeof(Operaciones)),
        new("/ingenieria/rutas-tiempos",                 "Rutas con tiempos", Icons.Material.Filled.AltRoute, typeof(RutasTiempos)),
        new("/ingenieria/matrices",                      "Matrices de operaciones", Icons.Material.Filled.AccountTree, typeof(Matrices)),
        // Personal por categoria y armado de lineas (scripts SQL 124 a 126). DES_FORMULARIO: "Ingenieria/PersonalOperacion.razor" y "Ingenieria/ArmadoLineas.razor".
        new("/ingenieria/personal-operacion",            "Personal por categoría", Icons.Material.Filled.Badge, typeof(PersonalOperacion)),
        new("/ingenieria/armado-lineas",                 "Armado de líneas", Icons.Material.Filled.ViewWeek, typeof(ArmadoLineas)),
        // Migracion de operaciones de rutas existentes (script SQL 128 y menu 129). DES_FORMULARIO: "Ingenieria/MigracionOperaciones.razor".
        new("/ingenieria/migracion-operaciones",         "Migración de operaciones", Icons.Material.Filled.MoveUp, typeof(MigracionOperaciones)),
        new("/ingenieria/grupos-especificacion-tecnica-grupos", "Grupos de Especificación Técnica", Icons.Material.Filled.Category, typeof(KONSolutions.Web.Pages.Ingenieria.GruposEspecificacionTecnicaGrupos)),
        // Matriz de Especificaciones Técnicas. Van las dos formas del nombre porque
        // Normalizar() convierte DES_FORMULARIO a kebab-case y no sabemos con cuál de las
        // dos quedará cargada la opción en el menú: sin el alias correcto, la pantalla
        // existe pero el menú responde "No se pudo abrir la pantalla".
        new("/ingenieria/matriz-especificaciones-tecnicas", "Matriz de Especificaciones Técnicas", Icons.Material.Filled.GridOn, typeof(KONSolutions.Web.Pages.Ingenieria.MatrizEspecificacionesTecnicas)),
        new("/ingenieria/matrizespecificacionestecnicas",   "Matriz de Especificaciones Técnicas", Icons.Material.Filled.GridOn, typeof(KONSolutions.Web.Pages.Ingenieria.MatrizEspecificacionesTecnicas)),
        // Control de Calidad
        new("/controlcalidad/clases-defectos-tecnicos",  "Clases de Defectos Técnicos", Icons.Material.Filled.ReportProblem, typeof(ClasesDefectosTecnicos)),
        new("/controlcalidad/clasesdefectostecnicos",    "Clases de Defectos Técnicos", Icons.Material.Filled.ReportProblem, typeof(ClasesDefectosTecnicos)),
        new("/controlcalidad/tipos-defectos-tecnicos",   "Clases de Defectos Técnicos", Icons.Material.Filled.ReportProblem, typeof(ClasesDefectosTecnicos)),
        new("/controlcalidad/tiposdefectostecnicos",     "Clases de Defectos Técnicos", Icons.Material.Filled.ReportProblem, typeof(ClasesDefectosTecnicos)),
        new("/ingenieria/tipos-defecto-tecnico",          "Clases de Defectos Técnicos", Icons.Material.Filled.ReportProblem, typeof(ClasesDefectosTecnicos)),
        new("/ingenieria/tiposdefectotecnico",            "Clases de Defectos Técnicos", Icons.Material.Filled.ReportProblem, typeof(ClasesDefectosTecnicos)),
        // Comercial
        // Purchase orders del cliente. DES_FORMULARIO = "Comercial/EntidadesPurchaseOrders.razor".
        new("/comercial/entidades-purchase-orders", "Purchase Orders", Icons.Material.Filled.Inventory2, typeof(KONSolutions.Web.Pages.Comercial.EntidadesPurchaseOrders)),
        new("/comercial/purchase-orders",           "Purchase Orders", Icons.Material.Filled.Inventory2, typeof(KONSolutions.Web.Pages.Comercial.EntidadesPurchaseOrders)),
        // Alias: la opción en BD quedó configurada con DES_FORMULARIO en SINGULAR
        // ("Comercial/EntidadesPurchaseOrder.razor"), que normaliza a esta ruta. Mismo
        // criterio que Lotes de Control y Punto de Reorden, que también tienen el nombre
        // en singular en el árbol de menú y en plural en la tabla.
        new("/comercial/entidades-purchase-order",  "Purchase Orders", Icons.Material.Filled.Inventory2, typeof(KONSolutions.Web.Pages.Comercial.EntidadesPurchaseOrders)),
        new("/comercial/entidades-temporadas",   "Temporadas por Entidad", Icons.Material.Filled.Style,             typeof(EntidadesTemporadas)),
        new("/comercial/tipos-precio",           "Tipos de Precio",        Icons.Material.Filled.Sell,             typeof(TiposPrecio)),
        new("/comercial/tipos-pedido-comercial", "Tipos de Pedido Comercial", Icons.Material.Filled.ShoppingCart,  typeof(TiposPedidoComercial)),
        new("/comercial/condiciones-pago",       "Condiciones de Pago",    Icons.Material.Filled.CreditScore,      typeof(CondicionesPago)),
        new("/comercial/formas-pago",            "Formas de Pago",         Icons.Material.Filled.Payments,         typeof(FormasPago)),
        new("/comercial/tipos-transporte",       "Tipos de Transporte",    Icons.Material.Filled.LocalShipping,    typeof(ComercialTiposTransporte)),
        new("/comercial/fases-incoterm",         "Fases de Incoterm",      Icons.Material.Filled.Timeline,         typeof(FasesIncoterm)),
        new("/comercial/tipos-descuento",        "Tipos de Descuento",     Icons.Material.Filled.Discount,         typeof(TiposDescuento)),
        new("/comercial/incoterms",              "Incoterms",              Icons.Material.Filled.Public,           typeof(Incoterms)),
        new("/comercial/especificaciones-items-tecnicos", "Especificaciones / Ítems Técnicos", Icons.Material.Filled.AccountTree, typeof(EspecificacionesItemsTecnicos)),
        new("/comercial/tipos-pos",              "Tipos de POS",           Icons.Material.Filled.PointOfSale,       typeof(TiposPos)),
        new("/comercial/tipos-impuesto",         "Tipos de Impuesto",      Icons.Material.Filled.RequestQuote,      typeof(TiposImpuesto)),
        new("/comercial/tipos-valor-calculo",    "Tipos de Valor de Cálculo", Icons.Material.Filled.Calculate,      typeof(TiposValorCalculo)),
        // Alias: el menú de la app tiene configurado DES_FORMULARIO = "Comercial/ValoresCalculo.razor"
        new("/comercial/valores-calculo",        "Tipos de Valor de Cálculo", Icons.Material.Filled.Calculate,      typeof(TiposValorCalculo)),
        new("/comercial/tipos-medio-contacto",   "Tipos de Medio de Contacto", Icons.Material.Filled.ContactMail,   typeof(TiposMedioContacto)),
        new("/comercial/tipos-direccion",        "Tipos de Dirección",     Icons.Material.Filled.Home,              typeof(TiposDireccion)),
        new("/comercial/solicitudes-comerciales", "Solicitudes Comerciales", Icons.Material.Filled.RequestPage,      typeof(KONSolutions.Web.Pages.Comercial.SolicitudesComerciales)),
        new("/comercial/solicitudes-comerciales-cliente", "Mis Solicitudes Comerciales", Icons.Material.Filled.RequestPage, typeof(KONSolutions.Web.Pages.Comercial.SolicitudesComercialesCliente)),
        new("/comercial/solicitudes-comerciales-dependencia", "Solicitudes de mi Dependencia", Icons.Material.Filled.RequestPage, typeof(KONSolutions.Web.Pages.Comercial.SolicitudesComercialesDependencia)),
        new("/comercial/tipos-solicitud-comercial", "Tipos de Solicitud Comercial", Icons.Material.Filled.Category, typeof(KONSolutions.Web.Pages.Comercial.TiposSolicitudComercial)),
        // Una sola pantalla con dos pestañas: productos por proveedor y proveedores por
        // producto. Se registran los dos nombres porque el menú puede entrar por cualquiera.
        new("/comercial/comparativo-compras",  "Comparativo de Compras", Icons.Material.Filled.Balance,     typeof(KONSolutions.Web.Pages.Comercial.ComparativoCompras)),
        new("/comercial/entidades-productos",    "Productos por Proveedor",  Icons.Material.Filled.Inventory2,     typeof(KONSolutions.Web.Pages.Comercial.EntidadesProductos)),
        new("/comercial/productos-entidades",    "Proveedores por Producto", Icons.Material.Filled.LocalShipping,  typeof(KONSolutions.Web.Pages.Comercial.EntidadesProductos)),
        new("/estandar/tipos-sucursales",        "Tipos de Sucursales",    Icons.Material.Filled.Storefront,        typeof(KONSolutions.Web.Pages.Estandar.TiposSucursales)),
        new("/estandar/sucursales",              "Sucursales",             Icons.Material.Filled.Business,          typeof(KONSolutions.Web.Pages.Estandar.Sucursales)),

        // Sistema
        new("/sistema/dashboard-monitoreo-bd",   "Monitoreo de BD",        Icons.Material.Filled.Storage,          typeof(KONSolutions.Web.Pages.Sistema.DashboardMonitoreoBD)),
        new("/sistema/genera-sps-text",          "Generar TXT de SPs",     Icons.Material.Filled.TextSnippet,      typeof(KONSolutions.Web.Pages.Sistema.GeneraSPsText)),
        new("/sistemas/genera-sps-text",         "Generar TXT de SPs",     Icons.Material.Filled.TextSnippet,      typeof(KONSolutions.Web.Pages.Sistema.GeneraSPsText)),
        new("/developer/logviewer",              "Log de Errores",         Icons.Material.Filled.BugReport,        typeof(LogViewer)),
        new("/developer/log-viewer",             "Log de Errores",         Icons.Material.Filled.BugReport,        typeof(LogViewer)),
        new("/seguridad/configuracion",          "Configuración",          Icons.Material.Filled.Settings,         typeof(Configuracion)),
        new("/seguridad/registro-facial-seguridad", "Registro Facial", Icons.Material.Filled.FaceRetouchingNatural, typeof(RegistroFacialSeguridad)),
        // Externo
        new("/externo/snippets",   "Snippets",  Icons.Material.Filled.Code,  typeof(Snippets)),
        new("/estandar/snippets",  "Snippets",  Icons.Material.Filled.Code,  typeof(Snippets)),
        // Sunat
        new("/sunat/regimenes-tributarios", "Regímenes Tributarios", Icons.Material.Filled.RequestQuote, typeof(KONSolutions.Web.Pages.Sunat.RegimenesTributarios)),
        // Menú circular
        new("/menu-circular", "Menú Circular", Icons.Material.Filled.DonutLarge, typeof(MenuCircularPage)),
        // Menú de tiles
        new("/menu-tiles", "Menú de Tiles", Icons.Material.Filled.GridView, typeof(KONSolutions.Web.Pages.MenuTilesPage)),
        // Producción
        new("/produccion/clases-proceso",          "Clases de Proceso",          Icons.Material.Filled.Category,     typeof(KONSolutions.Web.Pages.Produccion.ClasesProceso)),
        new("/produccion/procesos",                "Procesos",                   Icons.Material.Filled.AccountTree,  typeof(KONSolutions.Web.Pages.Produccion.Procesos)),
        new("/zonificacion/plano",                 "Plano de Zonificación",      Icons.Material.Filled.Architecture, typeof(KONSolutions.Web.Pages.Zonificacion.PlanoZonificacion)),
        // Alias: el menú en BD tiene DES_FORMULARIO = "Zonificacion/PlanoZonificacion.razor",
        // que normaliza a /zonificacion/plano-zonificacion (nombre del componente, no la @page).
        new("/zonificacion/plano-zonificacion",    "Plano de Zonificación",      Icons.Material.Filled.Architecture, typeof(KONSolutions.Web.Pages.Zonificacion.PlanoZonificacion)),
        // Alias por si la opción se registra en BD bajo el módulo Logística.
        new("/logistica/plano-zonificacion",       "Plano de Zonificación",      Icons.Material.Filled.Architecture, typeof(KONSolutions.Web.Pages.Zonificacion.PlanoZonificacion)),
        new("/zonificacion/tipos-zonificacion",    "Tipos de Zonificación",      Icons.Material.Filled.Map,          typeof(KONSolutions.Web.Pages.Zonificacion.TiposZonificacion)),
        // Alias: el árbol de menú en BD tiene DES_FORMULARIO = "Estandar/TiposZonificacion.razor",
        // aunque la pantalla vive en Pages/Zonificacion (el schema de la tabla es ZONIFICACION).
        new("/estandar/tipos-zonificacion",        "Tipos de Zonificación",      Icons.Material.Filled.Map,          typeof(KONSolutions.Web.Pages.Zonificacion.TiposZonificacion)),
        new("/logistica/almacenes",                "Almacenes",                  Icons.Material.Filled.Warehouse,    typeof(KONSolutions.Web.Pages.Logistica.Almacenes)),
        new("/logistica/contenedores-almacenaje",  "Contenedores de Almacenaje", Icons.Material.Filled.Inventory2,   typeof(KONSolutions.Web.Pages.Logistica.ContenedoresAlmacenaje)),
        new("/logistica/tipos-ubicacion",          "Tipos de Ubicación",         Icons.Material.Filled.PinDrop,      typeof(KONSolutions.Web.Pages.Logistica.TiposUbicacion)),
        new("/logistica/tipos-almacen",            "Tipos de Almacén",           Icons.Material.Filled.Category,     typeof(KONSolutions.Web.Pages.Logistica.TiposAlmacen)),
        new("/logistica/almacenes-cierre",         "Cierre de Almacenes",        Icons.Material.Filled.EventBusy,    typeof(KONSolutions.Web.Pages.Logistica.AlmacenesCierre)),
        // Módulo RFID (aplicación aparte). DES_FORMULARIO de cada opción: "Rfid/Emisiones.razor" y "Rfid/Etiquetas.razor" (los equipos son productos: se registran en Productos).
        new("/rfid/emisiones",                     "Emisión de TAG RFID",        Icons.Material.Filled.Nfc,          typeof(KONSolutions.Web.Pages.Rfid.Emisiones)),
        new("/rfid/etiquetas",                     "Etiquetas RFID",             Icons.Material.Filled.Sell,         typeof(KONSolutions.Web.Pages.Rfid.Etiquetas)),
        // Requerimientos y Reservas de stock. Las dos pantallas de detalle son consultas
        // transversales de solo lectura: los ítems se cargan dentro de su documento.
        new("/logistica/tipos-requerimiento",      "Tipos de Requerimiento",     Icons.Material.Filled.Rule,          typeof(KONSolutions.Web.Pages.Logistica.TiposRequerimiento)),
        new("/logistica/requerimientos-stock",     "Requerimientos de Almacén",  Icons.Material.Filled.Assignment,    typeof(KONSolutions.Web.Pages.Logistica.Requerimientos)),
        new("/logistica/requerimientos-stock-detalle", "Detalle de Requerimientos", Icons.Material.Filled.ListAlt,    typeof(KONSolutions.Web.Pages.Logistica.RequerimientosStockDetalle)),
        new("/logistica/tipos-reserva-stock",      "Tipos de Reserva de Stock",  Icons.Material.Filled.Rule,          typeof(KONSolutions.Web.Pages.Logistica.TiposReservaStock)),
        new("/logistica/reservas-stock",           "Reservas de Stock",          Icons.Material.Filled.BookmarkAdded, typeof(KONSolutions.Web.Pages.Logistica.ReservasStocks)),
        new("/logistica/reservas-stock-detalle",   "Detalle de Reservas",        Icons.Material.Filled.ListAlt,       typeof(KONSolutions.Web.Pages.Logistica.ReservasStockDetalle)),
        // Alias: el árbol de menú tiene DES_FORMULARIO = "Logistica/Requerimientos.razor" y
        // "Logistica/ReservasStocks.razor", que normalizan distinto de la ruta @page.
        new("/logistica/requerimientos",           "Requerimientos de Almacén",  Icons.Material.Filled.Assignment,    typeof(KONSolutions.Web.Pages.Logistica.Requerimientos)),
        new("/logistica/reservas-stocks",          "Reservas de Stock",          Icons.Material.Filled.BookmarkAdded, typeof(KONSolutions.Web.Pages.Logistica.ReservasStocks)),

        // Movimientos de Almacén. DOS rutas para la MISMA pantalla, y no es un descuido:
        // Normalizar() es generoso con la forma —acepta "MovimientosAlmacen.razor",
        // "Logistica/MovimientosAlmacen" o la ruta ya en kebab, y hasta limpia guiones raros
        // y espacios invisibles pegados a mano en DES_FORMULARIO— pero NO con singular contra
        // plural. Con las dos da igual cómo quede escrita la opción en la base.
        //
        // El documento (MovimientoAlmacenDocumento) NO se registra ni lleva @page: es un
        // componente que esta pantalla renderiza en su segunda solapa, igual que Reserva y
        // Requerimiento con los suyos.
        new("/logistica/movimientos-almacen",      "Movimientos de Almacén",     Icons.Material.Filled.SwapHoriz,     typeof(KONSolutions.Web.Pages.Logistica.MovimientosAlmacen)),
        new("/logistica/movimientos-almacenes",    "Movimientos de Almacén",     Icons.Material.Filled.SwapHoriz,     typeof(KONSolutions.Web.Pages.Logistica.MovimientosAlmacen)),

        // Lotes de control. El detalle es una consulta de solo lectura: los productos de un
        // lote los escriben los movimientos de almacén, no esta pantalla.
        new("/logistica/tipos-lote-control",       "Tipos de Lote de Control",   Icons.Material.Filled.Rule,          typeof(KONSolutions.Web.Pages.Logistica.TiposLoteControl)),
        new("/logistica/lotes-control",            "Lotes de Control",           Icons.Material.Filled.Inventory2,    typeof(KONSolutions.Web.Pages.Logistica.LotesControlPagina)),
        new("/logistica/lotes-control-detalle",    "Detalle de Lotes de Control",Icons.Material.Filled.ListAlt,       typeof(KONSolutions.Web.Pages.Logistica.LotesControlDetalle)),

        // Consulta de existencias. Se registran las variantes de nombre porque el árbol de menú
        // no siempre respeta el singular/plural de la tabla.
        new("/logistica/stocks-almacen-ubicacion", "Existencias por Ubicación", Icons.Material.Filled.Inventory,     typeof(KONSolutions.Web.Pages.Logistica.StocksAlmacen)),
        // Mapa de trazabilidad: del requerimiento a reservas, compras y movimientos de almacén.
        new("/logistica/trazabilidad-requerimientos", "Trazabilidad de Requerimientos", Icons.Material.Filled.AccountTree, typeof(KONSolutions.Web.Pages.Logistica.TrazabilidadRequerimientos)),
        // Traslados entre ubicaciones de un mismo almacén: de un rack a otro.
        new("/logistica/transferencias-ubicaciones", "Transferencias entre Ubicaciones", Icons.Material.Filled.SwapHoriz, typeof(KONSolutions.Web.Pages.Logistica.TransferenciasUbicaciones)),
        // Series de la guía de remisión electrónica (T001, T002...): de aquí sale el número.
        new("/logistica/guias-remision-series", "Series de Guías de Remisión", Icons.Material.Filled.Pin, typeof(KONSolutions.Web.Pages.Logistica.GuiasRemisionSeries)),
        // Bandeja de consulta de las guías emitidas: estado, respuesta de SUNAT y descarga del XML y la constancia.
        new("/logistica/guias-remision", "Bandeja de Guías de Remisión", Icons.Material.Filled.LocalShipping, typeof(KONSolutions.Web.Pages.Logistica.GuiasRemision)),
        // El almacén destino recibe o rechaza las salidas por transferencia que requieren confirmación.
        new("/logistica/confirmacion-recepcion-transferencias", "Confirmación de Recepción de Transferencias", Icons.Material.Filled.MoveToInbox, typeof(KONSolutions.Web.Pages.Logistica.ConfirmacionRecepcionTransferencias)),
        new("/logistica/stock-almacen-ubicacion",  "Existencias por Ubicación", Icons.Material.Filled.Inventory,     typeof(KONSolutions.Web.Pages.Logistica.StocksAlmacen)),
        new("/logistica/stocks-almacen",           "Existencias por Ubicación", Icons.Material.Filled.Inventory,     typeof(KONSolutions.Web.Pages.Logistica.StocksAlmacen)),
        // Alias: el componente se llama LotesControlPagina —LotesControl choca con el alias
        // del modelo dentro del propio archivo— así que se registra también ese nombre.
        new("/logistica/lotes-control-pagina",     "Lotes de Control",           Icons.Material.Filled.Inventory2,    typeof(KONSolutions.Web.Pages.Logistica.LotesControlPagina)),

        // Alias: el árbol de menú escribe "Lotes" en plural —"Logistica/TiposLotesControl.razor"—
        // mientras que la tabla y el componente van en singular. Se registran las dos formas
        // de los tres módulos para que la opción abra sin depender de cómo se tipeó.
        new("/logistica/tipos-lotes-control",      "Tipos de Lote de Control",   Icons.Material.Filled.Rule,          typeof(KONSolutions.Web.Pages.Logistica.TiposLoteControl)),
        new("/logistica/lote-control",             "Lotes de Control",           Icons.Material.Filled.Inventory2,    typeof(KONSolutions.Web.Pages.Logistica.LotesControlPagina)),
        new("/logistica/lotes-controles",          "Lotes de Control",           Icons.Material.Filled.Inventory2,    typeof(KONSolutions.Web.Pages.Logistica.LotesControlPagina)),
        new("/logistica/lotes-control-detalles",   "Detalle de Lotes de Control",Icons.Material.Filled.ListAlt,       typeof(KONSolutions.Web.Pages.Logistica.LotesControlDetalle)),
        new("/logistica/lote-control-detalle",     "Detalle de Lotes de Control",Icons.Material.Filled.ListAlt,       typeof(KONSolutions.Web.Pages.Logistica.LotesControlDetalle)),
        new("/logistica/lotes-controles-detalle",  "Detalle de Lotes de Control",Icons.Material.Filled.ListAlt,       typeof(KONSolutions.Web.Pages.Logistica.LotesControlDetalle)),
        // Punto de reorden (PRODUCTOS_ALMACENES). El árbol de menú tiene DES_FORMULARIO =
        // "Logistica/ProductosAlmacen.razor" (singular), pero la tabla y los SP van en
        // plural, así que se registran las dos formas — más el nombre funcional con el que
        // la opción figura en el menú.
        new("/logistica/productos-almacen",        "Punto de Reorden",           Icons.Material.Filled.Inventory,    typeof(KONSolutions.Web.Pages.Logistica.ProductosAlmacen)),
        new("/logistica/productos-almacenes",      "Punto de Reorden",           Icons.Material.Filled.Inventory,    typeof(KONSolutions.Web.Pages.Logistica.ProductosAlmacen)),
        new("/logistica/punto-reorden",            "Punto de Reorden",           Icons.Material.Filled.Inventory,    typeof(KONSolutions.Web.Pages.Logistica.ProductosAlmacen)),

        new("/logistica/clases-movimiento",        "Clases de Movimiento",       Icons.Material.Filled.SwapHoriz,    typeof(KONSolutions.Web.Pages.Logistica.ClasesMovimiento)),
        // Alias: el sub módulo "Tipos de Movimiento de Almacén" en el árbol de opciones
        // tiene su propio DES_FORMULARIO ("Logistica/TiposMovimiento.razor") aunque la
        // pantalla es la misma (maestro-detalle) — sin este alias, ese nodo del menú
        // daría el mismo error "no corresponde a ninguna pantalla registrada".
        new("/logistica/tipos-movimiento",         "Tipos de Movimiento",        Icons.Material.Filled.SwapHoriz,    typeof(KONSolutions.Web.Pages.Logistica.ClasesMovimiento)),
        new("/produccion/tipos-linea-produccion",  "Tipos de Línea de Producción", Icons.Material.Filled.Timeline,    typeof(KONSolutions.Web.Pages.Produccion.TiposLineaProduccion)),
        new("/ingenieria/disenos-version",          "Diseños Versión",            Icons.Material.Filled.Layers,       typeof(KONSolutions.Web.Pages.Ingenieria.DisenosVersion)),
        new("/ingenieria/clases-desarrollo",        "Clases de Desarrollo",       Icons.Material.Filled.Category,     typeof(KONSolutions.Web.Pages.Ingenieria.ClasesDesarrollo)),
        new("/ingenieria/tipos-desarrollo",         "Tipos de Desarrollo",        Icons.Material.Filled.Category,     typeof(KONSolutions.Web.Pages.Ingenieria.TiposDesarrollo)),
        new("/ingenieria/tipos-merma",              "Tipos de Merma",             Icons.Material.Filled.DeleteSweep,  typeof(KONSolutions.Web.Pages.Ingenieria.TiposMerma)),
        new("/estandar/tipos-uso",                  "Tipos de Uso",               Icons.Material.Filled.Category,     typeof(KONSolutions.Web.Pages.Estandar.TiposUso)),
        // Sunat
        new("/sunat/cubso-sunat",    "Cubso Sunat",             Icons.Material.Filled.Numbers, typeof(KONSolutions.Web.Pages.Sunat.CubsoSunat)),
        new("/sunat/codigos-unspsc", "Códigos UNSPSC (CUBSO)",  Icons.Material.Filled.Numbers, typeof(KONSolutions.Web.Pages.Sunat.CubsoSunat)),
        // Estandar
        new("/estandar/productos", "Productos", Icons.Material.Filled.Inventory2, typeof(KONSolutions.Web.Pages.Estandar.Productos)),
        // RRHH
        new("/rrhh/tipos-zona", "Tipos de Zona", Icons.Material.Filled.Map, typeof(KONSolutions.Web.Pages.Rrhh.TiposZona)),

        // Proyectos
        new("/proyectos/tipos-proyecto", "Tipos de Proyecto", Icons.Material.Filled.Category, typeof(KONSolutions.Web.Pages.Proyectos.TiposProyecto)),
        new("/proyectos/proyectos",      "Proyectos",         Icons.Material.Filled.Assignment, typeof(KONSolutions.Web.Pages.Proyectos.Proyectos)),
        // Alias: por si el menú en BD quedó configurado con un DES_FORMULARIO distinto
        // ("Proyectos/GestionProyectos.razor" → "Gestión de Proyectos" en la captura de error).
        new("/proyectos/gestion-proyectos", "Proyectos", Icons.Material.Filled.Assignment, typeof(KONSolutions.Web.Pages.Proyectos.Proyectos)),
        new("/proyectos/proyectos-roles", "Roles de tarea", Icons.Material.Filled.Badge, typeof(KONSolutions.Web.Pages.Proyectos.ProyectosRoles)),
        new("/proyectos/proyectos-roles-categorias", "Categorías de roles", Icons.Material.Filled.Category, typeof(KONSolutions.Web.Pages.Proyectos.ProyectosRolesCategorias)),
        new("/proyectos/tipos-documento-compromiso", "Tipos de documento de compromiso", Icons.Material.Filled.Gavel, typeof(KONSolutions.Web.Pages.Proyectos.TiposDocumentoCompromiso)),
        new("/proyectos/plantillas-documento", "Plantillas de documento", Icons.Material.Filled.Description, typeof(KONSolutions.Web.Pages.Proyectos.PlantillasDocumento)),
        new("/proyectos/proyectos-unidades-medida", "Unidades de medida de tareas", Icons.Material.Filled.Straighten, typeof(KONSolutions.Web.Pages.Proyectos.ProyectosUnidadesMedida)),
        new("/proyectos/proyectos-probabilidad-riesgo", "Probabilidad de riesgo", Icons.Material.Filled.Percent, typeof(KONSolutions.Web.Pages.Proyectos.ProyectosProbabilidadRiesgo)),
        new("/proyectos/proyectos-impacto-riesgo", "Impacto de riesgo", Icons.Material.Filled.Warning, typeof(KONSolutions.Web.Pages.Proyectos.ProyectosImpactoRiesgo)),
        new("/proyectos/feriados", "Feriados", Icons.Material.Filled.Event, typeof(KONSolutions.Web.Pages.Proyectos.Feriados)),
        new("/proyectos/parametros-proyectos", "Parámetros de proyectos", Icons.Material.Filled.Tune, typeof(KONSolutions.Web.Pages.Proyectos.ParametrosProyectos)),
        new("/proyectos/portafolio-proyectos", "Portafolio de proyectos", Icons.Material.Filled.Dashboard, typeof(KONSolutions.Web.Pages.Proyectos.PortafolioProyectos)),
        new("/proyectos/mis-tareas-proyectos", "Mis tareas", Icons.Material.Filled.Checklist, typeof(KONSolutions.Web.Pages.Proyectos.MisTareasProyectos)),

        // Reporteador. DES_FORMULARIO = "Reportes/ReportesFuentes.razor", "Reportes/ReportesDisenos.razor" y
        // "Reportes/ReportesBotones.razor" (script 05_REPORTES_MENU_Y_BOTONES.sql).
        new("/reportes/reportes-fuentes", "Fuentes de datos",    Icons.Material.Filled.Storage,        typeof(KONSolutions.Web.Pages.Reportes.ReportesFuentes)),
        new("/reportes/reportes-disenos", "Diseños de reportes", Icons.Material.Filled.DesignServices, typeof(KONSolutions.Web.Pages.Reportes.ReportesDisenos)),
        new("/reportes/reportes-botones", "Reportes en botones", Icons.Material.Filled.SmartButton,    typeof(KONSolutions.Web.Pages.Reportes.ReportesBotones)),
    };

    /// <summary>
    /// Normaliza DES_FORMULARIO a una ruta web comparable. El valor real en BD tiene
    /// el formato "Seguridad/Usuarios.razor" (carpeta + nombre de componente, con
    /// mayúsculas y extensión) — se convierte a "/seguridad/usuarios" antes de comparar.
    /// También tolera que ya venga como ruta web (con barra inicial, sin extensión).
    /// </summary>
    public static string Normalizar(string ruta)
    {
        var r = (ruta ?? "").Trim();

        // Defensivo: valores tipeados/pegados a mano en DES_FORMULARIO a veces traen
        // espacios "raros" (NBSP, zero-width) o guiones que no son el hyphen-minus (–, —)
        // — visualmente idénticos pero que rompen una comparación exacta. Se normalizan
        // antes de comparar en vez de exigir que el dato en BD sea perfecto.
        r = r.Replace('\u2013', '-').Replace('\u2014', '-')
             .Replace('\u00A0', ' ').Replace("\u200B", "")
             .Trim();

        if (r.EndsWith(".razor", StringComparison.OrdinalIgnoreCase))
            r = r[..^".razor".Length];
        // Convierte cada segmento de PascalCase a kebab-case antes de lowercase
        // p.ej. "BotonesEstandar" → "botones-estandar"
        r = string.Join("/", r.Replace('\\', '/').Split('/').Select(PascalToKebab));
        if (!r.StartsWith("/")) r = "/" + r;
        return r.TrimEnd('/');
    }

    private static string PascalToKebab(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < s.Length; i++)
        {
            if (i > 0 && char.IsUpper(s[i]) && !char.IsUpper(s[i - 1]))
                sb.Append('-');
            sb.Append(char.ToLowerInvariant(s[i]));
        }
        return sb.ToString();
    }

    /// <summary>Identidad de la pestaña de una opción de menú: la pantalla MÁS la opción.
    /// Dos opciones que comparten pantalla (Proformas y Órdenes de Compra son las dos
    /// PedidosComerciales.razor, filtradas por clase de operación) tienen identidades
    /// distintas y por lo tanto pestaña propia; volver a la misma opción reutiliza la suya.</summary>
    public static string ClaveOpcion(string ruta, string codAplicacion, string codOpcionAplicacion)
        => $"{Normalizar(ruta)}#{codAplicacion}/{codOpcionAplicacion}";

    /// <summary>Título que debe mostrar la pestaña: el nombre de la OPCIÓN tal como está en
    /// el menú (DES_OPCION_APLICACION), no el nombre genérico de la pantalla. Es lo que hace
    /// que el mosaico "Proformas" abra una pestaña que dice "Proformas" y no
    /// "Pedidos Comerciales". Si la opción viniera sin nombre, cae al de la pantalla.</summary>
    public static string TituloOpcion(string? nombreOpcion, PantallaInfo info)
        => string.IsNullOrWhiteSpace(nombreOpcion) ? info.Titulo : nombreOpcion.Trim();

    public static PantallaInfo? PorRuta(string ruta)
        => Pantallas.FirstOrDefault(p => Normalizar(p.Ruta).Equals(Normalizar(ruta), StringComparison.OrdinalIgnoreCase));
}
