(function () {
    'use strict';

    function isSpanishUi() {
        var lang = (document.documentElement.getAttribute('lang') || '').toLowerCase();
        return lang.indexOf('es') === 0;
    }

    function spanishDefaults() {
        return {
            library: 'Biblioteca de fotos',
            camera: 'Tomar foto',
            files: 'Elegir archivos'
        };
    }

    function englishDefaults() {
        return {
            library: 'Photo Library',
            camera: 'Take Photo',
            files: 'Choose Files'
        };
    }

    function labels() {
        var fallback = isSpanishUi() ? spanishDefaults() : englishDefaults();
        var injected = window.IndorFileSourceLabels;
        if (!injected) {
            return fallback;
        }

        // Guard: Spanish UI must never keep English sheet labels (stale inject / race).
        if (isSpanishUi()
            && (injected.library === 'Photo Library'
                || injected.camera === 'Take Photo'
                || injected.files === 'Choose Files')) {
            return fallback;
        }

        return {
            library: injected.library || fallback.library,
            camera: injected.camera || fallback.camera,
            files: injected.files || fallback.files
        };
    }

    function applyLabels(root) {
        var map = labels();
        var scope = root || document;
        scope.querySelectorAll('[data-indor-file-source], [data-pa-file-source], [data-nr-photo-source]').forEach(function (item) {
            var source = item.getAttribute('data-indor-file-source')
                || item.getAttribute('data-pa-file-source')
                || item.getAttribute('data-nr-photo-source');
            var text = source === 'camera' ? map.camera
                : (source === 'files' ? map.files : map.library);
            if (!text) {
                return;
            }
            var labelEl = item.querySelector('.indor-file-source-label');
            if (labelEl) {
                labelEl.textContent = text;
            }
        });
    }

    function openFileInput(input) {
        if (!input) {
            return;
        }
        var prev = input.style.pointerEvents;
        input.style.pointerEvents = 'auto';
        try {
            input.value = '';
            input.click();
        } finally {
            input.style.pointerEvents = prev || '';
        }
    }

    function mergeIntoTarget(target, fileList) {
        if (!target || !fileList || !fileList.length || typeof DataTransfer === 'undefined') {
            return;
        }
        var dt = new DataTransfer();
        if (target.files) {
            Array.prototype.forEach.call(target.files, function (f) { dt.items.add(f); });
        }
        Array.prototype.forEach.call(fileList, function (f) { dt.items.add(f); });
        target.files = dt.files;
        target.dispatchEvent(new Event('change', { bubbles: true }));
    }

    function initChooser(root) {
        if (!root || root.getAttribute('data-indor-file-source-ready') === '1') {
            return;
        }
        root.setAttribute('data-indor-file-source-ready', '1');
        applyLabels(root);

        var btn = root.querySelector('.pa-file-source-btn, .indor-file-source-btn, .nr-photo-add, .pa-media-photo-btn');
        var menu = root.querySelector('.pa-file-source-menu, .indor-file-source-menu, .nr-photo-menu, .pa-media-photo-menu');
        var cameraInput = root.querySelector('.pa-file-source-input--camera, .indor-file-source-input--camera, .pa-media-photo-input--camera');
        var libraryInput = root.querySelector('.pa-file-source-input--library, .indor-file-source-input--library, .pa-media-photo-input--library');
        var filesInput = root.querySelector('.pa-file-source-input--files, .indor-file-source-input--files, .pa-media-photo-input--files');
        var targetInput = root.querySelector('[data-pa-file-target], [data-indor-file-target]') || filesInput;

        // Only wire chooser roots that opt in — media-capture / NR keep their own click handlers.
        if (!root.hasAttribute('data-pa-file-source-chooser') && !root.hasAttribute('data-indor-file-source-chooser')) {
            return;
        }

        if (!btn || !menu) {
            return;
        }

        function setMenuOpen(open) {
            menu.hidden = !open;
            btn.setAttribute('aria-expanded', open ? 'true' : 'false');
        }

        function closeMenu() {
            setMenuOpen(false);
        }

        btn.addEventListener('click', function (e) {
            e.preventDefault();
            e.stopPropagation();
            setMenuOpen(menu.hidden);
        });

        root.querySelectorAll('[data-indor-file-source], [data-pa-file-source]').forEach(function (item) {
            item.addEventListener('click', function (e) {
                e.preventDefault();
                e.stopPropagation();
                var source = item.getAttribute('data-indor-file-source')
                    || item.getAttribute('data-pa-file-source');
                closeMenu();
                if (source === 'camera') {
                    openFileInput(cameraInput);
                } else if (source === 'library') {
                    if (libraryInput) {
                        libraryInput.removeAttribute('capture');
                    }
                    openFileInput(libraryInput);
                } else {
                    openFileInput(filesInput);
                }
            });
        });

        document.addEventListener('click', function (e) {
            if (!root.contains(e.target)) {
                closeMenu();
            }
        });
        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape') {
                closeMenu();
            }
        });

        [cameraInput, libraryInput].forEach(function (input) {
            if (!input || input === targetInput) {
                return;
            }
            input.addEventListener('change', function () {
                mergeIntoTarget(targetInput, input.files);
            });
        });
    }

    function boot() {
        applyLabels(document);
        document.querySelectorAll('[data-pa-file-source-chooser], [data-indor-file-source-chooser]').forEach(initChooser);
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', boot);
    } else {
        boot();
    }

    window.IndorFileSourceChooser = {
        initAll: boot,
        applyLabels: applyLabels
    };
})();
