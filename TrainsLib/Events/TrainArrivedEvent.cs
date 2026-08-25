using Game.Core.Trains;

namespace TrainsLib.Events
{
    /// <summary>
    /// Event representing the event of a train arriving at a station.
    /// </summary>
    public readonly struct TrainArrivedEvent
    {
        public TrainId TrainId { get; }
        public TrainSimulationData TrainData { get; }
        public ITrainStopController TrainStopController { get; }

        internal TrainArrivedEvent(TrainId trainId, TrainSimulationData trainData, ITrainStopController trainStopController)
        {
            TrainId = trainId;
            TrainData = trainData; 
            TrainStopController = trainStopController;
        }
    }
}
