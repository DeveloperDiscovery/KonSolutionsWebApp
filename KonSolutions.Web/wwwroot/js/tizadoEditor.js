// Ajuste manual de un tizado (DisenosTizadoEditorDialog.razor).
//
// Modulo ES cargado con import() por el componente (como planoEditor.js): no hay que tocar index.html.
// El <svg> del tizado se escala con CSS (width en %) y vive dentro de un contenedor con scroll, asi que
// OffsetX/OffsetY de Blazor no sirven; getScreenCTM() pasa de pantalla a las unidades del viewBox (mm).

// Posicion del puntero en las unidades del viewBox (mm de la tela), como [x, y].
export function puntoSvg(svg, clientX, clientY) {
    if (!svg || typeof svg.getScreenCTM !== 'function') return [0, 0];
    const ctm = svg.getScreenCTM();
    if (!ctm) return [0, 0];
    const p = (typeof svg.createSVGPoint === 'function') ? svg.createSVGPoint() : null;
    const local = p
        ? (() => { p.x = clientX; p.y = clientY; return p.matrixTransform(ctm.inverse()); })()
        : new DOMPoint(clientX, clientY).matrixTransform(ctm.inverse());
    return [local.x, local.y];
}

// Mientras se arrastra, el puntero se sale del dibujo con facilidad: la captura mantiene los eventos hasta soltar.
export function capturarPuntero(el, pointerId) {
    try { el?.setPointerCapture?.(pointerId); } catch { /* el puntero ya se solto */ }
}

export function liberarPuntero(el, pointerId) {
    try { if (el?.hasPointerCapture?.(pointerId)) el.releasePointerCapture(pointerId); } catch { /* ya liberado */ }
}
