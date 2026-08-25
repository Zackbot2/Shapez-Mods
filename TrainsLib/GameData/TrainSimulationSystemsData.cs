using Game.Core.Trains;
using System;
using System.Collections.Generic;
using System.Text;

namespace TrainsLib.GameData
{
    /// <summary>
    /// Data related to the train simulation systems.
    /// </summary>
    public static class TrainSimulationSystemsData
    {
        public static TrainSystem? TrainSystem {get; internal set; }
        public static TrainsSimulation? Simulation => TrainSystem?.TrainsSimulation;
    }
}
