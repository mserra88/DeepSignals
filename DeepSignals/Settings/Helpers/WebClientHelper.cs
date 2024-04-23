using System.Net;

namespace DeepSignals.Settings.Helpers
{
    public class WebClientHelper
    {
        private static List<string> proxyUrls = new List<string>() { Strings.Proxy.Default, Strings.Proxy.Alternative_1, Strings.Proxy.Alternative_2 };

        public static WebProxy Proxy() => new WebProxy(proxyUrls[new Random().Next(proxyUrls.Count)], true) { Credentials = CredentialCache.DefaultNetworkCredentials };

        /*
            WebClient web = new WebClient();
            if (web.Proxy != null)
            web.Proxy.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;
        */
    }
}