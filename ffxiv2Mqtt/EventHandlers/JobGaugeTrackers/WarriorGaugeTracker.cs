using Dalamud.Game.ClientState.JobGauge.Types;

namespace Ffxiv2Mqtt.EventHandlers.JobGaugeTrackers
{
    internal class WarriorGaugeTracker : BaseGaugeTracker
    {
        private byte beastGauge;

        public WarriorGaugeTracker(MqttManager m) : base(m) { }

        public void Update(WARGauge warriorGauge)
        {
            mqttManager.TestValue(warriorGauge.BeastGauge, ref beastGauge, "JobGauge/WAR/BeastGauge");
        }
    }
}
