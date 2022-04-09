using System.Diagnostics;
using System.Text.RegularExpressions;

namespace SongsApp.Services
{
    public class FilesService : IFilesService
    {
        private static readonly Regex spacesRegex = new(@"\s+", RegexOptions.Compiled);       
        
        public string[] SearchMp3FilesByPattern(string searchString, string rootDir)
        {
            if (string.IsNullOrWhiteSpace(searchString))
            {
                return Array.Empty<string>();
            }

            string pattern = spacesRegex.Replace(searchString,  "*");
            pattern = $"*{pattern}*mp3";
            string[] results = Directory.GetFiles(rootDir, pattern, SearchOption.AllDirectories);
            return results;
        }

        public void OpenFile(string filename)
        {
            Process process = new();
            process.StartInfo = new(filename);
            process.StartInfo.UseShellExecute = true;
            process.Start();
        }
    }
}
