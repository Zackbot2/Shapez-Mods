using Core.Localization;
using Game.Core.Coordinates;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnlimitedWaitStop
{
    public class WaitStopModuleProvider : IIslandModuleDataProvider
    {
        private readonly WaitStopDeciderRef _deciderRef;
        //public WaitStopIslandConfiguration config;
        public GlobalChunkCoordinate stationChunk;

        public WaitStopModuleProvider(WaitStopDeciderRef deciderRef) 
        {
            _deciderRef = deciderRef;
        }

        public IEnumerable<IHUDSidePanelModuleData> GetModules(IslandModel island)
        {
            Debug.Log("getting wait stop modules...");
            IIslandConfiguration configuration = island.Configuration;
            Debug.Log($"Island type: {island.GetType().FullName}");
            Debug.Log($"config type: {configuration?.GetType().FullName}");
            Debug.Log($"Configuration is null: {island.Configuration == null}");
            if (configuration is not WaitStopIslandConfiguration config)
            {
                Debug.Log("config mismatch, continuing anyway...");
                //yield break;
            }

            var transform = island.Transform;
            GlobalChunkCoordinate stationChunk = ChunkVector.Zero.ToGlobal(in transform);

            yield return new HUDSidePanelModuleInfoText.Data(new RawText("Wait Time: " + WaitStopRegistry.Get(stationChunk) + " seconds"));
            yield return new HUDSidePanelModuleGenericButton.Data("global.btn-configure".T(), () =>
            {
                ShowConfigDialog(stationChunk);
            });
        }

        public IEnumerable<IHUDSidePanelModuleData> GetStats()
        {
            yield break;
        }

        private void ShowConfigDialog(GlobalChunkCoordinate stationChunk)
        {
            IHUDDialogStack? dialogStack = _deciderRef.DialogStack;
            Debug.Log($"showing config dialog with dialogStack {dialogStack}{(dialogStack == null ? ". it's null." : dialogStack)}");
            if (dialogStack != null)
            {
                HUDDialogSimpleInput dialog = dialogStack.Show(Globals.Resources.UIDialogSimpleInputPrefab);
                dialog.Init(
                    title: "island.wait-stop.wait-time-dialog-title".T(),
                    description: "island.wait-stop.wait-time-dialog-desc".T(),
                    buttonText: "global.btn-confirm".T(),
                    defaultValue: new RawText(WaitStopRegistry.Get(stationChunk).ToString()),
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
                        return WaitStopRegistry.Get(stationChunk).ToString();
                    });
                dialog.OnConfirmed.Register(delegate (string text)
                {
                    text = text.Trim();
                    if (int.TryParse(text, out int result))
                    {
                        if (result < 0)
                        {
                            result = -1;
                        }
                        //config.WaitTimeSeconds = result;
                        WaitStopRegistry.Set(stationChunk, result);
                    }
                    _deciderRef.RefreshSidePanel?.Invoke();
                });
            }
        }
    }
}
