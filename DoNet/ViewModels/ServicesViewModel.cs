using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DoNet.Models;
using DoNet.Mvvm;
using DoNet.Services;

namespace DoNet.ViewModels;

/// <summary>Drives the Service Registry screen.</summary>
public sealed class ServicesViewModel : ObservableObject
{
    private readonly IServiceRegistry _registry;

    private IReadOnlyList<VpsServiceItemViewModel> _allServices = Array.Empty<VpsServiceItemViewModel>();
    private ServiceCategory _selectedCategory = ServiceCategory.Vps;
    private string _searchText = string.Empty;
    private VpsServiceItemViewModel? _inspectedService;
    private bool _isLoading;

    public ServicesViewModel()
        : this(AppServices.GetRequired<IServiceRegistry>())
    {
    }

    public ServicesViewModel(IServiceRegistry registry)
    {
        _registry = registry;

        CloseInspectorCommand = new RelayCommand(() => InspectedService = null);
        ShowInspectorCommand = new RelayCommand(parameter =>
        {
            if (parameter is VpsServiceItemViewModel item)
            {
                InspectedService = item;
            }
        });
        CopyDetailsCommand = new RelayCommand(CopyDetails, () => InspectedService is not null);
    }

    /// <summary>Raised with the text to place on the clipboard.</summary>
    public event EventHandler<string>? CopyRequested;

    public ObservableCollection<VpsServiceItemViewModel> Services { get; } = new();

    public RelayCommand ShowInspectorCommand { get; }

    public RelayCommand CloseInspectorCommand { get; }

    public RelayCommand CopyDetailsCommand { get; }

    // ---------------------------------------------------------------- state

    public ServiceCategory SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            if (SetProperty(ref _selectedCategory, value))
            {
                InspectedService = null;
                OnPropertyChanged(nameof(IsVpsSelected));
                ApplyFilter();
            }
        }
    }

    public bool IsVpsSelected => SelectedCategory == ServiceCategory.Vps;

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                ApplyFilter();
            }
        }
    }

    /// <summary>The row whose System Information panel is open, if any.</summary>
    public VpsServiceItemViewModel? InspectedService
    {
        get => _inspectedService;
        private set
        {
            if (SetProperty(ref _inspectedService, value))
            {
                OnPropertyChanged(nameof(IsInspectorOpen));
                CopyDetailsCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool IsInspectorOpen => InspectedService is not null;

    public bool IsLoading
    {
        get => _isLoading;
        private set => SetProperty(ref _isLoading, value);
    }

    /// <summary>True when the selected tab has nothing to show.</summary>
    public bool IsEmpty => !IsLoading && Services.Count == 0;

    /// <summary>"Showing 4 active VPS services".</summary>
    public string SummaryText => SelectedCategory switch
    {
        ServiceCategory.Vps => $"Showing {Services.Count} active VPS {Plural(Services.Count)}",
        ServiceCategory.Sms => "No SMS services configured yet",
        _ => "No domains configured yet",
    };

    public string EmptyStateTitle => SelectedCategory switch
    {
        ServiceCategory.Sms => "No SMS services yet",
        ServiceCategory.Domain => "No domains yet",
        _ => "Nothing matches your search",
    };

    public string EmptyStateMessage => SelectedCategory switch
    {
        ServiceCategory.Sms => "Connect an SMS gateway to manage it from here.",
        ServiceCategory.Domain => "Add a domain to track its DNS and certificates here.",
        _ => "Try a different name, IP address or system.",
    };

    // ---------------------------------------------------------------- behaviour

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        IsLoading = true;
        try
        {
            var services = await _registry.GetVpsServicesAsync(cancellationToken);
            _allServices = services.Select(s => new VpsServiceItemViewModel(s)).ToList();
            ApplyFilter();
        }
        catch (OperationCanceledException)
        {
            // Navigated away mid-load.
        }
        finally
        {
            IsLoading = false;
            OnPropertyChanged(nameof(IsEmpty));
        }
    }

    private void ApplyFilter()
    {
        Services.Clear();

        if (SelectedCategory == ServiceCategory.Vps)
        {
            foreach (var service in _allServices.Where(Matches))
            {
                Services.Add(service);
            }
        }

        OnPropertyChanged(nameof(SummaryText));
        OnPropertyChanged(nameof(EmptyStateTitle));
        OnPropertyChanged(nameof(EmptyStateMessage));
        OnPropertyChanged(nameof(IsEmpty));
    }

    private bool Matches(VpsServiceItemViewModel item)
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            return true;
        }

        var term = SearchText.Trim();
        return Contains(item.Name, term)
               || Contains(item.IpAddress, term)
               || Contains(item.SystemLabel, term)
               || Contains(item.OperatingSystem, term);
    }

    private static bool Contains(string source, string term)
        => source.Contains(term, StringComparison.OrdinalIgnoreCase);

    private void CopyDetails()
    {
        if (InspectedService is not { } item)
        {
            return;
        }

        var info = item.Information;
        var text = new StringBuilder()
            .AppendLine(item.Descriptor)
            .AppendLine($"CPU: {info.Cpu} ({info.CpuDetail})")
            .AppendLine($"Memory: {info.Memory} ({info.MemoryDetail})")
            .AppendLine($"Disk: {info.Disk} ({info.DiskDetail})")
            .AppendLine($"Bandwidth: {info.Bandwidth} ({info.BandwidthDetail})")
            .AppendLine($"OS: {info.OperatingSystem} ({info.KernelDetail})")
            .AppendLine($"Region: {info.Region} ({info.RegionDetail})")
            .AppendLine($"Uptime: {info.Uptime}")
            .AppendLine($"Load average: {info.LoadAverage}")
            .AppendLine($"Last reboot: {info.LastReboot}")
            .ToString();

        CopyRequested?.Invoke(this, text);
    }

    private static string Plural(int count) => count == 1 ? "service" : "services";
}
