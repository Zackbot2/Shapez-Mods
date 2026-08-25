using System;
using System.Collections.Generic;
using System.Text;
using TrainsLib.Rewirers;

namespace TrainsLib.GameData
{
    public static class GameTrainIslandsData
    {
        #region collections
        public static TrainIslandCollection<IIslandDefinition>? TrainIslands => GameIslandsProvider.CurrentGameIslands?.Trains;
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
