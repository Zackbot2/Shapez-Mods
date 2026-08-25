using Core.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace TrainsLib.Events
{
    public static class EventHandler
    {
        /// <summary>
        /// Fires when a train decides to stop at a station.
        /// </summary>
        public static IEvent<TrainArrivedEvent> OnTrainArrived => _onTrainArrived;
        private static readonly MultiRegisterEvent<TrainArrivedEvent> _onTrainArrived = new();

        /// <summary>
        /// Fires when a train decides to leave a station.
        /// </summary>
        public static IEvent<TrainLeftEvent> OnTrainLeft => _onTrainLeft;
        private static readonly MultiRegisterEvent<TrainLeftEvent> _onTrainLeft = new();
    }
}
