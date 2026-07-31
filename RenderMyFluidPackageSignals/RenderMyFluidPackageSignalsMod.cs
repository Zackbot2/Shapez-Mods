using System;
using System.Reflection;
using Game.Content.Features.Signals;
using MonoMod.RuntimeDetour;
using ShapezShifter.SharpDetour;
using ILogger = Core.Logging.ILogger;

namespace RenderMyFluidPackageSignals
{
    public class RenderMyFluidPackageSignalsMod
    {
        internal static ILogger Logger { get; private set; } = null!;

        // store hooks so they don't get GCed, and so we can dispose them later
        private Hook? _renderSignalHook = null;

        public RenderMyFluidPackageSignalsMod(ILogger logger)
        {
            MethodInfo renderSignal = typeof(HUDWireContentsHelper).GetMethod("RenderSignal", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance,
                binder: null,
                types: new[] { typeof(ISignal) },
                modifiers: null)!;

            _renderSignalHook = new Hook(
                renderSignal,
                (Action<Action<HUDWireContentsHelper, ISignal>, HUDWireContentsHelper, ISignal>)Patch);
        }

        private static void Patch(Action<HUDWireContentsHelper, ISignal> orig, HUDWireContentsHelper contentsHelper, ISignal signal)
        {
            if (signal is BeltItemSignal beltItemSignal && beltItemSignal.Value is FluidPackageItem)
            {
                return;
            }
            
            orig(contentsHelper, signal);
        }

        public void Dispose()
        {
            _renderSignalHook?.Dispose();
        }
    }
}
