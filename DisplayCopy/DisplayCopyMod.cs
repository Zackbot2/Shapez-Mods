using Core.Localization;
using Core.Logging;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Reflection;
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

        private static Type? stateMachineType;

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
            stateMachineType = attribute?.StateMachineType 
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
            Logger.Info?.Log($"Instructions BEFORE patch:");
            int index = 0;
            foreach (var instruction in context.Body.Instructions)
            {
                Logger.Info?.Log($"{index++:D3}: {instruction.OpCode} {instruction.Operand}");
            }

            FieldInfo stateField = stateMachineType?.GetField("<>1__state", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) 
                ?? throw new Exception("Failed to find state field.");

            /*
             * BEFORE PATCH:
             * push state
             * pop state. if 0, jump case0
             * push state
             * push 1
             * pop both. if equal, jump case1
             * push 0 (MoveNext returns bool)
             * return
             * case0:
             * set state -1
             * ...
             * set state 1
             * case1:
             * return false
             * 
             * AFTER PATCH:
             * push state
             * pop state. if 0, jump case0
             * push state
             * push 1
             * pop both. if equal, jump case1
             * > push state
             * > push 2
             * > pop both. if equal, jump case2
             * push 0 (MoveNext returns bool)
             * return
             * case0:
             * set state -1
             * ...
             * set state 1
             * return true
             * case1:
             * > set state -1
             * > emit AddButton method call
             * > set state 2
             * > return true
             * > case2:
             * return false
             */

            // find the highest assignment to the state field. this will be for the body
            int highestState = 0;
            Instruction? highestStateAssignment = null;
            foreach (Instruction instruction in context.Body.Instructions)
            {
                // if this assigns to the state field
                if (instruction.MatchStfld(stateField))
                {
                    // because MoveNext is compiler-generated, the previous instruction is ldc.i4.X for every assignment to the state field.
                    // ldc.i4.X contains the value that was assigned.
                    int? stateValue = instruction.Previous.GetInt();

                    if (stateValue.HasValue && stateValue > highestState)
                    {
                        highestState = stateValue.Value;
                        highestStateAssignment = instruction;
                    }
                }
            }

            if (highestStateAssignment == null)
                throw new Exception("Failed to find highest state assignment");

            // using that value, look for a beq that deals with this value
            Instruction? highestJump = null;
            foreach (Instruction instruction in context.Body.Instructions)
            {
                if (instruction.MatchBeq(out _) && instruction.Previous.GetInt() == highestState)
                {
                    highestJump = instruction;
                    break;
                }
            }

            if (highestJump == null)
                throw new Exception("Failed to find comparison jump for highest state");


            // method has been searched, our entry points have been identified. create the cursors (yes, two!) and start patching.
            // dispatch is the initial decision tree, and the body is where actual code happens. the compiler generates it cleanly this way.
            ILCursor dispatchCursor = new(context);
            ILCursor bodyCursor = new(context);


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
