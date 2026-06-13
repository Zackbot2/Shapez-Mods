using Core.Factory;
using Core.Localization;
using Game.Core.Coordinates;
using Game.Core.Rails;
using Game.Orchestration;
using MonoMod.RuntimeDetour;
using ShapezShifter.Flow;
using ShapezShifter.Flow.Atomic;
using ShapezShifter.Flow.Research;
using ShapezShifter.Flow.Toolbar;
using ShapezShifter.Hijack;
using ShapezShifter.SharpDetour;
using System;
using System.Collections.Generic;
using System.Text;
using ILogger = Core.Logging.ILogger;

namespace MoreTrainColours
{
    public class MoreTrainColoursMod : IMod
    {
        public static ILogger Logger = null!;

        // hooks
        private Hook? _gameScenarioHook;
        private Hook? _bakeMetadataIntoRuntimeHook;
        private Hook? _resolveGroupsHook;
        private Hook? _registerProducersHook;

        public MoreTrainColoursMod(ILogger logger)
        {
            Logger = logger;

            GameRewirers.AddRewirer(new MoreTrainColoursScenarioRewirer());

            _registerProducersHook = DetourHelper.CreatePrefixHook(
                (placersCreators, registry, disposables) => placersCreators.RegisterProducers(registry, disposables),
                (TrainIslandsPlacersCreators placersCreators, IPlacementInitiatorIdRegistry registry, ICollection<IDisposable> disposables) =>
                {
                    foreach (RailColor railColor in placersCreators.RailColorRegistry.AllColors)
                    {
                        Logger.Info?.Log($"Will attempt to register producers for {placersCreators.RailColorRegistry.SerialNameForColor(railColor)}");
                    }
                    return (registry, disposables);
                });

            _bakeMetadataIntoRuntimeHook = DetourHelper.CreatePostfixHook(
                (factory, islands) => factory.BakeMetadataIntoRuntime(islands),
                (IslandDefinitionFactory factory, MetaGameModeIslands islands, GameIslands __result) =>
                {
                    // the goal is to add in our groups without publicizing what we don't need to.
                    // the logic flow is: BakeMetadataIntoRuntime called -> initialize data -> pack into new GameIslands (which we have access to) -> HOOK -> return
                    // the data we need is, at this point in time, stored at __result.Groups.
                    // make sure to do this using the same control flow as the original method, as to not cause conflicts.

                    // the logic flow for creating the information is

                    //__result.Groups.
                    Logger.Info?.Log("BakeMetadataIntoRuntime fired");

                    return __result;
                });

            //_resolveGroupsHook = DetourHelper.CreatePostfixHook(
            //    (factory, metaGameModeIslands, converterGroupDefinitions) => factory.ResolveGroups(),
            //    (IslandDefinitionFactory factory, )
            //    )

            AddRailGroups();
            AddTrainProducerGroups();
        }

        public void Dispose() { }

        private void AddRailGroups()
        {
            Logger.Info?.Log("Adding rail groups...");
        }

        private void AddTrainProducerGroups()
        {
            Logger.Info?.Log("Adding train producer groups...");
            IslandDefinitionId islandId = new("BlackTrainProducerIsland");
            IslandDefinitionGroupId blackGroupId = new("BlackTrainProducerGroup");

            string titleId = "blackTrainProducerGroup.title";
            string descriptionId = "blackTrainProducerGroup.description";

            // create the layout
            ChunkLayoutLookup<ChunkVector, IslandChunkData> layout = new(new KeyValuePair<ChunkVector, IslandChunkData>[]
            {
                new(ChunkVector.Zero, new IslandChunkData(ChunkVector.Zero, Array.Empty<ChunkDirection>()))
            });

            // create connectors
            // these are east and west because so are the quick and wait stops
            LocalChunkPivot inputPivot = new(ChunkVector.Zero, ChunkDirection.West);

            List<EntityIO<LocalChunkPivot, IIslandConnector>> connectors = new()
            {
                new EntityIO<LocalChunkPivot, IIslandConnector>(inputPivot, new RailIslandInputConnector())
            };

            IslandConnectorData connectorData = new(connectors, new ChunkVector[] { ChunkVector.Zero });

            // using ShapezShifter, we can now add the island in the standard way
            IIslandGroupBuilder groupBuilder = IslandGroup.Create(blackGroupId)
           .WithPresentation(titleId.T(), descriptionId.T(), null)
           .AsTransportableIsland()
           .WithPreferredPlacement(DefaultPreferredPlacementMode.Single);

            Logger.Info?.Log("Added train producer group(s).");
        
            AtomicIslandExtender
        }
    }
}
