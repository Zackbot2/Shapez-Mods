using Core.Events;
using Game.Content.Features.Fluids;
using Game.Core.Coordinates;
using Game.Core.Map.Simulation;
using Game.Core.Trains;
using System;
using System.Collections.Generic;
using System.Text;

namespace HybridStop
{
    public class HybridStopIslandSystem : ISimulationSystem
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
    }
}
