(function () {
    'use strict';

    var MIN_LENGTH = 4;
    var MAX_LENGTH = 20;
    var ALPHANUMERIC = /^[A-Za-z0-9]+$/;
    var HAS_LETTER = /[A-Za-z]/;

    function t(key) {
        var i18n = window.IndorRealtorLicenseI18n || {};
        return i18n[key] || key;
    }

    function validate(value) {
        var license = (value || '').trim();
        if (!license) {
            return t('License number is required.');
        }
        if (license.length < MIN_LENGTH) {
            return t('License number must be at least 4 characters.');
        }
        if (license.length > MAX_LENGTH) {
            return t('License number cannot exceed 20 characters.');
        }
        if (!ALPHANUMERIC.test(license)) {
            return t('License number can only contain letters and numbers (no spaces or symbols).');
        }
        if (!HAS_LETTER.test(license)) {
            return t('License number must include at least one letter (cannot be only numbers).');
        }
        return '';
    }

    function attach(input, options) {
        if (!input || input.dataset.licenseBound === 'true') {
            return;
        }

        options = options || {};
        input.dataset.licenseBound = 'true';
        var validateOnLoad = options.validateOnLoad !== false;

        var errorEl = options.errorEl
            || (input.id ? document.getElementById(input.id + 'Error') : null)
            || input.closest('.rl-ep-field, .rl-field-block, .rl-cp-field')
                ?.querySelector('.rl-field-error, [data-license-error]');

        function sync() {
            var message = validate(input.value);
            input.setCustomValidity(message);
            if (errorEl) {
                errorEl.textContent = message;
            }
            var fieldWrap = input.closest('.rl-ep-field, .rl-field, .rl-cp-field');
            if (fieldWrap) {
                fieldWrap.classList.toggle('is-error', !!message);
            }
            input.classList.toggle('input-validation-error', !!message);
            return !message;
        }

        input.addEventListener('input', sync);
        input.addEventListener('blur', sync);
        if (validateOnLoad) {
            sync();
        }
        return sync;
    }

    window.IndorRealtorLicense = {
        validate: validate,
        attach: attach
    };
})();
