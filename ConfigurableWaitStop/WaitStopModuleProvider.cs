using Core.Localization;
using Game.Core.Coordinates;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ConfigurableWaitStop
{
    public class WaitStopModuleProvider : IIslandModuleDataProvider
    {
        private readonly WaitStopDeciderRef _deciderRef;
        public GlobalChunkCoordinate stationChunk;

        public WaitStopModuleProvider(WaitStopDeciderRef deciderRef)
        {
            _deciderRef = deciderRef;
        }

        public IEnumerable<IHUDSidePanelModuleData> GetModules(IslandModel island)
        {
            //Debug.Log("getting wait stop modules...");
            IIslandConfiguration configuration = island.Configuration;
            //Debug.Log($"Island type: {island.GetType().FullName}");
            //Debug.Log($"config type: {configuration?.GetType().FullName}");
            //Debug.Log($"Configuration is null: {island.Configuration == null}");
            if (configuration is not WaitStopIslandConfiguration config)
            {
                //Debug.Log("config mismatch! backing out.");
                yield break;
            }

            var transform = island.Transform;
            GlobalChunkCoordinate stationChunk = ChunkVector.Zero.ToGlobal(in transform);
            int currentWaitSeconds = config.WaitTimeSeconds;

            yield return new HUDSidePanelModuleInfoText.Data(new RawText($"Wait Time: {(currentWaitSeconds >= 0 ? $"{currentWaitSeconds} seconds" : "Infinite")} "));
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
                int currentWaitTime = config.WaitTimeSeconds;
                dialog.Init(
                    title: "island.wait-stop.wait-time-dialog-title".T(),
                    description: "island.wait-stop.wait-time-dialog-desc".T(),
                    buttonText: "global.btn-confirm".T(),
                    defaultValue: new RawText(currentWaitTime.ToString()));

                dialog.OnConfirmed.Register(delegate (string text)
                {
                    text = text.Trim();
                    if (int.TryParse(text, out int result))
                    {
                        if (result < 0)
                        {
                            result = -1;
                        }
                        config.WaitTimeSeconds = result;
                        _deciderRef.SetWaitSeconds(stationChunk, result);
                    }
                    _deciderRef.RefreshSidePanel?.Invoke();
                });
            }
        }
    }
}
