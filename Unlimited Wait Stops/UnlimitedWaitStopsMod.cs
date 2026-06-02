using System;
using MonoMod.RuntimeDetour;
using System.Reflection;
using System.Linq;
using ILogger = Core.Logging.ILogger;
using Game.Core.Trains;

namespace Unlimited_Wait_Stops
{
    public class UnlimitedWaitStopsMod : IMod
    {
        private Hook shouldTrainLeaveHook;
        private readonly ILogger _logger;

        public UnlimitedWaitStopsMod(ILogger logger)
        {
            _logger = logger;

            _logger.Info?.Log("Hello, Shapez 2!");

            shouldTrainLeaveHook = CreateHook();

            _logger.Info?.Log("Mod loaded successfully!");
        }

        private Hook CreateHook()
        {
            _logger.Info?.Log("Creating hook for ShouldTrainLeave...");
            var waitStopDecider = AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetType("Game.Core.Trains.WaitStopDecider", false))
                .FirstOrDefault(t => t != null);

            if (waitStopDecider == null) throw new InvalidOperationException("WaitStopDecider class not found");

            _logger.Info?.Log("WaitStopDecider class found: " + waitStopDecider.FullName);

            var shouldTrainLeave = waitStopDecider.GetMethod("ShouldTrainLeave", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
            if (shouldTrainLeave == null) throw new InvalidOperationException("ShouldTrainLeave method not found");
            
            _logger.Info?.Log("ShouldTrainLeave method found: " + shouldTrainLeave.Name);

            MethodInfo detour = typeof(UnlimitedWaitStopsMod).GetMethod(nameof(WaitStop_ShouldTrainLeave), BindingFlags.NonPublic | BindingFlags.Instance);
            if (detour == null) throw new InvalidOperationException("Detour method not found");
            
            _logger.Info?.Log("Detour method found: " + detour.Name);

            return new Hook(shouldTrainLeave, detour);
        }

        private bool WaitStop_ShouldTrainLeave(TrainId id, TrainSimulationData trainSimulationData)
        {
            throw new NotImplementedException();
        }

        public void Dispose() { }
    }
}
