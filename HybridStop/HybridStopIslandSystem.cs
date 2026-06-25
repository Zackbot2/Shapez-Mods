using Core.Events;
using Game.Core.Map.Simulation;
using System;
using System.Collections.Generic;

namespace HybridStop
{
    public class HybridStopIslandSystem : ISimulationSystem, IIslandObserverSimulationSystem
    {
        private readonly IslandDefinitionId _islandDefinitionId;
        private readonly HybridStopDecider _decider;

        private readonly MultiRegisterEvent<IConnectableSimulation> _onSimulationCreated = new();
        private readonly MultiRegisterEvent<IConnectableSimulation> _onBeforeSimulationDestroyed = new();

        public HybridStopIslandSystem(IslandDefinitionId islandDefinitionId, HybridStopDecider decider)
        {
            _islandDefinitionId = islandDefinitionId;
            _decider = decider;
        }

        public IEvent<IConnectableSimulation> OnSimulationCreated => _onSimulationCreated;
        public IEvent<IConnectableSimulation> OnBeforeSimulationDestroyed => _onBeforeSimulationDestroyed;
        public IEnumerable<IConnectableSimulation> ConnectableSimulations => Array.Empty<IConnectableSimulation>();

        public void IslandWasAdded(in IslandInstance island, IReadOnlyMapLayout layout) { }

        public void IslandWillBeRemoved(in IslandInstance island, IReadOnlyMapLayout layout) { }
    }
}
