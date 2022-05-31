using Dalamud.Game.ClientState.JobGauge.Types;

namespace Ffxiv2Mqtt.EventHandlers.JobGaugeTrackers
{
    internal class PaladinGaugeTracker : BaseGaugeTracker
    {
        private byte oathGauge;

        public PaladinGaugeTracker(MqttManager m) : base(m) { }

        public void Update(PLDGauge paladinGauge)
        {
            TestValue(paladinGauge.OathGauge, ref oathGauge, "JobGauge/PLD/OathGauge");
        }
    }
}
