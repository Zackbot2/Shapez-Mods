using MonoMod.RuntimeDetour;
using ShapezShifter.SharpDetour;
using System;
using System.Collections.Generic;
using System.Text;

namespace TrainsLib.GameData
{
    internal class TrainsDataHandler : IDisposable
    {
        public static TrainsDataHandler? Instance;

        private GameIslands? _gameIslands;

        // hooks
        private Hook? _initManagersHook;

        private TrainsDataHandler()
        {
            Instance ??= this;
        }

        public static void Initialize()
        {
            // create an instance, if one doesn't already exist
            Instance ??= new();

            Instance.CreateHooks();// create hooks
            
        }

        private void CreateHooks()
        {
            _initManagersHook = DetourHelper.CreatePostfixHook(
            (orchestrator, kb, cam, iface, data) => orchestrator.Init_4_Managers(kb, cam, iface, data),
            delegate (GameSessionOrchestrator orchestrator, Keybindings _kb, CameraGameSettings _cam, InterfaceGameSettings _iface, IGameData _data)
            {
                _gameIslands = orchestrator.Mode.Islands;

                // wait stop definition
                IslandDefinition waitStationDefinition = (IslandDefinition)_gameIslands.Trains.Navigation.WaitStation;
                GameTrainStationsData.WaitStationDefinition = waitStationDefinition;

                // quick stop definition
                IslandDefinition quickStationDefinition = (IslandDefinition)_gameIslands.Trains.Navigation.QuickStation;
                GameTrainStationsData.QuickStationDefinition = quickStationDefinition;
            });
        }

        public void Dispose()
        {
            // dispose all hooks
            _initManagersHook?.Dispose();

            // dispose this instance
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
