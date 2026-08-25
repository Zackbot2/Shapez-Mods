using ILogger = Core.Logging.ILogger;

namespace TrainsLib
{
    public class TrainsLibMod : IMod
    {
        internal static ILogger Logger { get; private set; } = null!;
        public static string ModName => nameof(TrainsLib);

        public TrainsLibMod(ILogger logger) 
        {
            Logger = logger;
            Logger.Info?.Log($"{ModName}: Initializing mod...");

            HookHandler.Initialize();

            Logger.Info?.Log($"{ModName}: Mod initialized successfully! 🚂");
        }

        public void Dispose() 
        {
            HookHandler.Instance?.Dispose();
        }
    }
}
