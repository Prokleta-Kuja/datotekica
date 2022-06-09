using datotekica.Models;
using Microsoft.AspNetCore.Components;

namespace datotekica.Pages;

public partial class My
{
    [Inject] NavigationManager _nav { get; set; } = null!;
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
}