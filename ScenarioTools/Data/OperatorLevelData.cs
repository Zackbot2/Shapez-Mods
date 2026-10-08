using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScenarioTools.Data
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

            public static SerializedResearchPlayerLevelConfig.Reward CreateLevelRewards(int minimumLevel, int chunkLimitReward, long blueprintPointReward, int researchPointReward)
            {
                return new()
                {
                    MinimumLevel = minimumLevel,
                    Rewards = CreateRewards(chunkLimitReward, blueprintPointReward, researchPointReward)
                };
            }

            public static ISerializedResearchReward[] CreateRewards(int chunkLimitReward, long blueprintPointReward, int researchPointReward)
            {
                List<ISerializedResearchReward> rewardList = new();

                // only add rewards that aren't 0, so they don't show up as just "0" in-game.
                if (chunkLimitReward != 0)
                {
                    rewardList.Add(new SerializedResearchRewardChunkLimit() { Amount = chunkLimitReward });
                }
                if (blueprintPointReward != 0)
                {
                    rewardList.Add(new SerializedResearchRewardBlueprintCurrency() { Amount = blueprintPointReward });
                }
                if (researchPointReward != 0)
                {
                    rewardList.Add(new SerializedResearchRewardResearchPoints() { Amount = researchPointReward });
                }

                return rewardList.ToArray();
            }

            public static bool LevelRewardsEqual(SerializedResearchPlayerLevelConfig.Reward a, SerializedResearchPlayerLevelConfig.Reward b)
            {
                if (a == null && b == null) return true;
                if (a == null || b == null) return false;
                if (a.MinimumLevel != b.MinimumLevel) return false;
                if (a.Rewards.Length != b.Rewards.Length) return false;
                for (int i = 0; i < a.Rewards.Length; i++)
                {
                    if (!RewardsEqual(a.Rewards[i], b.Rewards[i]))
                    {
                        return false;
                    }
                }
                return true;
            }

            public static bool RewardsEqual(ISerializedResearchReward a, ISerializedResearchReward b)
            {
                if (a == null && b == null) return true;
                if (a == null || b == null) return false;
                if (a.GetType() != b.GetType()) return false;
                return GetAmountForReward(a) == GetAmountForReward(b);
            }

            public static long GetAmountForReward(ISerializedResearchReward reward)
            {
                return reward switch
                {
                    SerializedResearchRewardResearchPoints researchPoints => researchPoints.Amount,
                    SerializedResearchRewardBlueprintCurrency blueprintPoints => blueprintPoints.Amount,
                    SerializedResearchRewardChunkLimit chunkLimit => chunkLimit.Amount,
                    _ => 0
                };
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

            /// <summary>
            /// Set the rewards for a given level. If rewards for that level don't exist, it will be added.
            /// </summary>
            /// <param name="minimumLevel"></param>
            /// <param name="chunkLimitReward"></param>
            /// <param name="blueprintPointReward"></param>
            /// <param name="researchPointReward"></param>
            public void SetRewardsForLevel(int minimumLevel, int chunkLimitReward, long blueprintPointReward, int researchPointReward)
            {
                SerializedResearchPlayerLevelConfig.Reward newRewards = new()
                {
                    MinimumLevel = minimumLevel,
                    Rewards = CreateRewards(chunkLimitReward, blueprintPointReward, researchPointReward)
                };

                if (!AddLevelRewards(newRewards))
                {
                    ReplaceLevelRewards(GetLevelRewards(minimumLevel), newRewards);
                }
            }

            public bool AddLevelRewards(SerializedResearchPlayerLevelConfig.Reward levelRewards)
            {
                if (Rewards.Any(r => r.MinimumLevel == levelRewards.MinimumLevel))
                {
                    return false;
                }
                Rewards = Rewards.Append(levelRewards).ToArray();
                return true;
            }

            public bool AddLevelRewards(int minimumLevel, ISerializedResearchReward[] rewards)
            {
                if (Rewards.Any(r => r.MinimumLevel == minimumLevel))
                {
                    return false;
                }
                Rewards = Rewards.Append(new SerializedResearchPlayerLevelConfig.Reward()
                {
                    MinimumLevel = minimumLevel,
                    Rewards = rewards
                }).ToArray();
                return true;
            }

            public bool AddLevelRewards(int minimumLevel, int chunkLimitReward, long blueprintPointReward, int researchPointReward)
            {
                if (Rewards.Any(r => r.MinimumLevel == minimumLevel))
                {
                    return false;
                }

                return AddLevelRewards(new SerializedResearchPlayerLevelConfig.Reward()
                {
                    MinimumLevel = minimumLevel,
                    Rewards = CreateRewards(chunkLimitReward, blueprintPointReward, researchPointReward)
                });
            }

            public SerializedResearchPlayerLevelConfig.Reward GetLevelRewards(int minimumLevel)
            {
                return Rewards.FirstOrDefault(r => r.MinimumLevel == minimumLevel);
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
                if (!Rewards.Any(r => LevelRewardsEqual(r, levelRewards)))
                {
                    return false;
                }
                Rewards = Rewards.Where(r => !LevelRewardsEqual(r, levelRewards)).ToArray();
                return true;
            }
        }
    }
}
