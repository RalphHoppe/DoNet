using System;
using System.Threading;
using System.Threading.Tasks;

namespace DoNet.Services;

public enum SystemStatusLevel
{
    Operational,
    Degraded,
    Outage,
    Unknown,
}

/// <summary>Service-health snapshot shown in the footer and the title bar pill.</summary>
/// <param name="Level">Coarse health bucket.</param>
/// <param name="Label">Human-readable description, e.g. "Operational".</param>
/// <param name="DetailsUri">Where to send the user when they click through.</param>
public sealed record SystemStatus(SystemStatusLevel Level, string Label, Uri? DetailsUri);

/// <summary>Reports platform health for the sign-in footer.</summary>
public interface ISystemStatusService
{
    Task<SystemStatus> GetCurrentStatusAsync(CancellationToken cancellationToken = default);
}

/// <summary>Always-green stand-in until the real status endpoint is wired up.</summary>
public sealed class DemoSystemStatusService : ISystemStatusService
{
    public Task<SystemStatus> GetCurrentStatusAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(new SystemStatus(
            SystemStatusLevel.Operational,
            "Operational",
            new Uri("https://status.donet.example")));
}
