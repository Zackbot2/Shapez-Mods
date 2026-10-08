using Core.Logging;
using Game.Core.Content.Buildings;
using Game.Core.Content.Islands;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using ScenarioTools.Data;
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

        private readonly BetterProgressionScenarioDataHandler _scenarioDataHandler;

        public BetterProgressionMod(ILogger logger)
        {
            Logger = logger;

            _scenarioDataHandler = new();
            DataHandlers.AddDataHandler(_scenarioDataHandler);
        }

        public void Dispose()
        {
            DataHandlers.RemoveDataHandler(_scenarioDataHandler);
        }
    }
}
