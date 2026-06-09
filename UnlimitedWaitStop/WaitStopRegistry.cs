using Game.Core.Coordinates;
using System.Collections.Concurrent;

namespace UnlimitedWaitStop
{
    internal class WaitStopRegistry
    {
        public static readonly ConcurrentDictionary<GlobalChunkCoordinate, int> WaitTimes = new();
    }
}
