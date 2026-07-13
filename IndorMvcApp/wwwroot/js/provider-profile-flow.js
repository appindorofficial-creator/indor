(function () {
    'use strict';

    var websiteNa = document.getElementById('prvFlowWebsiteNa');
    var websiteInput = document.getElementById('prvFlowWebsite');
    if (websiteNa && websiteInput) {
        function syncWebsite() {
            websiteInput.disabled = websiteNa.checked;
            if (websiteNa.checked) {
                websiteInput.value = '';
            }
        }
        websiteNa.addEventListener('change', syncWebsite);
        syncWebsite();
    }

    document.querySelectorAll('.prv-pro-edit-profile-service-chip').forEach(function (chip) {
        var input = chip.querySelector('input[type="checkbox"]');
        if (!input || chip.dataset.serviceChipBound === 'true') {
            return;
        }
        chip.dataset.serviceChipBound = 'true';

        function syncServiceChip() {
            var on = !!input.checked;
            chip.classList.toggle('is-selected', on);
            chip.setAttribute('aria-checked', on ? 'true' : 'false');
            chip.setAttribute('aria-pressed', on ? 'true' : 'false');
        }

        function toggleServiceChip(e) {
            if (e) {
                e.preventDefault();
            }
            if (input.disabled) {
                return;
            }
            input.checked = !input.checked;
            syncServiceChip();
            input.dispatchEvent(new Event('change', { bubbles: true }));
        }

        // Div chips (not <label>) — one explicit toggle per tap/click/keyboard.
        chip.addEventListener('click', toggleServiceChip);
        chip.addEventListener('keydown', function (e) {
            if (e.key !== 'Enter' && e.key !== ' ') {
                return;
            }
            toggleServiceChip(e);
        });
        input.addEventListener('change', syncServiceChip);
        syncServiceChip();
    });

    var docsForm = document.getElementById('prvFlowDocsForm');
    if (docsForm) {
        var docTypeField = document.getElementById('prvFlowDocType');
        var sectionField = document.getElementById('prvFlowActiveSection');
        var defaultDocType = docTypeField ? docTypeField.value : '';
        var uploadRoot = docsForm.querySelector('[data-doc-upload]');
        var primaryInput = uploadRoot ? uploadRoot.querySelector('[data-role="file-input"]') : null;
        var cameraInput = uploadRoot ? uploadRoot.querySelector('[data-role="camera-input"]') : null;
        var uploadBtn = uploadRoot ? uploadRoot.querySelector('[data-role="upload-btn"]') : null;
        var pending = uploadRoot ? uploadRoot.querySelector('[data-role="pending"]') : null;
        var fileNameEl = uploadRoot ? uploadRoot.querySelector('[data-role="file-name"]') : null;
        var fileTrigger = uploadRoot ? uploadRoot.querySelector('[data-role="file-trigger"]') : null;
        var clearBtn = uploadRoot ? uploadRoot.querySelector('[data-role="clear-pending"]') : null;

        function setActiveFileInput(active) {
            docsForm.querySelectorAll('.prv-flow-doc-input').forEach(function (input) {
                if (input === active) {
                    input.setAttribute('name', 'documentFile');
                } else {
                    input.removeAttribute('name');
                    input.value = '';
                }
            });
        }

        function showPending(file) {
            if (!file || !pending) {
                hidePending();
                return;
            }

            if (fileNameEl) {
                fileNameEl.textContent = file.name;
            }
            pending.classList.remove('is-hidden');
            if (fileTrigger) {
                fileTrigger.classList.add('is-hidden');
            }
            if (uploadBtn) {
                uploadBtn.disabled = false;
                uploadBtn.classList.remove('is-hidden');
            }
        }

        function hidePending() {
            if (pending) {
                pending.classList.add('is-hidden');
            }
            if (fileNameEl) {
                fileNameEl.textContent = '';
            }
            if (fileTrigger) {
                fileTrigger.classList.remove('is-hidden');
            }
            if (uploadBtn) {
                uploadBtn.disabled = true;
                uploadBtn.classList.remove('is-hidden');
            }
            if (primaryInput) {
                primaryInput.setAttribute('name', 'documentFile');
                primaryInput.value = '';
            }
            if (cameraInput) {
                cameraInput.removeAttribute('name');
                cameraInput.value = '';
            }
            if (docTypeField) {
                docTypeField.value = defaultDocType;
            }
        }

        function onFileSelected(input) {
            var file = input.files && input.files[0];
            if (!file) {
                hidePending();
                return;
            }

            if (docTypeField) {
                docTypeField.value = input.getAttribute('data-doc-type') || defaultDocType;
            }
            if (sectionField) {
                sectionField.value = input.getAttribute('data-section') || sectionField.value;
            }

            setActiveFileInput(input);
            showPending(file);
        }

        docsForm.querySelectorAll('.prv-flow-doc-input').forEach(function (input) {
            input.addEventListener('change', function () {
                onFileSelected(input);
            });
        });

        if (uploadBtn) {
            uploadBtn.disabled = true;
        }

        if (clearBtn) {
            clearBtn.addEventListener('click', function () {
                hidePending();
            });
        }

        docsForm.addEventListener('submit', function () {
            var activeFile = null;
            docsForm.querySelectorAll('.prv-flow-doc-input').forEach(function (input) {
                if (input.files && input.files.length > 0 && input.getAttribute('name') === 'documentFile') {
                    activeFile = input;
                }
            });

            if (!activeFile) {
                // Saving metadata only — do not post an empty file field.
                docsForm.querySelectorAll('.prv-flow-doc-input').forEach(function (input) {
                    input.removeAttribute('name');
                });
            } else if (docTypeField && !docTypeField.value) {
                docTypeField.value = activeFile.getAttribute('data-doc-type') || defaultDocType;
            }

            if (typeof window.indorShowNavigationLoading === 'function') {
                window.indorShowNavigationLoading();
            }
        });
    }

    document.querySelectorAll('.prv-flow-form button[type="submit"], .prv-flow-cta').forEach(function (btn) {
        btn.addEventListener('click', function () {
            if (typeof window.indorShowNavigationLoading === 'function') {
                window.indorShowNavigationLoading();
            }
        });
    });
})();
