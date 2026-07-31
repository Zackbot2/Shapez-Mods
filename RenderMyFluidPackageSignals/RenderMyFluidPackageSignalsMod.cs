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

        // store hooks so they don't get GCed, and so we can dispose them later
        private ILHook? _renderSignalHook = null;

        public RenderMyFluidPackageSignalsMod(ILogger logger)
        {
            Logger = logger;
            MethodInfo renderSignal = typeof(HUDWireContentsHelper).GetMethod("RenderSignal", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance,
                binder: null,
                types: new[] { typeof(ISignal) },
                modifiers: null)!;

            _renderSignalHook = new ILHook(renderSignal, PatchRenderSignal);

            Logger.Info?.Log("RenderMyFluidPackageSignalsMod initialized.");
        }

        private static void PatchRenderSignal(ILContext context)
        {
            ILCursor cursor = new(context);

            if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchIsinst<ShapeItem>()))
            {
                Logger.Error?.Log($"Could not find ShapeItem cast in {nameof(HUDWireContentsHelper.RenderSignal)}; IL patch was not applied.");
                return;
            }

            ILLabel continueLabel = cursor.DefineLabel();

            // duplicate the top value on the evaluation stack. this is the ISignal.
            // this is needed because the following emitted instructions will consume the top value, so we need to duplicate first in order to keep it there once we're done.
            cursor.Emit(OpCodes.Dup);

            // this checks whether the object on the stack is a FluidPackageItem. if it is, the same reference is left on the stack. if not, null is pushed onto the stack instead.
            cursor.Emit(OpCodes.Isinst, typeof(FluidPackageItem));

            // if the top value on the stack is false/null, execution jumps to continueLabel. otherwise, it'll keep going.
            cursor.Emit(OpCodes.Brfalse, continueLabel);

            // now we can pop our item off the stack (which we know is either FluidPackageItem or null) in order to leave the stack how we found it...
            cursor.Emit(OpCodes.Pop);

            // ...and return early to skip the rest of the method.
            cursor.Emit(OpCodes.Ret);

            // set continueLabel to the current cursor position. that means the execution will jump here, which is after the early return.
            cursor.MarkLabel(continueLabel);
        }

        public void Dispose()
        {
            _renderSignalHook?.Dispose();
        }
    }
}
