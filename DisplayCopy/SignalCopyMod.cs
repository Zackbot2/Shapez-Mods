using UnityEngine;
using Core.Localization;
using Core.Logging;
using Game.Content.Features.Signals;
using Game.Core.Map.Simulation;
using MonoMod.RuntimeDetour;
using ShapezShifter.SharpDetour;
using System.Collections.Generic;
using ILogger = Core.Logging.ILogger;

namespace DisplayCopy
{

    /*
     * important types
     * - DisplaySimulation: contains LastInput field - the value we need to access
     * - DisplayBuildingModuleDataProvider: provides HUD modules and access to the needed DisplaySimulation instance
     * 
     * goal: 
     * - hook DisplayBuildingModuleDataProvider to add a copy button
     * - when the button is pressed, read simulation data and copy it to the user's clipboard. it should be the same format as signal producers take.
     */

    public class SignalCopyMod : IMod
    {
        internal static ILogger Logger { get; private set; } = null!;

        private Hook? _getModulesHook;

        public SignalCopyMod(ILogger logger)
        {
            Logger = logger;

            _getModulesHook = DetourHelper.CreatePostfixHook<
                DisplayBuildingModuleDataProvider,
                BuildingModel,
                ILocalizedSimulation,
                DisplaySimulation,
                IEnumerable<IHUDSidePanelModuleData>>(
                (moduleDataProvider, building, localizedSimulation, actualSimulation) => 
                    moduleDataProvider.GetSimulationModules(building, localizedSimulation, actualSimulation),
                Wrap);

            Logger.Info?.Log("SignalCopy initialized.");
        }

        private static IEnumerable<IHUDSidePanelModuleData> Wrap(
            DisplayBuildingModuleDataProvider moduleDataProvider,
            BuildingModel building,
            ILocalizedSimulation localizedSimulation,
            DisplaySimulation actualSimulation,
            IEnumerable<IHUDSidePanelModuleData> original)
        {
            foreach (var module in original)
            {
                yield return module;
            }
            yield return new HUDSidePanelModuleGenericButton.Data("signal-copy.copy-button".T(), () =>
            {
                Logger.Info?.Log("COPY!!!!!!!");
                Logger.Info?.Log($"LastInput: {actualSimulation.LastInput}");
                string lastInputString = SignalToString(actualSimulation.LastInput);
                Logger.Info?.Log($"LastInput as string: {lastInputString}");

                GUIUtility.systemCopyBuffer = lastInputString;
            }); ;
        }

        /// <summary>
        /// Convert <paramref name="signal"/> into the string you'd enter into a signal producer to get said signal.
        /// </summary>
        /// <param name="signal"></param>
        /// <returns></returns>
        private static string SignalToString(ISignal signal)
        {
            // simple ones
            if (signal == null || signal is NullSignal)
                return "null";
            if (signal is ConflictSignal)
                return "conflict";
            if (signal is IntegerSignal intSignal)
                return intSignal.Value.ToString();

            // not simple ones
            if (signal is FluidSignal fluidSignal)
            {
                Logger?.Info?.Log($"FluidSignalToString: {fluidSignal.Value} -> {fluidSignal.Value.ToString().Split(' ')[0].ToLower()}");
                return fluidSignal.Value.ToString().Split(' ')[0].ToLower() switch
                {
                    "uncolored" => "color-u",
                    "red" => "color-r",
                    "green" => "color-g",
                    "blue" => "color-b",
                    "cyan" => "color-c",
                    "magenta" => "color-m",
                    "yellow" => "color-y",
                    "white" => "color-w",
                    "black" => "color-k",
                    _ => "N/A",
                };
            }

            if (signal is BeltItemSignal beltItemSignal)
            {
                if (beltItemSignal.Value is FluidPackageItem)
                    return "null";

                return beltItemSignal.Value?.ToString() ?? "null";
            }

            return signal.ToString();
        }

        public void Dispose()
        {
            _getModulesHook?.Dispose();
        }
    }
}
