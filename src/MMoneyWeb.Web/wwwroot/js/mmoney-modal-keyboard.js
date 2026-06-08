/**
 * MMoneyWeb — teclado global em modais Bootstrap (padrão GDI ERP).
 *
 * Modais: .modal.show (Bootstrap)
 * Opt-out: data-mmoney-modal-keyboard="false"
 * Salvar (Enter no último campo): [data-mmoney-submit] → .modal-footer .btn-success → .modal-footer .btn-primary
 */
(function () {
    'use strict';

    var attached = new Map();

    function getModalRoot(modal) {
        return modal.querySelector('form') ||
            modal.querySelector('.modal-content') ||
            modal;
    }

    function isKeyboardEnabled(modal) {
        return modal.getAttribute('data-mmoney-modal-keyboard') !== 'false';
    }

    function getFocusableFields(root) {
        return Array.from(root.querySelectorAll(
            'input:not([disabled]):not([readonly]):not([type="hidden"]), ' +
            'select:not([disabled]), textarea:not([disabled])'
        )).filter(function (el) {
            return el.tabIndex !== -1 &&
                el.type !== 'button' &&
                el.type !== 'submit' &&
                el.offsetParent !== null;
        });
    }

    function focusFirstField(root) {
        var fields = getFocusableFields(root);
        if (fields.length === 0) {
            return;
        }

        fields[0].focus();

        if (typeof fields[0].select === 'function' &&
            fields[0].type !== 'checkbox' &&
            fields[0].type !== 'radio' &&
            fields[0].tagName !== 'SELECT') {
            fields[0].select();
        }
    }

    function moveToNextField(root, current) {
        var fields = getFocusableFields(root);
        var index = fields.indexOf(current);
        if (index === -1 || index >= fields.length - 1) {
            return false;
        }

        var next = fields[index + 1];
        next.focus();

        if (typeof next.select === 'function' &&
            next.type !== 'checkbox' &&
            next.type !== 'radio' &&
            next.tagName !== 'SELECT') {
            next.select();
        }

        return true;
    }

    function findSubmitButton(modal, root) {
        return root.querySelector('[data-mmoney-submit]:not([disabled])') ||
            modal.querySelector('.modal-footer [data-mmoney-submit]:not([disabled])') ||
            modal.querySelector('.modal-footer .btn-success:not([disabled])') ||
            modal.querySelector('.modal-footer button.btn-primary:not([disabled])');
    }

    function triggerSubmit(modal, root) {
        var btn = findSubmitButton(modal, root);
        if (btn && typeof btn.click === 'function') {
            btn.click();
        }
    }

    function attach(modal) {
        if (!modal || attached.has(modal) || !isKeyboardEnabled(modal)) {
            return;
        }

        var root = getModalRoot(modal);
        if (!root) {
            return;
        }

        var onKeyDown = function (event) {
            if (event.key !== 'Enter') {
                return;
            }

            var target = event.target;
            if (!(target instanceof HTMLElement) || !root.contains(target)) {
                return;
            }

            if (target.tagName === 'TEXTAREA') {
                return;
            }

            var fields = getFocusableFields(root);
            var index = fields.indexOf(target);
            if (index === -1) {
                return;
            }

            event.preventDefault();

            if (typeof target.blur === 'function') {
                target.blur();
            }

            if (index === fields.length - 1) {
                window.setTimeout(function () {
                    triggerSubmit(modal, root);
                }, 0);
                return;
            }

            window.setTimeout(function () {
                moveToNextField(root, target);
            }, 0);
        };

        root.addEventListener('keydown', onKeyDown);
        attached.set(modal, { root: root, onKeyDown: onKeyDown });

        requestAnimationFrame(function () {
            if (document.contains(modal)) {
                focusFirstField(root);
            }
        });
    }

    function detach(modal) {
        var entry = attached.get(modal);
        if (!entry) {
            return;
        }

        entry.root.removeEventListener('keydown', entry.onKeyDown);
        attached.delete(modal);
    }

    function scan() {
        attached.forEach(function (_entry, modal) {
            if (!document.contains(modal)) {
                detach(modal);
            }
        });

        document.querySelectorAll('.modal.show, .modal.fade.show.d-block').forEach(function (modal) {
            attach(modal);
        });
    }

    window.mmoneyModalKeyboard = {
        initGlobal: function () {
            if (window.mmoneyModalKeyboard._initialized) {
                scan();
                return;
            }

            window.mmoneyModalKeyboard._initialized = true;
            scan();

            var observer = new MutationObserver(function () {
                scan();
            });

            observer.observe(document.body, { childList: true, subtree: true });
            document.addEventListener('blazor:enhanced:load', scan);
        },

        scan: scan,

        attach: attach,

        detach: detach
    };
})();
