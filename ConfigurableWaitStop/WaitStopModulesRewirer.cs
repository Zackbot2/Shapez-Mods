using ShapezShifter.Hijack;

namespace ConfigurableWaitStop
{
    /// <summary>
    /// Rewire <see cref="GameSessionOrchestrator.InjectIslandsModuleProviders"/> to add <see cref="WaitStopModuleProvider"/> to the wait stop's modules.
    /// </summary>
    public class WaitStopModulesRewirer : IIslandModulesRewirer
    {
        public WaitStopModulesRewirer() { }

        /// <summary>
        /// Adds a new <see cref="WaitStopModuleProvider"/> as a module provider for the wait stop island.
        /// One module provider handles every island with that <see cref="IslandDefinitionId"/>.
        /// </summary>
        /// <param name="modulesLookup"></param>
        public void AddModules(IslandsModulesLookup modulesLookup)
        {
            ConfigurableWaitStopMod.Logger.Info?.Log("Adding wait stop modules");
            modulesLookup.AddModuleProvider(WaitStopData.WaitStationId, new WaitStopModuleProvider());
        }
    }
}
