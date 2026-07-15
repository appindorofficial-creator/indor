/**
 * Shared inline validation for homeowner service wizards (_LayoutServiceWizard).
 *
 * Contract:
 * - Validates forms inside .iw-wizard-page
 * - .field-card with single-choice controls (.choice-card, .segment-btn, …):
 *     require one .active unless data-svc-optional
 * - Multi-select cards (.sign-card, .area-chip, .symptom-pill, …):
 *     optional unless data-svc-required-group
 * - Explicit: data-svc-required="inputId" forces a non-empty #inputId
 * - Visible [required] inputs/selects/textareas: must be filled
 * - Errors render under each .field-card only (no top summary)
 */
(function () {
    var SINGLE_CHOICE = [
        '.choice-card',
        '.segment-btn',
        '.location-btn',
        '.yesno-btn',
        '.story-card',
        '.gutter-card',
        '.service-card',
        '.urgency-btn',
        '.option-btn',
        '.chip-btn',
        '.chip-option',
        '.help-tile',
        '.plan-tier-card',
        '.timing-card',
        '.access-card',
        '.flush-card'
    ].join(',');

    var MULTI_CHOICE = [
        '.sign-card',
        '.area-chip',
        '.symptom-pill',
        '.concern-chip',
        '.check-tile',
        '.check-card',
        '.multi-chip'
    ].join(',');

    function isSpanishUi() {
        return (document.documentElement.lang || '').toLowerCase().indexOf('es') === 0;
    }

    function selectMessage() {
        if (window.IndorSvcWizardMsgs && window.IndorSvcWizardMsgs.selectOne) {
            return window.IndorSvcWizardMsgs.selectOne;
        }
        return isSpanishUi() ? 'Elige una de estas opciones.' : 'Please choose one of these options.';
    }

    function enterMessage() {
        if (window.IndorSvcWizardMsgs && window.IndorSvcWizardMsgs.enterField) {
            return window.IndorSvcWizardMsgs.enterField;
        }
        return isSpanishUi() ? 'Completa este campo.' : 'Please fill out this field.';
    }

    function clearCardError(card) {
        if (!card) return;
        card.classList.remove('is-invalid');
        card.querySelectorAll('[data-svc-error="true"]').forEach(function (el) {
            el.remove();
        });
    }

    function showCardError(card, message) {
        if (!card) return;
        card.classList.add('is-invalid');
        if (card.querySelector('[data-svc-error="true"]')) return;
        var msg = document.createElement('div');
        msg.className = 'field-inline-error';
        msg.setAttribute('data-svc-error', 'true');
        msg.setAttribute('role', 'alert');
        msg.textContent = message;
        card.appendChild(msg);
    }

    function isOptionalCard(card) {
        return card.hasAttribute('data-svc-optional');
    }

    function cardHasActive(card, selector) {
        return !!card.querySelector(selector + '.active');
    }

    function isChoiceComplete(card) {
        if (card.hasAttribute('data-svc-required') || card.hasAttribute('data-whf-required')) {
            var id = card.getAttribute('data-svc-required') || card.getAttribute('data-whf-required');
            var input = document.getElementById(id);
            return !!(input && String(input.value || '').trim());
        }

        var hasMulti = !!card.querySelector(MULTI_CHOICE);
        var hasSingle = !!card.querySelector(SINGLE_CHOICE);

        if (hasMulti && !hasSingle) {
            if (card.hasAttribute('data-svc-required-group')) {
                return cardHasActive(card, MULTI_CHOICE);
            }
            return true;
        }

        if (hasSingle) {
            return cardHasActive(card, SINGLE_CHOICE);
        }

        return true;
    }

    function validateForm(form) {
        var firstInvalid = null;

        form.querySelectorAll('.field-card').forEach(function (card) {
            clearCardError(card);
            if (isOptionalCard(card)) return;
            if (isChoiceComplete(card)) return;
            showCardError(card, selectMessage());
            if (!firstInvalid) firstInvalid = card;
        });

        form.querySelectorAll('input[required], select[required], textarea[required]').forEach(function (field) {
            if (field.type === 'hidden' || field.disabled) return;
            if (field.type === 'radio' || field.type === 'checkbox') return;
            if ((field.value || '').trim()) {
                var okCard = field.closest('.field-card');
                if (okCard && !okCard.querySelector('[data-svc-error="true"]')) {
                    okCard.classList.remove('is-invalid');
                }
                return;
            }
            var card = field.closest('.field-card') || field.parentElement;
            showCardError(card, enterMessage());
            if (!firstInvalid) firstInvalid = card;
        });

        return firstInvalid;
    }

    function hideTopSummaries(root) {
        root.querySelectorAll('.validation-summary').forEach(function (summary) {
            summary.setAttribute('hidden', 'hidden');
            summary.setAttribute('aria-hidden', 'true');
        });
    }

    function bindForm(form) {
        if (!form || form.getAttribute('data-svc-validate-bound') === 'true') return;
        form.setAttribute('data-svc-validate-bound', 'true');
        form.setAttribute('novalidate', 'novalidate');

        form.addEventListener('submit', function (e) {
            if (e.submitter && e.submitter.getAttribute('formnovalidate') != null) return;
            if (e.submitter && String(e.submitter.value || '').toLowerCase() === 'back') return;
            if (e.submitter && String(e.submitter.getAttribute('name') || '') === 'action'
                && String(e.submitter.value || '').toLowerCase() === 'back') return;

            var firstInvalid = validateForm(form);
            if (firstInvalid) {
                e.preventDefault();
                e.stopPropagation();
                firstInvalid.scrollIntoView({ behavior: 'smooth', block: 'center' });
            }
        }, true);

        form.addEventListener('click', function (e) {
            var target = e.target;
            if (!target || !target.closest) return;
            var choice = target.closest(SINGLE_CHOICE + ',' + MULTI_CHOICE);
            if (!choice) return;
            var card = choice.closest('.field-card');
            if (!card) return;
            // Defer so host scripts can toggle .active first
            setTimeout(function () {
                if (isChoiceComplete(card) || isOptionalCard(card)) {
                    clearCardError(card);
                }
            }, 0);
        });

        form.addEventListener('input', function (e) {
            var field = e.target;
            if (!field || !field.closest) return;
            var card = field.closest('.field-card');
            if (!card) return;
            if (field.hasAttribute('required') && !(field.value || '').trim()) return;
            if (card.hasAttribute('data-svc-required')) {
                var input = document.getElementById(card.getAttribute('data-svc-required'));
                if (input && !(input.value || '').trim()) return;
            }
            clearCardError(card);
        });

        // Server redisplay: translate empty choice cards to inline alerts
        if (form.querySelector('.validation-summary li, .validation-summary .field-validation-error')
            || form.closest('.iw-wizard-page')?.querySelector('.validation-summary:not([hidden]) li')) {
            validateForm(form);
        }
    }

    function boot() {
        var root = document.querySelector('.iw-wizard-page') || document;
        hideTopSummaries(root);
        root.querySelectorAll('form').forEach(bindForm);

        // Re-hide summaries after framework scripts
        setTimeout(function () { hideTopSummaries(root); }, 0);
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', boot);
    } else {
        boot();
    }

    window.IndorSvcWizardValidation = {
        validate: validateForm,
        bind: bindForm
    };
})();
