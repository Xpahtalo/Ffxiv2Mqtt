using Dalamud.Game.ClientState.JobGauge.Types;

namespace Ffxiv2Mqtt.EventHandlers.JobGaugeTrackers
{
    internal class WarriorGaugeTracker : BaseGaugeTracker
    {
        private byte beastGauge;

        public WarriorGaugeTracker(MqttManager m) : base(m) { }

        public void Update(WARGauge warriorGauge)
        {
            if (warriorGauge.BeastGauge != beastGauge)
            {
                mqttManager.PublishMessage("JobGauge/WAR/BeastGauge", warriorGauge.BeastGauge.ToString());
                beastGauge = warriorGauge.BeastGauge; 
            }
        }
    }
}
