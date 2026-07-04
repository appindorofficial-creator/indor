(function () {
    function initNotifications() {
        var wrap = document.getElementById('paNotifyWrap');
        var btn = document.getElementById('paNotifyBtn');
        var panel = document.getElementById('paNotifyPanel');
        if (!wrap || !btn || !panel) {
            return;
        }

        var markViewedUrl = wrap.getAttribute('data-mark-viewed-url');
        var token = wrap.querySelector('input[name="__RequestVerificationToken"]')?.value;
        var markedViewed = !btn.classList.contains('has-dot');

        function clearUnreadIndicators() {
            btn.classList.remove('has-dot');
            var badge = document.getElementById('paNotifyNewBadge');
            if (badge) {
                badge.remove();
            }
        }

        function markNotificationsViewed() {
            if (markedViewed || !markViewedUrl) {
                return;
            }

            markedViewed = true;
            clearUnreadIndicators();

            fetch(markViewedUrl, {
                method: 'POST',
                headers: {
                    'RequestVerificationToken': token,
                    'Content-Type': 'application/x-www-form-urlencoded',
                    'X-Requested-With': 'XMLHttpRequest'
                },
                body: '__RequestVerificationToken=' + encodeURIComponent(token || '')
            }).catch(function () {
                markedViewed = false;
            });
        }

        function closePanel() {
            panel.hidden = true;
            btn.setAttribute('aria-expanded', 'false');
        }

        function openPanel() {
            panel.hidden = false;
            btn.setAttribute('aria-expanded', 'true');
            markNotificationsViewed();
        }

        btn.addEventListener('click', function (e) {
            e.stopPropagation();
            if (panel.hidden) {
                openPanel();
            } else {
                closePanel();
            }
        });

        document.addEventListener('click', function (e) {
            if (!wrap.contains(e.target)) {
                closePanel();
            }
        });

        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape') {
                closePanel();
            }
        });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initNotifications);
    } else {
        initNotifications();
    }
})();
