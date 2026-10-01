using System;
using DoNet.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;

namespace DoNet.Views;

/// <summary>
/// The signed-in application shell: navigation rail, top header, and the frame
/// that hosts each section.
/// </summary>
public sealed partial class ShellPage : Page
{
    public ShellPage()
    {
        ViewModel = new ShellViewModel();

        InitializeComponent();

        Loaded += OnLoaded;
    }

    public ShellViewModel ViewModel { get; }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;

        // The shell draws its own branding, so the window's title-bar lockup is
        // hidden and the header row becomes the drag region.
        if (App.Shell is MainWindow window)
        {
            window.UseDragRegion(HeaderBar);
        }

        Navigate("Services");
    }

    private void OnNavChecked(object sender, RoutedEventArgs e)
    {
        if (sender is RadioButton { Tag: string section })
        {
            Navigate(section);
        }
    }

    private void Navigate(string section)
    {
        if (ContentFrame is null)
        {
            return;
        }

        var transition = new SuppressNavigationTransitionInfo();

        if (section == "Services")
        {
            if (ContentFrame.CurrentSourcePageType != typeof(ServicesPage))
            {
                ContentFrame.Navigate(typeof(ServicesPage), null, transition);
            }

            return;
        }

        ContentFrame.Navigate(typeof(ComingSoonPage), section, transition);
    }

    private async void OnHeaderSearchClick(object sender, RoutedEventArgs e)
        => await ShowPlaceholderAsync("Search", "Global search is coming soon.");

    private async void OnNewClick(object sender, RoutedEventArgs e)
        => await ShowPlaceholderAsync("New", "Pick what you'd like to create.");

    private async System.Threading.Tasks.Task ShowPlaceholderAsync(string title, string message)
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
