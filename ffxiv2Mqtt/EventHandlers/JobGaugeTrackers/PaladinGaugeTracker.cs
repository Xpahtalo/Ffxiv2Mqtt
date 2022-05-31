using Dalamud.Game.ClientState.JobGauge.Types;

namespace Ffxiv2Mqtt.EventHandlers.JobGaugeTrackers
{
    internal class PaladinGaugeTracker : BaseGaugeTracker
    {
        private byte oathGauge;

        public PaladinGaugeTracker(MqttManager m) : base(m) { }

        public void Update(PLDGauge paladinGauge)
        {
            if (paladinGauge.OathGauge != oathGauge)
            {
                mqttManager.PublishMessage("JobGauge/PLD/OathGauge", paladinGauge.OathGauge);
                oathGauge = paladinGauge.OathGauge;
            }
        }
    }
}
