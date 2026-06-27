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
        form.querySelectorAll('input, select, textarea').forEach(function (field) {
            field.addEventListener('invalid', function () {
                field.setCustomValidity(englishValidityMessage(field));
            });
            field.addEventListener('input', function () {
                clearFieldValidity(field);
            });
            field.addEventListener('change', function () {
                clearFieldValidity(field);
            });
        });
    }

    document.querySelectorAll('.pa-portal-page form').forEach(bindEnglishFormValidation);
})();
