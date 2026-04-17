using System.Diagnostics;

namespace ChroniXApi.Services
{
    public class ProcessService
    {
        public string[] GetRunningProcesses()
        {
            return Process.GetProcesses()
                          .Select(p => p.ProcessName)
                          .ToArray();
        }
    }
}