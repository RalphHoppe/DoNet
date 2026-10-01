using System;
using System.Threading;
using System.Threading.Tasks;
using DoNet.Mvvm;
using DoNet.Services;

namespace DoNet.ViewModels;

/// <summary>
/// Drives the sign-in screen. Holds no reference to any XAML type, so it can be
/// unit-tested or reused from another shell.
/// </summary>
public sealed class LoginViewModel : ObservableObject
{
    private readonly IAuthenticationService _authentication;
    private readonly ISystemStatusService _systemStatus;

    private string _username = string.Empty;
    private string _password = string.Empty;
    private bool _isPasswordRevealed;
    private bool _isBusy;
    private string? _errorMessage;
    private string _systemStatusLabel = "Operational";
    private bool _isSystemHealthy = true;

    public LoginViewModel()
        : this(
            AppServices.GetRequired<IAuthenticationService>(),
            AppServices.GetRequired<ISystemStatusService>())
    {
    }

    public LoginViewModel(IAuthenticationService authentication, ISystemStatusService systemStatus)
    {
        _authentication = authentication;
        _systemStatus = systemStatus;

        SignInCommand = new AsyncRelayCommand(SignInAsync, () => !IsBusy);
        ContinueAsGuestCommand = new AsyncRelayCommand(ContinueAsGuestAsync, () => !IsBusy);
        ForgotPasswordCommand = new AsyncRelayCommand(RequestPasswordResetAsync, () => !IsBusy);
    }

    /// <summary>Raised once a session has been established, so the shell can navigate on.</summary>
    public event EventHandler<UserSession>? SignedIn;

    /// <summary>Raised when the user asks for the password-reset flow.</summary>
    public event EventHandler? PasswordResetRequested;

    // ---------------------------------------------------------------- state

    public string Username
    {
        get => _username;
        set
        {
            if (SetProperty(ref _username, value))
            {
                ClearError();
            }
        }
    }

    public string Password
    {
        get => _password;
        set
        {
            if (SetProperty(ref _password, value))
            {
                ClearError();
            }
        }
    }

    /// <summary>Bound to the eye toggle inside the password field.</summary>
    public bool IsPasswordRevealed
    {
        get => _isPasswordRevealed;
        set => SetProperty(ref _isPasswordRevealed, value);
    }

    /// <summary>True while a sign-in round-trip is in flight.</summary>
    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                OnPropertyChanged(nameof(IsNotBusy));
                SignInCommand.RaiseCanExecuteChanged();
                ContinueAsGuestCommand.RaiseCanExecuteChanged();
                ForgotPasswordCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool IsNotBusy => !IsBusy;

    public string? ErrorMessage
    {
        get => _errorMessage;
        private set
        {
            if (SetProperty(ref _errorMessage, value))
            {
                OnPropertyChanged(nameof(HasError));
            }
        }
    }

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public string SystemStatusLabel
    {
        get => _systemStatusLabel;
        private set => SetProperty(ref _systemStatusLabel, value);
    }

    public bool IsSystemHealthy
    {
        get => _isSystemHealthy;
        private set => SetProperty(ref _isSystemHealthy, value);
    }

    /// <summary>Footer copyright line, kept current automatically.</summary>
    public string CopyrightNotice => $"\u00A9 {DateTime.Now.Year} DoNet Labs Inc.";

    // ---------------------------------------------------------------- commands

    public AsyncRelayCommand SignInCommand { get; }

    public AsyncRelayCommand ContinueAsGuestCommand { get; }

    public AsyncRelayCommand ForgotPasswordCommand { get; }

    // ---------------------------------------------------------------- behaviour

    /// <summary>Loads deferred data. Call from the view's Loaded handler.</summary>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var status = await _systemStatus.GetCurrentStatusAsync(cancellationToken);
            SystemStatusLabel = status.Label;
            IsSystemHealthy = status.Level == SystemStatusLevel.Operational;
        }
        catch (OperationCanceledException)
        {
            // Window closed mid-flight; nothing to report.
        }
        catch
        {
            SystemStatusLabel = "Unavailable";
            IsSystemHealthy = false;
        }
    }

    private async Task SignInAsync()
    {
        ClearError();

        if (string.IsNullOrWhiteSpace(Username))
        {
            ErrorMessage = "Enter your username to continue.";
            return;
        }

        if (string.IsNullOrEmpty(Password))
        {
            ErrorMessage = "Enter your password to continue.";
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _authentication.SignInAsync(Username.Trim(), Password);

            if (result is { Succeeded: true, Session: not null })
            {
                Password = string.Empty;
                SignedIn?.Invoke(this, result.Session);
            }
            else
            {
                ErrorMessage = result.ErrorMessage ?? "We couldn't sign you in. Try again.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Sign-in failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ContinueAsGuestAsync()
    {
        ClearError();

        IsBusy = true;
        try
        {
            var result = await _authentication.ContinueAsGuestAsync();

            if (result is { Succeeded: true, Session: not null })
            {
                SignedIn?.Invoke(this, result.Session);
            }
            else
            {
                ErrorMessage = result.ErrorMessage ?? "Guest access is unavailable right now.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Guest access failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RequestPasswordResetAsync()
    {
        await _authentication.RequestPasswordResetAsync(
            string.IsNullOrWhiteSpace(Username) ? null : Username.Trim());

        PasswordResetRequested?.Invoke(this, EventArgs.Empty);
    }

    private void ClearError()
    {
        if (HasError)
        {
            ErrorMessage = null;
        }
    }
}
