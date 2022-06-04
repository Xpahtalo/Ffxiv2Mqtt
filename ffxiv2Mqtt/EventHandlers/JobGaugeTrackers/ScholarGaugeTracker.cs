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
            mqttManager.TestValue(scholarGauge.Aetherflow, ref aetherflow, "JobGauge/SCH/Aetherflow");
            mqttManager.TestValue(scholarGauge.DismissedFairy, ref dismissedFairy, "JobGauge/SCH/DismissedFairy");
            mqttManager.TestValue(scholarGauge.FairyGauge, ref fairyGauge, "JobGauge/SCH/FairyGauge");
            mqttManager.TestCountDown(scholarGauge.SeraphTimer, ref seraphTimer, 1000, "JobGauge/SCH/SeraphTimer");
        }
    }
}
