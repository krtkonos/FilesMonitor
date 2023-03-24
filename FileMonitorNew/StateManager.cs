using System.Collections.Generic;
using System.IO;

namespace FileMonitorNew.Services
{
    public class StateManager
    {
        public void SaveStateToFile(Dictionary<string, int> state, string fileName)
        {
            using (var writer = new StreamWriter(fileName))
            {
                foreach (var item in state)
                {
                    writer.WriteLine($"{item.Key}={item.Value}");
                }
            }
        }

        public Dictionary<string, int> LoadStateFromFile(string fileName)
        {
            var state = new Dictionary<string, int>();

            if (System.IO.File.Exists(fileName))
            {
                using (var reader = new StreamReader(fileName))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var parts = line.Split('=');
                        if (parts.Length == 2 && int.TryParse(parts[1], out int value))
                        {
                            state[parts[0]] = value;
                        }
                    }
                }
            }

            return state;
        }

        public void SaveStringStateToFile(string data, string fileName)
        {
            using (var writer = new StreamWriter(fileName))
            {
                writer.WriteLine(data);
            }
        }

        public string LoadStringStateFromFile(string fileName)
        {
            string data = "";

            if (System.IO.File.Exists(fileName))
            {
                using (var reader = new StreamReader(fileName))
                {
                    data = reader.ReadLine();
                }
            }

            return data;
        }
    }
}
