using StackExchange.Redis;
using System;

namespace Redinsgo
{
  public class ConnectionHelper
  {
    private static readonly Lazy<ConnectionMultiplexer> lazyConnection;

    static ConnectionHelper()
    {
      lazyConnection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(new ConfigurationOptions { EndPoints = { "localhost" } }));
    }

    private static ConnectionMultiplexer Connection => lazyConnection.Value;

    public static IDatabase RedisCache => Connection.GetDatabase();
  }
}
