using Microsoft.JSInterop;

namespace KONSolutions.Web.Pages.Ingenieria;

/// <summary>Ajuste de alto del arbol / diagrama / ruta (elementos con data-fit) en las ventanas a pantalla completa:
/// terminan un poco antes del boton CERRAR de su propia ventana. El script JS va EMBEBIDO aqui y se inyecta solo la
/// primera vez, asi que no depende de copiar archivos a wwwroot ni de tocar index.html.</summary>
public static class EstructuraFit
{
    public static async Task AjustarAsync(IJSRuntime js)
    {
        try { await js.InvokeVoidAsync("estructuraFit.ajustar"); }
        catch (JSException)
        {
            await js.InvokeVoidAsync("eval", Script);            // primera vez: define window.estructuraFit y arranca el observador
            await js.InvokeVoidAsync("estructuraFit.ajustar");
        }
    }

    public const string Script = """
// Ventanas de Estructura y de Ruta de produccion (a pantalla completa o que cubren casi toda la pantalla): el arbol / diagrama marcados con [data-fit] llegan
// hasta un poco ANTES del borde superior del boton CERRAR (la barra .mud-dialog-actions de su propia ventana).
// Se mide contra ese boton y no contra la ventana ni con vh, asi no importa lo que haya arriba ni el zoom.
// El arbol lleva ademas data-default-height para que gridAutoAlto.js no lo estire hasta la barra de estado de la pagina de abajo.
// Se reaplica solo (al abrir, cambiar de pestana, ampliar o redimensionar): no depende de que Blazor lo llame.
window.estructuraFit = {
    HOLGURA: 14,   // px entre el pie del arbol y el boton CERRAR
    _pend: false,
    ajustar: function () {
        var f = window.estructuraFit;
        document.querySelectorAll('[data-fit]').forEach(function (e) {
            if (!e.offsetParent) return;                                   // pestana oculta
            var dlg = e.closest('.mud-dialog');
            if (!dlg) return;
            // ventana "grande": pantalla completa de MudBlazor, o cualquier ventana que cubra casi toda la pantalla
            // (la aplicacion las agranda por su cuenta sin usar la opcion FullScreen). Las ventanas pequenas no se tocan.
            var r = dlg.getBoundingClientRect();
            var grande = dlg.classList.contains('mud-dialog-fullscreen') || (r.width >= window.innerWidth * 0.92 && r.height >= window.innerHeight * 0.85);
            if (!grande) return;
            // ZOOM PROPIO DE LA APP: es transform:scale sobre #app y las ventanas viven dentro, asi que 1 px CSS = "esc" px de pantalla.
            // Todo se mide en px de pantalla (getBoundingClientRect) y al asignar el alto se divide entre esc.
            var esc = dlg.offsetWidth ? r.width / dlg.offsetWidth : 1;
            if (!(esc > 0.2 && esc < 5)) esc = 1;
            var acc = dlg.querySelector('.mud-dialog-actions');
            var ar = acc ? acc.getBoundingClientRect() : null;
            var tope = ar ? Math.min(ar.top, window.innerHeight - ar.height - 2) : window.innerHeight - 40;   // nunca pasa del borde de la pantalla
            var real = tope - e.getBoundingClientRect().top - f.HOLGURA;                                        // px de pantalla
            var h = Math.max(200, Math.floor(real / esc)) + 'px';                                               // px CSS
            if (e.style.maxHeight !== h) e.style.setProperty('max-height', h, 'important');
            if (e.hasAttribute('data-default-height') && e.style.height !== h) e.style.setProperty('height', h, 'important');
        });
    },
    pedir: function () {
        var f = window.estructuraFit;
        if (f._pend) return;
        f._pend = true;
        requestAnimationFrame(function () { f._pend = false; f.ajustar(); });
    },
    iniciar: function () {
        var f = window.estructuraFit;
        if (f._on) return;
        f._on = true;
        window.addEventListener('resize', f.pedir);
        window.addEventListener('kon:zoom', f.pedir);                     // el zoom propio de la app no dispara 'resize'
        new MutationObserver(function (muts) {
            if (!document.querySelector('[data-fit]')) return;
            for (var i = 0; i < muts.length; i++) {
                var t = muts[i].target;
                if (!(t.nodeType === 1 && t.hasAttribute('data-fit'))) { f.pedir(); return; }   // los cambios del propio arbol no se reprocesan
            }
        }).observe(document.body, { childList: true, subtree: true, attributes: true, attributeFilter: ['style', 'class'] });
        f.pedir();
    }
};
if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', function () { window.estructuraFit.iniciar(); });
else window.estructuraFit.iniciar();

""";
}
