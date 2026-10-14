using Core.Factory;
using Game.Core.Content.Islands;
using Game.Core.Modding;
using Game.Core.Research;
using MonoMod.RuntimeDetour;
using ShapezShifter.SharpDetour;
using System;
using System.Reflection;
using ILogger = Core.Logging.ILogger;

namespace TestMod
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// This class name is stupid.
    /// </remarks>
    public class TestModMod : IMod
    {
        public const string MOD_NAME = nameof(TestMod);

        internal static ILogger Logger = null!;

        private Hook? _tryAddShapeForStorageHook;

        public TestModMod(ILogger logger)
        {
            Logger = logger;

            MethodInfo target = typeof(ResearchManager).GetMethod("TryAddShapeForStorage", BindingFlags.Instance | BindingFlags.Public)!;
            //_tryAddShapeForStorageHook = DetourHelper.CreatePrefixHook<ResearchManager, ShapeCollectorProcessing, ShapeId, ulong, UnifiedShapeId>(
            //    original: (manager, processing, shapeId, totalAmount, unifiedShapeId) => manager.TryAddShapeForStorage(processing, shapeId, totalAmount, out unifiedShapeId),
            //    static delegate (ResearchManager manager, ShapeCollectorProcessing processing, ShapeId shapeId, ulong totalAmount, UnifiedShapeId unifiedShapeId)
            //    {
            //        Logger.Info?.Log($"[TestMod] TryAddShapeForStorage called with processing={processing}, shapeId={shapeId}, totalAmount={totalAmount}, unifiedShapeId={unifiedShapeId}");

            //        return (processing, shapeId, totalAmount, unifiedShapeId);
            //    });
            _tryAddShapeForStorageHook = new Hook(
                source: target!,
                target: (TryAddShapeForStorageHook)TryAddShapeForStoragePatch);
        }

        delegate bool TryAddShapeForStorageOrig(ResearchManager manager, ShapeCollectorProcessing processing, ShapeId shapeId, ulong totalAmount, out UnifiedShapeId unifiedShapeId);

        delegate bool TryAddShapeForStorageHook(TryAddShapeForStorageOrig orig, ResearchManager manager, ShapeCollectorProcessing processing, ShapeId shapeId, ulong totalAmount, out UnifiedShapeId unifiedShapeId);

        static bool TryAddShapeForStoragePatch(TryAddShapeForStorageOrig orig, ResearchManager manager, ShapeCollectorProcessing processing, ShapeId shapeId, ulong totalAmount, out UnifiedShapeId unifiedShapeId)
        {
            Logger.Info?.Log($"TryAddShapeForStorage called with processing={processing}, shapeId={shapeId}, totalAmount={totalAmount}");
            // Call the original method
            bool result = orig(manager, processing, shapeId, totalAmount, out unifiedShapeId);
            return result;
        }

        public void Dispose()
        {
            _tryAddShapeForStorageHook?.Dispose();
        }
    }
}
