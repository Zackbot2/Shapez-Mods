using Core.Collections;
using Core.Localization;
using Game.Content.Features.SpacePaths.IslandIO;
using Game.Core.Coordinates;
using HybridStopFactoryBuil;
using ShapezShifter.Flow;
using ShapezShifter.Flow.Atomic;
using ShapezShifter.Flow.Research;
using ShapezShifter.Flow.Toolbar;
using ShapezShifter.Kit;
using ShapezShifter.Textures;
using System;
using System.Collections.Generic;
using ILogger = Core.Logging.ILogger;

public class HybridStopMod : IMod
{
    public HybridStopMod(ILogger logger)
    {
        AddHybridStop();

        logger.Info?.Log("Mod loaded successfully!");
    }

    public void Dispose()
    {
    }

    private void AddHybridStop()
    {
        IslandDefinitionGroupId groupId = new("HybridStop");
        IslandDefinitionId definitionId = new("HybridStop");

        string titleId = "HybridStop.title";
        string descriptionId = "HybridStop.description";

        ModFolderLocator modResourcesLocator =
            ModDirectoryLocator.CreateLocator<HybridStopMod>().SubLocator("Resources");

        string iconPath = modResourcesLocator.SubPath("HybridStopIcon.png");

        IIslandGroupBuilder islandGroupBuilder = IslandGroup.Create(groupId)
           .WithTitle(titleId.T())
           .WithDescription(descriptionId.T())
           .WithIcon(FileTextureLoader.LoadTextureAsSprite(iconPath, out _))
           .AsNonTransportableIsland()
           .WithPreferredPlacement(DefaultPreferredPlacementMode.Area);

        var layout = FoundationLayout();


        IIslandBuilder islandBuilder = Island.Create(definitionId)
           .WithLayout(layout)
           .WithBoundingCollider()
           .WithConnectorData(FoundationConnectors(layout))
           .WithInteraction(flippable: false, canHoldBuildings: false)
           .WithDefaultChunkCost()
           .WithRenderingOptions(ChunkDrawingOptions(), drawPlayingField: true);

        AtomicIslands.Extend()
           .AllScenarios()
           .WithIsland(islandBuilder, islandGroupBuilder)
           .UnlockedAtMilestone(new ByIndexMilestoneSelector(^1))
           .WithDefaultPlacement()
           .InToolbar(ToolbarElementLocator.Root().ChildAt(5).ChildAt(4).ChildAt(^1).InsertAfter())
           .WithSimulation(new FluidTrashFactoryBuilder())
           .WithoutModules()
           .Build();
    }

    private IChunkDrawingContextProvider ChunkDrawingOptions()
    {
        return new HomogeneousChunkDrawing(ChunkPlatformDrawingContext.DrawAll());
    }

    private ChunkLayoutLookup<ChunkVector, IslandChunkData> FoundationLayout()
    {
        return new ChunkLayoutLookup<ChunkVector, IslandChunkData>(ChunkData());
    }

    private IEnumerable<KeyValuePair<ChunkVector, IslandChunkData>> ChunkData()
    {
        var origin = new ChunkVector(0, 0, 0);

        IslandChunkData islandChunkData = IslandLayoutFactory.CreateIslandChunkData(
            chunkTile: origin,
            notchDirections: Array.Empty<ChunkDirection>(),
            neighborChunks: origin.AsEnumerable(),
            isBuildable: true,
            flipped: false,
            out _);

        for (int i = 0; i < islandChunkData.TileVoidFlags_L.Length; i++)
        {
            islandChunkData.TileVoidFlags_L[i] = true;
        }

        yield return new KeyValuePair<ChunkVector, IslandChunkData>(origin, islandChunkData);
    }

    private IIslandConnectorData FoundationConnectors(ChunkLayoutLookup<ChunkVector, IslandChunkData> chunkLayout)
    {
        return new IslandConnectorData(
            new[]
            {
                TrainInputConnector(ChunkDirection.South),
                TrainOutputConnector(ChunkDirection.North)
            },
            chunkLayout.GetChunkPositions());

        EntityIO<LocalChunkPivot, IIslandConnector> TrainInputConnector(ChunkDirection dir)
        {
            var chunkPivot = new LocalChunkPivot(ChunkVector.Zero, dir);
            return new EntityIO<LocalChunkPivot, IIslandConnector>(chunkPivot, new RailIslandInputConnector());
        }
        EntityIO<LocalChunkPivot, IIslandConnector> TrainOutputConnector(ChunkDirection dir)
        {
            var chunkPivot = new LocalChunkPivot(ChunkVector.Zero, dir);
            return new EntityIO<LocalChunkPivot, IIslandConnector>(chunkPivot, new RailIslandOutputConnector());
        }
    }
}
