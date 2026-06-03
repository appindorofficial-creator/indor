document.querySelectorAll('[data-chip-group]').forEach(group => {
    group.querySelectorAll('.prv-chip input').forEach(input => {
        input.addEventListener('change', () => {
            input.closest('.prv-chip')?.classList.toggle('is-selected', input.checked);
        });
    });
});
