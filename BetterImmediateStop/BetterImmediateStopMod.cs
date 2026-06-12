using Game.Core.Coordinates;
using Game.Core.Simulation;
using Game.Core.Trains;
using MonoMod.RuntimeDetour;
using Shapez2ModConfig;
using ShapezShifter.SharpDetour;
using ILogger = Core.Logging.ILogger;

namespace BetterImmediateStop
{
    public class BetterImmediateStopMod : IMod
    {
        public static ILogger Logger = null!;

        // config
        public static ModConfig Config { get; private set; } = null!;
        public static int DefaultWaitSeconds => Config.GetEntry<int>(WAIT_TIME_ID).Value;
        private const string WAIT_TIME_ID = "maximum wait time";    // DO NOT CHANGE
        private const string CONFIG_ID = "Zackbot2.BetterImmediateStop";   // DO NOT CHANGE

        public static int WaitTimeSeconds => Config.GetEntry<int>(WAIT_TIME_ID).Value;
        public static Ticks WaitTimeTicks => Ticks.FromSeconds(WaitTimeSeconds);

        // store the hook so it doesn't get GCed
        private readonly Hook shouldTrainLeaveHook;

        public BetterImmediateStopMod(ILogger logger)
        {
            Logger = logger;

            Config = new(CONFIG_ID, GetType());

            Config.RegisterEntry<int>(WAIT_TIME_ID, 5).OnChanged.Register(value => Logger.Info?.Log($"Config entry \"{WAIT_TIME_ID}\" updated to {value}"));
            Config.Load();
            Config.Save();

            shouldTrainLeaveHook = CreateHook();

            Logger.Info?.Log("BetterImmediateStop loaded successfully.");
        }

        private Hook CreateHook()
        {
            return DetourHelper.Replace<QuickStopDecider, TrainId, TrainSimulationData, bool>((quickStopDecider, id, trainSimulationData) => 
                quickStopDecider.ShouldTrainLeave(id, trainSimulationData), QuickStop_ShouldTrainLeave);
        }

        private static bool QuickStop_ShouldTrainLeave(QuickStopDecider deciderInstance, TrainId id, TrainSimulationData trainSimulationData)
        {
            if (deciderInstance == null)
            {
                return false;
            }

            if (deciderInstance.TrainSimulationTimeProvider.SimulationTime - trainSimulationData.StopTime > WaitTimeTicks)
            {
                // force cancel all exchanges. LEAVE.
                for (int i = 1; i < trainSimulationData.Wagons.Length; i++)
                {
                    GlobalChunkCoordinate position = trainSimulationData.Wagons[i].Outgoing.Position;
                    if (deciderInstance.CargoSimulator.TryGetExchanger(position, out ICargoExchanger cargoExchanger) 
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

        public void Dispose() 
        {
            shouldTrainLeaveHook?.Dispose();
        }
    }
}
