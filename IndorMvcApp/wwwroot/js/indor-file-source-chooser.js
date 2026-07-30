(function () {
    'use strict';

    function isSpanishUi() {
        var lang = (document.documentElement.getAttribute('lang') || '').toLowerCase();
        if (lang.indexOf('es') === 0) {
            return true;
        }

        // Cookie fallback when <html lang> is missing/stale in WebView.
        try {
            var match = document.cookie.match(/(?:^|; )\s*\.Indor\.UiCulture=([^;]*)/);
            if (match) {
                var culture = decodeURIComponent(match[1] || '').toLowerCase();
                if (culture.indexOf('es') === 0 || culture.indexOf('|es') >= 0 || culture.indexOf('=es') >= 0) {
                    return true;
                }
            }
        } catch (e) { /* ignore */ }

        // If server already injected Spanish sheet labels, treat UI as Spanish.
        var injected = window.IndorFileSourceLabels;
        if (injected) {
            var blob = ((injected.library || '') + ' ' + (injected.camera || '') + ' ' + (injected.files || '')).toLowerCase();
            if (blob.indexOf('biblioteca') >= 0 || blob.indexOf('tomar foto') >= 0 || blob.indexOf('elegir') >= 0) {
                return true;
            }
        }

        return false;
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
        var es = isSpanishUi();
        var englishSheet = englishDefaults();
        scope.querySelectorAll('[data-indor-file-source], [data-pa-file-source], [data-nr-photo-source]').forEach(function (item) {
            var source = item.getAttribute('data-indor-file-source')
                || item.getAttribute('data-pa-file-source')
                || item.getAttribute('data-nr-photo-source');
            var mapped = source === 'camera' ? map.camera
                : (source === 'files' ? map.files : map.library);
            var english = source === 'camera' ? englishSheet.camera
                : (source === 'files' ? englishSheet.files : englishSheet.library);
            // Prefer server-rendered data-label (already localized) over JS map.
            var dataLabel = (item.getAttribute('data-label') || '').trim();
            var text = dataLabel || mapped;
            // Spanish UI must never keep English sheet copy (stale data-label / old HTML).
            if (es && (!text || text === english)) {
                text = mapped === english ? (
                    source === 'camera' ? 'Tomar foto'
                        : (source === 'files' ? 'Elegir archivos' : 'Biblioteca de fotos')
                ) : mapped;
            }
            // Never overwrite a non-English server label with English defaults.
            if (dataLabel && dataLabel !== english && text === english) {
                text = dataLabel;
            }
            if (!text) {
                return;
            }
            if (es || (dataLabel && dataLabel !== english)) {
                item.setAttribute('data-label', text);
            }
            var labelEl = item.querySelector('.indor-file-source-label');
            if (labelEl) {
                // Keep existing Spanish text if the computed replacement is English.
                var existing = (labelEl.textContent || '').trim();
                if (text === english && existing && existing !== english) {
                    text = existing;
                }
                labelEl.textContent = text;
            } else if (item.childNodes.length) {
                // Profile menus often put the label as text next to an <i> icon.
                var replaced = false;
                item.childNodes.forEach(function (node) {
                    if (node.nodeType === 3 && node.textContent && node.textContent.trim()) {
                        node.textContent = ' ' + text;
                        replaced = true;
                    }
                });
                if (!replaced) {
                    item.appendChild(document.createTextNode(' ' + text));
                }
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
            input.__indorAllowNative = true;
            input.value = '';
            input.click();
        } finally {
            input.style.pointerEvents = prev || '';
            window.setTimeout(function () {
                input.__indorAllowNative = false;
            }, 0);
        }
    }

    function mergeIntoTarget(target, fileList) {
        if (!target || !fileList || !fileList.length || typeof DataTransfer === 'undefined') {
            return;
        }
        var dt = new DataTransfer();
        if (target.multiple && target.files) {
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
                    openFileInput(filesInput || targetInput);
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

    // ── Global upgrade: plain <input type=file accept=image…> → localized sheet ──

    function acceptsImages(input) {
        var accept = (input.getAttribute('accept') || '').toLowerCase();
        if (!accept) {
            return true;
        }
        return /image|\.jpe?g|\.png|\.gif|\.webp|\.heic|\.heif|\.bmp/.test(accept);
    }

    function isManagedInput(input) {
        if (!input || input.type !== 'file') {
            return true;
        }
        if (input.getAttribute('data-indor-skip-source-upgrade') === '1') {
            return true;
        }
        if (input.classList.contains('indor-file-source-input--camera')
            || input.classList.contains('indor-file-source-input--library')
            || input.classList.contains('indor-file-source-input--files')
            || input.classList.contains('pa-file-source-input--camera')
            || input.classList.contains('pa-file-source-input--library')
            || input.classList.contains('pa-file-source-input--files')
            || input.classList.contains('pa-media-photo-input--camera')
            || input.classList.contains('pa-media-photo-input--library')
            || input.classList.contains('pa-media-photo-input--files')) {
            return true;
        }
        if (input.closest('[data-indor-file-source-chooser], [data-pa-file-source-chooser], .indor-file-source-picker, .pa-media-capture, .nr-photo-add-wrap, .nr-photo-box')) {
            return true;
        }
        // Camera-only inputs already skip the 3-option iOS sheet.
        if (input.hasAttribute('capture')) {
            return true;
        }
        return false;
    }

    var globalMenu = null;
    var globalCamera = null;
    var globalLibrary = null;
    var globalFiles = null;
    var activeTarget = null;
    var globalCloseBound = false;

    function ensureGlobalHelpers() {
        if (globalMenu) {
            return;
        }

        globalMenu = document.createElement('div');
        globalMenu.className = 'indor-file-source-menu indor-global-file-source-menu';
        globalMenu.setAttribute('role', 'menu');
        globalMenu.hidden = true;
        globalMenu.innerHTML =
            '<button type="button" class="indor-file-source-item" data-indor-file-source="library" role="menuitem">'
            + '<i class="fas fa-images" aria-hidden="true"></i><span class="indor-file-source-label"></span></button>'
            + '<button type="button" class="indor-file-source-item" data-indor-file-source="camera" role="menuitem">'
            + '<i class="fas fa-camera" aria-hidden="true"></i><span class="indor-file-source-label"></span></button>'
            + '<button type="button" class="indor-file-source-item" data-indor-file-source="files" role="menuitem">'
            + '<i class="fas fa-folder" aria-hidden="true"></i><span class="indor-file-source-label"></span></button>';

        globalCamera = document.createElement('input');
        globalCamera.type = 'file';
        globalCamera.accept = 'image/jpeg,image/png,image/webp,image/*,.heic,.heif';
        globalCamera.setAttribute('capture', 'environment');
        globalCamera.className = 'indor-file-source-input--camera';
        globalCamera.setAttribute('aria-hidden', 'true');
        globalCamera.tabIndex = -1;

        globalLibrary = document.createElement('input');
        globalLibrary.type = 'file';
        globalLibrary.accept = 'image/jpeg,image/png,image/webp,image/*,.heic,.heif';
        globalLibrary.className = 'indor-file-source-input--library';
        globalLibrary.setAttribute('aria-hidden', 'true');
        globalLibrary.tabIndex = -1;

        globalFiles = document.createElement('input');
        globalFiles.type = 'file';
        globalFiles.className = 'indor-file-source-input--files';
        globalFiles.setAttribute('aria-hidden', 'true');
        globalFiles.tabIndex = -1;

        var host = document.createElement('div');
        host.id = 'indorGlobalFileSourceHost';
        host.setAttribute('data-indor-skip-mutate', '1');
        host.appendChild(globalMenu);
        host.appendChild(globalCamera);
        host.appendChild(globalLibrary);
        host.appendChild(globalFiles);
        document.body.appendChild(host);

        applyLabels(globalMenu);

        globalMenu.querySelectorAll('[data-indor-file-source]').forEach(function (item) {
            item.addEventListener('click', function (e) {
                e.preventDefault();
                e.stopPropagation();
                var source = item.getAttribute('data-indor-file-source');
                var target = activeTarget;
                closeGlobalMenu();
                if (!target) {
                    return;
                }
                if (source === 'camera') {
                    globalCamera.multiple = !!target.multiple;
                    openFileInput(globalCamera);
                } else if (source === 'library') {
                    globalLibrary.multiple = !!target.multiple;
                    globalLibrary.removeAttribute('capture');
                    openFileInput(globalLibrary);
                } else {
                    globalFiles.accept = target.getAttribute('accept') || 'image/*,application/pdf,.pdf';
                    globalFiles.multiple = !!target.multiple;
                    openFileInput(globalFiles);
                }
            });
        });

        function onHelperChange(helper) {
            helper.addEventListener('change', function () {
                if (!activeTarget || !helper.files || !helper.files.length) {
                    return;
                }
                mergeIntoTarget(activeTarget, helper.files);
                activeTarget = null;
            });
        }
        onHelperChange(globalCamera);
        onHelperChange(globalLibrary);
        onHelperChange(globalFiles);

        if (!globalCloseBound) {
            globalCloseBound = true;
            document.addEventListener('click', function (e) {
                if (!globalMenu || globalMenu.hidden) {
                    return;
                }
                if (globalMenu.contains(e.target)) {
                    return;
                }
                closeGlobalMenu();
            });
            document.addEventListener('keydown', function (e) {
                if (e.key === 'Escape') {
                    closeGlobalMenu();
                }
            });
        }
    }

    function closeGlobalMenu() {
        if (globalMenu) {
            globalMenu.hidden = true;
        }
    }

    function positionGlobalMenu(anchor) {
        if (!globalMenu || !anchor) {
            return;
        }
        var rect = anchor.getBoundingClientRect();
        var menuWidth = Math.max(210, rect.width);
        var left = rect.left + window.scrollX;
        var top = rect.bottom + window.scrollY + 8;
        var maxLeft = window.scrollX + document.documentElement.clientWidth - menuWidth - 8;
        if (left > maxLeft) {
            left = Math.max(8 + window.scrollX, maxLeft);
        }
        globalMenu.style.width = menuWidth + 'px';
        globalMenu.style.left = left + 'px';
        globalMenu.style.top = top + 'px';
    }

    function openGlobalMenuFor(input, anchor) {
        ensureGlobalHelpers();
        applyLabels(globalMenu);
        activeTarget = input;
        positionGlobalMenu(anchor || input);
        globalMenu.hidden = false;
    }

    function upgradePlainFileInputs() {
        document.querySelectorAll('input[type="file"]').forEach(function (input) {
            if (input.getAttribute('data-indor-plain-upgrade') === '1') {
                return;
            }
            if (isManagedInput(input) || !acceptsImages(input)) {
                return;
            }
            input.setAttribute('data-indor-plain-upgrade', '1');

            input.addEventListener('click', function (e) {
                if (input.__indorAllowNative) {
                    return;
                }
                // Only intercept real user taps — programmatic .click() from existing menus stays native.
                if (!e.isTrusted) {
                    return;
                }
                e.preventDefault();
                e.stopPropagation();
                var anchor = input;
                if (input.id) {
                    var label = document.querySelector('label[for="' + input.id + '"]');
                    if (label) {
                        anchor = label;
                    }
                }
                openGlobalMenuFor(input, anchor);
            }, true);
        });
    }

    function boot() {
        applyLabels(document);
        document.querySelectorAll('[data-pa-file-source-chooser], [data-indor-file-source-chooser]').forEach(initChooser);
        upgradePlainFileInputs();
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', boot);
    } else {
        boot();
    }

    // Late-rendered upload widgets (wizards / AJAX).
    if (typeof MutationObserver !== 'undefined') {
        var scheduled = false;
        var observer = new MutationObserver(function (mutations) {
            var relevant = mutations.some(function (m) {
                return Array.prototype.some.call(m.addedNodes || [], function (node) {
                    if (!node || node.nodeType !== 1) {
                        return false;
                    }
                    if (node.id === 'indorGlobalFileSourceHost' || (node.closest && node.closest('#indorGlobalFileSourceHost'))) {
                        return false;
                    }
                    return node.matches && (node.matches('input[type="file"]')
                        || node.querySelector && node.querySelector('input[type="file"]'));
                });
            });
            if (!relevant || scheduled) {
                return;
            }
            scheduled = true;
            window.setTimeout(function () {
                scheduled = false;
                boot();
            }, 50);
        });
        observer.observe(document.documentElement, { childList: true, subtree: true });
    }

    window.IndorFileSourceChooser = {
        initAll: boot,
        applyLabels: applyLabels
    };
})();
