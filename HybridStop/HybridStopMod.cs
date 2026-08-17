using Core.Localization;
using Game.Content.Trains;
using Game.Content.Trains.Predictions;
using Game.Core.Content.Islands;
using Game.Core.Coordinates;
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
using ILogger = Core.Logging.ILogger;

namespace HybridStop
{
    public class HybridStopMod : IMod
    {
        internal static ILogger Logger = null!;

        // hooks and rewirers
        private RewirerHandle _hybridStopRewirer;
        private Hook? _createTrainStationSystemsHook;

        // readonly values, to minimize magic numbers/strings
        private readonly IslandDefinitionId hybridStopIslandId = new("HybridStop");
        private readonly IslandDefinitionGroupId hybridStopGroupId = new("HybridStop");
        private const string STOP_TITLE_ID = "HybridStopIsland.title";
        private const string STOP_DESCRIPTION_ID = "HybridStopIsland.description";

        public HybridStopMod(ILogger logger)
        {
            Logger = logger;

            _createTrainStationSystemsHook = DetourHelper.CreatePostfixHook<BuiltinPredictionSimulationSystems, TrainWagonCargoTypeId, TrainWagonCargoTypeId, IEnumerable<ISimulationSystem>>
                ((builtinPredictionSimulationSystems, shapeCargoType, fluidCargoType) => builtinPredictionSimulationSystems.CreateTrainStationSystems(shapeCargoType, fluidCargoType), WrapCreateTrainStationSystems);

            AddHybridStop();

            Logger.Info?.Log("HybridStop loaded successfully!");
        }

        private IEnumerable<ISimulationSystem> WrapCreateTrainStationSystems(BuiltinPredictionSimulationSystems builtinPredictionSimulationSystems, TrainWagonCargoTypeId shapeCargoType, TrainWagonCargoTypeId fluidCargoType, IEnumerable<ISimulationSystem> original)
        {
            foreach(ISimulationSystem simulationSystem in original)
            {
                if (simulationSystem is TrainStationPredictionSimulationSystem)
                {
                    // the game hardcodes this, so we have to roll with it. i'm tempted to make a library for this since i hate it so much.
                    List<IslandDefinitionId> trainStops = new()
                    {
                        builtinPredictionSimulationSystems.Mode.Islands.Trains.Navigation.QuickStation.Id,
                        builtinPredictionSimulationSystems.Mode.Islands.Trains.Navigation.WaitStation.Id,
                        hybridStopIslandId
                    };

                    ITrainSubStationSimulationSystem[] predictionSubSystems = builtinPredictionSimulationSystems.TrainStationPredictionSubSystems(shapeCargoType, fluidCargoType).ToArray();

                    yield return new TrainStationPredictionSimulationSystem(trainStops, predictionSubSystems, builtinPredictionSimulationSystems.Logger);
                }
                else
                {
                    yield return simulationSystem;
                }
            }
            yield break;
        }

        /// <summary>
        /// Add the hybrid stop island to the game.
        /// Rewires the simulation and uses ShapezShifter.Flow.
        /// </summary>
        private void AddHybridStop()
        {
            ModFolderLocator modResourcesLocator = ModDirectoryLocator.CreateLocator<HybridStopMod>().SubLocator("Resources");
            string iconPath = modResourcesLocator.SubPath("HybridStopIcon.png");
            string meshPath = modResourcesLocator.SubPath("HybridStop.fbx");

            // add the rewirer - this patches the simulation and the visuals when a hybrid stop is placed.
            _hybridStopRewirer = GameRewirers.AddRewirer(new HybridStopSimulationRewirer(hybridStopIslandId, hybridStopGroupId, modResourcesLocator, iconPath, meshPath));

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
        }

        public void Dispose()
        {
            if (_hybridStopRewirer != null)
            {
                GameRewirers.RemoveRewirer(_hybridStopRewirer);
            }

            _createTrainStationSystemsHook?.Dispose();
        }
    }
}
