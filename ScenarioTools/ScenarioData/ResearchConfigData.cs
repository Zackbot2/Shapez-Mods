using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScenarioTools.ScenarioData
{
    public class ResearchConfigData
    {
        public SerializedResearchConfig ResearchConfig => scenarioData.gameScenario.ResearchConfig;
        public BlueprintShapes BlueprintProgression { get; private set; }
        public InitiallyUnlockedUpgradeData InitialUpgrades { get; private set; }

        // raw ResearchConfig data wrappers
        public int BaseChunkLimitMultiplier { get => ResearchConfig.BaseChunkLimitMultiplier; set => ResearchConfig.BaseChunkLimitMultiplier = value; }
        public int BaseBlueprintRewardMultiplier { get => ResearchConfig.BaseBlueprintRewardMultiplier; set => ResearchConfig.BaseBlueprintRewardMultiplier = value; }
        public int InitialResearchPoints { get => ResearchConfig.InitialResearchPoints; set => ResearchConfig.InitialResearchPoints = value; }
        public int MaxShapeLayers { get => ResearchConfig.MaxShapeLayers; set => ResearchConfig.MaxShapeLayers = value; }
        public string ShapesConfigurationId { get => ResearchConfig.ShapesConfigurationId; set => ResearchConfig.ShapesConfigurationId = value; }
        public string ColorSchemeConfigurationId { get => ResearchConfig.ColorSchemeConfigurationId; set => ResearchConfig.ColorSchemeConfigurationId = value; }
        public bool ResearchLevelsAreProgressive { get => ResearchConfig.ResearchLevelsAreProgressive; set => ResearchConfig.ResearchLevelsAreProgressive = value; }
        public ResearchPointsGenerationMode ResearchPointsGenerationMode { get => ResearchConfig.ResearchPointsGenerationMode; set => ResearchConfig.ResearchPointsGenerationMode = value; }
        public string IntroductionWikiEntryId { get => ResearchConfig.IntroductionWikiEntryId; set => ResearchConfig.IntroductionWikiEntryId = value; }

        protected ScenarioData scenarioData;

        public ResearchConfigData(ScenarioData scenarioData_)
        {
            scenarioData = scenarioData_;
            BlueprintProgression = new BlueprintShapes(scenarioData_, this);
            InitialUpgrades = new InitiallyUnlockedUpgradeData(scenarioData_, ResearchConfig);
        }


        public class InitiallyUnlockedUpgradeData
        {
            public string[] InitiallyUnlockedUpgradeIds
            {
                get => researchConfig.InitiallyUnlockedUpgrades;
                set => researchConfig.InitiallyUnlockedUpgrades = value;
            }
            protected ScenarioData scenarioData;
            protected SerializedResearchConfig researchConfig;

            public InitiallyUnlockedUpgradeData(ScenarioData scenarioData_, SerializedResearchConfig researchConfig_)
            {
                scenarioData = scenarioData_;
                researchConfig = researchConfig_;
            }

            public void ReplaceAllWith(IEnumerable<string> upgradeIds)
            {
                InitiallyUnlockedUpgradeIds = upgradeIds.ToArray();
            }

            public bool ReplaceUpgradeId(string from, string to)
            {
                int index = Array.IndexOf(InitiallyUnlockedUpgradeIds, from);
                if (index >= 0)
                {
                    InitiallyUnlockedUpgradeIds[index] = to;
                    return true;
                }
                else return false;
            }

            public bool AddUpgradeId(string upgradeId)
            {
                if (!InitiallyUnlockedUpgradeIds.Contains(upgradeId))
                {
                    InitiallyUnlockedUpgradeIds = InitiallyUnlockedUpgradeIds.Append(upgradeId).ToArray();
                    return true;
                }
                else return false;
            }

            public bool RemoveUpgradeId(string upgradeId)
            {
                if (InitiallyUnlockedUpgradeIds.Contains(upgradeId))
                {
                    InitiallyUnlockedUpgradeIds = InitiallyUnlockedUpgradeIds.Where(id => id != upgradeId).ToArray();
                    return true;
                }
                else return false;
            }
        }


        public class BlueprintShapes
        {
            public SerializedBlueprintCurrencyShape[] BlueprintCurrencyShapes
            {
                get => researchData.ResearchConfig.BlueprintCurrencyShapes;
                set => researchData.ResearchConfig.BlueprintCurrencyShapes = value;
            }

            protected ScenarioData scenarioData;
            protected ResearchConfigData researchData;

            public BlueprintShapes(ScenarioData scenarioData_, ResearchConfigData researchData_)
            {
                scenarioData = scenarioData_;
                researchData = researchData_;
            }

            public void ReplaceAllWith(IEnumerable<SerializedBlueprintCurrencyShape> blueprintShapes)
            {
                BlueprintCurrencyShapes = blueprintShapes.ToArray();
            }

            public bool ReplaceBlueprintCurrencyShape(SerializedBlueprintCurrencyShape from, SerializedBlueprintCurrencyShape to)
            {
                SerializedBlueprintCurrencyShape? shapeInArray = BlueprintCurrencyShapes.FirstOrDefault(bp => bp.Equals(from));
                if (shapeInArray == null)
                {
                    return false;
                }
                int index = Array.IndexOf(BlueprintCurrencyShapes, shapeInArray);
                if (index >= 0)
                {
                    BlueprintCurrencyShapes[index] = to;
                    return true;
                }
                else return false;
            }

            public bool AddBlueprintCurrencyShape(SerializedBlueprintCurrencyShape shape)
            {
                if (!BlueprintCurrencyShapes.Contains(shape))
                {
                    BlueprintCurrencyShapes = BlueprintCurrencyShapes.Append(shape).ToArray();
                    return true;
                }
                else return false;
            }

            public bool RemoveBlueprintCurrencyShape(SerializedBlueprintCurrencyShape shape)
            {
                if (BlueprintCurrencyShapes.Contains(shape))
                {
                    BlueprintCurrencyShapes = BlueprintCurrencyShapes.Where(bp => !bp.Equals(shape)).ToArray();
                    return true;
                }
                else return false;
            }

            // replace by shape code
            public SerializedBlueprintCurrencyShape[] GetByCode(string shapeCode)
            {
                return BlueprintCurrencyShapes.Where(bp => bp.Shape == shapeCode).ToArray();
            }

            // replace by amount
            public SerializedBlueprintCurrencyShape[] GetByAmount(int amount)
            {
                return BlueprintCurrencyShapes.Where(bp => bp.Amount == amount).ToArray();
            }

            // replace by required mechanics
            public SerializedBlueprintCurrencyShape[] GetByRequiredMechanics(List<string> requiredMechanics)
            {
                return BlueprintCurrencyShapes.Where(bp => bp.RequiredMechanicIds != null && bp.RequiredMechanicIds.SequenceEqual(requiredMechanics)).ToArray();
            }

            // replace by required upgrades
            public SerializedBlueprintCurrencyShape[] GetByRequiredUpgrades(List<string> requiredUpgrades)
            {
                return BlueprintCurrencyShapes.Where(bp => bp.RequiredUpgradeIds != null && bp.RequiredUpgradeIds.SequenceEqual(requiredUpgrades)).ToArray();
            }
        }
    }
}
