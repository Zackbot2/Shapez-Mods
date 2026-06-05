using Game.Content.Features.Signals.Channels;
using Game.Core.Coordinates;
using Game.Core.Simulation;
using Game.Core.Trains;
using Game.Core.Trains.Stations;
using System;
using System.Collections.Generic;
using System.Text;

namespace HybridStop
{
    public class HybridStopDecider : ITrainStopDecider, ITrainLeaveDecider
    {
        private readonly TrainsSimulation Simulation;
        private readonly TrainsWagonCargo CargoSimulator;
        private readonly ISimulationTimeProvider TrainSimulationTimeProvider;

        public HybridStopDecider(TrainsSimulation simulation, TrainsWagonCargo cargoSimulator, ISimulationTimeProvider trainSimulationTimeProvider)
        {
            Simulation = simulation;
            CargoSimulator = cargoSimulator;
            TrainSimulationTimeProvider = trainSimulationTimeProvider;
        }

        public bool ShouldTrainLeave(TrainId id, TrainSimulationData trainSimulationData)
        {
            // never leave muahahahha
            return false;
        }

        public bool ShouldTrainStop(TrainId id, TrainSimulationData trainSimulationData)
        {
            // yeah, stop
            return true;
        }
    }
}
