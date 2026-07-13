window.rlShowNearbyMapError = function (message) {
    var root = document.getElementById('rlNearbyNetworkMap');
    if (!root) {
        return;
    }

    var i18n = (window.rlNearbyNetworkI18n) || {};
    root.innerHTML = '';
    var panel = document.createElement('div');
    panel.className = 'rl-map-error';
    panel.innerHTML = '<i class="fas fa-map-location-dot"></i><strong>' +
        (i18n.mapUnavailable || 'Map unavailable') +
        '</strong><span>' +
        (message || i18n.mapsLoadFailed || 'Google Maps could not load. Check your connection and try again.') +
        '</span>';
    root.appendChild(panel);
};

window.gm_authFailure = function () {
    window.rlShowNearbyMapError('Google Maps rejected this site or API key. Add this domain to your Maps key referrer restrictions.');
};

window.rlInitNearbyNetworkMap = function () {
    var root = document.getElementById('rlNearbyNetworkMap');
    var dataEl = document.getElementById('rl-network-map-data');

    if (!root || !dataEl) {
        return;
    }

    if (!window.google || !google.maps) {
        window.rlShowNearbyMapError('Google Maps did not finish loading.');
        return;
    }

    var config;
    try {
        config = JSON.parse(dataEl.textContent || '{}');
    } catch (e) {
        window.rlShowNearbyMapError('Map configuration could not be read.');
        return;
    }

    window.rlNearbyNetworkI18n = config.i18n || {};
    var i18n = window.rlNearbyNetworkI18n;

    function formatMiAway(miles) {
        var template = i18n.miAway || '{0} mi away';
        return template.replace('{0}', Number(miles).toFixed(1));
    }

    function parseCoord(value, fallback) {
        var num = typeof value === 'number' ? value : parseFloat(value);
        return Number.isFinite(num) ? num : fallback;
    }

    var defaultLat = parseCoord(config.center && config.center.lat, 35.2271);
    var defaultLng = parseCoord(config.center && config.center.lng, -80.8431);

    if (!Number.isFinite(defaultLat) || !Number.isFinite(defaultLng)) {
        window.rlShowNearbyMapError('Map center coordinates are invalid.');
        return;
    }

    var mapStyles = [
        { featureType: 'poi.business', stylers: [{ visibility: 'off' }] },
        { featureType: 'transit', stylers: [{ visibility: 'off' }] }
    ];

    var radiusMeters = (config.radiusMiles || 3) * 1609.34;
    var activeFilter = config.activeFilter || 'All';
    var mapDataUrl = config.mapDataUrl || '/Realtor/NetworkMapData';
    var searchInput = document.getElementById('rlMapSearchInput');
    var searchForm = document.getElementById('rlMapSearchForm');
    var locateBtn = document.getElementById('rlMapLocateBtn');
    var infoWindow = new google.maps.InfoWindow();

    var state = {
        center: { lat: defaultLat, lng: defaultLng },
        mode: 'initial',
        userLocation: null,
        markers: [],
        userMarker: null,
        centerMarker: null,
        radiusCircle: null
    };

    function inheritOverlay(ctor) {
        ctor.prototype = Object.create(google.maps.OverlayView.prototype);
        ctor.prototype.constructor = ctor;
    }

    function ZillowMarkerOverlay(position, options) {
        this.position = position;
        this.options = options || {};
    }

    inheritOverlay(ZillowMarkerOverlay);
    ZillowMarkerOverlay.prototype.onAdd = function () {
        var el = document.createElement('button');
        el.type = 'button';
        el.className = 'rl-zillow-marker rl-zillow-marker--' + (this.options.kind || 'provider');
        if (this.options.verified) {
            el.classList.add('is-verified');
        }
        el.textContent = this.options.label || '';
        el.addEventListener('click', this.options.onClick || function () {});
        this.div = el;
        this.getPanes().overlayMouseTarget.appendChild(el);
    };
    ZillowMarkerOverlay.prototype.draw = function () {
        var projection = this.getProjection();
        if (!projection || !this.div) {
            return;
        }
        var point = projection.fromLatLngToDivPixel(this.position);
        if (!point) {
            return;
        }
        this.div.style.left = point.x + 'px';
        this.div.style.top = point.y + 'px';
    };
    ZillowMarkerOverlay.prototype.onRemove = function () {
        if (this.div && this.div.parentNode) {
            this.div.parentNode.removeChild(this.div);
        }
        this.div = null;
    };

    function UserDotOverlay(position) {
        this.position = position;
    }

    inheritOverlay(UserDotOverlay);
    UserDotOverlay.prototype.onAdd = function () {
        this.div = document.createElement('div');
        this.div.className = 'rl-zillow-user-dot';
        this.div.setAttribute('aria-label', 'Your location');
        this.getPanes().overlayMouseTarget.appendChild(this.div);
    };
    UserDotOverlay.prototype.draw = function () {
        var projection = this.getProjection();
        if (!projection || !this.div) {
            return;
        }
        var point = projection.fromLatLngToDivPixel(this.position);
        if (!point) {
            return;
        }
        this.div.style.left = point.x + 'px';
        this.div.style.top = point.y + 'px';
    };
    UserDotOverlay.prototype.onRemove = function () {
        if (this.div && this.div.parentNode) {
            this.div.parentNode.removeChild(this.div);
        }
        this.div = null;
    };

    function SearchCenterOverlay(position) {
        this.position = position;
    }

    inheritOverlay(SearchCenterOverlay);
    SearchCenterOverlay.prototype.onAdd = function () {
        this.div = document.createElement('div');
        this.div.className = 'rl-zillow-search-dot';
        this.getPanes().overlayMouseTarget.appendChild(this.div);
    };
    SearchCenterOverlay.prototype.draw = function () {
        var projection = this.getProjection();
        if (!projection || !this.div) {
            return;
        }
        var point = projection.fromLatLngToDivPixel(this.position);
        if (!point) {
            return;
        }
        this.div.style.left = point.x + 'px';
        this.div.style.top = point.y + 'px';
    };
    SearchCenterOverlay.prototype.onRemove = function () {
        if (this.div && this.div.parentNode) {
            this.div.parentNode.removeChild(this.div);
        }
        this.div = null;
    };

    var map;
    try {
        map = new google.maps.Map(root, {
            center: state.center,
            zoom: 13,
            mapTypeId: google.maps.MapTypeId.ROADMAP,
            styles: mapStyles,
            disableDefaultUI: true,
            zoomControl: false,
            gestureHandling: 'greedy',
            clickableIcons: false
        });
    } catch (err) {
        window.rlShowNearbyMapError('Google Maps failed to initialize.');
        return;
    }

    function refreshMapLayout() {
        if (!map) {
            return;
        }
        google.maps.event.trigger(map, 'resize');
        map.setCenter(state.center);
    }

    window.setTimeout(refreshMapLayout, 0);
    window.setTimeout(refreshMapLayout, 250);
    window.addEventListener('resize', refreshMapLayout);
    window.addEventListener('orientationchange', function () {
        window.setTimeout(refreshMapLayout, 300);
    });

    state.radiusCircle = new google.maps.Circle({
        map: map,
        center: state.center,
        radius: radiusMeters,
        fillColor: '#0066CC',
        fillOpacity: 0.06,
        strokeColor: '#0066CC',
        strokeOpacity: 0.18,
        strokeWeight: 1,
        clickable: false
    });

    function updateFootLabel(title, subtitle) {
        var titleEl = document.getElementById('rl-map-location-label');
        var metaEl = document.getElementById('rl-map-location-meta');
        if (titleEl && title) {
            titleEl.textContent = title;
        }
        if (metaEl && subtitle) {
            metaEl.textContent = subtitle;
        }
    }

    function shortLabel(text, max) {
        if (!text) {
            return '';
        }
        text = String(text).trim();
        if (text.length <= max) {
            return text;
        }
        return text.slice(0, max - 1) + '\u2026';
    }

    function clearMarkers() {
        state.markers.forEach(function (marker) {
            marker.setMap(null);
        });
        state.markers = [];
    }

    function setSearchCenter(lat, lng, label) {
        state.center = { lat: lat, lng: lng };
        state.mode = 'search';
        if (state.centerMarker) {
            state.centerMarker.setMap(null);
        }
        state.centerMarker = new SearchCenterOverlay(new google.maps.LatLng(lat, lng));
        state.centerMarker.setMap(map);
        if (state.radiusCircle) {
            state.radiusCircle.setCenter(state.center);
        }
        if (searchInput && label) {
            searchInput.value = label;
        }
    }

    function setUserLocation(lat, lng) {
        state.userLocation = { lat: lat, lng: lng };
        if (state.userMarker) {
            state.userMarker.setMap(null);
        }
        state.userMarker = new UserDotOverlay(new google.maps.LatLng(lat, lng));
        state.userMarker.setMap(map);
    }

    function fitToResults(lat, lng, providers, listings) {
        var bounds = new google.maps.LatLngBounds();
        bounds.extend(new google.maps.LatLng(lat, lng));

        (providers || []).forEach(function (p) {
            var plat = parseCoord(p.lat, null);
            var plng = parseCoord(p.lng, null);
            if (Number.isFinite(plat) && Number.isFinite(plng)) {
                bounds.extend(new google.maps.LatLng(plat, plng));
            }
        });
        (listings || []).forEach(function (l) {
            var llat = parseCoord(l.lat, null);
            var llng = parseCoord(l.lng, null);
            if (Number.isFinite(llat) && Number.isFinite(llng)) {
                bounds.extend(new google.maps.LatLng(llat, llng));
            }
        });

        if ((providers || []).length + (listings || []).length === 0) {
            map.setCenter({ lat: lat, lng: lng });
            map.setZoom(13);
            refreshMapLayout();
            return;
        }

        map.fitBounds(bounds, { top: 110, right: 36, bottom: 120, left: 36 });
        refreshMapLayout();
    }

    function renderProviders(providers) {
        (providers || []).forEach(function (provider) {
            var plat = parseCoord(provider.lat, null);
            var plng = parseCoord(provider.lng, null);
            if (!Number.isFinite(plat) || !Number.isFinite(plng)) {
                return;
            }

            var overlay = new ZillowMarkerOverlay(new google.maps.LatLng(plat, plng), {
                kind: 'provider',
                label: shortLabel(provider.name || i18n.provider || 'Provider', 14),
                verified: !!provider.verified,
                onClick: function () {
                    document.querySelectorAll('.rl-zillow-marker.is-active').forEach(function (el) {
                        el.classList.remove('is-active');
                    });
                    if (overlay.div) {
                        overlay.div.classList.add('is-active');
                    }

                    var html = '<div class="rl-map-info"><strong>' + (provider.name || i18n.provider || 'Provider') + '</strong>';
                    if (provider.category) {
                        html += '<span>' + provider.category + '</span>';
                    }
                    if (provider.distanceMiles != null) {
                        html += '<span>' + formatMiAway(provider.distanceMiles) + '</span>';
                    }
                    html += '</div>';
                    infoWindow.setContent(html);
                    infoWindow.setPosition({ lat: plat, lng: plng });
                    infoWindow.open(map);
                }
            });
            overlay.setMap(map);
            state.markers.push(overlay);
        });
    }

    function renderListings(listings) {
        (listings || []).forEach(function (pin) {
            var plat = parseCoord(pin.lat, null);
            var plng = parseCoord(pin.lng, null);
            if (!Number.isFinite(plat) || !Number.isFinite(plng)) {
                return;
            }

            var label = pin.label || i18n.listing || 'Listing';
            if (label.charAt(0) === '$') {
                label = shortLabel(label, 10);
            } else {
                label = shortLabel(label, 12);
            }

            var overlay = new ZillowMarkerOverlay(new google.maps.LatLng(plat, plng), {
                kind: pin.type || 'home',
                label: label,
                onClick: function () {
                    infoWindow.setContent('<div class="rl-map-info"><strong>' + (pin.label || i18n.listing || 'Listing') + '</strong></div>');
                    infoWindow.setPosition({ lat: plat, lng: plng });
                    infoWindow.open(map);
                }
            });
            overlay.setMap(map);
            state.markers.push(overlay);
        });
    }

    function applyMapData(data, options) {
        options = options || {};
        clearMarkers();

        var lat = parseCoord(data.lat, defaultLat);
        var lng = parseCoord(data.lng, defaultLng);

        if (options.showSearchCenter) {
            setSearchCenter(lat, lng, data.centerLabel);
        } else if (options.showUserCenter) {
            state.mode = 'gps';
            state.center = { lat: lat, lng: lng };
            if (state.centerMarker) {
                state.centerMarker.setMap(null);
                state.centerMarker = null;
            }
            if (state.radiusCircle) {
                state.radiusCircle.setCenter(state.center);
            }
        }

        renderProviders(data.providers);
        renderListings(data.listings);
        fitToResults(lat, lng, data.providers, data.listings);

        var providerCount = data.providerCount != null ? data.providerCount : (data.providers || []).length;
        var listingCount = data.listingCount != null ? data.listingCount : (data.listings || []).length;
        var total = data.totalCount != null ? data.totalCount : providerCount + listingCount;

        var title = data.centerLabel || 'Nearby';
        var parts = [];
        if (providerCount > 0) {
            parts.push(providerCount + ' provider' + (providerCount === 1 ? '' : 's'));
        }
        if (listingCount > 0) {
            parts.push(listingCount + ' listing' + (listingCount === 1 ? '' : 's'));
        }
        var subtitle = (config.radiusMiles || 3).toFixed(1) + ' mi radius';
        if (parts.length) {
            subtitle += ' \u00b7 ' + parts.join(' \u00b7 ');
        } else {
            subtitle += ' \u00b7 ' + total + ' nearby';
        }

        updateFootLabel(title, subtitle);
    }

    function fetchMapData(params) {
        var url = new URL(mapDataUrl, window.location.origin);
        Object.keys(params).forEach(function (key) {
            if (params[key] != null && params[key] !== '') {
                url.searchParams.set(key, params[key]);
            }
        });
        url.searchParams.set('filter', activeFilter);

        return fetch(url.toString(), {
            headers: { Accept: 'application/json' },
            credentials: 'same-origin'
        }).then(function (response) {
            if (!response.ok) {
                throw new Error('not-found');
            }
            return response.json();
        });
    }

    function loadFromGps(options) {
        options = options || {};
        if (!navigator.geolocation) {
            return;
        }

        var highAccuracy = !!options.highAccuracy;
        if (locateBtn) {
            locateBtn.classList.add('is-loading');
            locateBtn.setAttribute('aria-busy', 'true');
            locateBtn.disabled = true;
        }

        function clearLocateLoading() {
            if (!locateBtn) {
                return;
            }
            locateBtn.classList.remove('is-loading');
            locateBtn.removeAttribute('aria-busy');
            locateBtn.disabled = false;
        }

        navigator.geolocation.getCurrentPosition(
            function (position) {
                var lat = position.coords.latitude;
                var lng = position.coords.longitude;
                setUserLocation(lat, lng);

                fetchMapData({ lat: lat, lng: lng })
                    .then(function (data) {
                        data.centerLabel = 'Your location';
                        applyMapData(data, { showUserCenter: true });
                        if (searchInput) {
                            searchInput.value = '';
                            searchInput.placeholder = 'Search address or neighborhood';
                        }
                    })
                    .catch(function () {
                        applyMapData({
                            lat: lat,
                            lng: lng,
                            centerLabel: 'Your location',
                            providers: config.providers || [],
                            listings: config.listings || []
                        }, { showUserCenter: true });
                    })
                    .finally(clearLocateLoading);
            },
            function () {
                clearLocateLoading();
                updateFootLabel(config.centerLabel || 'Your service area', 'Location unavailable \u00b7 enable GPS for best results');
            },
            {
                enableHighAccuracy: highAccuracy,
                timeout: highAccuracy ? 12000 : 5000,
                maximumAge: 60000
            }
        );
    }

    function loadFromAddress(query) {
        if (!query || !query.trim()) {
            loadFromGps();
            return;
        }

        fetchMapData({ q: query.trim() })
            .then(function (data) {
                applyMapData(data, { showSearchCenter: true });
            })
            .catch(function () {
                window.alert('Could not find that address. Try a full street address with city and state.');
            });
    }

    if (searchForm) {
        searchForm.addEventListener('submit', function (event) {
            event.preventDefault();
            loadFromAddress(searchInput ? searchInput.value : '');
        });
    }

    if (locateBtn) {
        locateBtn.addEventListener('click', function () {
            loadFromGps({ highAccuracy: true });
        });
    }

    applyMapData({
        lat: defaultLat,
        lng: defaultLng,
        centerLabel: config.centerLabel || 'Your service area',
        providers: config.providers || [],
        listings: config.listings || []
    });

    window.rlNearbyNetworkMapReady = true;
    root.removeAttribute('aria-busy');

    if (config.useDeviceLocation) {
        // Faster initial fix so the map feels ready sooner; locate button still uses high accuracy.
        loadFromGps({ highAccuracy: false });
    }
};
