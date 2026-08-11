using System;
using Game.Core.Modding;

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
        }
        public void Dispose() { }

        private void DoNothing(int value)
        {
            nothing -= value;
        }
    }
}
