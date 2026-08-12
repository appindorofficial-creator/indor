(function () {
    function isSpanishUi() {
        return (document.documentElement.lang || '').toLowerCase().indexOf('es') === 0;
    }

    function msg(key, en, es) {
        var bag = window.IndorNrWizardMsgs;
        if (bag && bag[key]) {
            return bag[key];
        }
        return isSpanishUi() ? es : en;
    }

    function dateMinMessage() {
        return msg('dateMin', 'Choose today or a future date.', 'Elige hoy o una fecha futura.');
    }

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

    function validityMessage(field) {
        if (field.validity.valueMissing) {
            if (field.type === 'radio') {
                return msg('selectOne', 'Please choose one of these options.', 'Elige una de estas opciones.');
            }

            if (field.type === 'checkbox') {
                return msg('checkBox', 'Please check this box if you want to proceed.', 'Marca esta casilla si quieres continuar.');
            }

            if (field.tagName === 'SELECT') {
                return msg('selectItem', 'Please select an item in the list.', 'Selecciona un elemento de la lista.');
            }

            if (field.type === 'number') {
                return msg('enterNumber', 'Please enter a number.', 'Ingresa un número.');
            }

            if (field.type === 'date') {
                return msg('chooseDate', 'Please choose a date.', 'Elige una fecha.');
            }

            return msg('enterField', 'Please fill out this field.', 'Completa este campo.');
        }

        if (field.validity.typeMismatch) {
            return field.type === 'email'
                ? msg('validEmail', 'Please enter a valid email address.', 'Ingresa un correo electrónico válido.')
                : msg('validValue', 'Please enter a valid value.', 'Ingresa un valor válido.');
        }

        if (field.validity.tooLong) {
            return msg('shortenText', 'Please shorten this text.', 'Acorta este texto.');
        }

        if (field.validity.rangeUnderflow || field.validity.rangeOverflow) {
            if (field.type === 'date') {
                return dateMinMessage();
            }

            return field.validity.rangeUnderflow
                ? msg('higherValue', 'Please enter a higher value.', 'Ingresa un valor más alto.')
                : msg('lowerValue', 'Please enter a lower value.', 'Ingresa un valor más bajo.');
        }

        return msg('validValue', 'Please enter a valid value.', 'Ingresa un valor válido.');
    }

    function clearFieldValidity(field) {
        field.setCustomValidity('');

        if (field.type === 'radio' && field.name) {
            document.querySelectorAll('input[type="radio"][name="' + field.name + '"]').forEach(function (radio) {
                radio.setCustomValidity('');
            });
        }
    }

    function resolveDateMinMessage(input, minDate) {
        if (minDate && input.value && input.value < minDate) {
            return dateMinMessage();
        }

        return validityMessage(input);
    }

    function bindSingleDateInput(input) {
        if (input.dataset.nrDateMinBound === 'true') {
            return;
        }

        input.dataset.nrDateMinBound = 'true';
        input.removeAttribute('min');
        input.removeAttribute('max');

        var minDate = input.getAttribute('data-min-date') || '';
        if (minDate) {
            input.setAttribute('data-min-date', minDate);
        }

        function syncDateMinValidity() {
            if (!input.value) {
                input.setCustomValidity('');
                return;
            }

            input.setCustomValidity(minDate && input.value < minDate ? dateMinMessage() : '');
        }

        input.addEventListener('invalid', function (event) {
            event.preventDefault();
            input.setCustomValidity(resolveDateMinMessage(input, minDate));
        });

        input.addEventListener('input', syncDateMinValidity);
        input.addEventListener('change', syncDateMinValidity);
        input.addEventListener('blur', syncDateMinValidity);
        syncDateMinValidity();
    }

    function initDateInputs(root) {
        (root || document).querySelectorAll('.nr-wizard-page input[type="date"]').forEach(bindSingleDateInput);
    }

    function bindDateMinValidation(form) {
        form.querySelectorAll('input[type="date"]').forEach(bindSingleDateInput);
    }

    function bindFormValidation(form) {
        if (form.dataset.nrEnglishValidation === 'true') {
            return;
        }

        form.dataset.nrEnglishValidation = 'true';
        form.noValidate = true;
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
                    input.setCustomValidity(dateMinMessage());
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
            hideNavLoading();
            if (firstInvalid.type === 'date') {
                var minDate = firstInvalid.getAttribute('data-min-date') || '';
                firstInvalid.setCustomValidity(resolveDateMinMessage(firstInvalid, minDate));
            } else {
                firstInvalid.setCustomValidity(validityMessage(firstInvalid));
            }
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

            // Always follow the explicit BackUrl. history.back() fights the
            // pushState system-back guard and can restore a wizard step after
            // the draft was already published/cleared (bfcache), which then
            // kicks the user out of the flow on the next continue.
            markBusy(link);
        });
    });

    function hideNavLoading() {
        if (typeof window.indorHideNavigationLoading === 'function') {
            window.indorHideNavigationLoading();
        }
    }

    function submitWizardForm(form) {
        if (!form) return;
        if (typeof form.requestSubmit === 'function') {
            form.requestSubmit();
            return;
        }

        var submitEvent;
        try {
            submitEvent = new Event('submit', { bubbles: true, cancelable: true });
        } catch (err) {
            submitEvent = document.createEvent('Event');
            submitEvent.initEvent('submit', true, true);
        }

        if (!form.dispatchEvent(submitEvent)) {
            hideNavLoading();
            return;
        }

        HTMLFormElement.prototype.submit.call(form);
    }

    function bindWizardFooterSubmitButtons(root) {
        (root || document).querySelectorAll('[data-nr-wizard-submit]').forEach(function (btn) {
            if (btn.dataset.nrWizardSubmitBound === 'true') return;
            btn.dataset.nrWizardSubmitBound = 'true';

            btn.addEventListener('click', function (e) {
                e.preventDefault();
                var formId = btn.getAttribute('data-nr-wizard-submit');
                var form = formId ? document.getElementById(formId) : null;
                if (!form) return;
                submitWizardForm(form);
            });
        });
    }

    initDateInputs();
    document.querySelectorAll('.nr-step-form, .nr-edit-form').forEach(bindFormValidation);
    bindWizardFooterSubmitButtons();

    // When HTML5 / custom validation cancels submit, never leave INDOR loader up.
    document.addEventListener('submit', function (e) {
        var form = e.target;
        if (!form || form.tagName !== 'FORM') return;
        if (!form.classList.contains('nr-step-form') && !form.classList.contains('nr-edit-form')) return;

        window.setTimeout(function () {
            if (e.defaultPrevented) {
                hideNavLoading();
            }
        }, 0);
    }, false);

    window.addEventListener('pageshow', function () {
        clearBusy();
        hideNavLoading();
        initDateInputs();
        bindWizardFooterSubmitButtons();
    });
})();
