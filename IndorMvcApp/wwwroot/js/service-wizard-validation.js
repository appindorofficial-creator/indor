/**
 * Shared inline validation for homeowner service wizards (_LayoutServiceWizard).
 *
 * Contract:
 * - Validates forms inside .iw-wizard-page
 * - .field-card with single-choice controls: require one .active unless data-svc-optional
 * - Multi-select cards: optional unless data-svc-required-group
 * - Explicit: data-svc-required="inputId" / data-whf-required forces non-empty #inputId
 * - data-svc-or-unknown="hiddenId": complete if hidden is "true", else require filled visible inputs
 * - Skips cards that are hidden / display:none
 * - Visible [required] inputs/selects/textareas: must be filled
 * - Errors render under each .field-card only (no top summary)
 */
(function () {
    var SINGLE_CHOICE = [
        '.choice-card',
        '.choice-btn',
        '.segment-btn',
        '.location-btn',
        '.location-tile',
        '.loc-btn',
        '.yesno-btn',
        '.story-card',
        '.gutter-card',
        '.service-card',
        '.service-option',
        '.service-tile',
        '.service-type-btn',
        '.urgency-btn',
        '.option-btn',
        '.chip-btn',
        '.chip-option',
        '.help-tile',
        '.help-btn',
        '.plan-tier-card',
        '.timing-card',
        '.timing-chip',
        '.access-card',
        '.access-btn',
        '.flush-card',
        '.need-card',
        '.count-btn',
        '.count-card',
        '.provider-card',
        '.issue-btn',
        '.issue-card',
        '.tile-btn',
        '.type-card',
        '.time-btn',
        '.time-card',
        '.area-card',
        '.area-btn',
        '.area-pill',
        '.reason-card',
        '.reminder-btn',
        '.freq-card',
        '.freq-chip',
        '.crew-card',
        '.hour-btn',
        '.contact-card',
        '.action-card',
        '.action-tile',
        '.action-btn',
        '.bin-card',
        '.day-btn',
        '.material-card',
        '.story-pill',
        '.spigot-card',
        '.goal-card',
        '.goal-btn',
        '.paint-age-btn',
        '.surface-card',
        '.color-card',
        '.color-update-card',
        '.addon-pill',
        '.symptom-btn',
        '.extra-btn',
        '.part-btn',
        '.focus-btn',
        '.concern-btn',
        '.concern-card',
        '.cable-card',
        '.pref-btn'
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

    function isCardVisible(card) {
        if (!card || card.hasAttribute('hidden')) return false;
        if (card.getAttribute('aria-hidden') === 'true') return false;
        var style = window.getComputedStyle(card);
        if (style.display === 'none' || style.visibility === 'hidden') return false;
        return card.offsetParent !== null || style.position === 'fixed';
    }

    function cardHasActive(card, selector) {
        return !!card.querySelector(selector + '.active');
    }

    function isUnknownOrFilledComplete(card) {
        var unknownId = card.getAttribute('data-svc-or-unknown');
        var unknown = unknownId ? document.getElementById(unknownId) : null;
        if (unknown && String(unknown.value || '').toLowerCase() === 'true') {
            return true;
        }

        var inputs = card.querySelectorAll('input:not([type="hidden"]):not([type="checkbox"]):not([type="radio"]):not(:disabled), textarea:not(:disabled)');
        if (!inputs.length) {
            return false;
        }
        for (var i = 0; i < inputs.length; i++) {
            if (!String(inputs[i].value || '').trim()) {
                return false;
            }
        }
        return true;
    }

    function isChoiceComplete(card) {
        if (card.hasAttribute('data-svc-or-unknown')) {
            return isUnknownOrFilledComplete(card);
        }

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
            if (!isCardVisible(card)) return;
            if (isOptionalCard(card)) return;
            if (isChoiceComplete(card)) return;
            var msg = card.hasAttribute('data-svc-or-unknown') ? enterMessage() : selectMessage();
            showCardError(card, msg);
            if (!firstInvalid) firstInvalid = card;
        });

        form.querySelectorAll('input[required], select[required], textarea[required]').forEach(function (field) {
            if (field.type === 'hidden' || field.disabled) return;
            if (field.type === 'radio' || field.type === 'checkbox') return;
            var hostCard = field.closest('.field-card');
            if (hostCard && !isCardVisible(hostCard)) return;
            if ((field.value || '').trim()) {
                var okCard = field.closest('.field-card');
                if (okCard && !okCard.querySelector('[data-svc-error="true"]')) {
                    okCard.classList.remove('is-invalid');
                }
                return;
            }
            var card = field.closest('.field-card') || field.parentElement;
            if (card && card.classList && card.classList.contains('field-card') && !isCardVisible(card)) return;
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
            var choice = target.closest(SINGLE_CHOICE + ',' + MULTI_CHOICE + ',[data-svc-or-unknown] button, .link-btn');
            if (!choice) return;
            var card = choice.closest('.field-card');
            if (!card) return;
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
            if (card.hasAttribute('data-svc-or-unknown') && !isUnknownOrFilledComplete(card)) return;
            clearCardError(card);
        });

        if (form.querySelector('.validation-summary li, .validation-summary .field-validation-error')
            || form.closest('.iw-wizard-page')?.querySelector('.validation-summary:not([hidden]) li')) {
            validateForm(form);
        }
    }

    function boot() {
        var root = document.querySelector('.iw-wizard-page') || document;
        hideTopSummaries(root);
        root.querySelectorAll('form').forEach(bindForm);
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
