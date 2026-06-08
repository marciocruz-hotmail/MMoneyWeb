using Microsoft.AspNetCore.Components;
using MMoneyWeb.Web.Services;

namespace MMoneyWeb.Web.Components;

/// <summary>
/// Base para componentes compartilhados — mesmo padrão de mensagens e exceções das páginas.
/// </summary>
public abstract class MmoneyComponentBase : ComponentBase
{
    [Inject]
    protected ILibMessageService LibMessages { get; set; } = default!;

    protected async Task ExecutarComErroSwalAsync(Func<Task> acao, string tituloErro = "Atenção")
    {
        try
        {
            await acao();
        }
        catch (Exception ex)
        {
            await ExibirErroAsync(tituloErro, ex);
        }
    }

    protected Task ExibirErroAsync(string titulo, Exception ex) =>
        LibMessages.ErrorAsync(titulo, ex);

    protected Task ExibirAlertaAsync(string titulo, string mensagem) =>
        LibMessages.AlertAsync(titulo, mensagem);

    protected Task ExibirSucessoAsync(string titulo, string mensagem) =>
        LibMessages.SuccessAsync(titulo, mensagem);
}
