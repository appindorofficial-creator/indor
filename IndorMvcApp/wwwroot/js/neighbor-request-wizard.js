(function () {
    var DATE_MIN_MESSAGE = 'Choose today or a future date.';

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
            if (field.type === 'date') {
                return DATE_MIN_MESSAGE;
            }

            return 'Please enter a higher value.';
        }

        if (field.validity.rangeOverflow) {
            if (field.type === 'date') {
                return DATE_MIN_MESSAGE;
            }

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

    function bindDateMinValidation(form) {
        form.querySelectorAll('input[type="date"]').forEach(function (input) {
            if (input.dataset.nrDateMinBound === 'true') {
                return;
            }

            var minDate = input.getAttribute('data-min-date') || input.getAttribute('min');
            if (!minDate) {
                return;
            }

            input.dataset.nrDateMinBound = 'true';
            input.setAttribute('data-min-date', minDate);
            input.removeAttribute('min');

            function syncDateMinValidity() {
                if (!input.value) {
                    input.setCustomValidity('');
                    return;
                }

                input.setCustomValidity(input.value < minDate ? DATE_MIN_MESSAGE : '');
            }

            input.addEventListener('input', syncDateMinValidity);
            input.addEventListener('change', syncDateMinValidity);
            input.addEventListener('blur', syncDateMinValidity);
            syncDateMinValidity();
        });
    }

    function bindEnglishFormValidation(form) {
        if (form.dataset.nrEnglishValidation === 'true') {
            return;
        }

        form.dataset.nrEnglishValidation = 'true';
        form.setAttribute('novalidate', 'novalidate');
        bindDateMinValidation(form);

        form.addEventListener('submit', function (e) {
            var fields = form.querySelectorAll('input, select, textarea');
            var firstInvalid = null;

            fields.forEach(function (field) {
                clearFieldValidity(field);
            });

            form.querySelectorAll('input[type="date"][data-min-date]').forEach(function (input) {
                var minDate = input.getAttribute('data-min-date');
                if (input.value && minDate && input.value < minDate) {
                    input.setCustomValidity(DATE_MIN_MESSAGE);
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

    document.querySelectorAll('.nr-step-form, .nr-edit-form').forEach(bindEnglishFormValidation);

    window.addEventListener('pageshow', clearBusy);
})();
