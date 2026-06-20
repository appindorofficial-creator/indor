(function () {
    const chipsRoot = document.getElementById('rlLanguageChips');
    const languagesInput = document.getElementById('rlLanguagesInput');
    const addBtn = document.getElementById('rlLanguageAddBtn');

    function parseLanguages(value) {
        if (!value) return [];
        return value.split(',').map(v => v.trim()).filter(Boolean);
    }

    function syncLanguagesInput(languages) {
        if (languagesInput) {
            languagesInput.value = languages.join(', ');
        }
    }

    function renderLanguageChips(languages) {
        if (!chipsRoot) return;
        chipsRoot.innerHTML = '';
        languages.forEach(lang => {
            const chip = document.createElement('button');
            chip.type = 'button';
            chip.className = 'rl-ep-language-chip';
            chip.innerHTML = `${lang} <i class="fas fa-xmark" aria-hidden="true"></i>`;
            chip.addEventListener('click', () => {
                const next = languages.filter(l => l.toLowerCase() !== lang.toLowerCase());
                renderLanguageChips(next);
                syncLanguagesInput(next);
            });
            chipsRoot.appendChild(chip);
        });
    }

    if (chipsRoot && languagesInput) {
        const initial = chipsRoot.dataset.initial || languagesInput.value || '';
        const languages = parseLanguages(initial);
        renderLanguageChips(languages);
        syncLanguagesInput(languages);
    }

    if (addBtn) {
        addBtn.addEventListener('click', () => {
            const value = window.prompt('Add a language');
            if (!value) return;
            const trimmed = value.trim();
            if (!trimmed) return;
            const current = parseLanguages(languagesInput?.value || '');
            if (current.some(l => l.toLowerCase() === trimmed.toLowerCase())) return;
            current.push(trimmed);
            renderLanguageChips(current);
            syncLanguagesInput(current);
        });
    }

    document.querySelectorAll('.rl-ep-specialty-input').forEach(input => {
        input.addEventListener('change', () => {
            const checked = document.querySelectorAll('.rl-ep-specialty-input:checked');
            if (checked.length > 3) {
                input.checked = false;
                return;
            }
            document.querySelectorAll('.rl-ep-specialty-chip').forEach(chip => {
                const box = chip.querySelector('.rl-ep-specialty-input');
                chip.classList.toggle('is-selected', box?.checked === true);
            });
        });
    });
})();
