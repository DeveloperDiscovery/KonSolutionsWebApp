// diagRendimiento.js
// ---------------------------------------------------------------------------
// Medidor temporal para encontrar qué congela una pantalla.
//
// NO HACE NADA SOLO. No se engancha a nada al cargar: hay que llamarlo a mano
// desde la consola del navegador. Está pensado para dejarlo instalado sin que
// moleste, y sacarlo cuando el problema esté resuelto.
//
// CÓMO USARLO
//   1. Abre la pantalla que se congela (Usuarios).
//   2. F12 → pestaña Console.
//   3. Escribe:   diagRendimiento.iniciar()
//   4. Haz exactamente lo que provoca el freeze: pararte en una fila y mover
//      con las flechas.
//   5. Cuando se descongele:   diagRendimiento.resumen()
//   6. Copia lo que imprime y pásamelo.
//
// QUÉ MIDE
//   · Tareas largas (longtask): cada bloque de más de 50 ms en el que el
//     navegador NO pudo atender al usuario. Es literalmente el congelamiento.
//   · Cuántas veces por segundo se despiertan los MutationObserver, que es el
//     sospechoso principal: la app tiene seis mirando todo el documento.
//   · Cuántos redibujados hace Blazor, contando las mutaciones sobre filas de
//     grilla.
//   · Cuánto pesa la pantalla: tablas, filas, celdas e imágenes en base64.
//
// POR QUÉ HACE FALTA: leyendo el código encontré varios candidatos plausibles,
// y elegir uno sin medir es adivinar. Esto dice cuál es.
// ---------------------------------------------------------------------------
window.diagRendimiento = {

    _observadorTareas: null,
    _observadorDom: null,
    _t0: 0,
    _tareas: [],          // { ms, cuando }
    _mutaciones: 0,       // total de registros de mutación
    _despertares: 0,      // veces que se llamó al callback
    _mutPorNodo: {},      // qué se está mutando, agrupado

    iniciar: function () {
        this.detener();
        this._t0 = performance.now();
        this._tareas = [];
        this._mutaciones = 0;
        this._despertares = 0;
        this._mutPorNodo = {};

        var self = this;

        // Tareas largas: el bloqueo real, medido por el navegador.
        if (window.PerformanceObserver) {
            try {
                this._observadorTareas = new PerformanceObserver(function (lista) {
                    lista.getEntries().forEach(function (e) {
                        self._tareas.push({ ms: Math.round(e.duration), cuando: Math.round(e.startTime - self._t0) });
                    });
                });
                this._observadorTareas.observe({ entryTypes: ['longtask'] });
            } catch (e) {
                console.warn('[diag] Este navegador no reporta longtask; el resto igual sirve.');
            }
        }

        // El mismo alcance que usan los observadores de la app, para contar lo que
        // ellos reciben. Este solo lee y cuenta: no escribe nada en el DOM.
        this._observadorDom = new MutationObserver(function (registros) {
            self._despertares++;
            self._mutaciones += registros.length;

            // Se agrupa por "qué se tocó" para ver si las mutaciones vienen de las
            // filas de la grilla, de un reloj de la barra, o de los propios
            // observadores escribiéndose entre ellos.
            for (var i = 0; i < registros.length && i < 200; i++) {
                var r = registros[i];
                var destino = r.target;
                var etiqueta;

                if (r.type === 'attributes') {
                    etiqueta = self._nombre(destino) + ' [attr:' + r.attributeName + ']';
                } else if (r.type === 'characterData') {
                    etiqueta = self._nombre(destino.parentElement) + ' [texto]';
                } else {
                    etiqueta = self._nombre(destino) + ' [hijos]';
                }

                self._mutPorNodo[etiqueta] = (self._mutPorNodo[etiqueta] || 0) + 1;
            }
        });
        this._observadorDom.observe(document.body, {
            childList: true, subtree: true, characterData: true, attributes: true
        });

        console.log('%c[diag] Midiendo. Reproduce el problema y después llama a diagRendimiento.resumen()',
                    'color:#0a0;font-weight:bold');
        return 'midiendo...';
    },

    _nombre: function (el) {
        if (!el || !el.tagName) return '(?)';
        var t = el.tagName.toLowerCase();
        var c = (el.className && typeof el.className === 'string')
                ? '.' + el.className.trim().split(/\s+/).slice(0, 2).join('.')
                : '';
        var id = el.id ? '#' + el.id : '';
        return t + id + c;
    },

    // Cuánto pesa lo que hay en pantalla. Se mide aparte porque no cambia mientras
    // se reproduce el problema, y explica por qué una pantalla sufre y otra no.
    peso: function () {
        var tablas = document.querySelectorAll('.mud-table');
        var det = [];
        Array.prototype.forEach.call(tablas, function (t) {
            var cont = t.closest('[data-resizable-grid]');
            var filas = t.querySelectorAll('tbody tr').length;
            var celdas = t.querySelectorAll('tbody td').length;
            var imgs = t.querySelectorAll('tbody img').length;

            // Lo que de verdad duele: imágenes embebidas como data URI.
            var bytesBase64 = 0;
            Array.prototype.forEach.call(t.querySelectorAll('tbody img'), function (img) {
                var src = img.getAttribute('src') || '';
                if (src.indexOf('data:') === 0) bytesBase64 += src.length;
            });

            det.push({
                grilla: cont ? cont.getAttribute('data-resizable-grid') : '(sin nombre)',
                filas: filas,
                celdas: celdas,
                imagenes: imgs,
                base64_KB: Math.round(bytesBase64 / 1024)
            });
        });
        return det;
    },

    resumen: function () {
        var seg = (performance.now() - this._t0) / 1000;

        var total = this._tareas.reduce(function (a, t) { return a + t.ms; }, 0);
        var peor = this._tareas.reduce(function (a, t) { return t.ms > a ? t.ms : a; }, 0);

        console.log('%c[diag] RESUMEN — ' + seg.toFixed(1) + ' s medidos',
                    'color:#08c;font-weight:bold;font-size:14px');

        console.log('Bloqueo: ' + this._tareas.length + ' tareas largas, ' +
                    (total / 1000).toFixed(1) + ' s en total, la peor de ' + peor + ' ms.');
        console.log('  (si esa suma se parece al tiempo que estuvo congelado, el problema es de esta pestaña, ' +
                    'no de la red ni del servidor)');

        console.log('Mutaciones del DOM: ' + this._mutaciones + ' en ' + this._despertares +
                    ' despertares — ' + (this._mutaciones / Math.max(seg, 1)).toFixed(0) + ' por segundo.');
        console.log('  (en una pantalla quieta esto tiene que quedarse cerca de 0. ' +
                    'Si no para nunca, hay un ciclo)');

        console.log('Las 10 tareas largas peores:');
        console.table(this._tareas.slice().sort(function (a, b) { return b.ms - a.ms; }).slice(0, 10));

        var top = Object.keys(this._mutPorNodo)
            .map(function (k) { return { que: k, veces: this._mutPorNodo[k] }; }, this)
            .sort(function (a, b) { return b.veces - a.veces; })
            .slice(0, 15);
        console.log('Qué se está mutando (las 15 más frecuentes):');
        console.table(top);

        console.log('Peso de las grillas en pantalla:');
        console.table(this.peso());

        return 'Copia todo lo de arriba y pásaselo a Hal.';
    },

    detener: function () {
        if (this._observadorTareas) { try { this._observadorTareas.disconnect(); } catch (e) { } this._observadorTareas = null; }
        if (this._observadorDom) { try { this._observadorDom.disconnect(); } catch (e) { } this._observadorDom = null; }
        return 'detenido';
    }
};
