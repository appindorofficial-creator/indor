(function () {
    function isSpanishUi() {
        var lang = (document.documentElement.getAttribute('lang') || '').toLowerCase();
        return lang === 'es' || lang.indexOf('es-') === 0;
    }

    function validityMessage(field) {
        var es = isSpanishUi();

        if (field.validity.valueMissing) {
            if (field.type === 'radio') {
                return es ? 'Elige una de estas opciones.' : 'Please choose one of these options.';
            }

            if (field.type === 'checkbox') {
                return es ? 'Marca esta casilla si quieres continuar.' : 'Please check this box if you want to proceed.';
            }

            if (field.tagName === 'SELECT') {
                return es ? 'Selecciona un elemento de la lista.' : 'Please select an item in the list.';
            }

            if (field.type === 'number') {
                return es ? 'Ingresa un número.' : 'Please enter a number.';
            }

            if (field.type === 'date') {
                return es ? 'Elige una fecha.' : 'Please choose a date.';
            }

            return es ? 'Completa este campo.' : 'Please fill out this field.';
        }

        if (field.validity.typeMismatch) {
            if (field.type === 'email') {
                return es ? 'Ingresa un correo electrónico válido.' : 'Please enter a valid email address.';
            }

            return es ? 'Ingresa un valor válido.' : 'Please enter a valid value.';
        }

        if (field.validity.tooLong) {
            return es ? 'Acorta este texto.' : 'Please shorten this text.';
        }

        if (field.validity.rangeUnderflow) {
            return es ? 'Ingresa un valor más alto.' : 'Please enter a higher value.';
        }

        if (field.validity.rangeOverflow) {
            return es ? 'Ingresa un valor más bajo.' : 'Please enter a lower value.';
        }

        return es ? 'Ingresa un valor válido.' : 'Please enter a valid value.';
    }

    function clearFieldValidity(field) {
        field.setCustomValidity('');

        if (field.type === 'radio' && field.name) {
            document.querySelectorAll('input[type="radio"][name="' + field.name + '"]').forEach(function (radio) {
                radio.setCustomValidity('');
            });
        }
    }

    function phoneValidityMessage() {
        return isSpanishUi()
            ? 'Ingresa un teléfono de EE. UU. de 10 dígitos (p. ej., 555 123 4567).'
            : 'Enter a valid 10-digit US phone number (e.g. 555 123 4567).';
    }

    function isValidPhoneField(field) {
        if (!field.value.trim()) {
            return true;
        }

        return window.IndorPhoneInput
            ? window.IndorPhoneInput.isValidOptional(field.value)
            : field.value.replace(/\D/g, '').length === 10;
    }

    function bindLocalizedFormValidation(form) {
        if (form.dataset.prvLocalizedValidation === 'true') {
            return;
        }

        form.dataset.prvLocalizedValidation = 'true';
        form.noValidate = true;
        form.setAttribute('novalidate', 'novalidate');

        form.addEventListener('submit', function (e) {
            var fields = form.querySelectorAll('input, select, textarea');
            var firstInvalid = null;

            fields.forEach(function (field) {
                clearFieldValidity(field);
            });

            fields.forEach(function (field) {
                if (field.getAttribute('data-phone-input') !== null && !isValidPhoneField(field)) {
                    if (!firstInvalid) {
                        firstInvalid = field;
                    }
                }
            });

            fields.forEach(function (field) {
                if (!firstInvalid && !field.checkValidity()) {
                    firstInvalid = field;
                }
            });

            if (!firstInvalid) {
                return;
            }

            e.preventDefault();
            var message = firstInvalid.getAttribute('data-phone-input') !== null && firstInvalid.value.trim()
                ? phoneValidityMessage()
                : validityMessage(firstInvalid);
            firstInvalid.setCustomValidity(message);
            firstInvalid.reportValidity();
            if (typeof firstInvalid.focus === 'function') {
                firstInvalid.focus({ preventScroll: true });
            }
        });

        form.querySelectorAll('input, select, textarea').forEach(function (field) {
            field.addEventListener('input', function () {
                clearFieldValidity(field);
            });
            field.addEventListener('change', function () {
                clearFieldValidity(field);
            });
        });
    }

    document.querySelectorAll('.prv-pro-wizard-form, .prv-pro-add-customer-form').forEach(bindLocalizedFormValidation);

    function clearSearchInputChrome(input) {
        input.style.setProperty('box-shadow', 'none', 'important');
        input.style.setProperty('outline', 'none', 'important');
        input.style.setProperty('border', 'none', 'important');
        input.style.setProperty('background', 'transparent', 'important');
    }

    function bindPlainSearchInput(input) {
        if (!input || input.dataset.prvPlainSearchBound === 'true') {
            return;
        }

        input.dataset.prvPlainSearchBound = 'true';
        ['focus', 'focusin', 'input'].forEach(function (eventName) {
            input.addEventListener(eventName, function () {
                clearSearchInputChrome(input);
            });
        });
        clearSearchInputChrome(input);
    }

    document.querySelectorAll('.prv-pro-search-input, .prv-pro-wizard-search input, .prv-pro-search-wrap input')
        .forEach(bindPlainSearchInput);
})();
