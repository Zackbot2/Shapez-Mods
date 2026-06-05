using Core.Collections.Scoped;
using Core.Logging;
using Game.Core.Map.Simulation;
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

        public HybridStopSimulationRewirer(IslandDefinitionId islandId, IslandDefinitionGroupId groupId, HybridStopDeciderRef deciderRef, string iconPath)
        {
            _islandDefinitionId = islandId;
            _groupDefinitionId = groupId;
            _deciderRef = deciderRef;
            _icon = FileTextureLoader.LoadTextureAsSprite(iconPath, out _);
        }

        public void ModifySimulationSystems(ICollection<ISimulationSystem> simulationSystems, SimulationSystemsDependencies dependencies)
        {
            TrainSystem trainSystem = null;
            foreach (ISimulationSystem s in simulationSystems)
            {
                if (s is TrainSystem ts)
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
                HybridStopDecider decider = new(trainsSimulation, trainsSimulation.TrainsWagonCargo, trainsSimulation.TrainSimulationTimeTracker);
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

            // yoink the visuals from the wait stop

            IslandDefinition hybridStopIsland = (IslandDefinition)rawHybridStopIsland;
            IslandDefinition waitStopIsland = (IslandDefinition)islands.Trains.Navigation.WaitStation;

            if (waitStopIsland.CustomData.TryGet(out IslandMeshDrawer.Data meshData))
            {
                hybridStopIsland.CustomData.AttachOrReplace(meshData);
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
            if (waitStopIsland.CustomData.TryGet(out ModularIslandMeshDrawer.Data modData))
            {
                hybridStopIsland.CustomData.AttachOrReplace(modData);
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
