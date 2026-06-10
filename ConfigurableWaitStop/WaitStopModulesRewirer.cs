using ShapezShifter.Hijack;

namespace ConfigurableWaitStop
{
    /// <summary>
    /// Rewires ShapezShifter to add <see cref="WaitStopModuleProvider"/> to the wait stop's modules.
    /// </summary>
    /// <remarks>
    /// <see cref="GameSessionOrchestrator"/> oversees the creation of island modules, with <see cref="GameSessionOrchestrator.InjectIslandsModuleProviders"/> at the forefront of this process.
    /// ShapezShifter's Hijack system adds a postfix to it, which then calls all <see cref="IIslandModulesRewirer.AddModules"/> implementations.
    /// That includes any islands that were added through <see cref="ShapezShifter.Flow.Atomic.AtomicIslandExtender"/> and given custom modules.
    /// That's what <see cref="ShapezShifter.Flow.Atomic.AtomicIslandExtender.WithCustomModules(IIslandModuleDataProvider)"/> does internally – uses <see cref="IIslandModulesRewirer"/> to give modules to any new islands added.
    /// Since we can't use that system because we aren't extending the island (we're modifying an existing one), we need to create our OWN <see cref="IIslandModulesRewirer"/>.
    /// </remarks>
    public class WaitStopModulesRewirer : IIslandModulesRewirer
    {
        public WaitStopModulesRewirer() { }

        /// <summary>
        /// Adds a new <see cref="WaitStopModuleProvider"/> as a module provider for the wait stop island.
        /// One module provider handles every island with that <see cref="IslandDefinitionId"/>.
        /// </summary>
        /// <param name="modulesLookup"></param>
        /// <remarks>
        /// <see cref="GameSessionOrchestrator.AddModules"/> is hooked by <see cref="IIslandModulesRewirer.AddModules"/>.
        /// The only place that actually implements this is <see cref="ShapezShifter.Flow.Atomic.IslandModulesExtender"/>, so we need to make our own for the wait stop's modules, similar to <see cref="WaitStopSimulationRewirer"/>.
        /// Thankfully, once we have a <see cref="IIslandModulesRewirer"/> implementation, <paramref name="modulesLookup"/> is handed right to us, which is exactly what we need.
        /// </remarks>
        public void AddModules(IslandsModulesLookup modulesLookup)
        {
            ConfigurableWaitStopMod.Logger.Info?.Log("Adding wait stop modules");
            modulesLookup.AddModuleProvider(WaitStopData.WaitStationId, new WaitStopModuleProvider());
        }
    }
}
