namespace FPSOptimizer.Core;

public interface ISystemScanner
{
    Task<SystemInfo> ScanAsync(CancellationToken ct = default);
    string RunCommand(string executable, string arguments);
}