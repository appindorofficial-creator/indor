window.rlInitNearbyNetworkMap = function () {
    var root = document.getElementById('rlNearbyNetworkMap');
    var dataEl = document.getElementById('rl-network-map-data');
    if (!root || !dataEl || !window.google || !google.maps) {
        return;
    }

    var config;
    try {
        config = JSON.parse(dataEl.textContent || '{}');
    } catch (e) {
        return;
    }

    if (!config.center || typeof config.center.lat !== 'number' || typeof config.center.lng !== 'number') {
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
    var filterBtn = document.getElementById('rlMapFilterBtn');
    var infoWindow = new google.maps.InfoWindow();

    var state = {
        center: { lat: config.center.lat, lng: config.center.lng },
        mode: 'initial',
        userLocation: null,
        markers: [],
        userMarker: null,
        centerMarker: null,
        radiusCircle: null
    };

    var map = new google.maps.Map(root, {
        center: state.center,
        zoom: 13,
        styles: mapStyles,
        disableDefaultUI: true,
        zoomControl: false,
        gestureHandling: 'greedy',
        clickableIcons: false
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
        return text.slice(0, max - 1) + '…';
    }

    function clearMarkers() {
        state.markers.forEach(function (marker) {
            marker.setMap(null);
        });
        state.markers = [];
    }

    function ZillowMarkerOverlay(position, options) {
        this.position = position;
        this.options = options || {};
    }

    ZillowMarkerOverlay.prototype = new google.maps.OverlayView();
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

    UserDotOverlay.prototype = new google.maps.OverlayView();
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

    SearchCenterOverlay.prototype = new google.maps.OverlayView();
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
            bounds.extend(new google.maps.LatLng(p.lat, p.lng));
        });
        (listings || []).forEach(function (l) {
            bounds.extend(new google.maps.LatLng(l.lat, l.lng));
        });

        if ((providers || []).length + (listings || []).length === 0) {
            map.setCenter({ lat: lat, lng: lng });
            map.setZoom(13);
            return;
        }

        map.fitBounds(bounds, { top: 110, right: 36, bottom: 120, left: 36 });
    }

    function renderProviders(providers) {
        (providers || []).forEach(function (provider) {
            if (typeof provider.lat !== 'number' || typeof provider.lng !== 'number') {
                return;
            }

            var overlay = new ZillowMarkerOverlay(new google.maps.LatLng(provider.lat, provider.lng), {
                kind: 'provider',
                label: shortLabel(provider.name || 'Provider', 14),
                verified: !!provider.verified,
                onClick: function () {
                    document.querySelectorAll('.rl-zillow-marker.is-active').forEach(function (el) {
                        el.classList.remove('is-active');
                    });
                    overlay.div?.classList.add('is-active');

                    var html = '<div class="rl-map-info"><strong>' + (provider.name || 'Provider') + '</strong>';
                    if (provider.category) {
                        html += '<span>' + provider.category + '</span>';
                    }
                    if (provider.distanceMiles != null) {
                        html += '<span>' + provider.distanceMiles.toFixed(1) + ' mi away</span>';
                    }
                    html += '</div>';
                    infoWindow.setContent(html);
                    infoWindow.setPosition({ lat: provider.lat, lng: provider.lng });
                    infoWindow.open(map);
                }
            });
            overlay.setMap(map);
            state.markers.push(overlay);
        });
    }

    function renderListings(listings) {
        (listings || []).forEach(function (pin) {
            if (typeof pin.lat !== 'number' || typeof pin.lng !== 'number') {
                return;
            }

            var label = pin.label || 'Listing';
            if (label.charAt(0) === '$') {
                label = shortLabel(label, 10);
            } else {
                label = shortLabel(label, 12);
            }

            var overlay = new ZillowMarkerOverlay(new google.maps.LatLng(pin.lat, pin.lng), {
                kind: pin.type || 'home',
                label: label,
                onClick: function () {
                    infoWindow.setContent('<div class="rl-map-info"><strong>' + (pin.label || 'Listing') + '</strong></div>');
                    infoWindow.setPosition({ lat: pin.lat, lng: pin.lng });
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

        if (options.showSearchCenter) {
            setSearchCenter(data.lat, data.lng, data.centerLabel);
        } else if (options.showUserCenter) {
            state.mode = 'gps';
            state.center = { lat: data.lat, lng: data.lng };
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

        fitToResults(data.lat, data.lng, data.providers, data.listings);

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
            subtitle += ' · ' + parts.join(' · ');
        } else {
            subtitle += ' · ' + total + ' nearby';
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

    function loadFromGps() {
        if (!navigator.geolocation) {
            return;
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
                    });
            },
            function () {
                applyMapData({
                    lat: config.center.lat,
                    lng: config.center.lng,
                    centerLabel: config.centerLabel || 'Your service area',
                    providers: config.providers || [],
                    listings: config.listings || []
                });
                updateFootLabel(config.centerLabel || 'Your service area', 'Location unavailable · enable GPS for best results');
            },
            { enableHighAccuracy: true, timeout: 12000, maximumAge: 60000 }
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
        locateBtn.addEventListener('click', loadFromGps);
    }

    if (filterBtn) {
        filterBtn.addEventListener('click', function () {
            var filters = document.querySelector('.rl-zillow-map-filters');
            if (filters) {
                filters.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
            }
        });
    }

    if (config.useDeviceLocation) {
        loadFromGps();
    } else {
        applyMapData({
            lat: config.center.lat,
            lng: config.center.lng,
            centerLabel: config.centerLabel,
            providers: config.providers || [],
            listings: config.listings || []
        });
    }
};
