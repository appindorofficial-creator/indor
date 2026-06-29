(function () {
    function bindPasswordToggle(inputId, toggleId) {
        var input = document.getElementById(inputId);
        var toggle = document.getElementById(toggleId);
        if (!input || !toggle) return;

        var icon = toggle.querySelector('i');
        if (!icon) return;

        var visible = input.type === 'text';

        function render() {
            input.type = visible ? 'text' : 'password';
            icon.className = visible ? 'fas fa-eye' : 'fas fa-eye-slash';
            toggle.setAttribute('aria-label', visible ? 'Hide password' : 'Show password');
            toggle.setAttribute('aria-pressed', visible ? 'true' : 'false');
            toggle.classList.toggle('is-visible', visible);
        }

        render();

        toggle.addEventListener('click', function (event) {
            event.preventDefault();
            visible = !visible;
            render();
        });
    }

    window.indorBindPasswordToggle = bindPasswordToggle;
})();
