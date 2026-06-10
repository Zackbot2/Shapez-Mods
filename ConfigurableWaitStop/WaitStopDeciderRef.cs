using Game.Core.Coordinates;
using Game.Core.Simulation;
using Game.Core.Trains;
using System;
using System.Collections.Concurrent;

namespace ConfigurableWaitStop
{
    public class WaitStopDeciderRef
    {
        public WaitStopDecider? Current { get; set; }
        public IHUDDialogStack? DialogStack { get; set; }
        public Action? RefreshSidePanel { get; set; }
        public IslandDefinitionId WaitStationId { get; set; }

        public readonly ConcurrentDictionary<GlobalChunkCoordinate, Ticks> WaitTimes = new();

        public const int DEFAULT_WAIT_SECONDS = 60;
        private readonly Ticks DefaultWaitTicks;

        public Ticks GetWaitTicks(GlobalChunkCoordinate chunk)
        {
            if (!WaitTimes.TryGetValue(chunk, out Ticks value))
            {
                value = DefaultWaitTicks;
                WaitTimes[chunk] = value;
            }
            return value;
        }

        public int GetWaitSeconds(GlobalChunkCoordinate chunk)
        {
            if (!WaitTimes.TryGetValue(chunk, out Ticks value))
            {
                value = DefaultWaitTicks;
                WaitTimes[chunk] = value;
            }
            return value.FullSeconds;
        }

        public void SetWaitTicks(GlobalChunkCoordinate chunk, Ticks waitTicks)
        {
            if (waitTicks.Value < 0)
            {
                waitTicks = DefaultWaitTicks;
            }
            WaitTimes[chunk] = waitTicks;
        }
        public void SetWaitSeconds(GlobalChunkCoordinate chunk, int waitSeconds)
        {
            if (waitSeconds < 0)
            {
                waitSeconds = DEFAULT_WAIT_SECONDS;
            }
            WaitTimes[chunk] = Ticks.FromSeconds(waitSeconds);
        }

        public WaitStopDeciderRef()
        {
             DefaultWaitTicks = Ticks.FromSeconds(DEFAULT_WAIT_SECONDS);
        }
    }
}
