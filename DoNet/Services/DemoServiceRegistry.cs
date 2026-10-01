using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DoNet.Models;

namespace DoNet.Services;

/// <summary>
/// Sample inventory so the registry screen is populated before a real
/// inventory API exists.
/// </summary>
public sealed class DemoServiceRegistry : IServiceRegistry
{
    private static readonly DateTimeOffset Snapshot = DateTimeOffset.Now.AddMinutes(-2);

    private static readonly IReadOnlyList<VpsService> Services = new[]
    {
        new VpsService(
            "Production API",
            "Ubuntu 22.04 LTS",
            "192.168.1.104",
            22,
            "7Hk2-Qp91-Zx4m",
            "Intel Xeon",
            new SystemInformation(
                "Intel Xeon E-2388G", "16 cores \u00B7 3.2 GHz",
                "32 GB DDR5", "12 GB used",
                "512 GB NVMe", "128 GB used",
                "1 Gbps", "420 Mbps avg",
                "Ubuntu 22.04 LTS", "Kernel 5.15.0",
                "us-east-1", "Availability Zone A",
                "14d 02h", "0.42 / 0.58 / 0.71", "2 days ago",
                Snapshot)),

        new VpsService(
            "Staging DB",
            "Ubuntu 22.04 LTS",
            "192.168.1.105",
            22,
            "Vb8n-Lr33-Wq7t",
            "AMD EPYC",
            new SystemInformation(
                "AMD EPYC 7313P", "32 cores \u00B7 3.0 GHz",
                "64 GB DDR4", "41 GB used",
                "1 TB NVMe", "612 GB used",
                "1 Gbps", "180 Mbps avg",
                "Ubuntu 22.04 LTS", "Kernel 5.15.0",
                "us-east-1", "Availability Zone B",
                "27d 11h", "1.04 / 0.92 / 0.88", "27 days ago",
                Snapshot)),

        new VpsService(
            "CI Runner",
            "Ubuntu 22.04 LTS",
            "192.168.1.106",
            22,
            "Mf5j-Tg02-Nc6r",
            "Intel Core i7",
            new SystemInformation(
                "Intel Core i7-12700K", "12 cores \u00B7 3.6 GHz",
                "32 GB DDR5", "26 GB used",
                "512 GB NVMe", "301 GB used",
                "500 Mbps", "95 Mbps avg",
                "Ubuntu 22.04 LTS", "Kernel 5.15.0",
                "eu-west-1", "Availability Zone A",
                "3d 06h", "2.18 / 1.74 / 1.30", "3 days ago",
                Snapshot)),

        new VpsService(
            "Backup Node",
            "Ubuntu 22.04 LTS",
            "192.168.1.107",
            22,
            "Dq4w-Yb77-Ks1p",
            "ARM Graviton",
            new SystemInformation(
                "AWS Graviton3", "8 cores \u00B7 2.6 GHz",
                "16 GB DDR5", "4 GB used",
                "4 TB HDD", "2.1 TB used",
                "250 Mbps", "38 Mbps avg",
                "Ubuntu 22.04 LTS", "Kernel 5.15.0",
                "us-west-2", "Availability Zone C",
                "61d 19h", "0.08 / 0.11 / 0.09", "61 days ago",
                Snapshot)),
    };

    public async Task<IReadOnlyList<VpsService>> GetVpsServicesAsync(
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(TimeSpan.FromMilliseconds(150), cancellationToken);
        return Services;
    }
}
