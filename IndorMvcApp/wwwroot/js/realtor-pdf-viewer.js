/**
 * WebView-safe PDF.js bootstrapping for Realtor report/document viewers.
 * Prefers fetch→ArrayBuffer (avoids range/stream quirks) and local worker assets.
 */
(function (global) {
    'use strict';

    function format(template, a, b) {
        return String(template || '')
            .replace('{0}', a)
            .replace('{1}', b == null ? '' : b);
    }

    function boot(options) {
        var viewer = options.viewer || document.getElementById('rlReportViewer');
        var canvas = options.canvas || document.getElementById('rlReportCanvas');
        var loading = options.loading || document.getElementById('rlReportLoading');
        var errorEl = options.errorEl || document.getElementById('rlReportError');
        var toolbar = options.toolbar || document.getElementById('rlReportToolbar');
        var prevBtn = options.prevBtn || document.getElementById('rlReportPrev');
        var nextBtn = options.nextBtn || document.getElementById('rlReportNext');
        var indicator = options.indicator || document.getElementById('rlReportPageIndicator');
        var titleLabel = options.titleLabel || document.getElementById('rlReportPageLabel');

        var pdfUrl = options.pdfUrl || (viewer && viewer.getAttribute('data-pdf-url')) || '';
        var workerSrc = options.workerSrc || '/lib/pdfjs/pdf.worker.min.js';
        var renderAll = !!options.renderAll;
        var startPage = parseInt(options.startPage, 10);
        if (!Number.isFinite(startPage) || startPage < 1) startPage = 1;

        var pageLabelTemplate = options.pageLabelTemplate || 'Page {0}';
        var ofTemplate = options.ofTemplate || 'Page {0} of {1}';

        var pdfDoc = null;
        var currentPage = startPage;
        var renderToken = 0;
        var resizeTimer = null;

        function showError() {
            if (loading) loading.hidden = true;
            if (canvas) canvas.hidden = true;
            if (errorEl) errorEl.hidden = false;
        }

        function updateChrome(pageNum, pageCount) {
            if (indicator) {
                indicator.textContent = format(ofTemplate, pageNum, pageCount);
            }
            if (titleLabel) {
                titleLabel.textContent = renderAll
                    ? format(ofTemplate, pageCount, pageCount)
                    : format(pageLabelTemplate, pageNum);
            }
            if (prevBtn) prevBtn.disabled = renderAll || pageNum <= 1;
            if (nextBtn) nextBtn.disabled = renderAll || pageNum >= pageCount;
            if (toolbar) toolbar.hidden = renderAll;
        }

        function pageMaxWidth() {
            return Math.max(280, ((viewer && viewer.clientWidth) || window.innerWidth) - 8);
        }

        function paintPage(page, targetCanvas) {
            var maxWidth = pageMaxWidth();
            var unscaled = page.getViewport({ scale: 1 });
            var scale = Math.min(2.5, maxWidth / unscaled.width);
            var viewport = page.getViewport({ scale: scale });
            var outputScale = window.devicePixelRatio || 1;
            targetCanvas.width = Math.floor(viewport.width * outputScale);
            targetCanvas.height = Math.floor(viewport.height * outputScale);
            targetCanvas.style.width = Math.floor(viewport.width) + 'px';
            targetCanvas.style.height = Math.floor(viewport.height) + 'px';
            var context = targetCanvas.getContext('2d');
            var transform = outputScale !== 1 ? [outputScale, 0, 0, outputScale, 0, 0] : null;
            return page.render({
                canvasContext: context,
                viewport: viewport,
                transform: transform
            }).promise;
        }

        function renderAllPages(token) {
            if (!pdfDoc || !viewer) return Promise.resolve();
            var pageCount = pdfDoc.numPages || 1;
            var host = document.getElementById('rlReportAllPages');
            if (!host) {
                host = document.createElement('div');
                host.id = 'rlReportAllPages';
                host.className = 'rl-insp-report-all-pages';
                viewer.appendChild(host);
            }
            host.innerHTML = '';
            if (canvas) canvas.hidden = true;

            var chain = Promise.resolve();
            for (var i = 1; i <= pageCount; i++) {
                (function (pageNum) {
                    chain = chain.then(function () {
                        if (token !== renderToken) return;
                        return pdfDoc.getPage(pageNum).then(function (page) {
                            if (token !== renderToken) return;
                            var pageCanvas = document.createElement('canvas');
                            pageCanvas.className = 'rl-insp-report-canvas';
                            host.appendChild(pageCanvas);
                            return paintPage(page, pageCanvas);
                        });
                    });
                })(i);
            }

            return chain.then(function () {
                if (token !== renderToken) return;
                if (loading) loading.hidden = true;
                updateChrome(1, pageCount);
            });
        }

        function renderPage(pageNum) {
            if (!pdfDoc) return;
            var pageCount = pdfDoc.numPages || 1;
            var token = ++renderToken;

            if (loading) loading.hidden = false;
            if (errorEl) errorEl.hidden = true;

            if (renderAll) {
                renderAllPages(token).catch(function () {
                    if (token === renderToken) showError();
                });
                return;
            }

            if (!canvas) return;
            pageNum = parseInt(pageNum, 10);
            if (!Number.isFinite(pageNum) || pageNum < 1) pageNum = 1;
            if (pageNum > pageCount) pageNum = pageCount;
            currentPage = pageNum;

            pdfDoc.getPage(pageNum).then(function (page) {
                if (token !== renderToken) return;
                return paintPage(page, canvas).then(function () {
                    if (token !== renderToken) return;
                    if (loading) loading.hidden = true;
                    canvas.hidden = false;
                    updateChrome(pageNum, pageCount);
                });
            }).catch(function () {
                if (token === renderToken) showError();
            });
        }

        function openWithData(data) {
            return global.pdfjsLib.getDocument({
                data: data,
                disableRange: true,
                disableStream: true,
                disableAutoFetch: true
            }).promise;
        }

        function openWithUrl(url) {
            return global.pdfjsLib.getDocument({
                url: url,
                withCredentials: true,
                disableRange: true,
                disableStream: true,
                disableAutoFetch: true
            }).promise;
        }

        function loadDocument() {
            if (!pdfUrl) {
                return Promise.reject(new Error('Missing PDF URL'));
            }

            return fetch(pdfUrl, {
                credentials: 'same-origin',
                cache: 'no-store',
                headers: { 'Accept': 'application/pdf,*/*' }
            }).then(function (response) {
                if (!response.ok) {
                    throw new Error('HTTP ' + response.status);
                }
                return response.arrayBuffer();
            }).then(openWithData).catch(function () {
                return openWithUrl(pdfUrl);
            });
        }

        if (!global.pdfjsLib) {
            showError();
            return;
        }

        global.pdfjsLib.GlobalWorkerOptions.workerSrc = workerSrc;

        loadDocument()
            .then(function (doc) {
                pdfDoc = doc;
                renderPage(startPage);
            })
            .catch(function () {
                showError();
            });

        if (prevBtn) {
            prevBtn.addEventListener('click', function () {
                if (!renderAll && currentPage > 1) renderPage(currentPage - 1);
            });
        }
        if (nextBtn) {
            nextBtn.addEventListener('click', function () {
                if (!renderAll && pdfDoc && currentPage < pdfDoc.numPages) renderPage(currentPage + 1);
            });
        }

        global.addEventListener('resize', function () {
            if (!pdfDoc) return;
            global.clearTimeout(resizeTimer);
            resizeTimer = global.setTimeout(function () {
                renderPage(renderAll ? 1 : currentPage);
            }, 150);
        });
    }

    global.IndorRealtorPdfViewer = { boot: boot };
})(window);
