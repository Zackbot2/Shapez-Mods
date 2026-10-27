using ILogger = Core.Logging.ILogger;
using System;
using MonoMod.RuntimeDetour;

namespace BiggerCameraZoomOut
{
    public class BiggerCameraZoomOutMod
    {
        public const string MOD_NAME = nameof(BiggerCameraZoomOut);
        internal static ILogger? Logger { get; private set; }

        // in vanilla, this is 20000f
        private const float MAX_ZOOM = 30000f;

        private Hook? _Update_ApplyAngleAndZoomHook;
        private Hook? _zoomCameraHook;
        private Hook? _convertZoomToPercentageHook;

        public BiggerCameraZoomOutMod(ILogger logger)
        {
            Logger = logger;

            
        }


    }
}
