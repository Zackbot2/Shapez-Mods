using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScenarioTools.Data
{
    public class ResearchStationConfigData
    {
        public SerializedResearchStationConfig ResearchStationConfig => scenarioData.gameScenario.ResearchStationConfig;
        public IReadOnlyDictionary<string, string> Recipes => ResearchStationConfig.Recipes;

        protected ScenarioData scenarioData;

        public ResearchStationConfigData(ScenarioData scenarioData_)
        {
            this.scenarioData = scenarioData_;
        }

        public bool AddRecipe(string input, string output)
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(output))
            {
                return false;
            }

            string[] shapeRotations;
            if (scenarioData.gameScenario.ResearchConfig.ShapesConfigurationId == "DefaultShapesQuadConfiguration")
            {
                shapeRotations = ShapeUtils.GetRotationsForQuadShape(input);
            }
            else
            {
                shapeRotations = new string[] { input };
            }

            if (Recipes.Keys.All(k => shapeRotations.Contains(k)))
            {
                return false;
            }

            foreach (string shape in shapeRotations)
            {
                if (string.IsNullOrEmpty(shape) || Recipes.ContainsKey(shape))
                {
                    continue;
                }

                ResearchStationConfig.Recipes.Add(shape, output);
            }
            
            return true;
        }

        public bool SetRecipe(string input, string output)
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(output))
            {
                return false;
            }
            string[] shapeRotations = ShapeUtils.GetRotationsForQuadShape(input);

            foreach (string shape in shapeRotations)
            {
                if (Recipes.ContainsKey(shape))
                {
                    ResearchStationConfig.Recipes[input] = output;
                }
                else
                {
                    AddRecipe(shape, output);
                }
            }

            return true;
        }
    }
}
