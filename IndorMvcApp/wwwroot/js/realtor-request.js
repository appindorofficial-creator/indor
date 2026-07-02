(function () {
    function bindSelectedCards(containerSelector) {
        document.querySelectorAll(containerSelector + ' input').forEach(function (input) {
            input.addEventListener('change', function () {
                var group = input.closest('.rr-need-grid, .rr-time-row, .rr-home-grid, .rr-chip-row, .rr-icon-row, .rr-contact-row, .rr-rent-grid');
                if (group) {
                    group.querySelectorAll('label').forEach(function (c) { c.classList.remove('selected'); });
                } else {
                    document.querySelectorAll(containerSelector).forEach(function (c) { c.classList.remove('selected'); });
                }
                var label = input.closest('label');
                if (label) label.classList.add('selected');
            });
        });
    }

    bindSelectedCards('.rr-need-card');
    bindSelectedCards('.rr-time-btn');
    bindSelectedCards('.rr-chip-btn');
    bindSelectedCards('.rr-home-card');
    bindSelectedCards('.rr-icon-card');
    bindSelectedCards('.rr-contact-btn');

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

})();
