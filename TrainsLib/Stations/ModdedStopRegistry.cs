using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TrainsLib.Stations
{
    /// <summary>
    /// Handles the registration of train stops added via mods. This uses the <see cref="ModdedTrainStop"/> class to represent a modded train stop.
    /// Registered train stops are wired into the game in all necessary places that <see cref="ShapezShifter"/> doesn't account for.
    /// </summary>
    public static class ModdedStopRegistry
    {
        internal static readonly List<ModdedTrainStop> RegisteredStops = new();

        /// <summary>
        /// Register a new modded train stop.
        /// Registering a stop does not add it to the game. That is up to you to do using <see cref="ShapezShifter"/>. TrainsLib is only responsible for tying up loose ends that ShapezShifter doesn't cover, and making it easier to access attributes of your modded train stop.
        /// </summary>
        /// <param name="moddedTrainStop">The modded train stop to register.</param>
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
        /// Unregister <paramref name="moddedTrainStop"/>. <b>Please only unregister stops that your mod is responsible for.</b>
        /// </summary>
        /// <param name="moddedTrainStop">The modded train stop to unregister.</param>
        /// <exception cref="InvalidOperationException">Thrown if the modded train stop is not registered.</exception>
        public static void UnregisterTrainStop(ModdedTrainStop moddedTrainStop)
        {
            if (!RegisteredStops.Remove(moddedTrainStop))
            {
                throw new InvalidOperationException($"The modded train stop {moddedTrainStop} is not registered and cannot be unregistered.");
            }
        }

        /// <summary>
        /// Get a registered <see cref="ModdedTrainStop"/> by its <see cref="IslandDefinitionId"/>.
        /// </summary>
        /// <param name="definitionId">The definition ID of the modded train stop.</param>
        /// <returns>Returns the modded train stop with <paramref name="definitionId"/>.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if no registered stop exists with the given definition ID.</exception>
        public static ModdedTrainStop GetStopByDefinitionId(IslandDefinitionId definitionId)
        {
            return RegisteredStops.FirstOrDefault(stop => stop.DefinitionId == definitionId)
                ?? throw new KeyNotFoundException($"No {nameof(ModdedTrainStop)} found with the definition ID {definitionId}.");
        }

        /// <summary>
        /// Try to get a registered <see cref="ModdedTrainStop"/> by its <see cref="IslandDefinitionId"/>.
        /// </summary>
        /// <param name="definitionId">The definition ID of the modded train stop.</param>
        /// <param name="moddedTrainStop">The modded train stop with the given definition ID, if it was found.</param>
        /// <returns>Returns <c>true</c> if a modded train stop with the given definition ID was found; otherwise, <c>false</c>.</returns>
        public static bool TryGetStopByDefinitionId(IslandDefinitionId definitionId, out ModdedTrainStop? moddedTrainStop)
        {
            moddedTrainStop = RegisteredStops.FirstOrDefault(stop => stop.DefinitionId == definitionId);
            return moddedTrainStop != null;
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

        /// <summary>
        /// Try to get a registered <see cref="ModdedTrainStop"/> by its <see cref="IIslandDefinition"/>.
        /// </summary>
        /// <param name="definition"></param>
        /// <param name="moddedTrainStop"></param>
        /// <returns>Returns <c>true</c> if a modded train stop with the given definition was found; otherwise, <c>false</c>.</returns>
        public static bool TryGetStopByDefinition(IIslandDefinition definition, out ModdedTrainStop? moddedTrainStop)
        {
            moddedTrainStop = RegisteredStops.FirstOrDefault(stop => stop.Definition == definition);
            return moddedTrainStop != null;
        }

        internal static void UnregisterAllStops()
        {
            TrainsLibLogger.LogInfo($"Unregistering all modded train stops. (${RegisteredStops.Count})");
            RegisteredStops.Clear();
        }
    }
}
