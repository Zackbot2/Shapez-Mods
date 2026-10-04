using ShapezShifter.Hijack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterProgression
{
    public class BetterProgressionScenarioRewirer : IGameScenarioRewirer
    {
        public GameScenario ModifyGameScenario(GameScenario scenario)
        {
            PrintScenarioInfo(scenario);            
            return scenario;
        }

        private long GetAmountForReward(GameScenario scenario, IResearchReward reward)
        {
            return reward switch
            {
                ResearchRewardResearchPoints researchPoints => researchPoints.Amount.Amount,
                ResearchRewardBlueprintCurrency blueprintCurrency => scenario.ResearchConfig.BaseBlueprintRewardMultiplier * blueprintCurrency.Amount.Main / 100,
                ResearchRewardChunkLimit chunkLimit => scenario.ResearchConfig.BaseChunkLimitMultiplier * chunkLimit.Amount.Amount,
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
                    scenarioString += $"\t\t\t- {reward.GetType().Name} -> {GetAmountForReward(scenario, reward)}\n";
                }
            }

            BetterProgressionMod.Logger.Info?.Log($"Scenario INFO:\n{scenarioString}");
        }
    }
}
