using System;
using System.Collections.Generic;
using System.Text;

namespace ScenarioTools
{
    public static class ScenarioModificationRegistry
    {
        public static List<ScenarioModification> ScenarioReplacements { get; } = new();

        public static ScenarioModification? GetScenarioReplacement(string scenarioId)
        {
            foreach (ScenarioModification replacement in ScenarioReplacements)
            {
                if (replacement.ScenarioId == scenarioId)
                {
                    return replacement;
                }
            }
            return null;
        }

        public static bool TryAddScenarioReplacement(ScenarioModification replacement)
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
