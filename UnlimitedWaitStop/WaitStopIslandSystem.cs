using Core.Events;
using Game.Core.Coordinates;
using Game.Core.Map.Simulation;
using Game.Core.Map.Simulation.Systems;
using Game.Core.Trains;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnlimitedWaitStop
{
    public class WaitStopIslandSystem : ISimulationSystem, IIslandObserverSimulationSystem
    {
        private readonly IslandDefinitionId _islandDefinitionId;
        private readonly WaitStopDecider _decider;

        private readonly MultiRegisterEvent<IConnectableSimulation> _onSimulationCreated = new();
        private readonly MultiRegisterEvent<IConnectableSimulation> _onBeforeSimulationDestroyed = new();

        public WaitStopIslandSystem(IslandDefinitionId islandDefinitionId, WaitStopDecider decider)
        {
            _islandDefinitionId = islandDefinitionId;
            _decider = decider;
        }

        public IEvent<IConnectableSimulation> OnSimulationCreated => _onSimulationCreated;
        public IEvent<IConnectableSimulation> OnBeforeSimulationDestroyed => _onBeforeSimulationDestroyed;
        public IEnumerable<IConnectableSimulation> ConnectableSimulations => Array.Empty<IConnectableSimulation>();

        void IIslandObserverSimulationSystem.IslandWasAdded(in IslandInstance island, IReadOnlyMapLayout layout)
        {
            IslandWasAdded(in island, layout);
        }

        void IIslandObserverSimulationSystem.IslandWillBeRemoved(in IslandInstance island, IReadOnlyMapLayout layout)
        {
            IslandWillBeRemoved(in island, layout);
        }

        public void IslandWillBeRemoved(in IslandInstance island, IReadOnlyMapLayout layout)
        {
            Debug.Log("Island removed: " + island.Definition.Id);
            if (island.Definition.Id == _islandDefinitionId)
            {
                GlobalChunkCoordinate stationChunk = GetInputChunk(in island);
                WaitStopRegistry.WaitTimes.Remove(stationChunk, out _);
            }
        }

        public void IslandWasAdded(in IslandInstance island, IReadOnlyMapLayout layout)
        {
            Debug.Log("Island added: " + island.Definition.Id);
            if (island.Definition.Id == _islandDefinitionId)
            {
                GlobalChunkCoordinate stationChunk = GetInputChunk(in island);
                if (island.Configuration is WaitStopIslandConfiguration config)
                {
                    WaitStopRegistry.WaitTimes[stationChunk] = config.WaitTimeSeconds;
                }
            }

        }

        private static GlobalChunkCoordinate GetInputChunk(in IslandInstance island)
        {
            return ChunkVector.Zero.ToGlobal(in island.Transform);
        }
    }
}
