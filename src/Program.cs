using System.Configuration;
using SongsApp.Services;

namespace SongsApp
{
    public class Program
    {
        private static string musicRootDir;
        private static string spotifyToken;
        private static bool azureEnabled;
        private static string? azurePrivateEndpoint;
        private static string? azurePrivateKey;
        static Program()
        {
            GetSettings();
        }

        static async Task Main()
        {
            int maxResultsToDisplay = 15;
            IMusicService musicService = InitMusicService();
            IFilesService filesService = new FilesService();
            
            App app = new(musicRootDir, maxResultsToDisplay, musicService, filesService);

            while (true)
            {
                await app.Run();
                Console.WriteLine("\n*****************************\nRestart..\n");
            }
        }


        private static void GetSettings()
        {
            try
            {
                var appSettings = ConfigurationManager.AppSettings;
                musicRootDir = appSettings.Get("musicRootDir");
                spotifyToken = appSettings.Get("spotifyToken");
                azureEnabled = appSettings.Get("azureEnabled").Trim().ToLower() == "true";
                if (azureEnabled)
                {
                    azurePrivateEndpoint = appSettings.Get("azurePrivateEndpoint");
                    azurePrivateKey = appSettings.Get("azurePrivateKey");                    
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error reading app settings. {e.Message}");
                return;
            }
        }

        private static IMusicService InitMusicService()
        {
            IMusicService musicService;
            if (azureEnabled)
            {
                musicService = new MusicService(azurePrivateEndpoint, azurePrivateKey, spotifyToken);
            }
            else
            {
                musicService = new MusicService(spotifyToken);
            }

            return musicService;
        }
    }
}

