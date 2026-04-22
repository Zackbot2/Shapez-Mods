using Core.Factory;
using HybridStop;
using ShapezShifter.Flow.Atomic;
using ShapezShifter.Hijack;
using System;
using System.Collections.Generic;
using System.Text;

namespace HybridStopFactoryBuil
{
    internal class FluidTrashFactoryBuilder : IBuildingSimulationFactoryBuilder<HybridStopSimulation>
    {
        public IFactory<HybridStopSimulation> BuildFactory(SimulationSystemsDependencies dependencies)
        {
            return new ParameterlessConstructionFactory<HybridStopSimulation>();
        }
    }
}
