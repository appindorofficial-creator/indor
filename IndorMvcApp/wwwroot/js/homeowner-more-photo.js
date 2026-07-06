(function () {
    'use strict';

    function wirePhotoPicker(options) {
        var photoForm = document.getElementById(options.formId);
        var menuBtn = document.getElementById(options.btnId);
        var menu = document.getElementById(options.menuId);
        var takeBtn = document.getElementById(options.takeId);
        var chooseBtn = document.getElementById(options.chooseId);
        var cameraInput = document.getElementById(options.cameraId);
        var libraryInput = document.getElementById(options.libraryId);
        var previewImg = document.getElementById(options.imgId);
        var placeholder = options.placeholderId ? document.getElementById(options.placeholderId) : null;
        var alertsId = options.alertsId;

        if (!photoForm || !menuBtn || !menu) {
            return;
        }

        function closePhotoMenu() {
            menu.hidden = true;
            menuBtn.setAttribute('aria-expanded', 'false');
        }

        menuBtn.addEventListener('click', function (e) {
            e.preventDefault();
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
            if (!alertsId) {
                return;
            }

            var wrap = document.getElementById(alertsId);
            if (!wrap) {
                return;
            }

            wrap.innerHTML = '';
            var alert = document.createElement('div');
            alert.className = 'more-profile-photo-alert more-profile-photo-alert--' + tone;
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
            formData.delete('foto');
            formData.delete('photo');
            formData.append(options.fieldName || 'foto', file);

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

    wirePhotoPicker({
        formId: 'moreProfilePhotoForm',
        btnId: 'moreProfilePhotoBtn',
        menuId: 'moreProfilePhotoMenu',
        takeId: 'moreProfilePhotoTake',
        chooseId: 'moreProfilePhotoChoose',
        cameraId: 'moreProfilePhotoCamera',
        libraryId: 'moreProfilePhotoLibrary',
        imgId: 'moreProfilePhotoImg',
        placeholderId: 'moreProfilePhotoPlaceholder',
        alertsId: 'moreProfilePhotoAlerts',
        fieldName: 'foto'
    });

    wirePhotoPicker({
        formId: 'editProfilePhotoForm',
        btnId: 'editProfilePhotoBtn',
        menuId: 'editProfilePhotoMenu',
        takeId: 'editProfilePhotoTake',
        chooseId: 'editProfilePhotoChoose',
        cameraId: 'editProfilePhotoCamera',
        libraryId: 'editProfilePhotoLibrary',
        imgId: 'photoPreviewImg',
        placeholderId: 'photoPreviewInitial',
        alertsId: 'editProfilePhotoAlerts',
        fieldName: 'foto'
    });
})();
