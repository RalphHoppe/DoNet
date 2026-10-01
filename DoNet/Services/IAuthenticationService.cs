using System.Threading;
using System.Threading.Tasks;

namespace DoNet.Services;

/// <summary>Identity of the signed-in principal for the current app session.</summary>
/// <param name="UserId">Stable identifier for the account.</param>
/// <param name="DisplayName">Name to show in the shell once signed in.</param>
/// <param name="IsGuest">True when the session was started via "continue as guest".</param>
public sealed record UserSession(string UserId, string DisplayName, bool IsGuest);

/// <summary>Outcome of a sign-in attempt.</summary>
public sealed record AuthenticationResult(bool Succeeded, UserSession? Session, string? ErrorMessage)
{
    public static AuthenticationResult Success(UserSession session) => new(true, session, null);

    public static AuthenticationResult Failure(string message) => new(false, null, message);
}

/// <summary>
/// Contract the sign-in screen talks to. Swap the registered implementation in
/// <see cref="AppServices"/> to point the UI at a real identity provider.
/// </summary>
public interface IAuthenticationService
{
    /// <summary>The session established by the last successful sign-in, if any.</summary>
    UserSession? CurrentSession { get; }

    Task<AuthenticationResult> SignInAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default);

    Task<AuthenticationResult> ContinueAsGuestAsync(CancellationToken cancellationToken = default);

    /// <summary>Kicks off the out-of-app password reset flow.</summary>
    Task RequestPasswordResetAsync(string? username, CancellationToken cancellationToken = default);
}
