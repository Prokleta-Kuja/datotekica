using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Options;

namespace datotekica.Auth;

public class HeaderAuthenticationHandler : SignInAuthenticationHandler<HeaderAuthenticationOptions>
{
    public HeaderAuthenticationHandler(IOptionsMonitor<HeaderAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
        : base(options, logger, encoder, clock)
    { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new List<Claim> { new(Claims.Subject, "developer") };
        // if (!Context.Request.Headers.TryGetValue(Options.RemoteUser, out var username))
        //     return Task.FromResult(AuthenticateResult.NoResult());

        // var claims = new List<Claim> { new(Claims.Subject, username) };

        // if (Context.Request.Headers.TryGetValue(Options.RemoteName, out var displayName))
        //     claims.Add(new(Claims.DisplayName, displayName));

        // if (Context.Request.Headers.TryGetValue(Options.RemoteEmail, out var email))
        //     claims.Add(new(Claims.Email, email));

        // TODO: roles/groups
        // if (Context.Request.Headers.TryGetValue(Options.RemoteName, out var g))
        //     claims.Add(new(Claims., displayName));

        System.Diagnostics.Debug.WriteLine(Context.Request.GetDisplayUrl());
        var identity = new ClaimsIdentity(claims, Scheme.Name, Claims.Subject, null);
        var user = new ClaimsPrincipal(identity);

        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(user, Scheme.Name)));
    }

    protected override Task HandleSignInAsync(ClaimsPrincipal user, AuthenticationProperties? properties)
    {
        throw new NotImplementedException();
    }

    protected override Task HandleSignOutAsync(AuthenticationProperties? properties)
    {
        throw new NotImplementedException();
    }
}