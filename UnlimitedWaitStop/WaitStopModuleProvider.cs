using Core.Localization;
using Game.Core.Coordinates;
using System;
using System.Collections.Generic;

namespace UnlimitedWaitStop
{
    public class WaitStopModuleProvider : IIslandModuleDataProvider
    {
        private readonly WaitStopDeciderRef _deciderRef;
        private IslandModel island;
        public WaitStopIslandConfiguration config;
        public GlobalChunkCoordinate stationChunk;

        public WaitStopModuleProvider(WaitStopDeciderRef deciderRef) 
        {
            _deciderRef = deciderRef;
        }

        public IEnumerable<IHUDSidePanelModuleData> GetModules(IslandModel island)
        {
            IIslandConfiguration configuration = island.Configuration;
            if (!(configuration is WaitStopIslandConfiguration config))
            {
                yield break;
            }

            var transform = island.Transform;
            GlobalChunkCoordinate stationChunk = ChunkVector.Zero.ToGlobal(in transform);

            yield return new HUDSidePanelModuleInfoText.Data(new RawText("Wait Time: " + config.WaitTimeSeconds + " seconds"));
            yield return new HUDSidePanelModuleGenericButton.Data("global.btn-configure".T(), () =>
            {
                ShowConfigDialog(config, stationChunk);
            });
        }

        public IEnumerable<IHUDSidePanelModuleData> GetStats()
        {
            yield break;
        }

        private void ShowConfigDialog(WaitStopIslandConfiguration config, GlobalChunkCoordinate stationChunk)
        {
            IHUDDialogStack? dialogStack = _deciderRef.DialogStack;
            if (dialogStack != null)
            {
                HUDDialogSimpleInput dialog = dialogStack.Show(Globals.Resources.UIDialogSimpleInputPrefab);
                dialog.Init(
                    title: "island.wait-stop.wait-time-dialog-title".T(),
                    description: "island.wait-stop.wait-time-dialog-desc".T(),
                    buttonText: "global.btn-confirm".T(),
                    defaultValue: new RawText(config.WaitTimeSeconds.ToString()),
                    inputCorrector: delegate (string input)
                    {
                        if (int.TryParse(input, out int result))
                        {
                            if (result < 0)
                            {
                                result = -1;
                            }
                            return result.ToString();
                        }
                        return config.WaitTimeSeconds.ToString();
                    });
                dialog.OnConfirmed.Register(delegate (string text)
                {
                    text = text.Trim();
                    _deciderRef.RefreshSidePanel?.Invoke();
                });
            }
        }
    }
}
