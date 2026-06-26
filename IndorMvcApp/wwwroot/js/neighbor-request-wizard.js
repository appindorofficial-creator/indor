(function () {
    function isPlainLeftClick(e) {
        return !e.defaultPrevented && e.button === 0 && !e.metaKey && !e.ctrlKey && !e.shiftKey && !e.altKey;
    }

    function markBusy(el) {
        el.classList.add('is-busy');
        el.setAttribute('aria-busy', 'true');
    }

    function clearBusy() {
        document.querySelectorAll('.nr-wizard-nav-btn.is-busy').forEach(function (el) {
            el.classList.remove('is-busy');
            el.removeAttribute('aria-busy');
        });
    }

    document.querySelectorAll('a.nr-wizard-nav-btn[data-nr-history-back]').forEach(function (link) {
        link.addEventListener('click', function (e) {
            if (!isPlainLeftClick(e)) {
                return;
            }

            if (window.history.length > 1) {
                e.preventDefault();
                markBusy(link);
                window.history.back();
                return;
            }

            markBusy(link);
        });
    });

    window.addEventListener('pageshow', clearBusy);
})();
