using Game.Core.Trains;
using System;
using System.Collections.Generic;
using System.Text;

namespace TrainsLib.GameData
{
    public static class SimulationSystemsData
    {
        public static TrainSystem? TrainSystem {get; internal set; }
        public static TrainsSimulation? Simulation => TrainSystem?.TrainsSimulation;
    }
}
