using Microsoft.JSInterop;

namespace datotekica.Extensions;

public static class IJSRuntimeExtensions
{
    public static ValueTask NavigateBack(this IJSRuntime js) => js.InvokeVoidAsync("window.history.back");
    public static ValueTask OpenNewTab(this IJSRuntime js, string url) => js.InvokeVoidAsync("open", url, "_blank");
    public static ValueTask CopyToClipboard(this IJSRuntime js, string? text) => js.InvokeVoidAsync("navigator.clipboard.writeText", text);
}