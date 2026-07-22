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

    document.querySelectorAll('.prv-pro-edit-profile-service-chip input').forEach(function (input) {
        input.addEventListener('change', function () {
            input.closest('.prv-pro-edit-profile-service-chip').classList.toggle('is-selected', input.checked);
        });
    });

    var docsForm = document.getElementById('prvFlowDocsForm');
    if (docsForm) {
        var docTypeField = document.getElementById('prvFlowDocType');
        var sectionField = document.getElementById('prvFlowActiveSection');
        var pendingFileInput = null;

        docsForm.querySelectorAll('.prv-flow-doc-input').forEach(function (input) {
            input.addEventListener('change', function () {
                if (!input.files || input.files.length === 0) {
                    return;
                }

                if (docTypeField) {
                    docTypeField.value = input.getAttribute('data-doc-type') || '';
                }
                if (sectionField) {
                    sectionField.value = input.getAttribute('data-section') || sectionField.value || '';
                }

                pendingFileInput = input;

                docsForm.querySelectorAll('.prv-flow-doc-input').forEach(function (other) {
                    if (other !== input) {
                        other.removeAttribute('name');
                    }
                });
                input.setAttribute('name', 'documentFile');

                if (typeof window.indorShowNavigationLoading === 'function') {
                    window.indorShowNavigationLoading();
                }
                docsForm.submit();
            });
        });

        docsForm.addEventListener('submit', function () {
            if (!pendingFileInput) {
                if (docTypeField) {
                    docTypeField.value = '';
                }
                docsForm.querySelectorAll('.prv-flow-doc-input').forEach(function (input) {
                    input.removeAttribute('name');
                });
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
