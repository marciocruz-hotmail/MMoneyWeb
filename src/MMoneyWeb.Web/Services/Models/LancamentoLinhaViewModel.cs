namespace MMoneyWeb.Web.Services.Models;

public sealed class LancamentoLinhaViewModel
{
    public int IdLancamento { get; init; }
    public DateOnly? DataVencimento { get; init; }
    public string DebitoCredito { get; init; } = "";
    public string Parcela { get; init; } = "";
    public string Descricao { get; init; } = "";
    public decimal Valor { get; init; }
    public decimal Saldo { get; init; }
    public short IdStatus { get; init; }
    public bool IsDebito { get; init; }
    public string StatusCssClass { get; init; } = "";
}
