using Game.Core.Trains.Stations;

namespace TrainsLib.Stations
{
    /// <summary>
    /// A decider for both stopping at a station and leaving a station.
    /// One instance exists for all stations that use it (even if those types aren't all the same), and all scenarios.
    /// The game makes 1 instance of it (that's you in this case) and only passes the reference to it wherever it's needed. For this reason, avoid storing state here, instead using dependency injection or read-only properties.
    /// </summary>
    public interface IModdedTrainStopDecider : ITrainStopDecider, ITrainLeaveDecider
    {

    }
}
