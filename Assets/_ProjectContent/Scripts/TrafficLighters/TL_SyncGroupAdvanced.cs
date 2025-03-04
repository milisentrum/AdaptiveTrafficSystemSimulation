using System.Collections.Generic;

namespace AdaptiveTrafficSystem.TrafficLighters
{
    public class TL_SyncGroupAdvanced : TL_SyncGroup
    {
        public List<TrafficLighter> oppositeLighters;

        public override void SwitchToOpen()
        {
            base.SwitchToOpen();
            foreach (var lighter in oppositeLighters)
            {
                lighter.SwitchToClose();
            }
        }

        public override void SwitchToClose()
        {
            base.SwitchToClose();
            foreach (var lighter in oppositeLighters)
            {
                lighter.SwitchToOpen();
            }
        }
    }
}