(function () {
    if (window.__indorRealtorPropertyFileOpen) {
        return;
    }
    window.__indorRealtorPropertyFileOpen = true;

    var lastOpen = { url: '', at: 0 };

    document.addEventListener('click', function (event) {
        var button = event.target && event.target.closest
            ? event.target.closest('.rl-file-open-btn')
            : null;
        if (!button || button.disabled) {
            return;
        }

        event.preventDefault();
        event.stopPropagation();
        event.stopImmediatePropagation();

        var url = button.getAttribute('data-file-url');
        if (!url) {
            return;
        }

        var now = Date.now();
        if (url === lastOpen.url && now - lastOpen.at < 1500) {
            return;
        }
        lastOpen = { url: url, at: now };

        button.disabled = true;
        var opened = window.open(url, '_blank', 'noopener,noreferrer');
        if (!opened) {
            window.location.assign(url);
        }

        window.setTimeout(function () {
            button.disabled = false;
        }, 1500);
    }, true);
})();
