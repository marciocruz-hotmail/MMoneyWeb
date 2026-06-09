using Microsoft.EntityFrameworkCore;
using MMoneyWeb.Web.Data;
using MMoneyWeb.Web.Domain;
using MMoneyWeb.Web.Services.Models;

namespace MMoneyWeb.Web.Services;

public abstract class LancamentosServiceBase(IDbContextFactory<MMoneyDbContext> dbContextFactory)
{
    protected IDbContextFactory<MMoneyDbContext> DbContextFactory { get; } = dbContextFactory;

    public const short TipoPagar = 1;
    public const short TipoReceber = 2;

    public const short StatusAberto = 1;
    public const short StatusAtrasado = 2;
    public const short StatusAgendado = 3;
    public const short StatusQuitado = 4;

    public Task<IReadOnlyList<ContaCorrente>> ObterContasAtivasAsync(CancellationToken cancellationToken = default) =>
        ObterContasAtivasCoreAsync(cancellationToken);

    protected async Task<IReadOnlyList<ContaCorrente>> ObterContasAtivasCoreAsync(CancellationToken cancellationToken)
    {
        await using var db = await DbContextFactory.CreateDbContextAsync(cancellationToken);
        return await db.ContasCorrentes
            .AsNoTracking()
            .Where(c => c.Ativo == 1)
            .OrderBy(c => c.Nome)
            .ToListAsync(cancellationToken);
    }

    public Task<IReadOnlyList<Competencia>> ObterCompetenciasAtivasAsync(CancellationToken cancellationToken = default) =>
        ObterCompetenciasAtivasCoreAsync(cancellationToken);

    protected async Task<IReadOnlyList<Competencia>> ObterCompetenciasAtivasCoreAsync(CancellationToken cancellationToken)
    {
        await using var db = await DbContextFactory.CreateDbContextAsync(cancellationToken);
        return await db.Competencias
            .AsNoTracking()
            .Where(c => c.Ativo == 1)
            .OrderBy(c => c.AnoMes)
            .ToListAsync(cancellationToken);
    }

    public Task<IReadOnlyList<LancamentoLinhaViewModel>> ObterLancamentosAsync(
        int idContaCorrente,
        int idCompetencia,
        CancellationToken cancellationToken = default) =>
        ObterLancamentosCoreAsync(idContaCorrente, idCompetencia, cancellationToken);

    protected async Task<IReadOnlyList<LancamentoLinhaViewModel>> ObterLancamentosCoreAsync(
        int idContaCorrente,
        int idCompetencia,
        CancellationToken cancellationToken)
    {
        await using var db = await DbContextFactory.CreateDbContextAsync(cancellationToken);

        var lancamentos = await db.Lancamentos
            .AsNoTracking()
            .Where(l => l.IdContaCorrente == idContaCorrente && l.IdCompetencia == idCompetencia)
            .OrderByDescending(l => l.IdStatus)
            .ThenBy(l => l.DataVencimento)
            .ThenBy(l => l.Ordem)
            .ToListAsync(cancellationToken);

        return MontarLinhasComSaldo(lancamentos);
    }

    public Task<LancamentoLookupsModel> ObterLookupsModalAsync(CancellationToken cancellationToken = default) =>
        ObterLookupsModalCoreAsync(cancellationToken);

    protected async Task<LancamentoLookupsModel> ObterLookupsModalCoreAsync(CancellationToken cancellationToken)
    {
        await using var db = await DbContextFactory.CreateDbContextAsync(cancellationToken);

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
        Task.FromResult(CriarModeloNovoCore(idContaCorrente, idCompetencia));

    protected static LancamentoFormModel CriarModeloNovoCore(int idContaCorrente, int idCompetencia) =>
        new()
        {
            IdLancamento = 0,
            IdContaCorrente = idContaCorrente,
            IdCompetencia = idCompetencia,
            DataVencimento = DateOnly.FromDateTime(DateTime.Today),
            DataLancamento = DateOnly.FromDateTime(DateTime.Today),
            NumParcelaAtual = 1,
            NumParcelaTotal = 1,
            IdTipo = TipoPagar,
            IdStatus = StatusAberto
        };

    public Task<LancamentoFormModel?> ObterLancamentoParaEdicaoAsync(int idLancamento, CancellationToken cancellationToken = default) =>
        ObterLancamentoParaEdicaoCoreAsync(idLancamento, cancellationToken);

    protected async Task<LancamentoFormModel?> ObterLancamentoParaEdicaoCoreAsync(int idLancamento, CancellationToken cancellationToken)
    {
        await using var db = await DbContextFactory.CreateDbContextAsync(cancellationToken);
        var lancamento = await db.Lancamentos.AsNoTracking().FirstOrDefaultAsync(l => l.IdLancamento == idLancamento, cancellationToken);
        if (lancamento is null)
        {
            return null;
        }

        return MapearParaFormComDefaults(lancamento);
    }

    protected static LancamentoFormModel MapearParaFormComDefaults(Lancamento lancamento)
    {
        var form = MapearParaForm(lancamento);
        AplicarParcelasDefaultQuandoVazias(form);
        return form;
    }

    public Task<CopiarLancamentosProximaCompetenciaResult> CopiarLancamentosProximaCompetenciaAsync(
        int idContaCorrente,
        int idCompetenciaAtual,
        CancellationToken cancellationToken = default) =>
        CopiarLancamentosProximaCompetenciaCoreAsync(idContaCorrente, idCompetenciaAtual, cancellationToken);

    protected async Task<CopiarLancamentosProximaCompetenciaResult> CopiarLancamentosProximaCompetenciaCoreAsync(
        int idContaCorrente,
        int idCompetenciaAtual,
        CancellationToken cancellationToken)
    {
        await using var db = await DbContextFactory.CreateDbContextAsync(cancellationToken);

        var proximaCompetencia = await ObterProximaCompetenciaAtivaCoreAsync(db, idCompetenciaAtual, cancellationToken)
            ?? throw new InvalidOperationException("Não há competência ativa seguinte à competência atual.");

        var lancamentos = await db.Lancamentos
            .Where(l => l.IdContaCorrente == idContaCorrente && l.IdCompetencia == idCompetenciaAtual)
            .OrderByDescending(l => l.IdStatus)
            .ThenBy(l => l.DataVencimento)
            .ThenBy(l => l.Ordem)
            .ToListAsync(cancellationToken);

        var copiasExistentes = await db.Lancamentos
            .Where(l =>
                l.IdContaCorrente == idContaCorrente &&
                l.IdCompetencia == proximaCompetencia.IdCompetencia &&
                l.IdLancamentoPai != null)
            .ToListAsync(cancellationToken);

        var copiasPorPai = copiasExistentes
            .GroupBy(l => l.IdLancamentoPai!.Value)
            .ToDictionary(g => g.Key, g => g.First());

        var copiados = 0;
        var atualizados = 0;
        var ignorados = 0;
        var alterado = false;

        foreach (var origem in lancamentos)
        {
            if (!DeveCopiarParaProximaCompetencia(origem))
            {
                ignorados++;
                continue;
            }

            if (!origem.DataVencimento.HasValue)
            {
                ignorados++;
                continue;
            }

            var novaDataVencimento = origem.DataVencimento.Value.AddMonths(1);

            if (copiasPorPai.TryGetValue(origem.IdLancamento, out var destinoExistente))
            {
                AtualizarCopiaParaProximaCompetencia(
                    destinoExistente,
                    origem,
                    proximaCompetencia.IdCompetencia,
                    novaDataVencimento);

                atualizados++;
                alterado = true;
                continue;
            }

            var ordem = await ObterProximaOrdemCoreAsync(
                db,
                idContaCorrente,
                proximaCompetencia.IdCompetencia,
                novaDataVencimento,
                cancellationToken);

            var novoDestino = CriarCopiaParaProximaCompetencia(
                origem,
                proximaCompetencia.IdCompetencia,
                novaDataVencimento,
                ordem);

            db.Lancamentos.Add(novoDestino);
            copiasPorPai[origem.IdLancamento] = novoDestino;

            copiados++;
            alterado = true;
        }

        if (alterado)
        {
            await db.SaveChangesAsync(cancellationToken);
        }

        return new CopiarLancamentosProximaCompetenciaResult
        {
            QuantidadeCopiada = copiados,
            QuantidadeAtualizada = atualizados,
            QuantidadeIgnorada = ignorados,
            ProximaCompetenciaAnoMes = proximaCompetencia.AnoMes
        };
    }

    protected static async Task<Competencia?> ObterProximaCompetenciaAtivaCoreAsync(
        MMoneyDbContext db,
        int idCompetenciaAtual,
        CancellationToken cancellationToken)
    {
        var competencias = await db.Competencias
            .AsNoTracking()
            .Where(c => c.Ativo == 1)
            .OrderBy(c => c.AnoMes)
            .ToListAsync(cancellationToken);

        var indiceAtual = competencias.FindIndex(c => c.IdCompetencia == idCompetenciaAtual);
        if (indiceAtual < 0 || indiceAtual >= competencias.Count - 1)
        {
            return null;
        }

        return competencias[indiceAtual + 1];
    }

    protected static bool DeveCopiarParaProximaCompetencia(Lancamento lancamento)
    {
        if (lancamento.Fixo == 1)
        {
            return true;
        }

        var parcelaAtual = lancamento.NumParcelaAtual ?? 0;
        var parcelaTotal = lancamento.NumParcelaTotal ?? 0;

        if (parcelaAtual <= 0 || parcelaTotal <= 0)
        {
            return false;
        }

        if (parcelaAtual == 1 && parcelaTotal == 1)
        {
            return false;
        }

        if (parcelaAtual >= parcelaTotal)
        {
            return false;
        }

        return true;
    }

    protected static Lancamento CriarCopiaParaProximaCompetencia(
        Lancamento origem,
        int idProximaCompetencia,
        DateOnly novaDataVencimento,
        int ordem)
    {
        var destino = new Lancamento();
        AplicarDadosCopiaParaProximaCompetencia(destino, origem, idProximaCompetencia, novaDataVencimento, ordem);
        return destino;
    }

    protected static void AtualizarCopiaParaProximaCompetencia(
        Lancamento destino,
        Lancamento origem,
        int idProximaCompetencia,
        DateOnly novaDataVencimento) =>
        AplicarDadosCopiaParaProximaCompetencia(destino, origem, idProximaCompetencia, novaDataVencimento);

    protected static void AplicarDadosCopiaParaProximaCompetencia(
        Lancamento destino,
        Lancamento origem,
        int idProximaCompetencia,
        DateOnly novaDataVencimento,
        int? ordem = null)
    {
        if (ordem.HasValue)
        {
            destino.Ordem = ordem.Value;
        }

        destino.NumParcelaAtual = origem.NumParcelaAtual;
        destino.NumParcelaTotal = origem.NumParcelaTotal;
        destino.DataLancamento = origem.DataLancamento;
        destino.DataVencimento = novaDataVencimento;
        destino.DataPagamento = null;
        destino.IdCompetencia = idProximaCompetencia;
        destino.IdContaCorrente = origem.IdContaCorrente;
        destino.Descricao = origem.Descricao;
        destino.Valor = origem.Valor;
        destino.IdCategoria = origem.IdCategoria;
        destino.DeduzIr = origem.DeduzIr;
        destino.Fixo = origem.Fixo;
        destino.IdCartaoCredito = origem.IdCartaoCredito;
        destino.SaldoAtual = null;
        destino.Copiado = 1;
        destino.IdLancamentoPai = origem.IdLancamento;
        destino.IdStatus = StatusAberto;
        destino.IdTipo = origem.IdTipo;
        destino.CodigoBarras = origem.CodigoBarras;
        destino.Obs = origem.Obs;
    }

    public Task<int> ExcluirLancamentosAsync(IReadOnlyCollection<int> idsLancamentos, CancellationToken cancellationToken = default) =>
        ExcluirLancamentosCoreAsync(idsLancamentos, cancellationToken);

    protected async Task<int> ExcluirLancamentosCoreAsync(IReadOnlyCollection<int> idsLancamentos, CancellationToken cancellationToken)
    {
        if (idsLancamentos.Count == 0)
        {
            return 0;
        }

        await using var db = await DbContextFactory.CreateDbContextAsync(cancellationToken);
        var lancamentos = await db.Lancamentos
            .Where(l => idsLancamentos.Contains(l.IdLancamento))
            .ToListAsync(cancellationToken);

        if (lancamentos.Count == 0)
        {
            return 0;
        }

        db.Lancamentos.RemoveRange(lancamentos);
        await db.SaveChangesAsync(cancellationToken);
        return lancamentos.Count;
    }

    public Task<int> SalvarLancamentoAsync(LancamentoFormModel form, CancellationToken cancellationToken = default) =>
        SalvarLancamentoCoreAsync(form, cancellationToken);

    protected async Task<int> SalvarLancamentoCoreAsync(LancamentoFormModel form, CancellationToken cancellationToken)
    {
        AplicarDefaultDataPagamentoNovoLiquidado(form);
        AplicarParcelasDefaultQuandoVazias(form);
        AplicarParcelasFixoAntesSalvar(form);

        await using var db = await DbContextFactory.CreateDbContextAsync(cancellationToken);
        var valorComSinal = AplicarSinalAoValor(form.ValorAbsoluto, form.IdTipo);

        if (form.IsNovo)
        {
            if (!form.DataVencimento.HasValue)
            {
                throw new InvalidOperationException("Data de vencimento é obrigatória para calcular a ordem do lançamento.");
            }

            form.Ordem = await ObterProximaOrdemCoreAsync(
                db,
                form.IdContaCorrente,
                form.IdCompetencia,
                form.DataVencimento.Value,
                cancellationToken);

            return await InserirLancamentoCoreAsync(db, form, valorComSinal, cancellationToken);
        }

        return await AtualizarLancamentoCoreAsync(db, form, valorComSinal, cancellationToken);
    }

    protected static async Task<int> InserirLancamentoCoreAsync(
        MMoneyDbContext db,
        LancamentoFormModel form,
        decimal valorComSinal,
        CancellationToken cancellationToken)
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

    protected static async Task<int> AtualizarLancamentoCoreAsync(
        MMoneyDbContext db,
        LancamentoFormModel form,
        decimal valorComSinal,
        CancellationToken cancellationToken)
    {
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

    protected static async Task<int> ObterProximaOrdemCoreAsync(
        MMoneyDbContext db,
        int idContaCorrente,
        int idCompetencia,
        DateOnly dataVencimento,
        CancellationToken cancellationToken)
    {
        var maxOrdem = await db.Lancamentos
            .AsNoTracking()
            .Where(l =>
                l.IdContaCorrente == idContaCorrente &&
                l.IdCompetencia == idCompetencia &&
                l.DataVencimento == dataVencimento)
            .MaxAsync(l => (int?)l.Ordem, cancellationToken);

        return CalcularProximaOrdemCore(maxOrdem);
    }

    protected static int CalcularProximaOrdemCore(int? maxOrdemExistente) =>
        (maxOrdemExistente ?? 0) + 1;

    protected static IReadOnlyList<LancamentoLinhaViewModel> MontarLinhasComSaldo(IReadOnlyList<Lancamento> lancamentos)
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

    protected static string FormatarParcela(Lancamento lancamento)
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

    protected static string ObterClasseStatus(short idStatus) => idStatus switch
    {
        StatusQuitado => "mmoney-lanc-row-quitado",
        StatusAberto => "mmoney-lanc-row-aberto",
        StatusAgendado => "mmoney-lanc-row-agendado",
        StatusAtrasado => "mmoney-lanc-row-atrasado",
        _ => "mmoney-lanc-row-aberto"
    };

    protected static void AplicarParcelasFixoAntesSalvar(LancamentoFormModel form)
    {
        if (!form.Fixo)
        {
            return;
        }

        form.NumParcelaAtual = 0;
        form.NumParcelaTotal = 0;
    }

    protected static void AplicarParcelasDefaultQuandoVazias(LancamentoFormModel form)
    {
        if (form.Fixo)
        {
            return;
        }

        if (form.NumParcelaAtual is null or <= 0)
        {
            form.NumParcelaAtual = 1;
        }

        if (form.NumParcelaTotal is null or <= 0)
        {
            form.NumParcelaTotal = 1;
        }
    }

    protected static void AplicarDefaultDataPagamentoNovoLiquidado(LancamentoFormModel form)
    {
        if (!form.IsNovo || form.IdStatus != StatusQuitado || form.DataPagamento.HasValue || !form.DataVencimento.HasValue)
        {
            return;
        }

        form.DataPagamento = form.DataVencimento;
    }

    protected static void AplicarDataPagamentoAoAlterarVencimento(LancamentoFormModel form)
    {
        if (!form.DataVencimento.HasValue)
        {
            return;
        }

        form.DataPagamento = form.DataVencimento;
    }

    /// <inheritdoc cref="AplicarDataPagamentoAoAlterarVencimento"/>
    protected static void AplicarDataPagamentoNovoAoAlterarVencimento(LancamentoFormModel form) =>
        AplicarDataPagamentoAoAlterarVencimento(form);

    protected static LancamentoFormModel MapearParaForm(Lancamento lancamento) => new()
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

    protected static decimal AplicarSinalAoValor(decimal valorAbsoluto, short idTipo) =>
        idTipo == TipoPagar ? -Math.Abs(valorAbsoluto) : Math.Abs(valorAbsoluto);

    /// <summary>
    /// Ponte para componentes UI e testes acederem métodos protegidos estáticos da base.
    /// </summary>
    public static class UiAccess
    {
        public static void AplicarDefaultDataPagamentoNovoLiquidado(LancamentoFormModel form) =>
            LancamentosServiceBase.AplicarDefaultDataPagamentoNovoLiquidado(form);

        public static void AplicarDataPagamentoAoAlterarVencimento(LancamentoFormModel form) =>
            LancamentosServiceBase.AplicarDataPagamentoAoAlterarVencimento(form);

        public static void AplicarDataPagamentoNovoAoAlterarVencimento(LancamentoFormModel form) =>
            LancamentosServiceBase.AplicarDataPagamentoAoAlterarVencimento(form);

        public static void AplicarParcelasDefaultQuandoVazias(LancamentoFormModel form) =>
            LancamentosServiceBase.AplicarParcelasDefaultQuandoVazias(form);

        public static void AplicarParcelasFixoAntesSalvar(LancamentoFormModel form) =>
            LancamentosServiceBase.AplicarParcelasFixoAntesSalvar(form);

        public static IReadOnlyList<LancamentoLinhaViewModel> MontarLinhasComSaldo(IReadOnlyList<Lancamento> lancamentos) =>
            LancamentosServiceBase.MontarLinhasComSaldo(lancamentos);

        public static int CalcularProximaOrdem(int? maxOrdemExistente) =>
            LancamentosServiceBase.CalcularProximaOrdemCore(maxOrdemExistente);

        public static bool DeveCopiarParaProximaCompetencia(Lancamento lancamento) =>
            LancamentosServiceBase.DeveCopiarParaProximaCompetencia(lancamento);

        public static Lancamento CriarCopiaParaProximaCompetencia(
            Lancamento origem,
            int idProximaCompetencia,
            DateOnly novaDataVencimento,
            int ordem) =>
            LancamentosServiceBase.CriarCopiaParaProximaCompetencia(
                origem,
                idProximaCompetencia,
                novaDataVencimento,
                ordem);

        public static void AtualizarCopiaParaProximaCompetencia(
            Lancamento destino,
            Lancamento origem,
            int idProximaCompetencia,
            DateOnly novaDataVencimento) =>
            LancamentosServiceBase.AtualizarCopiaParaProximaCompetencia(
                destino,
                origem,
                idProximaCompetencia,
                novaDataVencimento);
    }
}
