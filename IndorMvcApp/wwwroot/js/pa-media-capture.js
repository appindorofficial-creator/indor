(function () {
    'use strict';

    function csrfToken() {
        var el = document.querySelector('input[name="__RequestVerificationToken"]');
        return el ? el.value : '';
    }

    function parseAttachments(raw) {
        try {
            var list = JSON.parse(raw || '[]');
            return Array.isArray(list) ? list : [];
        } catch (_) {
            return [];
        }
    }

    function initCapture(root) {
        if (!root || root.getAttribute('data-pa-media-ready') === '1') {
            return;
        }
        root.setAttribute('data-pa-media-ready', '1');

        if (window.IndorFileSourceChooser && typeof window.IndorFileSourceChooser.applyLabels === 'function') {
            window.IndorFileSourceChooser.applyLabels(root);
        }

        var form = root.closest('form');
        if (form) {
            form.setAttribute('enctype', 'multipart/form-data');
        }

        var uploadUrl = root.getAttribute('data-upload-url') || '';
        var defaultKind = root.getAttribute('data-upload-kind') || 'photo';
        var cameraInput = root.querySelector('.pa-media-photo-input--camera');
        var libraryInput = root.querySelector('.pa-media-photo-input--library');
        var filesInput = root.querySelector('.pa-media-photo-input--files');
        // Legacy single-input markup still works.
        var photoInputs = [cameraInput, libraryInput, filesInput, root.querySelector('.pa-media-photo-input')]
            .filter(function (el, i, arr) { return el && arr.indexOf(el) === i; });
        var photoBtn = root.querySelector('.pa-media-photo-btn');
        var photoMenu = root.querySelector('.pa-media-photo-menu');
        var voiceBtn = root.querySelector('.pa-media-voice-btn');
        var voiceLabel = root.querySelector('.pa-media-voice-label');
        var preview = root.querySelector('.pa-media-preview');
        var hidden = root.querySelector('.pa-media-attachments-json');
        var attachments = parseAttachments(hidden && hidden.value);
        var mediaRecorder = null;
        var recordedChunks = [];

        function setMenuOpen(open) {
            if (!photoMenu || !photoBtn) {
                return;
            }
            photoMenu.hidden = !open;
            photoBtn.setAttribute('aria-expanded', open ? 'true' : 'false');
        }

        function closeMenu() {
            setMenuOpen(false);
        }

        function syncHidden() {
            if (hidden) {
                hidden.value = JSON.stringify(attachments);
            }
            renderPreview();
        }

        function renderPreview() {
            if (!preview) {
                return;
            }
            preview.innerHTML = '';
            if (!attachments.length) {
                preview.hidden = true;
                return;
            }
            preview.hidden = false;
            attachments.forEach(function (item, index) {
                var li = document.createElement('li');
                li.className = 'pa-media-preview-item';
                var icon = item.kind === 'voice'
                    ? 'fa-microphone'
                    : (item.kind === 'document' || item.kind === 'video' ? 'fa-file' : 'fa-image');
                var name = item.name
                    || (item.kind === 'voice' ? 'Voice note' : (item.kind === 'document' ? 'Document' : 'Photo'));
                li.innerHTML =
                    '<i class="fas ' + icon + '"></i>' +
                    '<span class="pa-media-preview-name"></span>' +
                    '<button type="button" class="pa-media-preview-remove" aria-label="Remove">&times;</button>';
                li.querySelector('.pa-media-preview-name').textContent = name;
                li.querySelector('.pa-media-preview-remove').addEventListener('click', function () {
                    attachments.splice(index, 1);
                    syncHidden();
                });
                preview.appendChild(li);
            });
        }

        function setBusy(busy) {
            root.classList.toggle('is-busy', !!busy);
            if (photoBtn) {
                photoBtn.disabled = !!busy;
            }
            if (voiceBtn && voiceBtn.getAttribute('data-recording') !== 'true') {
                voiceBtn.disabled = !!busy;
            }
        }

        async function uploadFile(file, kind) {
            if (!uploadUrl) {
                throw new Error('Missing upload URL');
            }
            var body = new FormData();
            body.append('file', file);
            body.append('kind', kind || 'photo');
            var token = csrfToken();
            if (token) {
                body.append('__RequestVerificationToken', token);
            }
            var res = await fetch(uploadUrl, {
                method: 'POST',
                body: body,
                credentials: 'same-origin',
                headers: token ? { RequestVerificationToken: token } : undefined
            });
            if (!res.ok) {
                throw new Error('Upload failed');
            }
            return res.json();
        }

        async function handlePhotoFiles(input) {
            var files = Array.prototype.slice.call((input && input.files) || []);
            if (input) {
                input.value = '';
            }
            if (!files.length) {
                return;
            }
            setBusy(true);
            try {
                for (var i = 0; i < files.length; i++) {
                    var result = await uploadFile(files[i], defaultKind);
                    if (result && result.path) {
                        attachments.push({
                            kind: result.kind || defaultKind,
                            path: result.path,
                            name: result.name || files[i].name
                        });
                    }
                }
                syncHidden();
            } catch (_) {
                alert(root.getAttribute('data-msg-upload-fail') || 'Upload failed');
            } finally {
                setBusy(false);
            }
        }

        if (libraryInput) {
            // Gallery must never force the camera (capture= is camera-only on mobile).
            libraryInput.removeAttribute('capture');
        }

        photoInputs.forEach(function (input) {
            input.addEventListener('change', function () {
                handlePhotoFiles(input);
            });
        });

        function openFileInput(input) {
            if (!input) {
                return;
            }
            // Off-screen inputs use pointer-events:none; briefly enable for programmatic open.
            var prev = input.style.pointerEvents;
            input.style.pointerEvents = 'auto';
            try {
                input.click();
            } finally {
                input.style.pointerEvents = prev || '';
            }
        }

        if (photoBtn) {
            if (photoMenu && (cameraInput || libraryInput)) {
                photoBtn.addEventListener('click', function (e) {
                    e.preventDefault();
                    e.stopPropagation();
                    if (window.IndorFileSourceChooser && typeof window.IndorFileSourceChooser.applyLabels === 'function') {
                        window.IndorFileSourceChooser.applyLabels(root);
                    }
                    setMenuOpen(photoMenu.hidden);
                });

                // Prefer shared _PaFileSourceMenu (data-pa-file-source); keep legacy data-pa-media-source.
                root.querySelectorAll('[data-pa-file-source], [data-pa-media-source]').forEach(function (item) {
                    item.addEventListener('click', function (e) {
                        e.preventDefault();
                        e.stopPropagation();
                        var source = item.getAttribute('data-pa-file-source')
                            || item.getAttribute('data-pa-media-source');
                        closeMenu();
                        // Library = no capture (gallery). Camera may use capture=environment.
                        // Files = broader accept (PDF etc.) without forcing the English OS sheet first.
                        var target;
                        if (source === 'camera') {
                            target = cameraInput || libraryInput;
                        } else if (source === 'files') {
                            target = filesInput || libraryInput || cameraInput;
                        } else {
                            target = libraryInput || cameraInput;
                        }
                        if (target === libraryInput || target === filesInput) {
                            target.removeAttribute('capture');
                        }
                        openFileInput(target);
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
            } else if (photoInputs.length) {
                // Fallback: prefer library (no capture) so OS offers camera + gallery.
                photoBtn.addEventListener('click', function () {
                    openFileInput(libraryInput || photoInputs[0]);
                });
            }
        }

        function stopRecording() {
            if (mediaRecorder && mediaRecorder.state !== 'inactive') {
                mediaRecorder.stop();
            }
        }

        if (voiceBtn) {
            voiceBtn.addEventListener('click', async function () {
                if (voiceBtn.getAttribute('data-recording') === 'true') {
                    stopRecording();
                    return;
                }
                if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
                    alert(root.getAttribute('data-msg-mic-denied') || 'Microphone unavailable');
                    return;
                }
                try {
                    var stream = await navigator.mediaDevices.getUserMedia({ audio: true });
                    recordedChunks = [];
                    var mime = MediaRecorder.isTypeSupported('audio/webm')
                        ? 'audio/webm'
                        : (MediaRecorder.isTypeSupported('audio/mp4') ? 'audio/mp4' : '');
                    mediaRecorder = mime
                        ? new MediaRecorder(stream, { mimeType: mime })
                        : new MediaRecorder(stream);
                    mediaRecorder.addEventListener('dataavailable', function (e) {
                        if (e.data && e.data.size > 0) {
                            recordedChunks.push(e.data);
                        }
                    });
                    mediaRecorder.addEventListener('stop', async function () {
                        stream.getTracks().forEach(function (t) { t.stop(); });
                        voiceBtn.setAttribute('data-recording', 'false');
                        voiceBtn.classList.remove('is-recording');
                        if (voiceLabel) {
                            voiceLabel.textContent = root.getAttribute('data-msg-record') || 'Voice note';
                        }
                        var blob = new Blob(recordedChunks, { type: mediaRecorder.mimeType || 'audio/webm' });
                        if (!blob.size) {
                            return;
                        }
                        var ext = (blob.type || '').indexOf('mp4') >= 0 ? 'm4a' : 'webm';
                        var file = new File([blob], 'voice-note.' + ext, { type: blob.type || 'audio/webm' });
                        setBusy(true);
                        try {
                            var result = await uploadFile(file, 'voice');
                            if (result && result.path) {
                                attachments = attachments.filter(function (a) { return a.kind !== 'voice'; });
                                attachments.push({
                                    kind: 'voice',
                                    path: result.path,
                                    name: result.name || 'Voice note'
                                });
                                syncHidden();
                            }
                        } catch (_) {
                            alert(root.getAttribute('data-msg-upload-fail') || 'Upload failed');
                        } finally {
                            setBusy(false);
                        }
                    });
                    mediaRecorder.start();
                    voiceBtn.setAttribute('data-recording', 'true');
                    voiceBtn.classList.add('is-recording');
                    if (voiceLabel) {
                        voiceLabel.textContent = root.getAttribute('data-msg-recording') || 'Recording…';
                    }
                } catch (_) {
                    alert(root.getAttribute('data-msg-mic-denied') || 'Microphone permission required');
                }
            });
        }

        syncHidden();
    }

    function boot() {
        document.querySelectorAll('[data-pa-media-capture]').forEach(initCapture);
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', boot);
    } else {
        boot();
    }
})();
