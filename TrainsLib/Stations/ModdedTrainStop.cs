using System;
using System.Collections.Generic;
using System.Text;
using TrainsLib.Rewirers;
using UnityEngine;

namespace TrainsLib.Stations
{
    /// <summary>
    /// Represents a custom train stop. Only one instance of this class should exist at a time.
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
                if (GameIslandsProvider.CurrentGameIslands != null &&
                    GameIslandsProvider.CurrentGameIslands.TryGetDefinition(DefinitionId, out IIslandDefinition? definition))
                {
                    return definition;
                }
                return null;
            }
        }

        public IModdedTrainStopDecider Decider { get; private set; }

        public ModdedTrainStop(IslandDefinitionId definitionId, IModdedTrainStopDecider decider)
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
