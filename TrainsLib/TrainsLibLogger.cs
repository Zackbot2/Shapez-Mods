using System;
using System.Collections.Generic;
using System.Text;
using ILogger = Core.Logging.ILogger;

namespace TrainsLib
{
    internal static class TrainsLibLogger
    {
        private static string ModName => TrainsLibMod.ModName;
        private static ILogger Logger => TrainsLibMod.Logger;

        public static void LogInfo(string message)
        {
            TrainsLibMod.Logger.Info?.Log($"{TrainsLibMod.ModName}: {message}");
        }

        public static void LogWarning(string message)
        {
            TrainsLibMod.Logger.Warning?.Log($"[WARN] {TrainsLibMod.ModName}: {message}");
        }

        public static void LogError(string message)
        {
            TrainsLibMod.Logger.Error?.Log($"[ERROR] {TrainsLibMod.ModName}: {message}");
        }
    }
}
