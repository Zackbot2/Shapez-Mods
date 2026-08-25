using Game.Core.Trains;
using Game.Core.Trains.Stations;
using Iced.Intel;
using ShapezShifter.Hijack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TrainsLib.GameData;
using TrainsLib.Stations;

namespace TrainsLib.Rewirers
{
    internal class TrainsSimulationSystemsRewirer : ISimulationSystemsRewirer
    {
        public void ModifySimulationSystems(ICollection<ISimulationSystem> simulationSystems, SimulationSystemsDependencies dependencies)
        {
            // find TrainSystem in simulationSystems
            // important: currently only handles one single train system.
            TrainSimulationSystemsData.TrainSystem = simulationSystems.OfType<TrainSystem>().FirstOrDefault();

            // register modded train stops
            foreach (ModdedTrainStop stop in ModdedStopRegistry.RegisteredStops)
            {
                TrainStationCoordinator coordinator = new(stop.DefinitionId, TrainSimulationSystemsData.TrainSystem.TrainsSimulation.BuiltInWagonStates.Moving, stop.Decider, stop.Decider);
                TrainSimulationSystemsData.TrainSystem.TrainsSimulation.AddCustomNavigationCoordinatorAfter<TrainStationCoordinator, TrainStationCoordinator>(coordinator);
            }
        }
    }
}
