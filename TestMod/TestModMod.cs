using System;

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
        private int nothing = 0;

        public TestModMod()
        {
            nothing++;
        }
        public void Dispose() { }
    }
}
