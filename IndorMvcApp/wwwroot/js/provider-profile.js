(function () {
    'use strict';

    function copyText(text) {
        if (navigator.clipboard && window.isSecureContext) {
            return navigator.clipboard.writeText(text);
        }

        return new Promise(function (resolve, reject) {
            var textarea = document.createElement('textarea');
            textarea.value = text;
            textarea.setAttribute('readonly', '');
            textarea.style.position = 'fixed';
            textarea.style.left = '-9999px';
            document.body.appendChild(textarea);
            textarea.select();
            try {
                if (document.execCommand('copy')) {
                    resolve();
                } else {
                    reject(new Error('copy failed'));
                }
            } catch (err) {
                reject(err);
            } finally {
                document.body.removeChild(textarea);
            }
        });
    }

    function showToast(message) {
        var toast = document.getElementById('prvProfileToast');
        if (!toast) {
            window.alert(message);
            return;
        }

        toast.textContent = message;
        toast.hidden = false;
        toast.classList.add('is-visible');

        if (showToast.timer) {
            clearTimeout(showToast.timer);
        }
        if (showToast.hideTimer) {
            clearTimeout(showToast.hideTimer);
        }

        showToast.timer = setTimeout(function () {
            toast.classList.remove('is-visible');
            showToast.hideTimer = setTimeout(function () {
                toast.hidden = true;
            }, 250);
        }, 2600);
    }

    function wireShareButton(button) {
        if (!button || button.dataset.shareWired === 'true') {
            return;
        }

        button.dataset.shareWired = 'true';
        var shareUrl = button.getAttribute('data-share-url') || button.href || window.location.href;
        var shareTitle = button.getAttribute('data-share-title') || document.title;
        var shareText = button.getAttribute('data-share-text') || 'View my provider profile on INDOR';

        button.addEventListener('click', function (event) {
            event.preventDefault();

            var payload = {
                title: shareTitle,
                text: shareText,
                url: shareUrl
            };

            if (navigator.share) {
                navigator.share(payload).catch(function (err) {
                    if (err && err.name === 'AbortError') {
                        return;
                    }
                    copyText(shareUrl)
                        .then(function () { showToast('Profile link copied to clipboard.'); })
                        .catch(function () { window.prompt('Copy profile link:', shareUrl); });
                });
                return;
            }

            copyText(shareUrl)
                .then(function () { showToast('Profile link copied to clipboard.'); })
                .catch(function () { window.prompt('Copy profile link:', shareUrl); });
        });
    }

    document.querySelectorAll('[data-provider-share]').forEach(wireShareButton);
})();
