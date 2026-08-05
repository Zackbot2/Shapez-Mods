using Core.Localization;
using Core.Logging;
using Game.Core.Map.Simulation;
using Game.Core.Trains.Stations;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using ShapezShifter.SharpDetour;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Opcodes = Mono.Cecil.Cil.OpCodes;

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

    public class DisplayCopyMod : IMod
    {
        internal static ILogger Logger { get; private set; } = null!;

        private Hook? _getModulesHook;

        public DisplayCopyMod(ILogger logger)
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

            Logger.Info?.Log("DisplayCopy initialized.");
        }

        private static IEnumerable<IHUDSidePanelModuleData> Wrap(
            DisplayBuildingModuleDataProvider moduleDataProvider,
            BuildingModel building,
            ILocalizedSimulation localizedSimulation,
            DisplaySimulation actualSimulation,
            IEnumerable<IHUDSidePanelModuleData> original)
        {
            Logger.Info?.Log("DisplayCopy: wrap called");
            foreach (var module in original)
            {
                yield return module;
            }
            yield return new HUDSidePanelModuleGenericButton.Data("copy".T(), () =>
            {
                Logger.Info?.Log("DisplayCopy: COPY!!!!!!!");
                Logger.Info?.Log($"LastInput: {actualSimulation.LastInput}");   
            }); ;
        }

        public void Dispose()
        {
            _getModulesHook?.Dispose();
        }
    }
}
