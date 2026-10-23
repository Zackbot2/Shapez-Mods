using Core.Logging;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using ShapezShifter.Hijack;
using ShapezShifter.SharpDetour;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace ScenarioTools
{
    public class ScenarioToolsMod : IMod
    {
        internal static ILogger Logger { get; private set; } = null!;
        internal const string MOD_NAME = nameof(ScenarioToolsMod);

        private readonly SerializedGameScenarioInterceptor? _gameScenarioInterceptor;
        private readonly IRewirerProvider _whatTheFuckRewirerProvider;

        private Dictionary<ShapeId, BlueprintCurrencyShape>? _bpCurrencyShapesByShapeId;

        private Hook _addShapeStoredHooksHook;
        private ILHook _tryAddShapeForStorageHook;

        public ScenarioToolsMod(ILogger logger)
        {
            Logger = logger;

            _whatTheFuckRewirerProvider = new CachedStaticallyAccessibleRewirerProvider(logger);
            _gameScenarioInterceptor = new SerializedGameScenarioInterceptor(_whatTheFuckRewirerProvider, logger);

            GameRewirers.AddRewirer(new ScenarioToolsSerializedGameScenarioRewirer());

            _addShapeStoredHooksHook = DetourHelper.CreatePrefixHook(
                original: (currencyManager) => currencyManager.AddShapeStoredHooks(),
                prefix: delegate (BlueprintCurrencyManager currencyManager)
                {
                    _bpCurrencyShapesByShapeId = new Dictionary<ShapeId, BlueprintCurrencyShape>();

                    foreach(BlueprintCurrencyShape bpShape in currencyManager.Mode.ResearchConfig.BlueprintCurrencyShapes)
                    {
                        foreach (string? shapeRotation in ShapeUtils.GetRotationsForQuadShape(bpShape.ShapeHash))
                        {
                            if (shapeRotation == null) break;

                            ShapeId bpShapeId;
                            try
                            {
                                bpShapeId = currencyManager.ShapeIdManager.Resolve(shapeRotation);
                            }
                            catch
                            {
                                Logger.Error?.Log($"[{MOD_NAME}] Failed to resolve ShapeId for blueprint shape {bpShape.ShapeHash} with rotation {shapeRotation}. This blueprint shape will not be available in manufacture scenarios.");
                                continue;
                            }
                            _bpCurrencyShapesByShapeId.TryAdd(bpShapeId, bpShape);
                        }
                    }
                    Logger.Info?.Log($"[{MOD_NAME}] Resolved ShapeIds for {_bpCurrencyShapesByShapeId.Count} blueprint shapes.");
                });

            MethodInfo tryAddShapeForStorageMethod = typeof(ResearchManager).GetMethod("TryAddShapeForStorage", BindingFlags.Instance | BindingFlags.Public);
            _tryAddShapeForStorageHook = new ILHook(
                source: tryAddShapeForStorageMethod,
                manip: context =>
                {
                    ILCursor cursor = new(context);

                    // find the call to ResearchPlayerLevelGoalManager.IsPlayerLevelShape and move right after it
                    if (!cursor.TryGotoNext(MoveType.After, instruction => instruction.MatchCallvirt<ResearchPlayerLevelGoalManager>(nameof(ResearchPlayerLevelGoalManager.IsPlayerLevelShape))))
                    {
                        Logger.Error?.Log($"[{MOD_NAME}] Failed to find the call to IsPlayerLevelShape in TryAddShapeForStorage. Custom blueprint shapes will not work in manufacture scenarios.");
                        return;
                    }

                    cursor.EmitLdarg2();    // load argument 2, which is shapeId

                    cursor.EmitLdarg0();    // load argument 0, which is always "this". in this case, that's the ResearchManager instance.

                    // emit the new check. it receives the output from the old one as its input
                    cursor.EmitDelegate<Func<bool, ShapeId, ResearchManager, bool>>((isPlayerLevelShape, shapeId, researchManager) => IsAcceptedShape(isPlayerLevelShape, shapeId, researchManager));
                });
        }

        public void Dispose()
        {
            _gameScenarioInterceptor?.Dispose();
            _addShapeStoredHooksHook.Dispose();
            _tryAddShapeForStorageHook.Dispose();
        }

        private bool IsAcceptedShape(bool isPlayerLevelShape, ShapeId shapeid, ResearchManager researchManager)
        {
            return isPlayerLevelShape || (_bpCurrencyShapesByShapeId != null && _bpCurrencyShapesByShapeId.ContainsKey(shapeid) && !IsBlueprintShapeLocked(_bpCurrencyShapesByShapeId[shapeid], researchManager));
        }

        public static bool IsBlueprintShapeLocked(BlueprintCurrencyShape bpShape, ResearchUnlockManager unlockManager)
        {
            return unlockManager.IsLocked(bpShape);
        }

        public static bool IsBlueprintShapeLocked(BlueprintCurrencyShape bpShape, ResearchManager researchManager)
        {
            return IsBlueprintShapeLocked(bpShape, researchManager.UnlockManager);
        }
    }
}
