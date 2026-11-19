// Gantt del expediente de proyectos: arrastrar para mover, y arrastrar los bordes para cambiar inicio o fin.
// El dibujo lo hace Blazor (barras con left/width en pixeles = dias x px por dia); este archivo solo atiende el arrastre.
// Al soltar llama a OnGanttCambio(tipo, cod, inicio, fin) del componente, que guarda en la base; si responde false (error),
// la barra vuelve a donde estaba. Respeta el zoom de la app (appZoom.js usa transform: scale): el desplazamiento se divide por la escala.
window.proyectoGantt = {
    _estado: new WeakMap(),

    init: function (wrap, dotnet) {
        if (!wrap || this._estado.has(wrap)) return;
        var st = { dotnet: dotnet, drag: null, tip: null };
        this._estado.set(wrap, st);
        var DIA = 86400000;
        function parse(iso) { if (!iso) return null; var p = iso.split('-'); return Date.UTC(+p[0], +p[1] - 1, +p[2]); }
        function fmt(ms) { return ms == null ? null : new Date(ms).toISOString().slice(0, 10); }
        function ver(ms) { var d = new Date(ms); return ('0' + d.getUTCDate()).slice(-2) + '/' + ('0' + (d.getUTCMonth() + 1)).slice(-2) + '/' + d.getUTCFullYear(); }

        function tip(texto, x, y) {
            if (!st.tip) { st.tip = document.createElement('div'); st.tip.className = 'gantt-tip'; document.body.appendChild(st.tip); }
            st.tip.textContent = texto; st.tip.style.left = (x + 14) + 'px'; st.tip.style.top = (y + 14) + 'px';
        }
        function quitaTip() { if (st.tip) { st.tip.remove(); st.tip = null; } }

        var yo = this;
        var tm = setInterval(function () { if (!yo.fit(wrap)) clearInterval(tm); }, 500);
        yo.fit(wrap);
        // ancho de la columna Fase / tarea / hito: se arrastra el borde derecho de su cabecera (se recuerda en este navegador)
        try { var gw = parseFloat(localStorage.getItem('pgLeftW')); if (gw >= 380 && gw <= 1400) wrap.style.setProperty('--lw', gw + 'px'); } catch (x) { }
        var rs = null;
        wrap.addEventListener('pointerdown', function (e) {
            var h = e.target.closest ? e.target.closest('.pg-lres') : null;
            if (!h || e.button !== 0) return;
            var l = wrap.querySelector('.pg-left');
            var rect = wrap.getBoundingClientRect();
            rs = { x0: e.clientX, w0: l.offsetWidth, scale: (wrap.offsetWidth ? rect.width / wrap.offsetWidth : 1) || 1, h: h, id: e.pointerId };
            try { h.setPointerCapture(e.pointerId); } catch (x) { }
            e.preventDefault(); e.stopPropagation();
        }, true);
        wrap.addEventListener('pointermove', function (e) {
            if (!rs) return;
            var w = Math.max(380, Math.min(1400, Math.round(rs.w0 + (e.clientX - rs.x0) / rs.scale)));
            wrap.style.setProperty('--lw', w + 'px');
        });
        function finRes() { if (!rs) return; try { localStorage.setItem('pgLeftW', String(parseFloat(wrap.style.getPropertyValue('--lw')) || '')); } catch (x) { } rs = null; }
        wrap.addEventListener('dblclick', function (e) { if (e.target.closest && e.target.closest('.pg-lres')) { wrap.style.removeProperty('--lw'); try { localStorage.removeItem('pgLeftW'); } catch (x) { } } });
        wrap.addEventListener('pointerup', finRes);
        wrap.addEventListener('pointercancel', finRes);

        wrap.addEventListener('pointerdown', function (e) {
            var bar = e.target.closest('[data-gdrag]');
            if (!bar || e.button !== 0 || bar.dataset.ro === '1') return;
            var mode = e.target.classList.contains('gh-l') ? 'l' : e.target.classList.contains('gh-r') ? 'r' : 'm';
            if (bar.dataset.tipo === 'HIT' && mode !== 'm') return;
            var rect = wrap.getBoundingClientRect();
            var scale = (wrap.offsetWidth ? rect.width / wrap.offsetWidth : 1) || 1;
            st.drag = {
                bar: bar, mode: mode, x0: e.clientX, scale: scale, px: parseFloat(wrap.dataset.px) || 10,
                ini: parse(bar.dataset.ini), fin: parse(bar.dataset.fin),
                left0: parseFloat(bar.style.left) || 0, w0: parseFloat(bar.style.width) || 0, dd: 0
            };
            try { bar.setPointerCapture(e.pointerId); } catch (x) { }
            bar.classList.add('gdrag');
            e.preventDefault();
        });

        wrap.addEventListener('pointermove', function (e) {
            var d = st.drag; if (!d) return;
            var dd = Math.round((e.clientX - d.x0) / d.scale / d.px);
            if (d.mode === 'l') dd = Math.min(dd, Math.round((d.w0 - d.px) / d.px));
            if (d.mode === 'r') dd = Math.max(dd, -Math.round((d.w0 - d.px) / d.px));
            d.dd = dd;
            if (d.mode === 'm') d.bar.style.left = (d.left0 + dd * d.px) + 'px';
            if (d.mode === 'l') { d.bar.style.left = (d.left0 + dd * d.px) + 'px'; d.bar.style.width = (d.w0 - dd * d.px) + 'px'; }
            if (d.mode === 'r') d.bar.style.width = (d.w0 + dd * d.px) + 'px';
            var ni = d.ini, nf = d.fin;
            if (d.mode === 'm') { if (ni != null) ni += dd * DIA; nf += dd * DIA; }
            if (d.mode === 'l') ni += dd * DIA;
            if (d.mode === 'r') nf += dd * DIA;
            tip(ni != null ? ver(ni) + '  \u2192  ' + ver(nf) + '  \u00b7  ' + (Math.round((nf - ni) / DIA) + 1) + ' d' : ver(nf), e.clientX, e.clientY);
        });

        function termina(e, cancelar) {
            var d = st.drag; if (!d) return;
            st.drag = null; quitaTip();
            d.bar.classList.remove('gdrag');
            try { d.bar.releasePointerCapture(e.pointerId); } catch (x) { }
            function restaurar() { d.bar.style.left = d.left0 + 'px'; d.bar.style.width = d.w0 + 'px'; }
            if (cancelar || d.dd === 0) { restaurar(); return; }
            var ni = d.ini, nf = d.fin;
            if (d.mode === 'm') { if (ni != null) ni += d.dd * DIA; nf += d.dd * DIA; }
            if (d.mode === 'l') ni += d.dd * DIA;
            if (d.mode === 'r') nf += d.dd * DIA;
            d.bar.classList.add('gsave');
            st.dotnet.invokeMethodAsync('OnGanttCambio', d.bar.dataset.tipo, parseInt(d.bar.dataset.cod, 10), fmt(ni), fmt(nf), d.mode === 'm' ? 'M' : 'R')
                .then(function (ok) { d.bar.classList.remove('gsave'); if (!ok) restaurar(); })
                .catch(function () { d.bar.classList.remove('gsave'); restaurar(); });
        }
        wrap.addEventListener('pointerup', function (e) { termina(e, false); });
        wrap.addEventListener('pointercancel', function (e) { termina(e, true); });
        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape' && st.drag) { var d = st.drag; st.drag = null; quitaTip(); d.bar.classList.remove('gdrag'); d.bar.style.left = d.left0 + 'px'; d.bar.style.width = d.w0 + 'px'; }
        });
    },

    // El alto del Gantt llega hasta la barra de estado (descontando la leyenda de abajo). Se revisa cada medio segundo:
    // asi se ajusta solo al ocultar los indicadores, cambiar el tamano de la ventana o el zoom de la app.
    fit: function (wrap) {
        if (!wrap || !wrap.isConnected) return false;
        var sb = document.querySelector('.status-bar');
        var r = wrap.getBoundingClientRect();
        if (!r.width) return true;
        var s = (wrap.offsetWidth ? r.width / wrap.offsetWidth : 1) || 1;
        var limite = sb ? sb.getBoundingClientRect().top : window.innerHeight;
        var h = Math.floor((limite - r.top) / s - 52); // 52 px: leyenda de abajo y aire
        if (h < 240) h = 240;
        var v = h + 'px';
        if (wrap.style.maxHeight !== v) wrap.style.maxHeight = v;
        return true;
    },

    // ancho disponible para la parte de fechas (ancho del contenedor menos la columna de la izquierda)
    ancho: function (wrap) {
        var l = wrap ? wrap.querySelector('.pg-left') : null;
        return wrap ? wrap.clientWidth - (l ? l.offsetWidth : 0) : 0;
    },

    foco: function (id) { setTimeout(function () { var e = document.getElementById(id); if (e) { e.focus(); e.select(); } }, 30); },

    dispose: function (wrap) { if (wrap) this._estado.delete(wrap); }
};
