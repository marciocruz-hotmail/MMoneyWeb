using System.Net;
using Microsoft.JSInterop;

namespace MMoneyWeb.Web.Services;

public interface ILibMessageService
{
    Task AlertAsync(string title, string message, CancellationToken cancellationToken = default);
    Task SuccessAsync(string title, string message, CancellationToken cancellationToken = default);
    Task ErrorAsync(string title, string message, CancellationToken cancellationToken = default);
    Task ErrorAsync(string title, Exception exception, CancellationToken cancellationToken = default);
    Task ProcessandoAsync(string? message = null, CancellationToken cancellationToken = default);
    Task ProcessandoHideAsync(CancellationToken cancellationToken = default);
    Task CarregandoShowAsync(string? message = null, CancellationToken cancellationToken = default);
    Task CarregandoHideAsync(CancellationToken cancellationToken = default);
    Task HideAllAsync(CancellationToken cancellationToken = default);
}

public sealed class LibMessageService(IJSRuntime jsRuntime) : ILibMessageService
{
    public Task AlertAsync(string title, string message, CancellationToken cancellationToken = default) =>
        InvokeAsync("LibMessageAlert", title, message, cancellationToken);

    public Task SuccessAsync(string title, string message, CancellationToken cancellationToken = default) =>
        InvokeAsync("LibMessageSuccess", title, message, cancellationToken);

    public Task ErrorAsync(string title, string message, CancellationToken cancellationToken = default) =>
        InvokeAsync("LibMessageError", title, FormatTraceHtml(message), cancellationToken);

    public Task ErrorAsync(string title, Exception exception, CancellationToken cancellationToken = default) =>
        ErrorAsync(title, exception.ToString(), cancellationToken);

    public Task ProcessandoAsync(string? message = null, CancellationToken cancellationToken = default) =>
        jsRuntime.InvokeVoidAsync("LibMessageProcessando", cancellationToken, message ?? "Processando . . .").AsTask();

    public Task ProcessandoHideAsync(CancellationToken cancellationToken = default) =>
        InvokeVoidAsync("LibMessageProcessandoHide", cancellationToken);

    public Task CarregandoShowAsync(string? message = null, CancellationToken cancellationToken = default) =>
        jsRuntime.InvokeVoidAsync("mmoneyLoading.show", cancellationToken, message ?? "Carregando...").AsTask();

    public Task CarregandoHideAsync(CancellationToken cancellationToken = default) =>
        jsRuntime.InvokeVoidAsync("mmoneyLoading.hide", cancellationToken).AsTask();

    public Task HideAllAsync(CancellationToken cancellationToken = default) =>
        InvokeVoidAsync("LibMessageHideAll", cancellationToken);

    private Task InvokeAsync(string functionName, string title, string message, CancellationToken cancellationToken) =>
        jsRuntime.InvokeVoidAsync(functionName, cancellationToken, title, message).AsTask();

    private Task InvokeVoidAsync(string functionName, CancellationToken cancellationToken) =>
        jsRuntime.InvokeVoidAsync(functionName, cancellationToken).AsTask();

    public static string FormatTraceHtml(string trace)
    {
        var encoded = WebUtility.HtmlEncode(trace);
        return $"""<pre class="mmoney-swal-trace">{encoded}</pre>""";
    }
}
