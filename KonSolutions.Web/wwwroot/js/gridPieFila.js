// Pie de grilla: fila actual + textos en español.
//
// Dos cosas, en TODAS las grillas de la app a la vez:
//
//   1. Al hacer clic en una fila, el pie muestra "Fila 8 de 17".
//   2. El pie deja de estar en inglés: "Rows per page:" → "Filas por página:",
//      "1-17 of 17" → "Filas 1-17 de 17".
//
// POR QUÉ ACÁ Y NO EN CADA PANTALLA
// ---------------------------------
// Hay 163 grillas en 133 archivos. Hacerlo por pantalla significa 163 ediciones, y
// además el número de fila no sale de un parámetro: hay que saber en cuál se hizo
// clic, y muchas grillas YA tienen su propio RowClick con su propia lógica (abren un
// diálogo, cargan un detalle). Pisarlos o encadenarlos uno por uno es donde aparecen
// los errores.
//
// Delegando el clic en el documento no se toca ningún RowClick existente: el evento
// sigue su curso normal y esto solo lo observa al pasar. Y las grillas que se agreguen
// mañana quedan cubiertas sin hacer nada.
//
// Es el mismo criterio de gridAutoAlto.js, gridColumnas.js y filtroExcelColumna.js:
// lo que aplica a todas las grillas por igual vive en un solo archivo.
// GUARDIA DE DOBLE CARGA. Este archivo se carga por DOS caminos —el <script> de
// index.html y el import de MainLayout— porque index.html es el archivo que más se
// cachea en Blazor WASM y no siempre llega actualizado. Si ya está cargado, el
// segundo camino no hace nada: sin esta guardia se registraría el listener de clic
// dos veces y el observer también.
if (!window.gridPieFila) {

window.gridPieFila = {

    // Lo que MudBlazor escribe en inglés y su reemplazo. Es el texto COMPLETO del
    // nodo, no una búsqueda parcial: así no se toca nada que apenas se le parezca.
    TEXTOS: {
        'Rows per page:': 'Filas por página:',
        'Rows per page': 'Filas por página'
    },

    // "1-17 of 17" y también "0-0 of 0". MudBlazor a veces separa con espacio duro
    // ( ), por eso el \s en el patrón en vez de un espacio literal.
    RE_INFO: /^(\d+)\s*-\s*(\d+)\s+of\s+(\d+)$/,

    // Última fila clickeada por grilla. WeakMap y no un atributo en el DOM porque
    // MudBlazor rehace el pie al paginar, ordenar o filtrar, y un atributo se perdería;
    // con esto se puede volver a dibujar el indicador después de cada re-render.
    _filaPorGrilla: new WeakMap(),

    _observer: null,
    _iniciado: false,

    // ── Utilidades ──────────────────────────────────────────────────────────

    // El div más exterior de la grilla. Es la unidad: una página puede tener varias
    // (maestro/detalle), y cada una lleva su propio pie.
    grillaDe: function (el) {
        return el ? el.closest('.mud-table') : null;
    },

    pieDe: function (grilla) {
        return grilla ? grilla.querySelector('.mud-table-pagination') : null;
    },

    // Solo las filas de datos. Se excluye el encabezado (vive en thead) y cualquier
    // fila que no tenga celdas de dato: las de agrupación traen th, no td.
    filasDe: function (grilla) {
        if (!grilla) return [];
        var cuerpo = grilla.querySelector('tbody');
        if (!cuerpo) return [];
        return Array.prototype.filter.call(
            cuerpo.querySelectorAll('tr'),
            function (tr) { return tr.querySelector('td') !== null; });
    },

    // ── 1 · Textos en español ───────────────────────────────────────────────

    traducirPie: function (pie) {
        if (!pie) return;
        var self = this;
        var leyendas = pie.querySelectorAll('.mud-table-pagination-caption, p, span, label');

        Array.prototype.forEach.call(leyendas, function (nodo) {
            // Solo nodos de texto simple: si tiene hijos con más marcado, no es una
            // leyenda.
            if (nodo.children.length > 0) return;

            var txt = (nodo.textContent || '').replace(/\u00A0/g, ' ').trim();
            if (!txt) return;

            var traducido = null;
            if (self.TEXTOS[txt] !== undefined) {
                traducido = self.TEXTOS[txt];
            } else {
                var m = txt.match(self.RE_INFO);
                if (m) traducido = 'Filas ' + m[1] + '-' + m[2] + ' de ' + m[3];
            }
            if (traducido === null) return;

            // NO se escribe nodo.textContent. Blazor es dueño de ese texto: si se lo
            // pisamos, en el siguiente render compara contra lo que el cree haber escrito,
            // y ahí aparecen los sintomas raros —la leyenda quedandose en "0-0 de 0"
            // aunque haya 17 filas—.
            //
            // En vez de eso se guarda la traduccion en un atributo, que Blazor no gestiona
            // y por lo tanto no le molesta. app.css esconde el texto original y muestra el
            // del atributo con ::after. El DOM de Blazor queda intacto.
            if (nodo.getAttribute('data-es') !== traducido) nodo.setAttribute('data-es', traducido);
        });
    },

    // ── 2 · Indicador de fila ───────────────────────────────────────────────

    // El indicador NO es un elemento: es un atributo data-fila en el pie, que app.css
    // dibuja con ::before y attr(). Antes se insertaba un <span> dentro del pie y eso
    // estaba mal: el pie lo renderiza Blazor, y meterle un hijo ajeno interfiere con su
    // diffing —Blazor compara el DOM que el cree que dibujo contra el real—. Se veia
    // como una leyenda de paginacion equivocada ("Filas 0-0 de 0") y como el indicador
    // cayendo a un renglon aparte.
    //
    // Un atributo data-* que Blazor no conoce no participa del diff, asi que no hay
    // conflicto posible, y un pseudo-elemento no es un nodo: no puede desacomodar nada.
    // La barra interna del pie: es la que realmente es flex. .mud-table-pagination es
    // solo el contenedor que la envuelve, y colgar el indicador de el lo dejaba en un
    // renglon aparte, arriba de todo.
    //
    // Se ubica por el padre de una leyenda en vez de por nombre de clase: el nombre de
    // esa barra cambia entre versiones de MudBlazor, pero que la leyenda viva adentro de
    // la fila flex no cambia.
    barraDe: function (pie) {
        if (!pie) return null;
        var leyenda = pie.querySelector('.mud-table-pagination-caption');
        return (leyenda && leyenda.parentElement) ? leyenda.parentElement : pie;
    },

    ponerIndicador: function (pieOriginal, texto) {
        var pie = this.barraDe(pieOriginal);
        if (!pie) return;
        if (texto) {
            if (pie.getAttribute('data-fila') !== texto) pie.setAttribute('data-fila', texto);
        } else if (pie.hasAttribute('data-fila')) {
            pie.removeAttribute('data-fila');
        }
    },

    // La fila con foco lleva una clase; el puntero de la izquierda y el resaltado los
    // pone app.css a partir de ella. No se agrega ninguna columna a la tabla: hacerlo
    // obligaria a que las 163 grillas declararan una columna mas, y cualquier colspan
    // existente quedaria corrido. El puntero se dibuja dentro de la primera celda.
    CLASE_FOCO: 'kon-fila-foco',

    // NO TOCAR EL DOM CUANDO NO HAY NADA QUE CAMBIAR.
    //
    // La version anterior quitaba la clase de todas las filas y se la volvia a poner a
    // la misma, SIEMPRE. Aunque el resultado fuera identico, eso son dos mutaciones, y
    // el MutationObserver de mas abajo observa todo el body: cada pasada se despertaba
    // a si misma y a los otros cinco observadores de la app (gridColumnas, gridRowSize,
    // autoFocus, inputPlaceholder, gridAutoAlto).
    //
    // El resultado era un ciclo permanente a 60 cuadros por segundo mientras hubiera una
    // fila marcada: nada se veia mal, pero el navegador no paraba nunca. Con la salida
    // temprana, en reposo no se escribe nada y el ciclo se corta solo.
    marcarFila: function (grilla, fila) {
        var self = this;
        var previas = grilla.querySelectorAll('.' + this.CLASE_FOCO);

        // Ya esta como tiene que estar: exactamente la fila pedida y ninguna otra.
        if (previas.length === 1 && previas[0] === fila) return;
        if (previas.length === 0 && !fila) return;

        Array.prototype.forEach.call(previas, function (tr) {
            if (tr !== fila) tr.classList.remove(self.CLASE_FOCO);
        });
        if (fila && !fila.classList.contains(this.CLASE_FOCO)) fila.classList.add(this.CLASE_FOCO);
    },

    // Vuelve a marcar despues de un re-render: MudBlazor rehace las filas y se lleva
    // puesta la clase. Se reubica por POSICION, que es lo unico que sobrevive: el
    // elemento anterior ya no existe.
    remarcarPorPosicion: function (grilla) {
        var fila = this._filaPorGrilla.get(grilla);
        if (!fila) return;
        var filas = this.filasDe(grilla);
        this.marcarFila(grilla, filas[fila - 1] || null);
    },

    pintar: function (grilla) {
        var pie = this.pieDe(grilla);
        if (!pie) return;

        var fila = this._filaPorGrilla.get(grilla);
        var total = this.filasDe(grilla).length;

        // Sin fila elegida, o si la que estaba ya no existe (se filtró, se cambió de
        // página), el indicador se apaga en vez de mentir.
        if (!fila || fila > total) {
            this.ponerIndicador(pie, '');
            this.marcarFila(grilla, null);
            return;
        }

        this.ponerIndicador(pie, 'Fila ' + fila + ' de ' + total);
        this.remarcarPorPosicion(grilla);
    },

    // POR QUE NO SE PIDE UNA CELDA.
    //
    // La version anterior arrancaba con ev.target.closest('td') y se iba si no habia
    // celda. Con el mouse siempre la hay: se hace clic sobre el texto, que vive dentro
    // de un td.
    //
    // Pero la navegacion con flechas (index.html) mueve la fila haciendo fila.click(),
    // y eso despacha el evento SOBRE EL <tr>: ev.target es la fila, no una celda, y
    // closest('td') devuelve null. O sea que el puntero se quedaba donde habia sido el
    // ultimo clic del mouse mientras la fila azul seguia subiendo o bajando con las
    // flechas: dos filas marcadas al mismo tiempo, cada una por su lado.
    //
    // Buscar directamente el <tr> cubre los dos casos. No hace falta exigir una celda
    // para filtrar encabezados o filas de agrupacion: filasDe() ya devuelve solo las
    // filas con td, y si la fila no esta en esa lista el indexOf de mas abajo la
    // descarta igual.
    onClick: function (ev) {
        var fila = ev.target && ev.target.closest ? ev.target.closest('tr') : null;
        var grilla = this.grillaDe(fila);
        if (!fila || !grilla) return;

        var filas = this.filasDe(grilla);
        var idx = filas.indexOf(fila);
        if (idx < 0) return;

        this._filaPorGrilla.set(grilla, idx + 1);
        this.marcarFila(grilla, fila);
        this.pintar(grilla);
    },

    // ── Repaso general ──────────────────────────────────────────────────────

    // Después de cada re-render de MudBlazor hay que volver a traducir y a dibujar:
    // el pie se reconstruye entero y se lleva puesto lo que hayamos escrito.
    repasar: function () {
        var self = this;

        // El pie: traducir y redibujar el indicador.
        var pies = document.querySelectorAll('.mud-table-pagination');
        Array.prototype.forEach.call(pies, function (pie) {
            self.traducirPie(pie);
            var grilla = self.grillaDe(pie);
            if (grilla) self.pintar(grilla);
        });

        // Las grillas SIN paginador tambien llevan puntero: no todas las grillas de la
        // app muestran pie, pero en todas se puede hacer clic en una fila.
        var tablas = document.querySelectorAll('.mud-table');
        Array.prototype.forEach.call(tablas, function (grilla) {
            if (!self.pieDe(grilla)) self.remarcarPorPosicion(grilla);
        });
    },

    _pendiente: null,
    pedirRepaso: function () {
        var self = this;
        if (this._pendiente) return;
        this._pendiente = requestAnimationFrame(function () {
            self._pendiente = null;
            // Nunca tumbar la UI por un detalle del pie.
            try { self.repasar(); } catch (e) { }
        });
    },

    iniciar: function () {
        if (this._iniciado) return;
        this._iniciado = true;

        var self = this;

        // Captura: así corre aunque un handler de la pantalla detenga la propagación.
        document.addEventListener('click', function (ev) {
            try { self.onClick(ev); } catch (e) { }
        }, true);

        // Blazor no avisa cuando termina de redibujar, así que se observa el DOM.
        // characterData incluido: al cambiar de página, MudBlazor reescribe el texto
        // de la leyenda sin reemplazar el nodo.
        this._observer = new MutationObserver(function () { self.pedirRepaso(); });
        this._observer.observe(document.body, {
            childList: true, subtree: true, characterData: true
        });

        this.pedirRepaso();
    }
};

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', function () { window.gridPieFila.iniciar(); });
} else {
    window.gridPieFila.iniciar();
}

} // fin de la guardia de doble carga
