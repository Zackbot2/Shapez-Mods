using Core.Logging;
using MonoMod.RuntimeDetour;
using ShapezShifter.Hijack;
using ShapezShifter.SharpDetour;
using System;
using System.Reflection;

/*
potential solutions to make this a modification-based system instead of a replacement-based one

REQUIREMENTS:
- extremely easy to hard-code data
- ability to modify things and also outright replace them

1. ISerializedGameScenario data container which contains methods to modify it
  - everything is all in one place, for better or for worse

2. static methods which accept a certain object input depending on the data and output the result
  - harder to learn, divides the class
*/

/*
- data container
- data container contains other data containers which can be accessed to modify something
  - for example: 
    - ScenarioData.ResearchConfigData.BlueprintProgression.ReplaceAllWith(new BlueprintProgression(...))
    - ScenarioData.ResearchConfigData.BlueprintProgression.ReplaceShape()
*/

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
