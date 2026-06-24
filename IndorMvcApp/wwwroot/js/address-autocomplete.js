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

    function tryGeocodeLinkedZip(sourceInput) {
        if (!(window.google && google.maps && google.maps.Geocoder)) return;

        var zipEl = getLinkedElement(sourceInput, 'Zip');
        var stateEl = getLinkedElement(sourceInput, 'State');
        if (!zipEl || !stateEl) return;
        if (zipEl.value.trim()) return;

        var state = stateEl.value.trim();
        if (!state) return;

        var isCitySource = sourceInput.hasAttribute('data-city-autocomplete');
        var cityEl = isCitySource ? sourceInput : getLinkedElement(sourceInput, 'City');
        var city = cityEl ? cityEl.value.trim() : '';
        var street = isCitySource ? '' : sourceInput.value.trim();
        if (!city && !street) return;

        var address;
        if (street && city) {
            address = street + ', ' + city + ', ' + state + ', USA';
        } else if (city) {
            address = city + ', ' + state + ', USA';
        } else {
            address = street + ', ' + state + ', USA';
        }

        scheduleGeocode(sourceInput, address, zipEl);
    }

    function runGeocode(sourceInput, address, zipEl) {
        if (zipEl.value.trim()) return;

        var geocoder = new google.maps.Geocoder();
        geocoder.geocode({
            address: address,
            componentRestrictions: { country: 'US' }
        }, function (results, status) {
            if (status !== 'OK' || !results || !results.length) return;
            if (zipEl.value.trim()) return;

            var components = results[0].address_components;
            var zip = getComponent(components, 'postal_code', false);
            if (!zip) return;

            zipEl.value = zip;
            zipEl.dispatchEvent(new Event('input', { bubbles: true }));
            zipEl.dispatchEvent(new Event('change', { bubbles: true }));

            var cityEl = sourceInput.hasAttribute('data-city-autocomplete')
                ? sourceInput
                : getLinkedElement(sourceInput, 'City');
            if (cityEl && !cityEl.value.trim()) {
                var city = getComponent(components, 'locality', false)
                    || getComponent(components, 'sublocality', false)
                    || getComponent(components, 'postal_town', false);
                if (city) {
                    if (sourceInput.hasAttribute('data-city-autocomplete')) {
                        cityEl.value = city;
                        cityEl.dispatchEvent(new Event('input', { bubbles: true }));
                        cityEl.dispatchEvent(new Event('change', { bubbles: true }));
                    } else {
                        fillLinkedField(sourceInput, 'City', city);
                    }
                }
            }
        });
    }

    function bindLinkedFieldGeocode(streetInput) {
        var stateEl = getLinkedElement(streetInput, 'State');
        var cityEl = getLinkedElement(streetInput, 'City');
        if (stateEl) {
            stateEl.addEventListener('change', function () { tryGeocodeLinkedZip(streetInput); });
        }
        if (cityEl) {
            cityEl.addEventListener('change', function () { tryGeocodeLinkedZip(streetInput); });
            cityEl.addEventListener('blur', function () { tryGeocodeLinkedZip(streetInput); });
        }
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
            fillLinkedField(cityInput, 'Zip', zip);
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

            input.addEventListener('change', function () { tryGeocodeLinkedZip(input); });
            input.addEventListener('blur', function () { tryGeocodeLinkedZip(input); });

            if (stateEl) {
                stateEl.addEventListener('change', function () { tryGeocodeLinkedZip(input); });
            }

            input.addEventListener('keydown', function (e) {
                if (e.key !== 'Enter') return;
                var open = document.querySelector('.pac-container');
                if (open && open.offsetParent !== null) {
                    e.preventDefault();
                }
            });
        });
    }

    function applyPlace(input, place) {
        if (!place) return;
        var components = place.address_components || null;

        if (input.dataset.acStreetOnly === 'true' && components) {
            var num = getComponent(components, 'street_number', false);
            var route = getComponent(components, 'route', false);
            var street = (num + ' ' + route).trim();
            input.value = street || place.formatted_address || input.value;
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
            fillLinkedField(input, 'Zip', getComponent(components, 'postal_code', false));

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
        if (!(window.google && google.maps && google.maps.places && google.maps.places.Autocomplete)) {
            return;
        }

        document.querySelectorAll('input[data-address-autocomplete]').forEach(function (input) {
            if (input.dataset.acInitialized === 'true') return;
            input.dataset.acInitialized = 'true';

            var ac = new google.maps.places.Autocomplete(input, {
                types: ['address'],
                componentRestrictions: { country: ['us'] },
                fields: ['formatted_address', 'address_components']
            });

            ac.addListener('place_changed', function () {
                applyPlace(input, ac.getPlace());
            });

            bindLinkedFieldGeocode(input);

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
