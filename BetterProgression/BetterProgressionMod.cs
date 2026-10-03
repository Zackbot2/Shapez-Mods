using Core.Logging;
using ShapezShifter.Hijack;
using System;

namespace BetterProgression
{
    public class BetterProgressionMod : IMod
    {
        internal static ILogger Logger { get; private set; } = null!;
        public static string ModName => nameof(BetterProgressionMod);

        // hooks and rewirers
        private readonly RewirerHandle? _scenarioRewirer;

        public BetterProgressionMod(ILogger logger)
        {
            Logger = logger;

            _scenarioRewirer = GameRewirers.AddRewirer(new BetterProgressionScenarioRewirer());
        }

        public void Dispose()
        {
            if (_scenarioRewirer != null) GameRewirers.RemoveRewirer(_scenarioRewirer.Value);
        }
    }
}
