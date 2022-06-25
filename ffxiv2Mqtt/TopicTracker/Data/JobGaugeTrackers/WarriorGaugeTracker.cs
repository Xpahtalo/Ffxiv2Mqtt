using Dalamud.Game.ClientState.JobGauge.Types;
using Ffxiv2Mqtt.TopicTracker.Interfaces;

namespace Ffxiv2Mqtt.TopicTracker.Data
{
    internal class WarriorGaugeTracker : BaseGaugeTracker, IUpdatable
    {
        public byte BeastGauge { get => beastGauge; }

        private byte beastGauge;
        
        private const uint WarriorId = 21;

        public WarriorGaugeTracker(MqttManager m) : base(m) {
            topic = "Player/JobGauge/WAR";
        }

        public void Update()
        {
            if (Dalamud.ClientState.IsPvP)
                return;
            var localPlayer = Dalamud.ClientState.LocalPlayer;
            if (localPlayer is null)
                return;
            if (localPlayer.ClassJob.Id != WarriorId)
                return;
            var gauge = Dalamud.JobGauges.Get<WARGauge>();
            
            TestValue(gauge.BeastGauge, ref beastGauge);

            PublishIfNeeded();
        }
    }
}
