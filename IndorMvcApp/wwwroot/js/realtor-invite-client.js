(function () {
    'use strict';

    var emailFormatRegex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*\.[a-zA-Z]{2,}$/;

    function hasLetter(text) {
        return /\p{L}/u.test(text);
    }

    function setFormFieldError(formId, fieldKey, message) {
        var form = document.getElementById(formId);
        if (!form) {
            return;
        }

        var errorEl = form.querySelector('[data-client-error="' + fieldKey + '"]');
        var input = form.querySelector('[name="' + fieldKey + '"]');
        var fieldWrap = input ? input.closest('.rl-field, .rl-cp-field') : null;

        if (errorEl) {
            errorEl.textContent = message || '';
        }
        if (input) {
            input.setAttribute('aria-invalid', message ? 'true' : 'false');
        }
        if (fieldWrap) {
            fieldWrap.classList.toggle('is-error', !!message);
        }
    }

    function clearFormErrors(formId, fieldKeys, bannerId) {
        fieldKeys.forEach(function (key) {
            setFormFieldError(formId, key, '');
        });
        var banner = document.getElementById(bannerId);
        if (banner) {
            banner.remove();
        }
    }

    function showFormErrorBanner(formId, bannerId, messages) {
        var form = document.getElementById(formId);
        if (!form || !messages.length) {
            return;
        }

        var existing = document.getElementById(bannerId);
        if (existing) {
            existing.remove();
        }

        var banner = document.createElement('div');
        banner.className = 'rl-error-banner';
        banner.id = bannerId;
        banner.setAttribute('role', 'alert');
        banner.innerHTML =
            '<i class="fas fa-circle-exclamation"></i>' +
            '<div><strong>Please fix the following to continue:</strong>' +
            '<ul class="rl-error-list">' +
            messages.map(function (msg) { return '<li>' + msg + '</li>'; }).join('') +
            '</ul></div>';

        var summary = form.querySelector('.ob-summary');
        if (summary && summary.nextSibling) {
            form.insertBefore(banner, summary.nextSibling);
        } else {
            form.insertBefore(banner, form.firstChild.nextSibling);
        }

        banner.scrollIntoView({ behavior: 'smooth', block: 'center' });
    }

    function validateFullName(value) {
        var name = (value || '').trim();
        if (!name) {
            return 'Full name is required.';
        }
        if (name.length < 2) {
            return 'Full name must be at least 2 characters.';
        }
        if (!hasLetter(name)) {
            return 'Enter a valid full name using letters (e.g. John Smith).';
        }
        if (/^[\d\s]+$/.test(name)) {
            return 'Full name cannot contain only numbers.';
        }
        var parts = name.split(/\s+/).filter(Boolean);
        if (parts.length < 2 || parts.some(function (part) { return !hasLetter(part); })) {
            return "Enter the client's first and last name.";
        }
        return '';
    }

    function validateEmail(value) {
        var email = (value || '').trim();
        if (!email) {
            return 'Email address is required.';
        }
        if (email.length > 254 || !emailFormatRegex.test(email)) {
            return 'Enter a valid email address (e.g. name@email.com).';
        }
        return '';
    }

    function validatePhone(value) {
        var digits = window.IndorPhoneInput
            ? window.IndorPhoneInput.normalize(value)
            : (value || '').replace(/\D/g, '').slice(0, 10);
        if (!digits) {
            return 'Phone number is required.';
        }
        if (digits.length !== 10) {
            return 'Enter a valid 10-digit US phone number (e.g. 555 123 4567).';
        }
        return '';
    }

    function validateZip(value) {
        if (window.IndorZipInput) {
            return window.IndorZipInput.validateRequired(value);
        }
        var zip = (value || '').trim();
        if (!zip) {
            return 'ZIP code is required.';
        }
        if (!/^\d{5}(-\d{4})?$/.test(zip)) {
            return 'Enter a valid 5-digit ZIP code (e.g. 77002).';
        }
        return '';
    }

    function validateInviteClientForm() {
        clearFormErrors('inviteClientForm', ['fullName', 'email', 'phone', 'clientRole'], 'inviteClientClientErrors');

        var fullNameInput = document.getElementById('inviteClientFullName');
        var emailInput = document.getElementById('inviteClientEmail');
        var phoneInput = document.getElementById('inviteClientPhone');
        var roleInput = document.querySelector('#inviteClientForm input[name="clientRole"]:checked');

        var errors = [];
        var fullNameError = validateFullName(fullNameInput ? fullNameInput.value : '');
        var emailError = validateEmail(emailInput ? emailInput.value : '');
        var phoneError = validatePhone(phoneInput ? phoneInput.value : '');
        var roleError = roleInput ? '' : 'Please select a client role.';

        if (fullNameError) {
            setFormFieldError('inviteClientForm', 'fullName', fullNameError);
            errors.push(fullNameError);
        }
        if (emailError) {
            setFormFieldError('inviteClientForm', 'email', emailError);
            errors.push(emailError);
        }
        if (phoneError) {
            setFormFieldError('inviteClientForm', 'phone', phoneError);
            errors.push(phoneError);
        }
        if (roleError) {
            setFormFieldError('inviteClientForm', 'clientRole', roleError);
            errors.push(roleError);
        }

        if (errors.length) {
            showFormErrorBanner('inviteClientForm', 'inviteClientClientErrors', errors);
            return false;
        }

        return true;
    }

    function validateInviteAccessForm() {
        var form = document.getElementById('inviteAccessForm');
        if (!form) {
            return true;
        }

        var existing = document.getElementById('inviteAccessClientErrors');
        if (existing) {
            existing.remove();
        }

        var errors = [];
        var accessChecked = form.querySelector(
            'input[name="AccessPropertyOverview"]:checked,' +
            'input[name="AccessFilesReports"]:checked,' +
            'input[name="AccessQuotesEstimates"]:checked,' +
            'input[name="AccessMessages"]:checked,' +
            'input[name="AccessProjectUpdates"]:checked,' +
            'input[name="AccessPayments"]:checked'
        );
        if (!accessChecked) {
            errors.push('Select at least one access permission.');
        }

        var collaboration = form.querySelector('input[name="CollaborationLevel"]:checked');
        if (!collaboration) {
            errors.push('Select a collaboration level.');
            document.querySelector('.rl-collab-grid')?.classList.add('is-error');
        } else {
            document.querySelector('.rl-collab-grid')?.classList.remove('is-error');
        }

        var deliveryChecked = form.querySelector(
            'input[name="DeliveryEmail"]:checked, input[name="DeliveryText"]:checked'
        );
        if (!deliveryChecked) {
            errors.push('Select at least one invitation delivery method (email or text).');
        }

        if (errors.length) {
            showFormErrorBanner('inviteAccessForm', 'inviteAccessClientErrors', errors);
            return false;
        }

        return true;
    }

    function validateCreatePropertyForm() {
        clearFormErrors(
            'createPropertyForm',
            ['Address', 'City', 'StateCode', 'PostalCode'],
            'createPropertyClientErrors'
        );

        var addressInput = document.getElementById('cp-address');
        var cityInput = document.getElementById('cp-city');
        var stateInput = document.getElementById('cp-state');
        var zipInput = document.getElementById('cp-postal-code');

        var errors = [];
        var address = addressInput ? addressInput.value.trim() : '';
        var city = cityInput ? cityInput.value.trim() : '';
        var state = stateInput ? stateInput.value.trim() : '';
        var zip = zipInput ? zipInput.value.trim() : '';
        var addressMessage = 'Enter a valid US street address with a street number (e.g. 123 Main St).';

        function isValidStreetAddress(value) {
            var line = String(value || '').trim();
            if (line.length < 5) return false;
            if (!/\p{L}/u.test(line)) return false;
            if (/^[\d\s.,#-]+$/.test(line)) return false;
            var tokens = line.split(/\s+/).filter(Boolean);
            var hasDigit = /\d/.test(line);
            var wordParts = tokens.filter(function (part) { return /\p{L}/u.test(part); }).length;
            if (!hasDigit) return false;
            if (wordParts < 1) return false;
            return true;
        }

        if (!address) {
            setFormFieldError('createPropertyForm', 'Address', 'Property address is required.');
            errors.push('Property address is required.');
        } else if (!isValidStreetAddress(address)) {
            setFormFieldError('createPropertyForm', 'Address', addressMessage);
            errors.push(addressMessage);
        }
        if (!city) {
            setFormFieldError('createPropertyForm', 'City', 'City is required.');
            errors.push('City is required.');
        }
        if (!state) {
            setFormFieldError('createPropertyForm', 'StateCode', 'State is required.');
            errors.push('State is required.');
        }

        var zipError = validateZip(zip);
        if (zipError) {
            setFormFieldError('createPropertyForm', 'PostalCode', zipError);
            errors.push(zipError);
        }

        if (errors.length) {
            showFormErrorBanner('createPropertyForm', 'createPropertyClientErrors', errors);
            var firstInvalid = document.querySelector('#createPropertyForm .rl-cp-field.is-error, #createPropertyForm .rl-field-error:not(:empty)');
            if (firstInvalid) {
                firstInvalid.scrollIntoView({ behavior: 'smooth', block: 'center' });
            }
            return false;
        }

        return true;
    }

    function validateInvitePropertyForm() {
        var form = document.getElementById('invitePropertyForm');
        if (!form) {
            return true;
        }

        var existing = document.getElementById('invitePropertyClientErrors');
        if (existing) {
            existing.remove();
        }

        var errors = [];
        var selected = form.querySelector('input[name="propertyFileId"]:checked');
        var searchInput = document.getElementById('invitePropertySearch');
        var searchHidden = document.getElementById('invitePropertySearchHidden');
        var query = (searchInput?.value || searchHidden?.value || '').trim();
        var hasOptions = form.querySelectorAll('input[name="propertyFileId"]').length > 0;

        if (hasOptions && !selected) {
            errors.push('Please select a property to continue.');
        } else if (!hasOptions && !query) {
            errors.push('Search for an address or create a new property to continue.');
        }

        if (errors.length) {
            showFormErrorBanner('invitePropertyForm', 'invitePropertyClientErrors', errors);
            return false;
        }

        return true;
    }

    function initInvitePropertyForm() {
        var form = document.getElementById('invitePropertyForm');
        var nextBtn = document.getElementById('invitePropertyNextBtn');
        var searchInput = document.getElementById('invitePropertySearch');
        var searchHidden = document.getElementById('invitePropertySearchHidden');
        var createUrl = form?.dataset.createUrl;

        if (searchInput && searchHidden) {
            searchInput.addEventListener('input', function () {
                searchHidden.value = searchInput.value;
                var existing = document.getElementById('invitePropertyClientErrors');
                if (existing) {
                    existing.remove();
                }
            });
        }

        document.querySelectorAll('.rl-property-pick input').forEach(function (radio) {
            radio.addEventListener('change', function () {
                document.querySelectorAll('.rl-property-pick').forEach(function (pill) {
                    pill.classList.remove('is-selected');
                });
                radio.closest('.rl-property-pick')?.classList.add('is-selected');
                var existing = document.getElementById('invitePropertyClientErrors');
                if (existing) {
                    existing.remove();
                }
            });
        });

        if (!form) {
            return;
        }

        form.addEventListener('submit', function (e) {
            var selected = form.querySelector('input[name="propertyFileId"]:checked');
            var query = (searchInput?.value || searchHidden?.value || '').trim();
            var hasOptions = form.querySelectorAll('input[name="propertyFileId"]').length > 0;

            if (!selected && !hasOptions && query.length > 0 && createUrl) {
                e.preventDefault();
                window.location.href = createUrl + '?address=' + encodeURIComponent(query);
                return;
            }

            if (!validateInvitePropertyForm()) {
                e.preventDefault();
            }
        });

        if (nextBtn) {
            nextBtn.addEventListener('click', function (e) {
                if (!validateInvitePropertyForm()) {
                    e.preventDefault();
                }
            });
        }
    }

    function initInviteClientForm() {
        var form = document.getElementById('inviteClientForm');
        var nextBtn = document.getElementById('inviteNextBtn');
        var phoneInput = document.getElementById('inviteClientPhone');
        var note = document.querySelector('#inviteClientForm textarea[name="quickNote"]');
        var count = document.getElementById('noteCount');

        if (phoneInput && window.IndorPhoneInput) {
            window.IndorPhoneInput.attach(phoneInput, {
                required: phoneInput.dataset.phoneRequired === 'true',
                invalidMessage: 'Enter a valid 10-digit US phone number (e.g. 555 123 4567).'
            });
        }

        if (note && count) {
            note.addEventListener('input', function () {
                count.textContent = String(note.value.length);
            });
        }

        if (form) {
            form.addEventListener('submit', function (e) {
                var submitter = e.submitter;
                if (submitter && submitter.classList.contains('rl-invite-cancel')) {
                    return;
                }
                if (!validateInviteClientForm()) {
                    e.preventDefault();
                }
            });
        }

        if (nextBtn && form) {
            nextBtn.addEventListener('click', function (e) {
                e.preventDefault();
                if (!validateInviteClientForm()) {
                    return;
                }
                if (typeof form.requestSubmit === 'function') {
                    form.requestSubmit(nextBtn);
                } else {
                    form.submit();
                }
            });
        }

        document.querySelectorAll('#inviteClientForm .rl-role-pill input').forEach(function (radio) {
            radio.addEventListener('change', function () {
                document.querySelectorAll('#inviteClientForm .rl-role-pill').forEach(function (pill) {
                    pill.classList.remove('is-selected');
                });
                radio.closest('.rl-role-pill')?.classList.add('is-selected');
                setFormFieldError('inviteClientForm', 'clientRole', '');
            });
        });

        ['inviteClientFullName', 'inviteClientEmail', 'inviteClientPhone'].forEach(function (id) {
            var input = document.getElementById(id);
            if (!input) {
                return;
            }
            input.addEventListener('input', function () {
                setFormFieldError('inviteClientForm', input.name, '');
            });
        });
    }

    function initInviteAccessForm() {
        var form = document.getElementById('inviteAccessForm');
        var nextBtn = document.getElementById('inviteAccessNextBtn');

        if (form) {
            form.addEventListener('submit', function (e) {
                if (!validateInviteAccessForm()) {
                    e.preventDefault();
                }
            });
        }

        if (nextBtn && form) {
            nextBtn.addEventListener('click', function (e) {
                if (!validateInviteAccessForm()) {
                    e.preventDefault();
                }
            });
        }
    }

    function initCreatePropertyForm() {
        var form = document.getElementById('createPropertyForm');
        var submitBtn = document.getElementById('createPropertySubmitBtn');
        var zipInput = document.getElementById('cp-postal-code');

        if (zipInput && window.IndorZipInput) {
            window.IndorZipInput.attach(zipInput, { required: true });
        }

        if (form) {
            form.addEventListener('submit', function (e) {
                if (!validateCreatePropertyForm()) {
                    e.preventDefault();
                }
            });
        }

        if (submitBtn && form) {
            submitBtn.addEventListener('click', function (e) {
                if (!validateCreatePropertyForm()) {
                    e.preventDefault();
                    return;
                }
            });
        }

        ['cp-address', 'cp-city', 'cp-state', 'cp-postal-code'].forEach(function (id) {
            var input = document.getElementById(id);
            if (!input) {
                return;
            }
            input.addEventListener('input', function () {
                setFormFieldError('createPropertyForm', input.name, '');
            });
            input.addEventListener('change', function () {
                setFormFieldError('createPropertyForm', input.name, '');
            });
        });
    }

    function init() {
        initInviteClientForm();
        initInvitePropertyForm();
        initInviteAccessForm();
        initCreatePropertyForm();
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
