// Alto automático de TODAS las grillas: cada [data-resizable-grid] se estira hasta el borde
// superior de la barra de estado.
//
// Por qué se mide con JS y no con flexbox: hay ~156 grillas repartidas en ~126 pantallas, con
// anidamientos muy distintos (dentro de MudPaper, de MudToolBar, de divs propios, de tabs
// internos, en paneles maestro/detalle...). Un selector CSS que dependa de la estructura
// cubriría unas y dejaría afuera otras. Medir la posición real de la grilla contra la de la
// barra de estado es independiente del anidamiento, así que sirve igual para todas.
//
// Quedan afuera dos casos, a propósito:
//   · [data-default-height]  → alto fijo declarado por la pantalla (maestro de un
//                              maestro/detalle: ahí el que se estira es el detalle).
//   · [data-alto-manual]     → el usuario arrastró el borde; su preferencia manda
//                              (doble clic en el borde vuelve al automático).
window.gridAutoAlto = {
    // Margen de respiro entre el pie de la grilla y la barra de estado.
    MARGEN: 8,
    ALTO_MIN: 150,

    _pendiente: null,

    seleccionar: function () {
        return document.querySelectorAll(
            '[data-resizable-grid]:not([data-default-height]):not([data-alto-manual])' +
            ':not([data-alto-compartido])');
    },

    // ── Grillas que COMPARTEN el espacio disponible ──────────────────────────────────
    //
    // El reparto normal le da a CADA grilla todo lo que hay hasta la barra de estado. Eso
    // vale cuando hay una sola; con dos apiladas en una columna —órdenes de compra: la
    // cabecera arriba y el detalle abajo— las dos pedirían el mismo espacio y la de abajo
    // terminaría fuera de la pantalla.
    //
    // Las que llevan data-alto-compartido="<grupo>" se reparten en partes iguales lo que
    // queda, descontando lo que NO es grilla y está entre medio (los títulos, las
    // separaciones). Ese "entre medio" no se estima: se mide con las posiciones actuales
    // —lo que hay entre el pie de una y el tope de la siguiente— así que sirve igual si
    // mañana se agrega un botón o se cambia el tamaño de la letra.
    //
    // Depende del alto real de la ventana y de la escala del zoom, igual que el reparto
    // normal, así que acompaña solo al cambiar cualquiera de los dos.
    repartirCompartidos: function (tope, escala) {
        var self = this;
        var grupos = {};

        // OJO: se toman TODAS, incluidas las que el usuario fijó a mano. Las manuales no se
        // tocan, pero sí ocupan lugar: si se las ignorara, las automáticas repartirían un
        // espacio que ya no está libre y la última quedaría fuera de la pantalla.
        document.querySelectorAll('[data-alto-compartido]').forEach(function (g) {
            if (g.offsetParent === null) return;   // en una pestaña oculta no hay layout real
            var k = g.getAttribute('data-alto-compartido');
            (grupos[k] = grupos[k] || []).push(g);
        });

        Object.keys(grupos).forEach(function (k) {
            var gs = grupos[k];
            // En el orden en que están en la pantalla, no en el del DOM.
            gs.sort(function (a, b) { return a.getBoundingClientRect().top - b.getBoundingClientRect().top; });

            var primero = gs[0].getBoundingClientRect();
            var ultimo  = gs[gs.length - 1].getBoundingClientRect();

            // Todo lo que hay entre las grillas y no es grilla: títulos, márgenes, botones.
            var altoGrillas = gs.reduce(function (acc, g) { return acc + g.getBoundingClientRect().height; }, 0);
            var entreMedio  = (ultimo.bottom - primero.top) - altoGrillas;

            var arriba = primero.top + self.scrollDeAncestro(gs[0]);
            var disponible = tope - arriba - self.MARGEN - entreMedio;

            // Lo que el usuario fijó a mano se respeta y se DESCUENTA; el resto se reparte
            // entre las automáticas. Así, subir la cabecera a mano agranda el detalle en vez
            // de dejar un hueco, y nadie se va fuera de la pantalla.
            var auto = [], usadoManual = 0;
            gs.forEach(function (g) {
                if (g.hasAttribute('data-alto-manual')) usadoManual += g.getBoundingClientRect().height;
                else auto.push(g);
            });
            if (auto.length === 0) return;

            var cada = Math.max(self.ALTO_MIN, ((disponible - usadoManual) / auto.length) / escala);

            var nuevo = cada.toFixed(1) + 'px';
            auto.forEach(function (g) { if (g.style.height !== nuevo) g.style.height = nuevo; });
        });
    },

    // Grillas con alto manual: se recuerda dónde empezaban, para saber si el layout se movió.
    _topManual: new WeakMap(),

    // Un alto manual vale mientras la pantalla siga siendo la misma. Si la grilla CAMBIA DE
    // LUGAR —se ocultan o se muestran los filtros, se achica la ventana, se pliega el menú—
    // lo que había debajo ya no es lo mismo, y conservar un alto elegido para el layout
    // anterior deja el hueco en blanco que se veía al ocultar los filtros.
    //
    // Se mide el BORDE SUPERIOR y no el alto: arrastrar el tirador cambia el alto pero NO
    // mueve el tope, así que la preferencia del usuario sobrevive a su propio arrastre y a
    // los re-render de Blazor; sólo la suelta un cambio real de la pantalla.
    //
    // El umbral de 8px evita que un corrimiento de un píxel —un borde, una fuente que
    // termina de cargar— cuente como cambio de layout.
    UMBRAL_TOP: 8,

    // Cuánto está scrolleado el contenedor que realmente scrollea esta grilla. En esta
    // aplicación suele ser .ws-tab-content (el cuerpo de la pestaña del workspace), no la
    // ventana; por eso se busca el ancestro en vez de mirar window.scrollY.
    scrollDeAncestro: function (el) {
        for (var p = el.parentElement; p; p = p.parentElement) {
            if (p.scrollHeight > p.clientHeight) {
                var ov = getComputedStyle(p).overflowY;
                if (ov === 'auto' || ov === 'scroll') return p.scrollTop;
            }
        }
        return (document.scrollingElement && document.scrollingElement.scrollTop) || 0;
    },

    liberarSiSeMovio: function () {
        var self = this;
        document.querySelectorAll('[data-resizable-grid][data-alto-manual]').forEach(function (g) {
            if (g.offsetParent === null) return;
            // Con el mismo criterio que el cálculo del alto: el tope SIN el scroll. Si no,
            // scrollear movería el tope y le soltaría el alto manual al usuario.
            var top = g.getBoundingClientRect().top + self.scrollDeAncestro(g);
            var previo = self._topManual.get(g);
            if (previo === undefined) { self._topManual.set(g, top); return; }
            if (Math.abs(top - previo) <= self.UMBRAL_TOP) return;

            delete g.dataset.altoManual;
            g.style.height = '';
            g.querySelectorAll('.mud-table-container').forEach(function (c) { c.style.height = ''; });
            self._topManual.delete(g);
            // También se olvida lo guardado: si no, al volver a entrar a la pantalla
            // reaparecería el alto viejo y estaríamos igual.
            var grupo = g.getAttribute('data-resizable-grid');
            if (grupo) { try { localStorage.removeItem('kon.grid.cols.alto.' + grupo); } catch (e) { } }
        });
    },

    // Coalescido a un frame: resize y ResizeObserver disparan en ráfaga.
    pedirRecalculo: function () {
        var self = this;
        if (this._pendiente) return;
        this._pendiente = requestAnimationFrame(function () {
            self._pendiente = null;
            try { self.recalcular(); } catch (e) { /* nunca tumbar la UI por el layout */ }
        });
    },

    recalcular: function () {
        var barra = document.querySelector('.status-bar');
        // Si todavía no existe la barra (arranque), se usa el borde inferior de la ventana.
        var tope = barra ? barra.getBoundingClientRect().top : window.innerHeight;

        // La escala del zoom propio de la app (transform:scale). getBoundingClientRect ya
        // devuelve px reales (post-transform), pero el valor que se asigna a style.height
        // vuelve a escalarse, así que hay que dividirlo para compensar.
        var escala = (window.appZoom && window.appZoom.currentScale) || 1;
        if (!escala || escala <= 0) escala = 1;

        var self = this;

        // Antes de repartir el alto automático: soltar las que quedaron fijadas para un
        // layout que ya no existe.
        this.liberarSiSeMovio();

        // Primero las que se reparten el espacio entre varias, después las que se lo
        // llevan entero: son dos criterios distintos y no se pisan.
        this.repartirCompartidos(tope, escala);

        this.seleccionar().forEach(function (grilla) {
            // Contenedor oculto (pestaña en segundo plano, menú circular girando): todavía no
            // hay layout real y getBoundingClientRect daría ceros. Se salta; al mostrarse, el
            // ResizeObserver vuelve a disparar.
            if (grilla.offsetParent === null) return;

            // EL TOPE, MEDIDO COMO SI LA PÁGINA NO ESTUVIERA SCROLLEADA.
            //
            // getBoundingClientRect().top baja y sube con el scroll. En una pantalla cuyo
            // contenido entra en el alto de la ventana eso no se nota, porque no hay scroll.
            // Pero si la página scrollea —Solicitudes Comerciales, que tiene un bloque de
            // cabecera alto— se arma un círculo: al scrollear, la grilla sube; al subir,
            // "sobra" más espacio hasta la barra de estado; se le da más alto; con más alto
            // la página es más larga y se puede scrollear más. La grilla crece un poco en
            // cada rueda del mouse, que es exactamente el síntoma.
            //
            // Sumarle el scroll del contenedor devuelve la posición que la grilla tendría
            // sin scrollear, que es la única estable. Si no hay contenedor con scroll, el
            // valor es 0 y la cuenta queda igual que antes.
            var arriba = grilla.getBoundingClientRect().top + self.scrollDeAncestro(grilla);
            var disponible = tope - arriba - self.MARGEN;
            var alto = Math.max(self.ALTO_MIN, disponible / escala);

            var nuevo = alto.toFixed(1) + 'px';
            // Solo se escribe si cambió: asignar el mismo valor volvería a disparar el
            // ResizeObserver y entraríamos en un ciclo de recálculos.
            if (grilla.style.height !== nuevo) grilla.style.height = nuevo;
        });
    },

    iniciar: function () {
        var pedir = this.pedirRecalculo.bind(this);

        // Zoom del navegador y cambios de tamaño de ventana.
        window.addEventListener('resize', pedir);
        // El zoom propio de la app usa transform:scale y NO dispara "resize" — appZoom avisa
        // por este evento.
        window.addEventListener('kon:zoom', pedir);
        // Al volver de una pestaña del navegador en segundo plano.
        document.addEventListener('visibilitychange', pedir);

        // Cambios de layout que desplazan la grilla sin tocar el tamaño de la ventana:
        // encabezados que pasan a dos líneas, cambio de pestaña, filtros que aparecen.
        if (window.ResizeObserver) {
            try { new ResizeObserver(pedir).observe(document.body); } catch (e) { }
        }
        // Grillas que se montan después (pestañas nuevas, diálogos, paneles maestro/detalle).
        if (window.MutationObserver) {
            try {
                new MutationObserver(pedir).observe(document.body, { childList: true, subtree: true });
            } catch (e) { }
        }

        pedir();
    }
};

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', function () { window.gridAutoAlto.iniciar(); });
} else {
    window.gridAutoAlto.iniciar();
}
