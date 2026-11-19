// konTrace.js
// ---------------------------------------------------------------------------
// Traza de diagnóstico. Se carga ANTES que todo y graba solo.
//
// POR QUE EXISTE, Y POR QUE ASI
// -----------------------------
// La consola ya nos dijo tres cosas que yo no podía deducir leyendo el código:
//
//   1. MudBlazor tira una excepción en CADA flecha:
//        Uncaught TypeError: Cannot read properties of undefined (reading 'toLowerCase')
//        at HTMLDivElement.onKeyDown (MudBlazor.min.js)
//        at HTMLDivElement.onKeyUp  (MudBlazor.min.js)
//
//   2. Justo después, la aplicación VOLVIO A ARRANCAR: hay un segundo
//      "INICIO CARGA MENU" 65 segundos después del primero, y esa carga tardó
//      23 segundos. Eso —no un bucle de dibujado— es el minuto o dos que ves.
//
//   3. Hubo un 401 en /api/v1/developer/error-log.
//
// Lo que falta saber es QUIEN reinicia la aplicación. Y ahí está el problema de
// diagnóstico: si la página se recarga, la consola se limpia y la evidencia se
// pierde. Por eso esto guarda en sessionStorage, que SOBREVIVE a la recarga.
//
// Después del congelamiento, en la consola:
//
//     konTrace.volcar()      ve toda la línea de tiempo, antes y después
//     konTrace.copiar()      la copia al portapapeles para pegármela
//     konTrace.limpiar()     empezar de cero
//
// QUE GRABA
//   · cada arranque de la página, y si fue navegación, RECARGA o botón atrás
//   · quién llamó a location.reload / replace / assign, con la pila de llamadas
//   · cada salida de la página (pagehide), con la pila
//   · todo error no atrapado, con su pila
//   · las flechas, para poder anclar "acá apreté"
//   · las respuestas HTTP con error o lentas (>1s)
//   · las tareas largas del navegador (>50ms): el congelamiento medido
//   · las líneas [TRACE] que ya escribe la aplicación
//
// NO TOCA EL DOM. Solo lee, mide y guarda.
// ---------------------------------------------------------------------------
(function () {
    'use strict';
    if (window.konTrace) return;            // guardia de doble carga

    var CLAVE = 'kon.trace';
    var TOPE  = 400;                        // entradas; las viejas se descartan

    var log = [];
    try { log = JSON.parse(sessionStorage.getItem(CLAVE) || '[]'); } catch (e) { log = []; }

    var guardarPendiente = null;

    function guardar() {
        if (guardarPendiente) return;
        guardarPendiente = setTimeout(function () {
            guardarPendiente = null;
            try { sessionStorage.setItem(CLAVE, JSON.stringify(log)); } catch (e) { }
        }, 250);
    }

    function guardarYa() {
        if (guardarPendiente) { clearTimeout(guardarPendiente); guardarPendiente = null; }
        try { sessionStorage.setItem(CLAVE, JSON.stringify(log)); } catch (e) { }
    }

    function hora() {
        var d = new Date();
        return d.toTimeString().slice(0, 8) + '.' +
               ('00' + d.getMilliseconds()).slice(-3);
    }

    function anotar(tipo, texto, extra) {
        log.push({ h: hora(), tipo: tipo, texto: String(texto).slice(0, 400), extra: extra || '' });
        if (log.length > TOPE) log.splice(0, log.length - TOPE);
        guardar();
    }

    // La pila de llamadas SIN la línea de esta función, que no aporta nada.
    function pila(saltar) {
        try {
            var l = (new Error()).stack.split('\n');
            return l.slice(1 + (saltar || 0), 7).map(function (x) { return x.trim(); }).join(' | ');
        } catch (e) { return ''; }
    }

    // ── 1 · El arranque, y de qué tipo ──────────────────────────────────────
    // Esto es lo que va a delatar la recarga: si entre tus flechas y el
    // congelamiento aparece un ARRANQUE de tipo "reload", el problema no es que
    // la grilla dibuje lento, es que la aplicación se está reiniciando entera.
    var tipoNav = 'desconocido';
    try {
        var nav = performance.getEntriesByType('navigation')[0];
        if (nav) tipoNav = nav.type;                       // navigate | reload | back_forward
    } catch (e) { }
    anotar('ARRANQUE', 'La página cargó — tipo: ' + tipoNav + ' — ' + location.pathname + location.search);

    // ── 2 · Quién recarga o navega ──────────────────────────────────────────
    // location.reload/replace/assign se envuelven para dejar la pila de quien las
    // llamó. Es la pregunta central: quién reinicia la aplicación.
    try {
        ['reload', 'replace', 'assign'].forEach(function (m) {
            var orig = window.location[m];
            if (typeof orig !== 'function') return;
            window.location[m] = function () {
                anotar('NAVEGA', 'location.' + m + '(' + (arguments[0] || '') + ')', pila(1));
                guardarYa();
                return orig.apply(window.location, arguments);
            };
        });
    } catch (e) {
        // Algunos navegadores no dejan reescribir location. No es grave: el
        // ARRANQUE de tipo "reload" igual delata que hubo recarga.
        anotar('AVISO', 'No se pudo instrumentar location: ' + e.message);
    }

    window.addEventListener('pagehide', function () {
        anotar('SALE', 'La página se está descargando');
        guardarYa();
    });

    // ── 3 · Errores ─────────────────────────────────────────────────────────
    window.addEventListener('error', function (ev) {
        var d = ev.error && ev.error.stack
              ? ev.error.stack.split('\n').slice(0, 4).join(' | ')
              : (ev.filename || '') + ':' + (ev.lineno || '');
        anotar('ERROR', ev.message || 'error', d);
    }, true);

    window.addEventListener('unhandledrejection', function (ev) {
        var r = ev.reason;
        anotar('PROMESA', (r && (r.message || r)) || 'rechazo sin motivo',
               (r && r.stack) ? r.stack.split('\n').slice(0, 4).join(' | ') : '');
    });

    // ── 4 · Las flechas ─────────────────────────────────────────────────────
    // Sirven de ancla: "acá apreté". Se mide además cuánto tarda el navegador en
    // volver a pintar después de la tecla — si eso da miles de ms, el bloqueo
    // arranca exactamente ahí.
    function anclaTecla(ev) {
        if (ev.key !== 'ArrowUp' && ev.key !== 'ArrowDown' &&
            ev.key !== 'ArrowLeft' && ev.key !== 'ArrowRight') return;
        var t0 = performance.now();
        anotar('TECLA', ev.type + ' ' + ev.key +
               ' sobre ' + (ev.target && ev.target.tagName ? ev.target.tagName.toLowerCase() : '?'));
        requestAnimationFrame(function () {
            var ms = Math.round(performance.now() - t0);
            if (ms > 100) anotar('LENTO', 'El navegador tardó ' + ms + ' ms en volver a pintar tras la tecla');
        });
    }
    window.addEventListener('keydown', anclaTecla, true);

    // ── 5 · Tareas largas: el congelamiento, medido ─────────────────────────
    try {
        new PerformanceObserver(function (lista) {
            lista.getEntries().forEach(function (e) {
                if (e.duration >= 200) anotar('BLOQUEO', 'El navegador quedó tomado ' + Math.round(e.duration) + ' ms');
            });
        }).observe({ entryTypes: ['longtask'] });
    } catch (e) { }

    // ── 6 · Llamadas HTTP con error o lentas ────────────────────────────────
    try {
        var fetchOrig = window.fetch;
        window.fetch = function (entrada, opciones) {
            var url = (typeof entrada === 'string') ? entrada : (entrada && entrada.url) || '';
            var t0 = performance.now();
            return fetchOrig.apply(this, arguments).then(function (resp) {
                var ms = Math.round(performance.now() - t0);
                if (!resp.ok)      anotar('HTTP', resp.status + ' ' + corta(url) + ' (' + ms + ' ms)');
                else if (ms > 1000) anotar('HTTP-LENTO', corta(url) + ' tardó ' + ms + ' ms');
                return resp;
            }, function (err) {
                anotar('HTTP-FALLA', corta(url) + ': ' + (err && err.message));
                throw err;
            });
        };
    } catch (e) { }

    function corta(u) {
        try { return u.replace(location.origin, ''); } catch (e) { return String(u).slice(0, 120); }
    }

    // ── 7 · Las trazas que ya escribe la aplicación ─────────────────────────
    // Así las líneas [TRACE] de C# quedan en la MISMA línea de tiempo que todo lo
    // de arriba, que es lo que permite ver "flecha → error → recarga → CARGA MENU".
    try {
        var logOrig = console.log;
        console.log = function () {
            try {
                var p = arguments[0];
                if (typeof p === 'string' && p.indexOf('[TRACE]') === 0) anotar('APP', p.slice(8, 300));
            } catch (e) { }
            return logOrig.apply(console, arguments);
        };
    } catch (e) { }

    // ── La cara visible ─────────────────────────────────────────────────────
    window.konTrace = {
        volcar: function () {
            console.log('%c[konTrace] ' + log.length + ' eventos — la línea de tiempo completa, ' +
                        'incluso a través de recargas', 'color:#08c;font-weight:bold;font-size:14px');
            console.table(log);

            var arranques = log.filter(function (e) { return e.tipo === 'ARRANQUE'; });
            if (arranques.length > 1) {
                console.log('%c[konTrace] OJO: la página arrancó ' + arranques.length +
                            ' veces. Si alguno dice "reload", la aplicación se reinició sola.',
                            'color:#c00;font-weight:bold');
            }
            var bloqueos = log.filter(function (e) { return e.tipo === 'BLOQUEO'; });
            if (bloqueos.length) {
                var total = bloqueos.reduce(function (a, e) {
                    return a + (parseInt((e.texto.match(/(\d+) ms/) || [0, 0])[1], 10) || 0);
                }, 0);
                console.log('%c[konTrace] Bloqueo total medido: ' + (total / 1000).toFixed(1) + ' s en ' +
                            bloqueos.length + ' tramos.', 'color:#c00;font-weight:bold');
            }
            return 'Usa konTrace.copiar() para pegármelo.';
        },

        copiar: function () {
            var txt = log.map(function (e) {
                return e.h + '  ' + e.tipo.padEnd(11) + ' ' + e.texto + (e.extra ? '\n                     ↳ ' + e.extra : '');
            }).join('\n');
            try { navigator.clipboard.writeText(txt); return 'Copiado: ' + log.length + ' eventos. Pégalo en el chat.'; }
            catch (e) { console.log(txt); return 'No pude copiar; está impreso arriba.'; }
        },

        limpiar: function () {
            log = [];
            try { sessionStorage.removeItem(CLAVE); } catch (e) { }
            anotar('ARRANQUE', 'Traza limpiada a mano');
            return 'listo';
        },

        // Por si quieres marcar un punto exacto: konTrace.marca('acá apreté abajo')
        marca: function (texto) { anotar('MARCA', texto || 'marca'); return 'anotado'; }
    };

    // ── QUE SE VEA QUE ESTA ──────────────────────────────────────────────────
    // La primera versión grababa en silencio y no avisaba nada, así que parecía que no
    // estaba instalada. Ahora se anuncia al cargar, y si detecta que la página arrancó
    // más de una vez —o sea que la aplicación se reinició— vuelca el resumen SOLO, sin
    // que haya que acordarse de pedirlo.
    var arranques = log.filter(function (e) { return e.tipo === 'ARRANQUE'; }).length;

    console.log('%c[konTrace] activo · ' + log.length + ' eventos guardados · ' +
                arranques + ' arranque(s) de pagina en esta sesion\n' +
                '           konTrace.volcar()  ver todo     konTrace.copiar()  copiarlo para Hal',
                'color:#0a0;font-weight:bold');

    if (arranques > 1) {
        console.log('%c[konTrace] LA PAGINA ARRANCO ' + arranques + ' VECES: la aplicacion se ' +
                    'reinicio sola. Volcado automatico:', 'color:#c00;font-weight:bold;font-size:14px');
        try { window.konTrace.volcar(); } catch (e) { }
    }
})();
