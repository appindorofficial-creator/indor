(function () {
    'use strict';

    var MIN_LENGTH = 4;
    var MAX_LENGTH = 20;
    var ALPHANUMERIC = /^[A-Za-z0-9]+$/;
    var HAS_LETTER = /[A-Za-z]/;

    function validate(value) {
        var license = (value || '').trim();
        if (!license) {
            return 'License number is required.';
        }
        if (license.length < MIN_LENGTH) {
            return 'License number must be at least ' + MIN_LENGTH + ' characters.';
        }
        if (license.length > MAX_LENGTH) {
            return 'License number cannot exceed ' + MAX_LENGTH + ' characters.';
        }
        if (!ALPHANUMERIC.test(license)) {
            return 'License number can only contain letters and numbers (no spaces or symbols).';
        }
        if (!HAS_LETTER.test(license)) {
            return 'License number must include at least one letter (cannot be only numbers).';
        }
        return '';
    }

    function attach(input, options) {
        if (!input || input.dataset.licenseBound === 'true') {
            return;
        }

        options = options || {};
        input.dataset.licenseBound = 'true';

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
        sync();
        return sync;
    }

    window.IndorRealtorLicense = {
        validate: validate,
        attach: attach
    };
})();
