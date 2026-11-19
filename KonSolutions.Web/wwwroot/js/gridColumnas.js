/*
    gridColumnas.js
    --------------------------------------------------------------------------
    Ancho de columnas ajustable con el mouse, y recordado entre sesiones, para
    TODAS las grillas de la app.

    Por qué en JS y no en Blazor: arrastrar el borde de una columna son decenas
    de eventos de mousemove por segundo. Pasarlos por el circuito de Blazor
    haría un render por cada uno y el arrastre se sentiría pegajoso. Acá se
    tocan los <col> directamente y Blazor ni se entera.

    Cómo identifica cada grilla: si el contenedor declara data-resizable-grid,
    ese es el nombre. Si no, se arma uno con la ruta de la página y los títulos
    de las columnas. Lo segundo es a propósito: así funciona en las grillas que
    ya existen sin tener que ir a marcarlas una por una, y si alguien agrega o
    saca una columna la clave cambia sola y no quedan anchos viejos aplicados a
    columnas que ya no son las mismas.

    Se guarda en localStorage y no en el servidor porque es una preferencia de
    cómo se ve la pantalla en ESTA máquina — el mismo usuario en un monitor
    distinto normalmente quiere otros anchos.
*/
(function () {
    'use strict';

    const PREFIJO = 'kon.grid.cols.';
    const MIN_PX = 40;          // por debajo de esto la columna deja de leerse
    const ZONA_AGARRE = 6;      // ancho en px de la franja sensible del borde
    const ALTO_MIN = 120;       // por debajo de esto no entra ninguna fila

    // ── Zoom de la app ──────────────────────────────────────────────────────
    // El zoom propio (appZoom.js) es transform:scale sobre #app. Eso significa que
    // getBoundingClientRect() y clientX devuelven pixeles VISUALES —ya multiplicados por la
    // escala— mientras que style.width se interpreta en pixeles de LAYOUT. Mezclarlos hace
    // que, con zoom distinto de 100%, la columna pegue un salto apenas se empieza a
    // arrastrar y que el borde no siga al mouse.
    //
    // Dividiendo por la escala, todo lo que se mide vuelve a layout y el ancho que se
    // guarda es el mismo se vea la pantalla al 80% o al 125%.
    function escala() {
        try {
            const s = window.appZoom && window.appZoom.getScale ? window.appZoom.getScale() : 1;
            return (typeof s === 'number' && s > 0) ? s : 1;
        } catch (e) { return 1; }
    }

    // ── Globo con el ancho mientras se arrastra ─────────────────────────────
    // Un tooltip del navegador (title) no sirve: no se actualiza con el boton apretado y
    // aparece recien tras una pausa. Este va pegado al cursor y cambia en vivo.
    let globo = null;

    function mostrarGlobo(x, y, texto) {
        if (!globo) {
            globo = document.createElement('div');
            globo.className = 'kon-col-ancho';
            // Estilos inline por el mismo motivo que la franja de agarre: si app.css
            // quedara cacheado, un div sin posicionar descuadraria la pagina.
            globo.style.cssText =
                'position:fixed;z-index:9999;pointer-events:none;' +
                'padding:3px 8px;border-radius:4px;font-size:12px;font-weight:700;' +
                'font-family:ui-monospace,SFMono-Regular,Menlo,monospace;white-space:nowrap;' +
                'background:var(--ink,#16130f);color:var(--paper,#efe9df);' +
                'box-shadow:0 2px 8px rgba(0,0,0,.25);';
            document.body.appendChild(globo);
        }
        globo.textContent = texto;
        globo.style.left = Math.round(x + 14) + 'px';
        globo.style.top  = Math.round(y + 18) + 'px';
    }

    function ocultarGlobo() {
        if (globo && globo.parentNode) globo.parentNode.removeChild(globo);
        globo = null;
    }

    // ── Identidad de la grilla ──────────────────────────────────────────────
    function claveDe(tabla) {
        // data-cols-key: identidad propia SOLO para los anchos de columna. Sirve cuando dos
        // grillas comparten data-resizable-grid a propósito —para que el alto se mueva en
        // grupo, como el detalle del pedido comercial y su grilla de cálculos— pero tienen
        // columnas distintas y no pueden compartir los anchos guardados.
        const propio = tabla.closest('[data-cols-key]');
        if (propio) return PREFIJO + propio.getAttribute('data-cols-key');

        const marcado = tabla.closest('[data-resizable-grid]');
        if (marcado) return PREFIJO + marcado.getAttribute('data-resizable-grid');

        const titulos = [...tabla.querySelectorAll('thead th')]
            .map(th => (th.textContent || '').trim().slice(0, 20))
            .join('~');
        return PREFIJO + location.pathname + '|' + titulos;
    }

    function leer(clave) {
        try { return JSON.parse(localStorage.getItem(clave) || '{}'); }
        catch { return {}; }
    }

    function guardar(clave, anchos) {
        try { localStorage.setItem(clave, JSON.stringify(anchos)); }
        catch { /* modo privado o cuota llena: se pierde la preferencia, no la pantalla */ }
    }

    // ── Columnas congeladas (position:sticky por CSS) ────────────────────────
    // Las pantallas con "líneas pendientes" fijan algunas columnas a los lados
    // (Selección, Requerimiento, Sec., etc.) con position:sticky + un ancho en
    // px puesto por app.css con !important. Ese ancho es la mitad de la cuenta:
    // el sticky apila varias columnas con left/right en píxeles EXACTOS, así
    // que si esta grilla les cambia el ancho — a mano, o reaplicando un valor
    // guardado de antes de que existiera el congelamiento — todo el cálculo de
    // left/right deja de coincidir con lo que se ve en pantalla y las columnas
    // se superponen. Por eso estas columnas se dejan afuera del todo: sin
    // agarre para arrastrarlas y sin aplicarles ningún ancho guardado.
    function esFija(th) {
        // OJO: no alcanza con mirar position:sticky a secas. FixedHeader="true"
        // (que usan casi todas las grillas, no sólo las que congelan columnas a
        // los lados) también pone sticky en TODOS los <th> de la cabecera, para
        // que se quede pegada arriba al hacer scroll vertical — con top fijado,
        // pero left/right en "auto" porque no necesita pegarse a los costados.
        // Mirar sólo sticky hacía que este guard se activara en TODA cabecera
        // fija de TODA la app: ninguna columna se podía arrastrar, y las que no
        // están realmente congeladas (Producto, Lote/Orden, etc.) se quedaban
        // sin el ancho que este script les aplica o recuerda.
        //
        // Nuestro congelamiento horizontal siempre fija left o right en un
        // valor en píxeles exacto (nunca "auto") — es la única marca que
        // distingue a una columna REALMENTE congelada de una cabecera
        // simplemente fija verticalmente.
        const est = getComputedStyle(th);
        if (est.position !== 'sticky') return false;
        return est.left !== 'auto' || est.right !== 'auto';
    }

    // ── Aplicar anchos ──────────────────────────────────────────────────────
    // Se escribe sobre el <th>, que es lo que MudDataGrid ya usa para su propio
    // Width. Un intento anterior inyectaba un <colgroup>: eso le desarmaba la
    // tabla a MudDataGrid y las columnas colapsaban.
    function aplicar(tabla, anchos) {
        const ths = tabla.querySelectorAll('thead th');
        ths.forEach((th, i) => {
            if (esFija(th)) return;                // el ancho de una columna congelada lo fija el CSS
            const guardado = anchos[i];
            if (!guardado) return;                 // sin ancho propio se respeta el de la pantalla
            const px = guardado + 'px';
            th.style.width = px;
            th.style.minWidth = px;
            th.style.maxWidth = px;
        });
    }

    // ── Arrastre ────────────────────────────────────────────────────────────
    function prepararCabecera(tabla) {
        const clave = claveDe(tabla);
        const anchos = leer(clave);
        aplicar(tabla, anchos);

        const ths = [...tabla.querySelectorAll('thead th')];
        ths.forEach((th, i) => {
            if (esFija(th)) return;                // columna congelada: sin agarre, ancho fijo por diseño
            // Se revisa el agarre y no sólo la marca: MudDataGrid reconstruye el
            // thead al re-renderizar, y una celda puede conservar la marca pero
            // haber perdido su franja.
            if (th.dataset.konResize && th.querySelector(':scope > .kon-col-resize')) return;
            th.dataset.konResize = '1';
            // La cabecera fija usa position:sticky puesto por CSS. Pisarlo con
            // relative la despegaría, y sticky ya sirve de ancla para el absolute.
            const pos = getComputedStyle(th).position;
            if (pos === 'static') th.style.position = 'relative';

            const agarre = document.createElement('span');
            agarre.className = 'kon-col-resize';
            // Posicionado desde acá a propósito: si app.css quedara cacheado, un
            // span sin posicionar entraría en el flujo del encabezado y lo
            // descuadraría. El CSS sólo agrega el color y la marca.
            agarre.style.cssText =
                'position:absolute;top:0;right:-3px;height:100%;width:' + ZONA_AGARRE +
                'px;cursor:col-resize;user-select:none;z-index:20;';
            agarre.title = 'Arrastrar para cambiar el ancho · doble clic para restablecer';
            th.appendChild(agarre);

            agarre.addEventListener('mousedown', ev => {
                ev.preventDefault();
                ev.stopPropagation();   // que no dispare el ordenamiento de la columna

                const s = escala();
                const xInicial = ev.clientX;
                // A pixeles de LAYOUT: es la unidad en la que se escribe style.width y en la
                // que se guarda el ancho. Ver el comentario de escala().
                const anchoInicial = th.getBoundingClientRect().width / s;

                document.body.classList.add('kon-redimensionando');
                agarre.classList.add('activo');

                let ancho = Math.round(anchoInicial);
                mostrarGlobo(ev.clientX, ev.clientY, ancho + ' px');

                function mover(e) {
                    ancho = Math.max(MIN_PX, Math.round(anchoInicial + (e.clientX - xInicial) / s));
                    const px = ancho + 'px';
                    th.style.width = px;
                    th.style.minWidth = px;
                    th.style.maxWidth = px;
                    mostrarGlobo(e.clientX, e.clientY, ancho + ' px');
                }

                function soltar() {
                    document.removeEventListener('mousemove', mover);
                    document.removeEventListener('mouseup', soltar);
                    document.body.classList.remove('kon-redimensionando');
                    agarre.classList.remove('activo');
                    ocultarGlobo();

                    const guardados = leer(clave);
                    guardados[i] = ancho;
                    guardar(clave, guardados);
                }

                document.addEventListener('mousemove', mover);
                document.addEventListener('mouseup', soltar);
            });

            // Doble clic: vuelve al ancho que declaró la pantalla.
            agarre.addEventListener('dblclick', ev => {
                ev.preventDefault();
                ev.stopPropagation();
                const guardados = leer(clave);
                delete guardados[i];
                guardar(clave, guardados);
                th.style.width = th.style.minWidth = th.style.maxWidth = '';
                aplicar(tabla, guardados);
            });
        });
    }

    // ── Alto de la grilla ───────────────────────────────────────────────────
    // El proyecto ya traía el estilo de un tirador de alto (.grid-resize-handle) pero
    // ningún código que lo manejara. Se engancha acá, junto al de columnas, porque el
    // problema es el mismo: preferencias de tamaño que conviene recordar.
    function prepararAlto(caja) {
        const cont = caja.querySelector('.mud-table-container');
        if (!cont) return;                       // la grilla todavía no se dibujó

        // El tirador se CREA si falta, en vez de exigir que cada pantalla lo escriba.
        //
        // Declarar data-resizable-grid y olvidarse del <div class="grid-resize-handle"> deja
        // una grilla que se anuncia redimensionable y no lo es, y el olvido no se ve en
        // ningún lado: la pantalla se ve igual. Pasaba en tres pantallas —Transacciones de
        // Almacén entre ellas— de las ciento cuarenta y cinco que lo declaran.
        //
        // Ponerlo acá cierra el agujero para las tres y para toda grilla que se agregue
        // después: alcanza con declarar el contenedor. Las que ya lo traen en el marcado
        // siguen usando el suyo.
        let tirador = caja.querySelector(':scope > .grid-resize-handle');
        if (!tirador) {
            // position:relative es lo que ancla el tirador al borde de la caja. Las pantallas
            // lo ponen en el style del div; si faltara, el tirador se iría a buscar el primer
            // ancestro posicionado y aparecería en cualquier parte.
            if (getComputedStyle(caja).position === 'static') caja.style.position = 'relative';
            tirador = document.createElement('div');
            tirador.className = 'grid-resize-handle';
            caja.appendChild(tirador);
        }
        if (tirador.dataset.konAlto) return;
        tirador.dataset.konAlto = '1';

        const grupo = caja.getAttribute('data-resizable-grid');

        // Varias grillas pueden declarar el mismo data-resizable-grid para moverse juntas
        // (el detalle del pedido comercial y su grilla de cálculos, por ejemplo). El tirador
        // vive en una sola de ellas, pero el alto se aplica a todas: si no, arrastrar una
        // desalinea a la otra. Sin grupo, la lista tiene un solo elemento y esto se comporta
        // igual que antes.
        const contenedores = () => grupo
            ? [...document.querySelectorAll('[data-resizable-grid="' + grupo + '"] .mud-table-container')]
            : [cont];

        // Las CAJAS, no sólo los contenedores internos. Acá estaba el motivo por el que
        // arrastrar el borde no servía de nada: gridAutoAlto estira la caja
        // [data-resizable-grid] hasta la barra de estado y vuelve a hacerlo en CADA cambio
        // del DOM. Esto escribía el alto sólo en el .mud-table-container de adentro, así que
        // el primer recálculo —y hay uno por cada re-render de Blazor— devolvía la grilla a
        // su alto automático. El arrastre funcionaba; duraba un suspiro.
        //
        // data-alto-manual es la marca que gridAutoAlto ya mira para dejar en paz a una
        // grilla; nadie la ponía nunca. Se pone acá, que es el único lugar donde se sabe que
        // el usuario decidió el alto a mano.
        const cajas = () => grupo
            ? [...document.querySelectorAll('[data-resizable-grid="' + grupo + '"]')]
            : [caja];
        const aplicarAlto = (valor) => {
            cajas().forEach(c => {
                if (valor) { c.dataset.altoManual = '1'; c.style.height = valor; }
                else { delete c.dataset.altoManual; c.style.height = ''; }
            });
            contenedores().forEach(c => { c.style.height = valor; });
        };

        const clave = PREFIJO + 'alto.' + (grupo || location.pathname);
        const guardado = parseInt(localStorage.getItem(clave) || '', 10);
        if (guardado > 0) aplicarAlto(guardado + 'px');

        tirador.addEventListener('mousedown', ev => {
            ev.preventDefault();
            // Mismo criterio que el ancho de columna: el alto tambien se medía en pixeles
            // visuales y se escribía en pixeles de layout, asi que con zoom la grilla
            // pegaba un salto al empezar a arrastrar. Y tambien muestra su medida.
            const s = escala();
            const yInicial = ev.clientY;

            // SE MIDE LA CAJA, NO EL CONTENEDOR INTERNO. Los dos son distintos y por eso el
            // arrastre parecía no responder: aplicarAlto escribe el alto en la CAJA
            // [data-resizable-grid], pero acá se partía del alto del .mud-table-container,
            // que dentro de la caja es flex:1 1 0% y mide lo que sobra después del
            // encabezado y del pie — unos 62px menos.
            //
            // O sea que al apretar el tirador la caja saltaba de golpe a 62px menos de lo
            // que medía, y recién sobre ese valor se sumaba el desplazamiento del mouse:
            // arrastrar 60px hacia abajo dejaba la grilla igual o más chica. Con el globito
            // mostrando un número que sí cambiaba, que es lo que más despista.
            //
            // El desfase existía desde siempre; se volvió visible cuando el alto pasó a
            // escribirse en la caja para que gridAutoAlto no lo pisara.
            const altoInicial = caja.getBoundingClientRect().height / s;
            let alto = Math.round(altoInicial);

            // Un CLIC no es un arrastre. Sin esta marca, apoyar el mouse en la franja de 3px
            // del tirador y soltar sin mover guardaba el alto que la grilla tenía en ese
            // momento y la dejaba marcada como "alto manual" PARA SIEMPRE: desde ahí
            // gridAutoAlto la ignora y la grilla deja de seguir al layout —por ejemplo, de
            // crecer al ocultar los filtros—. El usuario nunca decidió ese alto y no tiene
            // cómo saber que quedó fijado.
            let movio = false;

            document.body.style.cursor = 'row-resize';
            document.body.style.userSelect = 'none';
            tirador.classList.add('arrastrando');

            function mover(e) {
                // Umbral de 3px: un temblor de la mano al hacer clic no es una decisión.
                if (Math.abs(e.clientY - yInicial) > 3) movio = true;
                if (!movio) return;
                alto = Math.max(ALTO_MIN, Math.round(altoInicial + (e.clientY - yInicial) / s));
                aplicarAlto(alto + 'px');
                mostrarGlobo(e.clientX, e.clientY, alto + ' px');
            }
            function soltar() {
                document.removeEventListener('mousemove', mover);
                document.removeEventListener('mouseup', soltar);
                document.body.style.cursor = '';
                document.body.style.userSelect = '';
                tirador.classList.remove('arrastrando');
                ocultarGlobo();
                if (!movio) return;   // clic sin arrastre: no hay preferencia que guardar
                try { localStorage.setItem(clave, String(alto)); } catch { }
            }
            document.addEventListener('mousemove', mover);
            document.addEventListener('mouseup', soltar);
        });

        // Doble clic: vuelve al alto automático. Al borrar la marca, gridAutoAlto vuelve a
        // hacerse cargo de esta grilla; se le pide el recálculo en el momento para que el
        // cambio se vea ya y no en el próximo movimiento del DOM.
        tirador.addEventListener('dblclick', () => {
            try { localStorage.removeItem(clave); } catch { }
            aplicarAlto('');
            if (window.gridAutoAlto) window.gridAutoAlto.pedirRecalculo();
        });
    }

    function repasar(raiz) {
        (raiz || document).querySelectorAll('[data-resizable-grid]').forEach(caja => {
            try { prepararAlto(caja); } catch { /* que una grilla no tumbe a las demás */ }
        });

        // table[data-cols-key]: una tabla HTML simple (no MudDataGrid) que pide columnas
        // ajustables declarando su clave en la propia tabla, como el kardex de stock.
        const tablas = (raiz || document).querySelectorAll('table.mud-table-root, table.sim-grid, table[data-cols-key]');
        tablas.forEach(t => {
            if (!t.querySelector('thead th')) return;   // todavía sin cabecera
            try { prepararCabecera(t); } catch { /* una grilla rota no puede tumbar las demás */ }
        });
    }

    // Las grillas aparecen y se re-renderizan solas: al cambiar de solapa, al
    // abrir un documento, al filtrar. Por eso se observa el DOM en vez de
    // enganchar una sola vez al cargar.
    let pendiente = null;
    const observador = new MutationObserver(() => {
        clearTimeout(pendiente);
        pendiente = setTimeout(() => repasar(document), 120);
    });

    function iniciar() {
        repasar(document);
        observador.observe(document.body, { childList: true, subtree: true });
    }

    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', iniciar);
    else iniciar();

    // Para poder limpiarlo desde la consola si alguna vez hace falta.
    window.konGridColumnas = {
        restablecerTodo() {
            Object.keys(localStorage)
                .filter(k => k.startsWith(PREFIJO))
                .forEach(k => localStorage.removeItem(k));
            location.reload();
        }
    };
})();
