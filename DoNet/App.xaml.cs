using DoNet.Services;
using Microsoft.UI.Xaml;

namespace DoNet;

/// <summary>
/// Application entry point. Registers services, then opens the shell window.
/// </summary>
public partial class App : Application
{
    private Window? _window;

    public App()
    {
        InitializeComponent();
        AppServices.RegisterDefaults();
    }

    /// <summary>The shell window, available to anything that needs an owner.</summary>
    public static Window? Shell { get; private set; }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _window = new MainWindow();
        Shell = _window;
        _window.Activate();
    }
}
