using datotekica.Models;
using Microsoft.AspNetCore.Components;

namespace datotekica.Pages;

public partial class My
{
    [Parameter] public string? PageRoute { get; set; }
    DirectoryInfo? _root;
    DirectoryInfo? _current;
    List<MyFileModel> _files = new();
    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        if (user.Identity == null || !user.Identity.IsAuthenticated)
        {
            // TODO: navigate to forbidden
            return;
        }

        var username = user.Identity.Name!;
        _root = new DirectoryInfo(C.Paths.DataFor(username));
        _root.Create();

        if (string.IsNullOrWhiteSpace(PageRoute))
            _current = _root;
        else
        {
            var currentPath = Path.Combine(_root.FullName, PageRoute);
            _current = new DirectoryInfo(currentPath);
        }

        EnumerateCurrentDirectory();
    }
    void EnumerateCurrentDirectory()
    {
        if (_current == null)
            return;

        var currentPath = $"{C.Routes.MyFiles}/{PageRoute}";

        _files = _current.EnumerateFiles().Select(f => new MyFileModel(f, currentPath)).ToList();
        StateHasChanged();
    }
}