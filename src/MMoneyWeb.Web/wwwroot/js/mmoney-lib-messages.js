/**
 * Mensagens globais MMoneyWeb — padrão GDI ERP (SweetAlert2 + GdiSwalCompat).
 * Pré-requisitos: sweetalert2.min.js, gdi-swal2-dialog-shim.js
 */
(function () {
  'use strict';

  function _gdiSwalAlert(icon, title, msg, opcoes) {
    try {
      if (typeof GdiSwalCompat !== 'undefined' && GdiSwalCompat && typeof GdiSwalCompat.alert === 'function') {
        var cfg = { title: title, message: msg, icon: icon };
        if (opcoes && typeof opcoes === 'object') {
          for (var k in opcoes) {
            if (Object.prototype.hasOwnProperty.call(opcoes, k)) cfg[k] = opcoes[k];
          }
        }
        return GdiSwalCompat.alert(cfg);
      }
      alert((title ? title + '\n\n' : '') + (msg || ''));
    } catch (err) {
      alert('[_gdiSwalAlert] ' + (err && err.message ? err.message.toString() : String(err)));
    }
  }

  window.LibMessageAlert = function (title, msg, opcoes) {
    _gdiSwalAlert('warning', title, msg, opcoes);
  };

  window.LibMessageSuccess = function (title, msg, opcoes) {
    _gdiSwalAlert('success', title, msg, opcoes);
  };

  window.LibMessageError = function (title, msg, opcoes) {
    var cfg = opcoes || {};
    if (!cfg.size) cfg.size = 'large';
    _gdiSwalAlert('error', title, msg, cfg);
  };

  window.LibMessageHideAll = function () {
    try {
      if (typeof GdiSwalCompat !== 'undefined' && GdiSwalCompat && typeof GdiSwalCompat.hideAll === 'function') {
        GdiSwalCompat.hideAll();
      } else if (typeof Swal !== 'undefined' && typeof Swal.close === 'function') {
        Swal.close();
      }
    } catch (err) {
      alert('[LibMessageHideAll] ' + (err && err.message ? err.message.toString() : String(err)));
    }
  };

  window.LibMessageProcessando = function (msg) {
    try {
      if (typeof Swal === 'undefined') return;
      Swal.fire({
        title: msg || 'Processando . . .',
        allowOutsideClick: false,
        allowEscapeKey: false,
        showConfirmButton: false,
        didOpen: function () {
          Swal.showLoading();
        }
      });
    } catch (err) {
      alert('[LibMessageProcessando] ' + (err && err.message ? err.message.toString() : String(err)));
    }
  };

  window.LibMessageProcessandoHide = function () {
    try {
      if (typeof Swal !== 'undefined' && typeof Swal.close === 'function') {
        Swal.close();
      }
    } catch (err) {
      alert('[LibMessageProcessandoHide] ' + (err && err.message ? err.message.toString() : String(err)));
    }
  };

  window.mmoneyLibMessages = {
    alert: function (title, msg) { LibMessageAlert(title, msg); },
    success: function (title, msg) { LibMessageSuccess(title, msg); },
    error: function (title, msg) { LibMessageError(title, msg); },
    hideAll: function () { LibMessageHideAll(); },
    processando: function (msg) { LibMessageProcessando(msg); },
    processandoHide: function () { LibMessageProcessandoHide(); }
  };
})();
