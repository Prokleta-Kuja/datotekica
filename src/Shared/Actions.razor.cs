using System.Buffers;
using datotekica.Extensions;
using datotekica.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;

namespace datotekica.Shared;

public partial class Actions
{
    [Inject] NavigationManager _nav { get; set; } = null!;
    [Inject] ToastService _toast { get; set; } = null!;
    [Inject] CacheService _cache { get; set; } = null!;
    [Inject] IJSRuntime _js { get; set; } = null!;
    [Parameter] public string BasePath { get; set; } = null!;
    [Parameter] public bool CanWrite { get; set; }
    [Parameter] public int MaxFiles { get; set; } = 128;
    [Parameter] public long MaxFileSize { get; set; } = 1024 * 1024 * 250; // MB
    [Parameter] public Dictionary<string, bool>? Selected { get; set; }
    [Parameter] public EventCallback<List<FileInfo>> OnSuccessFiles { get; set; }
    [Parameter] public EventCallback<DirectoryInfo> OnSuccessDirectory { get; set; }
    [Parameter] public EventCallback OnDeleted { get; set; }
    [Parameter] public EventCallback OnDeselect { get; set; }
    CancellationTokenSource? _cts;
    double _totalBytes;
    double _transferedBytes;
    bool _creatingDir;
    string? _newDirName;
    string? _prevQuery;
    string? _search;
    string? _sortBy;
    public string? SortBy { get => _sortBy; set { _sortBy = value; UpdateQueryString(); } }
    bool _sortDesc;
    ElementReference _newDirRef;
    static readonly char[] s_invalids = Path.GetInvalidFileNameChars();
    protected override void OnParametersSet()
    {
        var query = new Uri(_nav.Uri).Query;
        var queryChanged = _prevQuery != query;


        if (queryChanged)
        {
            _prevQuery = query;
            var parsed = QueryHelpers.ParseQuery(query);

            if (parsed.TryGetValue(C.Query.Search, out var search) && _search != search)
                _search = search;
            else
                _search = null;

            if (parsed.TryGetValue(C.Query.Sort, out var sort) && _sortBy != sort)
                _sortBy = sort;
            else
                _sortBy = null;

            if (parsed.TryGetValue(C.Query.Direction, out _) && !_sortDesc)
                _sortDesc = true;
            else
                _sortDesc = false;
        }
    }
    void ToggleDirection()
    {
        _sortDesc = !_sortDesc;
        UpdateQueryString();
    }
    void UpdateQueryString()
    {
        var query = new Dictionary<string, object?>(3);
        query.Add(C.Query.Search, string.IsNullOrWhiteSpace(_search) ? null : _search);
        query.Add(C.Query.Sort, string.IsNullOrWhiteSpace(_sortBy) ? null : _sortBy);
        query.Add(C.Query.Direction, _sortDesc ? "↑" : null);

        var uriWithQuery = _nav.GetUriWithQueryParameters(query);
        _nav.NavigateTo(uriWithQuery);
    }
    async Task UploadFiles(InputFileChangeEventArgs e)
    {
        if (!CanWrite)
            return;

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

            _toast.ShowSuccess("File(s) uploaded.");
            if (OnSuccessFiles.HasDelegate)
                await OnSuccessFiles.InvokeAsync(uploaded);
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
    async void OpenCreateDirectory()
    {
        if (!CanWrite)
            return;

        _creatingDir = true;
        await Task.Delay(1);
        await _newDirRef.FocusAsync();
    }
    async void CloseCreateDirectory() { await Task.Delay(50); _creatingDir = false; StateHasChanged(); }
    async Task CreateDirectory()
    {
        if (string.IsNullOrWhiteSpace(_newDirName))
            return;

        var validName = s_invalids.Aggregate(_newDirName, (current, c) => current.Replace(c, '_'));
        var dir = new DirectoryInfo(Path.Combine(BasePath, validName));

        if (!dir.Exists)
        {
            dir.Create();
            if (OnSuccessDirectory.HasDelegate)
                await OnSuccessDirectory.InvokeAsync(dir);
        }

        _toast.ShowSuccess($"Directory {_newDirName} created.");
        _creatingDir = false;
        _newDirName = null;
    }
    async Task DeleteSelected()
    {
        if (Selected == null)
            return;

        int dirs = 0, files = 0;
        foreach (var item in Selected)
        {
            if (item.Value) // Is dir 
            {
                var dir = new DirectoryInfo(item.Key);
                dir.Delete(true);
                dirs++;
            }
            else
            {
                var file = new FileInfo(item.Key);
                file.Delete();
                files++;
            }
            Selected.Remove(item.Key);
        }

        _toast.ShowSuccess($"Deleted {dirs} directories and {files} files.");

        if (OnDeleted.HasDelegate)
            await OnDeleted.InvokeAsync();
    }
    async Task DeselectAll()
    {
        Selected?.Clear();

        if (OnDeselect.HasDelegate)
            await OnDeselect.InvokeAsync();
    }
    async Task Download()
    {
        if (Selected == null)
            return;

        var id = _cache.RegisterDownload(Selected);
        var url = C.Routes.AttachmentFor(id);
        await _js.OpenNewTab(url);
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