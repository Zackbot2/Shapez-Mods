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

        private List<ShapeId>? _blueprintShapes;

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
                    _blueprintShapes = new List<ShapeId>();

                    foreach(BlueprintCurrencyShape bpShape in currencyManager.Mode.ResearchConfig.BlueprintCurrencyShapes)
                    {
                        foreach (string? shapeRotation in ShapeUtils.GetRotationsForQuadShape(bpShape.ShapeHash))
                        {
                            if (shapeRotation == null) break;
                            ShapeId bpShapeId = currencyManager.ShapeIdManager.Resolve(shapeRotation);
                            _blueprintShapes.Add(bpShapeId);
                            Logger.Info?.Log($"Resolved blueprint ShapeId for {shapeRotation}: {bpShapeId}");
                        }
                    }
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

                    cursor.Emit(OpCodes.Ldarg_2);   // load argument 2, which is shapeId

                    // emit the new check. it receives the output from the old one as its input
                    cursor.EmitDelegate<Func<bool, ShapeId, bool>>(
                        (isPlayerLevelShape, shapeId) => IsAcceptedShape(isPlayerLevelShape, shapeId));
                });

        }

        public void Dispose()
        {
            _gameScenarioInterceptor?.Dispose();
            _addShapeStoredHooksHook.Dispose();
            _tryAddShapeForStorageHook.Dispose();
        }

        private bool IsAcceptedShape(bool isPlayerLevelShape, ShapeId shapeid)
        {
            return isPlayerLevelShape || (_blueprintShapes != null && _blueprintShapes.Contains(shapeid));
        }
    }
}
