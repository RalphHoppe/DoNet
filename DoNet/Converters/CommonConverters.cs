using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;

namespace DoNet.Converters;

/// <summary>true → Visible, false → Collapsed.</summary>
public sealed class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
        => value is true ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => value is Visibility.Visible;
}

/// <summary>true → Collapsed, false → Visible.</summary>
public sealed class InverseBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
        => value is true ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => value is not Visibility.Visible;
}

/// <summary>Drives <see cref="PasswordBox.PasswordRevealMode"/> from the eye toggle.</summary>
public sealed class BoolToPasswordRevealModeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
        => value is true ? PasswordRevealMode.Visible : PasswordRevealMode.Hidden;

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => value is PasswordRevealMode.Visible;
}
