(function () {
    function readGreetingMeta(name, fallback) {
        var meta = document.querySelector('meta[name="' + name + '"]');
        return meta && meta.content ? meta.content : fallback;
    }

    function greetingForHour(hour) {
        if (hour >= 5 && hour < 12) {
            return readGreetingMeta("indor-greeting-morning", "Good morning");
        }
        if (hour >= 12 && hour < 17) {
            return readGreetingMeta("indor-greeting-afternoon", "Good afternoon");
        }
        return readGreetingMeta("indor-greeting-evening", "Good evening");
    }

    function applyGreetings() {
        var hour = new Date().getHours();
        var text = greetingForHour(hour);
        document.querySelectorAll("[data-indor-greeting]").forEach(function (el) {
            var suffix = el.getAttribute("data-indor-greeting-suffix");
            if (suffix === null) suffix = ",";
            el.textContent = text + suffix;
        });
    }

    function scheduleHourlyRefresh() {
        var now = new Date();
        var msUntilNextHour =
            ((60 - now.getMinutes()) * 60 - now.getSeconds()) * 1000 +
            (1000 - now.getMilliseconds());

        window.setTimeout(function () {
            applyGreetings();
            scheduleHourlyRefresh();
        }, Math.max(msUntilNextHour, 1000));
    }

    function init() {
        applyGreetings();
        scheduleHourlyRefresh();

        document.addEventListener("visibilitychange", function () {
            if (document.visibilityState === "visible") {
                applyGreetings();
            }
        });

        window.addEventListener("pageshow", function (event) {
            if (event.persisted) {
                applyGreetings();
            }
        });
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", init);
    } else {
        init();
    }
})();
