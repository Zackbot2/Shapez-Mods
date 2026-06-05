using Core.Localization;
using Game.Core.Coordinates;
using System;
using System.Collections.Generic;

namespace HybridStop
{
    public class HybridStopModuleProvider : IIslandModuleDataProvider
    {
        private readonly HybridStopDeciderRef _deciderRef;
        public HybridStopModuleProvider(HybridStopDeciderRef deciderRef)
        {
            _deciderRef = deciderRef;
        }

        public IEnumerable<IHUDSidePanelModuleData> GetStats()
        {
            yield break;
        }

        public IEnumerable<IHUDSidePanelModuleData> GetModules(IslandModel island)
        {
            yield break;
        }
    }
}
