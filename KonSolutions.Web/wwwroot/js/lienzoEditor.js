// Lienzo del diseño libre de reportes (Pages/Reportes/LienzoEditor.razor), al estilo de Crystal Reports.
//
// Blazor dibuja las secciones y los objetos; este módulo solo atiende los gestos con el mouse, que tienen que ser
// fluidos: mover (uno o varios, también a otra sección), estirar desde las asas, seleccionar con un recuadro, cambiar
// el alto de una sección desde su borde inferior y soltar campos arrastrados desde el explorador. Mientras dura el
// gesto mueve los elementos en pantalla; al soltar avisa a C# con las medidas en milímetros, ya ajustadas a la
// cuadrícula, y C# vuelve a dibujar. Las teclas (Supr, flechas, Ctrl+Z...) se pasan a C# tal cual.
//
// Los objetos llevan sus medidas en atributos: data-obj (número), data-banda, data-x, data-y, data-an, data-al (mm)
// y data-tipo. Las secciones: data-area (el área donde van los objetos, con data-alto), data-rotulo y data-borde.
//
// Las líneas y los cuadros que cruzan secciones van en una capa aparte (.lz-cruces) con data-cruce (número),
// data-banda y data-y (donde empiezan), data-banda-fin y data-yf (donde terminan), data-x y data-an. Este módulo los
// pone en su lugar según dónde quedan las secciones en pantalla (cada vez que algo cambia), los mueve desde su borde y
// cambia dónde terminan desde su asa (data-cruce-fin). Estirar una línea o un cuadro desde abajo hasta otra sección
// también avisa dónde terminó (bandaFin, yFin): C# decide si puede cruzar.
window.lienzoEditor = (function () {
    const estados = new WeakMap();

    // Arrastrar desde el explorador o la barra: el valor viaja en data-arrastre ("CAM|1|COLUMNA", "NUE|TXT"...).
    document.addEventListener('dragstart', function (e) {
        const el = e.target && e.target.closest ? e.target.closest('[data-arrastre]') : null;
        if (!el || !el.closest('.lz')) return;
        e.dataTransfer.setData('text/plain', el.getAttribute('data-arrastre'));
        e.dataTransfer.effectAllowed = 'copy';
    });

    function px(st, mm) { return (mm * st.op.escala) + 'px'; }
    function mm(st, px) { return px / st.op.escala; }
    function ajustar(st, v) {
        if (!st.op.ajustar) return Math.round(v * 100) / 100;
        const g = st.op.rejilla || 1;
        return Math.round(Math.round(v / g) * g * 100) / 100;
    }
    // Al mover o estirar se ajusta el desplazamiento (de a pasos de la cuadrícula), no la posición: lo que estaba
    // alineado entre sí sigue alineado aunque no caiga en la cuadrícula (un diseño convertido, una medida escrita).
    function paso(st, d) { return ajustar(st, d); }
    function r2(v) { return Math.round(v * 100) / 100; }
    function num(el, a) { return parseFloat(el.getAttribute(a)) || 0; }
    function llamar(st, metodo, ...args) {
        // Si el componente ya se cerró, la llamada se rechaza: no es un error para el usuario.
        st.ref.invokeMethodAsync(metodo, ...args).catch(() => { });
    }
    function idDe(el) { return +(el.getAttribute('data-obj') ?? el.getAttribute('data-cruce')); }
    function seleccionados(st) {
        return [...st.raiz.querySelectorAll('[data-obj].lz-sel,[data-cruce].lz-sel')].map(idDe);
    }
    function marcar(st, ids) {
        st.raiz.querySelectorAll('[data-obj],[data-cruce]').forEach(el => el.classList.toggle('lz-sel', ids.includes(idDe(el))));
    }
    // Cuánto está escalado el lienzo en pantalla (el zoom propio de la aplicación usa transform:scale).
    function escalaPantalla(st) {
        const w = st.raiz.offsetWidth;
        return w ? (st.raiz.getBoundingClientRect().width / w) || 1 : 1;
    }

    // ── Líneas y cuadros que cruzan secciones ───────────────────────────────
    // Cada uno va de la Y de la sección donde empieza a la Y final de la sección donde termina. Solo se escribe lo
    // que cambió: así lo que se escribe no vuelve a disparar el observador.
    function ubicar(st) {
        const capa = st.raiz.querySelector('.lz-cruces');
        if (!capa) return;
        const g = st.gesto;
        const rr = st.raiz.getBoundingClientRect();
        const k = escalaPantalla(st);
        const e = st.op.escala;
        capa.querySelectorAll('[data-cruce]').forEach(el => {
            if (g && g.movido && g.el === el) return;                 // el que se está arrastrando
            const a = st.raiz.querySelector('[data-area="' + el.getAttribute('data-banda') + '"]');
            const b = st.raiz.querySelector('[data-area="' + el.getAttribute('data-banda-fin') + '"]');
            let v;
            if (!a || !b) v = { visibility: 'hidden' };
            else {
                const ra = a.getBoundingClientRect(), rb = b.getBoundingClientRect();
                const arriba = (ra.top - rr.top) / k + num(el, 'data-y') * e;
                const abajo = (rb.top - rr.top) / k + num(el, 'data-yf') * e;
                v = {
                    left: ((ra.left - rr.left) / k + num(el, 'data-x') * e).toFixed(1) + 'px', top: arriba.toFixed(1) + 'px',
                    width: (num(el, 'data-an') * e).toFixed(1) + 'px', height: Math.max(0, abajo - arriba).toFixed(1) + 'px', visibility: 'visible'
                };
            }
            const clave = JSON.stringify(v);
            if (el._lzPos === clave) return;
            el._lzPos = clave;
            Object.assign(el.style, v);
        });
    }
    function ubicarPronto(st) {
        if (st.ubicando) return;
        st.ubicando = requestAnimationFrame(() => { st.ubicando = null; try { ubicar(st); } catch (e) { } });
    }

    // La sección que está a esa altura de la pantalla (contando su borde de abajo); entre dos, la de arriba.
    function areaEn(st, y) {
        const areas = [...st.raiz.querySelectorAll('[data-area]')].map(a => ({ a, r: a.getBoundingClientRect() }));
        if (!areas.length) return null;
        return areas.find(x => y >= x.r.top - 0.5 && y <= x.r.bottom + 6)
            || areas.filter(x => x.r.top <= y).sort((p, q) => q.r.top - p.r.top)[0]
            || areas[0];
    }
    // La Y (mm) de un punto de la pantalla dentro de una sección, ajustada a la cuadrícula y sin salirse de ella.
    function yEn(st, d, y) {
        const v = ajustar(st, (y - d.r.top) / escalaPantalla(st) / st.op.escala);
        return Math.min(Math.max(0, v), num(d.a, 'data-alto'));
    }
    function datosCruce(el) {
        return {
            el, id: +el.getAttribute('data-cruce'), banda: +el.getAttribute('data-banda'), bandaFin: +el.getAttribute('data-banda-fin'),
            x: num(el, 'data-x'), y: num(el, 'data-y'), w: num(el, 'data-an'), h: num(el, 'data-al'), yf: num(el, 'data-yf')
        };
    }

    function iniciar(raiz, ref, op) {
        const st = { raiz, ref, op: op || { escala: 3.78, rejilla: 1, ajustar: true }, gesto: null };
        st.abajo = e => abajo(st, e);
        st.mover = e => mover(st, e);
        st.arriba = e => arriba(st, e);
        st.tecla = e => tecla(st, e);
        st.sobre = e => sobre(st, e);
        st.soltar = e => soltar(st, e);
        raiz.addEventListener('pointerdown', st.abajo);
        raiz.addEventListener('keydown', st.tecla);
        raiz.addEventListener('dragover', st.sobre);
        raiz.addEventListener('drop', st.soltar);
        // El alto: hasta la barra de estado, y otra vez cada vez que algo lo mueve.
        st.medir = () => { pedirAlto(st); ubicarPronto(st); };
        window.addEventListener('resize', st.medir);
        window.addEventListener('kon:zoom', st.medir);
        if (window.ResizeObserver) {
            st.observador = new ResizeObserver(st.medir);
            try { st.observador.observe(document.body); } catch (e) { }
            const lz = raiz.closest('.lz');
            if (lz) try { st.observador.observe(lz); } catch (e) { }
            try { st.observador.observe(raiz); } catch (e) { }
        }
        // Los cruces se vuelven a poner cada vez que Blazor cambia el lienzo (o un gesto mueve una sección).
        if (window.MutationObserver) {
            st.cambios = new MutationObserver(() => ubicarPronto(st));
            st.cambios.observe(raiz, {
                childList: true, subtree: true, attributes: true,
                attributeFilter: ['style', 'class', 'data-x', 'data-y', 'data-an', 'data-al', 'data-yf', 'data-banda', 'data-banda-fin', 'data-alto']
            });
        }
        estados.set(raiz, st);
        pedirAlto(st);
        ubicarPronto(st);
    }

    function opciones(raiz, op) {
        const st = estados.get(raiz);
        if (st) { st.op = op; pedirAlto(st); ubicarPronto(st); }
    }

    // ── Alto del lienzo ─────────────────────────────────────────────────────
    // Explorador, lienzo y propiedades llegan hasta el borde de arriba de la barra de estado, con el mismo respiro
    // de 8 px que las grillas (gridAutoAlto.js). Se descuenta lo que el panel de la pantalla tiene debajo del lienzo
    // (el relleno de la pestaña y el borde): si no, la pantalla quedaría unos píxeles más alta y aparecería una barra
    // de desplazamiento. Angosto (las propiedades van debajo), el alto vuelve a ser el natural.
    const RESPIRO = 8, ALTO_MIN = 420;

    function pedirAlto(st) {
        if (st.pendiente) return;
        st.pendiente = requestAnimationFrame(() => { st.pendiente = null; try { ajustarAlto(st); } catch (e) { } });
    }

    function ajustarAlto(st) {
        const cuerpo = st.raiz.closest('.lz-cuerpo');
        if (!cuerpo || cuerpo.offsetParent === null) return;          // pestaña oculta: todavía sin medidas
        if (window.matchMedia && window.matchMedia('(max-width: 1100px)').matches) {
            if (cuerpo.style.height) cuerpo.style.height = '';
            return;
        }
        const barra = document.querySelector('.status-bar');
        const tope = barra ? barra.getBoundingClientRect().top : window.innerHeight;
        // El zoom propio de la aplicación (transform:scale): las medidas llegan escaladas y el alto que se escribe se
        // vuelve a escalar, así que se divide.
        let escala = (window.appZoom && window.appZoom.currentScale) || 1;
        if (!(escala > 0)) escala = 1;
        const r = cuerpo.getBoundingClientRect();
        const panel = cuerpo.closest('.rep-panel');
        const debajo = panel ? Math.max(0, panel.getBoundingClientRect().bottom - r.bottom) : 0;
        // El tope como si la pantalla no estuviera desplazada (como en gridAutoAlto): si no, desplazarla cambiaría el alto.
        const desplazado = window.gridAutoAlto && window.gridAutoAlto.scrollDeAncestro ? window.gridAutoAlto.scrollDeAncestro(cuerpo) : 0;
        const alto = Math.max(ALTO_MIN, (tope - (r.top + desplazado) - debajo - RESPIRO) / escala);
        const nuevo = alto.toFixed(1) + 'px';
        // Solo si cambió: escribir lo mismo volvería a disparar el ResizeObserver.
        if (cuerpo.style.height !== nuevo) cuerpo.style.height = nuevo;
    }

    function destruir(raiz) {
        const st = estados.get(raiz);
        if (!st) return;
        raiz.removeEventListener('pointerdown', st.abajo);
        raiz.removeEventListener('keydown', st.tecla);
        raiz.removeEventListener('dragover', st.sobre);
        raiz.removeEventListener('drop', st.soltar);
        window.removeEventListener('pointermove', st.mover);
        window.removeEventListener('pointerup', st.arriba);
        window.removeEventListener('resize', st.medir);
        window.removeEventListener('kon:zoom', st.medir);
        if (st.observador) st.observador.disconnect();
        if (st.cambios) st.cambios.disconnect();
        if (st.pendiente) cancelAnimationFrame(st.pendiente);
        if (st.ubicando) cancelAnimationFrame(st.ubicando);
        estados.delete(raiz);
    }

    function enfocar(raiz) { if (raiz) raiz.focus({ preventScroll: true }); }

    // ── Empezar un gesto ────────────────────────────────────────────────────
    function abajo(st, e) {
        if (e.button !== 0) return;
        const t = e.target;
        const borde = t.closest('[data-borde]');
        const asa = t.closest('[data-asa]');
        const obj = t.closest('[data-obj]');
        const area = t.closest('[data-area]');
        const rotulo = t.closest('[data-rotulo]');
        const cruce = t.closest('[data-cruce]');
        const fin = t.closest('[data-cruce-fin]');
        enfocar(st.raiz);
        const base = { cx: e.clientX, cy: e.clientY, movido: false, aditivo: e.shiftKey || e.ctrlKey || e.metaKey };

        if (fin && cruce) {
            // El asa de fin de una línea o un cuadro que cruza: cambia dónde termina (también a otra sección).
            st.gesto = Object.assign(base, datosCruce(cruce), { tipo: 'fin', h0: parseFloat(cruce.style.height) || 0 });
        }
        else if (cruce) {
            // Se elige desde su borde, como un objeto, y se mueve entero (donde empieza y donde termina).
            const id = +cruce.getAttribute('data-cruce');
            let sel = seleccionados(st);
            const yaEstaba = sel.includes(id);
            if (base.aditivo) sel = yaEstaba ? sel.filter(x => x !== id) : sel.concat(id);
            else if (!yaEstaba) sel = [id];
            marcar(st, sel);
            llamar(st, 'Seleccionar', sel, null);
            if (!sel.includes(id)) return;
            st.gesto = Object.assign(base, datosCruce(cruce), {
                tipo: 'cruce', unico: !base.aditivo && yaEstaba && sel.length > 1,
                l0: parseFloat(cruce.style.left) || 0, t0: parseFloat(cruce.style.top) || 0, dx: 0, dy: 0
            });
        }
        else if (borde) {
            const id = +borde.getAttribute('data-borde');
            const a = st.raiz.querySelector('[data-area="' + id + '"]');
            if (!a) return;
            st.gesto = Object.assign(base, { tipo: 'alto', banda: id, area: a, h: num(a, 'data-alto') });
        }
        else if (asa && obj) {
            st.gesto = Object.assign(base, {
                tipo: 'estirar', el: obj, id: +obj.getAttribute('data-obj'), banda: +obj.getAttribute('data-banda'),
                dir: asa.getAttribute('data-asa'), linea: obj.getAttribute('data-tipo') === 'LIN',
                x: num(obj, 'data-x'), y: num(obj, 'data-y'), w: num(obj, 'data-an'), h: num(obj, 'data-al')
            });
        }
        else if (obj) {
            const id = +obj.getAttribute('data-obj');
            let sel = seleccionados(st);
            const yaEstaba = sel.includes(id);
            if (base.aditivo) sel = yaEstaba ? sel.filter(x => x !== id) : sel.concat(id);
            else if (!yaEstaba) sel = [id];
            marcar(st, sel);
            llamar(st, 'Seleccionar', sel, null);
            if (!sel.includes(id)) return;
            st.gesto = Object.assign(base, {
                tipo: 'mover', id, unico: !base.aditivo && yaEstaba && sel.length > 1,
                objs: sel.map(n => st.raiz.querySelector('[data-obj="' + n + '"]')).filter(Boolean).map(el => ({
                    el, id: +el.getAttribute('data-obj'), banda: +el.getAttribute('data-banda'),
                    x: num(el, 'data-x'), y: num(el, 'data-y'), w: num(el, 'data-an'), h: num(el, 'data-al')
                }))
            });
        }
        else if (area) {
            st.gesto = Object.assign(base, { tipo: 'recuadro', area, banda: +area.getAttribute('data-area') });
        }
        else if (rotulo) {
            marcar(st, []);
            llamar(st, 'Seleccionar', [], +rotulo.getAttribute('data-rotulo'));
            return;
        }
        else return;
        e.preventDefault();
        window.addEventListener('pointermove', st.mover);
        window.addEventListener('pointerup', st.arriba, { once: true });
    }

    // ── Durante el gesto ────────────────────────────────────────────────────
    function mover(st, e) {
        const g = st.gesto;
        if (!g) return;
        if (!g.movido && Math.abs(e.clientX - g.cx) + Math.abs(e.clientY - g.cy) < 3) return;
        if (!g.movido) {
            g.movido = true;
            st.raiz.classList.add('lz-moviendo');
            // Encima de las demás secciones mientras se arrastra (se guarda el orden que tenía para devolverlo).
            if (g.tipo === 'mover') g.objs.forEach(o => { o.z = o.el.style.zIndex; o.el.style.zIndex = '100000'; });
        }
        const dx = mm(st, e.clientX - g.cx), dy = mm(st, e.clientY - g.cy);
        switch (g.tipo) {
            case 'mover':
                for (const o of g.objs) {
                    o.nx = Math.max(0, r2(o.x + paso(st, dx)));
                    o.ny = r2(o.y + paso(st, dy));
                    o.el.style.left = px(st, o.nx);
                    o.el.style.top = px(st, o.ny);
                }
                break;
            case 'estirar': {
                const min = g.linea ? 0 : 1;
                let x = g.x, y = g.y, w = g.w, h = g.h;
                const d = g.dir;
                if (d.includes('e')) w = Math.max(min, g.w + paso(st, dx));
                if (d.includes('s')) h = Math.max(min, g.h + paso(st, dy));
                if (d.includes('w')) { x = Math.max(0, Math.min(g.x + paso(st, dx), g.x + g.w - min)); w = g.x + g.w - x; }
                if (d.includes('n')) { y = Math.max(0, Math.min(g.y + paso(st, dy), g.y + g.h - min)); h = g.y + g.h - y; }
                g.n = { x: Math.round(x * 100) / 100, y: Math.round(y * 100) / 100, w: Math.round(w * 100) / 100, h: Math.round(h * 100) / 100 };
                g.el.style.left = px(st, g.n.x); g.el.style.top = px(st, g.n.y);
                g.el.style.width = px(st, g.n.w); g.el.style.height = px(st, g.n.h);
                break;
            }
            case 'alto':
                g.nh = Math.min(400, Math.max(0, r2(g.h + paso(st, dy))));
                g.area.style.height = px(st, g.nh);
                break;
            case 'cruce':
                g.dx = Math.max(-g.x, paso(st, dx));
                g.dy = Math.max(-Math.min(g.y, g.yf), paso(st, dy));
                g.el.style.left = (g.l0 + g.dx * st.op.escala) + 'px';
                g.el.style.top = (g.t0 + g.dy * st.op.escala) + 'px';
                break;
            case 'fin':
                g.dy = paso(st, dy);
                g.el.style.height = Math.max(0, g.h0 + g.dy * st.op.escala) + 'px';
                break;
            case 'recuadro': {
                const r = g.area.getBoundingClientRect();
                const x1 = Math.min(g.cx, e.clientX) - r.left, y1 = Math.min(g.cy, e.clientY) - r.top;
                const x2 = Math.max(g.cx, e.clientX) - r.left, y2 = Math.max(g.cy, e.clientY) - r.top;
                if (!g.caja) {
                    g.caja = document.createElement('div');
                    g.caja.className = 'lz-recuadro';
                    g.area.appendChild(g.caja);
                }
                Object.assign(g.caja.style, { left: x1 + 'px', top: y1 + 'px', width: (x2 - x1) + 'px', height: (y2 - y1) + 'px' });
                g.fin = { x1, y1, x2, y2 };
                break;
            }
        }
    }

    // ── Fin del gesto: avisar a C# ──────────────────────────────────────────
    function arriba(st, e) {
        window.removeEventListener('pointermove', st.mover);
        const g = st.gesto;
        st.gesto = null;
        st.raiz.classList.remove('lz-moviendo');
        if (!g) return;
        if (g.tipo === 'mover') {
            if (g.movido) g.objs.forEach(o => { o.el.style.zIndex = o.z; });
            if (g.movido) {
                // Cada objeto queda en la sección donde cae su borde de arriba (o la más cercana).
                const areas = [...st.raiz.querySelectorAll('[data-area]')];
                const cambios = g.objs.map(o => {
                    const r = o.el.getBoundingClientRect();
                    let destino = areas.find(a => { const ra = a.getBoundingClientRect(); return r.top >= ra.top - 0.5 && r.top < ra.bottom; });
                    if (!destino) destino = areas.reduce((m, a) => Math.abs(a.getBoundingClientRect().top - r.top) < Math.abs(m.getBoundingClientRect().top - r.top) ? a : m, areas[0]);
                    const banda = +destino.getAttribute('data-area');
                    // En su misma sección baja lo que se movió; en otra, cae donde se soltó (ajustado a la cuadrícula).
                    const y = banda === o.banda ? Math.max(0, o.ny) : Math.max(0, ajustar(st, mm(st, r.top - destino.getBoundingClientRect().top)));
                    return { id: o.id, banda, x: o.nx, y, ancho: o.w, alto: o.h };
                });
                llamar(st, 'Geometria', cambios);
            }
            else if (g.unico) {
                marcar(st, [g.id]);
                llamar(st, 'Seleccionar', [g.id], null);
            }
        }
        else if (g.tipo === 'estirar' && g.movido) {
            const m = { id: g.id, banda: g.banda, x: g.n.x, y: g.n.y, ancho: g.n.w, alto: g.n.h };
            // Una línea o un cuadro estirado desde abajo hasta otra sección: dónde terminó (C# ve si puede cruzar).
            const tipo = g.el.getAttribute('data-tipo');
            if (g.dir.includes('s') && (tipo === 'LIN' || tipo === 'CUA')) {
                const fondo = g.el.getBoundingClientRect().bottom;
                const d = areaEn(st, fondo);
                if (d && +d.a.getAttribute('data-area') !== g.banda) { m.bandaFin = +d.a.getAttribute('data-area'); m.yFin = yEn(st, d, fondo); }
            }
            llamar(st, 'Geometria', [m]);
        }
        else if (g.tipo === 'cruce') {
            g.el._lzPos = null;
            if (g.movido) {
                llamar(st, 'Geometria', [{
                    id: g.id, banda: g.banda, x: r2(g.x + g.dx), y: r2(g.y + g.dy), ancho: g.w, alto: g.h, bandaFin: g.bandaFin, yFin: r2(g.yf + g.dy)
                }]);
            }
            else if (g.unico) {
                marcar(st, [g.id]);
                llamar(st, 'Seleccionar', [g.id], null);
            }
            ubicarPronto(st);
        }
        else if (g.tipo === 'fin') {
            g.el._lzPos = null;
            if (g.movido) {
                // Donde quedó el asa: en la misma sección final se mueve lo arrastrado; en otra, cae donde se soltó.
                const fondo = g.el.getBoundingClientRect().bottom;
                const d = areaEn(st, fondo);
                if (d) {
                    const banda = +d.a.getAttribute('data-area');
                    const yFin = banda === g.bandaFin ? Math.min(Math.max(0, r2(g.yf + g.dy)), num(d.a, 'data-alto')) : yEn(st, d, fondo);
                    llamar(st, 'Geometria', [{ id: g.id, banda: g.banda, x: g.x, y: g.y, ancho: g.w, alto: g.h, bandaFin: banda, yFin }]);
                }
            }
            ubicarPronto(st);
        }
        else if (g.tipo === 'alto' && g.movido) {
            llamar(st, 'AltoBanda', g.banda, g.nh);
        }
        else if (g.tipo === 'recuadro') {
            if (g.caja) g.caja.remove();
            if (!g.movido || !g.fin) {
                marcar(st, []);
                llamar(st, 'Seleccionar', [], g.banda);
                return;
            }
            // Los objetos de esa sección que toca el recuadro.
            const r = g.area.getBoundingClientRect();
            const ids = [...g.area.querySelectorAll('[data-obj]')].filter(el => {
                const o = el.getBoundingClientRect();
                const x1 = o.left - r.left, y1 = o.top - r.top, x2 = o.right - r.left, y2 = Math.max(o.bottom, o.top + 2) - r.top;
                return x1 < g.fin.x2 && x2 > g.fin.x1 && y1 < g.fin.y2 && y2 > g.fin.y1;
            }).map(el => +el.getAttribute('data-obj'));
            const sel = g.aditivo ? [...new Set(seleccionados(st).concat(ids))] : ids;
            marcar(st, sel);
            llamar(st, 'Seleccionar', sel, sel.length ? null : g.banda);
        }
    }

    // ── Teclado ─────────────────────────────────────────────────────────────
    function tecla(st, e) {
        if (e.target.closest('input,textarea,select,[contenteditable]')) return;
        const ctrl = e.ctrlKey || e.metaKey;
        const k = e.key;
        const simples = ['Delete', 'Backspace', 'ArrowLeft', 'ArrowRight', 'ArrowUp', 'ArrowDown', 'Escape'];
        if (simples.includes(k) || (ctrl && ['z', 'y', 'd', 'c', 'v', 'a'].includes(k.toLowerCase()))) {
            e.preventDefault();
            llamar(st, 'Tecla', k, e.shiftKey, ctrl);
        }
    }

    // ── Soltar lo arrastrado desde el explorador o la barra ────────────────
    function sobre(st, e) {
        if (!e.target.closest('[data-area]')) return;
        e.preventDefault();
        e.dataTransfer.dropEffect = 'copy';
    }

    function soltar(st, e) {
        const area = e.target.closest('[data-area]');
        if (!area) return;
        const valor = e.dataTransfer.getData('text/plain');
        if (!valor) return;
        e.preventDefault();
        const r = area.getBoundingClientRect();
        llamar(st, 'Soltar', valor, +area.getAttribute('data-area'),
            Math.max(0, ajustar(st, mm(st, e.clientX - r.left))), Math.max(0, ajustar(st, mm(st, e.clientY - r.top))));
        enfocar(st.raiz);
    }

    return { iniciar, opciones, destruir, enfocar };
})();

// Editor de fórmulas (Pages/Reportes/FormulaEditorDialog.razor): el texto se escribe en un <textarea> transparente
// encima de un <pre> con los colores; aquí se mantienen los dos (y los números de línea) en la misma posición al
// desplazarse, se inserta en el cursor lo que se elige de las listas y se lleva el cursor a la línea y columna de un error.
window.formulaEditor = (function () {
    function enlazar(ta, pre, lineas) {
        if (!ta || ta._feEnlazado) return;
        ta._feEnlazado = true;
        const seguir = function () {
            if (pre) { pre.scrollTop = ta.scrollTop; pre.scrollLeft = ta.scrollLeft; }
            if (lineas) lineas.scrollTop = ta.scrollTop;
        };
        ta.addEventListener('scroll', seguir);
        // Tab escribe espacios en vez de salir del cuadro.
        ta.addEventListener('keydown', function (e) {
            if (e.key !== 'Tab' || e.shiftKey || e.ctrlKey || e.altKey) return;
            e.preventDefault();
            insertar(ta, '    ', 0);
        });
        seguir();
    }

    // Inserta en el cursor (o en lugar de lo elegido) y deja el cursor "atras" caracteres antes del final de lo
    // insertado: Mayusculas() con atras = 1 queda entre los paréntesis. Avisa a Blazor con un evento input.
    function insertar(ta, texto, atras) {
        if (!ta) return;
        ta.focus();
        const ini = ta.selectionStart ?? ta.value.length;
        const fin = ta.selectionEnd ?? ini;
        ta.setRangeText(texto, ini, fin, 'end');
        const pos = Math.max(ini, ini + texto.length - (atras || 0));
        ta.setSelectionRange(pos, pos);
        ta.dispatchEvent(new Event('input', { bubbles: true }));
    }

    // Lleva el cursor a la línea y columna (desde 1) y elige el carácter que está ahí.
    function ir(ta, linea, columna) {
        if (!ta) return;
        const filas = ta.value.split('\n');
        let pos = 0;
        for (let i = 0; i < Math.min(linea - 1, filas.length); i++) pos += filas[i].length + 1;
        pos += Math.max(0, columna - 1);
        pos = Math.min(pos, ta.value.length);
        ta.focus();
        ta.setSelectionRange(pos, Math.min(pos + 1, ta.value.length));
        // Que la línea quede a la vista.
        const alto = parseFloat(getComputedStyle(ta).lineHeight) || 18;
        const y = (linea - 1) * alto;
        if (y < ta.scrollTop || y > ta.scrollTop + ta.clientHeight - alto) ta.scrollTop = Math.max(0, y - ta.clientHeight / 2);
    }

    return { enlazar, insertar, ir };
})();
