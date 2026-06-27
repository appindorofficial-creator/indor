(function () {
    function isPlainLeftClick(e) {
        return !e.defaultPrevented && e.button === 0 && !e.metaKey && !e.ctrlKey && !e.shiftKey && !e.altKey;
    }

    function markBusy(el) {
        el.classList.add('is-busy');
        el.setAttribute('aria-busy', 'true');
    }

    function clearBusy() {
        document.querySelectorAll('.nr-wizard-nav-btn.is-busy').forEach(function (el) {
            el.classList.remove('is-busy');
            el.removeAttribute('aria-busy');
        });
    }

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

    document.querySelectorAll('a.nr-wizard-nav-btn[data-nr-history-back]').forEach(function (link) {
        link.addEventListener('click', function (e) {
            if (!isPlainLeftClick(e)) {
                return;
            }

            if (window.history.length > 1) {
                e.preventDefault();
                markBusy(link);
                window.history.back();
                return;
            }

            markBusy(link);
        });
    });

    document.querySelectorAll('.nr-step-form').forEach(bindEnglishFormValidation);

    window.addEventListener('pageshow', clearBusy);
})();
