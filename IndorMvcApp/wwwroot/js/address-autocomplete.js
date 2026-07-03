(function () {
    'use strict';

    // Attaches Google Places Autocomplete to any input marked with
    // [data-address-autocomplete]. When the Places library is unavailable
    // (no API key, blocked, offline) the field stays a normal text input and
    // server-side validation still enforces a complete address.

    function getCityFromComponents(components) {
        if (!components) {
            return '';
        }

        return getComponent(components, 'locality', false)
            || getComponent(components, 'postal_town', false)
            || getComponent(components, 'sublocality', false)
            || getComponent(components, 'sublocality_level_1', false)
            || getComponent(components, 'administrative_area_level_3', false)
            || getComponent(components, 'neighborhood', false);
    }

    function applyAddressComponents(sourceInput, components) {
        if (!components || !components.length || !sourceInput) {
            return;
        }

        var city = getCityFromComponents(components);
        var stateCode = getComponent(components, 'administrative_area_level_1', true);
        var zipCode = getComponent(components, 'postal_code', false);

        if (city) {
            fillLinkedField(sourceInput, 'City', city);
        }

        if (stateCode) {
            var stateEl = getLinkedElement(sourceInput, 'State');
            if (stateEl) {
                ensureStateSelectSnapshot(stateEl);
                filterStateSelect(stateEl, stateCode);
                stateEl.value = stateCode;
                stateEl.dispatchEvent(new Event('change', { bubbles: true }));
            } else {
                fillLinkedField(sourceInput, 'State', stateCode);
            }
        }

        if (zipCode) {
            var zipEl = getLinkedElement(sourceInput, 'Zip');
            if (zipEl) {
                setZipValue(zipEl, zipCode);
            }
        }
    }

    function needsAddressCompletion(sourceInput) {
        var cityEl = getLinkedElement(sourceInput, 'City');
        var stateEl = getLinkedElement(sourceInput, 'State');
        var zipEl = getLinkedElement(sourceInput, 'Zip');

        return !!(
            (cityEl && !cityEl.value.trim())
            || (stateEl && !stateEl.value.trim())
            || (zipEl && shouldReplaceZip(zipEl))
        );
    }

    var streetGeocodeTimers = typeof WeakMap !== 'undefined' ? new WeakMap() : null;
    var streetResolveControllers = typeof WeakMap !== 'undefined' ? new WeakMap() : null;

    function runServerAddressResolve(sourceInput, address) {
        if (!address || !needsAddressCompletion(sourceInput)) {
            return;
        }

        var url = '/AddressLookup/Resolve?address=' + encodeURIComponent(address);
        var fetchOptions = {
            headers: { 'Accept': 'application/json' },
            credentials: 'same-origin'
        };

        if (streetResolveControllers) {
            var previous = streetResolveControllers.get(sourceInput);
            if (previous) {
                previous.abort();
            }
            var controller = new AbortController();
            streetResolveControllers.set(sourceInput, controller);
            fetchOptions.signal = controller.signal;
        }

        fetch(url, fetchOptions).then(function (response) {
            if (!response.ok || !needsAddressCompletion(sourceInput)) {
                return null;
            }
            return response.json();
        }).then(function (payload) {
            if (!payload || !needsAddressCompletion(sourceInput)) {
                return;
            }

            if (payload.city) {
                fillLinkedField(sourceInput, 'City', payload.city);
            }
            if (payload.state) {
                var stateEl = getLinkedElement(sourceInput, 'State');
                if (stateEl) {
                    ensureStateSelectSnapshot(stateEl);
                    filterStateSelect(stateEl, payload.state);
                    stateEl.value = payload.state;
                    stateEl.dispatchEvent(new Event('change', { bubbles: true }));
                }
            }
            if (payload.zip) {
                var zipEl = getLinkedElement(sourceInput, 'Zip');
                if (zipEl && shouldReplaceZip(zipEl)) {
                    setZipValue(zipEl, payload.zip);
                }
            }
        }).catch(function (err) {
            if (err && err.name === 'AbortError') {
                return;
            }
        });
    }

    function geocodeFullAddress(sourceInput, address) {
        if (!address || !needsAddressCompletion(sourceInput)) {
            return;
        }

        if (window.google && google.maps && google.maps.Geocoder) {
            var geocoder = new google.maps.Geocoder();
            geocoder.geocode({
                address: address,
                componentRestrictions: { country: 'US' }
            }, function (results, status) {
                if (status !== 'OK' || !results || !results.length) {
                    runServerAddressResolve(sourceInput, address);
                    return;
                }

                if (!needsAddressCompletion(sourceInput)) {
                    return;
                }

                applyAddressComponents(sourceInput, results[0].address_components || []);
                if (needsAddressCompletion(sourceInput)) {
                    tryGeocodeLinkedZip(sourceInput);
                }
            });
            return;
        }

        runServerAddressResolve(sourceInput, address);
    }

    function tryCompleteAddressFromStreet(sourceInput) {
        var street = sourceInput.value.trim();
        if (street.length < 4 || !needsAddressCompletion(sourceInput)) {
            return;
        }

        var cityEl = getLinkedElement(sourceInput, 'City');
        var stateEl = getLinkedElement(sourceInput, 'State');
        var city = cityEl ? cityEl.value.trim() : '';
        var state = stateEl ? stateEl.value.trim() : '';
        var address = city && state
            ? street + ', ' + city + ', ' + state + ', USA'
            : street + ', USA';

        geocodeFullAddress(sourceInput, address);
    }

    function scheduleStreetCompletion(sourceInput) {
        if (streetGeocodeTimers) {
            clearTimeout(streetGeocodeTimers.get(sourceInput));
            streetGeocodeTimers.set(sourceInput, setTimeout(function () {
                tryCompleteAddressFromStreet(sourceInput);
            }, 700));
        } else {
            tryCompleteAddressFromStreet(sourceInput);
        }
    }

    function bindStreetAddressCompletion(input) {
        if (input.dataset.streetCompletionBound === 'true') {
            return;
        }
        input.dataset.streetCompletionBound = 'true';

        input.addEventListener('blur', function () {
            window.setTimeout(function () {
                if (document.activeElement && document.activeElement.closest('.pac-container')) {
                    return;
                }
                tryCompleteAddressFromStreet(input);
            }, 250);
        });

        input.addEventListener('input', function () {
            if (!input.value.trim()) {
                return;
            }
            scheduleStreetCompletion(input);
        });
    }

    function getComponent(components, type, useShort) {
        if (!components) return '';
        var match = components.find(function (c) { return c.types.indexOf(type) !== -1; });
        if (!match) return '';
        return useShort ? match.short_name : match.long_name;
    }

    function getLinkedElement(input, suffix) {
        var targetId = input.dataset['ac' + suffix];
        if (!targetId) return null;
        return document.getElementById(targetId);
    }

    function fillLinkedField(input, suffix, value) {
        if (!value) return;
        var target = getLinkedElement(input, suffix);
        if (!target) return;
        if (suffix === 'State' && target.tagName === 'SELECT') {
            ensureStateSelectSnapshot(target);
            filterStateSelect(target, value);
        }
        target.value = value;
        target.dispatchEvent(new Event('input', { bubbles: true }));
        target.dispatchEvent(new Event('change', { bubbles: true }));
    }

    var geocodeTimers = typeof WeakMap !== 'undefined' ? new WeakMap() : null;
    var stateSelectSnapshots = typeof WeakMap !== 'undefined' ? new WeakMap() : null;
    var zipFetchControllers = typeof WeakMap !== 'undefined' ? new WeakMap() : null;
    var geocodeGenerations = typeof WeakMap !== 'undefined' ? new WeakMap() : null;

    function isUsStateCode(value) {
        return typeof value === 'string' && /^[A-Z]{2}$/i.test(value.trim());
    }

    function captureInitialStateSelectValues() {
        document.querySelectorAll('select[data-ac-zip][data-ac-city]').forEach(function (select) {
            var current = select.value.trim();
            if (current && isUsStateCode(current)) {
                select.dataset.initialState = current.toUpperCase();
            }
        });
    }

    function restoreInitialStateSelectValues() {
        document.querySelectorAll('select[data-ac-zip][data-ac-city]').forEach(function (select) {
            var initial = select.dataset.initialState;
            if (!initial || !isUsStateCode(initial) || select.value.trim()) {
                return;
            }

            restoreStateSelect(select);
            select.value = initial.toUpperCase();
        });
    }

    function ensureStateSelectSnapshot(select) {
        if (!stateSelectSnapshots || stateSelectSnapshots.has(select)) {
            return;
        }

        var snapshot = Array.from(select.options).map(function (opt) {
            return { value: opt.value, label: opt.textContent };
        });
        stateSelectSnapshots.set(select, snapshot);
    }

    function filterStateSelect(select, stateCode) {
        if (stateCode) {
            stateCode = stateCode.trim().toUpperCase();
            if (!isUsStateCode(stateCode)) {
                return;
            }
        }

        ensureStateSelectSnapshot(select);
        var snapshot = stateSelectSnapshots.get(select);
        if (!snapshot) {
            return;
        }

        var selected = stateCode || select.value;
        select.innerHTML = '';

        snapshot.forEach(function (item) {
            if (!stateCode || !item.value || item.value === stateCode) {
                var opt = document.createElement('option');
                opt.value = item.value;
                opt.textContent = item.label;
                if (item.value === selected) {
                    opt.selected = true;
                }
                select.appendChild(opt);
            }
        });
    }

    function restoreStateSelect(select) {
        ensureStateSelectSnapshot(select);
        var snapshot = stateSelectSnapshots.get(select);
        if (!snapshot) {
            return;
        }

        var current = select.value;
        select.innerHTML = '';
        snapshot.forEach(function (item) {
            var opt = document.createElement('option');
            opt.value = item.value;
            opt.textContent = item.label;
            if (item.value === current) {
                opt.selected = true;
            }
            select.appendChild(opt);
        });
    }

    function scheduleGeocode(sourceInput, address, zipEl) {
        if (geocodeTimers) {
            clearTimeout(geocodeTimers.get(sourceInput));
            geocodeTimers.set(sourceInput, setTimeout(function () {
                runGeocode(sourceInput, address, zipEl);
            }, 400));
        } else {
            runGeocode(sourceInput, address, zipEl);
        }
    }

    function shouldReplaceZip(zipEl) {
        return !zipEl.value.trim() || zipEl.dataset.autoZip === 'true';
    }

    function markAutoZip(zipEl) {
        zipEl.dataset.autoZip = 'true';
    }

    function bindManualZipEdit(zipEl) {
        if (!zipEl || zipEl.dataset.manualZipBound === 'true') {
            return;
        }
        zipEl.dataset.manualZipBound = 'true';
        zipEl.addEventListener('focus', function () {
            scrollFieldIntoView(zipEl);
        });
        zipEl.addEventListener('input', function () {
            if (zipEl.value.trim()) {
                delete zipEl.dataset.autoZip;
            }
        });
    }

    function setZipValue(zipEl, zip) {
        if (!zipEl || !zip) {
            return;
        }

        var normalized = window.IndorZipInput
            ? window.IndorZipInput.normalize(zip)
            : String(zip).trim();
        if (window.IndorZipInput && !window.IndorZipInput.isValidRequired(normalized)) {
            return;
        }

        zipEl.value = normalized;
        markAutoZip(zipEl);
        zipEl.dispatchEvent(new Event('input', { bubbles: true }));
        zipEl.dispatchEvent(new Event('change', { bubbles: true }));
        showZipHint(zipEl);
    }

    function showZipHint(zipEl) {
        if (!zipEl || !zipEl.id) {
            return;
        }
        var hint = document.querySelector('[data-ac-zip-hint-for="' + zipEl.id + '"]');
        if (hint) {
            hint.hidden = false;
        }
    }

    function runServerZipLookup(city, state, zipEl, street) {
        if (!city || !state || !zipEl || !shouldReplaceZip(zipEl)) {
            return;
        }

        var url = '/AddressLookup/Zip?city=' + encodeURIComponent(city)
            + '&state=' + encodeURIComponent(state);
        if (street) {
            url += '&street=' + encodeURIComponent(street);
        }

        var fetchOptions = {
            headers: { 'Accept': 'application/json' },
            credentials: 'same-origin'
        };

        if (zipFetchControllers) {
            var previous = zipFetchControllers.get(zipEl);
            if (previous) {
                previous.abort();
            }
            var controller = new AbortController();
            zipFetchControllers.set(zipEl, controller);
            fetchOptions.signal = controller.signal;
        }

        fetch(url, fetchOptions).then(function (response) {
            if (!response.ok || !shouldReplaceZip(zipEl)) {
                return null;
            }
            return response.json();
        }).then(function (payload) {
            if (!payload || !payload.zip || !shouldReplaceZip(zipEl)) {
                return;
            }
            setZipValue(zipEl, payload.zip);
        }).catch(function (err) {
            if (err && err.name === 'AbortError') {
                return;
            }
            // Ignore lookup failures; the user can enter ZIP manually.
        });
    }

    function findLinkedStreet(stateEl) {
        if (!stateEl || !stateEl.id) {
            return '';
        }

        var addressId = stateEl.dataset.acAddress;
        if (addressId) {
            var addressInput = document.getElementById(addressId);
            if (addressInput && addressInput.value.trim()) {
                return addressInput.value.trim();
            }
        }

        var street = '';
        document.querySelectorAll('[data-ac-state="' + stateEl.id + '"][data-address-autocomplete]').forEach(function (input) {
            if (input.value.trim()) {
                street = input.value.trim();
            }
        });
        return street;
    }

    function lookupZipFromStateSelect(stateEl) {
        if (!stateEl) {
            return;
        }

        var state = stateEl.value.trim();
        if (!state) {
            return;
        }

        var zipId = stateEl.dataset.acZip;
        var zipEl = zipId ? document.getElementById(zipId) : null;
        if (!zipEl || !shouldReplaceZip(zipEl)) {
            return;
        }

        bindManualZipEdit(zipEl);

        var cityId = stateEl.dataset.acCity;
        var cityEl = cityId ? document.getElementById(cityId) : null;
        var city = cityEl ? cityEl.value.trim() : '';
        var street = findLinkedStreet(stateEl);

        if (!city && !street) {
            return;
        }

        if (city) {
            runServerZipLookup(city, state, zipEl, street);
        }

        if (window.google && google.maps && google.maps.Geocoder) {
            var address = street && city
                ? street + ', ' + city + ', ' + state + ', USA'
                : (city ? city + ', ' + state + ', USA' : null);
            if (address) {
                runGeocode(stateEl, address, zipEl);
            }
        }
    }

    function resolveLinkedAddressFields(sourceInput) {
        var zipEl = getLinkedElement(sourceInput, 'Zip');
        var stateEl = getLinkedElement(sourceInput, 'State');
        if (!zipEl || !stateEl) {
            return null;
        }

        bindManualZipEdit(zipEl);

        var isCitySource = sourceInput.hasAttribute('data-city-autocomplete');
        var cityEl = isCitySource ? sourceInput : getLinkedElement(sourceInput, 'City');
        var street = isCitySource ? '' : sourceInput.value.trim();
        var city = cityEl ? cityEl.value.trim() : '';

        return {
            zipEl: zipEl,
            stateEl: stateEl,
            cityEl: cityEl,
            city: city,
            street: street
        };
    }

    function lookupLinkedZip(sourceInput) {
        var fields = resolveLinkedAddressFields(sourceInput);
        if (!fields || !shouldReplaceZip(fields.zipEl)) {
            return;
        }

        var state = fields.stateEl.value.trim();
        if (!state) {
            return;
        }

        if (!fields.city && !fields.street) {
            return;
        }

        if (fields.city) {
            runServerZipLookup(fields.city, state, fields.zipEl, fields.street);
        }

        if (window.google && google.maps && google.maps.Geocoder) {
            var address = fields.street && fields.city
                ? fields.street + ', ' + fields.city + ', ' + state + ', USA'
                : (fields.city ? fields.city + ', ' + state + ', USA' : null);
            if (address) {
                scheduleGeocode(sourceInput, address, fields.zipEl);
            }
        }
    }

    function tryGeocodeLinkedZip(sourceInput) {
        lookupLinkedZip(sourceInput);
    }

    function shouldRunInitialZipLookup(sourceInput) {
        var fields = resolveLinkedAddressFields(sourceInput);
        if (!fields || !shouldReplaceZip(fields.zipEl)) {
            return false;
        }

        if (!fields.stateEl.value.trim()) {
            return false;
        }

        return !!(fields.city || fields.street);
    }

    function runGeocode(sourceInput, address, zipEl) {
        if (!shouldReplaceZip(zipEl)) return;

        if (!(window.google && google.maps && google.maps.Geocoder)) {
            return;
        }

        var generation = 0;
        if (geocodeGenerations) {
            generation = (geocodeGenerations.get(zipEl) || 0) + 1;
            geocodeGenerations.set(zipEl, generation);
        }

        var geocoder = new google.maps.Geocoder();
        geocoder.geocode({
            address: address,
            componentRestrictions: { country: 'US' }
        }, function (results, status) {
            if (geocodeGenerations && geocodeGenerations.get(zipEl) !== generation) {
                return;
            }
            if (status !== 'OK' || !results || !results.length) {
                return;
            }
            if (!shouldReplaceZip(zipEl)) return;

            var components = results[0].address_components;
            var zip = getComponent(components, 'postal_code', false);
            if (!zip) return;

            setZipValue(zipEl, zip);

            var cityEl = sourceInput && sourceInput.hasAttribute('data-city-autocomplete')
                ? sourceInput
                : (sourceInput ? getLinkedElement(sourceInput, 'City') : null);
            if (cityEl && !cityEl.value.trim()) {
                var city = getCityFromComponents(components);
                if (city) {
                    if (sourceInput && sourceInput.hasAttribute('data-city-autocomplete')) {
                        cityEl.value = city;
                        cityEl.dispatchEvent(new Event('input', { bubbles: true }));
                        cityEl.dispatchEvent(new Event('change', { bubbles: true }));
                    } else if (sourceInput) {
                        fillLinkedField(sourceInput, 'City', city);
                    }
                }
            }

            var stateEl = sourceInput ? getLinkedElement(sourceInput, 'State') : null;
            if (stateEl && !stateEl.value.trim()) {
                var stateCode = getComponent(components, 'administrative_area_level_1', true);
                if (stateCode) {
                    ensureStateSelectSnapshot(stateEl);
                    filterStateSelect(stateEl, stateCode);
                    stateEl.value = stateCode;
                    stateEl.dispatchEvent(new Event('change', { bubbles: true }));
                }
            }
        });
    }

    function isPacVisible() {
        return Array.from(document.querySelectorAll('.pac-container')).some(function (pac) {
            if (pac.classList.contains('pac-container--dismissed')) {
                return false;
            }
            if (pac.style.display === 'none' || pac.style.visibility === 'hidden') {
                return false;
            }
            return pac.offsetParent !== null || pac.getClientRects().length > 0;
        });
    }

    function isAutocompleteFieldFocused() {
        var active = document.activeElement;
        return !!(active && isAutocompleteInput(active));
    }

    function shouldKeepPacOpen() {
        return isPacVisible() && (isAutocompleteFieldFocused() || !!lastAutocompleteInput);
    }

    function dismissPacDropdown(activeInput, shouldBlur) {
        document.querySelectorAll('.pac-container').forEach(function (pac) {
            pac.style.display = 'none';
            pac.style.visibility = 'hidden';
            pac.style.pointerEvents = 'none';
            pac.classList.add('pac-container--dismissed');
        });

        if (shouldBlur !== false && activeInput && typeof activeInput.blur === 'function') {
            activeInput.blur();
        }
    }

    var pacDismissSuppressed = false;
    var lastAutocompleteInput = null;

    function resetPacContainerStyles(pac) {
        pac.classList.remove('pac-container--dismissed');
        pac.style.removeProperty('display');
        pac.style.removeProperty('visibility');
        pac.style.removeProperty('pointer-events');
    }

    function hideStalePacContainers(activeInput) {
        var containers = Array.from(document.querySelectorAll('.pac-container'));
        if (containers.length === 0) {
            return;
        }

        if (!activeInput) {
            dismissPacDropdown(null);
            return;
        }

        if (containers.length === 1) {
            var single = containers[0];
            if (single.classList.contains('pac-container--dismissed')) {
                resetPacContainerStyles(single);
            }
            return;
        }

        containers.forEach(function (pac, index) {
            if (index < containers.length - 1) {
                pac.style.display = 'none';
                pac.style.visibility = 'hidden';
                pac.style.pointerEvents = 'none';
                pac.classList.add('pac-container--dismissed');
            } else {
                resetPacContainerStyles(pac);
            }
        });
    }

    function isAutocompleteInput(el) {
        return !!(el && el.matches && (el.matches('[data-address-autocomplete]') || el.matches('[data-city-autocomplete]')));
    }

    function setActiveAutocompleteInput(input) {
        if (!isAutocompleteInput(input)) {
            return;
        }

        if (lastAutocompleteInput && lastAutocompleteInput !== input) {
            dismissPacDropdown(null);
        }

        lastAutocompleteInput = input;
        hideStalePacContainers(input);
    }

    function suppressPacDismiss(ms) {
        pacDismissSuppressed = true;
        window.setTimeout(function () {
            pacDismissSuppressed = false;
        }, ms || 350);
    }

    function finalizeAutocompleteSelection(input) {
        suppressPacDismiss(600);
        dismissPacDropdown(null, false);
        window.setTimeout(function () {
            dismissPacDropdown(null, false);
        }, 200);
        window.setTimeout(function () {
            if (input && document.activeElement === input) {
                dismissPacDropdown(input, true);
            } else {
                dismissPacDropdown(null, false);
            }
        }, 400);
    }

    function bindPacSelectionGuards() {
        ['touchstart', 'mousedown', 'pointerdown'].forEach(function (eventName) {
            document.addEventListener(eventName, function (e) {
                if (!e.target.closest('.pac-container')) {
                    return;
                }
                if (e.target.closest('.pac-item') && eventName === 'mousedown') {
                    // Keep focus on the input until Google applies the selection.
                    e.preventDefault();
                }
                suppressPacDismiss(1500);
            }, true);
        });

        document.addEventListener('click', function (e) {
            if (!e.target.closest('.pac-item')) {
                return;
            }
            suppressPacDismiss(1500);
            window.setTimeout(function () {
                finalizeAutocompleteSelection(lastAutocompleteInput);
            }, 300);
        }, true);
    }

    function bindAutocompleteDismissHandlers(input) {
        if (input.dataset.pacDismissBound === 'true') {
            return;
        }
        input.dataset.pacDismissBound = 'true';

        input.addEventListener('focus', function () {
            setActiveAutocompleteInput(input);
            scrollFieldIntoView(input);
            startPacPositionLoop();
        });

        input.addEventListener('input', function () {
            setActiveAutocompleteInput(input);
            hideStalePacContainers(input);
            positionPacContainer(input);
            startPacPositionLoop();
        });

        input.addEventListener('blur', function () {
            window.setTimeout(function () {
                if (pacDismissSuppressed || isPacVisible()) {
                    return;
                }
                if (lastAutocompleteInput === input) {
                    lastAutocompleteInput = null;
                }
                dismissPacDropdown(null, false);
            }, 450);
        });
    }

    function observePacContainers() {
        if (!window.MutationObserver || document.documentElement.dataset.pacObserverBound === 'true') {
            return;
        }

        document.documentElement.dataset.pacObserverBound = 'true';
        var observer = new MutationObserver(function (mutations) {
            var addedPac = false;
            mutations.forEach(function (mutation) {
                mutation.addedNodes.forEach(function (node) {
                    if (node.nodeType === 1 && node.classList && node.classList.contains('pac-container')) {
                        addedPac = true;
                    }
                });
            });

            if (addedPac) {
                var activeInput = lastAutocompleteInput || document.activeElement;
                hideStalePacContainers(activeInput);
                positionPacContainer(activeInput);
                startPacPositionLoop();
            }
        });

        observer.observe(document.body, { childList: true, subtree: true });
    }

    function bindPacDismissHandlers() {
        if (document.documentElement.dataset.pacDismissBound === 'true') {
            return;
        }
        document.documentElement.dataset.pacDismissBound = 'true';

        bindPacSelectionGuards();

        document.addEventListener('focusin', function (e) {
            if (pacDismissSuppressed) {
                return;
            }
            if (e.target.closest('.pac-container')) {
                return;
            }
            if (isAutocompleteInput(e.target)) {
                return;
            }
            dismissPacDropdown(null, false);
        }, true);

        document.addEventListener('scroll', function () {
            if (pacDismissSuppressed || isAutocompleteFieldFocused() || shouldKeepPacOpen()) {
                positionPacContainer(lastAutocompleteInput);
                return;
            }
            dismissPacDropdown(null, false);
        }, true);

        window.addEventListener('resize', function () {
            if (isAutocompleteFieldFocused() || shouldKeepPacOpen()) {
                positionPacContainer(lastAutocompleteInput);
            }
        });

        if (window.visualViewport) {
            var syncPacWithViewport = function () {
                if (pacDismissSuppressed || isAutocompleteFieldFocused() || shouldKeepPacOpen()) {
                    positionPacContainer(lastAutocompleteInput);
                }
            };
            window.visualViewport.addEventListener('resize', syncPacWithViewport);
            window.visualViewport.addEventListener('scroll', syncPacWithViewport);
        }
    }

    var pacPositionRafId = null;

    function resolvePacAnchor(input) {
        return input && isAutocompleteInput(input) ? input : lastAutocompleteInput;
    }

    function resolvePacField(target) {
        if (!target || typeof target.closest !== 'function') {
            return null;
        }

        return target.closest('.pa-field')
            || target.closest('.pf-field')
            || target.closest('.rl-cp-field')
            || target.closest('.ob-field');
    }

    function resolvePacHost(target) {
        if (!target) {
            return null;
        }

        var field = resolvePacField(target);
        if (field) {
            if (field.classList.contains('rl-cp-field--address')) {
                var card = field.closest('.rl-cp-card');
                if (card) {
                    return card;
                }
            }

            if (field.querySelector('[data-address-autocomplete]')) {
                var addressFormHost = field.closest('.pf-home-address-form, .ob-form-card');
                if (addressFormHost) {
                    return addressFormHost;
                }
            }

            // In multi-column address rows, anchor to the full row so the
            // dropdown is not squeezed into half the screen on mobile.
            var row = field.closest('.pa-field-row, .rl-cp-grid, .pf-row');
            if (row && (row.classList.contains('pa-field-row--2')
                || row.classList.contains('pa-field-row--3')
                || row.classList.contains('rl-cp-grid')
                || row.classList.contains('pf-row'))) {
                return row;
            }
            return field;
        }

        var searchWrap = typeof target.closest === 'function' && target.closest('.ob-search');
        if (searchWrap) {
            return searchWrap.parentElement || searchWrap;
        }

        return target.parentElement || null;
    }

    function ensureRelativeHost(host) {
        if (!host) {
            return;
        }

        var position = window.getComputedStyle(host).position;
        if (position === 'static') {
            host.style.setProperty('position', 'relative');
        }
    }

    function resolvePacTopOffset(host, field) {
        if (!field || host === field) {
            return '100%';
        }

        var fieldRect = field.getBoundingClientRect();
        var hostRect = host.getBoundingClientRect();
        return Math.max(0, Math.round(fieldRect.bottom - hostRect.top + 2)) + 'px';
    }

    function applyPacPosition(pac, input) {
        if (!pac || pac.classList.contains('pac-container--dismissed')) {
            return;
        }

        var target = resolvePacAnchor(input);
        var host = resolvePacHost(target);
        if (!host || !target) {
            return;
        }

        var field = resolvePacField(target);

        // Anchor the dropdown to the field (or its grid row) instead of fighting
        // Google's body-level positioning, which overlaps inputs on mobile.
        ensureRelativeHost(host);
        if (pac.parentElement !== host) {
            host.appendChild(pac);
        }

        pac.style.setProperty('position', 'absolute', 'important');
        pac.style.setProperty('top', resolvePacTopOffset(host, field), 'important');
        pac.style.setProperty('left', '0', 'important');
        pac.style.setProperty('right', 'auto', 'important');
        pac.style.setProperty('bottom', 'auto', 'important');
        pac.style.setProperty('width', '100%', 'important');
        pac.style.setProperty('min-width', '100%', 'important');
        pac.style.setProperty('max-width', 'none', 'important');
        pac.style.setProperty('margin', '0', 'important');
        pac.style.setProperty('transform', 'none', 'important');
        pac.style.setProperty('box-sizing', 'border-box', 'important');
        pac.style.setProperty('z-index', '100000', 'important');
    }

    function isPacContainerShowing(pac) {
        if (pac.classList.contains('pac-container--dismissed')) {
            return false;
        }
        if (pac.style.display === 'none' || pac.style.visibility === 'hidden') {
            return false;
        }
        return pac.getClientRects().length > 0 || pac.querySelector('.pac-item') !== null;
    }

    function positionPacContainer(input) {
        var anchor = resolvePacAnchor(input);
        if (!anchor) {
            return;
        }

        var activePac = null;
        document.querySelectorAll('.pac-container').forEach(function (pac) {
            if (!isPacContainerShowing(pac)) {
                return;
            }

            if (!activePac) {
                activePac = pac;
                resetPacContainerStyles(pac);
                applyPacPosition(pac, anchor);
                return;
            }

            pac.style.display = 'none';
            pac.style.visibility = 'hidden';
            pac.style.pointerEvents = 'none';
            pac.classList.add('pac-container--dismissed');
        });
    }

    // Re-apply the position every animation frame while the field is engaged so
    // Google's own repositioning can never leave the dropdown stranded at the top.
    // Keeps running while the input is focused (Google reuses one hidden container
    // and only toggles it), and while any suggestions remain visible.
    function pacPositionTick() {
        var active = lastAutocompleteInput;
        var keepRunning = !!active && (document.activeElement === active || isPacVisible());
        if (keepRunning) {
            positionPacContainer(active);
            pacPositionRafId = window.requestAnimationFrame(pacPositionTick);
        } else {
            pacPositionRafId = null;
        }
    }

    function startPacPositionLoop() {
        if (pacPositionRafId !== null || typeof window.requestAnimationFrame !== 'function') {
            return;
        }
        pacPositionRafId = window.requestAnimationFrame(pacPositionTick);
    }

    function scrollFieldIntoView(input) {
        if (!input) {
            return;
        }

        suppressPacDismiss(1200);
        window.setTimeout(function () {
            if (window.visualViewport && typeof input.getBoundingClientRect === 'function') {
                var rect = input.getBoundingClientRect();
                var viewportBottom = window.visualViewport.offsetTop + window.visualViewport.height;
                var targetBottom = viewportBottom - 96;
                if (rect.bottom > targetBottom || rect.top < window.visualViewport.offsetTop + 12) {
                    var scrollBy = rect.bottom - targetBottom;
                    if (rect.top < window.visualViewport.offsetTop + 12) {
                        scrollBy = rect.top - (window.visualViewport.offsetTop + 72);
                    }
                    window.scrollBy({ top: scrollBy, behavior: 'smooth' });
                }
            } else if (typeof input.scrollIntoView === 'function') {
                try {
                    input.scrollIntoView({ block: 'center', behavior: 'smooth' });
                } catch (e) {
                    input.scrollIntoView(true);
                }
            }
            window.setTimeout(function () { positionPacContainer(input); }, 320);
        }, 280);
    }

    function bindStateSelectZipLookup() {
        var linkedStateIds = {};

        document.querySelectorAll('[data-ac-state]').forEach(function (input) {
            var stateId = input.dataset.acState;
            if (!stateId) {
                return;
            }
            linkedStateIds[stateId] = true;
        });

        document.querySelectorAll('select[data-ac-zip]').forEach(function (select) {
            if (select.id) {
                linkedStateIds[select.id] = true;
            }
        });

        Object.keys(linkedStateIds).forEach(function (stateId) {
            var stateEl = document.getElementById(stateId);
            if (!stateEl || stateEl.dataset.zipLookupBound === 'true') {
                return;
            }
            stateEl.dataset.zipLookupBound = 'true';

            stateEl.addEventListener('change', function () {
                lookupZipFromStateSelect(stateEl);
            });
        });

        document.querySelectorAll('select[data-ac-zip][data-ac-city]').forEach(function (select) {
            if (!select.id || select.dataset.zipLookupBound === 'true') {
                return;
            }
            select.dataset.zipLookupBound = 'true';
            select.addEventListener('change', function () {
                lookupZipFromStateSelect(select);
            });
        });
    }

    function bindCityZipGeocode(sourceInput) {
        if (sourceInput.dataset.geocodeBound === 'true') {
            return;
        }
        sourceInput.dataset.geocodeBound = 'true';

        var stateEl = getLinkedElement(sourceInput, 'State');
        var cityEl = sourceInput.hasAttribute('data-city-autocomplete')
            ? sourceInput
            : getLinkedElement(sourceInput, 'City');

        if (cityEl) {
            cityEl.addEventListener('change', function () { tryGeocodeLinkedZip(sourceInput); });
            cityEl.addEventListener('blur', function () { tryGeocodeLinkedZip(sourceInput); });
        }

        if (stateEl) {
            stateEl.addEventListener('change', function () { tryGeocodeLinkedZip(sourceInput); });
        }
    }

    function initZipGeocodeLinking() {
        document.querySelectorAll('input[data-city-autocomplete]').forEach(function (input) {
            bindCityZipGeocode(input);
        });

        document.querySelectorAll('input[data-address-autocomplete]').forEach(function (input) {
            bindCityZipGeocode(input);
        });

        document.querySelectorAll('input[data-ac-zip][data-ac-state]:not([data-city-autocomplete]):not([data-address-autocomplete])').forEach(function (input) {
            bindCityZipGeocode(input);
        });

        bindStateSelectZipLookup();

        document.querySelectorAll('[data-ac-state]').forEach(function (input) {
            bindManualZipEdit(getLinkedElement(input, 'Zip'));
        });

        document.querySelectorAll('input[data-city-autocomplete], input[data-address-autocomplete], input[data-ac-zip][data-ac-state]:not([data-city-autocomplete]):not([data-address-autocomplete])').forEach(function (input) {
            if (shouldRunInitialZipLookup(input)) {
                tryGeocodeLinkedZip(input);
            }
        });

        document.querySelectorAll('select[data-ac-zip][data-ac-city]').forEach(function (select) {
            if (select.value.trim()) {
                lookupZipFromStateSelect(select);
            }
        });
    }

    function applyCityPlace(cityInput, place) {
        if (!place) return;
        var components = place.address_components || null;

        var city = getCityFromComponents(components)
            || place.name
            || '';
        if (city) {
            cityInput.value = city;
        }

        var state = getComponent(components, 'administrative_area_level_1', true);
        var stateEl = getLinkedElement(cityInput, 'State');
        if (stateEl && state) {
            filterStateSelect(stateEl, state);
            stateEl.value = state;
            stateEl.dispatchEvent(new Event('change', { bubbles: true }));
        }

        var zip = getComponent(components, 'postal_code', false);
        if (zip) {
            var zipEl = getLinkedElement(cityInput, 'Zip');
            if (zipEl) {
                setZipValue(zipEl, zip);
            }
        } else {
            tryGeocodeLinkedZip(cityInput);
        }

        cityInput.dispatchEvent(new Event('input', { bubbles: true }));
        cityInput.dispatchEvent(new Event('change', { bubbles: true }));
    }

    function initCityAutocomplete() {
        if (!(window.google && google.maps && google.maps.places && google.maps.places.Autocomplete)) {
            return;
        }

        document.querySelectorAll('input[data-city-autocomplete]').forEach(function (input) {
            if (input.dataset.cityAcInitialized === 'true') return;
            input.dataset.cityAcInitialized = 'true';
            input.setAttribute('autocomplete', 'off');

            var stateEl = getLinkedElement(input, 'State');
            if (stateEl) {
                ensureStateSelectSnapshot(stateEl);
            }

            var ac = new google.maps.places.Autocomplete(input, {
                types: ['(cities)'],
                componentRestrictions: { country: ['us'] },
                fields: ['address_components', 'formatted_address', 'name']
            });

            ac.addListener('place_changed', function () {
                applyCityPlace(input, ac.getPlace());
                finalizeAutocompleteSelection(input);
            });

            bindAutocompleteDismissHandlers(input);

            input.addEventListener('input', function () {
                if (!input.value.trim() && stateEl) {
                    restoreStateSelect(stateEl);
                }
            });

            input.addEventListener('keydown', function (e) {
                if (e.key !== 'Enter') return;
                var open = document.querySelector('.pac-container');
                if (open && open.offsetParent !== null) {
                    e.preventDefault();
                }
            });
        });
    }

    function formatStreetOnlyLine(components, place, fallback) {
        var num = getComponent(components, 'street_number', false);
        var route = getComponent(components, 'route', false);
        var line = [num, route].filter(Boolean).join(' ').trim();
        if (line) {
            return line;
        }

        if (place.formatted_address) {
            return place.formatted_address.split(',')[0].trim();
        }

        return fallback;
    }

    function applyPlace(input, place) {
        if (!place) return;
        var components = place.address_components || null;

        if (input.dataset.acStreetOnly === 'true' && components) {
            var num = getComponent(components, 'street_number', false);
            var route = getComponent(components, 'route', false);
            var houseNumberEl = getLinkedElement(input, 'HouseNumber');
            if (houseNumberEl && num) {
                fillLinkedField(input, 'HouseNumber', num);
                input.value = (route || input.value).trim();
            } else {
                input.value = formatStreetOnlyLine(components, place, input.value);
            }
        } else if (place.formatted_address) {
            input.value = place.formatted_address;
        }

        if (components) {
            applyAddressComponents(input, components);

            var zipEl = getLinkedElement(input, 'Zip');
            if (zipEl && shouldReplaceZip(zipEl)) {
                tryGeocodeLinkedZip(input);
            }
        }

        input.dispatchEvent(new Event('input', { bubbles: true }));
        input.dispatchEvent(new Event('change', { bubbles: true }));
    }

    function initAddressAutocomplete() {
        captureInitialStateSelectValues();
        bindPacDismissHandlers();
        observePacContainers();
        initZipGeocodeLinking();

        document.querySelectorAll('input[data-address-autocomplete]').forEach(function (input) {
            bindStreetAddressCompletion(input);
        });

        if (!(window.google && google.maps && google.maps.places && google.maps.places.Autocomplete)) {
            restoreInitialStateSelectValues();
            return;
        }

        document.querySelectorAll('input[data-address-autocomplete]').forEach(function (input) {
            if (input.dataset.acInitialized === 'true') return;
            input.dataset.acInitialized = 'true';
            input.setAttribute('autocomplete', 'off');

            var stateEl = getLinkedElement(input, 'State');
            if (stateEl) {
                ensureStateSelectSnapshot(stateEl);
            }

            var ac = new google.maps.places.Autocomplete(input, {
                types: ['address'],
                componentRestrictions: { country: ['us'] },
                fields: ['formatted_address', 'address_components']
            });

            ac.addListener('place_changed', function () {
                var place = ac.getPlace();
                if (!place) {
                    return;
                }

                if (!place.address_components || !place.address_components.length) {
                    if (place.formatted_address) {
                        geocodeFullAddress(input, place.formatted_address);
                    }
                    finalizeAutocompleteSelection(input);
                    return;
                }

                applyPlace(input, place);
                finalizeAutocompleteSelection(input);
            });

            bindAutocompleteDismissHandlers(input);

            // Don't let Enter submit the form while a suggestion list is open.
            input.addEventListener('keydown', function (e) {
                if (e.key !== 'Enter') return;
                var open = document.querySelector('.pac-container');
                if (open && open.offsetParent !== null) {
                    e.preventDefault();
                }
            });
        });

        initCityAutocomplete();
        restoreInitialStateSelectValues();
    }

    // Google Maps JS calls this global callback once it finishes loading.
    window.indorInitAddressAutocomplete = initAddressAutocomplete;

    function scheduleGooglePlacesInitRetry() {
        if (window.__indorAcGoogleRetryStarted) {
            return;
        }
        window.__indorAcGoogleRetryStarted = true;

        var attempts = 0;
        var timer = window.setInterval(function () {
            attempts++;
            var pending = Array.from(document.querySelectorAll('input[data-address-autocomplete], input[data-city-autocomplete]'))
                .some(function (input) { return input.dataset.acInitialized !== 'true'; });

            if (!pending || attempts > 60) {
                window.clearInterval(timer);
                return;
            }

            if (window.google && google.maps && google.maps.places && google.maps.places.Autocomplete) {
                initAddressAutocomplete();
            }
        }, 250);
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function () {
            initAddressAutocomplete();
            scheduleGooglePlacesInitRetry();
        });
    } else {
        initAddressAutocomplete();
        scheduleGooglePlacesInitRetry();
    }
})();
