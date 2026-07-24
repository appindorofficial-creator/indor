(function () {
    if (window.__indorNavLoading) return;
    window.__indorNavLoading = true;

    var OVERLAY_ID = "indorNavigationLoading";
    var BOTTOM_NAV_SELECTOR = ".bottom-nav, .prv-pro-bottom-nav, .rl-bottom-nav, .pa-bottom-nav";
    var BOTTOM_NAV_SPA_TARGET_SELECTOR =
        ".bottom-nav [data-target], .prv-pro-bottom-nav [data-target], .rl-bottom-nav [data-target], .pa-bottom-nav [data-target]";
    var SPA_HIDE_DELAY_MS = 650;
    var MAX_VISIBLE_MS = 8000;
    // If the document URL has not changed after showing the overlay, navigation
    // was cancelled (common in WKWebView when a full-screen cover appears mid-click).
    var NAV_FAILSAFE_MS = 1600;
    var hideTimer = null;
    var maxVisibleTimer = null;
    var navFailsafeTimer = null;
    // When the last interaction was on an opted-out element, the beforeunload
    // catch-all must not force the overlay (respects data-no-nav-loading).
    var suppressUnloadOverlay = false;
    var suppressResetTimer = null;

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
        // Hash-only / mailto / javascript links never leave the page — skip the overlay.
        if (!isNavigableHref(href)) return true;

        try {
            var url = new URL(href, location.href);
            // Same document (with or without hash) never unloads — skip overlay
            // so "Cargando..." cannot stick when Edit/Back points at this page.
            if (url.pathname === location.pathname && url.search === location.search) {
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
        if (meta && meta.content) return meta.content;
        var lang = (document.documentElement.lang || '').toLowerCase();
        return lang.indexOf('es') === 0 ? 'Cargando...' : 'Loading';
    }

    function ensureOverlay() {
        var overlay = document.getElementById(OVERLAY_ID);
        var host = document.documentElement || document.body;
        if (!host) return null;

        if (overlay) {
            if (overlay.parentNode !== host) {
                host.appendChild(overlay);
            }
            return overlay;
        }

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

        host.appendChild(overlay);
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

    // iOS WKWebView mis-sizes position:fixed overlays (100vh/100dvh can resolve
    // to ~half the screen, leaving the page visible below the backdrop). Pin the
    // real viewport size in pixels so the cover is always full-screen.
    function viewportHeight() {
        var vv = window.visualViewport;
        return Math.max(
            window.innerHeight || 0,
            document.documentElement ? document.documentElement.clientHeight : 0,
            vv ? vv.height : 0
        );
    }

    function viewportWidth() {
        var vv = window.visualViewport;
        return Math.max(
            window.innerWidth || 0,
            document.documentElement ? document.documentElement.clientWidth : 0,
            vv ? vv.width : 0
        );
    }

    function sizeOverlayToViewport() {
        var overlay = document.getElementById(OVERLAY_ID);
        if (!overlay) return;
        overlay.style.height = viewportHeight() + "px";
        overlay.style.width = viewportWidth() + "px";
    }

    function onViewportResize() {
        var overlay = document.getElementById(OVERLAY_ID);
        if (overlay && overlay.classList.contains("is-visible")) {
            sizeOverlayToViewport();
        }
    }

    if (!window.__indorNavLoadingResizeBound) {
        window.__indorNavLoadingResizeBound = true;
        window.addEventListener("resize", onViewportResize);
        window.addEventListener("orientationchange", onViewportResize);
        if (window.visualViewport) {
            window.visualViewport.addEventListener("resize", onViewportResize);
        }
    }

    function markSuppressUnload() {
        suppressUnloadOverlay = true;
        if (suppressResetTimer) window.clearTimeout(suppressResetTimer);
        suppressResetTimer = window.setTimeout(function () {
            suppressUnloadOverlay = false;
        }, 2500);
    }

    function clearHideTimer() {
        if (!hideTimer) return;
        window.clearTimeout(hideTimer);
        hideTimer = null;
    }

    function clearMaxVisibleTimer() {
        if (!maxVisibleTimer) return;
        window.clearTimeout(maxVisibleTimer);
        maxVisibleTimer = null;
    }

    function clearNavFailsafeTimer() {
        if (!navFailsafeTimer) return;
        window.clearTimeout(navFailsafeTimer);
        navFailsafeTimer = null;
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
        if (!overlay) return;
        if (!opts.keepScheduledHide) {
            clearHideTimer();
        }
        clearMaxVisibleTimer();
        clearNavFailsafeTimer();
        updateLoadingText(overlay);
        overlay.removeAttribute("hidden");
        sizeOverlayToViewport();
        overlay.classList.add("is-visible");
        document.documentElement.classList.add("indor-nav-loading-active");
        document.body.classList.add("indor-nav-loading-active");
        if (opts.autoHide) {
            scheduleHideNavigationLoading(opts.autoHideDelay);
        } else {
            // Never leave a stuck "Cargando..." if navigation/error never completes.
            var maxMs = typeof opts.maxVisibleMs === "number" ? opts.maxVisibleMs : MAX_VISIBLE_MS;
            maxVisibleTimer = window.setTimeout(function () {
                maxVisibleTimer = null;
                hideNavigationLoading();
            }, maxMs);

            // Same-document failsafe: if we never left this URL, hide the spinner
            // so "Volver al inicio" / in-app links can be tapped again.
            if (opts.failsafeSameUrl !== false) {
                var hrefAtShow = location.href;
                navFailsafeTimer = window.setTimeout(function () {
                    navFailsafeTimer = null;
                    if (location.href === hrefAtShow) {
                        hideNavigationLoading();
                    }
                }, typeof opts.failsafeMs === "number" ? opts.failsafeMs : NAV_FAILSAFE_MS);
            }
        }
    }

    function hideNavigationLoading() {
        clearHideTimer();
        clearMaxVisibleTimer();
        clearNavFailsafeTimer();
        var overlay = document.getElementById(OVERLAY_ID);
        if (!overlay) return;

        overlay.classList.remove("is-visible");
        overlay.setAttribute("hidden", "hidden");
        overlay.style.height = "";
        overlay.style.width = "";
        document.documentElement.classList.remove("indor-nav-loading-active");
        document.body.classList.remove("indor-nav-loading-active");
    }

    window.indorShowNavigationLoading = showNavigationLoading;
    window.indorHideNavigationLoading = hideNavigationLoading;

    document.addEventListener("click", function (e) {
        if (!isPlainLeftClick(e)) return;
        if (!e.target || !e.target.closest) return;

        // Remember opt-outs so the beforeunload catch-all honours them too.
        if (e.target.closest("[data-no-nav-loading]")) {
            markSuppressUnload();
            return;
        }

        var spaTarget = e.target.closest(BOTTOM_NAV_SPA_TARGET_SELECTOR);
        if (spaTarget) {
            showNavigationLoading({ autoHide: true });
            return;
        }

        var bottomNavItem = e.target.closest(BOTTOM_NAV_SELECTOR + " .nav-item");
        if (bottomNavItem && !bottomNavItem.hasAttribute("data-no-nav-loading")) {
            var isSpaButton = bottomNavItem.tagName === "BUTTON"
                || bottomNavItem.getAttribute("role") === "button";
            var bottomNavHref = bottomNavItem.getAttribute("href") || "";
            if (isSpaButton || isSamePageHashNavigation(bottomNavHref)) {
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

        if (shouldSkipLink(link)) { markSuppressUnload(); return; }

        var href = link.getAttribute("href") || "";
        // Cross-origin / external targets may open outside the WebView and leave
        // this page mounted — never force a stuck spinner for those.
        if (!isSameOrigin(href)) { markSuppressUnload(); return; }

        // Defer the cover until after the browser commits the link's default
        // navigation. Showing it synchronously in capture can cancel navigation
        // in mobile WebViews and leave Cargando stuck on the same form.
        window.setTimeout(function () {
            showNavigationLoading();
        }, 0);
    }, true);

    document.addEventListener("submit", function (e) {
        var form = e.target;
        if (!form || form.tagName !== "FORM" || shouldSkipForm(form)) return;

        var event = e;
        window.setTimeout(function () {
            if (event.defaultPrevented) {
                hideNavigationLoading();
                return;
            }
            showNavigationLoading();
        }, 0);
    }, false);

    // Catch-all: any real navigation that unloads the page shows the cover, even
    // when triggered by JS (button onclick -> location.href), <button> elements,
    // programmatic redirects, etc. that the click/submit handlers can't detect.
    // Skipped links/forms call preventDefault so they never unload -> no false
    // spinner. The app defines no beforeunload confirm dialogs, so this is safe.
    window.addEventListener("beforeunload", function () {
        if (suppressUnloadOverlay) return;
        var overlay = document.getElementById(OVERLAY_ID);
        if (!overlay || !overlay.classList.contains("is-visible")) {
            showNavigationLoading();
        }
    });

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
