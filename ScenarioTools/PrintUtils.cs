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
                $"\tBaseChunkLimitMultiplier: {researchConfig.BaseChunkLimitMultiplier}\n" +
                $"\tResearchLevelsAreProgressive: {researchConfig.ResearchLevelsAreProgressive}\n" +
                $"\tBaseBlueprintRewardMultiplier: {researchConfig.BaseBlueprintRewardMultiplier}\n" +
                $"\tBaseChunkLimitMultiplier: {researchConfig.BaseChunkLimitMultiplier}\n" +
                $"\tBlueprintCurrencyShapes:\n";

            foreach (SerializedBlueprintCurrencyShape shape in researchConfig.BlueprintCurrencyShapes)
            {
                scenarioString += $"\t\t{shape.Shape} -> {shape.Amount}\n";
            }

            scenarioString += "PLAYER LEVEL CONFIG:\n" +
                "\tRewards:\n";

            foreach (SerializedResearchPlayerLevelConfig.Reward rewardConfig in playerLevelConfig.Rewards)
            {
                scenarioString += $"\t\t- level {rewardConfig.MinimumLevel}:\n";

                foreach (ISerializedResearchReward reward in rewardConfig.Rewards)
                {
                    scenarioString += $"\t\t\t- {reward.GetType().Name} -> {ScenarioReplacement.GetAmountForReward(scenario, reward)}\n";
                }
            }

            logger.Info?.Log($"Serialized scenario info:\n{scenarioString}");
        }
    }
}
