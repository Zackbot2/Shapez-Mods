using Core.Logging;
using Game.Core.Content.Buildings;
using Game.Core.Content.Islands;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using ShapezShifter.Hijack;
using ShapezShifter.SharpDetour;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace BetterProgression
{
    public class BetterProgressionMod : IMod
    {
        internal static ILogger Logger { get; private set; } = null!;
        public static string ModName => nameof(BetterProgressionMod);

        // hooks and rewirers
        private readonly RewirerHandle? _scenarioRewirer;
        private ILHook? _serializedGameScenarioHook;

        public BetterProgressionMod(ILogger logger)
        {
            Logger = logger;

            _scenarioRewirer = GameRewirers.AddRewirer(new BetterProgressionScenarioRewirer());

            ScenarioReplacementData.Initialize();
        }

        public void Dispose()
        {
            if (_scenarioRewirer != null) GameRewirers.RemoveRewirer(_scenarioRewirer.Value);
            _serializedGameScenarioHook?.Dispose();
            _serializedGameScenarioHook = null;
        }
    }
}
