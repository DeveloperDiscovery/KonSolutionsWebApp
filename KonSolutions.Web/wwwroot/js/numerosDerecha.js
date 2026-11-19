// Alineación a la derecha de TODO dato numérico en las grillas.
//
// POR QUÉ CON JS Y NO CON CSS
// El CSS no puede saber si una columna es numérica: en el DOM una celda con "1.250,00" y
// otra con "ORDCM" son el mismo <td>. Hasta ahora se resolvía a mano, columna por columna,
// con CellStyle="text-align:right" — hay 79 escritos así en el proyecto y cada grilla nueva
// nace con el problema otra vez.
//
// Acá se mira el CONTENIDO: por cada columna se leen sus celdas y, si TODAS las que tienen
// algo son números, se alinea la columna entera —celdas y encabezado—. La decisión es por
// columna y no por celda a propósito: alinear una celda sí y la de al lado no sería peor
// que no alinear nada.
//
// QUÉ NO SE TOCA, Y POR QUÉ
//   · Columnas con ceros a la izquierda ("00030"): son códigos, no cantidades. Un código
//     se lee de izquierda a derecha como un texto.
//   · Fechas y horas.
//   · Columnas vacías o con una sola fila: no alcanza para decidir; se dejan como están.
//   · Grillas marcadas con data-sin-alinear-numeros.
//
// Si alguna columna queda mal clasificada, el arreglo es el de siempre: ponerle su
// CellStyle explícito, que manda sobre esto.
window.numerosDerecha = {
    MAX_FILAS: 60,       // con eso alcanza para decidir; recorrer mil filas no cambia nada
    _pendiente: null,

    // ¿Es un número tal como lo escribe la aplicación? Se aceptan separadores de miles y
    // decimales de cualquiera de las dos convenciones, el signo, los paréntesis de negativo
    // contable y el porcentaje.
    esNumero: function (txt) {
        var t = (txt || '').trim();
        if (!t) return null;                                  // vacío: no decide
        if (/^0\d/.test(t.replace(/[^\d]/g, '')) && /^[\d]+$/.test(t)) return false; // código con ceros
        if (/[/]/.test(t)) return false;                      // fecha
        if (/^\d{1,2}:\d{2}/.test(t)) return false;           // hora
        return /^[(\-+]?\s*[\d.,\s]+\)?\s*%?$/.test(t) && /\d/.test(t);
    },

    aplicar: function () {
        var self = this;
        // La tabla de una grilla es .mud-table-root; .mud-table es el envoltorio de MudTable.
        // Apuntar sólo a .mud-table no coincidía con ninguna grilla del proyecto.
        document.querySelectorAll(
            'table.mud-table-root:not([data-sin-alinear-numeros]), table.mud-table:not([data-sin-alinear-numeros])'
        ).forEach(function (tabla) {
            var filas = [...tabla.querySelectorAll('tbody tr')].slice(0, self.MAX_FILAS);
            if (filas.length < 2) return;                     // una sola fila no alcanza

            var columnas = filas[0].children.length;
            for (var c = 0; c < columnas; c++) {
                var hayDato = false, todosNumero = true;

                for (var f = 0; f < filas.length && todosNumero; f++) {
                    var celda = filas[f].children[c];
                    if (!celda) continue;
                    // Una celda con un control adentro (editable) se juzga por su valor.
                    var campo = celda.querySelector('input');
                    var r = self.esNumero(campo ? campo.value : celda.textContent);
                    if (r === null) continue;                 // vacía
                    hayDato = true;
                    if (r === false) todosNumero = false;
                }

                if (!hayDato || !todosNumero) continue;

                // SÓLO LAS CELDAS. El encabezado va centrado en todas las grillas, por
                // regla general (ver app.css), así que acá no se toca: si esto lo alineara
                // a la derecha, las columnas numéricas tendrían el título de un lado y el
                // resto del otro.
                //
                // Se escribe en el style propio y no con una clase para que gane sobre los
                // estilos en línea que MudBlazor pone en algunas celdas.
                [...tabla.querySelectorAll('tbody tr')].forEach(function (fila) {
                    var celda = fila.children[c];
                    if (!celda) return;
                    if (celda.style.textAlign !== 'right') celda.style.textAlign = 'right';
                    var campo = celda.querySelector('input');
                    if (campo && campo.style.textAlign !== 'right') campo.style.textAlign = 'right';
                });
            }
        });
    },

    pedir: function () {
        var self = this;
        if (this._pendiente) return;
        this._pendiente = requestAnimationFrame(function () {
            self._pendiente = null;
            try { self.aplicar(); } catch (e) { /* nunca tumbar la UI por el formato */ }
        });
    },

    iniciar: function () {
        var pedir = this.pedir.bind(this);
        if (window.MutationObserver) {
            try { new MutationObserver(pedir).observe(document.body, { childList: true, subtree: true }); }
            catch (e) { }
        }
        pedir();
    }
};

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', function () { window.numerosDerecha.iniciar(); });
} else {
    window.numerosDerecha.iniciar();
}
