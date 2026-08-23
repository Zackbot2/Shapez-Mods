using ShapezShifter.Hijack;

namespace TrainsLib
{
    /// <summary>
    /// Provides access to the currently active <see cref="GameIslands"/> instance for the scenario.
    /// </summary>
    /// <remarks>
    /// Probably not what Lorenzo intended <see cref="IIslandsRewirer"/> to be used for, but did god intend for anything we do nowadays?
    /// We need this to be able to access the <see cref="IslandDefinition"/> of the hybrid stop.
    /// </remarks>
    internal class GameIslandsProvider : IIslandsRewirer
    {
        internal static GameIslands? ScenarioGameIslands { get; private set; }

        public GameIslands ModifyGameIslands(IslandDefinitionFactory factory, AuthoringIslands metaIslands, GameIslands gameIslands)
        {
            ScenarioGameIslands = gameIslands;

            return gameIslands;
        }
    }
}
