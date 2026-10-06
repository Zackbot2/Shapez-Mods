using Core.Logging;
using MonoMod.RuntimeDetour;
using ShapezShifter.Hijack;
using ShapezShifter.SharpDetour;
using System;
using System.Reflection;

namespace ScenarioTools
{
    public class ScenarioToolsMod : IMod
    {
        internal static ILogger Logger { get; private set; } = null!;
        internal const string MOD_NAME = nameof(ScenarioToolsMod);

        private readonly SerializedGameScenarioInterceptor? _gameScenarioInterceptor;
        private readonly IRewirerProvider _whatTheFuckRewirerProvider;

        public ScenarioToolsMod(ILogger logger)
        {
            Logger = logger;

            _whatTheFuckRewirerProvider = new CachedStaticallyAccessibleRewirerProvider(logger);
            _gameScenarioInterceptor = new SerializedGameScenarioInterceptor(_whatTheFuckRewirerProvider, logger);

            GameRewirers.AddRewirer(new ScenarioToolsSerializedGameScenarioRewirer());
        }

        public void Dispose()
        {
            _gameScenarioInterceptor?.Dispose();
        }
    }
}
