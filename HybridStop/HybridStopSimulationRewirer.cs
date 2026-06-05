using Core.Logging;
using Game.Core.Rendering.Islands;
using Game.Core.Trains;
using Game.Core.Trains.Stations;
using ShapezShifter.Hijack;
using System;
using System.Collections.Generic;
using Game.Core.Map.Simulation;
using Core.Collections.Scoped;

namespace HybridStop
{
    public class HybridStopSimulationRewirer : ISimulationSystemsRewirer, IRewirer, IEquatable<IRewirer>
    {
        private readonly IslandDefinitionId _islandDefinitionId;
        private readonly IslandDefinitionGroupId _groupDefinitionId;
        private readonly HybridStopDeciderRef _deciderRef;

        public HybridStopSimulationRewirer(IslandDefinitionId islandId, IslandDefinitionGroupId groupId, HybridStopDeciderRef deciderRef)
        {
            _islandDefinitionId = islandId;
            _groupDefinitionId = groupId;
            _deciderRef = deciderRef;
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
                ILogChannel warning = dependencies.Logger.Warning;
                warning?.Log("HybridStop: TrainSystem not found — hybrid stop coordinator NOT registered.");
            }
            else
            {
                TrainsSimulation trainsSimulation = trainSystem.TrainsSimulation;
                HybridStopDecider decider = new(trainsSimulation, trainsSimulation.TrainsWagonCargo, trainsSimulation.TrainSimulationTimeTracker);
                _deciderRef.Current = decider;
                TrainStationCoordinator coordinator = new(_islandDefinitionId, trainsSimulation.BuiltInWagonStates.Moving, decider, decider);
                trainsSimulation.AddCustomNavigationCoordinatorAfter<TrainStationCoordinator, TrainStationCoordinator>(coordinator);
                dependencies.Logger.Info?.Log("HybridStop: registered TrainStationCoordinator for island ID '" + _islandDefinitionId.Name + "'.");

                simulationSystems.Add(new HybridStopIslandSystem(this._islandDefinitionId, decider));
                PatchVisuals(dependencies);
            }
        }

        private void PatchVisuals(SimulationSystemsDependencies dependencies)
        {
            dependencies.Logger.Info?.Log("HybridStop: Patching visuals for island ID '" + _islandDefinitionId.Name + "'...");
            GameIslands islands = dependencies.Mode.Islands;

            if (!islands.TryGetDefinition(_islandDefinitionId, out IIslandDefinition rawOurIsland))
            {
                dependencies.Logger.Error?.Log("HybridStop: Island definition with ID '" + _islandDefinitionId.Name + "' not found — visual patch skipped.");
                return;
            }
            IslandDefinition ourIsland = (IslandDefinition)rawOurIsland;
            IslandDefinition waitIsland = (IslandDefinition)islands.Trains.Navigation.WaitStation;

            // yoink the visuals from the wait stop

            if (waitIsland.CustomData.TryGet(out IslandMeshDrawer.Data meshData))
            {
                ourIsland.CustomData.AttachOrReplace(meshData);
            }

            if (waitIsland.CustomData.TryGet(out IslandOverviewDrawer.Data overviewData))
            {
                ourIsland.CustomData.AttachOrReplace(overviewData);
            }

            if (waitIsland.CustomData.TryGet(out IslandFrameDrawData frameData))
            {
                ourIsland.CustomData.AttachOrReplace(frameData);
            }

            if (waitIsland.CustomData.TryGet(out IRailIslandColorPredictionDrawDataProvider railPred))
            {
                ourIsland.CustomData.AttachOrReplace(railPred);
            }

            if (waitIsland.CustomData.TryGet(out ModularIslandMeshDrawer.Data modData))
            {
                ourIsland.CustomData.AttachOrReplace(modData);
            }

            IIslandDefinitionGroup waitStationsGroup = islands.Groups.TrainWaitStationsGroup;
                
            if (waitStationsGroup.CustomData.TryGet(out IPresentationData waitGroupPres))
            {
                IslandDefinition ourGroup = (IslandDefinition)islands.AllDefinitionGroups.FirstOrDefault(g => g.Id == _groupDefinitionId);
                if (ourGroup != null && ourGroup.CustomData.TryGet(out IPresentationData ourGroupPres))
                {
                    ourGroup.CustomData.AttachOrReplace<IPresentationData>(new GroupPresentationData(
                        waitGroupPres.Icon, 
                        ourGroupPres.Title, 
                        ourGroupPres.Description,
                        shouldShowAsReward: false));
                }
            }

            dependencies.Logger.Info?.Log("HybridStop visual patch complete.");
        }
    }
}
