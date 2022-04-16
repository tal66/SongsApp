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

        public string[] GetRandomMp3File(string rootDir)
        {
            string[] all = Directory.GetFiles(rootDir, "*mp3", SearchOption.AllDirectories);
            Random random = new();
            long n = random.NextInt64(0, all.Length);
            return new string[] { all[n] };
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
