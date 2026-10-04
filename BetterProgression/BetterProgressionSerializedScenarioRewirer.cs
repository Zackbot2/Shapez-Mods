using System;
using System.Collections.Generic;
using System.Text;

namespace BetterProgression
{
    public class BetterProgressionSerializedScenarioRewirer : ISerializedGameScenarioRewirer
    {
        public SerializedGameScenario ModifySerializedGameScenario(SerializedGameScenario scenario)
        {
            PrintScenarioInfo(scenario);

            ScenarioReplacement? modifiedScenario = ScenarioReplacementData.GetScenarioReplacement(scenario.UniqueId);
            if (modifiedScenario != null)
            {
                BetterProgressionMod.Logger.Info?.Log($"Modifying scenario {scenario.UniqueId}...");
                modifiedScenario.ReplaceScenario(scenario);
                PrintScenarioInfo(scenario);
            }
            else
            {
                BetterProgressionMod.Logger.Info?.Log($"Not modifying scenario {scenario.UniqueId}.");
            }
            return scenario;
        }

        private void PrintScenarioInfo(SerializedGameScenario scenario)
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
                    scenarioString += $"\t\t\t- {reward.GetType().Name} -> {GetAmountForReward(scenario, reward)}\n";
                }
            }

            BetterProgressionMod.Logger.Info?.Log($"Serialized scenario info:\n{scenarioString}");
        }

        private long GetAmountForReward(SerializedGameScenario scenario, ISerializedResearchReward reward)
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

