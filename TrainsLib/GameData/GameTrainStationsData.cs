using System;
using System.Collections.Generic;
using System.Text;

namespace TrainsLib.GameData
{
    public static class GameTrainStationsData
    {
        #region wait stop
        /// <summary>
        /// <see cref="IslandDefinition"/> of the Wait Stop.
        /// Set when <see cref="GameSessionOrchestrator.Init_4_Managers"/> fires.
        /// </summary>
        public static IslandDefinition? WaitStationDefinition { get; internal set; }
        #endregion wait stop

        #region immediate stop
        /// <summary>
        /// <see cref="IslandDefinition"/> of the Immediate Stop.
        /// Set when <see cref="GameSessionOrchestrator.Init_4_Managers"/> fires.
        /// </summary>
        public static IslandDefinition? QuickStationDefinition { get; internal set; }
        #endregion immediate stop
    }
}
