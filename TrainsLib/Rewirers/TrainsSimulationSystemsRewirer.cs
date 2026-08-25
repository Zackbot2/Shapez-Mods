using Game.Core.Trains;
using ShapezShifter.Hijack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TrainsLib.GameData;

namespace TrainsLib.Rewirers
{
    internal class TrainsSimulationSystemsRewirer : ISimulationSystemsRewirer
    {
        public void ModifySimulationSystems(ICollection<ISimulationSystem> simulationSystems, SimulationSystemsDependencies dependencies)
        {
            // find TrainSystem in simulationSystems
            // important: currently only handles one single train system.
            GameTrainSimulationSystemsData.TrainSystem = simulationSystems.OfType<TrainSystem>().FirstOrDefault();
        }
    }
}
