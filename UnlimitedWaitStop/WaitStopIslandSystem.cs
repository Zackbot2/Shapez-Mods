using Core.Events;
using Game.Core.Coordinates;
using Game.Core.Map.Simulation;
using Game.Core.Trains;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnlimitedWaitStop
{
    public class WaitStopIslandSystem : ISimulationSystem, IIslandObserverSimulationSystem
    {
        private readonly IslandDefinitionId _islandDefinitionId;
        private readonly WaitStopDeciderRef _deciderRef;
        private readonly WaitStopDecider _decider;

        private readonly MultiRegisterEvent<IConnectableSimulation> _onSimulationCreated = new();
        private readonly MultiRegisterEvent<IConnectableSimulation> _onBeforeSimulationDestroyed = new();

        public WaitStopIslandSystem(IslandDefinitionId islandDefinitionId, WaitStopDeciderRef deciderRef, WaitStopDecider decider)
        {
            _islandDefinitionId = islandDefinitionId;
            _deciderRef = deciderRef;
            _decider = decider;
        }

        public IEvent<IConnectableSimulation> OnSimulationCreated => _onSimulationCreated;
        public IEvent<IConnectableSimulation> OnBeforeSimulationDestroyed => _onBeforeSimulationDestroyed;
        public IEnumerable<IConnectableSimulation> ConnectableSimulations => Array.Empty<IConnectableSimulation>();

        void IIslandObserverSimulationSystem.IslandWasAdded(in IslandInstance island, IReadOnlyMapLayout layout)
        {
            IslandWasAdded(in island);
        }

        void IIslandObserverSimulationSystem.IslandWillBeRemoved(in IslandInstance island, IReadOnlyMapLayout layout)
        {
            IslandWillBeRemoved(in island);
        }

        public void IslandWasAdded(in IslandInstance island)
        {
            if (island.Definition.Id == _islandDefinitionId)
            {
                GlobalChunkCoordinate stationChunk = GetInputChunk(in island);
                _deciderRef.SetWaitTicks(stationChunk, _decider.MaxTicksToWait);
            }

        }
        public void IslandWillBeRemoved(in IslandInstance island)
        {
            if (island.Definition.Id == _islandDefinitionId)
            {
                GlobalChunkCoordinate stationChunk = GetInputChunk(in island);
                _deciderRef.WaitTimes.Remove(stationChunk, out _);
            }
        }

        private static GlobalChunkCoordinate GetInputChunk(in IslandInstance island)
        {
            return ChunkVector.Zero.ToGlobal(in island.Transform);
        }
    }
}
