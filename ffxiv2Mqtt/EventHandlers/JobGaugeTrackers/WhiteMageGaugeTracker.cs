using Dalamud.Game.ClientState.JobGauge.Types;

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
            if (whiteMageGauge.Lily != lily)
            {
                mqttManager.PublishMessage("JobGauge/WHM/Lily", whiteMageGauge.Lily);
                lily = whiteMageGauge.Lily;
            }

            if (whiteMageGauge.BloodLily != bloodLily)
            {
                mqttManager.PublishMessage("JobGauge/WHM/BloodLily", whiteMageGauge.BloodLily);
                bloodLily = whiteMageGauge.BloodLily;
            }

            if (CheckCountUpTimer(lilyTimer, whiteMageGauge.LilyTimer, 1000))
            {
                mqttManager.PublishMessage("JobGauge/WHM/LilyTimer", whiteMageGauge.LilyTimer);
                lilyTimer = whiteMageGauge.LilyTimer;
            }
        }
    }
}
