/*
    autoFocus.js
    --------------------------------------------------------------------------
    Al abrir un diálogo, cambiar de pestaña del Workspace, o cambiar de
    sub-pestaña DENTRO de una pantalla (ej. "REQUERIMIENTOS"/"NUEVO" en
    Requerimientos.razor), pone el foco en el primer input habilitado — así
    el usuario puede empezar a escribir sin tener que hacer clic primero.

    Por qué en JS y no tocando cada .razor: agregar esto pantalla por pantalla
    (y diálogo por diálogo) implicaría editar decenas de archivos, uno por
    uno, con el mismo riesgo de olvidarse alguno cada vez que se crea uno
    nuevo. Mismo patrón de MutationObserver que gridColumnas.js: se observa
    el body completo una sola vez, sin que cada pantalla llame a nada.

    Cómo decide CUÁNDO reenfocar (importante, dos intentos anteriores fallaron
    acá): en vez de vigilar contenedores puntuales (.mud-dialog, .ws-tab-pane),
    que dejaban afuera casos como un <MudTabs> embebido dentro de una misma
    pantalla (el contenedor exterior no cambia, solo su contenido interno), se
    usa un criterio más simple y general: cada vez que el DOM cambia de forma
    relevante, si el foco actual "se perdió" (volvió a <body>, típicamente
    porque el elemento que lo tenía fue removido al cambiar de vista), se
    busca el primer campo habilitado y visible en TODO el documento y se le
    da foco. Si el usuario está escribiendo en algún campo, ese campo sigue
    siendo el activeElement y no se lo interrumpe.
*/
(function () {
    'use strict';

    const SELECTOR_FOCUSABLE = [
        'input:not([type="hidden"])',
        'textarea',
        'select',
        '[role="combobox"]'
    ].join(',');

    function esVisible(el) {
        return !!(el.offsetWidth || el.offsetHeight || el.getClientRects().length);
    }

    // El atributo/propiedad disabled ya alcanza para <input>/<textarea>/<select> (un
    // elemento realmente disabled no puede recibir foco ni con .focus()), pero se
    // refuerza igual para combos custom (AriaCombo usa [role="combobox"] sobre un
    // <input>, que si está deshabilitado ya cae en el chequeo de arriba) y por si
    // algún componente bloquea solo visualmente sin disabled/readonly reales.
    function estaDeshabilitado(el) {
        if (el.disabled || el.readOnly) return true;
        if (el.getAttribute('aria-disabled') === 'true') return true;
        if (el.closest('.mud-disabled, [aria-disabled="true"]')) return true;
        if (getComputedStyle(el).pointerEvents === 'none') return true;
        return false;
    }

    function primerCandidato(root) {
        const candidatos = root.querySelectorAll(SELECTOR_FOCUSABLE);
        for (const el of candidatos) {
            if (estaDeshabilitado(el)) continue;
            if (!esVisible(el)) continue;
            if (el.closest('[aria-hidden="true"]')) continue;
            return el;
        }
        return null;
    }

    // No alcanza con "activeElement === body": al hacer clic en una pestaña interna
    // (ej. el botón "NUEVO" de Requerimientos.razor) el foco queda en ESE botón, no en
    // body, y sin embargo la vista cambió por completo y hay que reenfocar igual. Se
    // distingue en cambio si lo que tiene el foco es un campo donde el usuario podría
    // estar escribiendo algo (texto, combo, contenteditable) — ahí sí no se lo molesta.
    function usuarioEscribiendo() {
        const a = document.activeElement;
        if (!a || a === document.body) return false;
        const tag = a.tagName;
        if (tag === 'INPUT' || tag === 'TEXTAREA' || tag === 'SELECT') return true;
        if (a.getAttribute && a.getAttribute('role') === 'combobox') return true;
        if (a.isContentEditable) return true;
        return false;
    }

    function intentarEnfocar() {
        if (usuarioEscribiendo()) return; // el usuario ya está en algún campo — no molestar
        const el = primerCandidato(document);
        if (!el) return;
        try { el.focus({ preventScroll: false }); } catch (e) { try { el.focus(); } catch (e2) { /* nada más que hacer */ } }
    }

    // Blazor a veces pinta un campo sin su estado final (disabled, etc.) un instante
    // antes de estabilizarse — un solo intento inmediato podía enfocar de más o de
    // menos. Se reintenta una vez más con espera real (no frames) para cubrir ese
    // margen. OJO: estos reintentos vuelven a chequear usuarioEscribiendo() en el
    // momento en que disparan, así que si para entonces el usuario ya empezó a
    // escribir, no lo interrumpen.
    function intentarEnfocarConReintentos() {
        intentarEnfocar();
        setTimeout(intentarEnfocar, 150);
    }

    // OJO — versión anterior también vigilaba atributos (style/class/hidden) en
    // cualquier elemento, no solo altas/bajas de nodos: MudBlazor cambia la clase
    // del label flotante (mud-shrink) en CADA tecla que se escribe, así que esa
    // versión terminaba re-disparando el intento de foco en cada letra tipeada y
    // le robaba el cursor al campo mientras el usuario escribía. Un cambio real de
    // vista (abrir un diálogo, cambiar de pestaña) siempre agrega/quita nodos del
    // DOM — así que alcanza con mirar childList, sin necesidad de mirar atributos.
    let pendiente = null;
    const observador = new MutationObserver(muts => {
        const huboNodosNuevos = muts.some(m => m.type === 'childList' && m.addedNodes.length > 0);
        if (!huboNodosNuevos) return;
        clearTimeout(pendiente);
        pendiente = setTimeout(intentarEnfocarConReintentos, 30);
    });

    function iniciar() {
        intentarEnfocarConReintentos();
        observador.observe(document.body, { childList: true, subtree: true });
    }

    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', iniciar);
    else iniciar();
})();
