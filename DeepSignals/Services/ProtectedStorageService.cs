using DeepSignals.Models;
using DeepSignals.Settings.Helpers;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Http;

namespace DeepSignals.Services
{
    public class ProtectedStorageService
    { 
        //[Inject] IConfiguration _config;

        private readonly ProtectedSessionStorage _protectedSessionStorage;

        private readonly ProtectedLocalStorage _protectedLocalStorage;

        public ProtectedStorageService(ProtectedLocalStorage protectedLocalStorage, ProtectedSessionStorage protectedSessionStorage)
        {
            _protectedSessionStorage = protectedSessionStorage;
            _protectedLocalStorage = protectedLocalStorage;
        }
/*
        public async Task<int> GetTabSessionVisits()
        {
            var CurrentVisits = await SessionHelper.GetSessionIntPD(_protectedSessionStorage, "CURRENT_VISITS");

            CurrentVisits++;

            await SessionHelper.SetSessionIntPD(_protectedSessionStorage, "CURRENT_VISITS", CurrentVisits);
  
            return CurrentVisits;
        }
*/
        public async Task<string> GetTabSessionGUID()
        {
            var SESSION_GUID = await SessionHelper.GetSessionStringPD(_protectedSessionStorage, "PDSESSION_GUID");
            var CURRENT_GUID = Guid.NewGuid().ToString();

            var SET_SESSION_GUID = false;
            if (string.IsNullOrEmpty(SESSION_GUID) && SESSION_GUID != CURRENT_GUID)
            {
                SESSION_GUID = CURRENT_GUID;
                await SessionHelper.SetSessionStringPD(_protectedSessionStorage, "PDSESSION_GUID", SESSION_GUID);
            }

            return SESSION_GUID;
        }

        public async Task SetCount(int value) => await _protectedLocalStorage.SetAsync("count", value);//CurrentCount

        public async Task<ProtectedBrowserStorageResult<int>> GetCount() => await _protectedLocalStorage.GetAsync<int>("count");

        public async Task SetLastUpdate(string value) => await _protectedLocalStorage.SetAsync("LASTUPDATE", value);

        public async Task<ProtectedBrowserStorageResult<string>> GetLastUpdate() => await _protectedLocalStorage.GetAsync<string>("LASTUPDATE");

    }
}