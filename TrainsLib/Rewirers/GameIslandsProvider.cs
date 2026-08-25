using ShapezShifter.Hijack;

namespace TrainsLib.Rewirers
{
    /// <summary>
    /// Provides access to the currently active <see cref="GameIslands"/> instance for the scenario.
    /// </summary>
    /// <remarks>
    /// Probably not what Lorenzo intended <see cref="IIslandsRewirer"/> to be used for, but did god intend for anything we do nowadays?
    /// </remarks>
    internal class GameIslandsProvider : IIslandsRewirer
    {
        internal static GameIslands? CurrentGameIslands { get; private set; }

        public GameIslands ModifyGameIslands(IslandDefinitionFactory factory, AuthoringIslands metaIslands, GameIslands gameIslands)
        {
            TrainsLibLogger.LogInfo($"Fetched GameIslands.");
            CurrentGameIslands = gameIslands;

            return gameIslands;
        }
    }
}
