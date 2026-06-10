using Game.Core.Serialization;
using Game.Core.Simulation;
using System;
using UnityEngine;

namespace ConfigurableWaitStop
{
    public class WaitStopIslandConfiguration : IIslandConfiguration, IEntityConfiguration, IEquatable<IEntityConfiguration>
    {
        public int WaitTimeSeconds = WaitStopDeciderRef.DEFAULT_WAIT_SECONDS;

        public Ticks WaitTimeTicks
        {
            get => Ticks.FromSeconds(WaitTimeSeconds);
            set => WaitTimeSeconds = value.FullSeconds;
        }

        public WaitStopIslandConfiguration() { }

        public bool Equals(IEntityConfiguration other)
        {
            Debug.Log("Comparing WaitStopIslandConfiguration");
            return other is WaitStopIslandConfiguration otherConfig && WaitTimeSeconds == otherConfig.WaitTimeSeconds;
        }

        public void Sync(ISerializationVisitor visitor)
        {
            Debug.Log("Syncing WaitStopIslandConfiguration");
            //Debug.Log(Environment.StackTrace);
            visitor.SyncInt_4(ref WaitTimeSeconds);
        }
    }
}
