using Core.Factory;
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
        public HybridStopMod(ILogger logger)
        {
            AddHybridStop(logger);

            logger.Info?.Log("Mod loaded successfully!");
        }

        public void Dispose() 
        {
            // remove hybrid stop
        }

        private void AddHybridStop(ILogger logger)
        {
            IslandDefinitionId islandId = new("HybridStop");
            IslandDefinitionGroupId groupId = new("HybridStop");
            HybridStopDeciderRef deciderRef = new();

            // add the rewirer - this patches the simulation and the visuals when a hybrid stop is placed.
            GameRewirers.AddRewirer(new HybridStopSimulationRewirer(islandId, groupId, deciderRef));

            string titleId = "HybridStop.title";
            string descriptionId = "HybridStop.description";

            ModFolderLocator modResourcesLocator =
                ModDirectoryLocator.CreateLocator<HybridStopMod>().SubLocator("Resources");

            string iconPath = modResourcesLocator.SubPath("HybridStopIcon.png");

            // create the layout
            ChunkLayoutLookup<ChunkVector, IslandChunkData> layout = new(new KeyValuePair<ChunkVector, IslandChunkData>[]
            {
                new(ChunkVector.Zero, new IslandChunkData(ChunkVector.Zero, Array.Empty<ChunkDirection>()))
            });

            // create connectors
            LocalChunkPivot inputPivot = new(ChunkVector.Zero, ChunkDirection.West);
            LocalChunkPivot outputPivot = new(ChunkVector.Zero, ChunkDirection.East);

            List<EntityIO<LocalChunkPivot, IIslandConnector>> connectors = new()
            {
                new EntityIO<LocalChunkPivot, IIslandConnector>(inputPivot, new RailIslandInputConnector()),
                new EntityIO<LocalChunkPivot, IIslandConnector>(outputPivot, new RailIslandOutputConnector())
            };

            IslandConnectorData connectorData = new(connectors, new ChunkVector[] {ChunkVector.Zero});

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
               .WithDefaultChunkCost()
               .WithRenderingOptions(new HomogeneousChunkDrawing(ChunkPlatformDrawingContext.DrawAll()), drawPlayingField: false);

            ((IslandBuilder)islandBuilder).IslandDefinition.CustomData.AttachOrReplace<IFactory<IIslandConfiguration>>(new LambdaFactory<IIslandConfiguration>(() => new HybridStopIslandConfiguration()));

            HybridStopModuleProvider provider = new(deciderRef);

            AtomicIslands.Extend()
               .AllScenarios()
               .WithIsland(islandBuilder, groupBuilder)
               .UnlockedAtMilestone(new ByIdMilestoneSelector(new Game.Core.Research.ResearchUpgradeId("Milestone_ShapeTrains")))
               .WithDefaultPlacement()
               .InToolbar(ToolbarElementLocator.Root().ChildAt(5).ChildAt(5).ChildAt(^1).InsertAfter())
               .WithoutSimulation()
               .WithCustomModules(provider)
               .Build();
        }
    }
}
