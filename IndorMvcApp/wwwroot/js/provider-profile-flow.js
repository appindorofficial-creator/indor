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

        function expiryIsSkipped() {
            var na = docsForm.querySelector('input[name="InsuranceNotApplicable"], input[name="LicenseNotApplicable"]');
            var unk = docsForm.querySelector('input[name="InsuranceUnknown"], input[name="LicenseUnknown"]');
            return (na && na.checked) || (unk && unk.checked);
        }

        function showExpiryError(show) {
            var input = docsForm.querySelector('.prv-flow-expiry-input');
            var err = docsForm.querySelector('[data-expiry-error]');
            if (input) input.classList.toggle('is-invalid', show);
            if (err) {
                if (!err.textContent) {
                    err.textContent = docsForm.getAttribute('data-expiry-error') || '';
                }
                err.classList.toggle('is-hidden', !show);
            }
        }

        function expiryIsPast() {
            var input = docsForm.querySelector('.prv-flow-expiry-input');
            var min = docsForm.getAttribute('data-min-expiry') || '';
            if (!input || !input.value || !min || expiryIsSkipped()) {
                return false;
            }
            return input.value < min;
        }

        function blockIfExpired(event) {
            if (!expiryIsPast()) {
                showExpiryError(false);
                return false;
            }
            event.preventDefault();
            showExpiryError(true);
            if (typeof window.indorHideNavigationLoading === 'function') {
                window.indorHideNavigationLoading();
            }
            var input = docsForm.querySelector('.prv-flow-expiry-input');
            if (input && typeof input.scrollIntoView === 'function') {
                input.scrollIntoView({ block: 'center' });
                input.focus();
            }
            return true;
        }

        docsForm.querySelectorAll('.prv-flow-doc-input').forEach(function (input) {
            input.addEventListener('change', function () {
                if (!input.files || input.files.length === 0) {
                    return;
                }

                if (blockIfExpired(new Event('submit'))) {
                    input.value = '';
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

        docsForm.addEventListener('submit', function (event) {
            if (blockIfExpired(event)) {
                return;
            }
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
            var form = btn.closest('#prvFlowDocsForm');
            if (form) {
                var min = form.getAttribute('data-min-expiry') || '';
                var input = form.querySelector('.prv-flow-expiry-input');
                var na = form.querySelector('input[name="InsuranceNotApplicable"], input[name="LicenseNotApplicable"]');
                var unk = form.querySelector('input[name="InsuranceUnknown"], input[name="LicenseUnknown"]');
                var skipped = (na && na.checked) || (unk && unk.checked);
                if (input && input.value && min && !skipped && input.value < min) {
                    return;
                }
            }
            if (typeof window.indorShowNavigationLoading === 'function') {
                window.indorShowNavigationLoading();
            }
        });
    });
})();
