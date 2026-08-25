using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TrainsLib.Stations
{
    /// <summary>
    /// Everything to do with train stops.
    /// </summary>
    public static class TrainStops
    {
        internal static readonly List<ModdedTrainStop> ModdedTrainStops = new();

        /// <summary>
        /// Register a new modded train stop. 
        /// </summary>
        /// <param name="moddedTrainStop"></param>
        public static void RegisterTrainStop(ModdedTrainStop moddedTrainStop)
        {
            ModdedTrainStop existingStop = ModdedTrainStops.FirstOrDefault(stop => stop.DefinitionId == moddedTrainStop.DefinitionId);
            if (existingStop != null)
            {
                throw new InvalidOperationException($"A modded train stop with the definition ID {moddedTrainStop.DefinitionId} is already registered: {existingStop}.");
            }

            ModdedTrainStops.Add(moddedTrainStop);
        }
    }
}
