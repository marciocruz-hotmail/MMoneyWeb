using Microsoft.EntityFrameworkCore;
using MMoneyWeb.Web.Data;
using MMoneyWeb.Web.Domain;
using MMoneyWeb.Web.Services;
using MMoneyWeb.Web.Services.Models;

namespace MMoneyWeb.Tests;
public class LancamentosViewServiceTests
{
    [Fact]
    public void AplicarDefaultDataPagamentoNovoLiquidado_UsaDataVencimentoQuandoNovoLiquidadoSemPagamento()
    {
        var vencimento = new DateOnly(2026, 6, 15);
        var form = new LancamentoFormModel
        {
            IdLancamento = 0,
            IdStatus = LancamentosViewService.StatusQuitado,
            DataVencimento = vencimento
        };

        LancamentosServiceBase.UiAccess.AplicarDefaultDataPagamentoNovoLiquidado(form);

        Assert.Equal(vencimento, form.DataPagamento);
    }

    [Fact]
    public void AplicarDefaultDataPagamentoNovoLiquidado_NaoAlteraQuandoPagamentoInformado()
    {
        var pagamento = new DateOnly(2026, 6, 10);
        var form = new LancamentoFormModel
        {
            IdLancamento = 0,
            IdStatus = LancamentosViewService.StatusQuitado,
            DataVencimento = new DateOnly(2026, 6, 15),
            DataPagamento = pagamento
        };

        LancamentosServiceBase.UiAccess.AplicarDefaultDataPagamentoNovoLiquidado(form);

        Assert.Equal(pagamento, form.DataPagamento);
    }

    [Fact]
    public void AplicarDefaultDataPagamentoNovoLiquidado_NaoAlteraEdicao()
    {
        var form = new LancamentoFormModel
        {
            IdLancamento = 42,
            IdStatus = LancamentosViewService.StatusQuitado,
            DataVencimento = new DateOnly(2026, 6, 15)
        };

        LancamentosServiceBase.UiAccess.AplicarDefaultDataPagamentoNovoLiquidado(form);

        Assert.Null(form.DataPagamento);
    }

    [Fact]
    public void AplicarDataPagamentoNovoAoAlterarVencimento_CopiaVencimentoParaPagamento()
    {
        var vencimento = new DateOnly(2026, 6, 20);
        var form = new LancamentoFormModel
        {
            IdLancamento = 0,
            DataVencimento = vencimento
        };

        LancamentosServiceBase.UiAccess.AplicarDataPagamentoNovoAoAlterarVencimento(form);

        Assert.Equal(vencimento, form.DataPagamento);
    }

    [Fact]
    public void AplicarDataPagamentoAoAlterarVencimento_CopiaVencimentoParaPagamentoEmEdicao()
    {
        var vencimento = new DateOnly(2026, 6, 20);
        var form = new LancamentoFormModel
        {
            IdLancamento = 10,
            DataVencimento = vencimento,
            DataPagamento = new DateOnly(2026, 6, 1)
        };

        LancamentosServiceBase.UiAccess.AplicarDataPagamentoAoAlterarVencimento(form);

        Assert.Equal(vencimento, form.DataPagamento);
    }

    [Fact]
    public void AplicarParcelasDefaultQuandoVazias_DefineUmQuandoNuloOuZero()
    {
        var form = new LancamentoFormModel
        {
            IdLancamento = 5,
            NumParcelaAtual = null,
            NumParcelaTotal = 0
        };

        LancamentosServiceBase.UiAccess.AplicarParcelasDefaultQuandoVazias(form);

        Assert.Equal(1, form.NumParcelaAtual);
        Assert.Equal(1, form.NumParcelaTotal);
    }

    [Fact]
    public void AplicarParcelasDefaultQuandoVazias_NaoAlteraQuandoFixo()
    {
        var form = new LancamentoFormModel
        {
            Fixo = true,
            NumParcelaAtual = null,
            NumParcelaTotal = null
        };

        LancamentosServiceBase.UiAccess.AplicarParcelasDefaultQuandoVazias(form);

        Assert.Null(form.NumParcelaAtual);
        Assert.Null(form.NumParcelaTotal);
    }

    [Fact]
    public void AplicarParcelasDefaultQuandoVazias_NaoAlteraQuandoInformado()
    {
        var form = new LancamentoFormModel
        {
            NumParcelaAtual = 2,
            NumParcelaTotal = 6
        };

        LancamentosServiceBase.UiAccess.AplicarParcelasDefaultQuandoVazias(form);

        Assert.Equal(2, form.NumParcelaAtual);
        Assert.Equal(6, form.NumParcelaTotal);
    }

    [Fact]
    public void AplicarParcelasFixoAntesSalvar_ZeraParcelasQuandoFixo()
    {
        var form = new LancamentoFormModel
        {
            Fixo = true,
            NumParcelaAtual = 1,
            NumParcelaTotal = 1
        };

        LancamentosServiceBase.UiAccess.AplicarParcelasFixoAntesSalvar(form);

        Assert.Equal(0, form.NumParcelaAtual);
        Assert.Equal(0, form.NumParcelaTotal);
    }

    [Fact]
    public void AplicarParcelasFixoAntesSalvar_NaoAlteraQuandoNaoFixo()
    {
        var form = new LancamentoFormModel
        {
            Fixo = false,
            NumParcelaAtual = 2,
            NumParcelaTotal = 6
        };

        LancamentosServiceBase.UiAccess.AplicarParcelasFixoAntesSalvar(form);

        Assert.Equal(2, form.NumParcelaAtual);
        Assert.Equal(6, form.NumParcelaTotal);
    }

    [Fact]
    public async Task CriarModeloNovoAsync_IniciaParcelasComUm()
    {
        var service = new LancamentosViewService(new NullDbContextFactory());
        var form = await service.CriarModeloNovoAsync(10, 20);

        Assert.Equal(1, form.NumParcelaAtual);
        Assert.Equal(1, form.NumParcelaTotal);
    }

    [Fact]
    public void MontarLinhasComSaldo_CalculaSaldoAcumuladoNaLinhaDoTempo()
    {
        var lancamentos = new List<Lancamento>
        {
            new() { IdLancamento = 1, IdTipo = LancamentosViewService.TipoReceber, IdStatus = LancamentosViewService.StatusQuitado, Valor = 100m, Fixo = 0, NumParcelaAtual = 1, NumParcelaTotal = 1 },
            new() { IdLancamento = 2, IdTipo = LancamentosViewService.TipoPagar, IdStatus = LancamentosViewService.StatusAberto, Valor = -30m, Fixo = 0, NumParcelaAtual = 1, NumParcelaTotal = 1 },
            new() { IdLancamento = 3, IdTipo = LancamentosViewService.TipoReceber, IdStatus = LancamentosViewService.StatusAgendado, Valor = 50m, Fixo = 1 }
        };

        var linhas = LancamentosServiceBase.UiAccess.MontarLinhasComSaldo(lancamentos);

        Assert.Equal(3, linhas.Count);
        Assert.Equal(100m, linhas[0].Saldo);
        Assert.Equal(70m, linhas[1].Saldo);
        Assert.Equal(120m, linhas[2].Saldo);
        Assert.Equal("C", linhas[0].DebitoCredito);
        Assert.Equal("D", linhas[1].DebitoCredito);
        Assert.Equal("Fixo", linhas[2].Parcela);
    }

    private sealed class NullDbContextFactory : IDbContextFactory<MMoneyDbContext>
    {
        public MMoneyDbContext CreateDbContext() =>
            throw new NotSupportedException("Factory de teste não cria DbContext.");

        public Task<MMoneyDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("Factory de teste não cria DbContext.");
    }
}
