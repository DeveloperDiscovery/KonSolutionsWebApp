// Ventanas de Estructura y de Ruta de produccion (pantalla completa): el arbol / diagrama marcados con [data-fit] llegan
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
            if (!dlg || !dlg.classList.contains('mud-dialog-fullscreen')) return;   // solo en pantalla completa
            var acc = dlg.querySelector('.mud-dialog-actions');
            var tope = acc ? acc.getBoundingClientRect().top : window.innerHeight;
            var h = Math.max(220, Math.floor(tope - e.getBoundingClientRect().top - f.HOLGURA)) + 'px';
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
