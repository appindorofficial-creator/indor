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
