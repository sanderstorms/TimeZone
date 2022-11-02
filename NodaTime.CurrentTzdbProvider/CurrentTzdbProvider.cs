using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using NodaTime.TimeZones;
using System.Net;

namespace NodaTime
{
    public sealed class CurrentTzdbProvider : IDateTimeZoneProvider
    {
        private static readonly CachedAsyncLazy<CurrentTzdbProvider> Instance = 
            new CachedAsyncLazy<CurrentTzdbProvider>(TimeSpan.FromDays(1), () => DownloadAsync());

        public static int ProxyPort { get; set; }

        public static string ProxyServer { get; set; }

        private readonly IDateTimeZoneProvider _provider;
        private readonly ILookup<string, string> _aliases; 

        private CurrentTzdbProvider(IDateTimeZoneProvider provider, ILookup<string, string> aliases)
        {
            _provider = provider;
            _aliases = aliases;
        }

        public static async Task<CurrentTzdbProvider> LoadAsync(int proxyPort, string proxyServer)
        {
            ProxyPort = proxyPort;
            ProxyServer = proxyServer;

            return await Instance;
        }

        private static async Task<CurrentTzdbProvider> DownloadAsync()
        {
            //create proxy object
            var proxy = new WebProxy()
            {
                Address = new Uri($"http://{ProxyServer}:{ProxyPort}"),
                BypassProxyOnLocal = false,
                UseDefaultCredentials = false
            };

            //Create client handler which uses that proxy
            var httpClientHandler = new HttpClientHandler()
            {
                    Proxy = proxy,
            };

            //// Omit this part if you don't need to authenticate with the web server:
            //if (needServerAuthentication)
            //{
            //    httpClientHandler.PreAuthenticate = true;
            //    httpClientHandler.UseDefaultCredentials = false;

            //    // *** These creds are given to the web server, not the proxy server ***
            //    httpClientHandler.Credentials = new NetworkCredential(
            //        userName: serverUserName,
            //        password: serverPassword);
            //}

            using (var client = new HttpClient(handler: httpClientHandler, disposeHandler: true))
            {
                
                var latest = new Uri((await client.GetStringAsync("http://nodatime.org/tzdb/latest.txt")).TrimEnd());
                var fileName = latest.Segments.Last();
                var path = Path.Combine(Path.GetTempPath(), fileName);

                if (!File.Exists(path))
                {
                    using (var httpStream = await client.GetStreamAsync(latest))
                    using (var fileStream = File.Create(path))
                    {
                        await httpStream.CopyToAsync(fileStream);
                    }
                }

                using (var fileStream = File.OpenRead(path))
                {
                    var source = TzdbDateTimeZoneSource.FromStream(fileStream);
                    var provider = new DateTimeZoneCache(source);
                    return new CurrentTzdbProvider(provider, source.Aliases);
                }
            }
        }

        public ILookup<string, string> Aliases
        {
            get { return _aliases; }
        }

        public DateTimeZone GetSystemDefault()
        {
            return _provider.GetSystemDefault();
        }

        public DateTimeZone GetZoneOrNull(string id)
        {
            return _provider.GetZoneOrNull(id);
        }

        public string VersionId
        {
            get { return _provider.VersionId; }
        }

        public ReadOnlyCollection<string> Ids
        {
            get { return _provider.Ids; }
        }

        public DateTimeZone this[string id]
        {
            get { return _provider[id]; }
        }
    }
}
