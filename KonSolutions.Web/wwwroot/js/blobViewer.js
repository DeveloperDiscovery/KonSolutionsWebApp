// Crea una blob: URL a partir de bytes descargados con fetch autenticado (HttpClient),
// para poder mostrarlos en un <iframe> sin depender de rutas UNC ni de Mixed Content.
window.blobViewer = {
    _urls: [],
    crear: function (bytes, contentType) {
        const blob = new Blob([new Uint8Array(bytes)], { type: contentType || 'application/pdf' });
        const url = URL.createObjectURL(blob);
        this._urls.push(url);
        return url;
    },
    liberar: function (url) {
        try { URL.revokeObjectURL(url); } catch (e) { }
        this._urls = this._urls.filter(u => u !== url);
    },
    // Descarga un archivo generado en la API (reportes): bytes + nombre + tipo.
    descargar: function (bytes, nombre, contentType) {
        const blob = new Blob([new Uint8Array(bytes)], { type: contentType || 'application/octet-stream' });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = nombre || 'reporte';
        document.body.appendChild(a);
        a.click();
        a.remove();
        setTimeout(() => URL.revokeObjectURL(url), 10000);
    },
    liberarTodo: function () {
        this._urls.forEach(u => { try { URL.revokeObjectURL(u); } catch (e) { } });
        this._urls = [];
    }
};
