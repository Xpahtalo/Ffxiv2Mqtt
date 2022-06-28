using Dalamud.Game.ClientState.JobGauge.Types;
using Ffxiv2Mqtt.Topic.Interfaces;

namespace Ffxiv2Mqtt.Topic.Data
{
    internal class SageGaugeTopic : JobGaugeTopic, IUpdatable
    {
        public byte Addersgall { get => addersgall; }
        public short AddersgallTimer { get => addersgallTimer; }
        public byte Addersting { get => addersting; }
        public bool Eukrasia { get => eukrasia; }

        private byte addersgall;
        private short addersgallTimer;
        private byte addersting;
        private bool eukrasia;

        private const uint SageId = 40;

        public SageGaugeTopic(MqttManager m) : base(m)
        {
            topic = "Player/JobGauge/SGE";
        }

        public void Update()
        {
            if (DalamudServices.ClientState.IsPvP)
                return;
            var localPlayer = DalamudServices.ClientState.LocalPlayer;
            if (localPlayer is null)
                return;
            if (localPlayer.ClassJob.Id != SageId)
                return;
            var gauge = DalamudServices.JobGauges.Get<SGEGauge>();

            TestValue(gauge.Addersgall, ref addersgall);
            TestCountUp(gauge.AddersgallTimer, ref addersgallTimer, syncTimer);
            TestValue(gauge.Addersting, ref addersting);
            TestValue(gauge.Eukrasia, ref eukrasia);

            PublishIfNeeded();
        }
    }
}
