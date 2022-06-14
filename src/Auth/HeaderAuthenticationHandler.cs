using System.Diagnostics;
using System.Security.Claims;
using System.Text.Encodings.Web;
using datotekica.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace datotekica.Auth;

public class HeaderAuthenticationHandler : SignInAuthenticationHandler<HeaderAuthenticationOptions>
{
    public HeaderAuthenticationHandler(IOptionsMonitor<HeaderAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
        : base(options, logger, encoder, clock)
    { }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        string username;
        if (Debugger.IsAttached)
            username = "developer";
        else if (Context.Request.Headers.TryGetValue(Options.RemoteUser, out var remoteuser))
            username = remoteuser;
        else
            return AuthenticateResult.NoResult();

        var usernameNormalized = username.ToLower();
        var db = Context.RequestServices.GetRequiredService<AppDbContext>();
        var dbUser = await db.Users.SingleOrDefaultAsync(u => u.UsernameNormalized == usernameNormalized);

        if (dbUser == null)
        {
            dbUser = new(usernameNormalized);
            db.Users.Add(dbUser);
            await db.SaveChangesAsync();
        }

        if (dbUser.Disabled.HasValue)
            return AuthenticateResult.Fail("User is disallowed");

        var claims = new List<Claim>();
        claims.Add(new(Claims.Subject, username));

        if (Context.Request.Headers.TryGetValue(Options.RemoteName, out var displayName))
            claims.Add(new(Claims.DisplayName, displayName));

        if (Context.Request.Headers.TryGetValue(Options.RemoteEmail, out var email))
            claims.Add(new(Claims.Email, email));

        // TODO: roles / groups
        // if (Context.Request.Headers.TryGetValue(Options.RemoteName, out var g))
        //      claims.Add(new(Claims., displayName));


        var identity = new ClaimsIdentity(claims, Scheme.Name, Claims.Subject, null);
        var user = new ClaimsPrincipal(identity);

        return AuthenticateResult.Success(new AuthenticationTicket(user, Scheme.Name));
    }

    protected override Task HandleSignInAsync(ClaimsPrincipal user, AuthenticationProperties? properties)
    {
        throw new NotImplementedException();
    }

    protected override Task HandleSignOutAsync(AuthenticationProperties? properties)
    {
        throw new NotImplementedException();
    }
    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Context.Response.Redirect(C.Routes.Forbidden);
        return Task.CompletedTask;
    }
    protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
    {
        Context.Response.Redirect(C.Routes.Forbidden);
        return Task.CompletedTask;
    }
}