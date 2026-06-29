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

    form.querySelectorAll('.prv-wiz-card--selectable').forEach(card => {
        card.addEventListener('click', (e) => {
            if (e.target.closest('a')) return;

            const radio = card.querySelector('input[type="radio"]');
            if (!radio) return;

            radio.checked = true;
            syncSelectableCards(radio.name);
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

    var key = 'indor.prv.companyInfo.draft';

    function field(name) {
        return form.querySelector('[name="' + name + '"]');
    }

    function collectDraft() {
        var terms = field('termsAccepted');
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
        var terms = field('termsAccepted');
        if (terms) terms.checked = !!draft.termsAccepted;
    }

    function saveDraft() {
        try {
            sessionStorage.setItem(key, JSON.stringify(collectDraft()));
        } catch (_) { }
    }

    function clearDraft() {
        try {
            sessionStorage.removeItem(key);
        } catch (_) { }
    }

    function saveDraftToServer() {
        var url = form.getAttribute('data-save-draft-url');
        if (!url) return Promise.resolve();

        var token = form.querySelector('input[name="__RequestVerificationToken"]')?.value;
        if (!token) return Promise.resolve();

        var body = new URLSearchParams(new FormData(form));
        return fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'RequestVerificationToken': token
            },
            body: body.toString(),
            credentials: 'same-origin',
            keepalive: true
        }).catch(function () { });
    }

    function saveDraftAndSync() {
        saveDraft();
        return saveDraftToServer();
    }

    try {
        var raw = sessionStorage.getItem(key);
        if (raw) applyDraft(JSON.parse(raw));
    } catch (_) { }

    form.addEventListener('input', saveDraft);
    form.addEventListener('change', saveDraft);
    form.addEventListener('submit', clearDraft);
    form.querySelectorAll('a[href*="Terms"], a[href*="Privacy"]').forEach(function (link) {
        link.addEventListener('click', function (e) {
            e.stopPropagation();
            saveDraftAndSync();
        });
    });

    window.addEventListener('pagehide', function () {
        saveDraftAndSync();
    });
})();
