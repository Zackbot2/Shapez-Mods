using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace TrainsLib
{
    /// <summary>
    /// Everything to do with train stops.
    /// </summary>
    public static class TrainStops
    {
        /// <summary>
        /// Set when <see cref="GameSessionOrchestrator.Init_4_Managers"/> fires.
        /// </summary>
        public static IslandDefinition? WaitStopDefinition { get; internal set; }
        public static IslandDefinition? ImmediateStopDefinition { get; internal set; }

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
