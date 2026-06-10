using Core.Factory;
using Game.Core.Coordinates;
using Game.Core.Simulation;
using Game.Core.Trains;
using MonoMod.RuntimeDetour;
using ShapezShifter.Flow;
using ShapezShifter.Flow.Atomic;
using ShapezShifter.Hijack;
using ShapezShifter.SharpDetour;
using System;
using System.Collections.Generic;
using ILogger = Core.Logging.ILogger;

namespace UnlimitedWaitStop
{
    public class UnlimitedWaitStopMod : IMod
    {
        private readonly ILogger _logger;
        private static ILogger s_logger;
        private readonly RewirerHandle _simulationRewirer;
        private readonly RewirerHandle _modulesRewirer;
        private GameIslands? _islands;

        // store hooks so they don't get GCed
        private Hook? shouldTrainLeaveHook;
        private Hook? _dialogStackHook;
        private Hook? _panelHook;

        public UnlimitedWaitStopMod(ILogger logger)
        {
            _logger = logger;
            s_logger = logger;

            WaitStopDeciderRef deciderRef = new();
            CreateHooks(deciderRef);

            _simulationRewirer = GameRewirers.AddRewirer(new WaitStopSimulationRewirer(deciderRef));
            _modulesRewirer = GameRewirers.AddRewirer(new WaitStopModulesRewirer(deciderRef, _logger));

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
        }

        private void CreateHooks(WaitStopDeciderRef deciderRef)
        {
            _dialogStackHook = DetourHelper.CreatePostfixHook((orchestrator, kb, cam, iface, data) =>
            orchestrator.Init_4_Managers(kb, cam, iface, data),
            delegate (GameSessionOrchestrator orchestrator, Keybindings _kb, CameraGameSettings _cam, InterfaceGameSettings _iface, IGameData _data)
            {
                deciderRef.DialogStack = orchestrator.DialogStack;
                _islands = orchestrator.Mode.Islands;
                deciderRef.WaitStationId = _islands.Trains.Navigation.WaitStation.Id;
            });
            _panelHook = DetourHelper.CreatePostfixHook((self, sel) => self.OnSelectionChanged(sel), delegate (HUDIslandSelectionDetails self, IEnumerable<IslandModel> _)
            {
                deciderRef.RefreshSidePanel = delegate
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
            Ticks waitTimeTicks = WaitStopRegistry.WaitTimes.TryGetValue(stationChunk, out int value) ? Ticks.FromSeconds(value) : deciderInstance.MaxTicksToWait;
            s_logger.Info?.Log($"Train {id} has been waiting for {deciderInstance.TrainSimulationTimeProvider.SimulationTime - trainSimulationData.StopTime}. It will wait for {waitTimeTicks}.");

            // if it's negative, ignore the max ticks to wait.
            return waitTimeTicks.Value >= 0 && deciderInstance.TrainSimulationTimeProvider.SimulationTime - trainSimulationData.StopTime > waitTimeTicks;
        }
    }
}
