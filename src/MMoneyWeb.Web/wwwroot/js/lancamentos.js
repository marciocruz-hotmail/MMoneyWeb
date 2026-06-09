// MMoneyWeb — preferências da tela de Lançamentos (conta + competência)
(function () {
    'use strict';

    var CONTA_KEY = 'mmoney.lancamentos.contaId';
    var COMPETENCIA_KEY = 'mmoney.lancamentos.competenciaId';

    window.mmoneyLancamentosPrefs = {
        get: function () {
            return {
                contaId: localStorage.getItem(CONTA_KEY) || '',
                competenciaId: localStorage.getItem(COMPETENCIA_KEY) || ''
            };
        },
        set: function (contaId, competenciaId) {
            if (contaId) {
                localStorage.setItem(CONTA_KEY, String(contaId));
            } else {
                localStorage.removeItem(CONTA_KEY);
            }

            if (competenciaId) {
                localStorage.setItem(COMPETENCIA_KEY, String(competenciaId));
            } else {
                localStorage.removeItem(COMPETENCIA_KEY);
            }
        }
    };
})();
