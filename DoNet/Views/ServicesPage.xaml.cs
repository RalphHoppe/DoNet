using System;
using DoNet.Services;
using DoNet.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.ApplicationModel.DataTransfer;

namespace DoNet.Views;

/// <summary>The Service Registry screen.</summary>
public sealed partial class ServicesPage : Page
{
    public ServicesPage()
    {
        ViewModel = new ServicesViewModel();

        InitializeComponent();

        ViewModel.CopyRequested += OnCopyRequested;
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    public ServicesViewModel ViewModel { get; }

    private async void OnLoaded(object sender, RoutedEventArgs e)
        => await ViewModel.LoadAsync();

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        ViewModel.CopyRequested -= OnCopyRequested;
        Loaded -= OnLoaded;
        Unloaded -= OnUnloaded;
    }

    private void OnCategoryChecked(object sender, RoutedEventArgs e)
    {
        if (sender is RadioButton { Tag: string tag }
            && Enum.TryParse<ServiceCategory>(tag, out var category))
        {
            ViewModel.SelectedCategory = category;
        }
    }

    /// <summary>The ⓘ button inside a row's SYSTEM chip opens the panel.</summary>
    private void OnSystemInfoClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: VpsServiceItemViewModel item })
        {
            ViewModel.ShowInspectorCommand.Execute(item);
        }
    }

    private void OnSearchFieldTapped(object sender, TappedRoutedEventArgs e)
        => SearchBox.Focus(FocusState.Programmatic);

    private void OnCopyRequested(object? sender, string text)
    {
        var package = new DataPackage();
        package.SetText(text);
        Clipboard.SetContent(package);
    }

    private async void OnNewServiceClick(object sender, RoutedEventArgs e)
    {
        if (XamlRoot is null)
        {
            return;
        }

        var dialog = new ContentDialog
        {
            XamlRoot = XamlRoot,
            Title = "New service",
            Content = "Service provisioning isn't wired up yet.",
            CloseButtonText = "Close",
            DefaultButton = ContentDialogButton.Close,
        };

        await dialog.ShowAsync();
    }
}
