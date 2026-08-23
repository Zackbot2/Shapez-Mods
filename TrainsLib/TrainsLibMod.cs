using MonoMod.RuntimeDetour;
using ShapezShifter.Hijack;
using ShapezShifter.SharpDetour;
using System;
using ILogger = Core.Logging.ILogger;

namespace TrainsLib
{
    public class TrainsLibMod : IMod
    {
        internal static ILogger Logger { get; private set; } = null!;
        public static string ModName => nameof(TrainsLib);

        private GameIslands? _islands;

        private RewirerHandle _gameIslandsProviderRewirer;
        private Hook? _initManagersHook;

        public TrainsLibMod(ILogger logger) 
        {
            Logger = logger;

            CreateHooks();

            Logger.Info?.Log($"{ModName}: Mod initialized successfully! 🚂");
        }

        public void Dispose() 
        {
            // dispose rewirers
            if (_gameIslandsProviderRewirer != null)
            {
                GameRewirers.RemoveRewirer(_gameIslandsProviderRewirer);
            }

            _initManagersHook?.Dispose();
        }

        private void CreateHooks()
        {
            _initManagersHook = DetourHelper.CreatePostfixHook(
            (orchestrator, kb, cam, iface, data) => orchestrator.Init_4_Managers(kb, cam, iface, data),
            delegate (GameSessionOrchestrator orchestrator, Keybindings _kb, CameraGameSettings _cam, InterfaceGameSettings _iface, IGameData _data)
            {
                _islands = orchestrator.Mode.Islands;

                IslandDefinition waitStationDefinition = (IslandDefinition)_islands.Trains.Navigation.WaitStation;
                TrainStops.WaitStopDefinition = waitStationDefinition;
            });
        }
    }
}
