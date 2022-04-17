using SongsApp.Services;
using SongsApp.Models;

namespace SongsApp
{
    public class App
    {
        private string musicRootDir;
        private int maxResultsToDisplay;
        private IMusicService musicService;
        private IFilesService filesService;
        private IDb db;

        public App(IMusicService musicService, IFilesService filesService, IDb db)
        {
            this.musicRootDir = Config.musicRootDir;
            this.maxResultsToDisplay = Config.maxResultsToDisplay;            
            this.musicService = musicService;
            this.filesService = filesService;
            this.db = db;

        }

        public async Task Run()
        {
            Console.WriteLine($"Choose:\n [1] local files '{musicRootDir}' [default] \n [2] spotify");
            string? sourceNumber = Console.ReadLine().Trim();
            string searchData;
            
            if (sourceNumber.Equals("2"))
            {
                searchData = await FindSongLinkAndOpenAsync(); 
            } else
            {
                searchData = FindSongFileAndPlay();
            }

            bool showLyrics = AskUserIfToShowLyrics();
            if (showLyrics)
            {
                Song song = await GetAndShowLyrics(searchData);
                await GetAndShowSummary(song);               
            }
        }

        private async Task<string> FindSongLinkAndOpenAsync()
        {
            Console.WriteLine("\nWhat to play? (artist + track | some lyrics):");
            string searchString = Console.ReadLine();

            string serviceLink = await musicService.GetSongLinkAsync(searchString);
            if (serviceLink.StartsWith("http"))
            {
                filesService.OpenFile(serviceLink);
            }
            else
            {
                Console.WriteLine($"not found\n{serviceLink}");
            }
            return searchString;
        }

        private string FindSongFileAndPlay()
        {
            string[] results = AskUserForPatternsAndSearchUntilFound();
            int nResults = results.Length;
            
            var selectedResultNumber = 0;
            if (nResults > 1)
            {
                selectedResultNumber = AskUserToSelectResult(maxResultsToDisplay, results);
            }

            var chosenFile = results[selectedResultNumber].Split(Path.DirectorySeparatorChar)[^1];

            Console.WriteLine("\n**************** Playing ******************\n");
            Console.WriteLine($"\t{chosenFile}\n");
            Console.WriteLine("*******************************************\n");
            filesService.OpenFile(results[selectedResultNumber]);

            return chosenFile;

            string[] AskUserForPatternsAndSearchUntilFound()
            {
                string[] results = Array.Empty<string>();
                int nResults = 0;

                while (nResults == 0)
                {
                    Console.WriteLine("\nWhat to play? (file keywords. [empty = random]):");
                    string searchString = Console.ReadLine();

                    try
                    {
                        if (string.IsNullOrWhiteSpace(searchString))
                        {
                            results = filesService.GetRandomMp3File(musicRootDir);
                        }
                        else
                        {
                            results = filesService.SearchMp3FilesByPattern(searchString, musicRootDir);

                        }
                    }
                    catch (IOException e)
                    {
                        Console.WriteLine($"Error: {e.Message}");
                    }

                    nResults = results.Length;

                    Console.WriteLine($"\n{nResults} results");
                }

                return results;
            }

            int AskUserToSelectResult(int maxResults, string[] results)
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
       
        private static bool AskUserIfToShowLyrics()
        {
            Console.WriteLine($"\n************** Lyrics lookup **************\n");

            Console.WriteLine("Search lyrics? [y]/n");
            var answer = Console.ReadLine();

            if (!string.IsNullOrEmpty(answer) && !answer.ToLower().Trim().Equals("y"))
            {
                return false;
            }

            return true;
        }

        private async Task<Song> GetAndShowLyrics(string searchTerm)
        {
            string artist = searchTerm.Split("-")[0].Trim();
            string songTitle = searchTerm.Split("-")[^1].Replace(".mp3", "").Trim();
            Song song = new(artist, songTitle);

            AskUserToConfirmOrChange(song);

            Console.WriteLine("********** Lyrics **********");

            string key = $"{artist}:{songTitle}";
            string lyrics = db.Get(key);
            if (string.IsNullOrWhiteSpace(lyrics))
            {
                lyrics = await musicService.GetSongLyricsAsync(song);
                db.Set(key, lyrics);
            } 
      
            song.Lyrics = lyrics;

            Console.WriteLine(lyrics);
            Console.WriteLine("\n*****************************\n");
            return song;

            void AskUserToConfirmOrChange(Song song)
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
        }

        private async Task<string> GetAndShowSummary(Song song)
        {
            string key = $"{song.Artist}:{song.Title}:summary";
            string summary = db.Get(key);
            if (string.IsNullOrWhiteSpace(summary))
            {
                summary = await musicService.GetSongSummaryAsync(song);
                db.Set(key, summary);
            }

            Console.WriteLine(summary);
            return summary;
        }

    }
}
