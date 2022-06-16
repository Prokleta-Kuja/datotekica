using datotekica.Entities;
using datotekica.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;

namespace datotekica.Pages.Configuration;

public partial class Users
{
    [Inject] AuthenticationStateProvider _auth { get; set; } = null!;
    [Inject] IDbContextFactory<AppDbContext> _factory { get; set; } = null!;
    bool _unauthorized;
    List<User> _list = new();
    UserCreateModel? _create;
    UserEditModel? _edit;
    HashSet<string> _otherUsernames = new(StringComparer.InvariantCultureIgnoreCase);
    Dictionary<string, string>? _errors;
    protected override async Task OnInitializedAsync()
    {
        var authState = await _auth.GetAuthenticationStateAsync();
        var user = authState.User;
        if (user.Identity == null || !user.Identity.IsAuthenticated || !C.IsAdmin(user.Identity.Name!))
        {
            _unauthorized = true;
            return;
        }
        await LoadInternalSharesAsync();
    }
    async Task LoadInternalSharesAsync()
    {
        var db = await _factory.CreateDbContextAsync();
        _list = await db.Users.ToListAsync();
    }
    void AddClicked()
    {
        _edit = null;
        _errors = null;
        _create = new UserCreateModel();
        _otherUsernames = _list.Select(u => u.UsernameNormalized).ToHashSet();
    }
    void EditClicked(User item)
    {
        _create = null;
        _errors = null;
        _edit = new UserEditModel(item);
        _otherUsernames = _list
            .Where(u => u.UserId != item.UserId)
            .Select(u => u.UsernameNormalized).ToHashSet();
    }
    void Clear()
    {
        _create = null;
        _edit = null;
        _errors = null;
        _otherUsernames.Clear();
    }
    async Task SaveCreateClicked()
    {
        if (_create == null)
            return;

        _errors = _create.Validate(_otherUsernames);
        if (_errors != null)
            return;

        var item = new User(_create.Username!.ToLower());
        using var db = await _factory.CreateDbContextAsync();
        db.Users.Add(item);
        await db.SaveChangesAsync();

        _list.Insert(0, item);
        Clear();
    }
    async Task SaveEditClicked()
    {
        if (_edit == null)
            return;

        _errors = _edit.Validate(_otherUsernames);
        if (_errors != null)
            return;

        var item = _list.SingleOrDefault(x => x.UserId == _edit.UserId);
        if (item == null)
            return;

        using var db = await _factory.CreateDbContextAsync();
        db.Attach(item);
        item.UsernameNormalized = _edit.Username!.ToLower();
        item.TimezoneId = _edit.TimezoneId!;
        item.LocaleId = _edit.LocaleId!;
        if (_edit.IsDeleted)
        {
            if (!item.Disabled.HasValue)
                item.Disabled = DateTime.UtcNow;
        }
        else
            item.Disabled = null;

        await db.SaveChangesAsync();
        Clear();
    }
}