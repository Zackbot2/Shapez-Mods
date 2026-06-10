using Core.Logging;
using ShapezShifter.Hijack;

namespace UnlimitedWaitStop
{
    public class WaitStopModulesRewirer : IIslandModulesRewirer
    {
        private readonly WaitStopDeciderRef _deciderRef;
        private readonly ILogger _logger;

        public WaitStopModulesRewirer(WaitStopDeciderRef deciderRef, ILogger logger) 
        {
            _deciderRef = deciderRef;
            _logger = logger;
        }

        public void AddModules(IslandsModulesLookup modulesLookup)
        {
            _logger.Info?.Log("Adding wait stop modules");
            if (_deciderRef.WaitStationId == null)
                return;

            modulesLookup.AddModuleProvider(_deciderRef.WaitStationId, new WaitStopModuleProvider(_deciderRef));
        }
    }
}
