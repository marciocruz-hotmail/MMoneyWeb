using System.ComponentModel.DataAnnotations;
using MMoneyWeb.Web.Services;
using MMoneyWeb.Web.Services.Models;

namespace MMoneyWeb.Tests;

public class LancamentoFormModelValidationTests
{
    [Fact]
    public void ValorAbsoluto_Range_NaoLancaExcecaoComCulturaPtBr()
    {
        System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt-BR");
        System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("pt-BR");

        var model = new LancamentoFormModel
        {
            IdTipo = LancamentosViewService.TipoReceber,
            IdStatus = LancamentosViewService.StatusQuitado,
            IdCompetencia = 1,
            IdCategoria = 1,
            IdContaCorrente = 1,
            DataVencimento = new DateOnly(2026, 5, 28),
            Descricao = "Teste",
            ValorAbsoluto = 10.16m
        };

        var results = new List<ValidationResult>();
        var ok = Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true);

        Assert.True(ok);
        Assert.Empty(results);
    }

    [Fact]
    public void ValorAbsoluto_Range_RejeitaZero()
    {
        var model = new LancamentoFormModel
        {
            IdTipo = LancamentosViewService.TipoReceber,
            IdStatus = LancamentosViewService.StatusAberto,
            IdCompetencia = 1,
            IdCategoria = 1,
            IdContaCorrente = 1,
            DataVencimento = new DateOnly(2026, 5, 28),
            Descricao = "Teste",
            ValorAbsoluto = 0m
        };

        var results = new List<ValidationResult>();
        var ok = Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true);

        Assert.False(ok);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(LancamentoFormModel.ValorAbsoluto)));
    }
}
