using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Timers;
using System.Text.Json;
using System.IO;

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

    class Program
    {
        private static readonly Dictionary<int, ProcessStat> Stats = new();
        private static readonly Dictionary<string, GroupStat> GroupStats = new(StringComparer.OrdinalIgnoreCase);
        private static readonly System.Timers.Timer Timer = new(1000);
        private static readonly string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.json");

        static void Main(string[] args)
        {
            Timer.Elapsed += OnTick;
            Timer.AutoReset = true;
            Timer.Start();

            Console.WriteLine("Tracking läuft. ENTER zum Beenden.");
            Console.ReadLine();

            Timer.Stop();
            SaveStats();

            var apps = GroupStats.Values
                .Where(x => x.IsAppGroup)
                .OrderBy(x => x.Name)
                .ToList();

            var background = GroupStats.Values
                .Where(x => !x.IsAppGroup)
                .OrderBy(x => x.Name)
                .ToList();

            Console.WriteLine($"=== Apps ({apps.Count}) ===");
            foreach (var app in apps)
            {
                Console.WriteLine(
                    $"{app.Name} ({app.ProcessCount}) | Vordergrund: {app.ForegroundSeconds}s | Hintergrund: {app.BackgroundSeconds}s");
            }

            Console.WriteLine();
            Console.WriteLine($"=== Background Processes ({background.Count}) ===");
            foreach (var bg in background)
            {
                Console.WriteLine(
                    $"{bg.Name} ({bg.ProcessCount}) | Vordergrund: {bg.ForegroundSeconds}s | Hintergrund: {bg.BackgroundSeconds}s");
            }

            Console.WriteLine();
            Console.WriteLine("Prozesse wurden in " + path + " gespeichert.");
        }

        private static void OnTick(object? sender, ElapsedEventArgs e)
        {
            int foregroundPid = GetForegroundPid();
            var running = Process.GetProcesses();

            var currentGroups = new Dictionary<string, List<ProcessStat>>(StringComparer.OrdinalIgnoreCase);
            var activePids = new HashSet<int>();

            foreach (var p in running)
            {
                try
                {
                    activePids.Add(p.Id);

                    string category = GetCategory(p);
                    string groupName = GetGroupName(p);

                    if (!Stats.ContainsKey(p.Id))
                    {
                        Stats[p.Id] = new ProcessStat
                        {
                            Pid = p.Id,
                            Name = p.ProcessName,
                            WindowTitle = p.MainWindowTitle ?? "",
                            Category = category,
                            GroupName = groupName,
                            ForegroundSeconds = 0,
                            BackgroundSeconds = 0
                        };
                    }
                    else
                    {
                        Stats[p.Id].Name = p.ProcessName;
                        Stats[p.Id].WindowTitle = p.MainWindowTitle ?? "";
                        Stats[p.Id].Category = category;
                        Stats[p.Id].GroupName = groupName;
                    }

                    if (p.Id == foregroundPid)
                        Stats[p.Id].ForegroundSeconds += 1;
                    else
                        Stats[p.Id].BackgroundSeconds += 1;

                    if (!currentGroups.ContainsKey(groupName))
                        currentGroups[groupName] = new List<ProcessStat>();

                    currentGroups[groupName].Add(Stats[p.Id]);
                }
                catch
                {
                    // Einige Prozesse können nicht vollständig gelesen werden
                }
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

                if (!GroupStats.ContainsKey(groupName))
                {
                    GroupStats[groupName] = new GroupStat
                    {
                        Name = groupName
                    };
                }

                GroupStats[groupName].ProcessCount = processes.Count;
                GroupStats[groupName].IsAppGroup = isAppGroup;

                if (isForegroundGroup)
                    GroupStats[groupName].ForegroundSeconds += 1;
                else
                    GroupStats[groupName].BackgroundSeconds += 1;
            }

            SaveStats();
        }

        private static void SaveStats()
        {
            try
            {
                var jsonData = Stats.Values
                    .OrderBy(x => x.GroupName)
                    .ThenBy(x => x.Name)
                    .ThenBy(x => x.Pid)
                    .ToList();

                string jsonString = JsonSerializer.Serialize(jsonData, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(path, jsonString);
            }
            catch
            {
                // Falls Schreiben gerade fehlschlägt, einfach ignorieren
            }
        }

        private static int GetForegroundPid()
        {
            IntPtr hwnd = Win32.GetForegroundWindow();

            if (hwnd == IntPtr.Zero)
                return -1;

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

                if (hasWindow)
                    return "App";

                return "Background Process";
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