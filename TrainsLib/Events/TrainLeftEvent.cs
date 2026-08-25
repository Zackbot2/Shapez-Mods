using Game.Core.Coordinates;
using Game.Core.Trains;

namespace TrainsLib.Events
{
    /// <summary>
    /// Represents the event of a train leaving a station.
    /// </summary>
    public readonly struct TrainLeftEvent
    {
        public TrainId TrainId { get; }
        public TrainSimulationData TrainData { get; }
        public ITrainStopController TrainStopController { get; }

        internal TrainLeftEvent(TrainId trainId, TrainSimulationData trainData, ITrainStopController trainStopController)
        {
            TrainId = trainId;
            TrainData = trainData;
            TrainStopController = trainStopController;
        }
    }
}
