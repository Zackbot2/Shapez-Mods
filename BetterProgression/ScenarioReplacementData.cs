using System;
using System.Collections.Generic;
using System.Text;

namespace BetterProgression
{
    public static class ScenarioReplacementData
    {
        public static List<ScenarioReplacement> ScenarioReplacements { get; } = new();

        public static ScenarioReplacement? GetScenarioReplacement(string scenarioId)
        {
            foreach (ScenarioReplacement replacement in ScenarioReplacements)
            {
                if (replacement.ScenarioId == scenarioId)
                {
                    return replacement;
                }
            }
            return null;
        }

        internal static void Initialize()
        {
            ScenarioReplacements.Add(new ScenarioReplacement(
                scenarioId: "insane-scenario",
                operatorLevelReplacements: new List<OperatorLevelReplacement>()
                {
                    new(minimumLevel: 1, chunkLimitReward: 25, blueprintPointReward: 2000, researchPointReward: 2),
                    new(minimumLevel: 10, chunkLimitReward: 50, blueprintPointReward: 3500, researchPointReward: 5),
                    new(minimumLevel: 25, chunkLimitReward: 100, blueprintPointReward: 5000, researchPointReward: 10),
                    new(minimumLevel: 50, chunkLimitReward: 150, blueprintPointReward: 7500, researchPointReward: 20),
                    new(minimumLevel: 75, chunkLimitReward: 200, blueprintPointReward: 10000, researchPointReward: 30),
                    new(minimumLevel: 100, chunkLimitReward: 500, blueprintPointReward: 15000, researchPointReward: 50),
                    new(minimumLevel: 200, chunkLimitReward: 1000, blueprintPointReward: 25000, researchPointReward: 50),
                    new(minimumLevel: 500, chunkLimitReward: 2000, blueprintPointReward: 50000, researchPointReward: 150)
                }
            ));

            ScenarioReplacements.Add(new ScenarioReplacement(
                scenarioId: "converter-regular-scenario",
                researchConfig: new ResearchConfigReplacement()
                {
                    BlueprintCurrencyShapes = new List<SerializedBlueprintCurrencyShape>()
                    {
                        new SerializedBlueprintCurrencyShape()
                        {
                            Shape = "CuCuCuCu",
                            Amount = 1
                        }
                    }
                },
                operatorLevelReplacements: new List<OperatorLevelReplacement>()
                {
                    new(minimumLevel: 1, chunkLimitReward: 25, blueprintPointReward: 2000, researchPointReward: 3),
                    new(minimumLevel: 10, chunkLimitReward: 50, blueprintPointReward: 3500, researchPointReward: 6),
                    new(minimumLevel: 25, chunkLimitReward: 100, blueprintPointReward: 5000, researchPointReward: 10),
                    new(minimumLevel: 50, chunkLimitReward: 150, blueprintPointReward: 7500, researchPointReward: 15),
                    new(minimumLevel: 75, chunkLimitReward: 250, blueprintPointReward: 10000, researchPointReward: 20),
                    new(minimumLevel: 100, chunkLimitReward: 500, blueprintPointReward: 15000, researchPointReward: 30),
                    new(minimumLevel: 200, chunkLimitReward: 1500, blueprintPointReward: 25000, researchPointReward: 50),
                    new(minimumLevel: 500, chunkLimitReward: 5000, blueprintPointReward: 50000, researchPointReward: 150)
                }
            ));
        }

    }
}
