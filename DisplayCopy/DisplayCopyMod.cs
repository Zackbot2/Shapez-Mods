using Core.Logging;
using System;

namespace DisplayCopy
{
    public class DisplayCopyMod : IMod
    {
        internal static ILogger Logger { get; private set; } = null!;

        public DisplayCopyMod(ILogger logger)
        {
            Logger = logger;
        }

        public void Dispose()
        {

        }
    }
}
