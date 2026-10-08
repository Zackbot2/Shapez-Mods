using Core.Logging;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using ShapezShifter.Hijack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ScenarioTools
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// This class is super similar to <see cref="GameScenarioInterceptor"/>, for the sole reason that I wanted it to be as close to the original implementation as possible.
    /// Not trying to steal any code, it would actually be easier if I just did it myself!
    /// </remarks>
    internal class SerializedGameScenarioInterceptor : IDisposable
    {
        private readonly IRewirerProvider RewirerProvider;
        private readonly ILogger Logger;
        private readonly ILHook IlHook;

        public SerializedGameScenarioInterceptor(IRewirerProvider rewirerProvider, ILogger logger)
        {
            RewirerProvider = rewirerProvider;
            Logger = logger;

            MethodInfo target = typeof(GameMode).GetMethod("From", BindingFlags.Static | BindingFlags.Public);
            IlHook = new ILHook(
                source: target!,
                manip: context =>
                {
                    ILCursor cursor = new(context);
                    cursor.GotoNext(MoveType.After, instruction => instruction.MatchCallvirt<IGameData>("GetRawScenario"));
                    cursor.EmitDelegate<Func<SerializedGameScenario, SerializedGameScenario>>(Postfix);
                });
        }

        public void Dispose()
        {
            IlHook.Dispose();
        }

        private SerializedGameScenario Postfix(SerializedGameScenario scenario)
        {
            // make a copy so that we don't modify the original serialized scenario.
            // that data is kept around as a reference of what the json contains so the game never has to read it again.
            SerializedGameScenario copiedScenario = SerializedGameScenario.DeepCopy(scenario);
            foreach (ISerializedGameScenarioRewirer rewirer in RewirerProvider.RewirersOfType<ISerializedGameScenarioRewirer>())
            {
                copiedScenario = rewirer.ModifySerializedGameScenario(copiedScenario);
            }

            return copiedScenario;
        }

        public static SerializedGameScenario DeepCopySerializedGameScenario(SerializedGameScenario scenario)
        {
            return new()
            {
                // value semantics
                FormatVersion = scenario.FormatVersion,
                GameVersion = scenario.GameVersion,
                UniqueId = scenario.UniqueId,
                IsTutorial = scenario.IsTutorial,
                SupportedGameModes = scenario.SupportedGameModes.ToArray(),
                NextScenarios = scenario.NextScenarios.ToArray(),
                ExampleShapes = scenario.ExampleShapes.ToArray(),
                Title = scenario.Title,
                Description = scenario.Description,
                PreviewImageId = scenario.PreviewImageId,

                // the hard part
                PlayerBadge = new SerializedPlayerBadge() { Title = scenario.PlayerBadge.Title, ImageId = scenario.PlayerBadge.ImageId },
                ResearchConfig = DeepCopySerializedResearchConfig(scenario.ResearchConfig),
                Progression = DeepCopySerializedResearchProgression(scenario.Progression),

            };
        }

        public static SerializedResearchConfig DeepCopySerializedResearchConfig(SerializedResearchConfig config)
        {
            return new()
            {
                BaseChunkLimitMultiplier = config.BaseChunkLimitMultiplier,
                BaseBlueprintRewardMultiplier = config.BaseBlueprintRewardMultiplier,
                MaxShapeLayers = config.MaxShapeLayers,
                InitialResearchPoints = config.InitialResearchPoints,
                ShapesConfigurationId = config.ShapesConfigurationId,
                ColorSchemeConfigurationId = config.ColorSchemeConfigurationId,
                ResearchLevelsAreProgressive = config.ResearchLevelsAreProgressive,
                ResearchPointsGenerationMode = config.ResearchPointsGenerationMode,
                BlueprintCurrencyShapes = config.BlueprintCurrencyShapes.Select(shape => new SerializedBlueprintCurrencyShape() { Shape = shape.Shape, Amount = shape.Amount }).ToArray(),
                IntroductionWikiEntryId = config.IntroductionWikiEntryId,
                InitiallyUnlockedUpgrades = config.InitiallyUnlockedUpgrades.ToArray(),
                TutorialConfig = config.TutorialConfig
            };
        }

        public static SerializedResearchProgression DeepCopySerializedResearchProgression(SerializedResearchProgression progression)
        {
            return new()
            {
                Levels = new() 
                { 
                    Levels = progression.Levels.Levels.Select(level => new SerializedResearchLevel() 
                    { 
                        Definition = new SerializedResearchLevelDefinition() 
                        {
                            Id = level.Definition.Id,
                            VideoId = level.Definition.VideoId,
                            PreviewImageId = level.Definition.PreviewImageId,
                            Title = level.Definition.Title,
                            Description = level.Definition.Description,
                            WikiEntryId = level.Definition.WikiEntryId,
                            SignatureShape = level.Definition.SignatureShape,
                            IconId = level.Definition.IconId
                        },
                        //Lines = 
                    }).ToArray(),
                },
                SideQuestGroups = new() 
                {

                },    
            };
        }
    }
}
