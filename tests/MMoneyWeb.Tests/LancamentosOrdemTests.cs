using MMoneyWeb.Web.Services;

namespace MMoneyWeb.Tests;

public class LancamentosOrdemTests
{
    [Theory]
    [InlineData(null, 1)]
    [InlineData(0, 1)]
    [InlineData(1, 2)]
    [InlineData(5, 6)]
    public void CalcularProximaOrdem_RetornaMaxMaisUm(int? maxOrdem, int esperado)
    {
        var proxima = LancamentosServiceBase.UiAccess.CalcularProximaOrdem(maxOrdem);
        Assert.Equal(esperado, proxima);
    }
}
