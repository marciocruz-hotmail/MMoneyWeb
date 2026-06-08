// MMoneyWeb — scripts de layout (vanilla JS, sem jQuery)
(function () {
    'use strict';

    var defaultLoadingMessage = 'Carregando...';
    var loadingDepth = 0;
    var bodyOverflowPrev = '';

    function getLoadingOverlay() {
        return document.getElementById('mmoney-loading');
    }

    function getLoadingMessageEl() {
        return document.getElementById('mmoney-loading-message');
    }

    function normalizeMessage(msg) {
        if (msg === undefined || msg === null) {
            return defaultLoadingMessage;
        }

        var text = String(msg).trim();
        return text.length > 0 ? text : defaultLoadingMessage;
    }

    function showLoading(msg) {
        var overlay = getLoadingOverlay();
        if (!overlay) {
            return;
        }

        var messageEl = getLoadingMessageEl();
        if (messageEl) {
            messageEl.textContent = normalizeMessage(msg);
        }

        loadingDepth++;
        if (loadingDepth === 1) {
            bodyOverflowPrev = document.body.style.overflow;
            document.body.style.overflow = 'hidden';
            overlay.classList.add('is-visible');
            overlay.setAttribute('aria-hidden', 'false');
        }
    }

    function hideLoading() {
        loadingDepth = 0;

        var overlay = getLoadingOverlay();
        if (!overlay) {
            document.body.style.removeProperty('overflow');
            return;
        }

        overlay.classList.remove('is-visible');
        overlay.setAttribute('aria-hidden', 'true');
        document.body.style.removeProperty('overflow');
    }

    window.mmoneyLoading = {
        show: showLoading,
        hide: hideLoading
    };

    function initLoadingForms() {
        document.addEventListener('submit', function (event) {
            var form = event.target;
            if (!(form instanceof HTMLFormElement)) {
                return;
            }

            var msg = form.getAttribute('data-mmoney-loading');
            if (!msg && (form.getAttribute('name') === 'login' || form.id === 'UserIdentity')) {
                msg = 'Validando credenciais...';
            }

            if (!msg) {
                return;
            }

            showLoading(msg);
        }, true);
    }

    function initSidebarScrollbar() {
        var sidebarWrapper = document.querySelector('.sidebar-wrapper');
        var isMobile = window.innerWidth <= 992;
        if (
            sidebarWrapper &&
            typeof OverlayScrollbarsGlobal !== 'undefined' &&
            OverlayScrollbarsGlobal.OverlayScrollbars &&
            !isMobile
        ) {
            OverlayScrollbarsGlobal.OverlayScrollbars(sidebarWrapper, {
                scrollbars: {
                    theme: 'os-theme-light',
                    autoHide: 'leave',
                    clickScroll: true
                }
            });
        }
    }

    function resetLoadingState() {
        hideLoading();
    }

    function isLoginPage() {
        return document.querySelector('.login-page') !== null;
    }

    function removeSkipLinks() {
        document.querySelectorAll('.skip-links').forEach(function (el) {
            el.remove();
        });
    }

    function focusLoginEmailField() {
        if (!isLoginPage()) {
            return;
        }

        var email = document.getElementById('Input.Email');
        if (!email || typeof email.focus !== 'function') {
            return;
        }

        email.focus({ preventScroll: true });
    }

    function initLayoutAfterNavigation() {
        resetLoadingState();
        removeSkipLinks();
        if (isLoginPage()) {
            focusLoginEmailField();
            requestAnimationFrame(focusLoginEmailField);
        } else {
            initSidebarScrollbar();
        }
    }

    function initModalKeyboard() {
        if (window.mmoneyModalKeyboard && typeof window.mmoneyModalKeyboard.initGlobal === 'function') {
            window.mmoneyModalKeyboard.initGlobal();
        }
    }

    function init() {
        initLayoutAfterNavigation();
        initLoadingForms();
        initBlazorErrorSwal();
        initModalKeyboard();
    }

    document.addEventListener('DOMContentLoaded', init);
    window.addEventListener('pageshow', resetLoadingState);
    document.addEventListener('blazor:enhanced:load', function () {
        initLayoutAfterNavigation();
        initModalKeyboard();
    });

    function initBlazorErrorSwal() {
        var errorUi = document.getElementById('blazor-error-ui');
        if (!errorUi || typeof LibMessageError !== 'function') {
            return;
        }

        var notificado = false;

        function tentarNotificar() {
            if (notificado) {
                return;
            }

            var visivel = window.getComputedStyle(errorUi).display !== 'none';
            if (!visivel) {
                return;
            }

            notificado = true;
            var trace = 'Ocorreu um erro não tratado no circuito Blazor.\n\nRecarregue a página se o problema persistir.';
            LibMessageError('Atenção', '<pre class="mmoney-swal-trace">' + trace.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;') + '</pre>');
            errorUi.style.display = 'none';
        }

        var observer = new MutationObserver(tentarNotificar);
        observer.observe(errorUi, { attributes: true, attributeFilter: ['style', 'class'] });
        tentarNotificar();
    }
})();
