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
            return TrainCouldExchange(trainId, trainSimulationData);
        }

        public bool ShouldTrainLeave(TrainId trainId, TrainSimulationData trainSimulationData)
        {
            if (TrainCanExchangeImmediately(trainId, trainSimulationData) || !TrainExchangeCompleted(trainSimulationData))
                return false;

            // keep the base wait stop logic for 5 seconds
            if (TrainSimulationTimeProvider.SimulationTime - trainSimulationData.StopTime < Ticks.FromSeconds(5f))
            {
                return !TrainCouldExchange(trainId, trainSimulationData);
            }

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

        private bool TrainHasIdleWagon(TrainId trainId, TrainSimulationData trainSimulationData)
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

                    if (!cargoExchanger.CouldExchangeWithCargo(wagonCargoData))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool TrainHasCompleteExchanges(TrainId trainId, TrainSimulationData trainSimulationData)
        {
            // return true if there is a layer of any wagon that cannot exchange, and the layer of the exchanger is ENABLED.
            Logger.Info?.Log($"checking if train {trainId} has any complete exchanges");

            for (int wagonIndex = 1; wagonIndex < trainSimulationData.Wagons.Length; wagonIndex++)
            {
                Logger.Info?.Log($"checking wagon {wagonIndex}");
                WagonNavigationData wagonNavigationData = trainSimulationData.Wagons[wagonIndex];
                if (wagonNavigationData.UpsideDown)
                    continue;

                GlobalChunkCoordinate position = wagonNavigationData.Outgoing.Position;
                if (!CargoSimulator.TryGetExchanger(position, out ICargoExchanger cargoExchanger))
                    break;

                TrainWagonId trainWagonId = Simulation.FindTrainWagonByIndex_Slow(trainId, wagonIndex);
                CargoSimulator.TryGetCargo(trainWagonId, out IWagonCargoData wagonCargoData);
                Logger.Info?.Log($"wagonCargoData type: {wagonCargoData.GetType()}");

                // capture both typed exchanger and typed cargo in the same pattern-match to avoid repeated casts
                if (cargoExchanger is TrainCargoUnloaderSimulation<FluidId> fluidUnloader &&
                    wagonCargoData is LayeredWagonCargo<CargoContainer<FluidId>> fluidWagonCargo)
                {
                    Logger.Info?.Log("wagon is fluid wagon");
                    Logger.Info?.Log("exchanger is fluid unloader");
                    if (IsUnloaderBlocking(fluidUnloader, fluidWagonCargo))
                        return true;
                }
                else if (cargoExchanger is TrainCargoLoaderSimulation<FluidId> fluidLoader &&
                         wagonCargoData is LayeredWagonCargo<CargoContainer<FluidId>> fluidWagonCargo2)
                {
                    if (IsLoaderBlocking(fluidLoader, fluidWagonCargo2))
                        return true;
                }
                else if (cargoExchanger is TrainCargoLoaderSimulation<ShapeId> shapeLoader && 
                    wagonCargoData is LayeredWagonCargo<CargoContainer<ShapeId>> shapeCargo)
                {
                    if (IsLoaderBlocking(shapeLoader, shapeCargo))
                        return true;
                }
                else if (cargoExchanger is TrainCargoUnloaderSimulation<ShapeId> shapeUnloader &&
                    wagonCargoData is LayeredWagonCargo<CargoContainer<ShapeId>> shapeCargo2)
                {
                    if (IsUnloaderBlocking(shapeUnloader, shapeCargo2))
                        return true;
                }
            }

            return false;
        }

        private bool IsUnloaderBlocking<TItem>(TrainCargoUnloaderSimulation<TItem> unloader, LayeredWagonCargo<CargoContainer<TItem>> wagonCargo)
            where TItem : unmanaged, IEquatable<TItem>
        {
            for (int j = 0; j < wagonCargo.Containers.Count; j++)
            {
                Logger.Info?.Log($"container {j}: is active {unloader.IsLayerActive(j)}, is empty {wagonCargo.Containers[j].IsEmpty}, can accept {unloader.BridgeLanes[j].CanAcceptContainer()}");
                if (unloader.IsLayerActive(j) && !wagonCargo.Containers[j].IsEmpty)
                {
                    int num = unloader.BridgeLanes[j].ContainerCount() + unloader.ContainerTracks[j].ContainerCount() + 1;
                    if (!unloader.BridgeLanes[j].CanAcceptContainer() || num > unloader.ContainerTracks[j].MaxContainersOnTrack)
                        return true;
                }
                else if (wagonCargo.Containers[j].IsEmpty)
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsLoaderBlocking<TItem>(TrainCargoLoaderSimulation<TItem> loader, LayeredWagonCargo<CargoContainer<TItem>> wagonCargo) where TItem : unmanaged, IEquatable<TItem>
        {
            // TODO: does not catch all cases
            for (int i = 0; i < wagonCargo.Containers.Count; i++)
            {
                CargoContainer<TItem> cargoContainer = wagonCargo.Containers[i];
                if (cargoContainer.MaxPackages == loader.ContainerCapacityProvider.MaxPackagesPerContainer)
                {
                    return true;
                }
                if (cargoContainer.IsFull && loader.IsLayerActive(i))
                {
                    return true;
                }
            }
            return false;
        }

    }
}
