using ShapezShifter.Hijack;

namespace ConfigurableWaitStop
{
    /// <summary>
    /// Rewire <see cref="GameSessionOrchestrator.InjectIslandsModuleProviders"/> to add <see cref="WaitStopModuleProvider"/> to the wait stop's modules.
    /// This is needed because wait stops don't have a module provider or any modules, so we need to tell the game that it's going to have them now.
    /// </summary>
    public class WaitStopModulesRewirer : IIslandModulesRewirer
    {
        public WaitStopModulesRewirer() { }

        /// <summary>
        /// Adds a new <see cref="WaitStopModuleProvider"/> for the wait stop island.
        /// One module provider handles every island with that <see cref="IslandDefinitionId"/>.
        /// </summary>
        /// <param name="modulesLookup"></param>
        public void AddModules(IslandsModulesLookup modulesLookup)
        {
            ConfigurableWaitStopMod.Logger.Info?.Log($"{ConfigurableWaitStopMod.ModName}: Adding wait stop modules");
            modulesLookup.AddModuleProvider(WaitStopData.WaitStationId, new WaitStopModuleProvider());
        }
    }
}
