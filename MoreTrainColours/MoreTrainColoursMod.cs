using MonoMod.RuntimeDetour;
using ShapezShifter.Hijack;
using ILogger = Core.Logging.ILogger;

namespace MoreTrainColours
{
    public class MoreTrainColoursMod : IMod
    {
        public static ILogger Logger = null!;

        // hooks
        private Hook? _gameScenarioHook;

        public MoreTrainColoursMod(ILogger logger)
        {
            Logger = logger;

            GameRewirers.AddRewirer(new MoreTrainColoursScenarioRewirer());
        }

        public void Dispose() { }
    }
}
