(function () {
    'use strict';

    function wirePhotoPicker(options) {
        var photoForm = document.getElementById(options.formId);
        var menu = document.getElementById(options.menuId);
        var menuBtns = [];
        if (options.btnId) {
            var primaryBtn = document.getElementById(options.btnId);
            if (primaryBtn) {
                menuBtns.push(primaryBtn);
            }
        }
        (options.extraBtnIds || []).forEach(function (btnId) {
            var extraBtn = document.getElementById(btnId);
            if (extraBtn) {
                menuBtns.push(extraBtn);
            }
        });
        var takeBtn = document.getElementById(options.takeId);
        var chooseBtn = document.getElementById(options.chooseId);
        var cameraInput = document.getElementById(options.cameraId);
        var libraryInput = document.getElementById(options.libraryId);
        var previewImg = document.getElementById(options.imgId);
        var placeholder = options.placeholderId ? document.getElementById(options.placeholderId) : null;
        var alertsId = options.alertsId;
        var alertClass = options.alertClass || 'more-profile-photo-alert';

        if (!photoForm || menuBtns.length === 0) {
            return;
        }

        if (!options.openPickerOnButton && !menu) {
            return;
        }

        var menuBtn = menuBtns[0];

        function closePhotoMenu() {
            if (!menu) {
                return;
            }
            menu.hidden = true;
            menuBtns.forEach(function (btn) {
                btn.setAttribute('aria-expanded', 'false');
            });
        }

        var pickerRoot = photoForm;

        function togglePhotoMenu(e) {
            if (!menu) {
                return;
            }
            e.preventDefault();
            e.stopPropagation();
            var willOpen = menu.hidden;
            menu.hidden = !willOpen;
            menuBtns.forEach(function (btn) {
                btn.setAttribute('aria-expanded', willOpen ? 'true' : 'false');
            });
        }

        function openFilePicker(input) {
            if (!input) {
                return;
            }
            closePhotoMenu();
            // Use click() only — showPicker() can no-op without throwing in some WebViews
            // when the input is visually-hidden, leaving the camera control looking "not enabled".
            input.click();
        }

        menuBtns.forEach(function (btn) {
            if (options.openPickerOnButton === 'library' && libraryInput) {
                btn.addEventListener('click', function (e) {
                    e.preventDefault();
                    e.stopPropagation();
                    openFilePicker(libraryInput);
                });
            } else if (options.openPickerOnButton === 'camera' && cameraInput) {
                btn.addEventListener('click', function (e) {
                    e.preventDefault();
                    e.stopPropagation();
                    openFilePicker(cameraInput);
                });
            } else {
                btn.addEventListener('click', togglePhotoMenu);
            }
        });

        document.addEventListener('click', function (e) {
            if (!menu) {
                return;
            }
            if (pickerRoot && pickerRoot.contains(e.target)) {
                return;
            }
            closePhotoMenu();
        });
        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape') {
                closePhotoMenu();
            }
        });

        if (takeBtn && (cameraInput || libraryInput)) {
            takeBtn.addEventListener('click', function (e) {
                e.preventDefault();
                e.stopPropagation();
                // Prefer the camera-labeled input; fall back to library if capture/camera is unavailable.
                openFilePicker(cameraInput || libraryInput);
            });
        }

        if (chooseBtn && (libraryInput || cameraInput)) {
            chooseBtn.addEventListener('click', function (e) {
                e.preventDefault();
                e.stopPropagation();
                openFilePicker(libraryInput || cameraInput);
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
            alert.className = alertClass + ' ' + alertClass + '--' + tone;
            alert.setAttribute('role', tone === 'success' ? 'status' : 'alert');
            alert.textContent = message;
            wrap.appendChild(alert);
        }

        function showPhotoInPreview(url) {
            if (!previewImg || !url) {
                return;
            }

            previewImg.src = url;
            previewImg.hidden = false;
            previewImg.removeAttribute('hidden');
            previewImg.style.display = 'block';
            if (placeholder) {
                placeholder.hidden = true;
                placeholder.setAttribute('hidden', '');
                placeholder.style.display = 'none';
            }

            var previewRoot = previewImg.closest(
                '.prv-pro-profile-photo-preview, .rl-profile-photo-preview, .pa-personal-photo-preview, .pf-edit-photo-preview, .pa-profile-photo-preview'
            );
            if (previewRoot) {
                previewRoot.classList.add('has-image');
            }
        }

        function handlePhotoSelected(input) {
            if (!input.files || input.files.length === 0) {
                return;
            }

            var file = input.files[0];
            if (file.type && file.type.startsWith('image/')) {
                showPhotoInPreview(URL.createObjectURL(file));
            }

            var fieldName = options.fieldName || 'photo';
            var formData = new FormData(photoForm);
            formData.delete(fieldName);
            formData.append(fieldName, file, file.name || 'profile-photo.jpg');

            fetch(photoForm.action, {
                method: 'POST',
                body: formData,
                credentials: 'same-origin',
                headers: { 'X-Requested-With': 'XMLHttpRequest' }
            })
                .then(function (response) {
                    return response.text().then(function (text) {
                        var data = null;
                        try {
                            data = text ? JSON.parse(text) : null;
                        } catch (err) {
                            throw new Error('Could not upload photo.');
                        }
                        if (!response.ok || !data || !data.ok) {
                            throw new Error((data && data.message) || 'Could not upload photo.');
                        }
                        return data;
                    });
                })
                .then(function (data) {
                    if (data.photoUrl) {
                        showPhotoInPreview(data.photoUrl);
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
        fieldName: 'foto',
        alertClass: 'more-profile-photo-alert'
    });

    wirePhotoPicker({
        formId: 'prvEditProfilePhotoForm',
        btnId: 'prvEditProfilePhotoBtn',
        menuId: 'prvEditProfilePhotoMenu',
        takeId: 'prvEditProfilePhotoTake',
        chooseId: 'prvEditProfilePhotoChoose',
        cameraId: 'prvEditProfilePhotoCamera',
        libraryId: 'prvEditProfilePhotoLibrary',
        imgId: 'prvEditProfilePhotoImg',
        placeholderId: 'prvEditProfilePhotoPlaceholder',
        alertsId: 'prvEditProfilePhotoAlerts',
        fieldName: 'photo',
        alertClass: 'prv-pro-profile-photo-alert'
    });

    wirePhotoPicker({
        formId: 'rlBusinessPhotoForm',
        btnId: 'rlBusinessPhotoBtn',
        extraBtnIds: ['rlBusinessPhotoChangeBtn'],
        menuId: 'rlBusinessPhotoMenu',
        takeId: 'rlBusinessPhotoTake',
        chooseId: 'rlBusinessPhotoChoose',
        cameraId: 'rlBusinessPhotoCamera',
        libraryId: 'rlBusinessPhotoLibrary',
        imgId: 'rlBusinessPhotoImg',
        placeholderId: 'rlBusinessPhotoPlaceholder',
        alertsId: 'rlBusinessPhotoAlerts',
        fieldName: 'photo',
        alertClass: 'rl-profile-photo-alert'
    });

    wirePhotoPicker({
        formId: 'paPersonalPhotoForm',
        btnId: 'paPersonalPhotoBtn',
        menuId: 'paPersonalPhotoMenu',
        takeId: 'paPersonalPhotoTake',
        chooseId: 'paPersonalPhotoChoose',
        cameraId: 'paPersonalPhotoCamera',
        libraryId: 'paPersonalPhotoLibrary',
        imgId: 'paPersonalPhotoImg',
        placeholderId: 'paPersonalPhotoPlaceholder',
        alertsId: 'paPersonalPhotoAlerts',
        fieldName: 'photo',
        alertClass: 'pa-personal-alert',
        // One-tap OS picker (Take Photo + Library). Avoids forced capture="environment"
        // WebView failures that left the control looking "not enabled".
        openPickerOnButton: 'library'
    });
})();
