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

    function tryGeocodeLinkedZip(streetInput) {
        if (!(window.google && google.maps && google.maps.Geocoder)) return;

        var zipEl = getLinkedElement(streetInput, 'Zip');
        var stateEl = getLinkedElement(streetInput, 'State');
        var cityEl = getLinkedElement(streetInput, 'City');
        if (!zipEl || !stateEl) return;
        if (zipEl.value.trim()) return;

        var street = streetInput.value.trim();
        var state = stateEl.value.trim();
        if (!street || !state) return;

        var city = cityEl ? cityEl.value.trim() : '';
        var address = street;
        if (city) address += ', ' + city;
        address += ', ' + state + ', USA';

        if (geocodeTimers) {
            clearTimeout(geocodeTimers.get(streetInput));
            geocodeTimers.set(streetInput, setTimeout(function () {
                runGeocode(streetInput, address, zipEl);
            }, 400));
        } else {
            runGeocode(streetInput, address, zipEl);
        }
    }

    function runGeocode(streetInput, address, zipEl) {
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

            var cityEl = getLinkedElement(streetInput, 'City');
            if (cityEl && !cityEl.value.trim()) {
                var city = getComponent(components, 'locality', false)
                    || getComponent(components, 'sublocality', false)
                    || getComponent(components, 'postal_town', false);
                if (city) fillLinkedField(streetInput, 'City', city);
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
            fillLinkedField(input, 'State', getComponent(components, 'administrative_area_level_1', true));
            fillLinkedField(input, 'Zip', getComponent(components, 'postal_code', false));
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
    }

    // Google Maps JS calls this global callback once it finishes loading.
    window.indorInitAddressAutocomplete = initAddressAutocomplete;

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initAddressAutocomplete);
    } else {
        initAddressAutocomplete();
    }
})();
