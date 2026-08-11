using Core.Factory;
using Game.Core.Content.Islands;
using Game.Core.Modding;
using ShapezShifter.SharpDetour;
using System;
using UnityEngine;

namespace TestMod
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// This class name is stupid.
    /// </remarks>
    public class TestModMod : IMod
    {
        private int nothing = 69;

        public TestModMod()
        {
            DoNothing(nothing);

            // do nothing
            DetourHelper.CreatePostfixHook(
                (factory, catalogPair, metaIslands) => factory.BakeMetadataIntoRuntime(catalogPair, metaIslands),
                delegate (IslandDefinitionFactory factory, IIslandCatalogPair catalogPair, AuthoringIslands metaIslands, GameIslands __result)
                {
                    return __result;
                });
        }
        public void Dispose() { }

        private void DoNothing(int value)
        {
            nothing -= value;
        }

        
    }
}
