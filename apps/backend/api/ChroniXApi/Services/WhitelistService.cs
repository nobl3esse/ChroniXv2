using System.Text.Json;

namespace ChroniXApi.Services
{
    public class WhitelistService
    {
        //Erstellt eine neue Liste allowedProcesses und initialisiert sie
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
                //Wenn Datei nicht exisitiert
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
            //Speichert in data eine neue Instanz von WhitelistData mit der Liste Processes initialsiert mir allowedProcesses
            var data = new WhitelistData { Processes = allowedProcesses };
            //WriteIndented bedeutet, dass die Json schön formtatiert wird mit Einrücken und Zeilenumbrüchen
            var options = new JsonSerializerOptions { WriteIndented = true };
            var jsonString = JsonSerializer.Serialize(data, options);
            //Beim aufruf, wird die Datei erstellt oder überschrieben
            File.WriteAllText(FilePath, jsonString);
        }

        public bool IsAllowed(string processName)
        {
            //Gibt true zurück wenn die liste allowedPrcoesses enthält processName
            return allowedProcesses.Contains(processName);
        }

        public List<string> GetAll()
        {
            //Gibt die Liste allowedProcesses zurück
            return allowedProcesses;
        }

        public bool Add(string processName)
        {
            if (allowedProcesses.Contains(processName))
            {
                return false;
            }

            allowedProcesses.Add(processName);
            SaveWhitelist();
            return true;
        }

        public bool Remove(string processName)
        {
            if (!allowedProcesses.Remove(processName))
            {
                return false;
            }
            SaveWhitelist();
            return true;
        }
    }
}