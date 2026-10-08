using ScenarioTools;
using ScenarioTools.Data;
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

            ScenarioData scenarioData = new(scenario);

            if (DataHandlers.Handlers.Count > 0)
            {
                ScenarioToolsMod.Logger.Info?.Log($"Modifying scenario data ({DataHandlers.Handlers.Count} handler{(DataHandlers.Handlers.Count != 1 ? "s" : "")})");

                foreach (IScenarioDataHandler handler in DataHandlers.Handlers)
                {
                    scenarioData = handler.ModifyScenarioData(scenarioData);
                }

                PrintUtils.PrintSerializedScenarioInfo(scenarioData.gameScenario, ScenarioToolsMod.Logger);
            }                

            return scenarioData.gameScenario;
        }
    }
}

