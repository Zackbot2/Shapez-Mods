using System;
using System.Collections.Generic;
using System.Text;

namespace ScenarioTools
{
    public static class ScenarioReplacementRegistry
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

        public static bool TryAddScenarioReplacement(ScenarioReplacement replacement)
        {
            if (replacement == null || string.IsNullOrWhiteSpace(replacement.ScenarioId) || GetScenarioReplacement(replacement.ScenarioId) != null)
            {
                return false;
            }

            ScenarioReplacements.Add(replacement);
            return true;
        }
    }
}
