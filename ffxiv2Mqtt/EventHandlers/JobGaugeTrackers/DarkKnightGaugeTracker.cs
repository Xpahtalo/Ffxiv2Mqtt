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
            if (darkKnightGauge.Blood != blood)
            {
                mqttManager.PublishMessage("JobGauge/DRK/Blood", darkKnightGauge.Blood);
                blood = darkKnightGauge.Blood;
            }

            if (CheckCountDownTimer(darksideTimeRemaining, darkKnightGauge.DarksideTimeRemaining, 1000))
            {
                mqttManager.PublishMessage("JobGauge/DRK/DarksideTimeRemaining", darkKnightGauge.DarksideTimeRemaining);
                darksideTimeRemaining = darkKnightGauge.DarksideTimeRemaining;
            }

            if (darkKnightGauge.HasDarkArts != hasDarkArts)
            {
                mqttManager.PublishMessage("JobGauge/DRK/DarkArts", darkKnightGauge.HasDarkArts);
                hasDarkArts = darkKnightGauge.HasDarkArts;
            }
            
            if(CheckCountDownTimer(shadowTimeRemaining, darkKnightGauge.ShadowTimeRemaining, 1000))
            {
                mqttManager.PublishMessage("JobGauge/DRK/ShadowTimeRemaining", darkKnightGauge.ShadowTimeRemaining);
                shadowTimeRemaining = darkKnightGauge.ShadowTimeRemaining;
            }
        }
    }
}
