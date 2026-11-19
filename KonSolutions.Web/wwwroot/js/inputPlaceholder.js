// inputPlaceholder — le pone placeholder=" " a los <input>/<textarea> de MudBlazor.
//
// POR QUÉ EXISTE
// La etiqueta de un campo outlined tiene dos posiciones: adentro cuando el campo está
// vacío y sin foco, y arriba sobre el borde el resto del tiempo. MudBlazor pasa de una a
// otra con un transform que incluye un scale(), así que la etiqueta de arriba se RENDERIZA
// más chica que el font-size que declara: dos campos con el mismo tamaño configurado se
// ven distinto según tengan dato o no.
//
// Para corregirlo hay que poder distinguir los dos estados desde CSS. El foco se detecta
// con :focus-within, que ya funciona. Lo que faltaba era saber si el campo está vacío, y
// para eso el selector natural es :placeholder-shown... que solo matchea si el <input>
// tiene el atributo placeholder. MudBlazor no lo pone (verificado en el DOM real), y por
// eso en app.css había un comentario diciendo que ese selector "nunca coincidía".
//
// Un espacio alcanza: no se ve nada en pantalla, pero el atributo existe y
// :placeholder-shown pasa a ser un detector fiable de "vacío". Es exactamente lo que ya
// hacía AriaCombo.razor (placeholder=" ") y por lo que su etiqueta siempre se vio bien.
//
// POR QUÉ ACÁ Y NO EN CADA PANTALLA
// Son ~295 usos de MudTextField/MudNumericField/MudDatePicker repartidos por toda la app.
// Editarlos uno por uno sería frágil (cualquier control nuevo nacería sin el atributo).
// Acá se resuelve en un solo lugar y vale también para lo que se agregue después.
window.inputPlaceholder = {
    // Solo los campos con etiqueta flotante. Un input sin <label> no tiene el problema.
    SELECTOR: '.mud-input-outlined-with-label input, .mud-input-outlined-with-label textarea',

    _pendiente: null,

    aplicar: function () {
        var nodos = document.querySelectorAll(this.SELECTOR);
        for (var i = 0; i < nodos.length; i++) {
            var el = nodos[i];
            // Respeta cualquier placeholder real ya definido (ej. "Buscar opción...").
            if (!el.hasAttribute('placeholder')) el.setAttribute('placeholder', ' ');
        }
        return nodos.length;
    },

    // Se relee TODO el documento en vez de mirar solo los nodos agregados en cada
    // mutación. La versión anterior hacía nodo.querySelectorAll(...), que busca solo entre
    // los DESCENDIENTES: cuando Blazor insertaba el <input> como nodo raíz de la mutación
    // —cosa que hace seguido al re-renderizar un campo suelto— ese input nunca recibía el
    // atributo y su etiqueta quedaba mal posicionada. Un re-escaneo completo no tiene ese
    // punto ciego; para que no cueste, va coalescido a un frame (igual criterio que
    // gridAutoAlto.pedirRecalculo).
    pedirRepaso: function () {
        var self = this;
        if (this._pendiente) return;
        this._pendiente = requestAnimationFrame(function () {
            self._pendiente = null;
            try { self.aplicar(); } catch (e) { /* nunca tumbar la UI por esto */ }
        });
    },

    iniciar: function () {
        if (this._observador) return;
        var self = this;
        this.aplicar();

        // Blazor pinta y repinta el DOM continuamente (cambiar de pestaña, abrir un
        // diálogo, agregar una fila a una grilla editable). Sin observar, esos controles
        // nacerían sin el atributo.
        this._observador = new MutationObserver(function () { self.pedirRepaso(); });
        this._observador.observe(document.body, { childList: true, subtree: true });
    },

    // Ayuda de diagnóstico: escribir inputPlaceholder.diagnostico() en la consola (F12)
    // dice cuántos campos hay y cuántos tienen el atributo. Si el segundo número es 0,
    // el problema es este script; si coinciden, el problema está en el CSS.
    diagnostico: function () {
        var todos = document.querySelectorAll(this.SELECTOR);
        var con = 0;
        for (var i = 0; i < todos.length; i++) if (todos[i].hasAttribute('placeholder')) con++;
        console.log('[inputPlaceholder] campos con etiqueta flotante: ' + todos.length +
                    ' | con atributo placeholder: ' + con);
        return { total: todos.length, conPlaceholder: con };
    }
};

// Blazor WASM arranca DESPUÉS de DOMContentLoaded, así que en ese momento todavía no hay
// ningún input en pantalla: la pasada inicial normalmente no encuentra nada y el trabajo
// real lo hace el MutationObserver. Se llama igual por si el script se carga tarde.
document.addEventListener('DOMContentLoaded', function () {
    try { window.inputPlaceholder.iniciar(); } catch (e) { /* nunca tumbar la app por esto */ }
});
