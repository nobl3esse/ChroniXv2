using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ChroniXApi.Services
{
    public class WhitelistService
    {

        private List<string> allowedProcesses = new List<string>();

        //Speichert den Pfad von Whitelist in einer konstanten Zeichenkette
        private const string FilePath = "whitelist.json";

        public class WhitelistData
        {
            //Erstellt eine neue Liste aus der Liste Processes die durch get und set verändert werden kann 
            public List<string> Processes { get; set; } = new List<string>();
        }

        public void LoadWhitelist()
        {
            if (!File.Exists(FilePath))
            {
                //Early return
                return;
            }

            //Ließt den Inhalt von Whitelist und speichert ihn in jsonString
            var jsonString = File.ReadAllText(FilePath);
            //Erstellt eine neue Json Option die den Serializer Case Insensitive macht
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            //Zerlegt die Json in ihre Wert und speichert sie in data
            var data = JsonSerializer.Deserialize<WhitelistData>(jsonString, options);

            //Wenn data nicht leer oder zu einem Objekt gehört
            //speicher die Processes aus data in allowedProcesses
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