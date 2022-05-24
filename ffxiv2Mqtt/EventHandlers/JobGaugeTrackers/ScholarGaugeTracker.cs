using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.JobGauge.Enums;

namespace Ffxiv2Mqtt.EventHandlers.JobGaugeTrackers
{
    internal class ScholarGaugeTracker : BaseGaugeTracker
    {
        private byte aetherflow;
        private DismissedFairy dismissedFairy;
        private byte fairyGauge;
        private short seraphTimer;

        public ScholarGaugeTracker(MqttManager m) : base(m) { }

        public void Update(SCHGauge scholarGauge)
        {
            if (scholarGauge.Aetherflow != aetherflow)
            {
                mqttManager.PublishMessage("JobGauge/SCH/Aetherflow", scholarGauge.Aetherflow);
                aetherflow = scholarGauge.Aetherflow;
            }

            if (scholarGauge.DismissedFairy != dismissedFairy)
            {
                mqttManager.PublishMessage("JobGauge/SCH/DismissedFairy", scholarGauge.DismissedFairy.ToString());
                dismissedFairy = scholarGauge.DismissedFairy;
            }

            if (scholarGauge.FairyGauge != fairyGauge)
            {
                mqttManager.PublishMessage("JobGauge/SCH/FairyGauge", scholarGauge.FairyGauge.ToString());
                fairyGauge = scholarGauge.FairyGauge;
            }

            if (CheckCountDownTimer(seraphTimer, scholarGauge.SeraphTimer, 1000))
            {
                mqttManager.PublishMessage("JobGauge/SCH/SeraphTimer", scholarGauge.SeraphTimer);
                seraphTimer = scholarGauge.SeraphTimer;
            }
        }
    }
}
