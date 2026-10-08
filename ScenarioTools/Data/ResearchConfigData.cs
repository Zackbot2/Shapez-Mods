using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScenarioTools.Data
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

            /// <summary>
            /// Set the values of a blueprint shape, found by its shape code. Null paramaters will not replace the original.
            /// </summary>
            /// <param name="shapeCode"></param>
            /// <param name="newAmount"></param>
            /// <param name="newRequiredUpgradeIds"></param>
            /// <param name="newRequiredMechanicIds"></param>
            /// <returns>Returns true if successful, false otherwise.</returns>
            public bool SetByCode(string shapeCode, string? newShapeCode = null, int? newAmount = null, string[]? newRequiredUpgradeIds = null, string[]? newRequiredMechanicIds = null)
            {
                if (string.IsNullOrWhiteSpace(shapeCode)) return false;

                SerializedBlueprintCurrencyShape? shapeInArray = BlueprintCurrencyShapes.FirstOrDefault(bp => bp.Shape == shapeCode);
                if (shapeInArray == null)
                {
                    return false;
                }

                int index = Array.IndexOf(BlueprintCurrencyShapes, shapeInArray);
                if (newShapeCode != null)
                {
                    shapeInArray.Shape = newShapeCode;
                }
                if (newAmount != null)
                {
                    shapeInArray.Amount = newAmount.Value;
                }
                if (newRequiredUpgradeIds != null)
                {
                    shapeInArray.RequiredUpgradeIds = newRequiredUpgradeIds;
                }
                if (newRequiredMechanicIds != null)
                {
                    shapeInArray.RequiredMechanicIds = newRequiredMechanicIds;
                }
                return true;
            }

            // replace by amount
            public SerializedBlueprintCurrencyShape[] GetByAmount(int amount)
            {
                return BlueprintCurrencyShapes.Where(bp => bp.Amount == amount).ToArray();
            }

            /// <summary>
            /// Set the values of a blueprint shape, found by its amount. Null paramaters will not replace the original.
            /// </summary>
            /// <param name="shapeCode"></param>
            /// <param name="amount"></param>
            /// <param name="newRequiredUpgradeIds"></param>
            /// <param name="newRequiredMechanicIds"></param>
            /// <returns>Returns true if successful, false otherwise.</returns>
            public bool SetByAmount(int amount, string? newShapeCode = null, int? newAmount = null, string[]? newRequiredUpgradeIds = null, string[]? newRequiredMechanicIds = null)
            {
                SerializedBlueprintCurrencyShape? shapeInArray = BlueprintCurrencyShapes.FirstOrDefault(bp => bp.Amount == amount);
                if (shapeInArray == null)
                {
                    return false;
                }

                int index = Array.IndexOf(BlueprintCurrencyShapes, shapeInArray);
                if (newShapeCode != null)
                {
                    shapeInArray.Shape = newShapeCode;
                }
                if (newAmount != null)
                {
                    shapeInArray.Amount = newAmount.Value;
                }
                if (newRequiredUpgradeIds != null)
                {
                    shapeInArray.RequiredUpgradeIds = newRequiredUpgradeIds;
                }
                if (newRequiredMechanicIds != null)
                {
                    shapeInArray.RequiredMechanicIds = newRequiredMechanicIds;
                }
                return true;
            }

            // replace by required mechanics
            public SerializedBlueprintCurrencyShape[] GetByRequiredMechanics(List<string> requiredMechanics)
            {
                return BlueprintCurrencyShapes.Where(bp => bp.RequiredMechanicIds != null && bp.RequiredMechanicIds.SequenceEqual(requiredMechanics)).ToArray();
            }

            /// <summary>
            /// Set the values of a blueprint shape, found by its required mechanics. Null paramaters will not replace the originals.
            /// The required mechanics must match exactly for this to match.
            /// </summary>
            /// <param name="requiredMechanicIds"></param>
            /// <param name="newShapeCode"></param>
            /// <param name="newAmount"></param>
            /// <param name="newRequiredUpgradeIds"></param>
            /// <param name="newRequiredMechanicIds"></param>
            /// <returns>Returns true if successful, false otherwise.</returns>
            public bool SetByAllRequiredMechanics(IEnumerable<string> requiredMechanicIds, string? newShapeCode = null, int? newAmount = null, string[]? newRequiredUpgradeIds = null, string[]? newRequiredMechanicIds = null)
            {
                SerializedBlueprintCurrencyShape? shapeInArray = BlueprintCurrencyShapes.FirstOrDefault(bp => bp.RequiredMechanicIds != null && bp.RequiredMechanicIds.SequenceEqual(requiredMechanicIds));
                if (shapeInArray == null)
                {
                    return false;
                }
                int index = Array.IndexOf(BlueprintCurrencyShapes, shapeInArray);
                if (newShapeCode != null)
                {
                    shapeInArray.Shape = newShapeCode;
                }
                if (newAmount != null)
                {
                    shapeInArray.Amount = newAmount.Value;
                }
                if (newRequiredUpgradeIds != null)
                {
                    shapeInArray.RequiredUpgradeIds = newRequiredUpgradeIds;
                }
                if (newRequiredMechanicIds != null)
                {
                    shapeInArray.RequiredMechanicIds = newRequiredMechanicIds;
                }
                return true;
            }

            /// <summary>
            /// Set the values of a blueprint shape, found by one of its required mechanics. Null paramaters will not replace the originals.
            /// </summary>
            /// <param name="requiredMechanicId"></param>
            /// <param name="newShapeCode"></param>
            /// <param name="newAmount"></param>
            /// <param name="newRequiredUpgradeIds"></param>
            /// <param name="newRequiredMechanicIds"></param>
            /// <returns>Returns true if successful, false otherwise.</returns>
            public bool SetByOneRequiredMechanic(string requiredMechanicId, string? newShapeCode = null, int? newAmount = null, string[]? newRequiredUpgradeIds = null, string[]? newRequiredMechanicIds = null)
            {
                SerializedBlueprintCurrencyShape? shapeInArray = BlueprintCurrencyShapes.FirstOrDefault(bp => bp.RequiredMechanicIds != null && bp.RequiredMechanicIds.Contains(requiredMechanicId));
                if (shapeInArray == null)
                {
                    return false;
                }
                int index = Array.IndexOf(BlueprintCurrencyShapes, shapeInArray);
                if (newShapeCode != null)
                {
                    shapeInArray.Shape = newShapeCode;
                }
                if (newAmount != null)
                {
                    shapeInArray.Amount = newAmount.Value;
                }
                if (newRequiredUpgradeIds != null)
                {
                    shapeInArray.RequiredUpgradeIds = newRequiredUpgradeIds;
                }
                if (newRequiredMechanicIds != null)
                {
                    shapeInArray.RequiredMechanicIds = newRequiredMechanicIds;
                }
                return true;
            }

            // replace by required upgrades
            public SerializedBlueprintCurrencyShape[] GetByRequiredUpgrades(IEnumerable<string> requiredUpgrades)
            {
                return BlueprintCurrencyShapes.Where(bp => bp.RequiredUpgradeIds != null && bp.RequiredUpgradeIds.SequenceEqual(requiredUpgrades)).ToArray();
            }

            /// <summary>
            /// Set the values of a blueprint shape, found by its required upgrades. Null paramaters will not replace the originals.
            /// The required upgrades must match exactly for this to match.
            /// </summary>
            /// <param name="requiredUpgradeIds"></param>
            /// <param name="newShapeCode"></param>
            /// <param name="newAmount"></param>
            /// <param name="newRequiredUpgradeIds"></param>
            /// <param name="newRequiredMechanicIds"></param>
            /// <returns>Returns true if successful, false otherwise.</returns>
            public bool SetByAllRequiredUpgrades(IEnumerable<string> requiredUpgradeIds, string? newShapeCode = null, int? newAmount = null, string[]? newRequiredUpgradeIds = null, string[]? newRequiredMechanicIds = null)
            {
                SerializedBlueprintCurrencyShape? shapeInArray = BlueprintCurrencyShapes.FirstOrDefault(bp => bp.RequiredUpgradeIds != null && bp.RequiredUpgradeIds.SequenceEqual(requiredUpgradeIds));
                if (shapeInArray == null)
                {
                    return false;
                }
                int index = Array.IndexOf(BlueprintCurrencyShapes, shapeInArray);
                if (newShapeCode != null)
                {
                    shapeInArray.Shape = newShapeCode;
                }
                if (newAmount != null)
                {
                    shapeInArray.Amount = newAmount.Value;
                }
                if (newRequiredUpgradeIds != null)
                {
                    shapeInArray.RequiredUpgradeIds = newRequiredUpgradeIds;
                }
                if (newRequiredMechanicIds != null)
                {
                    shapeInArray.RequiredMechanicIds = newRequiredMechanicIds;
                }
                return true;
            }

            /// <summary>
            /// Set the values of a blueprint shape, found by one of its required upgrades. Null paramaters will not replace the originals.
            /// </summary>
            /// <param name="requiredUpgradeId"></param>
            /// <param name="newShapeCode"></param>
            /// <param name="newAmount"></param>
            /// <param name="newRequiredUpgradeIds"></param>
            /// <param name="newRequiredMechanicIds"></param>
            /// <returns>Returns true if successful, false otherwise.</returns>
            public bool SetByOneRequiredUpgrade(string requiredUpgradeId, string? newShapeCode = null, int? newAmount = null, string[]? newRequiredUpgradeIds = null, string[]? newRequiredMechanicIds = null)
            {
                SerializedBlueprintCurrencyShape? shapeInArray = BlueprintCurrencyShapes.FirstOrDefault(bp => bp.RequiredUpgradeIds != null && bp.RequiredUpgradeIds.Contains(requiredUpgradeId));
                if (shapeInArray == null)
                {
                    return false;
                }
                int index = Array.IndexOf(BlueprintCurrencyShapes, shapeInArray);
                if (newShapeCode != null)
                {
                    shapeInArray.Shape = newShapeCode;
                }
                if (newAmount != null)
                {
                    shapeInArray.Amount = newAmount.Value;
                }
                if (newRequiredUpgradeIds != null)
                {
                    shapeInArray.RequiredUpgradeIds = newRequiredUpgradeIds;
                }
                if (newRequiredMechanicIds != null)
                {
                    shapeInArray.RequiredMechanicIds = newRequiredMechanicIds;
                }
                return true;
            }
        }
    }
}
