using System;
using System.Collections.Generic;
using System.Text;
using TrainsLib.GameData;
using UnityEngine;

namespace TrainsLib.Stations
{
    /// <summary>
    /// Represents a custom train stop. Extending from this class is encouraged but not required, and allows more functionality than otherwise possible.
    /// </summary>
    public class ModdedTrainStop
    {
        public IslandDefinitionId DefinitionId { get; private set; }

        /// <summary>
        /// Will only be defined after <see cref="IslandDefinitionFactory.BakeMetadataIntoRuntime"/> runs. 
        /// Do not use to access this stop's <see cref="IslandDefinitionId"/>. Instead, access the <see cref="DefinitionId"/> property.
        /// </summary>
        public IIslandDefinition? Definition
        {
            get
            {
                if (GameIslandsProvider.ScenarioGameIslands != null &&
                    GameIslandsProvider.ScenarioGameIslands.TryGetDefinition(DefinitionId, out IIslandDefinition? definition))
                {
                    return definition;
                }
                return null;
            }
        }

        public ICustomTrainStopDecider Decider { get; private set; }

        public ModdedTrainStop(IslandDefinitionId definitionId, ICustomTrainStopDecider decider)
        {
            if (definitionId == null)
            {
                throw new ArgumentNullException(nameof(definitionId));
            }

            DefinitionId = definitionId;
            Decider = decider ?? throw new ArgumentNullException(nameof(decider));
        }

        public override string ToString()
        {
            return $"{nameof(ModdedTrainStop)}: {DefinitionId}, {Decider}";
        }
    }
}
