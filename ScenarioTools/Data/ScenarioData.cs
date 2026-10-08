using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScenarioTools.Data
{
    public class ScenarioData
    {
        public SerializedGameScenario gameScenario;
        public ResearchConfigData Research { get; private set; }
        public OperatorLevelData OperatorLevels { get; private set; }
        public string ScenarioId => gameScenario.UniqueId;

        public ScenarioData(SerializedGameScenario scenario)
        {
            if (scenario == null || string.IsNullOrEmpty(scenario.UniqueId))
            {
                throw new ArgumentException($"Attempted to instantiate a {nameof(ScenarioData)} from an invalid scenario.");
            }
            gameScenario = scenario;
            Research = new ResearchConfigData(this);
            OperatorLevels = new OperatorLevelData(this);
        }
    }
}
