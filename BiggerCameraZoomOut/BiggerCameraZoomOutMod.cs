using System;
using MonoMod.RuntimeDetour;
using System.Reflection;
using ShapezShifter.SharpDetour;
using Unity.Mathematics;
using MonoMod.Cil;
using System.Reflection.Emit;
using ILogger = Core.Logging.ILogger;
using Opcodes = Mono.Cecil.Cil.OpCodes;

namespace BiggerCameraZoomOut
{
    public class BiggerCameraZoomOutMod : IMod
    {
        public const string MOD_NAME = nameof(BiggerCameraZoomOut);
        internal static ILogger? Logger { get; private set; }

        // in vanilla, this is 20000f
        private const float MAX_ZOOM = 40000f;

        private Hook? _zoomCameraHook;
        private Hook? _convertZoomToPercentageHook;
        private ILHook? _update_ApplyAngleAndZoomHook;
        private ILHook? _processPanelsForCollapsingHook;

        public BiggerCameraZoomOutMod(ILogger logger)
        {
            Logger = logger;

            _zoomCameraHook = DetourHelper.Replace(
                original: (controller, amount) => controller.ZoomCamera(amount),
                replacement: delegate (CameraController controller, float amount)
                {
                    controller.Viewport.TargetZoom = math.clamp(controller.Viewport.TargetZoom + amount * controller.Viewport.TargetZoom, CameraController.MinZoom, MAX_ZOOM);
                }
            );

            _convertZoomToPercentageHook = DetourHelper.Replace(
                original: (compass, zoom) => compass.ConvertZoomToPercentage(zoom),
                replacement: delegate (HUDCompass compass, float zoom)
                {
                    return math.pow(math.saturate((zoom - CameraController.MinZoom) / (MAX_ZOOM - CameraController.MinZoom)), 0.33333334f);
                }
            );

            MethodInfo update_ApplyAngleAndZoomMethod = typeof(CameraController).GetMethod("Update_ApplyAngleAndZoom", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            MethodInfo get_MaxZoom = typeof(CameraController).GetMethod("get_MaxZoom", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            _update_ApplyAngleAndZoomHook = new ILHook(
                source: update_ApplyAngleAndZoomMethod,
                manip: context =>
                {
                    ILCursor cursor = new(context);
                    if(!cursor.TryGotoNext(MoveType.Before, x => x.MatchCall(get_MaxZoom)) || cursor.Next == null)
                    {
                        AbortInitialization("Failed to hook Update_ApplyAngleAndZoom");
                        return;
                    }
                    
                    cursor.Next.OpCode = Opcodes.Ldc_R4;
                    cursor.Next.Operand = MAX_ZOOM;
                }
            );

            MethodInfo processPanelsForCollapsingMethod = typeof(HUDWaypointsVisualization).GetMethod("ProcessPanelsForCollapsing", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            _processPanelsForCollapsingHook = new ILHook(
                source: processPanelsForCollapsingMethod,
                manip: context =>
                {
                    ILCursor cursor = new(context);

                    if(!cursor.TryGotoNext(MoveType.Before, x => x.MatchLdcR4(20000f)) || cursor.Next == null)
                    {
                        AbortInitialization("Failed to hook ProcessPanelsForCollapsing");
                        return;
                    }
                    
                    cursor.Next.Operand = MAX_ZOOM;
                }
            );

            Logger?.Info?.Log($"[{MOD_NAME}] Mod initialized successfully!");
        }

        public void Dispose()
        {
            _update_ApplyAngleAndZoomHook?.Dispose();
            _zoomCameraHook?.Dispose();
            _convertZoomToPercentageHook?.Dispose();
            _processPanelsForCollapsingHook?.Dispose();
            Logger = null;
        }

        private void AbortInitialization(string message)
        {
            Logger?.Error?.Log($"[{MOD_NAME}] Mod initialization aborted: {message}");
            Dispose();
        }
    }
}
