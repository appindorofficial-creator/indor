(function () {
    if (window.__indorNavLoading) return;
    window.__indorNavLoading = true;

    var OVERLAY_ID = "indorNavigationLoading";
    var BOTTOM_NAV_SELECTOR = ".bottom-nav, .prv-pro-bottom-nav, .rl-bottom-nav, .pa-bottom-nav";
    var SPA_HIDE_DELAY_MS = 320;
    var hideTimer = null;

    function isPlainLeftClick(e) {
        return e.button === 0
            && !e.defaultPrevented
            && !e.metaKey
            && !e.ctrlKey
            && !e.shiftKey
            && !e.altKey;
    }

    function isSameOrigin(url) {
        try {
            return new URL(url, location.href).origin === location.origin;
        } catch (err) {
            return false;
        }
    }

    function isNavigableHref(href) {
        if (!href) return false;
        var lower = href.trim().toLowerCase();
        if (!lower || lower === "#" || lower.startsWith("javascript:")) return false;
        if (lower.startsWith("mailto:") || lower.startsWith("tel:")) return false;
        if (lower.startsWith("#")) return false;
        return true;
    }

    function isSamePageHashNavigation(href) {
        try {
            var url = new URL(href, location.href);
            return url.origin === location.origin
                && url.pathname === location.pathname
                && url.search === location.search
                && !!url.hash;
        } catch (err) {
            return false;
        }
    }

    function isBottomNavElement(el) {
        return !!(el && el.closest && el.closest(BOTTOM_NAV_SELECTOR));
    }

    function shouldSkipLink(link) {
        if (!link || link.hasAttribute("data-no-nav-loading")) return true;
        if (link.target && link.target.toLowerCase() === "_blank") return true;
        if (link.hasAttribute("download")) return true;
        if (link.getAttribute("role") === "button") return true;
        if (link.closest("[data-no-nav-loading]")) return true;

        var href = link.getAttribute("href") || "";
        if (!isNavigableHref(href)) return false;

        try {
            var url = new URL(href, location.href);
            if (url.pathname === location.pathname && url.search === location.search && url.hash) {
                return true;
            }
        } catch (err) {
            return true;
        }

        return false;
    }

    function shouldSkipForm(form) {
        if (!form || form.hasAttribute("data-no-nav-loading")) return true;
        if (form.closest("[data-no-nav-loading]")) return true;
        if (form.hasAttribute("data-property-research-loading")) return true;
        if (form.getAttribute("target") === "_blank") return true;

        var method = (form.getAttribute("method") || "get").toLowerCase();
        if (method !== "get" && method !== "post") return true;

        return false;
    }

    function getLoadingText() {
        var meta = document.querySelector('meta[name="indor-loading-text"]');
        return meta && meta.content ? meta.content : "Loading";
    }

    function ensureOverlay() {
        var overlay = document.getElementById(OVERLAY_ID);
        if (overlay) return overlay;

        overlay = document.createElement("div");
        overlay.id = OVERLAY_ID;
        overlay.className = "indor-nav-loading";
        overlay.setAttribute("hidden", "hidden");
        overlay.setAttribute("role", "status");
        overlay.setAttribute("aria-live", "polite");
        overlay.setAttribute("aria-label", getLoadingText());
        overlay.innerHTML =
            '<div class="indor-nav-loading__backdrop" aria-hidden="true"></div>' +
            '<div class="indor-nav-loading__panel">' +
                '<div class="indor-nav-loading__spinner" aria-hidden="true">' +
                    '<span class="indor-nav-loading__ring indor-nav-loading__ring--outer"></span>' +
                    '<span class="indor-nav-loading__ring indor-nav-loading__ring--inner"></span>' +
                    '<span class="indor-nav-loading__dot"></span>' +
                '</div>' +
                '<div class="indor-nav-loading__brand">INDOR</div>' +
                '<div class="indor-nav-loading__text"></div>' +
            '</div>';

        document.body.appendChild(overlay);
        updateLoadingText(overlay);
        return overlay;
    }

    function updateLoadingText(overlay) {
        if (!overlay) return;
        var text = getLoadingText();
        overlay.setAttribute("aria-label", text);
        var textEl = overlay.querySelector(".indor-nav-loading__text");
        if (textEl) {
            textEl.textContent = text.replace(/\.\.\.$/, "");
            if (/\.\.\.$/.test(text)) {
                var dots = document.createElement("span");
                dots.className = "indor-nav-loading__dots";
                textEl.appendChild(dots);
            }
        }
    }

    function clearHideTimer() {
        if (!hideTimer) return;
        window.clearTimeout(hideTimer);
        hideTimer = null;
    }

    function scheduleHideNavigationLoading(delay) {
        clearHideTimer();
        hideTimer = window.setTimeout(function () {
            hideTimer = null;
            hideNavigationLoading();
        }, typeof delay === "number" ? delay : SPA_HIDE_DELAY_MS);
    }

    function showNavigationLoading(options) {
        var opts = options || {};
        var overlay = ensureOverlay();
        if (!opts.keepScheduledHide) {
            clearHideTimer();
        }
        updateLoadingText(overlay);
        overlay.removeAttribute("hidden");
        overlay.classList.add("is-visible");
        document.body.classList.add("indor-nav-loading-active");
        if (opts.autoHide) {
            scheduleHideNavigationLoading(opts.autoHideDelay);
        }
    }

    function hideNavigationLoading() {
        clearHideTimer();
        var overlay = document.getElementById(OVERLAY_ID);
        if (!overlay) return;

        overlay.classList.remove("is-visible");
        overlay.setAttribute("hidden", "hidden");
        document.body.classList.remove("indor-nav-loading-active");
    }

    window.indorShowNavigationLoading = showNavigationLoading;
    window.indorHideNavigationLoading = hideNavigationLoading;

    document.addEventListener("click", function (e) {
        if (!isPlainLeftClick(e)) return;
        if (!e.target || !e.target.closest) return;

        var spaTarget = e.target.closest(BOTTOM_NAV_SELECTOR + " [data-target]");
        if (spaTarget) {
            var tag = spaTarget.tagName;
            if (tag === "BUTTON" || spaTarget.getAttribute("role") === "button" || tag === "A") {
                showNavigationLoading({ autoHide: true });
                return;
            }
        }

        var link = e.target.closest("a[href]");
        if (!link) return;

        if (isBottomNavElement(link) && isSamePageHashNavigation(link.getAttribute("href") || "")) {
            showNavigationLoading({ autoHide: true });
            return;
        }

        if (shouldSkipLink(link)) return;

        var href = link.getAttribute("href") || "";
        if (!isSameOrigin(href)) return;

        showNavigationLoading();
    }, true);

    document.addEventListener("submit", function (e) {
        var form = e.target;
        if (!form || form.tagName !== "FORM" || shouldSkipForm(form)) return;

        var event = e;
        window.setTimeout(function () {
            if (event.defaultPrevented) return;
            showNavigationLoading();
        }, 0);
    }, false);

    window.addEventListener("pageshow", hideNavigationLoading);
    document.addEventListener("visibilitychange", function () {
        if (document.visibilityState === "visible") {
            hideNavigationLoading();
        }
    });

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", ensureOverlay);
    } else if (document.body) {
        ensureOverlay();
    }
})();
