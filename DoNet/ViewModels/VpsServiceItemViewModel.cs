using DoNet.Models;
using DoNet.Mvvm;

namespace DoNet.ViewModels;

/// <summary>
/// One row in the registry table. Wraps the immutable <see cref="VpsService"/>
/// record with the per-row UI state the table needs.
/// </summary>
public sealed class VpsServiceItemViewModel : ObservableObject
{
    private const string Mask = "\u2022\u2022\u2022\u2022\u2022\u2022\u2022\u2022\u2022\u2022\u2022\u2022";

    private bool _isPasswordRevealed;

    public VpsServiceItemViewModel(VpsService model) => Model = model;

    public VpsService Model { get; }

    public string Name => Model.Name;

    public string OperatingSystem => Model.OperatingSystem;

    public string IpAddress => Model.IpAddress;

    public string PortText => Model.Port.ToString();

    public string SystemLabel => Model.SystemLabel;

    public string Descriptor => Model.Descriptor;

    public SystemInformation Information => Model.Information;

    /// <summary>Masked until the row's eye toggle is switched on.</summary>
    public string DisplayPassword => IsPasswordRevealed ? Model.RootPassword : Mask;

    public bool IsPasswordRevealed
    {
        get => _isPasswordRevealed;
        set
        {
            if (SetProperty(ref _isPasswordRevealed, value))
            {
                OnPropertyChanged(nameof(DisplayPassword));
            }
        }
    }
}
