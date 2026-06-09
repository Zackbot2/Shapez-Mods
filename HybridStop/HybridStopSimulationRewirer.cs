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
    public class HybridStopSimulationRewirer : ISimulationSystemsRewirer, IRewirer, IEquatable<IRewirer>
    {
        private readonly IslandDefinitionId _islandDefinitionId;
        private readonly IslandDefinitionGroupId _groupDefinitionId;
        private readonly HybridStopDeciderRef _deciderRef;
        private readonly Sprite _icon;
        private readonly Mesh _mesh;


        public HybridStopSimulationRewirer(IslandDefinitionId islandId, IslandDefinitionGroupId groupId, HybridStopDeciderRef deciderRef, ModFolderLocator modFolderLocator, string iconPath, string baseMeshPath)
        {
            _islandDefinitionId = islandId;
            _groupDefinitionId = groupId;
            _deciderRef = deciderRef;
            _icon = FileTextureLoader.LoadTextureAsSprite(iconPath, out _);

            // if you're following this as a sort of guide, make sure your mesh only has ONE material. this line will throw an error if you have more.
            _mesh = FileMeshLoader.LoadSingleMeshFromFile(modFolderLocator.SubPath(baseMeshPath));

            //Debug.Log($"Verts: {_mesh.vertexCount}");
            //Debug.Log($"Submeshes: {_mesh.subMeshCount}");
            //Debug.Log($"Triangles: {_mesh.triangles.Length / 3}");
            //Debug.Log($"Bounds: {_mesh.bounds}");
        }

        public void ModifySimulationSystems(ICollection<ISimulationSystem> simulationSystems, SimulationSystemsDependencies dependencies)
        {
            TrainSystem trainSystem = null;
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
                dependencies.Logger.Warning?.Log("HybridStop: TrainSystem not found — hybrid stop coordinator NOT registered.");
            }
            else
            {
                TrainsSimulation trainsSimulation = trainSystem.TrainsSimulation;
                HybridStopDecider decider = new(trainsSimulation, trainsSimulation.TrainsWagonCargo, trainsSimulation.TrainSimulationTimeTracker, dependencies.Logger);
                _deciderRef.Current = decider;
                // trainsSimulation.BuiltInWagonStates is obsolete, and the new one is private. not sure what they want us to do, so i'm just using the old one.
                TrainStationCoordinator coordinator = new(_islandDefinitionId, trainsSimulation.BuiltInWagonStates.Moving, decider, decider);
                trainsSimulation.AddCustomNavigationCoordinatorAfter<TrainStationCoordinator, TrainStationCoordinator>(coordinator);

                simulationSystems.Add(new HybridStopIslandSystem(this._islandDefinitionId, decider));
                PatchVisuals(dependencies);
            }
        }

        private void PatchVisuals(SimulationSystemsDependencies dependencies)
        {
            GameIslands islands = dependencies.Mode.Islands;

            if (!islands.TryGetDefinition(_islandDefinitionId, out IIslandDefinition rawHybridStopIsland))
            {
                dependencies.Logger.Error?.Log("HybridStop: Island definition with ID '" + _islandDefinitionId.Name + "' not found — visual patch skipped.");
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
                    .AddLod0Mesh(_mesh)
                    .UseLod0AsLod1()
                    .UseLod1AsLod2()
                    .UseLod2AsLod3()
                    .UseLod3AsLod4()
                    .UseLod4AsLod5()
                    .BuildLod6Mesh();

                RuntimeLODMeshMaterial hybridStopMeshMaterial = new(lodMesh, meshData.MeshMaterials[0].LODMaterial);

                hybridStopIsland.CustomData.AttachOrReplace(new IslandMeshDrawer.Data(new ILODMeshMaterial[] { hybridStopMeshMaterial })
                );
            }

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

            // patch the group's custom data
            // (i'm still not sure what a group is)

            IIslandDefinitionGroup waitStopGroup = islands.Groups.TrainWaitStationsGroup;

            if (waitStopGroup.CustomData.TryGet(out IPresentationData waitGroupPres))
            {
                IslandDefinitionGroup hybridStopGroup = islands.AllDefinitionGroups
                    .OfType<IslandDefinitionGroup>()
                    .FirstOrDefault(g => g.Id == _groupDefinitionId);

                if (hybridStopGroup != null && hybridStopGroup.CustomData.TryGet(out IPresentationData ourGroupPres))
                {
                    hybridStopGroup.CustomData.AttachOrReplace<IPresentationData>(new GroupPresentationData(
                        _icon,
                        ourGroupPres.Title,
                        ourGroupPres.Description,
                        shouldShowAsReward: false));
                }
            }
        }
    }
}
