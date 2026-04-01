using System.Management;

namespace FPSOptimizer.Core;

public sealed class SystemScanner : ISystemScanner
{
    // ── Entry Point ───────────────────────────────────────────────────────────

    public async Task<SystemInfo> ScanAsync(CancellationToken ct = default)
    {
        var cpuTask   = Task.Run(ScanCpu,       ct);
        var ramTask   = Task.Run(ScanRam,       ct);
        var gpuTask   = Task.Run(ScanGpu,       ct);
        var osTask    = Task.Run(ScanOs,        ct);
        var diskTask  = Task.Run(ScanDisk,      ct);
        var powerTask = Task.Run(ScanPowerPlan, ct);

        await Task.WhenAll(cpuTask, ramTask, gpuTask, osTask, diskTask, powerTask);

        return new SystemInfo
        {
            Cpu       = cpuTask.Result,
            Ram       = ramTask.Result,
            Gpu       = gpuTask.Result,
            Os        = osTask.Result,
            Disk      = diskTask.Result,
            PowerPlan = powerTask.Result,
        };
    }

    // ── CPU ──────────────────────────────────────────────────────────────────

    private static CpuInfo ScanCpu()
    {
        try
        {
            using var s  = Cimv2("SELECT Name, NumberOfCores, NumberOfLogicalProcessors FROM Win32_Processor");
            using var col = s.Get();
            var mo = col.Cast<ManagementObject>().FirstOrDefault();
            if (mo is null) return new();

            return new CpuInfo
            {
                Name    = mo["Name"]?.ToString()?.Trim()                  ?? "?",
                Cores   = Convert.ToInt32(mo["NumberOfCores"]             ?? 0),
                Threads = Convert.ToInt32(mo["NumberOfLogicalProcessors"] ?? 0),
            };
        }
        catch { return new(); }
    }

    // ── RAM ──────────────────────────────────────────────────────────────────

    private static RamInfo ScanRam()
    {
        try
        {
            double gb = 0;
            using var sysS   = Cimv2("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem");
            using var sysCol = sysS.Get();
            var sys = sysCol.Cast<ManagementObject>().FirstOrDefault();
            if (sys is not null)
                gb = Math.Round(Convert.ToDouble(sys["TotalPhysicalMemory"]) / 1_073_741_824.0, 1);

            int maxSpeed = 0;
            using var memS   = Cimv2("SELECT Speed FROM Win32_PhysicalMemory");
            using var memCol = memS.Get();
            foreach (ManagementObject mo in memCol)
                maxSpeed = Math.Max(maxSpeed, Convert.ToInt32(mo["Speed"] ?? 0));

            return new RamInfo
            {
                Gb    = gb,
                Speed = maxSpeed > 0 ? $"{maxSpeed} MHz" : "?",
            };
        }
        catch { return new(); }
    }

    // ── GPU ──────────────────────────────────────────────────────────────────
    // Fix față de original:
    //   • interogare fără filtru CurrentHorizontalResolution (pierdea unele GPU-uri)
    //   • exclus "Microsoft Basic Display" și "Remote Desktop"
    //   • prioritizare dedicat > integrat: NVIDIA > AMD > Arc > Intel iGPU
    //   • vendor detectat o singură dată, centralizat

    private static GpuInfo ScanGpu()
    {
        try
        {
            using var s   = Cimv2("SELECT Name, DriverVersion, AdapterRAM FROM Win32_VideoController");
            using var col = s.Get();

            var candidates = col
                .Cast<ManagementObject>()
                .Select(mo => new
                {
                    Name   = mo["Name"]?.ToString() ?? string.Empty,
                    Driver = mo["DriverVersion"]?.ToString() ?? "?",
                    Vram   = Convert.ToInt64(mo["AdapterRAM"] ?? 0L),
                })
                .Where(g => !string.IsNullOrWhiteSpace(g.Name)
                         && !g.Name.Contains("Microsoft Basic Display",
                                             StringComparison.OrdinalIgnoreCase)
                         && !g.Name.Contains("Remote Desktop",
                                             StringComparison.OrdinalIgnoreCase))
                .ToList();

            var best =
                candidates.FirstOrDefault(g => ContainsAny(g.Name, "NVIDIA", "GeForce")) ??
                candidates.FirstOrDefault(g => ContainsAny(g.Name, "AMD",    "Radeon"))  ??
                candidates.FirstOrDefault(g => g.Name.Contains("Arc",   StringComparison.OrdinalIgnoreCase)) ??
                candidates.FirstOrDefault(g => ContainsAny(g.Name, "Intel",  "UHD"))    ??
                candidates.FirstOrDefault();

            if (best is null) return new();

            return new GpuInfo
            {
                Name   = best.Name,
                Driver = best.Driver,
                Vram   = best.Vram > 0 ? $"{Math.Round(best.Vram / 1_073_741_824.0):0} GB" : "?",
                Vendor = ResolveVendor(best.Name),
            };
        }
        catch { return new(); }
    }

    private static GpuVendor ResolveVendor(string name)
    {
        if (ContainsAny(name, "NVIDIA", "GeForce"))      return GpuVendor.Nvidia;
        if (ContainsAny(name, "AMD",    "Radeon"))       return GpuVendor.Amd;
        if (ContainsAny(name, "Intel",  "UHD",   "Arc")) return GpuVendor.Intel;
        return GpuVendor.Unknown;
    }

    // ── OS ───────────────────────────────────────────────────────────────────

    private static OsInfo ScanOs()
    {
        try
        {
            using var s   = Cimv2("SELECT Caption, BuildNumber FROM Win32_OperatingSystem");
            using var col = s.Get();
            var mo = col.Cast<ManagementObject>().FirstOrDefault();
            if (mo is null) return new();

            return new OsInfo
            {
                Name  = mo["Caption"]?.ToString() ?? "Windows",
                Build = Convert.ToInt32(mo["BuildNumber"] ?? 0),
            };
        }
        catch { return new(); }
    }

    // ── Disk ─────────────────────────────────────────────────────────────────

    private static DiskInfo ScanDisk()
    {
        double freeGb = 0, totalGb = 0;
        bool   isSsd  = false;

        try
        {
            using var s   = Cimv2("SELECT FreeSpace, Size FROM Win32_LogicalDisk WHERE DeviceID='C:'");
            using var col = s.Get();
            var mo = col.Cast<ManagementObject>().FirstOrDefault();
            if (mo is not null)
            {
                freeGb  = Math.Round(Convert.ToDouble(mo["FreeSpace"]) / 1_073_741_824.0, 1);
                totalGb = Math.Round(Convert.ToDouble(mo["Size"])      / 1_073_741_824.0, 0);
            }
        }
        catch { /* continuă fără spațiu disk */ }

        try
        {
            // Namespace separat — poate lipsi pe unele sisteme
            var scope = new ManagementScope(@"\\.\ROOT\Microsoft\Windows\Storage");
            scope.Connect();
            using var s   = new ManagementObjectSearcher(scope, new ObjectQuery("SELECT MediaType FROM MSFT_PhysicalDisk"));
            using var col = s.Get();
            var mo = col.Cast<ManagementObject>().FirstOrDefault();
            if (mo is not null)
                isSsd = Convert.ToInt16(mo["MediaType"] ?? 0) == 4;
        }
        catch { /* namespace Storage WMI indisponibil → IsSsd rămâne false */ }

        return new DiskInfo { FreeGb = freeGb, TotalGb = totalGb, IsSsd = isSsd };
    }

    // ── Power Plan ───────────────────────────────────────────────────────────

    private string ScanPowerPlan()
    {
        var output = RunCommand("powercfg", "/getactivescheme");

        if (output.Contains("Ultimate",     StringComparison.OrdinalIgnoreCase)) return "Ultimate Performance ✓";
        if (output.Contains("High perform", StringComparison.OrdinalIgnoreCase)) return "High Performance";
        if (output.Contains("Balanced",     StringComparison.OrdinalIgnoreCase)) return "Balanced (neoptimal)";
        return "Personalizat";
    }

    // ── Utilities ────────────────────────────────────────────────────────────

    private static ManagementObjectSearcher Cimv2(string wql)
    {
        var scope = new ManagementScope(@"\\.\root\CIMV2");
        scope.Connect();
        return new ManagementObjectSearcher(scope, new ObjectQuery(wql));
    }

    private static bool ContainsAny(string source, params string[] terms)
        => terms.Any(t => source.Contains(t, StringComparison.OrdinalIgnoreCase));

    public string RunCommand(string executable, string arguments)
    {
        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo(executable, arguments)
            {
                RedirectStandardOutput = true,
                UseShellExecute        = false,
                CreateNoWindow         = true,
            };
            using var proc = System.Diagnostics.Process.Start(psi);
            return proc?.StandardOutput.ReadToEnd() ?? string.Empty;
        }
        catch { return string.Empty; }
    }
}