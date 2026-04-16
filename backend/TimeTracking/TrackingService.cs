using System.Diagnostics;
using System.Text.Json;

namespace TimeTracking
{
    public class TrackingService
    {
        private readonly Dictionary<int, ProcessStat> _stats = new();
        private readonly Dictionary<string, GroupStat> _groupStats = new(StringComparer.OrdinalIgnoreCase);
        private readonly System.Timers.Timer _timer = new(1000);
        private readonly string _path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.json");

        public void Start()
        {
            _timer.Elapsed += OnTick;
            _timer.AutoReset = true;
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
            SaveStats();
        }

        public IEnumerable<ProcessStat> GetStats() => _stats.Values
            .OrderBy(x => x.GroupName)
            .ThenBy(x => x.Name);

        public IEnumerable<GroupStat> GetGroupStats() => _groupStats.Values
            .OrderBy(x => x.Name);

        private void OnTick(object? sender, System.Timers.ElapsedEventArgs e)
        {
            int foregroundPid = GetForegroundPid();
            var running = Process.GetProcesses();

            var currentGroups = new Dictionary<string, List<ProcessStat>>(StringComparer.OrdinalIgnoreCase);

            foreach (var p in running)
            {
                try
                {
                    string category = GetCategory(p);
                    string groupName = GetGroupName(p);

                    if (!_stats.ContainsKey(p.Id))
                    {
                        _stats[p.Id] = new ProcessStat
                        {
                            Pid = p.Id,
                            Name = p.ProcessName,
                            WindowTitle = p.MainWindowTitle ?? "",
                            Category = category,
                            GroupName = groupName
                        };
                    }
                    else
                    {
                        _stats[p.Id].Name = p.ProcessName;
                        _stats[p.Id].WindowTitle = p.MainWindowTitle ?? "";
                        _stats[p.Id].Category = category;
                        _stats[p.Id].GroupName = groupName;
                    }

                    if (p.Id == foregroundPid)
                        _stats[p.Id].ForegroundSeconds += 1;
                    else
                        _stats[p.Id].BackgroundSeconds += 1;

                    if (!currentGroups.ContainsKey(groupName))
                        currentGroups[groupName] = new List<ProcessStat>();

                    currentGroups[groupName].Add(_stats[p.Id]);
                }
                catch { }
                finally
                {
                    p.Dispose();
                }
            }

            foreach (var group in currentGroups)
            {
                string groupName = group.Key;
                List<ProcessStat> processes = group.Value;

                bool isAppGroup = processes.Any(x => x.Category == "App");
                bool isForegroundGroup = processes.Any(x => x.Pid == foregroundPid);

                if (!_groupStats.ContainsKey(groupName))
                    _groupStats[groupName] = new GroupStat { Name = groupName };

                _groupStats[groupName].ProcessCount = processes.Count;
                _groupStats[groupName].IsAppGroup = isAppGroup;

                if (isForegroundGroup)
                    _groupStats[groupName].ForegroundSeconds += 1;
                else
                    _groupStats[groupName].BackgroundSeconds += 1;
            }

            SaveStats();
        }

        private void SaveStats()
        {
            try
            {
                var jsonData = _stats.Values
                    .OrderBy(x => x.GroupName)
                    .ThenBy(x => x.Name)
                    .ThenBy(x => x.Pid)
                    .ToList();

                string jsonString = JsonSerializer.Serialize(jsonData, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(_path, jsonString);
            }
            catch { }
        }

        private static int GetForegroundPid()
        {
            IntPtr hwnd = Win32.GetForegroundWindow();
            if (hwnd == IntPtr.Zero) return -1;
            Win32.GetWindowThreadProcessId(hwnd, out uint pid);
            return (int)pid;
        }

        private static string GetCategory(Process process)
        {
            try
            {
                bool hasWindow =
                    process.MainWindowHandle != IntPtr.Zero &&
                    !string.IsNullOrWhiteSpace(process.MainWindowTitle);

                return hasWindow ? "App" : "Background Process";
            }
            catch
            {
                return "Background Process";
            }
        }

        private static string GetGroupName(Process process)
        {
            string name = process.ProcessName.ToLowerInvariant();

            return name switch
            {
                "code" => "Visual Studio Code",
                "chrome" => "Google Chrome",
                "msedge" => "Microsoft Edge",
                "discord" => "Discord",
                "steam" => "Steam",
                "steamwebhelper" => "Steam",
                "githubdesktop" => "GitHub Desktop",
                "devenv" => "Visual Studio",
                "explorer" => "Windows Explorer",
                "applicationframehost" => "Windows Apps",
                "textinputhost" => "Windows Input",
                "systemsettings" => "Windows Settings",
                _ => process.ProcessName
            };
        }
    }
}