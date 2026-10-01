using System;
using DoNet.Services;
using DoNet.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Windows.System;

namespace DoNet.Views;

/// <summary>
/// The DoNet sign-in screen. The view owns layout and input plumbing only —
/// all state and behaviour live in <see cref="LoginViewModel"/>.
/// </summary>
public sealed partial class LoginPage : Page
{
    public LoginPage()
    {
        ViewModel = new LoginViewModel();

        InitializeComponent();

        ViewModel.SignedIn += OnSignedIn;
        ViewModel.PasswordResetRequested += OnPasswordResetRequested;

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    public LoginViewModel ViewModel { get; }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        UsernameBox.Focus(FocusState.Programmatic);
        await ViewModel.InitializeAsync();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        ViewModel.SignedIn -= OnSignedIn;
        ViewModel.PasswordResetRequested -= OnPasswordResetRequested;
        Loaded -= OnLoaded;
        Unloaded -= OnUnloaded;
    }

    /// <summary>
    /// PasswordBox.Password is intentionally not data-bound — pushing it through a
    /// binding would leave the secret in the binding engine. The view forwards it
    /// to the view model instead.
    /// </summary>
    private void OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        ViewModel.Password = PasswordInput.Password;
    }

    /// <summary>Enter submits from either field.</summary>
    private void OnFieldKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key != VirtualKey.Enter)
        {
            return;
        }

        e.Handled = true;

        if (ViewModel.SignInCommand.CanExecute(null))
        {
            ViewModel.SignInCommand.Execute(null);
        }
    }

    /// <summary>Clicking anywhere in a field puts the caret in its input.</summary>
    private void OnUsernameFieldTapped(object sender, TappedRoutedEventArgs e)
        => UsernameBox.Focus(FocusState.Programmatic);

    private void OnPasswordFieldTapped(object sender, TappedRoutedEventArgs e)
        => PasswordInput.Focus(FocusState.Programmatic);

    // The field chrome is drawn by the surrounding Border, so focus has to be
    // reflected onto it manually.
    private void OnInputGotFocus(object sender, RoutedEventArgs e)
        => SetFieldFocused(FieldFor(sender), isFocused: true);

    private void OnInputLostFocus(object sender, RoutedEventArgs e)
        => SetFieldFocused(FieldFor(sender), isFocused: false);

    private Border? FieldFor(object sender)
    {
        if (ReferenceEquals(sender, UsernameBox))
        {
            return UsernameField;
        }

        return ReferenceEquals(sender, PasswordInput) ? PasswordField : null;
    }

    private void SetFieldFocused(Border? field, bool isFocused)
    {
        if (field is null)
        {
            return;
        }

        field.Background = Resource<Microsoft.UI.Xaml.Media.Brush>(
            isFocused ? "FieldBackgroundFocusedBrush" : "FieldBackgroundBrush");

        field.BorderBrush = Resource<Microsoft.UI.Xaml.Media.Brush>(
            isFocused ? "BrandTealEdgeBrush" : "FieldBorderBrush");
    }

    private static T Resource<T>(string key) => (T)Application.Current.Resources[key];

    private async void OnSystemStatusClick(object sender, RoutedEventArgs e)
    {
        var status = await AppServices
            .GetRequired<ISystemStatusService>()
            .GetCurrentStatusAsync();

        if (status.DetailsUri is not null)
        {
            await Launcher.LaunchUriAsync(status.DetailsUri);
        }
    }

    private void OnSignedIn(object? sender, UserSession session)
    {
        // Enter the app shell. The sign-in page is dropped from the back stack
        // so Escape / back can't return to it with a live session.
        Frame.Navigate(typeof(ShellPage), session, new DrillInNavigationTransitionInfo());
        Frame.BackStack.Clear();
    }

    private async void OnPasswordResetRequested(object? sender, EventArgs e)
    {
        await ShowDialogAsync(
            "Reset your password",
            "We'll send a reset link to the e-mail address on your account.");
    }

    private async System.Threading.Tasks.Task ShowDialogAsync(string title, string message)
    {
        if (XamlRoot is null)
        {
            return;
        }

        var dialog = new ContentDialog
        {
            XamlRoot = XamlRoot,
            Title = title,
            Content = message,
            CloseButtonText = "Close",
            DefaultButton = ContentDialogButton.Close,
        };

        await dialog.ShowAsync();
    }
}
