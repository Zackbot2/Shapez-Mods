using Core.Localization;
using Game.Core.Coordinates;
using ShapezShifter.Flow;
using ShapezShifter.Flow.Atomic;
using ShapezShifter.Flow.Research;
using ShapezShifter.Flow.Toolbar;
using ShapezShifter.Hijack;
using ShapezShifter.Kit;
using System;
using System.Collections.Generic;
using ILogger = Core.Logging.ILogger;

namespace HybridStop
{
    public class HybridStopMod : IMod
    {
        private readonly ILogger _logger;

        private RewirerHandle _hybridStopRewirer;

        public HybridStopMod(ILogger logger)
        {
            _logger = logger;
            AddHybridStop();

            logger.Info?.Log("HybridStop loaded successfully!");
        }

        public void Dispose() 
        {
            if (_hybridStopRewirer != null)
            {
                GameRewirers.RemoveRewirer(_hybridStopRewirer);
            }
        }

        /// <summary>
        /// Add the hybrid stop island to the game.
        /// Rewires the simulation and uses ShapezShifter.Flow.
        /// </summary>
        private void AddHybridStop()
        {
            IslandDefinitionId islandId = new("HybridStop");
            IslandDefinitionGroupId groupId = new("HybridStop");

            ModFolderLocator modResourcesLocator = ModDirectoryLocator.CreateLocator<HybridStopMod>().SubLocator("Resources");
            string iconPath = modResourcesLocator.SubPath("HybridStopIcon.png");
            string meshPath = modResourcesLocator.SubPath("HybridStop.fbx");

            // add the rewirer - this patches the simulation and the visuals when a hybrid stop is placed.
            _hybridStopRewirer = GameRewirers.AddRewirer(new HybridStopSimulationRewirer(islandId, groupId, modResourcesLocator, iconPath, meshPath));

            string titleId = "HybridStopIsland.title";
            string descriptionId = "HybridStopIsland.description";

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

            // using ShapezShifter, we can now add the island in the standard way
            IIslandGroupBuilder groupBuilder = IslandGroup.Create(groupId)
               .WithPresentation(titleId.T(), descriptionId.T(), null)
               .AsTransportableIsland()
               .WithPreferredPlacement(DefaultPreferredPlacementMode.Single);

            IIslandBuilder islandBuilder = Island.Create(islandId)
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
        }
    }
}
