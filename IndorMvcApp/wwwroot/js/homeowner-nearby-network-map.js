window.hnShowNearbyMapError = function (message) {
    var root = document.getElementById('hnNearbyNetworkMap');
    if (!root) {
        return;
    }

    root.innerHTML = '';
    var panel = document.createElement('div');
    panel.className = 'hn-map-error';
    panel.innerHTML = '<i class="fas fa-map-location-dot"></i><strong>Map unavailable</strong><span>' +
        (message || 'Google Maps could not load. Check your connection and try again.') + '</span>';
    root.appendChild(panel);
};

window.hnInitNearbyNetworkMap = function () {
    var root = document.getElementById('hnNearbyNetworkMap');
    var dataEl = document.getElementById('hn-network-map-data');

    if (!root || !dataEl) {
        return;
    }

    if (!window.google || !google.maps) {
        window.hnShowNearbyMapError('Google Maps did not finish loading.');
        return;
    }

    var config;
    try {
        config = JSON.parse(dataEl.textContent || '{}');
    } catch (e) {
        window.hnShowNearbyMapError('Map configuration could not be read.');
        return;
    }

    function parseCoord(value, fallback) {
        var num = typeof value === 'number' ? value : parseFloat(value);
        return Number.isFinite(num) ? num : fallback;
    }

    var homeLat = parseCoord(config.home && config.home.lat, parseCoord(config.center && config.center.lat, 35.2271));
    var homeLng = parseCoord(config.home && config.home.lng, parseCoord(config.center && config.center.lng, -80.8431));

    if (!Number.isFinite(homeLat) || !Number.isFinite(homeLng)) {
        window.hnShowNearbyMapError('Your home location could not be determined.');
        return;
    }

    var radiusMiles = config.radiusMiles || 3;
    var radiusMeters = radiusMiles * 1609.34;
    var carouselItems = config.carouselItems || [];
    var recenterBtn = document.getElementById('hnMapRecenterBtn');
    var carouselEl = document.getElementById('hnMapCarousel');
    var dotsEl = document.getElementById('hnMapCarouselDots');

    var mapStyles = [
        { featureType: 'poi.business', stylers: [{ visibility: 'off' }] },
        { featureType: 'transit', stylers: [{ visibility: 'off' }] }
    ];

    var state = {
        home: { lat: homeLat, lng: homeLng },
        markers: [],
        markerById: {},
        activeItemId: null
    };

    function inheritOverlay(ctor) {
        ctor.prototype = Object.create(google.maps.OverlayView.prototype);
        ctor.prototype.constructor = ctor;
    }

    function IconMarkerOverlay(position, options) {
        this.position = position;
        this.options = options || {};
    }

    inheritOverlay(IconMarkerOverlay);
    IconMarkerOverlay.prototype.onAdd = function () {
        var wrap = document.createElement('button');
        wrap.type = 'button';
        wrap.className = 'hn-map-marker hn-map-marker--' + (this.options.kind || 'provider');
        if (this.options.isActive) {
            wrap.classList.add('is-active');
        }

        var icon = document.createElement('span');
        icon.className = 'hn-map-marker-icon';
        icon.innerHTML = '<i class="fas ' + (this.options.iconClass || 'fa-screwdriver-wrench') + '"></i>';
        wrap.appendChild(icon);

        if (this.options.label) {
            var label = document.createElement('span');
            label.className = 'hn-map-marker-label';
            label.textContent = this.options.label;
            wrap.appendChild(label);
        }

        wrap.addEventListener('click', this.options.onClick || function () {});
        this.div = wrap;
        this.getPanes().overlayMouseTarget.appendChild(wrap);
    };
    IconMarkerOverlay.prototype.draw = function () {
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
    IconMarkerOverlay.prototype.onRemove = function () {
        if (this.div && this.div.parentNode) {
            this.div.parentNode.removeChild(this.div);
        }
        this.div = null;
    };
    IconMarkerOverlay.prototype.setActive = function (active) {
        if (this.div) {
            this.div.classList.toggle('is-active', !!active);
        }
    };

    var map;
    try {
        map = new google.maps.Map(root, {
            center: state.home,
            zoom: 13,
            mapTypeId: google.maps.MapTypeId.ROADMAP,
            styles: mapStyles,
            disableDefaultUI: true,
            zoomControl: false,
            gestureHandling: 'greedy',
            clickableIcons: false
        });
    } catch (err) {
        window.hnShowNearbyMapError('Google Maps failed to initialize.');
        return;
    }

    function refreshMapLayout() {
        if (!map) {
            return;
        }
        google.maps.event.trigger(map, 'resize');
    }

    window.setTimeout(refreshMapLayout, 0);
    window.setTimeout(refreshMapLayout, 250);
    window.addEventListener('resize', refreshMapLayout);

    new google.maps.Circle({
        map: map,
        center: state.home,
        radius: radiusMeters,
        fillColor: '#0066CC',
        fillOpacity: 0.1,
        strokeColor: '#0066CC',
        strokeOpacity: 0.45,
        strokeWeight: 2,
        clickable: false
    });

    function clearMarkers() {
        state.markers.forEach(function (marker) {
            marker.setMap(null);
        });
        state.markers = [];
        state.markerById = {};
    }

    function markerKindForType(itemType) {
        switch (itemType) {
            case 'home':
                return 'home';
            case 'emergency':
                return 'emergency';
            case 'promotion':
                return 'promotion';
            default:
                return 'provider';
        }
    }

    function markerIconForType(itemType) {
        switch (itemType) {
            case 'home':
                return 'fa-house';
            case 'emergency':
                return 'fa-triangle-exclamation';
            case 'promotion':
                return 'fa-tags';
            default:
                return 'fa-screwdriver-wrench';
        }
    }

    function setActiveItem(itemId) {
        state.activeItemId = itemId;
        Object.keys(state.markerById).forEach(function (id) {
            state.markerById[id].setActive(id === itemId);
        });

        if (!carouselEl) {
            return;
        }

        var card = carouselEl.querySelector('[data-item-id="' + itemId + '"]');
        if (card) {
            card.scrollIntoView({ behavior: 'smooth', inline: 'center', block: 'nearest' });
        }

        updateDots(itemId);
        carouselEl.querySelectorAll('.hn-map-card').forEach(function (el) {
            el.classList.toggle('is-active', el.getAttribute('data-item-id') === itemId);
        });
    }

    function updateDots(itemId) {
        if (!dotsEl) {
            return;
        }
        dotsEl.querySelectorAll('.hn-map-dot').forEach(function (dot) {
            dot.classList.toggle('is-active', dot.getAttribute('data-item-id') === itemId);
        });
    }

    function renderHomeMarker() {
        var overlay = new IconMarkerOverlay(new google.maps.LatLng(state.home.lat, state.home.lng), {
            kind: 'you',
            iconClass: 'fa-house',
            label: 'You',
            onClick: function () {
                map.panTo(state.home);
            }
        });
        overlay.setMap(map);
        state.markers.push(overlay);
    }

    function renderItemMarkers() {
        carouselItems.forEach(function (item) {
            var lat = parseCoord(item.latitude, null);
            var lng = parseCoord(item.longitude, null);
            if (!Number.isFinite(lat) || !Number.isFinite(lng)) {
                return;
            }

            var overlay = new IconMarkerOverlay(new google.maps.LatLng(lat, lng), {
                kind: markerKindForType(item.itemType),
                iconClass: item.iconClass || markerIconForType(item.itemType),
                onClick: function () {
                    setActiveItem(item.id);
                    map.panTo({ lat: lat, lng: lng });
                }
            });
            overlay.setMap(map);
            state.markers.push(overlay);
            state.markerById[item.id] = overlay;
        });
    }

    function fitMapToResults() {
        var bounds = new google.maps.LatLngBounds();
        bounds.extend(new google.maps.LatLng(state.home.lat, state.home.lng));

        carouselItems.forEach(function (item) {
            var lat = parseCoord(item.latitude, null);
            var lng = parseCoord(item.longitude, null);
            if (Number.isFinite(lat) && Number.isFinite(lng)) {
                bounds.extend(new google.maps.LatLng(lat, lng));
            }
        });

        if (carouselItems.length === 0) {
            map.setCenter(state.home);
            map.setZoom(13);
            return;
        }

        map.fitBounds(bounds, { top: 56, right: 48, bottom: 240, left: 48 });
    }

    function escapeHtml(text) {
        if (!text) {
            return '';
        }
        return String(text)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;');
    }

    function renderCarousel() {
        if (!carouselEl) {
            return;
        }

        carouselEl.innerHTML = '';

        if (!carouselItems.length) {
            carouselEl.innerHTML =
                '<article class="hn-map-card hn-map-card--empty">' +
                '<div class="hn-map-card-body">' +
                '<span class="hn-map-card-kicker">YOUR HOME</span>' +
                '<strong class="hn-map-card-title">' + escapeHtml(config.homeLabel || 'Your home') + '</strong>' +
                '<p class="hn-map-card-sub">No nearby providers or listings in this filter yet.</p>' +
                '</div></article>';
            if (dotsEl) {
                dotsEl.innerHTML = '';
            }
            return;
        }

        carouselItems.forEach(function (item, index) {
            var card = document.createElement('article');
            card.className = 'hn-map-card' + (index === 0 ? ' is-active' : '');
            card.setAttribute('data-item-id', item.id);

            var mediaHtml = item.imageUrl
                ? '<div class="hn-map-card-media"><img src="' + escapeHtml(item.imageUrl) + '" alt="" /></div>'
                : '<div class="hn-map-card-media hn-map-card-media--icon">' +
                  '<span class="hn-map-card-icon"><i class="fas ' + escapeHtml(item.iconClass || markerIconForType(item.itemType)) + '"></i></span>' +
                  (item.isVerified ? '<span class="hn-map-card-verified"><i class="fas fa-check"></i></span>' : '') +
                  '</div>';

            var tagsHtml = (item.tags || []).map(function (tag) {
                return '<span class="hn-map-card-tag"><i class="fas fa-check-circle"></i> ' + escapeHtml(tag) + '</span>';
            }).join('');

            var distanceHtml = item.distanceMiles != null
                ? '<span class="hn-map-card-distance"><i class="fas fa-location-dot"></i> ' + Number(item.distanceMiles).toFixed(1) + ' miles away</span>'
                : '';

            var actionsHtml = '';
            if (item.primaryActionLabel) {
                actionsHtml += '<a href="' + escapeHtml(item.primaryActionUrl || '#') + '" class="hn-map-card-btn hn-map-card-btn--outline">' +
                    (item.itemType === 'provider' ? '<i class="far fa-comment-dots"></i> ' : '') +
                    escapeHtml(item.primaryActionLabel) + '</a>';
            }
            if (item.secondaryActionLabel) {
                actionsHtml += '<a href="' + escapeHtml(item.secondaryActionUrl || '#') + '" class="hn-map-card-btn hn-map-card-btn--primary">' +
                    escapeHtml(item.secondaryActionLabel) + '</a>';
            }

            card.innerHTML =
                mediaHtml +
                '<div class="hn-map-card-body">' +
                '<button type="button" class="hn-map-card-close" aria-label="Close">&times;</button>' +
                '<span class="hn-map-card-kicker">' + escapeHtml(item.badgeLabel || '') + '</span>' +
                (item.metaLabel ? '<span class="hn-map-card-time">' + escapeHtml(item.metaLabel) + '</span>' : '') +
                '<strong class="hn-map-card-title">' + escapeHtml(item.title || '') + '</strong>' +
                (tagsHtml ? '<div class="hn-map-card-tags">' + tagsHtml + '</div>' : '') +
                distanceHtml +
                (item.subtitle ? '<p class="hn-map-card-sub">' + escapeHtml(item.subtitle) + '</p>' : '') +
                (actionsHtml ? '<div class="hn-map-card-actions">' + actionsHtml + '</div>' : '') +
                '</div>';

            card.addEventListener('click', function (event) {
                if (event.target.closest('a') || event.target.closest('.hn-map-card-close')) {
                    if (event.target.closest('.hn-map-card-close')) {
                        event.preventDefault();
                        event.stopPropagation();
                        var panel = document.getElementById('hnMapCarouselPanel');
                        if (panel) {
                            panel.classList.add('is-collapsed');
                        }
                    }
                    return;
                }
                setActiveItem(item.id);
                map.panTo({ lat: parseCoord(item.latitude, homeLat), lng: parseCoord(item.longitude, homeLng) });
            });

            carouselEl.appendChild(card);
        });

        if (dotsEl) {
            dotsEl.innerHTML = carouselItems.map(function (item, index) {
                return '<button type="button" class="hn-map-dot' + (index === 0 ? ' is-active' : '') + '" data-item-id="' +
                    escapeHtml(item.id) + '" aria-label="Show item"></button>';
            }).join('');

            dotsEl.querySelectorAll('.hn-map-dot').forEach(function (dot) {
                dot.addEventListener('click', function () {
                    setActiveItem(dot.getAttribute('data-item-id'));
                });
            });
        }

        state.activeItemId = carouselItems[0] ? carouselItems[0].id : null;
        if (state.activeItemId && state.markerById[state.activeItemId]) {
            state.markerById[state.activeItemId].setActive(true);
        }
    }

    clearMarkers();
    renderHomeMarker();
    renderItemMarkers();
    renderCarousel();
    fitMapToResults();

    if (recenterBtn) {
        recenterBtn.addEventListener('click', function () {
            var panel = document.getElementById('hnMapCarouselPanel');
            if (panel) {
                panel.classList.remove('is-collapsed');
            }
            map.panTo(state.home);
            map.setZoom(13);
        });
    }

    window.hnNearbyNetworkMapReady = true;
};

if (!window.gm_authFailure) {
    window.gm_authFailure = function () {
        window.hnShowNearbyMapError('Google Maps rejected this site or API key. Add this domain to your Maps key referrer restrictions.');
    };
}
