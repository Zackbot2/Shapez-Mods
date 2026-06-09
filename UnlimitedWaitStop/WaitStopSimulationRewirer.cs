using Game.Core.Trains;
using Game.Core.Trains.Stations;
using ShapezShifter.Hijack;
using System;
using System.Collections.Generic;

namespace UnlimitedWaitStop
{
    public class WaitStopSimulationRewirer : ISimulationSystemsRewirer, IRewirer
    {
        private readonly WaitStopDeciderRef _deciderRef;

        public WaitStopSimulationRewirer(WaitStopDeciderRef deciderRef) 
        {
            _deciderRef = deciderRef;
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
                dependencies.Logger.Warning?.Log("UnlimitedWaitStop: TrainSystem not found — wait stop coordinator NOT registered.");
            }
            else
            {
                GameIslands islands = dependencies.Mode.Islands;
                TrainsSimulation trainsSimulation = trainSystem.TrainsSimulation;

                WaitStopDecider decider = new(trainsSimulation, trainsSimulation.TrainsWagonCargo, trainsSimulation.TrainSimulationTimeTracker, TimeSpan.FromSeconds(60));
                _deciderRef.Current = decider;
                IslandDefinition waitStopIsland = (IslandDefinition)islands.Trains.Navigation.WaitStation;

                // trainsSimulation.BuiltInWagonStates is obsolete, and the new one is private. not sure what they want us to do, so i'm just using the old one.
                TrainStationCoordinator coordinator = new(waitStopIsland.Id, trainsSimulation.BuiltInWagonStates.Moving, decider, decider);
                trainsSimulation.AddCustomNavigationCoordinatorAfter<TrainStationCoordinator, TrainStationCoordinator>(coordinator);

                simulationSystems.Add(new WaitStopIslandSystem(waitStopIsland.Id, decider));
            }
        }
    }
}
