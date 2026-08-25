using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TrainsLib.Stations
{
    public static class ModdedStopRegistry
    {
        internal static readonly List<ModdedTrainStop> RegisteredStops = new();

        /// <summary>
        /// Register a new modded train stop. 
        /// </summary>
        /// <param name="moddedTrainStop"></param>
        public static void RegisterTrainStop(ModdedTrainStop moddedTrainStop)
        {
            ModdedTrainStop existingStop = RegisteredStops.FirstOrDefault(stop => stop.DefinitionId == moddedTrainStop.DefinitionId);
            if (existingStop != null)
            {
                throw new InvalidOperationException($"A modded train stop with the definition ID {moddedTrainStop.DefinitionId} is already registered: {existingStop}.");
            }

            RegisteredStops.Add(moddedTrainStop);
        }

        /// <summary>
        /// Get a registered <see cref="ModdedTrainStop"/> by its <see cref="IslandDefinitionId"/>.
        /// </summary>
        /// <param name="definitionId"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException">Thrown if no registered stop exists with the given definition ID.</exception>
        public static ModdedTrainStop GetStopByDefinitionId(IslandDefinitionId definitionId)
        {
            return RegisteredStops.FirstOrDefault(stop => stop.DefinitionId == definitionId) 
                ?? throw new KeyNotFoundException($"No {nameof(ModdedTrainStop)} found with the definition ID {definitionId}.");
        }

        /// <summary>
        /// Get a registered <see cref="ModdedTrainStop"/> by its <see cref="IIslandDefinition"/>.
        /// </summary>
        /// <param name="definition"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public static ModdedTrainStop GetStopByDefinition(IIslandDefinition definition)
        {
            return RegisteredStops.FirstOrDefault(stop => stop.Definition == definition)
                ?? throw new KeyNotFoundException($"No {nameof(ModdedTrainStop)} found with the definition {definition}.");
        }
    }
}
