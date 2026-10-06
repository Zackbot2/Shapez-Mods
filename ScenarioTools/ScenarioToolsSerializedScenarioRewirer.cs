using ScenarioTools;
using System;
using System.Collections.Generic;
using System.Text;

namespace ScenarioTools
{
    public class ScenarioToolsSerializedGameScenarioRewirer : ISerializedGameScenarioRewirer
    {
        public SerializedGameScenario ModifySerializedGameScenario(SerializedGameScenario scenario)
        {
            PrintUtils.PrintSerializedScenarioInfo(scenario, ScenarioToolsMod.Logger);

            ScenarioReplacement? modifiedScenario = ScenarioReplacementRegistry.GetScenarioReplacement(scenario.UniqueId);
            if (modifiedScenario != null)
            {
                ScenarioToolsMod.Logger.Info?.Log($"Modifying scenario {scenario.UniqueId}...");
                modifiedScenario.ReplaceScenario(scenario);
                PrintUtils.PrintSerializedScenarioInfo(scenario, ScenarioToolsMod.Logger);
            }
            else
            {
                ScenarioToolsMod.Logger.Info?.Log($"Not modifying scenario {scenario.UniqueId}.");
            }
            return scenario;
        }
    }
}

