(function () {
    function bindSelectedCards(selector) {
        document.querySelectorAll(selector + ' input').forEach(function (input) {
            input.addEventListener('change', function () {
                var group = input.closest(selector.replace(' input', '')) || input.closest('.rr-need-grid, .rr-time-row, .rr-home-grid, .rr-chip-row, .rr-icon-row, .rr-contact-row, .rr-rent-grid');
                if (!group) {
                    document.querySelectorAll(selector.split(' ')[0]).forEach(function (c) { c.classList.remove('selected'); });
                } else {
                    group.querySelectorAll('label').forEach(function (c) { c.classList.remove('selected'); });
                }
                var label = input.closest('label');
                if (label) label.classList.add('selected');
            });
        });
    }

    bindSelectedCards('.rr-need-card input');
    bindSelectedCards('.rr-time-btn input');
    bindSelectedCards('.rr-chip-btn input');
    bindSelectedCards('.rr-home-card input');
    bindSelectedCards('.rr-icon-card input');
    bindSelectedCards('.rr-contact-btn input');

    document.querySelectorAll('.rr-rent-grid .rr-chip-btn input').forEach(function (radio) {
        radio.addEventListener('change', function () {
            document.querySelectorAll('.rr-rent-grid .rr-chip-btn').forEach(function (c) { c.classList.remove('selected'); });
            radio.closest('.rr-chip-btn').classList.add('selected');
        });
    });

    var priorityGrid = document.querySelector('.rr-priority-grid');
    if (priorityGrid) {
        var max = parseInt(priorityGrid.getAttribute('data-max') || '3', 10);
        priorityGrid.querySelectorAll('input[type="checkbox"]').forEach(function (cb) {
            cb.addEventListener('change', function () {
                var checked = priorityGrid.querySelectorAll('input[type="checkbox"]:checked');
                if (checked.length > max) {
                    cb.checked = false;
                    return;
                }
                priorityGrid.querySelectorAll('.rr-priority-chip').forEach(function (chip) {
                    chip.classList.toggle('selected', chip.querySelector('input').checked);
                });
            });
        });
    }

    var notes = document.getElementById('notesField') || document.getElementById('guidanceNotesField');
    var count = document.getElementById('notesCount') || document.getElementById('guidanceNotesCount');
    if (notes && count) {
        var update = function () { count.textContent = String(notes.value.length); };
        notes.addEventListener('input', update);
        update();
    }

    document.querySelectorAll('.rr-need-card input').forEach(function (radio) {
        radio.addEventListener('change', function () {
            document.querySelectorAll('.rr-need-card').forEach(function (c) { c.classList.remove('selected'); });
            radio.closest('.rr-need-card').classList.add('selected');
        });
    });
})();
