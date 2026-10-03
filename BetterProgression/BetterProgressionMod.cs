using Core.Logging;
using System;

namespace BetterProgression
{
    public class BetterProgressionMod : IMod
    {
        internal static ILogger Logger { get; private set; } = null!;
        public static string ModName => nameof(BetterProgressionMod);

        public BetterProgressionMod(ILogger logger)
        {
            Logger = logger;
        }

        public void Dispose()
        {
            
        }
    }
}
