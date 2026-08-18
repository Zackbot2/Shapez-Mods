using Game.Core.Content.Islands;
using Game.Core.Rendering.Islands;
using Game.Core.Trains;
using Game.Core.Trains.Stations;
using ShapezShifter.Hijack;
using ShapezShifter.Kit;
using ShapezShifter.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HybridStop
{
    public class HybridStopSimulationRewirer : ISimulationSystemsRewirer, IRewirer
    {
        private readonly IslandDefinitionId _hybridStopIslandDefinitionId;
        private readonly IslandDefinitionGroupId _hybridStopGroupDefinitionId;
        private readonly Sprite _hybridStopIcon;
        private readonly Mesh _hybridStopMesh;


        public HybridStopSimulationRewirer(IslandDefinitionId hybridStopIslandId, IslandDefinitionGroupId hybridStopGroupId, ModFolderLocator modFolderLocator, string iconPath, string baseMeshPath)
        {
            _hybridStopIslandDefinitionId = hybridStopIslandId;
            _hybridStopGroupDefinitionId = hybridStopGroupId;
            _hybridStopIcon = FileTextureLoader.LoadTextureAsSprite(iconPath, out _);

            // if you're following this as a sort of guide, make sure your mesh only has ONE material. this line will throw an error if you have more.
            _hybridStopMesh = FileMeshLoader.LoadSingleMeshFromFile(modFolderLocator.SubPath(baseMeshPath));
        }

        public void ModifySimulationSystems(ICollection<ISimulationSystem> simulationSystems, SimulationSystemsDependencies dependencies)
        {
            TrainSystem? trainSystem = null;

            // find the TrainSystem from the list of simulation systems that exist for this save
            foreach (ISimulationSystem simSystem in simulationSystems)
            {
                if (simSystem is TrainSystem ts)
                {
                    trainSystem = ts;
                    break;
                }
            }
            if (trainSystem == null)
            {
                throw new Exception("HybridStop: TrainSystem not found — hybrid stop coordinator NOT registered.");
            }

            // using that, grab its TrainsSimulation.
            // TrainSystem is the topmost manager of everything train related.
            // TrainsSimulation manages the simulation side of things, as the name implies.
            TrainsSimulation trainsSimulation = trainSystem.TrainsSimulation;
            HybridStopDecider decider = new(trainsSimulation, trainsSimulation.TrainsWagonCargo, trainsSimulation.TrainSimulationTimeTracker, dependencies.Logger);

            // trainsSimulation.BuiltInWagonStates is obsolete, and the new one is private. not sure what they want us to do here.
            TrainStationCoordinator coordinator = new TrainStationCoordinator(_hybridStopIslandDefinitionId, trainsSimulation.BuiltInWagonStates.Moving, decider, decider);
            // add a new coordinator for HybridStops that uses our custom decider
            trainsSimulation.AddCustomNavigationCoordinatorAfter<TrainStationCoordinator, TrainStationCoordinator>(coordinator);

            PatchVisuals(dependencies);
        }

        private void PatchVisuals(SimulationSystemsDependencies dependencies)
        {
            GameIslands islands = dependencies.Mode.Islands;

            if (!islands.TryGetDefinition(_hybridStopIslandDefinitionId, out IIslandDefinition? rawHybridStopIsland))
            {
                dependencies.Logger.Error?.Log("HybridStop: Island definition with ID '" + _hybridStopIslandDefinitionId.Name + "' not found — visual patch skipped.");
                return;
            }

            // yoink some of the data from the wait stop. we have our own model so we can handle that ourself.

            IslandDefinition hybridStopIsland = (IslandDefinition)rawHybridStopIsland;
            IslandDefinition waitStopIsland = (IslandDefinition)islands.Trains.Navigation.WaitStation;

            // grab the wait stop's IslandMeshDrawer.Data. we need this because it contains **materials**. we only want to replace the mesh, not the materials.
            if (waitStopIsland.CustomData.TryGet(out IslandMeshDrawer.Data meshData))
            {
                // build the LOD meshes. since we only have one and i'm not about to make 5 more, just use the same one for all of them.
                LOD6Mesh lodMesh = MeshLod.Create()
                    .AddLod0Mesh(_hybridStopMesh)
                    .UseLod0AsLod1()
                    .UseLod1AsLod2()
                    .UseLod2AsLod3()
                    .UseLod3AsLod4()
                    .UseLod4AsLod5()
                    .BuildLod6Mesh();

                RuntimeLODMeshMaterial hybridStopMeshMaterial = new(lodMesh, meshData.MeshMaterials[0].LODMaterial);

                hybridStopIsland.CustomData.AttachOrReplace(new IslandMeshDrawer.Data(new ILODMeshMaterial[] { hybridStopMeshMaterial }));
            }

            // Custom data types on the wait stop:
            // Core.Factory.LambdaFactory`1[[IIslandConfiguration, Game.Core.Map.Simulation, Version = 0.0.0.0, Culture = neutral, PublicKeyToken = null]]
            // IslandDefinitionGroup
            // IslandDefinitionGroupId
            // DefaultPreferredPlacementMode
            // IslandPlacementRequirementsProvider
            // IslandPlacementHelpersProvider
            // IslandConnectorData
            // TrainStationMetadata
            // EntityReplacementPreferenceData
            // ChunkCostProvider
            // IslandPresentationData
            // Game.Core.Rendering.Islands.ModularIslandMeshDrawer
            // Game.Core.Rendering.Islands.IslandMeshDrawer
            // Game.Core.Rendering.Islands.IslandOverviewDrawer
            // IslandFrameDrawData
            // IslandInteractionConfig
            // IslandCollisionData

            //HybridStopMod.Logger.Info?.Log("Custom data types on the wait stop:");
            //foreach (var customData in waitStopIsland.CustomData.CustomData)
            //{
            //    HybridStopMod.Logger.Info?.Log($"{customData.GetType().FullName}");
            //}

            if (waitStopIsland.CustomData.TryGet(out IslandOverviewDrawer.Data overviewData))
            {
                hybridStopIsland.CustomData.AttachOrReplace(overviewData);
            }
            if (waitStopIsland.CustomData.TryGet(out IslandFrameDrawData frameData))
            {
                hybridStopIsland.CustomData.AttachOrReplace(frameData);
            }
            if (waitStopIsland.CustomData.TryGet(out IRailIslandColorPredictionDrawDataProvider railPred))
            {
                hybridStopIsland.CustomData.AttachOrReplace(railPred);
            }

            if (waitStopIsland.CustomData.TryGet(out ModularIslandMeshDrawer modularIslandMeshDrawer))
            {
                hybridStopIsland.CustomData.AttachOrReplace(modularIslandMeshDrawer);
            }
            if (waitStopIsland.CustomData.TryGet(out IslandPresentationData presentationData))
            {
                //hybridStopIsland.CustomData.AttachOrReplace(presentationData);
            }
            if (waitStopIsland.CustomData.TryGet(out TrainStationMetadata stationMetadata))
            {
                hybridStopIsland.CustomData.AttachOrReplace(stationMetadata);
            }
            if (waitStopIsland.CustomData.TryGet(out IslandInteractionConfig interactionConfig))
            {
                hybridStopIsland.CustomData.AttachOrReplace(interactionConfig);
            }
            if (waitStopIsland.CustomData.TryGet(out IslandCollisionData collisionData))
            {
                hybridStopIsland.CustomData.AttachOrReplace(collisionData);
            }


            // patch the group's custom data
            // (i'm still not sure what a group is)

            IIslandDefinitionGroup waitStopGroup = islands.Groups.TrainWaitStationsGroup;

            if (waitStopGroup.CustomData.TryGet(out IPresentationData waitGroupPres))
            {
                IslandDefinitionGroup hybridStopGroup = islands.AllDefinitionGroups
                    .OfType<IslandDefinitionGroup>()
                    .FirstOrDefault(g => g.Id == _hybridStopGroupDefinitionId);

                if (hybridStopGroup != null && hybridStopGroup.CustomData.TryGet(out IPresentationData ourGroupPres))
                {
                    hybridStopGroup.CustomData.AttachOrReplace<IPresentationData>(new GroupPresentationData(
                        _hybridStopIcon,
                        ourGroupPres.Title,
                        ourGroupPres.Description,
                        shouldShowAsReward: false));
                }
            }
        }
    }
}
