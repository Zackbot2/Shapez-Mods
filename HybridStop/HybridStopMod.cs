using TrainsLib;
using Core.Localization;
using Game.Content.Trains;
using Game.Content.Trains.Predictions;
using Game.Core.Content.Islands;
using Game.Core.Coordinates;
using Game.Core.Rails;
using Game.Core.Trains;
using Game.Core.Trains.Stations;
using MonoMod.RuntimeDetour;
using ShapezShifter.Flow;
using ShapezShifter.Flow.Atomic;
using ShapezShifter.Flow.Research;
using ShapezShifter.Flow.Toolbar;
using ShapezShifter.Hijack;
using ShapezShifter.Kit;
using ShapezShifter.SharpDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Diagnostics;
using ILogger = Core.Logging.ILogger;
using TrainsLib.Stations;
using TrainsLib.GameData;

namespace HybridStop
{
    public class HybridStopMod : IMod
    {
        internal static ILogger Logger = null!;
        public static string ModName => nameof(HybridStop);

        // hooks and rewirers
        private RewirerHandle _hybridStopSimulationRewirer;

        // readonly values, to minimize magic numbers/strings
        private readonly IslandDefinitionId hybridStopIslandId = new("HybridStop");
        private readonly IslandDefinitionGroupId hybridStopGroupId = new("HybridStop");
        private const string STOP_TITLE_ID = "HybridStopIsland.title";
        private const string STOP_DESCRIPTION_ID = "HybridStopIsland.description";

        public HybridStopMod(ILogger logger)
        {
            Logger = logger;
            Logger.Info?.Log($"{ModName}: Initializing mod...");

            AddHybridStop();

            Logger.Info?.Log($"{ModName}: Mod successfully initialized!");
        }

        /// <summary>
        /// Adds the hybrid stop island to the game.
        /// </summary>
        private void AddHybridStop()
        {
            ModFolderLocator modResourcesLocator = ModDirectoryLocator.CreateLocator<HybridStopMod>().SubLocator("Resources");
            string iconPath = modResourcesLocator.SubPath("HybridStopIcon.png");
            string meshPath = modResourcesLocator.SubPath("HybridStop.fbx");

            // add the rewirer - this patches the simulation and the visuals when a hybrid stop is placed.
            _hybridStopSimulationRewirer = GameRewirers.AddRewirer(new HybridStopSimulationRewirer(hybridStopIslandId, hybridStopGroupId, modResourcesLocator, iconPath, meshPath));

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
               .UnlockedAtMilestone(new ByIdMilestoneSelector(new Game.Core.Research.ResearchUpgradeId("Milestone_ShapeTrains")))
               .WithDefaultPlacement()
               .InToolbar(ToolbarElementLocator.Root().ChildAt(5).ChildAt(5).ChildAt(1).InsertAfter())
               .WithoutSimulation()
               .WithoutModules()
               .Build();

            ModdedStopRegistry.RegisterTrainStop(new ModdedTrainStop(hybridStopIslandId, new HybridStopDecider()));
        }

        public void Dispose()
        {
            if (_hybridStopSimulationRewirer != null)
            {
                GameRewirers.RemoveRewirer(_hybridStopSimulationRewirer);
            }
        }
    }
}
