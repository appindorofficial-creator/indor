(function () {
    'use strict';

    // Attaches Google Places Autocomplete to any input marked with
    // [data-address-autocomplete]. When the Places library is unavailable
    // (no API key, blocked, offline) the field stays a normal text input and
    // server-side validation still enforces a complete address.

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
        target.value = value;
        target.dispatchEvent(new Event('input', { bubbles: true }));
        target.dispatchEvent(new Event('change', { bubbles: true }));
    }

    var geocodeTimers = typeof WeakMap !== 'undefined' ? new WeakMap() : null;
    var stateSelectSnapshots = typeof WeakMap !== 'undefined' ? new WeakMap() : null;

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
        zipEl.value = zip;
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

    function runServerZipLookup(city, state, zipEl) {
        if (!city || !state || !zipEl || !shouldReplaceZip(zipEl)) {
            return;
        }

        var url = '/AddressLookup/Zip?city=' + encodeURIComponent(city)
            + '&state=' + encodeURIComponent(state);

        fetch(url, {
            headers: { 'Accept': 'application/json' },
            credentials: 'same-origin'
        }).then(function (response) {
            if (!response.ok || !shouldReplaceZip(zipEl)) {
                return null;
            }
            return response.json();
        }).then(function (payload) {
            if (!payload || !payload.zip || !shouldReplaceZip(zipEl)) {
                return;
            }
            setZipValue(zipEl, payload.zip);
        }).catch(function () {
            // Ignore lookup failures; the user can enter ZIP manually.
        });
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
            runServerZipLookup(fields.city, state, fields.zipEl);
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

    function runGeocode(sourceInput, address, zipEl) {
        if (!shouldReplaceZip(zipEl)) return;

        if (!(window.google && google.maps && google.maps.Geocoder)) {
            return;
        }

        var geocoder = new google.maps.Geocoder();
        geocoder.geocode({
            address: address,
            componentRestrictions: { country: 'US' }
        }, function (results, status) {
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
                var city = getComponent(components, 'locality', false)
                    || getComponent(components, 'sublocality', false)
                    || getComponent(components, 'postal_town', false);
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
        });
    }

    function debounceGeocode(sourceInput) {
        if (geocodeTimers) {
            clearTimeout(geocodeTimers.get(sourceInput));
            geocodeTimers.set(sourceInput, setTimeout(function () {
                tryGeocodeLinkedZip(sourceInput);
            }, 400));
        } else {
            tryGeocodeLinkedZip(sourceInput);
        }
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
                document.querySelectorAll('[data-ac-state="' + stateId + '"]').forEach(function (input) {
                    lookupLinkedZip(input);
                });
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
            cityEl.addEventListener('input', function () { debounceGeocode(sourceInput); });
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
            tryGeocodeLinkedZip(input);
        });
    }

    function applyCityPlace(cityInput, place) {
        if (!place) return;
        var components = place.address_components || null;

        var city = getComponent(components, 'locality', false)
            || getComponent(components, 'sublocality', false)
            || getComponent(components, 'postal_town', false)
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
            });

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
            input.value = formatStreetOnlyLine(components, place, input.value);
        } else if (place.formatted_address) {
            input.value = place.formatted_address;
        }

        if (components) {
            var city = getComponent(components, 'locality', false)
                || getComponent(components, 'sublocality', false)
                || getComponent(components, 'postal_town', false);
            fillLinkedField(input, 'City', city);
            var stateCode = getComponent(components, 'administrative_area_level_1', true);
            fillLinkedField(input, 'State', stateCode);
            var zipCode = getComponent(components, 'postal_code', false);
            if (zipCode) {
                var zipEl = getLinkedElement(input, 'Zip');
                if (zipEl) {
                    setZipValue(zipEl, zipCode);
                }
            }

            var stateEl = getLinkedElement(input, 'State');
            if (stateEl && stateCode) {
                ensureStateSelectSnapshot(stateEl);
                filterStateSelect(stateEl, stateCode);
                stateEl.value = stateCode;
            }
        }

        input.dispatchEvent(new Event('input', { bubbles: true }));
        input.dispatchEvent(new Event('change', { bubbles: true }));
    }

    function initAddressAutocomplete() {
        initZipGeocodeLinking();

        if (!(window.google && google.maps && google.maps.places && google.maps.places.Autocomplete)) {
            return;
        }

        document.querySelectorAll('input[data-address-autocomplete]').forEach(function (input) {
            if (input.dataset.acInitialized === 'true') return;
            input.dataset.acInitialized = 'true';
            input.setAttribute('autocomplete', 'off');

            var ac = new google.maps.places.Autocomplete(input, {
                types: ['address'],
                componentRestrictions: { country: ['us'] },
                fields: ['formatted_address', 'address_components']
            });

            ac.addListener('place_changed', function () {
                var place = ac.getPlace();
                if (!place || !place.address_components || !place.address_components.length) {
                    return;
                }
                applyPlace(input, place);
            });

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
    }

    // Google Maps JS calls this global callback once it finishes loading.
    window.indorInitAddressAutocomplete = initAddressAutocomplete;

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initAddressAutocomplete);
    } else {
        initAddressAutocomplete();
    }
})();
