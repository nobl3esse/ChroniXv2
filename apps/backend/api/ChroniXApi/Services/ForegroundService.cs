using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ChroniXApi.Services
{
    public class ForegroundService
    {
        Dictionary<string, int> processTime = new Dictionary<string, int>();
        private Timer? _timer;

        //Windows API importieren
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        public string? GetForegroundWindowProcessName()
        {
            //Welches Fenster ist Vorne
            IntPtr windowHandle = GetForegroundWindow();

            //Welche ProcessId gehört zu diesem Fenster?
            GetWindowThreadProcessId(windowHandle, out uint processId);

            //Welcher Process hat diese Id?
            Process process = Process.GetProcessById((int)processId);

            return process.ProcessName;
        }

        public void StartTracking()
        {
            _timer = new Timer(TrackTime, null, 0, 1000);
        }

        public void StopTracking()
        {
            _timer?.Dispose();
        }

        private void TrackTime(object? state)
        {
            string? processName = GetForegroundWindowProcessName();

            if (processName == null) return;

            if (processTime.ContainsKey(processName))
            {
                processTime[processName] += 1;
            }
            else
            {
                processTime[processName] = 1;
            }
        }

        public Dictionary<string, int> GetProcessTimes()
        {
            return processTime;
        }
    }
}