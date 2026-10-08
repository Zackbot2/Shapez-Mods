using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static ScenarioTools.OperatorLevelModification;

namespace ScenarioTools.ScenarioData
{
    /// <summary>
    /// Corresponds to <see cref="SerializedResearchPlayerLevelConfig"/>.
    /// </summary>
    public class OperatorLevelData
    {
        public SerializedResearchPlayerLevelConfig OperatorLevelConfig { get => scenarioData.gameScenario.PlayerLevelConfig; set => scenarioData.gameScenario.PlayerLevelConfig = value; }
        public OperatorLevelRewardData Rewards { get; private set; }
        // raw SerializedResearchPlayerLevelConfig accessors for convenience
        public SerializedResearchPlayerLevelLevelShapes IconicLevelShapes { get => OperatorLevelConfig.IconicLevelShapes; set => OperatorLevelConfig.IconicLevelShapes = value; }
        public int IconicLevelShapeInterval { get => OperatorLevelConfig.IconicLevelShapeInterval; set => OperatorLevelConfig.IconicLevelShapeInterval = value; }
        public float RankDifficultyMultiplier { get => OperatorLevelConfig.RankDifficultyMultiplier; set => OperatorLevelConfig.RankDifficultyMultiplier = value; }


        protected ScenarioData scenarioData;

        public OperatorLevelData(ScenarioData scenarioData_)
        {
            scenarioData = scenarioData_;
            Rewards = new OperatorLevelRewardData(scenarioData_, this);
        }


        /// <summary>
        /// Corresponds to <see cref="SerializedResearchPlayerLevelConfig.Rewards"/>
        /// </summary>
        public class OperatorLevelRewardData
        {
            public SerializedResearchPlayerLevelConfig.Reward[] Rewards { get => operatorLevelData.OperatorLevelConfig.Rewards; set => operatorLevelData.OperatorLevelConfig.Rewards = value; }

            protected ScenarioData scenarioData;
            protected OperatorLevelData operatorLevelData;

            public OperatorLevelRewardData(ScenarioData scenarioData_, OperatorLevelData operatorLevelData_)
            {
                scenarioData = scenarioData_;
                operatorLevelData = operatorLevelData_;
            }

            public void ReplaceAllWith(IEnumerable<SerializedResearchPlayerLevelConfig.Reward> rewards)
            {
                Rewards = rewards.ToArray();
            }

            public bool ReplaceLevelRewards(SerializedResearchPlayerLevelConfig.Reward from, SerializedResearchPlayerLevelConfig.Reward to)
            {
                int index = Array.IndexOf(Rewards, from);
                if (index >= 0)
                {
                    Rewards[index] = to;
                    return true;
                }
                return false;
            }

            public bool AddLevelRewards(SerializedResearchPlayerLevelConfig.Reward levelRewards)
            {
                if (Rewards.Contains(levelRewards))
                {
                    return false;
                }
                Rewards = Rewards.Append(levelRewards).ToArray();
                return true;
            }
            
            public bool AddLevelRewards(int minimumLevel, int chunkLimitReward, long blueprintPointReward, int researchPointReward)
            {
                if (Rewards.Any(r => r.MinimumLevel == minimumLevel))
                {
                    return false;
                }

                List<ISerializedResearchReward> rewardList = new();

                // only add rewards that aren't 0, so they don't show up as just "0" in-game.
                if (chunkLimitReward != 0)
                {
                    rewardList.Add(new SerializedResearchRewardChunkLimit() { Amount = chunkLimitReward});
                }
                if (blueprintPointReward != 0)
                {
                    rewardList.Add(new SerializedResearchRewardBlueprintCurrency() { Amount = blueprintPointReward});
                }
                if (researchPointReward != 0)
                {
                    rewardList.Add(new SerializedResearchRewardChunkLimit() { Amount = researchPointReward });
                }

                return AddLevelRewards(new SerializedResearchPlayerLevelConfig.Reward()
                {
                    MinimumLevel = minimumLevel,
                    Rewards = rewardList.ToArray()
                });
            }

            public bool RemoveRewardsAtLevel(int minimumLevel)
            {
                SerializedResearchPlayerLevelConfig.Reward rewardToRemove = Rewards.FirstOrDefault(r => r.MinimumLevel == minimumLevel);
                if (rewardToRemove != null)
                {
                    return RemoveLevelRewards(rewardToRemove);
                }
                return false;
            }

            public bool RemoveLevelRewards(SerializedResearchPlayerLevelConfig.Reward levelRewards)
            {
                if (!Rewards.Contains(levelRewards))
                {
                    return false;
                }
                Rewards = Rewards.Where(r => r != levelRewards).ToArray();
                return true;
            }
        }
    }
}
