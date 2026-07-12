(function () {
    'use strict';

    var cfg = window.nbFeed || {};

    function tokenFrom(formId) {
        var form = document.getElementById(formId);
        if (!form) return null;
        var input = form.querySelector('input[name="__RequestVerificationToken"]');
        return input ? input.value : null;
    }

    function postAction(url, id, token) {
        var body = new URLSearchParams();
        body.append('id', String(id));
        if (token) {
            body.append('__RequestVerificationToken', token);
        }
        return fetch(url, {
            method: 'POST',
            headers: {
                'X-Requested-With': 'XMLHttpRequest',
                'Content-Type': 'application/x-www-form-urlencoded'
            },
            credentials: 'same-origin',
            body: body.toString()
        }).then(function (r) { return r.ok ? r.json() : Promise.reject(r.status); });
    }

    function applyToggle(btn, saved, labelSel) {
        btn.classList.toggle('is-on', saved);
        btn.setAttribute('aria-pressed', saved ? 'true' : 'false');
        var icon = btn.querySelector('i');
        if (icon) { icon.className = (saved ? 'fas' : 'far') + ' fa-bookmark'; }
        var label = btn.querySelector(labelSel);
        if (label && label.dataset.saved && label.dataset.save) {
            label.textContent = saved ? label.dataset.saved : label.dataset.save;
        }
    }

    function closeAllMenus() {
        document.querySelectorAll('.nb-menu-pop:not([hidden])').forEach(function (p) {
            p.setAttribute('hidden', 'hidden');
        });
        document.querySelectorAll('.nb-audience-pop:not([hidden])').forEach(function (p) {
            p.setAttribute('hidden', 'hidden');
        });
    }

    function toast(msg) {
        if (!msg) return;
        var t = document.createElement('div');
        t.className = 'nb-toast';
        t.textContent = msg;
        document.body.appendChild(t);
        requestAnimationFrame(function () { t.classList.add('is-on'); });
        setTimeout(function () {
            t.classList.remove('is-on');
            setTimeout(function () { t.remove(); }, 250);
        }, 1800);
    }

    function shareLink(url) {
        var abs = url;
        if (abs && abs.indexOf('http') !== 0) {
            abs = window.location.origin + url;
        }
        if (navigator.share) {
            navigator.share({ url: abs }).catch(function () { });
        } else if (navigator.clipboard) {
            navigator.clipboard.writeText(abs).then(function () { toast(cfg.copied); }).catch(function () { });
        }
    }

    document.addEventListener('click', function (e) {
        // Save post
        var saveBtn = e.target.closest('.nb-save');
        if (saveBtn) {
            e.preventDefault();
            var sform = document.getElementById('nbSaveForm');
            if (!sform) return;
            postAction(sform.getAttribute('action'), saveBtn.getAttribute('data-post-id'), tokenFrom('nbSaveForm'))
                .then(function (res) {
                    if (!res || !res.ok) return;
                    var row = saveBtn.closest('.nb-saved-row');
                    if (row && !res.saved) { row.remove(); return; }
                    applyToggle(saveBtn, res.saved, '.nb-save-label');
                })
                .catch(function () { });
            return;
        }

        // Save comment (tip)
        var csaveBtn = e.target.closest('.nb-comment-save');
        if (csaveBtn) {
            e.preventDefault();
            var cform = document.getElementById('nbSaveCommentForm');
            if (!cform) return;
            postAction(cform.getAttribute('action'), csaveBtn.getAttribute('data-comment-id'), tokenFrom('nbSaveCommentForm'))
                .then(function (res) { if (res && res.ok) applyToggle(csaveBtn, res.saved, '.nb-csave-label'); })
                .catch(function () { });
            return;
        }

        // Post overflow menu toggle
        var menuToggle = e.target.closest('[data-nb-menu-toggle]');
        if (menuToggle) {
            e.preventDefault();
            var pop = menuToggle.parentElement.querySelector('.nb-menu-pop');
            var wasHidden = pop.hasAttribute('hidden');
            closeAllMenus();
            if (wasHidden) { pop.removeAttribute('hidden'); }
            return;
        }

        // Audience selector toggle
        var audToggle = e.target.closest('[data-nb-audience-toggle]');
        if (audToggle) {
            e.preventDefault();
            var apop = audToggle.parentElement.querySelector('.nb-audience-pop');
            var aHidden = apop.hasAttribute('hidden');
            closeAllMenus();
            if (aHidden) { apop.removeAttribute('hidden'); }
            return;
        }

        var audItem = e.target.closest('.nb-audience-item');
        if (audItem) {
            e.preventDefault();
            var input = document.getElementById('nbAudienceInput');
            var lbl = document.getElementById('nbAudienceLabel');
            var ico = document.getElementById('nbAudienceIcon');
            if (input) input.value = audItem.getAttribute('data-code');
            if (lbl) lbl.textContent = audItem.getAttribute('data-label');
            if (ico) ico.className = 'fas ' + audItem.getAttribute('data-icon');
            closeAllMenus();
            return;
        }

        // Share
        var shareBtn = e.target.closest('.nb-share');
        if (shareBtn) {
            e.preventDefault();
            shareLink(shareBtn.getAttribute('data-share-url'));
            closeAllMenus();
            return;
        }

        // Delete post
        var delBtn = e.target.closest('.nb-delete');
        if (delBtn) {
            e.preventDefault();
            if (!window.confirm(cfg.deleteConfirm || 'Delete?')) { closeAllMenus(); return; }
            var dform = document.getElementById('nbDeleteForm');
            postAction(dform.getAttribute('action'), delBtn.getAttribute('data-post-id'), tokenFrom('nbDeleteForm'))
                .then(function (res) {
                    if (res && res.ok) {
                        var card = delBtn.closest('.nb-card');
                        if (card) { card.remove(); }
                        else { window.location.reload(); }
                    }
                })
                .catch(function () { });
            closeAllMenus();
            return;
        }

        // Report post
        var repBtn = e.target.closest('.nb-report');
        if (repBtn) {
            e.preventDefault();
            var rform = document.getElementById('nbReportForm');
            postAction(rform.getAttribute('action'), repBtn.getAttribute('data-post-id'), tokenFrom('nbReportForm'))
                .then(function () { toast(cfg.reportDone); })
                .catch(function () { });
            closeAllMenus();
            return;
        }

        // Reply to comment
        var replyBtn = e.target.closest('.nb-reply-btn');
        if (replyBtn) {
            e.preventDefault();
            var parentInput = document.getElementById('nbReplyParent');
            var hint = document.getElementById('nbReplyHint');
            var hintText = document.getElementById('nbReplyHintText');
            var input = document.getElementById('nbCommentInput');
            if (parentInput) parentInput.value = replyBtn.getAttribute('data-parent-id');
            if (hint && hintText) {
                hintText.textContent = (cfg.replyingTo || 'Replying to') + ' ' + replyBtn.getAttribute('data-author');
                hint.removeAttribute('hidden');
            }
            if (input) input.focus();
            return;
        }

        // Cancel reply
        var replyCancel = e.target.closest('#nbReplyCancel');
        if (replyCancel) {
            e.preventDefault();
            var pi = document.getElementById('nbReplyParent');
            var h = document.getElementById('nbReplyHint');
            if (pi) pi.value = '';
            if (h) h.setAttribute('hidden', 'hidden');
            return;
        }

        // Change ZIP
        var zipChange = e.target.closest('#nbZipChange');
        if (zipChange) {
            e.preventDefault();
            var current = zipChange.getAttribute('data-current') || '';
            var val = window.prompt(cfg.changeZipPrompt || 'Enter ZIP', current);
            if (val && /^\d{5}$/.test(val.trim())) {
                var t = val.trim();
                var base = window.location.pathname;
                window.location.href = base + '?zip=' + encodeURIComponent(t);
            }
            return;
        }

        // Clicking elsewhere closes menus
        if (!e.target.closest('.nb-menu') && !e.target.closest('.nb-audience')) {
            closeAllMenus();
        }
    });

    // Char counter
    var body = document.getElementById('nbBody');
    var counter = document.getElementById('nbCharCount');
    if (body && counter) {
        var update = function () { counter.textContent = String(body.value.length); };
        body.addEventListener('input', update);
        update();
    }

    // Scroll to top
    var scrollTop = document.getElementById('nbScrollTop');
    if (scrollTop) {
        window.addEventListener('scroll', function () {
            if (window.scrollY > 400) {
                scrollTop.removeAttribute('hidden');
            } else {
                scrollTop.setAttribute('hidden', 'hidden');
            }
        }, { passive: true });
        scrollTop.addEventListener('click', function () {
            window.scrollTo({ top: 0, behavior: 'smooth' });
        });
    }

    // Media picker + preview (photos/videos)
    var media = document.getElementById('nbMedia');
    if (media) {
        media.addEventListener('change', function () {
            var label = document.getElementById('nbMediaLabel');
            var preview = document.getElementById('nbMediaPreview');
            var files = media.files ? Array.prototype.slice.call(media.files, 0, 6) : [];
            if (label) {
                label.textContent = files.length
                    ? (files.length + (files.length === 1 ? ' file' : ' files'))
                    : (label.getAttribute('data-default') || label.textContent);
            }
            if (preview) {
                preview.innerHTML = '';
                files.forEach(function (f) {
                    var url = URL.createObjectURL(f);
                    var el;
                    if (f.type && f.type.indexOf('video/') === 0) {
                        el = document.createElement('video');
                        el.src = url;
                        el.muted = true;
                        el.className = 'nb-media-thumb';
                    } else {
                        el = document.createElement('img');
                        el.src = url;
                        el.className = 'nb-media-thumb';
                    }
                    preview.appendChild(el);
                });
            }
        });
    }
})();
