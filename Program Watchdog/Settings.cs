using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Program_Watchdog
{
    public class Settings
    {
        public string Program { get; set; }

        public string RestartTime { get; set; }

        public Settings()
        {
        }

        public static Settings ParseFromJson(string json)
        {
            return JsonConvert.DeserializeObject<Settings>(json);
        }

        public void SaveToFile(String fileName)
        {
            String json = ToJson();
            File.WriteAllText(fileName, json);
        }

        public static Settings ParseFromFile(String fileName)
        {
            if (!File.Exists(fileName))
                throw new FileNotFoundException("The file was not found.");

            String json = File.ReadAllText(fileName);
            return Settings.ParseFromJson(json);
        }

        public string ToJson()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented
            };
            return JsonConvert.SerializeObject(this, settings);
        }

        public override string ToString()
        {
            return ToJson();
        }
    }
}
