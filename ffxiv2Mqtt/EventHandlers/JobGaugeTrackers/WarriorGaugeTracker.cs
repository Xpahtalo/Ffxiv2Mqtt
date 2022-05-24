using Dalamud.Game.ClientState.JobGauge.Types;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace Ffxiv2Mqtt.EventHandlers.JobGaugeTrackers
{
    internal class WarriorGaugeTracker : BaseGaugeTracker
    {
        private WarriorGauge previousWarriorGauge;

        public WarriorGaugeTracker(MqttManager m) : base(m) { }

        public void Update(WARGauge warriorGauge)
        {
            if (warriorGauge.BeastGauge != previousWarriorGauge.BeastGauge)
            {
                mqttManager.PublishMessage("JobGauge/WAR/BeastGauge", warriorGauge.BeastGauge.ToString());
                previousWarriorGauge.BeastGauge = warriorGauge.BeastGauge; 
            }
        }
    }
}
