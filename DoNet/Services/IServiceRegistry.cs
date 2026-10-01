using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DoNet.Models;

namespace DoNet.Services;

/// <summary>The categories shown as tabs above the registry table.</summary>
public enum ServiceCategory
{
    Vps,
    Sms,
    Domain,
}

/// <summary>
/// Supplies the infrastructure inventory behind the Service Registry screen.
/// Swap the registered implementation in <see cref="AppServices"/> to read from
/// a real inventory API.
/// </summary>
public interface IServiceRegistry
{
    Task<IReadOnlyList<VpsService>> GetVpsServicesAsync(CancellationToken cancellationToken = default);
}
