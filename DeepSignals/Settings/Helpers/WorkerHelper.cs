using Microsoft.Extensions.Caching.Memory;

namespace DeepSignals.Settings.Helpers
{
    public class WorkerHelper
    {
        public static string GetClassName<T>() => typeof(T).Name;

        public static void UpdateWorkerInfo<T>(ILogger<T> _logger, string _logFile, string state)
        {
            var _state = DateTime.Now.ToString("hh:mm:ss dd/MM/yyyy") + ": " + state;

            _logger.LogInformation(_state);
            FileHelper.WriteLine(_logFile, _state);
        }
    }
}