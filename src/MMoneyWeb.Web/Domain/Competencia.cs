namespace MMoneyWeb.Web.Domain;

public class Competencia
{
    public int IdCompetencia { get; set; }
    public string? AnoMes { get; set; }
    public DateOnly? DataInicial { get; set; }
    public DateOnly? DataFinal { get; set; }
    public short? Ativo { get; set; }
}
