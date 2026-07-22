(function () {
    /**
     * Reusable PA service-flow validation.
     *
     * Bind targets: form.pa-emergency-form | form.pa-flow-form | form.pa-preventive-form | form[data-pa-flow-validate]
     *
     * Contract:
     * - Radios: required unless the question has data-pa-optional
     * - Checkbox groups: required when
     *     • question/section has data-pa-required-group, or
     *     • name is UpdateRecipientsList / UpdateRecipients / SelectedServices, or
     *     • question contains only checkboxes (and is not data-pa-optional)
     * - Text/select/textarea with required: must be non-empty
     * - Contact phone ([data-pa-contact-phone] / [data-phone-input][required]): exactly 10 digits
     * - Errors render only under each .pa-flow-question / [data-pa-required-group] / .pa-prev-section
     */
    function isSpanishUi() {
        return (document.documentElement.lang || '').toLowerCase().indexOf('es') === 0;
    }

    function foldText(value) {
        return String(value || '')
            .normalize('NFD')
            .replace(/[\u0300-\u036f]/g, '')
            .toLowerCase()
            .trim();
    }

    function questionFor(field) {
        return field.closest('.pa-flow-question, [data-pa-required-group], [data-pa-validate-group], .pa-prev-section');
    }

    function isOptionalQuestion(question) {
        return !!(question && question.hasAttribute('data-pa-optional'));
    }

    function selectMessage() {
        return isSpanishUi() ? 'Elige una de estas opciones.' : 'Please choose one of these options.';
    }

    function enterMessage() {
        return isSpanishUi() ? 'Completa este campo.' : 'Please fill out this field.';
    }

    function phoneMessage() {
        return isSpanishUi()
            ? 'Ingresa un teléfono de EE. UU. de 10 dígitos.'
            : 'Enter a valid 10-digit US phone number.';
    }

    function normalizePhoneDigits(value) {
        if (window.IndorPhoneInput && typeof window.IndorPhoneInput.normalize === 'function') {
            return window.IndorPhoneInput.normalize(value);
        }
        var digits = String(value || '').replace(/\D/g, '');
        if (digits.length === 11 && digits.charAt(0) === '1') {
            digits = digits.slice(1);
        }
        return digits.slice(0, 10);
    }

    function isContactPhoneField(field) {
        return !!(field && field.matches && field.matches('[data-pa-contact-phone], input[data-phone-input][required]'));
    }

    function isValidRequiredPhone(value) {
        return normalizePhoneDigits(value).length === 10;
    }

    function clearFlowErrors(form) {
        form.querySelectorAll('.pa-inline-error[data-pa-flow-error="true"]').forEach(function (el) {
            el.remove();
        });
        form.querySelectorAll('.pa-invalid').forEach(function (el) {
            el.classList.remove('pa-invalid');
        });
    }

    function showQuestionError(question, message) {
        if (!question) {
            return;
        }
        question.classList.add('pa-invalid');
        if (question.querySelector('.pa-inline-error[data-pa-flow-error="true"]')) {
            return;
        }
        var msg = document.createElement('div');
        msg.className = 'pa-inline-error';
        msg.setAttribute('data-pa-flow-error', 'true');
        msg.setAttribute('role', 'alert');
        msg.textContent = message;
        question.appendChild(msg);
    }

    function showFieldError(field, message) {
        var wrap = field.closest('.pa-field') || field.parentElement;
        if (wrap) {
            wrap.classList.add('pa-invalid');
        }
        var question = questionFor(field);
        if (question) {
            showQuestionError(question, message);
            return;
        }
        if (wrap && !wrap.querySelector('.pa-inline-error[data-pa-flow-error="true"]')) {
            var msg = document.createElement('div');
            msg.className = 'pa-inline-error';
            msg.setAttribute('data-pa-flow-error', 'true');
            msg.setAttribute('role', 'alert');
            msg.textContent = message;
            wrap.insertAdjacentElement('afterend', msg);
        }
    }

    function radioGroupMissing(form, name) {
        return !form.querySelector('input[type="radio"][name="' + CSS.escape(name) + '"]:checked');
    }

    function checkboxGroupMissing(form, name) {
        var boxes = form.querySelectorAll('input[type="checkbox"][name="' + CSS.escape(name) + '"]');
        if (!boxes.length) {
            return false;
        }
        return !Array.prototype.some.call(boxes, function (box) { return box.checked; });
    }

    function shouldRequireCheckboxGroup(form, name) {
        var box = form.querySelector('input[type="checkbox"][name="' + CSS.escape(name) + '"]');
        if (!box) {
            return false;
        }

        var question = questionFor(box);
        if (isOptionalQuestion(question)) {
            return false;
        }

        if (question && question.hasAttribute('data-pa-required-group')) {
            return true;
        }

        if (/^(UpdateRecipientsList|UpdateRecipients|SelectedServices)$/i.test(name)) {
            return true;
        }

        if (!question) {
            return false;
        }

        var hasRadio = !!question.querySelector('input[type="radio"]');
        var hasCheckbox = !!question.querySelector('input[type="checkbox"]');
        return hasCheckbox && !hasRadio;
    }

    function shouldRequireRadioGroup(form, name) {
        var radio = form.querySelector('input[type="radio"][name="' + CSS.escape(name) + '"]');
        if (!radio) {
            return false;
        }
        return !isOptionalQuestion(questionFor(radio));
    }

    function isRequiredCheckboxIssue(form, field) {
        if (!field || field.type !== 'checkbox' || !field.name) {
            return false;
        }
        return shouldRequireCheckboxGroup(form, field.name) && checkboxGroupMissing(form, field.name);
    }

    function collectFlowIssues(form) {
        var issues = [];
        var seenRadios = {};
        var seenCheckboxes = {};

        form.querySelectorAll('input[type="radio"][name]').forEach(function (radio) {
            if (seenRadios[radio.name] || !shouldRequireRadioGroup(form, radio.name)) {
                return;
            }
            seenRadios[radio.name] = true;
            if (!radioGroupMissing(form, radio.name)) {
                return;
            }
            issues.push({ field: radio, question: questionFor(radio), message: selectMessage() });
        });

        form.querySelectorAll('input[type="checkbox"][name]').forEach(function (box) {
            if (seenCheckboxes[box.name] || !shouldRequireCheckboxGroup(form, box.name)) {
                return;
            }
            seenCheckboxes[box.name] = true;
            if (!checkboxGroupMissing(form, box.name)) {
                return;
            }
            issues.push({ field: box, question: questionFor(box), message: selectMessage() });
        });

        form.querySelectorAll('input[required], select[required], textarea[required]').forEach(function (field) {
            if (field.type === 'radio' || field.type === 'checkbox' || field.type === 'hidden') {
                return;
            }
            if (isContactPhoneField(field)) {
                if (isValidRequiredPhone(field.value)) {
                    return;
                }
                issues.push({
                    field: field,
                    question: questionFor(field),
                    message: normalizePhoneDigits(field.value) ? phoneMessage() : enterMessage()
                });
                return;
            }
            if ((field.value || '').trim()) {
                return;
            }
            issues.push({ field: field, question: questionFor(field), message: enterMessage() });
        });

        return issues;
    }

    function validateFlowForm(form) {
        clearFlowErrors(form);
        var issues = collectFlowIssues(form);
        if (!issues.length) {
            return true;
        }

        issues.forEach(function (issue) {
            if (issue.question) {
                showQuestionError(issue.question, issue.message);
            } else {
                showFieldError(issue.field, issue.message);
            }
        });

        var first = issues[0].question || issues[0].field;
        if (first && first.scrollIntoView) {
            first.scrollIntoView({ behavior: 'smooth', block: 'center' });
        }
        return false;
    }

    function clearQuestionIfValid(form, field) {
        var question = questionFor(field);
        if (!question || !question.classList.contains('pa-invalid')) {
            return;
        }

        if (field.type === 'radio' && radioGroupMissing(form, field.name)) {
            return;
        }
        if (isRequiredCheckboxIssue(form, field)) {
            return;
        }
        if (field.hasAttribute('required')
            && field.type !== 'radio'
            && field.type !== 'checkbox'
            && !(field.value || '').trim()) {
            return;
        }
        if (isContactPhoneField(field) && !isValidRequiredPhone(field.value)) {
            return;
        }

        question.classList.remove('pa-invalid');
        var inline = question.querySelector('.pa-inline-error[data-pa-flow-error="true"]');
        if (inline) {
            inline.remove();
        }
    }

    function bindFlowFormValidation(form) {
        // Native bubbles cannot attach to hidden chip/toggle inputs.
        form.setAttribute('novalidate', 'novalidate');
        form.setAttribute('data-pa-flow-bound', 'true');

        form.addEventListener('submit', function (e) {
            if (!validateFlowForm(form)) {
                e.preventDefault();
                e.stopPropagation();
            }
        });

        form.addEventListener('change', function (e) {
            if (e.target && e.target.matches) {
                clearQuestionIfValid(form, e.target);
            }
        });

        form.addEventListener('input', function (e) {
            if (e.target && e.target.matches) {
                clearQuestionIfValid(form, e.target);
            }
        });
    }

    var flowFormSelector = [
        '.pa-portal-page form.pa-emergency-form',
        '.pa-portal-page form.pa-flow-form',
        '.pa-portal-page form.pa-preventive-form',
        '.pa-portal-page form[data-pa-flow-validate]'
    ].join(', ');

    document.querySelectorAll(flowFormSelector).forEach(bindFlowFormValidation);

    window.IndorPaFlowValidation = {
        validate: validateFlowForm,
        bind: bindFlowFormValidation
    };

    function bindServiceSearch(root) {
        var input = root.querySelector('input[type="search"]');
        if (!input) {
            return;
        }

        var shell = root.closest('.pa-portal-shell') || document;
        var empty = shell.querySelector('[data-pa-service-search-empty]');
        var focusBtn = root.querySelector('[data-pa-service-search-focus]');

        function applyFilter() {
            var query = foldText(input.value);
            var visibleCount = 0;

            shell.querySelectorAll('[data-pa-service-section]').forEach(function (section) {
                var sectionVisible = 0;
                section.querySelectorAll('[data-pa-service-item]').forEach(function (item) {
                    var haystack = foldText(item.getAttribute('data-search') || item.textContent);
                    var match = !query || haystack.indexOf(query) !== -1;
                    item.hidden = !match;
                    if (match) {
                        sectionVisible += 1;
                        visibleCount += 1;
                    }
                });
                section.hidden = query.length > 0 && sectionVisible === 0;
            });

            shell.querySelectorAll('.pa-emergency-banner, .pa-trust-row').forEach(function (el) {
                el.hidden = query.length > 0;
            });

            if (empty) {
                empty.hidden = !(query.length > 0 && visibleCount === 0);
            }
        }

        input.addEventListener('input', applyFilter);
        input.addEventListener('search', applyFilter);

        if (focusBtn) {
            focusBtn.addEventListener('click', function () {
                input.focus();
                input.select();
            });
        }
    }

    document.querySelectorAll('[data-pa-service-search]').forEach(bindServiceSearch);

    /**
     * Group Continue / Edit / Back into .pa-flow-actions (document-flow footer).
     * Also pulls orphan Edit/Back links that sit immediately after the form
     * (common on review screens) so they stay with the submit CTA.
     */
    function promotePaFlowActions(root) {
        var scope = root && root.querySelectorAll ? root : document;
        // Include .pa-portal-shell--flow so confirmed screens (buttons outside a form) get the same stacked footer.
        scope.querySelectorAll('form.pa-emergency-form, form.pa-flow-form, form.pa-preventive-form, .pa-portal-shell--flow > form, .pa-portal-shell--flow').forEach(function (container) {
            if (container.dataset.paFlowActionsBound === 'true') {
                return;
            }

            var footer = container.querySelector(':scope > .pa-flow-actions');
            var submit = footer
                ? footer.querySelector('.pa-flow-submit, a.pa-flow-submit')
                : container.querySelector(':scope > .pa-flow-submit, :scope > a.pa-flow-submit');

            if (!submit) {
                container.dataset.paFlowActionsBound = 'true';
                return;
            }

            if (!footer) {
                var actions = [submit];
                var cursor = submit.nextElementSibling;
                while (cursor && cursor.classList.contains('pa-flow-edit')) {
                    actions.push(cursor);
                    cursor = cursor.nextElementSibling;
                }
                if (cursor && (cursor.classList.contains('pa-flow-back-link') || cursor.classList.contains('pa-back-link'))) {
                    actions.push(cursor);
                }

                footer = document.createElement('div');
                footer.className = 'pa-flow-actions';
                submit.parentNode.insertBefore(footer, submit);
                actions.forEach(function (node) {
                    footer.appendChild(node);
                });
            }

            // Review screens often leave Edit/Back as siblings after </form>.
            var after = container.nextElementSibling;
            while (after && (after.classList.contains('pa-flow-edit')
                || after.classList.contains('pa-flow-back-link')
                || after.classList.contains('pa-back-link'))) {
                var next = after.nextElementSibling;
                footer.appendChild(after);
                after = next;
            }

            container.dataset.paFlowActionsBound = 'true';
        });
    }

    promotePaFlowActions(document);
})();
