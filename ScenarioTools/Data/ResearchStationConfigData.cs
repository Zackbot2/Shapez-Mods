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
            string[] shapeRotations = GetRotationsForShape(input);

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
            string[] shapeRotations = GetRotationsForShape(input);

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

        private static string RotateShape(string shapeCode)
        {
            string newCode = "";
            string[] layers = shapeCode.Split(':');

            foreach (string layer in layers)
            {
                string newLayer = "";
                string lastTwoChars = layer[^2..];
                newLayer += lastTwoChars;
                newLayer += layer[..^2];
                newCode += newLayer + ":";
            }
            return newCode.TrimEnd(':');
        }

        private static string[] GetRotationsForShape(string shapeCode)
        {
            string[] rotations = new string[4];
            rotations[0] = shapeCode;
            string? lastShape = null;
            for (int i = 1; i < 4; i++)
            {
                string rotatedShape = RotateShape(lastShape ?? shapeCode);

                if (rotations.Contains(rotatedShape)) break;

                rotations[i] = rotatedShape;
            }
            return rotations;
        }
    }
}
