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

    var photoForm = document.getElementById('prvProfilePhotoForm');
    var menuBtn = document.getElementById('prvProfilePhotoBtn');
    var menu = document.getElementById('prvProfilePhotoMenu');
    var takeBtn = document.getElementById('prvProfilePhotoTake');
    var chooseBtn = document.getElementById('prvProfilePhotoChoose');
    var cameraInput = document.getElementById('prvProfilePhotoCamera');
    var libraryInput = document.getElementById('prvProfilePhotoLibrary');
    var previewImg = document.getElementById('prvProfilePhotoImg');
    var placeholder = document.getElementById('prvProfilePhotoPlaceholder');

    if (photoForm && menuBtn && menu) {
        function closePhotoMenu() {
            menu.hidden = true;
            menuBtn.setAttribute('aria-expanded', 'false');
        }

        menuBtn.addEventListener('click', function (e) {
            e.stopPropagation();
            var willOpen = menu.hidden;
            menu.hidden = !willOpen;
            menuBtn.setAttribute('aria-expanded', willOpen ? 'true' : 'false');
        });

        document.addEventListener('click', closePhotoMenu);
        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape') {
                closePhotoMenu();
            }
        });

        if (takeBtn && cameraInput) {
            takeBtn.addEventListener('click', function (e) {
                e.stopPropagation();
                closePhotoMenu();
                cameraInput.click();
            });
        }

        if (chooseBtn && libraryInput) {
            chooseBtn.addEventListener('click', function (e) {
                e.stopPropagation();
                closePhotoMenu();
                libraryInput.click();
            });
        }

        function showPhotoAlert(message, tone) {
            var wrap = document.getElementById('prvProfilePhotoAlerts');
            if (!wrap) {
                return;
            }

            wrap.innerHTML = '';
            var alert = document.createElement('div');
            alert.className = 'prv-pro-profile-photo-alert prv-pro-profile-photo-alert--' + tone;
            alert.setAttribute('role', tone === 'success' ? 'status' : 'alert');
            alert.textContent = message;
            wrap.appendChild(alert);
        }

        function handlePhotoSelected(input) {
            if (!input.files || input.files.length === 0) {
                return;
            }

            var file = input.files[0];
            if (previewImg && file.type.startsWith('image/')) {
                previewImg.src = URL.createObjectURL(file);
                previewImg.hidden = false;
                if (placeholder) {
                    placeholder.hidden = true;
                }
            }

            var formData = new FormData(photoForm);
            formData.delete('photo');
            formData.append('photo', file);

            fetch(photoForm.action, {
                method: 'POST',
                body: formData,
                headers: { 'X-Requested-With': 'XMLHttpRequest' }
            })
                .then(function (response) {
                    return response.json().then(function (data) {
                        if (!response.ok || !data.ok) {
                            throw new Error(data.message || 'Could not upload photo.');
                        }
                        return data;
                    });
                })
                .then(function (data) {
                    if (previewImg && data.photoUrl) {
                        previewImg.src = data.photoUrl;
                        previewImg.hidden = false;
                        if (placeholder) {
                            placeholder.hidden = true;
                        }
                    }
                    showPhotoAlert(data.message || 'Profile photo updated.', 'success');
                })
                .catch(function (error) {
                    showPhotoAlert(error.message || 'Could not upload photo.', 'error');
                })
                .finally(function () {
                    input.value = '';
                });
        }

        if (cameraInput) {
            cameraInput.addEventListener('change', function () {
                handlePhotoSelected(cameraInput);
            });
        }

        if (libraryInput) {
            libraryInput.addEventListener('change', function () {
                handlePhotoSelected(libraryInput);
            });
        }
    }
})();
