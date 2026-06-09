using Game.Core.Serialization;
using System;

namespace UnlimitedWaitStop
{
    public class WaitStopIslandConfiguration : IIslandConfiguration, IEntityConfiguration, IEquatable<IEntityConfiguration>
    {
        public int WaitTimeSeconds = -1;

        public WaitStopIslandConfiguration() { }

        public bool Equals(IEntityConfiguration other)
        {
            return other is WaitStopIslandConfiguration otherConfig && WaitTimeSeconds == otherConfig.WaitTimeSeconds;
        }

        public void Sync(ISerializationVisitor visitor)
        {
            visitor.SyncInt_4(ref WaitTimeSeconds);
        }
    }
}
