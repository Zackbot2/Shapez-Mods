using MonoMod.RuntimeDetour;
using ShapezShifter.Hijack;
using ShapezShifter.SharpDetour;
using System;
using TrainsLib.GameData;
using TrainsLib.Stations;
using ILogger = Core.Logging.ILogger;

namespace TrainsLib
{
    public class TrainsLibMod : IMod
    {
        internal static ILogger Logger { get; private set; } = null!;
        public static string ModName => nameof(TrainsLib);

        private RewirerHandle _gameIslandsProviderRewirer;
        private Hook? _initManagersHook;

        public TrainsLibMod(ILogger logger) 
        {
            Logger = logger;

            TrainsDataHandler.Initialize();

            Logger.Info?.Log($"{ModName}: Mod initialized successfully! 🚂");
        }

        public void Dispose() 
        {
            // dispose rewirers
            if (_gameIslandsProviderRewirer != null)
            {
                GameRewirers.RemoveRewirer(_gameIslandsProviderRewirer);
            }
        }
    }
}
