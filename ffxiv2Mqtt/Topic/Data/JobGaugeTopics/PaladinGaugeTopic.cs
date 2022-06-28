using Dalamud.Game.ClientState.JobGauge.Types;
using Ffxiv2Mqtt.Topic.Interfaces;

namespace Ffxiv2Mqtt.Topic.Data
{
    internal class PaladinGaugeTopic : JobGaugeTopic, IUpdatable
    {
        public byte OathGauge { get => oathGauge; }
        private byte oathGauge;

        private const uint PaladinId = 19;

        public PaladinGaugeTopic(MqttManager m) : base(m)
        {
            topic = "Player/JobGauge/PLD";
        }

        public void Update()
        {
            if (DalamudServices.ClientState.IsPvP)
                return;
            var localPlayer = DalamudServices.ClientState.LocalPlayer;
            if (localPlayer is null)
                return;
            if ((Job)localPlayer.ClassJob.Id != Job.Paladin)
                return;
            
            var gauge = DalamudServices.JobGauges.Get<PLDGauge>();
            TestValue(gauge.OathGauge, ref oathGauge);

            PublishIfNeeded();
        }
    }
}
