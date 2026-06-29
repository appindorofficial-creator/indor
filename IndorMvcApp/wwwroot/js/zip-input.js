(function () {
    'use strict';

    var zipFormatRegex = /^\d{5}(-\d{4})?$/;

    function normalizeZipValue(value) {
        var raw = (value || '').trim();
        if (!raw) {
            return '';
        }

        var digits = raw.replace(/\D/g, '');
        if (digits.length <= 5) {
            return digits.slice(0, 5);
        }

        return digits.slice(0, 5) + '-' + digits.slice(5, 9);
    }

    function isValidRequiredZip(value) {
        return zipFormatRegex.test(normalizeZipValue(value));
    }

    function attachZipInput(input, options) {
        if (!input || input.dataset.zipBound === 'true') {
            return;
        }

        options = options || {};
        input.dataset.zipBound = 'true';
        input.setAttribute('inputmode', 'numeric');
        input.setAttribute('autocomplete', input.getAttribute('autocomplete') || 'postal-code');
        input.setAttribute('maxlength', '10');

        var required = options.required !== false;
        var requiredMessage = options.requiredMessage || 'ZIP code is required.';
        var invalidMessage = options.invalidMessage || 'Enter a valid 5-digit ZIP code (e.g. 77002).';

        function syncValue() {
            var normalized = normalizeZipValue(input.value);
            if (input.value !== normalized) {
                input.value = normalized;
            }

            if (!normalized) {
                input.setCustomValidity(required ? requiredMessage : '');
                return;
            }

            input.setCustomValidity(isValidRequiredZip(normalized) ? '' : invalidMessage);
        }

        input.addEventListener('beforeinput', function (e) {
            if (e.isComposing) {
                return;
            }
            if (e.inputType && e.inputType.indexOf('delete') === 0) {
                return;
            }
            if (e.inputType === 'insertFromPaste') {
                return;
            }
            if (e.data && /\D/.test(e.data)) {
                e.preventDefault();
            }
        });

        input.addEventListener('keydown', function (e) {
            if (e.ctrlKey || e.metaKey || e.altKey) {
                return;
            }
            var allowed = ['Backspace', 'Delete', 'Tab', 'ArrowLeft', 'ArrowRight', 'Home', 'End'];
            if (allowed.indexOf(e.key) >= 0) {
                return;
            }
            if (/^\d$/.test(e.key)) {
                return;
            }
            e.preventDefault();
        });

        input.addEventListener('input', syncValue);

        input.addEventListener('paste', function (e) {
            e.preventDefault();
            var text = (e.clipboardData || window.clipboardData).getData('text');
            input.value = normalizeZipValue(text);
            syncValue();
        });

        syncValue();
    }

    window.IndorZipInput = {
        attach: attachZipInput,
        normalize: normalizeZipValue,
        isValidRequired: isValidRequiredZip,
        validateRequired: function (value) {
            if (!value || !String(value).trim()) {
                return 'ZIP code is required.';
            }
            if (!isValidRequiredZip(value)) {
                return 'Enter a valid 5-digit ZIP code (e.g. 77002).';
            }
            return '';
        }
    };

    document.addEventListener('DOMContentLoaded', function () {
        document.querySelectorAll('[data-zip-input]').forEach(function (input) {
            attachZipInput(input, { required: input.dataset.zipRequired !== 'false' });
        });
    });
})();
