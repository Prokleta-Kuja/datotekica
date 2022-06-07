using Microsoft.AspNetCore.Authentication;

namespace datotekica.Auth;

public class HeaderAuthenticationOptions : AuthenticationSchemeOptions
{
    public const string DEFAULT_SCHEME = "Header";
    public string RemoteUser { get; }
    public string RemoteGroups { get; }
    public string RemoteName { get; }
    public string RemoteEmail { get; }

    public HeaderAuthenticationOptions()
    {
        RemoteUser = C.Env.HeaderUser;
        RemoteGroups = C.Env.HeaderGroups;
        RemoteName = C.Env.HeaderName;
        RemoteEmail = C.Env.HeaderEmail;
    }
}