using System;
using System.Reflection;
using Game.Content.Features.Signals;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using ILogger = Core.Logging.ILogger;

namespace RenderMyFluidPackageSignals
{
    public class RenderMyFluidPackageSignalsMod : IMod
    {
        internal static ILogger Logger { get; private set; } = null!;

        // store hooks so we can dispose them later
        private readonly ILHook? _prepareRenderSignalHook = null;

        public RenderMyFluidPackageSignalsMod(ILogger logger)
        {
            Logger = logger;
            MethodInfo prepareRenderSignal = typeof(HUDWireContentsHelper).GetMethod("PrepareRenderSignal", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance,
                binder: null,
                types: new[] { typeof(ISignal) },
                modifiers: null)
                ?? throw new InvalidOperationException($"Failed to find method {nameof(HUDWireContentsHelper)}.PrepareRenderSignal");

            _prepareRenderSignalHook = new ILHook(prepareRenderSignal, PatchPrepareRenderSignal);

            Logger.Info?.Log("RenderMyFluidPackageSignals initialized.");
        }

        private static void PatchPrepareRenderSignal(ILContext context)
        {
            ILCursor cursor = new(context);

            if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchIsinst<ShapeItem>()))
            {
                Logger.Error?.Log($"Could not find {nameof(ShapeItem)} cast in {nameof(HUDWireContentsHelper.PrepareRenderSignal)}; IL patch was not applied.");
                return;
            }

            ILLabel continueLabel = cursor.DefineLabel();

            // duplicate the top value on the evaluation stack. this is the ISignal.
            // this is needed because the following emitted instructions will consume the top value, so we need to duplicate first in order to keep it there once we're done.
            cursor.Emit(OpCodes.Dup);

            // this checks whether the object on the stack is a FluidPackageItem. if it is, the same reference is left on the stack. if not, null is pushed onto the stack instead.
            cursor.Emit(OpCodes.Isinst, typeof(FluidPackageItem));

            // this consumes the top value on the stack and reads it.
            // if it's false/null, execution jumps to continueLabel. otherwise, it'll keep going to the next instructions we emit.
            cursor.Emit(OpCodes.Brfalse, continueLabel);

            // up to this point, we've essentially just emitted an if statement that the cursor is now inside of.
            // ...and my entire fix for this method is to return early and do nothing else, lol.
            
            // for a return, make sure to pop the remaining ISignal off the stack. this isn't because of the instructions we emitted, but because every return needs to do this.
            cursor.Emit(OpCodes.Pop);

            // and now we can return safely
            cursor.Emit(OpCodes.Ret);

            // set continueLabel to the current cursor position. that means the execution will jump here, which is after the early return.
            // it's basically a metaphorical closing bracket to the metaphorical if statement.
            cursor.MarkLabel(continueLabel);
        }

        public void Dispose()
        {
            _prepareRenderSignalHook?.Dispose();
        }
    }
}
