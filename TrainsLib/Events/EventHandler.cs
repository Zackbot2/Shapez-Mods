using System;
using System.Collections.Generic;
using System.Text;

namespace TrainsLib.Events
{
    public static class EventHandler
    {
        public static event Action<TrainArrivedEvent>? OnTrainArrived;
    }
}
