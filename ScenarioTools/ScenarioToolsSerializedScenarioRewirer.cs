using ScenarioTools;
using ScenarioTools.ScenarioData;
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

            ScenarioData.ScenarioData scenarioData = new(scenario);



            return scenarioData.gameScenario;
        }
    }
}

