using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FileMonitorNew;
using FileMonitorNew.Services;

namespace FileMonitorNew.Pages
{
    public class IndexModel : PageModel
    {
        private FileWatcherService _fileWatcherService;

        public List<string> NewFiles { get; set; } = new List<string>();
        public List<string> ModifiedFiles { get; set; } = new List<string>();
        public List<string> DeletedFiles { get; set; } = new List<string>();
        public IndexModel()
        {
            var folderPath = LoadStringState("_folderPath");
            _fileWatcherService = new FileWatcherService(folderPath);
        }

        [BindProperty]
        public string FolderPath { get; set; }

        public void OnGet()
        {
            FolderPath = _fileWatcherService.FolderPath;

            if (string.IsNullOrEmpty(FolderPath))
            {
                return;
            }

            var changes = _fileWatcherService.CheckChanges();
            NewFiles = changes.NewFiles;
            ModifiedFiles = changes.ModifiedFiles;
            DeletedFiles = changes.DeletedFiles;
        }

        public void OnPost()
        {
            _fileWatcherService.UpdateFolderPath(Request.Form["FolderPath"].ToString());
            Response.Redirect("/");
        }

        public void OnPostShowChanges()
        {
            OnGet();
        }
        private string LoadStringState(string fileName)
        {
            if (System.IO.File.Exists(fileName))
            {
                using (var reader = new StreamReader(fileName))
                {
                    return reader.ReadLine();
                }
            }

            return null;
        }
    }
}
