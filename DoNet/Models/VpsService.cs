using System;

namespace DoNet.Models;

/// <summary>
/// Hardware and platform detail for a single VPS, shown in the System
/// Information panel.
/// </summary>
public sealed record SystemInformation(
    string Cpu,
    string CpuDetail,
    string Memory,
    string MemoryDetail,
    string Disk,
    string DiskDetail,
    string Bandwidth,
    string BandwidthDetail,
    string OperatingSystem,
    string KernelDetail,
    string Region,
    string RegionDetail,
    string Uptime,
    string LoadAverage,
    string LastReboot,
    DateTimeOffset UpdatedAt);

/// <summary>A row in the VPS service registry.</summary>
public sealed record VpsService(
    string Name,
    string OperatingSystem,
    string IpAddress,
    int Port,
    string RootPassword,
    string SystemLabel,
    SystemInformation Information)
{
    /// <summary>"Production API · 192.168.1.104" — the panel's subtitle.</summary>
    public string Descriptor => $"{Name} \u00B7 {IpAddress}";
}
