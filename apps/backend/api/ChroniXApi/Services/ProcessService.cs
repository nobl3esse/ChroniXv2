using System.Diagnostics;

namespace ChroniXApi.Services
{
    public class ProcessService
    {
        private static readonly HashSet<string> SystemUiProcesses = new()
        {
            "ApplicationFrameHost",
            "TextInputHost",
            "SystemSettings",
            "ShellExperienceHost",
            "StartMenuExperienceHost",
            "SearchHost",
            "LockApp",
            "SearchApp",
            "explorer",
            "tposd"
        };

        public string[] GetRunningProcesses()
        {
            return Process.GetProcesses()
                .Where(p => p.MainWindowHandle != IntPtr.Zero)
                .Where(p => !SystemUiProcesses.Contains(p.ProcessName))
                .Select(p => p.ProcessName)
                .Distinct()
                .ToArray();
        }
    }
}