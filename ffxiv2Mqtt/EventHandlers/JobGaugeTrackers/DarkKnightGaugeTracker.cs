using Dalamud.Game.ClientState.JobGauge.Types;

namespace Ffxiv2Mqtt.EventHandlers.JobGaugeTrackers
{
    internal class DarkKnightGaugeTracker : BaseGaugeTracker
    {
        private byte blood;
        private ushort darksideTimeRemaining;
        private bool hasDarkArts;
        private ushort shadowTimeRemaining;

        public DarkKnightGaugeTracker(MqttManager m) : base(m) { }

        public void Update(DRKGauge darkKnightGauge)
        {
            mqttManager.TestValue(darkKnightGauge.Blood, ref blood, "JobGauge/DRK/Blood");
            mqttManager.TestCountDown(darkKnightGauge.DarksideTimeRemaining, ref darksideTimeRemaining, 1000, "JobGauge/DRK/DarksideTimeRemaining");
            mqttManager.TestValue(darkKnightGauge.HasDarkArts, ref hasDarkArts, "JobGauge/DRK/DarkArts");
            mqttManager.TestCountDown(darkKnightGauge.ShadowTimeRemaining, ref shadowTimeRemaining, 1000, "JobGauge/DRK/ShadowTimeRemaining");
        }
    }
}
