
namespace SongsApp
{
    public class Song
    {
        public string Artist { get; set; }
        public string Title { get; set; }
        public string? Lyrics { get; set; }

        public Song(string artist, string title)
        {
            Artist = artist;
            Title = title;
        }
    }
}
