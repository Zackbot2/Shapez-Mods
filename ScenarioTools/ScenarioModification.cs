using ScenarioTools.Research;
using ShapezShifter.Kit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScenarioTools
{
    public class ScenarioModification
    {
        public string ScenarioId { get; private set; }
        public List<OperatorLevelModification> OperatorLevelReplacements { get; private set; } = new();
        public ResearchConfigModification ResearchConfig { get; } = new();

        public ScenarioModification(
            string scenarioId,
            ResearchConfigModification? researchConfig = null,
            List<OperatorLevelModification>? operatorLevelReplacements = null)
        {
            ScenarioId = scenarioId;

            if (researchConfig != null)
            {
                ResearchConfig = researchConfig;
            }

            if (operatorLevelReplacements != null)
            {
                OperatorLevelReplacements = operatorLevelReplacements;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scenarioToReplace"></param>
        /// <returns>Returns a new <see cref="SerializedGameScenario"/> based off of <paramref name="scenarioToReplace"/>, with non-null values depending on this <see cref="ScenarioModification"/>.</returns>
        /// <exception cref="ArgumentException"></exception>
        public void ReplaceScenario(SerializedGameScenario scenarioToReplace)
        {
            if (scenarioToReplace == null || scenarioToReplace.UniqueId != ScenarioId)
            {
                return;
            }

            if (ResearchConfig != null)
            {
                scenarioToReplace.ResearchConfig = ResearchConfig.ApplyTo(scenarioToReplace.ResearchConfig);
            }

            // operator level reward replacements
            if (OperatorLevelReplacements.Count > 0)
            {
                List<SerializedResearchPlayerLevelConfig.Reward> serializedRewards = new();
                foreach (OperatorLevelModification replacement in OperatorLevelReplacements)
                {
                    serializedRewards.Add(replacement.ToSerializedResearchPlayerLevelConfigReward());
                }
                scenarioToReplace.PlayerLevelConfig.Rewards = serializedRewards.ToArray();
            }
        }

        public static long GetAmountForReward(SerializedGameScenario scenario, ISerializedResearchReward reward)
        {
            return reward switch
            {
                SerializedResearchRewardResearchPoints researchPoints => researchPoints.Amount,
                SerializedResearchRewardBlueprintCurrency blueprintCurrency => scenario.ResearchConfig.BaseBlueprintRewardMultiplier * blueprintCurrency.Amount / 100,
                SerializedResearchRewardChunkLimit chunkLimit => scenario.ResearchConfig.BaseChunkLimitMultiplier * chunkLimit.Amount,
                _ => 0,
            };
        }
    }
}
