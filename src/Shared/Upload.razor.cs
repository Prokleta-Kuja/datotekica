using System.Buffers;
using datotekica.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace datotekica.Shared;

public partial class Upload
{
    [Inject] ToastService _toast { get; set; } = null!;
    [Parameter] public string BasePath { get; set; } = null!;
    [Parameter] public string Text { get; set; } = "Upload files";
    [Parameter] public int MaxFiles { get; set; } = 128;
    [Parameter] public long MaxFileSize { get; set; } = 1024 * 1024 * 250; // MB
    [Parameter] public EventCallback<List<FileInfo>> OnSuccess { get; set; }
    CancellationTokenSource? _cts;
    double _totalBytes;
    double _transferedBytes;
    static char[] s_invalids = Path.GetInvalidFileNameChars();
    async Task OnChange(InputFileChangeEventArgs e)
    {
        _cts = new();
        var files = e.GetMultipleFiles(MaxFiles);
        if (!files.Any())
        {
            _toast.ShowWarning("Nothing to upload");
            return;
        }

        var uploaded = new List<FileInfo>(files.Count);
        var buffer = ArrayPool<byte>.Shared.Rent(1024 * 16);
        _totalBytes = files.Sum(f => f.Size);

        await using var timer = new Timer(_ => InvokeAsync(() => StateHasChanged()));
        timer.Change(TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(500));

        try
        {
            foreach (var file in files)
            {
                var localfile = GetLocalFile(file.Name);
                try
                {
                    using var localStream = localfile.OpenWrite();
                    using var browserStream = file.OpenReadStream(MaxFileSize, _cts.Token);

                    while (await browserStream.ReadAsync(buffer, _cts.Token) is int read && read > 0)
                    {
                        _transferedBytes += read;
                        await localStream.WriteAsync(buffer, _cts.Token);
                    }

                    localfile.Refresh();
                    uploaded.Add(localfile);
                }
                catch (OperationCanceledException)
                {
                    localfile.Delete();
                    break;
                }
                catch (Exception)
                {
                    _toast.ShowError($"Error occured while uploading file {file.Name}");
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
    FileInfo GetLocalFile(string uploadName)
    {
        var validName = s_invalids.Aggregate(uploadName, (current, c) => current.Replace(c, '_'));
        var file = new FileInfo(Path.Combine(BasePath, validName));

        var ext = Path.GetExtension(validName);
        var count = 0;
        while (file.Exists)
        {
            var name = Path.GetFileNameWithoutExtension(validName);
            file = new FileInfo(Path.Combine(BasePath, $"{name}_{++count}{ext}"));
        }

        return file;
    }
}