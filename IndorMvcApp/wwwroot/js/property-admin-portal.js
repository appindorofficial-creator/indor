(function () {
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

    document.querySelectorAll('.pa-portal-page form').forEach(bindEnglishFormValidation);

    function foldText(value) {
        return String(value || '')
            .normalize('NFD')
            .replace(/[\u0300-\u036f]/g, '')
            .toLowerCase()
            .trim();
    }

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
})();
