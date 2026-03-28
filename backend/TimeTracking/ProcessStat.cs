using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Timers;

namespace TimeTracking
{
    public class ProcessStat
    {
        public int Pid { get; set; }
        public string Name { get; set; } = "";
        public double ForegroundSeconds { get; set; }
        public double BackgroundSeconds { get; set; }
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
        private static readonly System.Timers.Timer Timer = new(1000);

        static void Main(string[] args)
        {
            Timer.Elapsed += OnTick;
            Timer.AutoReset = true;
            Timer.Start();

            Console.WriteLine("Tracking läuft. ENTER zum Beenden.");
            Console.ReadLine();

            Timer.Stop();

            foreach(var stat in Stats.Values.OrderByDescending(s => s.ForegroundSeconds))
            {
                Console.WriteLine(
                    $"{stat.Name} ({stat.Pid}) | Vordergrund: {stat.ForegroundSeconds}s | Hintergrund: {stat.BackgroundSeconds}s");
            }
        }

        private static void OnTick(object? sender, ElapsedEventArgs e)
        {
            int foregroundPid = GetForegroundPid();
            var running = Process.GetProcesses();

            foreach (var p in running)
            {
                try
                {
                    if (!Stats.ContainsKey(p.Id))
                    {
                        Stats[p.Id] = new ProcessStat
                        {
                            Pid = p.Id,
                            Name = p.ProcessName
                        };
                    }

                    if(p.Id == foregroundPid)
                    {
                        Stats[p.Id].ForegroundSeconds += 1;
                    }
                    else
                    {
                        Stats[p.Id].BackgroundSeconds += 1;
                    }
                }
                catch
                {
                    // Einige Prozesse können nicht vollständig gelesen werden
                }
            }
        }

        private static int GetForegroundPid()
        {
            IntPtr hwnd = Win32.GetForegroundWindow();

            if(hwnd == IntPtr.Zero)
            {
                return -1;
            }

            Win32.GetWindowThreadProcessId(hwnd, out uint pid);
            return (int)pid;
        }
    }
}
