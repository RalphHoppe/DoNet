using System;
using System.Threading;
using System.Threading.Tasks;

namespace DoNet.Services;

/// <summary>
/// Stand-in identity provider so the sign-in screen is fully interactive before a
/// backend exists. Replace the registration in <see cref="AppServices"/> with a real
/// implementation; nothing in the UI layer needs to change.
///
/// Demo rules:
///   • any well-formed e-mail address is accepted as the username
///   • the password must be at least 6 characters
///   • the password "wrong" always fails, so the error state is easy to exercise
/// </summary>
public sealed class DemoAuthenticationService : IAuthenticationService
{
    private static readonly TimeSpan SimulatedLatency = TimeSpan.FromMilliseconds(1200);

    public UserSession? CurrentSession { get; private set; }

    public async Task<AuthenticationResult> SignInAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(SimulatedLatency, cancellationToken);

        if (!LooksLikeEmail(username))
        {
            return AuthenticationResult.Failure("Enter a valid e-mail address.");
        }

        if (password.Length < 6)
        {
            return AuthenticationResult.Failure("Your password must be at least 6 characters.");
        }

        if (string.Equals(password, "wrong", StringComparison.Ordinal))
        {
            return AuthenticationResult.Failure("That username and password don't match.");
        }

        var displayName = username.Split('@')[0];
        CurrentSession = new UserSession(Guid.NewGuid().ToString("N"), displayName, IsGuest: false);
        return AuthenticationResult.Success(CurrentSession);
    }

    public async Task<AuthenticationResult> ContinueAsGuestAsync(
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(TimeSpan.FromMilliseconds(450), cancellationToken);

        CurrentSession = new UserSession(Guid.NewGuid().ToString("N"), "Guest", IsGuest: true);
        return AuthenticationResult.Success(CurrentSession);
    }

    public Task RequestPasswordResetAsync(
        string? username,
        CancellationToken cancellationToken = default)
    {
        // A real implementation would open the reset page or post to the identity API.
        return Task.CompletedTask;
    }

    private static bool LooksLikeEmail(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var at = value.IndexOf('@');
        if (at <= 0 || at == value.Length - 1)
        {
            return false;
        }

        var domain = value[(at + 1)..];
        return domain.Contains('.')
               && !domain.StartsWith('.')
               && !domain.EndsWith('.')
               && value.IndexOf('@', at + 1) < 0
               && !value.Contains(' ');
    }
}
