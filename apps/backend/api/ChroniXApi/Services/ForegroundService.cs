using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ChroniXApi.Services
{
    public class ForegroundService
    {
        DatabaseService db = new DatabaseService();

        private WhitelistService ws;

        //Wird automatisch bei new ForegroundService() aufgerufen
        public ForegroundService(WhitelistService whitelistService)
        {
            ws = whitelistService;
            db.Initialize();
            processTime = db.LoadTimes();
        }

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
            _timer = new Timer(TrackTime, null, 0, 5000);
        }

        public void StopTracking()
        {
            foreach (var entry in processTime)
            {
                db.SaveTime(entry.Key, entry.Value);
            }
            _timer?.Dispose();
        }

        private void TrackTime(object? state)
        {
            string? processName = GetForegroundWindowProcessName();

            if (processName == null) return;

            //Zum Debuggen
            // Console.WriteLine($"Foreground: '{processName}', allowed: {ws.IsAllowed(processName)}");

            if (ws.IsAllowed(processName))
            {
                if (processTime.ContainsKey(processName))
                {
                    processTime[processName] += 5;
                }
                else
                {
                    processTime[processName] = 5;
                }
            }
        }

        public Dictionary<string, int> GetProcessTimes()
        {
            return processTime;
        }
    }
}