namespace MMoneyWeb.Web.Domain;

public class CartaoCredito
{
    public int IdCartaoCredito { get; set; }
    public string? Nome { get; set; }
    public short? Ativo { get; set; }
    public int? DiaVenc { get; set; }
    public string? Descricao { get; set; }
}
