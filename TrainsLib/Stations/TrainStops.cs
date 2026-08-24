using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace TrainsLib.Stations
{
    /// <summary>
    /// Everything to do with train stops.
    /// </summary>
    public static class TrainStops
    {
        private static readonly List<ModdedTrainStop> _moddedTrainStops = new();

        /// <summary>
        /// Register a new modded train stop. 
        /// </summary>
        /// <param name="moddedTrainStop"></param>
        public static void RegisterTrainStop(ModdedTrainStop moddedTrainStop)
        {
            _moddedTrainStops.Add(moddedTrainStop);
            TrainsLibMod.Logger.Info?.Log($"{TrainsLibMod.ModName}:");
        }
    }
}
