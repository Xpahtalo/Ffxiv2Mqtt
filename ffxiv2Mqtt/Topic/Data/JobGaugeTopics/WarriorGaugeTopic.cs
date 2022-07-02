using Ffxiv2Mqtt.Enums;
using Dalamud.Game.ClientState.JobGauge.Types;
using Ffxiv2Mqtt.Topic.Interfaces;

namespace Ffxiv2Mqtt.Topic.Data
{
    internal class WarriorGaugeTopic : JobGaugeTopic, IUpdatable
    {
        public byte BeastGauge { get => beastGauge; }

        private byte beastGauge;
        

        public WarriorGaugeTopic(MqttManager m) : base(m) {
            topic = "Player/JobGauge/WAR";
        }

        public void Update()
        {
            if (DalamudServices.ClientState.IsPvP)
                return;
            var localPlayer = DalamudServices.ClientState.LocalPlayer;
            if (localPlayer is null)
                return;
            if ((Job)localPlayer.ClassJob.Id != Job.Warrior)
                return;
            
            var gauge = DalamudServices.JobGauges.Get<WARGauge>();
            TestValue(gauge.BeastGauge, ref beastGauge);

            PublishIfNeeded();
        }
    }
}
