window.rlListingWizardInit = function (config) {
    config = config || {};
    var uiLabels = window.rlListingWizardLabels || {};

    var photoGrid = document.getElementById('rlListingPhotoGrid');
    var photoAdd = document.getElementById('rlListingPhotoAdd');
    var photoCountEl = document.getElementById('rlPhotoCount');
    var imageUrlInput = document.getElementById('ImageUrl');
    var extraPhotosInput = document.getElementById('AdditionalPhotoUrls');
    var galleryLinkInput = document.getElementById('PhotoGalleryLink');
    var pdfUrlInput = document.getElementById('PhotoPdfUrl');
    var pdfFileNameInput = document.getElementById('PhotoPdfFileName');
    var pdfFileInput = document.getElementById('photoPdfFile');
    var pdfDropZone = document.getElementById('rlPdfDropZone');
    var pdfCurrent = document.getElementById('rlPdfCurrent');
    var pdfCurrentLink = document.getElementById('rlPdfCurrentLink');
    var pdfRemoveBtn = document.getElementById('rlPdfRemove');
    var titleInput = document.getElementById('Title');
    var priceInput = document.querySelector('[name="Price"]');
    var addressInput = document.getElementById('ListingAddress');
    var latInput = document.getElementById('AddressLatitude');
    var lngInput = document.getElementById('AddressLongitude');
    var locateBtn = document.getElementById('rlUseMyLocation');
    var locationHint = document.getElementById('rlLocationHint');
    var openHouseToggle = document.getElementById('IsOpenHouse');
    var openHouseWrap = document.getElementById('OpenHouseMetaWrap');
    var descriptionInput = document.getElementById('ListingDescription');
    var descriptionCountEl = document.getElementById('rlDescriptionCount');
    var form = document.getElementById('rlListingDetailsForm');

    var photos = [];
    var defaultPlaceholder = '/inspeccion2.jpeg';

    if (config.initialPhoto && config.initialPhoto !== defaultPlaceholder) {
        photos.push(config.initialPhoto);
    }
    if (config.extraPhotos) {
        config.extraPhotos.split(',').map(function (p) { return p.trim(); }).filter(Boolean).forEach(function (url) {
            if (photos.indexOf(url) === -1) {
                photos.push(url);
            }
        });
    }

    function updatePhotoCount() {
        if (!photoCountEl) {
            return;
        }
        photoCountEl.textContent = photos.length + '/20';
    }

    function syncPhotoFields() {
        if (imageUrlInput) {
            imageUrlInput.value = photos[0] || '';
        }
        if (extraPhotosInput) {
            extraPhotosInput.value = photos.slice(1).join(',');
        }
        updatePhotoCount();
    }

    function escapeAttr(value) {
        return String(value || '')
            .replace(/&/g, '&amp;')
            .replace(/"/g, '&quot;')
            .replace(/</g, '&lt;');
    }

    function renderPhotos() {
        if (!photoGrid || !photoAdd) {
            return;
        }

        photoGrid.querySelectorAll('.rl-listing-photo-thumb, .rl-listing-photo-placeholder').forEach(function (el) {
            el.remove();
        });

        photos.slice(0, 20).forEach(function (url, index) {
            var thumb = document.createElement('div');
            thumb.className = 'rl-listing-photo-thumb';
            thumb.innerHTML =
                '<img src="' + escapeAttr(url) + '" alt="" />' +
                '<button type="button" class="rl-listing-photo-remove" data-index="' + index + '" aria-label="' + escapeAttr(uiLabels.removePhoto || 'Remove photo') + '">&times;</button>';
            photoGrid.appendChild(thumb);
        });

        var placeholders = Math.max(0, 3 - photos.length);
        for (var i = 0; i < placeholders; i++) {
            var placeholder = document.createElement('div');
            placeholder.className = 'rl-listing-photo-placeholder';
            placeholder.innerHTML = '<i class="fas fa-image"></i>';
            photoGrid.appendChild(placeholder);
        }

        photoAdd.style.display = photos.length >= 20 ? 'none' : '';
        syncPhotoFields();
    }

    function addPhotoUrl() {
        if (photos.length >= 20) {
            window.alert('You can add up to 20 individual photos.');
            return;
        }

        var url = window.prompt('Paste the URL of a property photo', 'https://');
        if (!url || !url.trim()) {
            return;
        }

        photos.push(url.trim());
        renderPhotos();
    }

    function showPdfCurrent(name, url) {
        if (!pdfCurrent) {
            return;
        }

        pdfCurrent.classList.remove('is-hidden');
        if (pdfCurrentLink) {
            pdfCurrentLink.textContent = name || (uiLabels.propertyPhotosPdf || 'Property photos.pdf');
            pdfCurrentLink.href = url || '#';
        }
    }

    function hidePdfCurrent() {
        if (pdfCurrent) {
            pdfCurrent.classList.add('is-hidden');
        }
        if (pdfUrlInput) {
            pdfUrlInput.value = '';
        }
        if (pdfFileNameInput) {
            pdfFileNameInput.value = '';
        }
        if (pdfFileInput) {
            pdfFileInput.value = '';
        }
    }

    if (photoAdd) {
        photoAdd.addEventListener('click', addPhotoUrl);
    }

    if (photoGrid) {
        photoGrid.addEventListener('click', function (event) {
            var btn = event.target.closest('.rl-listing-photo-remove');
            if (!btn) {
                return;
            }

            var index = parseInt(btn.getAttribute('data-index'), 10);
            if (!isNaN(index)) {
                photos.splice(index, 1);
                renderPhotos();
            }
        });
    }

    if (pdfFileInput) {
        pdfFileInput.addEventListener('change', function () {
            var file = pdfFileInput.files && pdfFileInput.files[0];
            if (!file) {
                return;
            }

            if (!/\.pdf$/i.test(file.name)) {
                window.alert('Please choose a PDF file.');
                pdfFileInput.value = '';
                return;
            }

            if (file.size > 15 * 1024 * 1024) {
                window.alert('PDF must be 15 MB or smaller.');
                pdfFileInput.value = '';
                return;
            }

            if (pdfUrlInput) {
                pdfUrlInput.value = '';
            }
            showPdfCurrent(file.name, '#');
        });
    }

    if (pdfRemoveBtn) {
        pdfRemoveBtn.addEventListener('click', hidePdfCurrent);
    }

    if (pdfDropZone && pdfFileInput) {
        ['dragenter', 'dragover'].forEach(function (eventName) {
            pdfDropZone.addEventListener(eventName, function (event) {
                event.preventDefault();
                pdfDropZone.classList.add('is-dragover');
            });
        });
        ['dragleave', 'drop'].forEach(function (eventName) {
            pdfDropZone.addEventListener(eventName, function (event) {
                event.preventDefault();
                pdfDropZone.classList.remove('is-dragover');
            });
        });
        pdfDropZone.addEventListener('drop', function (event) {
            var file = event.dataTransfer && event.dataTransfer.files && event.dataTransfer.files[0];
            if (!file) {
                return;
            }
            pdfFileInput.files = event.dataTransfer.files;
            pdfFileInput.dispatchEvent(new Event('change'));
        });
    }

    if (config.pdfUrl) {
        showPdfCurrent(config.pdfFileName || 'Property photos.pdf', config.pdfUrl);
    }

    if (galleryLinkInput && config.galleryLink) {
        galleryLinkInput.value = config.galleryLink;
    }

    document.querySelectorAll('.rl-listing-toggle-btn input').forEach(function (radio) {
        radio.addEventListener('change', function () {
            document.querySelectorAll('.rl-listing-toggle-btn').forEach(function (btn) {
                btn.classList.toggle('is-active', btn.contains(radio) && radio.checked);
            });
        });
    });

    document.querySelectorAll('.rl-listing-program-select-card input').forEach(function (checkbox) {
        checkbox.addEventListener('change', function () {
            var card = checkbox.closest('.rl-listing-program-select-card');
            if (card) {
                card.classList.toggle('is-selected', checkbox.checked);
            }
        });
    });

    function updateDescriptionCount() {
        if (!descriptionInput || !descriptionCountEl) {
            return;
        }
        var length = (descriptionInput.value || '').length;
        descriptionCountEl.textContent = length + '/500';
    }

    if (descriptionInput) {
        descriptionInput.addEventListener('input', updateDescriptionCount);
        updateDescriptionCount();
    }

    if (openHouseToggle && openHouseWrap) {
        openHouseToggle.addEventListener('change', function () {
            openHouseWrap.style.display = openHouseToggle.checked ? '' : 'none';
        });
    }

    if (locateBtn && navigator.geolocation) {
        locateBtn.addEventListener('click', function () {
            locateBtn.disabled = true;
            locateBtn.classList.add('is-loading');

            navigator.geolocation.getCurrentPosition(
                function (position) {
                    if (latInput) {
                        latInput.value = position.coords.latitude;
                    }
                    if (lngInput) {
                        lngInput.value = position.coords.longitude;
                    }
                    if (locationHint) {
                        locationHint.hidden = false;
                    }
                    if (addressInput && !addressInput.value.trim()) {
                        addressInput.value = 'Near me (' + position.coords.latitude.toFixed(5) + ', ' + position.coords.longitude.toFixed(5) + ')';
                    }
                    locateBtn.disabled = false;
                    locateBtn.classList.remove('is-loading');
                },
                function () {
                    window.alert('Could not access your location. Check browser permissions and try again.');
                    locateBtn.disabled = false;
                    locateBtn.classList.remove('is-loading');
                },
                { enableHighAccuracy: true, timeout: 12000, maximumAge: 60000 }
            );
        });
    } else if (locateBtn) {
        locateBtn.disabled = true;
    }

    if (form) {
        form.addEventListener('submit', function () {
            if (titleInput && !titleInput.value.trim() && priceInput && priceInput.value) {
                titleInput.value = '$' + Number(priceInput.value).toLocaleString('en-US');
            }
            syncPhotoFields();
        });
    }

    // ---------- Multi-step wizard navigation ----------
    var wizard = document.getElementById('rlListingWizard');
    if (wizard) {
        var stepSections = Array.prototype.slice.call(wizard.querySelectorAll('section.rl-lw-step[data-step]'));
        var stepperItems = Array.prototype.slice.call(wizard.querySelectorAll('.rl-lw-stepper-item[data-step]'));
        var titleEl = document.getElementById('rlLwTitle');
        var subEl = document.getElementById('rlLwSub');
        var backBtn = document.getElementById('rlLwBack');
        var feedUrl = wizard.getAttribute('data-feed-url') || '/Realtor/Network';
        var minStep = parseInt(wizard.getAttribute('data-start-step'), 10);
        if (isNaN(minStep) || minStep < 1) {
            minStep = 1;
        }
        var totalSteps = stepSections.length || 4;
        var currentStep = minStep;

        var stepMeta = window.rlListingWizardSteps || {
            1: { title: 'Post Listing with INDOR', sub: '' },
            2: { title: 'Create Your Listing', sub: 'Add key details to showcase your property.' },
            3: { title: 'Property Details', sub: '' },
            4: { title: 'Review & Publish', sub: '' }
        };

        function formatMoney(value) {
            var n = Number(value);
            if (!value || isNaN(n)) {
                return '$0';
            }
            return '$' + n.toLocaleString('en-US');
        }

        function setText(id, value) {
            var el = document.getElementById(id);
            if (el) {
                el.textContent = value;
            }
        }

        function populateReview() {
            var photoEl = document.getElementById('rlRvPhoto');
            if (photoEl) {
                photoEl.src = photos[0] || defaultPlaceholder;
            }
            var typeRadio = document.querySelector('[name="ListingType"]:checked');
            setText('rlRvType', typeRadio && typeRadio.value === 'rent' ? (uiLabels.forRent || 'For Rent') : (uiLabels.forSale || 'For Sale'));
            setText('rlRvPrice', formatMoney(priceInput && priceInput.value));
            setText('rlRvAddress', (addressInput && addressInput.value.trim()) || (uiLabels.addressNotSet || 'Address not set'));

            function statValue(name) {
                var el = document.querySelector('[name="' + name + '"]');
                return el && el.value !== '' ? el.value : '—';
            }
            var sqft = document.querySelector('[name="SquareFeet"]');
            setText('rlRvBeds', statValue('Bedrooms'));
            setText('rlRvBaths', statValue('Bathrooms'));
            setText('rlRvSqft', sqft && sqft.value !== '' ? Number(sqft.value).toLocaleString('en-US') : '—');
            setText('rlRvYear', statValue('YearBuilt'));
        }

        function clearStepErrors() {
            ['rlAddressError', 'rlPriceError', 'rlSubtypeError'].forEach(function (id) {
                var el = document.getElementById(id);
                if (el) {
                    el.hidden = true;
                }
            });
        }

        function validateStep(step) {
            if (step === 2) {
                var ok = true;
                clearStepErrors();
                if (addressInput && !addressInput.value.trim()) {
                    var ae = document.getElementById('rlAddressError');
                    if (ae) { ae.hidden = false; }
                    ok = false;
                }
                if (priceInput && !priceInput.value) {
                    var pe = document.getElementById('rlPriceError');
                    if (pe) { pe.hidden = false; }
                    ok = false;
                }
                return ok;
            }

            if (step === 3) {
                var subtypeInput = document.getElementById('PropertySubtype');
                var subtypeOk = true;
                clearStepErrors();
                if (subtypeInput && !subtypeInput.value.trim()) {
                    var se = document.getElementById('rlSubtypeError');
                    if (se) { se.hidden = false; }
                    subtypeOk = false;
                }
                return subtypeOk;
            }

            return true;
        }

        function setStep(step, skipValidation) {
            step = parseInt(step, 10);
            if (isNaN(step)) {
                step = minStep;
            }
            step = Math.max(minStep, Math.min(totalSteps, step));
            if (step > currentStep && !skipValidation) {
                for (var s = currentStep; s < step; s++) {
                    if (!validateStep(s)) {
                        return;
                    }
                }
            }
            currentStep = step;

            var activated = false;
            stepSections.forEach(function (section) {
                var sStep = parseInt(section.getAttribute('data-step'), 10);
                var on = sStep === step;
                section.classList.toggle('is-active', on);
                if (on) {
                    activated = true;
                }
            });
            // Never leave every panel display:none (blank white body under the stepper).
            if (!activated && stepSections.length) {
                stepSections[0].classList.add('is-active');
                currentStep = parseInt(stepSections[0].getAttribute('data-step'), 10) || minStep;
                step = currentStep;
            }
            stepperItems.forEach(function (item) {
                var iStep = parseInt(item.getAttribute('data-step'), 10);
                item.classList.toggle('is-done', iStep < step);
                item.classList.toggle('is-active', iStep === step);
            });

            var meta = stepMeta[step] || stepMeta[String(step)] || { title: '', sub: '' };
            if (titleEl) { titleEl.textContent = meta.title; }
            if (subEl) {
                subEl.textContent = meta.sub;
                subEl.style.display = meta.sub ? '' : 'none';
            }

            if (step === 4) {
                populateReview();
            }
            window.scrollTo({ top: 0, behavior: 'smooth' });
        }

        wizard.querySelectorAll('.rl-lw-next').forEach(function (btn) {
            btn.addEventListener('click', function () {
                setStep(currentStep + 1);
            });
        });

        wizard.querySelectorAll('.rl-lw-goto').forEach(function (btn) {
            btn.addEventListener('click', function () {
                var target = parseInt(btn.getAttribute('data-goto'), 10);
                if (!isNaN(target)) {
                    setStep(target, true);
                }
            });
        });

        if (backBtn) {
            backBtn.addEventListener('click', function () {
                if (currentStep > minStep) {
                    setStep(currentStep - 1, true);
                } else {
                    window.location.href = feedUrl;
                }
            });
        }

        // Key highlights chips
        var highlightsInput = document.getElementById('Highlights');
        var highlightBtns = Array.prototype.slice.call(wizard.querySelectorAll('.rl-lw-highlight'));
        function syncHighlights() {
            if (!highlightsInput) {
                return;
            }
            highlightsInput.value = highlightBtns
                .filter(function (b) { return b.classList.contains('is-selected'); })
                .map(function (b) { return b.getAttribute('data-value'); })
                .join(',');
        }
        highlightBtns.forEach(function (btn) {
            btn.addEventListener('click', function () {
                var on = btn.classList.toggle('is-selected');
                btn.setAttribute('aria-pressed', on ? 'true' : 'false');
                syncHighlights();
            });
        });
        syncHighlights();

        setStep(minStep, true);
    }

    renderPhotos();
};
