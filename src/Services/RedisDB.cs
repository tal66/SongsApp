using StackExchange.Redis;

namespace SongsApp.Services
{
    public class RedisDB : IDb
    {
        private static ConnectionMultiplexer redis;
        private static IDatabase db;
        public RedisDB()
        {
            if (Config.redisEnabled)
            {
                try
                {
                    redis = ConnectionMultiplexer.Connect(
                    new ConfigurationOptions
                    {
                        EndPoints = { Config.redisEndpoint }, 

                });

                    db = redis.GetDatabase();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"problem initiating redis. {e.Message}");
                }
            }            
        }

        public string Get(string key)
        {
            if (db is null)
            {
                return "";
            }
            return db.StringGet(key.Trim().ToLower());
        }

        public bool Set(string key, string val)
        {
            if (db is null)
            {
                return false;
            }
            return db.StringSet(key.Trim().ToLower(), val, TimeSpan.FromHours(10));
        }
    }
}
