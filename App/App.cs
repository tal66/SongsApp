
namespace SongsApp
{
    public class App
    {
        public static async Task Run()
        {
            Console.WriteLine($"Choose:\n [1] local files '{Program.musicRootDir}' [default] \n [2] spotify");
            string? source = Console.ReadLine().Trim();
            string searchData;
            
            if (!source.Equals("2"))
            {
                searchData = FindAndPlayFromPC();
            } else
            {
                searchData = await FindOnSpotifyAsync();               
            }

            bool showLyrics = AskUserIfToShowLyrics();
            if (showLyrics)
            {
                await GetAndShowLyrics(searchData);
            }
        }

        private static async Task<string> FindOnSpotifyAsync()
        {
            Console.WriteLine("\nWhat to play? (artist + track):");
            string searchString = Console.ReadLine();

            string spotifyLink = await Program.musicService.GetSpotifyLinkAsync($"{searchString}");
            if (spotifyLink.StartsWith("http"))
            {
                FilesService.OpenFile(spotifyLink);
            }
            else
            {
                Console.WriteLine($"not found\n{spotifyLink}");
            }
            return searchString;
        }

        private static string FindAndPlayFromPC()
        {
            string[] results = Array.Empty<string>();
            int nResults = 0;

            while (nResults == 0)
            {
                Console.WriteLine("\nWhat to play? (by file keywords):");
                string searchString = Console.ReadLine();

                try
                {
                    results = FilesService.SearchMp3FilesByPattern(searchString, Program.musicRootDir);
                } catch (IOException e)
                {
                    Console.WriteLine($"Error: {e.Message}");
                }
                               
                nResults = results.Length;

                Console.WriteLine($"\n{nResults} results");
            }

            var selectedResultNumber = 0;
            if (nResults > 1)
            {
                selectedResultNumber = AskUserToSelectResult(Program.maxResultsToDisplay, results);
            }

            var chosenFile = results[selectedResultNumber].Split(Path.DirectorySeparatorChar)[^1];

            Console.WriteLine("\n**************** Playing ******************\n");
            Console.WriteLine($"\t{chosenFile}\n");
            Console.WriteLine("*******************************************\n");
            FilesService.OpenFile(results[selectedResultNumber]);

            return chosenFile;
        }

        private static async Task GetAndShowLyrics(string searchTerm)
        {
            string artist = searchTerm.Split("-")[0].Trim();
            string songTitle = searchTerm.Split("-")[^1].Replace(".mp3", "").Trim();
            Song song = new(artist, songTitle);
            AskUserToConfirmOrChange(song);

            Console.WriteLine("********** Lyrics **********");

            string lyrics = await Program.musicService.GetLyricsAsync(song);

            Console.WriteLine(lyrics);
            Console.WriteLine("\n*****************************\n");
        }

        private static bool AskUserIfToShowLyrics()
        {
            Console.WriteLine($"************** Lyrics lookup **************\n");

            Console.WriteLine("Search lyrics? [y]/n");
            var answer = Console.ReadLine();

            if (!string.IsNullOrEmpty(answer) && !answer.ToLower().Trim().Equals("y"))
            {
                return false;
            }

            return true;
        }
       
        private static void AskUserToConfirmOrChange(Song song)
        {
            Console.Write($"Artist:  {song.Artist}.  Confirm <Enter>, or change to: ");
            var artistAnswer = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(artistAnswer))
            {
                song.Artist = artistAnswer;
            }

            Console.Write($"Title:   {song.Title}.  Confirm <Enter>, or change to: ");
            var songAnswer = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(songAnswer))
            {
                song.Title = songAnswer;
            }
            Console.WriteLine();
        }

        private static int AskUserToSelectResult(int maxResults, string[] results)
        {
            int nResults = results.Length;
            int selectedNum = -1;
            for (int i = 0; i < Math.Min(maxResults, nResults); i++)
            {
                var filepath = results[i];
                var filename = filepath.Split(Path.DirectorySeparatorChar)[^1];
                Console.WriteLine($" [{i}] {filename}");
            }

            bool selectionIsNum = false;
            while (!selectionIsNum || selectedNum < 0 || selectedNum >= results.Length)
            {
                Console.WriteLine("\nChoose (by number):");
                var selected = Console.ReadLine();
                selectionIsNum = int.TryParse(selected, out selectedNum);
            }
            return selectedNum;
        }
       
    }
}
