namespace MMoneyWeb.Web.Services.Models;

public sealed class CopiarLancamentosProximaCompetenciaResult
{
    public int QuantidadeCopiada { get; init; }
    public int QuantidadeAtualizada { get; init; }
    public int QuantidadeIgnorada { get; init; }
    public string? ProximaCompetenciaAnoMes { get; init; }
}
