using BepInEx.Logging;

namespace ValheimLib
{
    internal static class Log
    {
        internal static ManualLogSource _logSource;

        internal static void Init(ManualLogSource logSource)
        {
            _logSource = logSource;
        }

        internal static void LogDebug(object data) => _logSource.LogDebug(data);
        internal static void LogError(object data) => _logSource.LogError(data);
        internal static void LogFatal(object data) => _logSource.LogFatal(data);
        internal static void LogInfo(object data) => _logSource.LogInfo(data);
        internal static void LogMessage(object data) => _logSource.LogMessage(data);
        internal static void LogWarning(object data) => _logSource.LogWarning(data);
    }
}
