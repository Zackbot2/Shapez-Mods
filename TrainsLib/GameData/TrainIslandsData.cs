using System;
using System.Collections.Generic;
using System.Text;
using TrainsLib.Rewirers;

namespace TrainsLib.GameData
{
    /// <summary>
    /// Data related to train islands.
    /// </summary>
    public static class TrainIslandsData
    {
        #region collections
        public static TrainIslandCollection<IIslandDefinition>? TrainIslands => GameIslandsProvider.CurrentGameIslands?.Trains;

        public static TrainNavigationIslandCollection<IIslandDefinition>? TrainNavigationIslands => GameIslandsProvider.CurrentGameIslands?.Trains.Navigation;

        public static TrainProductionIslandCollection<IIslandDefinition>? TrainProductionIslands => GameIslandsProvider.CurrentGameIslands?.Trains.Production;
        #endregion collections

        #region train stops
        /// <summary>
        /// <see cref="IslandDefinition"/> of the Wait Stop.
        /// </summary>
        public static IslandDefinition? WaitStationDefinition => (IslandDefinition?)GameIslandsProvider.CurrentGameIslands?.Trains.Navigation.WaitStation;

        /// <summary>
        /// <see cref="IslandDefinition"/> of the Immediate Stop.
        /// </summary>
        public static IslandDefinition? QuickStationDefinition => (IslandDefinition?)GameIslandsProvider.CurrentGameIslands?.Trains.Navigation.QuickStation;
        #endregion train stops
    }
}
