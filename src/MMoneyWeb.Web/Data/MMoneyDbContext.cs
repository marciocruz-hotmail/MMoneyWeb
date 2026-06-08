using Microsoft.EntityFrameworkCore;
using MMoneyWeb.Web.Domain;

namespace MMoneyWeb.Web.Data;

/// <summary>
/// Contexto das entidades financeiras (tabelas legadas do MMoney).
/// Utilizar via IDbContextFactory&lt;MMoneyDbContext&gt; em componentes Blazor.
/// </summary>
public class MMoneyDbContext(DbContextOptions<MMoneyDbContext> options) : DbContext(options)
{
    public DbSet<ContaCorrente> ContasCorrentes => Set<ContaCorrente>();
    public DbSet<Competencia> Competencias => Set<Competencia>();
    public DbSet<Lancamento> Lancamentos => Set<Lancamento>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<CartaoCredito> CartoesCredito => Set<CartaoCredito>();
    public DbSet<LancamentoStatus> LancamentosStatus => Set<LancamentoStatus>();
    public DbSet<LancamentoTipo> LancamentosTipos => Set<LancamentoTipo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ContaCorrente>(entity =>
        {
            entity.ToTable("contas_correntes");
            entity.HasKey(e => e.IdContaCorrente);
            entity.Property(e => e.IdContaCorrente).HasColumnName("id_contacorrente");
            entity.Property(e => e.Nome).HasColumnName("nome");
            entity.Property(e => e.ExibirRelatorios).HasColumnName("exibir_relatorios");
            entity.Property(e => e.Ativo).HasColumnName("ativo");
        });

        modelBuilder.Entity<Competencia>(entity =>
        {
            entity.ToTable("competencias");
            entity.HasKey(e => e.IdCompetencia);
            entity.Property(e => e.IdCompetencia).HasColumnName("id_competencia");
            entity.Property(e => e.AnoMes).HasColumnName("ano_mes");
            entity.Property(e => e.DataInicial).HasColumnName("data_inicial");
            entity.Property(e => e.DataFinal).HasColumnName("data_final");
            entity.Property(e => e.Ativo).HasColumnName("ativo");
        });

        modelBuilder.Entity<Lancamento>(entity =>
        {
            entity.ToTable("lancamentos");
            entity.HasKey(e => e.IdLancamento);
            entity.Property(e => e.IdLancamento).HasColumnName("id_lancamento").ValueGeneratedOnAdd();
            entity.Property(e => e.Ordem).HasColumnName("ordem");
            entity.Property(e => e.NumParcelaAtual).HasColumnName("num_parcela_atual");
            entity.Property(e => e.NumParcelaTotal).HasColumnName("num_parcela_total");
            entity.Property(e => e.DataLancamento).HasColumnName("data_lancamento");
            entity.Property(e => e.DataVencimento).HasColumnName("data_vencimento");
            entity.Property(e => e.DataPagamento).HasColumnName("data_pagamento");
            entity.Property(e => e.IdCompetencia).HasColumnName("id_competencia");
            entity.Property(e => e.IdContaCorrente).HasColumnName("id_contacorrente");
            entity.Property(e => e.Descricao).HasColumnName("descricao");
            entity.Property(e => e.Valor).HasColumnName("valor").HasColumnType("decimal(18,2)");
            entity.Property(e => e.IdCategoria).HasColumnName("id_categoria");
            entity.Property(e => e.DeduzIr).HasColumnName("deduz_ir");
            entity.Property(e => e.Fixo).HasColumnName("fixo");
            entity.Property(e => e.IdCartaoCredito).HasColumnName("id_cartao_credito");
            entity.Property(e => e.SaldoAtual).HasColumnName("saldo_atual").HasColumnType("decimal(18,2)");
            entity.Property(e => e.Copiado).HasColumnName("copiado");
            entity.Property(e => e.IdLancamentoPai).HasColumnName("id_lancamento_pai");
            entity.Property(e => e.IdStatus).HasColumnName("id_status");
            entity.Property(e => e.IdTipo).HasColumnName("id_tipo");
            entity.Property(e => e.CodigoBarras).HasColumnName("codigo_barras");
            entity.Property(e => e.Obs).HasColumnName("obs");
        });

        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.ToTable("categorias");
            entity.HasKey(e => e.IdCategoria);
            entity.Property(e => e.IdCategoria).HasColumnName("id_categoria");
            entity.Property(e => e.Nome).HasColumnName("nome");
        });

        modelBuilder.Entity<CartaoCredito>(entity =>
        {
            entity.ToTable("cartoes_credito");
            entity.HasKey(e => e.IdCartaoCredito);
            entity.Property(e => e.IdCartaoCredito).HasColumnName("id_cartao_credito");
            entity.Property(e => e.Nome).HasColumnName("nome");
            entity.Property(e => e.Ativo).HasColumnName("ativo");
            entity.Property(e => e.DiaVenc).HasColumnName("dia_venc");
            entity.Property(e => e.Descricao).HasColumnName("descricao");
        });

        modelBuilder.Entity<LancamentoStatus>(entity =>
        {
            entity.ToTable("lancamentos_status");
            entity.HasKey(e => e.IdStatus);
            entity.Property(e => e.IdStatus).HasColumnName("id_status");
            entity.Property(e => e.Nome).HasColumnName("nome");
        });

        modelBuilder.Entity<LancamentoTipo>(entity =>
        {
            entity.ToTable("lancamentos_tipos");
            entity.HasKey(e => e.IdTipo);
            entity.Property(e => e.IdTipo).HasColumnName("id_tipo");
            entity.Property(e => e.Nome).HasColumnName("nome");
        });
    }
}
