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

        if (field.validity.rangeUnderflow || field.validity.rangeOverflow) {
            if (field.type === 'date') {
                return DATE_MIN_MESSAGE;
            }

            return field.validity.rangeUnderflow
                ? 'Please enter a higher value.'
                : 'Please enter a lower value.';
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

    function resolveDateMinMessage(input, minDate) {
        if (minDate && input.value && input.value < minDate) {
            return DATE_MIN_MESSAGE;
        }

        return englishValidityMessage(input);
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

            input.setCustomValidity(minDate && input.value < minDate ? DATE_MIN_MESSAGE : '');
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

    function bindEnglishFormValidation(form) {
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
            hideNavLoading();
            if (firstInvalid.type === 'date') {
                var minDate = firstInvalid.getAttribute('data-min-date') || '';
                firstInvalid.setCustomValidity(resolveDateMinMessage(firstInvalid, minDate));
            } else {
                firstInvalid.setCustomValidity(englishValidityMessage(firstInvalid));
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
    document.querySelectorAll('.nr-step-form, .nr-edit-form').forEach(bindEnglishFormValidation);
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
