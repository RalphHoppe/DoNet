using System;
using System.Globalization;
using DoNet.Mvvm;

namespace DoNet.ViewModels;

/// <summary>State for the shell header.</summary>
public sealed class ShellViewModel : ObservableObject
{
    /// <summary>"Wednesday, September 30" — today's date, written out.</summary>
    public string TodayLabel =>
        DateTime.Now.ToString("dddd, MMMM d", CultureInfo.CurrentCulture);
}
