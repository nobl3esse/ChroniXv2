using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ChroniXApi.Services
{
    public class WhitelistService
    {
        private List<string> allowedProcesses = new List<string>();
        private const string FilePath = "whitelist.json";

        public class WhitelistData
        {
            public List<string> Processes { get; set; } = new List<string>();
        }

        public void LoadWhitelist()
        {
            if (!File.Exists(FilePath))
            {
                //Early return
                return;
            }

            var jsonString = File.ReadAllText(FilePath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var data = JsonSerializer.Deserialize<WhitelistData>(jsonString, options);

            if (data != null)
            {
                allowedProcesses = data.Processes;
            }
        }

        public void SaveWhitelist()
        {
            var data = new WhitelistData { Processes = allowedProcesses };
            var options = new JsonSerializerOptions { WriteIndented = true };
            var jsonString = JsonSerializer.Serialize(data, options);
            File.WriteAllText(FilePath, jsonString);
        }

        public bool IsAllowed(string processName)
        {
            return allowedProcesses.Contains(processName);
        }

        public List<string> GetAll()
        {
            return allowedProcesses;
        }

        public void Add(string processName)
        {
            if (!allowedProcesses.Contains(processName))
            {
                allowedProcesses.Add(processName);
                SaveWhitelist();
            }
        }

        public void Remove(string processName)
        {
            if (allowedProcesses.Remove(processName))
            {
                SaveWhitelist();
            }
        }
    }
}