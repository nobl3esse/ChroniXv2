using System.Diagnostics;

namespace ChroniXApi.Services
{
    public class ProcessService
    {
        public string[] GetRunningProcesses()
        {
            return Process.GetProcesses()
                .Where(p => p.MainWindowHandle != IntPtr.Zero)
                .Select(p => p.ProcessName)
                .Distinct()
                .ToArray();
        }
    }
}