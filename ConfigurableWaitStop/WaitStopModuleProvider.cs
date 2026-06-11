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
            //Debug.Log("getting wait stop modules...");
            IIslandConfiguration configuration = island.Configuration;
            //Debug.Log($"Island type: {island.GetType().FullName}");
            //Debug.Log($"config type: {configuration?.GetType().FullName}");
            //Debug.Log($"Configuration is null: {island.Configuration == null}");
            if (configuration is not WaitStopIslandConfiguration config)
            {
                //Debug.LogWarning("config mismatch! backing out.");
                yield break;
            }

            var transform = island.Transform;
            GlobalChunkCoordinate stationChunk = ChunkVector.Zero.ToGlobal(in transform);
            int currentWaitSeconds = config.WaitTimeSeconds;

            yield return new HUDSidePanelModuleInfoText.Data(new RawText($"Maximum wait time: {(currentWaitSeconds >= 0 ? $"{currentWaitSeconds} seconds" : "Infinite")} "));
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

                // populate the dialog with our title, description, and value.
                dialog.Init(
                    title: "island.wait-stop.wait-time-dialog-title".T(),
                    description: "island.wait-stop.wait-time-dialog-desc".T(),
                    buttonText: "global.btn-confirm".T(),
                    defaultValue: new RawText(currentWaitTime.ToString()));

                // this triggers when you hit the confirm button. my implementation is pretty simple, it just parses to an int if it can.
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
