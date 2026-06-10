using Game.Core.Coordinates;
using System.Collections.Concurrent;

namespace UnlimitedWaitStop
{
    internal class WaitStopRegistry
    {
        const int DEFAULT_WAIT_TIME_SECONDS = -1;
        public static readonly ConcurrentDictionary<GlobalChunkCoordinate, int> WaitTimes = new();

        public static int Get(GlobalChunkCoordinate chunk)
        {
            if (!WaitTimes.TryGetValue(chunk, out int value))
            {
                value = DEFAULT_WAIT_TIME_SECONDS;
                WaitTimes[chunk] = value;
            }
            return value;
        }

        public static void Set(GlobalChunkCoordinate chunk, int waitTimeSeconds)
        {
            if (waitTimeSeconds < 0)
            {
                waitTimeSeconds = DEFAULT_WAIT_TIME_SECONDS;
            }
            WaitTimes[chunk] = waitTimeSeconds;
        }
    }
}
