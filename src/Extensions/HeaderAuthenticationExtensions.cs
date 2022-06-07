using datotekica.Auth;
using Microsoft.AspNetCore.Authentication;

namespace datotekica.Extensions;

public static class HeaderAuthenticationExtensions
{
    public static AuthenticationBuilder AddHeader(this AuthenticationBuilder builder)
        => builder.AddHeader(HeaderAuthenticationOptions.DEFAULT_SCHEME);
    public static AuthenticationBuilder AddHeader(this AuthenticationBuilder builder, string authenticationScheme)
        => builder.AddHeader(authenticationScheme, configureOptions: null!);
    public static AuthenticationBuilder AddHeader(this AuthenticationBuilder builder, Action<HeaderAuthenticationOptions> configureOptions)
        => builder.AddHeader(HeaderAuthenticationOptions.DEFAULT_SCHEME, configureOptions);
    public static AuthenticationBuilder AddHeader(this AuthenticationBuilder builder, string authenticationScheme, Action<HeaderAuthenticationOptions> configureOptions)
        => builder.AddHeader(authenticationScheme, displayName: null, configureOptions: configureOptions);
    public static AuthenticationBuilder AddHeader(this AuthenticationBuilder builder, string authenticationScheme, string? displayName, Action<HeaderAuthenticationOptions> configureOptions)
        => builder.AddScheme<HeaderAuthenticationOptions, HeaderAuthenticationHandler>(authenticationScheme, displayName, configureOptions);
}