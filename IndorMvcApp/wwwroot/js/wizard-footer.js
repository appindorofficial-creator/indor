(function () {
    if (window.__indorWizardFooter) return;
    window.__indorWizardFooter = true;

    var actionSelector = 'button.btn-primary, button.btn-outline, button[type="submit"], a.btn-primary, a.btn-outline';

    function isActionNode(node) {
        if (!node || node.nodeType !== 1) return false;
        if (node.matches('.sticky-footer, .validation-summary, .validation-summary-errors')) return false;
        if (node.matches(actionSelector)) return true;
        return node.classList.contains('hvac-save-btn') || node.classList.contains('hvac-outline-btn');
    }

    function trailingActions(parent) {
        var children = Array.from(parent.children);
        var actions = [];

        for (var i = children.length - 1; i >= 0; i--) {
            var child = children[i];
            if (child.classList.contains('sticky-footer')) break;
            if (isActionNode(child)) {
                actions.unshift(child);
            } else if (actions.length > 0) {
                break;
            }
        }

        return actions;
    }

    function promoteToStickyFooter(container) {
        if (!container || container.dataset.iwFooterBound === 'true') return;

        if (container.classList.contains('sticky-footer')
            || container.querySelector(':scope > .sticky-footer')) {
            container.dataset.iwFooterBound = 'true';
            return;
        }

        var innerWrap = container.querySelector(':scope > .cl-form-actions:not(.sticky-footer)');
        if (innerWrap) {
            innerWrap.classList.add('sticky-footer');
            container.dataset.iwFooterBound = 'true';
            return;
        }

        var actions = trailingActions(container);
        if (actions.length === 0) return;

        var footer = document.createElement('div');
        footer.className = 'sticky-footer';
        actions.forEach(function (node) {
            footer.appendChild(node);
        });
        container.appendChild(footer);

        if (container.classList.contains('cl-form-actions')) {
            container.classList.remove('cl-form-actions');
        }

        container.dataset.iwFooterBound = 'true';
    }

    function bindAll() {
        document.querySelectorAll('.iw-wizard-page .iw-wizard-shell form').forEach(promoteToStickyFooter);
        document.querySelectorAll('.iw-wizard-page .iw-wizard-shell > .cl-form-actions:not(.sticky-footer)').forEach(function (block) {
            block.classList.add('sticky-footer');
            block.dataset.iwFooterBound = 'true';
        });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', bindAll);
    } else {
        bindAll();
    }
})();
