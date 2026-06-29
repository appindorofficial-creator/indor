(function () {
    const chipsRoot = document.getElementById('rlLanguageChips');
    const languagesInput = document.getElementById('rlLanguagesInput');

    function syncLanguagesInput() {
        if (!chipsRoot || !languagesInput) return;
        const selected = Array.from(chipsRoot.querySelectorAll('.rl-ep-language-chip.is-selected'))
            .map(chip => chip.dataset.language)
            .filter(Boolean);
        languagesInput.value = selected.join(', ');
    }

    if (chipsRoot && languagesInput) {
        chipsRoot.querySelectorAll('.rl-ep-language-chip').forEach(chip => {
            chip.addEventListener('click', () => {
                const isSelected = chip.classList.toggle('is-selected');
                chip.setAttribute('aria-pressed', isSelected ? 'true' : 'false');
                syncLanguagesInput();
            });
        });
        syncLanguagesInput();
    }

    function scrollToFirstValidationError() {
        const firstError = document.querySelector(
            '.validation-summary-errors li, .rl-field-error:not(:empty), span.field-validation-error:not(:empty)'
        );
        if (!firstError) {
            return;
        }

        const target = firstError.closest('.rl-ep-field-row')
            || firstError.closest('label.rl-ep-field')
            || firstError;
        target.scrollIntoView({ behavior: 'smooth', block: 'center' });
    }

    const contactForm = document.querySelector('.rl-ep-form');
    if (contactForm) {
        contactForm.addEventListener('invalid', scrollToFirstValidationError, true);
        contactForm.addEventListener('submit', function () {
            window.setTimeout(scrollToFirstValidationError, 50);
        });

        if (document.querySelector('.validation-summary-errors, .field-validation-error, .rl-field-error:not(:empty)')) {
            window.setTimeout(scrollToFirstValidationError, 100);
        }
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
