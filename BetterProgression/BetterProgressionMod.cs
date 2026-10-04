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
        private List<ISerializedGameScenarioRewirer> _serializedGameScenarioRewirers = new();

        // hooks and rewirers
        private readonly RewirerHandle? _scenarioRewirer;
        private ILHook? _serializedGameScenarioHook;

        public BetterProgressionMod(ILogger logger)
        {
            Logger = logger;

            _scenarioRewirer = GameRewirers.AddRewirer(new BetterProgressionScenarioRewirer());
            _serializedGameScenarioRewirers.Add(new BetterProgressionSerializedScenarioRewirer());

            MethodInfo target = typeof(GameMode).GetMethod("From", BindingFlags.Static | BindingFlags.Public);
            MethodInfo getRawScenario = typeof(IGameData).GetMethod("GetRawScenario", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            _serializedGameScenarioHook = new ILHook(
                source: target,
                context =>
                {
                    ILCursor cursor = new(context);
                    cursor.GotoNext(MoveType.After, instruction => instruction.MatchCallvirt<IGameData>("GetRawScenario"));
                    cursor.EmitDelegate<Func<SerializedGameScenario, SerializedGameScenario>>(ModifyScenario);
                });

            ScenarioReplacementData.Initialize();
        }

        public void Dispose()
        {
            if (_scenarioRewirer != null) GameRewirers.RemoveRewirer(_scenarioRewirer.Value);
            _serializedGameScenarioHook?.Dispose();
            _serializedGameScenarioHook = null;
        }

        private SerializedGameScenario ModifyScenario(SerializedGameScenario scenario)
        {
            foreach (ISerializedGameScenarioRewirer rewirer in _serializedGameScenarioRewirers)
            {
                scenario = rewirer.ModifySerializedGameScenario(scenario);
            }

            return scenario;
        }
    }
}
