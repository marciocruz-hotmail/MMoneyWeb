using Microsoft.AspNetCore.Components;
using MMoneyWeb.Web.Services;

namespace MMoneyWeb.Web.Components;

/// <summary>
/// Base para páginas do app — mensagens SweetAlert2 e tratamento de exceções com trace completo.
/// </summary>
public abstract class MmoneyPageBase : ComponentBase
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

    protected async Task<T?> ExecutarComErroSwalAsync<T>(Func<Task<T>> acao, string tituloErro = "Atenção")
        where T : class
    {
        try
        {
            return await acao();
        }
        catch (Exception ex)
        {
            await ExibirErroAsync(tituloErro, ex);
            return null;
        }
    }

    protected async Task ExecutarComProcessandoAsync(
        Func<Task> acao,
        string mensagemProcessando = "Processando . . .",
        string tituloErro = "Atenção",
        string? tituloSucesso = null,
        string? mensagemSucesso = null)
    {
        try
        {
            await LibMessages.ProcessandoAsync(mensagemProcessando);
            await acao();
            await LibMessages.ProcessandoHideAsync();

            if (tituloSucesso is not null && mensagemSucesso is not null)
            {
                await ExibirSucessoAsync(tituloSucesso, mensagemSucesso);
            }
        }
        catch (Exception ex)
        {
            await LibMessages.ProcessandoHideAsync();
            await ExibirErroAsync(tituloErro, ex);
        }
    }

    protected Task ExibirErroAsync(string titulo, Exception ex) =>
        LibMessages.ErrorAsync(titulo, ex);

    protected Task ExibirErroAsync(string titulo, string mensagem) =>
        LibMessages.ErrorAsync(titulo, mensagem);

    protected Task ExibirAlertaAsync(string titulo, string mensagem) =>
        LibMessages.AlertAsync(titulo, mensagem);

    protected Task<bool> ConfirmarAsync(string titulo, string mensagem) =>
        LibMessages.ConfirmAsync(titulo, mensagem);

    protected Task ExibirSucessoAsync(string titulo, string mensagem) =>
        LibMessages.SuccessAsync(titulo, mensagem);
}
