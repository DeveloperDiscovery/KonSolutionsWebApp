// Firefox no inicia un arrastre si el dragstart no deja datos en dataTransfer, y Blazor no llega a dataTransfer.
// Cualquier elemento marcado con data-arrastrable (las fichas del constructor de formulas) los deja aqui; el estado del arrastre lo lleva el componente.
document.addEventListener('dragstart', function (e) {
    var el = e.target && e.target.closest ? e.target.closest('[data-arrastrable]') : null;
    if (!el || !e.dataTransfer) return;
    try { e.dataTransfer.setData('text/plain', 'ficha'); e.dataTransfer.effectAllowed = 'move'; } catch (x) { }
}, true);
