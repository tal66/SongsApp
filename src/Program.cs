using SongsApp.Services;
using Microsoft.Extensions.Configuration;

namespace SongsApp
{

    public class Program
    {
        static async Task Main()
        {
            Settings();
            IMusicService musicService = new MusicService();
            IFilesService filesService = new FilesService();
            App app = new(musicService, filesService);

            while (true)
            {
                await app.Run();
                Console.WriteLine("\n******************************************\nRestart..\n");
            }
        }


        private static void Settings()
        {
            try
            {
                ConfigurationBuilder builder = new();
                builder
                    .AddJsonFile("appsettings.json", true, true)
                    .AddJsonFile("appsettings.development.json", true, true);
                var config = builder.Build();
                
                Config.maxResultsToDisplay = 15; 
                Config.musicRootDir = config.GetSection("musicRootDir").Value;               
                Config.spotifyToken = config.GetSection("spotify:token").Value;
                Config.azureEnabled = Convert.ToBoolean(config.GetSection("azure:enabled").Value);
                Config.azurePrivateEndpoint = config.GetSection("azure:privateEndpoint").Value;
                Config.azurePrivateKey = config.GetSection("azure:privateKey").Value;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error reading app settings. {e.Message}");
            }
        }
      
    }
}

