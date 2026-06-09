using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using MMoneyWeb.Web.Data;

namespace MMoneyWeb.Web.Services;

public sealed record LancamentosFiltroPrefs(int? IdContaCorrente, int? IdCompetencia);

public interface IUserLancamentosPrefsService
{
    Task<LancamentosFiltroPrefs> ObterAsync(CancellationToken cancellationToken = default);

    Task SalvarAsync(int idContaCorrente, int idCompetencia, CancellationToken cancellationToken = default);
}

public sealed class UserLancamentosPrefsService(
    AuthenticationStateProvider authenticationStateProvider,
    UserManager<ApplicationUser> userManager) : IUserLancamentosPrefsService
{
    public async Task<LancamentosFiltroPrefs> ObterAsync(CancellationToken cancellationToken = default)
    {
        var user = await ObterUsuarioAutenticadoAsync(cancellationToken);
        if (user is null)
        {
            return new LancamentosFiltroPrefs(null, null);
        }

        return new LancamentosFiltroPrefs(user.LancamentosIdContaCorrente, user.LancamentosIdCompetencia);
    }

    public async Task SalvarAsync(int idContaCorrente, int idCompetencia, CancellationToken cancellationToken = default)
    {
        var user = await ObterUsuarioAutenticadoAsync(cancellationToken);
        if (user is null)
        {
            return;
        }

        user.LancamentosIdContaCorrente = idContaCorrente;
        user.LancamentosIdCompetencia = idCompetencia;
        await userManager.UpdateAsync(user);
    }

    private async Task<ApplicationUser?> ObterUsuarioAutenticadoAsync(CancellationToken cancellationToken)
    {
        var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
        var principal = authState.User;
        if (principal.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        return await userManager.GetUserAsync(principal);
    }
}
