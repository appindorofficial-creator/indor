/* Compatibility shim — prefer indor-file-source-chooser.js (loaded globally). */
(function () {
    if (window.IndorFileSourceChooser && typeof window.IndorFileSourceChooser.initAll === 'function') {
        window.IndorFileSourceChooser.initAll();
        return;
    }
    document.querySelectorAll('[data-pa-file-source-chooser]').forEach(function () { /* no-op until global loads */ });
})();
