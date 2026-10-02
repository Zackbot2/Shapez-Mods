using Core.Collections;
using Core.Localization;
using Cysharp.Threading.Tasks;
using Game.Content.Trains;
using Game.Content.Trains.Predictions;
using Game.Core.Content.Islands;
using Game.Core.Coordinates;
using Game.Core.Rails;
using Game.Core.Research;
using Game.Core.Trains;
using Game.Core.Trains.Stations;
using Game.Orchestration;
using MonoMod.RuntimeDetour;
using ShapezShifter.Flow;
using ShapezShifter.Flow.Atomic;
using ShapezShifter.Flow.Research;
using ShapezShifter.Flow.Toolbar;
using ShapezShifter.Hijack;
using ShapezShifter.Kit;
using ShapezShifter.SharpDetour;
using ShapezShifter.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using TrainsLib.GameData;
using TrainsLib.Stations;
using UnityEngine;
using UnityEngine.Diagnostics;
using ILogger = Core.Logging.ILogger;

namespace HybridStop
{
    public class HybridStopMod : IMod
    {
        internal static ILogger Logger = null!;
        public static string ModName => nameof(HybridStop);

        private readonly ModFolderLocator _modResourcesLocator = ModDirectoryLocator.CreateLocator<HybridStopMod>().SubLocator("Resources");
        private GameImageId _hybridStopResearchImageId;

        // hooks and rewirers
        private RewirerHandle _hybridStopSimulationRewirer;
        private Hook? GameInitHook;

        // readonly values, to minimize magic numbers/strings
        private readonly IslandDefinitionId hybridStopIslandId = new("HybridStop");
        private readonly IslandDefinitionGroupId hybridStopGroupId = new("HybridStop");

        // constants
        private const string STOP_TITLE_ID = "HybridStopIsland.title";
        private const string STOP_DESCRIPTION_ID = "HybridStopIsland.description";
        private const string STOP_RESEARCH_ID = "HybridStop";
        private const string STOP_RESEARCH_TITLE_ID = "research.HybridStop.title";
        private const string STOP_RESEARCH_DESCRIPTION_ID = "research.HybridStop.description";
        private const string STOP_RESEARCH_SHOP_IMAGE_ID = "HybridStopShopImage";

        public HybridStopMod(ILogger logger)
        {
            Logger = logger;
            Logger.Info?.Log($"{ModName}: Initializing mod...");

            _hybridStopResearchImageId = new GameImageId(STOP_RESEARCH_SHOP_IMAGE_ID);
            Sprite hybridStopShopImage = FileTextureLoader.LoadTextureAsSprite(_modResourcesLocator.SubPath("HybridStopShopImage.png"), out _);

            AddHybridStop();

            // add the shop image to the game's data
            GameInitHook = DetourHelper.CreatePostfixHook<GameOrchestrator, UniTask>(
                original: orchestrator => orchestrator.InitializeMainMenu(),
                postfix: (self, result) =>
                {
                    GameData? gameData = self.InitializationDependencyContainer.Resolve<IGameData>() as GameData;
                    if (!gameData?._Images.ContainsKey(_hybridStopResearchImageId) == true)
                    {
                        Logger.Info?.Log($"{ModName}: Injecting HybridStop shop upgrade image...");
                        gameData?._Images.Add(_hybridStopResearchImageId, hybridStopShopImage);
                    }
                    return result;
                });

            Logger.Info?.Log($"{ModName}: Mod successfully initialized!");
        }

        /// <summary>
        /// Adds the hybrid stop island to the game.
        /// </summary>
        private void AddHybridStop()
        {
            
            string iconPath = _modResourcesLocator.SubPath("HybridStopIcon.png");
            string meshPath = _modResourcesLocator.SubPath("HybridStop.fbx");

            // add the rewirer - this patches the simulation and the visuals when a hybrid stop is placed.
            _hybridStopSimulationRewirer = GameRewirers.AddRewirer(new HybridStopSimulationRewirer(hybridStopIslandId, hybridStopGroupId, _modResourcesLocator, iconPath, meshPath));

            // create the layout
            ChunkLayoutLookup<ChunkVector, IslandChunkData> layout = new(new KeyValuePair<ChunkVector, IslandChunkData>[]
            {
                new(ChunkVector.Zero, new IslandChunkData(ChunkVector.Zero, Array.Empty<ChunkDirection>()))
            });

            // create connectors
            // these are east and west because so are the quick and wait stops
            LocalChunkPivot inputPivot = new(ChunkVector.Zero, ChunkDirection.West);
            LocalChunkPivot outputPivot = new(ChunkVector.Zero, ChunkDirection.East);

            List<EntityIO<LocalChunkPivot, IIslandConnector>> connectors = new()
            {
                new EntityIO<LocalChunkPivot, IIslandConnector>(inputPivot, new RailIslandInputConnector()),
                new EntityIO<LocalChunkPivot, IIslandConnector>(outputPivot, new RailIslandOutputConnector())
            };

            IslandConnectorData connectorData = new(connectors, new ChunkVector[] {ChunkVector.Zero});

            // using ShapezShifter, we can now add the island using Flow's standard pipeline

            IPresentableUnlockableSideUpgradeBuilder hybridStopSideUpgrade = SideUpgrade.New()
                .WithPresentationData(CreateHybridStopPresentationData())
                .WithCost(new ResearchCostPoints(new ResearchPointCurrency(60)).AsEnumerable())
                .WithCustomRequirements(new ResearchMechanicId("RUTrains").AsEnumerable(), Array.Empty<ResearchUpgradeId>());

            IIslandGroupBuilder groupBuilder = IslandGroup.Create(hybridStopGroupId)
               .WithPresentation(STOP_TITLE_ID.T(), STOP_DESCRIPTION_ID.T(), null)
               .AsTransportableIsland()
               .WithPreferredPlacement(DefaultPreferredPlacementMode.Single);

            IIslandBuilder islandBuilder = Island.Create(hybridStopIslandId)
               .WithLayout(layout)
               .WithPerChunkColliders()
               .WithConnectorData(connectorData)
               .WithInteraction(
                   flippable: true,
                   canHoldBuildings: false,
                   allowNonForcingReplacement: false,
                   skipReplacementConnectorChecks: false,
                   isTransportBuilding: false,
                   selectable: true,
                   buildable: true,
                   removable: true)
               .WithCustomChunkCost(ChunkLimitCurrency.Zero)    // FREE!!!!
               .WithRenderingOptions(new HomogeneousChunkDrawing(ChunkPlatformDrawingContext.DrawAll()), drawPlayingField: false);

            AtomicIslands.Extend()
                .AllScenarios()
                .WithIsland(islandBuilder, groupBuilder)
                .UnlockedWithNewSideUpgrade(hybridStopSideUpgrade)
                .WithDefaultPlacement()
                .InToolbar(ToolbarElementLocator.Root().ChildAt(5).ChildAt(5).ChildAt(1).InsertAfter())
                .WithoutSimulation()
                .WithoutModules()
                .Build();

            ModdedStopRegistry.RegisterTrainStop(new ModdedTrainStop(hybridStopIslandId, new HybridStopDecider()));
        }

        public void Dispose()
        {
            GameInitHook?.Dispose();

            if (_hybridStopSimulationRewirer != null)
            {
                GameRewirers.RemoveRewirer(_hybridStopSimulationRewirer);
            }
        }

        private SideUpgradePresentationData CreateHybridStopPresentationData()
        {   
            return new SideUpgradePresentationData(
                id: new ResearchUpgradeId(STOP_RESEARCH_ID),
                previewImageId: _hybridStopResearchImageId,
                videoId: GameVideoId.Empty,
                title: STOP_RESEARCH_TITLE_ID.T(),
                description: STOP_RESEARCH_DESCRIPTION_ID.T(),
                hidden: false,
                category: "Trains");
        }
    }
}
