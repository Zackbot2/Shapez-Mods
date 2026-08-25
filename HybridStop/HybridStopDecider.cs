using Core.Logging;
using Game.Content.Features.Fluids;
using Game.Core.Coordinates;
using Game.Core.Trains;
using Game.Core.Trains.Stations;
using System;
using TrainsLib.GameData;
using TrainsLib.Stations;

namespace HybridStop
{
    /// <summary>
    /// The core logic module of any train stop.
    /// </summary>
    /// <remarks>
    /// Only one instance of this class exists at a time during runtime, shared across all hybrid stops.
    /// </remarks>
    public class HybridStopDecider : IModdedTrainStopDecider
    {
        private TrainsSimulation Simulation => SimulationSystemsData.TrainSystem!.TrainsSimulation;
        private TrainsWagonCargo CargoSimulator => SimulationSystemsData.TrainSystem!.CargoSimulator;
        private ISimulationTimeProvider TrainSimulationTimeProvider => Simulation.TrainSimulationTimeTracker;
        private ILogger Logger => HybridStopMod.Logger;

        public HybridStopDecider() { }

        public bool ShouldTrainStop(TrainId trainId, TrainSimulationData trainSimulationData)
        {
            // stop like a wait stop
            return TrainCouldExchange(trainId, trainSimulationData);
        }

        public bool ShouldTrainLeave(TrainId trainId, TrainSimulationData trainSimulationData)
        {
            if (CanTrainExchangeImmediately(trainId, trainSimulationData) || !IsTrainExchangeCompleted(trainSimulationData))
                return false;

            return !TrainCouldExchange(trainId, trainSimulationData) || TrainHasCompleteExchanges(trainId, trainSimulationData);
        }

        /// <summary>
        /// Are there any exchangers where this train could exchange cargo, even if the exchange is not immediate?
        /// </summary>
        /// <param name="trainId"></param>
        /// <param name="trainSimulationData"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Has the train completed all of its active exchanges?
        /// </summary>
        /// <param name="trainSimulationData"></param>
        /// <returns>Returns <c>true</c> if no wagons are actively exchanging, otherwise <c>false</c>.</returns>
        private bool IsTrainExchangeCompleted(TrainSimulationData trainSimulationData)
        {
            for (int i = 1; i < trainSimulationData.Wagons.Length; i++)
            {
                GlobalChunkCoordinate position = trainSimulationData.Wagons[i].Outgoing.Position;
                if (!CargoSimulator.TryGetExchanger(position, out ICargoExchanger cargoExchanger))
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

        /// <summary>
        /// Are there any packages that can be immediately exchanged?
        /// </summary>
        /// <param name="trainId"></param>
        /// <param name="trainSimulationData"></param>
        /// <returns></returns>
        private bool CanTrainExchangeImmediately(TrainId trainId, TrainSimulationData trainSimulationData)
        {
            for (int i = 1; i < trainSimulationData.Wagons.Length; i++)
            {
                WagonNavigationData wagonNavigationData = trainSimulationData.Wagons[i];
                if (!wagonNavigationData.UpsideDown)
                {
                    if (!CargoSimulator.TryGetExchanger(wagonNavigationData.Outgoing.Position, out ICargoExchanger cargoExchanger))
                        break;
                    
                    TrainWagonId wagonId = Simulation.FindTrainWagonByIndex_Slow(trainId, i);
                    CargoSimulator.TryGetCargo(wagonId, out IWagonCargoData wagonCargo);

                    if (cargoExchanger.CanExchangeImmediatelyWithCargo(wagonCargo))
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Does the train with id <paramref name="trainId"/> have any floors of wagons that have completed exchanging?
        /// </summary>
        /// <param name="trainId"></param>
        /// <param name="trainSimulationData"></param>
        /// <returns>Returns <c>true</c> if there is a layer of any wagon that cannot exchange, and the layer of the exchanger is ENABLED.</returns>
        private bool TrainHasCompleteExchanges(TrainId trainId, TrainSimulationData trainSimulationData)
        {
            // start at 1 because the engine is at index 0
            for (int wagonNumber = 1; wagonNumber < trainSimulationData.Wagons.Length; wagonNumber++)
            {
                WagonNavigationData wagonNavigationData = trainSimulationData.Wagons[wagonNumber];
                if (wagonNavigationData.UpsideDown)
                    continue;

                GlobalChunkCoordinate position = wagonNavigationData.Outgoing.Position;
                if (!CargoSimulator.TryGetExchanger(position, out ICargoExchanger cargoExchanger))
                    break;

                TrainWagonId trainWagonId = Simulation.FindTrainWagonByIndex_Slow(trainId, wagonNumber);
                CargoSimulator.TryGetCargo(trainWagonId, out IWagonCargoData wagonCargoData);

                //Type wagonType = wagonCargoData.GetType();                  // gets LayeredWagonCargo<CargoContainer<ShapeId>>
                //Type containerType = wagonType.GetGenericArguments()[0];    // gets CargoContainer<ShapeId>
                //Type itemType = containerType.GetGenericArguments()[0];         // gets ShapeId
                //Logger.Info?.Log($"wagon item type: {itemType}");

                // use a tuple pattern-matching switch statement here, for a few reasons:
                // 1. this is extremely efficient when compiled, something to do with smart decision trees under the hood
                // 2. fairly straightforward to expand for different exchanger types and/or cargo types
                // 3. it's pretty easy to read
                // 4. inline variable names don't conflict like they would in an if else chain
                switch (cargoExchanger, wagonCargoData)
                {
                    // shape unloader
                    case (TrainCargoUnloaderSimulation<ShapeId> sUnloader, LayeredWagonCargo<CargoContainer<ShapeId>> sCargo):
                        // return true if the check passes, but don't return false if it fails. remember that we're in a loop
                        if (IsUnloaderBlocked(sUnloader, sCargo))
                            return true;
                        break;

                    // shape loader
                    case (TrainCargoLoaderSimulation<ShapeId> sLoader, LayeredWagonCargo<CargoContainer<ShapeId>> sCargo):
                        if (IsLoaderBlocked(sLoader, sCargo))
                            return true;
                        break;

                    // shape transferrer
                    case (TrainCargoTransferrerSimulation<ShapeId>.TransferExchangeHandle sTransferrer, LayeredWagonCargo<CargoContainer<ShapeId>> sCargo):
                        if (IsTransferrerBlocked(sTransferrer, sCargo))
                            return true;
                        break;

                    // fluid unloader
                    case (TrainCargoUnloaderSimulation<FluidId> fUnloader, LayeredWagonCargo<CargoContainer<FluidId>> fCargo):
                        
                        if (IsUnloaderBlocked(fUnloader, fCargo)) 
                            return true;
                        break;

                    // fluid loader
                    case (TrainCargoLoaderSimulation<FluidId> fLoader, LayeredWagonCargo<CargoContainer<FluidId>> fCargo):
                        if (IsLoaderBlocked(fLoader, fCargo))
                            return true;
                        break;

                    // fluid transferrer
                    case (TrainCargoTransferrerSimulation<FluidId>.TransferExchangeHandle fTransferrer, LayeredWagonCargo<CargoContainer<FluidId>> fCargo):
                        if (IsTransferrerBlocked(fTransferrer, fCargo))
                            return true;
                        break;

                }
            }
            // if we get here, it means that everything else failed and we don't have any complete exchanges
            return false;
        }

        #region blocked exchangers

        /// <summary>
        /// Ignoring deactivated layers, is this <paramref name="unloader"/> trying to pull from an empty <paramref name="wagon"/>?
        /// </summary>
        /// <typeparam name="TItem">The type of item we're dealing with, typically <see cref="ShapeId"/> or <see cref="FluidId"/>.</typeparam>
        /// <param name="unloader">The unloader in question.</param>
        /// <param name="wagon">The wagon in question.</param>
        /// <returns></returns>
        private bool IsUnloaderBlocked<TItem>(TrainCargoUnloaderSimulation<TItem> unloader, LayeredWagonCargo<CargoContainer<TItem>> wagon)
            where TItem : unmanaged, IEquatable<TItem>
        {
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
        /// Ignoring deactivated layers, is this <paramref name="loader"/> trying to push to a full <paramref name="wagon"/>?
        /// </summary>
        /// <typeparam name="TItem">The type of item we're dealing with, typically <see cref="ShapeId"/> or <see cref="FluidId"/>.</typeparam>
        /// <param name="loader">The loader in question.</param>
        /// <param name="wagon">The wagon in question.</param>
        /// <returns></returns>
        private bool IsLoaderBlocked<TItem>(TrainCargoLoaderSimulation<TItem> loader, LayeredWagonCargo<CargoContainer<TItem>> wagon) where TItem : unmanaged, IEquatable<TItem>
        {
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

        /// <summary>
        /// Ignoring deactivated layers (do those exist for transfer stations?), is this <paramref name="transferrerHandle"/>
        /// </summary>
        /// <typeparam name="TItem">The type of item we're dealing with, typically <see cref="ShapeId"/> or <see cref="FluidId"/>.</typeparam>
        /// <param name="transferrerHandle"></param>
        /// <param name="wagon"></param>
        /// <returns></returns>
        private bool IsTransferrerBlocked<TItem>(TrainCargoTransferrerSimulation<TItem>.TransferExchangeHandle transferrerHandle, LayeredWagonCargo<CargoContainer<TItem>> wagon) where TItem : unmanaged, IEquatable<TItem>
        {
            bool isUnloading = transferrerHandle.TransferExchangeMode == TrainCargoTransferrerSimulation<TItem>.TransferExchangeMode.UnloadingFromTrainIntoStation;
            bool isLoading = transferrerHandle.TransferExchangeMode == TrainCargoTransferrerSimulation<TItem>.TransferExchangeMode.LoadingFromStationIntoTrain;

            for (int layerNum = 0; layerNum < wagon.Containers.Count; layerNum++)
            {
                if (transferrerHandle.IsLayerActive(layerNum))
                {
                    if ((isUnloading && wagon.Containers[layerNum].IsEmpty)
                        || (isLoading && wagon.Containers[layerNum].IsFull))
                        return true;
                }
            }

            return false;
        }
        
        #endregion blocked exchangers
    }
}
