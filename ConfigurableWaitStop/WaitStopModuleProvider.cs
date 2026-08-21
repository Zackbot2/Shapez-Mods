using Core.Localization;
using Game.Core.Coordinates;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ConfigurableWaitStop
{
    /// <summary>
    /// Handles providing side panel modules for wait stop islands, as well as their config dialog.
    /// </summary>
    public class WaitStopModuleProvider : IIslandModuleDataProvider
    {
        public GlobalChunkCoordinate stationChunk;

        public WaitStopModuleProvider() { }

        public IEnumerable<IHUDSidePanelModuleData> GetModules(IslandModel island)
        {
            IIslandConfiguration configuration = island.Configuration;
            if (configuration is not WaitStopIslandConfiguration config)
            {
                //Debug.LogWarning("config mismatch! backing out.");
                yield break;
            }

            GlobalChunkTransform transform = island.Transform;
            GlobalChunkCoordinate stationChunk = ChunkVector.Zero.ToGlobal(in transform);
            int currentWaitSeconds = config.WaitTimeSeconds;

            // populate the dialogue
            //yield return new HUDSidePanelModuleInfoText.Data(new RawText($"Maximum wait time: {(currentWaitSeconds >= 0 ? $"{currentWaitSeconds} seconds" : "Infinite")} "));

            yield return new HUDSidePanelModuleInfoText.Data(currentWaitSeconds >= 0 
                ? "configurable-wait-stop.wait-time-module-seconds".T().Bind("num-seconds", new RawText(currentWaitSeconds.ToString())) 
                : "configurable-wait-stop.wait-time-module-infinite".T());

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
            IHUDDialogStack? dialogStack = WaitStopData.DialogStack;
            if (dialogStack != null)
            {
                HUDDialogSimpleInput dialog = dialogStack.Show(Globals.Resources.UIDialogSimpleInputPrefab);
                int currentWaitTime = config.WaitTimeSeconds;

                // populate the dialog with our title, description, and value. these are found in the translations.json file.
                dialog.Init(
                    title: "island.wait-stop.wait-time-dialog-title".T(),
                    description: "island.wait-stop.wait-time-dialog-desc".T(),
                    buttonText: "global.btn-confirm".T(),
                    defaultValue: new RawText(currentWaitTime.ToString()));

                // this triggers when you hit the confirm button.
                // my implementation is pretty simple, it just parses to an int if it can. otherwise, it doesn't change it.
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
                        WaitStopData.SetWaitSeconds(stationChunk, result);
                    }
                    WaitStopData.RefreshSidePanel?.Invoke();
                });
            }
        }
    }
}
