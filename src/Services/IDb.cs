

namespace SongsApp.Services
{
    public interface IDb
    {
        string Get(string key);
        bool Set(string key, string val);
    }
}
