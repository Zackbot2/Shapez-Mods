using Core.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ScenarioTools
{
    public static class PrintUtils
    {
        public static void PrintSerializedScenarioInfo(SerializedGameScenario scenario, ILogger logger)
        {
            SerializedResearchConfig researchConfig = scenario.ResearchConfig;
            SerializedResearchPlayerLevelConfig playerLevelConfig = scenario.PlayerLevelConfig;

            string scenarioString = $"Unique ID: {scenario.UniqueId}\n" +
                $"RESEARCH CONFIG:\n" +
                $"\tBaseBlueprintRewardMultiplier: {researchConfig.BaseBlueprintRewardMultiplier}\n" +
                $"\tBaseChunkLimitMultiplier: {researchConfig.BaseChunkLimitMultiplier}\n" +
                $"\tResearchLevelsAreProgressive: {researchConfig.ResearchLevelsAreProgressive}\n" +
                $"\tBlueprintCurrencyShapes:\n";

            foreach (SerializedBlueprintCurrencyShape shape in researchConfig.BlueprintCurrencyShapes)
            {
                scenarioString += $"\t\t{shape.Shape} -> {shape.Amount}\n";
            }

            scenarioString += "PLAYER LEVEL CONFIG:\n" +
                "\tRewards:\n";

            foreach (SerializedResearchPlayerLevelConfig.Reward rewardConfig in playerLevelConfig.Rewards)
            {
                scenarioString += $"\t\tlevel {rewardConfig.MinimumLevel}:\n";

                foreach (ISerializedResearchReward reward in rewardConfig.Rewards)
                {
                    scenarioString += $"\t\t\t{reward.GetType().Name} -> {GetAmountForReward(scenario, reward)} ({GetAmountForRewardUnmultiplied(reward)})\n";
                }
            }

            scenarioString += "RESEARCH STATION CONFIG:\n" +
                "\tRecipes:\n";

            foreach (KeyValuePair<string, string> recipe in scenario.ResearchStationConfig.Recipes)
            {
                scenarioString += $"\t\t{recipe.Key} -> {recipe.Value}\n";
            }

            logger.Info?.Log($"Serialized scenario info:\n{scenarioString}");
        }

        public static long GetAmountForReward(SerializedGameScenario scenario, ISerializedResearchReward reward)
        {
            if (scenario == null)
            {
                return GetAmountForRewardUnmultiplied(reward);
            }

            return reward switch
            {
                SerializedResearchRewardResearchPoints researchPoints => researchPoints.Amount,
                SerializedResearchRewardBlueprintCurrency blueprintPoints => blueprintPoints.Amount * scenario.ResearchConfig.BaseBlueprintRewardMultiplier / 100,
                SerializedResearchRewardChunkLimit chunkLimit => chunkLimit.Amount * scenario.ResearchConfig.BaseChunkLimitMultiplier / 100,
                _ => 0
            };
        }

        public static long GetAmountForRewardUnmultiplied(ISerializedResearchReward reward)
        {
            return reward switch
            {
                SerializedResearchRewardResearchPoints researchPoints => researchPoints.Amount,
                SerializedResearchRewardBlueprintCurrency blueprintPoints => blueprintPoints.Amount,
                SerializedResearchRewardChunkLimit chunkLimit => chunkLimit.Amount,
                _ => 0
            };
        }
    }
}
