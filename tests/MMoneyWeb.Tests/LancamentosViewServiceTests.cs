using MMoneyWeb.Web.Domain;
using MMoneyWeb.Web.Services;

namespace MMoneyWeb.Tests;

public class LancamentosViewServiceTests
{
    [Fact]
    public void MontarLinhasComSaldo_CalculaSaldoAcumuladoNaLinhaDoTempo()
    {
        var lancamentos = new List<Lancamento>
        {
            new() { IdLancamento = 1, IdTipo = LancamentosViewService.TipoReceber, IdStatus = LancamentosViewService.StatusQuitado, Valor = 100m, Fixo = 0, NumParcelaAtual = 1, NumParcelaTotal = 1 },
            new() { IdLancamento = 2, IdTipo = LancamentosViewService.TipoPagar, IdStatus = LancamentosViewService.StatusAberto, Valor = -30m, Fixo = 0, NumParcelaAtual = 1, NumParcelaTotal = 1 },
            new() { IdLancamento = 3, IdTipo = LancamentosViewService.TipoReceber, IdStatus = LancamentosViewService.StatusAgendado, Valor = 50m, Fixo = 1 }
        };

        var linhas = LancamentosViewService.MontarLinhasComSaldo(lancamentos);

        Assert.Equal(3, linhas.Count);
        Assert.Equal(100m, linhas[0].Saldo);
        Assert.Equal(70m, linhas[1].Saldo);
        Assert.Equal(120m, linhas[2].Saldo);
        Assert.Equal("C", linhas[0].DebitoCredito);
        Assert.Equal("D", linhas[1].DebitoCredito);
        Assert.Equal("Fixo", linhas[2].Parcela);
    }
}
