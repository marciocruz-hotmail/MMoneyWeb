using MMoneyWeb.Web.Domain;

namespace MMoneyWeb.Web.Services.Models;

public sealed class LancamentoLookupsModel
{
    public IReadOnlyList<ContaCorrente> Contas { get; init; } = [];
    public IReadOnlyList<Competencia> Competencias { get; init; } = [];
    public IReadOnlyList<Categoria> Categorias { get; init; } = [];
    public IReadOnlyList<CartaoCredito> Cartoes { get; init; } = [];
    public IReadOnlyList<LancamentoStatus> Status { get; init; } = [];
    public IReadOnlyList<LancamentoTipo> Tipos { get; init; } = [];
}
