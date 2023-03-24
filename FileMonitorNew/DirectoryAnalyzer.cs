using System;
using System.Collections.Generic;
using System.IO;

namespace FileMonitorNew.Services
{
    public class DirectoryAnalyzer
    {
        public Dictionary<string, int> AnalyzeDirectory(string watchFolder)
        {
            var state = new Dictionary<string, int>();

            if (!Directory.Exists(watchFolder))
            {
                Directory.CreateDirectory(watchFolder);
            }

            foreach (var file in Directory.GetFiles(watchFolder, "*", SearchOption.AllDirectories))
            {
                var relativePath = file.Replace(watchFolder, "").TrimStart('/', '\\');

                if (System.IO.File.Exists(file))
                {
                    var fileInfo = new FileInfo(file);
                    var lastWriteTime = fileInfo.LastWriteTime;
                    var currentVersion = lastWriteTime.GetHashCode();

                    if (!state.ContainsKey(relativePath))
                    {
                        state[relativePath] = currentVersion;
                    }
                    else if (state[relativePath] != currentVersion)
                    {
                        state[relativePath] = currentVersion;
                    }
                }
            }

            return state;
        }
    }
}
