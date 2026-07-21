function syncSelectableCards(groupName) {
    document.querySelectorAll(`.prv-wiz-card--selectable input[name="${CSS.escape(groupName)}"]`).forEach(radio => {
        const card = radio.closest('.prv-wiz-card--selectable');
        if (!card) return;
        card.classList.toggle('is-selected', radio.checked);
        card.setAttribute('aria-pressed', radio.checked ? 'true' : 'false');
    });
}

document.querySelectorAll('.prv-wiz-card--selectable').forEach(card => {
    const input = card.querySelector('input[type="radio"], input[type="checkbox"]');
    if (!input) return;

    input.addEventListener('change', () => syncSelectableCards(input.name));
    syncSelectableCards(input.name);
});

(function wireEntryPathCards() {
    const form = document.getElementById('prvEntryForm');
    if (!form) return;

    const pathInput = document.getElementById('prvEntryPath');
    const cards = form.querySelectorAll('.prv-entry-card[data-path], .prv-wiz-card--selectable[data-path]');

    function selectPath(path, activeCard) {
        if (pathInput) {
            pathInput.value = path;
        }

        cards.forEach(card => {
            const selected = card === activeCard;
            card.classList.toggle('is-selected', selected);
            card.setAttribute('aria-checked', selected ? 'true' : 'false');
        });
    }

    cards.forEach(card => {
        card.addEventListener('click', () => {
            selectPath(card.dataset.path, card);
        });

        card.addEventListener('keydown', (e) => {
            if (e.key !== 'Enter' && e.key !== ' ') return;
            e.preventDefault();
            selectPath(card.dataset.path, card);
        });
    });
})();

document.querySelectorAll('[data-chip-group]').forEach(group => {
    const single = group.dataset.singleSelect === 'true' || group.classList.contains('prv-chip-grid--single');

    group.querySelectorAll('.prv-chip input').forEach(input => {
        input.addEventListener('change', () => {
            if (single && input.type === 'radio') {
                group.querySelectorAll('.prv-chip').forEach(chip => {
                    chip.classList.remove('is-selected');
                    const check = chip.querySelector('.prv-chip-check');
                    if (check) check.remove();
                });
                const chip = input.closest('.prv-chip');
                chip?.classList.add('is-selected');
                if (chip && !chip.querySelector('.prv-chip-check')) {
                    const icon = document.createElement('i');
                    icon.className = 'fas fa-circle-check prv-chip-check';
                    chip.appendChild(icon);
                }
            } else {
                input.closest('.prv-chip')?.classList.toggle('is-selected', input.checked);
            }
        });
    });
});

function wireChipSearch(searchId, gridId) {
    const searchInput = document.getElementById(searchId);
    const grid = document.getElementById(gridId);
    if (!searchInput || !grid) return;
    searchInput.addEventListener('input', () => {
        const q = searchInput.value.trim().toLowerCase();
        grid.querySelectorAll('.prv-chip').forEach(chip => {
            const label = (chip.dataset.label || chip.textContent || '').toLowerCase();
            chip.style.display = !q || label.includes(q) ? '' : 'none';
        });
    });
}

wireChipSearch('categorySearch', 'categoryGrid');
wireChipSearch('serviceSearch', 'serviceGrid');

(function wireCompanyInfoDraft() {
    var form = document.getElementById('prvCompanyInfoForm');
    if (!form) return;

    // localStorage survives Android WebView process death; sessionStorage does not.
    var key = 'indor.prv.companyInfo.draft';
    var storage = window.localStorage || window.sessionStorage;

    function field(name) {
        return form.querySelector('[name="' + name + '"]');
    }

    function termsCheckbox() {
        return form.querySelector('input[type="checkbox"][name="termsAccepted"]');
    }

    function collectDraft() {
        var terms = termsCheckbox();
        return {
            businessName: field('businessName')?.value ?? '',
            primaryContact: field('primaryContact')?.value ?? '',
            phone: field('phone')?.value ?? '',
            email: field('email')?.value ?? '',
            primaryCategoryId: field('primaryCategoryId')?.value ?? '',
            serviceAreas: field('serviceAreas')?.value ?? '',
            website: field('website')?.value ?? '',
            einNumber: field('einNumber')?.value ?? '',
            termsAccepted: !!(terms && terms.checked)
        };
    }

    function draftHasContent(draft) {
        if (!draft) return false;
        return !!(
            draft.businessName ||
            draft.primaryContact ||
            draft.phone ||
            draft.email ||
            draft.primaryCategoryId ||
            draft.serviceAreas ||
            draft.website ||
            draft.einNumber
        );
    }

    function applyDraft(draft) {
        if (!draft) return;
        var map = {
            businessName: draft.businessName,
            primaryContact: draft.primaryContact,
            phone: draft.phone,
            email: draft.email,
            serviceAreas: draft.serviceAreas,
            website: draft.website,
            einNumber: draft.einNumber
        };
        Object.keys(map).forEach(function (name) {
            var el = field(name);
            if (el && map[name] != null) el.value = map[name];
        });
        var category = field('primaryCategoryId');
        if (category && draft.primaryCategoryId) category.value = draft.primaryCategoryId;
        var terms = termsCheckbox();
        if (terms) terms.checked = !!draft.termsAccepted;
    }

    function saveDraft() {
        try {
            storage.setItem(key, JSON.stringify(collectDraft()));
        } catch (_) { }
    }

    function clearDraft() {
        try {
            storage.removeItem(key);
            if (window.sessionStorage) sessionStorage.removeItem(key);
        } catch (_) { }
    }

    function buildDraftBody() {
        var body = new URLSearchParams(new FormData(form));
        return body.toString();
    }

    function saveDraftToServer() {
        var url = form.getAttribute('data-save-draft-url');
        if (!url) return Promise.resolve();

        var token = form.querySelector('input[name="__RequestVerificationToken"]')?.value;
        if (!token) return Promise.resolve();

        return fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'RequestVerificationToken': token
            },
            body: buildDraftBody(),
            credentials: 'same-origin',
            keepalive: true
        }).catch(function () { });
    }

    function sendDraftBeacon() {
        var url = form.getAttribute('data-save-draft-url');
        if (!url || typeof navigator.sendBeacon !== 'function') {
            return false;
        }

        try {
            return navigator.sendBeacon(url, new FormData(form));
        } catch (_) {
            return false;
        }
    }

    function saveDraftAndSync() {
        saveDraft();
        return saveDraftToServer();
    }

    function restoreDraftFromStorage() {
        try {
            var raw = storage.getItem(key);
            if (!raw && window.sessionStorage) {
                raw = sessionStorage.getItem(key);
                if (raw) {
                    // Migrate legacy session drafts so resume after backgrounding works.
                    try { storage.setItem(key, raw); } catch (_) { }
                }
            }
            if (!raw) return;
            var draft = JSON.parse(raw);
            if (draftHasContent(draft)) {
                applyDraft(draft);
            }
        } catch (_) { }
    }

    restoreDraftFromStorage();

    form.addEventListener('input', saveDraft);
    form.addEventListener('change', saveDraft);
    form.addEventListener('submit', clearDraft);

    form.querySelectorAll('a[href*="Terms"], a[href*="Privacy"]').forEach(function (link) {
        link.addEventListener('click', function (e) {
            e.preventDefault();
            e.stopPropagation();
            saveDraft();
            var href = link.href;
            saveDraftToServer().finally(function () {
                window.location.assign(href);
            });
        });
    });

    window.addEventListener('pagehide', function () {
        saveDraft();
        if (!sendDraftBeacon()) {
            saveDraftToServer();
        }
    });

    window.addEventListener('pageshow', function () {
        restoreDraftFromStorage();
    });
})();

(function wireDocFilePickers() {
    function wirePicker(picker) {
        var input = picker.querySelector('.prv-file-picker-input');
        var nameEl = picker.querySelector('.prv-file-picker-name');
        if (!input || !nameEl || input.dataset.prvPickerWired === 'true') {
            return;
        }

        input.dataset.prvPickerWired = 'true';
        input.addEventListener('change', function () {
            var file = input.files && input.files[0];
            nameEl.textContent = file ? file.name : 'No file selected';
            picker.classList.toggle('has-file', !!file);
        });
    }

    document.querySelectorAll('.prv-doc-upload').forEach(function (block) {
        var existingPicker = block.querySelector('.prv-file-picker');
        if (existingPicker) {
            wirePicker(existingPicker);
            return;
        }

        var input = block.querySelector('input[type="file"]');
        if (!input) {
            return;
        }

        var linkedLabel = input.id
            ? block.querySelector('label[for="' + CSS.escape(input.id) + '"]')
            : null;
        if (linkedLabel) {
            var fieldLabel = document.createElement('span');
            fieldLabel.className = 'prv-doc-label';
            fieldLabel.textContent = linkedLabel.textContent.trim();
            linkedLabel.replaceWith(fieldLabel);
        }

        input.classList.add('prv-file-picker-input');

        var picker = document.createElement('label');
        picker.className = 'prv-file-picker';

        var btn = document.createElement('span');
        btn.className = 'prv-file-picker-btn';
        btn.textContent = 'Choose file';

        var nameEl = document.createElement('span');
        nameEl.className = 'prv-file-picker-name';
        nameEl.textContent = 'No file selected';

        input.replaceWith(picker);
        picker.appendChild(input);
        picker.appendChild(btn);
        picker.appendChild(nameEl);
        wirePicker(picker);
    });
})();
