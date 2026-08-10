using Game.Core.Trains;
using ShapezShifter.Hijack;
using System.Collections.Generic;
using System.Linq;

namespace ConfigurableWaitStop
{
    /// <summary>
    /// Rewires the simulation to use <see cref="WaitStopIslandSimulationSystem"/>.
    /// </summary>
    /// <remarks>
    /// <see cref="SimulationSystemsInterceptor"/> is in charge of all <see cref="ISimulationSystemsRewirer"/> instances.
    /// In its constructor, it uses the <see cref="IRewirerProvider"/> to get all of the rewirers of the ISimulationSystemsRewirer type, and calls <see cref="ISimulationSystemsRewirer.ModifySimulationSystems"/> on each one.
    /// So all we need to do is have this class implement that method, have something else add the rewirer, and that's it.
    /// </remarks>
    public class WaitStopSimulationRewirer : ISimulationSystemsRewirer
    {
        public WaitStopSimulationRewirer() { }

        public void ModifySimulationSystems(ICollection<ISimulationSystem> simulationSystems, SimulationSystemsDependencies dependencies)
        {
            simulationSystems.Add(new WaitStopIslandSimulationSystem());
        }
    }
}
