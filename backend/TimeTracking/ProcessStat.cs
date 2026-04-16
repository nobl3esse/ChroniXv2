using System.Runtime.InteropServices;

namespace TimeTracking
{
    public class ProcessStat
    {
        public int Pid { get; set; }
        public string Name { get; set; } = "";
        public string WindowTitle { get; set; } = "";
        public string Category { get; set; } = "";
        public string GroupName { get; set; } = "";
        public double ForegroundSeconds { get; set; }
        public double BackgroundSeconds { get; set; }
    }

    public class GroupStat
    {
        public string Name { get; set; } = "";
        public int ProcessCount { get; set; }
        public double ForegroundSeconds { get; set; }
        public double BackgroundSeconds { get; set; }
        public bool IsAppGroup { get; set; }
    }

    public static class Win32
    {
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);
    }
}