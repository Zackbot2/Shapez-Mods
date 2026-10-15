using Core.Factory;
using Game.Core.Content.Islands;
using Game.Core.GameMode;
using Game.Core.Modding;
using Game.Core.Research;
using MonoMod.RuntimeDetour;
using ShapezShifter.SharpDetour;
using System;
using System.Collections.Generic;
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
        private Hook? _processRewardsHook;

        public TestModMod(ILogger logger)
        {
            Logger = logger;

            //MethodInfo tryAddShapeForStorageMethod = typeof(ResearchManager).GetMethod("TryAddShapeForStorage", BindingFlags.Instance | BindingFlags.Public)!;
            //_tryAddShapeForStorageHook = new Hook(
            //    source: tryAddShapeForStorageMethod!,
            //    target: (TryAddShapeForStorageHook)TryAddShapeForStoragePatch);

            MethodInfo processRewardsMethod = typeof(ResearchProgressionBalancer).GetMethod("ProcessRewards", BindingFlags.Instance | BindingFlags.NonPublic)!;
            _processRewardsHook = new Hook(
                source: processRewardsMethod!,
                target: (ProcessRewardsHook)ProcessRewardsPatch);
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

        delegate List<IResearchReward> ProcessRewardsOrig(ResearchProgressionBalancer balancer, List<IResearchReward> rewards);
        delegate List<IResearchReward> ProcessRewardsHook(ProcessRewardsOrig orig, ResearchProgressionBalancer balancer, List<IResearchReward> rewards);

        static List<IResearchReward> ProcessRewardsPatch(ProcessRewardsOrig orig, ResearchProgressionBalancer balancer, List<IResearchReward> rewards)
        {
            Logger.Info?.Log($"ResearchProgressionBalancer data: BlueprintCostMultiplier={balancer.BlueprintCostMultiplier}; BlueprintRewardMultiplier={balancer.BlueprintRewardMultiplier}; ChunkLimitMultiplier={balancer.ChunkLimitMultiplier}; ResearchShapeCostMultiplier={balancer.ResearchShapeCostMultiplier}");

            return orig(balancer, rewards);
        }

        public void Dispose()
        {
            _tryAddShapeForStorageHook?.Dispose();
            _processRewardsHook?.Dispose();
        }
    }
}
