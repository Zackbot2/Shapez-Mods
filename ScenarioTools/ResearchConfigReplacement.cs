using System;
using System.Collections.Generic;
using System.Text;

namespace ScenarioTools
{
    public class ResearchConfigReplacement
    {
        public int? BaseChunkLimitMultiplier;
        public int? BaseBlueprintRewardMultiplier;
        public int? MaxShapeLayers;
        public int? InitialResearchPoints;
        public string? ShapesConfigurationId;
        public string? ColorSchemeConfigurationId;
        public bool? ResearchLevelsAreProgressive;
        public ResearchPointsGenerationMode? ResearchPointsGenerationMode;
        public List<SerializedBlueprintCurrencyShape>? BlueprintCurrencyShapes;
        public string? IntroductionWikiEntryId;
        public List<string>? InitiallyUnlockedUpgrades;
        public string? TutorialConfig;

        public ResearchConfigReplacement() { }

        /// <summary>
        /// Apply this <see cref="ResearchConfigReplacement"/> to a <see cref="SerializedResearchConfig"/>, returning a new <see cref="SerializedResearchConfig"/> with the non-null values of this <see cref="ResearchConfigReplacement"/> applied."/>
        /// Does not modify the original <paramref name="config"/> object.
        /// Any values that are null will instead be taken from <paramref name="config"/>.
        /// </summary>
        /// <param name="config"></param>
        /// <returns>A new <see cref="SerializedResearchConfig"/> with the non-null values of this <see cref="ResearchConfigReplacement"/> applied, or null if <paramref name="config"/> is null.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public SerializedResearchConfig? ApplyTo(SerializedResearchConfig config)
        {
            if (config == null)
            {
                ScenarioToolsMod.Logger.Error?.Log("Cannot apply ResearchConfigReplacement to null SerializedResearchConfig.");
                return null;
            }

            return new SerializedResearchConfig()
            {
                BaseChunkLimitMultiplier = BaseChunkLimitMultiplier ?? config.BaseChunkLimitMultiplier,
                BaseBlueprintRewardMultiplier = BaseBlueprintRewardMultiplier ?? config.BaseBlueprintRewardMultiplier,
                MaxShapeLayers = MaxShapeLayers ?? config.MaxShapeLayers,
                InitialResearchPoints = InitialResearchPoints ?? config.InitialResearchPoints,
                ShapesConfigurationId = ShapesConfigurationId ?? config.ShapesConfigurationId,
                ColorSchemeConfigurationId = ColorSchemeConfigurationId ?? config.ColorSchemeConfigurationId,
                ResearchLevelsAreProgressive = ResearchLevelsAreProgressive ?? config.ResearchLevelsAreProgressive,
                ResearchPointsGenerationMode = ResearchPointsGenerationMode ?? config.ResearchPointsGenerationMode,
                BlueprintCurrencyShapes = BlueprintCurrencyShapes?.ToArray() ?? config.BlueprintCurrencyShapes,
                IntroductionWikiEntryId = IntroductionWikiEntryId ?? config.IntroductionWikiEntryId,
                InitiallyUnlockedUpgrades = InitiallyUnlockedUpgrades?.ToArray() ?? config.InitiallyUnlockedUpgrades,
                TutorialConfig = TutorialConfig ?? config.TutorialConfig
            };
        }
    }
}
