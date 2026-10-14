using Core.Localization;
using Core.Logging;
using Game.Core.Content.Buildings;
using Game.Core.Content.Islands;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using ScenarioTools.Data;
using ShapezShifter.Hijack;
using ShapezShifter.SharpDetour;
using System;
using System.Collections.Generic;
using System.Reflection;
using ILogger = Core.Logging.ILogger;

namespace BetterProgression
{
    public class BetterProgressionMod : IMod
    {
        public const string MOD_NAME = nameof(BetterProgressionMod);

        internal static ILogger Logger { get; private set; } = null!;

        private readonly BetterProgressionScenarioDataHandler _scenarioDataHandler;
        private ILHook? _computeCurrentChunkLimitHook;
        private ILHook? _renderUpgradesHook;
        private ILHook? _rebuildViewHook;
        private ILHook? _upgradeSetterHook;
        private ILHook? _rewardsSetterHook;

        public BetterProgressionMod(ILogger logger)
        {
            Logger = logger;

            _scenarioDataHandler = new();
            DataHandlers.AddDataHandler(_scenarioDataHandler);

            MethodInfo computeCurrentChunkLimitMethod = typeof(ResearchChunkLimitManager).GetMethod(nameof(ResearchChunkLimitManager.ComputeCurrentChunkLimit), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            _computeCurrentChunkLimitHook = new ILHook(
                source: computeCurrentChunkLimitMethod,
                manip: context =>
                {
                    ILCursor cursor = new(context);

                    if (!cursor.TryGotoNext(MoveType.After, x => x.MatchCallvirt<IResearchLinearUpgrades>(nameof(IResearchLinearUpgrades.GetCurrentLevel))))
                    {
                        throw new InvalidOperationException($"Could not find call to IResearchLinearUpgrades.GetCurrentLevel in {nameof(ResearchChunkLimitManager)}.{nameof(ResearchChunkLimitManager.ComputeCurrentChunkLimit)}");
                    }

                    if (!cursor.TryGotoNext(MoveType.Before, x => x.MatchLdloc1()))
                    {
                        throw new InvalidOperationException($"Could not find load local variable 1 after call to IResearchLinearUpgrades.GetCurrentLevel in {nameof(ResearchChunkLimitManager)}.{nameof(ResearchChunkLimitManager.ComputeCurrentChunkLimit)}");
                    }

                    // load argument 0 onto the stack. argument 0 is "this", aka the ResearchChunkLimitManager instance.
                    cursor.EmitLdarg0();

                    // using the instance we just pushed to the stack, load the Mode field and push it onto the stack.
                    cursor.EmitLdfld(typeof(ResearchChunkLimitManager).GetField("Mode", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public));

                    if (!cursor.TryGotoNext(MoveType.Before, x => x.MatchAdd()))
                    {
                        throw new InvalidOperationException($"Could not find add instruction after call to IResearchLinearUpgrades.GetCurrentLevel in {nameof(ResearchChunkLimitManager)}.{nameof(ResearchChunkLimitManager.ComputeCurrentChunkLimit)}");
                    }

                    // just before the add, there are 2 important values on the stack: the mode and the current level value.
                    // read the chunk limit, multiply it using the GameMode, and push it back onto the stack.
                    cursor.EmitDelegate<Func<GameMode, int, int>>(ApplyGameParametersToChunkLimitCurrency);
                }
            );

            MethodInfo renderUpgradesMethod = typeof(HUDResearchSideUpgradesDisplay)
                .GetMethod(nameof(HUDResearchSideUpgradesDisplay.RenderUpgrades), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            _renderUpgradesHook = new ILHook(
                source: renderUpgradesMethod,
                manip: context =>
                {
                    ILCursor cursor = new(context);

                    if (!cursor.TryGotoNext(MoveType.Before, x => x.MatchCall<HUDResearchSideUpgradesDisplay>(nameof(HUDResearchSideUpgradesDisplay.ShouldRenderUpgrade))))
                    {
                        throw new InvalidOperationException($"Could not find {nameof(HUDResearchSideUpgradesDisplay.ShouldRenderUpgrade)} call in {nameof(HUDResearchSideUpgradesDisplay)}");
                    }
                    cursor.EmitLdarg0();
                    cursor.EmitDelegate<Func<ResearchLinearUpgrade, HUDResearchSideUpgradesDisplay, ResearchLinearUpgrade>>(PatchRenderUpgrades);
                    //cursor.EmitLdloc(13);
                });

            Logger.Info?.Log($"{MOD_NAME} initialized successfully!");
        }

        public void Dispose()
        {
            DataHandlers.RemoveDataHandler(_scenarioDataHandler);
            _computeCurrentChunkLimitHook?.Dispose();
            _renderUpgradesHook?.Dispose();
            _rebuildViewHook?.Dispose();
            _rewardsSetterHook?.Dispose();
            _upgradeSetterHook?.Dispose();
        }

        private static int ApplyGameParametersToChunkLimitCurrency(GameMode mode, int input)
        {
            Logger.Info?.Log($"Applying game parameters to chunk limit currency: input={input}, multiplier={mode.Parameters.DifficultyParameters.ChunkLimitMultiplier}. Output will be {input * mode.Parameters.DifficultyParameters.ChunkLimitMultiplier / 100}");
            //Logger.Info?.Log(Environment.StackTrace);
            return input * mode.Parameters.DifficultyParameters.ChunkLimitMultiplier / 100;
        }

        private static ResearchLinearUpgrade PatchRenderUpgrades(ResearchLinearUpgrade upgrade, HUDResearchSideUpgradesDisplay upgradesDisplay)
        {
            Logger.Info?.Log($"Should render upgrade {upgrade.Id}? {upgradesDisplay.ShouldRenderUpgrade(upgrade)}");
            for (int i=0; i < upgrade._Levels.Count; i++)
            {
                ResearchLinearUpgrade.LevelData level = upgrade._Levels[i];
                Logger.Info?.Log($"Level {level.Level} has chunk limit {level.Value}");
                upgrade._Levels[i] = new ResearchLinearUpgrade.LevelData(level.Level, ApplyGameParametersToChunkLimitCurrency(upgradesDisplay.Mode, level.Value), level.NextCost, ApplyGameParametersToChunkLimitCurrency(upgradesDisplay.Mode, level.NextValue));
            }

            return upgrade;
        }
    }
}
