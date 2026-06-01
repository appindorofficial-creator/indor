(function () {
    'use strict';

    var meta = document.querySelector('meta[name="app-version"]');
    var version = meta && meta.getAttribute('content');
    if (!version) return;

    var storageKey = 'indor_app_version';
    var previous = sessionStorage.getItem(storageKey);

    if (previous && previous !== version) {
        sessionStorage.setItem(storageKey, version);
        window.location.reload();
        return;
    }

    sessionStorage.setItem(storageKey, version);

    window.addEventListener('pageshow', function (event) {
        if (event.persisted) {
            window.location.reload();
        }
    });
})();
