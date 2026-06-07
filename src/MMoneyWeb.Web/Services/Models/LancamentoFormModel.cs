using System.ComponentModel.DataAnnotations;

namespace MMoneyWeb.Web.Services.Models;

public sealed class LancamentoFormModel
{
    public int IdLancamento { get; set; }

    [Range(0, int.MaxValue)]
    public int Ordem { get; set; }

    public bool Fixo { get; set; }

    public bool DeduzIr { get; set; }

    [Required(ErrorMessage = "Informe o tipo.")]
    public short IdTipo { get; set; } = LancamentosViewService.TipoPagar;

    [Required(ErrorMessage = "Informe o status.")]
    public short IdStatus { get; set; } = LancamentosViewService.StatusAberto;

    [Range(1, int.MaxValue, ErrorMessage = "Informe a competência.")]
    public int IdCompetencia { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Informe a categoria.")]
    public int IdCategoria { get; set; }

    [Required(ErrorMessage = "Informe a data de vencimento.")]
    public DateOnly? DataVencimento { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Informe a conta.")]
    public int IdContaCorrente { get; set; }

    public int IdCartaoCredito { get; set; }

    [Range(0, 9999, ErrorMessage = "Parcela inválida.")]
    public int? NumParcelaAtual { get; set; }

    [Range(0, 9999, ErrorMessage = "Quantidade de parcelas inválida.")]
    public int? NumParcelaTotal { get; set; }

    [Required(ErrorMessage = "Informe a descrição.")]
    [MaxLength(500)]
    public string Descricao { get; set; } = "";

    [Required(ErrorMessage = "Informe o valor.")]
    [Range(typeof(decimal), "0.01", "999999999999", ErrorMessage = "Informe um valor maior que zero.")]
    public decimal ValorAbsoluto { get; set; }

    public DateOnly? DataLancamento { get; set; }

    public DateOnly? DataPagamento { get; set; }

    [MaxLength(100)]
    public string? CodigoBarras { get; set; }

    [MaxLength(2000)]
    public string? Obs { get; set; }

    public int? IdLancamentoPai { get; set; }

    public bool IsNovo => IdLancamento <= 0;

    public string TituloModal => IsNovo ? "Lançamentos [ Cadastrando ]" : $"Lançamentos [ Edição — Id. {IdLancamento} ]";
}
