using SongsApp.Models;

namespace SongsApp.Services
{
    public interface IMusicService
    {
        Task<string> GetSongLyricsAsync(Song song);
        Task<string> GetSongLinkAsync(string searchString);
        Task<string> GetSongSummaryAsync(Song song);
    }
}
