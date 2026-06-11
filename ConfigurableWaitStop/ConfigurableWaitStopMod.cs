using Core.Factory;
using Game.Core.Coordinates;
using Game.Core.Simulation;
using Game.Core.Trains;
using Game.Core.Trains.Stations;
using MonoMod.RuntimeDetour;
using ShapezShifter.Hijack;
using ShapezShifter.SharpDetour;
using System.Collections.Generic;
using ILogger = Core.Logging.ILogger;

namespace ConfigurableWaitStop
{
    public class ConfigurableWaitStopMod : IMod
    {
        // feel free to use this code as a reference for how to make your own mods! i did a lot of digging, and i've done my best to document my code as best i can.

        public static ILogger Logger { get; set; } = null!;

        private GameIslands? _islands;
        private readonly RewirerHandle _simulationRewirer;
        private readonly RewirerHandle _modulesRewirer;
        // store hooks so they don't get GCed, and so we can dispose them later
        private Hook _shouldTrainLeaveHook = null!;
        private Hook _initManagersHook = null!;
        private Hook _panelHook = null!;
        private Hook _bakeMetadataIntoRuntimeHook = null!;
        private Hook _registerIslandHook = null!;
        private Hook _unregisterIslandHook = null!;

        public ConfigurableWaitStopMod(ILogger logger)
        {
            Logger = logger;

            CreateHooks();

            // rewire the simulation to use WaitStopIslandSystem.
            _simulationRewirer = GameRewirers.AddRewirer(new WaitStopSimulationRewirer());
            // this one is weird - it rewires into ShapezShifter, in order to patch the wait stop's modules.
            _modulesRewirer = GameRewirers.AddRewirer(new WaitStopModulesRewirer());

            Logger.Info?.Log("ConfigurableWaitStop loaded successfully!");
        }

        public void Dispose()
        {
            // remove the rewirers we made
            if (_simulationRewirer != null)
            {
                GameRewirers.RemoveRewirer(_simulationRewirer);
            }

            if (_modulesRewirer != null)
            {
                GameRewirers.RemoveRewirer(_modulesRewirer);
            }

            // dispose of all hooks
            _shouldTrainLeaveHook.Dispose();
            _initManagersHook.Dispose();
            _panelHook.Dispose();
            _bakeMetadataIntoRuntimeHook.Dispose();
            _registerIslandHook.Dispose();
            _unregisterIslandHook.Dispose();
        }

        /// <summary>
        /// Create all needed hooks for the mod to function.
        /// </summary>
        private void CreateHooks()
        {
            // IslandDefinitionFactory.BakeMetadataIntoRuntime is where island definitions are originally read in and configurations are assigned when you load a savegame.
            // we need to hook this in order for our configurations to be read when a savegame is loaded.
            // the game reads this island data ONCE and never comes back, meaning we can't hook any later than this.
            _bakeMetadataIntoRuntimeHook = DetourHelper.CreatePostfixHook(
                (factory, islands) => factory.BakeMetadataIntoRuntime(islands),
                (IslandDefinitionFactory factory, MetaGameModeIslands islands, GameIslands __result) =>
                {
                    Logger.Info?.Log("Injecting WaitStop config factory during bake...");

                    // IMPORTANT: modify definitions BEFORE islands are used elsewhere
                    foreach (var def in __result.AllDefinitions)
                    {
                        if (def is IslandDefinition islandDef && WaitStopData.WaitStationId != null && islandDef.Id == WaitStopData.WaitStationId)
                        {
                            islandDef.CustomData.AttachOrReplace<IFactory<IIslandConfiguration>>(
                                new LambdaFactory<IIslandConfiguration>(() => new WaitStopIslandConfiguration())
                            );

                            Logger.Info?.Log("WaitStop config factory attached");
                        }
                    }
                    return __result;
                });

            // GameSessionOrchestrator.Init_4_Managers is a little further down the line, this handles the island data that was created in IslandDefinitionFactory.BakeMetadataIntoRuntime.
            // here, we can use data it's populated the islands with in order to grab the wait stop's island definition as well as the orchestrator's dialog stack.
            _initManagersHook = DetourHelper.CreatePostfixHook((orchestrator, kb, cam, iface, data) =>
                orchestrator.Init_4_Managers(kb, cam, iface, data),
                delegate (GameSessionOrchestrator orchestrator, Keybindings _kb, CameraGameSettings _cam, InterfaceGameSettings _iface, IGameData _data)
                {
                    WaitStopData.DialogStack = orchestrator.DialogStack;
                    _islands = orchestrator.Mode.Islands;

                    // from the orchestrator, we can get the wait station's island definition and sub in our custom configuration data.
                    IslandDefinition waitStationDefinition = (IslandDefinition)_islands.Trains.Navigation.WaitStation;
                    WaitStopData.WaitStationId = waitStationDefinition.Id;
                });

            _panelHook = DetourHelper.CreatePostfixHook((self, sel) => self.OnSelectionChanged(sel), delegate (HUDIslandSelectionDetails self, IEnumerable<IslandModel> _)
                {
                    WaitStopData.RefreshSidePanel = delegate
                    {
                        self?.SidePanel_MarkDirty();
                    };
                });

            // even further down the line, TrainStationCoordinator.RegisterIsland is called when the has all the islands figured out and is now assembling the simulation.
            // because WaitStopDecider doesn't have access to the configuration directly, we need to store the custom wait time FOR it. i do this using WaitStopData.WaitTimes.
            // when the wait stop island is registered, we grab its wait time data and store it in the ref, which is usable by the decider.
            // in summary, it goes WaitStopIslandConfiguration -> WaitStopData.WaitTimes -> WaitStopDecider.ShouldTrainLeave
            _registerIslandHook = DetourHelper.CreatePostfixHook((stationCoordinator, island) =>
                stationCoordinator.RegisterIsland(island), delegate (TrainStationCoordinator stationCoordinator, IslandInstance island, bool __result)
                {
                    if (!__result)
                        return __result;

                    if (island.Configuration is WaitStopIslandConfiguration waitStopConfig)
                    {
                        Logger.Info?.Log($"Registering wait stop island at {island.Transform.Position} with wait time of {waitStopConfig.WaitTimeTicks}");
                        WaitStopData.SetWaitSeconds(island.Transform.Position, waitStopConfig.WaitTimeSeconds);
                    }
                    return __result;
                }
            );

            // this is pretty much the same as _registerIslandHook, but in reverse. when the island is unregistered, we need to get rid of its entry.
            _unregisterIslandHook = DetourHelper.CreatePostfixHook((stationCoordinator, island) =>
                stationCoordinator.UnregisterIsland(island), delegate (TrainStationCoordinator stationCoordinator, IslandInstance island, bool __result)
                {
                    if (!__result)
                        return __result;

                    if (island.Configuration is WaitStopIslandConfiguration waitStopConfig)
                    {
                        WaitStopData.WaitTimes.TryRemove(island.Transform.Position, out _);
                    }
                    return __result;
                }
            );

            // and finally, we have the ONLY important hook for the functionality of the mod. everything else is just so this one hook works.
            // this overrides the wait stop's behaviour and replaces it with our own, defined in WaitStop_ShouldTrainLeave
            _shouldTrainLeaveHook = DetourHelper.Replace<WaitStopDecider, TrainId, TrainSimulationData, bool>(
                (waitStopDecider, id, trainSimulationData) => waitStopDecider.ShouldTrainLeave(id, trainSimulationData),
                WaitStop_ShouldTrainLeave);
        }

        private static bool WaitStop_ShouldTrainLeave(WaitStopDecider deciderInstance, TrainId id, TrainSimulationData trainSimulationData)
        {
            if (deciderInstance == null)
            {
                Logger.Error?.Log($"{nameof(WaitStop_ShouldTrainLeave)} is missing a {nameof(WaitStopDecider)} instance! Location: {trainSimulationData.Head.Incoming.Position}");
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
            Ticks waitTimeTicks = WaitStopData.GetWaitTicks(stationChunk);

            //Logger.Info?.Log($"Train {id} has been waiting for {deciderInstance.TrainSimulationTimeProvider.SimulationTime - trainSimulationData.StopTime}. It will wait for {waitTimeTicks}.");

            // if it's negative, ignore the max ticks to wait.
            return waitTimeTicks.Value >= 0 && deciderInstance.TrainSimulationTimeProvider.SimulationTime - trainSimulationData.StopTime > waitTimeTicks;
        }
    }
}
