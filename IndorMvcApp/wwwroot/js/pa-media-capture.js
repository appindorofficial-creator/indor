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

        var form = root.closest('form');
        if (form) {
            form.setAttribute('enctype', 'multipart/form-data');
        }

        var uploadUrl = root.getAttribute('data-upload-url') || '';
        var photoInput = root.querySelector('.pa-media-photo-input');
        var photoBtn = root.querySelector('.pa-media-photo-btn');
        var voiceBtn = root.querySelector('.pa-media-voice-btn');
        var voiceLabel = root.querySelector('.pa-media-voice-label');
        var preview = root.querySelector('.pa-media-preview');
        var hidden = root.querySelector('.pa-media-attachments-json');
        var attachments = parseAttachments(hidden && hidden.value);
        var mediaRecorder = null;
        var recordedChunks = [];

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
                var icon = item.kind === 'voice' ? 'fa-microphone' : 'fa-image';
                var name = item.name || (item.kind === 'voice' ? 'Voice note' : 'Photo');
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

        if (photoBtn && photoInput) {
            photoBtn.addEventListener('click', function () {
                photoInput.click();
            });
            photoInput.addEventListener('change', async function () {
                var files = Array.prototype.slice.call(photoInput.files || []);
                photoInput.value = '';
                if (!files.length) {
                    return;
                }
                setBusy(true);
                try {
                    for (var i = 0; i < files.length; i++) {
                        var result = await uploadFile(files[i], 'photo');
                        if (result && result.path) {
                            attachments.push({
                                kind: 'photo',
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
            });
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
