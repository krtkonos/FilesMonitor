namespace FileMonitorNew.Services
{
    public class FileWatcherService
    {
        private string _watchFolder = "";
        private string _stateFile = "";

        public FileWatcherService(string folderPath)
        {
            FolderPath = string.IsNullOrEmpty(folderPath) ? LoadStringStateFromFile("_folderPath") : folderPath;

            if (string.IsNullOrEmpty(FolderPath))
            {
                return;
            }

            _watchFolder = FolderPath.TrimEnd('/', '\\');
            _stateFile = Path.Combine(_watchFolder, "state.txt");
        }

        public string FolderPath { get; private set; }

        public (List<string> NewFiles, List<string> ModifiedFiles, List<string> DeletedFiles) CheckChanges()
        {
            var newFiles = new List<string>();
            var modifiedFiles = new List<string>();
            var deletedFiles = new List<string>();

            var currentState = LoadStateFromFile(_stateFile);
            var newState = AnalyzeDirectory();
            var versDict = LoadStateFromFile(Path.Combine(_watchFolder, "vers.txt"));

            foreach (var file in newState.Keys)
            {
                var fileNameWithVersion = $"{file} V.{versDict[file]}";

                if (currentState.ContainsKey(file))
                {
                    if (currentState[file] != newState[file])
                    {
                        var versionNumber = versDict[file] + 1;
                        versDict[file] = versionNumber;
                        modifiedFiles.Add($"{fileNameWithVersion} ({newState[file]})");
                    }
                }
                else
                {
                    newFiles.Add($"{fileNameWithVersion} ({newState[file]})");
                }
            }

            foreach (var file in currentState.Keys)
            {
                if (!newState.ContainsKey(file))
                {
                    var fileNameWithVersion = $"{file} V.{versDict[file]}";
                    deletedFiles.Add($"{fileNameWithVersion} ({currentState[file]})");
                    versDict.Remove(file);
                }
            }

            SaveStateToFile(newState, _stateFile);
            SaveStringStateToFile(FolderPath, "_folderPath");
            SaveStateToFile(versDict, Path.Combine(_watchFolder, "vers.txt"));
            return (newFiles, modifiedFiles, deletedFiles);
        }

        public void UpdateFolderPath(string newPath)
        {
            FolderPath = newPath;
            SaveStringStateToFile(FolderPath, "_folderPath");

        }
        private Dictionary<string, int> AnalyzeDirectory()
        {
            var state = new Dictionary<string, int>();

            if (!Directory.Exists(_watchFolder))
            {
                Directory.CreateDirectory(_watchFolder);
            }

            var versFile = Path.Combine(_watchFolder, "vers.txt");
            var versDict = LoadStateFromFile(versFile);

            foreach (var file in Directory.GetFiles(_watchFolder, "*", SearchOption.AllDirectories))
            {
                var relativePath = file.Replace(_watchFolder, "").TrimStart('/', '\\');

                if (System.IO.File.Exists(file))
                {
                    var fileInfo = new FileInfo(file);
                    var lastWriteTime = fileInfo.LastWriteTime;
                    var currentVersion = lastWriteTime.GetHashCode();

                    if (!state.ContainsKey(relativePath))
                    {
                        state[relativePath] = currentVersion;
                        if (!versDict.ContainsKey(relativePath))
                        {
                            versDict[relativePath] = 1;
                        }
                    }
                    else if (state[relativePath] != currentVersion)
                    {
                        var versionNumber = versDict[relativePath] + 1;
                        state[relativePath] = currentVersion;
                        versDict[relativePath] = versionNumber;
                    }
                }
            }

            SaveStateToFile(versDict, versFile);
            foreach (var file in state.Keys)
            {
                if (!versDict.ContainsKey(file))
                {
                    versDict[file] = 1;
                }
            }

                return state;
        }
        private void SaveStateToFile(string data, string fileName)
        {
            using (var writer = new StreamWriter(fileName))
            {
                writer.WriteLine(data);
            }
        }

        private void SaveStateToFile(Dictionary<string, int> state, string fileName)
        {
            using (var writer = new StreamWriter(fileName))
            {
                foreach (var item in state)
                {
                    writer.WriteLine($"{item.Key}={item.Value}");
                }
            }
        }

        private Dictionary<string, int> LoadStateFromFile(string fileName)
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
        private void SaveStringStateToFile(string data, string fileName)
        {
            using (var writer = new StreamWriter(fileName))
            {
                writer.WriteLine(data);
            }
        }
        private string LoadStringStateFromFile(string fileName)
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
