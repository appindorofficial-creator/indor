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

    /** Hide/show without relying solely on the HTML [hidden] UA rule (author display:flex wins). */
    function setPaHidden(el, hide) {
        if (!el) {
            return;
        }
        el.classList.toggle('pa-search-hidden', !!hide);
        el.hidden = !!hide;
        if (hide) {
            el.setAttribute('aria-hidden', 'true');
        } else {
            el.removeAttribute('aria-hidden');
        }
    }

    function haystackMatch(el, q) {
        if (!q) {
            return true;
        }
        var haystack = ((el.getAttribute('data-search') || '') + ' ' + (el.textContent || '')).toLowerCase();
        return haystack.indexOf(q) !== -1;
    }

    function wireServicesSearch() {
        var form = document.getElementById('paServicesSearchForm');
        var input = document.getElementById('paServicesSearch');
        if (!form || !input || form.getAttribute('data-pa-search-wired') === '1') {
            return;
        }
        form.setAttribute('data-pa-search-wired', '1');

        var empty = document.getElementById('paServicesSearchEmpty');
        var filterBtn = document.getElementById('paServicesFilterBtn');
        var filterPills = document.getElementById('paServicesFilterPills');

        function applyFilter() {
            var q = (input.value || '').trim().toLowerCase();
            var anyVisible = false;

            document.querySelectorAll('[data-pa-service-section]').forEach(function (section) {
                var visibleInSection = 0;
                section.querySelectorAll('.pa-service-grid-item').forEach(function (item) {
                    var show = haystackMatch(item, q);
                    setPaHidden(item, !show);
                    if (show) {
                        visibleInSection++;
                    }
                });
                setPaHidden(section, visibleInSection === 0);
                if (visibleInSection > 0) {
                    anyVisible = true;
                }
            });

            document.querySelectorAll('[data-pa-search-hide]').forEach(function (el) {
                setPaHidden(el, !!q);
            });

            setPaHidden(empty, !q || anyVisible);
        }

        ['input', 'keyup', 'search', 'change'].forEach(function (evt) {
            input.addEventListener(evt, applyFilter);
        });
        form.addEventListener('submit', function (event) {
            event.preventDefault();
            applyFilter();
            input.blur();
        });

        if (filterBtn && filterPills) {
            filterBtn.addEventListener('click', function () {
                filterPills.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
            });
        }

        applyFilter();
    }

    function wireNetworkSearch() {
        var form = document.getElementById('paNetworkSearchForm');
        var input = document.getElementById('paNetworkSearch');
        if (!form || !input || form.getAttribute('data-pa-search-wired') === '1') {
            return;
        }
        form.setAttribute('data-pa-search-wired', '1');

        var empty = document.getElementById('paNetworkSearchEmpty');
        var neighborhoods = document.getElementById('paNetworkNeighborhoods');
        var activeFilter = ((form.querySelector('input[name="filter"]') || {}).value || 'All').toLowerCase();

        function kindAllowed(kind) {
            if (activeFilter === 'all') {
                return true;
            }
            return kind === activeFilter;
        }

        function applyFilter() {
            var q = (input.value || '').trim().toLowerCase();
            var anyVisible = false;
            var listingsVisible = 0;

            document.querySelectorAll('[data-pa-network-item]').forEach(function (item) {
                var kind = (item.getAttribute('data-pa-network-kind') || '').toLowerCase();
                if (!kindAllowed(kind)) {
                    setPaHidden(item, true);
                    return;
                }

                var show = haystackMatch(item, q);
                setPaHidden(item, !show);
                if (show) {
                    anyVisible = true;
                    if (kind === 'listings') {
                        listingsVisible++;
                    }
                }
            });

            document.querySelectorAll('[data-pa-network-block]').forEach(function (block) {
                var kind = (block.getAttribute('data-pa-network-block') || '').toLowerCase();
                if (!kindAllowed(kind)) {
                    setPaHidden(block, true);
                    return;
                }

                if (kind === 'promotions') {
                    setPaHidden(block, !!q);
                    if (!q) {
                        anyVisible = true;
                    }
                    return;
                }

                if (kind === 'listings') {
                    // Show neighborhood matches while searching, or when Listings filter is active.
                    var showBlock = listingsVisible > 0 && (q || activeFilter === 'listings');
                    setPaHidden(block, !showBlock);
                    return;
                }

                var visibleItems = block.querySelectorAll('[data-pa-network-item]:not([hidden]):not(.pa-search-hidden)');
                setPaHidden(block, visibleItems.length === 0);
            });

            if (neighborhoods && kindAllowed('listings') && q && listingsVisible === 0) {
                setPaHidden(neighborhoods, true);
            }

            setPaHidden(empty, !q || anyVisible);
        }

        ['input', 'keyup', 'search', 'change'].forEach(function (evt) {
            input.addEventListener(evt, applyFilter);
        });
        form.addEventListener('submit', function () {
            // Allow GET navigation so ?q= stays in sync with chip links.
            applyFilter();
        });

        applyFilter();
    }

    // Wire search before validation binding so a form validation setup error cannot skip filtering.
    wireServicesSearch();
    wireNetworkSearch();

    try {
        document.querySelectorAll('.pa-portal-page form').forEach(bindEnglishFormValidation);
    } catch (err) {
        // Non-fatal: English validation messages are progressive enhancement only.
    }
})();
