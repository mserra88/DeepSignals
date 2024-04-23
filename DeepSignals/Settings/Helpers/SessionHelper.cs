using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Http;

namespace DeepSignals.Settings.Helpers
{
    public class SessionHelper
    {
        public static async Task SetSessionStringPD(ProtectedSessionStorage protectedSessionStore, string key, object value) => protectedSessionStore.SetAsync(key, value);

        public static async Task<string> GetSessionStringPD(ProtectedSessionStorage protectedSessionStore, string key) => (await protectedSessionStore.GetAsync<string>(key)).Value;

        public static async Task SetSessionIntPD(ProtectedSessionStorage protectedSessionStore, string key, object value) => protectedSessionStore.SetAsync(key, value);

        public static async Task<int> GetSessionIntPD(ProtectedSessionStorage protectedSessionStore, string key) => (await protectedSessionStore.GetAsync<int>(key)).Value;

        public static string GetSessionString(HttpContext httpContext, string key) => httpContext.Session.GetString(key);

        public static void SetSessionString(HttpContext httpContext, string key, string value) => httpContext.Session.SetString(key, value);

        public static int GetSessionInt(HttpContext httpContext, string key)
        {
            int? value = httpContext.Session.GetInt32(key);

            // Realizar una conversión explícita de int? a int
            int result = value ?? 0; // Asigna 0 si el valor es nulo

            return result;
        }

        public static void SetSessionInt(HttpContext httpContext, string key, int value) => httpContext.Session.SetInt32(key, value);

        private static void RemoveSessionKey(HttpContext httpContext, string key) => httpContext.Session.Remove(key);

        /*
        private static void SetSessionInt32(IHttpContextAccessor HttpContextAccessor, string key, int value)
        {
            HttpContextAccessor.HttpContext.Session.SetInt32(key, value);
        }
        */
        /*
        private static int GetSessionInt32(IHttpContextAccessor HttpContextAccessor, string key)
        {
            int? ccc = HttpContextAccessor.HttpContext.Session.GetInt32(key);
            return ccc ?? 0;
        }
        */

    }
}