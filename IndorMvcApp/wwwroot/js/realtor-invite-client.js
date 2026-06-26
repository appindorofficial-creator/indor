(function () {
    function initInviteClientForm() {
        var form = document.getElementById('inviteClientForm');
        var nextBtn = document.getElementById('inviteNextBtn');
        var phoneInput = document.getElementById('inviteClientPhone');
        var note = document.querySelector('textarea[name="quickNote"]');
        var count = document.getElementById('noteCount');

        if (phoneInput && window.IndorPhoneInput) {
            window.IndorPhoneInput.attach(phoneInput);
        }

        if (note && count) {
            note.addEventListener('input', function () {
                count.textContent = String(note.value.length);
            });
        }

        if (nextBtn && form) {
            nextBtn.addEventListener('click', function (e) {
                e.preventDefault();
                if (phoneInput) {
                    phoneInput.dispatchEvent(new Event('input'));
                    if (!phoneInput.checkValidity()) {
                        phoneInput.reportValidity();
                        return;
                    }
                }
                if (typeof form.requestSubmit === 'function') {
                    form.requestSubmit();
                } else {
                    form.submit();
                }
            });
        }

        document.querySelectorAll('.rl-role-pill input').forEach(function (radio) {
            radio.addEventListener('change', function () {
                document.querySelectorAll('.rl-role-pill').forEach(function (pill) {
                    pill.classList.remove('is-selected');
                });
                radio.closest('.rl-role-pill')?.classList.add('is-selected');
            });
        });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initInviteClientForm);
    } else {
        initInviteClientForm();
    }
})();
