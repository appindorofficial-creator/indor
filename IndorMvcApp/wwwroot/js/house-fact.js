(function () {
    function initHouseFact(root) {
        if (!root || root.dataset.hfBound === "true") return;
        root.dataset.hfBound = "true";

        var overviewScreen = root.querySelector('[data-hf-screen="overview"]');
        var detailScreen = root.querySelector('[data-hf-screen="detail"]');
        var detailTitle = root.querySelector("[data-hf-detail-title]");
        var detailSections = root.querySelectorAll(".hf-detail-section");
        var openButtons = root.querySelectorAll("[data-hf-open-category]");
        var backBtn = root.querySelector("[data-hf-back-overview]");
        var layoutBackButtons = function () {
            return document.querySelectorAll(".hf-top .app-back-btn, .app-header .app-back-btn");
        };

        function setLayoutBackVisible(visible) {
            layoutBackButtons().forEach(function (button) {
                if (button.getAttribute("aria-hidden") === "true") {
                    return;
                }

                button.hidden = !visible;
            });
        }

        function sectionMatchesCategory(section, categoryKey, sectionIds) {
            if (sectionIds && sectionIds.length > 0) {
                return sectionIds.indexOf(section.dataset.hfSectionId) >= 0;
            }

            return section.dataset.hfCategory === categoryKey;
        }

        function expandAllSections() {
            if (!detailScreen || !overviewScreen) return;
            setLayoutBackVisible(false);
            overviewScreen.hidden = true;
            detailScreen.hidden = false;
            if (detailTitle) {
                detailTitle.textContent = root.getAttribute("data-hf-all-details-title") || "All property details";
            }
            detailSections.forEach(function (section) {
                section.hidden = false;
                section.open = true;
            });

            var scrollTarget = detailScreen.querySelector("[data-hf-back-overview]") || detailScreen;
            scrollTarget.scrollIntoView({ behavior: "smooth", block: "start" });
        }

        function scrollToSectionCards() {
            if (!overviewScreen) return;

            // Stay on overview — guide to dedicated section tabs (not the "More details" dump).
            setLayoutBackVisible(true);
            if (detailScreen) {
                detailScreen.hidden = true;
            }
            overviewScreen.hidden = false;

            var target = overviewScreen.querySelector(".hf-jump-scroll")
                || overviewScreen.querySelector(".hf-jump-row")
                || overviewScreen.querySelector(".hf-category-grid")
                || overviewScreen;
            target.classList.add("hf-sections-spotlight");
            target.scrollIntoView({ behavior: "smooth", block: "center" });
            window.setTimeout(function () {
                target.classList.remove("hf-sections-spotlight");
            }, 1800);
        }

        function openCategory(categoryKey, title, sectionIds) {
            if (!detailScreen || !overviewScreen) return;

            setLayoutBackVisible(false);
            overviewScreen.hidden = true;
            detailScreen.hidden = false;

            if (detailTitle) {
                detailTitle.textContent = title || root.getAttribute("data-hf-section-details-title") || "Section details";
            }

            detailSections.forEach(function (section) {
                var match = sectionMatchesCategory(section, categoryKey, sectionIds);
                section.hidden = !match;
                section.open = match;
            });

            var scrollTarget = detailScreen.querySelector("[data-hf-back-overview]") || detailScreen;
            scrollTarget.scrollIntoView({ behavior: "smooth", block: "start" });
        }

        function backToOverview() {
            if (!detailScreen || !overviewScreen) return;
            setLayoutBackVisible(true);
            detailScreen.hidden = true;
            overviewScreen.hidden = false;
            detailSections.forEach(function (section) {
                section.hidden = true;
                section.open = false;
            });
            var scrollTarget = overviewScreen.querySelector(".hf-explore-more")
                || overviewScreen.querySelector(".hf-category-grid")
                || overviewScreen.querySelector("[data-hf-expand-all]")
                || overviewScreen;
            scrollTarget.scrollIntoView({ behavior: "smooth", block: "start" });
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

        root.querySelectorAll("[data-hf-expand-all]").forEach(function (expandAllBtn) {
            expandAllBtn.addEventListener("click", expandAllSections);
        });

        root.querySelectorAll("[data-hf-scroll-categories]").forEach(function (scrollBtn) {
            scrollBtn.addEventListener("click", scrollToSectionCards);
        });
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
