using Microsoft.EntityFrameworkCore;
using MMoneyWeb.Web.Data;
using MMoneyWeb.Web.Domain;
using MMoneyWeb.Web.Services.Models;

namespace MMoneyWeb.Web.Services;

public sealed class LancamentosViewService(IDbContextFactory<MMoneyDbContext> dbContextFactory)
{
    public const short TipoPagar = 1;
    public const short TipoReceber = 2;

    public const short StatusAberto = 1;
    public const short StatusAtrasado = 2;
    public const short StatusAgendado = 3;
    public const short StatusQuitado = 4;

    public async Task<IReadOnlyList<ContaCorrente>> ObterContasAtivasAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await db.ContasCorrentes
            .AsNoTracking()
            .Where(c => c.Ativo == 1)
            .OrderBy(c => c.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Competencia>> ObterCompetenciasAtivasAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await db.Competencias
            .AsNoTracking()
            .Where(c => c.Ativo == 1)
            .OrderBy(c => c.AnoMes)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<LancamentoLinhaViewModel>> ObterLancamentosAsync(
        int idContaCorrente,
        int idCompetencia,
        CancellationToken cancellationToken = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var lancamentos = await db.Lancamentos
            .AsNoTracking()
            .Where(l => l.IdContaCorrente == idContaCorrente && l.IdCompetencia == idCompetencia)
            .OrderByDescending(l => l.IdStatus)
            .ThenBy(l => l.DataVencimento)
            .ThenBy(l => l.Ordem)
            .ToListAsync(cancellationToken);

        return MontarLinhasComSaldo(lancamentos);
    }

    public static IReadOnlyList<LancamentoLinhaViewModel> MontarLinhasComSaldo(IReadOnlyList<Lancamento> lancamentos)
    {
        var linhas = new List<LancamentoLinhaViewModel>(lancamentos.Count);
        decimal? saldoAnterior = null;

        foreach (var lancamento in lancamentos)
        {
            var valor = lancamento.Valor ?? 0m;
            var saldo = saldoAnterior is null ? valor : saldoAnterior.Value + valor;
            saldoAnterior = saldo;

            var isDebito = lancamento.IdTipo == TipoPagar;
            var status = lancamento.IdStatus ?? StatusAberto;

            linhas.Add(new LancamentoLinhaViewModel
            {
                IdLancamento = lancamento.IdLancamento,
                DataVencimento = lancamento.DataVencimento,
                DebitoCredito = isDebito ? "D" : "C",
                Parcela = FormatarParcela(lancamento),
                Descricao = lancamento.Descricao?.Trim() ?? "",
                Valor = valor,
                Saldo = saldo,
                IdStatus = status,
                IsDebito = isDebito,
                StatusCssClass = ObterClasseStatus(status)
            });
        }

        return linhas;
    }

    internal static string FormatarParcela(Lancamento lancamento)
    {
        if (lancamento.Fixo == 1)
        {
            return "Fixo";
        }

        if (lancamento.NumParcelaAtual is > 0 && lancamento.NumParcelaTotal is > 0)
        {
            return $"{lancamento.NumParcelaAtual}/{lancamento.NumParcelaTotal}";
        }

        return "1/1";
    }

    internal static string ObterClasseStatus(short idStatus) => idStatus switch
    {
        StatusQuitado => "mmoney-lanc-row-quitado",
        StatusAberto => "mmoney-lanc-row-aberto",
        StatusAgendado => "mmoney-lanc-row-agendado",
        StatusAtrasado => "mmoney-lanc-row-atrasado",
        _ => "mmoney-lanc-row-aberto"
    };

    public async Task<LancamentoLookupsModel> ObterLookupsModalAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        return new LancamentoLookupsModel
        {
            Contas = await db.ContasCorrentes.AsNoTracking().Where(c => c.Ativo == 1).OrderBy(c => c.Nome).ToListAsync(cancellationToken),
            Competencias = await db.Competencias.AsNoTracking().Where(c => c.Ativo == 1).OrderBy(c => c.AnoMes).ToListAsync(cancellationToken),
            Categorias = await db.Categorias.AsNoTracking().OrderBy(c => c.Nome).ToListAsync(cancellationToken),
            Cartoes = await db.CartoesCredito.AsNoTracking().Where(c => c.Ativo == 1).OrderBy(c => c.Nome).ToListAsync(cancellationToken),
            Status = await db.LancamentosStatus.AsNoTracking().OrderBy(s => s.IdStatus).ToListAsync(cancellationToken),
            Tipos = await db.LancamentosTipos.AsNoTracking().OrderBy(t => t.IdTipo).ToListAsync(cancellationToken)
        };
    }

    public Task<LancamentoFormModel> CriarModeloNovoAsync(int idContaCorrente, int idCompetencia) =>
        Task.FromResult(new LancamentoFormModel
        {
            IdContaCorrente = idContaCorrente,
            IdCompetencia = idCompetencia,
            DataVencimento = DateOnly.FromDateTime(DateTime.Today),
            DataLancamento = DateOnly.FromDateTime(DateTime.Today),
            IdTipo = TipoPagar,
            IdStatus = StatusAberto
        });

    public async Task<LancamentoFormModel?> ObterLancamentoParaEdicaoAsync(int idLancamento, CancellationToken cancellationToken = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var lancamento = await db.Lancamentos.AsNoTracking().FirstOrDefaultAsync(l => l.IdLancamento == idLancamento, cancellationToken);
        if (lancamento is null)
        {
            return null;
        }

        return MapearParaForm(lancamento);
    }

    public async Task<int> SalvarLancamentoAsync(LancamentoFormModel form, CancellationToken cancellationToken = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var valorComSinal = AplicarSinalAoValor(form.ValorAbsoluto, form.IdTipo);

        if (form.IsNovo)
        {
            var novo = new Lancamento
            {
                Ordem = form.Ordem,
                NumParcelaAtual = form.NumParcelaAtual,
                NumParcelaTotal = form.NumParcelaTotal,
                DataLancamento = form.DataLancamento ?? DateOnly.FromDateTime(DateTime.Today),
                DataVencimento = form.DataVencimento,
                DataPagamento = form.DataPagamento,
                IdCompetencia = form.IdCompetencia,
                IdContaCorrente = form.IdContaCorrente,
                Descricao = form.Descricao.Trim(),
                Valor = valorComSinal,
                IdCategoria = form.IdCategoria,
                DeduzIr = (short)(form.DeduzIr ? 1 : 0),
                Fixo = (short)(form.Fixo ? 1 : 0),
                IdCartaoCredito = form.IdCartaoCredito > 0 ? form.IdCartaoCredito : null,
                IdLancamentoPai = form.IdLancamentoPai > 0 ? form.IdLancamentoPai : null,
                IdStatus = form.IdStatus,
                IdTipo = form.IdTipo,
                CodigoBarras = string.IsNullOrWhiteSpace(form.CodigoBarras) ? null : form.CodigoBarras.Trim(),
                Obs = string.IsNullOrWhiteSpace(form.Obs) ? null : form.Obs.Trim(),
                Copiado = 0
            };

            db.Lancamentos.Add(novo);
            await db.SaveChangesAsync(cancellationToken);
            return novo.IdLancamento;
        }

        var existente = await db.Lancamentos.FirstOrDefaultAsync(l => l.IdLancamento == form.IdLancamento, cancellationToken)
            ?? throw new InvalidOperationException($"Lançamento {form.IdLancamento} não encontrado.");

        existente.Ordem = form.Ordem;
        existente.NumParcelaAtual = form.NumParcelaAtual;
        existente.NumParcelaTotal = form.NumParcelaTotal;
        existente.DataLancamento = form.DataLancamento;
        existente.DataVencimento = form.DataVencimento;
        existente.DataPagamento = form.DataPagamento;
        existente.IdCompetencia = form.IdCompetencia;
        existente.IdContaCorrente = form.IdContaCorrente;
        existente.Descricao = form.Descricao.Trim();
        existente.Valor = valorComSinal;
        existente.IdCategoria = form.IdCategoria;
        existente.DeduzIr = (short)(form.DeduzIr ? 1 : 0);
        existente.Fixo = (short)(form.Fixo ? 1 : 0);
        existente.IdCartaoCredito = form.IdCartaoCredito > 0 ? form.IdCartaoCredito : null;
        existente.IdLancamentoPai = form.IdLancamentoPai > 0 ? form.IdLancamentoPai : null;
        existente.IdStatus = form.IdStatus;
        existente.IdTipo = form.IdTipo;
        existente.CodigoBarras = string.IsNullOrWhiteSpace(form.CodigoBarras) ? null : form.CodigoBarras.Trim();
        existente.Obs = string.IsNullOrWhiteSpace(form.Obs) ? null : form.Obs.Trim();

        await db.SaveChangesAsync(cancellationToken);
        return existente.IdLancamento;
    }

    private static LancamentoFormModel MapearParaForm(Lancamento lancamento) => new()
    {
        IdLancamento = lancamento.IdLancamento,
        Ordem = lancamento.Ordem ?? 0,
        Fixo = lancamento.Fixo == 1,
        DeduzIr = lancamento.DeduzIr == 1,
        IdTipo = lancamento.IdTipo ?? TipoPagar,
        IdStatus = lancamento.IdStatus ?? StatusAberto,
        IdCompetencia = lancamento.IdCompetencia ?? 0,
        IdCategoria = lancamento.IdCategoria ?? 0,
        DataVencimento = lancamento.DataVencimento,
        IdContaCorrente = lancamento.IdContaCorrente ?? 0,
        IdCartaoCredito = lancamento.IdCartaoCredito ?? 0,
        NumParcelaAtual = lancamento.NumParcelaAtual,
        NumParcelaTotal = lancamento.NumParcelaTotal,
        Descricao = lancamento.Descricao?.Trim() ?? "",
        ValorAbsoluto = Math.Abs(lancamento.Valor ?? 0m),
        DataLancamento = lancamento.DataLancamento,
        DataPagamento = lancamento.DataPagamento,
        CodigoBarras = lancamento.CodigoBarras,
        Obs = lancamento.Obs,
        IdLancamentoPai = lancamento.IdLancamentoPai
    };

    private static decimal AplicarSinalAoValor(decimal valorAbsoluto, short idTipo) =>
        idTipo == TipoPagar ? -Math.Abs(valorAbsoluto) : Math.Abs(valorAbsoluto);
}
