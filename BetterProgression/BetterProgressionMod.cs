using Core.Localization;
using Core.Logging;
using Game.Core.Content;
using Game.Core.Content.Buildings;
using Game.Core.Content.Islands;
using Game.Core.Effects;
using Game.Core.Rendering.MeshGeneration;
using Global.Store;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using ScenarioTools.Data;
using ShapezShifter.Hijack;
using ShapezShifter.SharpDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ILogger = Core.Logging.ILogger;

namespace BetterProgression
{
    // (vanilla?) bug: blueprint reward from milestones is twice as much as it says in the milestone menu

    public class BetterProgressionMod : IMod
    {
        public const string MOD_NAME = nameof(BetterProgressionMod);

        internal static ILogger Logger { get; private set; } = null!;

        private readonly BetterProgressionScenarioDataHandler _scenarioDataHandler;
        private Hook? _gameDataFromHook;

        public BetterProgressionMod(ILogger logger)
        {
            Logger = logger;

            Logger.Info?.Log($"[{MOD_NAME}] Initializing BetterProgression...");

            _scenarioDataHandler = new();
            DataHandlers.AddDataHandler(_scenarioDataHandler);

            MethodInfo gameModeFromMethod = typeof(GameMode).GetMethod("From", BindingFlags.Static | BindingFlags.Public);
            _gameDataFromHook = new Hook(
                source: gameModeFromMethod,
                target: 
                (Func<Func<GameParameters, IGameData, VisualThemeBaseResources, IEntityContent, IUnlockableContentOwnershipProvider, IMeshCache, IAnimationRendererBaker, IAccentColorMeshAccessor, IAccentColorAnimationDefinitionAccessor, IslandGroupDefinitionRegistry, GameMode>, GameParameters, IGameData, VisualThemeBaseResources, IEntityContent, IUnlockableContentOwnershipProvider, IMeshCache, IAnimationRendererBaker, IAccentColorMeshAccessor, IAccentColorAnimationDefinitionAccessor, IslandGroupDefinitionRegistry, GameMode>)
                ((orig, parameters, gameData, resources, entityContent, ownershipProvider, meshCache, animationRendererBaker, accentColorMeshAccessor, accentColorAnimationDefinitionAccessor, islandGroupDefinitionFactory) =>
                {
                    GameMode result = orig(parameters, gameData, resources, entityContent, ownershipProvider, meshCache, animationRendererBaker, accentColorMeshAccessor, accentColorAnimationDefinitionAccessor, islandGroupDefinitionFactory);

                    ResearchProgressionLinearUpgrades linearUpgrades = result.Scenario.Progression.LinearUpgrades;
                    ResearchLinearUpgrade chunkLimitUpgrade = linearUpgrades.UpgradesById[linearUpgrades.ChunkLimitAddUpgrade];

                    if (linearUpgrades == null || chunkLimitUpgrade == null)
                    {
                        Logger.Error?.Log($"[{MOD_NAME}] Failed to apply game parameters to chunk limit currency!");
                        return result;
                    }

                    for (int i = 0; i < chunkLimitUpgrade.Levels.Count; i++)
                    {
                        ResearchLinearUpgrade.LevelData levelData = chunkLimitUpgrade._Levels[i];
                        chunkLimitUpgrade._Levels[i] = new ResearchLinearUpgrade.LevelData(levelData.Level, ApplyGameParametersToChunkLimitCurrency(levelData.Value, result), levelData.NextCost, ApplyGameParametersToChunkLimitCurrency(levelData.NextValue, result));
                    }

                    return result;
                }));

            Logger.Info?.Log($"[{MOD_NAME}] Initialized successfully!");
        }

        public void Dispose()
        {
            Logger.Info?.Log($"[{MOD_NAME}] Disposing BetterProgression...");
            DataHandlers.RemoveDataHandler(_scenarioDataHandler);
            _gameDataFromHook?.Dispose();
        }

        private static int ApplyGameParametersToChunkLimitCurrency(int input, GameMode mode)
        {
            return input * mode.Parameters.DifficultyParameters.ChunkLimitMultiplier / 100;
        }
    }
}
