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

namespace BetterProgression
{
    public class BetterProgressionMod : IMod
    {
        internal static ILogger Logger { get; private set; } = null!;
        public static string ModName => nameof(BetterProgressionMod);

        private readonly BetterProgressionScenarioDataHandler _scenarioDataHandler;

        private ILHook? _rebuildViewHook;
        private ILHook? _upgradeSetterHook;
        private ILHook? _rewardsSetterHook;

        public BetterProgressionMod(ILogger logger)
        {
            Logger = logger;

            _scenarioDataHandler = new();
            DataHandlers.AddDataHandler(_scenarioDataHandler);

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
                    ResearchRewardChunkLimit scaledAmount = new(new ChunkLimitCurrency(reward.Amount.Amount * mode.Parameters.DifficultyParameters.ChunkLimitMultiplier));
                    
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
