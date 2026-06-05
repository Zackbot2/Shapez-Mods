using Core.Localization;
using Game.Core.Coordinates;
using System;
using System.Collections.Generic;

namespace UnlimitedWaitStop
{
    public class WaitStopModuleProvider : IIslandModuleDataProvider
    {
        public IEnumerable<IHUDSidePanelModuleData> GetModules(IslandModel island)
        {
            IIslandConfiguration configuration = island.Configuration;
            WaitStopIslandConfiguration config = configuration as WaitStopIslandConfiguration;
            if (config == null)
            {
                yield break;
            }

            var transform = island.Transform;
            GlobalChunkCoordinate stationChunk = ChunkVector.Zero.ToGlobal(in transform);

            yield return new HUDSidePanelModuleInfoText.Data(new RawText("Wait Time: " + config.WaitTimeSeconds + " seconds"));
            yield return new HUDSidePanelModuleGenericButton.Data("global.btn-configure".T(), () =>
            {
                ShowChannelConfigDialog(config, stationChunk);
            });
        }

        public IEnumerable<IHUDSidePanelModuleData> GetStats()
        {
            yield break;
        }

        private void ShowChannelConfigDialog(WaitStopIslandConfiguration config, GlobalChunkCoordinate stationChunk)
        {

        }
}
