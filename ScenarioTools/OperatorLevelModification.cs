using System;
using System.Collections.Generic;
using System.Text;

namespace ScenarioTools
{
    public class OperatorLevelModification
    {
        public enum RewardType
        {
            ResearchPoints,
            BlueprintPoints,
            ChunkLimit
        }

        public int MinimumLevel { get; private set; }
        public ISerializedResearchReward[] Rewards { get; private set; }

        public OperatorLevelModification(int minimumLevel, ISerializedResearchReward[] rewards)
        {
            MinimumLevel = minimumLevel;
            Rewards = rewards;
        }

        public OperatorLevelModification(int minimumLevel, long chunkLimitReward, long blueprintPointReward, long researchPointReward)
        {
            MinimumLevel = minimumLevel;
            Rewards = new ISerializedResearchReward[]
            {
                CreateSerializedReward(RewardType.ChunkLimit, chunkLimitReward),
                CreateSerializedReward(RewardType.BlueprintPoints, blueprintPointReward),
                CreateSerializedReward(RewardType.ResearchPoints, researchPointReward)
            };
        }

        public SerializedResearchPlayerLevelConfig.Reward ToSerializedResearchPlayerLevelConfigReward()
        {
            return new SerializedResearchPlayerLevelConfig.Reward
            {
                MinimumLevel = MinimumLevel,
                Rewards = Rewards
            };
        }

        public static SerializedResearchPlayerLevelConfig.Reward CreateOperatorLevelReward(int minimumLevel, int chunkLimitReward, long blueprintPointReward, int researchPointReward)
        {
            List<ISerializedResearchReward> rewardList = new();

            // only add rewards that aren't 0, so they don't show up as just "0" in-game.
            if (chunkLimitReward != 0)
            {
                rewardList.Add(CreateSerializedReward(RewardType.ChunkLimit, chunkLimitReward));
            }
            if (blueprintPointReward != 0)
            {
                rewardList.Add(CreateSerializedReward(RewardType.BlueprintPoints, blueprintPointReward));
            }
            if (researchPointReward != 0)
            {
                rewardList.Add(CreateSerializedReward(RewardType.ResearchPoints, researchPointReward));
            }

            return new SerializedResearchPlayerLevelConfig.Reward()
            {
                MinimumLevel = minimumLevel,
                Rewards = rewardList.ToArray()
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

        public static Type GetSerializedTypeForRewardType(RewardType rewardType)
        {
            return rewardType switch
            {
                RewardType.ResearchPoints => typeof(SerializedResearchRewardResearchPoints),
                RewardType.BlueprintPoints => typeof(SerializedResearchRewardBlueprintCurrency),
                RewardType.ChunkLimit => typeof(SerializedResearchRewardChunkLimit),
                _ => throw new ArgumentException($"Invalid reward type: {rewardType}")
            };
        }
    }
}
