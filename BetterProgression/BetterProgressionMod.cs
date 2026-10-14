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
        internal static ILogger Logger { get; private set; } = null!;
        public static string ModName => nameof(BetterProgressionMod);

        private readonly BetterProgressionScenarioDataHandler _scenarioDataHandler;

        private ILHook? _computeCurrentChunkLimitHook;
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

            // method to hook
            MethodInfo collectCurrencyRewardsMethod = typeof(HUDResearchRewardHelpers).GetMethod("CollectCurrencyRewards", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            MethodInfo replacementMethod = typeof(BetterProgressionMod).GetMethod("CollectCurrencyRewardsWithMode", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

            // call sites
            MethodInfo rebuildViewMethod = typeof(HUDResearchUnlockSummary).GetMethod("RebuildView", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            MethodInfo upgradeSetter = typeof(HUDResearchRewardsDisplay)
                .GetProperty("Upgrade", BindingFlags.Instance | BindingFlags.Public)?
                .GetSetMethod(nonPublic: true)
                ?? throw new MissingMethodException("Upgrade setter not found.");
            MethodInfo rewardsSetter = typeof(HUDResearchRewardsDisplay)
                .GetProperty("Rewards", BindingFlags.Instance | BindingFlags.Public)?
                .GetSetMethod(nonPublic: true)
                ?? throw new MissingMethodException("Rewards setter not found.");

            // mode fields
            FieldInfo summaryModeField = typeof(HUDResearchUnlockSummary).GetField("Mode", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            FieldInfo displayModeField = typeof(HUDResearchRewardsDisplay).GetField("Mode", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            

            _rebuildViewHook = new ILHook(
                source: rebuildViewMethod,
                manip: context => PatchCurrencyRewards(context, summaryModeField, collectCurrencyRewardsMethod, replacementMethod)
            );

            _upgradeSetterHook = new ILHook(
                source: upgradeSetter,
                manip: context => PatchCurrencyRewards(context, displayModeField, collectCurrencyRewardsMethod, replacementMethod)
            );

            _rewardsSetterHook = new ILHook(
                source: rewardsSetter,
                manip: context => PatchCurrencyRewards(context, displayModeField, collectCurrencyRewardsMethod, replacementMethod)
            );
        }

        public void Dispose()
        {
            DataHandlers.RemoveDataHandler(_scenarioDataHandler);
            _rebuildViewHook?.Dispose();
            _rewardsSetterHook?.Dispose();
            _upgradeSetterHook?.Dispose();
        }

        private static int ApplyGameParametersToChunkLimitCurrency(GameMode mode, int input)
        {
            Logger.Info?.Log($"Applying game parameters to chunk limit currency: input={input}, multiplier={mode.Parameters.DifficultyParameters.ChunkLimitMultiplier}. Output will be {input * mode.Parameters.DifficultyParameters.ChunkLimitMultiplier / 100}");
            Logger.Info?.Log(Environment.StackTrace);
            return input * mode.Parameters.DifficultyParameters.ChunkLimitMultiplier / 100;
        }

        private static void PatchCurrencyRewards(ILContext context, FieldInfo modeField, MethodInfo originalMethod, MethodInfo replacementMethod)
        {
            ILCursor cursor = new(context);

            if (!cursor.TryGotoNext(MoveType.Before, x => x.MatchCall(originalMethod)))
            {
                throw new InvalidOperationException("Could not find CollectCurrencyRewards call in HUDResearchUnlockSummary.RebuildView");
            }

            if (cursor.Next == null || cursor.Next.OpCode != OpCodes.Call)
            {
                throw new InvalidOperationException("Expected a call instruction after the CollectCurrencyRewards call");
            }
            cursor.Next!.Operand = replacementMethod;
            
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldfld, modeField);
        }

        private static void CollectCurrencyRewardsWithMode(List<IResearchReward> rewards, GameIcons icons, List<HUDResearchRewardHelpers.RewardDisplayEntry> results, GameMode mode)
        {
            int startIndex = results.Count;

            HUDResearchRewardHelpers.CollectCurrencyRewards(rewards, icons, results);

            int rewardIndex = 0;

            for (int i = startIndex; i < results.Count; i++)
            {
                HUDResearchRewardHelpers.RewardDisplayEntry result = results[i];

                if (result.Icon == icons.RewardChunkLimit)
                {
                    while (rewardIndex < rewards.Count && rewards[rewardIndex] is not ResearchRewardChunkLimit)
                    {
                        rewardIndex++;
                    }

                    if (rewardIndex >= rewards.Count)
                    {
                        Logger.Error?.Log($"Could not find ResearchRewardChunkLimit matching result entry.");
                        continue;
                    }

                    ResearchRewardChunkLimit reward = (ResearchRewardChunkLimit)rewards[rewardIndex++];
                    ResearchRewardChunkLimit scaledAmount = new(new ChunkLimitCurrency(ApplyGameParametersToChunkLimitCurrency(mode, reward.Amount.Amount)));
                    
                    IText title = "research.reward-chunk-limit".T().Bind("amount", scaledAmount.Amount.Format());
                    results[i] = new HUDResearchRewardHelpers.RewardDisplayEntry(
                        title,
                        scaledAmount.Amount.Format(),
                        result.Icon,
                        title,
                        result.TooltipDescription
                    );

                }
            }
        }
    }
}
