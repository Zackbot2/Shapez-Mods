using Core.Events;
using Game.Core.Coordinates;
using Game.Core.Map.Simulation;
using Game.Core.Trains;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ConfigurableWaitStop
{
    /// <summary>
    /// Handles the simulation logic for wait stop islands, specifically when a wait stop is placed or removed.
    /// </summary>
    public class WaitStopIslandSystem : ISimulationSystem, IIslandObserverSimulationSystem
    {

        private readonly MultiRegisterEvent<IConnectableSimulation> _onSimulationCreated = new();
        public IEvent<IConnectableSimulation> OnSimulationCreated => _onSimulationCreated;

        private readonly MultiRegisterEvent<IConnectableSimulation> _onBeforeSimulationDestroyed = new();
        public IEvent<IConnectableSimulation> OnBeforeSimulationDestroyed => _onBeforeSimulationDestroyed;
        public IEnumerable<IConnectableSimulation> ConnectableSimulations => Array.Empty<IConnectableSimulation>();

        public WaitStopIslandSystem() { }

        void IIslandObserverSimulationSystem.IslandWasAdded(in IslandInstance island, IReadOnlyMapLayout layout)
        {
            IslandWasAdded(in island);
        }

        void IIslandObserverSimulationSystem.IslandWillBeRemoved(in IslandInstance island, IReadOnlyMapLayout layout)
        {
            IslandWillBeRemoved(in island);
        }

        /// <summary>
        /// Called when an island is placed (including when loading a savegame).
        /// </summary>
        /// <param name="island"></param>
        public void IslandWasAdded(in IslandInstance island)
        {
            // we don't care about anything other than the wait stop island.
            if (island.Definition.Id == WaitStopData.WaitStationId)
            {
                GlobalChunkCoordinate stationChunk = GetInputChunk(in island);
                WaitStopData.SetWaitTicks(stationChunk, ((WaitStopIslandConfiguration)island.Configuration).WaitTimeTicks);
            }
        }

        /// <summary>
        /// Called when an island is being removed (including when exiting a savegame).
        /// </summary>
        /// <param name="island"></param>
        public void IslandWillBeRemoved(in IslandInstance island)
        {
            // we don't care about anything other than the wait stop island.
            if (island.Definition.Id == WaitStopData.WaitStationId)
            {
                GlobalChunkCoordinate stationChunk = GetInputChunk(in island);
                WaitStopData.WaitTimes.Remove(stationChunk, out _);
            }
        }

        private static GlobalChunkCoordinate GetInputChunk(in IslandInstance island)
        {
            return ChunkVector.Zero.ToGlobal(in island.Transform);
        }
    }
}
