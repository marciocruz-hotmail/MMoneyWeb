namespace MMoneyWeb.Web.Domain;

public class Lancamento
{
    public int IdLancamento { get; set; }
    public int? Ordem { get; set; }
    public int? NumParcelaAtual { get; set; }
    public int? NumParcelaTotal { get; set; }
    public DateOnly? DataLancamento { get; set; }
    public DateOnly? DataVencimento { get; set; }
    public DateOnly? DataPagamento { get; set; }
    public int? IdCompetencia { get; set; }
    public int? IdContaCorrente { get; set; }
    public string? Descricao { get; set; }
    public decimal? Valor { get; set; }
    public int? IdCategoria { get; set; }
    public short? DeduzIr { get; set; }
    public short? Fixo { get; set; }
    public int? IdCartaoCredito { get; set; }
    public decimal? SaldoAtual { get; set; }
    public short? Copiado { get; set; }
    public int? IdLancamentoPai { get; set; }
    public short? IdStatus { get; set; }
    public short? IdTipo { get; set; }
    public string? CodigoBarras { get; set; }
    public string? Obs { get; set; }
}
