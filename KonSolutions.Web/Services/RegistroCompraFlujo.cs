using System.Net.Http.Json;
using KONSolutions.Shared.Common;
using KONSolutions.Shared.Models.Comercial;
using KONSolutions.Web.Pages.Comercial;
using MudBlazor;

namespace KONSolutions.Web.Services;

/// <summary>
/// Las acciones del flujo de un comprobante del Registro de Compras: validar, enviar a aprobación, aprobar, contabilizar y anular.
/// Las usan la lista (botones de la fila) y el documento (botones del pie): con una sola copia las dos pantallas hacen lo mismo y piden lo mismo.
/// Cada botón del menú se reconoce por su DES_METODO (lo deja el script 40). Qué botón corresponde a qué estado lo dice <see cref="Motivo"/>; la base vuelve a exigir cada regla.
/// </summary>
public sealed class RegistroCompraFlujo
{
    private readonly HttpClient _http;
    private readonly ISnackbar _snackbar;
    private readonly ErrorLogService _errorLog;
    private readonly IDialogService _dialogs;
    private readonly string _pagina;

    public RegistroCompraFlujo(HttpClient http, ISnackbar snackbar, ErrorLogService errorLog, IDialogService dialogs, string pagina)
    { _http = http; _snackbar = snackbar; _errorLog = errorLog; _dialogs = dialogs; _pagina = pagina; }

    public const string Validar = "Validar", EnviarAprobacion = "EnviarAprobacion", Aprobar = "Aprobar", Contabilizar = "Contabilizar", Anular = "Anular", ImportarXml = "ImportarXml", TraerXmlRecibido = "TraerXmlRecibido";

    /// <summary>¿El botón es del flujo (de los que actúan sobre un comprobante)?</summary>
    public static bool EsDeFlujo(string? metodo) => (metodo ?? "").Trim() is Validar or EnviarAprobacion or Aprobar or Contabilizar or Anular;

    /// <summary>Por qué el botón no se puede usar con el comprobante en ese estado; null = se puede (o no es un botón de flujo).</summary>
    public static string? Motivo(string? metodo, string? estado)
    {
        var e = (estado ?? "REG").Trim().ToUpperInvariant();
        return (metodo ?? "").Trim() switch
        {
            Validar => RegistroCompraEstados.PuedeValidar(e) ? null : $"Un comprobante {RegistroCompraEstados.Texto(e).ToLowerInvariant()} ya no se valida.",
            EnviarAprobacion => RegistroCompraEstados.PuedeEnviarAprobacion(e) ? null : "Solo se envía a aprobación un comprobante observado.",
            Aprobar => RegistroCompraEstados.PuedeAprobar(e) ? null : "Solo se aprueba un comprobante validado, bloqueado o sin orden.",
            Contabilizar => RegistroCompraEstados.PuedeContabilizar(e) ? null : "Solo se contabiliza un comprobante aprobado.",
            Anular => RegistroCompraEstados.PuedeAnular(e) ? null : "El comprobante ya está anulado.",
            _ => null
        };
    }

    /// <summary>Ejecuta la acción sobre el comprobante. Devuelve verdadero si algo cambió (el llamador recarga).</summary>
    /// <param name="justificacionActual">Lo que ya estaba escrito en el comprobante (por ejemplo, con lo que se envió a aprobación), para ofrecerlo al aprobar.</param>
    public async Task<bool> EjecutarAsync(string? metodo, decimal numRegistro, string? estado, string? justificacionActual)
    {
        var m = (metodo ?? "").Trim();
        if (Motivo(m, estado) is { } motivo)
        {
            await ErrorDialogHelper.MostrarAvisoAsync(motivo, _dialogs, _pagina);
            return false;
        }
        try
        {
            switch (m)
            {
                case Validar: return await ValidarAsync(numRegistro);
                case Contabilizar: return await ContabilizarAsync(numRegistro);
                case EnviarAprobacion:
                    return await PedirAsync(numRegistro, "enviar-aprobacion", "Enviar a aprobación", "El comprobante tiene observaciones. Explica por qué se puede aprobar igual.",
                        "Enviar", true, "Enviado a aprobación.", Color.Primary, justificacionActual);
                case Aprobar:
                    var exige = RegistroCompraEstados.AprobarExigeJustificacion(estado);
                    return await PedirAsync(numRegistro, "aprobar", "Aprobar comprobante",
                        exige ? "El comprobante está bloqueado o no tiene orden: la aprobación exige una justificación." : "Puedes dejar una nota con la aprobación (opcional).",
                        "Aprobar", exige, "Comprobante aprobado.", Color.Success, justificacionActual);
                case Anular:
                    return await PedirAsync(numRegistro, "anular", "Anular comprobante", "Anular deja el comprobante sin efecto. Escribe el motivo.",
                        "Anular", true, "Comprobante anulado.", Color.Error, null);
                default:
                    await ErrorDialogHelper.MostrarAvisoAsync($"El botón \"{m}\" todavía no tiene una acción conectada en esta pantalla.", _dialogs, _pagina, "Acción pendiente");
                    return false;
            }
        }
        catch (Exception ex)
        {
            await _errorLog.LogAsync(ex, _pagina, nameof(EjecutarAsync));
            await ErrorDialogHelper.MostrarAvisoAsync("No se pudo completar la acción. Ver detalle en el log de errores.", _dialogs, _pagina);
            return false;
        }
    }

    private string Url(decimal numRegistro, string accion) => $"{ApiRoutes.Comercial.RegistroCompras}/{numRegistro:0}/{accion}";

    private async Task<bool> ValidarAsync(decimal numRegistro)
    {
        var resp = await _http.PutAsync(Url(numRegistro, "validar"), null);
        if (!resp.IsSuccessStatusCode)
        {
            await ErrorDialogHelper.MostrarAsync(resp, _errorLog, _dialogs, _pagina, nameof(ValidarAsync), "No se pudo validar el comprobante");
            return false;
        }
        var r = await resp.Content.ReadFromJsonAsync<ApiResponse<RegistroCompraValidacion>>();
        var estado = r?.Data?.COD_ESTADO_ALIAS_CONCILIACION ?? "";
        _snackbar.Add($"Validado: {RegistroCompraEstados.Texto(estado)}.", estado == RegistroCompraEstados.Validada ? Severity.Success : Severity.Warning);
        return true;
    }

    private async Task<bool> ContabilizarAsync(decimal numRegistro)
    {
        var ok = await _dialogs.ShowMessageBox("Contabilizar comprobante",
            "Se deja el comprobante contabilizado y su deuda en el cronograma de pagos. No se genera el asiento contable. ¿Continuar?", yesText: "Contabilizar", cancelText: "Cancelar");
        if (ok != true) return false;
        var resp = await _http.PutAsync(Url(numRegistro, "contabilizar"), null);
        if (!resp.IsSuccessStatusCode)
        {
            await ErrorDialogHelper.MostrarAsync(resp, _errorLog, _dialogs, _pagina, nameof(ContabilizarAsync), "No se pudo contabilizar el comprobante");
            return false;
        }
        _snackbar.Add("Comprobante contabilizado.", Severity.Success);
        return true;
    }

    private async Task<bool> PedirAsync(decimal numRegistro, string accion, string titulo, string aviso, string confirmar, bool obligatoria, string exito, Color color, string? textoInicial)
    {
        var dlg = await _dialogs.ShowAsync<RegistroCompraJustificacionDialog>("",
            new DialogParameters
            {
                ["Url"] = Url(numRegistro, accion), ["Titulo"] = titulo, ["Aviso"] = aviso, ["Confirmar"] = confirmar,
                ["Obligatoria"] = obligatoria, ["ExitoMensaje"] = exito, ["ColorConfirmar"] = color, ["TextoInicial"] = textoInicial
            },
            new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true, CloseButton = false });
        var res = await dlg.Result;
        return res is { Canceled: false };
    }
}
