using Game.Core.Serialization;
using Game.Core.Simulation;
using System;
using UnityEngine;

namespace UnlimitedWaitStop
{
    public class WaitStopIslandConfiguration : IIslandConfiguration, IEntityConfiguration, IEquatable<IEntityConfiguration>
    {
        private int _waitTimeSeconds = -1;
        public int WaitTimeSeconds
        {
            get => _waitTimeSeconds;
            set
            {
                if (value != _waitTimeSeconds)
                    WaitTimeTicks = Ticks.FromSeconds(value);
                
                _waitTimeSeconds = value;
            }
        }
        
        public Ticks WaitTimeTicks
        {
            get => Ticks.FromSeconds(_waitTimeSeconds);

            set => _waitTimeSeconds = value.FullSeconds;
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
            visitor.SyncInt_4(ref _waitTimeSeconds);
        }
    }
}
