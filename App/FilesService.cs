using System.Diagnostics;
using System.Text.RegularExpressions;

namespace SongsApp
{
    public class FilesService
    {  
        public static string[] SearchMp3FilesByPattern(string searchString, string rootDir)
        {
            if (string.IsNullOrWhiteSpace(searchString))
            {
                return Array.Empty<string>();
            }

            string pattern = Regex.Replace(searchString, @"\s+", "*");
            pattern = $"*{pattern}*mp3";
            string[] results = Directory.GetFiles(rootDir, pattern, SearchOption.AllDirectories);
            return results;
        }

        public static void OpenFile(string filename)
        {
            Process process = new();
            process.StartInfo = new(filename);
            process.StartInfo.UseShellExecute = true;
            process.Start();
        }
    }
}
