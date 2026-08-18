using ShapezShifter.Hijack;
using System;
using System.Collections.Generic;
using System.Text;

namespace HybridStop
{
    internal class GameIslandsProvider : IIslandsRewirer
    {
        internal static GameIslands? GameIslands;

        public GameIslands ModifyGameIslands(IslandDefinitionFactory factory, AuthoringIslands metaIslands, GameIslands gameIslands)
        {
            HybridStopMod.Logger.Info?.Log("ModifyGameIslands ran");

            GameIslands = gameIslands;

            return gameIslands;
        }
    }
}
