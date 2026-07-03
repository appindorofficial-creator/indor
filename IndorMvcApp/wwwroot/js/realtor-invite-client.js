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
        if (!banner) {
            return;
        }
        banner.classList.add('is-hidden');
        var list = banner.querySelector('.rl-error-list');
        if (list) {
            list.innerHTML = '';
        }
    }

    function showFormErrorBanner(formId, bannerId, messages) {
        var form = document.getElementById(formId);
        if (!form || !messages.length) {
            return;
        }

        var banner = document.getElementById(bannerId);
        if (!banner) {
            banner = document.createElement('div');
            banner.className = 'rl-error-banner';
            banner.id = bannerId;
            banner.setAttribute('role', 'alert');
            banner.innerHTML =
                '<i class="fas fa-circle-exclamation"></i>' +
                '<div><strong>Please fix the following to continue:</strong>' +
                '<ul class="rl-error-list"></ul></div>';

            var summary = form.querySelector('.ob-summary');
            if (summary && summary.nextSibling) {
                form.insertBefore(banner, summary.nextSibling);
            } else {
                form.insertBefore(banner, form.firstChild.nextSibling);
            }
        }

        var list = banner.querySelector('.rl-error-list');
        if (list) {
            list.innerHTML = messages.map(function (msg) { return '<li>' + msg + '</li>'; }).join('');
        }
        banner.classList.remove('is-hidden');
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

    var streetSuffixes = {
        st: true, street: true, rd: true, road: true, ave: true, avenue: true,
        blvd: true, boulevard: true, dr: true, drive: true, ln: true, lane: true,
        way: true, ct: true, court: true, cir: true, circle: true, pl: true, place: true,
        pkwy: true, parkway: true, ter: true, terrace: true, trl: true, trail: true,
        hwy: true, highway: true, loop: true, pass: true, path: true, row: true,
        run: true, walk: true, xing: true, crossing: true, pike: true, sq: true,
        square: true, aly: true, alley: true, cres: true, crescent: true, cv: true,
        cove: true, bnd: true, bend: true, pt: true, point: true, grv: true, grove: true,
        vw: true, view: true
    };

    var streetDirectionals = {
        n: true, s: true, e: true, w: true, ne: true, nw: true, se: true, sw: true,
        north: true, south: true, east: true, west: true
    };

    function normalizeStreetToken(token) {
        return token.replace(/^[.,#]+|[.,#]+$/g, '').toLowerCase();
    }

    function hasCompleteStreetLine(tokens) {
        var normalized = tokens
            .map(normalizeStreetToken)
            .filter(function (token) { return token.length > 0; });

        if (normalized.some(function (token) { return streetSuffixes[token]; })) {
            return normalized.some(function (token) {
                return /[a-zA-Z]/.test(token) && !streetDirectionals[token];
            });
        }

        var nameTokens = normalized.filter(function (token) {
            return /[a-zA-Z]/.test(token) && !streetDirectionals[token];
        });
        return nameTokens.length >= 2;
    }

    function validateStreetAddress(value, requireStreetNumber) {
        var line = String(value || '').trim();
        if (!line) {
            return 'Property address is required.';
        }
        if (line.length < 5) {
            return 'Enter a complete street address.';
        }
        if (!/[a-zA-Z]/.test(line)) {
            return 'Enter a valid street address with a street name.';
        }
        if (/^[\d\s.,#-]+$/.test(line)) {
            return 'Address cannot contain only numbers.';
        }

        var tokens = line.split(/\s+/).filter(Boolean);
        var hasDigit = /\d/.test(line);
        if (requireStreetNumber && !hasDigit) {
            return 'Enter a street number (e.g. 123 Main St).';
        }
        if (requireStreetNumber && !hasCompleteStreetLine(tokens)) {
            return 'Enter a complete street address with street name and type (e.g. 123 Main St).';
        }
        return '';
    }

    function validateInviteClientForm() {
        clearFormErrors('inviteClientForm', ['fullName', 'email', 'phone', 'clientRole'], 'inviteClientFormErrors');

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
            showFormErrorBanner('inviteClientForm', 'inviteClientFormErrors', errors);
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
            'createPropertyFormErrors'
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

        var addressError = validateStreetAddress(address, true);
        if (addressError) {
            setFormFieldError('createPropertyForm', 'Address', addressError);
            errors.push(addressError);
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
            showFormErrorBanner('createPropertyForm', 'createPropertyFormErrors', errors);
            var firstInvalid = document.querySelector('#createPropertyForm .rl-cp-field.is-error, #createPropertyForm .rl-field-error:not(:empty)');
            if (firstInvalid) {
                firstInvalid.scrollIntoView({ behavior: 'smooth', block: 'center' });
            }
            return false;
        }

        return true;
    }

    function getPropertySearchQuery(searchInput) {
        return (searchInput?.value || '').trim();
    }

    function shouldRedirectPropertyToCreate(form, searchInput, createUrl) {
        if (!form || !createUrl) {
            return false;
        }

        var selected = form.querySelector('input[name="propertyFileId"]:checked');
        var query = getPropertySearchQuery(searchInput);
        return !selected && query.length > 0;
    }

    function redirectPropertyToCreate(searchInput, createUrl) {
        var query = getPropertySearchQuery(searchInput);
        if (!query || !createUrl) {
            return false;
        }

        window.location.href = createUrl + '?address=' + encodeURIComponent(query);
        return true;
    }

    function validateInvitePropertyForm() {
        var form = document.getElementById('invitePropertyForm');
        if (!form) {
            return true;
        }

        clearFormErrors('invitePropertyForm', [], 'invitePropertyFormErrors');

        var searchInput = document.getElementById('invitePropertySearch');
        var createUrl = form.dataset.createUrl;
        var errors = [];
        var selected = form.querySelector('input[name="propertyFileId"]:checked');
        var query = getPropertySearchQuery(searchInput);
        var hasOptions = form.querySelectorAll('input[name="propertyFileId"]').length > 0;

        if (shouldRedirectPropertyToCreate(form, searchInput, createUrl)) {
            return true;
        }

        if (!selected && !query) {
            if (hasOptions) {
                errors.push('Select a property from the list, or enter an address to create a new one.');
            } else {
                errors.push('Enter an address and tap Next to create a new property.');
            }
        }

        if (errors.length) {
            showFormErrorBanner('invitePropertyForm', 'invitePropertyFormErrors', errors);
            setPropertySearchError(errors[0]);
            var scrollTarget = document.getElementById('invitePropertySearchError')
                || document.getElementById('invitePropertyFormErrors');
            if (scrollTarget) {
                scrollTarget.scrollIntoView({ behavior: 'smooth', block: 'center' });
            }
            return false;
        }

        return true;
    }

    function setPropertySearchError(message) {
        var errorEl = document.getElementById('invitePropertySearchError');
        var searchWrap = document.querySelector('#invitePropertyForm .rl-search-bar');
        if (errorEl) {
            errorEl.textContent = message || '';
        }
        if (searchWrap) {
            searchWrap.classList.toggle('is-error', !!message);
        }
    }

    function initInvitePropertyForm() {
        var form = document.getElementById('invitePropertyForm');
        var nextBtn = document.getElementById('invitePropertyNextBtn');
        var searchInput = document.getElementById('invitePropertySearch');
        var createUrl = form?.dataset.createUrl;
        var searchUrl = form?.dataset.searchUrl;

        if (searchInput) {
            searchInput.addEventListener('input', function () {
                clearFormErrors('invitePropertyForm', [], 'invitePropertyFormErrors');
                setPropertySearchError('');
            });

            searchInput.addEventListener('keydown', function (e) {
                if (e.key !== 'Enter' || !searchUrl) {
                    return;
                }

                e.preventDefault();
                var query = getPropertySearchQuery(searchInput);
                window.location.href = query
                    ? searchUrl + '?q=' + encodeURIComponent(query)
                    : searchUrl;
            });
        }

        document.querySelectorAll('.rl-property-pick input').forEach(function (radio) {
            radio.addEventListener('change', function () {
                document.querySelectorAll('.rl-property-pick').forEach(function (pill) {
                    pill.classList.remove('is-selected');
                });
                radio.closest('.rl-property-pick')?.classList.add('is-selected');
                clearFormErrors('invitePropertyForm', [], 'invitePropertyFormErrors');
                setPropertySearchError('');
            });
        });

        if (!form) {
            return;
        }

        form.addEventListener('submit', function (e) {
            if (shouldRedirectPropertyToCreate(form, searchInput, createUrl)) {
                e.preventDefault();
                redirectPropertyToCreate(searchInput, createUrl);
                return;
            }

            if (!validateInvitePropertyForm()) {
                e.preventDefault();
            }
        });

        if (nextBtn && form) {
            nextBtn.addEventListener('click', function (e) {
                if (shouldRedirectPropertyToCreate(form, searchInput, createUrl)) {
                    e.preventDefault();
                    redirectPropertyToCreate(searchInput, createUrl);
                    return;
                }

                if (!validateInvitePropertyForm()) {
                    e.preventDefault();
                    return;
                }

                if (typeof form.requestSubmit === 'function') {
                    e.preventDefault();
                    form.requestSubmit(nextBtn);
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
            input.addEventListener('blur', function () {
                var message = '';
                if (id === 'inviteClientFullName') {
                    message = validateFullName(input.value);
                } else if (id === 'inviteClientEmail') {
                    message = validateEmail(input.value);
                } else if (id === 'inviteClientPhone') {
                    message = validatePhone(input.value);
                }
                if (message) {
                    setFormFieldError('inviteClientForm', input.name, message);
                }
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
                e.preventDefault();
                if (!validateCreatePropertyForm()) {
                    return;
                }
                if (typeof form.requestSubmit === 'function') {
                    form.requestSubmit(submitBtn);
                } else {
                    form.submit();
                }
            });
        }

        if (zipInput) {
            zipInput.addEventListener('blur', function () {
                var zipError = validateZip(zipInput.value);
                if (zipError) {
                    setFormFieldError('createPropertyForm', 'PostalCode', zipError);
                }
            });
        }

        var addressInput = document.getElementById('cp-address');
        if (addressInput) {
            addressInput.addEventListener('blur', function () {
                var message = validateStreetAddress(addressInput.value, true);
                if (message) {
                    setFormFieldError('createPropertyForm', 'Address', message);
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
