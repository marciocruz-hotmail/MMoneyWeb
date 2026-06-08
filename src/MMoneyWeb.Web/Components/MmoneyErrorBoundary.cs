using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MMoneyWeb.Web.Services;

namespace MMoneyWeb.Web.Components;

/// <summary>
/// Captura exceções não tratadas na árvore de componentes e exibe SweetAlert2 com trace completo.
/// </summary>
public sealed class MmoneyErrorBoundary : ErrorBoundary
{
    [Inject]
    private ILibMessageService LibMessages { get; set; } = default!;

    private bool erroExibido;

    protected override async Task OnErrorAsync(Exception exception)
    {
        if (erroExibido)
        {
            return;
        }

        erroExibido = true;

        try
        {
            await LibMessages.ErrorAsync("Atenção", exception);
        }
        catch
        {
            // Swal indisponível — ErrorContent permanece visível.
        }
    }

    protected override void OnParametersSet()
    {
        if (CurrentException is null)
        {
            erroExibido = false;
        }
    }
}
