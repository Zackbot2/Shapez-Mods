using Game.Core.Trains;
using System;

namespace UnlimitedWaitStop
{
    public class WaitStopDeciderRef
    {
        public WaitStopDecider? Current { get; set; }
        public IHUDDialogStack? DialogStack { get; set; }
        public Action RefreshSidePanel { get; set; }

        public WaitStopDeciderRef() { }
    }
}
