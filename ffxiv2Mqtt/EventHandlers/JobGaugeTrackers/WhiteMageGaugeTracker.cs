﻿using Dalamud.Game.ClientState.JobGauge.Types;

namespace Ffxiv2Mqtt.EventHandlers.JobGaugeTrackers
{
    internal class WhiteMageGaugeTracker : BaseGaugeTracker
    {
        private byte lily;
        private byte bloodLily;
        private short lilyTimer;

        public WhiteMageGaugeTracker(MqttManager m) : base(m) { }

        public void Update(WHMGauge whiteMageGauge)
        {
            TestValue(whiteMageGauge.Lily, ref lily, "JobGauge/WHM/Lily");
            TestValue(whiteMageGauge.BloodLily, ref bloodLily, "JobGauge/WHM/BloodLily");
            TestCountUp(whiteMageGauge.LilyTimer, ref lilyTimer, 1000, "JobGauge/WHM/LilyTimer");
        }
    }
}
