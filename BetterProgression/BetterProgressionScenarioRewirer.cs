using ShapezShifter.Hijack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterProgression
{
    public class BetterProgressionScenarioRewirer : IGameScenarioRewirer
    {
        private enum RewardType
        {
            ResearchPoints,
            BlueprintPoints,
            ChunkLimit
        }

        private readonly Dictionary<string, List<SerializedResearchPlayerLevelConfig.Reward>> playerLevelConfigsByScenario = new()
        {
            {
                "insane-scenario", new List<SerializedResearchPlayerLevelConfig.Reward>
                // base reward multipliers: chunk limit = 300, blueprint points = 500
                {
                    CreateOperatorLevelReward(minimumLevel: 1, chunkLimitReward: 25, blueprintPointReward: 2000, researchPointReward: 2),
                    CreateOperatorLevelReward(minimumLevel: 10, chunkLimitReward: 50, blueprintPointReward: 3500, researchPointReward: 5),
                    CreateOperatorLevelReward(minimumLevel: 25, chunkLimitReward: 100, blueprintPointReward: 5000, researchPointReward: 10),
                    CreateOperatorLevelReward(minimumLevel: 50, chunkLimitReward: 150, blueprintPointReward: 7500, researchPointReward: 20),
                    CreateOperatorLevelReward(minimumLevel: 75, chunkLimitReward: 200, blueprintPointReward: 10000, researchPointReward: 30),
                    CreateOperatorLevelReward(minimumLevel: 100, chunkLimitReward: 500, blueprintPointReward: 15000, researchPointReward: 50),
                    CreateOperatorLevelReward(minimumLevel: 200, chunkLimitReward: 1000, blueprintPointReward: 25000, researchPointReward: 50),
                    CreateOperatorLevelReward(minimumLevel: 500, chunkLimitReward: 2000, blueprintPointReward: 50000, researchPointReward: 150)
                }
            },
            {
                "converter-regular-scenario", new List<SerializedResearchPlayerLevelConfig.Reward>
                // base reward multipliers: chunk limit = 250, blueprint points = 1000
                {
                    CreateOperatorLevelReward(minimumLevel: 1, chunkLimitReward: 25, blueprintPointReward: 2000, researchPointReward: 3),
                    CreateOperatorLevelReward(minimumLevel: 10, chunkLimitReward: 50, blueprintPointReward: 3500, researchPointReward: 6),
                    CreateOperatorLevelReward(minimumLevel: 25, chunkLimitReward: 100, blueprintPointReward: 5000, researchPointReward: 10),
                    CreateOperatorLevelReward(minimumLevel: 50, chunkLimitReward: 150, blueprintPointReward: 7500, researchPointReward: 15),
                    CreateOperatorLevelReward(minimumLevel: 75, chunkLimitReward: 250, blueprintPointReward: 10000, researchPointReward: 20),
                    CreateOperatorLevelReward(minimumLevel: 100, chunkLimitReward: 500, blueprintPointReward: 15000, researchPointReward: 30),
                    CreateOperatorLevelReward(minimumLevel: 200, chunkLimitReward: 1500, blueprintPointReward: 25000, researchPointReward: 50),
                    CreateOperatorLevelReward(minimumLevel: 500, chunkLimitReward: 5000, blueprintPointReward: 50000, researchPointReward: 150)
                }
            }
        };

        private static SerializedResearchPlayerLevelConfig.Reward CreateOperatorLevelReward(int minimumLevel, long chunkLimitReward, long blueprintPointReward, long researchPointReward)
        {
            return new()
            {
                MinimumLevel = minimumLevel,
                Rewards = new ISerializedResearchReward[]
                {
                    CreateSerializedReward(RewardType.ChunkLimit, chunkLimitReward),
                    CreateSerializedReward(RewardType.BlueprintPoints, blueprintPointReward),
                    CreateSerializedReward(RewardType.ResearchPoints, researchPointReward)
                }
            };
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

            BetterProgressionMod.Logger.Info?.Log($"Rewiring scenario {scenario.UniqueId.Id}...");

            if (playerLevelConfigsByScenario.TryGetValue(scenario.UniqueId.Id, out List<SerializedResearchPlayerLevelConfig.Reward> newRewards))
            {
                scenario.PlayerLevelConfig.Rewards = newRewards
                .Select(r => new ResearchPlayerLevelConfig.Reward(r))
                .ToList();
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

            string scenarioString = $"Unique ID: {scenario.UniqueId}\n" +
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
