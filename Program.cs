using System.Configuration;


namespace SongsApp
{
    public class Program
    {
        public static string musicRootDir = ConfigurationManager.AppSettings.Get("musicRootDir");        
        public static string spotifyToken = ConfigurationManager.AppSettings.Get("spotifyToken");
        public static int maxResultsToDisplay = 15;
        public static IMusicService musicService = new MusicService();

        static async Task Main()
        {
            while (true)
            {
                await App.Run();
                Console.WriteLine("Restart..");
            }            
        } 
    }
}

