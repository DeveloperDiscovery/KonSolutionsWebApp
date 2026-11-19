using System.Reflection;
using Xunit;

// Los dialogos CRUD cuyas tablas tienen columnas de auditoria muestran el PanelAuditoria (referencia: UsuarioDialog / AlmacenesDialog).
// Cada uno debe: (1) usar <PanelAuditoria> en su razor y (2) tener en el modelo del Item las propiedades de auditoria que ese panel lee.
public class AuditoriaDialogosTests
{
    static readonly string[] Props = { "COD_USUARIO_REGISTRO", "COD_ESTACION_REGISTRO", "FEC_REGISTRO", "COD_USUARIO_ACTUALIZACION", "COD_ESTACION_ACTUALIZACION", "FEC_ACTUALIZACION" };

    public static IEnumerable<object[]> Dialogos() => new[]
    {
        "Contabilidad/AsientosContablesDialog", "Contabilidad/CuentasContablesDialog", "Estandar/PeriodosDialog", "Estandar/ZonificacionDialog",
        "Estandar/OpcionesProcesosEstadosFlujosDialog", "Estandar/SucursalTipoComprobanteDialog",
        "Logistica/GuiasRemisionSeriesDialog", "Logistica/InventariosDialog", "Logistica/InventariosAlmacenesDialog", "Logistica/InventariosAlmacenesPuntoTomaDialog",
        "Logistica/InventariosAlmacenesPuntoTomaConteoDialog", "Logistica/InventariosAlmacenesPuntoTomaEquiposAsignadosDialog", "Logistica/AlmacenesCierreDialog",
        "Proyectos/FeriadosDialog", "Proyectos/PlantillasDocumentoDialog", "Proyectos/ProyectosRolesCategoriasDialog", "Proyectos/TiposDocumentoCompromisoDialog",
        "Proyectos/ProyectoFaseDialog", "Proyectos/ProyectoTareaDialog", "Proyectos/TiposProyectoDialog", "Rfid/EmisionFichaDialog", "Ingenieria/OperacionDialog", "Ingenieria/OperacionFichaDialog", "Ingenieria/ArmadoDialog", "Ingenieria/PersonalOperacionDialog", "Logistica/RequerimientoDocumentoDialog", "Logistica/ReservaDocumentoDialog",
    }.Select(d => new object[] { d });

    static string Raiz()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "KonSolutions.Web", "Pages"))) dir = dir.Parent;
        Assert.NotNull(dir);
        return Path.Combine(dir!.FullName, "KonSolutions.Web", "Pages");
    }

    [Theory]
    [MemberData(nameof(Dialogos))]
    public void ElDialogoMuestraLaAuditoria_YSuModeloTieneLosCampos(string dialogo)
    {
        var src = File.ReadAllText(Path.Combine(Raiz(), dialogo + ".razor"));
        Assert.Matches(@"<PanelAuditoria\s", src);
        foreach (var campo in new[] { "COD_USUARIO_REGISTRO", "FEC_REGISTRO", "FEC_ACTUALIZACION" }) Assert.Matches(@"(Item\?\.|Operacion\.)" + campo, src);
        var nombre = dialogo.Split('/').Last();
        var tipo = typeof(KONSolutions.Web.Pages.Ingenieria.Operaciones).Assembly.GetTypes().Single(t => t.Name == nombre && typeof(Microsoft.AspNetCore.Components.IComponent).IsAssignableFrom(t));
        var item = (tipo.GetProperty("Item") ?? tipo.GetProperty("Operacion"))!.PropertyType;
        foreach (var p in Props) Assert.NotNull(item.GetProperty(p));
    }
}
