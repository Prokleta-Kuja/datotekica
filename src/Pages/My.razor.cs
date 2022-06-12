using datotekica.Extensions;
using datotekica.Models;
using datotekica.Services;
using datotekica.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace datotekica.Pages;

public partial class My
{
    [Inject] NavigationManager _nav { get; set; } = null!;
    [Inject] CacheService _cache { get; set; } = null!;
    [Inject] IJSRuntime _js { get; set; } = null!;
    [Parameter] public string? PageRoute { get; set; }
    bool _unauthorized;
    bool _notFound;
    string? _prevPageRoute;
    DirectoryInfo? _root;
    DirectoryInfo? _current;
    string? _currentPath;
    string? _parentPath;
    List<MyDirectoryModel> _dirs = new();
    List<MyFileModel> _files = new();
    Dictionary<string, bool> _selected = new();
    protected override async Task OnInitializedAsync()
    {
        _prevPageRoute = PageRoute;
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        if (user.Identity == null || !user.Identity.IsAuthenticated)
        {
            _unauthorized = true;
            return;
        }

        var username = user.Identity.Name!;
        _root = new DirectoryInfo(C.Paths.DataFor(username));
        _root.Create();

        EnumerateCurrentDirectory();
    }
    protected override void OnParametersSet()
    {
        if (_prevPageRoute != PageRoute)
        {
            _notFound = false;
            _prevPageRoute = PageRoute;
            EnumerateCurrentDirectory();
        }
    }
    void EnumerateCurrentDirectory()
    {
        if (_root == null)
            return;

        if (string.IsNullOrWhiteSpace(PageRoute))
            _current = _root;
        else
        {
            var currentPath = Path.Combine(_root.FullName, PageRoute);
            _current = new DirectoryInfo(currentPath);
            if (!_current.Exists)
            {
                _notFound = true;
                return;
            }
        }

        _currentPath = _nav.Uri.TrimEnd('/');
        _parentPath = _currentPath[.._currentPath.LastIndexOf('/')];

        _dirs = _current.EnumerateDirectories().Select(d => new MyDirectoryModel(d, _currentPath)).ToList();
        _files = _current.EnumerateFiles().Select(f => new MyFileModel(f, _currentPath)).ToList();
        StateHasChanged();
    }
    void AddUploadedFiles(List<FileInfo> uploaded)
    {
        if (string.IsNullOrWhiteSpace(_currentPath))
            return;

        _files.AddRange(uploaded.Select(uf => new MyFileModel(uf, _currentPath)));
        StateHasChanged();
    }
    void AddDirectory(DirectoryInfo dir)
    {
        if (!string.IsNullOrWhiteSpace(_currentPath))
            _dirs.Add(new MyDirectoryModel(dir, _currentPath));
    }
    async Task DownloadFile(string path)
    {
        var id = _cache.RegisterDownload(path, false);
        var url = C.Routes.DownloadFor(id);
        await _js.OpenNewTab(url);
    }
    void ToggleSelected(string path, bool isDir)
    {
        if (_selected.ContainsKey(path))
            _selected.Remove(path);
        else
            _selected.Add(path, isDir);
    }
    string IsSelected(string path, bool isDir) => _selected.ContainsKey(path) ? "bi bi-check-all text-warning" : isDir ? "bi bi-folder-fill" : "bi bi-file-earmark";
}