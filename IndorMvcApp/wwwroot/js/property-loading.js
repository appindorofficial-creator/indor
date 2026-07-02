(function () {
    const STEPS = [
        "Geocoding your address…",
        "Searching public records…",
        "Checking Redfin & Realtor listings…",
        "Scanning county assessor data…",
        "Building your House Fact profile…",
        "Organizing results — almost there…"
    ];

    const STEP_INTERVAL_MS = 4500;

    function getOverlay() {
        return document.getElementById("propertyResearchLoading");
    }

    function getStepEl() {
        return document.getElementById("prlStepText");
    }

    let stepIndex = 0;
    let stepTimer = null;

    function rotateStep() {
        const stepEl = getStepEl();
        if (!stepEl) return;

        stepIndex = (stepIndex + 1) % STEPS.length;
        stepEl.classList.add("is-changing");

        window.setTimeout(function () {
            stepEl.textContent = STEPS[stepIndex];
            stepEl.classList.remove("is-changing");
        }, 280);
    }

    function hideLoading() {
        const overlay = getOverlay();
        if (overlay) {
            overlay.classList.remove("is-visible");
            overlay.setAttribute("hidden", "hidden");
        }
        document.body.classList.remove("prl-loading");
        if (stepTimer) {
            window.clearInterval(stepTimer);
            stepTimer = null;
        }
    }

    function showLoading(submitBtn) {
        const overlay = getOverlay();
        if (!overlay) return;

        stepIndex = 0;
        const stepEl = getStepEl();
        if (stepEl) {
            stepEl.textContent = STEPS[0];
            stepEl.classList.remove("is-changing");
        }

        overlay.removeAttribute("hidden");
        overlay.classList.add("is-visible");
        document.body.classList.add("prl-loading");

        if (submitBtn) {
            submitBtn.disabled = true;
            submitBtn.setAttribute("aria-busy", "true");
        }

        if (stepTimer) window.clearInterval(stepTimer);
        stepTimer = window.setInterval(rotateStep, STEP_INTERVAL_MS);
    }

    function submitAfterPaint(form, submitBtn) {
        showLoading(submitBtn);
        // Let the browser paint the overlay before the long POST starts.
        window.requestAnimationFrame(function () {
            window.requestAnimationFrame(function () {
                form.submit();
            });
        });
    }

    function shouldShowResearchLoading(form, submitter) {
        if (form.hasAttribute("data-property-research-loading")) {
            return true;
        }

        return !!(submitter && submitter.getAttribute("data-trigger-research-loading") === "true");
    }

    function handleResearchSubmit(event) {
        const form = event.target;
        if (!(form instanceof HTMLFormElement)) {
            return;
        }

        const submitter = event.submitter;
        if (!shouldShowResearchLoading(form, submitter)) {
            return;
        }

        if (form.dataset.prlSubmitted === "true") {
            return;
        }

        if (typeof form.reportValidity === "function" && !form.reportValidity()) {
            return;
        }

        event.preventDefault();
        form.dataset.prlSubmitted = "true";

        const submitBtn = submitter || form.querySelector('[type="submit"]');
        submitAfterPaint(form, submitBtn);
    }

    function bindForms() {
        document.querySelectorAll("[data-property-research-loading]").forEach(function (form) {
            if (form.dataset.prlBound === "true") return;
            form.dataset.prlBound = "true";
            form.addEventListener("submit", handleResearchSubmit);
        });

        if (document.body.dataset.prlDocumentBound === "true") {
            return;
        }

        document.body.dataset.prlDocumentBound = "true";
        document.addEventListener("submit", handleResearchSubmit, true);
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", bindForms);
    } else {
        bindForms();
    }

    window.addEventListener("pageshow", hideLoading);
})();
