using System;
using DoNet.Views;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Windows.Graphics;

namespace DoNet;

/// <summary>
/// Application shell. Owns the window chrome — custom title bar, sizing and
/// placement — and hosts the navigation frame.
/// </summary>
public sealed partial class MainWindow : Window
{
    // Logical (DIP) sizing; converted to physical pixels using the window DPI.
    private const int DefaultWidth = 1360;
    private const int DefaultHeight = 860;
    private const int MinimumWidth = 760;
    private const int MinimumHeight = 640;

    private bool _isClampingSize;

    public MainWindow()
    {
        InitializeComponent();

        ConfigureTitleBar();
        ConfigureWindow();

        RootFrame.Navigate(typeof(LoginPage));
    }

    /// <summary>
    /// Extends app content into the caption area and themes the system caption
    /// buttons so they sit invisibly on top of the starfield. Using the system
    /// buttons (rather than hand-drawn ones) keeps Snap Layouts, the window menu
    /// and accessibility behaviour intact.
    /// </summary>
    private void ConfigureTitleBar()
    {
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);

        var titleBar = AppWindow.TitleBar;

        titleBar.BackgroundColor = Colors.Transparent;
        titleBar.InactiveBackgroundColor = Colors.Transparent;
        titleBar.ForegroundColor = ColorFromHex(0xF2, 0xF7, 0xF5);
        titleBar.InactiveForegroundColor = ColorFromHex(0x8A, 0x9D, 0x98);

        titleBar.ButtonBackgroundColor = Colors.Transparent;
        titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
        titleBar.ButtonForegroundColor = ColorFromHex(0xC4, 0xD4, 0xD0);
        titleBar.ButtonInactiveForegroundColor = ColorFromHex(0x74, 0x85, 0x81);

        titleBar.ButtonHoverBackgroundColor = ColorFromHex(0x1F, 0x3A, 0x36);
        titleBar.ButtonHoverForegroundColor = ColorFromHex(0xF2, 0xF7, 0xF5);
        titleBar.ButtonPressedBackgroundColor = ColorFromHex(0x16, 0x2B, 0x28);
        titleBar.ButtonPressedForegroundColor = ColorFromHex(0xD6, 0xE2, 0xDF);
    }

    private void ConfigureWindow()
    {
        if (AppWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.PreferredMinimumWidth = MinimumWidth;
            presenter.PreferredMinimumHeight = MinimumHeight;
        }

        ResizeAndCentre();

        // Fallback clamp for runtimes where PreferredMinimum* is unavailable.
        AppWindow.Changed += OnAppWindowChanged;
    }

    private void ResizeAndCentre()
    {
        var scale = GetScaleFactor();
        var width = (int)Math.Round(DefaultWidth * scale);
        var height = (int)Math.Round(DefaultHeight * scale);

        var area = DisplayArea.GetFromWindowId(AppWindow.Id, DisplayAreaFallback.Primary);
        var work = area.WorkArea;

        // Never open larger than the display we land on.
        width = Math.Min(width, work.Width - 80);
        height = Math.Min(height, work.Height - 80);

        var x = work.X + ((work.Width - width) / 2);
        var y = work.Y + ((work.Height - height) / 2);

        AppWindow.MoveAndResize(new RectInt32(x, y, width, height));
    }

    private void OnAppWindowChanged(AppWindow sender, AppWindowChangedEventArgs args)
    {
        if (!args.DidSizeChange || _isClampingSize)
        {
            return;
        }

        var scale = GetScaleFactor();
        var minWidth = (int)Math.Round(MinimumWidth * scale);
        var minHeight = (int)Math.Round(MinimumHeight * scale);

        var width = Math.Max(sender.Size.Width, minWidth);
        var height = Math.Max(sender.Size.Height, minHeight);

        if (width == sender.Size.Width && height == sender.Size.Height)
        {
            return;
        }

        _isClampingSize = true;
        try
        {
            sender.Resize(new SizeInt32(width, height));
        }
        finally
        {
            _isClampingSize = false;
        }
    }

    /// <summary>
    /// Hands the window's drag region to an element owned by a page. The default
    /// title bar is collapsed so it stops intercepting pointer input over the
    /// page's own header.
    /// </summary>
    public void UseDragRegion(UIElement element)
    {
        AppTitleBar.Visibility = Visibility.Collapsed;
        SetTitleBar(element);
    }

    /// <summary>Restores the window's own title bar as the drag region.</summary>
    public void ResetTitleBar()
    {
        AppTitleBar.Visibility = Visibility.Visible;
        SetTitleBar(AppTitleBar);
    }

    private double GetScaleFactor()
    {
        var scale = Content?.XamlRoot?.RasterizationScale ?? 0d;
        return scale > 0 ? scale : 1.0;
    }

    private static Windows.UI.Color ColorFromHex(byte r, byte g, byte b)
        => Windows.UI.Color.FromArgb(255, r, g, b);
}
