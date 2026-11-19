// ─────────────────────────────────────────────────────────────────────────────
// ARCHIVO RECONSTRUIDO. Si tienes tu ApiRoutes.cs original (Git, copia local), úsalo y
// aplica solo ApiRoutes.PARCHE.txt. Este archivo parte de la versión antigua que quedó
// en el proyecto y le suma las rutas que faltaban (bloques "Rutas restauradas"), deducidas
// de los controllers de la API y de cómo las usan las páginas.
// ─────────────────────────────────────────────────────────────────────────────
namespace KONSolutions.Web.Services;

/// <summary>
/// Rutas centralizadas de la API.
/// </summary>
public static class ApiRoutes
{
    public const string Prefix = "api/v1";

    public static class Auth
    {
        public const string Login = $"{Prefix}/auth/login";
    }

    public static class Administracion
    {
        public const string TiposCargoAdministrativo = $"{Prefix}/administracion/tipos-cargo-administrativo";
        public const string Cargos = $"{Prefix}/administracion/cargos";
        public const string Areas = $"{Prefix}/administracion/areas";
        public const string AreasLoadcombos = $"{Areas}/loadcombos";
        public const string AreasCargosEntidades = $"{Prefix}/administracion/areas-cargos-entidades";
        public const string AreasCargosEntidadesTreeview = $"{AreasCargosEntidades}/treeview";
        public const string EntidadesDisponibles = $"{Prefix}/administracion/areas-cargos-entidades/entidades-disponibles";
        public const string AsesoresPorEtiqueta = $"{AreasCargosEntidades}/asesores-por-etiqueta";

        // ── Rutas restauradas (reconstruidas desde los controllers de la API) ──
        public static string AreasCargosEntidadesPorCentroCosto(string codCentroCosto) =>
            $"{Prefix}/administracion/areas-cargos-entidades/por-centro-costo/{Uri.EscapeDataString(codCentroCosto)}";
        public const string AreasCombobox = $"{Prefix}/administracion/areas/combobox";
        public const string CargosCombobox = $"{Prefix}/administracion/cargos/combobox";
    }

    public static class Contabilidad
    {
        public const string TiposCentroCosto = $"{Prefix}/contabilidad/tipos-centro-costo";
        public const string CentrosCosto     = $"{Prefix}/contabilidad/centros-costo";

        // ── Rutas restauradas (reconstruidas desde los controllers de la API) ──
        public const string AsientosContables = $"{Prefix}/contabilidad/asientos-contables";
        public const string CuentasContables = $"{Prefix}/contabilidad/cuentas-contables";
        public const string CuentasNaturaleza = $"{Prefix}/contabilidad/cuentas-naturaleza";
        public const string CuentasNaturalezaCombobox = $"{Prefix}/contabilidad/cuentas-naturaleza/combobox";
        public const string TiposCuentaContable = $"{Prefix}/contabilidad/tipos-cuenta-contable";
        public const string TiposCuentaContableCombobox = $"{Prefix}/contabilidad/tipos-cuenta-contable/combobox";
    }

    public static class Entorno
    {
        public const string ControlCambios  = $"{Prefix}/entorno/control-cambios";
        public const string LimitesColumnas = $"{Prefix}/entorno/limites-columnas";
        public const string TiposNotificacion = $"{Prefix}/entorno/tipos-notificacion";
        public const string TiposNotificacionAcciones = $"{Prefix}/entorno/tipos-notificacion-acciones";
        public const string EjecutarSentencias = $"{Prefix}/entorno/ejecutar-sentencias";
        public const string BancoColores = $"{Prefix}/entorno/banco-colores";
        public const string DiccionarioEtiquetas = $"{Prefix}/entorno/diccionario-etiquetas";
        public const string DominiosEmpresa = $"{Prefix}/entorno/dominios-empresa";
        public const string DominiosEmpresaCombobox = $"{DominiosEmpresa}/combobox";
        public const string NotificacionesFlujoPendientes = $"{Prefix}/entorno/notificaciones-flujo/pendientes";
        public const string NotificacionesFlujoShowall    = $"{Prefix}/entorno/notificaciones-flujo/showall";
        public const string NotificacionesAprobaciones = $"{Prefix}/entorno/notificaciones-aprobaciones";
        /// <summary>Aprobar, rechazar u observar una notificación: el servidor ejecuta el comando guardado de esa acción.</summary>
        public static string NotificacionAprobacionAccion(decimal numNotificacion, string codAccion) => $"{NotificacionesAprobaciones}/{numNotificacion:0}/{codAccion.ToLowerInvariant()}";
        public const string Servidores = $"{Prefix}/entorno/servidores";
        public const string TiposLectura = $"{Prefix}/entorno/tipos-lectura";
        public const string TiposLecturaCombobox = $"{TiposLectura}/combobox";
        public const string PathFiles  = $"{Prefix}/entorno/pathfiles";
        public const string PathFilesCombobox = $"{PathFiles}/combobox";
        // Documentos por carpeta — genérico y reutilizable desde cualquier diálogo (Solicitudes
        // Comerciales, Entidades, etc.). codKeyPathfile = clave de ENTORNO.PATHFILES; identificador
        // = subcarpeta (N° de Solicitud, código de Entidad, etc.).
        public static string DocumentosCarpeta(string codKeyPathfile, string identificador) =>
            $"{Prefix}/entorno/documentos-carpeta/{Uri.EscapeDataString(codKeyPathfile)}/{Uri.EscapeDataString(identificador)}";
        public const string PostIts    = $"{Prefix}/entorno/postits";
        public const string DashboardBd = $"{Prefix}/entorno/dashboard-bd";

        // ── Rutas restauradas (reconstruidas desde los controllers de la API) ──
        public const string AplicacionesOpcionesAprobaciones = $"{Prefix}/entorno/aplicaciones-opciones-aprobaciones";
        public const string AplicacionesOpcionesAprobacionesResumen = $"{Prefix}/entorno/aplicaciones-opciones-aprobaciones/resumen";
        public const string EntidadesConfigurar = $"{Prefix}/entorno/entidades-configurar";
        public static string EntidadesConfigurarDelete(string desTable, string desColumn) =>
            $"{Prefix}/entorno/entidades-configurar?desTable={Uri.EscapeDataString(desTable)}&desColumn={Uri.EscapeDataString(desColumn)}";
        public const string EntidadesProcedures = $"{Prefix}/entorno/entidades-procedures";
        public static string EntidadesProceduresParameters(string name) =>
            $"{Prefix}/entorno/entidades-procedures/parameters?name={Uri.EscapeDataString(name)}";
    }

    public static class Sistema
    {
        public const string GenerarSpsText = $"{Prefix}/sistema/herramientas/generar-sps-text";
        public const string Tablas         = $"{Prefix}/sistema/herramientas/tablas";
    }

    public static class Seguridad
    {
        public const string Usuarios              = $"{Prefix}/seguridad/usuarios";
        public const string UsuariosCombobox      = $"{Prefix}/seguridad/usuarios/combobox";
        public const string BotonesEstandar       = $"{Prefix}/seguridad/botones-estandar";
        public const string RolesUsuarioUsuarios  = $"{Prefix}/seguridad/roles-usuario-usuarios";
        public const string MisRoles              = $"{Prefix}/seguridad/roles-usuario-usuarios/mis-roles";
        public const string RolesUsuariosAplicacionesOpcionesBotones = $"{Prefix}/seguridad/roles-usuarios-aplicaciones-opciones-botones";
        public const string BotonesTreeview       = $"{Prefix}/seguridad/roles-usuarios-aplicaciones-opciones-botones/treeview";
        public const string BotonesTreeviewLogos  = $"{BotonesTreeview}/logos";
        public const string UsuariosFavoritos     = $"{Prefix}/seguridad/usuarios-favoritos";
        public const string Favoritos             = $"{Prefix}/seguridad/favoritos";
        public const string FavoritosWorkspace    = $"{Prefix}/seguridad/favoritos/workspace";
        public const string FavoritosWorkspaceLogos = $"{Prefix}/seguridad/favoritos/workspace/logos";
        public const string EstadosPorTabla       = $"{Prefix}/seguridad/estados-por-tabla";
        public const string UsuariosSearch        = $"{Usuarios}/search";
        public const string UsuariosPassword      = $"{Usuarios}/password";
        public const string UsuariosPasswordReset = $"{Usuarios}/password-reset";
        public const string UsuariosEstados       = $"{Usuarios}/estados";
        public const string UsuariosResetIntentos = $"{Usuarios}/resetintentos";
        public const string PerfilesCombobox      = $"{Prefix}/seguridad/perfiles-usuario/combobox";
        public const string Perfiles              = $"{Prefix}/seguridad/perfiles-usuario";
        public const string Roles                 = $"{Prefix}/seguridad/roles-usuario";
        // ── Hijos del rol (asignación pura: dos grillas, sin diálogo de alta) ──
        // Cada uno expone además /disponibles/{...}: lo que le falta al rol lo resuelve un SP,
        // no una resta en el cliente.
        public const string RolesAlmacenes            = $"{Prefix}/seguridad/roles-usuario-almacenes";
        public const string RolesAlmacenesDisponibles = $"{RolesAlmacenes}/disponibles";
        public const string RolesAlmacenesAsignar     = $"{RolesAlmacenes}/asignar";
        public const string RolesAlmacenesDesasignar  = $"{RolesAlmacenes}/desasignar";

        public const string RolesAlmacenesTiposMovimiento            = $"{Prefix}/seguridad/roles-usuario-almacenes-tipos-movimiento";
        public const string RolesAlmacenesTiposMovimientoDisponibles = $"{RolesAlmacenesTiposMovimiento}/disponibles";
        public const string RolesAlmacenesTiposMovimientoAsignar     = $"{RolesAlmacenesTiposMovimiento}/asignar";
        public const string RolesAlmacenesTiposMovimientoDesasignar  = $"{RolesAlmacenesTiposMovimiento}/desasignar";

        public const string RolesTiposOperacion            = $"{Prefix}/seguridad/roles-usuario-tipos-operacion";
        public const string RolesTiposOperacionDisponibles = $"{RolesTiposOperacion}/disponibles";
        public const string RolesTiposOperacionAsignar     = $"{RolesTiposOperacion}/asignar";
        public const string RolesTiposOperacionDesasignar  = $"{RolesTiposOperacion}/desasignar";

        public const string RolesAsientosContables            = $"{Prefix}/seguridad/roles-usuario-asientos-contables";
        public const string RolesAsientosContablesDisponibles = $"{RolesAsientosContables}/disponibles";
        public const string RolesAsientosContablesAsignar     = $"{RolesAsientosContables}/asignar";
        public const string RolesAsientosContablesDesasignar  = $"{RolesAsientosContables}/desasignar";

        public const string ComplejidadPassword   = $"{Prefix}/seguridad/complejidad-password";
        public const string Aplicaciones          = $"{Prefix}/seguridad/aplicaciones";
        public const string RolesCombobox               = $"{Roles}/combobox";
        public const string ComplejidadPasswordCombobox = $"{ComplejidadPassword}/combobox";
        public const string AplicacionesCombobox        = $"{Aplicaciones}/combobox";
        public const string AplicacionesOpcionesTreeview = $"{Prefix}/seguridad/aplicaciones-opciones-treeview";
        public const string AplicacionesOpcionesTreeviewLogos = $"{AplicacionesOpcionesTreeview}/logos";
        public const string Opciones       = $"{Prefix}/seguridad/roles-usuarios-aplicaciones-opciones";
        public const string OpcionesPorRol = $"{Prefix}/seguridad/roles-usuarios-aplicaciones-opciones/por-rol";
        public const string Botones        = $"{Prefix}/seguridad/roles-usuarios-aplicaciones-opciones-botones";
        public const string AplicacionesOpcionesBotones = $"{Prefix}/seguridad/aplicaciones-opciones-botones";
        public const string AsignacionRoles    = $"{Prefix}/seguridad/usuario-asignaciones/roles-de-usuario";
        public const string AsignacionOpciones = $"{Prefix}/seguridad/usuario-asignaciones/opciones";
        public const string AsignacionBotones  = $"{Prefix}/seguridad/usuario-asignaciones/botones";
        public const string TiposOpciones        = $"{Prefix}/seguridad/tipos-opciones";
        public const string TiposOpcionesCombobox = $"{Prefix}/seguridad/tipos-opciones/combobox";
        public const string AplicacionesOpciones  = $"{Prefix}/seguridad/aplicaciones-opciones";
        public const string AplicacionesOpcionesMove = $"{AplicacionesOpciones}/move";
        public const string OpcionesTreeviewAsignadas   = $"{Opciones}/treeview-asignadas";
        public const string OpcionesTreeviewDisponibles = $"{Opciones}/treeview-disponibles";

        // ── Rutas restauradas (reconstruidas desde los controllers de la API) ──
        public const string AplicacionesOpcionesBotonesLoadcombos = $"{Prefix}/seguridad/aplicaciones-opciones-botones/loadcombos";
        public const string AplicacionesOpcionesLoadcombos = $"{Prefix}/seguridad/aplicaciones-opciones/loadcombos";
        public const string BotonesPorOpcion = $"{Prefix}/seguridad/roles-usuarios-aplicaciones-opciones-botones/por-opcion";
        public const string PerfilesLoadcombos = $"{Prefix}/seguridad/perfiles-usuario/loadcombos";
        public const string UsuariosLoadcombos = $"{Prefix}/seguridad/usuarios/loadcombos";
    }

    public static class Estandar
    {
        public const string TiposUso              = $"{Prefix}/estandar/tipos-uso";
        public const string TiposUsoCombobox      = $"{TiposUso}/combobox";
        public const string EntidadesUsuariosComerciales  = $"{Prefix}/estandar/entidades-usuarios-sugeridos/comerciales";
        public const string EntidadesUsuariosDependientes = $"{Prefix}/estandar/entidades-usuarios-sugeridos/dependientes";
        public const string TiposSucursales         = $"{Prefix}/estandar/tipos-sucursales";
        public const string TiposSucursalesCombobox = $"{TiposSucursales}/combobox";
        public const string Sucursales              = $"{Prefix}/estandar/sucursales";
        public const string SucursalesCombobox      = $"{Sucursales}/combobox";
        public const string SucursalesLoadcombos    = $"{Sucursales}/loadcombos";
        public const string EstadosCombobox       = $"{Prefix}/estandar/estados/combobox";
        public const string TiposEstados          = $"{Prefix}/estandar/tipos-estados";
        public const string TiposEstadosCombobox  = $"{TiposEstados}/combobox";
        public const string TablasTiposEstado           = $"{Prefix}/estandar/tablas-tipos-estado";
        public const string TablasTiposEstadoDisponibles = $"{TablasTiposEstado}/tablas-disponibles";
        public const string Estados               = $"{Prefix}/estandar/estados";
        public const string EstadosPorTipo        = $"{Estados}/por-tipo";
        public const string EstadosImportEstandar = $"{Estados}/import-estandar";
        public const string EstadosEstandar       = $"{Prefix}/estandar/estados-estandar";
        public const string TiposMotivosCombobox  = $"{Prefix}/estandar/tipos-motivos/combobox";
        public const string TiposMotivos          = $"{Prefix}/estandar/tipos-motivos";
        public const string Motivos               = $"{Prefix}/estandar/motivos";
        public const string MotivosPorTipo        = $"{Motivos}/por-tipo";
        public const string TiposEntidades        = $"{Prefix}/estandar/tipos-entidades";
        public const string TiposEntidadesCombobox = $"{TiposEntidades}/combobox";
        public const string EntidadesTiposEntidades = $"{Prefix}/estandar/entidades-tipos-entidades";
        public const string EntidadesTiposEntidadesShowselect = $"{EntidadesTiposEntidades}/showselect";
        public const string EntidadesTiposEntidadesShowcomboxid = $"{EntidadesTiposEntidades}/showcomboxid";
        public const string TiposVia              = $"{Prefix}/estandar/tipos-via";
        public const string TiposViaCombobox      = $"{TiposVia}/combobox";
        public const string EtiquetasBusquedaAreas = $"{Prefix}/estandar/etiquetas-busqueda-areas";
        public const string Sexos                 = $"{Prefix}/estandar/sexos";
        public const string TiposPersonas         = $"{Prefix}/estandar/tipos-personas";
        public const string Anos                  = $"{Prefix}/estandar/anos";
        public const string Meses                 = $"{Prefix}/estandar/meses";
        public const string TiposUnidadMedida               = $"{Prefix}/estandar/tipos-unidad-medida";
        public const string TiposUnidadMedidaCombobox       = $"{TiposUnidadMedida}/combobox";
        public const string UnidadesMedida                  = $"{Prefix}/estandar/unidades-medida";
        public const string UnidadesMedidaCombobox          = $"{UnidadesMedida}/combobox";
        public const string UnidadesMedidaDownloadSnippet       = $"{UnidadesMedida}/download-from-snippet";
        public const string UnidadesMedidaEquivalencias              = $"{Prefix}/estandar/unidades-medida-equivalencias";
        public const string UnidadesMedidaEquivalenciasPorTipo       = $"{UnidadesMedidaEquivalencias}/por-tipo";
        public const string UnidadesMedidaEquivalenciasUpsert        = $"{UnidadesMedidaEquivalencias}/upsert";
        public const string UnidadesMedidaEquivalenciasCargarEstandar= $"{UnidadesMedidaEquivalencias}/cargar-estandar";
        public const string TiposDocumentoIdentidad         = $"{Prefix}/estandar/tipos-documento-identidad";
        public const string TiposDocumentoIdentidadCombobox = $"{TiposDocumentoIdentidad}/combobox";
        public const string EntidadesDocumentosIdentidad    = $"{Prefix}/estandar/entidades-documentos-identidad";
        public const string PaisesCombobox                  = $"{Paises}/combobox";
        public const string Ubigeos                    = $"{Prefix}/estandar/ubigeos";
        public const string UbigeosCombobox            = $"{Ubigeos}/combobox";
        public const string UbigeosDownloadFromSnippet = $"{Ubigeos}/download-from-snippet";
        public const string TiposEmpresa               = $"{Prefix}/estandar/tipos-empresa";
        public const string TiposEmpresaCombobox       = $"{TiposEmpresa}/combobox";
        public const string Paises                     = $"{Prefix}/estandar/paises";
        public const string PaisesDownloadFromSnippet  = $"{Paises}/download-from-snippet";
        public const string PaisesDownloadStatus       = $"{Paises}/flag-download-status";
        public const string TiposProducto              = $"{Prefix}/estandar/tipos-producto";
        public const string TiposProductoCombobox      = $"{TiposProducto}/combobox";
        public const string GruposProducto               = $"{Prefix}/estandar/grupos-producto";
        public const string GruposProductoCombobox       = $"{GruposProducto}/combobox";
        public const string GruposProductoLoadcombos     = $"{GruposProducto}/loadcombos";
        public const string FamiliasProducto             = $"{Prefix}/estandar/familias-producto";
        public const string FamiliasProductoCombobox     = $"{FamiliasProducto}/combobox";
        // Encadenados: cada uno pide el código de su padre.
        public static string FamiliasProductoPorGrupo(string grupo) => $"{FamiliasProducto}/combobox/{Uri.EscapeDataString(grupo)}";
        public const string FamiliasProductoLoadcombos   = $"{FamiliasProducto}/loadcombos";
        public const string SubFamiliasProducto          = $"{Prefix}/estandar/sub-familias-producto";
        public const string SubFamiliasProductoLoadcombos = $"{SubFamiliasProducto}/loadcombos";
        public static string SubFamiliasProductoPorFamilia(string grupo, string familia)
            => $"{SubFamiliasProducto}/combobox/{Uri.EscapeDataString(grupo)}/{Uri.EscapeDataString(familia)}";
        // La lista con filtros (GetAll del controlador): la ruta que de verdad existe en la API para pedir las sub familias de una familia.
        public static string SubFamiliasProductoLista(string grupo, string familia)
            => $"{SubFamiliasProducto}?codGrupoProducto={Uri.EscapeDataString(grupo)}&codFamiliaProducto={Uri.EscapeDataString(familia)}";
        // Pestaña RFID de Configuración general: los productos que pueden ser la estación (por grupo, familia y sub familia) y los archivos de la carpeta de una ruta de Rutas de archivos.
        public static string ConfiguracionGeneralRfidEstaciones(string grupo, string? familia, string? subFamilia)
            => $"{Prefix}/estandar/configuracion-general/rfid/estaciones?codGrupoProducto={Uri.EscapeDataString(grupo)}"
             + (string.IsNullOrWhiteSpace(familia) ? "" : $"&codFamiliaProducto={Uri.EscapeDataString(familia)}")
             + (string.IsNullOrWhiteSpace(subFamilia) ? "" : $"&codSubFamiliaProducto={Uri.EscapeDataString(subFamilia)}");
        public static string ConfiguracionGeneralRfidPlantillas(string codKeyPathfile)
            => $"{Prefix}/estandar/configuracion-general/rfid/plantillas/{Uri.EscapeDataString(codKeyPathfile)}";
        public const string Entidades                  = $"{Prefix}/estandar/entidades";
        public const string EntidadesCombobox          = $"{Entidades}/combobox";
        public const string EntidadesParaDepender      = $"{Entidades}/para-depender";
        public const string EntidadesImages            = $"{Entidades}/images";
        public const string Monedas                    = $"{Prefix}/estandar/monedas";
        public const string MonedasCombobox            = $"{Prefix}/estandar/monedas/combobox";
        public const string MonedasTipoCambio          = $"{Prefix}/estandar/monedas-tipo-cambio";
        public const string MonedasTipoCambioDownloadSnippet = $"{MonedasTipoCambio}/download-from-snippet";
        public const string ConfiguracionUsuario           = $"{Prefix}/estandar/configuracion-usuario";
        public const string ConfiguracionUsuarioMia        = $"{ConfiguracionUsuario}/mi-configuracion";
        public const string TiposMenuOpciones              = $"{Prefix}/estandar/tipos-menu-opciones";
        public const string TiposMenuOpcionesCombobox      = $"{TiposMenuOpciones}/combobox";
        public const string ClasesOperacion              = $"{Prefix}/estandar/clases-operacion";
        public const string ClasesOperacionCombobox      = $"{ClasesOperacion}/combobox";
        public const string TiposOperacion               = $"{Prefix}/estandar/tipos-operacion";
        public const string TiposOperacionCombobox       = $"{TiposOperacion}/combobox";
        public const string TiposOperacionSearch         = $"{TiposOperacion}/search";
        public const string TiposComprobantesPago         = $"{Prefix}/estandar/tipos-comprobantes-pago";
        public const string TiposComprobantesPagoCombobox = $"{TiposComprobantesPago}/combobox";

        // Tabla hija de Sucursales: el listado cuelga del COD_SUCURSAL
        // (…/sucursales-tipos-comprobante-pago/{codSucursal}).
        public const string SucursalesTiposComprobantePago       = $"{Prefix}/estandar/sucursales-tipos-comprobante-pago";
        public const string SucursalesTiposComprobantePagoSearch = $"{SucursalesTiposComprobantePago}/search";
        public const string Productos           = $"{Prefix}/estandar/productos";
        public const string ProductosSearch     = $"{Productos}/search";
        public const string ProductosLoadcombos = $"{Productos}/loadcombos";
        public const string ProductosEspecificacionesTecnicas = $"{Prefix}/estandar/productos-especificaciones-tecnicas";
        public static string ProductosActualizarDescripcionDetalle(string codProducto) => $"{Productos}/{Uri.EscapeDataString(codProducto)}/actualizar-descripcion-detalle";

        // ── Rutas restauradas (reconstruidas desde los controllers de la API) ──
        public const string ClasesOperacionLoadcombos = $"{Prefix}/estandar/clases-operacion/loadcombos";
        public const string ConfiguracionesProductos = $"{Prefix}/estandar/configuraciones-productos";
        public static string ConfiguracionesProductosPorId(string codTipoConfiguracion, int numSecuencia) =>
            $"{Prefix}/estandar/configuraciones-productos/{Uri.EscapeDataString(codTipoConfiguracion)}/{numSecuencia}";
        public static string ConfiguracionesProductosPorTipo(string codTipoConfiguracion) =>
            $"{Prefix}/estandar/configuraciones-productos?codTipoConfiguracion={Uri.EscapeDataString(codTipoConfiguracion)}";
        public static string EntidadDocumentoIdentidad(decimal codEntidad, int codTipoDocumentoIdentidad) =>
            $"{Prefix}/estandar/entidades-documentos-identidad/{codEntidad}/{codTipoDocumentoIdentidad}";
        public static string EntidadesDependientes(string codTipoEntidad, string codEntidadDependencia) =>
            $"{Prefix}/estandar/entidades/dependientes?codTipoEntidad={Uri.EscapeDataString(codTipoEntidad)}&codEntidadDependencia={Uri.EscapeDataString(codEntidadDependencia)}";
        public static string EntidadesDependientesDe(decimal codEntidad) =>
            $"{Prefix}/estandar/entidades-usuarios-sugeridos/dependientes/{codEntidad}";
        public const string EntidadesLoadcombos = $"{Prefix}/estandar/entidades/loadcombos";
        public static string EntidadesPorTipo(string codTipoEntidad) =>
            $"{Prefix}/estandar/entidades?codTipoEntidad={Uri.EscapeDataString(codTipoEntidad)}";
        public const string EstructuraProducto = $"{Prefix}/estandar/estructura-producto";
        public static string EstructuraProductoPorId(string codGrupoProducto) =>
            $"{Prefix}/estandar/estructura-producto/{Uri.EscapeDataString(codGrupoProducto)}";
        public const string EstructuraRfid = $"{Prefix}/estandar/estructura-rfid";
        public const string EstructuraCodebar = $"{Prefix}/estandar/estructura-codebar";
        public const string MonedasTipoCambioObtener = $"{Prefix}/estandar/monedas-tipo-cambio/obtener";
        public const string OpcionesProcesosEstados = $"{Prefix}/estandar/opciones-procesos-estados";
        public const string CopiaEstadosConfiguracion = $"{Prefix}/estandar/copia-estados/configuracion";
        public const string CopiaEstadosTipo = $"{Prefix}/estandar/copia-estados/tipo";
        public const string MatrizTiposOperacionCalculo = $"{Prefix}/estandar/matriz-tipos-operacion/calculo";
        public const string MatrizTiposOperacionComprobantes = $"{Prefix}/estandar/matriz-tipos-operacion/comprobantes";
        public const string MatrizTiposOperacionFormulas = $"{Prefix}/estandar/matriz-tipos-operacion/formulas";
        public const string OpcionesProcesosEstadosFlujos = $"{Prefix}/estandar/opciones-procesos-estados-flujos";
        public const string OpcionesProcesosEstadosFlujosDependencia = $"{Prefix}/estandar/opciones-procesos-estados-flujos-dependencia";
        public const string OpcionesProcesosEstadosFlujosDependenciaValidar = $"{Prefix}/estandar/opciones-procesos-estados-flujos-dependencia/validar";
        public const string OpcionesProcesosEstadosFlujosTreelist = $"{Prefix}/estandar/opciones-procesos-estados-flujos/treelist";
        public const string Periodos = $"{Prefix}/estandar/periodos";
        public const string PeriodosSearch = $"{Prefix}/estandar/periodos/search";
        public const string ProductosCombobox = $"{Prefix}/estandar/productos/combobox";
        public const string ProductosEspecificacionesArbol = $"{Prefix}/estandar/productos/especificaciones-arbol";
        public const string ProductosFiltrarEspecificaciones = $"{Prefix}/estandar/productos/filtrar-especificaciones";
        public const string SucursalesSerieComprobante = $"{Prefix}/estandar/sucursales-serie-comprobante";
        public static string SucursalesSerieComprobanteDe(string codSucursal, string codTipoComprobantePago) =>
            $"{Prefix}/estandar/sucursales-serie-comprobante/{Uri.EscapeDataString(codSucursal)}/{Uri.EscapeDataString(codTipoComprobantePago)}";
        public const string TiposConfiguracion = $"{Prefix}/estandar/tipos-configuracion";
        public const string TiposOperacionLoadcombos = $"{Prefix}/estandar/tipos-operacion/loadcombos";
        public const string TiposPeriodo = $"{Prefix}/estandar/tipos-periodo";
        // Credenciales, certificado y ambiente de la emisión electrónica (SUNAT). Una sola fila: GET y PUT.
        public const string ConfiguracionEmisionElectronica = $"{Prefix}/estandar/configuracion-emision-electronica";
    }

    public static class Comercial
    {
        public const string EntidadesAsesoresComerciales    = $"{Prefix}/comercial/entidades-asesores-comerciales";
        public const string EntidadesTemporadas             = $"{Prefix}/comercial/entidades-temporadas";
        public const string EntidadesTemporadasColecciones  = $"{Prefix}/comercial/entidades-temporadas-colecciones";
        public const string TiposPrecio                  = $"{Prefix}/comercial/tipos-precio";
        public const string TiposPrecioCombobox          = $"{TiposPrecio}/combobox";
        public const string TiposPedidoComercial         = $"{Prefix}/comercial/tipos-pedido-comercial";
        public const string TiposPedidoComercialCombobox = $"{TiposPedidoComercial}/combobox";
        public const string CondicionesPago              = $"{Prefix}/comercial/condiciones-pago";
        public const string CondicionesPagoCombobox      = $"{CondicionesPago}/combobox";
        public const string FormasPago                   = $"{Prefix}/comercial/formas-pago";
        public const string FormasPagoCombobox           = $"{FormasPago}/combobox";
        public const string TiposTransporte              = $"{Prefix}/comercial/tipos-transporte";
        public const string TiposTransporteCombobox      = $"{TiposTransporte}/combobox";
        public const string FasesIncoterm                = $"{Prefix}/comercial/fases-incoterm";
        public const string FasesIncotermCombobox        = $"{FasesIncoterm}/combobox";
        public const string TiposDescuento               = $"{Prefix}/comercial/tipos-descuento";
        public const string TiposDescuentoCombobox       = $"{TiposDescuento}/combobox";
        public const string Incoterms                    = $"{Prefix}/comercial/incoterms";
        public const string IncotermsCombobox            = $"{Incoterms}/combobox";
        public const string IncotermsFases                    = $"{Prefix}/comercial/incoterms-fases";
        public const string IncotermsTiposTransporte          = $"{Prefix}/comercial/incoterms-tipos-transporte";
        public const string TiposPos                  = $"{Prefix}/comercial/tipos-pos";
        public const string TiposPosCombobox          = $"{TiposPos}/combobox";
        public const string TiposImpuesto             = $"{Prefix}/comercial/tipos-impuesto";
        public const string TiposImpuestoCombobox     = $"{TiposImpuesto}/combobox";
        public const string TiposImpuestoValores      = $"{Prefix}/comercial/tipos-impuesto-valores";
        public const string TiposValorCalculo         = $"{Prefix}/comercial/tipos-valor-calculo";
        public const string TiposValorCalculoCombobox = $"{TiposValorCalculo}/combobox";
        public const string TiposOperacionCalculo         = $"{Prefix}/comercial/tipos-operacion-calculo";
        public const string TiposOperacionCalculoCombobox = $"{TiposOperacionCalculo}/combobox";
        public const string TiposOperacionCalculoSearch   = $"{TiposOperacionCalculo}/search";
        public const string TiposOperacionCalculoCopy     = $"{TiposOperacionCalculo}/copy";
        public const string TiposOperacionCalculoAfectados       = $"{Prefix}/comercial/tipos-operacion-calculo-afectados";
        public const string TiposOperacionCalculoAfectadosSearch = $"{TiposOperacionCalculoAfectados}/search";
        public const string TiposOperacionTiposComprobantePago         = $"{Prefix}/comercial/tipos-operacion-tipos-comprobante-pago";
        public const string TiposOperacionTiposComprobantePagoCombobox = $"{TiposOperacionTiposComprobantePago}/combobox";
        public const string TiposMedioContacto        = $"{Prefix}/comercial/tipos-medio-contacto";
        public const string TiposMedioContactoCombobox = $"{TiposMedioContacto}/combobox";
        public const string EntidadesTiposMedioContacto = $"{Prefix}/comercial/entidades-tipos-medio-contacto";
        public const string TiposDireccion            = $"{Prefix}/comercial/tipos-direccion";
        public const string TiposDireccionCombobox    = $"{TiposDireccion}/combobox";
        public const string EntidadesDirecciones      = $"{Prefix}/comercial/entidades-direcciones";
        public const string SolicitudesComerciales         = $"{Prefix}/comercial/solicitudes-comerciales";
        public const string SolicitudesComercialesCombobox = $"{SolicitudesComerciales}/combobox";
        public const string SolicitudesComercialesAsignarResponsable = $"{SolicitudesComerciales}/asignar-responsable";
        public const string SolicitudesComercialesReasignaAsesor = $"{SolicitudesComerciales}/reasigna-asesor";
        public const string SolicitudesComercialesLoadcombos = $"{SolicitudesComerciales}/loadcombos";
        public const string SolicitudesComercialesMemorias = $"{Prefix}/comercial/solicitudes-comerciales-memorias";
        public const string SolicitudesComercialesNotificaciones = $"{Prefix}/comercial/solicitudes-comerciales-notificaciones";
        public const string SolicitudesComercialesCliente       = $"{Prefix}/comercial/solicitudes-comerciales-cliente";
        public const string SolicitudesComercialesClienteSearch = $"{SolicitudesComercialesCliente}/search";
        public const string TiposSolicitudComercial         = $"{Prefix}/comercial/tipos-solicitud-comercial";
        public const string TiposSolicitudComercialCombobox = $"{TiposSolicitudComercial}/combobox";
        public const string TiposSolicitudComercialAreas    = $"{Prefix}/comercial/tipos-solicitud-comercial-areas";
        public const string TiposSolicitudComercialAreasCargos = $"{Prefix}/comercial/tipos-solicitud-comercial-areas-cargos";

        // ── Rutas restauradas (reconstruidas desde los controllers de la API) ──
        public const string EntidadesDireccionesLoadcombos = $"{Prefix}/comercial/entidades-direcciones/loadcombos";
        public const string EntidadesInformacionComercial = $"{Prefix}/comercial/entidades-informacion-comercial";
        public const string EntidadesProductos = $"{Prefix}/comercial/entidades-productos";
        public const string EntidadesProductosTiposPrecios = $"{Prefix}/comercial/entidades-productos-tipos-precios";
        public const string EntidadesPurchaseOrders = $"{Prefix}/comercial/entidades-purchase-orders";
        public const string EntidadesPurchaseOrdersById = $"{Prefix}/comercial/entidades-purchase-orders/byid";
        public const string EntidadesPurchaseOrdersDespacho = $"{Prefix}/comercial/entidades-purchase-orders-despacho";
        public const string EntidadesPurchaseOrdersEspecificacionesTecnicas = $"{Prefix}/comercial/entidades-purchase-orders-especificaciones-tecnicas";
        public const string EntidadesPurchaseOrdersSearch = $"{Prefix}/comercial/entidades-purchase-orders/search";
        public const string EntidadesTemporadasLoadcombos = $"{Prefix}/comercial/entidades-temporadas/loadcombos";
        public const string EntidadesUnidadesComerciales = $"{Prefix}/comercial/entidades-unidades-comerciales";
        // Registro de Compras (comprobantes de pago de proveedores, validados contra sus órdenes)
        public const string RegistroCompras = $"{Prefix}/comercial/registro-compras";
        public const string RegistroComprasLoadcombos = $"{RegistroCompras}/loadcombos";
        public const string RegistroComprasImportarXml = $"{RegistroCompras}/importar-xml";
        public const string RegistroComprasImportarCarpeta = $"{RegistroCompras}/importar-carpeta";
        public const string PedidosComerciales = $"{Prefix}/comercial/pedidos-comerciales";
        public const string PedidosComercialesAtencion = $"{PedidosComerciales}/atencion";
        public const string PedidosComercialesCalculoCalcular = $"{Prefix}/comercial/pedidos-comerciales-calculo/calcular";
        public const string PedidosComercialesCalculoPorCabecera = $"{Prefix}/comercial/pedidos-comerciales-calculo/por-cabecera";
        public const string PedidosComercialesCalculoSincronizar = $"{Prefix}/comercial/pedidos-comerciales-calculo/sincronizar";
        public const string PedidosComercialesCalculoRevertir = $"{Prefix}/comercial/pedidos-comerciales-calculo/revertir";
        public const string PedidosComercialesCalculoRevertirProforma = $"{Prefix}/comercial/pedidos-comerciales-calculo/revertir-proforma";
        public const string PedidosComercialesComparativo = $"{Prefix}/comercial/pedidos-comerciales-comparativo";
        public const string PedidosComercialesDetalle = $"{Prefix}/comercial/pedidos-comerciales-detalle";
        public const string PedidosComercialesEstados = $"{Prefix}/comercial/pedidos-comerciales-estados";
        public static string PedidosComercialesEstadosDe(string codClaseOperacion, string codTipoOperacion, decimal numPedidoComercial)
            => $"{PedidosComercialesEstados}?codClaseOperacion={Uri.EscapeDataString(codClaseOperacion)}&codTipoOperacion={Uri.EscapeDataString(codTipoOperacion)}&numPedidoComercial={numPedidoComercial}";
        public const string PedidosComercialesDetallePorCabecera = $"{Prefix}/comercial/pedidos-comerciales-detalle/por-cabecera";
        public const string PedidosComercialesLoadcombos = $"{Prefix}/comercial/pedidos-comerciales/loadcombos";
        public const string PedidosComercialesRequerimientos = $"{Prefix}/comercial/pedidos-comerciales-requerimientos";
        public const string PedidosComercialesRequerimientosStockLinkDetalle = $"{Prefix}/comercial/pedidos-comerciales-requerimientos-stock-link/detalle";
        public const string TiposPedidoComercialLoadcombos = $"{Prefix}/comercial/tipos-pedido-comercial/loadcombos";
        public const string TiposSolicitudComercialAreasCargosLoadcombos = $"{Prefix}/comercial/tipos-solicitud-comercial-areas-cargos/loadcombos";
    }

    public static class Ingenieria
    {
        public const string TiposEspecificacionTecnica         = $"{Prefix}/ingenieria/tipos-especificacion-tecnica";
        public const string TiposEspecificacionTecnicaCombobox = $"{TiposEspecificacionTecnica}/combobox";
        public const string EspecificacionesTecnicas            = $"{Prefix}/ingenieria/especificaciones-tecnicas";
        public const string EspecificacionesTecnicasCombobox    = $"{EspecificacionesTecnicas}/combobox";
        public const string EspecificacionesTecnicasLoadcombos  = $"{EspecificacionesTecnicas}/loadcombos";
        public const string EspecificacionesItemsTecnicosTreelist = $"{Prefix}/ingenieria/especificaciones-items-tecnicos/treelist";
        public const string EspecificacionesItemsTecnicosUpdate   = $"{Prefix}/ingenieria/especificaciones-items-tecnicos";
        public const string EspecificacionesItemsTecnicosDelete   = $"{Prefix}/ingenieria/especificaciones-items-tecnicos";
        public const string EspecificacionesItemsTecnicosTreelistPorEspecificacion = $"{Prefix}/ingenieria/especificaciones-items-tecnicos/treelist-por-especificacion";
        public const string EspecificacionesItemsTecnicosComboDependientes = $"{Prefix}/ingenieria/especificaciones-items-tecnicos/combo-dependientes";
        // Mismo SP (SP.SCHEMAIngenieria.EspecificacionesTecnicasItems.*) que el árbol de arriba,
        // pero este SHOWBYID sí trae los 6 campos de auditoría — se usa solo para el botón de
        // auditoría del diálogo de edición (el árbol nunca los selecciona).
        public const string EspecificacionesTecnicasItems = $"{Prefix}/ingenieria/especificaciones-tecnicas-items";
        public const string TiposMerma         = $"{Prefix}/ingenieria/tipos-merma";
        public const string TiposMermaCombobox = $"{TiposMerma}/combobox";
        public const string TiposRegistroDatos         = $"{Prefix}/ingenieria/tipos-registro-datos";
        public const string TiposRegistroDatosCombobox = $"{TiposRegistroDatos}/combobox";
        public const string TiposDiseno         = $"{Prefix}/ingenieria/tipos-diseno";
        public const string TiposDisenoCombobox = $"{TiposDiseno}/combobox";
        public const string Disenos         = $"{Prefix}/ingenieria/disenos";
        public const string DisenosCombobox = $"{Disenos}/combobox";
        public const string DisenosLoadcombos = $"{Disenos}/loadcombos";
        public const string DisenosMemorias = $"{Prefix}/ingenieria/disenos-memorias";
        public const string DisenosVersionesMemorias = $"{Prefix}/ingenieria/disenos-versiones-memorias";
        public const string DisenosProducto         = $"{Prefix}/ingenieria/disenos-producto";
        public const string DisenosProductoCombobox = $"{DisenosProducto}/combobox";
        public const string GruposEspecificacionTecnica         = $"{Prefix}/ingenieria/grupos-familias-sub-familias-especificacion-tecnica";
        public const string GruposEspecificacionTecnicaTreelist = $"{GruposEspecificacionTecnica}/treelist";
        public const string GruposEspecificacionTecnicaCopiar   = $"{GruposEspecificacionTecnica}/copiar";
        public const string DisenosVersion          = $"{Prefix}/ingenieria/disenos-version";
        public const string DisenosVersionCombobox  = $"{DisenosVersion}/combobox";
        public const string DisenosVersionSearch    = $"{DisenosVersion}/search";
        public const string DisenosVersionLoadcombos = $"{DisenosVersion}/loadcombos";
        public const string DisenosEstructura               = $"{Prefix}/ingenieria/disenos-estructura";
        public static string DisenosEstructuraDe(string codDiseno, int numVersion, int maxNivel = 10) => $"{DisenosEstructura}/{Uri.EscapeDataString(codDiseno)}/{numVersion}?maxNivel={maxNivel}";
        public const string DisenosEstructuraSector         = $"{DisenosEstructura}/sector";
        public static string DisenosEstructuraSectorEliminar(string codDiseno, int numVersion, string codSector) => $"{DisenosEstructuraSector}/{Uri.EscapeDataString(codDiseno)}/{numVersion}/{Uri.EscapeDataString(codSector)}";
        public const string DisenosEstructuraComponente     = $"{DisenosEstructura}/componente";
        public const string DisenosEstructuraComponenteQuitar = $"{DisenosEstructura}/componente/quitar";
        public const string DisenosEstructuraComponenteCambiar = $"{DisenosEstructura}/componente/cambiar";
        public const string DisenosEstructuraInsumo         = $"{DisenosEstructura}/insumo";
        public const string DisenosEstructuraInsumoEliminar = $"{DisenosEstructura}/insumo/eliminar";
        public const string DisenosEstructuraBloquear       = $"{DisenosEstructura}/bloquear";
        public const string DisenosEstructuraDesbloquear    = $"{DisenosEstructura}/desbloquear";
        public const string DisenosEstructuraCopiarVersion  = $"{DisenosEstructura}/copiar-version";
        public const string DisenosEstructuraCopiarSegmento = $"{DisenosEstructura}/copiar-segmento";
        public const string DisenosRutaCatalogos         = $"{DisenosEstructura}/ruta-catalogos";
        public const string DisenosRutaCrear             = $"{DisenosEstructura}/ruta/crear";
        public const string DisenosRutaAsignar           = $"{DisenosEstructura}/ruta/asignar";
        public const string DisenosRutaCopiar            = $"{DisenosEstructura}/ruta/copiar";
        public const string DisenosEspecificaciones        = $"{Prefix}/ingenieria/disenos-especificaciones";
        public static string DisenosEspecificacionesDe(string codDiseno, int numVersion) => $"{DisenosEspecificaciones}/{Uri.EscapeDataString(codDiseno)}/{numVersion}";
        public const string DisenosEspecificacionesGuardar = $"{DisenosEspecificaciones}/guardar";
        // Operaciones y tiempos estandar (scripts SQL 119 a 122)
        public const string CategoriasOcupacionales        = $"{Prefix}/ingenieria/categorias-ocupacionales";
        public const string TiposMaquina                   = $"{Prefix}/ingenieria/tipos-maquina";
        public const string MetodosTiempo                  = $"{Prefix}/ingenieria/metodos-tiempo";
        public const string TiposDistribucionLinea         = $"{Prefix}/produccion/tipos-distribucion-linea";
        public const string Operaciones                    = $"{Prefix}/ingenieria/operaciones";
        public const string OperacionesCombos              = $"{Operaciones}/combos";
        public const string OperacionesSiguienteCodigo     = $"{Operaciones}/siguiente-codigo";
        public static string Operacion(string cod, string? sub = null) => $"{Operaciones}/{Uri.EscapeDataString(cod)}" + (sub is null ? "" : "/" + sub);
        public const string OperacionesTiempos             = $"{Operaciones}/tiempos";
        public const string OperacionesTiemposAprobar      = $"{Operaciones}/tiempos/aprobar";
        public const string OperacionesTiemposEliminar     = $"{Operaciones}/tiempos/eliminar";
        public const string OperacionesEstudios            = $"{Operaciones}/estudios";
        public static string OperacionesEstudioElementos(int num) => $"{Operaciones}/estudios/{num}/elementos";
        public const string RutasTiempos                   = $"{Prefix}/ingenieria/rutas-tiempos";
        public static string RutaTiempos(string ruta, string sub) => $"{RutasTiempos}/{Uri.EscapeDataString(ruta)}/{sub}";
        // Matrices de operaciones (script SQL 130)
        public const string Matrices                       = $"{Prefix}/ingenieria/matrices";
        public const string MatricesDuplicar               = $"{Matrices}/duplicar";
        public static string Matriz(string cod)            => $"{Matrices}/{Uri.EscapeDataString(cod)}";
        // Ruta de operaciones de una version de diseno (scripts SQL 130 y 132)
        public const string DisenosMatrices                = $"{Prefix}/ingenieria/disenos-matrices";
        public static string DisenosMatricesDeVersion(string diseno, int version) => $"{DisenosMatrices}/{Uri.EscapeDataString(diseno)}/{version}/matrices";
        public static string DisenosRutaOperaciones(string diseno, int version)   => $"{DisenosMatrices}/{Uri.EscapeDataString(diseno)}/{version}/ruta";
        public static string DisenosRutaIndependizar(string diseno, int version)  => $"{DisenosMatrices}/{Uri.EscapeDataString(diseno)}/{version}/independizar";
        public const string DisenosMatrizAsignar           = $"{DisenosMatrices}/asignar";
        public const string DisenosGuardarComoMatriz       = $"{DisenosMatrices}/guardar-como-matriz";
        // Personal por categoria y armado de lineas (scripts SQL 124 y 125)
        public const string PersonalOperacion              = $"{Prefix}/produccion/personal-operacion";
        public const string PersonalOperacionCombos        = $"{PersonalOperacion}/combos";
        public const string PersonalOperacionDisponibles   = $"{PersonalOperacion}/disponibles";
        public const string PersonalOperacionResumen       = $"{PersonalOperacion}/resumen";
        public static string PersonalOperacionDe(int codEntidad) => $"{PersonalOperacion}/{codEntidad}";
        public const string LineasArmados                  = $"{Prefix}/ingenieria/lineas-armados";
        public const string LineasArmadosCombos            = $"{LineasArmados}/combos";
        public const string LineasArmadosSiguienteCodigo   = $"{LineasArmados}/siguiente-codigo";
        public static string LineaArmado(string cod, string? sub = null) => $"{LineasArmados}/{Uri.EscapeDataString(cod)}" + (sub is null ? "" : "/" + sub);
        // Migracion de operaciones de rutas existentes (script SQL 128)
        public const string OperacionesMigracion           = $"{Prefix}/ingenieria/operaciones-migracion";
        public const string OperacionesMigracionResumen    = $"{OperacionesMigracion}/resumen";
        public const string OperacionesMigracionPropuesta  = $"{OperacionesMigracion}/propuesta";
        public const string DisenosTizado                  = $"{Prefix}/ingenieria/disenos-tizado";
        public static string DisenosTizadoMoldes(string codDiseno, int numVersion) => $"{DisenosTizado}/moldes/{Uri.EscapeDataString(codDiseno)}/{numVersion}";
        public const string DisenosTizadoMoldesGuardar     = $"{DisenosTizado}/moldes/guardar";
        public const string DisenosTizadoMoldesEliminar    = $"{DisenosTizado}/moldes/eliminar";
        public const string DisenosTizadoImportarDxf       = $"{DisenosTizado}/moldes/importar-dxf";
        public const string DisenosTizadoCalcular          = $"{DisenosTizado}/calcular";
        public const string DisenosTizadoGuardar           = $"{DisenosTizado}/guardar";
        public static string DisenosTizados(string codDiseno, int numVersion) => $"{DisenosTizado}/tizados/{Uri.EscapeDataString(codDiseno)}/{numVersion}";
        public static string DisenosTizadoDetalle(string codDiseno, int numVersion, int num) => $"{DisenosTizados(codDiseno, numVersion)}/{num}";
        public static string DisenosTizadoDxf(string codDiseno, int numVersion, int num) => $"{DisenosTizadoDetalle(codDiseno, numVersion, num)}/dxf";
        public const string DisenosTizadoEliminar          = $"{DisenosTizado}/tizados/eliminar";
        public const string DisenosTizadoAjustar           = $"{DisenosTizado}/tizados/ajustar";
        public static string DisenosTizadoGradacion(string codDiseno, int numVersion) => $"{DisenosTizado}/gradacion/{Uri.EscapeDataString(codDiseno)}/{numVersion}";
        public const string DisenosTizadoGradacionPrevia   = $"{DisenosTizado}/gradacion/previsualizar";
        public const string DisenosTizadoGradacionGenerar  = $"{DisenosTizado}/gradacion/generar";
        public const string DisenosTizadoGradacionEliminar = $"{DisenosTizado}/gradacion/eliminar";
        public const string ComponentesProductoCombobox     = $"{Prefix}/ingenieria/componentes-producto/combobox";
        public const string ProduccionRutas         = $"{Prefix}/ingenieria/produccion-rutas";
        public const string ProduccionRutasCombobox = $"{ProduccionRutas}/combobox";
        public const string ClasesDesarrollo         = $"{Prefix}/ingenieria/clases-desarrollo";
        public const string ClasesDesarrolloCombobox = $"{ClasesDesarrollo}/combobox";
        public const string TiposDesarrollo          = $"{Prefix}/ingenieria/tipos-desarrollo";
        public const string TiposDesarrolloCombobox  = $"{TiposDesarrollo}/combobox";
        public const string TiposDesarrolloSearch    = $"{TiposDesarrollo}/search";
        public const string GruposEspecificacionTecnicaGrupos         = $"{Prefix}/ingenieria/grupos-familias-sub-familias-grupos-especificacion-tecnica";
        public const string GruposEspecificacionTecnicaGruposCombobox = $"{GruposEspecificacionTecnicaGrupos}/combobox";

        // ── Rutas restauradas (reconstruidas desde los controllers de la API) ──
        public const string GruposEspecificacionTecnicaLoadcombos = $"{Prefix}/ingenieria/grupos-familias-sub-familias-especificacion-tecnica/loadcombos";
    }

    public static class Controlcalidad
    {
        public const string ClasesDefectosTecnicos         = $"{Prefix}/controlcalidad/clases-defectos-tecnicos";
        public const string ClasesDefectosTecnicosCombobox = $"{ClasesDefectosTecnicos}/combobox";
        public const string TiposDefectosTecnicos          = $"{Prefix}/controlcalidad/tipos-defectos-tecnicos";
        public const string TiposDefectosTecnicosCombobox  = $"{TiposDefectosTecnicos}/combobox";
    }

    public static class Biometria
    {
        public const string Base      = $"{Prefix}/biometria";
        public const string Estados   = $"{Base}/estados";
        public const string Estado    = $"{Base}/estado";
        public const string Registrar = $"{Base}/registrar";
        public const string Verificar = $"{Base}/verificar";
    }

    public static class Externo
    {
        public const string Snippets = $"{Prefix}/externo/snippets";
        public const string Traducir = $"{Snippets}/traducir";
    }
    public static class Sunat
    {
        public const string RegimenesTributarios         = $"{Prefix}/sunat/regimenes-tributarios";
        public const string RegimenesTributariosCombobox = $"{RegimenesTributarios}/combobox";
        public const string RegimenesTributariosSearch   = $"{RegimenesTributarios}/search";

        public const string CubsoSunat                   = $"{Prefix}/sunat/cubso-sunat";
        public const string CubsoSunatCombobox            = $"{CubsoSunat}/combobox";
        public const string CubsoSunatSearch              = $"{CubsoSunat}/search";
        public const string CubsoSunatDownloadFromSnippet = $"{CubsoSunat}/download-from-snippet";

        // ── Rutas restauradas (reconstruidas desde los controllers de la API) ──
        public const string RegimenesTributariosLoadcombos = $"{Prefix}/sunat/regimenes-tributarios/loadcombos";
    }

    public static class Developer
    {
        public const string ErrorLog         = $"{Prefix}/developer/error-log";
        public const string AccessLog        = $"{Prefix}/developer/access-log";
        public const string Logs             = $"{Prefix}/developer/logs";
        // Inventario de métodos de un Controller (por reflexión), para el combo "Método de
        // la API" al configurar botones. Lo sirve MetodosApiController: no toca la base.
        public const string MetadataMetodosController = $"{Prefix}/developer/metadata/metodos-controller";
    }

    public static class Logistica
    {
        // ── Requerimientos y Reservas de stock ────────────────────────────────
        // Los lotes alimentan el combo del detalle: el ítem puede no tener lote, así que
        // el combo se deja vaciar.
        // Existencias por almacén y ubicación. La consulta va por POST: son veintitrés
        // filtros y varios de texto libre.
        public const string StocksAlmacenUbicacion             = $"{Prefix}/logistica/stocks-almacen-ubicacion";
        public const string StocksAlmacenUbicacionSearchDetail = $"{StocksAlmacenUbicacion}/search-detail";
        // Los cinco combos independientes en una sola llamada.
        public const string StocksAlmacenUbicacionLoadCombos   = $"{StocksAlmacenUbicacion}/loadcombos";
        // Lee una etiqueta (RFID o código de barras): qué es y cómo está su stock.
        public const string StocksAlmacenUbicacionEtiqueta     = $"{StocksAlmacenUbicacion}/etiqueta";
        // Kardex de una línea de stock: clave completa y fechas en la query string.
        public const string StocksAlmacenUbicacionKardex       = $"{StocksAlmacenUbicacion}/kardex";
        // Mapa de trazabilidad de requerimientos (tipo y clave del nodo en la query string).
        // Traslados entre ubicaciones de un mismo almacén (de un rack a otro).
        public const string TrasladosUbicaciones               = $"{Prefix}/logistica/traslados-ubicaciones";
        public static string TrasladosUbicacionesTipos(string codClaseMovimiento) =>
            $"{TrasladosUbicaciones}/tipos?COD_CLASE_MOVIMIENTO={Uri.EscapeDataString(codClaseMovimiento)}";
        public static string TrasladosUbicacionesMapa(string codAlmacen) =>
            $"{TrasladosUbicaciones}/mapa?COD_ALMACEN={Uri.EscapeDataString(codAlmacen)}";
        public static string TrasladosUbicacionesStock(string codAlmacen, string codUbicacion) =>
            $"{TrasladosUbicaciones}/stock?COD_ALMACEN={Uri.EscapeDataString(codAlmacen)}&COD_UBICACION_ALMACEN={Uri.EscapeDataString(codUbicacion)}";
        public const string TrasladosUbicacionesRegistra       = $"{TrasladosUbicaciones}/registra";
        public static string TrasladosUbicacionesHistorial(string codAlmacen, DateTime? desde, DateTime? hasta) =>
            $"{TrasladosUbicaciones}/historial?COD_ALMACEN={Uri.EscapeDataString(codAlmacen)}"
            + (desde is { } d ? $"&FEC_INICIAL={d:yyyy-MM-dd}" : "")
            + (hasta is { } h ? $"&FEC_FINAL={h:yyyy-MM-dd}" : "");
        // Confirmación de recepción de transferencias: el almacén destino recibe o rechaza las salidas.
        public const string ConfirmacionRecepcion              = $"{Prefix}/logistica/confirmacion-recepcion";
        public static string ConfirmacionRecepcionPendientes(string codRol, string? codAlmacen) =>
            $"{ConfirmacionRecepcion}/pendientes?COD_ROL_USUARIO={Uri.EscapeDataString(codRol)}"
            + (string.IsNullOrWhiteSpace(codAlmacen) ? "" : $"&COD_ALMACEN={Uri.EscapeDataString(codAlmacen)}");
        public static string ConfirmacionRecepcionDetalle(decimal numMovimiento) =>
            $"{ConfirmacionRecepcion}/{numMovimiento.ToString(System.Globalization.CultureInfo.InvariantCulture)}/detalle";
        public static string ConfirmacionRecepcionMotivosRechazo(decimal numMovimiento) =>
            $"{ConfirmacionRecepcion}/{numMovimiento.ToString(System.Globalization.CultureInfo.InvariantCulture)}/motivos-rechazo";
        public static string ConfirmacionRecepcionLecturaContexto(decimal numMovimiento) =>
            $"{ConfirmacionRecepcion}/{numMovimiento.ToString(System.Globalization.CultureInfo.InvariantCulture)}/lectura";
        /// <summary>Las etiquetas que leyó la salida (con numSecuencia, solo las de esa línea): lo que pasa al ingreso si se confirma sin leer.</summary>
        public static string ConfirmacionRecepcionEtiquetas(decimal numMovimiento, int? numSecuencia = null) =>
            $"{ConfirmacionRecepcion}/{numMovimiento.ToString(System.Globalization.CultureInfo.InvariantCulture)}/etiquetas"
            + (numSecuencia is null ? "" : $"?NUM_SECUENCIA={numSecuencia}");
        public const string ConfirmacionRecepcionLectura       = $"{ConfirmacionRecepcion}/lectura";
        public const string ConfirmacionRecepcionConfirmar     = $"{ConfirmacionRecepcion}/confirmar";
        public const string ConfirmacionRecepcionRechazar      = $"{ConfirmacionRecepcion}/rechazar";
        public const string Trazabilidad                       = $"{Prefix}/logistica/trazabilidad";
        public const string TrazabilidadNodo                   = $"{Trazabilidad}/nodo";
        public const string TrazabilidadVecinos                = $"{Trazabilidad}/vecinos";
        public const string TrazabilidadEntidad                = $"{Trazabilidad}/entidad";
        public const string TrazabilidadBuscar                 = $"{Trazabilidad}/buscar";
        public const string TrazabilidadFranjas                = $"{Trazabilidad}/franjas";
        // Los lotes de un proveedor.
        public static string LotesControlPorEntidad(decimal cod) => $"{LotesControl}/por-entidad/{cod}";

        public const string LotesControl                = $"{Prefix}/logistica/lotes-control";
        public const string LotesControlCombobox        = $"{LotesControl}/combobox";
        // El detalle del lote se pide por cabecera: el procedimiento lo exige.
        public const string LotesControlDetalle         = $"{Prefix}/logistica/lotes-control-detalle";
        public const string LotesControlDetallePorDoc   = $"{LotesControlDetalle}/por-cabecera";
        public const string TiposLoteControl            = $"{Prefix}/logistica/tipos-lote-control";
        public const string TiposLoteControlCombobox    = $"{TiposLoteControl}/combobox";

        public const string TiposRequerimiento          = $"{Prefix}/logistica/tipos-requerimiento";
        public const string TiposRequerimientoCombobox  = $"{TiposRequerimiento}/combobox";
        public const string TiposReservaStock           = $"{Prefix}/logistica/tipos-reserva-stock";
        public const string TiposReservaStockCombobox   = $"{TiposReservaStock}/combobox";

        public const string RequerimientosStock         = $"{Prefix}/logistica/requerimientos-stock";
        public const string RequerimientosStockCombobox = $"{RequerimientosStock}/combobox";
        // El detalle cuelga de la cabecera: se pide por número de documento.
        public const string RequerimientosStockDetalle       = $"{Prefix}/logistica/requerimientos-stock-detalle";
        public const string RequerimientosStockDetallePorDoc = $"{RequerimientosStockDetalle}/por-cabecera";

        public const string ReservasStock               = $"{Prefix}/logistica/reservas-stock";
        public const string ReservasStockCombobox       = $"{ReservasStock}/combobox";
        public const string ReservasStockDetalle        = $"{Prefix}/logistica/reservas-stock-detalle";
        public const string ReservasStockDetallePorDoc  = $"{ReservasStockDetalle}/por-cabecera";

        // Guías de remisión que bajó el servicio del correo (XML recibidos).
        public const string GreRecibidas             = $"{Prefix}/logistica/gre-recibidas";
        public static string GreRecibida(decimal numGre, string? parte = null) => $"{GreRecibidas}/{numGre:0}" + (parte is null ? "" : "/" + parte);
        public const string Almacenes                = $"{Prefix}/logistica/almacenes";
        public const string AlmacenesCombobox        = $"{Almacenes}/combobox";
        public const string AlmacenesSearch          = $"{Almacenes}/search";

        // Tabla hija: el listado y el combo cuelgan del COD_ALMACEN
        // (…/almacenes-ubicaciones-almacen/{codAlmacen}).
        public const string AlmacenesUbicaciones     = $"{Prefix}/logistica/almacenes-ubicaciones-almacen";
        public const string AlmacenesUbicacionesSearch = $"{AlmacenesUbicaciones}/search";

        public const string ContenedoresAlmacenaje         = $"{Prefix}/logistica/contenedores-almacenaje";
        public const string ContenedoresAlmacenajeCombobox = $"{ContenedoresAlmacenaje}/combobox";
        public const string ContenedoresAlmacenajeSearch   = $"{ContenedoresAlmacenaje}/search";

        // Casilleros: hijos del contenedor. Showall/combobox llevan el COD_CONTENEDOR en la ruta.
        public const string ContenedoresCasilleros         = $"{Prefix}/logistica/contenedores-almacenaje-casilleros";
        public const string ContenedoresCasillerosCombobox = $"{ContenedoresCasilleros}/combobox";
        public const string ContenedoresCasillerosSearch   = $"{ContenedoresCasilleros}/search";
        public const string ContenedoresCasillerosGenerar  = $"{ContenedoresCasilleros}/generar";

        // Sin ruta de search: la tabla no tiene SP _SEARCH y se filtra en el cliente.
        public const string TiposUbicacion           = $"{Prefix}/logistica/tipos-ubicacion";
        public const string TiposUbicacionCombobox   = $"{TiposUbicacion}/combobox";

        // Cierre de almacenes por fecha: POST cierra (uno o varios almacenes), PUT modifica, PUT {id}/reabrir reabre, DELETE borra un cierre reabierto.
        public const string AlmacenesCierre          = $"{Prefix}/logistica/almacenes-cierre";
        public static string AlmacenesCierreReabrir(int numCierreAlmacen) => $"{AlmacenesCierre}/{numCierreAlmacen}/reabrir";

        public const string TiposAlmacen             = $"{Prefix}/logistica/tipos-almacen";
        public const string TiposAlmacenCombobox     = $"{TiposAlmacen}/combobox";
        public const string TiposAlmacenSearch       = $"{TiposAlmacen}/search";

        public const string ClasesMovimiento         = $"{Prefix}/logistica/clases-movimiento";
        public const string ClasesMovimientoCombobox = $"{ClasesMovimiento}/combobox";
        public const string ClasesMovimientoSearch   = $"{ClasesMovimiento}/search";

        public const string TiposMovimiento          = $"{Prefix}/logistica/tipos-movimiento";
        public const string TiposMovimientoCombobox  = $"{TiposMovimiento}/combobox";
        public const string TiposMovimientoSearch    = $"{TiposMovimiento}/search";

        // Tipos de movimiento habilitados por almacén. Las lecturas llevan almacén y clase
        // en la ruta porque el SP exige las dos: un tipo sólo existe dentro de su clase.
        public const string AlmacenesTiposMovimiento           = $"{Prefix}/logistica/almacenes-tipos-movimiento";
        public const string AlmacenesTiposMovimientoAsignar    = $"{AlmacenesTiposMovimiento}/asignar";
        public const string AlmacenesTiposMovimientoDesasignar = $"{AlmacenesTiposMovimiento}/desasignar";

        // ── Rutas restauradas (reconstruidas desde los controllers de la API) ──
        public const string AlmacenesLoadcombos = $"{Prefix}/logistica/almacenes/loadcombos";
        public static string AlmacenesUbicacionesCombobox(string codAlmacen) =>
            $"{Prefix}/logistica/almacenes-ubicaciones-almacen/combobox/{Uri.EscapeDataString(codAlmacen)}";
        public const string ContenedoresAlmacenajeLoadcombos = $"{Prefix}/logistica/contenedores-almacenaje/loadcombos";
        public static string ContenedoresCasillerosPorContenedor(string codContenedor) =>
            $"{Prefix}/logistica/contenedores-almacenaje-casilleros/{Uri.EscapeDataString(codContenedor)}";
        public static string ContenedoresCasillerosPorUbicacion(string codAlmacen, string codUbicacionAlmacen) =>
            $"{Prefix}/logistica/contenedores-almacenaje-casilleros/por-ubicacion/{Uri.EscapeDataString(codAlmacen)}/{Uri.EscapeDataString(codUbicacionAlmacen)}";
        public const string FlujosAprobacion = $"{Prefix}/logistica/flujos-aprobacion";
        public const string FlujosAprobacionAreaCargo = $"{Prefix}/logistica/flujos-aprobacion-area-cargo";
        public const string FlujosAprobacionAreaCargoCopy = $"{Prefix}/logistica/flujos-aprobacion-area-cargo/copy";
        public const string FlujosAprobacionSearch = $"{Prefix}/logistica/flujos-aprobacion/search";
        public const string FlujosAprobacionTiposOperacion = $"{Prefix}/logistica/flujos-aprobacion-tipos-operacion";
        public const string Inventarios = $"{Prefix}/logistica/inventarios";
        public static string InventariosAbrir(int numInventario) =>
            $"{Prefix}/logistica/inventarios/{numInventario}/abrir";
        public const string InventariosAlmacenes = $"{Prefix}/logistica/inventarios-almacenes";
        public const string InventariosAlmacenesImagen = $"{Prefix}/logistica/inventarios-almacenes-imagen";
        public const string InventariosAlmacenesProductos = $"{Prefix}/logistica/inventarios-almacenes-productos";
        public const string InventariosAlmacenesPuntoToma = $"{Prefix}/logistica/inventarios-almacenes-punto-toma";
        public const string InventariosAlmacenesPuntoTomaConteo = $"{Prefix}/logistica/inventarios-almacenes-punto-toma-conteo";
        public const string InventariosAlmacenesPuntoTomaConteoReadRfidCodebar = $"{Prefix}/logistica/inventarios-almacenes-punto-toma-conteo/read-rfid-codebar";
        public const string InventariosAlmacenesPuntoTomaConteoSearch = $"{Prefix}/logistica/inventarios-almacenes-punto-toma-conteo/search";
        public const string InventariosAlmacenesPuntoTomaEquiposAsignados = $"{Prefix}/logistica/inventarios-almacenes-punto-toma-equipos-asignados";
        public const string InventariosAlmacenesPuntoTomaPersonal = $"{Prefix}/logistica/inventarios-almacenes-punto-toma-personal";
        public const string InventariosAlmacenesUbicaciones = $"{Prefix}/logistica/inventarios-almacenes-ubicaciones";
        public static string InventariosCerrar(int numInventario) =>
            $"{Prefix}/logistica/inventarios/{numInventario}/cerrar";
        public static string InventariosImagen(int numInventario) =>
            $"{Prefix}/logistica/inventarios/{numInventario}/imagen";
        public static string InventariosRequerimiento(int numInventario) =>
            $"{Prefix}/logistica/inventarios/{numInventario}/requerimiento";
        public static string InventariosRequerimientoRevierte(int numInventario) =>
            $"{Prefix}/logistica/inventarios/{numInventario}/requerimiento-revierte";
        public const string InventariosSearch = $"{Prefix}/logistica/inventarios/search";
        public static string LotesControlComboboxDe(decimal codEntidad) =>
            $"{Prefix}/logistica/lotes-control/combobox?codEntidad={codEntidad}";
        public const string MovimientosAlmacen = $"{Prefix}/logistica/movimientos-almacen";
        public static string MovimientosAlmacenAgregarDesdePedidoComercial(decimal numMovimientoAlmacen) =>
            $"{Prefix}/logistica/movimientos-almacen/{numMovimientoAlmacen}/agregar-desde-pedido-comercial";
        // Devolución de compra: guías del mismo proveedor, items y cantidades a devolver.
        public static string MovimientosAlmacenDevolucionGuiasDisponibles(decimal num) =>
            $"{Prefix}/logistica/movimientos-almacen/{num}/devolucion-compra/guias-disponibles";
        public static string MovimientosAlmacenDevolucionGuias(decimal num) =>
            $"{Prefix}/logistica/movimientos-almacen/{num}/devolucion-compra/guias";
        public static string MovimientosAlmacenDevolucionGuia(decimal num, int origen) =>
            $"{Prefix}/logistica/movimientos-almacen/{num}/devolucion-compra/guias/{origen}";
        public static string MovimientosAlmacenDevolucionLineas(decimal num, string? origenes = null) =>
            $"{Prefix}/logistica/movimientos-almacen/{num}/devolucion-compra/lineas" +
            (string.IsNullOrWhiteSpace(origenes) ? "" : $"?origenes={Uri.EscapeDataString(origenes)}");
        public static string MovimientosAlmacenDevolucionAdicionar(decimal num) =>
            $"{Prefix}/logistica/movimientos-almacen/{num}/devolucion-compra/adicionar";
        public static string MovimientosAlmacenAgregarDesdeRequerimiento(decimal numMovimientoAlmacen) =>
            $"{Prefix}/logistica/movimientos-almacen/{numMovimientoAlmacen}/agregar-desde-requerimiento";
        public static string MovimientosAlmacenAgregarDesdeReserva(decimal numMovimientoAlmacen) =>
            $"{Prefix}/logistica/movimientos-almacen/{numMovimientoAlmacen}/agregar-desde-reserva";
        // Botón TRANSFERIR (documento y grilla): pasa el movimiento a su estado "enviado".
        public static string MovimientosAlmacenTransferir(decimal numMovimientoAlmacen) =>
            $"{Prefix}/logistica/movimientos-almacen/{numMovimientoAlmacen}/transferir";
        public const string MovimientosAlmacenDetalle = $"{Prefix}/logistica/movimientos-almacen-detalle";
        // Alta manual del detalle: stock con disponible (salidas) y combos de la línea.
        public static string MovimientosAlmacenDetalleStockDisponible(decimal numMovimientoAlmacen) =>
            $"{MovimientosAlmacenDetalle}/stock-disponible?NUM_MOVIMIENTO_ALMACEN={numMovimientoAlmacen}";
        public static string MovimientosAlmacenDetalleCombosLinea(decimal numMovimientoAlmacen) =>
            $"{MovimientosAlmacenDetalle}/combos-linea?NUM_MOVIMIENTO_ALMACEN={numMovimientoAlmacen}";
        // Lecturas de etiquetas (RFID / código de barras) del movimiento.
        public const string MovimientosAlmacenLecturas = $"{MovimientosAlmacenDetalle}/lecturas";
        public const string MovimientosAlmacenEtiquetasUpdate = $"{MovimientosAlmacenDetalle}/etiquetas";
        public static string MovimientosAlmacenEtiquetas(decimal numMovimientoAlmacen, int? numSecuencia = null, string? search = null) =>
            $"{MovimientosAlmacenDetalle}/etiquetas?NUM_MOVIMIENTO_ALMACEN={numMovimientoAlmacen}"
            + (numSecuencia is null ? "" : $"&NUM_SECUENCIA={numSecuencia}")
            + (string.IsNullOrWhiteSpace(search) ? "" : $"&Search={Uri.EscapeDataString(search)}");
        public static string MovimientosAlmacenEtiquetaDelete(decimal numMovimientoAlmacen, int numSecuencia, string codStickerRfid) =>
            $"{MovimientosAlmacenDetalle}/etiquetas?NUM_MOVIMIENTO_ALMACEN={numMovimientoAlmacen}&NUM_SECUENCIA={numSecuencia}&COD_STICKER_RFID={Uri.EscapeDataString(codStickerRfid)}";
        public static string MovimientosAlmacenLecturasPendientes(decimal numMovimientoAlmacen) =>
            $"{MovimientosAlmacenDetalle}/lecturas-pendientes?NUM_MOVIMIENTO_ALMACEN={numMovimientoAlmacen}";
        public static string MovimientosAlmacenLoadcombosPorClase(string codClaseMovimiento) =>
            $"{Prefix}/logistica/movimientos-almacen/loadcombos?codClaseMovimiento={Uri.EscapeDataString(codClaseMovimiento)}";
        public static string MovimientosAlmacenOrdenesAsignados(decimal numMovimientoAlmacen) =>
            $"{Prefix}/logistica/movimientos-almacen/{numMovimientoAlmacen}/ordenes-compra/asignados";
        public static string MovimientosAlmacenOrdenesAsignadosDetalle(decimal numMovimientoAlmacen) =>
            $"{Prefix}/logistica/movimientos-almacen/{numMovimientoAlmacen}/ordenes-compra/asignados/detalle";
        public static string MovimientosAlmacenOrdenesAsignar(decimal numMovimientoAlmacen) =>
            $"{Prefix}/logistica/movimientos-almacen/{numMovimientoAlmacen}/ordenes-compra/asignar";
        public static string MovimientosAlmacenOrdenesDisponibles(decimal numMovimientoAlmacen) =>
            $"{Prefix}/logistica/movimientos-almacen/{numMovimientoAlmacen}/ordenes-compra/disponibles";
        public static string MovimientosAlmacenOrdenesDisponiblesDetalle(decimal numMovimientoAlmacen, string codClaseOperacion, string codTipoOperacion, IEnumerable<int> pedidos) =>
            $"{Prefix}/logistica/movimientos-almacen/{numMovimientoAlmacen}/ordenes-compra/disponibles/detalle?codClaseOperacion={Uri.EscapeDataString(codClaseOperacion)}&codTipoOperacion={Uri.EscapeDataString(codTipoOperacion)}&pedidos={string.Join("_", pedidos)}";
        public const string MovimientosAlmacenOrdenesLoteUbicacion = $"{Prefix}/logistica/movimientos-almacen/ordenes-compra/lote-ubicacion";
        public static string MovimientosAlmacenOrdenesQuitar(decimal numMovimientoAlmacen) =>
            $"{Prefix}/logistica/movimientos-almacen/{numMovimientoAlmacen}/ordenes-compra/quitar";
        public static string MovimientosAlmacenRequerimientosAsignados(decimal numMovimientoAlmacen) =>
            $"{Prefix}/logistica/movimientos-almacen/{numMovimientoAlmacen}/requerimientos/asignados";
        public static string MovimientosAlmacenRequerimientosAsignadosDetalle(decimal numMovimientoAlmacen) =>
            $"{Prefix}/logistica/movimientos-almacen/{numMovimientoAlmacen}/requerimientos/asignados/detalle";
        public static string MovimientosAlmacenRequerimientosAsignar(decimal numMovimientoAlmacen) =>
            $"{Prefix}/logistica/movimientos-almacen/{numMovimientoAlmacen}/requerimientos/asignar";
        public static string MovimientosAlmacenRequerimientosCobertura(int numMovimiento, int numRequerimiento, string criterio) =>
            $"{Prefix}/logistica/movimientos-almacen/{numMovimiento}/requerimientos/disponibles/{numRequerimiento}/cobertura?criterio={Uri.EscapeDataString(criterio)}";
        public const string MovimientosAlmacenRequerimientosDisponibles = $"{Prefix}/logistica/movimientos-almacen/requerimientos/disponibles";
        public static string MovimientosAlmacenRequerimientosQuitar(decimal numMovimientoAlmacen) =>
            $"{Prefix}/logistica/movimientos-almacen/{numMovimientoAlmacen}/requerimientos/quitar";
        public const string MovimientosAlmacenTransporte = $"{Prefix}/logistica/movimientos-almacen-transporte";
        public static string MovimientosAlmacenTransporteDe(decimal numMovimientoAlmacen) =>
            $"{Prefix}/logistica/movimientos-almacen-transporte?NUM_MOVIMIENTO_ALMACEN={numMovimientoAlmacen}";
        public const string PedidosComercialesCronogramaEntregas = $"{Prefix}/logistica/pedidos-comerciales-cronograma-entregas";
        public const string PedidosComercialesCronogramaEntregasPorPedido = $"{Prefix}/logistica/pedidos-comerciales-cronograma-entregas/por-pedido";
        public const string ProductosAlmacenes = $"{Prefix}/logistica/productos-almacenes";
        public const string ProductosAlmacenesCantidades = $"{Prefix}/logistica/productos-almacenes/cantidades";
        public const string ProductosAlmacenesPorAlmacen = $"{Prefix}/logistica/productos-almacenes/por-almacen";
        public const string ProductosAlmacenesRequerimiento = $"{Prefix}/logistica/productos-almacenes/requerimiento";
        public const string ProductosAlmacenesRequerir = $"{Prefix}/logistica/productos-almacenes/requerir";
        public const string ProductosAlmacenesReset = $"{Prefix}/logistica/productos-almacenes/reset";
        public const string ProductosAlmacenesReversion = $"{Prefix}/logistica/productos-almacenes/reversion";
        public static string RequerimientosStockGeneraReservas(int numRequerimientoStock) =>
            $"{Prefix}/logistica/requerimientos-stock/{numRequerimientoStock}/genera-reservas";
        public const string RequerimientosStockLoadcombos = $"{Prefix}/logistica/requerimientos-stock/loadcombos";
        // El filtro es opcional: sin tipo (null o vacío) se pide el listado completo.
        public static string RequerimientosStockPorTipo(string? codTipoRequerimiento) =>
            string.IsNullOrWhiteSpace(codTipoRequerimiento)
                ? $"{Prefix}/logistica/requerimientos-stock"
                : $"{Prefix}/logistica/requerimientos-stock?COD_TIPO_REQUERIMIENTO={Uri.EscapeDataString(codTipoRequerimiento)}";
        public static string RequerimientosStockRevierteReservas(int numRequerimientoStock) =>
            $"{Prefix}/logistica/requerimientos-stock/{numRequerimientoStock}/revierte-reservas";
        // Los dos filtros son opcionales (null o vacío = sin filtro). Los nombres son los del
        // ReservasStockSearchDto de la API.
        public static string ReservasStockFiltrado(string? codTipoReserva, string? codProducto)
        {
            var q = new List<string>();
            if (!string.IsNullOrWhiteSpace(codTipoReserva)) q.Add($"COD_TIPO_RESERVA={Uri.EscapeDataString(codTipoReserva)}");
            if (!string.IsNullOrWhiteSpace(codProducto))    q.Add($"COD_PRODUCTO={Uri.EscapeDataString(codProducto)}");
            return $"{Prefix}/logistica/reservas-stock" + (q.Count > 0 ? "?" + string.Join("&", q) : "");
        }
        public static string ReservasStockPorProducto(string codProducto) =>
            $"{Prefix}/logistica/reservas-stock?COD_PRODUCTO={Uri.EscapeDataString(codProducto)}";
        public const string TiposInventario = $"{Prefix}/logistica/tipos-inventario";
        public const string TiposInventarioCombobox = $"{Prefix}/logistica/tipos-inventario/combobox";
        public const string TiposInventarioSearch = $"{Prefix}/logistica/tipos-inventario/search";
        public const string TiposMovimientoLoadcombos = $"{Prefix}/logistica/tipos-movimiento/loadcombos";
        public const string TiposMovimientoTiposComprobantePago = $"{Prefix}/logistica/tipos-movimiento-tipos-comprobante-pago";
        public const string TiposTransporte = $"{Prefix}/logistica/tipos-transporte";
        public const string TiposTransporteCombobox = $"{Prefix}/logistica/tipos-transporte/combobox";
        public const string TiposTransporteSearch = $"{Prefix}/logistica/tipos-transporte/search";
        public const string Transportes = $"{Prefix}/logistica/transportes";
        public const string TransportesCombobox = $"{Prefix}/logistica/transportes/combobox";
        public const string TransportesEspecificacionesTecnicas = $"{Prefix}/logistica/transportes-especificaciones-tecnicas";
        public static string TransportesEspecificacionesTecnicasPorTransporte(string codTransporte) =>
            $"{Prefix}/logistica/transportes-especificaciones-tecnicas?COD_TRANSPORTE={Uri.EscapeDataString(codTransporte)}";
        public const string TransportesSearch = $"{Prefix}/logistica/transportes/search";
        public const string UsuariosTiposMovimientoDirectAccess = $"{Prefix}/logistica/usuarios-tipos-movimiento-direct-access";
        public static string UsuariosTiposMovimientoDirectAccessDe(string codClaseMovimiento) =>
            $"{Prefix}/logistica/usuarios-tipos-movimiento-direct-access?codClaseMovimiento={Uri.EscapeDataString(codClaseMovimiento)}";

        // ── Guía de remisión electrónica ─────────────────────────────────────
        public const string GuiasRemisionSeries = $"{Prefix}/logistica/guias-remision-series";
        public const string GuiasRemisionSeriesCombobox = $"{Prefix}/logistica/guias-remision-series/combobox";
        public const string GuiasRemision = $"{Prefix}/logistica/guias-remision";
        public static string GuiaRemisionPorMovimiento(decimal numMovimiento) =>
            $"{Prefix}/logistica/guias-remision/por-movimiento/{numMovimiento.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
        /// <summary>La revisión previa. Con <paramref name="m1l"/> se revisa como traslado en vehículos M1 o L (opción de la emisión).</summary>
        public static string GuiaRemisionValidar(decimal numMovimiento, bool m1l = false) =>
            $"{Prefix}/logistica/guias-remision/validar/{numMovimiento.ToString(System.Globalization.CultureInfo.InvariantCulture)}{(m1l ? "?m1l=true" : "")}";
        public static string GuiaRemisionEnvios(int numGuia) => $"{Prefix}/logistica/guias-remision/{numGuia}/envios";
        public static string GuiaRemisionXml(int numGuia) => $"{Prefix}/logistica/guias-remision/{numGuia}/xml";
        public static string GuiaRemisionCdr(int numGuia) => $"{Prefix}/logistica/guias-remision/{numGuia}/cdr";
        // La pestaña del movimiento pide la guía por su movimiento (objeto MOVIMIENTOS_ALMACEN); la bandeja, por el número de la guía (objeto GUIAS_REMISION).
        public static string GuiaRemisionEnviosPorMovimiento(decimal numMovimiento) =>
            $"{GuiaRemisionPorMovimiento(numMovimiento)}/envios";
        public static string GuiaRemisionXmlPorMovimiento(decimal numMovimiento) =>
            $"{GuiaRemisionPorMovimiento(numMovimiento)}/xml";
        public static string GuiaRemisionCdrPorMovimiento(decimal numMovimiento) =>
            $"{GuiaRemisionPorMovimiento(numMovimiento)}/cdr";
        // La emisión. Desde el movimiento (objeto MOVIMIENTOS_ALMACEN) se pide por el número del movimiento; desde la bandeja (objeto GUIAS_REMISION),
        // por el de la guía.
        public const string GuiaRemisionEmitir = $"{Prefix}/logistica/guias-remision/emitir";
        /// <summary>El chequeo de catálogos: lo que les falta a los registros que usan las salidas para poder emitir sus guías.</summary>
        public const string GuiasRemisionCatalogosPendientes = $"{Prefix}/logistica/guias-remision/catalogos-pendientes";
        public static string GuiaRemisionConsultarPorMovimiento(decimal numMovimiento) => $"{GuiaRemisionPorMovimiento(numMovimiento)}/consultar";
        public static string GuiaRemisionReintentarPorMovimiento(decimal numMovimiento) => $"{GuiaRemisionPorMovimiento(numMovimiento)}/reintentar";
        public static string GuiaRemisionOpcionesPorMovimiento(decimal numMovimiento) => $"{GuiaRemisionPorMovimiento(numMovimiento)}/opciones";
        /// <summary>¿El movimiento ya no se puede modificar porque su guía está ENVIADA o ACEPTADA? Siempre contesta 200.</summary>
        public static string GuiaRemisionBloqueoPorMovimiento(decimal numMovimiento) => $"{GuiaRemisionPorMovimiento(numMovimiento)}/bloqueo";
        public static string GuiaRemisionConsultar(int numGuia) => $"{Prefix}/logistica/guias-remision/{numGuia}/consultar";
        public static string GuiaRemisionReintentar(int numGuia) => $"{Prefix}/logistica/guias-remision/{numGuia}/reintentar";
        public static string GuiaRemisionOpciones(int numGuia) => $"{Prefix}/logistica/guias-remision/{numGuia}/opciones";
        /// <summary>El listado de la bandeja de guías, con el rango de fechas de emisión.</summary>
        public static string GuiasRemisionListado(DateTime? desde, DateTime? hasta)
        {
            var q = new List<string>();
            if (desde is { } d) q.Add($"FEC_DESDE={d:yyyy-MM-dd}");
            if (hasta is { } h) q.Add($"FEC_HASTA={h:yyyy-MM-dd}");
            return q.Count == 0 ? GuiasRemision : $"{GuiasRemision}?{string.Join("&", q)}";
        }
    }

    public static class Zonificacion
    {
        // Tabla hija de Sucursales: el listado y el combo cuelgan del COD_SUCURSAL.
        public const string Zonificaciones       = $"{Prefix}/zonificacion/zonificacion";
        public const string ZonificacionesSearch = $"{Zonificaciones}/search";

        public const string TiposZonificacion         = $"{Prefix}/zonificacion/tipos-zonificacion";
        public const string TiposZonificacionCombobox = $"{TiposZonificacion}/combobox";
        public const string TiposZonificacionSearch   = $"{TiposZonificacion}/search";

        // ── Rutas restauradas (reconstruidas desde los controllers de la API) ──
        public static string ZonificacionesLoadcombos(string codSucursal) =>
            $"{Prefix}/zonificacion/zonificacion/loadcombos/{Uri.EscapeDataString(codSucursal)}";
    }

    public static class Produccion
    {
        public const string ClasesProceso          = $"{Prefix}/produccion/clases-proceso";
        public const string ClasesProcesoCombobox  = $"{ClasesProceso}/combobox";
        public const string ClasesProcesoSearch    = $"{ClasesProceso}/search";

        public const string Procesos               = $"{Prefix}/produccion/procesos";
        public const string ProcesosCombobox        = $"{Procesos}/combobox";
        public const string ProcesosSearch          = $"{Procesos}/search";

        public const string SubProcesos             = $"{Prefix}/produccion/sub-procesos";
        public const string SubProcesosCombobox     = $"{SubProcesos}/combobox";
        public const string SubProcesosSearch       = $"{SubProcesos}/search";

        public const string TiposLineaProduccion         = $"{Prefix}/produccion/tipos-linea-produccion";
        public const string TiposLineaProduccionCombobox = $"{TiposLineaProduccion}/combobox";
        public const string TiposLineaProduccionSearch   = $"{TiposLineaProduccion}/search";

        // ── Rutas restauradas (reconstruidas desde los controllers de la API) ──
        public const string ProcesosLoadcombos = $"{Prefix}/produccion/procesos/loadcombos";
    }

    public static class Rrhh
    {
        public const string TiposZona         = $"{Prefix}/rrhh/tipos-zona";
        public const string TiposZonaCombobox = $"{TiposZona}/combobox";
        public const string TiposZonaSearch   = $"{TiposZona}/search";
    }

    public static class Diagnostico
    {
        public const string Accion = $"{Prefix}/diagnostico/accion";
    }

    public static class Finanzas
    {
        public const string Bancos = $"{Prefix}/finanzas/bancos";
        public const string BancosCombobox = $"{Prefix}/finanzas/bancos/combobox";
        public const string BancosCuentasBancarias = $"{Prefix}/finanzas/bancos-cuentas-bancarias";
        public const string EntidadesCuentasBancarias = $"{Prefix}/finanzas/entidades-cuentas-bancarias";
        public const string TiposCuentasBancarias = $"{Prefix}/finanzas/tipos-cuentas-bancarias";
        public const string TiposCuentasBancariasCombobox = $"{Prefix}/finanzas/tipos-cuentas-bancarias/combobox";

        // XML recibidos de los proveedores: los comprobantes que bajó el servicio del correo y el servicio mismo.
        public const string CpeRecibidos = $"{Prefix}/finanzas/cpe-recibidos";
        public static string CpeRecibido(decimal numCpe, string? parte = null) => $"{CpeRecibidos}/{numCpe:0}" + (parte is null ? "" : "/" + parte);
        public static string CpeRecibidoTipoOperacion(decimal numCpe, string codClaseOperacion) => $"{CpeRecibido(numCpe, "tipo-operacion")}?codClaseOperacion={Uri.EscapeDataString(codClaseOperacion)}";
        public const string XmlRecibidosEstado = $"{Prefix}/finanzas/xml-recibidos/estado";
        public const string XmlRecibidosEjecutar = $"{Prefix}/finanzas/xml-recibidos/ejecutar";
    }

    public static class Planeamiento
    {
        public static string OrdenProduccionComboboxPorProceso(string codProceso) =>
            $"{Prefix}/planeamiento/orden-produccion/combobox/{Uri.EscapeDataString(codProceso)}";
        public static string OrdenProduccionItemsComboboxPorOrden(string codProceso, int numOrdenProduccion) =>
            $"{Prefix}/planeamiento/orden-produccion-items/combobox/{Uri.EscapeDataString(codProceso)}/{numOrdenProduccion}";
        public static string OrdenProduccionUnidadComboboxPorItem(string codProceso, int numOrdenProduccion, int numOrdenProduccionItem) =>
            $"{Prefix}/planeamiento/orden-produccion-unidad/combobox/{Uri.EscapeDataString(codProceso)}/{numOrdenProduccion}/{numOrdenProduccionItem}";
    }

    public static class Proyectos
    {
        public const string ProyectosDoc = $"{Prefix}/proyectos/proyectos";
        public const string ProyectosDocCombobox = $"{Prefix}/proyectos/proyectos/combobox";
        public const string TiposProyecto = $"{Prefix}/proyectos/tipos-proyecto";
        public const string TiposProyectoCombobox = $"{Prefix}/proyectos/tipos-proyecto/combobox";

        // Catálogos
        public const string Roles = $"{Prefix}/proyectos/proyectos-roles";
        public const string RolesCategorias = $"{Prefix}/proyectos/proyectos-roles-categorias";
        public const string TiposDocumentoCompromiso = $"{Prefix}/proyectos/tipos-documento-compromiso";
        public const string PlantillasDocumento = $"{Prefix}/proyectos/plantillas-documento";
        public const string Feriados = $"{Prefix}/estandar/feriados";
        public const string Parametros = $"{Prefix}/proyectos/parametros";
        public const string UnidadesMedida = $"{Prefix}/proyectos/proyectos-unidades-medida-tareas";
        public const string ProbabilidadRiesgo = $"{Prefix}/proyectos/proyectos-probabilidad-riesgo";
        public const string ImpactoRiesgo = $"{Prefix}/proyectos/proyectos-impacto-riesgo";

        // Tablas del expediente (alta, modificación y baja; las lecturas por proyecto van por Expediente)
        public const string Fases = $"{Prefix}/proyectos/proyectos-fases";
        public const string Tareas = $"{Prefix}/proyectos/proyectos-tareas";
        public const string SubTareas = $"{Prefix}/proyectos/proyectos-sub-tareas";
        public const string Hitos = $"{Prefix}/proyectos/proyectos-hitos";
        public const string RecursoHumano = $"{Prefix}/proyectos/proyectos-recurso-humano-asignado";
        public const string Gastos = $"{Prefix}/proyectos/proyectos-gastos";
        public const string Riesgos = $"{Prefix}/proyectos/proyectos-riesgos";
        public const string Planes = $"{Prefix}/proyectos/proyectos-planes-mitigacion-riesgos";
        public const string Memoria = $"{Prefix}/proyectos/proyectos-memoria";
        public const string Materiales = $"{Prefix}/proyectos/proyectos-materiales";
        public const string GastosAdicionales = $"{Prefix}/proyectos/proyectos-gastos-adicionales";
        public const string TiposPropios = $"{Prefix}/proyectos/parametros/tipos-propios";

        // Expediente: listados por proyecto y acciones (Gantt, avance, confirmación, gastos, archivos)
        public const string Expediente = $"{Prefix}/proyectos/expediente";
        public static string Exp(int num, string parte) => $"{Expediente}/{num}/{parte}";
        public const string Portafolio = $"{Expediente}/portafolio";
        public const string MisTareas = $"{Expediente}/mis-tareas";
    }

    // Módulo RFID: emisión de códigos de etiquetas (TID + correlativo), equipos y consulta de etiquetas.
    public static class Rfid
    {
        public const string Equipos        = $"{Prefix}/rfid/equipos";
        public const string EquiposCombo   = $"{Equipos}/combo";

        // Emisiones: POST crea (y POST {id}/enrolar enrola un TID), PUT modifica, PUT {id}/anular anula, DELETE borra una sin etiquetas enroladas.
        public const string Emisiones      = $"{Prefix}/rfid/emisiones";
        public const string EmisionesSearch = $"{Emisiones}/search";
        public static string EmisionDetalle(int numEmision)   => $"{Emisiones}/{numEmision}/detalle";
        public static string EmisionEtiquetas(int numEmision) => $"{Emisiones}/{numEmision}/etiquetas";
        public static string EmisionAnular(int numEmision)    => $"{Emisiones}/{numEmision}/anular";
        public static string EmisionEnrolar(int numEmision)   => $"{Emisiones}/{numEmision}/enrolar";
        public static string EtiquetaAnular(int numEmision, int numItem, int numEtiqueta) => $"{Emisiones}/{numEmision}/etiquetas/{numItem}/{numEtiqueta}/anular";

        // De dónde salen las etiquetas: ingresos por compra (con sus líneas) y filas de stock.
        public const string OrigenCompras  = $"{Emisiones}/origen/compras";
        public static string OrigenComprasLineas(decimal numMovimientoAlmacen) => $"{OrigenCompras}/{numMovimientoAlmacen.ToString(System.Globalization.CultureInfo.InvariantCulture)}/lineas";
        public const string OrigenStock    = $"{Emisiones}/origen/stock";

        // Impresión: la web pide imprimir (POST) y el servicio del agente en la planta toma los trabajos (la cola RFID.TRABAJOS_IMPRESION).
        public const string Impresion        = $"{Prefix}/rfid/impresion";
        public const string ImpresionActivos = $"{Impresion}?FLG_SOLO_ACTIVOS=true";
        public static string ImpresionDeEmision(int numEmision) => $"{Impresion}?NUM_EMISION={numEmision}";
        public static string ImpresionCancelar(int numTrabajo)  => $"{Impresion}/{numTrabajo}/cancelar";

        public const string Etiquetas        = $"{Prefix}/rfid/etiquetas";
        public const string EtiquetasSearch  = $"{Etiquetas}/search";
        public const string EtiquetasGrabada = $"{Etiquetas}/grabada";
    }

    // Reporteador: generar desde los botones, y las pantallas de fuentes, diseños y asignación a botones.
    public static class Reportes
    {
        public const string Base = $"{Prefix}/reportes";
        private static string E(string? s) => Uri.EscapeDataString(s ?? "");

        // Ejecución por botón (todas las pantallas, a través de AccionesBotones)
        public static string OpcionBotones(string codAplicacion, string codOpcion, string? codRol) =>
            $"{Base}/opcion-botones?COD_APLICACION={E(codAplicacion)}&COD_OPCION_APLICACION={E(codOpcion)}&COD_ROL_USUARIO={E(codRol)}";
        public static string Dialogo(string codAplicacion, string codOpcion, string codBoton, string? codRol) =>
            $"{Base}/dialogo?COD_APLICACION={E(codAplicacion)}&COD_OPCION_APLICACION={E(codOpcion)}&COD_BOTON_OPCION={E(codBoton)}&COD_ROL_USUARIO={E(codRol)}";
        public const string Combo   = $"{Base}/combo";
        public const string Generar = $"{Base}/generar";

        // Fuentes
        public const string Fuentes         = $"{Base}/fuentes";
        public static string Fuente(int num) => $"{Fuentes}/{num}";
        public static string Procedimientos(string? buscar) => $"{Fuentes}/procedimientos?buscar={E(buscar)}";
        public static string ParametrosSp(string? esquema, string procedimiento) =>
            $"{Fuentes}/parametros-sp?esquema={E(esquema)}&procedimiento={E(procedimiento)}";
        public const string FuentesEtiquetas = $"{Fuentes}/etiquetas";
        public const string FuentesProbar    = $"{Fuentes}/probar";

        // Diseños
        public const string Disenos = $"{Base}/disenos";
        public static string Diseno(int num) => $"{Disenos}/{num}";
        public const string DisenosFuentes = $"{Disenos}/fuentes";
        public static string DisenosFuente(int num) => $"{Disenos}/fuentes/{num}";
        public static string PlantillaBase(int num, string formato) => $"{Disenos}/{num}/plantilla-base/{E(formato)}";
        public static string Plantilla(int num, string formato) => $"{Disenos}/{num}/plantilla/{E(formato)}";
        public static string DisenoDialogo(int num) => $"{Disenos}/{num}/dialogo";
        public static string DisenoCombo(int num) => $"{Disenos}/{num}/combo";
        public static string DisenoGenerar(int num) => $"{Disenos}/{num}/generar";
        public static string DisenoConvertir(int num) => $"{Disenos}/{num}/convertir";
        public const string FormulaComprobar = $"{Disenos}/formulas/comprobar";
        public const string FormulaFunciones = $"{Disenos}/formulas/funciones";

        // Asignación a botones
        public const string Botones = $"{Base}/botones";
        public static string BotonesListar(string? codAplicacion, string? buscar, bool soloAsignados) =>
            $"{Botones}?COD_APLICACION={E(codAplicacion)}&buscar={E(buscar)}&soloAsignados={(soloAsignados ? "true" : "false")}";
        public static string BotonDetalle(string codAplicacion, string codOpcion, string codBoton, int? numDiseno) =>
            $"{Botones}/detalle?COD_APLICACION={E(codAplicacion)}&COD_OPCION_APLICACION={E(codOpcion)}&COD_BOTON_OPCION={E(codBoton)}"
            + (numDiseno is { } n ? $"&NUM_DISENO={n}" : "");
        public static string BotonQuitar(string codAplicacion, string codOpcion, string codBoton) =>
            $"{Botones}?COD_APLICACION={E(codAplicacion)}&COD_OPCION_APLICACION={E(codOpcion)}&COD_BOTON_OPCION={E(codBoton)}";
        public const string BotonesDisenos = $"{Botones}/disenos";
        public const string BotonesRutas   = $"{Botones}/rutas";
    }
}
