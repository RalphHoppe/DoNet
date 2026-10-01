using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DoNet.Mvvm;

/// <summary>
/// An <see cref="ICommand"/> for <c>async Task</c> handlers. While the command is
/// running it reports <see cref="CanExecute"/> as <c>false</c>, which keeps the UI
/// from firing it twice.
/// </summary>
public sealed class AsyncRelayCommand : ICommand
{
    private readonly Func<object?, Task> _execute;
    private readonly Func<object?, bool>? _canExecute;
    private bool _isRunning;

    public AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
        : this(_ => execute(), canExecute is null ? null : _ => canExecute())
    {
    }

    public AsyncRelayCommand(Func<object?, Task> execute, Func<object?, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public bool IsRunning
    {
        get => _isRunning;
        private set
        {
            if (_isRunning == value)
            {
                return;
            }

            _isRunning = value;
            RaiseCanExecuteChanged();
        }
    }

    public bool CanExecute(object? parameter)
        => !_isRunning && (_canExecute?.Invoke(parameter) ?? true);

    public async void Execute(object? parameter) => await ExecuteAsync(parameter);

    public async Task ExecuteAsync(object? parameter = null)
    {
        if (!CanExecute(parameter))
        {
            return;
        }

        IsRunning = true;
        try
        {
            await _execute(parameter);
        }
        finally
        {
            IsRunning = false;
        }
    }

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
