using Core.Events;
using Game.Core.Map.Simulation;
using Game.Core.Trains;
using System;
using System.Collections.Generic;

namespace UnlimitedWaitStop
{
    public class WaitStopIslandSystem : ISimulationSystem
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
    }
}
