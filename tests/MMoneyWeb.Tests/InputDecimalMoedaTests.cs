using MMoneyWeb.Web.Components.Shared;

namespace MMoneyWeb.Tests;

public class InputDecimalMoedaTests
{
    [Theory]
    [InlineData("10,16", "10,16")]
    [InlineData("R$ 10,16", "10,16")]
    [InlineData("1.234,56", "1234,56")]
    [InlineData("  10 , 16  ", "10,16")]
    [InlineData("abc12,34xyz", "12,34")]
    [InlineData("10.16", "1016")]
    [InlineData("", "")]
    public void NormalizarTextoMoeda_RemoveCaracteresInvalidos(string entrada, string esperado)
    {
        Assert.Equal(esperado, InputDecimalMoeda.NormalizarTextoMoeda(entrada));
    }

    [Theory]
    [InlineData("10,16", 10.16)]
    [InlineData("R$ 1.234,56", 1234.56)]
    [InlineData("1.234,56", 1234.56)]
    [InlineData("10,16,00", 10.16)]
    [InlineData("", 0)]
    public void TryParseMoeda_AceitaFormatoBrasileiro(string entrada, decimal esperado)
    {
        var ok = InputDecimalMoeda.TryParseMoeda(entrada, out var valor);

        Assert.True(ok);
        Assert.Equal(esperado, valor);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData(",,, ")]
    public void TryParseMoeda_RejeitaValorInvalido(string entrada)
    {
        var ok = InputDecimalMoeda.TryParseMoeda(entrada, out _);

        Assert.False(ok);
    }
}
