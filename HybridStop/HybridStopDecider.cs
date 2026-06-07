using Game.Content.Features.Fluids;
using Game.Content.Features.Signals.Channels;
using Game.Core.Coordinates;
using Game.Core.Simulation;
using Game.Core.Trains;
using Game.Core.Trains.Stations;
using System;
using System.Collections.Generic;
using System.Text;
using Game.Content.Features;
using Core.Logging;

namespace HybridStop
{
    public class HybridStopDecider : ITrainStopDecider, ITrainLeaveDecider
    {
        private readonly TrainsSimulation Simulation;
        private readonly TrainsWagonCargo CargoSimulator;
        private readonly ISimulationTimeProvider TrainSimulationTimeProvider;
        private readonly ILogger Logger;

        public HybridStopDecider(TrainsSimulation simulation, TrainsWagonCargo cargoSimulator, ISimulationTimeProvider trainSimulationTimeProvider, ILogger logger)
        {
            Simulation = simulation;
            CargoSimulator = cargoSimulator;
            TrainSimulationTimeProvider = trainSimulationTimeProvider;
            Logger = logger;
        }
        public bool ShouldTrainStop(TrainId trainId, TrainSimulationData trainSimulationData)
        {
            // stop like a wait stop
            return TrainCouldExchange(trainId, trainSimulationData);
        }

        public bool ShouldTrainLeave(TrainId trainId, TrainSimulationData trainSimulationData)
        {
            if (TrainCanExchangeImmediately(trainId, trainSimulationData) || !TrainExchangeCompleted(trainSimulationData))
                return false;

            // keep the base wait stop logic for 4 seconds
            //if (TrainSimulationTimeProvider.SimulationTime - trainSimulationData.StopTime < Ticks.FromSeconds(4f))
            //{
            //    return !TrainCouldExchange(trainId, trainSimulationData);
            //}

            // leave as soon as a single floor of a car cannot exchange, but only for exchangers that are enabled. if they're disabled, ignore.
            return !TrainCouldExchange(trainId, trainSimulationData) || TrainHasCompleteExchanges(trainId, trainSimulationData);
        }

        private bool TrainCouldExchange(TrainId trainId, TrainSimulationData trainSimulationData)
        {
            for (int i = 1; i < trainSimulationData.Wagons.Length; i++)
            {
                WagonNavigationData wagonNavigationData = trainSimulationData.Wagons[i];
                if (!wagonNavigationData.UpsideDown)
                {
                    GlobalChunkCoordinate position = wagonNavigationData.Outgoing.Position;
                    if (!CargoSimulator.TryGetExchanger(position, out ICargoExchanger cargoExchanger))
                    {
                        break;
                    }

                    TrainWagonId trainWagonId = Simulation.FindTrainWagonByIndex_Slow(trainId, i);
                    CargoSimulator.TryGetCargo(trainWagonId, out IWagonCargoData wagonCargoData);
                    if (cargoExchanger.CouldExchangeWithCargo(wagonCargoData))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool TrainExchangeCompleted(TrainSimulationData trainSimulationData)
        {
            for (int i = 1; i < trainSimulationData.Wagons.Length; i++)
            {
                GlobalChunkCoordinate position = trainSimulationData.Wagons[i].Outgoing.Position;
                ICargoExchanger cargoExchanger;
                if (!this.CargoSimulator.TryGetExchanger(position, out cargoExchanger))
                {
                    break;
                }
                if (cargoExchanger.IsActivelyExchangingWithTrain())
                {
                    return false;
                }
            }
            return true;
        }

        private bool TrainCanExchangeImmediately(TrainId trainId, TrainSimulationData trainSimulationData)
        {
            for (int i = 1; i < trainSimulationData.Wagons.Length; i++)
            {
                WagonNavigationData wagonNavigationData = trainSimulationData.Wagons[i];
                if (!wagonNavigationData.UpsideDown)
                {
                    GlobalChunkCoordinate position = wagonNavigationData.Outgoing.Position;
                    ICargoExchanger cargoExchanger;
                    if (!this.CargoSimulator.TryGetExchanger(position, out cargoExchanger))
                    {
                        break;
                    }
                    TrainWagonId wagonId = this.Simulation.FindTrainWagonByIndex_Slow(trainId, i);
                    IWagonCargoData wagonCargo;
                    this.CargoSimulator.TryGetCargo(wagonId, out wagonCargo);
                    if (cargoExchanger.CanExchangeImmediatelyWithCargo(wagonCargo))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Does the train with id <paramref name="trainId"/> have any floors of wagons that have completed exchanging?
        /// </summary>
        /// <param name="trainId"></param>
        /// <param name="trainSimulationData"></param>
        /// <returns></returns>
        private bool TrainHasCompleteExchanges(TrainId trainId, TrainSimulationData trainSimulationData)
        {
            // return true if there is a layer of any wagon that cannot exchange, and the layer of the exchanger is ENABLED.
            Logger.Info?.Log($"checking if train {trainId} has any complete exchanges");

            // start at 1 because the engine is at index 0
            for (int wagonNumber = 1; wagonNumber < trainSimulationData.Wagons.Length; wagonNumber++)
            {
                Logger.Info?.Log($"checking wagon {wagonNumber}");
                WagonNavigationData wagonNavigationData = trainSimulationData.Wagons[wagonNumber];
                if (wagonNavigationData.UpsideDown)
                    continue;

                GlobalChunkCoordinate position = wagonNavigationData.Outgoing.Position;
                if (!CargoSimulator.TryGetExchanger(position, out ICargoExchanger cargoExchanger))
                    break;

                TrainWagonId trainWagonId = Simulation.FindTrainWagonByIndex_Slow(trainId, wagonNumber);
                CargoSimulator.TryGetCargo(trainWagonId, out IWagonCargoData wagonCargoData);
                
                Type wagonType = wagonCargoData.GetType();                  // gets LayeredWagonCargo<CargoContainer<ShapeId>>
                Type containerType = wagonType.GetGenericArguments()[0];    // gets CargoContainer<ShapeId>
                Type itemType = containerType.GetGenericArguments()[0];         // gets ShapeId
                Logger.Info?.Log($"wagon type: {itemType}");

                // use a tuple pattern-matching switch statement here, for a few reasons:
                // 1. this is extremely efficient when compiled, something to do with smart decision trees under the hood
                // 2. fairly straightforward to expand for different exchanger types and/or cargo types
                // 3. it's pretty easy to read
                // 4. inline variable names don't conflict like they would in an if else chain
                switch (cargoExchanger, wagonCargoData)
                {
                    case (TrainCargoUnloaderSimulation<FluidId> fUnloader, LayeredWagonCargo<CargoContainer<FluidId>> fCargo):
                        // return true if the check passes, but don't return false if it fails. remember that we're in a loop
                        if (IsUnloaderBlocked(fUnloader, fCargo)) 
                            return true;
                        break;

                    case (TrainCargoLoaderSimulation<FluidId> fLoader, LayeredWagonCargo<CargoContainer<FluidId>> fCargo):
                        if (IsLoaderBlocked(fLoader, fCargo))
                            return true;
                        break;

                    case (TrainCargoUnloaderSimulation<ShapeId> sUnloader, LayeredWagonCargo<CargoContainer<ShapeId>> sCargo):
                        if (IsUnloaderBlocked(sUnloader, sCargo))
                            return true;
                        break;

                    case (TrainCargoLoaderSimulation<ShapeId> sLoader, LayeredWagonCargo<CargoContainer<ShapeId>> sCargo):
                        if (IsLoaderBlocked(sLoader, sCargo))
                            return true;
                        break;
                }
            }
            // if we get here, it means that everything else failed and we don't have any complete exchanges
            return false;
        }

        /// <summary>
        /// Ignoring deactivated layers, is this <paramref name="unloader"/> trying to pull from an empty <paramref name="wagon"/>?
        /// </summary>
        /// <typeparam name="TItem">The type of item we're dealing with, typically <see cref="ShapeId"/> or <see cref="FluidId"/></typeparam>
        /// <param name="unloader"></param>
        /// <param name="wagon"></param>
        /// <returns></returns>
        private bool IsUnloaderBlocked<TItem>(TrainCargoUnloaderSimulation<TItem> unloader, LayeredWagonCargo<CargoContainer<TItem>> wagon)
            where TItem : unmanaged, IEquatable<TItem>
        {
            Logger.Info?.Log($"is unloader of type {typeof(TItem).Name} blocked?");
            for (int layerNum = 0; layerNum < wagon.Containers.Count; layerNum++)
            {
                if (wagon.Containers[layerNum].IsEmpty && unloader.IsLayerActive(layerNum))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Ignoring deactivated layers, is this <paramref name="loader"/> trying to push into a full <paramref name="wagon"/>?
        /// </summary>
        /// <typeparam name="TItem">The type of item we're dealing with, typically <see cref="ShapeId"/> or <see cref="FluidId"/></typeparam>
        /// <param name="loader"></param>
        /// <param name="wagon"></param>
        /// <returns></returns>
        private bool IsLoaderBlocked<TItem>(TrainCargoLoaderSimulation<TItem> loader, LayeredWagonCargo<CargoContainer<TItem>> wagon) where TItem : unmanaged, IEquatable<TItem>
        {
            Logger.Info?.Log($"is loader of type {nameof(TItem)}blocked?");
            for (int layerNum = 0; layerNum < wagon.Containers.Count; layerNum++)
            {
                CargoContainer<TItem> cargoContainer = wagon.Containers[layerNum];

                if (cargoContainer.IsFull && loader.IsLayerActive(layerNum))
                {
                    return true;
                }
            }
            return false;
        }

    }
}
