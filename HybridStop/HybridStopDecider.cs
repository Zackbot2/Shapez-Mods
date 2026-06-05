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

namespace HybridStop
{
    public class HybridStopDecider : ITrainStopDecider, ITrainLeaveDecider
    {
        private readonly TrainsSimulation Simulation;
        private readonly TrainsWagonCargo CargoSimulator;
        private readonly ISimulationTimeProvider TrainSimulationTimeProvider;

        public HybridStopDecider(TrainsSimulation simulation, TrainsWagonCargo cargoSimulator, ISimulationTimeProvider trainSimulationTimeProvider)
        {
            Simulation = simulation;
            CargoSimulator = cargoSimulator;
            TrainSimulationTimeProvider = trainSimulationTimeProvider;
        }
        public bool ShouldTrainStop(TrainId trainId, TrainSimulationData trainSimulationData)
        {
            return TrainCouldExchange(trainId, trainSimulationData);
        }

        public bool ShouldTrainLeave(TrainId trainId, TrainSimulationData trainSimulationData)
        {
            // leave as soon as a single floor of a car cannot exchange, but only for exchangers that are enabled. if they're disabled, ignore.
            return TrainCouldExchange(trainId, trainSimulationData) && TrainHasCompleteExchanges(trainId, trainSimulationData);
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

        private bool TrainHasCompleteExchanges(TrainId trainId, TrainSimulationData trainSimulationData)
        {
            // return true if there is a layer of any wagon that cannot exchange, and the layer of the exchanger is ENABLED.

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
                        if (wagonCargoData is LayeredWagonCargo<FluidId> fluidCargo)
                        {
                            // see if it's an unloader or a loader, then see if the wagon is empty/full
                            if (cargoExchanger is TrainCargoUnloaderSimulation<FluidId> fluidUnloader)
                            {
                                for (int j = 0; j < fluidCargo.Containers.Count; j++)
                                {
                                    if (fluidUnloader.IsLayerActive(j) && !fluidCargo.Containers[j].IsEmpty)
                                    {
                                        int num = fluidUnloader.BridgeLanes[j].ContainerCount() + fluidUnloader.ContainerTracks[j].ContainerCount() + 1;
                                        if (fluidUnloader.BridgeLanes[j].CanAcceptContainer() && num <= fluidUnloader.ContainerTracks[j].MaxContainersOnTrack)
                                        {
                                            return true;
                                        }
                                    }
                                }
                                return false;
                            }
                            else if (cargoExchanger is TrainCargoLoaderSimulation<FluidId> fluidLoader)
                            {
                                for (int j = 0; j < fluidCargo.Containers.Count; i++)
                                {
                                    if (fluidLoader.IsLayerActive(i) && fluidCargo.Containers[i].IsEmpty)
                                    {
                                        return true;
                                    }
                                }
                                return false;
                            }
                        }
                        else if (wagonCargoData is LayeredWagonCargo<ShapeId> shapeCargo)
                        {
                            IList<ShapeId> layers = shapeCargo.Containers;
                        }

                    }
                }
            }
            return false;
        }
    }
}
