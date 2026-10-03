using ShapezShifter.Hijack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static UISoundEffects;

namespace BetterProgression
{
    public class BetterProgressionScenarioRewirer : IGameScenarioRewirer
    {
        private readonly Dictionary<string, List<ResearchPlayerLevelConfig.Reward>> playerLevelConfigsByScenario = new()
        {
            { 
                "insane-scenario", new List<ResearchPlayerLevelConfig.Reward>
                {
                    new(new SerializedResearchPlayerLevelConfig.Reward()
                    {
                        MinimumLevel = 1,
                        Rewards = new ISerializedResearchReward[]
                        { 
                            CreateSerializedReward(RewardType.ChunkLimit, 25),
                            CreateSerializedReward(RewardType.BlueprintPoints, 5000),
                            CreateSerializedReward(RewardType.ResearchPoints, 2)
                        }
                    }),
                    new(new SerializedResearchPlayerLevelConfig.Reward()
                    {
                        MinimumLevel = 10,
                        Rewards = new ISerializedResearchReward[]
                        {
                            CreateSerializedReward(RewardType.ChunkLimit, 50),
                            CreateSerializedReward(RewardType.BlueprintPoints, 10000),
                            CreateSerializedReward(RewardType.ResearchPoints, 5)
                        }
                    })
                }
            }
        };

        private ResearchPlayerLevelConfig.Reward CreateRewardForPlayerLevel(int playerLevel, RewardType rewardType, long rewardAmount)
        {
            return new ResearchPlayerLevelConfig.Reward(new SerializedResearchPlayerLevelConfig.Reward()
            {
                MinimumLevel = playerLevel,
                Rewards = new ISerializedResearchReward[] { new SerializedResearchRewardChunkLimit() { Amount = playerLevel * 100 } }
            });
        }

        private enum RewardType
        {
            ResearchPoints,
            BlueprintPoints,
            ChunkLimit
        }

        private static Type GetSerializedTypeForRewardType(RewardType rewardType)
        {
            return rewardType switch
            {
                RewardType.ResearchPoints => typeof(SerializedResearchRewardResearchPoints),
                RewardType.BlueprintPoints => typeof(SerializedResearchRewardBlueprintCurrency),
                RewardType.ChunkLimit => typeof(SerializedResearchRewardChunkLimit),
                _ => throw new ArgumentException($"Invalid reward type: {rewardType}")
            };
        }

        private static ISerializedResearchReward CreateSerializedReward(RewardType rewardType, long amount)
        {
            return rewardType switch
            {
                RewardType.ResearchPoints => new SerializedResearchRewardResearchPoints() { Amount = (int)amount },
                RewardType.BlueprintPoints => new SerializedResearchRewardBlueprintCurrency() { Amount = amount },
                RewardType.ChunkLimit => new SerializedResearchRewardChunkLimit() { Amount = (int)amount },
                _ => throw new ArgumentException($"Invalid reward type: {rewardType}")
            };
        }

        public GameScenario ModifyGameScenario(GameScenario scenario)
        {
            PrintScenarioInfo(scenario);

            IOrderedEnumerable<ResearchPlayerLevelConfig.Reward> orderedLevels = scenario.PlayerLevelConfig.Rewards.OrderBy(level => level.MinimumLevel);

            for (int i = 0; i < orderedLevels.Count(); i++)
            {
                ResearchPlayerLevelConfig.Reward level = orderedLevels.ElementAt(i);
                
            }

            PrintScenarioInfo(scenario);
            return scenario;
        }

        private long GetAmountForReward(IResearchReward reward)
        {
            return reward switch
            {
                ResearchRewardResearchPoints researchPoints => researchPoints.Amount.Amount,
                ResearchRewardBlueprintCurrency blueprintCurrency => blueprintCurrency.Amount.Main,
                ResearchRewardChunkLimit chunkLimit => chunkLimit.Amount.Amount,
                _ => 0,
            };
        }

        private void PrintScenarioInfo(GameScenario scenario)
        {
            ResearchConfig researchConfig = scenario.ResearchConfig;
            ResearchPlayerLevelConfig playerLevelConfig = scenario.PlayerLevelConfig;

            string scenarioString = $"Unique ID: {scenario.UniqueId.Id}\n" +
                $"RESEARCH CONFIG:\n" +
                $"\tBaseChunkLimitMultiplier: {researchConfig.BaseChunkLimitMultiplier}\n" +
                $"\tResearchLevelsAreProgressive: {researchConfig.ResearchLevelsAreProgressive}\n" +
                $"\tBaseBlueprintRewardMultiplier: {researchConfig.BaseBlueprintRewardMultiplier}\n" +
                $"\tBaseChunkLimitMultiplier: {researchConfig.BaseChunkLimitMultiplier}\n" +
                $"\tBlueprintCurrencyShapes:\n";

            foreach (BlueprintCurrencyShape shape in researchConfig.BlueprintCurrencyShapes)
            {
                scenarioString += $"\t\t{shape.ShapeHash} -> {shape.Amount.Main}\n";
            }

            scenarioString += "PLAYER LEVEL CONFIG:\n" +
                "\tRewards:\n";

            foreach (ResearchPlayerLevelConfig.Reward rewardConfig in playerLevelConfig.Rewards)
            {
                scenarioString += $"\t\t- level {rewardConfig.MinimumLevel}:\n";

                foreach (IResearchReward reward in rewardConfig.Rewards)
                {
                    scenarioString += $"\t\t\t- {reward.GetType().Name} -> {GetAmountForReward(reward)}\n";
                }
            }

            BetterProgressionMod.Logger.Info?.Log($"Scenario INFO:\n{scenarioString}");
        }
    }
}
