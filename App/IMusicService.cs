
namespace SongsApp
{
    public interface IMusicService
    {
        Task<string> GetLyricsAsync(Song song);
        Task<string> GetSpotifyLinkAsync(string searchString);
    }
}
