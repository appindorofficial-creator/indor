(function () {
    'use strict';

    function pfMsg(text) {
        var i18n = window.IndorPropertyFileI18n;
        return (i18n && i18n[text]) || text;
    }

    function showFormErrorBanner(formId, bannerId, messages) {
        var form = document.getElementById(formId);
        if (!form || !messages.length) {
            return;
        }

        var existing = document.getElementById(bannerId);
        if (existing) {
            existing.remove();
        }

        var banner = document.createElement('div');
        banner.className = 'rl-error-banner';
        banner.id = bannerId;
        banner.setAttribute('role', 'alert');
        banner.innerHTML =
            '<i class="fas fa-circle-exclamation"></i>' +
            '<div><strong>' + pfMsg('Please fix the following to continue:') + '</strong>' +
            '<ul class="rl-error-list">' +
            messages.map(function (msg) { return '<li>' + pfMsg(msg) + '</li>'; }).join('') +
            '</ul></div>';

        var summary = form.querySelector('.ob-summary');
        if (summary && summary.nextSibling) {
            form.insertBefore(banner, summary.nextSibling);
        } else {
            form.insertBefore(banner, form.firstChild.nextSibling);
        }

        banner.scrollIntoView({ behavior: 'smooth', block: 'center' });
    }

    function validateDetailsForm() {
        var form = document.getElementById('propFileForm');
        if (!form) {
            return true;
        }

        var existing = document.getElementById('propFileDetailsErrors');
        if (existing) {
            existing.remove();
        }

        var errors = [];
        var propertySelected = form.querySelector('input[name="sourcePropertyId"]:checked');
        var hasProperties = form.querySelectorAll('input[name="sourcePropertyId"]').length > 0;

        if (hasProperties && !propertySelected) {
            errors.push('Select a property from the list below to continue.');
            document.querySelector('.rl-prop-file-property-list, .rl-invite-form')?.classList.add('is-error');
        }

        var filePhaseSelected = form.querySelector('input[name="filePhase"]:checked');
        if (!filePhaseSelected) {
            errors.push('Select a file type.');
            document.querySelector('.rl-file-type-list')?.classList.add('is-error');
        }

        if (errors.length) {
            showFormErrorBanner('propFileForm', 'propFileDetailsErrors', errors);
            return false;
        }

        return true;
    }

    function validateAddItemsForm() {
        var form = document.getElementById('addItemsForm');
        if (!form) {
            return true;
        }

        var existing = document.getElementById('propFileAddItemsErrors');
        if (existing) {
            existing.remove();
        }

        var checked = form.querySelectorAll('input[name="categoryTypes"]:checked');
        if (!checked.length) {
            showFormErrorBanner('addItemsForm', 'propFileAddItemsErrors', ['Select at least one item type.']);
            document.querySelector('.rl-category-grid')?.classList.add('is-error');
            return false;
        }

        document.querySelector('.rl-category-grid')?.classList.remove('is-error');
        return true;
    }

    function initDetailsForm() {
        var form = document.getElementById('propFileForm');
        if (!form) {
            return;
        }

        form.addEventListener('submit', function (e) {
            if (!validateDetailsForm()) {
                e.preventDefault();
            }
        });

        document.querySelectorAll('.rl-property-pick input').forEach(function (radio) {
            radio.addEventListener('change', function () {
                document.querySelectorAll('.rl-property-pick').forEach(function (p) {
                    p.classList.remove('is-selected');
                });
                radio.closest('.rl-property-pick')?.classList.add('is-selected');
                document.getElementById('propFileDetailsErrors')?.remove();
            });
        });

        document.querySelectorAll('.rl-file-type-option input').forEach(function (radio) {
            radio.addEventListener('change', function () {
                document.querySelectorAll('.rl-file-type-option').forEach(function (p) {
                    p.classList.remove('is-selected');
                });
                radio.closest('.rl-file-type-option')?.classList.add('is-selected');
                document.querySelector('.rl-file-type-list')?.classList.remove('is-error');
                document.getElementById('propFileDetailsErrors')?.remove();
            });
        });
    }

    function initAddItemsForm() {
        var form = document.getElementById('addItemsForm');
        if (!form) {
            return;
        }

        form.addEventListener('submit', function (e) {
            if (!validateAddItemsForm()) {
                e.preventDefault();
            }
        });

        document.querySelectorAll('.rl-category-card input').forEach(function (checkbox) {
            checkbox.addEventListener('change', function () {
                checkbox.closest('.rl-category-card')?.classList.toggle('is-selected', checkbox.checked);
                if (form.querySelectorAll('input[name="categoryTypes"]:checked').length) {
                    document.getElementById('propFileAddItemsErrors')?.remove();
                    document.querySelector('.rl-category-grid')?.classList.remove('is-error');
                }
            });
        });
    }

    function init() {
        initDetailsForm();
        initAddItemsForm();
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
