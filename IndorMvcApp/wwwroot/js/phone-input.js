(function () {
    function normalizePhoneDigits(value) {
        var digits = (value || '').replace(/\D/g, '');
        if (digits.length === 11 && digits.charAt(0) === '1') {
            digits = digits.slice(1);
        }
        return digits.slice(0, 10);
    }

    function isValidOptionalPhone(value) {
        var digits = normalizePhoneDigits(value);
        return !digits || digits.length === 10;
    }

    function attachPhoneInput(input, options) {
        if (!input || input.dataset.phoneBound === 'true') {
            return;
        }

        options = options || {};
        input.dataset.phoneBound = 'true';
        input.setAttribute('inputmode', 'numeric');
        input.setAttribute('autocomplete', input.getAttribute('autocomplete') || 'tel');
        input.setAttribute('maxlength', '10');

        var invalidMessage = options.invalidMessage || 'Enter a valid 10-digit US phone number.';

        function syncValue() {
            var digits = normalizePhoneDigits(input.value);
            if (input.value !== digits) {
                input.value = digits;
            }
            input.setCustomValidity(isValidOptionalPhone(digits) ? '' : invalidMessage);
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
            input.value = normalizePhoneDigits(text);
            syncValue();
        });

        syncValue();
    }

    window.IndorPhoneInput = {
        attach: attachPhoneInput,
        normalize: normalizePhoneDigits,
        isValidOptional: isValidOptionalPhone
    };

    document.addEventListener('DOMContentLoaded', function () {
        document.querySelectorAll('[data-phone-input]').forEach(function (input) {
            attachPhoneInput(input);
        });
    });
})();
