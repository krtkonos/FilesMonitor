using System.Collections.Generic;
using System.IO;

namespace FileMonitorNew.Services
{
    public class FileWatcherService
    {
        private string _watchFolder = "";
        private string _stateFile = "";
        private DirectoryAnalyzer _directoryAnalyzer;
        private StateManager _stateManager;

        public FileWatcherService(string folderPath)
        {
            _directoryAnalyzer = new DirectoryAnalyzer();
            _stateManager = new StateManager();

            FolderPath = string.IsNullOrEmpty(folderPath) ? _stateManager.LoadStringStateFromFile("_folderPath") : folderPath;

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

            var currentState = _stateManager.LoadStateFromFile(_stateFile);
            var newState = _directoryAnalyzer.AnalyzeDirectory(_watchFolder);
            var versDict = _stateManager.LoadStateFromFile(Path.Combine(_watchFolder, "vers.txt"));

            foreach (var file in newState.Keys)
            {
                if (!versDict.ContainsKey(file))
                {
                    versDict[file] = 1;
                }
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

            _stateManager.SaveStateToFile(newState, _stateFile);
            _stateManager.SaveStringStateToFile(FolderPath, "_folderPath");
            _stateManager.SaveStateToFile(versDict, Path.Combine(_watchFolder, "vers.txt"));
            return (newFiles, modifiedFiles, deletedFiles);
        }

        public void UpdateFolderPath(string newPath)
        {
            FolderPath = newPath;
            _stateManager.SaveStringStateToFile(FolderPath, "_folderPath");
        }
    }
}
