using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MMoneyWeb.Web.Data;
using MMoneyWeb.Web.Services;
using MMoneyWeb.Web.Services.Models;

namespace MMoneyWeb.Tests;

public class SalvarLancamentoIntegrationTests
{
    [Fact]
    public async Task SalvarLancamentoAsync_NovoReceberQuitado_PersisteSemErro()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "MMoneyWeb.Web")))
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("MMoneyConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var options = new DbContextOptionsBuilder<MMoneyDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        await using var db = new MMoneyDbContext(options);

        var conta = await db.ContasCorrentes.AsNoTracking()
            .Where(c => c.Ativo == 1 && c.Nome != null && c.Nome.Contains("ITA"))
            .OrderBy(c => c.IdContaCorrente)
            .FirstOrDefaultAsync();

        var competencia = await db.Competencias.AsNoTracking()
            .Where(c => c.Ativo == 1 && c.AnoMes == "2026_05")
            .FirstOrDefaultAsync();

        var categoria = await db.Categorias.AsNoTracking()
            .Where(c => c.Nome == "Receitas")
            .FirstOrDefaultAsync();

        Assert.NotNull(conta);
        Assert.NotNull(competencia);
        Assert.NotNull(categoria);

        var factory = new TestDbContextFactory(options);
        var service = new LancamentosViewService(factory);

        var form = new LancamentoFormModel
        {
            IdLancamento = 0,
            IdTipo = LancamentosViewService.TipoReceber,
            IdStatus = LancamentosViewService.StatusQuitado,
            IdCompetencia = competencia!.IdCompetencia,
            IdCategoria = categoria!.IdCategoria,
            IdContaCorrente = conta!.IdContaCorrente,
            DataVencimento = new DateOnly(2026, 5, 28),
            DataPagamento = new DateOnly(2026, 5, 28),
            DataLancamento = DateOnly.FromDateTime(DateTime.Today),
            NumParcelaAtual = 1,
            NumParcelaTotal = 1,
            Descricao = "Rendimentos",
            ValorAbsoluto = 10.16m
        };

        var id = await service.SalvarLancamentoAsync(form);
        Assert.True(id > 0);

        var salvo = await db.Lancamentos.AsNoTracking().FirstOrDefaultAsync(l => l.IdLancamento == id);
        Assert.NotNull(salvo);
        Assert.Equal("Rendimentos", salvo!.Descricao?.Trim());
        Assert.True(salvo.Ordem is > 0);

        db.Lancamentos.Remove(new MMoneyWeb.Web.Domain.Lancamento { IdLancamento = id });
        await db.SaveChangesAsync();
    }

    private sealed class TestDbContextFactory(DbContextOptions<MMoneyDbContext> options) : IDbContextFactory<MMoneyDbContext>
    {
        public MMoneyDbContext CreateDbContext() => new(options);

        public Task<MMoneyDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(CreateDbContext());
    }
}
