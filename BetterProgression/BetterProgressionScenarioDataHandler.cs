using System;
using System.Collections.Generic;
using ScenarioTools.Data;

namespace BetterProgression
{
    public class BetterProgressionScenarioDataHandler : IScenarioDataHandler
    {
        public const string OPERATOR_CERTIFICATION_SCENARIO_ID = "onboarding-scenario";
        public const string CLASSIC_REGULAR_SCENARIO_ID = "default-scenario";
        public const string CLASSIC_HARD_SCENARIO_ID = "hard-scenario";
        public const string CLASSIC_INSANE_SCENARIO_ID = "insane-scenario";
        public const string CLASSIC_HEX_SCENARIO_ID = "hexagonal-scenario";
        public const string MANUFACTURE_REGULAR_SCENARIO_ID = "converter-regular-scenario";
        public const string MANUFACTURE_HARD_SCENARIO_ID = "converter-hard-scenario";

        public ScenarioData ModifyScenarioData(ScenarioData scenarioData)
        {
            return scenarioData.ScenarioId switch
            {
                CLASSIC_INSANE_SCENARIO_ID => ModifyInsaneScenario(scenarioData),
                _ => scenarioData
            };
        }

        private ScenarioData ModifyInsaneScenario(ScenarioData scenarioData)
        {
            if (scenarioData == null)
            {
                throw new ArgumentNullException(nameof(scenarioData));
            }

            if (scenarioData.ScenarioId != CLASSIC_INSANE_SCENARIO_ID)
            {
                BetterProgressionMod.Logger.Warning?.Log($"Attempted to modify {CLASSIC_INSANE_SCENARIO_ID} when passed {nameof(scenarioData)} had ID {scenarioData.ScenarioId}");
                return scenarioData;
            }
            // operator levels
            scenarioData.OperatorLevels.Rewards.SetRewardsForLevel(minimumLevel: 1, chunkLimitReward: 25, blueprintPointReward: 2000, researchPointReward: 2);
            scenarioData.OperatorLevels.Rewards.SetRewardsForLevel(minimumLevel: 10, chunkLimitReward: 50, blueprintPointReward: 3500, researchPointReward: 5);
            scenarioData.OperatorLevels.Rewards.SetRewardsForLevel(minimumLevel: 25, chunkLimitReward: 100, blueprintPointReward: 5000, researchPointReward: 10);
            scenarioData.OperatorLevels.Rewards.SetRewardsForLevel(minimumLevel: 50, chunkLimitReward: 150, blueprintPointReward: 7500, researchPointReward: 20);
            scenarioData.OperatorLevels.Rewards.SetRewardsForLevel(minimumLevel: 75, chunkLimitReward: 200, blueprintPointReward: 10000, researchPointReward: 30);
            scenarioData.OperatorLevels.Rewards.SetRewardsForLevel(minimumLevel: 100, chunkLimitReward: 500, blueprintPointReward: 15000, researchPointReward: 50);
            scenarioData.OperatorLevels.Rewards.SetRewardsForLevel(minimumLevel: 200, chunkLimitReward: 1000, blueprintPointReward: 25000, researchPointReward: 50);
            scenarioData.OperatorLevels.Rewards.SetRewardsForLevel(minimumLevel: 500, chunkLimitReward: 2000, blueprintPointReward: 50000, researchPointReward: 150);

            // blueprint shapes
            scenarioData.Research.BlueprintProgression.SetByCode("Su--Su--:WuRuWuCu:WbRbWbCb", newAmount: 4);
            scenarioData.Research.BlueprintProgression.SetByCode("Sc--Sc--:WwRwWwCw:WcRcWcCc", newAmount: 8);
            scenarioData.Research.BlueprintProgression.SetByCode("Sc--Sc--:WwRwWwCw:WcRcWcCc:P-P-P-P-:CwRwCw--", newAmount: 12);
            scenarioData.Research.BlueprintProgression.AddBlueprintCurrencyShape(new SerializedBlueprintCurrencyShape()
            {
                Shape = "Sc--Sc--:WwRkWwCk:WcRcWccc:P-P-P-P-:CwRwCw--",
                Amount = 20,
                RequiredUpgradeIds = new[] { "CBSpecial_Crystals" },
                RequiredMechanicIds = Array.Empty<string>()
            });
            return scenarioData;
        }
    }
}
