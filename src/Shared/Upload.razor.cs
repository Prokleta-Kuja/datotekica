using System.Buffers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace datotekica.Shared;

public partial class Upload
{
    [Parameter] public string BasePath { get; set; } = null!;
    [Parameter] public EventCallback<List<FileInfo>> OnSuccess { get; set; }
    ElementReference dropZoneElement;
    InputFile? inputFile;
    IJSObjectReference _module = null!;
    IJSObjectReference _dropZoneInstance = null!;
    CancellationTokenSource? _cts;
    double _totalBytes;
    double _transferedBytes;
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _module = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./Shared/Upload.razor.js");
            _dropZoneInstance = await _module.InvokeAsync<IJSObjectReference>("initializeFileDropZone", dropZoneElement, inputFile?.Element);
        }
    }

    // Called when a new file is uploaded
    async Task OnChange(InputFileChangeEventArgs e)
    {
        _cts = new();
        var files = e.GetMultipleFiles(100); // TODO: make configurable
        if (!files.Any())
        {
            // TODO: notify nothing to download
            return;
        }

        var uploaded = new List<FileInfo>(files.Count);
        var buffer = ArrayPool<byte>.Shared.Rent(4096);
        _totalBytes = files.Sum(f => f.Size);

        await using var timer = new Timer(_ => InvokeAsync(() => StateHasChanged()));
        timer.Change(TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(500));

        try
        {
            foreach (var file in files)
            {
                var localfile = new FileInfo(Path.Combine(BasePath, file.Name));
                try
                {
                    // TODO: safe file name
                    localfile.Delete(); // TODO: remove
                    using var localStream = localfile.OpenWrite();
                    using var browserStream = file.OpenReadStream(10_024_000 * 50, _cts.Token); // TODO: make configurable

                    while (await browserStream.ReadAsync(buffer, _cts.Token) is int read && read > 0)
                    {
                        _transferedBytes += read;
                        await localStream.WriteAsync(buffer, _cts.Token);
                    }

                    uploaded.Add(localfile);
                }
                catch (OperationCanceledException)
                {
                    localfile.Delete();
                    break;
                }
                catch (System.Exception)
                {
                    // TODO: show notification
                    throw;
                }
            }

            if (OnSuccess.HasDelegate)
                await OnSuccess.InvokeAsync(uploaded);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
            _totalBytes = _transferedBytes = 0;
            StateHasChanged();
        }
    }

    // Unregister the drop zone events
    public async ValueTask DisposeAsync()
    {
        try
        {
            if (_dropZoneInstance != null)
            {
                await _dropZoneInstance.InvokeVoidAsync("dispose");
                await _dropZoneInstance.DisposeAsync();
            }

            if (_module != null)
                await _module.DisposeAsync();
        }
        catch (JSDisconnectedException) { } // If user closed the tab this will throw interop calls cannot be issued
    }
}