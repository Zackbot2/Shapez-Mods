using Core.Disposing;
using Game.Content.Trains;
using Game.Content.Trains.Predictions;
using Game.Core.Rails;
using Game.Core.Trains;
using Game.Core.Trains.Stations;
using MonoMod.RuntimeDetour;
using ShapezShifter.Hijack;
using ShapezShifter.SharpDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TrainsLib.Events;
using TrainsLib.Rewirers;
using TrainsLib.Stations;
using EventRegistry = TrainsLib.Events.EventRegistry;

namespace TrainsLib
{
    /// <summary>
    /// Handles hooks and rewirers for <see cref="TrainsLib"/>.
    /// </summary>
    internal class HookHandler : IDisposable
    {
        public static HookHandler? Instance;

        // hooks & rewirers
        private readonly List<Hook> _hooks = new();
        private readonly List<RewirerHandle> _rewirerHandles = new();

        private HookHandler()
        {
            Instance ??= this;
        }

        public void Dispose()
        {
            // dispose all hooks
            _hooks.Clear();

            // dispose all rewirers
            _rewirerHandles.ForEach(handle => GameRewirers.RemoveRewirer(handle));
            _rewirerHandles.Clear();

            // dispose this instance
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// Initialize the <see cref="HookHandler"/>, creating an instance and all hooks/rewirers.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public static void Initialize()
        {
            // create an instance, if one doesn't already exist
            if (Instance != null)
            {
                throw new InvalidOperationException($"Cannot initialize an already initialized {nameof(HookHandler)}.");
            }
            Instance = new();

            Instance.CreateHooks();
            Instance.RegisterRewirers();
        }

        /// <summary>
        /// Create all hooks required for the mod to function.
        /// </summary>
        private void CreateHooks()
        {
            TrainsLibLogger.LogInfo("Creating hooks...");

            // TrainStationCoordinator.ShouldTrainStop hook
            //_hooks.Add(DetourHelper.CreatePostfixHook(
            //    (TrainStationCoordinator coordinator, TrainId trainId, TrainSimulationData trainData) => coordinator.ShouldTrainStop(trainId, trainData),
            //    delegate (TrainStationCoordinator coordinator, TrainId trainId, TrainSimulationData train, bool __result)
            //    {
            //        if (__result)
            //        {
            //            EventHandler.OnTrainArrivedEvent.Invoke(new TrainArrivedEvent(
            //                trainId,
            //                train,
            //                coordinator.StopController
            //            ));
            //            TrainsLibLogger.LogInfo($"Train {trainId} arrived at a station. (ShouldTrainStop)");
            //        }
            //        return __result;
            //    }
            //));

            // TrainStationCoordinator.TrainStopController.ShouldHaltTrain hook
            _hooks.Add(DetourHelper.CreatePostfixHook(
                (TrainStationCoordinator.TrainStopController controller, TrainId trainId, TrainSimulationData train) => controller.ShouldHaltTrain(trainId, train),
                delegate (TrainStationCoordinator.TrainStopController controller, TrainId trainId, TrainSimulationData train, bool __result)
                {
                    if (__result)
                    {
                        EventRegistry.OnTrainArrivedEvent.Invoke(new TrainArrivedEvent(
                            trainId,
                            train,
                            controller
                        ));
                    }
                    return __result;
                }
                ));

            // TrainStationCoordinator.TrainStopController.ShouldTrainContinue hook
            _hooks.Add(DetourHelper.CreatePostfixHook(
                (TrainStationCoordinator.TrainStopController controller, TrainId trainId, TrainSimulationData train) => controller.ShouldTrainContinue(trainId, train),
                delegate (TrainStationCoordinator.TrainStopController controller, TrainId trainId, TrainSimulationData train, bool __result)
                {
                    if (__result)
                    {
                        EventRegistry.OnTrainLeftEvent.Invoke(new TrainLeftEvent(
                            trainId,
                            train,
                            controller
                        ));
                    }
                    return __result;
                }
                ));

            // BuiltinPredictionSimulationSystems.CreateTrainStationSystems
            _hooks.Add(DetourHelper.CreatePostfixHook(
                (BuiltinPredictionSimulationSystems builtinSystems, TrainWagonCargoTypeId shapeCargoType, TrainWagonCargoTypeId fluidCargoType) => 
                builtinSystems.CreateTrainStationSystems(shapeCargoType, fluidCargoType),
                BuiltinPredictionSimulationSystemsHook
                ));

            // TrainNavigationPredictionCoordinators.CreateTrainPredictionCoordinators
            _hooks.Add(DetourHelper.CreateStaticPrefixHook(
                (ITrainNavigationSimulationConfig trainNavigationConfiguration, TrainIslandCollection<IIslandDefinition> trainIslands, IEnumerable<IIslandDefinition> rails, IReadOnlyRailColorRegistry railColorRegistry) =>
                TrainNavigationPredictionCoordinators.CreateTrainPredictionCoordinators(trainNavigationConfiguration, trainIslands, rails, railColorRegistry),
                delegate (ITrainNavigationSimulationConfig trainNavigationConfiguration, TrainIslandCollection<IIslandDefinition> trainIslands, IEnumerable<IIslandDefinition> rails, IReadOnlyRailColorRegistry railColorRegistry)
                {
                    // stop.Definition may be null, but i'd like to throw if it gets to this point and is still null
                    rails.ToList().AddRange(TrainStops.ModdedTrainStops.Select(stop => stop.Definition!));
                    return (trainNavigationConfiguration, trainIslands, rails, railColorRegistry);
                }
                ));
        }

        private void RegisterRewirers()
        {
            TrainsLibLogger.LogInfo("Registering rewiwers...");

            _rewirerHandles.Add(GameRewirers.AddRewirer(new TrainsSimulationSystemsRewirer()));
            _rewirerHandles.Add(GameRewirers.AddRewirer(new GameIslandsProvider()));
        }

        /// <summary>
        /// Hook <see cref="BuiltinPredictionSimulationSystems.CreateTrainStationSystems"/> in order to create shape prediction systems for all <see cref="ModdedTrainStop"/>s registered in <see cref="TrainStops"/>."/>
        /// </summary>
        /// <param name="builtinSystems"></param>
        /// <param name="shapeCargoType"></param>
        /// <param name="fluidCargoType"></param>
        /// <param name="original">The original collection being returned.</param>
        /// <returns></returns>
        private IEnumerable<ISimulationSystem> BuiltinPredictionSimulationSystemsHook(BuiltinPredictionSimulationSystems builtinSystems, TrainWagonCargoTypeId shapeCargoType, TrainWagonCargoTypeId fluidCargoType, IEnumerable<ISimulationSystem> original)
        {
            // when it's going to return a PredictionSimulationSystem, throw that one out and create a new one that includes the hybrid stop.
            foreach (ISimulationSystem simulationSystem in original)
            {
                if (simulationSystem is TrainStationPredictionSimulationSystem)
                {
                    // the game hardcodes this, so we have to roll with it.
                    List<IslandDefinitionId> trainStops = new()
                        {
                            builtinSystems.Mode.Islands.Trains.Navigation.QuickStation.Id,
                            builtinSystems.Mode.Islands.Trains.Navigation.WaitStation.Id,
                        };
                    trainStops.AddRange(TrainStops.ModdedTrainStops.Select(stop => stop.DefinitionId));

                    ITrainSubStationSimulationSystem[] predictionSubSystems = builtinSystems.TrainStationPredictionSubSystems(shapeCargoType, fluidCargoType).ToArray();

                    yield return new TrainStationPredictionSimulationSystem(trainStops, predictionSubSystems, builtinSystems.Logger);
                }
                else
                {
                    yield return simulationSystem;
                }
            }
            yield break;
        }
    }
}
