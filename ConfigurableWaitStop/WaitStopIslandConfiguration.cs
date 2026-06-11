using Game.Core.Serialization;
using Game.Core.Simulation;
using System;
using UnityEngine;

namespace ConfigurableWaitStop
{
    /// <summary>
    /// Stores the effective <see cref="WaitTimeSeconds"/>. stored with its respective stop in the save file.
    /// </summary>
    public class WaitStopIslandConfiguration : IIslandConfiguration
    {
        public int WaitTimeSeconds = WaitStopData.DEFAULT_WAIT_SECONDS;

        public Ticks WaitTimeTicks
        {
            get => Ticks.FromSeconds(WaitTimeSeconds);
            set => WaitTimeSeconds = value.FullSeconds;
        }

        public WaitStopIslandConfiguration() { }

        public bool Equals(IEntityConfiguration other)
        {
            //Debug.Log("Comparing WaitStopIslandConfiguration");
            return other is WaitStopIslandConfiguration otherConfig && WaitTimeSeconds == otherConfig.WaitTimeSeconds;
        }

        /// <summary>
        /// This will be called when a wait stop is saved, loaded, placed or deleted.
        /// </summary>
        /// <param name="visitor"></param>
        public void Sync(ISerializationVisitor visitor)
        {
            //Debug.Log("Syncing WaitStopIslandConfiguration");
            //Debug.Log(Environment.StackTrace);
            visitor.SyncInt_4(ref WaitTimeSeconds);
        }
    }
}
