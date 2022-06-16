using datotekica.Entities;
using datotekica.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;

namespace datotekica.Pages.Configuration;

public partial class InternalShares
{
    [Inject] AuthenticationStateProvider _auth { get; set; } = null!;
    [Inject] IDbContextFactory<AppDbContext> _factory { get; set; } = null!;
    bool _unauthorized;
    List<InternalShare> _list = new();
    Dictionary<int, string> _allUsers = new();
    Dictionary<int, (string username, bool canWrite)> _selectedUsers = new();
    Dictionary<int, string> _availableUsers = new();
    InternalShareCreateModel? _create;
    InternalShareEditModel? _edit;
    int _selectedUserId;
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
        _list = await db.InternalShares.Include(s => s.InternalShareUsers).ToListAsync();
        _allUsers = await db.Users.ToDictionaryAsync(u => u.UserId, u => u.UsernameNormalized);
    }
    void AddClicked()
    {
        Clear();
        _create = new InternalShareCreateModel();
    }
    void EditClicked(InternalShare item)
    {
        Clear();
        _edit = new InternalShareEditModel(item);

        foreach (var user in item.InternalShareUsers)
            _selectedUsers.Add(user.UserId, (_allUsers[user.UserId], user.CanWrite));

        foreach (var user in _allUsers)
            if (!_selectedUsers.ContainsKey(user.Key))
                _availableUsers.Add(user.Key, user.Value);
    }
    void Clear()
    {
        _create = null;
        _edit = null;
        _selectedUsers.Clear();
        _availableUsers.Clear();
    }
    void AddUser()
    {
        if (_selectedUserId == 0)
            return;

        _selectedUsers.Add(_selectedUserId, (_allUsers[_selectedUserId], false));
        _availableUsers.Remove(_selectedUserId);
        _selectedUserId = 0;
    }
    void RemoveUser(int userId)
    {
        _selectedUsers.Remove(userId);
        _availableUsers.Add(userId, _allUsers[userId]);
    }
    void ToggleWritable(int userId)
    {
        if (_selectedUsers.TryGetValue(userId, out var value))
            _selectedUsers[userId] = (value.username, !value.canWrite);
    }
    async Task SaveCreateClicked()
    {
        if (_create == null)
            return;

        _errors = _create.Validate();
        if (_errors != null)
            return;

        var item = new InternalShare(_create.Mount!, _create.Name!);
        using var db = await _factory.CreateDbContextAsync();
        db.InternalShares.Add(item);
        await db.SaveChangesAsync();

        _list.Insert(0, item);
        Clear();
    }
    async Task SaveEditClicked()
    {
        if (_edit == null)
            return;

        _errors = _edit.Validate();
        if (_errors != null)
            return;

        var item = _list.SingleOrDefault(x => x.InternalShareId == _edit.InternalShareId);
        if (item == null)
            return;

        using var db = await _factory.CreateDbContextAsync();
        db.Attach(item);
        item.Name = _edit.Name!;
        item.Mount = _edit.Mount!;

        var existingUserIds = new HashSet<int>();
        foreach (var user in item.InternalShareUsers)
        {
            existingUserIds.Add(user.UserId);
            if (_selectedUsers.TryGetValue(user.UserId, out var existingUser))
                user.CanWrite = existingUser.canWrite;
            else
                item.InternalShareUsers.Remove(user);
        }
        foreach (var user in _selectedUsers)
            if (existingUserIds.Contains(user.Key))
                continue;
            else
                item.InternalShareUsers.Add(new(user.Key, user.Value.canWrite));

        await db.SaveChangesAsync();
        Clear();
    }
}