using Core.Factory;
using Game.Core.Coordinates;
using Game.Core.Simulation;
using Game.Core.Trains;
using MonoMod.RuntimeDetour;
using ShapezShifter.Hijack;
using ShapezShifter.SharpDetour;
using System.Collections.Generic;
using ILogger = Core.Logging.ILogger;

namespace UnlimitedWaitStop
{
    public class UnlimitedWaitStopMod : IMod
    {
        private readonly ILogger _logger;
        //private static ILogger s_logger;
        private readonly RewirerHandle _simulationRewirer;
        private readonly RewirerHandle _modulesRewirer;
        private GameIslands? _islands;
        private static WaitStopDeciderRef _deciderRef;

        // store hooks so they don't get GCed
        private Hook? shouldTrainLeaveHook;
        private Hook? _dialogStackHook;
        private Hook? _panelHook;
        private Hook? _bakeMetadataIntoRuntimeHook;

        public UnlimitedWaitStopMod(ILogger logger)
        {
            _logger = logger;
            //s_logger = logger;

            _deciderRef = new();
            CreateHooks();

            _simulationRewirer = GameRewirers.AddRewirer(new WaitStopSimulationRewirer(_deciderRef));

            // this one is weird - it rewires into ShapezShifter, in order to patch the wait stop's modules.
            _modulesRewirer = GameRewirers.AddRewirer(new WaitStopModulesRewirer(_deciderRef, _logger));

            _logger.Info?.Log("UnlimitedWaitStop loaded successfully!");
        }

        public void Dispose()
        {
            if (_simulationRewirer != null)
            {
                GameRewirers.RemoveRewirer(_simulationRewirer);
            }

            if (_modulesRewirer != null)
            {
                GameRewirers.RemoveRewirer(_modulesRewirer);
            }

            shouldTrainLeaveHook?.Dispose();
            _dialogStackHook?.Dispose();
            _panelHook?.Dispose();
            _bakeMetadataIntoRuntimeHook?.Dispose();
        }

        private void CreateHooks()
        {
            _bakeMetadataIntoRuntimeHook = DetourHelper.CreatePostfixHook(
                (factory, islands) => factory.BakeMetadataIntoRuntime(islands),
                (IslandDefinitionFactory factory, MetaGameModeIslands islands, GameIslands __result) =>
                {
                    _logger.Debug?.Log("Injecting WaitStop config factory during bake...");

                    // IMPORTANT: modify definitions BEFORE islands are used elsewhere
                    foreach (var def in __result.AllDefinitions)
                    {
                        if (def is IslandDefinition islandDef &&
                            islandDef.Id == _islands?.Trains.Navigation.WaitStation.Id)
                        {
                            islandDef.CustomData.AttachOrReplace<IFactory<IIslandConfiguration>>(
                                new LambdaFactory<IIslandConfiguration>(() =>
                                    new WaitStopIslandConfiguration()
                                )
                            );

                            _logger.Debug?.Log("WaitStop config factory attached");
                        }
                    }
                    return __result;
                });

            _dialogStackHook = DetourHelper.CreatePostfixHook((orchestrator, kb, cam, iface, data) =>
                orchestrator.Init_4_Managers(kb, cam, iface, data),
                delegate (GameSessionOrchestrator orchestrator, Keybindings _kb, CameraGameSettings _cam, InterfaceGameSettings _iface, IGameData _data)
                {
                    _deciderRef.DialogStack = orchestrator.DialogStack;
                    _islands = orchestrator.Mode.Islands;

                    // from the orchestrator, we can get the wait station's island definition and sub in our custom configuration data.
                    IslandDefinition waitStationDefinition = (IslandDefinition)_islands.Trains.Navigation.WaitStation;
                    _deciderRef.WaitStationId = waitStationDefinition.Id;
                });

            _panelHook = DetourHelper.CreatePostfixHook((self, sel) => self.OnSelectionChanged(sel), delegate (HUDIslandSelectionDetails self, IEnumerable<IslandModel> _)
                {
                    _deciderRef.RefreshSidePanel = delegate
                    {
                        self?.SidePanel_MarkDirty();
                    };
                });

            shouldTrainLeaveHook = DetourHelper.Replace<WaitStopDecider, TrainId, TrainSimulationData, bool>(
                (waitStopDecider, id, trainSimulationData) => waitStopDecider.ShouldTrainLeave(id, trainSimulationData),
                WaitStop_ShouldTrainLeave);
        }

        private static bool WaitStop_ShouldTrainLeave(WaitStopDecider deciderInstance, TrainId id, TrainSimulationData trainSimulationData)
        {
            if (deciderInstance == null)
            {
                return false;
            }

            if (!deciderInstance.TrainExchangeCompleted(trainSimulationData))
            {
                return false;
            }

            if (!deciderInstance.TrainCouldExchange(id, trainSimulationData))
            {
                return true;
            }

            GlobalChunkCoordinate stationChunk = trainSimulationData.Head.Incoming.Position;
            //Ticks waitTimeTicks = WaitStopRegistry.WaitTimes.TryGetValue(stationChunk, out Ticks value) ? value : deciderInstance.MaxTicksToWait;
            Ticks waitTimeTicks = _deciderRef.GetWaitTicks(stationChunk);

            //s_logger.Info?.Log($"Train {id} has been waiting for {deciderInstance.TrainSimulationTimeProvider.SimulationTime - trainSimulationData.StopTime}. It will wait for {waitTimeTicks}.");

            // if it's negative, ignore the max ticks to wait.
            return waitTimeTicks.Value >= 0 && deciderInstance.TrainSimulationTimeProvider.SimulationTime - trainSimulationData.StopTime > waitTimeTicks;
        }
    }
}
