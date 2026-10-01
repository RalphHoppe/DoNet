using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DoNet.Mvvm;

/// <summary>
/// Minimal <see cref="INotifyPropertyChanged"/> base class for view models.
/// Hand-rolled so the project carries no MVVM framework dependency.
/// </summary>
public abstract class ObservableObject : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    /// <summary>
    /// Assigns <paramref name="value"/> to <paramref name="field"/> and raises
    /// <see cref="PropertyChanged"/> when the value actually changed.
    /// </summary>
    /// <returns><c>true</c> when the value changed.</returns>
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
