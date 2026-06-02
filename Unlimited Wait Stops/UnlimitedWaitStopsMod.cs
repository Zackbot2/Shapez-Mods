using Game.Core.Trains;
using MonoMod.RuntimeDetour;
using ShapezShifter.SharpDetour;
using ILogger = Core.Logging.ILogger;

namespace Unlimited_Wait_Stops
{
    public class UnlimitedWaitStopsMod : IMod
    {
        private readonly ILogger _logger;

        // store the hook so it doesn't get GCed
        private readonly Hook shouldTrainLeaveHook;

        public UnlimitedWaitStopsMod(ILogger logger)
        {
            _logger = logger;

            _logger.Info?.Log("Hello, Shapez 2!");

            shouldTrainLeaveHook = CreateHook();

            _logger.Info?.Log("Mod loaded successfully!");
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

            return !deciderInstance.TrainCouldExchange(id, trainSimulationData);
        }

        public void Dispose() { }
    }
}
