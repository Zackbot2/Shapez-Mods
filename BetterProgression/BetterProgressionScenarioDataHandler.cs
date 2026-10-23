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
                CLASSIC_REGULAR_SCENARIO_ID => ModifyClassicRegularScenario(scenarioData),
                CLASSIC_HARD_SCENARIO_ID => ModifyClassicHardScenario(scenarioData),
                CLASSIC_INSANE_SCENARIO_ID => ModifyClassicInsaneScenario(scenarioData),
                MANUFACTURE_REGULAR_SCENARIO_ID => ModifyManufactureRegularScenario(scenarioData),
                MANUFACTURE_HARD_SCENARIO_ID => ModifyManufactureHardScenario(scenarioData),
                _ => scenarioData
            };
        }

        private ScenarioData ModifyClassicRegularScenario(ScenarioData scenarioData)
        {
            if (scenarioData.ScenarioId != CLASSIC_REGULAR_SCENARIO_ID)
            {
                BetterProgressionMod.Logger.Warning?.Log($"Attempted to modify {CLASSIC_REGULAR_SCENARIO_ID} but the passed {nameof(scenarioData)} had ID {scenarioData.ScenarioId}");
                return scenarioData;
            }

            // operator levels
            scenarioData.OperatorLevels.Rewards.SetRewardsForLevel(minimumLevel: 1, chunkLimitReward: 25, blueprintPointReward: 1000, researchPointReward: 2);
            scenarioData.OperatorLevels.Rewards.SetRewardsForLevel(minimumLevel: 10, chunkLimitReward: 50, blueprintPointReward: 2500, researchPointReward: 5);
            scenarioData.OperatorLevels.Rewards.SetRewardsForLevel(minimumLevel: 25, chunkLimitReward: 150, blueprintPointReward: 5000, researchPointReward: 10);
            scenarioData.OperatorLevels.Rewards.SetRewardsForLevel(minimumLevel: 50, chunkLimitReward: 200, blueprintPointReward: 7500, researchPointReward: 20);
            scenarioData.OperatorLevels.Rewards.SetRewardsForLevel(minimumLevel: 75, chunkLimitReward: 250, blueprintPointReward: 10000, researchPointReward: 30);
            scenarioData.OperatorLevels.Rewards.SetRewardsForLevel(minimumLevel: 100, chunkLimitReward: 350, blueprintPointReward: 15000, researchPointReward: 50);
            scenarioData.OperatorLevels.Rewards.SetRewardsForLevel(minimumLevel: 150, chunkLimitReward: 500, blueprintPointReward: 20000, researchPointReward: 50);

            return scenarioData;
        }

        private ScenarioData ModifyClassicHardScenario(ScenarioData scenarioData)
        {
            if (scenarioData.ScenarioId != CLASSIC_HARD_SCENARIO_ID)
            {
                BetterProgressionMod.Logger.Warning?.Log($"Attempted to modify {CLASSIC_HARD_SCENARIO_ID} but the passed {nameof(scenarioData)} had ID {scenarioData.ScenarioId}");
                return scenarioData;
            }

            // operator levels
            scenarioData.OperatorLevels.Rewards.SetRewardsForLevel(minimumLevel: 1, chunkLimitReward: 25, blueprintPointReward: 5000, researchPointReward: 2);
            scenarioData.OperatorLevels.Rewards.SetRewardsForLevel(minimumLevel: 10, chunkLimitReward: 50, blueprintPointReward: 7500, researchPointReward: 5);
            scenarioData.OperatorLevels.Rewards.SetRewardsForLevel(minimumLevel: 25, chunkLimitReward: 150, blueprintPointReward: 10000, researchPointReward: 10);
            scenarioData.OperatorLevels.Rewards.SetRewardsForLevel(minimumLevel: 50, chunkLimitReward: 200, blueprintPointReward: 15000, researchPointReward: 20);
            scenarioData.OperatorLevels.Rewards.SetRewardsForLevel(minimumLevel: 75, chunkLimitReward: 250, blueprintPointReward: 20000, researchPointReward: 30);
            scenarioData.OperatorLevels.Rewards.SetRewardsForLevel(minimumLevel: 100, chunkLimitReward: 350, blueprintPointReward: 25000, researchPointReward: 50);
            scenarioData.OperatorLevels.Rewards.SetRewardsForLevel(minimumLevel: 150, chunkLimitReward: 500, blueprintPointReward: 30000, researchPointReward: 50);

            return scenarioData;
        }

        private ScenarioData ModifyClassicInsaneScenario(ScenarioData scenarioData)
        {

            if (scenarioData.ScenarioId != CLASSIC_INSANE_SCENARIO_ID)
            {
                BetterProgressionMod.Logger.Warning?.Log($"Attempted to modify {CLASSIC_INSANE_SCENARIO_ID} but the passed {nameof(scenarioData)} had ID {scenarioData.ScenarioId}");
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

        private ScenarioData ModifyManufactureRegularScenario(ScenarioData scenarioData)
        {
            if (scenarioData.ScenarioId != MANUFACTURE_REGULAR_SCENARIO_ID)
            {
                BetterProgressionMod.Logger.Warning?.Log($"Attempted to modify {MANUFACTURE_REGULAR_SCENARIO_ID} but the passed {nameof(scenarioData)} had ID {scenarioData.ScenarioId}");
                return scenarioData;
            }

            // operator levels
            scenarioData.OperatorLevels.Rewards.SetRewardsForLevel(minimumLevel: 1, chunkLimitReward: 25, blueprintPointReward: 500, researchPointReward: 3);
            scenarioData.OperatorLevels.Rewards.SetRewardsForLevel(minimumLevel: 10, chunkLimitReward: 50, blueprintPointReward: 1500, researchPointReward: 6);
            scenarioData.OperatorLevels.Rewards.SetRewardsForLevel(minimumLevel: 25, chunkLimitReward: 100, blueprintPointReward: 3000, researchPointReward: 10);
            scenarioData.OperatorLevels.Rewards.SetRewardsForLevel(minimumLevel: 50, chunkLimitReward: 150, blueprintPointReward: 6000, researchPointReward: 15);
            scenarioData.OperatorLevels.Rewards.SetRewardsForLevel(minimumLevel: 75, chunkLimitReward: 250, blueprintPointReward: 10000, researchPointReward: 20);
            scenarioData.OperatorLevels.Rewards.SetRewardsForLevel(minimumLevel: 100, chunkLimitReward: 500, blueprintPointReward: 15000, researchPointReward: 30);
            scenarioData.OperatorLevels.Rewards.SetRewardsForLevel(minimumLevel: 200, chunkLimitReward: 1500, blueprintPointReward: 20000, researchPointReward: 50);
            scenarioData.OperatorLevels.Rewards.SetRewardsForLevel(minimumLevel: 500, chunkLimitReward: 5000, blueprintPointReward: 30000, researchPointReward: 150);

            // blueprint shapes
            scenarioData.Research.BlueprintProgression.SetByCode("WuWuXuXu", newShapeCode: "CuXuCuXu");
            scenarioData.Research.BlueprintProgression.SetByCode("RrRrRrRr:CuCuCuCu", newShapeCode: "RbXrRbXr:Cr--Cr--", newRequiredUpgradeIds: new string[] { "ConverterMilestoneTier3" });
            scenarioData.Research.BlueprintProgression.SetByCode("WwWwWwWw:CrCyCrCy:Sy--Sy--", newShapeCode: "RrXyRr--:Wy--WyXy");
            scenarioData.Research.BlueprintProgression.SetByCode("WrWwWrWw:P-P-P-P-:CwCwCwCw", newShapeCode: "RrXmRrCw:SmP-SmP-:P-SwP-Xm:Wr--Wr--");
            scenarioData.Research.BlueprintProgression.SetByCode("crcrcrcr:WucgSucg:CwCwCwCw", newShapeCode: "RccwRccw:SwXwSwcc:P-SwP-Xw:Wc--Wc--");

            return scenarioData;
        }
    
        private ScenarioData ModifyManufactureHardScenario(ScenarioData scenarioData)
        {
            if (scenarioData.ScenarioId != MANUFACTURE_HARD_SCENARIO_ID)
            {
                BetterProgressionMod.Logger.Warning?.Log($"Attempted to modify {MANUFACTURE_HARD_SCENARIO_ID}, but the passed {nameof(scenarioData)} had ID {scenarioData.ScenarioId}");
                return scenarioData;
            }

            // operator levels
            scenarioData.OperatorLevels.Rewards.SetRewardsForLevel(minimumLevel: 1, chunkLimitReward: 25, blueprintPointReward: 500, researchPointReward: 3);
            scenarioData.OperatorLevels.Rewards.SetRewardsForLevel(minimumLevel: 10, chunkLimitReward: 50, blueprintPointReward: 1000, researchPointReward: 5);
            scenarioData.OperatorLevels.Rewards.SetRewardsForLevel(minimumLevel: 25, chunkLimitReward: 75, blueprintPointReward: 2500, researchPointReward: 10);
            scenarioData.OperatorLevels.Rewards.SetRewardsForLevel(minimumLevel: 50, chunkLimitReward: 100, blueprintPointReward: 5000, researchPointReward: 15);
            scenarioData.OperatorLevels.Rewards.SetRewardsForLevel(minimumLevel: 100, chunkLimitReward: 150, blueprintPointReward: 10000, researchPointReward: 20);
            scenarioData.OperatorLevels.Rewards.SetRewardsForLevel(minimumLevel: 150, chunkLimitReward: 200, blueprintPointReward: 20000, researchPointReward: 20);

            // blueprint shapes
            scenarioData.Research.BlueprintProgression.ReplaceAllWith(new SerializedBlueprintCurrencyShape[]
            {
                new()
                {
                    Shape = "XkXk----",
                    Amount = 2,
                    RequiredUpgradeIds = new[] { "ConverterMilestoneTier_Initial" },
                    RequiredMechanicIds = Array.Empty<string>()
                },
                new()
                {
                    Shape = "RuCuXuCu",
                    Amount = 8,
                    RequiredUpgradeIds = new[] { "ConverterMilestoneTier1" },
                    RequiredMechanicIds = Array.Empty<string>()
                },
                new()
                {
                    Shape = "RuCuXbCu:RbCbYbCb",
                    Amount = 32,
                    RequiredUpgradeIds = new[] { "ConverterMilestoneTier3" },
                    RequiredMechanicIds = Array.Empty<string>()
                },
                new()
                {
                    Shape = "RwCwXcCw:RcCcYcCc",
                    Amount = 64,
                    RequiredUpgradeIds = new[] { "ConverterMilestoneTier6" },
                    RequiredMechanicIds = Array.Empty<string>()
                },
                new()
                {
                    Shape = "RwCwXwCw:cccccccc:RwCwYwCw",
                    Amount = 128,
                    RequiredUpgradeIds = new[] { "ConverterMilestoneTier8" },
                    RequiredMechanicIds = Array.Empty<string>()
                },
                new()
                {
                    Shape = "RyCyYyCy:ckckP-ck:P-P-WrP-:RwCw--Cw",
                    Amount = 256,
                    RequiredUpgradeIds = new[] { "ConverterMilestoneTier_Final" },
                    RequiredMechanicIds = Array.Empty<string>()
                }
            });

            // trades
            scenarioData.ResearchStationConfig.AddRecipe("XuXu----", "WrSuWrSu");
            scenarioData.ResearchStationConfig.AddRecipe("RuCuXuCu", "Yb--Yb--");
            scenarioData.ResearchStationConfig.AddRecipe("RuCuXbCu:RbCbYbCb", "Yc--Yc--");
            scenarioData.ResearchStationConfig.AddRecipe("RwCwXcCw:RcCcYcCc", "Yw--Yw--");
            scenarioData.ResearchStationConfig.AddRecipe("RwCwXwCw:cccccccc:RwCwYwCw", "Xk--Xk--");
            scenarioData.ResearchStationConfig.AddRecipe("RyCyYyCy:ckckP-ck:P-P-WrP-:RwCw--Cw", "Xk--Xk--");

            return scenarioData;
        }
    }
}
