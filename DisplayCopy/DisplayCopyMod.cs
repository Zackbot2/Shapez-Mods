using Core.Localization;
using Core.Logging;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DisplayCopy
{

    /*
     * important types
     * - DisplaySimulation: contains LastInput field - the value we need to access
     * - DisplayBuildingModuleDataProvider: provides HUD and simulation modules
     * 
     * goal: 
     * - hook DisplayBuildingModuleDataProvider to add a copy button
     * - when the button is pressed, read simulation data and copy it to the user's clipboard. it should be the same format as signal producers take.
     */

    public class DisplayCopyMod : IMod
    {
        internal static ILogger Logger { get; private set; } = null!;

        private ILHook? _getModulesHook;

        public DisplayCopyMod(ILogger logger)
        {
            Logger = logger;

            // GetSimulationModules is an iterator method, so this is gonna be fun...
            MethodInfo getSimulationModules = typeof(DisplayBuildingModuleDataProvider).GetMethod("GetSimulationModules", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                ?? throw new InvalidOperationException($"Failed to find method {typeof(DisplayBuildingModuleDataProvider)}.GetSimulationModules");

            // get the IteratorStateMachineAttribute attached to the method. we need it for the type of state machine
            IteratorStateMachineAttribute attribute = getSimulationModules.GetCustomAttribute<IteratorStateMachineAttribute>();

            // get the type of the state machine
            Type stateMachineType = attribute?.StateMachineType 
                ?? throw new InvalidOperationException($"Failed to find state machine type for {getSimulationModules.Name}");

            // with this info, we now need to get the MoveNext method inside this type.
            // THIS is what we actually want to hook; hooking DisplayBuildingModuleDataProvider.GetSimulationModules alone would do nothing.
            MethodInfo moveNextMethod = stateMachineType.GetMethod("MoveNext", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            // now we can finally hook the method. and that was the easy part.
            _getModulesHook = new ILHook(moveNextMethod, PatchMoveNext);

            Logger.Info?.Log("DisplayCopy initialized.");
        }

        private static void PatchMoveNext(ILContext context)
        {
            ILCursor cursor = new(context);


        }

        private static HUDSidePanelModuleGenericButton.Data CreateButton()
        {
            return new HUDSidePanelModuleGenericButton.Data("copy".T(), () =>
            {
                // leave empty for now
            });
        }

        public void Dispose()
        {
            _getModulesHook?.Dispose();
        }
    }
}
