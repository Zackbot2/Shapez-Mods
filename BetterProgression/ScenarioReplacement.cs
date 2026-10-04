using ShapezShifter.Kit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterProgression
{
    public class ScenarioReplacement
    {
        public string ScenarioId { get; private set; }
        public List<OperatorLevelReplacement> OperatorLevelReplacements { get; private set; } = new();
        public ResearchConfigReplacement ResearchConfig { get; } = new();

        public ScenarioReplacement(
            string scenarioId,
            ResearchConfigReplacement? researchConfig = null,
            List<OperatorLevelReplacement>? operatorLevelReplacements = null)
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
        /// <returns>Returns a new <see cref="SerializedGameScenario"/> based off of <paramref name="scenarioToReplace"/>, with non-null values depending on this <see cref="ScenarioReplacement"/>.</returns>
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
                foreach (OperatorLevelReplacement replacement in OperatorLevelReplacements)
                {
                    serializedRewards.Add(replacement.ToSerializedResearchPlayerLevelConfigReward());
                }
                scenarioToReplace.PlayerLevelConfig.Rewards = serializedRewards.ToArray();
            }
        }
    }
}
