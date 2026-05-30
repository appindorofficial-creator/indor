(function () {
    var primaryCategories = ["snapshot", "systems", "roof", "permits", "hoa"];

    function initHouseFact(root) {
        if (!root || root.dataset.hfBound === "true") return;
        root.dataset.hfBound = "true";

        var overviewScreen = root.querySelector('[data-hf-screen="overview"]');
        var detailScreen = root.querySelector('[data-hf-screen="detail"]');
        var detailTitle = root.querySelector("[data-hf-detail-title]");
        var detailSections = root.querySelectorAll(".hf-detail-section");
        var openButtons = root.querySelectorAll("[data-hf-open-category]");
        var backBtn = root.querySelector("[data-hf-back-overview]");
        var expandAllBtn = root.querySelector("[data-hf-expand-all]");

        function sectionMatchesCategory(section, categoryKey, sectionIds) {
            if (sectionIds && sectionIds.length > 0) {
                return sectionIds.indexOf(section.dataset.hfSectionId) >= 0;
            }

            if (categoryKey === "more") {
                return primaryCategories.indexOf(section.dataset.hfCategory) < 0;
            }

            return section.dataset.hfCategory === categoryKey;
        }

        function openCategory(categoryKey, title, sectionIds) {
            if (!detailScreen || !overviewScreen) return;

            overviewScreen.hidden = true;
            detailScreen.hidden = false;

            if (detailTitle) {
                detailTitle.textContent = title || "Section details";
            }

            detailSections.forEach(function (section) {
                var match = sectionMatchesCategory(section, categoryKey, sectionIds);
                section.hidden = !match;
                section.open = match;
            });

            window.scrollTo({ top: 0, behavior: "smooth" });
        }

        function backToOverview() {
            if (!detailScreen || !overviewScreen) return;
            detailScreen.hidden = true;
            overviewScreen.hidden = false;
            detailSections.forEach(function (section) {
                section.hidden = true;
                section.open = false;
            });
            window.scrollTo({ top: 0, behavior: "smooth" });
        }

        openButtons.forEach(function (button) {
            button.addEventListener("click", function () {
                var key = button.getAttribute("data-hf-open-category");
                var title = button.getAttribute("data-hf-title") || button.textContent.trim();
                var idsRaw = button.getAttribute("data-hf-section-ids");
                var sectionIds = idsRaw ? idsRaw.split(",").filter(Boolean) : null;
                openCategory(key, title, sectionIds);
            });
        });

        if (backBtn) {
            backBtn.addEventListener("click", backToOverview);
        }

        if (expandAllBtn) {
            expandAllBtn.addEventListener("click", function () {
                if (!detailScreen || !overviewScreen) return;
                overviewScreen.hidden = true;
                detailScreen.hidden = false;
                if (detailTitle) {
                    detailTitle.textContent = "All property details";
                }
                detailSections.forEach(function (section) {
                    section.hidden = false;
                    section.open = true;
                });
                window.scrollTo({ top: 0, behavior: "smooth" });
            });
        }
    }

    function bindAll() {
        document.querySelectorAll("[data-house-fact-root]").forEach(initHouseFact);
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", bindAll);
    } else {
        bindAll();
    }
})();
