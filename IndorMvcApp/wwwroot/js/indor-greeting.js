(function () {
    function greetingForHour(hour) {
        if (hour >= 5 && hour < 12) return 'Good morning';
        if (hour >= 12 && hour < 17) return 'Good afternoon';
        return 'Good evening';
    }

    function applyGreetings() {
        var hour = new Date().getHours();
        var text = greetingForHour(hour);
        document.querySelectorAll('[data-indor-greeting]').forEach(function (el) {
            var suffix = el.getAttribute('data-indor-greeting-suffix');
            if (suffix === null) suffix = ',';
            el.textContent = text + suffix;
        });
    }

    function init() {
        applyGreetings();
        document.addEventListener('visibilitychange', function () {
            if (document.visibilityState === 'visible') {
                applyGreetings();
            }
        });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
