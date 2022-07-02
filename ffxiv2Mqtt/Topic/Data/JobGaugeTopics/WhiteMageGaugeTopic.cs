using Ffxiv2Mqtt.Enums;
using Dalamud.Game.ClientState.JobGauge.Types;
using Ffxiv2Mqtt.Topic.Interfaces;

namespace Ffxiv2Mqtt.Topic.Data
{
    internal class WhiteMageGaugeTopic : JobGaugeTopic, IUpdatable
    {
        public byte Lily { get => lily; }
        public byte BloodLily { get => bloodLily; }
        public short LilyTimer { get => lilyTimer; }

        private byte lily;
        private byte bloodLily;
        private short lilyTimer;


        public WhiteMageGaugeTopic(MqttManager m) : base(m)
        {
            topic = "Player/JobGauge/WHM";
        }

        public void Update()
        {
            if (DalamudServices.ClientState.IsPvP)
                return;
            var localPlayer = DalamudServices.ClientState.LocalPlayer;
            if (localPlayer is null)
                return;
            if ((Job)localPlayer.ClassJob.Id != Job.WhiteMage)
                return;
            var gauge = DalamudServices.JobGauges.Get<WHMGauge>();

            TestValue(gauge.Lily, ref lily);
            TestValue(gauge.BloodLily, ref bloodLily);
            TestCountUp(gauge.LilyTimer, ref lilyTimer, syncTimer);

            PublishIfNeeded();
        }
    }
}
