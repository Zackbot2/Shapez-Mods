using Game.Core.Coordinates;
using Game.Core.Simulation;
using System;
using System.Collections.Concurrent;

namespace ConfigurableWaitStop
{
    /// <summary>
    /// Stores data related to wait stops.
    /// </summary>
    internal static class WaitStopData
    {
        public static IHUDDialogStack? DialogStack { get; set; }
        public static Action? RefreshSidePanel { get; set; }
        public static IslandDefinitionId WaitStationId { get; set; }

        /// <summary>
        /// Stores the wait times for every wait stop.
        /// </summary>
        /// <remarks>
        /// Since <see cref="Game.Core.Trains.WaitStopDecider"/> doesn't have access to <see cref="WaitStopIslandConfiguration"/>, we need to store the wait times for it.
        /// This is where that happens.
        /// </remarks>
        public static readonly ConcurrentDictionary<GlobalChunkCoordinate, Ticks> WaitTimes = new();
        private static readonly Ticks DefaultWaitTicks = Ticks.FromSeconds(ConfigurableWaitStopMod.DefaultWaitSeconds);

        /// <summary>
        /// Get the wait time in <see cref="Ticks"/> for the wait stop at <paramref name="chunk"/>.
        /// If no wait time is found, it will be stored as the default and returned.
        /// </summary>
        /// <param name="chunk"></param>
        /// <returns></returns>
        public static Ticks GetWaitTicks(GlobalChunkCoordinate chunk)
        {
            if (!WaitTimes.TryGetValue(chunk, out Ticks value))
            {
                value = DefaultWaitTicks;
                WaitTimes[chunk] = value;
            }
            return value;
        }

        /// <summary>
        /// Get the wait time in seconds for the wait stop at <paramref name="chunk"/>.
        /// If no wait time is found, it will be stored as the default and returned.
        /// </summary>
        /// <param name="chunk"></param>
        /// <returns></returns>
        public static int GetWaitSeconds(GlobalChunkCoordinate chunk)
        {
            if (!WaitTimes.TryGetValue(chunk, out Ticks value))
            {
                value = DefaultWaitTicks;
                WaitTimes[chunk] = value;
            }
            return value.FullSeconds;
        }

        /// <summary>
        /// Set the wait time in <see cref="Ticks"/> for the wait stop at <paramref name="chunk"/>.
        /// </summary>
        /// <param name="chunk"></param>
        /// <param name="waitTicks"></param>
        public static void SetWaitTicks(GlobalChunkCoordinate chunk, Ticks waitTicks)
        {
            if (waitTicks.Value < 0)
            {
                waitTicks = Ticks.FromSeconds(-1);
            }
            WaitTimes[chunk] = waitTicks;
        }

        /// <summary>
        /// Set the wait time in seconds for the wait stop at <paramref name="chunk"/>.
        /// </summary>
        /// <param name="chunk"></param>
        /// <param name="waitSeconds"></param>
        public static void SetWaitSeconds(GlobalChunkCoordinate chunk, int waitSeconds)
        {
            if (waitSeconds < 0)
            {
                waitSeconds = -1;
            }
            WaitTimes[chunk] = Ticks.FromSeconds(waitSeconds);
        }
    }
}
