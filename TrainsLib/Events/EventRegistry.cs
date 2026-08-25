using Core.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace TrainsLib.Events
{
    /// <summary>
    /// Registers events related to trains.
    /// </summary>
    public static class EventRegistry
    {
        /// <summary>
        /// Fires when a train decides to stop at a station.
        /// </summary>
        public static IEvent<TrainArrivedEvent> OnTrainArrived => OnTrainArrivedEvent;
        internal static readonly MultiRegisterEvent<TrainArrivedEvent> OnTrainArrivedEvent = new();

        /// <summary>
        /// Fires when a train decides to leave a station.
        /// </summary>
        public static IEvent<TrainLeftEvent> OnTrainLeft => OnTrainLeftEvent;
        internal static readonly MultiRegisterEvent<TrainLeftEvent> OnTrainLeftEvent = new();
    }
}
