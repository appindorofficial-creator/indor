/**
 * Shared inline validation for homeowner service wizards (_LayoutServiceWizard).
 *
 * Contract:
 * - Validates forms inside .iw-wizard-page
 * - .field-card with single-choice controls: require one .active unless data-svc-optional
 * - Multi-select cards: optional unless data-svc-required-group
 * - Explicit: data-svc-required="inputId" / data-whf-required forces non-empty #inputId
 * - data-svc-address: required street address must include city/state hint (comma or ZIP)
 * - Skips cards that are hidden / display:none
 * - Visible [required] inputs/selects/textareas: must be filled
 * - Errors render under each .field-card only (no top summary)
 * - Server ModelState field errors (asp-validation-for) are promoted to .field-card.is-invalid
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
        '.help-row',
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
        '.time-slot',
        '.area-card',
        '.area-btn',
        '.area-pill',
        '.reason-card',
        '.reminder-card',
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
        '.date-chip',
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
        '.pref-btn',
        '.service-block',
        '.prv-chip',
        '.prv-entry-card',
        '.prv-type-card',
        '.prv-wiz-slot'
    ].join(',');

    var MULTI_CHOICE = [
        '.sign-card',
        '.area-card',
        '.issue-card',
        '.area-chip',
        '.symptom-pill',
        '.concern-chip',
        '.concern-card',
        '.check-tile',
        '.check-card',
        '.csc-check-item',
        '.multi-chip',
        '.prv-chip'
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

    function completeFieldsMessage() {
        if (window.IndorSvcWizardMsgs && window.IndorSvcWizardMsgs.completeFields) {
            return window.IndorSvcWizardMsgs.completeFields;
        }
        return isSpanishUi()
            ? 'Completa los campos marcados para continuar.'
            : 'Please complete the highlighted fields to continue.';
    }

    function clearFormError(form) {
        if (!form) return;
        form.querySelectorAll('[data-svc-form-error="true"]').forEach(function (el) {
            el.remove();
        });
    }

    function showFormError(form, message) {
        if (!form) return;
        var existing = form.querySelector('[data-svc-form-error="true"]');
        if (!existing) {
            existing = document.createElement('div');
            existing.className = 'field-inline-error svc-form-error';
            existing.setAttribute('data-svc-form-error', 'true');
            existing.setAttribute('role', 'alert');
            var footer = form.querySelector('.sticky-footer');
            var btn = form.querySelector('.btn-primary, button[type="submit"]');
            if (footer) {
                footer.insertBefore(existing, footer.firstChild);
            } else if (btn && btn.parentElement) {
                btn.parentElement.insertBefore(existing, btn);
            } else {
                form.appendChild(existing);
            }
        }
        existing.textContent = message;
    }

    function clearCardError(card) {
        if (!card) return;
        card.classList.remove('is-invalid');
        card.querySelectorAll('[data-svc-error="true"]').forEach(function (el) {
            el.remove();
        });
        card.querySelectorAll('.field-validation-error').forEach(function (el) {
            el.classList.remove('field-validation-error');
            el.classList.add('field-validation-valid');
            el.textContent = '';
        });
    }

    function showCardError(card, message) {
        if (!card) return;
        card.classList.add('is-invalid');
        var existing = card.querySelector('[data-svc-error="true"]');
        if (existing) {
            existing.textContent = message;
            return;
        }
        var serverMsg = card.querySelector('[data-valmsg-for]');
        if (serverMsg) {
            serverMsg.classList.remove('field-validation-valid');
            serverMsg.classList.add('field-validation-error', 'field-inline-error');
            serverMsg.setAttribute('role', 'alert');
            serverMsg.textContent = message;
            return;
        }
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

    function isTextLikeRequiredTarget(card) {
        var id = card.getAttribute('data-svc-required') || card.getAttribute('data-whf-required');
        if (!id) return false;
        var input = document.getElementById(id);
        if (!input) return false;

        var tag = (input.tagName || '').toUpperCase();
        if (tag === 'TEXTAREA' || tag === 'SELECT') return true;
        if (tag === 'INPUT') {
            var type = String(input.type || 'text').toLowerCase();
            if (type !== 'hidden' && type !== 'checkbox' && type !== 'radio' && type !== 'button' && type !== 'submit') {
                return true;
            }
            // Hidden value synced from a visible text/date/select control in the same card
            if (type === 'hidden' && card.querySelector('input:not([type="hidden"]), select, textarea')) {
                return true;
            }
        }
        return false;
    }

    function addressMessage() {
        if (window.IndorSvcWizardMsgs && window.IndorSvcWizardMsgs.address) {
            return window.IndorSvcWizardMsgs.address;
        }
        return isSpanishUi()
            ? 'Ingresa una dirección completa con ciudad y estado (p. ej. 123 Main St, Charlotte, NC).'
            : 'Enter a complete address with city and state (e.g. 123 Main St, Charlotte, NC).';
    }

    function isValidStreetAddress(value) {
        var address = String(value || '').trim();
        if (address.length < 5) return false;
        if (!/\p{L}/u.test(address)) return false;
        if (/^[\d\s.,#-]+$/.test(address)) return false;
        var tokens = address.split(/\s+/).filter(Boolean);
        var hasDigit = /\d/.test(address);
        var wordParts = tokens.filter(function (part) { return /\p{L}/u.test(part); }).length;
        if (!hasDigit || wordParts < 2) return false;
        var hasComma = address.indexOf(',') >= 0;
        var hasZip = tokens.slice(1).some(function (t) {
            return /^\d{5}(-\d{4})?$/.test(String(t).replace(/[.,]+$/, ''));
        });
        return hasComma || hasZip;
    }

    function requiredInputForCard(card) {
        var id = card.getAttribute('data-svc-required') || card.getAttribute('data-whf-required');
        return id ? document.getElementById(id) : null;
    }

    function incompleteCardMessage(card) {
        if (card.hasAttribute('data-svc-address')) {
            var input = requiredInputForCard(card);
            if (input && String(input.value || '').trim() && !isValidStreetAddress(input.value)) {
                return addressMessage();
            }
        }
        if (card.hasAttribute('data-svc-or-unknown') || isTextLikeRequiredTarget(card)) {
            return enterMessage();
        }
        return selectMessage();
    }

    function isCardVisible(card) {
        if (!card || card.hasAttribute('hidden')) return false;
        if (card.getAttribute('aria-hidden') === 'true') return false;
        var style = window.getComputedStyle(card);
        if (style.display === 'none' || style.visibility === 'hidden') return false;
        return card.offsetParent !== null || style.position === 'fixed';
    }

    function cardHasActive(card, selector) {
        return !!card.querySelector(selector + '.active, ' + selector + '.is-selected, ' + selector + ':has(input:checked)');
    }

    function isUnknownOrFilledComplete(card) {
        var unknownId = card.getAttribute('data-svc-or-unknown');
        var unknown = unknownId ? document.getElementById(unknownId) : null;
        if (unknown) {
            var type = String(unknown.type || '').toLowerCase();
            if (type === 'checkbox' || type === 'radio') {
                if (unknown.checked) return true;
            } else if (String(unknown.value || '').toLowerCase() === 'true') {
                return true;
            }
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
            if (!input) {
                return false;
            }
            var inputType = String(input.type || '').toLowerCase();
            if (inputType === 'checkbox' || inputType === 'radio') {
                if (!input.checked) {
                    return false;
                }
            } else if (!String(input.value || '').trim()) {
                return false;
            }
            if (card.hasAttribute('data-svc-address') && !isValidStreetAddress(input.value)) {
                return false;
            }
            return true;
        }

        var hasMulti = !!card.querySelector(MULTI_CHOICE);
        var hasSingle = !!card.querySelector(SINGLE_CHOICE);

        if (card.hasAttribute('data-svc-required-group')) {
            return !!card.querySelector('input[type="checkbox"]:checked, input[type="radio"]:checked')
                || cardHasActive(card, MULTI_CHOICE)
                || cardHasActive(card, SINGLE_CHOICE);
        }

        if (hasMulti && !hasSingle) {
            return true;
        }

        if (hasSingle) {
            return cardHasActive(card, SINGLE_CHOICE);
        }

        return true;
    }

    function validateForm(form) {
        var firstInvalid = null;
        clearFormError(form);

        form.querySelectorAll('.field-card').forEach(function (card) {
            clearCardError(card);
            if (!isCardVisible(card)) return;
            if (isOptionalCard(card)) return;
            if (isChoiceComplete(card)) return;
            var msg = incompleteCardMessage(card);
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

    function promoteServerFieldErrors(root) {
        root.querySelectorAll('.field-card .field-validation-error').forEach(function (el) {
            var text = String(el.textContent || '').trim();
            if (!text) return;
            var card = el.closest('.field-card');
            if (!card) return;
            card.classList.add('is-invalid');
            el.setAttribute('role', 'alert');
            if (!el.classList.contains('field-inline-error')) {
                el.classList.add('field-inline-error');
            }
        });
    }

    function hideTopSummaries(root) {
        root.querySelectorAll('.validation-summary, .ob-summary').forEach(function (summary) {
            summary.setAttribute('hidden', 'hidden');
            summary.setAttribute('aria-hidden', 'true');
        });
    }

    function enhanceProviderWizardCards(root) {
        if (!root || !root.classList || !root.classList.contains('prv-page')) {
            return;
        }

        root.querySelectorAll('form .ob-field').forEach(function (field) {
            if (field.classList.contains('field-card')) return;
            var input = field.querySelector('input:not([type="hidden"]):not([type="checkbox"]):not([type="radio"]), select, textarea');
            if (!input) return;
            if (!input.id && input.name) {
                input.id = String(input.name).replace(/[^\w-]/g, '_');
            }
            field.classList.add('field-card');
            if (input.hasAttribute('required') || input.getAttribute('data-pa-required') === 'true') {
                field.setAttribute('data-svc-required', input.id);
                input.removeAttribute('required');
            } else {
                field.setAttribute('data-svc-optional', '');
            }
        });

        root.querySelectorAll('form .prv-exam-q').forEach(function (card) {
            card.classList.add('field-card');
            card.setAttribute('data-svc-required-group', '');
            card.querySelectorAll('[required]').forEach(function (el) {
                el.removeAttribute('required');
            });
        });

        root.querySelectorAll('form .prv-check-row, form .prv-wiz-terms').forEach(function (row) {
            var input = row.querySelector('input[type="checkbox"], input[type="radio"]');
            if (!input) return;
            row.classList.add('field-card');
            if (!input.id) {
                input.id = input.name ? String(input.name).replace(/[^\w-]/g, '_') : ('prvCheck' + Math.random().toString(36).slice(2, 8));
            }
            if (input.hasAttribute('required') || input.getAttribute('data-pa-required') === 'true') {
                row.setAttribute('data-svc-required', input.id);
                input.removeAttribute('required');
            } else if (!row.hasAttribute('data-svc-optional') && !row.hasAttribute('data-svc-required')) {
                row.setAttribute('data-svc-optional', '');
            }
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
                && ['back', 'skip'].includes(String(e.submitter.value || '').toLowerCase())) return;

            var firstInvalid = validateForm(form);
            if (firstInvalid) {
                e.preventDefault();
                e.stopPropagation();
                // Continuar can flash the full-screen "Cargando..." cover before submit
                // is cancelled (esp. WKWebView). Always clear it when staying on-page.
                if (typeof window.indorHideNavigationLoading === 'function') {
                    window.indorHideNavigationLoading();
                }
                firstInvalid.scrollIntoView({ behavior: 'smooth', block: 'start' });
                var focusable = firstInvalid.querySelector(
                    'input:not([type="hidden"]):not([type="button"]):not([type="submit"]):not([disabled]), select:not([disabled]), textarea:not([disabled])'
                );
                if (focusable && typeof focusable.focus === 'function') {
                    try { focusable.focus({ preventScroll: true }); } catch (err) { focusable.focus(); }
                }
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
                var hostForm = card.closest('form');
                if (hostForm && !hostForm.querySelector('.field-card.is-invalid')) {
                    clearFormError(hostForm);
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
        enhanceProviderWizardCards(root);
        hideTopSummaries(root);
        promoteServerFieldErrors(root);
        root.querySelectorAll('form').forEach(bindForm);
        setTimeout(function () {
            hideTopSummaries(root);
            promoteServerFieldErrors(root);
        }, 0);
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
