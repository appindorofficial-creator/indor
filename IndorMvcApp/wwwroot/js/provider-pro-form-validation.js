(function () {
    function englishValidityMessage(field) {
        if (field.validity.valueMissing) {
            if (field.type === 'radio') {
                return 'Please choose one of these options.';
            }

            if (field.type === 'checkbox') {
                return 'Please check this box if you want to proceed.';
            }

            if (field.tagName === 'SELECT') {
                return 'Please select an item in the list.';
            }

            if (field.type === 'number') {
                return 'Please enter a number.';
            }

            if (field.type === 'date') {
                return 'Please choose a date.';
            }

            return 'Please fill out this field.';
        }

        if (field.validity.typeMismatch) {
            return field.type === 'email'
                ? 'Please enter a valid email address.'
                : 'Please enter a valid value.';
        }

        if (field.validity.tooLong) {
            return 'Please shorten this text.';
        }

        if (field.validity.rangeUnderflow) {
            return 'Please enter a higher value.';
        }

        if (field.validity.rangeOverflow) {
            return 'Please enter a lower value.';
        }

        return 'Please enter a valid value.';
    }

    function clearFieldValidity(field) {
        field.setCustomValidity('');

        if (field.type === 'radio' && field.name) {
            document.querySelectorAll('input[type="radio"][name="' + field.name + '"]').forEach(function (radio) {
                radio.setCustomValidity('');
            });
        }
    }

    function bindEnglishFormValidation(form) {
        if (form.dataset.prvEnglishValidation === 'true') {
            return;
        }

        form.dataset.prvEnglishValidation = 'true';
        form.noValidate = true;
        form.setAttribute('novalidate', 'novalidate');

        form.addEventListener('submit', function (e) {
            var fields = form.querySelectorAll('input, select, textarea');
            var firstInvalid = null;

            fields.forEach(function (field) {
                clearFieldValidity(field);
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
            firstInvalid.setCustomValidity(englishValidityMessage(firstInvalid));
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

    document.querySelectorAll('.prv-pro-wizard-form').forEach(bindEnglishFormValidation);

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
