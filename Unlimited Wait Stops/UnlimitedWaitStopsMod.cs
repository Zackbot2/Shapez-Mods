using Game.Core.Trains;
using MonoMod.RuntimeDetour;
using System;
using System.Linq;
using System.Reflection;
using ILogger = Core.Logging.ILogger;

namespace Unlimited_Wait_Stops
{
    public class UnlimitedWaitStopsMod : IMod
    {
        private readonly ILogger _logger;

        // store the hook so it doesn't get GCed
        private readonly Hook shouldTrainLeaveHook;
        private static MethodInfo? _trainExchangeCompletedMethod;
        private static MethodInfo? _trainCouldExchangeMethod;

        public UnlimitedWaitStopsMod(ILogger logger)
        {
            _logger = logger;

            _logger.Info?.Log("Hello, Shapez 2!");

            shouldTrainLeaveHook = CreateHook();

            _logger.Info?.Log("Mod loaded successfully!");
        }

        private Hook CreateHook()
        {
            _logger.Info?.Log("Creating hook for Game.Core.Trains.WaitStopDecider.ShouldTrainLeave...");
            var waitStopDecider = AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetType("Game.Core.Trains.WaitStopDecider", false))
                .FirstOrDefault(t => t != null);

            if (waitStopDecider == null) throw new InvalidOperationException("WaitStopDecider class not found");

            _logger.Info?.Log("WaitStopDecider class found: " + waitStopDecider.FullName);

            var shouldTrainLeave = waitStopDecider.GetMethod("ShouldTrainLeave", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
            if (shouldTrainLeave == null) throw new InvalidOperationException("ShouldTrainLeave method not found");
            
            _logger.Info?.Log("ShouldTrainLeave method found: " + shouldTrainLeave.Name);

            MethodInfo detour = typeof(UnlimitedWaitStopsMod).GetMethod(nameof(WaitStop_ShouldTrainLeave), BindingFlags.NonPublic | BindingFlags.Static);
            if (detour == null) throw new InvalidOperationException("Detour method not found");
            
            _logger.Info?.Log("Detour method found: " + detour.Name);

            return new Hook(shouldTrainLeave, detour);
        }

        private static bool WaitStop_ShouldTrainLeave(object deciderInstance, TrainId id, TrainSimulationData trainSimulationData)
        {
            if (deciderInstance == null)
            {
                return false;
            }

            var deciderType = deciderInstance.GetType();
            _trainExchangeCompletedMethod = deciderType.GetMethod("TrainExchangeCompleted", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (!(bool)_trainExchangeCompletedMethod.Invoke(deciderInstance, new object[] { trainSimulationData }))
            {
                return false;
            }

            _trainCouldExchangeMethod = deciderType.GetMethod("TrainCouldExchange", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            return !(bool)_trainCouldExchangeMethod.Invoke(deciderInstance, new object[] { id, trainSimulationData });
        }

        public void Dispose() { }
    }
}
