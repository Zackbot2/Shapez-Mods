using Core.Logging;

namespace MoreTrainColours
{
    public class MoreTrainColoursMod : IMod
    {
        public static ILogger Logger = null!;

        public MoreTrainColoursMod(ILogger logger)
        {
            Logger = logger;
        }

        public void Dispose() { }
    }
}
