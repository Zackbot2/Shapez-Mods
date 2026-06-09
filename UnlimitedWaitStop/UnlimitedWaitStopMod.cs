using Game.Core.Trains;
using MonoMod.RuntimeDetour;
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
        private readonly RewirerHandle _waitStopRewirer;

        // store hooks so they don't get GCed
        private readonly Hook shouldTrainLeaveHook;
        private IDisposable _dialogStackHook;
        private IDisposable _panelHook;

        public UnlimitedWaitStopMod(ILogger logger)
        {
            _logger = logger;

            WaitStopDeciderRef deciderRef = new();

            _dialogStackHook = DetourHelper.CreatePostfixHook((orchestrator, kb, cam, iface, data) => 
            orchestrator.Init_4_Managers(kb, cam, iface, data), 
            delegate (GameSessionOrchestrator orchestrator, Keybindings _kb, CameraGameSettings _cam, InterfaceGameSettings _iface, IGameData _data)
            {
                deciderRef.DialogStack = orchestrator.DialogStack;
            });

            _panelHook = DetourHelper.CreatePostfixHook((self, sel) => self.OnSelectionChanged(sel), delegate (HUDIslandSelectionDetails self, IEnumerable<IslandModel> _)
            {
                deciderRef.RefreshSidePanel = delegate
                {
                    self?.SidePanel_MarkDirty();
                };
            });

            _waitStopRewirer = GameRewirers.AddRewirer(new WaitStopSimulationRewirer(deciderRef));

            shouldTrainLeaveHook = CreateHook();

            _logger.Info?.Log("UnlimitedWaitStop loaded successfully!");
        }

        public void Dispose()
        {
            if (_waitStopRewirer != null)
            {
                GameRewirers.RemoveRewirer(_waitStopRewirer);
            }
            shouldTrainLeaveHook.Dispose();
            _dialogStackHook.Dispose();
            _panelHook.Dispose();
        }

        private Hook CreateHook()
        {
            return DetourHelper.Replace<WaitStopDecider, TrainId, TrainSimulationData, bool>(
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

            // if it's set to -1, that means we ignore the max ticks to wait.
            return deciderInstance.MaxTicksToWait.FullSeconds != -1 && trainSimulationData.StopTime > deciderInstance.MaxTicksToWait;
        }
    }
}
