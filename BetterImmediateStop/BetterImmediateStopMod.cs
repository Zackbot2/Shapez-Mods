using Game.Core.Trains;
using Game.Core.Simulation;
using Game.Core.Coordinates;
using MonoMod.RuntimeDetour;
using ShapezShifter.SharpDetour;
using ILogger = Core.Logging.ILogger;

namespace BetterImmediateStop
{
    public class BetterImmediateStopMod : IMod
    {
        private readonly ILogger _logger;

        // store the hook so it doesn't get GCed
        private readonly Hook shouldTrainLeaveHook;

        public BetterImmediateStopMod(ILogger logger)
        {
            _logger = logger;
            _logger.Info?.Log("Hello, Shapez 2!");
            shouldTrainLeaveHook = CreateHook();
            _logger.Info?.Log("Mod loaded successfully!");
        }

        private Hook CreateHook()
        {
            return DetourHelper.Replace<QuickStopDecider, TrainId, TrainSimulationData, bool>(
                (quickStopDecider, id, trainSimulationData) => 
                    quickStopDecider.ShouldTrainLeave(id, trainSimulationData)
                    , QuickStop_ShouldTrainLeave);
        }

        private static bool QuickStop_ShouldTrainLeave(QuickStopDecider deciderInstance, TrainId id, TrainSimulationData trainSimulationData)
        {
            if (deciderInstance == null)
            {
                return false;
            }

            if (deciderInstance.TrainSimulationTimeProvider.SimulationTime - trainSimulationData.StopTime > deciderInstance.TicksToWait)
            {
                // force cancel all exchanges. LEAVE.
                for (int i = 1; i < trainSimulationData.Wagons.Length; i++)
                {
                    GlobalChunkCoordinate position = trainSimulationData.Wagons[i].Outgoing.Position;
                    if (!deciderInstance.CargoSimulator.TryGetExchanger(position, out ICargoExchanger cargoExchanger) 
                        && cargoExchanger.IsActivelyExchangingWithTrain())
                    {
                        cargoExchanger.CancelExchange();
                    }
                }

                return true;
            }

            return deciderInstance.TrainExchangeCompleted(trainSimulationData) 
                && !deciderInstance.TrainCanExchangeImmediately(id, trainSimulationData);
        }

        public void Dispose() { }
    }
}
